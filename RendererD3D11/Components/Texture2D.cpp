#include "Texture2D.h"

Texture2D::Texture2D( Device& _Device, ID3D11Texture2D& _Texture, const IPixelFormatDescriptor& _Format )
	: Component( _Device )
	, m_Format( _Format )
	, m_bIsDepthStencil( false )
	, m_bIsCubeMap( false )
	, m_pCachedDepthStencilView( NULL )
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

Texture2D::Texture2D( Device& _Device, int _Width, int _Height, int _ArraySize, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent, bool _bStaging, bool _bWriteable, bool _bUnOrderedAccess )
	: Component( _Device )
	, m_Format( _Format )
	, m_bIsDepthStencil( false )
	, m_pCachedDepthStencilView( NULL )
{
	ASSERT( _Width <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( _Height <= MAX_TEXTURE_SIZE, "Texture size out of range !" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_LastAssignedSlots[ShaderStageIndex] = -1;
	m_LastAssignedSlotsUAV = -1;

	m_Width = _Width;
	m_Height = _Height;
	if ( _ArraySize == -6 )
	{	// Special cube map case !
		ASSERT( _Width == _Height, "When creating a cube map, width & height must match !" );
		m_ArraySize = 6;
		m_bIsCubeMap = true;
	}
	else
	{
		ASSERT( _ArraySize > 0, "Invalid array size !" );
		m_ArraySize = _ArraySize;
		m_bIsCubeMap = false;
	}

	m_MipLevelsCount = ComputeMipLevelsCount( _Width, _Height, _MipLevelsCount );

	D3D11_TEXTURE2D_DESC	Desc;
	Desc.Width = _Width;
	Desc.Height = _Height;
	Desc.ArraySize = m_ArraySize;
	Desc.MipLevels = m_MipLevelsCount;
	Desc.Format = _Format.DirectXFormat();
	Desc.SampleDesc.Count = 1;
	Desc.SampleDesc.Quality = 0;
	if ( _bStaging )
	{
		Desc.Usage = D3D11_USAGE_STAGING;
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | (_bWriteable ? D3D11_CPU_ACCESS_WRITE : 0);
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

	if ( _ppContent != NULL )
	{
		D3D11_SUBRESOURCE_DATA*	pInitialData = new D3D11_SUBRESOURCE_DATA[m_MipLevelsCount*m_ArraySize];

		for ( int ArrayIndex=0; ArrayIndex < m_ArraySize; ArrayIndex++ )
		{
			_Width = m_Width;
			_Height = m_Height;
			for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
			{
				pInitialData[ArrayIndex*m_MipLevelsCount+MipLevelIndex].pSysMem = _ppContent[ArrayIndex*m_MipLevelsCount+MipLevelIndex];
				pInitialData[ArrayIndex*m_MipLevelsCount+MipLevelIndex].SysMemPitch = _Width * _Format.Size();
				pInitialData[ArrayIndex*m_MipLevelsCount+MipLevelIndex].SysMemSlicePitch = _Width * _Height * _Format.Size();
				NextMipSize( _Width, _Height );
			}
		}

		Check( m_Device.DXDevice().CreateTexture2D( &Desc, pInitialData, &m_pTexture ) );

		delete[] pInitialData;
	}
	else
		Check( m_Device.DXDevice().CreateTexture2D( &Desc, NULL, &m_pTexture ) );
}

Texture2D::Texture2D( Device& _Device, int _Width, int _Height, const IDepthStencilFormatDescriptor& _Format )
	: Component( _Device )
	, m_Format( _Format )
	, m_bIsDepthStencil( true )
	, m_pCachedDepthStencilView( NULL )
{
	ASSERT( _Width <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( _Height <= MAX_TEXTURE_SIZE, "Texture size out of range !" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_LastAssignedSlots[ShaderStageIndex] = -1;
	m_LastAssignedSlotsUAV = -1;

	m_Width = _Width;
	m_Height = _Height;
	m_ArraySize = 1;
	m_MipLevelsCount = 1;

	D3D11_TEXTURE2D_DESC	Desc;
	Desc.Width = _Width;
	Desc.Height = _Height;
	Desc.ArraySize = 1;
	Desc.MipLevels = 1;
	Desc.Format = _Format.DirectXFormat();
	Desc.SampleDesc.Count = 1;
	Desc.SampleDesc.Quality = 0;
	Desc.Usage = D3D11_USAGE_DEFAULT;
	Desc.BindFlags = D3D11_BIND_DEPTH_STENCIL | D3D11_BIND_SHADER_RESOURCE;
	Desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
	Desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG( 0 );

	Check( m_Device.DXDevice().CreateTexture2D( &Desc, NULL, &m_pTexture ) );
}

static void		ReleaseDirectXObject( void*& _pValue, void* _pUserData )
{
	IUnknown*	pObject = (IUnknown*) _pValue;
	pObject->Release();
}

Texture2D::~Texture2D()
{
	ASSERT( m_pTexture != NULL, "Invalid texture to destroy !" );

	m_CachedShaderViews.ForEach( ReleaseDirectXObject, NULL );
	m_CachedTargetViews.ForEach( ReleaseDirectXObject, NULL );
	m_CachedUAVs.ForEach( ReleaseDirectXObject, NULL );

	if ( m_pCachedDepthStencilView != NULL )
		m_pCachedDepthStencilView->Release();

	m_pTexture->Release();
	m_pTexture = NULL;
}

ID3D11ShaderResourceView*	Texture2D::GetShaderView( int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize ) const
{
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = m_MipLevelsCount - _MipLevelStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | ((_MipLevelsCount | (_MipLevelStart << 4)) << 12)) << 12);
	U32	Hash = (_MipLevelStart << 0) | (_ArrayStart << 4) | (_MipLevelsCount << (4+12)) | (_ArraySize << (4+12+4));	// Re-organized to have most likely changes (i.e. mip & array starts) first
	ID3D11ShaderResourceView*	pExistingView = (ID3D11ShaderResourceView*) m_CachedShaderViews.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
	Desc.Format = m_bIsDepthStencil ? ((IDepthStencilFormatDescriptor&) m_Format).ReadableDirectXFormat() : m_Format.DirectXFormat();
	Desc.ViewDimension = m_ArraySize > 1 ? (m_bIsCubeMap ? D3D10_1_SRV_DIMENSION_TEXTURECUBE : D3D11_SRV_DIMENSION_TEXTURE2DARRAY) : D3D11_SRV_DIMENSION_TEXTURE2D;
	Desc.Texture2DArray.MostDetailedMip = _MipLevelStart;
	Desc.Texture2DArray.MipLevels = _MipLevelsCount;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11ShaderResourceView*	pView;
	Check( m_Device.DXDevice().CreateShaderResourceView( m_pTexture, &Desc, &pView ) );

	m_CachedShaderViews.Add( Hash, pView );

	return pView;
}

ID3D11RenderTargetView*		Texture2D::GetTargetView( int _MipLevelIndex, int _ArrayStart, int _ArraySize ) const
{
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;

	// Check if we already have it
//	U32	Hash = _ArraySize | ((_ArrayStart | (_MipLevelIndex << 12)) << 12);
	U32	Hash = (_MipLevelIndex << 0) | (_ArrayStart << 4) | (_ArraySize << (4+12));	// Re-organized to have most likely changes (i.e. mip & array starts) first
	ID3D11RenderTargetView*	pExistingView = (ID3D11RenderTargetView*) m_CachedTargetViews.Get( Hash );
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
	Check( m_Device.DXDevice().CreateRenderTargetView( m_pTexture, &Desc, &pView ) );

	m_CachedTargetViews.Add( Hash, pView );

	return pView;
}

ID3D11UnorderedAccessView*	Texture2D::GetUAV( int _MipLevelIndex, int _ArrayStart, int _ArraySize ) const
{
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

	ID3D11UnorderedAccessView*	pView;
	Check( m_Device.DXDevice().CreateUnorderedAccessView( m_pTexture, &Desc, &pView ) );

	m_CachedUAVs.Add( Hash, pView );

	return pView;
}

ID3D11DepthStencilView*		Texture2D::GetDepthStencilView() const
{
	if ( m_pCachedDepthStencilView == NULL )
	{
		D3D11_DEPTH_STENCIL_VIEW_DESC	Desc;
		Desc.Format = ((IDepthStencilFormatDescriptor&) m_Format).WritableDirectXFormat();
		Desc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
		Desc.Flags = 0;
		Desc.Texture2D.MipSlice = 0;

		Check( m_Device.DXDevice().CreateDepthStencilView( m_pTexture, &Desc, &m_pCachedDepthStencilView ) );
	}

	return m_pCachedDepthStencilView;
}

void	Texture2D::Set( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().VSSetShaderResources( _SlotIndex, 1, &_pView );
	m_Device.DXContext().HSSetShaderResources( _SlotIndex, 1, &_pView );
	m_Device.DXContext().DSSetShaderResources( _SlotIndex, 1, &_pView );
	m_Device.DXContext().GSSetShaderResources( _SlotIndex, 1, &_pView );
	m_Device.DXContext().PSSetShaderResources( _SlotIndex, 1, &_pView );
	m_Device.DXContext().CSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[0] = _SlotIndex;
	m_LastAssignedSlots[1] = _SlotIndex;
	m_LastAssignedSlots[2] = _SlotIndex;
	m_LastAssignedSlots[3] = _SlotIndex;
	m_LastAssignedSlots[4] = _SlotIndex;
	m_LastAssignedSlots[5] = _SlotIndex;
}

void	Texture2D::SetVS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().VSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[0] = _SlotIndex;
}
void	Texture2D::SetHS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().HSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[1] = _SlotIndex;
}
void	Texture2D::SetDS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().DSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[2] = _SlotIndex;
}
void	Texture2D::SetGS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().GSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[3] = _SlotIndex;
}
void	Texture2D::SetPS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().PSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[4] = _SlotIndex;
}
void	Texture2D::SetCS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().CSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[5] = _SlotIndex;
}

