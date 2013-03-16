//////////////////////////////////////////////////////////////////////////
// This shader concatenates the front & back Z Buffers into a single RG32F linear ZBuffer
//
#include "Inc/Global.hlsl"

cbuffer	cbRender	: register( b10 )
{
	float3		dUV;
};

Texture2D	_TexDepthFront : register( t10 );
Texture2D	_TexDepthBack : register( t11 );

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

float2	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = dUV * _In.__Position.xy;

	float	Q = _CameraData.w / (_CameraData.w - _CameraData.z);	// Zf / (Zf-Zn)

	float	Zproj = _TexDepthFront.SampleLevel( LinearClamp, UV, 0.0 ).x;
	float	Zfront = (Q * _CameraData.z) / (Q - Zproj);

			Zproj = _TexDepthBack.SampleLevel( LinearClamp, UV, 0.0 ).x;
	float	Zback = (Q * _CameraData.z) / (Q - Zproj);

	return float2( Zfront, Zback );
}