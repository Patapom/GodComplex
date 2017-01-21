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
// The ImageFile is a useful container that supports conversion between pixel formats and is the main transition buffer
//	for the Bitmap class that holds the fully device-independent version of images and is used as a Profile Connection Space
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#pragma unmanaged
#include "..\ImageUtilityLib\ImageFile.h"
#pragma managed

#include "NativeByteArray.h"
#include "MetaData.h"

using namespace System;

namespace ImageUtility {
	[System::Diagnostics::DebuggerDisplayAttribute( "{Width}x{Height} {PixelFormat} {FileFormat}" )]
	public ref class ImageFile {
	public:
		#pragma region NESTED TYPES

		// The delegate used to tone map an HDR image into a LDR color (warning: any returned value above 1 will be clamped!)
		delegate void	ToneMapper( float3 _HDRColor, float3% _LDRColor );

		// The delegate used to transform a source color into a target color LDR used by a System::Drawing::Bitmap
		// NOTE: Any value outside the [0,1] range will be clamped!
		delegate void	ColorTransformer( float4% _color );

		// The delegate used to read from/write to an image
		delegate void	PixelReadWrite( UInt32 _X, UInt32 _Y, float4% _color );

		// This enum matches the classes available in PixelFormat.h (which in turn match the DXGI formats)
		enum class PIXEL_FORMAT : UInt32 {
			UNKNOWN = ~0U,
			NOT_NATIVELY_SUPPORTED = 0x80000000U,	// This flag is used by formats that are not natively supported by FreeImage

			// 8-bits
			R8		= 0,
//			RG8		= 1,		<== FreeImage believes it's R5G6B5!!!
			RGB8	= 2,
			RGBA8	= 3,

			// 16-bits
			R16		= 4,
//			RG16	= 5,		// Unsupported
			RGB16	= 6,
			RGBA16	= 7,

			// 16-bits half-precision floating points
			// WARNING: These formats are NOT natively supported by FreeImage but can be used by DDS for example so I chose
			//			 to support them as regular U16 formats but treating the raw U16 as half-floats internally...
			// NOTE: These are NOT loadable or saveable by the regular Load()/Save() routine, this won't crash but it will produce garbage
			//		 These formats should only be used for in-memory manipulations and DDS-related routines that can manipulate them
			//
			R16F	= 8		| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as R16_UNORM
			RG16F	= 9		| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGB16_UNORM (WARNING: extra blue channel is "unused", actually pixels RGBRGBRGB... scanlines are treated as RGRGRGRG...)
			RGB16F	= 10	| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGB16_UNORM
			RGBA16F	= 11	| NOT_NATIVELY_SUPPORTED,	// Unsupported by FreeImage, aliased as RGBA16_UNORM

			// 32-bits
			R32F	= 12,
//			RG32F	= 13,		// Unsupported
			RGB32F	= 14,
			RGBA32F = 15,
		};

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

		enum class SAVE_FLAGS {
			NONE = 0
 			, SF_BMP_DEFAULT				 = 0
			, SF_BMP_SAVE_RLE				 = 1
			, SF_CUT_DEFAULT				 = 0
			, SF_DDS_DEFAULT				 = 0
			, SF_EXR_DEFAULT				 = 0		//! save data as half with piz-based wavelet compression
			, SF_EXR_FLOAT					 = 0x0001	//! save data as float instead of as half (not recommended)
			, SF_EXR_NONE					 = 0x0002	//! save with no compression
			, SF_EXR_ZIP					 = 0x0004	//! save with zlib compression, in blocks of 16 scan lines
			, SF_EXR_PIZ					 = 0x0008	//! save with piz-based wavelet compression
			, SF_EXR_PXR24					 = 0x0010	//! save with lossy 24-bit float compression
			, SF_EXR_B44					 = 0x0020	//! save with lossy 44% float compression - goes to 22% when combined with EXR_LC
			, SF_EXR_LC						 = 0x0040	//! save images with one luminance and two chroma channels, rather than as RGB (lossy compression)
			, SF_FAXG3_DEFAULT				 = 0
			, SF_GIF_DEFAULT				 = 0
			, SF_GIF_LOAD256				 = 1		//! load the image as a 256 color image with ununsed palette entries, if it's 16 or 2 color
			, SF_GIF_PLAYBACK				 = 2		//! 'Play' the GIF to generate each frame (as 32bpp) instead of returning raw frame data when loading
			, SF_HDR_DEFAULT				 = 0
			, SF_ICO_DEFAULT				 = 0
			, SF_ICO_MAKEALPHA				 = 1		//! convert to 32bpp and create an alpha channel from the AND-mask when loading
			, SF_IFF_DEFAULT				 = 0
			, SF_J2K_DEFAULT				 = 0		//! save with a 16:1 rate
			, SF_JP2_DEFAULT				 = 0		//! save with a 16:1 rate
			, SF_JPEG_DEFAULT				 = 0		//! loading (see JPEG_FAST); saving (see JPEG_QUALITYGOOD|JPEG_SUBSAMPLING_420)
			, SF_JPEG_FAST					 = 0x0001	//! load the file as fast as possible, sacrificing some quality
			, SF_JPEG_ACCURATE				 = 0x0002	//! load the file with the best quality, sacrificing some speed
			, SF_JPEG_CMYK					 = 0x0004	//! load separated CMYK "as is" (use | to combine with other load flags)
			, SF_JPEG_EXIFROTATE			 = 0x0008	//! load and rotate according to Exif 'Orientation' tag if available
			, SF_JPEG_GREYSCALE				 = 0x0010	//! load and convert to a 8-bit greyscale image
			, SF_JPEG_QUALITYSUPERB			 = 0x80		//! save with superb quality (100:1)
			, SF_JPEG_QUALITYGOOD			 = 0x0100	//! save with good quality (75:1)
			, SF_JPEG_QUALITYNORMAL			= 0x0200	//! save with normal quality (50:1)
			, SF_JPEG_QUALITYAVERAGE		 = 0x0400	//! save with average quality (25:1)
			, SF_JPEG_QUALITYBAD			 = 0x0800	//! save with bad quality (10:1)
			, SF_JPEG_PROGRESSIVE			 = 0x2000	//! save as a progressive-JPEG (use | to combine with other save flags)
			, SF_JPEG_SUBSAMPLING_411		 = 0x1000	//! save with high 4x1 chroma subsampling (4:1:1) 
			, SF_JPEG_SUBSAMPLING_420		 = 0x4000	//! save with medium 2x2 medium chroma subsampling (4:2:0) - default value
			, SF_JPEG_SUBSAMPLING_422		 = 0x8000	//! save with low 2x1 chroma subsampling (4:2:2) 
			, SF_JPEG_SUBSAMPLING_444		 = 0x10000	//! save with no chroma subsampling (4:4:4)
			, SF_JPEG_OPTIMIZE				 = 0x20000	//! on saving, compute optimal Huffman coding tables (can reduce a few percent of file size)
			, SF_JPEG_BASELINE				 = 0x40000	//! save basic JPEG, without metadata or any markers
			, SF_KOALA_DEFAULT				 = 0
			, SF_LBM_DEFAULT				 = 0
			, SF_MNG_DEFAULT				 = 0
			, SF_PCD_DEFAULT				 = 0
			, SF_PCD_BASE					 = 1		//! load the bitmap sized 768 x 512
			, SF_PCD_BASEDIV4				 = 2		//! load the bitmap sized 384 x 256
			, SF_PCD_BASEDIV16				 = 3		//! load the bitmap sized 192 x 128
			, SF_PCX_DEFAULT				 = 0
			, SF_PFM_DEFAULT				 = 0
			, SF_PICT_DEFAULT				 = 0
			, SF_PNG_DEFAULT				 = 0
			, SF_PNG_IGNOREGAMMA			 = 1		//! loading: avoid gamma correction
			, SF_PNG_Z_BEST_SPEED			 = 0x0001	//! save using ZLib level 1 compression flag (default value is 6)
			, SF_PNG_Z_DEFAULT_COMPRESSION	 = 0x0006	//! save using ZLib level 6 compression flag (default recommended value)
			, SF_PNG_Z_BEST_COMPRESSION		 = 0x0009	//! save using ZLib level 9 compression flag (default value is 6)
			, SF_PNG_Z_NO_COMPRESSION		 = 0x0100	//! save without ZLib compression
			, SF_PNG_INTERLACED				 = 0x0200	//! save using Adam7 interlacing (use | to combine with other save flags)
			, SF_PNM_DEFAULT				 = 0
			, SF_PNM_SAVE_RAW				 = 0		//! if set the writer saves in RAW format (i.e. P4, P5 or P6)
			, SF_PNM_SAVE_ASCII				 = 1		//! if set the writer saves in ASCII format (i.e. P1, P2 or P3)
			, SF_PSD_DEFAULT				 = 0
			, SF_PSD_CMYK					 = 1		//! reads tags for separated CMYK (default is conversion to RGB)
			, SF_PSD_LAB					 = 2		//! reads tags for CIELab (default is conversion to RGB)
			, SF_RAS_DEFAULT				 = 0
			, SF_RAW_DEFAULT				 = 0		//! load the file as linear RGB 48-bit
			, SF_RAW_PREVIEW				 = 1		//! try to load the embedded JPEG preview with included Exif Data or default to RGB 24-bit
			, SF_RAW_DISPLAY				 = 2		//! load the file as RGB 24-bit
			, SF_RAW_HALFSIZE				 = 4		//! output a half-size color image
			, SF_RAW_UNPROCESSED			 = 8		//! output a FIT_UINT16 raw Bayer image
			, SF_SGI_DEFAULT				 = 0
			, SF_TARGA_DEFAULT				 = 0
			, SF_TARGA_LOAD_RGB888			 = 1		//! if set the loader converts RGB555 and ARGB8888 -> RGB888.
			, SF_TARGA_SAVE_RLE				 = 2		//! if set, the writer saves with RLE compression
			, SF_TIFF_DEFAULT				 = 0
			, SF_TIFF_CMYK					 = 0x0001	//! reads/stores tags for separated CMYK (use | to combine with compression flags)
			, SF_TIFF_PACKBITS				 = 0x0100	//! save using PACKBITS compression
			, SF_TIFF_DEFLATE				 = 0x0200	//! save using DEFLATE compression (a.k.a. ZLIB compression)
			, SF_TIFF_ADOBE_DEFLATE			 = 0x0400	//! save using ADOBE DEFLATE compression
			, SF_TIFF_NONE					 = 0x0800	//! save without any compression
			, SF_TIFF_CCITTFAX3				 = 0x1000	//! save using CCITT Group 3 fax encoding
			, SF_TIFF_CCITTFAX4				 = 0x2000	//! save using CCITT Group 4 fax encoding
			, SF_TIFF_LZW					 = 0x4000	//! save using LZW compression
			, SF_TIFF_JPEG					 = 0x8000	//! save using JPEG compression
			, SF_TIFF_LOGLUV				 = 0x10000	//! save using LogLuv compression
			, SF_WBMP_DEFAULT				 = 0
			, SF_XBM_DEFAULT				 = 0
			, SF_XPM_DEFAULT				 = 0
			, SF_WEBP_DEFAULT				 = 0		//! save with good quality (75:1)
			, SF_WEBP_LOSSLESS				 = 0x100	//! save in lossless mode
			, SF_JXR_DEFAULT				 = 0		//! save with quality 80 and no chroma subsampling (4:4:4)
			, SF_JXR_LOSSLESS				 = 0x0064	//! save lossless
			, SF_JXR_PROGRESSIVE			 = 0x2000	//! save as a progressive-JXR (use | to combine with other save flags)
		};

		#pragma endregion

	internal:
		bool							m_ownedObject;
		ImageUtilityLib::ImageFile*		m_nativeObject;

		// Special wrapper constructor
		ImageFile( ImageUtilityLib::ImageFile& _nativeObject, bool _deleteOnDesctruction ) {
			m_ownedObject = _deleteOnDesctruction;
			m_nativeObject = &_nativeObject;
		}

	public:
		#pragma region PROPERTIES

// 		property ImageUtilityLib::ImageFile&	NativeObject	{
// 			ImageUtilityLib::ImageFile& get() { return *m_nativeObject; }
// 		}

 		// Gets the bitmap's raw content
		property IntPtr		Bits {
			IntPtr		get() {
				void*	nativePtr = m_nativeObject->GetBits();
				return IntPtr( nativePtr );
			}
		}

		// Gets the image's pixel format
		property PIXEL_FORMAT	PixelFormat {
			PIXEL_FORMAT	get() {
				ImageUtilityLib::ImageFile::PIXEL_FORMAT	nativeFormat = m_nativeObject->GetPixelFormat();
				return PIXEL_FORMAT( nativeFormat );
			}
		}

		// Gets the source bitmap type
		property FILE_FORMAT	FileFormat {
			FILE_FORMAT	get() {
				ImageUtilityLib::ImageFile::FILE_FORMAT	nativeFormat = m_nativeObject->GetFileFormat();
				return FILE_FORMAT( nativeFormat );
			}
		}

		// Gets the image width
		property UInt32		Width {
			UInt32	get() { return m_nativeObject->Width(); }
		}

		// Gets the image height
		property UInt32		Height {
			UInt32	get() { return m_nativeObject->Height(); }
		}

		// Gets the image width
		property UInt32		Pitch {
			UInt32	get() { return m_nativeObject->Pitch(); }
		}

		// Gets the pixel size
		property UInt32		PixelSize {
			UInt32	get() { return m_nativeObject->GetPixelFormatAccessor().Size(); }
		}

		// Tells if the image has an alpha channel
		property bool		HasAlpha {
			bool	get() { return m_nativeObject->HasAlpha(); }
		}

		// Gets the image's metadata (i.e. ISO, Tv, Av, focal length, etc.)
		property MetaData^	Metadata {
			MetaData^	get() {
				return gcnew MetaData( this );
			}
		}

		// Gets the color profile associated to the image
		property ImageUtility::ColorProfile^	ColorProfile {
			ImageUtility::ColorProfile^	get() {
				return gcnew ImageUtility::ColorProfile( m_nativeObject->GetColorProfile() );
			}
			void set( ImageUtility::ColorProfile^ value ) {
				if ( value == nullptr )
					throw gcnew Exception( "Invalid color profile! You MUST provide a valid color profile at all times for the image's metadata structure." );
				m_nativeObject->SetColorProfile( *value->m_nativeObject );
			}
		}

		// Builds a System.Drawing.Bitmap from the image
		// Warning: throws an exception if your image format is HDR! (cf. ToneMapFrom())
		property System::Drawing::Bitmap^	AsBitmap {
			System::Drawing::Bitmap^	get();
		}

		// Generic color getter/setter
		property SharpMath::float4		default[UInt32, UInt32] {
			SharpMath::float4		get( UInt32 _X, UInt32 _Y ) {
				bfloat4	color;
				m_nativeObject->Get( _X, _Y, color );
				return SharpMath::float4( color.x, color.y, color.z, color.w );
			}
			void		set( UInt32 _X, UInt32 _Y, SharpMath::float4 value ) {
				m_nativeObject->Set( _X, _Y, bfloat4( value.x, value.y, value.z, value.w ) );
			}
		}
		void		Add( UInt32 _X, UInt32 _Y, SharpMath::float4^ value ) {
			m_nativeObject->Add( _X, _Y, bfloat4( value->x, value->y, value->z, value->w ) );
		}

		#pragma endregion

	public:

		ImageFile() {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImageFile();
		}
		ImageFile( System::IO::FileInfo^ _fileName ) {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImageFile();
			Load( _fileName );
		}
		ImageFile( System::IO::FileInfo^ _fileName, FILE_FORMAT _format ) {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImageFile();
			Load( _fileName, _format );
		}
		ImageFile( NativeByteArray^ _fileContent, FILE_FORMAT _format ) {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImageFile();
			Load( _fileContent, _format );
		}
		ImageFile( U32 _width, U32 _height, PIXEL_FORMAT _format, ImageUtility::ColorProfile^ _colorProfile ) {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImageFile();
			Init( _width, _height, _format, _colorProfile );
		}
		ImageFile( ImageFile^ _other ) {
			m_ownedObject = true;
			m_nativeObject = new ImageUtilityLib::ImageFile( *_other->m_nativeObject );
		}

		// Creates a bitmap from a System::Drawing.Bitmap and a color profile
		ImageFile( System::Drawing::Bitmap^ _bitmap, ImageUtility::ColorProfile^ _colorProfile );

		~ImageFile();

	public:

		// Initialize with provided dimensions and pixel format
		//	_width, _height, the dimensions of the image
		//	_format, the pixel format of the image
		//	_colorProfile, the compulsory color profile associated to the image (if not sure, create a standard sRGB color profile)
		void				Init( U32 _width, U32 _height, PIXEL_FORMAT _format, ImageUtility::ColorProfile^ _colorProfile );

		// Releases the image
		void				Exit();

		// Load from a file or memory
		void				Load( System::IO::FileInfo^ _fileName );
		void				Load( System::IO::FileInfo^ _fileName, FILE_FORMAT _format );
		void				Load( System::IO::Stream^ _imageStream, FILE_FORMAT _format );
		void				Load( NativeByteArray^ _fileContent, FILE_FORMAT _format );

		// Save to a file or memory
		void				Save( System::IO::FileInfo^ _fileName );
		void				Save( System::IO::FileInfo^ _fileName, FILE_FORMAT _format );
		void				Save( System::IO::FileInfo^ _fileName, FILE_FORMAT _format, SAVE_FLAGS _options );
		void				Save( System::IO::Stream^ _imageStream, FILE_FORMAT _format, SAVE_FLAGS _options );
		NativeByteArray^	Save( FILE_FORMAT _format, SAVE_FLAGS _options );
		
		// Converts the source image to a target format
		// Warning: throws an exception if your image format is HDR, you need to use ToneMapFrom() method and provide a valid tone mapper operator for proper HDR->LDR conversion instead!
		void				ConvertFrom( ImageFile^ _source, PIXEL_FORMAT _targetFormat );

		// Tone maps a HDR image into a LDR RGBA8 format
		void				ToneMapFrom( ImageFile^ _source, ToneMapper^ _toneMapper );

		// Generic color getter/setter
		void				ReadScanline( UInt32 _Y, cli::array< float4 >^ _color ) { ReadScanline( _Y, _color, 0 ); }
		void				ReadScanline( UInt32 _Y, cli::array< float4 >^ _color, UInt32 _startX );
		void				ReadPixels( PixelReadWrite^ _reader ) { ReadPixels( _reader, 0, 0, Width, Height ); }
		void				ReadPixels( PixelReadWrite^ _reader, UInt32 _startX, UInt32 _startY, UInt32 _width, UInt32 _height );
		void				WriteScanline( UInt32 _Y, cli::array< float4 >^ _color ) { WriteScanline( _Y, _color, 0 ); }
		void				WriteScanline( UInt32 _Y, cli::array< float4 >^ _color, UInt32 _startX );
		void				WritePixels( PixelReadWrite^ _writer ) { WritePixels( _writer, 0, 0, Width, Height ); }
		void				WritePixels( PixelReadWrite^ _writer, UInt32 _startX, UInt32 _startY, UInt32 _width, UInt32 _height );

		// Retrieves the image file type based on the image file name
		// WARNING: The image file MUST exist on disk as FreeImage inspects the content!
		static FILE_FORMAT	GetFileTypeFromExistingFileContent( System::IO::FileInfo^ _fileName );
		// Same version from filename only
		static FILE_FORMAT	GetFileTypeFromFileNameOnly( System::IO::FileInfo^ _fileName );

		// Loads a System::Drawing.Bitmap into a byte[] containing RGBARGBARG... pixels
		// <param name="_Bitmap">The source System::Drawing.Bitmap to load</param>
		// <param name="_Width">The bitmap's width</param>
		// <param name="_Height">The bitmaps's height</param>
		// <returns>The byte array containing a sequence of R,G,B,A,R,G,B,A pixels and of length Widht*Height*4</returns>
		static cli::array< Byte >^	LoadBitmap( System::Drawing::Bitmap^ _bitmap, int& _width, int& _height );

		// Builds a System.Drawing.Bitmap from the image, applying the user's transform
		// NOTE: Does not throw an exception, but any color outside the [0,1] will be clamped!
		System::Drawing::Bitmap^	AsCustomBitmap( ColorTransformer^ _transformer );
		// Same but the bitmap is already constructed at the proper size
		void						AsCustomBitmap( System::Drawing::Bitmap^ _bitmap, ColorTransformer^ _transformer );

		// Builds a System.Drawing.Bitmap from the tiled image
		// Warning: throws an exception if your image format is HDR! (cf. ToneMapFrom())
		// The 2 parameters are the Width and Height of the expected bitmap, the image will tile as much as possible to fill the target bitmap size
		System::Drawing::Bitmap^	AsTiledBitmap( UInt32 _width, UInt32 _height );


	public:
		//////////////////////////////////////////////////////////////////////////
		// Plotting helpers
		// The plot delegate that returns y=f(x)
		delegate float		PlotDelegate( float _x );

		// Clears the image with the provided color
		void				Clear( SharpMath::float4^ _color );

		// Plots the y=f(x) graph for a given X and Y range
		void				PlotGraph( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2^ _rangeY, PlotDelegate^ _delegate );

		// Plots the y=f(x) graph for a given X range, Y range is automatically determined and returned
		void				PlotGraphAutoRangeY( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2% _rangeY, PlotDelegate^ _delegate );

		// Plots the y=f(x) graph in logarithmic scale for a given X and Y range and given log bases for each axis
		// NOTE: Use a log base of 1 for a linear scale
//		void				PlotLogGraph( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2^ _rangeY, PlotDelegate^ _delegate );	// Default log10
		void				PlotLogGraph( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2^ _rangeY, PlotDelegate^ _delegate, float _logBaseX, float _logBaseY );

		// Plots the y=f(x) graph in logarithmic scale for a given X range, Y range is automatically determined and returned
//		void				PlotLogGraphAutoRangeY( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2% _rangeY, PlotDelegate^ _delegate );	// Default log10
		void				PlotLogGraphAutoRangeY( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2% _rangeY, PlotDelegate^ _delegate, float _logBaseX, float _logBaseY );

		// Plots the graph axes for the given X Y ranges
		void				PlotAxes( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2^ _rangeY, float _stepX, float _stepY );

		// Plots the graph axes for the given X and Y ranges and given log bases for each axis
		// NOTE: Use a _negative_ log base to indicate the step size and use a linear scale
		void				PlotLogAxes( SharpMath::float4^ _color, SharpMath::float2^ _rangeX, SharpMath::float2^ _rangeY, float _logBaseX, float _logBaseY );

		// Plots a line segment of the given color
		//	_P0, _P1, the position of the line segment's points (i.e. X=0 is left border, X=Width-1 is right border, Y=0 is top border, Y=Height-1 is bottom border)
		void				DrawLine( SharpMath::float4^ _color, SharpMath::float2^ _P0, SharpMath::float2^ _P1 );

		// Converts "ranged coordinates" into (X,Y) pixel coordinates
		SharpMath::float2	RangedCoordinates2ImageCoordinates( SharpMath::float2^ _rangeX, SharpMath::float2^ _rangeY, SharpMath::float2^ _rangedCoordinates );
		// Converts (X,Y) pixel coordinates into "ranged coordinates"
		SharpMath::float2	ImageCoordinates2RangedCoordinates( SharpMath::float2^ _rangeX, SharpMath::float2^ _rangeY, SharpMath::float2^ _imageCoordinates );


	public:
		//////////////////////////////////////////////////////////////////////////
		// DDS-related methods
		enum class COMPRESSION_TYPE {
			NONE,
			BC4,
			BC5,
			BC6H,
			BC6H_GPU,
			BC7,
			BC7_GPU,
		};

		// Compresses a single image
		NativeByteArray^					DDSCompress( COMPRESSION_TYPE _compressionType );

		// Saves a DDS image in memory to disk (usually used after a compression)
		static void							DDSSaveFromMemory( NativeByteArray^ _DDSImage, System::IO::FileInfo^ _fileName );
		static void							DDSSaveFromMemory( NativeByteArray^ _DDSImage, System::IO::Stream^ _imageStream );

		// Cube map handling
		static cli::array< ImageFile^ >^	DDSLoadCubeMap( System::IO::FileInfo^ _fileName );
		static cli::array< ImageFile^ >^	DDSLoadCubeMap( System::IO::Stream^ _imageStream );
		static void							DDSSaveCubeMap( cli::array< ImageFile^ >^ _cubeMapFaces, bool _compressBC6H, System::IO::FileInfo^ _fileName );
		static void							DDSSaveCubeMap( cli::array< ImageFile^ >^ _cubeMapFaces, bool _compressBC6H, System::IO::Stream^ _imageStream );

		// 3D Texture handling
		static cli::array< ImageFile^ >^	DDSLoad3DTexture( System::IO::FileInfo^ _fileName, U32& _slicesCount );
		static cli::array< ImageFile^ >^	DDSLoad3DTexture( System::IO::Stream^ _imageStream );
		static void							DDSSave3DTexture( cli::array< ImageFile^ >^ _slices, bool _compressBC6H, System::IO::FileInfo^ _fileName );
		static void							DDSSave3DTexture( cli::array< ImageFile^ >^ _slices, bool _compressBC6H, System::IO::Stream^ _imageStream );
	};
}
