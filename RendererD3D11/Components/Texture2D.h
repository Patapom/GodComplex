#pragma once

#include "Component.h"
#include "../Structures/PixelFormats.h"
#include "../Structures/DepthStencilFormats.h"
#include "../../Utility/TextureFilePOM.h"

class Texture2D : public Component
{
protected:	// CONSTANTS

	static const int	MAX_TEXTURE_SIZE = 8192;	// Should be enough!
	static const int	MAX_TEXTURE_POT = 13;

	static const int	HASHTABLE_SIZE = 1 << 13;	// 8Kb per hashtable, 3 hashtable per texture => 24Kb overhead

private:	// FIELDS

	int								m_Width;
	int								m_Height;
	int								m_ArraySize;
	int								m_MipLevelsCount;

	const IFormatDescriptor&		m_Format;
	bool							m_bIsDepthStencil;
	bool							m_bIsCubeMap;

	ID3D11Texture2D*				m_pTexture;

	// Cached resource views
	mutable DictionaryU32			m_CachedShaderViews;
	mutable DictionaryU32			m_CachedTargetViews;
	mutable DictionaryU32			m_CachedUAVs;
	mutable ID3D11DepthStencilView*	m_pCachedDepthStencilView;
	mutable int						m_LastAssignedSlots[6];
	mutable int						m_LastAssignedSlotsUAV;
	D3D11_MAPPED_SUBRESOURCE		m_LockedResource;


public:	 // PROPERTIES

	int			GetWidth() const			{ return m_Width; }
	int			GetHeight() const			{ return m_Height; }
	int			GetArraySize() const		{ return m_ArraySize; }
	int			GetMipLevelsCount() const	{ return m_MipLevelsCount; }
	bool		IsCubeMap() const			{ return m_bIsCubeMap; }
	const IFormatDescriptor&	GetFormatDescriptor() const	{ return m_Format; }

	float3	GetdUV() const				{ return float3( 1.0f / m_Width, 1.0f / m_Height, 0.0f ); }


public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	Texture2D( Device& _Device, int _Width, int _Height, int _ArraySize, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false );
	// This is for creating a depth stencil buffer
	Texture2D( Device& _Device, int _Width, int _Height, const IDepthStencilFormatDescriptor& _Format, int _ArraySize=1 );
	~Texture2D();

	ID3D11ShaderResourceView*	GetShaderView( int _MipLevelStart=0, int _MipLevelsCount=0, int _ArrayStart=0, int _ArraySize=1 ) const;
	ID3D11RenderTargetView*		GetTargetView( int _MipLevelIndex=0, int _ArrayStart=0, int _ArraySize=0 ) const;
	ID3D11UnorderedAccessView*	GetUAV(  int _MipLevelIndex=0, int _ArrayStart=0, int _ArraySize=0 ) const;
	ID3D11DepthStencilView*		GetDepthStencilView() const;

	// Uploads the texture to the shader
	void		Set( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetVS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetHS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetDS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetGS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetPS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetCS( int _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		RemoveFromLastAssignedSlots() const;

	// Upload the texture as a UAV for a compute shader
	void		SetCSUAV( int _SlotIndex, ID3D11UnorderedAccessView* _pView=NULL ) const;
	void		RemoveFromLastAssignedSlotUAV() const;

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

	// Creates an immutable texture from a POM file
	Texture2D( Device& _Device, const TextureFilePOM& _POM, bool _bUnOrderedAccess=false );
#endif

public:
	static void	NextMipSize( int& _Width, int& _Height );
	static int	ComputeMipLevelsCount( int _Width, int _Height, int _MipLevelsCount );
	int			CalcSubResource( int _MipLevelIndex, int _ArrayIndex );

private:
	// _bStaging, true if this is a staging texture (i.e. CPU accessible as read/write)
	// _bUnOrderedAccess, true if the texture can also be used as a UAV (Random access read/write from a compute shader)
	// _pMipDescriptors, if not NULL then the row pitch & depth pitch will be read from this array for each mip level
	//
	void		Init( const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false, TextureFilePOM::MipDescriptor* _pMipDescriptors=NULL );
};

