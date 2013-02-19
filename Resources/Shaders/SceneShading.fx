//////////////////////////////////////////////////////////////////////////
// This shader performs the actual shading
//
#include "Inc/Global.fx"

cbuffer	cbRender	: register( b10 )
{
	float3		dUV;
};

Texture2D	_TexGBuffer : register( t10 );
Texture2D	_TexDepth : register( t11 );

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = dUV * _In.__Position.xy;
return float4( UV, 0, 0 );
	return 1.0 / _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;
}