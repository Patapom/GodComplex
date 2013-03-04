//////////////////////////////////////////////////////////////////////////
// This shader performs a downsampling of the front Z Buffer
//
#include "Inc/Global.fx"

cbuffer	cbObject	: register( b10 )
{
	float3		_dUV;
};

Texture2D			_TexDepth : register( t10 );

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position * _dUV.xy;	// This is the coordinate of the center of the target pixel
//			UV -= 0.25 * _dUV.xy;			// But we need to stand at the center of the upper-left source pixel!

	return _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	// Simply let the bilinear interpolation do the average!
}