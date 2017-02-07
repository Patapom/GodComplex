#include "stdafx.h"

#include "Texture2D.h"
#include "Texture3D.h"

Texture3D::Texture3D( Device& _Device, U32 _width, U32 _height, U32 _depth, U32 _mipLevelsCount, const BaseLib::IPixelAccessor& _format, BaseLib::COMPONENT_FORMAT _componentFormat, const void* const* _ppContent, bool _staging, bool _UAV )
	: Component( _Device )
	, m_width( _width )
	, m_height( _height )
	, m_depth( _depth )
	, m_pixelFormat( &_format )
	, m_componentFormat( _componentFormat )
	, m_mipLevelsCount( _mipLevelsCount )
{
	Init( _ppContent, _staging, _UAV );
}

Texture3D::Texture3D( Device& _device, const ImageUtilityLib::ImagesMatrix& _images, BaseLib::COMPONENT_FORMAT _componentFormat )
	: Component( _device ) {
	ASSERT( _images.GetType() == ImageUtilityLib::ImagesMatrix::TYPE::TEXTURE3D, "Invalid images matrix type!" );
	ASSERT( _images.GetArraySize() == 1, "Unexpected array size! Must be 1 for 3D textures. Other slices will simply be ignored..." );
	m_mipLevelsCount = _images[0].GetMipLevelsCount();
	ASSERT( m_mipLevelsCount > 0, "Invalid mip levels count!" );

	// Retrieve default image size
	const ImageUtilityLib::ImagesMatrix::Mips::Mip&	referenceMip = _images[0][0];
	m_width = referenceMip.Width();
	m_height = referenceMip.Height();
	m_depth = referenceMip.Depth();
	ASSERT( m_width <= MAX_TEXTURE_SIZE, "Texture size out of range!" );
	ASSERT( m_height <= MAX_TEXTURE_SIZE, "Texture size out of range!" );
	ASSERT( m_depth <= MAX_TEXTURE_SIZE, "Texture size out of range!" );

	// Retrieve image format
	m_pixelFormat = &ImageUtilityLib::ImageFile::PixelFormat2Accessor( _images.GetFormat() );
	m_componentFormat = _componentFormat;
	DXGI_FORMAT	textureFormat = ImageUtilityLib::ImageFile::PixelFormat2DXGIFormat( _images.GetFormat(), _componentFormat );

	// Prepare main descriptor
	D3D11_TEXTURE3D_DESC	desc;
	desc.Width = m_width;
	desc.Height = m_height;
	desc.Depth = m_depth;
	desc.MipLevels = m_mipLevelsCount;
	desc.Format = textureFormat;
	desc.Usage = D3D11_USAGE_IMMUTABLE;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
	desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	desc.MiscFlags = 0;

	// Prepare individual sub-resource descriptors
	D3D11_SUBRESOURCE_DATA  subResourceDescriptors[MAX_TEXTURE_POT];
	const ImageUtilityLib::ImagesMatrix::Mips&	mips = _images[0];

	U32	W = m_width;
	U32	H = m_height;
	U32	D = m_depth;
	for ( U32 mipLevelIndex=0; mipLevelIndex < m_mipLevelsCount; mipLevelIndex++ ) {
		const ImageUtilityLib::ImagesMatrix::Mips::Mip&	mip = mips[mipLevelIndex];
		ASSERT( mip.Width() == W && mip.Height() == H && mip.Depth() == D, "Mip's width/height/depth mismatch!" );

		// Allocate temporary memory where we'll store a sequential version of all the slices (at the moment stored inside separate images!)
		const ImageUtilityLib::ImageFile*	sliceImage = mip[0];
		RELEASE_ASSERT( sliceImage != NULL, "Invalid mip slice image!" );
		U32	rowPitch = sliceImage->Pitch();
		U32	slicePitch = H * rowPitch;
		subResourceDescriptors[mipLevelIndex].SysMemPitch = rowPitch;
		subResourceDescriptors[mipLevelIndex].SysMemSlicePitch = slicePitch;
		U8*	targetSlicePtr = new U8[D * slicePitch];
		subResourceDescriptors[mipLevelIndex].pSysMem = targetSlicePtr;

		// Concatenate each slice
		for ( U32 sliceIndex=0; sliceIndex < D; sliceIndex++, targetSlicePtr+=slicePitch ) {
			const ImageUtilityLib::ImageFile*	sliceImage = mip[sliceIndex];
			RELEASE_ASSERT( sliceImage != NULL, "Invalid mip slice image!" );
			RELEASE_ASSERT( sliceImage->Pitch() == rowPitch && sliceImage->Width() == W && sliceImage->Height() == H, "Image slice's dimensions mismatch!" );
			memcpy_s( targetSlicePtr, slicePitch, sliceImage->GetBits(), slicePitch );
		}

		NextMipSize( W, H, D );
	}

	Check( m_device.DXDevice().CreateTexture3D( &desc, subResourceDescriptors, &m_texture ) );

	// Release temporary memory
	for ( U32 mipLevelIndex=0; mipLevelIndex < m_mipLevelsCount; mipLevelIndex++ ) {
		SAFE_DELETE_ARRAY( subResourceDescriptors[mipLevelIndex].pSysMem );
	}

	// Clear last assignment slots
	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_lastAssignedSlots[ShaderStageIndex] = ~0U;
	m_lastAssignedSlotsUAV = ~0U;
}

