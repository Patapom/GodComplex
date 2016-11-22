#include "stdafx.h"

#include "ImageFile.h"
#include "Bitmap.h"

using namespace ImageUtilityLib;

U32	ImageFile::ms_freeImageUsageRefCount = 0;

ImageFile::ImageFile()
	: m_bitmap( nullptr )
	, m_pixelFormat( PIXEL_FORMAT::UNKNOWN )
	, m_pixelAccessor( nullptr )
	, m_fileFormat( FILE_FORMAT::UNKNOWN )
{}

ImageFile::ImageFile( const ImageFile& _other )
	: m_bitmap( nullptr ) {
	*this = _other;
}

ImageFile::ImageFile( const wchar_t* _fileName, FILE_FORMAT _format )
	: m_bitmap( nullptr )
{
	Load( _fileName, _format );
}

ImageFile::ImageFile( const U8* _fileContent, U64 _fileSize, FILE_FORMAT _format )
	: m_bitmap( nullptr )
{
	Load( _fileContent, _fileSize, _format );
}

ImageFile::ImageFile( U32 _width, U32 _height, PIXEL_FORMAT _format, const ColorProfile& _colorProfile )
	: m_bitmap( nullptr )
{
	Init( _width, _height, _format, _colorProfile );
}

ImageFile::~ImageFile() {
	Exit();
	UnUseFreeImage();
}

bool	ImageFile::HasAlpha() const {
	switch ( m_pixelFormat ) {
	case PIXEL_FORMAT::RGBA8:
	case PIXEL_FORMAT::RGBA16:
	case PIXEL_FORMAT::RGBA16F:
	case PIXEL_FORMAT::RGBA32F:
		return true;
	}
	return false;
}

const IPixelAccessor&	ImageFile::GetPixelFormatAccessor( PIXEL_FORMAT _pixelFormat ) {
	switch ( _pixelFormat ) {
		// 8-bits
	case PIXEL_FORMAT::R8:			return PF_R8::Descriptor;
	case PIXEL_FORMAT::RG8:			return PF_RG8::Descriptor;
	case PIXEL_FORMAT::RGB8:		return PF_RGB8::Descriptor;
	case PIXEL_FORMAT::RGBA8:		return PF_RGBA8::Descriptor;

		// 16-bits
	case PIXEL_FORMAT::R16:			return PF_R16::Descriptor;
//	case PIXEL_FORMAT::RG16,		return PF_RG16::Descriptor;		// Unsupported
	case PIXEL_FORMAT::RGB16:		return PF_RGB16::Descriptor;
	case PIXEL_FORMAT::RGBA16:		return PF_RGBA16::Descriptor;

		// 16-bits half-precision floating points
	case PIXEL_FORMAT::R16F:		return PF_R16F::Descriptor;
	case PIXEL_FORMAT::RG16F:		return PF_RG16F::Descriptor;
	case PIXEL_FORMAT::RGB16F:		return PF_RGB16F::Descriptor;
	case PIXEL_FORMAT::RGBA16F:		return PF_RGBA16F::Descriptor;

		// 32-bits
	case PIXEL_FORMAT::R32F:		return PF_R32F::Descriptor;
// 	case PIXEL_FORMAT::RG32F:		return PF_RG32F::Descriptor;	// Unsupported
	case PIXEL_FORMAT::RGB32F:		return PF_RGB32F::Descriptor;
	case PIXEL_FORMAT::RGBA32F:		return PF_RGBA32F::Descriptor;
	}

	return PF_Unknown::Descriptor;
}

void	ImageFile::Get( U32 _X, U32 _Y, bfloat4& _color ) const {
	const unsigned	pitch  = FreeImage_GetPitch( m_bitmap );
	const U8*		bits = (BYTE*) FreeImage_GetBits( m_bitmap );
	bits += pitch * _Y + m_pixelAccessor->Size() * _X;

	m_pixelAccessor->RGBA( bits, _color );
}
void	ImageFile::Set( U32 _X, U32 _Y, const bfloat4& _color ) {
	const unsigned	pitch  = FreeImage_GetPitch( m_bitmap );
	U8*		bits = (BYTE*) FreeImage_GetBits( m_bitmap );
	bits += pitch * _Y + m_pixelAccessor->Size() * _X;

	m_pixelAccessor->Write( bits, _color );
}
void	ImageFile::Add( U32 _X, U32 _Y, const bfloat4& _color ) {
	const unsigned	pitch  = FreeImage_GetPitch( m_bitmap );
	U8*		bits = (BYTE*) FreeImage_GetBits( m_bitmap );
	bits += pitch * _Y + m_pixelAccessor->Size() * _X;

	bfloat4	temp;
	m_pixelAccessor->RGBA( bits, temp );
	temp += _color;
	m_pixelAccessor->Write( bits, temp );
}


ImageFile&	ImageFile::operator=( const ImageFile& _other ) {
	UseFreeImage();
	Exit();

	m_bitmap = FreeImage_Clone( _other.m_bitmap );
	m_pixelFormat = _other.m_pixelFormat;
	m_pixelAccessor = _other.m_pixelAccessor;
	m_fileFormat = _other.m_fileFormat;
	m_metadata = _other.m_metadata;

	return *this;
}

void	ImageFile::Init( U32 _width, U32 _height, PIXEL_FORMAT _format, const ColorProfile& _colorProfile ) {
	UseFreeImage();
	Exit();

	m_pixelFormat = _format;
	m_pixelAccessor = &GetPixelFormatAccessor( _format );

	FREE_IMAGE_TYPE	bitmapType = PixelFormat2FIT( _format );
	int				BPP = int( PixelFormat2BPP( _format ) );
	m_bitmap = FreeImage_AllocateT( bitmapType, _width, _height, BPP );
	if ( m_bitmap == nullptr )
		throw "Failed to initialize image file!";

	// Copy color profile
	m_metadata.m_colorProfile = new ColorProfile( _colorProfile );
}

void	ImageFile::Exit() {
	if ( m_bitmap != nullptr ) {
		FreeImage_Unload( m_bitmap );
		m_bitmap = nullptr;
	}
	m_pixelFormat = PIXEL_FORMAT::UNKNOWN;
	m_pixelAccessor = nullptr;
	m_fileFormat = FILE_FORMAT::UNKNOWN;
	m_metadata.Reset();
}

//////////////////////////////////////////////////////////////////////////
// Load
void	ImageFile::Load( const wchar_t* _fileName ) {
	// Attempt to retrieve the file format from the file name
	FILE_FORMAT	format = GetFileType( _fileName );
	Load( _fileName, format );
}
void	ImageFile::Load( const wchar_t* _fileName, FILE_FORMAT _format ) {
	UseFreeImage();
	Exit();

	if ( _format == FILE_FORMAT::UNKNOWN )
		throw "Unrecognized image file format!";

	m_fileFormat = _format;
	m_bitmap = FreeImage_LoadU( FileFormat2FIF( _format ), _fileName );
	if ( m_bitmap == nullptr )
		throw "Failed to load image file!";

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back
	FreeImage_FlipVertical( m_bitmap );

	m_pixelFormat = Bitmap2PixelFormat( *m_bitmap );
	m_pixelAccessor = &GetPixelFormatAccessor( m_pixelFormat );

	m_metadata.RetrieveFromImage( *this );
}
void	ImageFile::Load( const void* _fileContent, U64 _fileSize, FILE_FORMAT _format ) {
	UseFreeImage();
	Exit();

	if ( _format == FILE_FORMAT::UNKNOWN )
		throw "Unrecognized image file format!";

	FIMEMORY*	mem = FreeImage_OpenMemory( (BYTE*) _fileContent, U32(_fileSize) );
	if ( mem == nullptr )
		throw "Failed to read bitmap content into memory!";

	m_fileFormat = _format;
	m_bitmap = FreeImage_LoadFromMemory( FileFormat2FIF( _format ), mem );
	FreeImage_CloseMemory( mem );

	if ( m_bitmap == nullptr )
		throw "Failed to load image file!";

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back
	FreeImage_FlipVertical( m_bitmap );

	m_pixelFormat = Bitmap2PixelFormat( *m_bitmap );
	m_pixelAccessor = &GetPixelFormatAccessor( m_pixelFormat );

	m_metadata.RetrieveFromImage( *this );
}

