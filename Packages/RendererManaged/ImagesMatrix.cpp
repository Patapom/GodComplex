// This is the main DLL file.

#include "stdafx.h"

#include "ImagesMatrix.h"
//#include "..\ImageUtilityLib\ColorProfile.h"

using namespace SharpMath;
using namespace ImageUtility;

namespace Renderer {

	void ImagesMatrix::default::set( UInt32 _index, Mips^ value ) {
		if ( _index >= UInt32(m_mipsArray->Length) ) throw gcnew Exception( "Array index out of range!" );
		if ( value != nullptr && value->MipLevelsCount != m_mipLevelsCount ) throw gcnew Exception( "Provided mips count and matrix mips count mismatch!" );
		m_mipsArray[_index] = value;
	}

	ImagesMatrix::ImagesMatrix( UInt32 _width, UInt32 _height, UInt32 _arraySize, UInt32 _mipLevelsCount ) {
		m_width = _width;
		m_height = _height;
		m_mipLevelsCount = _mipLevelsCount;
		m_isCubeMap = false;
		m_mipsArray = gcnew array< Mips^ >( _arraySize );
		for ( UInt32 arrayIndex=0; arrayIndex < _arraySize; arrayIndex++ ) {
			m_mipsArray[arrayIndex] = gcnew Mips( m_mipLevelsCount );
		}
	}
	ImagesMatrix::ImagesMatrix( array< ImageUtility::ImageFile^ >^ _mips0, UInt32 _mipLevelsCount ) {
		if ( _mips0 == nullptr ) throw gcnew Exception( "Invalid mips 0 array!" );
		if ( _mips0->Length == 0 ) throw gcnew Exception( "Mips 0 array is empty!" );

		m_width = _mips0[0]->Width;
		m_height = _mips0[0]->Height;
		m_mipLevelsCount = _mipLevelsCount;
		m_isCubeMap = false;
		m_mipsArray = gcnew array< Mips^ >( _mips0->Length );
		for ( UInt32 arrayIndex=0; arrayIndex < UInt32(_mips0->Length); arrayIndex++ ) {
			ImageFile^	mip0 = _mips0[arrayIndex];
			if ( mip0->Width != m_width ) throw gcnew Exception( "Mip 0 image at index #" + arrayIndex + " has a different width!" );
			if ( mip0->Height != m_height ) throw gcnew Exception( "Mip 0 image at index #" + arrayIndex + " has a different height!" );
			m_mipsArray[arrayIndex] = gcnew Mips( mip0, m_mipLevelsCount );
		}
	}

	void	ImagesMatrix::NextMipSize( UInt32% _width, UInt32% _height ) {
		_width = (1+_width) >> 1;
		_height = (1+_height) >> 1;
	}

	//////////////////////////////////////////////////////////////////////////
	// Mips Class
	void ImagesMatrix::Mips::default::set( UInt32 _index, ImageFile^ value ) {
		m_images[_index] = value;
	}

	ImagesMatrix::Mips::Mips( UInt32 _mipLevelsCount ) {
		if ( _mipLevelsCount == 0 ) throw gcnew Exception( "Mip levels count cannot be 0!" );
		m_images = gcnew array< ImageFile^ >( _mipLevelsCount );
	}
	ImagesMatrix::Mips::Mips( ImageUtility::ImageFile^ _mip0, UInt32 _mipLevelsCount ) {
		if ( _mipLevelsCount == 0 ) throw gcnew Exception( "Mip levels count cannot be 0!" );
		m_images = gcnew array< ImageFile^ >( _mipLevelsCount );
		BuildMips( _mip0 );
	}
	ImagesMatrix::Mips::Mips( array< ImageUtility::ImageFile^ >^ _mipLevels ) {
		if ( _mipLevels == nullptr ) throw gcnew Exception( "Invalid mip levels!" );
		if ( _mipLevels->Length == 0 ) throw gcnew Exception( "Mip levels count cannot be 0!" );
		m_images = _mipLevels;
	}

	void	ImagesMatrix::Mips::BuildMips( ImageUtility::ImageFile^ _mip0 ) {
		m_images[0] = _mip0;
		if ( m_images->Length == 1 )
			return;	// Nothing to build...

		UInt32					mipLevelsCount = m_images->Length;
		UInt32					W = _mip0->Width;
		UInt32					H = _mip0->Height;
		ImageFile::PIXEL_FORMAT	format = _mip0->PixelFormat;
		ImageFile^				currentMip = _mip0;
		ColorProfile^			profile = _mip0->ColorProfile;
//		ImageUtilityLib::ColorProfile&	nativeProfile = profile->NativeObject;

//		bool	sRGB = _mip0->ColorProfile->GammaCurve == ColorProfile::GAMMA_CURVE::sRGB;

		UInt32	W2 = (W+1) & ~1U;	// Ensure an even number of pixels
		array< float4 >^	scanline0 = gcnew array<float4>( W2 );
		array< float4 >^	scanline1 = gcnew array<float4>( W2 );
		array< float4 >^	scanlineMip = gcnew array<float4>( W2 );
		float4	V00, V01, V10, V11, V;

		for ( UInt32 mipLevelIndex=1; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
			UInt32	prevW = W;
			UInt32	prevH = H;
			NextMipSize( W, H );

			ImageFile^	prevMip = currentMip;
			currentMip = gcnew ImageFile( W, H, format, profile );

			for ( UInt32 Y=0; Y < H; Y++ ) {
				prevMip->ReadScanline( 2*Y+0, scanline0 );
				prevMip->ReadScanline( MIN( prevH-1, 2*Y+1 ), scanline1 );	// Duplicate end scanline if odd height
				if ( prevW & 1 ) {
					// Duplicate end pixels if odd width
					scanline0[prevW] = scanline0[prevW-1];
					scanline1[prevW] = scanline1[prevW-1];
				}

				UInt32	prevX = 0;
				for ( UInt32 X=0; X < W; X++ ) {
					// Read 4 pixel values in linear space
					profile->GammaRGB2LinearRGB( scanline0[prevX], V00 );
					profile->GammaRGB2LinearRGB( scanline1[prevX], V10 );
					prevX++;
					profile->GammaRGB2LinearRGB( scanline0[prevX], V01 );
					profile->GammaRGB2LinearRGB( scanline1[prevX], V11 );

					// Average and convert back to gamma space
					V = 0.25f * (V00 + V01 + V10 + V11);
					profile->LinearRGB2GammaRGB( scanlineMip[X], V );
				}

				currentMip->WriteScanline( Y, scanlineMip );
			}
		}

		delete[] scanlineMip;
		delete[] scanline1;
		delete[] scanline0;
	}
}
