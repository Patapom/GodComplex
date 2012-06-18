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

	ID3D11Texture2D*	m_pTexture;

	// Cached resource views
	mutable DictionaryU32			m_CachedShaderViews;
	mutable DictionaryU32			m_CachedTargetViews;
	mutable ID3D11DepthStencilView*	m_pCachedDepthStencilView;


public:	 // PROPERTIES

	int	 GetWidth() const			{ return m_Width; }
	int	 GetHeight() const			{ return m_Height; }
	int	 GetArraySize() const		{ return m_ArraySize; }
	int	 GetMipLevelsCount() const	{ return m_MipLevelsCount; }


public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	Texture2D( Device& _Device, int _Width, int _Height, int _ArraySize, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent );
	// This is for creating a depth stencil buffer
	Texture2D( Device& _Device, int _Width, int _Height, const DepthStencilFormatDescriptor& _Format );
	~Texture2D();

	ID3D11ShaderResourceView*	GetShaderView( int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize ) const;
	ID3D11RenderTargetView*		GetTargetView( int _MipLevelIndex, int _ArrayStart, int _ArraySize ) const;
	ID3D11DepthStencilView*		GetDepthStencilView() const;

	// Uploads the texture to the shader
	void		Set( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetVS( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetGS( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetPS( int _SlotIndex, bool _bIKnowWhatImDoing=false );

	// Used by the Device for the default backbuffer
	Texture2D( Device& _Device, ID3D11Texture2D& _Texture, const IPixelFormatDescriptor& _Format );

public:
	static void	NextMipSize( int& _Width, int& _Height );
	static int	ComputeMipLevelsCount( int _Width, int _Height, int _MipLevelsCount );
};

