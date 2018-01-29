////////////////////////////////////////////////////////////////////////////////
// Shaders to reproject radiance from last frame and to compute HBIL
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "HBIL.hlsl"

static const float	GATHER_SPHERE_RADIUS_M = 1.0;		// Radius of the sphere that will gather our irradiance samples (in meters)
static const float	GATHER_SPHERE_MAX_RADIUS_P = 100.0;	// Maximum radius (in pixels) that we allow our sphere to get

#define MAX_ANGLES	8									// Amount of circle subdivisions per pixel
#define MAX_SAMPLES	32									// Maximum amount of samples per circle subdivision

Texture2D< float4 >	_tex_sourceRadiance : register(t0);
Texture2D< float4 >	_tex_normal : register(t1);
Texture2D< float >	_tex_depth : register(t2);			// Depth or distance buffer (here we're given distances)
Texture2D< float >	_tex_blueNoise : register(t3);

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

////////////////////////////////////////////////////////////////////////////////
// Implement the methods expected by the HBIL header
float	FetchDepth( float2 _pixelPosition ) {
	return Z_FAR * _tex_depth.SampleLevel( LinearClamp, _pixelPosition / _resolution, 0.0 );
}

float3	FetchRadiance( float2 _pixelPosition ) {
	return _tex_sourceRadiance.SampleLevel( LinearClamp, _pixelPosition / _resolution, 0.0 ).xyz;
}


float3	GatherIrradiance_DEBUG( float2 _ssPosition, float2 _ssDirection, float _Z0, float3 _csNormal, float _radialStepSize, uint _stepsCount, out float3 _ssBentNormal, out float2 _coneAngles, inout float4 _DEBUG ) {

	// Pre-compute factors for the integrals
	float2	integralFactors_Front = ComputeIntegralFactors( _ssDirection, _csNormal );
	float2	integralFactors_Back = ComputeIntegralFactors( -_ssDirection, _csNormal );

	// Compute initial cos(angle) for front & back horizons
	// We do that by projecting the screen-space direction ssDirection onto the tangent plane given by the normal
	//	then the cosine of the angle from the Z axis is simply given by the Pythagorean theorem:
	//                             P
	//			   N\  |Z		  -*-
	//				 \ |	  ---  ^
	//				  \|  ---      |
	//             --- *..........>+ ssDirection
	//        --- 
	//
	float	hitDistance_Front = -dot( _ssDirection, _csNormal.xy ) * (abs(_csNormal.z) > 1e-4 ? 1.0 / _csNormal.z : 1.0);
	float	maxCos_Front = hitDistance_Front / sqrt( hitDistance_Front*hitDistance_Front + dot(_ssDirection,_ssDirection) );
	float	maxCos_Back = -maxCos_Front;	// Back cosine is simply the mirror value

//_DEBUG = hitDistance_Front;
//_DEBUG = abs(dot( _ssDirection, _csNormal.xy )) / _csNormal.z;
//_DEBUG = float4( _csNormal.xy, 0, 0 );
//_DEBUG = float4( _ssDirection, 0, 0 );
//_DEBUG = dot( _ssDirection, _csNormal.xy );
//_DEBUG = maxCos_Front;
//_DEBUG = -dot( _ssDirection, _csNormal.xy );
//_DEBUG = -_csNormal.z;
//_DEBUG.xyz = _csNormal;
//_DEBUG.xyz = 1.0 / _csNormal.z;

	// Gather irradiance from front & back directions while updating the horizon angles at the same time
	float3	sumRadiance = 0.0;
	float	radius = 0.0;
	float2	ssStep = _radialStepSize * _ssDirection;
	float2	ssPosition_Front = _ssPosition;
	float2	ssPosition_Back = _ssPosition;
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		radius += _radialStepSize;
		ssPosition_Front += ssStep;
		ssPosition_Back -= ssStep;

		sumRadiance += SampleIrradiance( ssPosition_Front, _Z0, radius, integralFactors_Front, maxCos_Front );
		sumRadiance += SampleIrradiance( ssPosition_Back, _Z0, radius, integralFactors_Back, maxCos_Back );
	}

