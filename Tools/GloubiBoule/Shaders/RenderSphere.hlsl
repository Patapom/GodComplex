#include "Includes/global.hlsl"
#include "Includes/Photons.hlsl"
#include "Includes/Noise.hlsl"
#include "Includes/Room.hlsl"
#include "Includes/HeightMap.hlsl"


cbuffer CB_RenderSphere : register(b2) {
};

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	wsPosition : POSITION;
	float3	wsNormal : NORMAL;
	float3	wsTangent : TANGENT;
	float2	UV : TEXCOORD0;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

//	float2	HeightMapUV = Direction2HeightMapUV( _In.Normal );
//	float	Height = _TexHeight.SampleLevel( LinearWrap, HeightMapUV, 0.0 ).x;
//	float	SphereRadius = SPHERE_RADIUS + Height;
//	Out.wsPosition = _In.Position * SphereRadius;

	float3	UVW = 0.25 * _In.Position + 1.0 * _Time * float3( 0.12, -0.1, 0.19 );
	Out.wsPosition = SPHERE_RADIUS * _In.Position + 0.1 * NoiseVector( UVW );

	Out.__Position = mul( float4( Out.wsPosition, 1.0 ), _World2Proj );
	Out.wsNormal = _In.Normal;
	Out.wsTangent = _In.Tangent;
	Out.UV = _In.UV;
	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.UV;
//	return float3( UV, 0 );
	return _TexHeight.Sample( LinearWrap, UV ).x;
}
