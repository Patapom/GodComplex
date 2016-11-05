#include "stdafx.h"
#include "Bitmap.h"

using namespace ImageUtilityLib;

// This is the core of the bitmap class
// This method converts any image file into a float4 CIE XYZ format using the provided profile or the profile associated to the file
void	Bitmap::FromImageFile( const ImageFile& _sourceFile, const ColorProfile* _profileOverride, bool _unPremultiplyAlpha ) {
	m_colorProfile = _profileOverride != nullptr ? _profileOverride : _sourceFile.GetColorProfile();
 	if ( m_colorProfile == nullptr )
 		throw "The provided file doesn't contain a valid color profile and you did not provide any profile override to initialize the bitmap!";

	// Convert for float4 format
	FIBITMAP*	float4Bitmap = FreeImage_ConvertToType( _sourceFile.m_bitmap, FIT_RGBAF );

	m_width = FreeImage_GetWidth( float4Bitmap );
	m_height = FreeImage_GetHeight( float4Bitmap );

	// Convert to XYZ in bulk using profile
	const bfloat4*	source = (const bfloat4*) FreeImage_GetBits( float4Bitmap );
	m_XYZ = new bfloat4[m_width * m_height];
	m_colorProfile->RGB2XYZ( source, m_XYZ, U32(m_width * m_height) );

	FreeImage_Unload( float4Bitmap );

	if ( _unPremultiplyAlpha ) {
		// Un-pre-multiply by alpha
		bfloat4*	unPreMultipliedTarget = m_XYZ;
		for ( U32 i=m_width*m_height; i > 0; i--, unPreMultipliedTarget++ ) {
			if ( unPreMultipliedTarget->w > 0.0f ) {
				float	invAlpha = 1.0f / unPreMultipliedTarget->w;
				unPreMultipliedTarget->x *= invAlpha;
				unPreMultipliedTarget->y *= invAlpha;
				unPreMultipliedTarget->z *= invAlpha;
			}
		}
	}
}

// And this method converts back the bitmap to any format
void	Bitmap::ToImageFile( ImageFile& _targetFile, ImageFile::PIXEL_FORMAT _targetFormat, bool _premultiplyAlpha ) const {
 	if ( m_colorProfile == nullptr )
 		throw "The bitmap doesn't contain a valid color profile to initialize the image file!";

	FREE_IMAGE_TYPE	targetType = ImageFile::PixelFormat2FIT( _targetFormat );
	if ( targetType == FIT_UNKNOWN )
		throw "Unsupported target type!";

	// Convert back to float4 RGB using color profile
	ImageFile		float4Image( m_width, m_height, ImageFile::PIXEL_FORMAT::RGBA32F, *m_colorProfile );
	const bfloat4*	source = m_XYZ;
	bfloat4*			target = (bfloat4*) float4Image.GetBits();
	if ( _premultiplyAlpha ) {
		// Pre-multiply by alpha
		const bfloat4*	unPreMultipliedSource = m_XYZ;
		bfloat4*			preMultipliedTarget = target;
		for ( U32 i=m_width*m_height; i > 0; i--, unPreMultipliedSource++, preMultipliedTarget++ ) {
			preMultipliedTarget->x = unPreMultipliedSource->x * unPreMultipliedSource->w;
			preMultipliedTarget->y = unPreMultipliedSource->y * unPreMultipliedSource->w;
			preMultipliedTarget->z = unPreMultipliedSource->z * unPreMultipliedSource->w;
			preMultipliedTarget->w = unPreMultipliedSource->w;
		}
		source = target;	// In-place conversion
	}
	m_colorProfile->XYZ2RGB( source, target, m_width*m_height );

	// Convert to target bitmap
	FIBITMAP*	targetBitmap = FreeImage_ConvertToType( float4Image.m_bitmap, targetType );

	// Substitute bitmap pointer into target file
	_targetFile.Exit();
	_targetFile.m_bitmap = targetBitmap;
}
