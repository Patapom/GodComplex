//////////////////////////////////////////////////////////////////////////
// This shader is split in two:
//	1) A compute shader that iterates on all the scene vertices to find the bounding rectangle of the scene from the light's POV
//	2) The classical shadow map renderer
//
#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"

cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};

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
};


///////////////////////////////////////////////////////////
// Shadow Map bounds computation
//StructuredBuffer<VS>	_VBObject : register( t10 );

// TODO??


///////////////////////////////////////////////////////////
// Shadow Map rendering
PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = World2ShadowMapProj( WorldPosition );

	return Out;
}

// Won't be used anyway!
float	PS( PS_IN _In ) : SV_TARGET0
{
	return 0.0;
}
