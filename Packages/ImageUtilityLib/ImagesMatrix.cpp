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
	m_mipsArray.SetCount( 1 );		// Only 1 slice of mip
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

void	ImagesMatrix::MakeSigned() {
	for ( U32 i=0; i < m_mipsArray.Count(); i++ ) {
		Mips&	mips = m_mipsArray[i];
		mips.MakeSigned();
	}
}
void	ImagesMatrix::MakeUnSigned() {
	for ( U32 i=0; i < m_mipsArray.Count(); i++ ) {
		Mips&	mips = m_mipsArray[i];
		mips.MakeUnSigned();
	}
}

//////////////////////////////////////////////////////////////////////////
// DDS Loading/Saving
//
#pragma region DDS-Related Helpers

static void		Copy( const DirectX::Image& _source, ImageFile& _target ) {
	if (	_source.width != _target.Width()
		||	_source.height != _target.Height() ) {
			throw "Source and target image sizes mismatch!";
	}

	U32		targetSize = _target.Pitch();
	U32		sourceSize = U32(_source.rowPitch);
	for ( U32 Y=0; Y < _source.height; Y++ ) {
		const U8*	scanlineSource = _source.pixels + Y * sourceSize;
		U8*			scanlineTarget = _target.GetBits() + Y * targetSize;
		memcpy_s( scanlineTarget, targetSize, scanlineSource, targetSize );
	}
}

static void		Copy( const ImageFile& _source, const DirectX::Image& _target ) {
	if (	_source.Width() != _target.width
		||	_source.Height() != _target.height ) {
			throw "Source and target image sizes mismatch!";
	}

	U32		targetSize = U32(_target.rowPitch);
	U32		sourceSize = _source.Pitch();
	for ( U32 Y=0; Y < _target.height; Y++ ) {
		const U8*	scanlineSource = _source.GetBits() + Y * sourceSize;
		U8*			scanlineTarget = _target.pixels + Y * targetSize;
		memcpy_s( scanlineTarget, targetSize, scanlineSource, targetSize );
	}
}

