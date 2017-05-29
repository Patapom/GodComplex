//
//
#include "Global.hlsl"

cbuffer CB_PostProcess : register(b10) {
	float	_lightSize;
};

static const float3	CORNELL_SIZE = float3( 5.528f, 5.488f, 5.592f );
static const float3	CORNELL_POS = 0.0;
static const float	CORNELL_THICKNESS = 0.1f;

static const float3	CORNELL_SMALL_BOX_SIZE = float3( 1.65, 1.65, 1.65 );	// It's a cube
static const float3	CORNELL_SMALL_BOX_POS = float3( 1.855, 0.5 * CORNELL_SMALL_BOX_SIZE.y, 1.69 ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float	CORNELL_SMALL_BOX_ANGLE = 0.29145679447786709199560462143289;	// ~16°

static const float3	CORNELL_LARGE_BOX_SIZE = float3( 1.65, 3.3, 1.65 );
static const float3	CORNELL_LARGE_BOX_POS = float3( 3.685, 0.5 * CORNELL_LARGE_BOX_SIZE.y, 3.5125 ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );
static const float	CORNELL_LARGE_BOX_ANGLE = -0.30072115015043337195437489062082;	// ~17°

static const float3	CORNELL_LIGHT_SIZE = float3( 1.3, 0.0, 1.05 );
static const float3	CORNELL_LIGHT_POS = float3( 2.78, 5.2, 2.795 ) - 0.0 * float3( CORNELL_LIGHT_SIZE.x, 0.0, CORNELL_LIGHT_SIZE.z ) - 0.5 * float3( CORNELL_SIZE.x, 0.0, CORNELL_SIZE.z );

float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize ) {
	_wsPosition -= _wsBoxCenter;
	_wsBoxSize *= 0.5;

	float x = max( _wsPosition.x - _wsBoxSize.x, -_wsPosition.x - _wsBoxSize.x );
	float y = max( _wsPosition.y - _wsBoxSize.y, -_wsPosition.y - _wsBoxSize.y );
	float z = max( _wsPosition.z - _wsBoxSize.z, -_wsPosition.z - _wsBoxSize.z );
	return max( max( x, y ), z );
}
float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize, float _rotationAngle ) {
	float	s = sin( _rotationAngle );
	float	c = cos( _rotationAngle );
	float3	rotatedX = float3( c, 0.0, s );
	float3	rotatedZ = float3( -s, 0.0, c );

	_wsPosition -= _wsBoxCenter;
	_wsPosition = float3( dot( _wsPosition, rotatedX ), _wsPosition.y, dot( _wsPosition, rotatedZ ) );
	return DistBox( _wsPosition, 0.0, _wsBoxSize );
}
float2	DistMin( float2 a, float2 b ) {
	return a.x < b.x ? a : b;
}

float2	Map( float3 _wsPosition );

float3	Normal( float3 _wsPosition ) {
	const float	eps = 0.001;

	_wsPosition.x += eps;
	float	Dx = Map( _wsPosition ).x;
	_wsPosition.x -= 2.0 * eps;
	Dx -= Map( _wsPosition ).x;
	_wsPosition.x += eps;

	_wsPosition.y += eps;
	float	Dy = Map( _wsPosition ).x;
	_wsPosition.y -= 2.0 * eps;
	Dy -= Map( _wsPosition ).x;
	_wsPosition.y += eps;

	_wsPosition.z += eps;
	float	Dz = Map( _wsPosition ).x;
	_wsPosition.z -= 2.0 * eps;
	Dz -= Map( _wsPosition ).x;
//	_wsPosition.x += eps;

// 	Dx *= 2.0 / eps;
// 	Dy *= 2.0 / eps;
// 	Dz *= 2.0 / eps;

	return normalize( float3( Dx, Dy, Dz ) );
}

