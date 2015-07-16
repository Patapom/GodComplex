////////////////////////////////////////////////////////////////////////////////
// Result display
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)

cbuffer	CBDisplay : register( b0 ) {
	uint2	_Size;
	float	_Time;
}

SamplerState LinearClamp	: register( s0 );
SamplerState LinearWrap		: register( s2 );

Texture3D<float>			_TexVisibility : register( t0 );	// This is an interpolable array of 16 principal visiblity directions

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position / _Size;
	return _TexVisibility.SampleLevel( LinearWrap, float3( UV, 0.2 * _Time ), 0.0 );
	return float3( sin( _Time ) * UV, 0 );
	return float3( sin( _Time ), 0, 0 );
}