//////////////////////////////////////////////////////////////////////////
// Save
void	ImageFile::Save( const wchar_t* _fileName ) const {
	FILE_FORMAT	format = GetFileType( _fileName );
	Save( _fileName, format );
}
void	ImageFile::Save( const wchar_t* _fileName, FILE_FORMAT _format ) const {
	Save( _fileName, _format, SAVE_FLAGS(0) );
}
void	ImageFile::Save( const wchar_t* _fileName, FILE_FORMAT _format, SAVE_FLAGS _options ) const {
	if ( _format == FILE_FORMAT::UNKNOWN )
		throw "Unrecognized image file format!";
	if ( m_bitmap == nullptr )
		throw "Invalid bitmap to save!";

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back before saving
	FreeImage_FlipVertical( m_bitmap );

	m_fileFormat = _format;
	if ( !FreeImage_SaveU( FileFormat2FIF( _format ), m_bitmap, _fileName, int(_options) ) )
		throw "Failed to save the image file!";

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back after saving
	FreeImage_FlipVertical( m_bitmap );
}
void	ImageFile::Save( FILE_FORMAT _format, SAVE_FLAGS _options, U64 _fileSize, void*& _fileContent ) const {
	if ( _format == FILE_FORMAT::UNKNOWN )
		throw "Unrecognized image file format!";
	if ( m_bitmap == nullptr )
		throw "Invalid bitmap to save!";

	m_fileFormat = _format;

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back before saving
	FreeImage_FlipVertical( m_bitmap );

	// Save into a stream of unknown size
	FIMEMORY*	stream = FreeImage_OpenMemory();
	if ( !FreeImage_SaveToMemory( FileFormat2FIF( _format ), m_bitmap, stream, int(_options) ) )
		throw "Failed to save the image file!";

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back before saving
	FreeImage_FlipVertical( m_bitmap );

	// Copy into a custom buffer
	_fileSize = FreeImage_TellMemory( stream );
	_fileContent = new U8[_fileSize];
	FIMEMORY*	target = FreeImage_OpenMemory( (BYTE*) _fileContent, U32(_fileSize) );

	FreeImage_SeekMemory( stream, 0, SEEK_SET );
	FreeImage_ReadMemory( _fileContent, 1, _fileSize, stream );

	FreeImage_CloseMemory( target );
	FreeImage_CloseMemory( stream );
}

//////////////////////////////////////////////////////////////////////////
// Conversion
void	ImageFile::ConvertFrom( const ImageFile& _source, PIXEL_FORMAT _targetFormat ) {
	Exit();

	// Ensure we're not dealing with half-precision floats!
	if (	(U32(_source.m_pixelFormat) & U32(PIXEL_FORMAT::NOT_NATIVELY_SUPPORTED))
		 || (U32(_targetFormat) & U32(PIXEL_FORMAT::NOT_NATIVELY_SUPPORTED)) )
		throw "You cannot convert to or from the half-precision floating point formats because they are not natively supported by FreeImage! (and I'm lazy and never wrote the converters myself :/)";

	// Convert source
	FREE_IMAGE_TYPE	sourceType = PixelFormat2FIT( _source.m_pixelFormat );
	FREE_IMAGE_TYPE	targetType = PixelFormat2FIT( _targetFormat );
	if ( targetType == FIT_BITMAP ) {
		// Check the source is not a HDR format
		if ( sourceType == FIT_RGBF || sourceType == FIT_RGBAF )
			throw "You need to use the ToneMap() function to convert HDR formats into a LDR format!";

		// Convert to temporary bitmap first
		// If the source is already a standard type bitmap then it is cloned
		FIBITMAP*	temp = FreeImage_ConvertToType( _source.m_bitmap, FIT_BITMAP );
		if ( temp == nullptr )
			throw "FreeImage failed to convert to standard bitmap type!";

		// Now check bits per pixel
		U32		sourceBPP = FreeImage_GetBPP( temp );
		U32		targetBPP = PixelFormat2BPP( _targetFormat );
		if ( sourceBPP == targetBPP ) {
			// Okay so the source and target BPP are the same, just use our freshly converted bitmap then
			m_bitmap = temp;
			temp = nullptr;
		} else {
			switch ( sourceBPP ) {
			case 8:
				switch ( targetBPP ) {
				case 16: throw "8 -> 16 bits per pixel is not a supported conversion!";
				case 24: m_bitmap = FreeImage_ConvertTo24Bits( temp );
				case 32: m_bitmap = FreeImage_ConvertTo32Bits( temp );
				}
				break;

			case 16:
				switch ( targetBPP ) {
				case 8: m_bitmap = FreeImage_ConvertTo8Bits( temp );
				case 24: m_bitmap = FreeImage_ConvertTo24Bits( temp );
				case 32: m_bitmap = FreeImage_ConvertTo32Bits( temp );
				}
				break;

			case 24:
				switch ( targetBPP ) {
				case 8: m_bitmap = FreeImage_ConvertTo8Bits( temp );
				case 16: throw "24 -> 16 bits per pixel is not a supported conversion!";
				case 32: m_bitmap = FreeImage_ConvertTo32Bits( temp );
				}
				break;

			case 32:
				switch ( targetBPP ) {
				case 8: m_bitmap = FreeImage_ConvertTo8Bits( temp );
				case 16: throw "32 -> 16 bits per pixel is not a supported conversion!";
				case 24: m_bitmap = FreeImage_ConvertTo24Bits( temp );
				}
				break;
			}
		}

		if ( temp != nullptr ) {
			FreeImage_Unload( temp );
		}
	} else {
		// Not a simple bitmap type
		m_bitmap = FreeImage_ConvertToType( _source.m_bitmap, targetType );
	}

	// Get pixel format from bitmap
	m_pixelFormat = Bitmap2PixelFormat( *m_bitmap );
	m_pixelAccessor = &GetPixelFormatAccessor( m_pixelFormat );

	// Copy metadata
	m_metadata = _source.m_metadata;

	// Copy file format
	m_fileFormat = _source.m_fileFormat;
}

