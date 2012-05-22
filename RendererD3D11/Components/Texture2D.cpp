#include "Texture2D.h"

Texture2D::Texture2D( Device& _Device, int _Width, int _Height, int _ArraySize, const PixelFormatDescriptor& _Format, int _MipLevelsCount, const void* _ppContent[] ) : Component( _Device )
	, m_Format( _Format )
{
	ASSERT( _Width <= MAX_TEXTURE_SIZE );
	ASSERT( _Height <= MAX_TEXTURE_SIZE );

	m_Width = _Width;
	m_Height = _Height;
	m_ArraySize = _ArraySize;

	m_MipLevelsCount = ValidateMipLevels( _Width, _Height, _MipLevelsCount );

	D3D11_TEXTURE2D_DESC	Desc;
	Desc.Width = _Width;
	Desc.Height = _Height;
	Desc.ArraySize = _ArraySize;
	Desc.MipLevels = m_MipLevelsCount;
	Desc.Format = _Format.DirectXFormat();
	Desc.SampleDesc.Count = 1;
	Desc.SampleDesc.Quality = 0;
	Desc.Usage = _ppContent != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DEFAULT;
	Desc.BindFlags = _ppContent != NULL ? D3D11_BIND_SHADER_RESOURCE : D3D11_BIND_RENDER_TARGET;
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

			_Width = MAX( 1, (_Width >> 1) );
			_Height = MAX( 1, (_Height >> 1) );
		}

		Check( m_Device.DXDevice()->CreateTexture2D( &Desc, pInitialData, &m_pTexture ) );
	}
	else
		Check( m_Device.DXDevice()->CreateTexture2D( &Desc, NULL, &m_pTexture ) );
}

Texture2D::~Texture2D()
{
	ASSERT( m_pTexture != NULL );

	m_pTexture->Release();
	m_pTexture = NULL;
}

ID3D11ShaderResourceView*	Texture2D::GetShaderView( int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize )
{
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = m_MipLevelsCount - _MipLevelStart;

	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = m_ArraySize > 1 ? D3D11_SRV_DIMENSION_TEXTURE2DARRAY : D3D11_SRV_DIMENSION_TEXTURE2D;
	Desc.Texture2DArray.MostDetailedMip = _MipLevelStart;
	Desc.Texture2DArray.MipLevels = _MipLevelsCount;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11ShaderResourceView*	pView;
	Check( m_Device.DXDevice()->CreateShaderResourceView( m_pTexture, &Desc, &pView ) );

	return pView;
}

ID3D11RenderTargetView*		Texture2D::GetTargetView( int _MipLevelIndex, int _ArrayStart, int _ArraySize )
{
	if ( _ArraySize == 0 )
		_ArraySize = m_ArraySize - _ArrayStart;

	D3D11_RENDER_TARGET_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = m_ArraySize > 1 ? D3D11_RTV_DIMENSION_TEXTURE2DARRAY : D3D11_RTV_DIMENSION_TEXTURE2D;
	Desc.Texture2DArray.MipSlice = _MipLevelIndex;
	Desc.Texture2DArray.FirstArraySlice = _ArrayStart;
	Desc.Texture2DArray.ArraySize = _ArraySize;

	ID3D11RenderTargetView*	pView;
	Check( m_Device.DXDevice()->CreateRenderTargetView( m_pTexture, &Desc, &pView ) );

	return pView;
}

int	 Texture2D::ValidateMipLevels( int _Width, int _Height, int _MipLevelsCount )
{
	int MaxSize = MAX( _Width, _Height );
	int	MaxMipLevelsCount = int( ceilf( logf( MaxSize+1.0f ) / logf( 2.0f ) ) );
	
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = MaxMipLevelsCount;
	else
		_MipLevelsCount = MIN( _MipLevelsCount, MaxMipLevelsCount );

	ASSERT( _MipLevelsCount <= MAX_TEXTURE_POT );
	return _MipLevelsCount;
}