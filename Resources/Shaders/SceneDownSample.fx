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

#if 0
	UV -= 0.25 * _dUV.xy;			// But we need to stand at the center of the upper-left source pixel!
	float3	dUV = 0.5 * _dUV;

	float	Z00 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV += dUV.xz;
	float	Z01 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV += dUV.zy;
	float	Z11 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV -= dUV.xz;
	float	Z10 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV -= dUV.zy;

//	return exp2( 0.25 * (log2( Z00 ) + log2( Z01 ) + log2( Z10 ) + log2( Z11 )) );
//	return exp( 0.25 * (log( Z00 ) + log( Z01 ) + log( Z10 ) + log( Z11 )) );
	return pow( 1000.0, 0.14476482730108394255037630630554 * 0.25 * (log( Z00 ) + log( Z01 ) + log( Z10 ) + log( Z11 )) );

#elif 0
	UV -= 0.25 * _dUV.xy;			// But we need to stand at the center of the upper-left source pixel!
	float3	dUV = 0.5 * _dUV;

	float	Z00 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV += dUV.xz;
	float	Z01 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV += dUV.zy;
	float	Z11 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV -= dUV.xz;
	float	Z10 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV -= dUV.zy;

	const float	Z_INFINITY = 800.0;
	float2	Z = float2( 1e-3, 1e-6 );
	Z += lerp( float2( Z00, 1.0 ), 0.0, saturate(100*(Z00-Z_INFINITY)) );
	Z += lerp( float2( Z01, 1.0 ), 0.0, saturate(100*(Z01-Z_INFINITY)) );
	Z += lerp( float2( Z10, 1.0 ), 0.0, saturate(100*(Z10-Z_INFINITY)) );
	Z += lerp( float2( Z11, 1.0 ), 0.0, saturate(100*(Z11-Z_INFINITY)) );
	return Z.x / Z.y;

#elif 1
	UV -= 0.25 * _dUV.xy;			// But we need to stand at the center of the upper-left source pixel!
	float3	dUV = 0.5 * _dUV;

	float	Z00 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV += dUV.xz;
	float	Z01 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV += dUV.zy;
	float	Z11 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV -= dUV.xz;
	float	Z10 = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	UV -= dUV.zy;
	float	Z = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;

	return min( min( min( Z00, Z01 ), Z10 ), Z11 );
//	return 0.5 * (Z + min( min( min( Z00, Z01 ), Z10 ), Z11 ) );
#else
	return _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;	// Simply let the bilinear interpolation do the average!
#endif
}