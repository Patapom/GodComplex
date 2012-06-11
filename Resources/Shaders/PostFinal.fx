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
	float4	SourceHDR = TEX2D( _TexHDR, LinearWrap, _In.Position.xy * INV_SCREEN_SIZE );

	float2	UV = 2.0 * float2( ASPECT_RATIO * _In.Position.x, _In.Position.y ) * INV_SCREEN_SIZE;
	float4	Background = TEX2DLOD( _TexNoise, LinearWrap, UV, _LOD );
//return Background;
//return 0.5 * ((Background.y - Background.x) - (Background.w - Background.z));
//return (Background.w - Background.z);
//return 0.5 * Background.x;
//return float4( UV, 0, 0 );

	return lerp( Background, SourceHDR, SourceHDR.w );	// Alpha blend...
}