/// <summary>
/// Maps a world-space position into a distance to the nearest object in the scene
/// </summary>
/// <param name="_wsPosition"></param>
/// <returns></returns>
float2	Map( float3 _wsPosition ) {
	// Walls
	float2	distance = float2( DistBox( _wsPosition, float3( 0, 0, 0 ), float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 1.0 );	// Floor
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0, CORNELL_SIZE.y, 0 ), float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 2.0 ) );	// Ceiling
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( -0.5f * CORNELL_SIZE.x, 0.5f * CORNELL_SIZE.y, 0 ), float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 3.0 ) );	// Left wall
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0.5f * CORNELL_SIZE.x, 0.5f * CORNELL_SIZE.y, 0 ), float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 4.0 ) );	// Right wall
			distance = DistMin( distance, float2( DistBox( _wsPosition, float3( 0, 0.5f * CORNELL_SIZE.y, 0.5f * CORNELL_SIZE.z ), float3( CORNELL_SIZE.x, CORNELL_SIZE.y, CORNELL_THICKNESS ) ), 5.0 ) );	// Back wall

	// Small box
	distance = DistMin( distance, float2( DistBox( _wsPosition, CORNELL_SMALL_BOX_POS, CORNELL_SMALL_BOX_SIZE, CORNELL_SMALL_BOX_ANGLE ), 6.0 ) );

	// Large box
	distance = DistMin( distance, float2( DistBox( _wsPosition, CORNELL_LARGE_BOX_POS, CORNELL_LARGE_BOX_SIZE, CORNELL_LARGE_BOX_ANGLE ), 7.0 ) );

	return distance;
}

float3	Albedo( float3 _wsPosition, float _materialID ) {
	switch ( (int) _materialID ) {
		case 3:		return 0.6 * float3( 0.2, 0.5, 1.0 ); break;
		case 4:		return 0.6 * float3( 1.0, 0.1, 0.01 ); break;
		default:	return 0.6; break;
	}
}

// Traces a ray from source to target and returns the visibility
float	ShadowTrace( float3 _wsSource, float3 _wsTarget, const uint _stepsCount ) {
	float4	wsPos = float4( _wsSource, 0.0 );
	_wsTarget -= _wsSource;
	float4	wsView = float4( _wsTarget, length( _wsTarget ) );
	float	remainingDistance = wsView.w;
			wsView /= remainingDistance;

	for ( uint i=0; i < _stepsCount; i++ ) {
		float	distance = Map( wsPos.xyz ).x;
		if ( distance.x > remainingDistance )
			return 1.0;	// Unoccluded!
		wsPos += distance * wsView;
		remainingDistance -= distance;
	}

	return 0.0;
}