void	ImageFile::ToneMapFrom( const ImageFile& _source, toneMapper_t _toneMapper ) {
	Exit();

	// Check the source is a HDR format
	switch ( _source.m_pixelFormat ) {
	case PIXEL_FORMAT::R16F:
	case PIXEL_FORMAT::RG16F:
	case PIXEL_FORMAT::RGB16F:
	case PIXEL_FORMAT::RGBA16F:
	case PIXEL_FORMAT::R32F:
	case PIXEL_FORMAT::RG32F:
	case PIXEL_FORMAT::RGB32F:
	case PIXEL_FORMAT::RGBA32F:
		break;	// Okay!
	default:
		throw "You must provide a HDR format to use the ToneMap() function!";
	}

	U32	W = _source.Width();
	U32	H = _source.Height();
	const IPixelAccessor&	accessor = *_source.m_pixelAccessor;
	U32	pixelSize = accessor.Size();

	// Convert source
	if (	_source.m_pixelFormat == ImageFile::PIXEL_FORMAT::R16F
		 || _source.m_pixelFormat == ImageFile::PIXEL_FORMAT::R32F ) {
		// Convert to R8
		m_bitmap = FreeImage_Allocate( W, H, 8, FI_RGBA_RED_MASK, 0, 0 );

		const unsigned	src_pitch  = FreeImage_GetPitch( _source.m_bitmap );
		const unsigned	dst_pitch  = FreeImage_GetPitch( m_bitmap );

		const U8*	src_bits = (U8*) FreeImage_GetBits( _source.m_bitmap );
		U8*			dst_bits = (U8*) FreeImage_GetBits( m_bitmap );

		bfloat3		tempHDR;
		bfloat3		tempLDR;
		for ( U32 Y=0; Y < H; Y++ ) {
			const U8*	src_pixel = src_bits;
			U8*			dst_pixel = (BYTE*) dst_bits;
			for ( U32 X=0; X < W; X++, src_pixel+=pixelSize, dst_pixel++ ) {
				// Apply tone mapping
				tempHDR.x = accessor.Red( src_pixel );
				tempHDR.y = tempHDR.x;
				tempHDR.z = tempHDR.x;
				(*_toneMapper)( tempHDR, tempLDR );
				tempLDR.x = CLAMP( tempLDR.x, 0.0f, 1.0f );

				// Write clamped LDR value
				dst_pixel[FI_RGBA_RED]   = BYTE(255.0F * tempLDR.x + 0.5F);
			}
			src_bits += src_pitch;
			dst_bits += dst_pitch;
		}
	// ===============================================================================
	} else if (	_source.m_pixelFormat == ImageFile::PIXEL_FORMAT::RG16F
			 || _source.m_pixelFormat == ImageFile::PIXEL_FORMAT::RG32F ) {
		// Convert to RG8
		m_bitmap = FreeImage_Allocate( W, H, 16, FI_RGBA_RED_MASK, FI_RGBA_GREEN_MASK, 0 );

		const unsigned	src_pitch  = FreeImage_GetPitch( _source.m_bitmap );
		const unsigned	dst_pitch  = FreeImage_GetPitch( m_bitmap );

		const U8*	src_bits = (U8*) FreeImage_GetBits( _source.m_bitmap );
		U8*			dst_bits = (U8*) FreeImage_GetBits( m_bitmap );

		bfloat4		tempHDR;
		bfloat3		tempLDR;
		for ( U32 Y=0; Y < H; Y++ ) {
			const U8*	src_pixel = src_bits;
			U8*			dst_pixel = (BYTE*) dst_bits;
			for ( U32 X=0; X < W; X++, src_pixel+=pixelSize, dst_pixel += 2 ) {
				// Apply tone mapping
				accessor.RGBA( src_pixel, tempHDR );
				(*_toneMapper)( (bfloat3&) tempHDR, tempLDR );
				tempLDR.x = CLAMP( tempLDR.x, 0.0f, 1.0f );
				tempLDR.y = CLAMP( tempLDR.y, 0.0f, 1.0f );
				tempLDR.z = CLAMP( tempLDR.z, 0.0f, 1.0f );

				// Write clamped LDR value
				dst_pixel[FI_RGBA_RED]   = BYTE(255.0F * tempLDR.x + 0.5F);
				dst_pixel[FI_RGBA_GREEN] = BYTE(255.0F * tempLDR.y + 0.5F);
			}
			src_bits += src_pitch;
			dst_bits += dst_pitch;
		}
	// ===============================================================================
	} else if (	_source.m_pixelFormat == ImageFile::PIXEL_FORMAT::RGB16F
			 || _source.m_pixelFormat == ImageFile::PIXEL_FORMAT::RGB32F ) {
		// Convert to RGB8
		m_bitmap = FreeImage_Allocate( W, H, 24, FI_RGBA_RED_MASK, FI_RGBA_GREEN_MASK, FI_RGBA_BLUE_MASK );

		const unsigned	src_pitch  = FreeImage_GetPitch( _source.m_bitmap );
		const unsigned	dst_pitch  = FreeImage_GetPitch( m_bitmap );

		const U8*	src_bits = (U8*) FreeImage_GetBits( _source.m_bitmap );
		U8*			dst_bits = (U8*) FreeImage_GetBits( m_bitmap );

		bfloat4		tempHDR;
		bfloat3		tempLDR;
		for ( U32 Y=0; Y < H; Y++ ) {
			const U8*	src_pixel = src_bits;
			U8*			dst_pixel = (BYTE*) dst_bits;
			for ( U32 X=0; X < W; X++, src_pixel+=pixelSize, dst_pixel += 3 ) {
				// Apply tone mapping
				accessor.RGBA( src_pixel, tempHDR );
				(*_toneMapper)( (bfloat3&) tempHDR, tempLDR );
				tempLDR.x = CLAMP( tempLDR.x, 0.0f, 1.0f );
				tempLDR.y = CLAMP( tempLDR.y, 0.0f, 1.0f );
				tempLDR.z = CLAMP( tempLDR.z, 0.0f, 1.0f );

				// Write clamped LDR value
				dst_pixel[FI_RGBA_RED]   = BYTE(255.0F * tempLDR.x + 0.5F);
				dst_pixel[FI_RGBA_GREEN] = BYTE(255.0F * tempLDR.y + 0.5F);
				dst_pixel[FI_RGBA_BLUE]  = BYTE(255.0F * tempLDR.z + 0.5F);
			}
			src_bits += src_pitch;
			dst_bits += dst_pitch;
		}
	// ===============================================================================
	} else if (	_source.m_pixelFormat == ImageFile::PIXEL_FORMAT::RGBA16F
			 || _source.m_pixelFormat == ImageFile::PIXEL_FORMAT::RGBA32F ) {
		// Convert to RGBA8
		m_bitmap = FreeImage_Allocate( W, H, 32, FI_RGBA_RED_MASK, FI_RGBA_GREEN_MASK, FI_RGBA_BLUE_MASK );

		const unsigned	src_pitch  = FreeImage_GetPitch( _source.m_bitmap );
		const unsigned	dst_pitch  = FreeImage_GetPitch( m_bitmap );

		const U8*	src_bits = (BYTE*) FreeImage_GetBits( _source.m_bitmap );
		U8*			dst_bits = (BYTE*) FreeImage_GetBits( m_bitmap );

		bfloat4		tempHDR;
		bfloat3		tempLDR;
		for ( U32 Y=0; Y < H; Y++ ) {
			const U8*	src_pixel = src_bits;
			U8*			dst_pixel = (BYTE*) dst_bits;
			for ( U32 X=0; X < W; X++, src_pixel+=pixelSize, dst_pixel += 4 ) {
				// Apply tone mapping
				accessor.RGBA( src_pixel, tempHDR );
				(*_toneMapper)( (bfloat3&) tempHDR, tempLDR );
				tempLDR.x = CLAMP( tempLDR.x, 0.0f, 1.0f );
				tempLDR.y = CLAMP( tempLDR.y, 0.0f, 1.0f );
				tempLDR.z = CLAMP( tempLDR.z, 0.0f, 1.0f );
				float	A = CLAMP( tempHDR.w, 0.0f, 1.0f );

				// Write clamped LDR value
				dst_pixel[FI_RGBA_RED]   = BYTE(255.0F * tempLDR.x + 0.5F);
				dst_pixel[FI_RGBA_GREEN] = BYTE(255.0F * tempLDR.y + 0.5F);
				dst_pixel[FI_RGBA_BLUE]  = BYTE(255.0F * tempLDR.z + 0.5F);
				dst_pixel[FI_RGBA_ALPHA] = BYTE(255.0F * A + 0.5F);
			}
			src_bits += src_pitch;
			dst_bits += dst_pitch;
		}
	} else
		throw "Unsupported source HDR format!";

	// Get pixel format from bitmap
	m_pixelFormat = Bitmap2PixelFormat( *m_bitmap );
	m_pixelAccessor = &GetPixelFormatAccessor( m_pixelFormat );

	// Copy metadata
	m_metadata = _source.m_metadata;

	// Copy file format
	m_fileFormat = _source.m_fileFormat;
}

