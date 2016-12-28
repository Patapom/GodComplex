#include "stdafx.h"

#include "Texture2D.h"

Texture2D::Texture2D( Device& _Device, ID3D11Texture2D& _Texture, const IPixelFormatDescriptor& _Format )
	: Component( _Device )
	, m_Format( _Format )
	, m_bIsDepthStencil( false )
	, m_bIsCubeMap( false )
{
	D3D11_TEXTURE2D_DESC	Desc;
	_Texture.GetDesc( &Desc );

	m_Width = Desc.Width;
	m_Height = Desc.Height;
	m_ArraySize = Desc.ArraySize;
	m_MipLevelsCount = Desc.MipLevels;

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_LastAssignedSlots[ShaderStageIndex] = -1;
	m_LastAssignedSlotsUAV = -1;

	m_pTexture = &_Texture;
}

Texture2D::Texture2D( Device& _Device, U32 _Width, U32 _Height, int _ArraySize, U32 _MipLevelsCount, const IPixelFormatDescriptor& _Format, const void* const* _ppContent, bool _bStaging, bool _bUnOrderedAccess )
	: Component( _Device )
	, m_Width( _Width )
	, m_Height( _Height )
	, m_Format( _Format )
	, m_MipLevelsCount( _MipLevelsCount )
	, m_bIsDepthStencil( false )
	, m_bIsCubeMap( false )
{
	if ( _ArraySize < 0 ) {
		// Special cube map case!
		ASSERT( m_Width == m_Height, "When creating a cube map, width & height must match!" );
		m_ArraySize = -_ArraySize;
		ASSERT( (m_ArraySize % 6) == 0, "Cube map array size must be multiples of 6!" );
		m_bIsCubeMap = true;
	} else {
		// Regular case
		m_ArraySize = _ArraySize;
	}

	Init( _ppContent, _bStaging, _bUnOrderedAccess );
}

