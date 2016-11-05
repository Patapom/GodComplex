#include "stdafx.h"
#include "ColorProfile.h"

using namespace ImageUtilityLib;

const bfloat2	ColorProfile::ILLUMINANT_A( 0.44757f, 0.40745f );	// Incandescent, tungsten
const bfloat2	ColorProfile::ILLUMINANT_D50( 0.34567f, 0.35850f );	// Daylight, Horizon
const bfloat2	ColorProfile::ILLUMINANT_D55( 0.33242f, 0.34743f );	// Mid-Morning, Mid-Afternoon
const bfloat2	ColorProfile::ILLUMINANT_D65( 0.31271f, 0.32902f );	// Daylight, Noon, Overcast (sRGB reference illuminant)
const bfloat2	ColorProfile::ILLUMINANT_E( 1/3.0f, 1/3.0f );		// Reference

const float		ColorProfile::GAMMA_EXPONENT_sRGB = 2.4f;
const float		ColorProfile::GAMMA_EXPONENT_ADOBE = 2.19921875f;
const float		ColorProfile::GAMMA_EXPONENT_PRO_PHOTO = 1.8f;

// Standard chromaticities
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::Empty;
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::sRGB			( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.3000f, 0.6000f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_D65 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::AdobeRGB_D50	( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.2100f, 0.7100f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_D50 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::AdobeRGB_D65	( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.2100f, 0.7100f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_D65 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::ProPhoto		( bfloat2( 0.7347f, 0.2653f ), bfloat2( 0.1596f, 0.8404f ), bfloat2( 0.0366f, 0.0001f ), ILLUMINANT_D50 );
const ColorProfile::Chromaticities	ColorProfile::Chromaticities::Radiance		( bfloat2( 0.6400f, 0.3300f ), bfloat2( 0.2900f, 0.6000f ), bfloat2( 0.1500f, 0.0600f ), ILLUMINANT_E );

//////////////////////////////////////////////////////////////////////////
// Copy constructor
ColorProfile::ColorProfile( const ColorProfile& _other ) : m_internalConverter( nullptr ) {
	m_profileFoundInFile = _other.m_profileFoundInFile;
	m_chromaticities = _other.m_chromaticities;
	m_gammaCurve = _other.m_gammaCurve;
	m_gamma = _other.m_gamma;

	// Rebuild internal converter and matrices
	BuildTransformFromChroma( true );
}


//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_sRGB
//
const float4x4	ColorProfile::InternalColorConverter_sRGB::MAT_RGB2XYZ(
	0.4124f, 0.2126f, 0.0193f, 0.0f,
	0.3576f, 0.7152f, 0.1192f, 0.0f,
	0.1805f, 0.0722f, 0.9505f, 0.0f,
	0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_sRGB::MAT_XYZ2RGB(
	3.2406f, -0.9689f,  0.0557f, 0.0f,
	-1.5372f,  1.8758f, -0.2040f, 0.0f,
	-0.4986f,  0.0415f,  1.0570f, 0.0f,
	0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_sRGB::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = Linear2sRGB( _RGB.x );
	_RGB.y = Linear2sRGB( _RGB.y );
	_RGB.z = Linear2sRGB( _RGB.z );
}

void ColorProfile::InternalColorConverter_sRGB::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = sRGB2Linear( _RGB.x );
	_XYZ.y = sRGB2Linear( _RGB.y );
	_XYZ.z = sRGB2Linear( _RGB.z );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_sRGB::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = Linear2sRGB( _RGB->x );
		_RGB->y = Linear2sRGB( _RGB->y );
		_RGB->z = Linear2sRGB( _RGB->z );
	}
}

void ColorProfile::InternalColorConverter_sRGB::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = sRGB2Linear( _RGB->x );
		_XYZ->y = sRGB2Linear( _RGB->y );
		_XYZ->z = sRGB2Linear( _RGB->z );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_AdobeRGB_D50
//
const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D50::MAT_RGB2XYZ(
	0.60974f, 0.31111f, 0.01947f, 0.0f,
	0.20528f, 0.62567f, 0.06087f, 0.0f,
	0.14919f, 0.06322f, 0.74457f, 0.0f,
	0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D50::MAT_XYZ2RGB(
	 1.96253f, -0.97876f,  0.02869f, 0.0f,
	-0.61068f,  1.91615f, -0.14067f, 0.0f,
	-0.34137f,  0.03342f,  1.34926f, 0.0f,
	 0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_AdobeRGB_D50::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = powf( _RGB.x, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.y = powf( _RGB.y, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.z = powf( _RGB.z, 1.0f / GAMMA_EXPONENT_ADOBE );
}

void ColorProfile::InternalColorConverter_AdobeRGB_D50::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = powf( _RGB.x, GAMMA_EXPONENT_ADOBE );
	_XYZ.y = powf( _RGB.y, GAMMA_EXPONENT_ADOBE );
	_XYZ.z = powf( _RGB.z, GAMMA_EXPONENT_ADOBE );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_AdobeRGB_D50::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = powf( _RGB->x, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->y = powf( _RGB->y, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->z = powf( _RGB->z, 1.0f / GAMMA_EXPONENT_ADOBE );
	}
}

void ColorProfile::InternalColorConverter_AdobeRGB_D50::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = powf( _RGB->x, GAMMA_EXPONENT_ADOBE );
		_XYZ->y = powf( _RGB->y, GAMMA_EXPONENT_ADOBE );
		_XYZ->z = powf( _RGB->z, GAMMA_EXPONENT_ADOBE );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_AdobeRGB_D65
//
const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D65::MAT_RGB2XYZ(
	0.57667f, 0.29734f, 0.02703f, 0.0f,
	0.18556f, 0.62736f, 0.07069f, 0.0f,
	0.18823f, 0.07529f, 0.99134f, 0.0f,
	0.00000f, 0.00000f, 0.00000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_AdobeRGB_D65::MAT_XYZ2RGB(
	 2.04159f, -0.96924f,  0.01344f, 0.0f,
	-0.56501f,  1.87597f, -0.11836f, 0.0f,
	-0.34473f,  0.04156f,  1.01517f, 0.0f,
	 0.00000f,  0.00000f,  0.00000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_AdobeRGB_D65::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = powf( _XYZ.x, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.y = powf( _XYZ.y, 1.0f / GAMMA_EXPONENT_ADOBE );
	_RGB.z = powf( _XYZ.z, 1.0f / GAMMA_EXPONENT_ADOBE );
}

void ColorProfile::InternalColorConverter_AdobeRGB_D65::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = powf( _RGB.x, GAMMA_EXPONENT_ADOBE );
	_XYZ.y = powf( _RGB.y, GAMMA_EXPONENT_ADOBE );
	_XYZ.z = powf( _RGB.z, GAMMA_EXPONENT_ADOBE );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_AdobeRGB_D65::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = powf( _RGB->x, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->y = powf( _RGB->y, 1.0f / GAMMA_EXPONENT_ADOBE );
		_RGB->z = powf( _RGB->z, 1.0f / GAMMA_EXPONENT_ADOBE );
	}
}

void ColorProfile::InternalColorConverter_AdobeRGB_D65::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = powf( _RGB->x, GAMMA_EXPONENT_ADOBE );
		_XYZ->y = powf( _RGB->y, GAMMA_EXPONENT_ADOBE );
		_XYZ->z = powf( _RGB->z, GAMMA_EXPONENT_ADOBE );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_ProPhoto
//
const float4x4	ColorProfile::InternalColorConverter_ProPhoto::MAT_RGB2XYZ(
	0.7977f, 0.2880f, 0.0000f, 0.0f,
	0.1352f, 0.7119f, 0.0000f, 0.0f,
	0.0313f, 0.0001f, 0.8249f, 0.0f,
	0.0000f, 0.0000f, 0.0000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_ProPhoto::MAT_XYZ2RGB(
	 1.3460f, -0.5446f,  0.0000f, 0.0f,
	-0.2556f,  1.5082f,  0.0000f, 0.0f,
	-0.0511f,  0.0205f,  1.2123f, 0.0f,
	 0.0000f,  0.0000f,  0.0000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_ProPhoto::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;

	// Gamma correct
	_RGB.x = _RGB.x > 0.001953f ? powf( _RGB.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.x;
	_RGB.y = _RGB.y > 0.001953f ? powf( _RGB.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.y;
	_RGB.z = _RGB.z > 0.001953f ? powf( _RGB.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.z;
}

void ColorProfile::InternalColorConverter_ProPhoto::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = _RGB.x > 0.031248f ? powf( _RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.x / 16.0f;
	_XYZ.y = _RGB.y > 0.031248f ? powf( _RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.y / 16.0f;
	_XYZ.z = _RGB.z > 0.031248f ? powf( _RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.z / 16.0f;
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_ProPhoto::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * MAT_XYZ2RGB;

		// Gamma correct
		_RGB->x = _RGB->x > 0.001953f ? powf( _RGB->x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->x;
		_RGB->y = _RGB->y > 0.001953f ? powf( _RGB->y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->y;
		_RGB->z = _RGB->z > 0.001953f ? powf( _RGB->z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->z;
	}
}

void ColorProfile::InternalColorConverter_ProPhoto::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = _RGB->x > 0.031248f ? powf( _RGB->x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->x / 16.0f;
		_XYZ->y = _RGB->y > 0.031248f ? powf( _RGB->y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->y / 16.0f;
		_XYZ->z = _RGB->z > 0.031248f ? powf( _RGB->z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->z / 16.0f;
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Radiance
//
const float4x4	ColorProfile::InternalColorConverter_Radiance::MAT_RGB2XYZ(
	0.5141447f, 0.2651059f, 0.0241005f, 0.0f,
	0.3238845f, 0.6701059f, 0.1228527f, 0.0f,
	0.1619709f, 0.0647883f, 0.8530467f, 0.0f,
	0.0000000f, 0.0000000f, 0.0000000f, 1.0f		// Alpha stays the same
);

const float4x4	ColorProfile::InternalColorConverter_Radiance::MAT_XYZ2RGB(
	 2.5653124f, -1.02210832f,  0.07472437f, 0.0f,
	-1.1668493f,  1.97828662f, -0.25193953f, 0.0f,
	-0.3984632f,  0.04382159f,  1.17721522f, 0.0f,
	 0.0000000f,  0.00000000f,  0.00000000f, 1.0f	// Alpha stays the same
);

void ColorProfile::InternalColorConverter_Radiance::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * MAT_XYZ2RGB;
}

void ColorProfile::InternalColorConverter_Radiance::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Transform into XYZ
	_XYZ = _RGB * MAT_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Radiance::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_RGB = *_XYZ * MAT_XYZ2RGB;
	}
}

void ColorProfile::InternalColorConverter_Radiance::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_XYZ = *_RGB * MAT_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_NoGamma
//
void ColorProfile::InternalColorConverter_Generic_NoGamma::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;
}

void ColorProfile::InternalColorConverter_Generic_NoGamma::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Transform into XYZ
	_XYZ = _RGB * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_NoGamma::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_RGB = *_XYZ * m_XYZ2RGB;
	}
}

void ColorProfile::InternalColorConverter_Generic_NoGamma::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		*_XYZ = *_RGB * m_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_StandardGamma
//
void ColorProfile::InternalColorConverter_Generic_StandardGamma::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;

	// Gamma correct
	_RGB.x = powf( _RGB.x, m_InvGamma );
	_RGB.y = powf( _RGB.y, m_InvGamma );
	_RGB.z = powf( _RGB.z, m_InvGamma );
}

void ColorProfile::InternalColorConverter_Generic_StandardGamma::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = powf( _RGB.x, m_Gamma );
	_XYZ.y = powf( _RGB.y, m_Gamma );
	_XYZ.z = powf( _RGB.z, m_Gamma );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_StandardGamma::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * m_XYZ2RGB;

		// Gamma correct
		_RGB->x = powf( _RGB->x, m_InvGamma );
		_RGB->y = powf( _RGB->y, m_InvGamma );
		_RGB->z = powf( _RGB->z, m_InvGamma );
	}
}

void ColorProfile::InternalColorConverter_Generic_StandardGamma::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = powf( _RGB->x, m_Gamma );
		_XYZ->y = powf( _RGB->y, m_Gamma );
		_XYZ->z = powf( _RGB->z, m_Gamma );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * m_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_sRGBGamma
//
void ColorProfile::InternalColorConverter_Generic_sRGBGamma::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;

	// Gamma correct
	_RGB.x = _RGB.x > 0.0031308f ? 1.055f * powf( _RGB.x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB.x;
	_RGB.y = _RGB.y > 0.0031308f ? 1.055f * powf( _RGB.y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB.y;
	_RGB.z = _RGB.z > 0.0031308f ? 1.055f * powf( _RGB.z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB.z;
}

void ColorProfile::InternalColorConverter_Generic_sRGBGamma::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = _RGB.x < 0.04045f ? _RGB.x / 12.92f : powf( (_RGB.x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
	_XYZ.y = _RGB.y < 0.04045f ? _RGB.y / 12.92f : powf( (_RGB.y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
	_XYZ.z = _RGB.z < 0.04045f ? _RGB.z / 12.92f : powf( (_RGB.z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_sRGBGamma::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * m_XYZ2RGB;

		// Gamma correct
		_RGB->x = _RGB->x > 0.0031308f ? 1.055f * powf( _RGB->x, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB->x;
		_RGB->y = _RGB->y > 0.0031308f ? 1.055f * powf( _RGB->y, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB->y;
		_RGB->z = _RGB->z > 0.0031308f ? 1.055f * powf( _RGB->z, 1.0f / GAMMA_EXPONENT_sRGB ) - 0.055f : 12.92f * _RGB->z;
	}
}

void ColorProfile::InternalColorConverter_Generic_sRGBGamma::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = _RGB->x < 0.04045f ? _RGB->x / 12.92f : powf( (_RGB->x + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_XYZ->y = _RGB->y < 0.04045f ? _RGB->y / 12.92f : powf( (_RGB->y + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_XYZ->z = _RGB->z < 0.04045f ? _RGB->z / 12.92f : powf( (_RGB->z + 0.055f) / 1.055f, GAMMA_EXPONENT_sRGB );
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * m_RGB2XYZ;
	}
}

//////////////////////////////////////////////////////////////////////////
// InternalColorConverter_Generic_ProPhoto
//
void ColorProfile::InternalColorConverter_Generic_ProPhoto::XYZ2RGB( const bfloat4& _XYZ, bfloat4& _RGB ) const {
	// Transform into RGB
	_RGB = _XYZ * m_XYZ2RGB;

	// Gamma correct
	_RGB.x = _RGB.x > 0.001953f ? powf( _RGB.x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.x;
	_RGB.y = _RGB.y > 0.001953f ? powf( _RGB.y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.y;
	_RGB.z = _RGB.z > 0.001953f ? powf( _RGB.z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB.z;
}

void ColorProfile::InternalColorConverter_Generic_ProPhoto::RGB2XYZ( const bfloat4& _RGB, bfloat4& _XYZ ) const {
	// Gamma un-correct
	_XYZ.x = _RGB.x > 0.031248f ? powf( _RGB.x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.x / 16.0f;
	_XYZ.y = _RGB.y > 0.031248f ? powf( _RGB.y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.y / 16.0f;
	_XYZ.z = _RGB.z > 0.031248f ? powf( _RGB.z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB.z / 16.0f;
	_XYZ.w = _RGB.w;

	// Transform into XYZ
	_XYZ = _XYZ * m_RGB2XYZ;
}

void ColorProfile::InternalColorConverter_Generic_ProPhoto::XYZ2RGB( const bfloat4* _XYZ, bfloat4* _RGB, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Transform into RGB
		*_RGB = *_XYZ * m_XYZ2RGB;

		// Gamma correct
		_RGB->x = _RGB->x > 0.001953f ? powf( _RGB->x, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->x;
		_RGB->y = _RGB->y > 0.001953f ? powf( _RGB->y, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->y;
		_RGB->z = _RGB->z > 0.001953f ? powf( _RGB->z, 1.0f / GAMMA_EXPONENT_PRO_PHOTO ) : 16.0f * _RGB->z;
	}
}

void ColorProfile::InternalColorConverter_Generic_ProPhoto::RGB2XYZ( const bfloat4* _RGB, bfloat4* _XYZ, U32 _length ) const {
	for ( S32 i=S32(_length); i >= 0; i--, _XYZ++, _RGB++ ) {
		// Gamma un-correct
		_XYZ->x = _RGB->x > 0.031248f ? powf( _RGB->x, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->x / 16.0f;
		_XYZ->y = _RGB->y > 0.031248f ? powf( _RGB->y, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->y / 16.0f;
		_XYZ->z = _RGB->z > 0.031248f ? powf( _RGB->z, GAMMA_EXPONENT_PRO_PHOTO ) : _RGB->z / 16.0f;
		_XYZ->w = _RGB->w;

		// Transform into XYZ
		*_XYZ = *_XYZ * m_RGB2XYZ;
	}
}
