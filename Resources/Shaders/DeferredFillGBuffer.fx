//////////////////////////////////////////////////////////////////////////
// This shader displays the objects
//
#include "Inc/Global.fx"

Texture2DArray	_TexObject	: register(t10);

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};
//]

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
	float4	Albedo_MatID		: SV_TARGET0;
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
	float4	Tex0 = _TexObject.Sample( LinearWrap, _In.UV, 0 );	// RGB=Albedo A=AO
	float4	Tex1 = _TexObject.Sample( LinearWrap, _In.UV, 1 );	// RGB=Normal A=Roughness

	float3	WorldNormal = normalize( _In.Normal );
	float3	WorldTangent = normalize( _In.Tangent );
	float3	WorldBiTangent = normalize( _In.BiTangent );

	float3	NormalMap = 2.0 * Tex1.xyz - 1.0;

	float3	NewNormal = NormalMap.x * WorldTangent + NormalMap.y * WorldBiTangent + NormalMap.z * WorldNormal;

	float3	CameraNormal = mul( float4( NewNormal, 0.0 ), _World2Camera ).xyz;

	PS_OUT	Out;
	Out.Albedo_MatID = float4( Tex0.xyz, 0 );
	Out.Normal_Roughness_AO = float4( CameraNormal.xy, Tex1.w, Tex0.w );

	return Out;
}