void	Texture2D::RemoveFromLastAssignedSlots() const
{
	Device::SHADER_STAGE_FLAGS	pStageFlags[] = {
		Device::SSF_VERTEX_SHADER,
		Device::SSF_HULL_SHADER,
		Device::SSF_DOMAIN_SHADER,
		Device::SSF_GEOMETRY_SHADER,
		Device::SSF_PIXEL_SHADER,
		Device::SSF_COMPUTE_SHADER,
	};
	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		if ( m_LastAssignedSlots[ShaderStageIndex] != -1 )
		{
			m_Device.RemoveShaderResources( m_LastAssignedSlots[ShaderStageIndex], 1, pStageFlags[ShaderStageIndex] );
			m_LastAssignedSlots[ShaderStageIndex] = -1;
		}
}

// UAV setting
void	Texture2D::SetCSUAV( int _SlotIndex, ID3D11UnorderedAccessView* _pView ) const
{
	_pView = _pView != NULL ? _pView : GetUAV( 0, 0, 0 );
	UINT	InitialCount = -1;
	m_Device.DXContext().CSSetUnorderedAccessViews( _SlotIndex, 1, &_pView, &InitialCount );
	m_LastAssignedSlotsUAV = _SlotIndex;
}

void	Texture2D::RemoveFromLastAssignedSlotUAV() const
{
	ID3D11UnorderedAccessView*	pNULL = NULL;
	UINT	InitialCount = -1;
	if ( m_LastAssignedSlotsUAV != -1 )
		m_Device.DXContext().CSSetUnorderedAccessViews( m_LastAssignedSlotsUAV, 1, &pNULL, &InitialCount );
}

