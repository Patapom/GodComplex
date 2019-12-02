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

	WorldPosition.x += 0.2 * FastNoise( 2.0 * WorldPosition );
	WorldPosition.y += 0.2 * FastNoise( 3.0 * WorldPosition );
	WorldPosition.z += 0.2 * FastNoise( 0.5 * WorldPosition );

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
	return float4( _In.UV, 0, 1 );
}