Texture2D::Texture2D( Device& _Device, U32 _Width, U32 _Height, U32 _ArraySize, const IDepthStencilFormatDescriptor& _Format )
	: Component( _Device )
	, m_Format( _Format )
	, m_bIsDepthStencil( true )
	, m_bIsCubeMap( false )
{
	ASSERT( _Width <= MAX_TEXTURE_SIZE, "Texture size out of range!" );
	ASSERT( _Height <= MAX_TEXTURE_SIZE, "Texture size out of range!" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_LastAssignedSlots[ShaderStageIndex] = -1;
	m_LastAssignedSlotsUAV = -1;

	m_Width = _Width;
	m_Height = _Height;
	m_ArraySize = _ArraySize;
	m_MipLevelsCount = 1;

	D3D11_TEXTURE2D_DESC	Desc;
	Desc.Width = m_Width;
	Desc.Height = m_Height;
	Desc.ArraySize = m_ArraySize;
	Desc.MipLevels = 1;
	Desc.Format = _Format.DirectXFormat();
	Desc.SampleDesc.Count = 1;
	Desc.SampleDesc.Quality = 0;
	Desc.Usage = D3D11_USAGE_DEFAULT;
	Desc.BindFlags = D3D11_BIND_DEPTH_STENCIL | D3D11_BIND_SHADER_RESOURCE;
	Desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
	Desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG( 0 );

	Check( m_device.DXDevice().CreateTexture2D( &Desc, NULL, &m_pTexture ) );
}

static void		ReleaseDirectXObject( int _EntryIndex, void*& _pValue, void* _pUserData ) {
	IUnknown*	pObject = (IUnknown*) _pValue;
	pObject->Release();
}

Texture2D::~Texture2D() {
	ASSERT( m_pTexture != NULL, "Invalid texture to destroy!" );

	m_CachedSRVs.ForEach( ReleaseDirectXObject, NULL );
	m_CachedRTVs.ForEach( ReleaseDirectXObject, NULL );
	m_CachedUAVs.ForEach( ReleaseDirectXObject, NULL );
	m_CachedDSVs.ForEach( ReleaseDirectXObject, NULL );

	m_pTexture->Release();
	m_pTexture = NULL;
}

void	Texture2D::Init( const void* const* _ppContent, bool _bStaging, bool _bUnOrderedAccess, TextureFilePOM::MipDescriptor* _pMipDescriptors ) {
	ASSERT( m_Width <= MAX_TEXTURE_SIZE, "Texture size out of range!" );
	ASSERT( m_Height <= MAX_TEXTURE_SIZE, "Texture size out of range!" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_LastAssignedSlots[ShaderStageIndex] = -1;
	m_LastAssignedSlotsUAV = -1;

	m_MipLevelsCount = ComputeMipLevelsCount( m_Width, m_Height, m_MipLevelsCount );

	D3D11_TEXTURE2D_DESC	Desc;
	Desc.Width = m_Width;
	Desc.Height = m_Height;
	Desc.ArraySize = m_ArraySize;
	Desc.MipLevels = m_MipLevelsCount;
	Desc.Format = m_Format.DirectXFormat();
	Desc.SampleDesc.Count = 1;
	Desc.SampleDesc.Quality = 0;
	if ( _bStaging )
	{
		Desc.Usage = D3D11_USAGE_STAGING;
//		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | (_bWriteable ? D3D11_CPU_ACCESS_WRITE : 0);
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | D3D11_CPU_ACCESS_WRITE;
		Desc.BindFlags = 0;
		Desc.MiscFlags = 0;
	}
	else
	{
		Desc.Usage = _ppContent != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DEFAULT;
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
		Desc.BindFlags = _ppContent != NULL ? D3D11_BIND_SHADER_RESOURCE : (D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE | (_bUnOrderedAccess ? D3D11_BIND_UNORDERED_ACCESS: 0));
		Desc.MiscFlags = m_bIsCubeMap ? D3D11_RESOURCE_MISC_TEXTURECUBE : 0;
	}

	if ( _ppContent != NULL ) {
		D3D11_SUBRESOURCE_DATA*	pInitialData = new D3D11_SUBRESOURCE_DATA[m_MipLevelsCount*m_ArraySize];

		for ( U32 ArrayIndex=0; ArrayIndex < m_ArraySize; ArrayIndex++ ) {
			U32	Width = m_Width;
			U32	Height = m_Height;
			for ( U32 MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ ) {
				U32	RowPitch = _pMipDescriptors != NULL ? _pMipDescriptors[MipLevelIndex].RowPitch : Width * m_Format.Size();
				U32	DepthPitch = _pMipDescriptors != NULL ? _pMipDescriptors[MipLevelIndex].DepthPitch : Height * RowPitch;

				pInitialData[ArrayIndex*m_MipLevelsCount+MipLevelIndex].pSysMem = _ppContent[ArrayIndex*m_MipLevelsCount+MipLevelIndex];
				pInitialData[ArrayIndex*m_MipLevelsCount+MipLevelIndex].SysMemPitch = RowPitch;
				pInitialData[ArrayIndex*m_MipLevelsCount+MipLevelIndex].SysMemSlicePitch = DepthPitch;
				NextMipSize( Width, Height );
			}
		}

		Check( m_device.DXDevice().CreateTexture2D( &Desc, pInitialData, &m_pTexture ) );

		delete[] pInitialData;
	}
	else
		Check( m_device.DXDevice().CreateTexture2D( &Desc, NULL, &m_pTexture ) );
}

ID3D11ShaderResourceView*	Texture2D::GetSRV( U32 _MipLevelStart, U32 _MipLevelsCount, U32 _ArrayStart, U32 _ArraySize, bool _AsArray ) const {
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = m_MipLevelsCount - _MipLevelStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | ((_MipLevelsCount | (_MipLevelStart << 4)) << 12)) << 12);
	U32	Hash = (_MipLevelStart << 0) | (_ArrayStart << 4) | (_MipLevelsCount << (4+12)) | (_ArraySize << (4+12+4));	// Re-organized to have most likely changes (i.e. mip & array starts) first
		Hash ^= _AsArray ? 0x80000000UL : 0;

	ID3D11ShaderResourceView*	pExistingView = (ID3D11ShaderResourceView*) m_CachedSRVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
	Desc.Format = m_bIsDepthStencil ? ((IDepthStencilFormatDescriptor&) m_Format).ReadableDirectXFormat() : m_Format.DirectXFormat();
	if ( _AsArray )
	{	// Force as a Texture2DArray
		Desc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
		Desc.Texture2DArray.MostDetailedMip = _MipLevelStart;
		Desc.Texture2DArray.MipLevels = _MipLevelsCount;
		Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
		Desc.Texture2DArray.ArraySize = _ArraySize;
	}
	else
	{
		Desc.ViewDimension = m_ArraySize > 1 ? (m_bIsCubeMap ? (m_ArraySize > 6 ? D3D11_SRV_DIMENSION_TEXTURECUBEARRAY : D3D11_SRV_DIMENSION_TEXTURECUBE) : D3D11_SRV_DIMENSION_TEXTURE2DARRAY) : D3D11_SRV_DIMENSION_TEXTURE2D;
		if ( m_bIsCubeMap )
		{
			Desc.TextureCubeArray.MostDetailedMip = _MipLevelStart;
			Desc.TextureCubeArray.MipLevels = _MipLevelsCount;
			Desc.TextureCubeArray.First2DArrayFace = _ArrayStart;
			Desc.TextureCubeArray.NumCubes = _ArraySize / 6;
		}
		else
		{
			Desc.Texture2DArray.MostDetailedMip = _MipLevelStart;
			Desc.Texture2DArray.MipLevels = _MipLevelsCount;
			Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
			Desc.Texture2DArray.ArraySize = _ArraySize;
		}
	}

	ID3D11ShaderResourceView*	pView;
	Check( m_device.DXDevice().CreateShaderResourceView( m_pTexture, &Desc, &pView ) );

	m_CachedSRVs.Add( Hash, pView );

	return pView;
}

