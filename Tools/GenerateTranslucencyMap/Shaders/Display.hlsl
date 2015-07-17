////////////////////////////////////////////////////////////////////////////////
// Result display
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2 degree observer (sRGB white point) (cf. http://wiki.patapom.com/index.php/Colorimetry)

cbuffer	CBDisplay : register( b0 ) {
	uint2	_Size;
	float	_Time;
}

SamplerState LinearClamp	: register( s0 );
SamplerState LinearWrap		: register( s2 );

Texture2D<float>			_TexThickness : register( t0 );
Texture2D<float3>			_TexNormal : register( t1 );
Texture2D<float3>			_TexTransmittance : register( t2 );
Texture2D<float3>			_TexAlbedo : register( t3 );
Texture3D<float>			_TexVisibility : register( t4 );	// This is an interpolable array of 16 principal visiblity directions

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _Size;

	float	phi = fmod( _Time + PI, 2.0 * PI ) - PI;

//phi = 0*0.5 * PI;

	if ( all(UV < 0.1) ) {
		UV /= 0.1;
		float2	xy = 2.0 * UV - 1.0;
				xy.y *= -1.0;
		if ( length( xy ) < 1.0 ) {
			float	pixelPhi = atan2( xy.y, xy.x );
			return	abs(pixelPhi-phi) < 0.05 ? 1.0 : 0.0;
		}
	}

//	return _TexThickness.SampleLevel( LinearWrap, UV, 0.0 );
//	return _TexAlbedo.SampleLevel( LinearWrap, UV, 0.0 );
// 	return _TexTransmittance.SampleLevel( LinearWrap, UV, 0.0 );
//	return _TexNormal.SampleLevel( LinearWrap, UV, 0.0 );
	return _TexVisibility.SampleLevel( LinearWrap, float3( UV, phi / (2.0 * PI) ), 0.0 );
	return float3( sin( _Time ) * UV, 0 );
	return float3( sin( _Time ), 0, 0 );
}