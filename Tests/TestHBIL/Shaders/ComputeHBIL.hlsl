////////////////////////////////////////////////////////////////////////////////
// Shaders to compute HBIL
////////////////////////////////////////////////////////////////////////////////
//
//#define AVERAGE_COSINES 1

#include "Global.hlsl"
#include "HBIL.hlsl"

static const float	GATHER_SPHERE_MAX_RADIUS_P = 100.0;	// Maximum radius (in pixels) that we allow our sphere to get

#define MAX_ANGLES	1									// Amount of circle subdivisions per pixel
#define MAX_SAMPLES	32									// Maximum amount of samples per circle subdivision

Texture2D< float >	_tex_depth : register(t0);			// Depth or distance buffer (here we're given depth)
Texture2D< float4 >	_tex_sourceRadiance : register(t1);	// Last frame's reprojected radiance buffer
Texture2D< float4 >	_tex_normal : register(t2);			// Camera-space normal vectors
Texture2D< float >	_tex_blueNoise : register(t3);

cbuffer CB_HBIL : register( b3 ) {
	float	_gatherSphereMaxRadius_m;		// Radius of the sphere that will gather our irradiance samples (in meters)
	float2	_bilateralValues;
};

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

////////////////////////////////////////////////////////////////////////////////
// Implement the methods expected by the HBIL header
float	FetchDepth( float2 _pixelPosition, float _mipLevel ) {
//	return _tex_sourceRadiance.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel ).w;
	return Z_FAR * _tex_depth.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel );
}

float3	FetchRadiance( float2 _pixelPosition, float _mipLevel ) {
	return _tex_sourceRadiance.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel ).xyz;
}

float	BilateralFilterDepth( float _centralZ, float _neighborZ, float _radius_m ) {
//return 1.0;	// Accept always

	float	deltaZ = _neighborZ - _centralZ;

	#if 1
		// Relative test
		float	relativeZ = abs( deltaZ ) / _centralZ;
//		return smoothstep( _bilateralValues.y, _bilateralValues.x, relativeZ );
		return smoothstep( 0.4, 0.0, relativeZ );	// Discard when deltaZ is larger than 40% central Z (empirical value)
	#elif 0
		// Absolute test
		return smoothstep( _bilateralValues.y, _bilateralValues.x, abs(deltaZ) );
		return smoothstep( 1.0, 0.0, abs(deltaZ) );
	#else
		// Reject if outside of gather sphere radius
		float	r = _radius_m / _gatherSphereMaxRadius_m;
		float	sqSphereZ = 1.0 - r*r;
		return smoothstep( _bilateralValues.y*_bilateralValues.y * sqSphereZ, _bilateralValues.x*_bilateralValues.x * sqSphereZ, deltaZ*deltaZ );
		return smoothstep( 0.1*0.1 * sqSphereZ, 0.0 * sqSphereZ, deltaZ*deltaZ );	// Empirical values
	#endif
}
float	BilateralFilterRadiance( float _centralZ, float _neighborZ, float _radius_m ) {
//return 1.0;	// Accept always
//return 0.0;	// Reject always

	float	deltaZ = _neighborZ - _centralZ;

	#if 1
		// Relative test
		float	relativeZ = abs( deltaZ ) / _centralZ;
//		return smoothstep( 0.1*_bilateralValues.y, 0.1*_bilateralValues.x, relativeZ );	// Discard when deltaZ is larger than 1% central Z
//		return smoothstep( 0.015, 0.0, relativeZ );	// Discard when deltaZ is larger than 1.5% central Z (empirical value)
		return smoothstep( 0.15, 0.0, relativeZ );	// Discard when deltaZ is larger than 1.5% central Z (empirical value)
	#elif 0
		// Absolute test
		return smoothstep( 1.0, 0.0, abs(deltaZ) );
	#else
		// Reject if outside of gather sphere radius
		float	r = _radius_m / _gatherSphereMaxRadius_m;
		float	sqSphereZ = 1.0 - r*r;
		return smoothstep( _bilateralValues.y*_bilateralValues.y * sqSphereZ, _bilateralValues.x*_bilateralValues.x * sqSphereZ, deltaZ*deltaZ );
		return smoothstep( 0.1*0.1 * sqSphereZ, 0.0 * sqSphereZ, deltaZ*deltaZ );	// Empirical values
	#endif
}


