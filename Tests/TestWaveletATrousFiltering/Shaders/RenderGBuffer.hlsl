//
//
#include "Global.hlsl"
#include "DistanceField.hlsl"

struct PS_OUT {
	float4	_albedo_gloss		: SV_TARGET0;
	float4	_normal_distance	: SV_TARGET1;
};

PS_OUT	PS( VS_IN _In ) {
	float2	UV = _In.__Position.xy / _resolution;
	float	aspectRatio = float( _resolution.x ) / _resolution.y;
	float3	csView = float3( TAN_HALF_FOV * aspectRatio * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;

	PS_OUT	Out;
	Out._albedo_gloss = 0.0;
	Out._normal_distance = float4( 0, 0, 0, INFINITY );

	float2	distance = Trace( wsPos, wsView, 0.5, 100 );
	float	D = distance.x;//length( _Camera2World[3].xyz - wsPos );
	if ( D > 100.0 )
		return Out;

	wsPos += distance.x * wsView;

	float3	wsNormal = Normal( wsPos );
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );
	float3	albedo = Albedo( wsPos, distance.y );

	Out._albedo_gloss = float4( albedo, 0.0 );
	Out._normal_distance = float4( wsNormal, distance.x );

// 	float3	lighting = 0.0;
// 	#if 1
// 		const float3	LIGHT_ILLUMINANCE = 50.0;
// 		float3			LIGHT_SIZE = float3( _lightSize, 0.0, 1.3/1.05 * _lightSize );
// 
// 		const uint	SCENE_SAMPLES = 8;
// 		for ( uint sampleIndex=0; sampleIndex < SCENE_SAMPLES; sampleIndex++ ) {
// 			float	randPhi = float(sampleIndex) / SCENE_SAMPLES;
// 			float	randTheta = ReverseBits( sampleIndex + uint( _In.__Position.x * _In.__Position.y ) );
// 			float	cosTheta = randTheta;
// 			float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
// 			float2	scPhi;
// 			sincos( randPhi * 2.0 * PI, scPhi.x, scPhi.y );
// 
// 			float3	lsRayDir = float3( scPhi.x * sinTheta, scPhi.y * sinTheta, cosTheta );
// 			float3	wsRayDir = lsRayDir.x * wsTangent + lsRayDir.y * wsBiTangent + lsRayDir.z * wsNormal;
// 
// 
// 			float2	sceneHitDistance = Trace( wsPos, wsRayDir, 0.01, 60 );
//  			if ( sceneHitDistance.x > 20.0 )
//  				continue;
// 
// 			float3	wsSceneHitPos = wsPos + sceneHitDistance.x * wsRayDir;
// 			float3	wsSceneHitNormal = Normal( wsSceneHitPos );
// 			float3	sceneHitAlbedo = Albedo( wsSceneHitPos, sceneHitDistance.y );
// 			float3	wsLightPos = CORNELL_LIGHT_POS + float3( randPhi - 0.5, 0.0, randTheta - 0.5 ) * LIGHT_SIZE;
// //float3	wsLightPos = CORNELL_LIGHT_POS;
// 			float3	wsLight = wsLightPos - wsSceneHitPos;
// 			float	distance2Light = length( wsLight );
// 					wsLight /= distance2Light;
// 			float	shadow = ShadowTrace( wsPos, wsLightPos, 100 );
// 			float3	sceneLighting = (INVPI * sceneHitAlbedo) * saturate( dot( wsSceneHitNormal, wsLight ) ) * shadow * LIGHT_ILLUMINANCE / (distance2Light * distance2Light);
// 
// //			lighting += (INVPI * albedo) * sceneLighting * saturate( dot( wsNormal, wsRayDir ) );
// 			lighting += (INVPI * albedo) * sceneLighting;
// 		}
// 		lighting /= SCENE_SAMPLES;
// 	#else
// 		const uint	LIGHT_SAMPLES = 8;
// 		for ( uint lightSampleIndex=0; lightSampleIndex < LIGHT_SAMPLES; lightSampleIndex++ ) {
// 			float	jitter = ReverseBits( lightSampleIndex + uint( _In.__Position.x * _In.__Position.y ) );
// 			float2	lightUV = float2( float(lightSampleIndex) / LIGHT_SAMPLES, jitter );
// 	//		float3	wsLightPos = CORNELL_LIGHT_POS + float3( lightUV.x, 0.0, lightUV.y ) * CORNELL_LIGHT_SIZE;
// 			float3	wsLightPos = CORNELL_LIGHT_POS + float3( lightUV.x, 0.0, lightUV.y ) * float3( _lightSize, 0.0, 1.3/1.05 * _lightSize );
// 	//float3	wsLightPos = float3( 0, 5, 1 );//float3( 1.0, 5.5 + 2.5 * sin( 4.0 * _time ), 1.0 );
// 			float	shadow = ShadowTrace( wsPos, wsLightPos, 100 );
// 			lighting += shadow;
// 		}
// 		lighting /= LIGHT_SAMPLES;
// 
// //lighting = ReverseBits( uint( _In.__Position.x * _In.__Position.y ) );
// 	#endif

	return Out;
}