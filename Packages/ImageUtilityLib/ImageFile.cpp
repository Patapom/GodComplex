#include "stdafx.h"

#include "ImageFile.h"
#include "Bitmap.h"
#include "ImagesMatrix.h"

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
	case PIXEL_FORMAT::BGRA8:
//	case PIXEL_FORMAT::RGBA8:	// Shouldn't have to ask as it's not a format natively supported by FreeImage!
	case PIXEL_FORMAT::RGBA16:
	case PIXEL_FORMAT::RGBA16F:
	case PIXEL_FORMAT::RGBA32F:
		return true;
	}
	return false;
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
	m_pixelAccessor = &BaseLib::PixelFormat2PixelAccessor( _format );

	FREE_IMAGE_TYPE	bitmapType = PixelFormat2FIT( _format );
	int				BPP = int( PixelFormat2BPP( _format ) );
	m_bitmap = FreeImage_AllocateT( bitmapType, _width, _height, BPP );
	if ( m_bitmap == nullptr )
		throw "Failed to initialize image file!";

	// Assign color profile
	SetColorProfile( _colorProfile );
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
	FILE_FORMAT	format = GetFileTypeFromExistingFileContent( _fileName );
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
	m_pixelAccessor = &PixelFormat2PixelAccessor( m_pixelFormat );

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
	m_pixelAccessor = &PixelFormat2PixelAccessor( m_pixelFormat );

	m_metadata.RetrieveFromImage( *this );
}

//////////////////////////////////////////////////////////////////////////
// Save
void	ImageFile::Save( const wchar_t* _fileName ) const {
	FILE_FORMAT	format = GetFileTypeFromFileNameOnly( _fileName );
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
	ms_lastDumpedText[0] = '\0';
	if ( !FreeImage_SaveU( FileFormat2FIF( _format ), m_bitmap, _fileName, int(_options) ) ) {
		if ( ms_lastDumpedText[0] != '\0' )
			throw ms_lastDumpedText;
		throw "Failed to save the image file!";
	}

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back after saving
	FreeImage_FlipVertical( m_bitmap );
}
void	ImageFile::Save( FILE_FORMAT _format, SAVE_FLAGS _options, U64& _fileSize, void*& _fileContent ) const {
	if ( _format == FILE_FORMAT::UNKNOWN )
		throw "Unrecognized image file format!";
	if ( m_bitmap == nullptr )
		throw "Invalid bitmap to save!";

	m_fileFormat = _format;

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back before saving
	FreeImage_FlipVertical( m_bitmap );

	// Save into a stream of unknown size
	FIMEMORY*	stream = FreeImage_OpenMemory();
	if ( !FreeImage_SaveToMemory( FileFormat2FIF( _format ), m_bitmap, stream, int(_options) ) ) {
		if ( ms_lastDumpedText[0] != '\0' )
			throw ms_lastDumpedText;
		throw "Failed to save the image file!";
	}

	// Apparently, FreeImage **always** flips the images vertically so we need to flip them back before saving
	FreeImage_FlipVertical( m_bitmap );

	// Copy into a custom buffer
	_fileSize = FreeImage_TellMemory( stream );
	_fileContent = new U8[_fileSize];
	FIMEMORY*	target = FreeImage_OpenMemory( (BYTE*) _fileContent, U32(_fileSize) );

	FreeImage_SeekMemory( stream, 0, SEEK_SET );
	FreeImage_ReadMemory( _fileContent, 1, U32( _fileSize ), stream );

	FreeImage_CloseMemory( target );
	FreeImage_CloseMemory( stream );
}

