//////////////////////////////////////////////////////////////////////////
// This shader fills up the G-Buffer
//
#include "Inc/Global.hlsl"

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};
//]

Texture2D	_TexDiffuseSpec			: register(t10);
Texture2D	_TexNormalRoughnessAO	: register(t11);

struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_OUT
{
	float4	Diffuse_Spec		: SV_TARGET0;
	float4	Normal_Roughness_AO	: SV_TARGET1;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	float3	WorldNormal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	float3	WorldTangent = mul( float4( _In.Tangent, 0.0 ), _Local2World ).xyz;
	float3	WorldBiTangent = cross( WorldNormal, WorldTangent );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Normal = WorldNormal;
	Out.Tangent = WorldTangent;
	Out.BiTangent = WorldBiTangent;
	Out.UV = _In.UV;

	return Out;
}

PS_OUT	PS( PS_IN _In )
{
	float4	Tex0 = _TexDiffuseSpec.Sample( LinearWrap, _In.UV, 0 );			// RGB=Albedo A=Specular
	float4	Tex1 = _TexNormalRoughnessAO.Sample( LinearWrap, _In.UV, 1 );	// RG=NormalXY B=Roughness A=Ambient Occlusion

	float3	WorldNormal = normalize( _In.Normal );
	float3	WorldTangent = normalize( _In.Tangent );
	float3	WorldBiTangent = normalize( _In.BiTangent );

	float3	NormalMap = float3( 2.0 * Tex1.xy - 1.0, 0.0 );
			NormalMap.z = sqrt( 1.0 - dot(NormalMap.xy,NormalMap.xy) );

	WorldNormal = NormalMap.x * WorldTangent + NormalMap.y * WorldBiTangent + NormalMap.z * WorldNormal;

	float3	CameraNormal = mul( float4( WorldNormal, 0.0 ), _World2Camera ).xyz;

	// Stereographic projection (from http://en.wikipedia.org/wiki/Stereographic_projection)
	float2	StereoNormal = CameraNormal.xy / (1.57 * (1 + CameraNormal.z));

	PS_OUT	Out;
	Out.Diffuse_Spec = Tex0;
	Out.Normal_Roughness_AO = float4( StereoNormal, Tex1.zw );

	return Out;
}