// Traces a ray into the scene and returns hit distance and material ID
float2	Trace( float3 _wsPos, float3 _wsDir, float _initialDistance, const uint _stepsCount ) {
	float4	wsPos = float4( _wsPos, 0.0 );
	float4	wsView = float4( _wsDir, 1.0 );
	float2	distance = float2( _initialDistance, -1 );
	for ( uint i=0; i < _stepsCount; i++ ) {
		wsPos += distance.x * wsView;
		distance = Map( wsPos.xyz );
		if ( distance.x < 0.01 )
			break;
	}
	distance.x = wsPos.w;
	return distance;
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _resolution;

// 	float3	projView = float3( 2.0 * UV - 1.0, 0.0 );
// 	float4	wsView_ = mul( float4( projView, 1.0 ), _Proj2World );
// //	float3	wsView = normalize( wsView_.xyz / wsView_.w );
// 	float3	wsView = normalize( wsView_.xyz );

	float	aspectRatio = float( _resolution.x ) / _resolution.y;
	float3	csView = float3( TAN_HALF_FOV * aspectRatio * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;
// return float4( wsView, 1.0 );
// return float4( UV, 0, 1 );

// 	float2	distance = float2( 0.5, -1 );
// 	for ( uint i=0; i < 100; i++ ) {
// 		wsPos += distance.x * wsView;
// 		distance = Map( wsPos );
// 		if ( distance.x < 0.01 )
// 			break;
// 	}
	float2	distance = Trace( wsPos, wsView, 0.5, 100 );
	float	D = distance.x;//length( _Camera2World[3].xyz - wsPos );
	if ( D > 100.0 )
		return 0.0;

	wsPos += distance.x * wsView;

	float3	wsNormal = Normal( wsPos );
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );
	float3	albedo = Albedo( wsPos, distance.y );

// return float4( albedo, 1.0 );
// return float4( wsNormal, 1.0 );
// return 0.1 * D;

	float3	lighting = 0.0;
	#if 1
		const float3	LIGHT_ILLUMINANCE = 50.0;
		float3			LIGHT_SIZE = float3( _lightSize, 0.0, 1.3/1.05 * _lightSize );

		const uint	SCENE_SAMPLES = 64;
		for ( uint sampleIndex=0; sampleIndex < SCENE_SAMPLES; sampleIndex++ ) {
			float	randPhi = float(sampleIndex) / SCENE_SAMPLES;
			float	randTheta = ReverseBits( sampleIndex + uint( _In.__Position.x * _In.__Position.y ) );
			float	cosTheta = randTheta;
			float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
			float2	scPhi;
			sincos( randPhi * 2.0 * PI, scPhi.x, scPhi.y );

			float3	lsRayDir = float3( scPhi.x * sinTheta, scPhi.y * sinTheta, cosTheta );
			float3	wsRayDir = lsRayDir.x * wsTangent + lsRayDir.y * wsBiTangent + lsRayDir.z * wsNormal;


			float2	sceneHitDistance = Trace( wsPos, wsRayDir, 0.01, 60 );
 			if ( sceneHitDistance.x > 20.0 )
 				continue;

			float3	wsSceneHitPos = wsPos + sceneHitDistance.x * wsRayDir;
			float3	wsSceneHitNormal = Normal( wsSceneHitPos );
			float3	sceneHitAlbedo = Albedo( wsSceneHitPos, sceneHitDistance.y );
			float3	wsLightPos = CORNELL_LIGHT_POS + float3( randPhi - 0.5, 0.0, randTheta - 0.5 ) * LIGHT_SIZE;
//float3	wsLightPos = CORNELL_LIGHT_POS;
			float3	wsLight = wsLightPos - wsSceneHitPos;
			float	distance2Light = length( wsLight );
					wsLight /= distance2Light;
			float	shadow = ShadowTrace( wsPos, wsLightPos, 50 );
			float3	sceneLighting = (INVPI * sceneHitAlbedo) * saturate( dot( wsSceneHitNormal, wsLight ) ) * shadow * LIGHT_ILLUMINANCE / (distance2Light * distance2Light);

//			lighting += (INVPI * albedo) * sceneLighting * saturate( dot( wsNormal, wsRayDir ) );
			lighting += (INVPI * albedo) * sceneLighting;
		}
		lighting /= SCENE_SAMPLES;
	#else
		const uint	LIGHT_SAMPLES = 64;
		for ( uint lightSampleIndex=0; lightSampleIndex < LIGHT_SAMPLES; lightSampleIndex++ ) {
			float	jitter = ReverseBits( lightSampleIndex + uint( _In.__Position.x * _In.__Position.y ) );
			float2	lightUV = float2( float(lightSampleIndex) / LIGHT_SAMPLES, jitter );
	//		float3	wsLightPos = CORNELL_LIGHT_POS + float3( lightUV.x, 0.0, lightUV.y ) * CORNELL_LIGHT_SIZE;
			float3	wsLightPos = CORNELL_LIGHT_POS + float3( lightUV.x, 0.0, lightUV.y ) * float3( _lightSize, 0.0, 1.3/1.05 * _lightSize );
	//float3	wsLightPos = float3( 0, 5, 1 );//float3( 1.0, 5.5 + 2.5 * sin( 4.0 * _time ), 1.0 );
			float	shadow = ShadowTrace( wsPos, wsLightPos, 50 );
			lighting += shadow;
		}
		lighting /= LIGHT_SAMPLES;

//lighting = ReverseBits( uint( _In.__Position.x * _In.__Position.y ) );
	#endif

	return float4( lighting, 1.0 );
}