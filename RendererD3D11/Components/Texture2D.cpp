#include "stdafx.h"

#include "Texture2D.h"

Texture2D::Texture2D( Device& _device, ID3D11Texture2D& _Texture )//, const BaseLib::IPixelAccessor& _format, BaseLib::COMPONENT_FORMAT _componentFormat )
	: Component( _device )
// 	, m_pixelFormat( &_format )
// 	, m_componentFormat( _componentFormat )
	, m_depthFormat( NULL )
	, m_isCubeMap( false )
{
	D3D11_TEXTURE2D_DESC	desc;
	_Texture.GetDesc( &desc );

	// 
	U32		pixelSize;
	ImageUtilityLib::ImageFile::PIXEL_FORMAT	pixelFormat = ImageUtilityLib::ImageFile::DXGIFormat2PixelFormat( desc.Format, m_componentFormat, pixelSize );
	m_pixelFormat = &ImageUtilityLib::ImageFile::PixelFormat2Accessor( pixelFormat );

	m_width = desc.Width;
	m_height = desc.Height;
	m_arraySize = desc.ArraySize;
	m_mipLevelsCount = desc.MipLevels;

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_lastAssignedSlots[ShaderStageIndex] = -1;
	m_lastAssignedSlotsUAV = -1;

	m_texture = &_Texture;
}

Texture2D::Texture2D( Device& _device, U32 _width, U32 _height, int _arraySize, U32 _mipLevelsCount, const BaseLib::IPixelAccessor& _format, BaseLib::COMPONENT_FORMAT _componentFormat, const void* const* _ppContent, bool _staging, bool _UAV )
	: Component( _device )
	, m_width( _width )
	, m_height( _height )
	, m_pixelFormat( &_format )
	, m_componentFormat( _componentFormat )
	, m_depthFormat( NULL )
	, m_mipLevelsCount( _mipLevelsCount )
	, m_isCubeMap( false )
{
	if ( _arraySize < 0 ) {
		// Special cube map case!
		ASSERT( m_width == m_height, "When creating a cube map, width & height must match!" );
		m_arraySize = -_arraySize;
		ASSERT( (m_arraySize % 6) == 0, "Cube map array size must be multiples of 6!" );
		m_isCubeMap = true;
	} else {
		// Regular case
		m_arraySize = _arraySize;
	}

	Init( _ppContent, _staging, _UAV );
}

Texture2D::Texture2D( Device& _device, const ImageUtilityLib::ImagesMatrix& _images, BaseLib::COMPONENT_FORMAT _componentFormat )
	: Component( _device )
{
	ASSERT( _images.GetType() == ImageUtilityLib::ImagesMatrix::TYPE::TEXTURE2D || _images.GetType() == ImageUtilityLib::ImagesMatrix::TYPE::TEXTURECUBE, "Invalid images matrix type!" );
	m_isCubeMap = _images.GetType() == ImageUtilityLib::ImagesMatrix::TYPE::TEXTURECUBE;

	m_arraySize = _images.GetArraySize();
	ASSERT( m_arraySize > 0, "Invalid array size!" );
	m_mipLevelsCount = _images[0].GetMipLevelsCount();
	ASSERT( m_mipLevelsCount > 0, "Invalid mip levels count!" );

	// Retrieve default image size
	const ImageUtilityLib::ImagesMatrix::Mips::Mip&	referenceMip = _images[0][0];
	m_width = referenceMip.Width();
	m_height = referenceMip.Height();
	ASSERT( m_width <= MAX_TEXTURE_SIZE, "Texture size out of range!" );
	ASSERT( m_height <= MAX_TEXTURE_SIZE, "Texture size out of range!" );

	// Retrieve image format
	m_pixelFormat = &ImageUtilityLib::ImageFile::PixelFormat2Accessor( _images.GetFormat() );
	m_componentFormat = _componentFormat;
	m_depthFormat = NULL;
	DXGI_FORMAT	textureFormat = ImageUtilityLib::ImageFile::PixelFormat2DXGIFormat( _images.GetFormat(), _componentFormat );

	// Prepare main descriptor
	D3D11_TEXTURE2D_DESC	desc;
	desc.Width = m_width;
	desc.Height = m_height;
	desc.ArraySize = m_arraySize;
	desc.MipLevels = m_mipLevelsCount;
	desc.Format = textureFormat;
	desc.SampleDesc.Count = 1;
	desc.SampleDesc.Quality = 0;
	desc.Usage = D3D11_USAGE_IMMUTABLE;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
	desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	desc.MiscFlags = m_isCubeMap ? D3D11_RESOURCE_MISC_TEXTURECUBE : 0;

	// Prepare individual sub-resource descriptors
	D3D11_SUBRESOURCE_DATA*	subResourceDescriptors = new D3D11_SUBRESOURCE_DATA[m_mipLevelsCount*m_arraySize];

	for ( U32 arrayIndex=0; arrayIndex < m_arraySize; arrayIndex++ ) {
		const ImageUtilityLib::ImagesMatrix::Mips&	mips = _images[arrayIndex];
		ASSERT( mips.GetMipLevelsCount() == m_mipLevelsCount, "Mip levels count mismatch!" );

		U32	W = m_width;
		U32	H = m_height;
		for ( U32 mipLevelIndex=0; mipLevelIndex < m_mipLevelsCount; mipLevelIndex++ ) {
			const ImageUtilityLib::ImagesMatrix::Mips::Mip&	mip = mips[mipLevelIndex];
			ASSERT( mip.Width() == W && mip.Height() == H, "Mip's width/height mismatch!" );
			ASSERT( mip.Depth() == 1, "Unexpected mip depth! Must be 1 for 2D textures. Other slices will simply be ignored..." );
			const ImageUtilityLib::ImageFile*	mipImage = mip[0];
			RELEASE_ASSERT( mipImage != NULL, "Invalid mip image!" );

			U32	rowPitch = mipImage->Pitch();
			U32	depthPitch = H * rowPitch;

			subResourceDescriptors[arrayIndex*m_mipLevelsCount+mipLevelIndex].pSysMem = mipImage->GetBits();
			subResourceDescriptors[arrayIndex*m_mipLevelsCount+mipLevelIndex].SysMemPitch = rowPitch;
			subResourceDescriptors[arrayIndex*m_mipLevelsCount+mipLevelIndex].SysMemSlicePitch = depthPitch;

			NextMipSize( W, H );
		}
	}

	Check( m_device.DXDevice().CreateTexture2D( &desc, subResourceDescriptors, &m_texture ) );

	delete[] subResourceDescriptors;

	// Clear last assignment slots
	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_lastAssignedSlots[ShaderStageIndex] = ~0U;
	m_lastAssignedSlotsUAV = ~0U;
}