float2	ComputeMipLevel( float2 _radius, float2 _radialStepSizes ) {
return 0;
	float	radiusPixel = _radius.x;
	float	deltaRadius = _radialStepSizes.x;
	float	pixelArea = PI / (2.0 * MAX_ANGLES) * 2.0 * radiusPixel * deltaRadius;
//	return 0.5 * log2( pixelArea ) * _bilateralValues;
	return 0.5 * log2( pixelArea ) * float2( 0, 2 );	// Unfortunately, sampling lower mips for depth gives very nasty halos! Maybe use max depth? Meh. Not conclusive either...
	return 1.5 * 0.5 * log2( pixelArea );
	return 0.5 * log2( pixelArea );
}


float3	GatherIrradiance_TEMP( float2 _ssPosition, float2 _ssDirection, float _Z0, float3 _csNormal, float2 _radialStepSizes, uint _stepsCount, float3 _centralRadiance, out float3 _ssBentNormal, out float2 _coneAngles, inout float4 _DEBUG ) {

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
	float	hitDistance_Front = -dot( _ssDirection, _csNormal.xy ) * (abs(_csNormal.z) > 1e-6 ? 1.0 / _csNormal.z : 0.0);
//	float	maxCos_Front = hitDistance_Front / sqrt( hitDistance_Front*hitDistance_Front + dot(_ssDirection,_ssDirection) );
	float	maxCos_Front = hitDistance_Front / sqrt( hitDistance_Front*hitDistance_Front + 1.0 );
	float	maxCos_Back = -maxCos_Front;	// Back cosine is simply the mirror value

	// Gather irradiance from front & back directions while updating the horizon angles at the same time
	float3	sumRadiance = 0.0;
	float2	radius = 0.0;
	float2	ssStep = _radialStepSizes.x * _ssDirection;
	float2	ssPosition_Front = _ssPosition;
	float2	ssPosition_Back = _ssPosition;
	float3	previousRadiance_Front = _centralRadiance;
	float3	previousRadianceBack = _centralRadiance;
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		radius += _radialStepSizes;
		ssPosition_Front += ssStep;
		ssPosition_Back -= ssStep;

		float2	mipLevel = ComputeMipLevel( radius, _radialStepSizes );

		sumRadiance += SampleIrradiance( ssPosition_Front, _Z0, radius.y, mipLevel, integralFactors_Front, previousRadiance_Front, maxCos_Front );
		sumRadiance += SampleIrradiance( ssPosition_Back, _Z0, radius.y, mipLevel, integralFactors_Back, previousRadianceBack, maxCos_Back );
	}

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


#if AVERAGE_COSINES
_coneAngles = float2( saturate( dot( _ssBentNormal, ssHorizon_Front ) ), saturate( dot( _ssBentNormal, ssHorizon_Back ) ) );
#endif


//_DEBUG = float4( _coneAngles, 0, 0 );
_DEBUG = _coneAngles.x / (0.5*PI);


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
	float	Z = FetchDepth( pixelPosition, 0.0 );
	float	distance = Z * Z2Distance;
	float3	wsNormal = normalize( _tex_normal[pixelPosition].xyz );

	// Read back last frame's radiance value that we always can use as a default for neighbor areas
	float3	centralRadiance = _tex_sourceRadiance[pixelPosition].xyz;

	#if 0
		// Z-Plane
		float3	wsRight = _Camera2World[0].xyz;
		float3	wsUp = _Camera2World[1].xyz;
		float3	wsAt = _Camera2World[2].xyz;
	#else
		// Compute face-cam normal
		float3	wsRight = normalize( cross( wsView, _Camera2World[1].xyz ) );
		float3	wsUp = cross( wsRight, wsView );
		float3	wsAt = wsView;
	#endif
	float3	N = float3( dot( wsNormal, wsRight ), -dot( wsNormal, wsUp ), -dot( wsNormal, wsAt ) );	// Camera-space normal

