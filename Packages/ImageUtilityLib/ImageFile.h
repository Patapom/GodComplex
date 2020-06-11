//////////////////////////////////////////////////////////////////////////
// This class wraps over the FreeImage and DirectXTex libraries
//
// The following image formats are currently supported:
//	� Any format supported by the FreeImage v3.17.0 library, namely:
//		� BMP files [reading, writing]
//		� Dr. Halo CUT files [reading] *
//		� DDS files [reading]
//		� EXR files [reading, writing]
//		� Raw Fax G3 files [reading]
//		� GIF files [reading, writing]
//		� HDR files [reading, writing]
//		� ICO files [reading, writing]
//		� IFF files [reading]
//		� JBIG files [reading, writing] **
//		� JNG files [reading, writing]
//		� JPEG/JIF files [reading, writing]
//		� JPEG-2000 File Format [reading, writing]
//		� JPEG-2000 codestream [reading, writing]
//		� JPEG-XR files [reading, writing]
//		� KOALA files [reading]
//		� Kodak PhotoCD files [reading]
//		� MNG files [reading]
//		� PCX files [reading]
//		� PBM/PGM/PPM files [reading, writing]
//		� PFM files [reading, writing]
//		� PNG files [reading, writing]
//		� Macintosh PICT files [reading]
//		� Photoshop PSD files [reading]
//		� RAW camera files [reading]
//		� Sun RAS files [reading]
//		� SGI files [reading]
//		� TARGA files [reading, writing]
//		� TIFF files [reading, writing]
//		� WBMP files [reading, writing]
//		� WebP files [reading, writing]
//		� XBM files [reading]
//		� XPM files [reading, writing]
//
//	� The DDS format is fully supported thanks to the DirectXTex library
//
// The ImageFile is a useful container that supports conversion between pixel formats and is the main transition buffer
//	for the Bitmap class that holds the fully device-independent version of images and is used as a Profile Connection Space
//
////////////////////////////////////////////////////////////////////////////
//
#pragma once

#define IMAGE_FILE_INCLUDED

#include "MetaData.h"

namespace ImageUtilityLib {

	class Bitmap;
	class ColorProfile;
	class ImagesMatrix;

	class	ImageFile {
		friend class Bitmap;
		friend class MetaData;
		friend class ImagesMatrix;
	public:
		#pragma region NESTED TYPES

		// The delegate used to tone map an HDR image into a LDR color (warning: any returned value above 1 will be clamped!)
		typedef void	(*toneMapper_t)( const bfloat3& _HDRColor, bfloat3& _LDRColor );

		// The delegate used to tone map an HDR image into a LDR color (warning: any returned value above 1 will be clamped!)
		typedef void	(*pixelReaderWriter_t)( U32 _X, U32 _Y, bfloat4& _Color );

		// Wraps around free image's "FREE_IMAGE_FORMAT" enum
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
		
		// This is an aggregate of the various flags that can be fed to the Save() method, depending on the target file format
		// NOTE: This enum should match the FreeImage defines found in FreemImage.h
 		enum class SAVE_FLAGS {
 			  SF_BMP_DEFAULT				 = 0
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

	private:
		#pragma region FIELDS

		FIBITMAP*					m_bitmap;
		PIXEL_FORMAT				m_pixelFormat;			// The bitmap's pixel format
		const IPixelAccessor*		m_pixelAccessor;		// The bitmap's pixel accessor to read/write the image's content
		mutable FILE_FORMAT			m_fileFormat;			// File format (available if created from a file or saved to a file at some point)

		MetaData					m_metadata;				// Contains relevant metadata (e.g. ISO, Tv, Av, focal length, etc.)

		#pragma endregion

	public:
		#pragma region PROPERTIES

		// Gets the bitmap's raw content
		U8*					GetBits()				{ return FreeImage_GetBits( m_bitmap ); }
		const U8*			GetBits() const			{ return FreeImage_GetBits( m_bitmap ); }

		// Gets the image's pixel format
		PIXEL_FORMAT		GetPixelFormat() const	{ return m_pixelFormat; }

		// Gets the pixel format's accessor
		const IPixelAccessor&	GetPixelAccessor() const { return *m_pixelAccessor; }

		// Gets the source bitmap type
		FILE_FORMAT			GetFileFormat() const	{ return m_fileFormat; }

		// Gets the image width
		U32					Width() const			{ return FreeImage_GetWidth( m_bitmap ); }

		// Gets the image height
		U32					Height() const			{ return FreeImage_GetHeight( m_bitmap ); }

