#include "Global.hlsl"

cbuffer CB_Object : register(b2) {
	float4x4	_Local2World;
};

Texture2D< float4 >	_TexAreaLight : register(t0);
Texture2D< float4 >	_TexAreaLightSAT : register(t1);

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};


PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.UV = _In.UV;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	float4	StainedGlass = _TexAreaLight.Sample( LinearClamp, _In.UV );
			StainedGlass = 0.01 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
	return float4( StainedGlass.xyz, 1 );
}
