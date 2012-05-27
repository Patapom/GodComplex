#include "Inc/Global.fx"

Texture2D	_TexNoise	: register(t0);

//[
cbuffer	cbTextureLOD	: register( b0 )
{
	float	_LOD;
};
//]

struct	VS_IN
{
	float4	Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = 2.0 * _In.Position.xy * INV_SCREEN_SIZE;
	return float4( _TexNoise.SampleLevel( LinearWrap, UV, _LOD ) );
	return float4( UV, 0, 0 );
}
