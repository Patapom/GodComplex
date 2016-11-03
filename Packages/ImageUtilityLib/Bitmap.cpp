#include "Bitmap.h"

using namespace ImageUtilityLib;

// This is the core of the bitmap class
// This method converts any image file into a float4 CIE XYZ format using the provided profile or the profile associated to the file
void	Bitmap::FromImageFile( const ImageFile& _sourceFile, const ColorProfile* _profileOverride ) {
	m_colorProfile = _profileOverride != nullptr ? _profileOverride : _sourceFile.GetColorProfile();
 	if ( m_colorProfile == nullptr )
 		throw "The provided file doesn't contain a valid color profile to initialize the bitmap!";

	// Convert for float4 format
	FIBITMAP*	float4Bitmap = FreeImage_ConvertToType( _sourceFile.m_bitmap, FIT_RGBAF );

	m_width = FreeImage_GetWidth( float4Bitmap );
	m_height = FreeImage_GetHeight( float4Bitmap );

	// Convert to XYZ in bulk using profile
	const float4*	source = (const float4*) FreeImage_GetBits( float4Bitmap );
	m_XYZ = new float4[m_width * m_height];
	m_colorProfile->RGB2XYZ( source, m_XYZ, U32(m_width * m_height) );

	FreeImage_Unload( float4Bitmap );
}

// And this method converts back the bitmap to any format
void	Bitmap::ToImageFile( ImageFile& _targetFile, ImageFile::PIXEL_FORMAT _targetFormat ) const {
 	if ( m_colorProfile == nullptr )
 		throw "The bitmap doesn't contain a valid color profile to initialize the image file!";

	FREE_IMAGE_TYPE	targetType = ImageFile::PixelFormat2FIT( _targetFormat );
	if ( targetType == FIT_UNKNOWN )
		throw "Unsupported target type!";

	// Convert back to float4 RGB using color profile
	ImageFile	float4Image( m_width, m_height, ImageFile::PIXEL_FORMAT::RGBA32F );
	float4*		target = (float4*) float4Image.Bits();
	m_colorProfile->XYZ2RGB( m_XYZ, target, m_width*m_height );

	// Convert to target bitmap
	FIBITMAP*	targetBitmap = FreeImage_ConvertToType( float4Image.m_bitmap, targetType );

	// Substitute bitmap pointer into target file
	_targetFile.Exit();
	_targetFile.m_bitmap = targetBitmap;
}
