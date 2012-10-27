#pragma once

#include "Component.h"
#include "../Structures/PixelFormats.h"

class Texture3D : public Component
{
protected:  // CONSTANTS

	static const int	MAX_TEXTURE_SIZE = 8192;	// Should be enough !
	static const int	MAX_TEXTURE_POT = 13;

private:	// FIELDS

	int				 m_Width;
	int				 m_Height;
	int				 m_Depth;
	int				 m_MipLevelsCount;
	const IPixelFormatDescriptor&  m_Format;

	ID3D11Texture3D*	m_pTexture;

	// Cached resource views
	mutable DictionaryU32			m_CachedShaderViews;
	mutable DictionaryU32			m_CachedTargetViews;


public:	 // PROPERTIES

	int	 GetWidth() const			{ return m_Width; }
	int	 GetHeight() const			{ return m_Height; }
	int	 GetDepth() const			{ return m_Depth; }
	int	 GetMipLevelsCount() const	{ return m_MipLevelsCount; }

public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	Texture3D( Device& _Device, int _Width, int _Height, int _Depth, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent );
	~Texture3D();

	ID3D11ShaderResourceView*	GetShaderView( int _MipLevelStart, int _MipLevelsCount ) const;
	ID3D11RenderTargetView*		GetTargetView( int _MipLevelIndex, int _FirstWSlice, int _WSize ) const;

	// Uploads the texture to the shader
	void		Set( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetVS( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetHS( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetDS( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetGS( int _SlotIndex, bool _bIKnowWhatImDoing=false );
	void		SetPS( int _SlotIndex, bool _bIKnowWhatImDoing=false );

public:
	static void	NextMipSize( int& _Width, int& _Height, int& _Depth );
	static int	ComputeMipLevelsCount( int _Width, int _Height, int _Depth, int _MipLevelsCount );
};