void	ImageFile::ReadScanline( U32 _Y, bfloat4* _color, U32 _startX, U32 _count ) const {
	U32	W = Width();
	U32	pixelSize = m_pixelAccessor->Size();

	const unsigned	pitch  = FreeImage_GetPitch( m_bitmap );
	const U8*		bits = (BYTE*) FreeImage_GetBits( m_bitmap );
	bits += pitch * _Y + _startX * pixelSize;

	_count = MIN( _count, W-_startX );
	for ( U32 i=_count; i > 0; i--, bits += pixelSize, _color++ ) {
		m_pixelAccessor->RGBA( bits, *_color );
	}
}
void	ImageFile::WriteScanline( U32 _Y, const bfloat4* _color, U32 _startX, U32 _count ) {
	U32	W = Width();
	U32	pixelSize = m_pixelAccessor->Size();

	const unsigned	pitch  = FreeImage_GetPitch( m_bitmap );
	U8*				bits = (BYTE*) FreeImage_GetBits( m_bitmap );
	bits += pitch * _Y + _startX * pixelSize;

	_count = MIN( _count, W-_startX );
	for ( U32 i=_count; i > 0; i--, bits += pixelSize, _color++ ) {
		m_pixelAccessor->Write( bits, *_color );
	}
}


//////////////////////////////////////////////////////////////////////////
// Helpers
ImageFile::FILE_FORMAT	ImageFile::GetFileType( const wchar_t* _imageFileNameName ) {
	if ( _imageFileNameName == nullptr )
		return FILE_FORMAT::UNKNOWN;

#if 1
	// Don't bother: use FreeImage!
	FILE_FORMAT	result = FIF2FileFormat( FreeImage_GetFileTypeU( _imageFileNameName, 0 ) );
	return result;

#else
	// Search for last . occurrence
	size_t	length = strlen( _imageFileNameName );
	size_t	extensionIndex;
	for ( extensionIndex=length-1; extensionIndex >= 0; extensionIndex++ ) {
		if ( _imageFileNameName[extensionIndex] == '.' )
			break;
	}
	if ( extensionIndex == 0 )
		return FILE_FORMAT::UNKNOWN;

//	// Copy extension and make it uppercase
//	char	temp[64];
//	strcpy_s( temp, 64, _imageFileNameName + extensionIndex );
//	_strupr_s( temp, 64 );

	const char*	extension = _imageFileNameName + extensionIndex;

	// Check for known extensions
	struct KnownExtension {
		const char*	extension;
		FILE_FORMAT	format;
	}	knownExtensions[] = {
		{ ".JPG",	FILE_FORMAT::JPEG },
		{ ".JPEG",	FILE_FORMAT::JPEG },
		{ ".JPE",	FILE_FORMAT::JPEG },
		{ ".BMP",	FILE_FORMAT::BMP },
		{ ".ICO",	FILE_FORMAT::ICO },
		{ ".PNG",	FILE_FORMAT::PNG },
		{ ".TGA",	FILE_FORMAT::TARGA },
		{ ".TIF",	FILE_FORMAT::TIFF },
		{ ".TIFF",	FILE_FORMAT::TIFF },
		{ ".GIF",	FILE_FORMAT::GIF },
		{ ".CRW",	FILE_FORMAT::RAW },
		{ ".CR2",	FILE_FORMAT::RAW },
		{ ".DNG",	FILE_FORMAT::RAW },
		{ ".HDR",	FILE_FORMAT::HDR },
		{ ".EXR",	FILE_FORMAT::EXR },
		{ ".DDS",	FILE_FORMAT::DDS },
		{ ".PSD",	FILE_FORMAT::PSD },
		{ ".PSB",	FILE_FORMAT::PSD },
	};

	U32						knownExtensionsCount = sizeof(knownExtensions) / sizeof(KnownExtension);
	const KnownExtension*	knownExtension = knownExtensions;
	for ( U32 knownExtensionIndex=0; knownExtensionIndex < knownExtensionsCount; knownExtensionIndex++, knownExtension++ ) {
		if ( _stricmp( extension, knownExtension->extension ) == 0 ) {
			return knownExtension->format;
		}
	}

	return FILE_FORMAT::UNKNOWN;
#endif
}

U32	ImageFile::PixelFormat2BPP( PIXEL_FORMAT _pixelFormat ) {
	switch (_pixelFormat ) {
		// 8-bits
		case PIXEL_FORMAT::R8:		return 8;
		case PIXEL_FORMAT::RG8:		return 16;
		case PIXEL_FORMAT::RGB8:	return 24;
		case PIXEL_FORMAT::RGBA8:	return 32;

		// 16-bits
		case PIXEL_FORMAT::R16:		return 16;
//		case PIXEL_FORMAT::RG16:	return 32;	// Unsupported
		case PIXEL_FORMAT::RGB16:	return 48;
		case PIXEL_FORMAT::RGBA16:	return 64;

		// 16-bits half-precision floating points
		case PIXEL_FORMAT::R16F:	return 16;
		case PIXEL_FORMAT::RG16F:	return 32;
		case PIXEL_FORMAT::RGB16F:	return 48;
		case PIXEL_FORMAT::RGBA16F:	return 64;

		// 32-bits
		case PIXEL_FORMAT::R32F:	return 32;
		case PIXEL_FORMAT::RG32F:	return 64;
		case PIXEL_FORMAT::RGB32F:	return 96;
		case PIXEL_FORMAT::RGBA32F:	return 128;
	};

	return 0;
}

// Determine target bitmap type based on target pixel format
FREE_IMAGE_TYPE	ImageFile::PixelFormat2FIT( PIXEL_FORMAT _pixelFormat ) {
	switch ( _pixelFormat ) {
		// 8-bits
		case ImageFile::PIXEL_FORMAT::R8:		return FIT_BITMAP;
		case ImageFile::PIXEL_FORMAT::RG8:		return FIT_BITMAP;
		case ImageFile::PIXEL_FORMAT::RGB8:		return FIT_BITMAP;
		case ImageFile::PIXEL_FORMAT::RGBA8:	return FIT_BITMAP;
		// 16-bits
		case ImageFile::PIXEL_FORMAT::R16:		return FIT_UINT16;
//		case ImageFile::PIXEL_FORMAT::RG16:		// Unsupported
		case ImageFile::PIXEL_FORMAT::RGB16:	return FIT_RGB16;
		case ImageFile::PIXEL_FORMAT::RGBA16:	return FIT_RGBA16;
		// 16-bits half-precision floating points
		case ImageFile::PIXEL_FORMAT::R16F:		return FIT_UINT16;
		case ImageFile::PIXEL_FORMAT::RG16F:	return FIT_RGB16;	// Here we unfortunately have to use a larger format to accomodate for our 2 components
		case ImageFile::PIXEL_FORMAT::RGB16F:	return FIT_RGB16;
		case ImageFile::PIXEL_FORMAT::RGBA16F:	return FIT_RGBA16;
		// 32-bits
		case ImageFile::PIXEL_FORMAT::R32F:		return FIT_FLOAT;
//		case ImageFile::PIXEL_FORMAT::RG32F:	// Unsupported
		case ImageFile::PIXEL_FORMAT::RGB32F:	return FIT_RGBF;
		case ImageFile::PIXEL_FORMAT::RGBA32F:	return FIT_RGBAF;
	}

	return FIT_UNKNOWN;
}

