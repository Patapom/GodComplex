#pragma once

#include "Component.h"
#include "../Structures/PixelFormats.h"
#include "../../Utility/TextureFilePOM.h"

class Texture3D : public Component
{
protected:  // CONSTANTS

	static const U32	MAX_TEXTURE_SIZE = 8192;	// Should be enough !
	static const U32	MAX_TEXTURE_POT = 13;

private:	// FIELDS

	U32				 m_Width;
	U32				 m_Height;
	U32				 m_Depth;
	U32				 m_MipLevelsCount;
	const IPixelFormatDescriptor&  m_Format;

	ID3D11Texture3D*	m_pTexture;

	// Cached resource views
	mutable BaseLib::DictionaryU32	m_CachedSRVs;
	mutable BaseLib::DictionaryU32	m_CachedRTVs;
	mutable BaseLib::DictionaryU32	m_CachedUAVs;
	mutable U32						m_LastAssignedSlots[6];
	mutable U32						m_LastAssignedSlotsUAV;
	D3D11_MAPPED_SUBRESOURCE		m_LockedResource;


public:	 // PROPERTIES

	U32	 GetWidth() const			{ return m_Width; }
	U32	 GetHeight() const			{ return m_Height; }
	U32	 GetDepth() const			{ return m_Depth; }
	U32	 GetMipLevelsCount() const	{ return m_MipLevelsCount; }
	const IFormatDescriptor&	GetFormatDescriptor() const	{ return m_Format; }

	bfloat4	GetdUVW() const		{ return bfloat4( 1.0f / m_Width, 1.0f / m_Height, 1.0f / m_Depth, 0.0f ); }

public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	Texture3D( Device& _Device, U32 _Width, U32 _Height, U32 _Depth, U32 _MipLevelsCount, const IPixelFormatDescriptor& _Format, const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false );
	~Texture3D();

	// _AsArray is used to force the SRV as viewing a Texture2DArray instead of a Texture3D (note that otherwise, _FirstWSlice and _WSize are not used)
	ID3D11ShaderResourceView*	GetSRV( U32 _MipLevelStart=0, U32 _MipLevelsCount=0, U32 _FirstWSlice=0, U32 _WSize=0, bool _AsArray=false ) const;			// Shader Resource View => Read-Only Input
	ID3D11RenderTargetView*		GetRTV( U32 _MipLevelIndex=0, U32 _FirstWSlice=0, U32 _WSize=0 ) const;	// Render Target View => Write-Only Output
	ID3D11UnorderedAccessView*	GetUAV( U32 _MipLevelIndex, U32 _FirstWSlice, U32 _WSize ) const;		// Unordered Access View => Read/Write

	// Uploads the texture to the shader
	void		Set( U32 _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetVS( U32 _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetHS( U32 _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetDS( U32 _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetGS( U32 _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetPS( U32 _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		SetCS( U32 _SlotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _pView=NULL ) const;
	void		RemoveFromLastAssignedSlots() const;

	// Upload the texture as a UAV for a compute shader
	void		SetCSUAV( U32 _SlotIndex, ID3D11UnorderedAccessView* _pView=NULL ) const;
	void		RemoveFromLastAssignedSlotUAV() const;

	// Texture access by the CPU
	void		CopyFrom( Texture3D& _SourceTexture );
	D3D11_MAPPED_SUBRESOURCE&	Map( U32 _MipLevelIndex );
	void		UnMap( U32 _MipLevelIndex );

#if defined(_DEBUG) || !defined(GODCOMPLEX)
	// I/O for staging textures
	void		Save( const char* _pFileName );
	void		Load( const char* _pFileName );

	// Creates an immutable texture from a POM file
	Texture3D( Device& _Device, const TextureFilePOM& _POM, bool _bUnOrderedAccess=false );
#endif

public:
	static void	NextMipSize( U32& _Width, U32& _Height, U32& _Depth );
	static U32	ComputeMipLevelsCount( U32 _Width, U32 _Height, U32 _Depth, U32 _MipLevelsCount );

private:
	// _bStaging, true if this is a staging texture (i.e. CPU accessible as read/write)
	// _bUnOrderedAccess, true if the texture can also be used as a UAV (Random access read/write from a compute shader)
	// _pMipDescriptors, if not NULL then the row pitch & depth pitch will be read from this array for each mip level
	//
	void		Init( const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false, TextureFilePOM::MipDescriptor* _pMipDescriptors=NULL );
};

