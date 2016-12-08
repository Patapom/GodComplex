////////////////////////////////////////////////////////////////////////////////
// Result display
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2 degree observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)
static const float	LIGHT_INTENSITY = 4.0;
static const float	AIRLIGHT_BETA = 0.01;
static const float	CAMERA_FOV = 60.0 * PI / 180.0;

cbuffer	CBDisplay : register( b0 ) {
	uint2		_Size;
	float		_Time;
	float		_cosAO;
	float4x4	_world2Proj;
}

SamplerState LinearClamp	: register( s0 );
SamplerState LinearWrap		: register( s2 );

Texture2D<float4>	_TexHDR : register( t0 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }
