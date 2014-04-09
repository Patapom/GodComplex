#include "Texture3D.h"

Texture3D::Texture3D( Device& _Device, int _Width, int _Height, int _Depth, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent, bool _bStaging, bool _bUnOrderedAccess )
	: Component( _Device )
	, m_Width( _Width )
	, m_Height( _Height )
	, m_Depth( _Depth )
	, m_Format( _Format )
	, m_MipLevelsCount( _MipLevelsCount )
{
	Init( _ppContent, _bStaging, _bUnOrderedAccess );
}

static void		ReleaseDirectXObject( int _EntryIndex, void*& _pValue, void* _pUserData )
{
	IUnknown*	pObject = (IUnknown*) _pValue;
	pObject->Release();
}

Texture3D::~Texture3D()
{
	ASSERT( m_pTexture != NULL, "Invalid texture to destroy !" );

	m_CachedSRVs.ForEach( ReleaseDirectXObject, NULL );
	m_CachedRTVs.ForEach( ReleaseDirectXObject, NULL );
	m_CachedUAVs.ForEach( ReleaseDirectXObject, NULL );

	m_pTexture->Release();
	m_pTexture = NULL;
}

void	Texture3D::Init( const void* const* _ppContent, bool _bStaging, bool _bUnOrderedAccess, TextureFilePOM::MipDescriptor* _pMipDescriptors )
{
	ASSERT( m_Width <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( m_Height <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( m_Depth <= MAX_TEXTURE_SIZE, "Texture size out of range !" );

	for ( int ShaderStageIndex=0; ShaderStageIndex < 6; ShaderStageIndex++ )
		m_LastAssignedSlots[ShaderStageIndex] = -1;
	m_LastAssignedSlotsUAV = -1;

	m_MipLevelsCount = ComputeMipLevelsCount( m_Width, m_Height, m_Depth, m_MipLevelsCount );

	D3D11_TEXTURE3D_DESC	Desc;
	Desc.Width = m_Width;
	Desc.Height = m_Height;
	Desc.Depth = m_Depth;
	Desc.MipLevels = m_MipLevelsCount;
	Desc.Format = m_Format.DirectXFormat();
	Desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG( 0 );

	if ( _bStaging )
	{
		Desc.Usage = D3D11_USAGE_STAGING;
		Desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | D3D11_CPU_ACCESS_WRITE;
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
		int	Width = m_Width;
		int	Height = m_Height;
		int	Depth = m_Depth;
		for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		{
			int	RowPitch = _pMipDescriptors != NULL ? _pMipDescriptors[MipLevelIndex].RowPitch : Width * m_Format.Size();
			int	DepthPitch = _pMipDescriptors != NULL ? _pMipDescriptors[MipLevelIndex].DepthPitch : Height * RowPitch;

			pInitialData[MipLevelIndex].pSysMem = _ppContent[MipLevelIndex];
			pInitialData[MipLevelIndex].SysMemPitch = RowPitch;
			pInitialData[MipLevelIndex].SysMemSlicePitch = DepthPitch;
			NextMipSize( Width, Height, Depth );
		}

		Check( m_Device.DXDevice().CreateTexture3D( &Desc, pInitialData, &m_pTexture ) );
	}
	else
		Check( m_Device.DXDevice().CreateTexture3D( &Desc, NULL, &m_pTexture ) );
}

ID3D11ShaderResourceView*	Texture3D::GetShaderView( int _MipLevelStart, int _MipLevelsCount ) const
{
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = m_MipLevelsCount - _MipLevelStart;

	// Check if we already have it
//	U32	Hash = _MipLevelsCount | (_MipLevelStart << 4);
	U32	Hash = _MipLevelStart | (_MipLevelsCount << 4);
	ID3D11ShaderResourceView*	pExistingView = (ID3D11ShaderResourceView*) m_CachedSRVs.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MostDetailedMip = _MipLevelStart;
	Desc.Texture3D.MipLevels = _MipLevelsCount;

	ID3D11ShaderResourceView*	pView;
	Check( m_Device.DXDevice().CreateShaderResourceView( m_pTexture, &Desc, &pView ) );

	m_CachedSRVs.Add( Hash, pView );

	return pView;
}

ID3D11RenderTargetView*		Texture3D::GetTargetView( int _MipLevelIndex, int _FirstWSlice, int _WSize ) const
{
	if ( _WSize == 0 )
		_WSize = m_Depth - _FirstWSlice;

	// Check if we already have it
//	U32	Hash = _WSize | ((_FirstWSlice | (_MipLevelIndex << 12)) << 12);
	U32	Hash = (_MipLevelIndex << 0) | (_FirstWSlice << 12) | (_WSize << (4+12));	// Re-organized to have most likely changes (i.e. mip & slice starts) first
	ID3D11RenderTargetView*	pExistingView = (ID3D11RenderTargetView*) m_CachedRTVs.Get( Hash );
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

	m_CachedRTVs.Add( Hash, pView );

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
	ASSERT( _SourceTexture.m_Width == m_Width && _SourceTexture.m_Height == m_Height && _SourceTexture.m_Depth == m_Depth, "Size mismatch!" );
	ASSERT( _SourceTexture.m_MipLevelsCount == m_MipLevelsCount, "Mips count mismatch!" );
	ASSERT( _SourceTexture.m_Format.DirectXFormat() == m_Format.DirectXFormat(), "Format mismatch!" );

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

#include "..\..\Utility\TextureFilePOM.h"

// I/O for staging textures
void	Texture3D::Save( const char* _pFileName )
{
	TextureFilePOM	POM;
	POM.AllocateContent( *this );

	// Fill up content
 	int	Depth = m_Depth;
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		Map( MipLevelIndex );

		POM.m_ppContent[MipLevelIndex] = new void*[Depth*m_LockedResource.DepthPitch];

		POM.m_pMipsDescriptors[MipLevelIndex].RowPitch = m_LockedResource.RowPitch;
		POM.m_pMipsDescriptors[MipLevelIndex].DepthPitch = m_LockedResource.DepthPitch;

		memcpy_s( POM.m_ppContent[MipLevelIndex], Depth * m_LockedResource.DepthPitch, m_LockedResource.pData, Depth * m_LockedResource.DepthPitch );
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
// 		fwrite( &m_LockedResource.RowPitch, sizeof(int), 1, pFile );
// 		fwrite( &m_LockedResource.DepthPitch, sizeof(int), 1, pFile );
// 		for ( int SliceIndex=0; SliceIndex < Depth; SliceIndex++ )
// 			fwrite( ((U8*) m_LockedResource.pData) + SliceIndex * m_LockedResource.DepthPitch, m_LockedResource.DepthPitch, 1, pFile );
// 		UnMap( MipLevelIndex );
// 
// 		Depth = MAX( 1, Depth >> 1 );
// 	}
// 
// 	// We're done!
// 	fclose( pFile );
}

void	Texture3D::Load( const char* _pFileName )
{
	TextureFilePOM	POM;
	POM.Load( _pFileName );

	// Read up content
 	int	Depth = m_Depth;
	for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
	{
		Map( MipLevelIndex );

		ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].RowPitch == m_LockedResource.RowPitch, "Incompatible row pitch!" );
		ASSERT( POM.m_pMipsDescriptors[MipLevelIndex].DepthPitch == m_LockedResource.DepthPitch, "Incompatible depth pitch!" );

		memcpy_s( m_LockedResource.pData, Depth * m_LockedResource.DepthPitch, POM.m_ppContent[MipLevelIndex], Depth * m_LockedResource.DepthPitch );
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
// 		int	RowPitch, DepthPitch;
// 		fread_s( &RowPitch, sizeof(int), sizeof(int), 1, pFile );
// 		fread_s( &DepthPitch, sizeof(int), sizeof(int), 1, pFile );
// 		ASSERT( RowPitch == m_LockedResource.RowPitch, "Incompatible row pitch!" );
// 		ASSERT( DepthPitch == m_LockedResource.DepthPitch, "Incompatible depth pitch!" );
// 
// 		for ( int SliceIndex=0; SliceIndex < Depth; SliceIndex++ )
// 			fread_s( ((U8*) m_LockedResource.pData) + SliceIndex * m_LockedResource.DepthPitch, m_LockedResource.DepthPitch, m_LockedResource.DepthPitch, 1, pFile );
// 
// 		UnMap( MipLevelIndex );
// 
// 		Depth = MAX( 1, Depth >> 1 );
// 	}
// 
// 	// We're done!
// 	fclose( pFile );
}

Texture3D::Texture3D( Device& _Device, const TextureFilePOM& _POM, bool _bUnOrderedAccess )
	: Component( _Device )
	, m_Width( _POM.m_Width )
	, m_Height( _POM.m_Height )
	, m_Depth( _POM.m_ArraySizeOrDepth )
	, m_Format( *_POM.m_pPixelFormat )
	, m_MipLevelsCount( _POM.m_MipsCount )
{
	Init( _POM.m_ppContent, false, _bUnOrderedAccess, _POM.m_pMipsDescriptors );
}

#endif