void	ImagesMatrix::DDSLoadFile( const wchar_t* _fileName, COMPONENT_FORMAT& _componentFormat ) {
	// Load the image
	DirectX::ScratchImage*	DXT = new DirectX::ScratchImage();
	DirectX::TexMetadata	meta;
	DWORD	flags = DirectX::DDS_FLAGS_NONE;
	HRESULT	hResult = DirectX::LoadFromDDSFile( _fileName, flags, &meta, *DXT );
	if ( hResult != S_OK ) {
		throw "An error occurred while loading the DDS file!";
	}

	// Convert into an image matrix
	DDSLoad( DXT, &meta, _componentFormat );

	delete DXT;
}
void	ImagesMatrix::DDSLoadMemory( U64 _fileSize, void* _fileContent, COMPONENT_FORMAT& _componentFormat ) {
	// Load the image
	DirectX::ScratchImage*	DXT = new DirectX::ScratchImage();
	DirectX::TexMetadata	meta;
	DWORD	flags = DirectX::DDS_FLAGS_NONE;
	HRESULT	hResult = DirectX::LoadFromDDSMemory( _fileContent, _fileSize, flags, &meta, *DXT );
	if ( hResult != S_OK ) {
		throw "An error occurred while loading the DDS file!";
	}

	// Convert into an image matrix
	DDSLoad( DXT, &meta, _componentFormat );

	delete DXT;
}
void	ImagesMatrix::DDSLoad( const void* _blindPointerImage, const void* _blindPointerMetaData, COMPONENT_FORMAT& _componentFormat ) {
	const DirectX::ScratchImage&	image = *reinterpret_cast<const DirectX::ScratchImage*>( _blindPointerImage );
	const DirectX::TexMetadata&		meta = *reinterpret_cast<const DirectX::TexMetadata*>( _blindPointerMetaData );

	// Retrieve supported format
	U32				pixelSize = 0;
	PIXEL_FORMAT	format = DXGIFormat2PixelFormat( meta.format, _componentFormat, pixelSize );
	if ( format == PIXEL_FORMAT::UNKNOWN )
		throw "Unsupported format! Cannot find appropriate target image format to support source DXGI format...";

	ColorProfile	profile( _componentFormat == COMPONENT_FORMAT::UNORM_sRGB ? ColorProfile::STANDARD_PROFILE::sRGB : ColorProfile::STANDARD_PROFILE::LINEAR );

	// Build content slices
	U32	mipLevelsCount = U32(meta.mipLevels);
	if ( meta.depth == 1 ) {
		// We are dealing with a 2D texture format
		if ( image.GetImageCount() != meta.arraySize * meta.mipLevels )
			throw "Unexpected amount of images!";

		U32	arraySize = U32(meta.arraySize);

		if ( meta.IsCubemap() ) {
			// We are dealing with a texture cube array
			if ( meta.width != meta.height )
				throw "Image width & height mismatch!";
			if ( (meta.arraySize % 6) != 0 )
				throw "Array size is not an integer multiple of 6!";

			U32	cubeMapsCount = arraySize / 6;
			InitCubeTextureArray( U32(meta.width), cubeMapsCount, mipLevelsCount );
		} else {
			// We are dealing with a regular texture 2D array
			InitTexture2DArray( U32(meta.width), U32(meta.height), arraySize, mipLevelsCount );
		}

		if ( (U32(format) & U32(PIXEL_FORMAT::RAW_BUFFER)) == 0 ) {
			// Allocate actual images
			AllocateImageFiles( format, profile );

			// Fill up the content
			for ( U32 arrayIndex=0; arrayIndex < arraySize; arrayIndex++ ) {
				for ( U32 mipIndex=0; mipIndex < mipLevelsCount; mipIndex++ ) {
				 	const DirectX::Image*	sourceImage = image.GetImage( mipIndex, arrayIndex, 0U );
					ImageFile&				targetImage = *m_mipsArray[arrayIndex][mipIndex][0];
					targetImage.m_fileFormat = ImageFile::FILE_FORMAT::DDS;
					Copy( *sourceImage, targetImage );
				}
			}
		} else {
			// Copy raw data without any processing
			class	getSliceSize_t : public ImagesMatrix::GetRawBufferSizeFunctor {
			public:
				const DirectX::ScratchImage&	m_image;
				getSliceSize_t( const DirectX::ScratchImage& _image ) : m_image( _image ) {}
				const U8*	operator()( U32 _arraySliceIndex, U32 _mipLevelIndex, U32& _rowPitch, U32& _slicePitch ) const override {
					const DirectX::Image* mipImage = m_image.GetImage( _mipLevelIndex, _arraySliceIndex, 0U );
					_rowPitch = U32(mipImage->rowPitch);
					_slicePitch = U32(mipImage->slicePitch);

					return mipImage->pixels;
				}
			} getSliceSize( image );
			AllocateRawBuffers( format, getSliceSize );
		}

	} else {
		// We are dealing with a 3D texture format
		InitTexture3D( U32(meta.width), U32(meta.height), U32(meta.depth), U32(meta.mipLevels) );

		if ( (U32(format) & U32(PIXEL_FORMAT::RAW_BUFFER)) == 0 ) {
			// Allocate actual images
			AllocateImageFiles( format, profile );

			// Fill up the content
			for ( U32 mipIndex=0; mipIndex < mipLevelsCount; mipIndex++ ) {
				ImagesMatrix::Mips::Mip&	mip = m_mipsArray[0][mipIndex];

				for ( U32 sliceIndex=0; sliceIndex < mip.Depth(); sliceIndex++ ) {
					const DirectX::Image*	sourceImage = image.GetImage( mipIndex, 0U, sliceIndex );
					ImageFile&				targetImage = *mip[sliceIndex];
					targetImage.m_fileFormat = ImageFile::FILE_FORMAT::DDS;
					Copy( *sourceImage, targetImage );
				}
			}
		} else {
			// Copy raw data without any processing
			class	getMipSize_t : public ImagesMatrix::GetRawBufferSizeFunctor {
			public:
				const DirectX::ScratchImage&	m_image;
				getMipSize_t( const DirectX::ScratchImage& _image ) : m_image( _image ) {}
				const U8*	operator()( U32 _arraySliceIndex, U32 _mipLevelIndex, U32& _rowPitch, U32& _slicePitch ) const override {
					const DirectX::Image* mipImage = m_image.GetImage( _mipLevelIndex, 0U, 0U );

					_rowPitch = U32(mipImage->rowPitch);
					_slicePitch = U32(mipImage->slicePitch);

					return mipImage->pixels;	// DirectXTex stores the pixels into a contiguous buffer so it's okay to only give the first slice's pointer here
				}
			} getMipSize( image );
			AllocateRawBuffers( format, getMipSize );
		}
	}
}