//	float3	T, B;
//	BuildOrthonormalBasis( N, T, B );

	Z -= 1e-2;	// Prevent acnea by offseting the central depth closer

	// Compute screen radius of gather sphere
	float	screenSize_m = 2.0 * Z * TAN_HALF_FOV;	// Vertical size of the screen in meters when extended to distance Z
	float	sphereRadius_pixels = _resolution.y * _gatherSphereMaxRadius_m / screenSize_m;
			sphereRadius_pixels = min( GATHER_SPHERE_MAX_RADIUS_P, sphereRadius_pixels );							// Prevent it to grow larger than our fixed limit
	float	radiusStepSize_pixels = max( 1.0, sphereRadius_pixels / MAX_SAMPLES );									// This gives us our radial step size in pixels
	uint	samplesCount = clamp( uint( ceil( sphereRadius_pixels / radiusStepSize_pixels ) ), 1, MAX_SAMPLES );	// Reduce samples count if possible
	float	radiusStepSize_meters = sphereRadius_pixels * screenSize_m / (samplesCount * _resolution.y);			// This gives us our radial step size in meters

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

phi = 0.0;

		float2	ssDirection;
		sincos( phi, ssDirection.y, ssDirection.x );

		// Gather irradiance and average cone direction for that slice
		float3	ssBentNormal;
		float2	coneAngles;
		sumIrradiance += GatherIrradiance_TEMP( __Position.xy, ssDirection, Z, N, float2( radiusStepSize_pixels, radiusStepSize_meters ), samplesCount, centralRadiance, ssBentNormal, coneAngles, GATHER_DEBUG );

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



#if AVERAGE_COSINES
averageConeAngle = acos( averageConeAngle );
varianceConeAngle = acos( varianceConeAngle );
#endif



	sumIrradiance *= PI / MAX_ANGLES;

//sumIrradiance += float3( 0.1, 0, 0 );

	sumIrradiance = max( 0.0, sumIrradiance );

//sumIrradiance = float3( 1, 0, 1 );

float3	DEBUG_VALUE = float3( 1,0,1 );
DEBUG_VALUE = ssAverageBentNormal;
DEBUG_VALUE = ssAverageBentNormal.x * wsRight - ssAverageBentNormal.y * wsUp - ssAverageBentNormal.z * wsAt;	// World-space normal
//DEBUG_VALUE = cos( averageConeAngle );
//DEBUG_VALUE = dot( ssAverageBentNormal, N );
//DEBUG_VALUE = 0.01 * Z;
//DEBUG_VALUE = sphereRadius_pixels / GATHER_SPHERE_MAX_RADIUS_P;
//DEBUG_VALUE = 0.1 * (radiusStepSize_pixels-1);
//DEBUG_VALUE = 0.5 * float(samplesCount) / MAX_SAMPLES;
//DEBUG_VALUE = varianceConeAngle;
//DEBUG_VALUE = stdDeviation;
//DEBUG_VALUE = float3( GATHER_DEBUG.xy, 0 );
//DEBUG_VALUE = float3( GATHER_DEBUG.zw, 0 );
DEBUG_VALUE = GATHER_DEBUG.xyz;
//DEBUG_VALUE = N;

	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0 );
	Out.bentCone = float4( max( 0.01, cos( averageConeAngle ) ) * ssAverageBentNormal, 1.0 - stdDeviation / (0.5 * PI) );

Out.bentCone = float4( DEBUG_VALUE, 1 );

	return Out;
}
