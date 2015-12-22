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
	float	_A;		// Shoulder Strength.
	float	_B;		// Linear Strength.
	float	_C;		// Linear Angle.
	float	_D;		// Toe Strength.
	float	_E;		// Toe Numerator.
	float	_F;		// Toe Denominator.
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

float3	ToneMappingFilmic_Hable( float3 x ) {
	return ((x * (_A*x + _C*_B) + _D*_E) / (x * (_A*x + _B) + _D*_F)) - _E / _F;
}

// Filmic operator from Mike Day (Insomniac Games) (https://d3cw3dd2w32x2b.cloudfront.net/wp-content/uploads/2012/09/an-efficient-and-user-friendly-tone-mapping-operator.pdf)
// The filmic curve is separated into 2 joining curves T(x) for toe and S(x) for shoulder joining in c, the junction point
//	T(x) = k * (1-t)*(x-b) / ((c-b) - t*(x-b))
//	S(x) = k + (1-k)*(x-c) / ((1-s)*(w-c) + s*(x-c))
//
// k is computed so that T(c)=S(c) and their tangent are equal T'(c)=S'(c) which gives:
//	k = (1-t)(c-b) / ((1-s)(w-c) + (1-t)(c-b))
//
float	ToneMappingFilmic_Insomniac( float x ) {
	float	w = _WhitePoint;	// White point
	float	b = _A;				// Black point
	float	c = _B;				// Junction point
	float	t = _C;				// Toe strength
	float	s = _D;				// Shoulder strength
	float	k = _E;				// Junction factor

	#if 0
		// Simple version
		return	x < c
			? k*(1-t)*(x-b) / ((c-b) - t*(x-b))
			: k+(1-k)*(x-c) / ((1-s)*(w-c) + s*(x-c));
	#else
		// "Optimized" version where coeffs for Toe and Shoulder could be passed as float4 to the CB
		float4	Coeffs_Toe = float4( k-k*t, -t, -k*b + k*b*t, c-b+t*b );
		float4	Coeffs_Shoulder = float4( k*(s-1)+1, s, k*(1-s)*(w-c) - k*s*c - c*(1-k), (1-s)*(w-c) - s*c );

		float4	Coeffs = x < c ? Coeffs_Toe : Coeffs_Shoulder;
		float2	Fraction = Coeffs.xy * x + Coeffs.zw;
		return Fraction.x / Fraction.y;
	#endif
}
float3	ToneMappingFilmic_Insomniac( float3 x ) {
	return float3( ToneMappingFilmic_Insomniac( x.x ), ToneMappingFilmic_Insomniac( x.y ), ToneMappingFilmic_Insomniac( x.z ) );
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

		if ( _Flags & 8 )
			Color = max( 0.0, ToneMappingFilmic_Hable( Color ) / max( 1e-3, ToneMappingFilmic_Hable( _WhitePoint ) ) );
		else
			Color = saturate( ToneMappingFilmic_Insomniac( Color ) );
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
		DEBUG_DisplayLuminanceHistogram( _WhitePoint, UV, float2( _MouseU, _MouseV ), (_Flags & 2) ? _DebugLuminanceLevel : 0.0001, _Resolution.xy, _GlobalTime, Color, OriginalColor );
	}

	return Color;
}