static void		ReleaseDirectXObject( int _EntryIndex, void*& _pValue, void* _pUserData ) {
	IUnknown*	pObject = (IUnknown*) _pValue;
	pObject->Release();
}

Texture3D::~Texture3D() {
	ASSERT( m_texture != NULL, "Invalid texture to destroy !" );

	m_cachedSRVs.ForEach( ReleaseDirectXObject, NULL );
	m_cachedRTVs.ForEach( ReleaseDirectXObject, NULL );
	m_cachedUAVs.ForEach( ReleaseDirectXObject, NULL );

	m_texture->Release();
	m_texture = NULL;
}

void	Texture3D::Init( const void* const* _ppContent, bool _bStaging, bool _bUnOrderedAccess, TextureFilePOM::MipDescriptor* _pMipDescriptors ) {
	ASSERT( m_width <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( m_height <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( m_depth <= MAX_TEXTURE_SIZE, "Texture size out of range !" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_lastAssignedSlots[ShaderStageIndex] = ~0U;
	m_lastAssignedSlotsUAV = ~0U;

	m_mipLevelsCount = ComputeMipLevelsCount( m_width, m_height, m_depth, m_mipLevelsCount );

	D3D11_TEXTURE3D_DESC	Desc;
	Desc.Width = m_width;
	Desc.Height = m_height;
	Desc.Depth = m_depth;
	Desc.MipLevels = m_mipLevelsCount;
	Desc.Format = Texture2D::PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	Desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG( 0 );

	if ( _bStaging ) {
		Desc.Usage = D3D11_USAGE_STAGING;
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | D3D11_CPU_ACCESS_WRITE;
		Desc.BindFlags = 0;
	} else {
		Desc.Usage = _ppContent != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DEFAULT;
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
		Desc.BindFlags = _ppContent != NULL ? D3D11_BIND_SHADER_RESOURCE : (D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE | (_bUnOrderedAccess ? D3D11_BIND_UNORDERED_ACCESS: 0));
	}

	if ( _ppContent != NULL ) {
		D3D11_SUBRESOURCE_DATA  pInitialData[MAX_TEXTURE_POT];
		U32	W = m_width;
		U32	H = m_height;
		U32	D = m_depth;
		for ( U32 mipLevelIndex=0; mipLevelIndex < m_mipLevelsCount; mipLevelIndex++ ) {
			U32	rowPitch = _pMipDescriptors != NULL ? _pMipDescriptors[mipLevelIndex].rowPitch : W * m_pixelFormat->Size();
			U32	depthPitch = _pMipDescriptors != NULL ? _pMipDescriptors[mipLevelIndex].depthPitch : H * rowPitch;

			pInitialData[mipLevelIndex].pSysMem = _ppContent[mipLevelIndex];
			pInitialData[mipLevelIndex].SysMemPitch = rowPitch;
			pInitialData[mipLevelIndex].SysMemSlicePitch = depthPitch;
			NextMipSize( W, H, D );
		}

		Check( m_device.DXDevice().CreateTexture3D( &Desc, pInitialData, &m_texture ) );
	}
	else
		Check( m_device.DXDevice().CreateTexture3D( &Desc, NULL, &m_texture ) );
}

ID3D11ShaderResourceView*	Texture3D::GetSRV( U32 _MipLevelStart, U32 _mipLevelsCount, U32 _FirstWSlice, U32 _WSize, bool _AsArray ) const {
	if ( _mipLevelsCount == 0 )
		_mipLevelsCount = m_mipLevelsCount - _MipLevelStart;
	if ( _WSize == 0 )
		_WSize = m_depth - _FirstWSlice;

	// Check if we already have it
	U32	Hash;
	if ( _AsArray )
		Hash = (_MipLevelStart << 0) | (_FirstWSlice << 4) | (_mipLevelsCount << (4+12)) | (_WSize << (4+12+4));	// Re-organized to have most likely changes (i.e. mip & array starts) first
	else
		Hash = _MipLevelStart | (_mipLevelsCount << 4);
	Hash ^= _AsArray ? 0x80000000UL : 0;

	ID3D11ShaderResourceView*	pExistingView = (ID3D11ShaderResourceView*) m_cachedSRVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	D3D11_SHADER_RESOURCE_VIEW_DESC	desc;
	desc.Format = Texture2D::PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	if ( _AsArray ) {
		// Force as a Texture2DArray
		desc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
		desc.Texture2DArray.MostDetailedMip = _MipLevelStart;
		desc.Texture2DArray.MipLevels = _mipLevelsCount;
		desc.Texture2DArray.FirstArraySlice = _FirstWSlice;
		desc.Texture2DArray.ArraySize = _WSize;
	} else {
		desc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE3D;
		desc.Texture3D.MostDetailedMip = _MipLevelStart;
		desc.Texture3D.MipLevels = _mipLevelsCount;
	}

	ID3D11ShaderResourceView*	pView;
	Check( m_device.DXDevice().CreateShaderResourceView( m_texture, &desc, &pView ) );

	m_cachedSRVs.Add( Hash, pView );

	return pView;
}

ID3D11RenderTargetView*		Texture3D::GetRTV( U32 _mipLevelIndex, U32 _FirstWSlice, U32 _WSize ) const {
	if ( _WSize == 0 )
		_WSize = m_depth - _FirstWSlice;

	// Check if we already have it
//	U32	Hash = _WSize | ((_FirstWSlice | (_mipLevelIndex << 12)) << 12);
	U32	Hash = (_mipLevelIndex << 0) | (_FirstWSlice << 12) | (_WSize << (4+12));	// Re-organized to have most likely changes (i.e. mip & slice starts) first
	ID3D11RenderTargetView*	pExistingView = (ID3D11RenderTargetView*) m_cachedRTVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	D3D11_RENDER_TARGET_VIEW_DESC	Desc;
	Desc.Format = Texture2D::PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	Desc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MipSlice = _mipLevelIndex;
	Desc.Texture3D.FirstWSlice = _FirstWSlice;
	Desc.Texture3D.WSize = _WSize;

	ID3D11RenderTargetView*	pView;
	Check( m_device.DXDevice().CreateRenderTargetView( m_texture, &Desc, &pView ) );

	m_cachedRTVs.Add( Hash, pView );

	return pView;
}

ID3D11UnorderedAccessView*	Texture3D::GetUAV( U32 _mipLevelIndex, U32 _FirstWSlice, U32 _WSize ) const {
	if ( _WSize == 0 )
		_WSize = m_depth - _FirstWSlice;

	// Check if we already have it
//	U32	Hash = _WSize | ((_FirstWSlice | (_mipLevelIndex << 12)) << 12);
	U32	Hash = (_mipLevelIndex << 0) | (_FirstWSlice << 12) | (_WSize << (4+12));	// Re-organized to have most likely changes (i.e. mip & slice starts) first
	ID3D11UnorderedAccessView*	pExistingView = (ID3D11UnorderedAccessView*) m_cachedUAVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_UNORDERED_ACCESS_VIEW_DESC	Desc;
	Desc.Format = Texture2D::PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	Desc.ViewDimension = D3D11_UAV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MipSlice = _mipLevelIndex;
	Desc.Texture3D.FirstWSlice = _FirstWSlice;
	Desc.Texture3D.WSize = _WSize;

	ID3D11UnorderedAccessView*	pView;
	Check( m_device.DXDevice().CreateUnorderedAccessView( m_texture, &Desc, &pView ) );

	m_cachedUAVs.Add( Hash, pView );

	return pView;
}

void	Texture3D::Set( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0 );
	m_device.DXContext().VSSetShaderResources( _SlotIndex, 1, &_pView );
	m_device.DXContext().HSSetShaderResources( _SlotIndex, 1, &_pView );
	m_device.DXContext().DSSetShaderResources( _SlotIndex, 1, &_pView );
	m_device.DXContext().GSSetShaderResources( _SlotIndex, 1, &_pView );
	m_device.DXContext().PSSetShaderResources( _SlotIndex, 1, &_pView );
	m_device.DXContext().CSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[0] = _SlotIndex;
	m_lastAssignedSlots[1] = _SlotIndex;
	m_lastAssignedSlots[2] = _SlotIndex;
	m_lastAssignedSlots[3] = _SlotIndex;
	m_lastAssignedSlots[4] = _SlotIndex;
	m_lastAssignedSlots[5] = _SlotIndex;
}
void	Texture3D::SetVS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0 );
	m_device.DXContext().VSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[0] = _SlotIndex;
}
void	Texture3D::SetHS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0 );
	m_device.DXContext().HSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[1] = _SlotIndex;
}
void	Texture3D::SetDS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0 );
	m_device.DXContext().DSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[2] = _SlotIndex;
}
void	Texture3D::SetGS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0 );
	m_device.DXContext().GSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[3] = _SlotIndex;
}
void	Texture3D::SetPS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0 );
	m_device.DXContext().PSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[4] = _SlotIndex;
}
void	Texture3D::SetCS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0 );
	m_device.DXContext().CSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[5] = _SlotIndex;
}

