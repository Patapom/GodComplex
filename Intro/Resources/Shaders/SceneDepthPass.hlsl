//////////////////////////////////////////////////////////////////////////
// This shader performs the Z pre-pass
//
#include "Inc/Global.hlsl"

cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};

struct	VS_IN
{
	float3	Position	: POSITION;
// 	float3	Normal		: NORMAL;
// 	float3	Tangent		: TANGENT;
// 	float2	UV			: TEXCOORD0;
};

float4	VS( VS_IN _In ) : SV_POSITION
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	return mul( WorldPosition, _World2Proj );
}