//_DEBUG = 0.008 * radius;
//_DEBUG = float4( ssPosition_Back - _ssPosition, 0, 0 );
//_DEBUG = maxCos_Front;
//_DEBUG = max( maxCos_Back, maxCos_Front );
//_DEBUG = min( maxCos_Back, maxCos_Front );

	// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
	#if USE_NUMERICAL_INTEGRATION
		// Half brute force where we perform the integration numerically as a sum...
		// This solution is prefered to the analytical integral that shows some precision artefacts unfortunately...
		//
		float	thetaFront = acos( maxCos_Front );
		float	thetaBack = -acos( maxCos_Back );

		_ssBentNormal = 0.001 * _N;
		for ( uint i=0; i < USE_NUMERICAL_INTEGRATION; i++ ) {
			float	theta = lerp( thetaBack, thetaFront, (i+0.5) / USE_NUMERICAL_INTEGRATION );
			float	sinTheta, cosTheta;
			sincos( theta, sinTheta, cosTheta );
			float3	ssUnOccludedDirection = float3( sinTheta * _ssDirection, cosTheta );

			float	cosAlpha = saturate( dot( ssUnOccludedDirection, _N ) );

			float	weight = cosAlpha * abs(sinTheta);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
			_ssBentNormal += weight * ssUnOccludedDirection;
		}

		float	dTheta = (thetaFront - thetaBack) / USE_NUMERICAL_INTEGRATION;
		_ssBentNormal *= dTheta;
	#else
		// Analytical solution
		float	cosTheta0 = maxCos_Front;
		float	cosTheta1 = maxCos_Back;
		float	sinTheta0 = sqrt( 1.0 - cosTheta0*cosTheta0 );
		float	sinTheta1 = sqrt( 1.0 - cosTheta1*cosTheta1 );
		float	cosTheta0_3 = cosTheta0*cosTheta0*cosTheta0;
		float	cosTheta1_3 = cosTheta1*cosTheta1*cosTheta1;
		float	sinTheta0_3 = sinTheta0*sinTheta0*sinTheta0;
		float	sinTheta1_3 = sinTheta1*sinTheta1*sinTheta1;

		float2	sliceSpaceNormal = float2( dot( _csNormal.xy, _ssDirection ), _csNormal.z );

		float	averageX = sliceSpaceNormal.x * (cosTheta0_3 + cosTheta1_3 - 3.0 * (cosTheta0 + cosTheta1) + 4.0)
						 + sliceSpaceNormal.y * (sinTheta0_3 - sinTheta1_3);

		float	averageY = sliceSpaceNormal.x * (sinTheta0_3 - sinTheta1_3)
						 + sliceSpaceNormal.y * (2.0 - cosTheta0_3 - cosTheta1_3);

		_ssBentNormal = float3( averageX * _ssDirection, averageY );
	#endif

	_ssBentNormal = normalize( _ssBentNormal );

//_DEBUG = float4( _ssBentNormal, 0 );

	// Compute cone angles
	float3	ssHorizon_Front = float3( sqrt( 1.0 - maxCos_Front*maxCos_Front ) * _ssDirection, maxCos_Front );
	float3	ssHorizon_Back = float3( -sqrt( 1.0 - maxCos_Back*maxCos_Back ) * _ssDirection, maxCos_Back );
	#if USE_FAST_ACOS
		_coneAngles.x = FastPosAcos( saturate( dot( _ssBentNormal, ssHorizon_Front ) ) );
		_coneAngles.y = FastPosAcos( saturate( dot( _ssBentNormal, ssHorizon_Back ) ) ) ;
	#else
		_coneAngles.x = acos( saturate( dot( _ssBentNormal, ssHorizon_Front ) ) );
		_coneAngles.y = acos( saturate( dot( _ssBentNormal, ssHorizon_Back ) ) );
	#endif

	return sumRadiance;
}

////////////////////////////////////////////////////////////////////////////////
// Computes bent cone & irradiance gathering from pixel's surroundings
struct PS_OUT {
	float4	irradiance : SV_TARGET0;
	float4	bentCone : SV_TARGET1;
};

PS_OUT	PS( float4 __Position : SV_POSITION ) {
	float2	UV = __Position.xy / _resolution;
	uint2	pixelPosition = uint2( floor( __Position.xy ) );
	float	noise = _tex_blueNoise[pixelPosition & 0x3F];

	// Setup camera ray
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;

	// Read back depth/distance & normal + rebuild camera-space TBN
	float	Z = FetchDepth( pixelPosition );
	float	distance = Z * Z2Distance;
	float3	wsNormal = normalize( _tex_normal[pixelPosition].xyz );
	#if 0
		// Camera plane normal
		float3	N = mul( float4( wsNormal, 0 ), _World2Camera ).xyz;
				N.z = -N.z;
		float3	T, B;
		BuildOrthonormalBasis( N, T, B );
	#elif 0
		// Same, with manually computed T, B
		float3	wsTangent
		float3	N = mul( float4( wsNormal, 0 ), _World2Camera ).xyz;
				N.z = -N.z;
		float3	T = normalize( cross( float3( 0, 1, 0 ), N ) );
		float3	B = cross( N, T );
	#else
		// Face-cam normal
		float3	wsRight = normalize( cross( wsView, _World2Camera[1].xyz ) );
		float3	wsUp = cross( wsRight, wsView );
		float3	N = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), -dot( wsNormal, wsView ) );	// Camera-space normal
		float3	T, B;
		BuildOrthonormalBasis( N, T, B );
	#endif

	Z -= 1e-2;	// Prevent acnea by offseting the central depth closer

	// Compute screen radius of gather sphere
	float	screenSize_m = 2.0 * Z * TAN_HALF_FOV;	// Vertical size of the screen in meters when extended to distance Z
	float	sphereRadius_pixels = _resolution.y * GATHER_SPHERE_RADIUS_M / screenSize_m;
			sphereRadius_pixels = min( GATHER_SPHERE_MAX_RADIUS_P, sphereRadius_pixels );							// Prevent it to grow larger than our fixed limit
	float	radiusStepSize_pixels = max( 1.0, sphereRadius_pixels / MAX_SAMPLES );									// This gives us our radial step size
	uint	samplesCount = clamp( uint( ceil( sphereRadius_pixels / radiusStepSize_pixels ) ), 1, MAX_SAMPLES );	// Reduce samples count if possible

	// Start gathering radiance and bent normal by subdividing the screen-space disk around our pixel into Z slices
	float4	GATHER_DEBUG = 0.0;
	float3	sumIrradiance = 0.0;
	float3	ssAverageBentNormal = 0.0;
	float	averageConeAngle = 0.0;
	float	varianceConeAngle = 0.0;
