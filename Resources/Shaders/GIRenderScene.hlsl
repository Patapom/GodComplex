//////////////////////////////////////////////////////////////////////////
// This shader displays the Global Illumination test room
//
#include "Inc/Global.hlsl"

// Texture2DArray	_TexLightMaps	: register(t10);
// Texture2DArray	_TexWalls		: register(t11);

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
	float3	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Normal		: NORMAL;
	float3	UV			: TEXCOORD0;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Normal = _In.Normal;
	Out.UV = _In.UV;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return 0.5;
}