ID3D11RenderTargetView*		Texture2D::GetRTV( U32 _MipLevelIndex, U32 _ArrayStart, U32 _ArraySize ) const {
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | (_MipLevelIndex << 12)) << 12);
	U32	Hash = (_MipLevelIndex << 0) | (_ArrayStart << 4) | (_ArraySize << (4+12));	// Re-organized to have most likely changes (i.e. mip & array starts) first
	ID3D11RenderTargetView*	pExistingView = (ID3D11RenderTargetView*) m_CachedRTVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_RENDER_TARGET_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = m_ArraySize > 1 ? D3D11_RTV_DIMENSION_TEXTURE2DARRAY : D3D11_RTV_DIMENSION_TEXTURE2D;
	Desc.Texture2DArray.MipSlice = _MipLevelIndex;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11RenderTargetView*	pView;
	Check( m_device.DXDevice().CreateRenderTargetView( m_pTexture, &Desc, &pView ) );

	m_CachedRTVs.Add( Hash, pView );

	return pView;
}

ID3D11UnorderedAccessView*	Texture2D::GetUAV( U32 _MipLevelIndex, U32 _ArrayStart, U32 _ArraySize ) const {
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | (_MipLevelIndex << 12)) << 12);
	U32	Hash = (_MipLevelIndex << 0) | (_ArrayStart << 4) | (_ArraySize << (4+12));	// Re-organized to have most likely changes (i.e. mip & array starts) first
	ID3D11UnorderedAccessView*	pExistingView = (ID3D11UnorderedAccessView*) m_CachedUAVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_UNORDERED_ACCESS_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = m_ArraySize > 1 ? D3D11_UAV_DIMENSION_TEXTURE2DARRAY : D3D11_UAV_DIMENSION_TEXTURE2D;
	Desc.Texture2DArray.MipSlice = _MipLevelIndex;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11UnorderedAccessView*	pView = NULL;
	Check( m_device.DXDevice().CreateUnorderedAccessView( m_pTexture, &Desc, &pView ) );

	m_CachedUAVs.Add( Hash, pView );

	return pView;
}