Texture2D::Texture2D( Device& _device, U32 _width, U32 _height, U32 _arraySize, const BaseLib::IDepthAccessor& _format )
	: Component( _device )
	, m_pixelFormat( NULL )
	, m_componentFormat( BaseLib::COMPONENT_FORMAT::AUTO )
	, m_depthFormat( &_format )
	, m_isCubeMap( false )
{
	ASSERT( _width <= MAX_TEXTURE_SIZE, "Texture size out of range!" );
	ASSERT( _height <= MAX_TEXTURE_SIZE, "Texture size out of range!" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_lastAssignedSlots[ShaderStageIndex] = -1;
	m_lastAssignedSlotsUAV = -1;

	m_width = _width;
	m_height = _height;
	m_arraySize = _arraySize;
	m_mipLevelsCount = 1;

	D3D11_TEXTURE2D_DESC	Desc;
	Desc.Width = m_width;
	Desc.Height = m_height;
	Desc.ArraySize = m_arraySize;
	Desc.MipLevels = 1;
	Desc.Format = PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	Desc.SampleDesc.Count = 1;
	Desc.SampleDesc.Quality = 0;
	Desc.Usage = D3D11_USAGE_DEFAULT;
	Desc.BindFlags = D3D11_BIND_DEPTH_STENCIL | D3D11_BIND_SHADER_RESOURCE;
	Desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
	Desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG( 0 );

	Check( m_device.DXDevice().CreateTexture2D( &Desc, NULL, &m_texture ) );
}

static void		ReleaseDirectXObject( int _EntryIndex, void*& _pValue, void* _pUserData ) {
	IUnknown*	pObject = (IUnknown*) _pValue;
	pObject->Release();
}

Texture2D::~Texture2D() {
	ASSERT( m_texture != NULL, "Invalid texture to destroy!" );

	m_cachedSRVs.ForEach( ReleaseDirectXObject, NULL );
	m_cachedRTVs.ForEach( ReleaseDirectXObject, NULL );
	m_cachedUAVs.ForEach( ReleaseDirectXObject, NULL );
	m_cachedDSVs.ForEach( ReleaseDirectXObject, NULL );

	m_texture->Release();
	m_texture = NULL;
}

void	Texture2D::Init( const void* const* _ppContent, bool _staging, bool _UAV, TextureFilePOM::MipDescriptor* _pMipDescriptors ) {
	ASSERT( m_width <= MAX_TEXTURE_SIZE, "Texture size out of range!" );
	ASSERT( m_height <= MAX_TEXTURE_SIZE, "Texture size out of range!" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_lastAssignedSlots[ShaderStageIndex] = -1;
	m_lastAssignedSlotsUAV = -1;

	m_mipLevelsCount = ComputeMipLevelsCount( m_width, m_height, m_mipLevelsCount );

	D3D11_TEXTURE2D_DESC	desc;
	desc.Width = m_width;
	desc.Height = m_height;
	desc.ArraySize = m_arraySize;
	desc.MipLevels = m_mipLevelsCount;
	desc.Format = PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	desc.SampleDesc.Count = 1;
	desc.SampleDesc.Quality = 0;
	if ( _staging ) {
		desc.Usage = D3D11_USAGE_STAGING;
		desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | D3D11_CPU_ACCESS_WRITE;
		desc.BindFlags = 0;
		desc.MiscFlags = 0;
	} else {
		desc.Usage = _ppContent != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DEFAULT;
		desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
		desc.BindFlags = _ppContent != NULL ? D3D11_BIND_SHADER_RESOURCE : (D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE | (_UAV ? D3D11_BIND_UNORDERED_ACCESS: 0));
		desc.MiscFlags = m_isCubeMap ? D3D11_RESOURCE_MISC_TEXTURECUBE : 0;
	}

	if ( _ppContent != NULL ) {
		D3D11_SUBRESOURCE_DATA*	pInitialData = new D3D11_SUBRESOURCE_DATA[m_mipLevelsCount*m_arraySize];

		for ( U32 ArrayIndex=0; ArrayIndex < m_arraySize; ArrayIndex++ ) {
			U32	Width = m_width;
			U32	Height = m_height;
			for ( U32 MipLevelIndex=0; MipLevelIndex < m_mipLevelsCount; MipLevelIndex++ ) {
				U32	rowPitch = _pMipDescriptors != NULL ? _pMipDescriptors[MipLevelIndex].rowPitch : Width * m_pixelFormat->Size();
				U32	depthPitch = _pMipDescriptors != NULL ? _pMipDescriptors[MipLevelIndex].depthPitch : Height * rowPitch;

				pInitialData[ArrayIndex*m_mipLevelsCount+MipLevelIndex].pSysMem = _ppContent[ArrayIndex*m_mipLevelsCount+MipLevelIndex];
				pInitialData[ArrayIndex*m_mipLevelsCount+MipLevelIndex].SysMemPitch = rowPitch;
				pInitialData[ArrayIndex*m_mipLevelsCount+MipLevelIndex].SysMemSlicePitch = depthPitch;
				NextMipSize( Width, Height );
			}
		}

		Check( m_device.DXDevice().CreateTexture2D( &desc, pInitialData, &m_texture ) );

		delete[] pInitialData;
	}
	else
		Check( m_device.DXDevice().CreateTexture2D( &desc, NULL, &m_texture ) );
}

ID3D11ShaderResourceView*	Texture2D::GetSRV( U32 _mipLevelStart, U32 _mipLevelsCount, U32 _arrayStart, U32 _arraySize, bool _asArray ) const {
	if ( _arraySize == 0 )
		_arraySize = m_arraySize - _arrayStart;
	if ( _mipLevelsCount == 0 )
		_mipLevelsCount = m_mipLevelsCount - _mipLevelStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | ((_MipLevelsCount | (_MipLevelStart << 4)) << 12)) << 12);
	U32	Hash = (_mipLevelStart << 0) | (_arrayStart << 4) | (_mipLevelsCount << (4+12)) | (_arraySize << (4+12+4));	// Re-organized to have most likely changes (i.e. mip & array starts) first
		Hash ^= _asArray ? 0x80000000UL : 0;

	ID3D11ShaderResourceView*	pExistingView = (ID3D11ShaderResourceView*) m_cachedSRVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
//	Desc.Format = m_depthFormat != NULL ? ((IDepthStencilFormatDescriptor*) m_format)->ReadableDirectXFormat() : m_format->DirectXFormat();
	Desc.Format = m_depthFormat != NULL ? DepthAccessor2DXGIFormat( *m_depthFormat, DEPTH_ACCESS_TYPE::VIEW_READABLE ) : PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	if ( _asArray ) {
		// Force as a Texture2DArray
		Desc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
		Desc.Texture2DArray.MostDetailedMip = _mipLevelStart;
		Desc.Texture2DArray.MipLevels = _mipLevelsCount;
		Desc.Texture2DArray.FirstArraySlice = _arrayStart;
		Desc.Texture2DArray.ArraySize = _arraySize;
	} else {
		Desc.ViewDimension = m_arraySize > 1 ? (m_isCubeMap ? (m_arraySize > 6 ? D3D11_SRV_DIMENSION_TEXTURECUBEARRAY : D3D11_SRV_DIMENSION_TEXTURECUBE) : D3D11_SRV_DIMENSION_TEXTURE2DARRAY) : D3D11_SRV_DIMENSION_TEXTURE2D;
		if ( m_isCubeMap ) {
			Desc.TextureCubeArray.MostDetailedMip = _mipLevelStart;
			Desc.TextureCubeArray.MipLevels = _mipLevelsCount;
			Desc.TextureCubeArray.First2DArrayFace = _arrayStart;
			Desc.TextureCubeArray.NumCubes = _arraySize / 6;
		} else {
			Desc.Texture2DArray.MostDetailedMip = _mipLevelStart;
			Desc.Texture2DArray.MipLevels = _mipLevelsCount;
			Desc.Texture2DArray.FirstArraySlice = _arrayStart;
			Desc.Texture2DArray.ArraySize = _arraySize;
		}
	}

	ID3D11ShaderResourceView*	pView;
	Check( m_device.DXDevice().CreateShaderResourceView( m_texture, &Desc, &pView ) );

	m_cachedSRVs.Add( Hash, pView );

	return pView;
}

ID3D11RenderTargetView*		Texture2D::GetRTV( U32 _mipLevelIndex, U32 _ArrayStart, U32 _ArraySize ) const {
	if ( _ArraySize == 0 )
		_ArraySize = m_arraySize - _ArrayStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | (_mipLevelIndex << 12)) << 12);
	U32	Hash = (_mipLevelIndex << 0) | (_ArrayStart << 4) | (_ArraySize << (4+12));	// Re-organized to have most likely changes (i.e. mip & array starts) first
	ID3D11RenderTargetView*	pExistingView = (ID3D11RenderTargetView*) m_cachedRTVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_RENDER_TARGET_VIEW_DESC	Desc;
	Desc.Format = PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	Desc.ViewDimension = m_arraySize > 1 ? D3D11_RTV_DIMENSION_TEXTURE2DARRAY : D3D11_RTV_DIMENSION_TEXTURE2D;
	Desc.Texture2DArray.MipSlice = _mipLevelIndex;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11RenderTargetView*	pView;
	Check( m_device.DXDevice().CreateRenderTargetView( m_texture, &Desc, &pView ) );

	m_cachedRTVs.Add( Hash, pView );

	return pView;
}

ID3D11UnorderedAccessView*	Texture2D::GetUAV( U32 _mipLevelIndex, U32 _ArrayStart, U32 _ArraySize ) const {
	if ( _ArraySize == 0 )
		_ArraySize = m_arraySize - _ArrayStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | (_mipLevelIndex << 12)) << 12);
	U32	Hash = (_mipLevelIndex << 0) | (_ArrayStart << 4) | (_ArraySize << (4+12));	// Re-organized to have most likely changes (i.e. mip & array starts) first
	ID3D11UnorderedAccessView*	pExistingView = (ID3D11UnorderedAccessView*) m_cachedUAVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_UNORDERED_ACCESS_VIEW_DESC	Desc;
	Desc.Format = PixelAccessor2DXGIFormat( *m_pixelFormat, m_componentFormat );
	Desc.ViewDimension = m_arraySize > 1 ? D3D11_UAV_DIMENSION_TEXTURE2DARRAY : D3D11_UAV_DIMENSION_TEXTURE2D;
	Desc.Texture2DArray.MipSlice = _mipLevelIndex;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11UnorderedAccessView*	pView = NULL;
	Check( m_device.DXDevice().CreateUnorderedAccessView( m_texture, &Desc, &pView ) );

	m_cachedUAVs.Add( Hash, pView );

	return pView;
}

ID3D11DepthStencilView*		Texture2D::GetDSV( U32 _ArrayStart, U32 _ArraySize ) const {
	if ( _ArraySize == 0 )
		_ArraySize = m_arraySize - _ArrayStart;

	// Check if we already have it
	U32	Hash = (_ArrayStart << 0) | (_ArraySize << 12);
	ID3D11DepthStencilView*	pExistingView = (ID3D11DepthStencilView*) m_cachedDSVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	D3D11_DEPTH_STENCIL_VIEW_DESC	Desc;
//	Desc.Format = ((IDepthStencilFormatDescriptor&) m_format).WritableDirectXFormat();
	Desc.Format = DepthAccessor2DXGIFormat( *m_depthFormat, DEPTH_ACCESS_TYPE::VIEW_WRITABLE );
	Desc.ViewDimension = m_arraySize == 1 ? D3D11_DSV_DIMENSION_TEXTURE2D : D3D11_DSV_DIMENSION_TEXTURE2DARRAY;
	Desc.Flags = 0;
	Desc.Texture2DArray.MipSlice = 0;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11DepthStencilView*	pView = NULL;
	Check( m_device.DXDevice().CreateDepthStencilView( m_texture, &Desc, &pView ) );

	m_cachedDSVs.Add( Hash, pView );

	return pView;
}

void	Texture2D::Set( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const {
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
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

void	Texture2D::SetVS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().VSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[0] = _SlotIndex;
}
void	Texture2D::SetHS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().HSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[1] = _SlotIndex;
}
void	Texture2D::SetDS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().DSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[2] = _SlotIndex;
}
void	Texture2D::SetGS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().GSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[3] = _SlotIndex;
}
void	Texture2D::SetPS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().PSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[4] = _SlotIndex;
}
void	Texture2D::SetCS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().CSSetShaderResources( _SlotIndex, 1, &_pView );
	m_lastAssignedSlots[5] = _SlotIndex;
}

void	Texture2D::RemoveFromLastAssignedSlots() const {
	Device::SHADER_STAGE_FLAGS	pStageFlags[] = {
		Device::SSF_VERTEX_SHADER,
		Device::SSF_HULL_SHADER,
		Device::SSF_DOMAIN_SHADER,
		Device::SSF_GEOMETRY_SHADER,
		Device::SSF_PIXEL_SHADER,
		Device::SSF_COMPUTE_SHADER,
	};
	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		if ( m_lastAssignedSlots[ShaderStageIndex] != -1 ) {
			m_device.RemoveShaderResources( m_lastAssignedSlots[ShaderStageIndex], 1, pStageFlags[ShaderStageIndex] );
			m_lastAssignedSlots[ShaderStageIndex] = -1;
		}
}

// UAV setting
void	Texture2D::SetCSUAV( U32 _SlotIndex, ID3D11UnorderedAccessView* _pView ) const {
	_pView = _pView != NULL ? _pView : GetUAV( 0, 0, 0 );
	UINT	InitialCount = -1;
	m_device.DXContext().CSSetUnorderedAccessViews( _SlotIndex, 1, &_pView, &InitialCount );
	m_lastAssignedSlotsUAV = _SlotIndex;
}

void	Texture2D::RemoveFromLastAssignedSlotUAV() const {
	ID3D11UnorderedAccessView*	pNULL = NULL;
	UINT	InitialCount = -1;
	if ( m_lastAssignedSlotsUAV != -1 )
		m_device.DXContext().CSSetUnorderedAccessViews( m_lastAssignedSlotsUAV, 1, &pNULL, &InitialCount );
	m_lastAssignedSlotsUAV = -1;
}

void	Texture2D::CopyFrom( Texture2D& _SourceTexture ) {
	ASSERT( _SourceTexture.m_width == m_width && _SourceTexture.m_height == m_height, "Size mismatch!" );
	ASSERT( _SourceTexture.m_arraySize == m_arraySize, "Array size mismatch!" );
	ASSERT( _SourceTexture.m_mipLevelsCount == m_mipLevelsCount, "Mips count mismatch!" );
	ASSERT( _SourceTexture.m_pixelFormat == m_pixelFormat || _SourceTexture.m_depthFormat == m_depthFormat, "Format mismatch!" );

	m_device.DXContext().CopyResource( m_texture, _SourceTexture.m_texture );
}

const D3D11_MAPPED_SUBRESOURCE&	Texture2D::MapRead( U32 _mipLevelIndex, U32 _arrayIndex ) const {
//	Device&	notConstDevice = const_cast< Device& >( m_device );
//	Check( notConstDevice.DXContext().Map( const_cast<ID3D11Texture2D*>( m_texture ), CalcSubResource( _mipLevelIndex, _arrayIndex ), D3D11_MAP_READ, 0, &m_lockedResource ) );
	Check( m_device.DXContext().Map( m_texture, CalcSubResource( _mipLevelIndex, _arrayIndex ), D3D11_MAP_READ, 0, &m_lockedResource ) );
	return m_lockedResource;
}
const D3D11_MAPPED_SUBRESOURCE&	Texture2D::MapWrite( U32 _mipLevelIndex, U32 _arrayIndex ) {
	Check( m_device.DXContext().Map( m_texture, CalcSubResource( _mipLevelIndex, _arrayIndex ), D3D11_MAP_WRITE, 0, &m_lockedResource ) );
	return m_lockedResource;
}

void	Texture2D::UnMap( U32 _mipLevelIndex, U32 _arrayIndex ) const {
	m_device.DXContext().Unmap( m_texture, CalcSubResource( _mipLevelIndex, _arrayIndex ) );
}

void	Texture2D::ReadAsImagesMatrix( ImageUtilityLib::ImagesMatrix& _images ) const {
	if ( m_depthFormat != NULL ) {
//		_images.InitTexture2DArray( m_width, m_height, m_arraySize, m_mipLevelsCount );
		RELEASE_ASSERT( false, "TODO!" );
		return;
	}

	// Initialize the matrix to the proper dimensions
	if ( IsCubeMap() )
		_images.InitCubeTextureArray( m_width, m_arraySize / 6, m_mipLevelsCount );
	else
		_images.InitTexture2DArray( m_width, m_height, m_arraySize, m_mipLevelsCount );

	// Allocate actual images
	ImageUtilityLib::ImageFile::PIXEL_FORMAT	format = ImageUtilityLib::ImageFile::Accessor2PixelFormat( *m_pixelFormat );
	ImageUtilityLib::ColorProfile				dummyProfile( m_componentFormat == BaseLib::COMPONENT_FORMAT::UNORM_sRGB ? ImageUtilityLib::ColorProfile::STANDARD_PROFILE::sRGB : ImageUtilityLib::ColorProfile::STANDARD_PROFILE::LINEAR );
	_images.AllocateImageFiles( format, dummyProfile );

	// Fill up each image with mapped content
	for ( U32 arrayIndex=0; arrayIndex < m_arraySize; arrayIndex++ ) {
		ImageUtilityLib::ImagesMatrix::Mips&	mips = _images[arrayIndex];
		for ( U32 mipLevelIndex=0; mipLevelIndex < m_mipLevelsCount; mipLevelIndex++ ) {
			ImageUtilityLib::ImagesMatrix::Mips::Mip&	targetMip = mips[mipLevelIndex];
			ImageUtilityLib::ImageFile&					targetImage = *targetMip[0];

			const D3D11_MAPPED_SUBRESOURCE&	mappedSourceMip = MapRead( mipLevelIndex, arrayIndex );
			const U8*		sourceData = reinterpret_cast<U8*>( mappedSourceMip.pData );
			U8*				targetData = targetImage.GetBits();
			U32				targetPitch = targetImage.Pitch();
			for ( U32 Y=0; Y < targetMip.Height(); Y++ ) {
				const U8*	sourceScanline = sourceData + mappedSourceMip.RowPitch * Y;
				U8*			targetScanline = targetData + targetPitch * Y;
				memcpy_s( targetScanline, targetPitch, sourceScanline, mappedSourceMip.RowPitch );
			}
			UnMap( mipLevelIndex, arrayIndex );
		}
	}
}

DXGI_FORMAT	Texture2D::PixelAccessor2DXGIFormat( const BaseLib::IPixelAccessor& _pixelAccessor, BaseLib::COMPONENT_FORMAT _componentFormat ) {
	// Let's re-use the work done in ImageFile to convert to DXGI format!
	ImageUtilityLib::ImageFile::PIXEL_FORMAT	imagePixelFormat = ImageUtilityLib::ImageFile::Accessor2PixelFormat( _pixelAccessor );
	DXGI_FORMAT	textureFormat = ImageUtilityLib::ImageFile::PixelFormat2DXGIFormat( imagePixelFormat, _componentFormat );
	return textureFormat;
/*
	switch ( _pixelAccessor.Size() ) {
	case 1:
		if ( &_pixelAccessor == &BaseLib::PF_R8::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R8_UNORM;
			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R8_SNORM;
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R8_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R8_SINT;
			}
		}
		break;

	case 2:
		if ( &_pixelAccessor == &BaseLib::PF_RG8::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R8G8_UNORM;
			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R8G8_SNORM;
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R8G8_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R8G8_SINT;
			}
		}
		// ======= 16-bits floating-point formats =======

		if ( &_pixelAccessor == &BaseLib::PF_R16F::Descriptor ) {
			return DXGI_FORMAT_R16_FLOAT;
		} else
		else if ( &_pixelAccessor == &BaseLib::PF_RG16F::Descriptor ) {
			return DXGI_FORMAT_R16G16_FLOAT;
		} else if ( &_pixelAccessor == &BaseLib::PF_RGBA16F::Descriptor ) {
			return DXGI_FORMAT_R16G16B16A16_FLOAT;
		}

		// ======= 16-bits integer formats =======
		if ( &_pixelAccessor == &BaseLib::PF_R16::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R16_UNORM;
			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R16_SNORM;
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R16_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R16_SINT;
			}
		} else if ( &_pixelAccessor == &BaseLib::PF_RG16::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R16G16_UNORM;
			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R16G16_SNORM;
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R16G16_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R16G16_SINT;
			}
		} else if ( &_pixelAccessor == &BaseLib::PF_RGBA16::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R16G16B16A16_UNORM;
			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R16G16B16A16_SNORM;
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R16G16B16A16_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R16G16B16A16_SINT;
			}
		}
		break;

	case 4:
		// ======= 32-bits floating-point formats =======
		if ( &_pixelAccessor == &BaseLib::PF_R32F::Descriptor ) {
			return DXGI_FORMAT_R32_FLOAT;
		} else if ( &_pixelAccessor == &BaseLib::PF_RGBA8::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
			case BaseLib::COMPONENT_FORMAT::UNORM_sRGB:	return DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R8G8B8A8_UNORM;
			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R8G8B8A8_SNORM;
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R8G8B8A8_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R8G8B8A8_SINT;
			}
		}
c'est tout faux partout ici!
		
		else if ( &_pixelAccessor == &BaseLib::PF_RG32F::Descriptor ) {
			return DXGI_FORMAT_R32G32_FLOAT;
		} else if ( &_pixelAccessor == &BaseLib::PF_RGBA32F::Descriptor ) {
			return DXGI_FORMAT_R32G32B32A32_FLOAT;
		}

		// ======= 32-bits integer formats =======
		if ( &_pixelAccessor == &BaseLib::PF_R32::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
// 			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R32_UNORM;	// Doesn't exist anyway
// 			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R32_SNORM;	// Doesn't exist anyway
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R32_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R32_SINT;
			}
		} else if ( &_pixelAccessor == &BaseLib::PF_RG32::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
// 			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R32G32_UNORM;	// Doesn't exist anyway
// 			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R32G32_SNORM;	// Doesn't exist anyway
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R32G32_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R32G32_SINT;
			}
		} else if ( &_pixelAccessor == &BaseLib::PF_RGBA32::Descriptor ) {
			switch ( _componentFormat ) {
			case BaseLib::COMPONENT_FORMAT::AUTO:
// 			case BaseLib::COMPONENT_FORMAT::UNORM:	return DXGI_FORMAT_R32G32B32A32_UNORM;	// Doesn't exist anyway
// 			case BaseLib::COMPONENT_FORMAT::SNORM:	return DXGI_FORMAT_R32G32B32A32_SNORM;	// Doesn't exist anyway
			case BaseLib::COMPONENT_FORMAT::UINT:	return DXGI_FORMAT_R32G32B32A32_UINT;
			case BaseLib::COMPONENT_FORMAT::SINT:	return DXGI_FORMAT_R32G32B32A32_SINT;
			}
		}
		break;
	}

	return DXGI_FORMAT_UNKNOWN;*/
}

