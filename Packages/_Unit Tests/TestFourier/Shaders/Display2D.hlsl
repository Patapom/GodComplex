#include "Global.hlsl"

Texture2D<float2>	_texSpectrum : register( t0 );
Texture2D<float2>	_texReconstructedSignal : register( t1 );

Texture2D<float2>	_texSpectrumFFTW : register( t2 );

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = float2(_In.__Position.xy) / _resolution;
			UV.x *= 2.0;
	float	secondPart = step( 1.0, UV.x );
			UV.x = frac( UV.x );

//return float3( UV, 0 );


//return GenerateSignal2D( UV, _time, _signalFlags & 7, (_signalFlags >> 4) & 7 );
//return float3( 40.0 * _texSpectrum.SampleLevel( LinearClamp, UV, 0.0 ), 0 );

//	float	signal = GenerateSignal( U, _signalFlags & 7 );
	float2	spectrum = _texSpectrum[frac( 0.5 + UV ) * _signalSize];
	float2	spectrumFFTW = _texSpectrumFFTW[frac( 0.5 + UV ) * _signalSize];
//	float2	reconstructedSignal = _texReconstructedSignal[uint2( U * _signalSize, 0 )];

spectrum = lerp( spectrum, spectrumFFTW, secondPart );

spectrum *= 40.0;

//if ( _signalFlags & 0x200U )
//	spectrum = abs( spectrum - spectrumFFTW );
//else if ( _signalFlags & 0x100U )
//	spectrum = spectrumFFTW;



//	float2	UV = float2( U, 2.0 * (0.5 - float(_In.__Position.y) / _resolution.y) );
	float3	color = float3( spectrum, 0.0 );
	if ( _signalFlags & 0x100U )
		color = length( spectrum );
	else if ( _signalFlags & 0x200U )
		color = (PI + atan2( spectrum.y, spectrum.x )) / (2.0 * PI);
	
	return color;
}