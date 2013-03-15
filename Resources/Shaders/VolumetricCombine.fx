//////////////////////////////////////////////////////////////////////////
// This shader finally combines the volumetric rendering with the actual screen
//
#include "Inc/Global.fx"

Texture2D	_TexDebug	: register(t10);

//[
cbuffer	cbObject	: register( b10 )
{
	float3		_dUV;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;
//return float4( UV, 0, 1 );

	float2	Depth = _TexDebug.SampleLevel( LinearClamp, UV, 0.0 ).xy;
//return 0.1 * Depth.x;
	return 0.3 * (Depth.y - Depth.x);
}