DXGI_FORMAT	Texture2D::DepthAccessor2DXGIFormat( const BaseLib::IDepthAccessor& _depthAccessor, DEPTH_ACCESS_TYPE _accessType ) {
	if ( &_depthAccessor == &BaseLib::PF_D32::Descriptor ) {
		switch ( _accessType ) {
			case DEPTH_ACCESS_TYPE::SURFACE_CREATION:		return DXGI_FORMAT_R32_TYPELESS;
			case DEPTH_ACCESS_TYPE::VIEW_READABLE:			return DXGI_FORMAT_R32_FLOAT;
			case DEPTH_ACCESS_TYPE::VIEW_WRITABLE:			return DXGI_FORMAT_D32_FLOAT;
		}
	} else if ( &_depthAccessor == &BaseLib::PF_D24S8::Descriptor ) {
		switch ( _accessType ) {
			case DEPTH_ACCESS_TYPE::SURFACE_CREATION:		return DXGI_FORMAT_R24G8_TYPELESS;
			case DEPTH_ACCESS_TYPE::VIEW_READABLE:			return DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
			case DEPTH_ACCESS_TYPE::VIEW_WRITABLE:			return DXGI_FORMAT_D24_UNORM_S8_UINT;
		}
	} else if ( &_depthAccessor == &BaseLib::PF_D16::Descriptor ) {
		switch ( _accessType ) {
			case DEPTH_ACCESS_TYPE::SURFACE_CREATION:		return DXGI_FORMAT_R16_TYPELESS;
			case DEPTH_ACCESS_TYPE::VIEW_READABLE:			return DXGI_FORMAT_R16_UNORM;
			case DEPTH_ACCESS_TYPE::VIEW_WRITABLE:			return DXGI_FORMAT_D16_UNORM;
		}
	}

	return DXGI_FORMAT_UNKNOWN;
}

void	Texture2D::NextMipSize( U32& _Width, U32& _Height ) {
	_Width = MAX( 1U, _Width >> 1 );
	_Height = MAX( 1U, _Height >> 1 );
}

U32	 Texture2D::ComputeMipLevelsCount( U32 _Width, U32 _Height, U32 _MipLevelsCount ) {
	U32 MaxSize = MAX( _Width, _Height );
	U32	MaxMipLevelsCount = U32( ceilf( logf( MaxSize+1.0f ) / logf( 2.0f ) ) );
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = MaxMipLevelsCount;
	else
		_MipLevelsCount = MIN( _MipLevelsCount, MaxMipLevelsCount );

	ASSERT( _MipLevelsCount <= MAX_TEXTURE_POT, "Texture mip level out of range !" );
	return _MipLevelsCount;
}

U32	Texture2D::CalcSubResource( U32 _mipLevelIndex, U32 _arrayIndex ) const {
	return _mipLevelIndex + (_arrayIndex * m_mipLevelsCount);
}

#if 0//defined(_DEBUG) || !defined(GODCOMPLEX)

#include "..\..\Utility\TextureFilePOM.h"

