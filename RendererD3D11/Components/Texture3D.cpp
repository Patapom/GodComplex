#include "Texture3D.h"

Texture3D::Texture3D( Device& _Device, int _Width, int _Height, int _Depth, const PixelFormatDescriptor& _Format, int _MipLevelsCount, const void* _ppContent[] ) : Component( _Device )
	, m_Format( _Format )
{
	ASSERT( _Width <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( _Height <= MAX_TEXTURE_SIZE, "Texture size out of range !" );
	ASSERT( _Depth <= MAX_TEXTURE_SIZE, "Texture size out of range !" );

	m_Width = _Width;
	m_Height = _Height;
	m_Depth = _Depth;

	m_MipLevelsCount = ValidateMipLevels( _Width, _Height, _Depth, _MipLevelsCount );

	D3D11_TEXTURE3D_DESC	Desc;
	Desc.Width = _Width;
	Desc.Height = _Height;
	Desc.Depth = _Depth;
	Desc.MipLevels = m_MipLevelsCount;
	Desc.Format = _Format.DirectXFormat();
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
			_Depth = MAX( 1, (_Depth >> 1) );
		}

		Check( m_Device.DXDevice()->CreateTexture3D( &Desc, pInitialData, &m_pTexture ) );
	}
	else
		Check( m_Device.DXDevice()->CreateTexture3D( &Desc, NULL, &m_pTexture ) );
}

Texture3D::~Texture3D()
{
	ASSERT( m_pTexture != NULL, "Invalid texture to destroy !" );

	m_pTexture->Release();
	m_pTexture = NULL;
}

ID3D11ShaderResourceView*	Texture3D::GetShaderView( int _MipLevelStart, int _MipLevelsCount )
{
	if ( _MipLevelsCount == 0 )
		_MipLevelsCount = m_MipLevelsCount - _MipLevelStart;

	D3D11_SHADER_RESOURCE_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MostDetailedMip = _MipLevelStart;
	Desc.Texture3D.MipLevels = _MipLevelsCount;

	ID3D11ShaderResourceView*	pView;
	Check( m_Device.DXDevice()->CreateShaderResourceView( m_pTexture, &Desc, &pView ) );

	return pView;
}

ID3D11RenderTargetView*		Texture3D::GetTargetView( int _MipLevelIndex, int _FirstWSlice, int _WSize )
{
	if ( _WSize == 0 )
		_WSize = m_Depth - _FirstWSlice;

	D3D11_RENDER_TARGET_VIEW_DESC	Desc;
	Desc.Format = m_Format.DirectXFormat();
	Desc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE3D;
	Desc.Texture3D.MipSlice = _MipLevelIndex;
	Desc.Texture3D.FirstWSlice = _FirstWSlice;
	Desc.Texture3D.WSize = _WSize;

	ID3D11RenderTargetView*	pView;
	Check( m_Device.DXDevice()->CreateRenderTargetView( m_pTexture, &Desc, &pView ) );

	return pView;
}

int	 Texture3D::ValidateMipLevels( int _Width, int _Height, int _Depth, int _MipLevelsCount )
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