ID3D11DepthStencilView*		Texture2D::GetDSV( U32 _ArrayStart, U32 _ArraySize ) const {
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;

	// Check if we already have it
	U32	Hash = (_ArrayStart << 0) | (_ArraySize << 12);
	ID3D11DepthStencilView*	pExistingView = (ID3D11DepthStencilView*) m_CachedDSVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	D3D11_DEPTH_STENCIL_VIEW_DESC	Desc;
	Desc.Format = ((IDepthStencilFormatDescriptor&) m_Format).WritableDirectXFormat();
	Desc.ViewDimension = m_ArraySize == 1 ? D3D11_DSV_DIMENSION_TEXTURE2D : D3D11_DSV_DIMENSION_TEXTURE2DARRAY;
	Desc.Flags = 0;
	Desc.Texture2DArray.MipSlice = 0;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11DepthStencilView*	pView = NULL;
	Check( m_device.DXDevice().CreateDepthStencilView( m_pTexture, &Desc, &pView ) );

	m_CachedDSVs.Add( Hash, pView );

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
	m_LastAssignedSlots[0] = _SlotIndex;
	m_LastAssignedSlots[1] = _SlotIndex;
	m_LastAssignedSlots[2] = _SlotIndex;
	m_LastAssignedSlots[3] = _SlotIndex;
	m_LastAssignedSlots[4] = _SlotIndex;
	m_LastAssignedSlots[5] = _SlotIndex;
}

void	Texture2D::SetVS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().VSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[0] = _SlotIndex;
}
void	Texture2D::SetHS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().HSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[1] = _SlotIndex;
}
void	Texture2D::SetDS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().DSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[2] = _SlotIndex;
}
void	Texture2D::SetGS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().GSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[3] = _SlotIndex;
}
void	Texture2D::SetPS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().PSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[4] = _SlotIndex;
}
void	Texture2D::SetCS( U32 _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetSRV( 0, 0, 0, 0 );
	m_device.DXContext().CSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[5] = _SlotIndex;
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
		if ( m_LastAssignedSlots[ShaderStageIndex] != -1 ) {
			m_device.RemoveShaderResources( m_LastAssignedSlots[ShaderStageIndex], 1, pStageFlags[ShaderStageIndex] );
			m_LastAssignedSlots[ShaderStageIndex] = -1;
		}
}

// UAV setting
void	Texture2D::SetCSUAV( U32 _SlotIndex, ID3D11UnorderedAccessView* _pView ) const {
	_pView = _pView != NULL ? _pView : GetUAV( 0, 0, 0 );
	UINT	InitialCount = -1;
	m_device.DXContext().CSSetUnorderedAccessViews( _SlotIndex, 1, &_pView, &InitialCount );
	m_LastAssignedSlotsUAV = _SlotIndex;
}

void	Texture2D::RemoveFromLastAssignedSlotUAV() const {
	ID3D11UnorderedAccessView*	pNULL = NULL;
	UINT	InitialCount = -1;
	if ( m_LastAssignedSlotsUAV != -1 )
		m_device.DXContext().CSSetUnorderedAccessViews( m_LastAssignedSlotsUAV, 1, &pNULL, &InitialCount );
	m_LastAssignedSlotsUAV = -1;
}

void	Texture2D::CopyFrom( Texture2D& _SourceTexture ) {
	ASSERT( _SourceTexture.m_Width == m_Width && _SourceTexture.m_Height == m_Height, "Size mismatch!" );
	ASSERT( _SourceTexture.m_ArraySize == m_ArraySize, "Array size mismatch!" );
	ASSERT( _SourceTexture.m_MipLevelsCount == m_MipLevelsCount, "Mips count mismatch!" );
	ASSERT( _SourceTexture.m_Format.DirectXFormat() == m_Format.DirectXFormat(), "Format mismatch!" );

	m_device.DXContext().CopyResource( m_pTexture, _SourceTexture.m_pTexture );
}

D3D11_MAPPED_SUBRESOURCE&	Texture2D::Map( U32 _MipLevelIndex, U32 _ArrayIndex ) {
	Check( m_device.DXContext().Map( m_pTexture, CalcSubResource( _MipLevelIndex, _ArrayIndex ), D3D11_MAP_READ, 0, &m_LockedResource ) );
	return m_LockedResource;
}

