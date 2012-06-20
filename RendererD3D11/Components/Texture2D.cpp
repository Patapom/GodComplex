#include "Texture2D.h"

Texture2D::Texture2D( Device& _Device, ID3D11Texture2D& _Texture, const IPixelFormatDescriptor& _Format )
	: Component( _Device )
	, m_Format( _Format )
	, m_bIsDepthStencil( false )
	, m_pCachedDepthStencilView( NULL )
{
	D3D11_TEXTURE2D_DESC	Desc;
	_Texture.GetDesc( &Desc );

	m_Width = Desc.Width;
	m_Height = Desc.Height;
	m_ArraySize = Desc.ArraySize;
	m_MipLevelsCount = Desc.MipLevels;

	m_pTexture = &_Texture;
}

Texture2D::Texture2D( Device& _Device, int _Width, int _Height, int _ArraySize, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent )
	: Component( _Device )
	, m_Format( _Format )
	, m_bIsDepthStencil( false )
	, m_pCachedDepthStencilView( NULL )
{
	ASSERT( _Width <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( _Height <= MAX_TEXTURE_SIZE, "Texture size out of range !" );

	m_Width = _Width;
	m_Height = _Height;
	m_ArraySize = _ArraySize;

	m_MipLevelsCount = ComputeMipLevelsCount( _Width, _Height, _MipLevelsCount );

	D3D11_TEXTURE2D_DESC	Desc;
	Desc.Width = _Width;
	Desc.Height = _Height;
	Desc.ArraySize = _ArraySize;
	Desc.MipLevels = m_MipLevelsCount;
	Desc.Format = _Format.DirectXFormat();
	Desc.SampleDesc.Count = 1;
	Desc.SampleDesc.Quality = 0;
	Desc.Usage = _ppContent != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DEFAULT;
	Desc.BindFlags = _ppContent != NULL ? D3D11_BIND_SHADER_RESOURCE : (D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE);
	Desc.CPUAccessFlags = D3D11_CPU_ACCESS_FLAG( 0 );
	Desc.MiscFlags = D3D11_RESOURCE_MISC_FLAG( 0 );

	if ( _ppContent != NULL )
	{
		D3D11_SUBRESOURCE_DATA  pInitialData[MAX_TEXTURE_POT];
		for ( int MipLevelIndex=0; MipLevelIndex < m_MipLevelsCount; MipLevelIndex++ )
		{
			pInitialData[MipLevelIndex].pSysMem = _ppContent[MipLevelIndex];
			pInitialData[MipLevelIndex].SysMemPitch = _Width * _Format.Size();
			pInitialData[MipLevelIndex].SysMemSlicePitch = _Width * _Height * _Format.Size();
			NextMipSize( _Width, _Height );
		}

		Check( m_Device.DXDevice().CreateTexture2D( &Desc, pInitialData, &m_pTexture ) );
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
	U32	Hash = _ArraySize | ((_ArrayStart | ((_MipLevelsCount | (_MipLevelStart << 4)) << 12)) << 12);
	ID3D11ShaderResourceView*	pExistingView = (ID3D11ShaderResourceView*) m_CachedShaderViews.Get( Hash );
	if ( pExistingView != NULL )
		return pExistingView;

	// Create a new one
	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
	Desc.Format = m_bIsDepthStencil ? ((IDepthStencilFormatDescriptor&) m_Format).ReadableDirectXFormat() : m_Format.DirectXFormat();
	Desc.ViewDimension = m_ArraySize > 1 ? D3D11_SRV_DIMENSION_TEXTURE2DARRAY : D3D11_SRV_DIMENSION_TEXTURE2D;
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
	U32	Hash = _ArraySize | ((_ArrayStart | (_MipLevelIndex << 12)) << 12);
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

ID3D11DepthStencilView*		Texture2D::GetDepthStencilView() const
{
	if ( m_pCachedDepthStencilView == NULL )
	{
		D3D11_DEPTH_STENCIL_VIEW_DESC	Desc;
		Desc.Format = ((IDepthStencilFormatDescriptor&) m_Format).WritableDirectXFormat();
		Desc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
#ifdef DIRECTX11
		Desc.Flags = D3D11_DSV_READ_ONLY_DEPTH | D3D11_DSV_READ_ONLY_STENCIL;	// Change that if that poses a problem later...
#else
		Desc.Flags = 0;
#endif
		Desc.Texture2D.MipSlice = 0;

		Check( m_Device.DXDevice().CreateDepthStencilView( m_pTexture, &Desc, &m_pCachedDepthStencilView ) );
	}

	return m_pCachedDepthStencilView;
}

void	Texture2D::Set( int _SlotIndex, bool _bIKnowWhatImDoing )
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	ID3D11ShaderResourceView*	pView = GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().VSSetShaderResources( _SlotIndex, 1, &pView );
	m_Device.DXContext().GSSetShaderResources( _SlotIndex, 1, &pView );
	m_Device.DXContext().PSSetShaderResources( _SlotIndex, 1, &pView );
}
void	Texture2D::SetVS( int _SlotIndex, bool _bIKnowWhatImDoing )
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	ID3D11ShaderResourceView*	pView = GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().VSSetShaderResources( _SlotIndex, 1, &pView );
}
void	Texture2D::SetGS( int _SlotIndex, bool _bIKnowWhatImDoing )
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	ID3D11ShaderResourceView*	pView = GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().GSSetShaderResources( _SlotIndex, 1, &pView );
}
void	Texture2D::SetPS( int _SlotIndex, bool _bIKnowWhatImDoing )
{
	ASSERT( _SlotIndex >= 10 || _bIKnowWhatImDoing, "WARNING: Assigning a reserved texture slot ! (i.e. all slots [0,9] are reserved for global textures)" );

	ID3D11ShaderResourceView*	pView = GetShaderView( 0, 0, 0, 0 );
	m_Device.DXContext().PSSetShaderResources( _SlotIndex, 1, &pView );
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