ImageFile::PIXEL_FORMAT	ImageFile::Bitmap2PixelFormat( const FIBITMAP& _bitmap ) {
	FREE_IMAGE_TYPE	type = FreeImage_GetImageType( const_cast< FIBITMAP* >( &_bitmap ) );
	switch ( type ) {
		// 8-bits
		case FIT_BITMAP: {
			U32	bpp = FreeImage_GetBPP( const_cast< FIBITMAP* >( &_bitmap ) );
			switch ( bpp ) {
				case 8:							return PIXEL_FORMAT::R8;
				case 16:						return PIXEL_FORMAT::RG8;
				case 24:						return PIXEL_FORMAT::RGB8;
				case 32:						return PIXEL_FORMAT::RGBA8;
			}
			break;
		}
		// 16-bits
		case FIT_UINT16:						return PIXEL_FORMAT::R16;
		case FIT_RGB16:							return PIXEL_FORMAT::RGB16;
		case FIT_RGBA16:						return PIXEL_FORMAT::RGBA16;
		// 32-bits
		case FIT_FLOAT:							return PIXEL_FORMAT::R32F;
		case FIT_RGBF:							return PIXEL_FORMAT::RGB32F;
		case FIT_RGBAF:							return PIXEL_FORMAT::RGBA32F;
	}

	return PIXEL_FORMAT::UNKNOWN;
}

void	ImageFile::UseFreeImage() {
	if ( ms_freeImageUsageRefCount == 0 ) {
		FreeImage_Initialise( TRUE );
	}
	ms_freeImageUsageRefCount++;
}
void	ImageFile::UnUseFreeImage() {
	ms_freeImageUsageRefCount--;
	if ( ms_freeImageUsageRefCount == 0 ) {
		FreeImage_DeInitialise();
	}
}

//////////////////////////////////////////////////////////////////////////
// Graph Plotting Helpers
const U32	GRAPH_MARGIN = 10;	// 10 pixels margin

void	ImageFile::Clear( const bfloat4& _color ) {
	U32			W = Width();
	U32			H = Height();
	bfloat4*	tempScanline = new bfloat4[W];
	for ( U32 X=0; X < W; X++ )
		tempScanline[X] = _color;
	for ( U32 Y=0; Y < H; Y++ )
		WriteScanline( Y, tempScanline );
	delete[] tempScanline;
}

void	ImageFile::PlotGraph( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, PlotDelegate_t _delegate ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;
	float	Dx = (_rangeX.y - _rangeX.x) / (X1 - X0);
	float	DY = (Y1 - Y0) / (_rangeY.y - _rangeY.x);

	float	x = _rangeX.x;
	float	y = (*_delegate)( x );
	bfloat2	P1( float(X0), Y0 + (y - _rangeY.x) * DY );
	bfloat2	P0;
	for ( S32 X=X0+1; X < X1; X++ ) {
		P0 = P1;

		x += Dx;
		y = (*_delegate)( x );

		P1.x++;
		P1.y = Y0 + (y - _rangeY.x) * DY;

		DrawLine( _color, P0, P1 );
	}
}

void	ImageFile::PlotGraphAutoRangeY( const bfloat4& _color, const bfloat2& _rangeX, bfloat2& _rangeY, PlotDelegate_t _delegate ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;
	float	Dx = (_rangeX.y - _rangeX.x) / (X1 - X0);

	// Process values first to determine vertical range
	List< bfloat2 >	points( X1-X0 );
	float	x = _rangeX.x;
	_rangeY.Set( FLT_MAX, -FLT_MAX );
	for ( S32 X=X0; X < X1; X++, x+=Dx ) {
		bfloat2&	P = points.Append();
		P.x = float(X);
		P.y = (*_delegate)( x );

		_rangeY.x = MIN( _rangeY.x, P.y );
		_rangeY.y = MAX( _rangeY.y, P.y );
	}

	float	DY = (Y1 - Y0) / (_rangeY.y - _rangeY.x);

	// Draw actual graph
	U32		DX = X1-X0-1;
	points[0].y = Y0 + (points[0].y - _rangeY.x) * DY;
	for ( U32 X=0; X < DX; ) {
		bfloat2&	P0 = points[X++];
		bfloat2&	P1 = points[X];
		P1.y = Y0 + (P1.y - _rangeY.x) * DY;
		DrawLine( _color, P0, P1 );
	}
}

void	ImageFile::PlotLogGraph( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, PlotDelegate_t _delegate, float _logBaseX, float _logBaseY ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;

	bool	linearX = _logBaseX <= 1.0f;
	bool	linearY = _logBaseY <= 1.0f;

	float	Dx = (_rangeX.y - _rangeX.x) / (X1 - X0);
	float	DY = (Y1 - Y0) / (_rangeY.y - _rangeY.x);

 	float	logFactorY = linearY ? 1.0f : 1.0f / logf( _logBaseY );

	float	x = linearX ? _rangeX.x : powf( _logBaseX, _rangeX.x );
	float	y = (*_delegate)( x );
	if ( !linearY )
		y = logFactorY * logf( y );

	bfloat2	P1( float(X0), Y0 + (y - _rangeY.x) * DY );
	bfloat2	P0;
	U32		DX = X1-X0;
	for ( U32 X=1; X < DX; X++ ) {
		P0 = P1;

		x = linearX ? _rangeX.x + X * Dx : powf( _logBaseX, _rangeX.x + X * Dx );
		y = (*_delegate)( x );
		if ( !linearY )
			y = logFactorY * logf( y );

		P1.x++;
		P1.y = Y0 + (y - _rangeY.x) * DY;

		DrawLine( _color, P0, P1 );
	}
}

void	ImageFile::PlotLogGraphAutoRangeY( const bfloat4& _color, const bfloat2& _rangeX, bfloat2& _rangeY, PlotDelegate_t _delegate, float _logBaseX, float _logBaseY ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;

	bool	linearX = _logBaseX <= 1.0f;
	bool	linearY = _logBaseY <= 1.0f;

	float	Dx = (_rangeX.y - _rangeX.x) / (X1 - X0);

 	float	logFactorY = linearY ? 1.0f : 1.0f / logf( _logBaseY );

	// Process values first to determine vertical range
	U32		DX = X1-X0;

	List< bfloat2 >	points( DX );
	_rangeY.Set( FLT_MAX, -FLT_MAX );
	for ( U32 X=0; X < DX; X++ ) {
		float	x = linearX ? _rangeX.x + X * Dx : powf( _logBaseX, _rangeX.x + X * Dx );

		bfloat2&	P = points.Append();
		P.x = float(X0 + X);
		P.y = (*_delegate)( x );
		if ( !linearY )
			P.y = logFactorY * logf( P.y );

		_rangeY.x = MIN( _rangeY.x, P.y );
		_rangeY.y = MAX( _rangeY.y, P.y );
	}

	float	DY = (Y1 - Y0) / (_rangeY.y - _rangeY.x);

	// Draw actual graph
	points[0].y = Y0 + (points[0].y - _rangeY.x) * DY;
	for ( U32 X=0; X < DX-1; ) {
		bfloat2&	P0 = points[X++];
		bfloat2&	P1 = points[X];
		P1.y = Y0 + (P1.y - _rangeY.x) * DY;
		DrawLine( _color, P0, P1 );
	}
}