//////////////////////////////////////////////////////////////////////////
// Conversion
void	ImageFile::ConvertFrom( const ImageFile& _source, PIXEL_FORMAT _targetFormat ) {
	Exit();

	// Ensure we're not dealing with unsupported types!
	if (	(U32(_source.m_pixelFormat) & U32(PIXEL_FORMAT::NO_FREEIMAGE_SUPPORT))
		 || (U32(_targetFormat) & U32(PIXEL_FORMAT::NO_FREEIMAGE_SUPPORT)) ) {
//		throw "Unsupported source or target type!";
		ConvertFrom_NoSupport( _source, _targetFormat );
	} else {
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
					case 24: m_bitmap = FreeImage_ConvertTo24Bits( temp ); break;
					case 32: m_bitmap = FreeImage_ConvertTo32Bits( temp ); break;
					}
					break;

				case 16:
					switch ( targetBPP ) {
					case 8: m_bitmap = FreeImage_ConvertTo8Bits( temp ); break;
					case 24: m_bitmap = FreeImage_ConvertTo24Bits( temp ); break;
					case 32: m_bitmap = FreeImage_ConvertTo32Bits( temp ); break;
					}
					break;

				case 24:
					switch ( targetBPP ) {
					case 8: m_bitmap = FreeImage_ConvertTo8Bits( temp ); break;
					case 16: throw "24 -> 16 bits per pixel is not a supported conversion!";
					case 32: m_bitmap = FreeImage_ConvertTo32Bits( temp ); break;
					}
					break;

				case 32:
					switch ( targetBPP ) {
					case 8: m_bitmap = FreeImage_ConvertTo8Bits( temp ); break;
					case 16: throw "32 -> 16 bits per pixel is not a supported conversion!";
					case 24: m_bitmap = FreeImage_ConvertTo24Bits( temp ); break;
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
			if ( m_bitmap == NULL ) {
//				throw "Failed to convert!";
				ConvertFrom_NoSupport( _source, _targetFormat );
			}
		}

		// Get pixel format from bitmap
		m_pixelFormat = Bitmap2PixelFormat( *m_bitmap );
	}

	m_pixelAccessor = &PixelFormat2PixelAccessor( m_pixelFormat );

	// Copy metadata
	m_metadata = _source.m_metadata;

	// Copy file format
	m_fileFormat = _source.m_fileFormat;
}