		// Getsthe image pitch
		U32					Pitch() const			{ return FreeImage_GetPitch( m_bitmap ); }

		// Tells if the image has an alpha channel
		bool				HasAlpha() const;

		// Gets the image's metadata (i.e. ISO, Tv, Av, focal length, etc.)
		MetaData&			GetMetadata()			{ return m_metadata; }
		const MetaData&		GetMetadata() const		{ return m_metadata; }

		// Gets or sets the color profile associated to the image
		ColorProfile&		GetColorProfile()								{ return m_metadata.GetColorProfile(); }
		const ColorProfile&	GetColorProfile() const							{ return m_metadata.GetColorProfile(); }
		void				SetColorProfile( const ColorProfile& _value )	{ m_metadata.SetColorProfile( _value ); }

		// Generic color getter/setter
		void				Get( U32 _X, U32 _Y, bfloat4& _color ) const;
		void				Set( U32 _X, U32 _Y, const bfloat4& _color );
		void				Add( U32 _X, U32 _Y, const bfloat4& _color );

		#pragma endregion

	public:

		ImageFile();
		ImageFile( const ImageFile& _other );
		ImageFile( const ImageFile& _other, U32 _newWidth, U32 _newHeight, PIXEL_FORMAT _newFormat );	// Builds from other and possibily rescale
		ImageFile( const wchar_t* _fileName, FILE_FORMAT _format );
		ImageFile( const U8* _fileContent, U64 _fileSize, FILE_FORMAT _format );
		ImageFile( U32 _width, U32 _height, PIXEL_FORMAT _format, const ColorProfile& _colorProfile );
		~ImageFile();

		ImageFile&			operator=( const ImageFile& _other );

		// Initialize with provided dimensions and pixel format
		//	_width, _height, the dimensions of the image
		//	_format, the pixel format of the image
		//	_colorProfile, the compulsory color profile associated to the image (if not sure, create a standard sRGB color profile)
		void				Init( U32 _width, U32 _height, PIXEL_FORMAT _format, const ColorProfile& _colorProfile );

		// Releases the image
		void				Exit();

		// Load from a file or memory
		void				Load( const wchar_t* _fileName );
		void				Load( const wchar_t* _fileName, FILE_FORMAT _format );
		void				Load( const void* _fileContent, U64 _fileSize, FILE_FORMAT _format );

		// Save to a file or memory
		void				Save( const wchar_t* _fileName ) const;
		void				Save( const wchar_t* _fileName, FILE_FORMAT _format ) const;
		void				Save( const wchar_t* _fileName, FILE_FORMAT _format, SAVE_FLAGS _options ) const;
		void				Save( FILE_FORMAT _format, SAVE_FLAGS _options, U64& _fileSize, void*& _fileContent ) const;	// NOTE: The caller MUST delete the returned buffer!
		
		// Converts the source image to a target format
		// WARNING: HDR formats need to be tone mapped first before being converted to regular LDR formats
		void				ConvertFrom( const ImageFile& _source, PIXEL_FORMAT _targetFormat );

		// Tone maps a HDR image into a LDR RGBA8 format
		void				ToneMapFrom( const ImageFile& _source, toneMapper_t _toneMapper );

		// Copies the source image into this image
		void				CopySource( const ImageFile& _source, U32 _offsetX=0, U32 _offsetY=0 );

		// Rescales the source image into this image
		void				RescaleSource( const ImageFile& _source );

		// Makes the image signed/unsigned
		// WARNING: Works only for integer formats: throws if called on floating-point formats!
		void				MakeSigned();
		void				MakeUnSigned();

		// Generic color getter/setter
		void				ReadScanline( U32 _Y, bfloat4* _color, U32 _startX=0, U32 _count=~0U ) const;
		void				WriteScanline( U32 _Y, const bfloat4* _color, U32 _startX=0, U32 _count=~0U );
		void				ReadPixels( pixelReaderWriter_t _reader, U32 _startX=0, U32 _startY=0, U32 _width=~0U, U32 _height=~0U ) const;
		void				WritePixels( pixelReaderWriter_t _writer, U32 _startX=0, U32 _startY=0, U32 _width=~0U, U32 _height=~0U );
		void				ReadWritePixels( pixelReaderWriter_t _writer, U32 _startX=0, U32 _startY=0, U32 _width=~0U, U32 _height=~0U );	// Same as WritePixels() but the color provided to the delegate is the actual current color of the pixel