#if MAX_ANGLES > 1
	for ( uint angleIndex=0; angleIndex < MAX_ANGLES; angleIndex++ )
#else
	uint	angleIndex = 0;
#endif
	{
		float	phi = (angleIndex + noise) * PI / MAX_ANGLES;
		float2	ssDirection;
		sincos( phi, ssDirection.y, ssDirection.x );

		// Gather irradiance and average cone direction for that slice
		float3	ssBentNormal;
		float2	coneAngles;
		sumIrradiance += GatherIrradiance_DEBUG( __Position.xy, ssDirection, Z, N, radiusStepSize_pixels, samplesCount, ssBentNormal, coneAngles, GATHER_DEBUG );

		ssAverageBentNormal += ssBentNormal;

		// We're using running variance computation from https://www.johndcook.com/blog/standard_deviation/
		//	Avg(N) = Avg(N-1) + [V(N) - Avg(N-1)] / N
		//	S(N) = S(N-1) + [V(N) - Avg(N-1)] * [V(N) - Avg(N)]
		// And variance = S(finalN) / (finalN-1)
		//
		float	previousAverageConeAngle = averageConeAngle;
		averageConeAngle += (coneAngles.x - averageConeAngle) / (2*angleIndex+1);
		varianceConeAngle += (coneAngles.x - previousAverageConeAngle) * (coneAngles.x - averageConeAngle);

		previousAverageConeAngle = averageConeAngle;
		averageConeAngle += (coneAngles.y - averageConeAngle) / (2*angleIndex+2);
		varianceConeAngle += (coneAngles.y - previousAverageConeAngle) * (coneAngles.y - averageConeAngle);
	}

	// Finalize bent cone & irradiance
	#if MAX_ANGLES > 1
		varianceConeAngle /= 2.0*MAX_ANGLES - 1.0;
	#endif
	ssAverageBentNormal = normalize( ssAverageBentNormal );
	float	stdDeviation = sqrt( varianceConeAngle );

	sumIrradiance *= PI / MAX_ANGLES;

float3	DEBUG_VALUE = float3( 1,0,1 );
DEBUG_VALUE = ssAverageBentNormal;
//DEBUG_VALUE = cos( averageConeAngle );
//DEBUG_VALUE = dot( ssAverageBentNormal, N );
//DEBUG_VALUE = 0.01 * Z;
//DEBUG_VALUE = sphereRadius_pixels / GATHER_SPHERE_MAX_RADIUS_P;
//DEBUG_VALUE = 0.1 * (radiusStepSize_pixels-1);
//DEBUG_VALUE = 0.5 * float(samplesCount) / MAX_SAMPLES;
//DEBUG_VALUE = varianceConeAngle;
DEBUG_VALUE = stdDeviation;
//DEBUG_VALUE = float3( GATHER_DEBUG.xy, 0 );
//DEBUG_VALUE = float3( GATHER_DEBUG.zw, 0 );
//DEBUG_VALUE = GATHER_DEBUG.xyz;
//DEBUG_VALUE = N;

	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0 );
	Out.bentCone = float4( max( 0.01, cos( averageConeAngle ) ) * ssAverageBentNormal, 1.0 - stdDeviation / (0.5 * PI) );

//Out.bentCone = float4( DEBUG_VALUE, 1 );

	return Out;
}

////////////////////////////////////////////////////////////////////////////////
// Reprojects last frame's radiance
float4	PS_Reproject( float4 __Position : SV_POSITION ) : SV_TARGET0 {
	return _tex_sourceRadiance[uint2(floor(__Position.xy))];	// #TODO!
}
