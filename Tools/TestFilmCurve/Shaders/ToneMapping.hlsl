#include "Global.hlsl"

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
};

struct VS_IN {
	float4	__Position : SV_POSITION;
};

TextureCube<float4>	_texCubeMap : register(t0);

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

	float4	wsView4 = mul( float4( 2.0 * UV.x - 1.0, 1.0 - 2.0 * UV.y, 0, 1 ), _Proj2World );
	float3	wsView = normalize( wsView4.xyz / wsView4.w - _Camera2World[3].xyz );
//	return wsView;

	float3	Color = _texCubeMap.Sample( LinearWrap, wsView ).xyz;

	Color *= _Exposure;

	if ( _Flags & 1 ) {
//		Color = max( 0.0, ToneMappingFilmic( 3.0 * Color ) / max( 1e-3, ToneMappingFilmic( _WhitePoint ) ) );

//		Color = Sigmoid( 1.0 * Color );

		// Try darkening saturated colors
//		Color = saturate( Color );
		float	MinRGB = min( min( Color.x, Color.y ), Color.z );
		float	MaxRGB = max( max( Color.x, Color.y ), Color.z );
		float	L = 0.5 * (MinRGB + MaxRGB);
//		float	S = (MaxRGB - MinRGB) / (1.00001 - abs(2*L-1));
		float	S = (MaxRGB - MinRGB) / MaxRGB;

		Color *= 1.0 - _A * pow( S, _B );
//		Color = S;
	}

	return Color;
}