		// Retrieves the image file type based on the image file name
		// WARNING: The image file MUST exist on disk as FreeImage inspects the content!
		static FILE_FORMAT	GetFileTypeFromExistingFileContent( const wchar_t* _imageFileNameName );
		// Same version from filename only
		static FILE_FORMAT	GetFileTypeFromFileNameOnly( const wchar_t* _imageFileNameName );


	public:
		//////////////////////////////////////////////////////////////////////////
		// Plotting helpers
		// The plot delegate that returns y=f(x)
		typedef float		(*PlotDelegate_t)( float _x );

		// Clears the image with the provided color
		void				Clear( const bfloat4& _color );

		// Plots the y=f(x) graph for a given X and Y range
		void				PlotGraph( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, PlotDelegate_t _delegate );

		// Plots the y=f(x) graph for a given X range, Y range is automatically determined and returned
		void				PlotGraphAutoRangeY( const bfloat4& _color, const bfloat2& _rangeX, bfloat2& _rangeY, PlotDelegate_t _delegate );

		// Plots the y=f(x) graph in logarithmic scale for a given X and Y range and given log bases for each axis
		//	_rangeX, _rangeY, the ranges for the axes. When in log-space, ranges are in log-space as well (e.g. [-2,+2] in log10 means plotting from 1e-2 to 1e+2)
		// NOTE: Use a log base of 1 for a linear scale
		void				PlotLogGraph( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, PlotDelegate_t _delegate, float _logBaseX=10.0f, float _logBaseY=10.0f );

		// Plots the y=f(x) graph in logarithmic scale for a given X range and given log bases for each axis, Y range is automatically determined and returned
		//	_rangeX, _rangeY, the ranges for the axes. When in log-space, ranges are in log-space as well (e.g. [-2,+2] in log10 means plotting from 1e-2 to 1e+2)
		// NOTE: Use a log base of 1 for a linear scale
		void				PlotLogGraphAutoRangeY( const bfloat4& _color, const bfloat2& _rangeX, bfloat2& _rangeY, PlotDelegate_t _delegate, float _logBaseX=10.0f, float _logBaseY=10.0f );

		// Plots the graph axes for the given X and Y ranges
		void				PlotAxes( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, float _stepX, float _stepY );

		// Plots the graph axes for the given X and Y ranges and given log bases for each axis
		//	_rangeX, _rangeY, the ranges for the axes. When in log-space, ranges are in log-space as well (e.g. [-2,+2] in log10 means plotting from 1e-2 to 1e+2)
		// NOTE: Use a _negative_ log base to indicate the step size and use a linear scale
		void				PlotLogAxes( const bfloat4& _color, const bfloat2& _rangeX, const bfloat2& _rangeY, float _logBaseX, float _logBaseY );

		// Plots a line segment of the given color
		//	_P0, _P1, the position of the line segment's points (i.e. X=0 is left border, X=Width-1 is right border, Y=0 is top border, Y=Height-1 is bottom border)
		void				DrawLine( const bfloat4& _color, const bfloat2& _P0, const bfloat2& _P1 );

		// Converts "ranged coordinates" into (X,Y) pixel coordinates
		void				RangedCoordinates2ImageCoordinates( const bfloat2& _rangeX, const bfloat2& _rangeY, const bfloat2& _rangedCoordinates, bfloat2& _imageCoordinates );
		// Converts (X,Y) pixel coordinates into "ranged coordinates"
		void				ImageCoordinates2RangedCoordinates( const bfloat2& _rangeX, const bfloat2& _rangeY, const bfloat2& _imageCoordinates, bfloat2& _rangedCoordinates );

	private:
		static U32						PixelFormat2BPP( PIXEL_FORMAT _pixelFormat );

		static FREE_IMAGE_TYPE			PixelFormat2FIT( PIXEL_FORMAT _pixelFormat );
		static PIXEL_FORMAT				Bitmap2PixelFormat( const FIBITMAP& _bitmap );

		static FILE_FORMAT				FIF2FileFormat( FREE_IMAGE_FORMAT _format )	{ return FILE_FORMAT( _format ); }
		static FREE_IMAGE_FORMAT		FileFormat2FIF( FILE_FORMAT _format )		{ return FREE_IMAGE_FORMAT( _format ); }

		void				ConvertFrom_NoSupport( const ImageFile& _source, PIXEL_FORMAT _targetFormat );

	private:	// Ref-counting for free image lib init/release
		static U32		ms_freeImageUsageRefCount;
		void				UseFreeImage();
		void				UnUseFreeImage();
	};

}	// namespace
