#include "Inc/Global.fx"

Texture2D	_TexNoise	: register(t0);
Texture2D	_TexHDR		: register(t1);

//[
cbuffer	cbTextureLOD	: register( b1 )
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
//	float2	UV = 2.0 * float2( ASPECT_RATIO * _In.Position.x, _In.Position.y ) * INV_SCREEN_SIZE;
//	return float4( UV, 0, 0 );
//	return Tex2DLOD( _TexNoise, LinearWrap, UV, _LOD );
	return Tex2D( _TexHDR, LinearWrap, _In.Position.xy * INV_SCREEN_SIZE );
}
