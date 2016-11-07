// LibRawManaged.h
// Managed version of the libraw library from http://www.libraw.org/download
//
// NOTE: If you're missing the static libraries found in ../LibRaw-0.16.0/lib/debug or /release
//		  you need to rebuild them using the VS2012 solution located in ../LibRaw-0.16.0
//
#pragma once

#pragma unmanaged
#include "libraw.h"
#pragma managed

using namespace System;

namespace LibRawManaged {

	public ref class RawFile
	{
	public:

		enum class	COLOR_PROFILE
		{
			sRGB,
			ADOBE_RGB
		};

	private:
		::LibRaw*		m_pLibRaw;

		// Decoded image data
		int				m_Width;
		int				m_Height;
		cli::array<cli::array<System::UInt16>^,2>^ 	m_Image;

		// Shot info
		float			m_ISOSpeed;
		float			m_Aperture;
		float			m_ShutterSpeed;
		float			m_FocalLength;

		// Color profile & info
		COLOR_PROFILE	m_ColorProfile;
		int				m_MaximumWhite;	// The maximum color value encoded in the RAW file

	public:

		property int				Width	{ int get() { return m_Width; } }
		property int				Height	{ int get() { return m_Height; } }
		property cli::array<cli::array<System::UInt16>^,2>^ Image	{ cli::array<cli::array<System::UInt16>^,2>^ get() { return m_Image; } }

		property float				ISOSpeed		{ float get() { return m_ISOSpeed; } }
		property float				Aperture		{ float get() { return m_Aperture; } }
		property float				ShutterSpeed	{ float get() { return m_ShutterSpeed; } }
		property float				FocalLength	{ float get() { return m_FocalLength; } }

		property COLOR_PROFILE		ColorProfile	{ COLOR_PROFILE get() { return m_ColorProfile; } }
		property int				Maximum			{ int get() { return m_MaximumWhite; } }

	public:
		RawFile() {
			m_pLibRaw = new ::LibRaw();
			m_Image = nullptr;
			m_ColorProfile = COLOR_PROFILE::sRGB;
		}

		~RawFile() {
			delete m_pLibRaw;
		}

		void	UnpackRAW( System::IO::Stream^ _Stream ) {
			if ( _Stream == nullptr )
				throw gcnew System::Exception( "Invalid image stream!" );

			// Read the file's content into a managed stream
			int	FileLength = (int) _Stream->Length;
			cli::array<System::Byte>^	ManagedFileContent = gcnew cli::array<System::Byte>( FileLength );
			_Stream->Read( ManagedFileContent, 0, FileLength );

			// Allocate memory to store the stream's content in an unmanaged buffer
			byte*	pBuffer = new byte[FileLength];
			System::Runtime::InteropServices::Marshal::Copy( ManagedFileContent, 0, System::IntPtr( pBuffer ), FileLength );

			// Open from memory
			if ( m_pLibRaw->open_buffer( pBuffer, FileLength ) != LIBRAW_SUCCESS )
				throw gcnew Exception( "Failed loading RAW file from memory" );

			// Let us unpack the image
			if ( m_pLibRaw->unpack() != LIBRAW_SUCCESS )
				throw gcnew Exception( "Failed unpacking RAW file" );

			// Convert from imgdata.rawdata to imgdata.image:
			m_pLibRaw->imgdata.params.user_qual = 10;
//			m_pLibRaw->imgdata.params.use_auto_wb = 1;
			m_pLibRaw->imgdata.params.use_camera_wb = 1;

//			m_pLibRaw->raw2image();
// 			if ( m_pLibRaw->raw2image_ex( true ) != LIBRAW_SUCCESS )
// 				throw gcnew Exception( "Failed converting RAW to image" );

 			if ( m_pLibRaw->dcraw_process() != LIBRAW_SUCCESS )
				throw gcnew Exception( "Failed processing image" );
			

			// Retrieve shot info
			m_ISOSpeed = m_pLibRaw->imgdata.other.iso_speed;
			m_Aperture = m_pLibRaw->imgdata.other.aperture;
			m_ShutterSpeed = m_pLibRaw->imgdata.other.shutter;
			m_FocalLength = m_pLibRaw->imgdata.other.focal_len;

			// Retrieve color info
			// TODO: retrieve profile
			m_MaximumWhite = m_pLibRaw->imgdata.color.maximum;


			// Write this as a readable RGB array
			m_Width = m_pLibRaw->imgdata.sizes.iwidth;
			m_Height = m_pLibRaw->imgdata.sizes.iheight;

			m_Image = gcnew cli::array<cli::array<System::UInt16>^,2>( m_Width, m_Height );

			for ( int Y=0; Y < m_Height; Y++ )
			{
				for ( int X=0; X < m_Width; X++ )
				{
					cli::array<System::UInt16>^	Pixel = gcnew cli::array<System::UInt16>( 4 );
					m_Image[X,Y] = Pixel;

					Pixel[0] = m_pLibRaw->imgdata.image[m_Width*Y+X][0];
					Pixel[1] = m_pLibRaw->imgdata.image[m_Width*Y+X][1];
					Pixel[2] = m_pLibRaw->imgdata.image[m_Width*Y+X][2];
					Pixel[3] = m_pLibRaw->imgdata.image[m_Width*Y+X][3];
				}
			}

			// Get ready for the next image
			m_pLibRaw->recycle();
		}
	};
}
