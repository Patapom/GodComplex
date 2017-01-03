////////////////////////////////////////////////////////////////////////////////
// Global Defines
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float	INVPI = 0.31830988618379067153776752674503;
static const float	SQRT2 = 1.4142135623730950488016887242097;

cbuffer	CBDisplay : register( b0 ) {
	uint2		_resolution;
	uint		_signalSize;
	uint		_signalFlags;
	float		_time;
	float2		_signalScaleUV;
}

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border

Texture2D<float4>	_TexHDR : register( t0 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

uint	wang_hash( uint seed ) {
    seed = (seed ^ 61) ^ (seed >> 16);
    seed *= 9;
    seed = seed ^ (seed >> 4);
    seed *= 0x27d4eb2d;
    seed = seed ^ (seed >> 15);
    return seed;
}

float	GenerateSignal( float t, float _time, uint _signalFlags ) {
	float	r = 1.0;	// CONSTANT
	switch ( _signalFlags ) {
		case 1:	// SQUARE,
			r = 0.5 * sin( _time ) + (fmod( t + 0.2 * _time, 0.5 ) < 0.25 ? 0.5 : -0.5);
			break;
		case 2:	// SINE,
			r = cos( 4.0 * (1.001 + sin( _time )) * 2.0 * PI * t );
			break;
		case 3:	// SAW,
			r = 0.5 * sin( _time ) + (frac( 4.0 * t + 0.2 * _time ) - 0.5);
			break;
		case 4:	{ // SINC,
			float	a = 4.0 * (1.001 + sin( _time )) * 4.0 * PI * (t - 0.5);
			r = abs(a) > 0.0 ? sin( a ) / a : 1.0;
			break;
		}
		case 5:	// RANDOM,
			r = wang_hash( 491537.0 * (t + 0.2 * _time) ) / 4294967295.0 - 0.5;
			break;
	}

	return r;
}

float	GenerateSignal2D( float2 _UV, float2 _time, uint _signalFlagsX, uint _signalFlagsY ) {
//	return cos( _signalScaleUV.x * 2.0 * PI * _UV.x + _time.x )
//		 * cos( _signalScaleUV.y * 2.0 * PI * _UV.y + _time.y );

	float	rx = GenerateSignal( _UV.x, _time.x, _signalFlagsX );
	float	ry = GenerateSignal( _UV.y, _time.y, _signalFlagsY );
	return rx * ry;
}