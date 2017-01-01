#include "Global.hlsl"

Texture2D<float2>	_texSpectrum : register( t0 );
Texture2D<float2>	_texReconstructedSignal : register( t1 );

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float	UV = float(_In.__Position.xy) / _resolution;
return float3( UV, 0 );

//	float	signal = GenerateSignal( U, _signalFlags & 7 );
//	float2	spectrum = _texSpectrum[uint2( frac( 0.5 + U ) * _signalSize, 0 )];
//	float2	reconstructedSignal = _texReconstructedSignal[uint2( U * _signalSize, 0 )];
//
//spectrum *= 40.0;
//
//	float2	UV = float2( U, 2.0 * (0.5 - float(_In.__Position.y) / _resolution.y) );
//	float3	color = 0.0;
//	if ( (_signalFlags & 0x100U) != 0 && abs( signal - UV.y ) < 1.0/_resolution.y )
//		color = 1.0;
//	if ( (_signalFlags & 0x200U) != 0 ) {
//		if ( abs( reconstructedSignal.x - UV.y ) < 2.0/_resolution.y )
//			color = float3( 1, 0.5, 0 );
//		if ( abs( reconstructedSignal.y - UV.y ) < 2.0/_resolution.y )
//			color = float3( 0, 0.5, 1 );
//	}
//
//	if ( (uint(_In.__Position.x) & 1) == 0 ) {
//		// Even columns: draw real values
//		if (   (spectrum.x > 0.0 && UV.y > 0.0 && UV.y < spectrum.x)
//			|| (spectrum.x < 0.0 && UV.y < 0.0 && UV.y > spectrum.x) )
//			color = float3( 1, 0, 0 );
//	} else {
//		// Odd columns: draw imaginary values
//		if (   (spectrum.y > 0.0 && UV.y > 0.0 && UV.y < spectrum.y)
//			|| (spectrum.y < 0.0 && UV.y < 0.0 && UV.y > spectrum.y) )
//			color = float3( 0, 0, 1 );
//	}
//
//	return color;
}