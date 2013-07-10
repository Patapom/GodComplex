#include "Texture3D.h"

Texture3D::Texture3D( Device& _Device, int _Width, int _Height, int _Depth, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent, bool _bStaging, bool _bWriteable, bool _bUnOrderedAccess ) : Component( _Device )
	, m_Format( _Format )
{
	ASSERT( _Width <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( _Height <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( _Depth <= MAX_TEXTURE_SIZE, "Texture size out of range !" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_LastAssignedSlots[ShaderStageIndex] = -1;
	m_LastAssignedSlotsUAV = -1;

	m_Width = _Width;
	m_Height = _Height;
	m_Depth = _Depth;

	m_MipLevelsCount = ComputeMipLevelsCount( _Width, _Height, _Depth, _MipLevelsCount );

	D3D11_TEXTURE3D_DESC	Desc;
	Desc.Width = _Width;
	Desc.Height = _Height;
	Desc.Depth = _Depth;
	Desc.MipLevels = m_MipLevelsCount;
	Desc.Format = _Format.DirectXFormat();
	Desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG( 0 );

	if ( _bStaging )
	{
		Desc.Usage = D3D11_USAGE_STAGING;
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | (_bWriteable ? D3D11_CPU_ACCESS_WRITE : 0);
		Desc.BindFlags = 0;
	}
	else
	{
		Desc.Usage = _ppContent != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DEFAULT;
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
		Desc.BindFlags = _ppContent != NULL ? D3D11_BIND_SHADER_RESOURCE : (D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE | (_bUnOrderedAccess ? D3D11_BIND_UNORDERED_ACCESS: 0));
	}

	if ( _ppContent != NULL )
	{
		D3D11_SUBRESOURCE_DATA  pInitialData[MAX_TEXTURE_POT];
		for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		{
			pInitialData[MipLevelIndex].pSysMem = _ppContent[MipLevelIndex];
			pInitialData[MipLevelIndex].SysMemPitch = _Width * _Format.Size();
			pInitialData[MipLevelIndex].SysMemSlicePitch = _Width * _Height * _Format.Size();
			NextMipSize( _Width, _Height, _Depth );
		}

		Check( m_Device.DXDevice().CreateTexture3D( &Desc, pInitialData, &m_pTexture ) );
	}
	else
		Check( m_Device.DXDevice().CreateTexture3D( &Desc, NULL, &m_pTexture ) );
}

static void		ReleaseDirectXObject( void*& _pValue, void* _pUserData )
{
	IUnknown*	pObject = (IUnknown*) _pValue;
	pObject->Release();
}

Texture3D::~Texture3D()
{
	ASSERT( m_pTexture != NULL, "Invalid texture to destroy !" );

	m_CachedShaderViews.ForEach( ReleaseDirectXObject, NULL );
	m_CachedTargetViews.ForEach( ReleaseDirectXObject, NULL );
	m_CachedUAVs.ForEach( ReleaseDirectXObject, NULL );

	m_pTexture->Release();
	m_pTexture = NULL;
}

ID3D11ShaderResourceView*	Texture3D::GetShaderView( int _MipLevelStart, int _MipLevelsCount ) const
{
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = m_MipLevelsCount - _MipLevelStart;

	// Check if we already have it
//	U32	Hash = _MipLevelsCount | (_MipLevelStart << 4);
	U32	Hash = _MipLevelStart | (_MipLevelsCount << 4);
	ID3D11ShaderResourceView*	pExistingView = (ID3D11ShaderResourceView*) m_CachedShaderViews.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MostDetailedMip = _MipLevelStart;
	Desc.Texture3D.MipLevels = _MipLevelsCount;

	ID3D11ShaderResourceView*	pView;
	Check( m_Device.DXDevice().CreateShaderResourceView( m_pTexture, &Desc, &pView ) );

	m_CachedShaderViews.Add( Hash, pView );

	return pView;
}

ID3D11RenderTargetView*		Texture3D::GetTargetView( int _MipLevelIndex, int _FirstWSlice, int _WSize ) const
{
	if ( _WSize == 0 )
		_WSize = m_Depth - _FirstWSlice;

	// Check if we already have it
//	U32	Hash = _WSize | ((_FirstWSlice | (_MipLevelIndex << 12)) << 12);
	U32	Hash = (_MipLevelIndex << 0) | (_FirstWSlice << 12) | (_WSize << (4+12));	// Re-organized to have most likely changes (i.e. mip & slice starts) first
	ID3D11RenderTargetView*	pExistingView = (ID3D11RenderTargetView*) m_CachedTargetViews.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	D3D11_RENDER_TARGET_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MipSlice = _MipLevelIndex;
	Desc.Texture3D.FirstWSlice = _FirstWSlice;
	Desc.Texture3D.WSize = _WSize;

	ID3D11RenderTargetView*	pView;
	Check( m_Device.DXDevice().CreateRenderTargetView( m_pTexture, &Desc, &pView ) );

	m_CachedTargetViews.Add( Hash, pView );

	return pView;
}

ID3D11UnorderedAccessView*	Texture3D::GetUAV( int _MipLevelIndex, int _FirstWSlice, int _WSize ) const
{
	if ( _WSize == 0 )
		_WSize = m_Depth - _FirstWSlice;

	// Check if we already have it
//	U32	Hash = _WSize | ((_FirstWSlice | (_MipLevelIndex << 12)) << 12);
	U32	Hash = (_MipLevelIndex << 0) | (_FirstWSlice << 12) | (_WSize << (4+12));	// Re-organized to have most likely changes (i.e. mip & slice starts) first
	ID3D11UnorderedAccessView*	pExistingView = (ID3D11UnorderedAccessView*) m_CachedUAVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_UNORDERED_ACCESS_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = D3D11_UAV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MipSlice = _MipLevelIndex;
	Desc.Texture3D.FirstWSlice = _FirstWSlice;
	Desc.Texture3D.WSize = _WSize;

	ID3D11UnorderedAccessView*	pView;
	Check( m_Device.DXDevice().CreateUnorderedAccessView( m_pTexture, &Desc, &pView ) );

	m_CachedUAVs.Add( Hash, pView );

	return pView;
}

void	Texture3D::Set( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0 );
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
void	Texture3D::SetVS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0 );
	m_Device.DXContext().VSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[0] = _SlotIndex;
}
void	Texture3D::SetHS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0 );
	m_Device.DXContext().HSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[1] = _SlotIndex;
}
void	Texture3D::SetDS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0 );
	m_Device.DXContext().DSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[2] = _SlotIndex;
}
void	Texture3D::SetGS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0 );
	m_Device.DXContext().GSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[3] = _SlotIndex;
}
void	Texture3D::SetPS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0 );
	m_Device.DXContext().PSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[4] = _SlotIndex;
}
void	Texture3D::SetCS( int _SlotIndex, bool _bIKnowWhatImDoing, ID3D11ShaderResourceView* _pView ) const
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	_pView = _pView != NULL ? _pView : GetShaderView( 0, 0 );
	m_Device.DXContext().CSSetShaderResources( _SlotIndex, 1, &_pView );
	m_LastAssignedSlots[5] = _SlotIndex;
}