void	ImagesMatrix::DDSSaveFile( const wchar_t* _fileName, COMPONENT_FORMAT _componentFormat ) const {

	// Create and fill the image
	DirectX::ScratchImage*	DXT = NULL;
	DDSSave( (void**) &DXT, _componentFormat );

	// Save to disk
	DWORD	flags = DirectX::DDS_FLAGS_FORCE_RGB | DirectX::DDS_FLAGS_NO_16BPP | DirectX::DDS_FLAGS_EXPAND_LUMINANCE | DirectX::DDS_FLAGS_FORCE_DX10_EXT;
	HRESULT	hResult = DirectX::SaveToDDSFile( DXT->GetImages(), DXT->GetImageCount(), DXT->GetMetadata(), flags, _fileName );
	if ( hResult != S_OK ) {
		throw "An error occurred while saving the DDS file!";
	}

	// Release the image
	delete DXT;
}
void	ImagesMatrix::DDSSaveMemory( U64& _fileSize, void*& _fileContent, COMPONENT_FORMAT _componentFormat ) const {

	// Create and fill the image
	DirectX::ScratchImage*	DXT = NULL;
	DDSSave( (void**) &DXT, _componentFormat );

	// Transfer into a memory blob
	DirectX::Blob	blob;
	DWORD			flags = DirectX::DDS_FLAGS_FORCE_RGB | DirectX::DDS_FLAGS_NO_16BPP | DirectX::DDS_FLAGS_EXPAND_LUMINANCE | DirectX::DDS_FLAGS_FORCE_DX10_EXT;
	HRESULT			hResult = DirectX::SaveToDDSMemory( DXT->GetImages(), DXT->GetImageCount(), DXT->GetMetadata(), flags, blob );
	if ( hResult != S_OK ) {
		throw "An error occurred while saving the DDS file into memory!";
	}

	// Release the image
	delete DXT;

	// Copy blob content
	_fileSize = blob.GetBufferSize();
	 U8*	targetBuffer = new U8[_fileSize];
	_fileContent = targetBuffer;
	memcpy_s( targetBuffer, _fileSize, blob.GetBufferPointer(), blob.GetBufferSize() );
}

