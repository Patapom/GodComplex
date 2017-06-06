//
//
#include "Global.hlsl"
#include "DistanceField.hlsl"

cbuffer CB_PostProcess : register(b10) {
	float	_lightSize;
};

Texture2DArray<float4>	_Tex_GBuffer : register(t0);
Texture2D<float4>		_Tex_BlueNoise : register(t1);


static const uint	SCENE_SAMPLES = 4;


float4	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _resolution;

	float4	noise = _Tex_BlueNoise[uint2( _In.__Position.xy ) & 0x3F];

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


// 	return float4( 0.1 * _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).www, 1 );
// 	return float4( _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).xyz, 1 );

//*

// 	float2	distance = Trace( wsPos, wsView, 0.5, 100 );
//	float	D = distance.x;//length( _Camera2World[3].xyz - wsPos );
// 	if ( D > 100.0 )
// 		return 0.0;
// 
// 	wsPos += distance.x * wsView;
// 
// 	float3	albedo = Albedo( wsPos, distance.y );
// 	float3	wsNormal = Normal( wsPos );

	float4	albedo_gloss = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 0.0 ), 0.0 );
	float4	normal_distance = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 );
	float3	albedo = albedo_gloss.xyz;
	float3	wsNormal = normal_distance.xyz;
	float	distance = normal_distance.w;
	if ( distance > 100.0 )
		return 0.0;
 	wsPos += distance * wsView;

	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

//return float4( 0.25 * wsPos, 1.0 );
// return float4( albedo, 1.0 );
// return float4( wsNormal, 1.0 );
// return 0.1 * D;

	float3	lighting = 0.0;
	#if 1
		wsPos += 0.001 * wsNormal;	// Offset a little off the wall to avoid acnea

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Add indirect lighting contribution
		[loop]
		for ( uint sampleIndex=0; sampleIndex < SCENE_SAMPLES; sampleIndex++ ) {
			float	X0 = float(sampleIndex) / SCENE_SAMPLES;
			float	X1 = ReverseBits( sampleIndex );
			float	phi = 2.0 * PI * (X0 + noise.x);
			float2	sinCosPhi;
			sincos( phi, sinCosPhi.x, sinCosPhi.y );

			float	cosTheta = sqrt( X1 );
			float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );

			float3	lsRayDir = float3( sinTheta * sinCosPhi, cosTheta );
			float3	wsRayDir = lsRayDir.x * wsTangent + lsRayDir.y * wsBiTangent + lsRayDir.z * wsNormal;

			float2	sceneHitDistance = Trace( wsPos, wsRayDir, 0.0, 100 );
 			if ( sceneHitDistance.x  > 10.0 )
 				continue;	// No hit...

			// Retrieve scene information at hit
			float3	wsSceneHitPos = wsPos + sceneHitDistance.x * wsRayDir;
			float3	wsSceneHitNormal = Normal( wsSceneHitPos );
			float3	sceneHitAlbedo = Albedo( wsSceneHitPos, sceneHitDistance.y );

			// Trace shadow ray to light
// 			float3	wsLightPos = CORNELL_LIGHT_POS + float3( X0 * noise.y - 0.5, 0.0, X1 * noise.z - 0.5 ) * LIGHT_SIZE;
float3	wsLightPos = CORNELL_LIGHT_POS;
			float3	wsLight = wsLightPos - wsSceneHitPos;
			float	distance2Light = length( wsLight );
					wsLight /= distance2Light;

			wsSceneHitPos += (0.01 / dot( wsLight, wsSceneHitNormal )) * wsLight;	// Offset a bit from the surface

//			float	shadow = ShadowTrace( wsSceneHitPos, wsLightPos, 100 );
			float2	shadowDistance = Trace( wsSceneHitPos, wsLight, 0.0, 100 );
			float	shadow = smoothstep( 0.95, 1.0, shadowDistance.x / distance2Light );
					shadow *= saturate( wsLight.y );	// saturate( -dot( wsLight, float3( 0, -1, 0 ) ) ) assuming the light is emitting toward the bottom

			// Compute lighting
			float3	sceneRadiance = (INVPI * sceneHitAlbedo) * saturate( dot( wsSceneHitNormal, wsLight ) ) * shadow * LIGHT_ILLUMINANCE / (distance2Light * distance2Light);

//			lighting += (INVPI * albedo) * sceneRadiance * saturate( dot( wsNormal, wsRayDir ) );
			lighting += sceneRadiance * cosTheta;
		}
		lighting *= 2.0 * PI / SCENE_SAMPLES;


		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Add direct lighting contribution
// 		float3	wsLightPos = CORNELL_LIGHT_POS + float3( noise.y - 0.5, 0.0, noise.z - 0.5 ) * LIGHT_SIZE;
float3	wsLightPos = CORNELL_LIGHT_POS;
		float3	wsLight = wsLightPos - wsPos;
		float	distance2Light = length( wsLight );
				wsLight /= distance2Light;
		float	shadow = ShadowTrace( wsPos + saturate( 0.01 / dot( wsLight, wsNormal ) ) * wsLight, wsLightPos, 100 );
				shadow *= saturate( wsLight.y );	// saturate( -dot( wsLight, float3( 0, -1, 0 ) ) ) assuming the light is emitting toward the bottom

		lighting += saturate( dot( wsNormal, wsLight ) ) * shadow * LIGHT_ILLUMINANCE / (distance2Light * distance2Light);

		lighting *= (INVPI * albedo);

	#else
		const uint	LIGHT_SAMPLES = 8;
		for ( uint lightSampleIndex=0; lightSampleIndex < LIGHT_SAMPLES; lightSampleIndex++ ) {
			float	jitter = ReverseBits( lightSampleIndex + uint( _In.__Position.x * _In.__Position.y ) );
			float2	lightUV = float2( float(lightSampleIndex) / LIGHT_SAMPLES, jitter );
	//		float3	wsLightPos = CORNELL_LIGHT_POS + float3( lightUV.x, 0.0, lightUV.y ) * CORNELL_LIGHT_SIZE;
			float3	wsLightPos = CORNELL_LIGHT_POS + float3( lightUV.x, 0.0, lightUV.y ) * float3( _lightSize, 0.0, 1.3/1.05 * _lightSize );
	//float3	wsLightPos = float3( 0, 5, 1 );//float3( 1.0, 5.5 + 2.5 * sin( 4.0 * _time ), 1.0 );
			float	shadow = ShadowTrace( wsPos, wsLightPos, 100 );
			lighting += shadow;
		}
		lighting /= LIGHT_SAMPLES;

//lighting = ReverseBits( uint( _In.__Position.x * _In.__Position.y ) );
	#endif

	return float4( lighting, 1.0 );
//*/
}