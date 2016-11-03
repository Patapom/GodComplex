#include "ImageFile.h"
#include "Bitmap.h"
#include <string>

using namespace ImageUtilityLib;

U32	ImageFile::ms_freeImageUsageRefCount = 0;

ImageFile::ImageFile()
	: m_bitmap( nullptr )
	, m_colorProfile( nullptr )
	, m_fileFormat( FILE_FORMAT::UNKNOWN )
{}

ImageFile::ImageFile( FILE_FORMAT _format, const wchar_t* _fileName ) {
	UseFreeImage();
	m_fileFormat = _format;
	m_bitmap = FreeImage_LoadU( FORMAT2FIF( _format ), _fileName );
	m_colorProfile = CreateColorProfile( m_fileFormat, *m_bitmap );
}

ImageFile::ImageFile( FILE_FORMAT _format, const U8* _fileContent, U64 _fileSize ) {
	UseFreeImage();
	FIMEMORY*	mem = FreeImage_OpenMemory( (BYTE*) _fileContent, U32(_fileSize) );
	if ( mem == nullptr )
		throw "Failed to read bitmap content into memory!";

	m_fileFormat = _format;
	m_bitmap = FreeImage_LoadFromMemory( FORMAT2FIF( _format ), mem );
	FreeImage_CloseMemory( mem );
	m_colorProfile = CreateColorProfile( m_fileFormat, *m_bitmap );
}

ImageFile::~ImageFile() {
	Exit();
	UnUseFreeImage();
}

void	ImageFile::Init( U32 _width, U32 _height, PIXEL_FORMAT _format ) {
	UseFreeImage();
	Exit();

	FREE_IMAGE_TYPE	bitmapType = PixelFormat2FIT( _format );
	int				BPP = int( PixelFormat2BPP( _format ) );
	m_bitmap = FreeImage_AllocateT( bitmapType, _width, _height, BPP );
}

void	ImageFile::Exit() {
	if ( m_bitmap != nullptr ) {
		FreeImage_Unload( m_bitmap );
		m_bitmap = nullptr;
	}
	SAFE_DELETE( m_colorProfile );
	m_fileFormat = FILE_FORMAT::UNKNOWN;
	m_metadata.Reset();
}

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

