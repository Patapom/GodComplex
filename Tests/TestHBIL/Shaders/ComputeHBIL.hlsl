////////////////////////////////////////////////////////////////////////////////
// Shaders to compute HBIL
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "HBIL.hlsl"

static const float	GATHER_SPHERE_MAX_RADIUS_P = 100.0;	// Maximum radius (in pixels) that we allow our sphere to get

#define MAX_ANGLES	8									// Amount of circle subdivisions per pixel
#define MAX_SAMPLES	32									// Maximum amount of samples per circle subdivision

Texture2D< float4 >	_tex_sourceRadiance : register(t0);
Texture2D< float4 >	_tex_normal : register(t1);
Texture2D< float >	_tex_depth : register(t2);			// Depth or distance buffer (here we're given distances)
Texture2D< float >	_tex_blueNoise : register(t3);

cbuffer CB_HBIL : register( b3 ) {
	float	_gatherSphereMaxRadius_m;		// Radius of the sphere that will gather our irradiance samples (in meters)
	float2	_bilateralValues;
};

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

////////////////////////////////////////////////////////////////////////////////
// Implement the methods expected by the HBIL header
float	FetchDepth( float2 _pixelPosition ) {
	return Z_FAR * _tex_depth.SampleLevel( LinearClamp, _pixelPosition / _resolution, 0.0 );
}

float3	FetchRadiance( float2 _pixelPosition ) {
	return _tex_sourceRadiance.SampleLevel( LinearClamp, _pixelPosition / _resolution, 0.0 ).xyz;
}

float	BilateralFilterDepth( float _centralZ, float _neighborZ, float _radius_m ) {
//return 1.0;	// Accept always

	float	deltaZ = _neighborZ - _centralZ;

	#if 1
		// Relative test
		float	relativeZ = abs( deltaZ ) / _centralZ;
//		return smoothstep( _bilateralValues.y, _bilateralValues.x, relativeZ );
		return smoothstep( 0.4, 0.0, relativeZ );	// Discard when deltaZ is larger than 50% central Z (empirical value)
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

	// Read back last frame's radiance value that we always can use as a default for neighbor areas
	float3	centralRadiance = _tex_sourceRadiance[pixelPosition].xyz;

	// Face-cam normal
	float3	wsRight = normalize( cross( wsView, _World2Camera[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	N = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), -dot( wsNormal, wsView ) );	// Camera-space normal
	float3	T, B;
	BuildOrthonormalBasis( N, T, B );

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

//phi = 0.0;

		float2	ssDirection;
		sincos( phi, ssDirection.y, ssDirection.x );

		// Gather irradiance and average cone direction for that slice
		float3	ssBentNormal;
		float2	coneAngles;
		sumIrradiance += GatherIrradiance( __Position.xy, ssDirection, Z, N, float2( radiusStepSize_pixels, radiusStepSize_meters ), samplesCount, centralRadiance, ssBentNormal, coneAngles, GATHER_DEBUG );

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

//sumIrradiance += float3( 0.1, 0, 0 );

	sumIrradiance = max( 0.0, sumIrradiance );

//sumIrradiance = float3( 1, 0, 1 );

float3	DEBUG_VALUE = float3( 1,0,1 );
DEBUG_VALUE = ssAverageBentNormal;
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
//DEBUG_VALUE = GATHER_DEBUG.xyz;
//DEBUG_VALUE = 2.0 * Z;

	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0 );
	Out.bentCone = float4( max( 0.01, cos( averageConeAngle ) ) * ssAverageBentNormal, 1.0 - stdDeviation / (0.5 * PI) );

//Out.bentCone = float4( DEBUG_VALUE, 1 );

	return Out;
}