void	ImageFile::PlotAxes( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, float _stepX, float _stepY ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;
	float	DX = (X1 - X0) / (_rangeX.y - _rangeX.x);
	float	DY = (Y1 - Y0) / (_rangeY.y - _rangeY.x);

	// Draw main axes
	float	AxisX0 = X0 + (0.0f - _rangeX.x) * DX;
	float	AxisY0 = Y0 + (0.0f - _rangeY.x) * DY;
	DrawLine( _color, bfloat2( AxisX0, 0 ), bfloat2( AxisX0, (float) H-1 ) );
	DrawLine( _color, bfloat2( 0.0f, AxisY0 ), bfloat2( (float) W-1, AxisY0 ) );

	// Draw horizontal scale ticks
	{
		bfloat2	tick0( 0, AxisY0 );
		bfloat2	tick1( 0, AxisY0+4 );

		S32	tickStartX = S32( floorf( _rangeX.x / _stepX ) );
		S32	tickEndX = S32( ceilf( _rangeX.y / _stepX ) );
			tickEndX = tickStartX + MIN( 10000, tickEndX - tickStartX );	// Ensure no more than 10000 ticks
		for ( S32 tickIndex=tickStartX; tickIndex <= tickEndX; tickIndex++ ) {
			tick0.x = tick1.x = X0 + DX * (tickIndex * _stepX - _rangeX.x);
			DrawLine( _color, tick0, tick1 );
		}
	}

	// Draw vertical scale ticks
	{
		bfloat2	tick0( AxisX0-4, 0 );
		bfloat2	tick1( AxisX0, 0 );

		S32	tickStartY = S32( floorf( _rangeY.x / _stepY ) );
		S32	tickEndY = S32( ceilf( _rangeY.y / _stepY ) );
			tickEndY = tickStartY + MIN( 10000, tickEndY - tickStartY );	// Ensure no more than 10000 ticks
		for ( S32 tickIndex=tickStartY; tickIndex <= tickEndY; tickIndex++ ) {
			tick0.y = tick1.y = Y0 + DY * (tickIndex * _stepY - _rangeY.x);
			DrawLine( _color, tick0, tick1 );
		}
	}
}

void	ImageFile::PlotLogAxes( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, float _logBaseX, float _logBaseY ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;
	float	DX = (X1 - X0) / (_rangeX.y - _rangeX.x);
	float	DY = (Y1 - Y0) / (_rangeY.y - _rangeY.x);

	bool	linearX = _logBaseX <= 1.0f;
	bool	linearY = _logBaseY <= 1.0f;
	float	stepX = _logBaseX < 0.0f ? -_logBaseX : 1.0f;
	float	stepY = _logBaseY < 0.0f ? -_logBaseY : 1.0f;

	// Draw main axes
	float	AxisX0 = linearX ? X0 + (0.0f - _rangeX.x) * DX : X0;
	DrawLine( _color, bfloat2( AxisX0, 0 ), bfloat2( AxisX0, (float) H-1 ) );
	float	AxisY0 = linearY ? Y0 + (0.0f - _rangeY.x) * DY : Y0;
	DrawLine( _color, bfloat2( 0.0f, AxisY0 ), bfloat2( (float) W-1, AxisY0 ) );

	// Draw horizontal scale ticks
	{
		bfloat2	tick0( 0, AxisY0 );
		bfloat2	tick1( 0, AxisY0+4 );

		if ( linearX ) {
			S32	tickStartX = S32( floorf( _rangeX.x / stepX ) );
			S32	tickEndX = S32( ceilf( _rangeX.y / stepX ) );
				tickEndX = tickStartX + MIN( 10000, tickEndX - tickStartX );	// Ensure no more than 10000 ticks
			for ( S32 tickIndex=tickStartX; tickIndex <= tickEndX; tickIndex++ ) {
				tick0.x = tick1.x = X0 + DX * (tickIndex * stepX - _rangeX.x);
				DrawLine( _color, tick0, tick1 );
			}
		} else {
			// Log scale
			float	logFactor = 1.0f / logf( _logBaseX );

			S32		intervalStartY = S32( floorf( _rangeX.x ) );
			S32		intervalEndY = S32( ceilf( _rangeX.y ) );
			S32		stepsCount = S32( floorf( _logBaseX ) );
			for ( S32 intervalIndex=intervalStartY; intervalIndex <= intervalEndY; intervalIndex++ ) {
				float	v = powf( _logBaseX, float(intervalIndex) );

				// Draw one large graduation at the start of the interval
				float	x = logFactor * logf( v );
				tick0.x = tick1.x = X0 + DX * (x - _rangeX.x);
				tick1.y = AxisY0 + 6;
				DrawLine( _color, tick0, tick1 );

				// Draw a tiny graduation every 1/logBase step
				tick1.y = AxisY0 + 3;
				for ( int i=2; i < stepsCount; i++ ) {
					x = logFactor * logf( v * i );
					tick0.x = tick1.x = X0 + DX * (x - _rangeX.x);
					DrawLine( _color, tick0, tick1 );
				}
			}
		}
	}

	// Draw vertical scale ticks
	{
		bfloat2	tick0( AxisX0-4, 0 );
		bfloat2	tick1( AxisX0, 0 );

		if ( linearY ) {
			S32	tickStartY = S32( floorf( _rangeY.x / stepY ) );
			S32	tickEndY = S32( ceilf( _rangeY.y / stepY ) );
				tickEndY = tickStartY + MIN( 10000, tickEndY - tickStartY );	// Ensure no more than 10000 ticks
			for ( S32 tickIndex=tickStartY; tickIndex <= tickEndY; tickIndex++ ) {
				tick0.y = tick1.y = Y0 + DY * (tickIndex * stepY - _rangeY.x);
				DrawLine( _color, tick0, tick1 );
			}
		} else {
			// Log scale
			float	logFactor = 1.0f / logf( _logBaseY );

			S32		intervalStartY = S32( floorf( _rangeY.x ) );
			S32		intervalEndY = S32( ceilf( _rangeY.y ) );
			S32		stepsCount = S32( floorf( _logBaseY ) );
			for ( S32 intervalIndex=intervalStartY; intervalIndex <= intervalEndY; intervalIndex++ ) {
				float	v = powf( _logBaseY, float(intervalIndex) );

				// Draw one large graduation at the start of the interval
				float	y = logFactor * logf( v );
				tick0.y = tick1.y = Y0 + DY * (y - _rangeY.x);
				tick0.x = AxisX0 - 6;
				DrawLine( _color, tick0, tick1 );

				// Draw a tiny graduation every 1/10 step
				tick0.x = AxisX0 - 3;
				for ( int i=2; i < stepsCount; i++ ) {
					y = logFactor * logf( v * i );
					tick0.y = tick1.y = Y0 + DY * (y - _rangeY.x);
					DrawLine( _color, tick0, tick1 );
				}
			}
		}
	}
}