void	ImageFile::ConvertFrom_NoSupport( const ImageFile& _source, PIXEL_FORMAT _targetFormat ) {
	m_pixelFormat = _targetFormat;

	U32	W = _source.Width();
	U32	H = _source.Height();

	// Create target bitmap
	FREE_IMAGE_TYPE	bitmapType = PixelFormat2FIT( m_pixelFormat );
	int				BPP = int( PixelFormat2BPP( m_pixelFormat ) );
	m_bitmap = FreeImage_AllocateT( bitmapType, W, H, BPP );

	const IPixelAccessor&	sourceAccessor = PixelFormat2PixelAccessor( _source.m_pixelFormat );
	const IPixelAccessor&	targetAccessor = PixelFormat2PixelAccessor( m_pixelFormat );

	const U8*	sourceBits = _source.GetBits();
	U8*			targetBits = GetBits();
	U32			sourcePixelSize = sourceAccessor.Size();
	U32			sourcePitch = _source.Pitch();
	U32			targetPixelSize = targetAccessor.Size();
	U32			targetPitch = Pitch();

	bfloat4	temp;
	for ( U32 Y=0; Y < H; Y++ ) {
		const U8*	scanlineSource = sourceBits + Y * sourcePitch;
		U8*			scanlineTarget = targetBits + Y * targetPitch;
		for ( U32 X=0; X < W; X++, scanlineSource += sourcePixelSize, scanlineTarget += targetPixelSize ) {
			sourceAccessor.RGBA( scanlineSource, temp );
			targetAccessor.Write( scanlineTarget, temp );
		}
	}
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
	if (	_source.m_pixelFormat == PIXEL_FORMAT::R16F
		 || _source.m_pixelFormat == PIXEL_FORMAT::R32F ) {
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
	} else if (	_source.m_pixelFormat == PIXEL_FORMAT::RG16F
			 || _source.m_pixelFormat == PIXEL_FORMAT::RG32F ) {
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
	} else if (	_source.m_pixelFormat == PIXEL_FORMAT::RGB16F
			 || _source.m_pixelFormat == PIXEL_FORMAT::RGB32F ) {
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
	} else if (	_source.m_pixelFormat == PIXEL_FORMAT::RGBA16F
			 || _source.m_pixelFormat == PIXEL_FORMAT::RGBA32F ) {
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
	m_pixelAccessor = &PixelFormat2PixelAccessor( m_pixelFormat );

	// Copy metadata
	m_metadata = _source.m_metadata;

	// Copy file format
	m_fileFormat = _source.m_fileFormat;
}

void	ImageFile::CopySource( const ImageFile& _source, U32 _offsetX, U32 _offsetY ) {
	U32			sourceWidth = _source.Width();
	U32			sourceHeight = _source.Height();
	U32			targetWidth = Width();
	U32			targetHeight = Height();

	// Clip source rectangle with offset to target dimensions
	U32			right = MIN( targetWidth, _offsetX + sourceWidth );
	U32			W = right - _offsetX;
	U32			bottom = MIN( targetHeight, _offsetY + sourceHeight );
	U32			H = bottom - _offsetY;

	// Copy each scanline
	bfloat4*	sourceScanline = new bfloat4[sourceWidth];

	for ( U32 Y=0; Y < H; Y++ ) {
		_source.ReadScanline( Y, sourceScanline );
		WriteScanline( _offsetY + Y, sourceScanline, _offsetX, W );
	}

	SAFE_DELETE_ARRAY( sourceScanline );
}

void	ImageFile::RescaleSource( const ImageFile& _source ) {
	U32			sourceWidth = _source.Width();
	U32			sourceHeight = _source.Height();
	U32			targetWidth = Width();
	U32			targetHeight = Height();
	float		horizontalScale = (float) sourceWidth / targetWidth;

	// Read "height" scanlines
	bfloat4*	sourceScanline = new bfloat4[sourceWidth];
	bfloat4*	targetScanline = new bfloat4[targetWidth];

	for ( U32 Y=0; Y < targetHeight; Y++ ) {
		U32	sourceY = U32( sourceHeight * Y / targetHeight );
		_source.ReadScanline( sourceY, sourceScanline );
		for ( U32 X=0; X < targetWidth; X++ ) {
			targetScanline[X] = sourceScanline[U32( horizontalScale * (X+0.5f) )];
		}
		WriteScanline( Y, targetScanline );
	}

	SAFE_DELETE_ARRAY( targetScanline );
	SAFE_DELETE_ARRAY( sourceScanline );
}

void	U8toS8( U8*& _scanline ) {
	S16	signedValue = S16( *_scanline );
	S8	temp = S8( signedValue - 128 );
	*_scanline++ = temp;
}
void	U16toS16( U8*& _scanline ) {
	S16*	scanline = (S16*) _scanline;
	S32		signedValue = S32( *((U16*) scanline) );
	S16		temp = S16( signedValue - 32768 );
	*scanline++ = temp;
	_scanline = (U8*) scanline;
}
void	U32toS32( U8*& _scanline ) {
	S32*	scanline = (S32*) _scanline;
	S64		signedValue = S64( *((U32*) scanline) );
	S32		temp = S32( signedValue - 2147483648 );
	*scanline++ = temp;
	_scanline = (U8*) scanline;
}

void	ImageFile::MakeSigned() {
	U32	W = Width();
	U32	H = Height();
	U32	pixelSize = m_pixelAccessor->Size();

	U32	pitch  = FreeImage_GetPitch( m_bitmap );
	U8*	bits = (BYTE*) FreeImage_GetBits( m_bitmap );

	for ( U32 Y=0; Y < H; Y++ ) {
		U8*	scanline = bits + Y * pitch;
		switch ( m_pixelFormat ) {
			// 8-Bits Formats
			case PIXEL_FORMAT::R8:
				for ( U32 X=0; X < W; X++ ) {
					U8toS8( scanline );
				}
				break;
			case PIXEL_FORMAT::RG8:
				for ( U32 X=0; X < W; X++ ) {
					U8toS8( scanline );
					U8toS8( scanline );
				}
				break;
			case PIXEL_FORMAT::RGB8:
			case PIXEL_FORMAT::BGR8:
				for ( U32 X=0; X < W; X++ ) {
					U8toS8( scanline );
					U8toS8( scanline );
					U8toS8( scanline );
				}
				break;
			case PIXEL_FORMAT::RGBA8:
			case PIXEL_FORMAT::BGRA8:
				for ( U32 X=0; X < W; X++ ) {
					U8toS8( scanline );
					U8toS8( scanline );
					U8toS8( scanline );
					scanline++;
				}
				break;

			// 16-Bits Formats
			case PIXEL_FORMAT::R16:
				for ( U32 X=0; X < W; X++ ) {
					U16toS16( scanline );
				}
				break;
			case PIXEL_FORMAT::RG16:
				for ( U32 X=0; X < W; X++ ) {
					U16toS16( scanline );
					U16toS16( scanline );
				}
				break;
			case PIXEL_FORMAT::RGB16:
				for ( U32 X=0; X < W; X++ ) {
					U16toS16( scanline );
					U16toS16( scanline );
					U16toS16( scanline );
				}
				break;
			case PIXEL_FORMAT::RGBA16:
				for ( U32 X=0; X < W; X++ ) {
					U16toS16( scanline );
					U16toS16( scanline );
					U16toS16( scanline );
					scanline+=2;
				}
				break;

			// 32-Bits Formats
			case PIXEL_FORMAT::R32:
				for ( U32 X=0; X < W; X++ ) {
					U32toS32( scanline );
				}
				break;
			case PIXEL_FORMAT::RG32:
				for ( U32 X=0; X < W; X++ ) {
					U32toS32( scanline );
					U32toS32( scanline );
				}
				break;
			case PIXEL_FORMAT::RGB32:
				for ( U32 X=0; X < W; X++ ) {
					U32toS32( scanline );
					U32toS32( scanline );
					U32toS32( scanline );
				}
				break;
			case PIXEL_FORMAT::RGBA32:
				for ( U32 X=0; X < W; X++ ) {
					U32toS32( scanline );
					U32toS32( scanline );
					U32toS32( scanline );
					scanline+=4;
				}
				break;

		default:
			throw "Not supported!";
		}
	}
}

void	S8toU8( U8*& _scanline ) {
	S16	signedValue = S16( *((S8*) _scanline) );
	U8	temp = U8( signedValue + 128 );
	*_scanline++ = temp;
}
void	S16toU16( U8*& _scanline ) {
	U16*	scanline = (U16*) _scanline;
	S32		signedValue = S32( *((S16*) _scanline) );
	U16		temp = U16( signedValue + 32768 );
	*scanline++ = temp;
	_scanline = (U8*) scanline;
}
void	S32toU32( U8*& _scanline ) {
	U32*	scanline = (U32*) _scanline;
	S64		signedValue = S64( *((S32*) _scanline) );
	S32		temp = S32( signedValue + 2147483648 );
	*scanline++ = temp;
	_scanline = (U8*) scanline;
}

void	ImageFile::MakeUnSigned() {
	U32	W = Width();
	U32	H = Height();
	U32	pixelSize = m_pixelAccessor->Size();

	U32	pitch  = FreeImage_GetPitch( m_bitmap );
	U8*	bits = (BYTE*) FreeImage_GetBits( m_bitmap );

	for ( U32 Y=0; Y < H; Y++ ) {
		U8*	scanline = bits + Y * pitch;
		switch ( m_pixelFormat ) {
			// 8-Bits Formats
			case PIXEL_FORMAT::R8:
				for ( U32 X=0; X < W; X++ ) {
					S8toU8( scanline );
				}
				break;
			case PIXEL_FORMAT::RG8:
				for ( U32 X=0; X < W; X++ ) {
					S8toU8( scanline );
					S8toU8( scanline );
				}
				break;
			case PIXEL_FORMAT::RGB8:
			case PIXEL_FORMAT::BGR8:
				for ( U32 X=0; X < W; X++ ) {
					S8toU8( scanline );
					S8toU8( scanline );
					S8toU8( scanline );
				}
				break;
			case PIXEL_FORMAT::RGBA8:
			case PIXEL_FORMAT::BGRA8:
				for ( U32 X=0; X < W; X++ ) {
					S8toU8( scanline );
					S8toU8( scanline );
					S8toU8( scanline );
					scanline++;
				}
				break;

			// 16-Bits Formats
			case PIXEL_FORMAT::R16:
				for ( U32 X=0; X < W; X++ ) {
					S16toU16( scanline );
				}
				break;
			case PIXEL_FORMAT::RG16:
				for ( U32 X=0; X < W; X++ ) {
					S16toU16( scanline );
					S16toU16( scanline );
				}
				break;
			case PIXEL_FORMAT::RGB16:
				for ( U32 X=0; X < W; X++ ) {
					S16toU16( scanline );
					S16toU16( scanline );
					S16toU16( scanline );
				}
				break;
			case PIXEL_FORMAT::RGBA16:
				for ( U32 X=0; X < W; X++ ) {
					S16toU16( scanline );
					S16toU16( scanline );
					S16toU16( scanline );
					scanline+=2;
				}
				break;

			// 32-Bits Formats
			case PIXEL_FORMAT::R32:
				for ( U32 X=0; X < W; X++ ) {
					S32toU32( scanline );
				}
				break;
			case PIXEL_FORMAT::RG32:
				for ( U32 X=0; X < W; X++ ) {
					S32toU32( scanline );
					S32toU32( scanline );
				}
				break;
			case PIXEL_FORMAT::RGB32:
				for ( U32 X=0; X < W; X++ ) {
					S32toU32( scanline );
					S32toU32( scanline );
					S32toU32( scanline );
				}
				break;
			case PIXEL_FORMAT::RGBA32:
				for ( U32 X=0; X < W; X++ ) {
					S32toU32( scanline );
					S32toU32( scanline );
					S32toU32( scanline );
					scanline+=4;
				}
				break;

		default:
			throw "Not supported!";
		}
	}
}

void	ImageFile::ReadScanline( U32 _Y, bfloat4* _color, U32 _startX, U32 _count ) const {
	U32	W = Width();
	U32	pixelSize = m_pixelAccessor->Size();

	const U32	pitch  = FreeImage_GetPitch( m_bitmap );
	const U8*	bits = (BYTE*) FreeImage_GetBits( m_bitmap );
	bits += pitch * _Y + _startX * pixelSize;

	_count = MIN( _count, W-_startX );
	for ( U32 i=_count; i > 0; i--, bits += pixelSize, _color++ ) {
		m_pixelAccessor->RGBA( bits, *_color );
	}
}
void	ImageFile::WriteScanline( U32 _Y, const bfloat4* _color, U32 _startX, U32 _count ) {
	U32	W = Width();
	U32	pixelSize = m_pixelAccessor->Size();

	U32	pitch  = FreeImage_GetPitch( m_bitmap );
	U8*	bits = (BYTE*) FreeImage_GetBits( m_bitmap );
		bits += pitch * _Y + _startX * pixelSize;

	_count = MIN( _count, W-_startX );
	for ( U32 i=_count; i > 0; i--, bits += pixelSize, _color++ ) {
		m_pixelAccessor->Write( bits, *_color );
	}
}

void	ImageFile::ReadPixels( pixelReaderWriter_t _reader, U32 _startX, U32 _startY, U32 _width, U32 _height ) const {
	if ( _width == ~0U )
		_width = Width();
	if ( _height == ~0U )
		_height = Height();

	bfloat4*	tempScanline = new bfloat4[_width];
	for ( U32 Y=0; Y < _height; Y++ ) {
		ReadScanline( _startY+Y, tempScanline, _startX, _width );
		for ( U32 X=0; X < _width; X++ ) {
			(*_reader)( _startX+X, _startY+Y, tempScanline[X] );
		}
	}
	delete[] tempScanline;
}

void	ImageFile::WritePixels( pixelReaderWriter_t _writer, U32 _startX, U32 _startY, U32 _width, U32 _height ) {
	if ( _width == ~0U )
		_width = Width();
	if ( _height == ~0U )
		_height = Height();

	bfloat4*	tempScanline = new bfloat4[_width];
	for ( U32 Y=0; Y < _height; Y++ ) {
		for ( U32 X=0; X < _width; X++ ) {
			(*_writer)( _startX+X, _startY+Y, tempScanline[X] );
		}
		WriteScanline( _startY+Y, tempScanline, _startX, _width );
	}
	delete[] tempScanline;
}

void	ImageFile::ReadWritePixels( pixelReaderWriter_t _writer, U32 _startX, U32 _startY, U32 _width, U32 _height ) {
	if ( _width == ~0U )
		_width = Width();
	if ( _height == ~0U )
		_height = Height();

	bfloat4*	tempScanline = new bfloat4[_width];
	for ( U32 Y=0; Y < _height; Y++ ) {
		ReadScanline( _startY+Y, tempScanline, _startX, _width );
		for ( U32 X=0; X < _width; X++ ) {
			(*_writer)( _startX+X, _startY+Y, tempScanline[X] );
		}
		WriteScanline( _startY+Y, tempScanline, _startX, _width );
	}
	delete[] tempScanline;
}


//////////////////////////////////////////////////////////////////////////
// Helpers
ImageFile::FILE_FORMAT	ImageFile::GetFileTypeFromExistingFileContent( const wchar_t* _imageFileNameName ) {
	if ( _imageFileNameName == nullptr )
		return FILE_FORMAT::UNKNOWN;

	FILE_FORMAT	result = FIF2FileFormat( FreeImage_GetFileTypeU( _imageFileNameName, 0 ) );
	return result;
}

ImageFile::FILE_FORMAT	ImageFile::GetFileTypeFromFileNameOnly( const wchar_t* _imageFileNameName ) {
	if ( _imageFileNameName == nullptr )
		return FILE_FORMAT::UNKNOWN;

	// Search for last . occurrence
	size_t	length = wcslen( _imageFileNameName );
	size_t	extensionIndex;
	for ( extensionIndex=length-1; extensionIndex >= 0; extensionIndex-- ) {
		if ( _imageFileNameName[extensionIndex] == '.' )
			break;
	}
	if ( extensionIndex == 0 && _imageFileNameName[extensionIndex] != '.' )
		return FILE_FORMAT::UNKNOWN;

	const wchar_t*	extension = _imageFileNameName + extensionIndex;

	// Check for known extensions
	struct KnownExtension {
		const wchar_t*	extension;
		FILE_FORMAT	format;
	}	knownExtensions[] = {
		{ L".PNG",	FILE_FORMAT::PNG },
		{ L".JPG",	FILE_FORMAT::JPEG },
		{ L".JPEG",	FILE_FORMAT::JPEG },
		{ L".JPE",	FILE_FORMAT::JPEG },
		{ L".TGA",	FILE_FORMAT::TARGA },
		{ L".DDS",	FILE_FORMAT::DDS },
		{ L".TIF",	FILE_FORMAT::TIFF },
		{ L".TIFF",	FILE_FORMAT::TIFF },
		{ L".GIF",	FILE_FORMAT::GIF },
		{ L".CRW",	FILE_FORMAT::RAW },
		{ L".CR2",	FILE_FORMAT::RAW },
		{ L".DNG",	FILE_FORMAT::RAW },
		{ L".HDR",	FILE_FORMAT::HDR },
		{ L".EXR",	FILE_FORMAT::EXR },
		{ L".J2K",	FILE_FORMAT::J2K },
		{ L".JP2",	FILE_FORMAT::JP2 },
		{ L".JNG",	FILE_FORMAT::JNG },
		{ L".LBM",	FILE_FORMAT::LBM },
		{ L".IFF",	FILE_FORMAT::IFF },	// = LBM
		{ L".BMP",	FILE_FORMAT::BMP },
		{ L".ICO",	FILE_FORMAT::ICO },
		{ L".PSD",	FILE_FORMAT::PSD },
		{ L".PSB",	FILE_FORMAT::PSD },
		{ L".PCD",	FILE_FORMAT::PCD },
		{ L".PCX",	FILE_FORMAT::PCX },
		{ L".XBM",	FILE_FORMAT::XBM },
		{ L".XPM",	FILE_FORMAT::XPM },
		{ L".WEBP",	FILE_FORMAT::WEBP },
	};

	U32						knownExtensionsCount = sizeof(knownExtensions) / sizeof(KnownExtension);
	const KnownExtension*	knownExtension = knownExtensions;
	for ( U32 knownExtensionIndex=0; knownExtensionIndex < knownExtensionsCount; knownExtensionIndex++, knownExtension++ ) {
		if ( _wcsicmp( extension, knownExtension->extension ) == 0 ) {
			return knownExtension->format;
		}
	}

	return FILE_FORMAT::UNKNOWN;
}

U32	ImageFile::PixelFormat2BPP( PIXEL_FORMAT _pixelFormat ) {
	switch (_pixelFormat ) {
		// 8-bits
		case PIXEL_FORMAT::R8:		return 8;
		case PIXEL_FORMAT::RG8:		return 24;	// Supported as BGR8, otherwise FreeImage thinks it's R5G6B5! :(
		case PIXEL_FORMAT::RGB8:	return 24;
		case PIXEL_FORMAT::BGR8:	return 24;
		case PIXEL_FORMAT::RGBA8:	return 32;
		case PIXEL_FORMAT::BGRA8:	return 32;

		// 16-bits
		case PIXEL_FORMAT::R16:		return 16;
		case PIXEL_FORMAT::RG16:	return 48;	// Supported as RGB16
		case PIXEL_FORMAT::RGB16:	return 48;
		case PIXEL_FORMAT::RGBA16:	return 64;

		// 16-bits half-precision floating points
		case PIXEL_FORMAT::R16F:	return 16;
		case PIXEL_FORMAT::RG16F:	return 48;	// Supported as RGB16F
		case PIXEL_FORMAT::RGB16F:	return 48;
		case PIXEL_FORMAT::RGBA16F:	return 64;

		// 32-bits
		case PIXEL_FORMAT::R32:		return 32;
		case PIXEL_FORMAT::RG32:	return 96;	// Supported as RGB32
		case PIXEL_FORMAT::RGB32:	return 96;
		case PIXEL_FORMAT::RGBA32:	return 128;

		// 32-bits floating points
		case PIXEL_FORMAT::R32F:	return 32;
		case PIXEL_FORMAT::RG32F:	return 96;	// Supported as RGB32F
		case PIXEL_FORMAT::RGB32F:	return 96;
		case PIXEL_FORMAT::RGBA32F:	return 128;
	};

	return 0;
}

// Determine target bitmap type based on target pixel format
FREE_IMAGE_TYPE	ImageFile::PixelFormat2FIT( PIXEL_FORMAT _pixelFormat ) {
	switch ( _pixelFormat ) {
		// 8-bits
		case PIXEL_FORMAT::R8:		return FIT_BITMAP;
		case PIXEL_FORMAT::RG8:		return FIT_BITMAP;	// Here we unfortunately have to use a larger format to accommodate for our 2 components, otherwise FreeImage thinks it's R5G6B5! :(
case PIXEL_FORMAT::RGB8:	return FIT_BITMAP;	// This is NOT the internal representation of a FreeImage bitmap: use BGR8 instead!
		case PIXEL_FORMAT::BGR8:	return FIT_BITMAP;
case PIXEL_FORMAT::RGBA8:	return FIT_BITMAP;	// This is NOT the internal representation of a FreeImage bitmap: use BGRA8 instead!
		case PIXEL_FORMAT::BGRA8:	return FIT_BITMAP;
		// 16-bits
		case PIXEL_FORMAT::R16:		return FIT_UINT16;
		case PIXEL_FORMAT::RG16:	return FIT_RGB16;	// Here we unfortunately have to use a larger format to accommodate for our 2 components
		case PIXEL_FORMAT::RGB16:	return FIT_RGB16;
		case PIXEL_FORMAT::RGBA16:	return FIT_RGBA16;
		// 16-bits half-precision floating points
		case PIXEL_FORMAT::R16F:	return FIT_UINT16;
		case PIXEL_FORMAT::RG16F:	return FIT_RGB16;	// Here we unfortunately have to use a larger format to accommodate for our 2 components
		case PIXEL_FORMAT::RGB16F:	return FIT_RGB16;
		case PIXEL_FORMAT::RGBA16F:	return FIT_RGBA16;
		// 32-bits
		case PIXEL_FORMAT::R32:		return FIT_UINT32;
		case PIXEL_FORMAT::RG32:	return FIT_RGBF;	// Here we unfortunately have to use a larger format to accommodate for our 2 components
		case PIXEL_FORMAT::RGB32:	return FIT_RGBF;
		case PIXEL_FORMAT::RGBA32:	return FIT_RGBAF;
		// 32-bits floating points
		case PIXEL_FORMAT::R32F:	return FIT_FLOAT;
		case PIXEL_FORMAT::RG32F:	return FIT_RGBF;	// Here we unfortunately have to use a larger format to accommodate for our 2 components
		case PIXEL_FORMAT::RGB32F:	return FIT_RGBF;
		case PIXEL_FORMAT::RGBA32F:	return FIT_RGBAF;
	}

	return FIT_UNKNOWN;
}

PIXEL_FORMAT	ImageFile::Bitmap2PixelFormat( const FIBITMAP& _bitmap ) {
	FREE_IMAGE_TYPE	type = FreeImage_GetImageType( const_cast< FIBITMAP* >( &_bitmap ) );
	switch ( type ) {
		// 8-bits
		case FIT_BITMAP: {
			U32	bpp = FreeImage_GetBPP( const_cast< FIBITMAP* >( &_bitmap ) );
			switch ( bpp ) {
				case 8:				return PIXEL_FORMAT::R8;
				case 16:			return PIXEL_FORMAT::RG8;	// Supported as BGR8 with padding, otherwise FreeImage thinks it's R5G6B5! :(
				case 24:			return PIXEL_FORMAT::BGR8;
				case 32:			return PIXEL_FORMAT::BGRA8;
			}
			break;
		}
		// 16-bits
		case FIT_UINT16:			return PIXEL_FORMAT::R16;
		case FIT_RGB16:				return PIXEL_FORMAT::RGB16;
		case FIT_RGBA16:			return PIXEL_FORMAT::RGBA16;
		// 32-bits
		case FIT_FLOAT:				return PIXEL_FORMAT::R32F;
		case FIT_RGBF:				return PIXEL_FORMAT::RGB32F;
		case FIT_RGBAF:				return PIXEL_FORMAT::RGBA32F;
	}

	return PIXEL_FORMAT::UNKNOWN;
}

//typedef void (*FreeImage_OutputMessageFunction)(FREE_IMAGE_FORMAT fif, const char *msg);
wchar_t	ImageFile::ms_lastDumpedText[1024];

void FreeImage_OutputMessage( FREE_IMAGE_FORMAT _fif, const char* _message ) {
	size_t	convertedCharsCount;
	mbstowcs_s( &convertedCharsCount, ImageFile::ms_lastDumpedText, _message, MIN( 1023, (int) strlen(_message)+1 ) );
	ImageFile::ms_lastDumpedText[1023] = '\0';
 
	OutputDebugString( ImageFile::ms_lastDumpedText );
}

void	ImageFile::UseFreeImage() {
	if ( ms_freeImageUsageRefCount == 0 ) {
		FreeImage_Initialise( TRUE );
		FreeImage_SetOutputMessage( FreeImage_OutputMessage );
	}
	ms_freeImageUsageRefCount++;
}
void	ImageFile::UnUseFreeImage() {
	ms_freeImageUsageRefCount--;
	if ( ms_freeImageUsageRefCount == 0 ) {
		FreeImage_DeInitialise();
	}
}

#pragma region Graph Plotting Helpers

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
	float	AxisX0 = linearX ? X0 + (0.0f - _rangeX.x) * DX : X0 + (0.0f - _rangeX.x) * DX;
	DrawLine( _color, bfloat2( AxisX0, 0 ), bfloat2( AxisX0, (float) H-1 ) );
	float	AxisY0 = linearY ? Y0 + (0.0f - _rangeY.x) * DY : Y0 + (0.0f - _rangeY.x) * DY;
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
	if ( P1.x > W-1 ) {
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
			ASSERT( X0 >= 0 && X0 < W, "Offscreen! Check vertical clipping!" );
			Set( Y, X0, _color );
		}
	} else {
		// Draw regular horizontal line
		for ( ; X0 <= X1; X0++, P0.y+=slope ) {
			int	Y = int( floorf( P0.y ) );
			ASSERT( Y >= 0 && Y < H, "Offscreen! Check vertical clipping!" );
			Set( X0, Y, _color );
		}
	}
}