void	Texture3D::RemoveFromLastAssignedSlots() const {
	static Device::SHADER_STAGE_FLAGS	pStageFlags[] = {
		Device::SSF_VERTEX_SHADER,
		Device::SSF_HULL_SHADER,
		Device::SSF_DOMAIN_SHADER,
		Device::SSF_GEOMETRY_SHADER,
		Device::SSF_PIXEL_SHADER,
		Device::SSF_COMPUTE_SHADER,
	};
	for ( U32 ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		if ( m_lastAssignedSlots[ShaderStageIndex] != -1 ) {
			m_device.RemoveShaderResources( m_lastAssignedSlots[ShaderStageIndex], 1, pStageFlags[ShaderStageIndex] );
			m_lastAssignedSlots[ShaderStageIndex] = -1;
		}
}

// UAV setting
void	Texture3D::SetCSUAV( U32 _SlotIndex, ID3D11UnorderedAccessView* _pView ) const {
	_pView = _pView != NULL ? _pView : GetUAV( 0, 0, 0 );
	UINT	InitialCount = -1;
	m_device.DXContext().CSSetUnorderedAccessViews( _SlotIndex, 1, &_pView, &InitialCount );
	m_lastAssignedSlotsUAV = _SlotIndex;
}

void	Texture3D::RemoveFromLastAssignedSlotUAV() const {
	ID3D11UnorderedAccessView*	pNULL = NULL;
	UINT	InitialCount = -1;
	if ( m_lastAssignedSlotsUAV != -1 )
		m_device.DXContext().CSSetUnorderedAccessViews( m_lastAssignedSlotsUAV, 1, &pNULL, &InitialCount );
	m_lastAssignedSlotsUAV = -1;
}


void	Texture3D::CopyFrom( Texture3D& _sourceTexture ) {
	ASSERT( _sourceTexture.m_width == m_width && _sourceTexture.m_height == m_height && _sourceTexture.m_depth == m_depth, "Size mismatch!" );
	ASSERT( _sourceTexture.m_mipLevelsCount == m_mipLevelsCount, "Mips count mismatch!" );
	ASSERT( _sourceTexture.m_pixelFormat == m_pixelFormat, "Format mismatch!" );

	m_device.DXContext().CopyResource( m_texture, _sourceTexture.m_texture );
}

const D3D11_MAPPED_SUBRESOURCE&	Texture3D::MapRead( U32 _mipLevelIndex ) const {
	Check( m_device.DXContext().Map( m_texture, _mipLevelIndex, D3D11_MAP_READ, 0, &m_lockedResource ) );
	return m_lockedResource;
}

const D3D11_MAPPED_SUBRESOURCE&	Texture3D::MapWrite( U32 _mipLevelIndex ) {
	Check( m_device.DXContext().Map( m_texture, _mipLevelIndex, D3D11_MAP_WRITE, 0, &m_lockedResource ) );
	return m_lockedResource;
}

void	Texture3D::UnMap( U32 _mipLevelIndex ) const {
	m_device.DXContext().Unmap( m_texture, _mipLevelIndex );
}

void	Texture3D::ReadAsImagesMatrix( ImageUtilityLib::ImagesMatrix& _images ) const {
	// Initialize the matrix to the proper dimensions
	_images.InitTexture3D( m_width, m_height, m_depth, m_mipLevelsCount );

	// Allocate actual images
	ImageUtilityLib::ImageFile::PIXEL_FORMAT	format = ImageUtilityLib::ImageFile::Accessor2PixelFormat( *m_pixelFormat );
	ImageUtilityLib::ColorProfile				dummyProfile( m_componentFormat == BaseLib::COMPONENT_FORMAT::UNORM_sRGB ? ImageUtilityLib::ColorProfile::STANDARD_PROFILE::sRGB : ImageUtilityLib::ColorProfile::STANDARD_PROFILE::LINEAR );
	_images.AllocateImageFiles( format, dummyProfile );

	// Fill up each image with mapped content
	for ( U32 mipLevelIndex=0; mipLevelIndex < m_mipLevelsCount; mipLevelIndex++ ) {
		ImageUtilityLib::ImagesMatrix::Mips::Mip&	targetMip = _images[0][mipLevelIndex];

		const D3D11_MAPPED_SUBRESOURCE&	mappedSourceMip = MapRead( mipLevelIndex );

		for ( U32 sliceIndex=0; sliceIndex < m_depth; sliceIndex++ ) {
			ImageUtilityLib::ImageFile&	targetImage = *targetMip[sliceIndex];
			const U8*					sourceData = reinterpret_cast<U8*>( mappedSourceMip.pData ) + sliceIndex * mappedSourceMip.DepthPitch;
			U8*							targetData = targetImage.GetBits();
			U32							targetPitch = targetImage.Pitch();
			for ( U32 Y=0; Y < targetMip.Height(); Y++ ) {
				const U8*	sourceScanline = sourceData + mappedSourceMip.RowPitch * Y;
				U8*			targetScanline = targetData + targetPitch * Y;
				memcpy_s( targetScanline, targetPitch, sourceScanline, mappedSourceMip.RowPitch );
			}
		}

		UnMap( mipLevelIndex );
	}
}

void	Texture3D::NextMipSize( U32& _width, U32& _height, U32& _depth ) {
	_width = MAX( 1U, _width >> 1 );
	_height = MAX( 1U, _height >> 1 );
	_depth = MAX( 1U, _depth >> 1 );
}

U32	 Texture3D::ComputeMipLevelsCount( U32 _width, U32 _height, U32 _depth, U32 _mipLevelsCount ) {
	U32 MaxSize = MAX( MAX( _width, _height ), _depth );
	U32	MaxMipLevelsCount = int( ceilf( logf( MaxSize+1.0f ) / logf( 2.0f ) ) );
	
	if ( _mipLevelsCount == 0 )
		_mipLevelsCount = MaxMipLevelsCount;
	else
		_mipLevelsCount = MIN( _mipLevelsCount, MaxMipLevelsCount );

	ASSERT( _mipLevelsCount <= MAX_TEXTURE_POT, "Texture mip level out of range !" );
	return _mipLevelsCount;
}

#if 0//defined(_DEBUG) || !defined(GODCOMPLEX)

#include "..\..\Utility\TextureFilePOM.h"

// I/O for staging textures
void	Texture3D::Save( const char* _pFileName ) {
	TextureFilePOM	POM;
	POM.AllocateContent( *this );

	// Fill up content
 	int	Depth = m_depth;
	for ( U32 MipLevelIndex=0; MipLevelIndex < m_mipLevelsCount; MipLevelIndex++ ) {
		Map( MipLevelIndex );

		POM.m_ppContent[MipLevelIndex] = new void*[Depth*m_lockedResource.DepthPitch];

		POM.m_pMipsDescriptors[MipLevelIndex].rowPitch = m_lockedResource.RowPitch;
		POM.m_pMipsDescriptors[MipLevelIndex].depthPitch = m_lockedResource.DepthPitch;

		memcpy_s( POM.m_ppContent[MipLevelIndex], Depth * m_lockedResource.DepthPitch, m_lockedResource.pData, Depth * m_lockedResource.DepthPitch );
		UnMap( MipLevelIndex );

 		Depth = MAX( 1, Depth >> 1 );
	}

	POM.Save( _pFileName );

// 	FILE*	pFile;
// 	fopen_s( &pFile, _pFileName, "wb" );
// 	ASSERT( pFile != NULL, "Can't create file!" );
// 
// 	// Write the type and format
// 	U8		Type = 0x02;
// 	U8		Format = U32(m_Format.DirectXFormat()) & 0xFF;
// 	fwrite( &Type, sizeof(U8), 1, pFile );
// 	fwrite( &Format, sizeof(U8), 1, pFile );
// 
// 	// Write the dimensions
// 	fwrite( &m_Width, sizeof(int), 1, pFile );
// 	fwrite( &m_Height, sizeof(int), 1, pFile );
// 	fwrite( &m_Depth, sizeof(int), 1, pFile );
// 	fwrite( &m_MipLevelsCount, sizeof(int), 1, pFile );
// 
// 	// Write each mip
// 	int	Depth = m_Depth;
// 	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
// 	{
// 		Map( MipLevelIndex );
// 		fwrite( &m_LockedResource.rowPitch, sizeof(int), 1, pFile );
// 		fwrite( &m_LockedResource.depthPitch, sizeof(int), 1, pFile );
// 		for ( int SliceIndex=0; SliceIndex < Depth; SliceIndex++ )
// 			fwrite( ((U8*) m_LockedResource.pData) + SliceIndex * m_LockedResource.depthPitch, m_LockedResource.depthPitch, 1, pFile );
// 		UnMap( MipLevelIndex );
// 
// 		Depth = MAX( 1, Depth >> 1 );
// 	}
// 
// 	// We're done!
// 	fclose( pFile );
}

void	Texture3D::Load( const char* _pFileName ) {
	TextureFilePOM	POM;
	POM.Load( _pFileName );

	// Read up content
 	int	Depth = m_depth;
	for ( U32 MipLevelIndex=0; MipLevelIndex < m_mipLevelsCount; MipLevelIndex++ ) {
		Map( MipLevelIndex );

		ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].rowPitch == m_lockedResource.RowPitch, "Incompatible row pitch!" );
		ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].depthPitch == m_lockedResource.DepthPitch, "Incompatible depth pitch!" );

		memcpy_s( m_lockedResource.pData, Depth * m_lockedResource.DepthPitch, POM.m_ppContent[MipLevelIndex], Depth * m_lockedResource.DepthPitch );
		UnMap( MipLevelIndex );

		Depth = MAX( 1, Depth >> 1 );
	}

