//////////////////////////////////////////////////////////////////////////
// This class wraps over the FreeImage and DirectXTex libraries
//
// The following image formats are currently supported:
//	• Any format supported by the FreeImage v3.17.0 library, namely:
//		• BMP files [reading, writing]
//		• Dr. Halo CUT files [reading] *
//		• DDS files [reading]
//		• EXR files [reading, writing]
//		• Raw Fax G3 files [reading]
//		• GIF files [reading, writing]
//		• HDR files [reading, writing]
//		• ICO files [reading, writing]
//		• IFF files [reading]
//		• JBIG files [reading, writing] **
//		• JNG files [reading, writing]
//		• JPEG/JIF files [reading, writing]
//		• JPEG-2000 File Format [reading, writing]
//		• JPEG-2000 codestream [reading, writing]
//		• JPEG-XR files [reading, writing]
//		• KOALA files [reading]
//		• Kodak PhotoCD files [reading]
//		• MNG files [reading]
//		• PCX files [reading]
//		• PBM/PGM/PPM files [reading, writing]
//		• PFM files [reading, writing]
//		• PNG files [reading, writing]
//		• Macintosh PICT files [reading]
//		• Photoshop PSD files [reading]
//		• RAW camera files [reading]
//		• Sun RAS files [reading]
//		• SGI files [reading]
//		• TARGA files [reading, writing]
//		• TIFF files [reading, writing]
//		• WBMP files [reading, writing]
//		• WebP files [reading, writing]
//		• XBM files [reading]
//		• XPM files [reading, writing]
//
//	• The DDS format is fully supported thanks to the DirectXTex library
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#include "Types.h"
#include "FreeImage.h"
#include "MetaData.h"

namespace ImageUtilityLib {

	class Bitmap;
	class ColorProfile;

	class	ImageFile {
		friend class Bitmap;
	public:
		#pragma region NESTED TYPES

		// Wraps around free image's enum
		enum class	FILE_FORMAT {
			UNKNOWN = -1,
			BMP		= 0,
			ICO		= 1,
			JPEG	= 2,
			JNG		= 3,
			KOALA	= 4,
			LBM		= 5,
			IFF = LBM,
			MNG		= 6,
			PBM		= 7,
			PBMRAW	= 8,
			PCD		= 9,
			PCX		= 10,
			PGM		= 11,
			PGMRAW	= 12,
			PNG		= 13,
			PPM		= 14,
			PPMRAW	= 15,
			RAS		= 16,
			TARGA	= 17,
			TIFF	= 18,
			WBMP	= 19,
			PSD		= 20,
			CUT		= 21,
			XBM		= 22,
			XPM		= 23,
			DDS		= 24,
			GIF     = 25,
			HDR		= 26,
			FAXG3	= 27,
			SGI		= 28,
			EXR		= 29,
			J2K		= 30,
			JP2		= 31,
			PFM		= 32,
			PICT	= 33,
			RAW		= 34,
			WEBP	= 35,
			JXR		= 36
		};

		enum class BIT_DEPTH {
			BPP8	= 8,
			BPP16	= 16,
			BPP16F	= 16,
			BPP32	= 32,
			BPP32F	= 32,
		};

		// Formatting flags for the Save() method
		enum class FORMAT_FLAGS {
			NONE = 0,

			// Bits per pixel component
			SAVE_8BITS_UNORM = 0,	// Save as byte
			SAVE_16BITS_UNORM = 1,	// Save as UInt16 if possible (valid for PNG, TIFF)
			SAVE_32BITS_FLOAT = 2,	// Save as float if possible (valid for TIFF)

			// Gray
			GRAY = 4,				// Save as gray levels

			SKIP_ALPHA = 8,			// Don't save alpha
			PREMULTIPLY_ALPHA = 16,	// RGB should be multiplied by alpha
		};

		
		// This is an aggregate of the various options that can be fed to the Save() method
		
		struct FormatEncoderOptions {
// 			// FILE_FORMAT == JPEG
// 			int	JPEGQualityLevel = 80;	// 80%
// 
// 			// FILE_FORMAT == PNG
// 			PngInterlaceOption	PNGInterlace = PngInterlaceOption.Default;
// 
// 			// FILE_FORMAT == TIFF
// 			TiffCompressOption	TIFFCompression = TiffCompressOption.Rle;
		};

		// This enum matches the classes available in PixelFormat.h (which in turn match the DXGI formats)
		enum class PIXEL_FORMAT {
			// 8-bits
			R8,
			RG8,
			RGB8,
			RGBA8,

			// 16-bits
			R16,
//			RG16,
			RGB16,
			RGBA16,
//			R16F,			// Unsupported
//			RG16F,			// Unsupported
//			RGBA16F,		// Unsupported

			// 32-bits
			R32F,
//			RG32F,			// Unsupported
			RGB32F,
			RGBA32F,
		};

