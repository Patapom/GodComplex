#include "stdafx.h"
#include "ImagesMatrix.h"

using namespace ImageUtilityLib;
using namespace BaseLib;

ImagesMatrix::ImagesMatrix()
	: m_type( TYPE::GENERIC )
	, m_format( ImageFile::PIXEL_FORMAT::UNKNOWN ) {
}
ImagesMatrix::~ImagesMatrix() {
	ReleaseImageFiles();
}

void	ImagesMatrix::InitTexture2DArray( U32 _width, U32 _height, U32 _arraySize, U32 _mipLevelsCount ) {
	m_type = ImagesMatrix::TYPE::TEXTURE2D;
	m_mipsArray.SetCount( _arraySize );
	for ( U32 arraySliceIndex=0; arraySliceIndex < _arraySize; arraySliceIndex++ ) {
		Mips&	sliceMips = m_mipsArray[arraySliceIndex];
		sliceMips.Init( _mipLevelsCount );

		U32	W = _width;
		U32	H = _height;
		for ( U32 mipLevelIndex=0; mipLevelIndex < _mipLevelsCount; mipLevelIndex++ ) {
			Mips::Mip&	mip = sliceMips[mipLevelIndex];
			mip.Init( W, H, 1 );	// Each mip in a texture 2D array has only 1 slice
			NextMipSize( W, H );
		}
	}
}
void	ImagesMatrix::InitCubeTextureArray( U32 _cubeMapSize, U32 _cubeMapsCount, U32 _mipLevelsCount ) {
	U32	cubeFacesCount = 6 * _cubeMapsCount;
	m_type = ImagesMatrix::TYPE::TEXTURECUBE;
	m_mipsArray.SetCount( cubeFacesCount );
	for ( U32 arraySliceIndex=0; arraySliceIndex < cubeFacesCount; arraySliceIndex++ ) {
		Mips&	sliceMips = m_mipsArray[arraySliceIndex];
		sliceMips.Init( _mipLevelsCount );

		U32	S = _cubeMapSize;
		for ( U32 mipLevelIndex=0; mipLevelIndex < _mipLevelsCount; mipLevelIndex++ ) {
			Mips::Mip&	mip = sliceMips[mipLevelIndex];
			mip.Init( S, S, 1 );	// Each mip in a texture 2D array has only 1 slice
			NextMipSize( S );
		}
	}
}

void	ImagesMatrix::InitTexture3D( U32 _width, U32 _height, U32 _depth, U32 _mipLevelsCount ) {
	m_type = ImagesMatrix::TYPE::TEXTURE3D;
	m_mipsArray.SetCount( 1 );
	Mips&	mips = m_mipsArray[0];
	mips.Init( _mipLevelsCount );

	U32	W = _width;
	U32	H = _height;
	U32	D = _depth;
	for ( U32 mipLevelIndex=0; mipLevelIndex < _mipLevelsCount; mipLevelIndex++ ) {
		Mips::Mip&	mip = mips[mipLevelIndex];
		mip.Init( W, H, D );	// Each mip in a texture 3D has many slices to cover the size of the reduced 3D texture
		NextMipSize( W, H, D );
	}
}

void	ImagesMatrix::InitTextureGeneric( U32 _width, U32 _height, U32 _depth, U32 _arraySize, U32 _mipLevelsCount ) {
	m_type = ImagesMatrix::TYPE::GENERIC;
	m_mipsArray.SetCount( _arraySize );
	for ( U32 arraySliceIndex=0; arraySliceIndex < _arraySize; arraySliceIndex++ ) {
		Mips&	sliceMips = m_mipsArray[arraySliceIndex];
		sliceMips.Init( _mipLevelsCount );

		U32	W = _width;
		U32	H = _height;
		U32	D = _depth;
		for ( U32 mipLevelIndex=0; mipLevelIndex < _mipLevelsCount; mipLevelIndex++ ) {
			Mips::Mip&	mip = sliceMips[mipLevelIndex];
			mip.Init( W, H, D );
			NextMipSize( W, H, D );
		}
	}
}

void	ImagesMatrix::AllocateImageFiles( ImageFile::PIXEL_FORMAT _format, const ColorProfile& _colorProfile ) {
	ReleaseImageFiles();	// Release first

	m_format = _format;
	m_colorProfile = _colorProfile;

	for ( U32 i=0; i < m_mipsArray.Count(); i++ ) {
		m_mipsArray[i].AllocateImageFiles( _format, _colorProfile );
	}
}

void	ImagesMatrix::ReleaseImageFiles() {
	for ( U32 i=0; i < m_mipsArray.Count(); i++ ) {
		m_mipsArray[i].ReleaseImageFiles();
	}

	m_format = ImageFile::PIXEL_FORMAT::UNKNOWN;
}

void	ImagesMatrix::Mips::Init( U32 _mipLevelsCount ) {
	m_mips.SetCount( _mipLevelsCount );
}

void	ImagesMatrix::Mips::AllocateImageFiles( ImageFile::PIXEL_FORMAT _format, const ColorProfile& _colorProfile ) {
	ReleaseImageFiles();	// Release first
	for ( U32 i=0; i < m_mips.Count(); i++ ) {
		m_mips[i].AllocateImageFiles( _format, _colorProfile );
	}
}

void	ImagesMatrix::Mips::ReleaseImageFiles() {
	for ( U32 i=0; i < m_mips.Count(); i++ ) {
		m_mips[i].ReleaseImageFiles();
	}
}

void	ImagesMatrix::Mips::Mip::Init( U32 _width, U32 _height, U32 _depth ) {
	m_width = _width;
	m_height = _height;
	m_images.SetCount( _depth );
	memset( m_images.Ptr(), 0, _depth*sizeof(ImageFile*) );
}

void	ImagesMatrix::Mips::Mip::AllocateImageFiles( ImageFile::PIXEL_FORMAT _format, const ColorProfile& _colorProfile ) {
	ReleaseImageFiles();	// Release first
	for ( U32 i=0; i < m_images.Count(); i++ ) {
		ImageFile*	imageSlice = new ImageFile( m_width, m_height, _format, _colorProfile );
		m_images[i] = imageSlice;
	}
}

void	ImagesMatrix::Mips::Mip::ReleaseImageFiles() {
	for ( U32 i=0; i < m_images.Count(); i++ ) {
		SAFE_DELETE( m_images[i] );
	}
}

void	ImagesMatrix::NextMipSize( U32& _size ) {
	_size = (1+_size) >> 1;
}
void	ImagesMatrix::NextMipSize( U32& _width, U32& _height ) {
	NextMipSize( _width );
	NextMipSize( _height );
}
void	ImagesMatrix::NextMipSize( U32& _width, U32& _height, U32& _depth ) {
	NextMipSize( _width, _height );
	_depth = (1+_depth) >> 1;
}


/*
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

	delete scanlineMip;
	delete scanline1;
	delete scanline0;
}
*/