void	ImagesMatrix::DDSSave( void** _blindPointerImage, COMPONENT_FORMAT _componentFormat ) const {
	DirectX::ScratchImage*&	image = *reinterpret_cast<DirectX::ScratchImage**>( _blindPointerImage );

	DXGI_FORMAT	DXFormat = PixelFormat2DXGIFormat( GetFormat(), _componentFormat );
	if ( DXFormat == DXGI_FORMAT_UNKNOWN )
		throw "Unsupported image format! Cannot find appropriate target DXGI format to support source image format...";

	// Build DTex scratch image
	image = new DirectX::ScratchImage();

	U32	arraySize = GetArraySize();
	if ( arraySize == 0 )
		throw "Invalid array size!";
	U32	mipLevelsCount = m_mipsArray[0].GetMipLevelsCount();
	if ( mipLevelsCount == 0 )
		throw "Invalid mip levels count!";

	U32	W = m_mipsArray[0][0].Width();
	U32	H = m_mipsArray[0][0].Height();
	U32	D = m_mipsArray[0][0].Depth();
	if ( W == 0 || H == 0 || D == 0 )
		throw "Invalid dimensions!";

	switch ( m_type ) {
		case ImagesMatrix::TYPE::GENERIC:
		case ImagesMatrix::TYPE::TEXTURE2D:
		case ImagesMatrix::TYPE::TEXTURECUBE: {
			// Assume 2D texture
			if ( D != 1 )
				throw "Invalid depth! Must be 1 for a 2D texture";

			HRESULT	hr = S_FALSE;
			if ( m_type == ImagesMatrix::TYPE::TEXTURECUBE ) {
				if ( W != H )
					throw "Cube texture width and height mismatch!";
				if ( (arraySize % 6) != 0 )
					throw "Array size is not an integer multiple of 6!";

				hr = image->InitializeCube( DXFormat, W, H, arraySize / 6, mipLevelsCount );
			} else {
				hr = image->Initialize2D( DXFormat, W, H, arraySize, mipLevelsCount );
			}
			if ( hr != S_OK )
				throw "Failed to initialize 2D texture!";

// Why can't we just do it ourselves???
// 			if ( _images.GetType() == ImagesMatrix::TYPE::TEXTURECUBE ) {
// 				image->GetMetadata().miscFlags |= DirectX::TEX_MISC_TEXTURECUBE;
// 			}

			// Copy to scratch image
			if ( (U32(m_format) & U32(PIXEL_FORMAT::RAW_BUFFER)) == 0 ) {
				for ( U32 arrayIndex=0; arrayIndex < arraySize; arrayIndex++ ) {
					const ImagesMatrix::Mips&	sourceMips = m_mipsArray[arrayIndex];
					for ( U32 mipLevelIndex=0; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
						const ImagesMatrix::Mips::Mip&	sourceMip = sourceMips[mipLevelIndex];
						const ImageFile*				sourceImage = sourceMip[0];
						if ( sourceImage == NULL )
							throw "Invalid source image! The images matrix is not initialized!";

						const DirectX::Image*	targetImage = image->GetImage( mipLevelIndex, arrayIndex, 0 );
						Copy( *sourceImage, *targetImage );
					}
				}
			} else {
				for ( U32 arrayIndex=0; arrayIndex < arraySize; arrayIndex++ ) {
					const ImagesMatrix::Mips&	sourceMips = m_mipsArray[arrayIndex];
					for ( U32 mipLevelIndex=0; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
						const ImagesMatrix::Mips::Mip&	sourceMip = sourceMips[mipLevelIndex];
						U32								sourceSlicePitch = sourceMip.SlicePitch();
						const U8*						sourceRawBuffer = sourceMip.GetRawBuffer();
						const DirectX::Image*			targetImage = image->GetImage( mipLevelIndex, arrayIndex, 0 );
						memcpy_s( targetImage->pixels, targetImage->slicePitch, sourceRawBuffer, sourceSlicePitch );
					}
				}
			}
			break;
		}

		case ImagesMatrix::TYPE::TEXTURE3D: {
			// Assume 3D texture
			if ( arraySize != 1 )
				throw "Invalid array size! Must be 1 for a 3D texture!";	// At least for the moment, DirectX doesn't support arrays of 3D textures (but we do! :D)

			HRESULT	hr = image->Initialize3D( DXFormat, W, H, D, mipLevelsCount );
			if ( hr != S_OK )
				throw "Failed to initialize 3D texture!";

			// Copy to scratch image
			if ( (U32(m_format) & U32(PIXEL_FORMAT::RAW_BUFFER)) == 0 ) {
				for ( U32 mipLevelIndex=0; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
					for ( U32 sliceIndex=0; sliceIndex < D; sliceIndex++ ) {
						const ImageFile*	sourceImage = m_mipsArray[0][mipLevelIndex][sliceIndex];
						if ( sourceImage == NULL )
							throw "Invalid source image! The images matrix is not initialized!";
						const DirectX::Image*	targetImage = image->GetImage( mipLevelIndex, 0, sliceIndex );
						Copy( *sourceImage, *targetImage );
					}
				}
			} else {
				const ImagesMatrix::Mips&	sourceMips = m_mipsArray[0];
				for ( U32 mipLevelIndex=0; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
					const ImagesMatrix::Mips::Mip&	sourceMip = sourceMips[mipLevelIndex];
					U32								sourceSlicePitch = sourceMip.SlicePitch();
					const U8*						sourceRawBuffer = sourceMip.GetRawBuffer();

					for ( U32 sliceIndex=0; sliceIndex < sourceMip.Depth(); sliceIndex++ ) {
						const DirectX::Image*	targetImage = image->GetImage( mipLevelIndex, 0, sliceIndex );
						memcpy_s( targetImage->pixels, targetImage->slicePitch, sourceRawBuffer + sliceIndex * sourceSlicePitch, sourceMip.SlicePitch() );
					}
				}
			}
			break;
		}
	}
}


//////////////////////////////////////////////////////////////////////////
// DDS Compression
//
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

#pragma endregion