		#pragma endregion

	private:
		#pragma region FIELDS

		FIBITMAP*		m_bitmap;
		FILE_FORMAT		m_fileFormat;		// File format (available if created from a file or saved to a file at some point)

		MetaData		m_metadata;
		ColorProfile*	m_colorProfile;		// An optional color profile found in the input file if the bitmap was loaded from a file

		#pragma endregion

	public:
		#pragma region PROPERTIES

		// Gets the bitmap's content
		void*				Bits()					{ return FreeImage_GetBits( m_bitmap ); }
		const void*			Bits() const			{ return FreeImage_GetBits( m_bitmap ); }

		// Gets the source bitmap type
		FILE_FORMAT			FileFormat() const		{ return m_fileFormat; }

		// Gets the image width
		U32					Width() const			{ return FreeImage_GetWidth( m_bitmap ); }

		// Gets the image height
		U32					Height() const			{ return FreeImage_GetHeight( m_bitmap ); }

		// Tells if the image has an alpha channel
//		bool				HasAlpha				{ get { return m_hasAlpha; } set { m_hasAlpha = value; } }

		// Gets the image's metadata (i.e. ISO, Tv, Av, focal length, etc.)
		const MetaData&		GetMetadata() const		{ return m_metadata; }

		// Gets the optional color profile retrieved during file loading
		const ColorProfile*	GetColorProfile() const		{ return m_colorProfile; }

		#pragma endregion

	public:

		ImageFile();
		ImageFile( FILE_FORMAT _format, const wchar_t* _fileName );
		ImageFile( FILE_FORMAT _format, const U8* _fileContent, U64 _fileSize );
		ImageFile( const Bitmap& _bitmap, PIXEL_FORMAT _targetFormat );
		ImageFile( U32 _width, U32 _height, PIXEL_FORMAT _format ) : m_bitmap( nullptr ), m_colorProfile( nullptr ) {
			Init( _width, _height, _format );
		}
		~ImageFile();

		// Initialize with provided dimensions and pixel format
		void	Init( U32 _width, U32 _height, PIXEL_FORMAT _format );

		// Releases the image
		void	Exit();
		
		// Save to a file
		// <param name="_Stream"></param>
		// <param name="_FileType"></param>
		// <param name="_Parms"></param>
		void	Save( const char* _fileName ) {
			Save( _fileName, FORMAT_FLAGS::NONE );
		}
		void	Save( const char* _fileName, FORMAT_FLAGS _Parms ) {
			Save( _fileName, _Parms, nullptr );
		}
		void	Save( const char* _fileName, FORMAT_FLAGS _Parms, const FormatEncoderOptions* _options ) {
			FILE_FORMAT	FileType = GetFileType( _fileName );
// 			using ( System.IO.FileStream S = _fileName.Create() )
// 				Save( S, FileType, _Parms, _options );
		}
		
		// Save to a stream
		// <param name="_Stream">The stream to write the image to</param>
		// <param name="_FileType">The file type to save as</param>
		// <param name="_Parms">Additional formatting flags</param>
		// <param name="_options">An optional block of options for encoding</param>
		// <exception cref="NotSupportedException">Occurs if the image type is not supported by the Bitmap class</exception>
		// <exception cref="Exception">Occurs if the source image format cannot be converted to RGBA32F which is the generic format we read from</exception>
//		void	Save( System.IO.Stream _Stream, FILE_FORMAT _FileType, FORMAT_FLAGS _Parms, const FormatEncoderOptions* _options ) const;
		
		// Retrieves the image file type based on the image file name
		// <param name="_ImageFileNameName">The image file name</param>
		static FILE_FORMAT	GetFileType( const char* _imageFileNameName );

	private:
		// Retrieves relevant image metadata
		void				RetrieveMetaData();

		static BIT_DEPTH			PixelFormat2BPP( PIXEL_FORMAT _pixelFormat );

		static FREE_IMAGE_TYPE		PixelFormat2FIT( PIXEL_FORMAT _pixelFormat );

		static FILE_FORMAT			FIF2FORMAT( FREE_IMAGE_FORMAT _format )	{ return FILE_FORMAT( _format ); }
		static FREE_IMAGE_FORMAT	FORMAT2FIF( FILE_FORMAT _format )		{ return FREE_IMAGE_FORMAT( _format ); }

		// Attempts to create a color profile from a bitmap
		// NOTE: The caller MUST delete the returned profile!
		static ColorProfile*		CreateColorProfile( FILE_FORMAT _format, const FIBITMAP& _bitmap );

	private:	// Ref-counting for free image lib init/release
		static U32		ms_freeImageUsageRefCount;
		void	ImageFile::UseFreeImage();
		void	ImageFile::UnUseFreeImage();
	};

}	// namespace