void	Texture3D::RemoveFromLastAssignedSlots() const
{
	static Device::SHADER_STAGE_FLAGS	pStageFlags[] = {
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
void	Texture3D::SetCSUAV( int _SlotIndex, ID3D11UnorderedAccessView* _pView ) const
{
	_pView = _pView != NULL ? _pView : GetUAV( 0, 0, 0 );
	UINT	InitialCount = -1;
	m_Device.DXContext().CSSetUnorderedAccessViews( _SlotIndex, 1, &_pView, &InitialCount );
	m_LastAssignedSlotsUAV = _SlotIndex;
}

void	Texture3D::RemoveFromLastAssignedSlotUAV() const
{
	ID3D11UnorderedAccessView*	pNULL = NULL;
	UINT	InitialCount = -1;
	if ( m_LastAssignedSlotsUAV != -1 )
		m_Device.DXContext().CSSetUnorderedAccessViews( m_LastAssignedSlotsUAV, 1, &pNULL, &InitialCount );
	m_LastAssignedSlotsUAV = -1;
}


void	Texture3D::CopyFrom( Texture3D& _SourceTexture )
{
	m_Device.DXContext().CopyResource( m_pTexture, _SourceTexture.m_pTexture );
}

D3D11_MAPPED_SUBRESOURCE&	Texture3D::Map( int _MipLevelIndex )
{
	Check( m_Device.DXContext().Map( m_pTexture, _MipLevelIndex, D3D11_MAP_READ, 0, &m_LockedResource ) );
	return m_LockedResource;
}

void	Texture3D::UnMap( int _MipLevelIndex )
{
	m_Device.DXContext().Unmap( m_pTexture, _MipLevelIndex );
}

void	Texture3D::NextMipSize( int& _Width, int& _Height, int& _Depth )
{
	_Width = MAX( 1, _Width >> 1 );
	_Height = MAX( 1, _Height >> 1 );
	_Depth = MAX( 1, _Depth >> 1 );
}

int	 Texture3D::ComputeMipLevelsCount( int _Width, int _Height, int _Depth, int _MipLevelsCount )
{
	int MaxSize = MAX( MAX( _Width, _Height ), _Depth );
	int	MaxMipLevelsCount = int( ceilf( logf( MaxSize+1.0f ) / logf( 2.0f ) ) );
	
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = MaxMipLevelsCount;
	else
		_MipLevelsCount = MIN( _MipLevelsCount, MaxMipLevelsCount );

	ASSERT( _MipLevelsCount <= MAX_TEXTURE_POT, "Texture mip level out of range !" );
	return _MipLevelsCount;
}

#ifdef _DEBUG

#include <stdio.h>

// I/O for staging textures
void	Texture3D::Save( const char* _pFileName )
{
	FILE*	pFile;
	fopen_s( &pFile, _pFileName, "wb" );
	ASSERT( pFile != NULL, "Can't create file!" );

	// Write the type and format
	U8		Type = 0x02;
	U8		Format = U32(m_Format.DirectXFormat()) & 0xFF;
	fwrite( &Type, sizeof(U8), 1, pFile );
	fwrite( &Format, sizeof(U8), 1, pFile );

	// Write the dimensions
	fwrite( &m_Width, sizeof(int), 1, pFile );
	fwrite( &m_Height, sizeof(int), 1, pFile );
	fwrite( &m_Depth, sizeof(int), 1, pFile );
	fwrite( &m_MipLevelsCount, sizeof(int), 1, pFile );

	// Write each mip
	int	W = m_Width;
	int	H = m_Height;
	int	D = m_Depth;
	int	S = m_Format.Size();
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		int	MipLevelSize = W*H*D*S;

		Map( MipLevelIndex );
		fwrite( m_LockedResource.pData, MipLevelSize, 1, pFile );
		UnMap( MipLevelIndex );

		W = MAX( 1, W >> 1 );
		H = MAX( 1, H >> 1 );
		D = MAX( 1, D >> 1 );
	}

	// We're done!
	fclose( pFile );
}

void	Texture3D::Load( const char* _pFileName )
{
	FILE*	pFile;
	fopen_s( &pFile, _pFileName, "rb" );
	ASSERT( pFile != NULL, "Can't load file!" );

	// Read the type and format
	U8		Type, Format;
	fread_s( &Type, sizeof(U8), sizeof(U8), 1, pFile );
	fread_s( &Format, sizeof(U8), sizeof(U8), 1, pFile );
	DXGI_FORMAT	FileFormat = DXGI_FORMAT( Format );
	ASSERT( FileFormat == m_Format.DirectXFormat(), "Incompatible format!" );
	ASSERT( Type == 0x02, "File is not a texture 3D!" );

	// Read the dimensions
	int	W, H, D, M;
	fread_s( &W, sizeof(int), sizeof(int), 1, pFile );
	fread_s( &H, sizeof(int), sizeof(int), 1, pFile );
	fread_s( &D, sizeof(int), sizeof(int), 1, pFile );
	fread_s( &M, sizeof(int), sizeof(int), 1, pFile );

	ASSERT( W == m_Width, "Incompatible width!" );
	ASSERT( H == m_Height, "Incompatible height!" );
	ASSERT( D == m_Depth, "Incompatible depth!" );
	ASSERT( M == m_MipLevelsCount, "Incompatible mip levels count!" );

	// Read each mip
	int	S = m_Format.Size();
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		int	MipLevelSize = W*H*D*S;

		Map( MipLevelIndex );
		fread_s( m_LockedResource.pData, MipLevelSize, MipLevelSize, 1, pFile );
		UnMap( MipLevelIndex );

		W = MAX( 1, W >> 1 );
		H = MAX( 1, H >> 1 );
		D = MAX( 1, D >> 1 );
	}

	// We're done!
	fclose( pFile );
}

#endif
