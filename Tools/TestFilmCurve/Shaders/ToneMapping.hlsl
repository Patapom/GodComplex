#include "Global.hlsl"
#include "AutoExposure/Common.hlsl"
#include "AutoExposure/DebugDrawDigits.hlsl"
#include "AutoExposure/DebugHistogram.hlsl"


Texture2D<uint>		_texTallHistogram : register(t3);	// The histogram from last pass used as a RO texture!


// Pines slides =>
// 	• increase contrast in log space around middle gray ( log(0.18) )
// 	• add toe and shoulder to taste (requires knowledge of display)
// 	• darken saturated colors to taste (i.e. to emulate subtractive-color reproduction)
// 

cbuffer CB_ToneMapping : register( b10 ) {
	float	_Exposure;
	uint	_Flags;
	float	_A;
	float	_B;
	float	_C;
	float	_D;
	float	_E;
	float	_F;
	float	_WhitePoint;

	float	_SaturationFactor;
	float	_DarkenFactor;
	float	_DebugLuminanceLevel;
	float	_MouseU;
	float	_MouseV;
};

struct VS_IN {
	float4	__Position : SV_POSITION;
};

Texture2D<float4>	_texHDR : register(t2);

VS_IN	VS( VS_IN _In ) { return _In; }

float3	ToneMappingFilmic( float3 x ) {
	return ((x * (_A*x + _C*_B) + _D*_E) / (x * (_A*x + _B) + _D*_F)) - _E / _F;
}

float3	Sigmoid( float3 x ) {

	const float	_A = 15.0;
	const float	_B = 0.3;

	return 1.0 / (1.0 + exp( -_A * (x - _B) ) );
//	return 1.0 / (1.0 + x);
}

float3	PS( VS_IN _In ) : SV_TARGET0 {

	float2	UV = _In.__Position.xy / _Resolution.xy;
	float3	OriginalColor = _texHDR.SampleLevel( LinearClamp, UV, 0.0 ).xyz;


// Debug tall histogram
// 	uint2	PixPos = uint2( 0.4 * _In.__Position.xy );
// 	if ( PixPos.x < 128 && PixPos.y < 202 )
// 		return 0.01 * _texTallHistogram[PixPos];


	// Apply auto-exposure
	autoExposure_t	currentExposure = ReadAutoExposureParameters();
	float3	Color = OriginalColor * currentExposure.EngineLuminanceFactor;

	// Apply tone mapping
	if ( _Flags & 1 ) {
		Color = max( 0.0, ToneMappingFilmic( 3.0 * Color ) / max( 1e-3, ToneMappingFilmic( _WhitePoint ) ) );

//		Color = Sigmoid( 1.0 * Color );
		
// 		// Try darkening saturated colors
// //		Color = saturate( Color );
// 		float	MinRGB = min( min( Color.x, Color.y ), Color.z );
// 		float	MaxRGB = max( max( Color.x, Color.y ), Color.z );
// 		float	L = 0.5 * (MinRGB + MaxRGB);
// //		float	S = (MaxRGB - MinRGB) / (1.00001 - abs(2*L-1));
// 		float	S = (MaxRGB - MinRGB) / MaxRGB;
// 
// 		Color *= 1.0 - _A * pow( abs( S ), _B );
// //		Color = S;
 	}
	
	if ( _Flags & 2 ) {
		// Show debug luminance value
		float	Color_WorldLuma = BISOU_TO_WORLD_LUMINANCE * dot( OriginalColor, LUMINANCE );
		float	Color_dB = Luminance2dB( Color_WorldLuma );
		float	Debug_dB = Luminance2dB( _DebugLuminanceLevel );
		if ( abs( Color_dB - Debug_dB ) < 0.4 ) {
			uint2	pixelIndex = uint2( floor( 0.25 * _In.__Position.xy + 4.0 * _GlobalTime ) );
			bool	checker = (pixelIndex.x & 1) ^ (pixelIndex.y & 1);
			Color = checker ? float3( 1, 0, 0 ) : float3( 0, 0, 1 );
		}
	}
	
	// Show debug histogram
	if ( _Flags & 4 ) {
		DEBUG_DisplayLuminanceHistogram( UV, float2( _MouseU, _MouseV ), (_Flags & 2) ? _DebugLuminanceLevel : 0.0001, _Resolution.xy, _GlobalTime, Color, OriginalColor );
	}

	return Color;
}