void	Texture2D::UnMap( U32 _MipLevelIndex, U32 _ArrayIndex ) {
	m_device.DXContext().Unmap( m_pTexture, CalcSubResource( _MipLevelIndex, _ArrayIndex ) );
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

U32	Texture2D::CalcSubResource( U32 _MipLevelIndex, U32 _ArrayIndex ) {
	return _MipLevelIndex + (_ArrayIndex * m_MipLevelsCount);
}

#if defined(_DEBUG) || !defined(GODCOMPLEX)

#include "..\..\Utility\TextureFilePOM.h"

// I/O for staging textures
void	Texture2D::Save( const char* _pFileName )
{
	TextureFilePOM	POM;
	POM.AllocateContent( *this );

	// Fill up content
	for ( U32 MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ ) {
		for ( U32 SliceIndex=0; SliceIndex < m_ArraySize; SliceIndex++ ) {
			Map( MipLevelIndex, SliceIndex );

			if ( SliceIndex == 0 ) {
				// Save mip infos only once
				POM.m_pMipsDescriptors[MipLevelIndex].RowPitch = m_LockedResource.RowPitch;
				POM.m_pMipsDescriptors[MipLevelIndex].DepthPitch = m_LockedResource.DepthPitch;
			}

			POM.m_ppContent[MipLevelIndex+m_MipLevelsCount*SliceIndex] = new void*[m_LockedResource.DepthPitch];
			memcpy_s( POM.m_ppContent[MipLevelIndex+m_MipLevelsCount*SliceIndex], m_LockedResource.DepthPitch, m_LockedResource.pData, m_LockedResource.DepthPitch );

			UnMap( MipLevelIndex, SliceIndex );
		}
	}

	POM.Save( _pFileName );
}

void	Texture2D::Load( const char* _pFileName ) {
	TextureFilePOM	POM;
	POM.Load( _pFileName );

	// Read up content
	for ( U32 MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ ) {
		for ( U32 SliceIndex=0; SliceIndex < m_ArraySize; SliceIndex++ ) {
			Map( MipLevelIndex, SliceIndex );

			if ( SliceIndex == 0 ) {
				// Test only once
				ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].RowPitch == m_LockedResource.RowPitch, "Incompatible row pitch!" );
				ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].DepthPitch == m_LockedResource.DepthPitch, "Incompatible depth pitch!" );
			}

			memcpy_s( m_LockedResource.pData, m_LockedResource.DepthPitch, POM.m_ppContent[MipLevelIndex+m_MipLevelsCount*SliceIndex], m_LockedResource.DepthPitch );
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
// 		int	RowPitch, DepthPitch;
// 		fread_s( &RowPitch, sizeof(int), sizeof(int), 1, pFile );
// 		fread_s( &DepthPitch, sizeof(int), sizeof(int), 1, pFile );
// 
// 		for ( int SliceIndex=0; SliceIndex < m_ArraySize; SliceIndex++ )
// 		{
// 			Map( MipLevelIndex, SliceIndex );
// 			ASSERT( RowPitch == m_LockedResource.RowPitch, "Incompatible row pitch!" );
// 			ASSERT( DepthPitch == m_LockedResource.DepthPitch, "Incompatible depth pitch!" );
// 			fread_s( m_LockedResource.pData, m_LockedResource.DepthPitch, m_LockedResource.DepthPitch, 1, pFile );
// 			UnMap( MipLevelIndex, SliceIndex );
// 		}
// 	}
// 
// 	// We're done!
// 	fclose( pFile );
}

Texture2D::Texture2D( Device& _Device, const TextureFilePOM& _POM, bool _bUnOrderedAccess )
	: Component( _Device )
	, m_Width( _POM.m_Width )
	, m_Height( _POM.m_Height )
	, m_ArraySize( _POM.m_ArraySizeOrDepth )
	, m_Format( *_POM.m_pPixelFormat )
	, m_MipLevelsCount( _POM.m_MipsCount )
	, m_bIsDepthStencil( false )
	, m_bIsCubeMap( _POM.m_Type == TextureFilePOM::TEX_CUBE )
{
	Init( _POM.m_ppContent, false, _bUnOrderedAccess, _POM.m_pMipsDescriptors );
}

#endif
