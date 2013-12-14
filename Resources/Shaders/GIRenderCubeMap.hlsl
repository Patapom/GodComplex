//////////////////////////////////////////////////////////////////////////
// This shader renders a cube map at a specified position
// Each face of the cubemap will be composed of 2 render targets:
//	RT0 = Albedo (RGB) + Empty (A)
//	RT1 = Normal (RGB) + Distance (Z)
//
#include "Inc/Global.hlsl"

//[
cbuffer	cbCubeMapCamera	: register( b9 )
{
	float4x4	_CubeMap2World;
	float4x4	_CubeMapWorld2Proj;
};
//]

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};
//]

//[
cbuffer	cbMaterial	: register( b11 )
{
	float3		_DiffuseAlbedo;
	bool		_HasDiffuseTexture;
	float3		_SpecularAlbedo;
	bool		_HasSpecularTexture;
	float		_SpecularExponent;
};
//]

Texture2D<float4>	_TexDiffuseAlbedo : register( t10 );
Texture2D<float4>	_TexSpecularAlbedo : register( t11 );


struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float3	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float3	UV			: TEXCOORD0;
};

struct	PS_OUT
{
	float3	DiffuseAlbedo	: SV_TARGET0;
	float4	NormalDistance	: SV_TARGET1;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _CubeMapWorld2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World );
	Out.Tangent = mul( float4( _In.Tangent, 0.0 ), _Local2World );
	Out.BiTangent = mul( float4( _In.BiTangent, 0.0 ), _Local2World );
	Out.UV = _In.UV;

	return Out;
}

PS_OUT	PS( PS_IN _In )
{
	PS_OUT	Out;
	Out.DiffuseAlbedo = _DiffuseAlbedo;
	if ( _HasDiffuseTexture )
		Out.DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV ).xyz;

	Out.NormalDistance = float4( normalize( _In.Normal ), length( _In.Position - _CubeMap2World[3].xyz ) );	// Store distance
//	Out.NormalDistance = float4( normalize( _In.Normal ), dot( _In.Position - _CubeMap2World[3].xyz, _CubeMap2World[2].xyz ) );	// Store Z
	
	return Out;
}