void	ImageFile::RangedCoordinates2ImageCoordinates( const bfloat2& _rangeX, const bfloat2& _rangeY, const bfloat2& _rangedCoordinates, bfloat2& _imageCoordinates ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;

	_imageCoordinates.x = X0 + (_rangedCoordinates.x - _rangeX.x) * (X1 - X0) / (_rangeX.y - _rangeX.x);
	_imageCoordinates.y = Y0 + (_rangedCoordinates.y - _rangeY.x) * (Y1 - Y0) / (_rangeY.y - _rangeY.x);
}
void	ImageFile::ImageCoordinates2RangedCoordinates( const bfloat2& _rangeX, const bfloat2& _rangeY, const bfloat2& _imageCoordinates, bfloat2& _rangedCoordinates ) {
	S32		W = Width();
	S32		H = Height();
	S32		X0 = GRAPH_MARGIN;
	S32		Y0 = H - GRAPH_MARGIN;
	S32		X1 = W - GRAPH_MARGIN;
	S32		Y1 = GRAPH_MARGIN;

	_rangedCoordinates.x = _rangeX.x + (_imageCoordinates.x - X0) * (_rangeX.y - _rangeX.x) / (X1 - X0);
	_rangedCoordinates.y = _rangeY.x + (_imageCoordinates.y - Y0) * (_rangeY.y - _rangeY.x) / (Y1 - Y0);
}

#pragma endregion