// 	FILE*	pFile;
// 	fopen_s( &pFile, _pFileName, "rb" );
// 	ASSERT( pFile != NULL, "Can't load file!" );
// 
// 	// Read the type and format
// 	U8		Type, Format;
// 	fread_s( &Type, sizeof(U8), sizeof(U8), 1, pFile );
// 	fread_s( &Format, sizeof(U8), sizeof(U8), 1, pFile );
// 	DXGI_FORMAT	FileFormat = DXGI_FORMAT( Format );
// 	ASSERT( FileFormat == m_Format.DirectXFormat(), "Incompatible format!" );
// 	ASSERT( Type == 0x02, "File is not a texture 3D!" );
// 
// 	// Read the dimensions
// 	int	W, H, D, M;
// 	fread_s( &W, sizeof(int), sizeof(int), 1, pFile );
// 	fread_s( &H, sizeof(int), sizeof(int), 1, pFile );
// 	fread_s( &D, sizeof(int), sizeof(int), 1, pFile );
// 	fread_s( &M, sizeof(int), sizeof(int), 1, pFile );
// 
// 	ASSERT( W == m_Width, "Incompatible width!" );
// 	ASSERT( H == m_Height, "Incompatible height!" );
// 	ASSERT( D == m_Depth, "Incompatible depth!" );
// 	ASSERT( M == m_MipLevelsCount, "Incompatible mip levels count!" );
// 
// 	// Read each mip
// 	int	Depth = m_Depth;
// 	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
// 	{
// 		Map( MipLevelIndex );
// 		int	rowPitch, depthPitch;
// 		fread_s( &rowPitch, sizeof(int), sizeof(int), 1, pFile );
// 		fread_s( &depthPitch, sizeof(int), sizeof(int), 1, pFile );
// 		ASSERT( rowPitch == m_LockedResource.rowPitch, "Incompatible row pitch!" );
// 		ASSERT( depthPitch == m_LockedResource.depthPitch, "Incompatible depth pitch!" );
// 
// 		for ( int SliceIndex=0; SliceIndex < Depth; SliceIndex++ )
// 			fread_s( ((U8*) m_LockedResource.pData) + SliceIndex * m_LockedResource.depthPitch, m_LockedResource.depthPitch, m_LockedResource.depthPitch, 1, pFile );
// 
// 		UnMap( MipLevelIndex );
// 
// 		Depth = MAX( 1, Depth >> 1 );
// 	}
// 
// 	// We're done!
// 	fclose( pFile );
}

Texture3D::Texture3D( Device& _Device, const TextureFilePOM& _POM, bool _UAV )
	: Component( _Device )
	, m_width( _POM.m_Width )
	, m_height( _POM.m_Height )
	, m_depth( _POM.m_ArraySizeOrDepth )
	, m_Format( *_POM.m_pPixelFormat )
	, m_mipLevelsCount( _POM.m_MipsCount ) {
	Init( _POM.m_ppContent, false, _UAV, _POM.m_pMipsDescriptors );
}

#endif