ImageFile::FILE_FORMAT	ImageFile::GetFileType( const char* _imageFileNameName ) {
	if ( _imageFileNameName == nullptr )
		return FILE_FORMAT::UNKNOWN;

#if 1
	FILE_FORMAT	result = FIF2FORMAT( FreeImage_GetFileType( _imageFileNameName, 0 ) );
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

ImageFile::BIT_DEPTH	ImageFile::PixelFormat2BPP( PIXEL_FORMAT _pixelFormat ) {
	switch (_pixelFormat ) {
		// 8-bits
		case PIXEL_FORMAT::R8:
		case PIXEL_FORMAT::RG8:
		case PIXEL_FORMAT::RGB8:
		case PIXEL_FORMAT::RGBA8:
			return BIT_DEPTH::BPP8;

		// 16-bits
		case PIXEL_FORMAT::R16:
//		case PIXEL_FORMAT::RG16:
		case PIXEL_FORMAT::RGB16:
		case PIXEL_FORMAT::RGBA16:
			return BIT_DEPTH::BPP16;
//		case PIXEL_FORMAT::R16F:
//		case PIXEL_FORMAT::RG16F:
//		case PIXEL_FORMAT::RGBA16F:
			return BIT_DEPTH::BPP16F;

		// 32-bits
		case PIXEL_FORMAT::R32F:
//		case PIXEL_FORMAT::RG32F:
		case PIXEL_FORMAT::RGB32F:
		case PIXEL_FORMAT::RGBA32F:
			return BIT_DEPTH::BPP32F;
	};

	return BIT_DEPTH(0U);
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
// 		case ImageFile::PIXEL_FORMAT::RGBA16F:	// Unsupported
		// 32-bits
		case ImageFile::PIXEL_FORMAT::R32F:		return FIT_FLOAT;
//		case ImageFile::PIXEL_FORMAT::RG32F:	// Unsupported
		case ImageFile::PIXEL_FORMAT::RGB32F:	return FIT_RGBF;
		case ImageFile::PIXEL_FORMAT::RGBA32F:	return FIT_RGBAF;
	}

	return FIT_UNKNOWN;
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

void	ImageFile::RetrieveMetaData() {
// @TODO!

//			EnumerateMetaDataJPG( _MetaData, out m_profileFoundInFile, out bGammaFoundInFile );
// 			if ( !m_profileFoundInFile && !bGammaFoundInFile )
// 				bGammaFoundInFile = true;			// Unless specified otherwise, we override the gamma no matter what since JPEGs use a 2.2 gamma by default anyway

// 			EnumerateMetaDataPNG( _MetaData, out m_profileFoundInFile, out bGammaFoundInFile );

// 			EnumerateMetaDataTIFF( _MetaData, out m_profileFoundInFile, out bGammaFoundInFile );

// 					case FILE_TYPE.CRW:
// 					case FILE_TYPE.CR2:
// 					case FILE_TYPE.DNG:
// 						{
// 							using ( System::IO::MemoryStream Stream = new System::IO::MemoryStream( _ImageFileContent ) )
// 								using ( LibRawManaged.RawFile Raw = new LibRawManaged.RawFile() ) {
// 									Raw.UnpackRAW( Stream );
// 
// 									ColorProfile.Chromaticities	Chroma = Raw.ColorProfile == LibRawManaged.RawFile.COLOR_PROFILE.ADOBE_RGB
// 																		? ColorProfile.Chromaticities.AdobeRGB_D65	// Use Adobe RGB
// 																		: ColorProfile.Chromaticities.sRGB;			// Use default sRGB color profile
// 
// 									// Create a default sRGB linear color profile
// 									m_colorProfile = _ProfileOverride != nullptr ? _ProfileOverride
// 										: new ColorProfile(
// 											Chroma,
// 											ColorProfile.GAMMA_CURVE.STANDARD,	// But with a standard gamma curve...
// 											1.0f								// Linear
// 										);
// 
// 									// Also get back valid camera shot info
// 									m_hasValidShotInfo = true;
// 									m_ISOSpeed = Raw.ISOSpeed;
// 									m_shutterSpeed = Raw.ShutterSpeed;
// 									m_aperture = Raw.Aperture;
// 									m_focalLength = Raw.FocalLength;


}

ColorProfile*	ImageFile::CreateColorProfile( FILE_FORMAT _format, const FIBITMAP& _bitmap ) {

Apparemment, c'est pas ce que je pensais... Ca stocke juste un bloc de data trouvé dans le fichier :(
// 	// Try to use the file's associated profile if it exists
// 	FIICCPROFILE*	fileProfile = FreeImage_GetICCProfile( _bitmap );
// 	if ( fileProfile != nullptr ) {
// 
// 	}

	// Otherwise, recover our own
	ColorProfile*	profile = nullptr;
	switch ( _format ) {
		case FILE_FORMAT::JPEG:
// pas de gamma sur les JPEG si non spécifié !
// Il y a bien une magouille faite lors de la conversion par le FormatConvertedBitmap!
			profile = new ColorProfile( ColorProfile::Chromaticities::sRGB,		// Default for JPEGs is sRGB
										ColorProfile::GAMMA_CURVE::STANDARD,	// JPG uses a 2.2 gamma by default
										2.2f
									);
			break;

		case FILE_FORMAT::PNG:
			profile = new ColorProfile( ColorProfile::Chromaticities::sRGB,		// Default for PNGs is standard sRGB	
										ColorProfile::GAMMA_CURVE::sRGB,
										ColorProfile::GAMMA_EXPONENT_sRGB
									);
			break;

		case FILE_FORMAT::TIFF:
			profile = new ColorProfile( ColorProfile::Chromaticities::sRGB,		// Default for TIFFs is sRGB
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f									// Linear gamma by default
									);
			break;

		case FILE_FORMAT::GIF:
			profile = new ColorProfile( ColorProfile::Chromaticities::sRGB,		// Default for GIFs is standard sRGB with no gamma
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f
									);
			break;

		case FILE_FORMAT::BMP:	// BMP Don't have metadata!
			profile = new ColorProfile( ColorProfile::Chromaticities::sRGB,		// Default for BMPs is standard sRGB with no gamma
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f
									);
			break;

		case FILE_FORMAT::RAW:	// Raw files have no correction
			profile = new ColorProfile( ColorProfile::Chromaticities::sRGB,
										ColorProfile::GAMMA_CURVE::STANDARD,
										1.0f
									);
			break;

		case FILE_FORMAT::TARGA: {
			float	gammaExponent = 2.2f;
			int		num, den;
			if (	MetaData::GetInteger( FIMD_COMMENTS, _bitmap, "GammaNumerator", num )
				&&	MetaData::GetInteger( FIMD_COMMENTS, _bitmap, "GammaDenominator", den ) ) {
				gammaExponent = float(num) / den;
			}

			profile = new ColorProfile( ColorProfile::Chromaticities::sRGB,		// Use default sRGB color profile
										ColorProfile::GAMMA_CURVE::STANDARD,	// But with a standard gamma curve...
										gammaExponent							// ...whose gamma is retrieved from extension data, if available
									);
			}
			break;
	}

	return profile;
}
