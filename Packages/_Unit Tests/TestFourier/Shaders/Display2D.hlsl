#include "Global.hlsl"

Texture2D<float2>	_texSpectrum : register( t0 );
Texture2D<float2>	_texReconstructedSignal : register( t1 );
//Texture2D<float2>	_texSpectrumFFTW : register( t2 );

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = float2(_In.__Position.xy) / _resolution;
			UV.x *= 2.0;
	float	secondPart = step( 1.0, UV.x );
			UV.x = frac( UV.x );

	float2	UV_point = (0.5 + floor( (UV - 1.0/_resolution) * _signalSize )) / _signalSize;
	float2	signal = float2( GenerateSignal2D( UV_point, _time, _signalFlags & 7, (_signalFlags >> 4) & 7, _signalFlags & 0x800U ), 0 );
	float2	spectrum = _texSpectrum[frac( 0.5 + UV ) * _signalSize];
//	float2	spectrumFFTW = _texSpectrumFFTW[frac( 0.5 + UV ) * _signalSize];
	float2	reconstructedSignal = _texReconstructedSignal[UV * _signalSize];

//spectrum = 40.0 * lerp( spectrum, spectrumFFTW, secondPart );
	float3	spectrumColor = float3( 40.0 * spectrum, 0.0 );

	float2	finalSignal = signal;
	switch ( (_signalFlags >> 8) & 0x3U ) {
	case 1: finalSignal = reconstructedSignal; break;
	case 2: finalSignal = 10.0 * abs( reconstructedSignal - signal ); break;
	}
	float3	signalColor = float3( finalSignal, 0 );
	if ( signalColor.x < 0.0 )	signalColor.z = -signalColor.x;

//if ( _signalFlags & 0x200U )
//	spectrum = abs( spectrum - spectrumFFTW );
//else if ( _signalFlags & 0x100U )
//	spectrum = spectrumFFTW;

	float3	color = lerp( spectrumColor, signalColor, secondPart );
	if ( _signalFlags & 0x400U )
		color = length( color.xy );
//		color = length( color.xy ) * (PI + atan2( color.y, color.x )) / (2.0 * PI);
	
	return color;
}