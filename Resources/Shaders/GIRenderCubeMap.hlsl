//////////////////////////////////////////////////////////////////////////
// This shader renders a cube map at a specified position
// Each face of the cubemap will be composed of 2 render targets:
//	RT0 = Albedo (RGB) + Empty (A)
//	RT1 = Normal (RGB) + Distance (A)
//	RT2 = Static Lit Scene (RGB) + Emissive Object ID (A)
//
#include "Inc/Global.hlsl"
#include "Inc/GI.hlsl"

//[
cbuffer	cbCubeMapCamera	: register( b9 )
{
	float4x4	_CubeMap2World;
	float4x4	_CubeMapWorld2Proj;
};
//]

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
	float3	DiffuseAlbedo		: SV_TARGET0;
	float4	NormalDistance		: SV_TARGET1;
	float4	StaticLitEmmissive	: SV_TARGET2;
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

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// First RT stores albedo
	//
	Out.DiffuseAlbedo = _DiffuseAlbedo;
	if ( _HasDiffuseTexture )
		Out.DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV ).xyz;

	Out.DiffuseAlbedo *= INVPI;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Second RT stores geometry with normal and distance
	//
	Out.NormalDistance = float4( normalize( _In.Normal ), length( _In.Position - _CubeMap2World[3].xyz ) );	// Store distance
//	Out.NormalDistance = float4( normalize( _In.Normal ), dot( _In.Position - _CubeMap2World[3].xyz, _CubeMap2World[2].xyz ) );	// Store Z
	

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Third RT stores static direct lighting & any emissive material's ID
	//
	float3	View = normalize( _In.Position - _Camera2World[3].xyz );
	float3	Normal = _In.Normal;

	float3	AccumDiffuse = 0.0;
	for ( int LightIndex=0; LightIndex < _StaticLightsCount; LightIndex++ )
	{
		LightStruct	LightSource = _SBLightsStatic[LightIndex];
		AccumDiffuse += AccumulateLight( _In.Position, _In.Normal, LightSource );
	}
	AccumDiffuse *= Out.DiffuseAlbedo;

//AccumDiffuse = _StaticLightsCount;

	Out.StaticLitEmmissive = float4( AccumDiffuse, asfloat( uint( any( abs( _EmissiveColor ) > 1e-4 ) ? _MaterialID : 0xFFFFFFFFUL ) ) );

	return Out;
}
