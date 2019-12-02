//////////////////////////////////////////////////////////////////////////
// This shader displays the scene
//
#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"

cbuffer	cbGeneral	: register( b9 )
{
	bool		_ShowIndirect;
};

// Scene descriptor
cbuffer	cbScene	: register( b10 )
{
	uint		_StaticLightsCount;
	uint		_DynamicLightsCount;
	uint		_ProbesCount;
};

// Object descriptor
cbuffer	cbObject	: register( b11 )
{
	float4x4	_Local2World;
};

// Material descriptor
cbuffer	cbMaterial	: register( b12 )
{
	uint		_MaterialID;
	float3		_DiffuseAlbedo;

	bool		_HasDiffuseTexture;
	float3		_SpecularAlbedo;

	bool		_HasSpecularTexture;
	float3		_EmissiveColor;

	float		_SpecularExponent;
	uint		_FaceOffset;	// The offset to apply to the object's face index
};

// Optional textures associated to the material
Texture2D<float4>	_TexDiffuseAlbedo : register( t10 );
Texture2D<float4>	_TexSpecularAlbedo : register( t11 );


struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float2	UV			: TEXCOORD0;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	Out.Tangent = mul( float4( _In.Tangent, 0.0 ), _Local2World ).xyz;
	Out.BiTangent = mul( float4( _In.BiTangent, 0.0 ), _Local2World ).xyz;
	Out.UV = _In.UV;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//	clip( 0.5 - _HasDiffuseTexture );
	float3	DiffuseAlbedo = _DiffuseAlbedo;
	if ( _HasDiffuseTexture )
		DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV.xy ).xyz;

	DiffuseAlbedo *= INVPI;

	return float4( DiffuseAlbedo, 1 );
//	return float4( normalize( _In.Normal ), 1 );


// 	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 	// Compute direct lighting
// 	float3	View = normalize( _In.Position - _Camera2World[3].xyz );
// 
// 	float3	Normal = normalize( _In.Normal );
// 	float3	Tangent = normalize( _In.Tangent );
// 	float3	BiTangent = normalize( _In.BiTangent );
// 
// 	float3	AccumDiffuse = 0.0;
// 	float3	AccumSpecular = 0.0;
// 
// 	// Process static lights
// 	for ( int LightIndex=0; LightIndex < _StaticLightsCount; LightIndex++ )
// 	{
// 		LightStruct	LightSource = _SBLightsStatic[LightIndex];
// 		AccumDiffuse += AccumulateLight( _In.Position, _In.Normal, Normal, LightSource );
// 	}
// 
// 	// Process dynamic lights
// 	for ( int LightIndex=0; LightIndex < _DynamicLightsCount; LightIndex++ )
// 	{
// 		LightStruct	LightSource = _SBLightsDynamic[LightIndex];
// 		AccumDiffuse += AccumulateLight( _In.Position, _In.Normal, Normal, LightSource );
// 	}
// 
// 	AccumDiffuse *= DiffuseAlbedo;
// 
// 	return float4( Indirect + AccumDiffuse, 1 );
}