void	ImagesMatrix::BuildMips( IMAGE_TYPE _imageType ) {
	switch ( m_type ) {
		case ImagesMatrix::TYPE::TEXTURE3D:
			RELEASE_ASSERT( m_mipsArray.Count() == 1, "Only 1 slice is supported for 3D texture mip building!" );
			m_mipsArray[0].BuildMips3D( _imageType );
			break;

		case ImagesMatrix::TYPE::TEXTURE2D:
		case ImagesMatrix::TYPE::TEXTURECUBE:
			for ( U32 sliceIndex=0; sliceIndex < m_mipsArray.Count(); sliceIndex++ ) {
				Mips&	mips = m_mipsArray[sliceIndex];
				mips.BuildMips2D( _imageType );
			}
			break;

		default:
			RELEASE_ASSERT( false, "Not implemented!" );
	}
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

void	ImagesMatrix::Mips::MakeSigned() {
	for ( U32 mipIndex=0; mipIndex < m_mips.Count(); mipIndex++ ) {
		Mips::Mip&	mip = m_mips[mipIndex];
		mip.MakeSigned();
	}
}
void	ImagesMatrix::Mips::MakeUnSigned() {
	for ( U32 mipIndex=0; mipIndex < m_mips.Count(); mipIndex++ ) {
		Mips::Mip&	mip = m_mips[mipIndex];
		mip.MakeUnSigned();
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

void	ImagesMatrix::Mips::Mip::MakeSigned() {
	for ( U32 sliceIndex=0; sliceIndex < m_images.Count(); sliceIndex++ ) {
		ImageFile*	sliceImage = m_images[sliceIndex];
		if ( sliceImage != NULL ) {
			sliceImage->MakeSigned();
		}
	}
}
void	ImagesMatrix::Mips::Mip::MakeUnSigned() {
	for ( U32 sliceIndex=0; sliceIndex < m_images.Count(); sliceIndex++ ) {
		ImageFile*	sliceImage = m_images[sliceIndex];
		if ( sliceImage != NULL ) {
			sliceImage->MakeUnSigned();
		}
	}
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
	U32	sizeLog2 = U32( floorf( log2f( (float) _size ) ) );	// Worst case scenario: we want a 2^N-1 size to give us N-1
	return 1 + sizeLog2;							// And we need 1 more mip level to reach level 2^0
}


//////////////////////////////////////////////////////////////////////////
// Mips Building
//
#pragma region Mips Building Helpers

void	sRGB2Linear( const bfloat4& _source, bfloat4& _target ) {
	_target.x = ColorProfile::sRGB2Linear( _source.x ),
	_target.y = ColorProfile::sRGB2Linear( _source.y ),
	_target.z = ColorProfile::sRGB2Linear( _source.z ),
	_target.w = _source.w;
}
void	Linear2sRGB( const bfloat4& _source, bfloat4& _target ) {
	_target.x = ColorProfile::Linear2sRGB( _source.x ),
	_target.y = ColorProfile::Linear2sRGB( _source.y ),
	_target.z = ColorProfile::Linear2sRGB( _source.z ),
	_target.w = _source.w;
}

void	ImagesMatrix::Mips::BuildMips2D( IMAGE_TYPE _imageType ) {
	if ( m_mips.Count() == 1 )
		return;	// No mip to build anyway...

	for ( U32 mipLevelIndex=1; mipLevelIndex < m_mips.Count(); mipLevelIndex++ ) {
		const Mip&	sourceMip = m_mips[mipLevelIndex-1];
		RELEASE_ASSERT( sourceMip.Depth() == 1, "Only a depth of 1 is allowed for 2D mips building!" );
		Mip&		targetMip = m_mips[mipLevelIndex];
		RELEASE_ASSERT( targetMip.Depth() == 1, "Only a depth of 1 is allowed for 2D mips building!" );

		ImageFile*	sourceMipImage = sourceMip[0];
		RELEASE_ASSERT( sourceMipImage != NULL, "Unallocated images: can't create mips!" );
		ImageFile*	targetMipImage = targetMip[0];
		RELEASE_ASSERT( targetMipImage != NULL, "Unallocated images: can't create mips!" );

		BuildMip2D( *sourceMipImage, *targetMipImage, _imageType );
	}
}

void	ImagesMatrix::Mips::BuildMips3D( IMAGE_TYPE _imageType ) {
	if ( m_mips.Count() == 1 )
		return;	// No mip to build anyway...

	for ( U32 mipLevelIndex=1; mipLevelIndex < m_mips.Count(); mipLevelIndex++ ) {
		const Mip&	sourceMip = m_mips[mipLevelIndex-1];
		Mip&		targetMip = m_mips[mipLevelIndex];
		targetMip.BuildMip3D( sourceMip, _imageType );
	}
}

void	ImagesMatrix::Mips::BuildMip2D( const ImageFile& _sourceMip, ImageFile& _targetMip, IMAGE_TYPE _imageType ) {
	U32			W0 = _sourceMip.Width();
	U32			H0 = _sourceMip.Height();
	bfloat4*	sourceScanlines = new bfloat4[2*W0];

	U32			W1 = _targetMip.Width();
	U32			H1 = _targetMip.Height();
	bfloat4*	targetScanline = new bfloat4[W1];

	bfloat4		two( 2, 2, 2, 1 );
	bfloat4		one( 1, 1, 1, 0 );
	bfloat4		eighth( 0.125f, 0.125f, 0.125f, 0 );
	bfloat4		halff( 0.5f, 0.5f, 0.5f, 0 );

	bfloat4		V00, V01, V10, V11, V0, V1, V;

	for ( U32 Y1=0; Y1 < H1; Y1++ ) {
		U32	Y00 = Y1 << 1;
		U32	Y01 = MIN( H0-1, Y00 + 1 );
		_sourceMip.ReadScanline( Y00, sourceScanlines );
		_sourceMip.ReadScanline( Y01, sourceScanlines + W0 );

		switch ( _imageType ) {
			case ImagesMatrix::NORMAL_MAP:
				// Average as vectors
				for ( U32 X1=0; X1 < W1; X1++ ) {
					U32	X00 = X1 << 1;
					U32	X01 = MIN( W0-1, X00+1 );

					V00 = two * sourceScanlines[X00] - one;
					V10 = two * sourceScanlines[X01] - one;
					V01 = two * sourceScanlines[W0+X00] - one;
					V11 = two * sourceScanlines[W0+X01] - one;
					V = eighth * (V00 + V10 + V01 + V11) + halff;
					targetScanline[X1] = V;
				}
				break;

			case ImagesMatrix::sRGB:
				// Average as sRGB-encoded colors
				for ( U32 X1=0; X1 < W1; X1++ ) {
					U32	X00 = X1 << 1;
					U32	X01 = MIN( W0-1, X00+1 );

					sRGB2Linear( sourceScanlines[X00], V00 );
					sRGB2Linear( sourceScanlines[X01], V10 );
					sRGB2Linear( sourceScanlines[W0+X00], V01 );
					sRGB2Linear( sourceScanlines[W0+X01], V11 );
					V = 0.25f * (V00 + V10 + V01 + V11);
					Linear2sRGB( V, targetScanline[X1] );
				}
				break;

			case ImagesMatrix::LINEAR:
				// Average as regular linear values
				for ( U32 X1=0; X1 < W1; X1++ ) {
					U32	X00 = X1 << 1;
					U32	X01 = MIN( W0-1, X00+1 );

					V00 = sourceScanlines[X00];
					V10 = sourceScanlines[X01];
					V01 = sourceScanlines[W0+X00];
					V11 = sourceScanlines[W0+X01];
					V = 0.25f * (V00 + V10 + V01 + V11);
					targetScanline[X1] = V;
				}
				break;

			default:
				throw "Not implemented!";
		}

		_targetMip.WriteScanline( Y1, targetScanline );
	}

	SAFE_DELETE_ARRAY( targetScanline );
	SAFE_DELETE_ARRAY( sourceScanlines );
}

void	ImagesMatrix::Mips::Mip::BuildMip3D( const Mip& _sourceMip, IMAGE_TYPE _imageType ) {
	U32			W0 = _sourceMip.Width();
	U32			H0 = _sourceMip.Height();
	U32			D0 = _sourceMip.Depth();
	bfloat4*	sourceScanlines = new bfloat4[4*W0];
	bfloat4*	sourceScanlinesZ0 = sourceScanlines;
	bfloat4*	sourceScanlinesZ1 = sourceScanlines + 2*W0;

	U32			W1 = Width();
	U32			H1 = Height();
	U32			D1 = Depth();
	bfloat4*	targetScanline = new bfloat4[W1];

	bfloat4		V000, V001, V010, V011, V100, V101, V110, V111, V;

	for ( U32 Z1=0; Z1 < D1; Z1++ ) {
		U32	Z00 = Z1 << 1;
		U32	Z01 = MIN( D0-1, Z00 + 1 );
		const ImageFile&	sourceSlice0 = *_sourceMip[Z00];
		const ImageFile&	sourceSlice1 = *_sourceMip[Z01];
		ImageFile&			targetSlice = *m_images[Z1];

		for ( U32 Y1=0; Y1 < H1; Y1++ ) {
			U32	Y00 = Y1 << 1;
			U32	Y01 = MIN( H0-1, Y00 + 1 );

			sourceSlice0.ReadScanline( Y00, sourceScanlinesZ0 );
			sourceSlice0.ReadScanline( Y01, sourceScanlinesZ0 + W0 );
			sourceSlice1.ReadScanline( Y00, sourceScanlinesZ1 );
			sourceSlice1.ReadScanline( Y01, sourceScanlinesZ1 + W0 );

			switch ( _imageType ) {
				case ImagesMatrix::sRGB:
					// Average as sRGB-encoded colors
					for ( U32 X1=0; X1 < W1; X1++ ) {
						U32	X00 = X1 << 1;
						U32	X01 = MIN( W0-1, X00+1 );

						sRGB2Linear( sourceScanlinesZ0[X00], V000 );
						sRGB2Linear( sourceScanlinesZ0[X01], V100 );
						sRGB2Linear( sourceScanlinesZ0[W0+X00], V010 );
						sRGB2Linear( sourceScanlinesZ0[W0+X01], V110 );
						sRGB2Linear( sourceScanlinesZ1[X00], V001 );
						sRGB2Linear( sourceScanlinesZ1[X01], V101 );
						sRGB2Linear( sourceScanlinesZ1[W0+X00], V011 );
						sRGB2Linear( sourceScanlinesZ1[W0+X01], V111 );
						V = 0.125f * (V000 + V001 + V010 + V011 + V100 + V101 + V110 + V111);
						Linear2sRGB( V, targetScanline[X1] );
					}
					break;
				case ImagesMatrix::LINEAR:
					// Average as regular linear values
					for ( U32 X1=0; X1 < W1; X1++ ) {
						U32	X00 = X1 << 1;
						U32	X01 = MIN( W0-1, X00+1 );

						V000 = sourceScanlinesZ0[X00];
						V100 = sourceScanlinesZ0[X01];
						V010 = sourceScanlinesZ0[W0+X00];
						V110 = sourceScanlinesZ0[W0+X01];
						V001 = sourceScanlinesZ1[X00];
						V101 = sourceScanlinesZ1[X01];
						V011 = sourceScanlinesZ1[W0+X00];
						V111 = sourceScanlinesZ1[W0+X01];

						V = 0.125f * (V000 + V001 + V010 + V011 + V100 + V101 + V110 + V111);
						targetScanline[X1] = V;
					}

				default:
					throw "Not implemented!";
			}

			targetSlice.WriteScanline( Y1, targetScanline );
		}
	}

	SAFE_DELETE_ARRAY( targetScanline );
	SAFE_DELETE_ARRAY( sourceScanlines );
}

/*
Renderer.PixelsBuffer[]	ImagesMatrix::Mips::BuildMips( float4[,] _mip0, bool _isNormalMap, bool _sRGB ) {
	uint	W = (uint) _mip0.GetLength( 0 );
	uint	H = (uint) _mip0.GetLength( 1 );
	uint	mipsCount = 1 + (uint) Mathf.Ceiling( Mathf.Log( Math.Max( W, H ) ) / Mathf.Log( 2 ) );

	float4[,]	sourceMip = _mip0;
	List< Renderer.PixelsBuffer >	mips = new List<Renderer.PixelsBuffer>( (int) mipsCount );
	for ( uint mipLevel=0; mipLevel < mipsCount; mipLevel++ ) {

		// Write mip
		Renderer.PixelsBuffer	mip = new  Renderer.PixelsBuffer( W*H*4 );
		mips.Add( mip );
		using ( System.IO.BinaryWriter Wr = mip.OpenStreamWrite() ) {
			if ( _isNormalMap ) {
				for ( int Y=0; Y < H; Y++ ) {
					for ( int X=0; X < W; X++ ) {
						Wr.Write( (sbyte) Mathf.Clamp( 256.0f * sourceMip[X,Y].x - 128.0f, -128, 127 ) );
						Wr.Write( (sbyte) Mathf.Clamp( 256.0f * sourceMip[X,Y].y - 128.0f, -128, 127 ) );
						Wr.Write( (sbyte) Mathf.Clamp( 256.0f * sourceMip[X,Y].z - 128.0f, -128, 127 ) );
						Wr.Write( (byte) 255 );
					}
				}
			} else {
				for ( int Y=0; Y < H; Y++ ) {
					for ( int X=0; X < W; X++ ) {
						Wr.Write( (byte) Mathf.Clamp( 256.0f * sourceMip[X,Y].x, 0, 255 ) );
						Wr.Write( (byte) Mathf.Clamp( 256.0f * sourceMip[X,Y].y, 0, 255 ) );
						Wr.Write( (byte) Mathf.Clamp( 256.0f * sourceMip[X,Y].z, 0, 255 ) );
						Wr.Write( (byte) 255 );
					}
				}
			}
		}

		// Build a new mip
		if ( mipLevel < mipsCount-1 ) {
			uint	oldW = W;
			uint	oldH = H;
			W = Math.Max( 1, W >> 1 );
			H = Math.Max( 1, H >> 1 );
			float4[,]	targetMip = new float4[W,H];

			float4	V00, V01, V10, V11, V;
			float4	two = new float4( 2, 2, 2, 1 );
			float4	one = new float4( 1, 1, 1, 0 );
			float4	eighth = new float4( 0.125f, 0.125f, 0.125f, 0 );
			float4	halff = new float4( 0.5f, 0.5f, 0.5f, 0 );
			if ( _isNormalMap ) {
				// Sum as vectors
				for ( int Y=0; Y < H; Y++ ) {
					int	Y0 = Math.Min( (int) oldH-1, 2*Y+0 );
					int	Y1 = Math.Min( (int) oldH-1, 2*Y+1 );
					for ( int X=0; X < W; X++ ) {
						int	X0 = Math.Min( (int) oldW-1, 2*X+0 );
						int	X1 = Math.Min( (int) oldW-1, 2*X+1 );

						V00 = two * sourceMip[X0,Y0] - one;
						V01 = two * sourceMip[X0,Y1] - one;
						V10 = two * sourceMip[X1,Y0] - one;
						V11 = two * sourceMip[X1,Y1] - one;
						V = eighth * (V00 + V01 + V10 + V11) + halff;
						targetMip[X,Y] = V;
					}
				}
			} else if ( _sRGB ) {
				// Sum as sRGB-packed colors
				for ( int Y=0; Y < H; Y++ ) {
					int	Y0 = Math.Min( (int) oldH-1, 2*Y+0 );
					int	Y1 = Math.Min( (int) oldH-1, 2*Y+1 );
					for ( int X=0; X < W; X++ ) {
						int	X0 = Math.Min( (int) oldW-1, 2*X+0 );
						int	X1 = Math.Min( (int) oldW-1, 2*X+1 );

						V00 = sRGB2Linear( sourceMip[X0,Y0] );
						V01 = sRGB2Linear( sourceMip[X0,Y1] );
						V10 = sRGB2Linear( sourceMip[X1,Y0] );
						V11 = sRGB2Linear( sourceMip[X1,Y1] );
						V = 0.25f * (V00 + V01 + V10 + V11);
						targetMip[X,Y] = Linear2sRGB( V );
					}
				}
			} else {
				// Sum as regular linear values
				for ( int Y=0; Y < H; Y++ ) {
					int	Y0 = Math.Min( (int) oldH-1, 2*Y+0 );
					int	Y1 = Math.Min( (int) oldH-1, 2*Y+1 );
					for ( int X=0; X < W; X++ ) {
						int	X0 = Math.Min( (int) oldW-1, 2*X+0 );
						int	X1 = Math.Min( (int) oldW-1, 2*X+1 );

						V00 = sourceMip[X0,Y0];
						V01 = sourceMip[X0,Y1];
						V10 = sourceMip[X1,Y0];
						V11 = sourceMip[X1,Y1];
						V = 0.25f * (V00 + V01 + V10 + V11);
						targetMip[X,Y] = V;
					}
				}
			}

			sourceMip = targetMip;
		}
	}

	return mips.ToArray();
}
*/
#pragma endregion
