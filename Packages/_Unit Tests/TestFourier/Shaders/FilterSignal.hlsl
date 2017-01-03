#include "Global.hlsl"

Texture2D<float2>	_texSpectrum : register( t0 );

float2	PS( VS_IN _In ) : SV_TARGET0 {
	float	U = float(_In.__Position.x) / _signalSize;
	float2	spectrum = _texSpectrum[uint2( _In.__Position.x, 0 )];

//	int		frequency = ((i + halfSize) % size) - halfSize;
	float	frequency = (frac( U + 0.5 ) - 0.5) * _signalSize;

	float	filter = 1.0;
	switch ( (_signalFlags >> 16) & 0xFU ) {
	case 1://FILTER_TYPE.CUT_LARGE:
		filter = abs(frequency) > 256.0 ? 0.0 : 1.0;	// Cut
		break;
	case 2://FILTER_TYPE.CUT_MEDIUM:
		filter = abs(frequency) > 128.0 ? 0.0 : 1.0;	// Cut
		break;
	case 3://FILTER_TYPE.CUT_SHORT:
		filter = abs(frequency) > 64.0 ? 0.0 : 1.0;		// Cut
		break;
	case 4://FILTER_TYPE.EXP:
		filter = exp( -0.01 * abs(frequency) );			// Exponential
		break;
	case 5://FILTER_TYPE.GAUSSIAN:
		filter = exp( -0.005 * frequency*frequency );	// Gaussian
		break;
	case 6://FILTER_TYPE.INVERSE:
		filter = min( 1.0, 4.0 / (1.0 + abs( frequency )) );	// Inverse
		break;
// 	case 7://FILTER_TYPE.SINUS:
// 		filter = sin( -2.0 * PI * frequency / 32 ); };		// Gni ?
// 		break;
	}

	return filter * spectrum;
}