void	Texture2D::CopyFrom( Texture2D& _SourceTexture )
{
	m_Device.DXContext().CopyResource( m_pTexture, _SourceTexture.m_pTexture );
}

D3D11_MAPPED_SUBRESOURCE&	Texture2D::Map( int _MipLevelIndex, int _ArrayIndex )
{
	Check( m_Device.DXContext().Map( m_pTexture, CalcSubResource( _MipLevelIndex, _ArrayIndex ), D3D11_MAP_READ, 0, &m_LockedResource ) );
	return m_LockedResource;
}

void	Texture2D::UnMap( int _MipLevelIndex, int _ArrayIndex )
{
	m_Device.DXContext().Unmap( m_pTexture, CalcSubResource( _MipLevelIndex, _ArrayIndex ) );
}

void	Texture2D::NextMipSize( int& _Width, int& _Height )
{
	_Width = MAX( 1, _Width >> 1 );
	_Height = MAX( 1, _Height >> 1 );
}

int	 Texture2D::ComputeMipLevelsCount( int _Width, int _Height, int _MipLevelsCount )
{
	int MaxSize = MAX( _Width, _Height );
	int	MaxMipLevelsCount = int( ceilf( logf( MaxSize+1.0f ) / logf( 2.0f ) ) );
	
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = MaxMipLevelsCount;
	else
		_MipLevelsCount = MIN( _MipLevelsCount, MaxMipLevelsCount );

	ASSERT( _MipLevelsCount <= MAX_TEXTURE_POT, "Texture mip level out of range !" );
	return _MipLevelsCount;
}

