//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};

Texture2DArray< float4 >	_Tex_GBuffer : register(t0);
Texture2D< float4 >			_Tex_Wall : register(t1);
Texture2D< float4 >			_Tex_BlueNoise : register(t2);

float3	MapColor( float3 _wsPosition, float3 _wsNormal, float _materialID ) {
	float3	color;
	if ( _materialID < 0.5 ) {
		// Map wall color depending on normal
		float2	UV = 0.5 * (1.0 + _wsPosition.xy);
//		color = float3( UV, 0 );
		color = 1.0 * _Tex_Wall[uint2( 64.0 * UV )].xyz;

// TODO: Tweak color depending on normal (invert blue and green apparently)

	} else {
		// Map sphere color depending on height
		float	V = 0.5 * (1.0 + (_wsPosition.y - SPHERE_CENTER.y) * (1.0 / SPHERE_RADIUS));
//		color = V;
		const float	BANDS_COUNT = 20;
		V *= BANDS_COUNT;
		float	bandIndex = floor( V );
		float	Vband = 2.0 * frac( V ) - 1.0;
				Vband = sqrt( 1.0 - Vband*Vband );
		float	intensity = abs( sin( 4.0 * _Time + bandIndex * sin( _Time ) ) ) * Vband;
		float3	bandColor = frac( float3( 13.289 * bandIndex, 0.9 - 3.18949 * bandIndex, 17.0 * bandIndex ) );
				bandColor = 0.25 + 0.75 * bandColor;
		color = intensity * bandColor;
	}
	return 2.0 * color;
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _Resolution;
	float	aspectRatio = float(_Resolution.x) / _Resolution.y;

	float	noise = _NoiseInfluence * _Tex_BlueNoise[uint2( _In.__Position.xy ) & 0x3F].x;
//return _Tex_BlueNoise.SampleLevel( LinearWrap, 10.0 * float2( aspectRatio * UV.x, UV.y ), 0.0 ).x;

	float3	csView = float3( aspectRatio * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 );
	float	viewLength = length( csView );
			csView /= viewLength;

	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

	float4	wsNormal_Distance = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 ); 
	float4	wsMaterialID_Roughness = _Tex_GBuffer.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 ); 

	// Compute hit position
	float3	wsPos = _Camera2World[3].xyz + wsNormal_Distance.w * wsView; 

	// Build tangent space
	float3	wsNormal = wsNormal_Distance.xyz;
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );
//return cross( wsTangent, wsBiTangent );
//return wsPos;

	///////////////////////////////////////////////////////////////////
	// Compute emissive color
	float3	emissive = MapColor( wsPos, wsNormal, wsMaterialID_Roughness.x );

	///////////////////////////////////////////////////////////////////
	// Importance sample specular distribution
	const uint	SAMPLES_COUNT = 256;

	float	alpha = wsMaterialID_Roughness.y;
	float	sqrAlpha = alpha * alpha;

	float	Gv = GSmith( wsNormal, wsView, sqrAlpha );

	float3	specular = 0.0;

	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		// Generate random half vector
		float	X0 = float(i) / SAMPLES_COUNT;
		float	X1 = ReverseBits( i );
		float	phi = 2.0 * PI * (X0 + noise);
		float2	sinCosPhi;
		sincos( phi, sinCosPhi.x, sinCosPhi.y );

		float	sqrCosTheta = (1.0 - X1) / ((alpha*alpha - 1.0) * X1 + 1.0);
		float	cosTheta = sqrt( sqrCosTheta );
		float	sinTheta = sqrt( 1.0 - sqrCosTheta );

		float3	lsHalf = float3( sinTheta * sinCosPhi.y, sinTheta * sinCosPhi.x, cosTheta );

		// Generate world-space light ray
		float3	wsHalf = lsHalf.x * wsTangent + lsHalf.y * wsBiTangent + lsHalf.z * wsNormal;
		float3	wsLight = wsView - 2.0 * dot( wsHalf, wsView ) * wsHalf;

		// Intersect scene in light direction
		float2	d = Map( wsPos, wsLight );
		float3	wsSceneHitPos = wsPos + d.x * wsLight;
		float3	wsSceneNormal = 0.0;	// !!!!!TODO!!!!!
		float3	sceneColor = 10.0 * MapColor( wsSceneHitPos, wsSceneNormal, d.y );

		// Compute Fresnel
		const float	F0 = 0.04;	// Dielectric
		float	F = FresnelSchlick( F0, cosTheta );

		// Compute masking
//		float	Gl = GSmith( wsNormal, wsLight, sqrAlpha );
//		float	Gl = GSmith( wsHalf, wsLight, sqrAlpha );
		float	Gl = 1;

//		float	NdotV = dot( 
//		float	G_V = NoV + sqrt( (NoV - NoV * a2) * NoV + a2 );
//		float G_L = NoL + sqrt( (NoL - NoL * a2) * NoL + a2 );
//		return rcp( G_V * G_L );

		specular += sceneColor * F * Gl;
	}
//	specular *= Gv / SAMPLES_COUNT;
	specular *= 1.0 / SAMPLES_COUNT;

	return emissive + specular;
}
