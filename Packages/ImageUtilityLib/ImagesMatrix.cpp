#include "stdafx.h"
#include "ImagesMatrix.h"
#include <d3d11.h>

using namespace ImageUtilityLib;
using namespace BaseLib;

ImagesMatrix::ImagesMatrix()
	: m_type( TYPE::GENERIC )
	, m_format( PIXEL_FORMAT::UNKNOWN ) {
}
ImagesMatrix::~ImagesMatrix() {
	ReleasePointers();
}

void	ImagesMatrix::InitTexture2DArray( U32 _width, U32 _height, U32 _arraySize, U32 _mipLevelsCount ) {
	RELEASE_ASSERT( _width > 0 && _height > 0, "Invalid texture size!" );
	RELEASE_ASSERT( _arraySize > 0, "Invalid texture array size!" );
	if ( _mipLevelsCount == 0 )
		_mipLevelsCount = ComputeMipsCount( MAX( _width, _height ) );

	ReleasePointers();

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
	RELEASE_ASSERT( _cubeMapSize > 0, "Invalid texture size!" );
	RELEASE_ASSERT( _cubeMapsCount > 0, "Invalid amount of cube maps!" );
	if ( _mipLevelsCount == 0 )
		_mipLevelsCount = ComputeMipsCount( _cubeMapSize );

	ReleasePointers();

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
	RELEASE_ASSERT( _width > 0 && _height > 0 && _depth > 0, "Invalid texture size!" );
	if ( _mipLevelsCount == 0 )
		_mipLevelsCount = ComputeMipsCount( MAX( MAX( _width, _height ), _depth ) );

	ReleasePointers();

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

void	ImagesMatrix::AllocateImageFiles( PIXEL_FORMAT _format, const ColorProfile& _colorProfile ) {
//	ReleasePointers();	// Release first <= NOPE! The user may have already filled some components of the matrix!

	m_format = _format;
	m_colorProfile = _colorProfile;

	for ( U32 i=0; i < m_mipsArray.Count(); i++ ) {
		m_mipsArray[i].AllocateImageFiles( _format, _colorProfile );
	}
}

void	ImagesMatrix::AllocateRawBuffers( PIXEL_FORMAT _format, const GetRawBufferSizeFunctor& _getRawBufferSizeDelegate ) {
	m_format = _format;
	RELEASE_ASSERT( (U32(m_format) & U32(PIXEL_FORMAT::RAW_BUFFER)) != 0, "The specified format does NOT have the RAW_BUFFER flag!" );

	U32	rowPitch, depthPitch;
	for ( U32 sliceIndex=0; sliceIndex < m_mipsArray.Count(); sliceIndex++ ) {
		Mips&	mips = m_mipsArray[sliceIndex];
		for ( U32 mipLevelIndex=0; mipLevelIndex < mips.GetMipLevelsCount(); mipLevelIndex++ ) {
			Mips::Mip&	mip = mips[mipLevelIndex];

			// Ask the raw buffer info for that mip
			const U8*	sourceBuffer = _getRawBufferSizeDelegate( sliceIndex, mipLevelIndex, rowPitch, depthPitch );

			// Actual allocation
			mip.AllocateRawBuffer( rowPitch, depthPitch, sourceBuffer );
		}
	}
}

void	ImagesMatrix::ReleasePointers() {
	for ( U32 i=0; i < m_mipsArray.Count(); i++ ) {
		m_mipsArray[i].ReleasePointers();
	}

	m_format = PIXEL_FORMAT::UNKNOWN;
}
void	ImagesMatrix::ClearPointers() {
	for ( U32 i=0; i < m_mipsArray.Count(); i++ ) {
		m_mipsArray[i].ClearPointers();
	}

	m_format = PIXEL_FORMAT::UNKNOWN;
}

void	ImagesMatrix::ConvertFrom( const ImagesMatrix& _source, PIXEL_FORMAT _targetFormat, const ColorProfile& _colorProfile ) {
	ReleasePointers();

	// Initialize like the source matrix
	const Mips::Mip&	sourceRefMip = _source[0][0];
	U32					mipLevelsCount = _source[0].GetMipLevelsCount();

	switch ( _source.m_type ) {
	case ImagesMatrix::TYPE::TEXTURE2D:
		InitTexture2DArray( sourceRefMip.Width(), sourceRefMip.Height(), _source.GetArraySize(), mipLevelsCount );
		break;
	case ImagesMatrix::TYPE::TEXTURE3D:
		InitTexture3D( sourceRefMip.Width(), sourceRefMip.Height(), sourceRefMip.Depth(), mipLevelsCount );
		break;
	case ImagesMatrix::TYPE::TEXTURECUBE:
		InitCubeTextureArray( sourceRefMip.Width(), _source.GetArraySize(), mipLevelsCount );
		break;
	case ImagesMatrix::TYPE::GENERIC:
		InitTextureGeneric( sourceRefMip.Width(), sourceRefMip.Height(), sourceRefMip.Depth(), _source.GetArraySize(), mipLevelsCount );
		break;
	}

	// Convert each image
	for ( U32 arrayIndex=0; arrayIndex < m_mipsArray.Count(); arrayIndex++ ) {
		const Mips&	sourceMips = _source[arrayIndex];
		Mips&		targetMips = m_mipsArray[arrayIndex];
		for ( U32 mipLevelIndex=0; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
			const Mips::Mip&	sourceMip = sourceMips[mipLevelIndex];
			Mips::Mip&			targetMip = targetMips[mipLevelIndex];

			for ( U32 Z=0; Z < targetMip.Depth(); Z++ ) {
				const ImageFile*	sourceImage = sourceMip[Z];
				RELEASE_ASSERT( sourceImage != NULL, "Invalid source image to convert from!" );
				ImageFile*&			targetImage = targetMip[Z];
				targetImage = new ImageFile();
				targetImage->ConvertFrom( *sourceImage, _targetFormat );
			}
		}
	}

	m_format = _targetFormat;
	m_colorProfile = _colorProfile;
}


class CompressedImagesCopier : public ImagesMatrix::GetRawBufferSizeFunctor {
	virtual const U8*	operator()( U32 _arraySliceIndex, U32 _mipLevelIndex, U32& _rowPitch, U32& _slicePitch ) const override {
		ImagesMatrix::Mips::Mip&	targetMip = m_targetMatrix[_arraySliceIndex][_mipLevelIndex];

		U32	W = targetMip.Width();
		U32	H = targetMip.Height();
		U32	D = targetMip.Depth();

		const DirectX::Image&	referenceSourceImage = *m_sourceImages.GetImage( _mipLevelIndex, _arraySliceIndex, 0 );
		_rowPitch = U32( referenceSourceImage.rowPitch );
		_slicePitch = U32( referenceSourceImage.slicePitch );

		return referenceSourceImage.pixels;	// DirectXTex stores the pixels into a contiguous buffer so it's okay to only give the first slice's pointer here

// 		U8*		targetPixels = new U8[_slicePitch * D];
// 		for ( U32 Z=0; Z < D; Z++ ) {
// 			const DirectX::Image&	sourceImage = *m_sourceImages.GetImage( _mipLevelIndex, _arraySliceIndex, Z );
// 			U8*						targetSlice = targetPixels + Z * _slicePitch;
// 			memcpy_s( targetSlice, _slicePitch, sourceImage.pixels, _slicePitch );
// 		}
// 
// 		return targetPixels;
	}
	const DirectX::ScratchImage&	m_sourceImages;
	ImagesMatrix&					m_targetMatrix;
public:
	CompressedImagesCopier( const DirectX::ScratchImage& _sourceImages, ImagesMatrix& _targetMatrix ) : m_sourceImages( _sourceImages ), m_targetMatrix( _targetMatrix ) {}
};

void	ImagesMatrix::DDSCompress( const ImagesMatrix& _source, COMPRESSION_TYPE _compressionType, COMPONENT_FORMAT _componentFormat, void* _blindPointerDevice ) {
	if ( (U32(_source.m_format) & U32(PIXEL_FORMAT::RAW_BUFFER)) != 0 )
		throw "Unsupported raw buffer source pixel format: the source images must be of a valid pixel type to be compressed!";

	// Ensure compression is possible
	DXGI_FORMAT	sourceFormat = PixelFormat2DXGIFormat( _source.m_format, _componentFormat );
	if ( sourceFormat == DXGI_FORMAT_UNKNOWN )
		throw "Unsupported source format and/or component format!";

	DXGI_FORMAT	targetFormat = CompressionType2DXGIFormat( _compressionType, _componentFormat );
	if ( targetFormat == DXGI_FORMAT_UNKNOWN )
		throw "Unsupported target format and/or component format!";

	// Retrieve texture dimensions
	U32			arraySize = _source.m_mipsArray.Count();
	RELEASE_ASSERT( arraySize > 0, "Invalid source array size!" );

	const Mips&	sourceReferenceMips = _source[0];
	U32			mipLevelsCount = sourceReferenceMips.GetMipLevelsCount();
	RELEASE_ASSERT( mipLevelsCount > 0, "Invalid source mip levels count!" );

	const Mips::Mip&	sourceReferenceMip = sourceReferenceMips[0];
	U32			W = sourceReferenceMip.Width();
	U32			H = sourceReferenceMip.Height();
	U32			D = sourceReferenceMip.Depth();

	// =============================================================
	// Create & fill source scratch image
	DirectX::ScratchImage	sourceImagesContainer;
	if ( m_type != ImagesMatrix::TYPE::TEXTURE3D ) {
		sourceImagesContainer.Initialize2D( sourceFormat, W, H, arraySize, mipLevelsCount );
	} else {
		sourceImagesContainer.Initialize3D( sourceFormat, W, H, D, mipLevelsCount );
	}

	for ( U32 arrayIndex=0; arrayIndex < arraySize; arrayIndex++ ) {
		const Mips&	sourceMips = _source[arrayIndex];
		for ( U32 mipLevelIndex=0; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
			const Mips::Mip&	sourceMip = sourceMips[mipLevelIndex];
			for ( U32 Z=0; Z < D; Z++ ) {
				const ImageFile*	sourceImage = sourceMip[Z];
				if ( sourceImage == NULL )
					throw "Invalid image: the ImagesMatrix must be allocated with valid image files before compression!";

				const DirectX::Image&	sourceDXImage = *sourceImagesContainer.GetImage( mipLevelIndex, arrayIndex, Z );

				// Copy image slice
				const U8*	sourcePixels = sourceImage->GetBits();
				U32			sourceRowPitch = sourceImage->Pitch();
				U8*			targetPixels = sourceDXImage.pixels;
				U32			targetRowPitch = U32( sourceDXImage.rowPitch );
				for ( U32 Y=0; Y < H; Y++ ) {
					const U8*	scanlineSource = sourcePixels + Y * sourceRowPitch;
					U8*			scanlineTarget = targetPixels + Y * targetRowPitch;
					memcpy_s( scanlineTarget, targetRowPitch, scanlineSource, sourceRowPitch );
				}
			}
		}
	}

	// =============================================================
	// Perform actual compression
	DirectX::TEX_COMPRESS_FLAGS	flags = DirectX::TEX_COMPRESS_PARALLEL;
	DirectX::ScratchImage		targetImagesContainer;
	ID3D11Device*				device = reinterpret_cast< ID3D11Device* >( _blindPointerDevice );

	HRESULT	hr = device != NULL ? DirectX::Compress( device, sourceImagesContainer.GetImages(), sourceImagesContainer.GetImageCount(), sourceImagesContainer.GetMetadata(), targetFormat, targetImagesContainer )
								: DirectX::Compress( sourceImagesContainer.GetImages(), sourceImagesContainer.GetImageCount(), sourceImagesContainer.GetMetadata(), targetFormat, flags, 0.5f, targetImagesContainer );
	if ( hr != S_OK )
		throw "Compression failed while using DirectXTex...";

	// =============================================================
	// Write back compressed buffers

	// Generic allocate, but overwrite type
	InitTextureGeneric( W, H, D, arraySize, mipLevelsCount );
	m_type = _source.m_type;

	// Allocate and copy compressed buffers
	COMPONENT_FORMAT	targetComponentFormat;
	U32					pixelSize;
	PIXEL_FORMAT		format = DXGIFormat2PixelFormat( targetFormat, targetComponentFormat, pixelSize );
	ASSERT( targetComponentFormat == _componentFormat, "Component formats mismatch!" );	// Routine check that's quite useless after all since we chose the component format ourselves so obviously it should be equal to what we chose... :/

	CompressedImagesCopier	 compressor( sourceImagesContainer, *this );
	AllocateRawBuffers( format, compressor );
}

// void	ImagesMatrix::DDSCompress( COMPRESSION_TYPE _compressionType, COMPONENT_FORMAT _componentFormat, DXGI_FORMAT& _targetFormat, U32& _rowPitch, U32& _slicePitch, U8*& _compressedRawBuffer ) const {
// 
// 	DXGI_FORMAT	sourceFormat = PixelFormat2DXGIFormat( m_pixelFormat, _componentFormat );
// 	if ( sourceFormat == DXGI_FORMAT_UNKNOWN )
// 		throw "Unsupported source format and/or component format!";
// 
// 	_targetFormat = CompressionType2DXGIFormat( _compressionType, _componentFormat );
// 	if ( _targetFormat == DXGI_FORMAT_UNKNOWN )
// 		throw "Unsupported target format and/or component format!";
// 
// 	// Create temporary DirectXTex container and copy our image into it
// 	DirectX::ScratchImage		sourceImagesContainer;
// 	sourceImagesContainer.Initialize2D( sourceFormat, Width(), Height(), 1, 1 );
// 	const DirectX::Image&		imageSource = *sourceImagesContainer.GetImage( 0, 0, 0 );
// 	Copy( *this, imageSource );
// 
// 	// Compress
// 	DirectX::TEX_COMPRESS_FLAGS	flags = DirectX::TEX_COMPRESS_PARALLEL;
// 	DirectX::ScratchImage		targetImagesContainer;
// 	HRESULT	hr = DirectX::Compress( imageSource, _targetFormat, flags, 0.5f, targetImagesContainer );
// 	if ( hr != S_OK )
// 		throw "Compression failed while using DirectXTex...";
// 
// 	// Copy back into compressed buffer
// 	const DirectX::Image&		imageTarget = *targetImagesContainer.GetImage( 0, 0, 0 );
// 	_rowPitch = U32(imageTarget.rowPitch);
// 	_slicePitch = U32(imageTarget.slicePitch);
// 	_compressedRawBuffer = new U8[_slicePitch];
// 	memcpy_s( _compressedRawBuffer, _slicePitch, imageTarget.pixels, _slicePitch );
// }

DXGI_FORMAT	ImagesMatrix::CompressionType2DXGIFormat( COMPRESSION_TYPE _compressionType, COMPONENT_FORMAT _componentFormat ) {
	switch ( _compressionType ) {
	case COMPRESSION_TYPE::BC4:
		switch ( _componentFormat ) {
		case COMPONENT_FORMAT::AUTO:
		case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_BC4_UNORM;
		case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_BC4_SNORM;
		}

	case COMPRESSION_TYPE::BC5:
		switch ( _componentFormat ) {
		case COMPONENT_FORMAT::AUTO:
		case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_BC5_UNORM;
		case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_BC5_SNORM;
		}

	case COMPRESSION_TYPE::BC6H:
		switch ( _componentFormat ) {
		case COMPONENT_FORMAT::AUTO:
		case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_BC6H_UF16;
		case COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_BC6H_SF16;
		}

	case COMPRESSION_TYPE::BC7:
		switch ( _componentFormat ) {
		case COMPONENT_FORMAT::AUTO:
		case COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_BC7_UNORM;
		case COMPONENT_FORMAT::UNORM_sRGB:	return DXGI_FORMAT_BC7_UNORM_SRGB;
		}
	}

	return DXGI_FORMAT_UNKNOWN;
}

//////////////////////////////////////////////////////////////////////////
// Mips class
//
void	ImagesMatrix::Mips::Init( U32 _mipLevelsCount ) {
	m_mips.SetCount( _mipLevelsCount );
}

void	ImagesMatrix::Mips::AllocateImageFiles( PIXEL_FORMAT _format, const ColorProfile& _colorProfile ) {
//	ReleasePointers();	// Release first <= DON'T! User may already have provided some images as valid components of the matrix, we just need to allocate empty slots!
	for ( U32 i=0; i < m_mips.Count(); i++ ) {
		m_mips[i].AllocateImageFiles( _format, _colorProfile );
	}
}

void	ImagesMatrix::Mips::ReleasePointers() {
	for ( U32 i=0; i < m_mips.Count(); i++ ) {
		m_mips[i].ReleasePointers();
	}
}
void	ImagesMatrix::Mips::ClearPointers() {
	for ( U32 i=0; i < m_mips.Count(); i++ ) {
		m_mips[i].ClearPointers();
	}
}

void	ImagesMatrix::Mips::Mip::Init( U32 _width, U32 _height, U32 _depth ) {
	m_width = _width;
	m_height = _height;
	m_images.SetCount( _depth );
	memset( m_images.Ptr(), 0, _depth*sizeof(ImageFile*) );
}

void	ImagesMatrix::Mips::Mip::AllocateImageFiles( PIXEL_FORMAT _format, const ColorProfile& _colorProfile ) {
//	ReleasePointers();	// Release first <= DON'T! User may already have provided some images as valid components of the matrix, we just need to allocate empty slots!
	for ( U32 i=0; i < m_images.Count(); i++ ) {
		if ( m_images[i] == NULL ) {
			ImageFile*	imageSlice = new ImageFile( m_width, m_height, _format, _colorProfile );
			m_images[i] = imageSlice;
		}
	}
}
void	ImagesMatrix::Mips::Mip::AllocateRawBuffer( U32 _rowPitch, U32 _slicePitch, const U8* _sourceBuffer ) {
	m_rowPitch = _rowPitch;
	m_slicePitch = _slicePitch;

	U32	bufferSize = Depth() * m_slicePitch;
	if ( m_rawBuffer == NULL ) {
		m_rawBuffer = new U8[bufferSize];
	}

	if ( _sourceBuffer != NULL ) {
		memcpy_s( m_rawBuffer, bufferSize, _sourceBuffer, bufferSize );
	}
}

void	ImagesMatrix::Mips::Mip::ReleasePointers() {
	for ( U32 i=0; i < m_images.Count(); i++ ) {
		SAFE_DELETE( m_images[i] );
	}
	SAFE_DELETE_ARRAY( m_rawBuffer );
}
void	ImagesMatrix::Mips::Mip::ClearPointers() {
	for ( U32 i=0; i < m_images.Count(); i++ ) {
		m_images[i] = NULL;
	}
	m_rawBuffer = NULL;
}

void	ImagesMatrix::NextMipSize( U32& _size ) {
//	_size = (1+_size) >> 1;
	_size = MAX( 1U, _size >> 1 );
}
void	ImagesMatrix::NextMipSize( U32& _width, U32& _height ) {
	NextMipSize( _width );
	NextMipSize( _height );
}
void	ImagesMatrix::NextMipSize( U32& _width, U32& _height, U32& _depth ) {
	NextMipSize( _width, _height );
	_depth = (1+_depth) >> 1;
}

// Examples:
//	15 gives us 4 mips at respective sizes 15, 7, 3, 1
//	16 gives us 5 mips at respective sizes 16, 8, 4, 2, 1
U32		ImagesMatrix::ComputeMipsCount( U32 _size ) {
	U32	sizeLog2 = U32( floorf( log2f( _size ) ) );	// Worst case scenario: we want a 2^N-1 size to give us N-1
	return 1 + sizeLog2;							// And we need 1 more mip level to reach level 2^0
}
