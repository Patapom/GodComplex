#include "ImageFile.h"
#include "Bitmap.h"
#include <string>

using namespace ImageUtilityLib;

U32	ImageFile::ms_freeImageUsageRefCount = 0;

ImageFile::ImageFile()
	: m_bitmap( nullptr )
	, m_pixelFormat( PIXEL_FORMAT::UNKNOWN )
	, m_fileFormat( FILE_FORMAT::UNKNOWN )
{}

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

ImageFile::ImageFile( U32 _width, U32 _height, PIXEL_FORMAT _format )
	: m_bitmap( nullptr )
{
	Init( _width, _height, _format );
}

ImageFile::~ImageFile() {
	Exit();
	UnUseFreeImage();
}

bool	ImageFile::HasAlpha() const {
	switch ( m_pixelFormat ) {
	case PIXEL_FORMAT::RGBA8:
	case PIXEL_FORMAT::RGBA16:
//	case PIXEL_FORMAT::RGBA16F:
	case PIXEL_FORMAT::RGBA32F:
		return true;
	}
	return false;
}

void	ImageFile::Init( U32 _width, U32 _height, PIXEL_FORMAT _format ) {
	UseFreeImage();
	Exit();

	m_pixelFormat = _format;

	FREE_IMAGE_TYPE	bitmapType = PixelFormat2FIT( _format );
	int				BPP = int( PixelFormat2BPP( _format ) );
	m_bitmap = FreeImage_AllocateT( bitmapType, _width, _height, BPP );
	if ( m_bitmap == nullptr )
		throw "Failed to initialize image file!";
}

void	ImageFile::Exit() {
	if ( m_bitmap != nullptr ) {
		FreeImage_Unload( m_bitmap );
		m_bitmap = nullptr;
	}
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

	m_pixelFormat = Bitmap2PixelFormat( *m_bitmap );

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

	m_pixelFormat = Bitmap2PixelFormat( *m_bitmap );

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

	m_fileFormat = _format;
	if ( !FreeImage_SaveU( FileFormat2FIF( _format ), m_bitmap, _fileName, int(_options) ) )
		throw "Failed to save the image file!";
}
void	ImageFile::Save( FILE_FORMAT _format, SAVE_FLAGS _options, void*& _fileContent, U64 _fileSize ) const {
	if ( _format == FILE_FORMAT::UNKNOWN )
		throw "Unrecognized image file format!";

	m_fileFormat = _format;

	// Save into a stream of unknown size
	FIMEMORY*	stream = FreeImage_OpenMemory();
	if ( !FreeImage_SaveToMemory( FileFormat2FIF( _format ), m_bitmap, stream, int(_options) ) )
		throw "Failed to save the image file!";

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

	// Convert source
	m_pixelFormat = _targetFormat;
	m_bitmap = FreeImage_ConvertToType( _source.m_bitmap, PixelFormat2FIT( _targetFormat ) );

	// Copy metadata
	m_metadata = _source.m_metadata;

	// Copy file format and attempt to create a profile
	m_fileFormat = _source.m_fileFormat;
}

//////////////////////////////////////////////////////////////////////////
// Helpers
ImageFile::FILE_FORMAT	ImageFile::GetFileType( const wchar_t* _imageFileNameName ) {
	if ( _imageFileNameName == nullptr )
		return FILE_FORMAT::UNKNOWN;

#if 1
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
// 		case PIXEL_FORMAT::R16F:	return 16;	// Unsupported
// 		case PIXEL_FORMAT::RG16F:	return 32;	// Unsupported
// 		case PIXEL_FORMAT::RGB16F:	return 48;	// Unsupported
// 		case PIXEL_FORMAT::RGBA16F:	return 64;	// Unsupported

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
// 		case ImageFile::PIXEL_FORMAT::R16F:		// Unsupported
// 		case ImageFile::PIXEL_FORMAT::RG16F:	// Unsupported
// 		case ImageFile::PIXEL_FORMAT::RGB16F:	// Unsupported
// 		case ImageFile::PIXEL_FORMAT::RGBA16F:	// Unsupported
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
			// IThe philosophy of FreeImage regarding "regular bitmaps" is to always allocate a 32-bits entry
			//	per pixel whether each input pixel is 1-, 2-, 4-, 8-, 24- or 32-bits...
			//
//			FREE_IMAGE_COLOR_TYPE	color_type = FreeImage_GetColorType( const_cast< FIBITMAP* >( &_bitmap ) );
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
