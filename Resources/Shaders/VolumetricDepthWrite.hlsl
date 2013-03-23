//////////////////////////////////////////////////////////////////////////
// This shader renders the volume box's front & back Z in a RG16F target
//
#include "Inc/Global.hlsl"

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2View;
	float4x4	_View2Proj;
	float2		_dUV;
};

cbuffer	cbShadow	: register( b11 )
{
	float4x4	_World2Shadow;
	float4x4	_Shadow2World;
	float		_ShadowZMax;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float	Z			: DEPTH;
};

PS_IN VS ( VS_IN _In ) 
{ 
	float4 ViewPosition = mul( float4( _In.Position, 1.0 ), _Local2View );
	float Z = ViewPosition.z;
	float4 ProjPosition = mul( ViewPosition, _View2Proj );

	PS_IN	Out;
	Out.__Position = ProjPosition;
	Out.Z = Z;

	return Out;
} 

float4	PS( PS_IN _In ) : SV_TARGET0
{
//return 1;
	return _In.Z;
}