// I/O for staging textures
void	Texture2D::Save( const char* _pFileName )
{
	TextureFilePOM	POM;
	POM.AllocateContent( *this );

	// Fill up content
	for ( U32 MipLevelIndex=0; MipLevelIndex < m_mipLevelsCount; MipLevelIndex++ ) {
		for ( U32 SliceIndex=0; SliceIndex < m_arraySize; SliceIndex++ ) {
			Map( MipLevelIndex, SliceIndex );

			if ( SliceIndex == 0 ) {
				// Save mip infos only once
				POM.m_pMipsDescriptors[MipLevelIndex].rowPitch = m_lockedResource.RowPitch;
				POM.m_pMipsDescriptors[MipLevelIndex].depthPitch = m_lockedResource.DepthPitch;
			}

			POM.m_ppContent[MipLevelIndex+m_mipLevelsCount*SliceIndex] = new void*[m_lockedResource.DepthPitch];
			memcpy_s( POM.m_ppContent[MipLevelIndex+m_mipLevelsCount*SliceIndex], m_lockedResource.DepthPitch, m_lockedResource.pData, m_lockedResource.DepthPitch );

			UnMap( MipLevelIndex, SliceIndex );
		}
	}

	POM.Save( _pFileName );
}

void	Texture2D::Load( const char* _pFileName ) {
	TextureFilePOM	POM;
	POM.Load( _pFileName );

	// Read up content
	for ( U32 MipLevelIndex=0; MipLevelIndex < m_mipLevelsCount; MipLevelIndex++ ) {
		for ( U32 SliceIndex=0; SliceIndex < m_arraySize; SliceIndex++ ) {
			Map( MipLevelIndex, SliceIndex );

			if ( SliceIndex == 0 ) {
				// Test only once
				ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].rowPitch == m_lockedResource.RowPitch, "Incompatible row pitch!" );
				ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].depthPitch == m_lockedResource.DepthPitch, "Incompatible depth pitch!" );
			}

			memcpy_s( m_lockedResource.pData, m_lockedResource.DepthPitch, POM.m_ppContent[MipLevelIndex+m_mipLevelsCount*SliceIndex], m_lockedResource.DepthPitch );
			UnMap( MipLevelIndex, SliceIndex );
		}
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
// 	ASSERT( Type == 0x00 || Type == 0x01, "File is not a texture 2D or a cube map!" );
// 	m_bIsCubeMap = Type == 0x01;
// 
// 	// Read the dimensions
// 	int	W, H, A, M;
// 	fread_s( &W, sizeof(int), sizeof(int), 1, pFile );
// 	fread_s( &H, sizeof(int), sizeof(int), 1, pFile );
// 	fread_s( &A, sizeof(int), sizeof(int), 1, pFile );
// 	fread_s( &M, sizeof(int), sizeof(int), 1, pFile );
// 
// 	ASSERT( W == m_Width, "Incompatible width!" );
// 	ASSERT( H == m_Height, "Incompatible height!" );
// 	ASSERT( A == m_ArraySize, "Incompatible array size!" );
// 	ASSERT( M == m_MipLevelsCount, "Incompatible mip levels count!" );
// 
// 	// Read each slice
// 	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
// 	{
// 		int	rowPitch, depthPitch;
// 		fread_s( &rowPitch, sizeof(int), sizeof(int), 1, pFile );
// 		fread_s( &depthPitch, sizeof(int), sizeof(int), 1, pFile );
// 
// 		for ( int SliceIndex=0; SliceIndex < m_ArraySize; SliceIndex++ )
// 		{
// 			Map( MipLevelIndex, SliceIndex );
// 			ASSERT( rowPitch == m_LockedResource.rowPitch, "Incompatible row pitch!" );
// 			ASSERT( depthPitch == m_LockedResource.depthPitch, "Incompatible depth pitch!" );
// 			fread_s( m_LockedResource.pData, m_LockedResource.depthPitch, m_LockedResource.depthPitch, 1, pFile );
// 			UnMap( MipLevelIndex, SliceIndex );
// 		}
// 	}
// 
// 	// We're done!
// 	fclose( pFile );
}

// Texture2D::Texture2D( Device& _device, const TextureFilePOM& _POM, bool _UAV )
// 	: Component( _device )
// 	, m_width( _POM.m_Width )
// 	, m_height( _POM.m_Height )
// 	, m_arraySize( _POM.m_ArraySizeOrDepth )
// 	, m_format( _POM.m_pPixelFormat )
// 	, m_mipLevelsCount( _POM.m_MipsCount )
// 	, m_isCubeMap( _POM.m_Type == TextureFilePOM::TEX_CUBE ) {
// 	Init( _POM.m_ppContent, false, _UAV, _POM.m_pMipsDescriptors );
// }

#endif
