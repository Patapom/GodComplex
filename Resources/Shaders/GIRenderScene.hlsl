//////////////////////////////////////////////////////////////////////////
// This shader displays the Global Illumination test room
//
#include "Inc/Global.hlsl"

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
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float3	UV			: TEXCOORD0;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World );
	Out.Tangent = mul( float4( _In.Tangent, 0.0 ), _Local2World );
	Out.BiTangent = mul( float4( _In.BiTangent, 0.0 ), _Local2World );
	Out.UV = _In.UV;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//	clip( 0.5 - _HasDiffuseTexture );
	if ( _HasDiffuseTexture )
//		return float4( 1, 0, 0, 1 );
		return float4( _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV ).xyz, 1.0 );

	return float4( normalize( _In.Normal ), 1 );
}
