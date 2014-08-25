////////////////////////////////////////////////////////////////////////////////
// Displays the world cube
////////////////////////////////////////////////////////////////////////////////
#include "Global.hlsl"

cbuffer	cbRender : register(b8)
{
	float4		_Dimensions;		// XY=Dimensions of the render target, ZW=1/XY
	float4		_DEBUG;
}

Texture2DArray<float4>	_TexPhotons : register(t0);

struct VS_IN
{
	float3	Position : POSITION;
//	float3	UVW : NORMAL;
};

struct PS_IN
{
	float4	__Position : SV_POSITION;
	float3	Position : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In )
{
	float4x4	World2Camera = _World2Camera;
	World2Camera[3] = float4( 0, 0, 4, 1 );
	float4	CameraPosition = mul( float4( _In.Position, 1.0 ), World2Camera );

	PS_IN	Out;
	Out.__Position = mul( CameraPosition, _Camera2Proj );
	Out.__Position /= Out.__Position.w;
	Out.__Position = float4( -1.0 + 0.1, -1.0 + 0.1, 0, 0 ) + float4( 0.1 * Out.__Position.xyz, Out.__Position.w );
	Out.Position = _In.Position;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return float4( _In.Position, 0 );
}
