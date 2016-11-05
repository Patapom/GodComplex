#pragma once

#pragma unmanaged
#include "..\ImageUtilityLib\ImageFile.h"
#pragma managed

#include "ColorProfile.h"

using namespace System;

namespace ImageUtility {

	public ref class ImageFile {
	public:
		#pragma region NESTED TYPES

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

		};

		#pragma endregion

	internal:
		ImageUtilityLib::ImageFile*		m_nativeObject;

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

// 		// Gets the image's metadata (i.e. ISO, Tv, Av, focal length, etc.)
// 		const MetaData&		GetMetadata() const		{ return m_metadata; }

		// Gets the color profile associated to the image
		property ImageUtility::ColorProfile^	ColorProfile {
			ImageUtility::ColorProfile^	get() { return gcnew ImageUtility::ColorProfile( m_nativeObject->GetColorProfile() ); }
		}

		#pragma endregion

	public:

		ImageFile() : m_nativeObject( nullptr ) {
		}
		ImageFile( System::IO::FileInfo^ _fileName, FILE_FORMAT _format ) {
			Load( _fileName, _format );
		}
		ImageFile( cli::array< Byte >^ _fileContent, FILE_FORMAT _format ) {
			Load( _fileContent, _format );
		}
		ImageFile( U32 _width, U32 _height, PIXEL_FORMAT _format, ImageUtility::ColorProfile^ _colorProfile ) {
			Init( _width, _height, _format, _colorProfile );
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
		void				Load( cli::array< Byte >^ _fileContent, FILE_FORMAT _format );

		// Save to a file or memory
		void				Save( System::IO::FileInfo^ _fileName );
		void				Save( System::IO::FileInfo^ _fileName, FILE_FORMAT _format );
		void				Save( System::IO::FileInfo^ _fileName, FILE_FORMAT _format, SAVE_FLAGS _options );
		void				Save( System::IO::Stream^ _imageStream, FILE_FORMAT _format, SAVE_FLAGS _options );
		cli::array< Byte >^	Save( FILE_FORMAT _format, SAVE_FLAGS _options );
		
		// Converts the source image to a target format
		void				ConvertFrom( ImageFile^ _source, PIXEL_FORMAT _targetFormat );

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
			Byte*				pScanline;
			Byte				R, G, B, A;

			System::Drawing::Imaging::BitmapData^	lockedBitmap = _bitmap->LockBits( System::Drawing::Rectangle( 0, 0, _width, _height ), System::Drawing::Imaging::ImageLockMode::ReadOnly, System::Drawing::Imaging::PixelFormat::Format32bppArgb );

			int	targetIndex = 0;
			for ( int Y=0; Y < _height; Y++ ) {
				pScanline = (Byte*) lockedBitmap->Scan0.ToPointer() + Y * lockedBitmap->Stride;
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
	};
}