int	Texture2D::CalcSubResource( int _MipLevelIndex, int _ArrayIndex )
{
	return _MipLevelIndex + (_ArrayIndex * m_MipLevelsCount);
}

#ifdef _DEBUG

#include <stdio.h>

// I/O for staging textures
void	Texture2D::Save( const char* _pFileName )
{
	FILE*	pFile;
	fopen_s( &pFile, _pFileName, "wb" );
	ASSERT( pFile != NULL, "Can't create file!" );

	// Write the dimensions
	fwrite( &m_Width, sizeof(int), 1, pFile );
	fwrite( &m_Height, sizeof(int), 1, pFile );
	fwrite( &m_ArraySize, sizeof(int), 1, pFile );
	fwrite( &m_MipLevelsCount, sizeof(int), 1, pFile );

	// Write each slice
	for ( int SliceIndex=0; SliceIndex < m_ArraySize; SliceIndex++ )
	{
		for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		{
			Map( MipLevelIndex, SliceIndex );
			fwrite( m_LockedResource.pData, m_LockedResource.DepthPitch, 1, pFile );
			UnMap( MipLevelIndex, SliceIndex );
		}
	}

	// We're done!
	fclose( pFile );
}

void	Texture2D::Load( const char* _pFileName )
{
	FILE*	pFile;
	fopen_s( &pFile, _pFileName, "rb" );
	ASSERT( pFile != NULL, "Can't load file!" );

	// Read the dimensions
	int	W, H, A, M;
	fread_s( &W, sizeof(int), sizeof(int), 1, pFile );
	fread_s( &H, sizeof(int), sizeof(int), 1, pFile );
	fread_s( &A, sizeof(int), sizeof(int), 1, pFile );
	fread_s( &M, sizeof(int), sizeof(int), 1, pFile );

	ASSERT( W == m_Width, "Incompatible width!" );
	ASSERT( H == m_Height, "Incompatible height!" );
	ASSERT( A == m_ArraySize, "Incompatible array size!" );
	ASSERT( M == m_MipLevelsCount, "Incompatible mip levels count!" );

	// Read each slice
	for ( int SliceIndex=0; SliceIndex < m_ArraySize; SliceIndex++ )
	{
		for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		{
			Map( MipLevelIndex, SliceIndex );
			fread_s( m_LockedResource.pData, m_LockedResource.DepthPitch, m_LockedResource.DepthPitch, 1, pFile );
			UnMap( MipLevelIndex, SliceIndex );
		}
	}

	// We're done!
	fclose( pFile );
}

#endif
