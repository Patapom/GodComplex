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

	public ref class ImageFile {
	public:
		#pragma region NESTED TYPES

		// The delegate used to tone map an HDR image into a LDR color (warning: any returned value above 1 will be clamped!)
		delegate void	ToneMapper( float3 _HDRColor, float3% _LDRColor );

		// This enum matches the classes available in PixelFormat.h (which in turn match the DXGI formats)
		enum class PIXEL_FORMAT {
			UNKNOWN,

			// 8-bits
			R8,
			RG8,
			RGB8,
			RGBA8,

			// 16-bits
			R16,
//			RG16,		// Unsupported
			RGB16,
			RGBA16,
//			R16F,		// Unsupported
// 			RG16F,		// Unsupported
// 			RGB16F,		// Unsupported
// 			RGBA16F,	// Unsupported

			// 32-bits
			R32F,
			RG32F,
			RGB32F,
			RGBA32F,
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
			// TODO!
		};

		#pragma endregion

	internal:
		ImageUtilityLib::ImageFile*		m_nativeObject;

		// Special wrapper constructor
		ImageFile( ImageUtilityLib::ImageFile& _nativeObject ) {
			m_nativeObject = &_nativeObject;
		}

	public:
		#pragma region PROPERTIES

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
				return Metadata->ColorProfile;
			}
		}

		// Builds a System.Drawing.Bitmap from the image
		// Warning: throws an exception if your image format is HDR! (cf. ToneMapFrom())
		property System::Drawing::Bitmap^	AsBitmap {
			System::Drawing::Bitmap^	get();
		}

		#pragma endregion

	public:

		ImageFile() {
			m_nativeObject = new ImageUtilityLib::ImageFile();
		}
		ImageFile( System::IO::FileInfo^ _fileName, FILE_FORMAT _format ) {
			Load( _fileName, _format );
		}
		ImageFile( NativeByteArray^ _fileContent, FILE_FORMAT _format ) {
			Load( _fileContent, _format );
		}
		ImageFile( U32 _width, U32 _height, PIXEL_FORMAT _format, ImageUtility::ColorProfile^ _colorProfile ) {
			Init( _width, _height, _format, _colorProfile );
		}
		ImageFile( ImageFile^ _other ) {
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

		// Retrieves the image file type based on the image file name
		static FILE_FORMAT	GetFileType( System::IO::FileInfo^ _fileName );

		// Loads a System::Drawing.Bitmap into a byte[] containing RGBARGBARG... pixels
		// <param name="_Bitmap">The source System::Drawing.Bitmap to load</param>
		// <param name="_Width">The bitmap's width</param>
		// <param name="_Height">The bitmaps's height</param>
		// <returns>The byte array containing a sequence of R,G,B,A,R,G,B,A pixels and of length Widht*Height*4</returns>
		static cli::array< Byte >^	LoadBitmap( System::Drawing::Bitmap^ _bitmap, int& _width, int& _height ) {
			_width = _bitmap->Width;
			_height = _bitmap->Height;

			cli::array< System::Byte >^	result = gcnew cli::array< System::Byte >( 4*_width*_height );

			System::Drawing::Imaging::BitmapData^	lockedBitmap = _bitmap->LockBits( System::Drawing::Rectangle( 0, 0, _width, _height ), System::Drawing::Imaging::ImageLockMode::ReadOnly, System::Drawing::Imaging::PixelFormat::Format32bppArgb );

			Byte	R, G, B, A;
			int		targetIndex = 0;
			for ( int Y=0; Y < _height; Y++ ) {
				pin_ptr<Byte>	pScanline = (Byte*) lockedBitmap->Scan0.ToPointer() + Y * lockedBitmap->Stride;
				for ( int X=0; X < _width; X++ ) {
					// Read in shitty order
					B = *pScanline++;
					G = *pScanline++;
					R = *pScanline++;
					A = *pScanline++;

					// Write in correct order
					result[targetIndex++] = R;
					result[targetIndex++] = G;
					result[targetIndex++] = B;
					result[targetIndex++] = A;
				}
			}

			_bitmap->UnlockBits( lockedBitmap );

			return result;
		}

	public:
		//////////////////////////////////////////////////////////////////////////
		// DDS-related methods
		enum class COMPRESSION_TYPE {
			NONE,
			BC4,
			BC5,
			BC6H,
			BC7,
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
