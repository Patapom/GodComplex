#pragma once

#include "Component.h"
#include "../Structures/PixelFormats.h"
#include "../../Utility/TextureFilePOM.h"

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
	mutable DictionaryU32			m_CachedSRVs;
	mutable DictionaryU32			m_CachedRTVs;
	mutable DictionaryU32			m_CachedUAVs;
	mutable int						m_LastAssignedSlots[6];
	mutable int						m_LastAssignedSlotsUAV;
	D3D11_MAPPED_SUBRESOURCE		m_LockedResource;


public:	 // PROPERTIES

	int	 GetWidth() const			{ return m_Width; }
	int	 GetHeight() const			{ return m_Height; }
	int	 GetDepth() const			{ return m_Depth; }
	int	 GetMipLevelsCount() const	{ return m_MipLevelsCount; }
	const IFormatDescriptor&	GetFormatDescriptor() const	{ return m_Format; }

	float4	GetdUVW() const		{ return float4( 1.0f / m_Width, 1.0f / m_Height, 1.0f / m_Depth, 0.0f ); }

public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	Texture3D( Device& _Device, int _Width, int _Height, int _Depth, const IPixelFormatDescriptor& _Format, int _MipLevelsCount, const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false );
	~Texture3D();

	ID3D11ShaderResourceView*	GetShaderView( int _MipLevelStart=0, int _MipLevelsCount=0 ) const;
	ID3D11RenderTargetView*		GetTargetView( int _MipLevelIndex=0, int _FirstWSlice=0, int _WSize=0 ) const;
	ID3D11UnorderedAccessView*	GetUAV( int _MipLevelIndex, int _FirstWSlice, int _WSize ) const;

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

	// Texture access by the CPU
	void		CopyFrom( Texture3D& _SourceTexture );
	D3D11_MAPPED_SUBRESOURCE&	Map( int _MipLevelIndex );
	void		UnMap( int _MipLevelIndex );

#ifdef _DEBUG
	// I/O for staging textures
	void		Save( const char* _pFileName );
	void		Load( const char* _pFileName );

	// Creates an immutable texture from a POM file
	Texture3D( Device& _Device, const TextureFilePOM& _POM, bool _bUnOrderedAccess=false );
#endif

public:
	static void	NextMipSize( int& _Width, int& _Height, int& _Depth );
	static int	ComputeMipLevelsCount( int _Width, int _Height, int _Depth, int _MipLevelsCount );

private:
	// _bStaging, true if this is a staging texture (i.e. CPU accessible as read/write)
	// _bUnOrderedAccess, true if the texture can also be used as a UAV (Random access read/write from a compute shader)
	// _pMipDescriptors, if not NULL then the row pitch & depth pitch will be read from this array for each mip level
	//
	void		Init( const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false, TextureFilePOM::MipDescriptor* _pMipDescriptors=NULL );
};