void	ImageFile::DrawLine( const bfloat4& _color, const bfloat2& _P0, const bfloat2& _P1 ) {
	float	W = float(Width());
	float	H = float(Height());

	bfloat2	P0 = _P0;
	bfloat2	P1 = _P1;
	if (	!ISVALID( P0.x ) || !ISVALID( P0.y )
		||	!ISVALID( P1.x ) || !ISVALID( P1.y ) ) {
//		ASSERT( false, "NaN or infinite values! Can't draw..." );
		return;
	}

	// Offset positions by half a pixel so the integer grid lies on pixel centers
	P0.x -= 0.5f;
	P0.y -= 0.5f;
	P1.x -= 0.5f;
	P1.y -= 0.5f;

	bfloat2	Delta = P1 - P0;
	bool	flipped = false;
	if ( fabs(Delta.x) < fabs(Delta.y) ) {
		//---------------------------------------------------------------
		// Vertical line: flip everything!
		Swap( P0.x, P0.y );
		Swap( P1.x, P1.y );
		Swap( Delta.x, Delta.y );
		Swap( W, H );
		flipped = true;
	}

	// Always order left to right
	if ( P0.x > P1.x ) {
		Swap( P0, P1 );
		Delta = -Delta;
	}

	if ( Delta.x < 1e-3f )
		return;	// Empty interval

	float	slope = Delta.y / Delta.x;
	float	recSlope = fabs(Delta.y) > 1e-8f ? Delta.x / Delta.y : 0.0f;

	// Perform clipping
	if ( P0.x < 0.0f ) {
		// Clip left
		float	clipDelta = P0.x;
		P0.y -= clipDelta * slope;
		P0.x = 0.0f;
	}
	if ( P1.x >= W ) {
		// Clip right
		float	clipDelta = W-1 - P1.x;
		P1.y += clipDelta * slope;
		P1.x = W-1;
	}
	if ( slope >= 0.0f ) {
		// Drawing from top to bottom
		if ( P1.y < 0.0f || P0.y >= H-1 )
			return;	// Entirely out of screen
		if ( P0.y < 0.0f ) {
			// Clip top
			float	clipDelta = P0.y;
			P0.x -= clipDelta * recSlope;
			P0.y = 0.0f;
		}
		if ( P1.y > H-1 ) {
			// Clip bottom
			float	clipDelta = H-1 - P1.y;
			P1.x += clipDelta * recSlope;
			P1.y = H-1;
		}
	} else {
		// Drawing from bottom to top
		if ( P0.y < 0.0f || P1.y >= H-1 )
			return;	// Entirely out of screen
		if ( P1.y < 0.0f ) {
			// Clip top
			float	clipDelta = P1.y;
			P1.x -= clipDelta * recSlope;
			P1.y = 0.0f;
		}
		if ( P0.y > H-1 ) {
			// Clip bottom
			float	clipDelta = H-1 - P0.y;
			P0.x += clipDelta * recSlope;
			P0.y = H-1;
		}
	}
	if ( P1.x - P0.x < 1e-3f )
		return;	// Empty interval

// This fails sometimes but the slope is very similar anyway!
// #if _DEBUG
// // Make sure we didn't alter the slope!
// float	newSlope = (P1.y - P0.y) / (P1.x - P0.x);
// ASSERT( fabs( newSlope - slope ) < 1e-4f, "Slope differs after clipping!" );
// #endif

	// At this point we only have positions within the ranges X€[0,W[ and Y€[0,H[
	int		X0 = int( floorf( P0.x + 0.5f ) );	// Lies on start pixel center
	int		X1 = int( floorf( P1.x + 0.5f ) );	// Lies on end pixel center

	P0.y += 0.5f - (P0.x - X0) * slope;	// First step: go back to the start pixel's X center

	// Draw
	if ( flipped ) {
		// Draw flipped vertical line
		for ( ; X0 <= X1; X0++, P0.y+=slope ) {
			int	Y = int( floorf( P0.y ) );
			ASSERT( Y >= 0 && Y < H, "Offscreen! Check clipping!" );
			Set( Y, X0, _color );
		}
	} else {
		// Draw regular horizontal line
		for ( ; X0 <= X1; X0++, P0.y+=slope ) {
			int	Y = int( floorf( P0.y ) );
			ASSERT( Y >= 0 && Y < H, "Offscreen! Check clipping!" );
			Set( X0, Y, _color );
		}
	}
}


//////////////////////////////////////////////////////////////////////////
// DDS-Related Helpers
//

// Compresses a single image
void	ImageFile::DDSCompress( COMPRESSION_TYPE _compressionType, U32& _compressedImageSize, void*& _compressedImage ) {
//	Implement meeeee!
}

// Saves a DDS image in memory to disk (usually used after a compression)
void	ImageFile::DDSSaveFromMemory( U32 _DDSImageSize, const void* _DDSImage, const wchar_t* _fileName ) {

}

// Cube map handling
void	ImageFile::DDSLoadCubeMapFile( const wchar_t* _fileName, U32& _cubeMapsCount, ImageFile*& _cubeMapFaces ) {

}
void	ImageFile::DDSLoadCubeMapMemory( U64 _fileSize, void* _fileContent, U32& _cubeMapsCount, ImageFile*& _cubeMapFaces ) {

}
void	ImageFile::DDSSaveCubeMapFile( U32 _cubeMapsCount, const ImageFile** _cubeMapFaces, bool _compressBC6H, const wchar_t* _fileName ) {

}
void	ImageFile::DDSSaveCubeMapMemory( U32 _cubeMapsCount, const ImageFile** _cubeMapFaces, bool _compressBC6H, U64 _fileSize, const void* _fileContent ) {

}

// 3D Texture handling
void	ImageFile::DDSLoad3DTextureFile( const wchar_t* _fileName, U32& _slicesCount, ImageFile*& _slices ) {

}
void	ImageFile::DDSLoad3DTextureMemory( U64 _fileSize, void* _fileContent, U32& _slicesCount, ImageFile*& _slices ) {

}
void	ImageFile::DDSSave3DTextureFile( U32 _slicesCount, const ImageFile** _slices, bool _compressBC6H, const wchar_t* _fileName ) {

}
void	ImageFile::DDSSave3DTextureMemory( U32 _slicesCount, const ImageFile** _slices, bool _compressBC6H, U64 _fileSize, const void* _fileContent ) {

}

