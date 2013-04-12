#pragma once

#include "Component.h"
#include "../Structures/PixelFormats.h"
#include "../Structures/DepthStencilFormats.h"

class Texture2D : public Component
{
protected:	// CONSTANTS

	static const int	MAX_TEXTURE_SIZE = 8192;	// Should be enough !
	static const int	MAX_TEXTURE_POT = 13;

	static const int	HASHTABLE_SIZE = 1 << 13;	// 8Kb per hashtable, 2 hashtable per texture => 16Kb overhead

private:	// FIELDS

	int					m_Width;
	int					m_Height;
	int					m_ArraySize;
	int					m_MipLevelsCount;

	const IFormatDescriptor&	m_Format;
	bool				m_bIsDepthStencil;
	bool				m_bIsCubeMap;

	ID3D11Texture2D*	m_pTexture;

	// Cached resource views
	mutable DictionaryU32			m_CachedShaderViews;
	mutable DictionaryU32			m_CachedTargetViews;
	mutable ID3D11DepthStencilView*	m_pCachedDepthStencilView;

	D3D11_MAPPED_SUBRESOURCE		m_LockedResource;


public:	 // PROPERTIES

	int			GetWidth() const			{ return m_Width; }
	int			GetHeight() const			{ return m_Height; }
	int			GetArraySize() const		{ return m_ArraySize; }
	int			GetMipLevelsCount() const	{ return m_MipLevelsCount; }

	NjFloat3	GetdUV() const				{ return NjFloat3( 1.0f / m_Width, 1.0f / m_Height, 0.0f ); }


public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	Texture2D( Device& _Device, int _Width, int _Height, int _ArraySize, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent, bool _bStaging=false, bool _bWriteable=false, bool _bUnOrderedAccess=false );
	// This is for creating a depth stencil buffer
	Texture2D( Device& _Device, int _Width, int _Height, const IDepthStencilFormatDescriptor& _Format );
	~Texture2D();

	ID3D11ShaderResourceView*	GetShaderView( int _MipLevelStart=0, int _MipLevelsCount=0, int _ArrayStart=0, int _ArraySize=1 ) const;
	ID3D11RenderTargetView*		GetTargetView( int _MipLevelIndex=0, int _ArrayStart=0, int _ArraySize=0 ) const;
	ID3D11DepthStencilView*		GetDepthStencilView() const;

	// Uploads the texture to the shader
	void		Set( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetVS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetHS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetDS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetGS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetPS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;

	// Used by the Device for the default backbuffer
	Texture2D( Device& _Device, ID3D11Texture2D& _Texture, const IPixelFormatDescriptor& _Format );

	// Texture access by the CPU
	void		CopyFrom( Texture2D& _SourceTexture );
	D3D11_MAPPED_SUBRESOURCE&	Map( int _MipLevelIndex, int _ArrayIndex );
	void		UnMap( int _MipLevelIndex, int _ArrayIndex );

#ifdef _DEBUG
	// I/O for staging textures
	void		Save( const char* _pFileName );
	void		Load( const char* _pFileName );
#endif

public:
	static void	NextMipSize( int& _Width, int& _Height );
	static int	ComputeMipLevelsCount( int _Width, int _Height, int _MipLevelsCount );
	int			CalcSubResource( int _MipLevelIndex, int _ArrayIndex );
};