//////////////////////////////////////////////////////////////////////////
#pragma region Old code...
// 		// Formatting flags for the Save() method
// 		enum class FORMAT_FLAGS {
// 			NONE = 0,
// 
// 			// Bits per pixel component
// 			SAVE_8BITS_UNORM = 0,	// Save as byte
// 			SAVE_16BITS_UNORM = 1,	// Save as UInt16 if possible (valid for PNG, TIFF)
// 			SAVE_32BITS_FLOAT = 2,	// Save as float if possible (valid for TIFF)
// 
// 			// Gray
// 			GRAY = 4,				// Save as gray levels
// 
// 			SKIP_ALPHA = 8,			// Don't save alpha
// 			PREMULTIPLY_ALPHA = 16,	// RGB should be multiplied by alpha
// 		};
//
// Save to a stream
// <param name="_Stream">The stream to write the image to</param>
// <param name="_FileType">The file type to save as</param>
// <param name="_Parms">Additional formatting flags</param>
// <param name="_options">An optional block of options for encoding</param>
// <exception cref="NotSupportedException">Occurs if the image type is not supported by the Bitmap class</exception>
// <exception cref="Exception">Occurs if the source image format cannot be converted to RGBA32F which is the generic format we read from</exception>
//		void	Save( System.IO.Stream _Stream, FILE_FORMAT _FileType, FORMAT_FLAGS _Parms, const FormatEncoderOptions* _options ) const;
//
// <summary>
// Save to a stream
// </summary>
// <param name="_Stream">The stream to write the image to</param>
// <param name="_FileType">The file type to save as</param>
// <param name="_Parms">Additional formatting flags</param>
// <param name="_options">An optional block of options for encoding</param>
// <exception cref="NotSupportedException">Occurs if the image type is not supported by the Bitmap class</exception>
// <exception cref="Exception">Occurs if the source image format cannot be converted to RGBA32F which is the generic format we read from</exception>
// void	ImageFile::Save( System.IO.Stream _Stream, FILE_TYPE _FileType, FORMAT_FLAGS _Parms, const FormatEncoderOptions* _options ) const {
// 	if ( m_colorProfile == null )
// 		throw new Exception( "You can't save the bitmap if you don't provide a valid color profile!" );
// 
// 	try
// 	{
// 		switch ( _FileType )
// 		{
// 			case FILE_TYPE.JPEG:
// 			case FILE_TYPE.PNG:
// 			case FILE_TYPE.TIFF:
// 			case FILE_TYPE.GIF:
// 			case FILE_TYPE.BMP:
// 				{
// 					BitmapEncoder	Encoder = null;
// 					switch ( _FileType )
// 					{
// 						case FILE_TYPE.JPEG:	Encoder = new JpegBitmapEncoder(); break;
// 						case FILE_TYPE.PNG:		Encoder = new PngBitmapEncoder(); break;
// 						case FILE_TYPE.TIFF:	Encoder = new TiffBitmapEncoder(); break;
// 						case FILE_TYPE.GIF:		Encoder = new GifBitmapEncoder(); break;
// 						case FILE_TYPE.BMP:		Encoder = new BmpBitmapEncoder(); break;
// 					}
// 
// 					if ( _options != null )
// 					{
// 						switch ( _FileType )
// 						{
// 							case FILE_TYPE.JPEG:
// 								(Encoder as JpegBitmapEncoder).QualityLevel = _options.JPEGQualityLevel;
// 								break;
// 
// 							case FILE_TYPE.PNG:
// 								(Encoder as PngBitmapEncoder).Interlace = _options.PNGInterlace;
// 								break;
// 
// 							case FILE_TYPE.TIFF:
// 								(Encoder as TiffBitmapEncoder).Compression = _options.TIFFCompression;
// 								break;
// 
// 							case FILE_TYPE.GIF:
// 								break;
// 
// 							case FILE_TYPE.BMP:
// 								break;
// 						}
// 					}
// 
// 
// 					// Find the appropriate pixel format
// 					int		BitsPerComponent = 8;
// 					bool	IsFloat = false;
// 					if ( (_Parms & FORMAT_FLAGS.SAVE_16BITS_UNORM) != 0 )
// 						BitsPerComponent = 16;
// 					if ( (_Parms & FORMAT_FLAGS.SAVE_32BITS_FLOAT) != 0 )
// 					{	// Floating-point format
// 						BitsPerComponent = 32;
// 						IsFloat = true;
// 					}
// 
// 					int		ComponentsCount = (_Parms & FORMAT_FLAGS.GRAY) == 0 ? 3 : 1;
// 					if ( m_hasAlpha && (_Parms & FORMAT_FLAGS.SKIP_ALPHA) == 0 )
// 						ComponentsCount++;
// 
// 					bool	PreMultiplyAlpha = (_Parms & FORMAT_FLAGS.PREMULTIPLY_ALPHA) != 0;
// 
// 					System.Windows.Media.PixelFormat	Format;
// 					if ( ComponentsCount == 1 )
// 					{	// Gray
// 						switch ( BitsPerComponent )
// 						{
// 							case 8:		Format = System.Windows.Media.PixelFormats.Gray8; break;
// 							case 16:	Format = System.Windows.Media.PixelFormats.Gray16; break;
// 							case 32:	Format = System.Windows.Media.PixelFormats.Gray32Float; break;
// 							default:	throw new Exception( "Unsupported format!" );
// 						}
// 					}
// 					else if ( ComponentsCount == 3 )
// 					{	// RGB
// 						switch ( BitsPerComponent )
// 						{
// 							case 8:		Format = System.Windows.Media.PixelFormats.Bgr24; break;
// 							case 16:	Format = System.Windows.Media.PixelFormats.Rgb48; break;
// 							case 32:	throw new Exception( "32BITS formats aren't supported without ALPHA!" );
// 							default:	throw new Exception( "Unsupported format!" );
// 						}
// 					}
// 					else
// 					{	// RGBA
// 						switch ( BitsPerComponent )
// 						{
// 							case 8:		Format = PreMultiplyAlpha ? System.Windows.Media.PixelFormats.Pbgra32 : System.Windows.Media.PixelFormats.Bgra32; break;
// 							case 16:	Format = PreMultiplyAlpha ? System.Windows.Media.PixelFormats.Prgba64 : System.Windows.Media.PixelFormats.Rgba64; break;
// 							case 32:	Format = PreMultiplyAlpha ? System.Windows.Media.PixelFormats.Prgba128Float : System.Windows.Media.PixelFormats.Rgba128Float;
// 								if ( !IsFloat ) throw new Exception( "32BITS_UNORM format isn't supported if not floating-point!" );
// 								break;
// 							default:	throw new Exception( "Unsupported format!" );
// 						}
// 					}
// 
// 					// Convert into appropriate frame
// 					BitmapFrame	Frame = ConvertFrame( Format );
// 					Encoder.Frames.Add( Frame );
// 
// 					// Save
// 					Encoder.Save( _Stream );
// 				}
// 				break;
// 
// //					case FILE_TYPE.TGA:
// //TODO!
// // 						{
// // 							// Load as a System.Drawing.Bitmap and convert to float4
// // 							using ( System.IO.MemoryStream Stream = new System.IO.MemoryStream( _ImageFileContent ) )
// // 								using ( TargaImage TGA = new TargaImage( Stream ) )
// // 								{
// // 									// Create a default sRGB linear color profile
// // 									m_ColorProfile = new ColorProfile(
// // 											ColorProfile.Chromaticities.sRGB,	// Use default sRGB color profile
// // 											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
// // 											TGA.ExtensionArea.GammaRatio		// ...whose gamma is retrieved from extension data
// // 										);
// // 
// // 									// Convert
// // 									byte[]	ImageContent = LoadBitmap( TGA.Image, out m_Width, out m_Height );
// // 									m_Bitmap = new float4[m_Width,m_Height];
// // 									byte	A;
// // 									int		i = 0;
// // 									for ( int Y=0; Y < m_Height; Y++ )
// // 										for ( int X=0; X < m_Width; X++ )
// // 										{
// // 											m_Bitmap[X,Y].x = BYTE_TO_FLOAT * ImageContent[i++];
// // 											m_Bitmap[X,Y].y = BYTE_TO_FLOAT * ImageContent[i++];
// // 											m_Bitmap[X,Y].z = BYTE_TO_FLOAT * ImageContent[i++];
// // 
// // 											A = ImageContent[i++];
// // 											m_bHasAlpha |= A != 0xFF;
// // 
// // 											m_Bitmap[X,Y].w = BYTE_TO_FLOAT * A;
// // 										}
// // 
// // 									// Convert to CIEXYZ
// // 									m_ColorProfile.RGB2XYZ( m_Bitmap );
// // 								}
// // 							return;
// // 						}
// 
// //					case FILE_TYPE.HDR:
// //TODO!
// // 						{
// // 							// Load as XYZ
// // 							m_Bitmap = LoadAndDecodeHDRFormat( _ImageFileContent, true, out m_ColorProfile );
// // 							m_Width = m_Bitmap.GetLength( 0 );
// // 							m_Height = m_Bitmap.GetLength( 1 );
// // 							return;
// // 						}
// 
// 			case FILE_TYPE.CRW:
// 			case FILE_TYPE.CR2:
// 			case FILE_TYPE.DNG:
// 			default:
// 				throw new NotSupportedException( "The image file type \"" + _FileType + "\" is not supported by the Bitmap class!" );
// 		}
// 	}
// 	catch ( Exception )
// 	{
// 		throw;	// Go on !
// 	}
// 	finally
// 	{
// 	}
// }
#pragma endregion
