#pragma once

#include "Component.h"
#include "../Structures/PixelFormats.h"
#include "../Structures/DepthStencilFormats.h"
#include "../../Utility/TextureFilePOM.h"

class Texture2D : public Component {
protected:	// CONSTANTS

	static const int	MAX_TEXTURE_SIZE = 8192;	// Should be enough!
	static const int	MAX_TEXTURE_POT = 13;

	static const int	HASHTABLE_SIZE = 1 << 13;	// 8Kb per hashtable, 3 hashtable per texture => 24Kb overhead

private:	// FIELDS

	U32								m_Width;
	U32								m_Height;
	U32								m_ArraySize;
	U32								m_MipLevelsCount;

	const IFormatDescriptor&		m_Format;
	bool							m_bIsDepthStencil;
	bool							m_bIsCubeMap;

	ID3D11Texture2D*				m_pTexture;

	// Cached resource views
	mutable BaseLib::DictionaryU32	m_CachedSRVs;
	mutable BaseLib::DictionaryU32	m_CachedRTVs;
	mutable BaseLib::DictionaryU32	m_CachedUAVs;
	mutable BaseLib::DictionaryU32	m_CachedDSVs;
	mutable U32						m_LastAssignedSlots[6];
	mutable U32						m_LastAssignedSlotsUAV;
	D3D11_MAPPED_SUBRESOURCE		m_LockedResource;


public:	 // PROPERTIES

	U32							GetWidth() const			{ return m_Width; }
	U32							GetHeight() const			{ return m_Height; }
	U32							GetArraySize() const		{ return m_ArraySize; }
	U32							GetMipLevelsCount() const	{ return m_MipLevelsCount; }
	bool						IsCubeMap() const			{ return m_bIsCubeMap; }
	const IFormatDescriptor&	GetFormatDescriptor() const	{ return m_Format; }

	bfloat3						GetdUV() const				{ return bfloat3( 1.0f / m_Width, 1.0f / m_Height, 0.0f ); }


public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	// NOTE: If _ArraySize is < 0 then a cube map or cube map array is created (WARNING: the array size must be a valid multiple of 6!)
	Texture2D( Device& _Device, U32 _Width, U32 _Height, int _ArraySize, U32 _MipLevelsCount, const IPixelFormatDescriptor& _Format, const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false );
	// This is for creating a depth stencil buffer
	Texture2D( Device& _Device, U32 _Width, U32 _Height, U32 _ArraySize, const IDepthStencilFormatDescriptor& _Format );
	~Texture2D();

	// _AsArray is used to force the SRV as viewing a Texture2DArray instead of a TextureCube or TextureCubeArray
	ID3D11ShaderResourceView*	GetSRV( U32 _MipLevelStart=0, U32 _MipLevelsCount=0, U32 _ArrayStart=0, U32 _ArraySize=0, bool _AsArray=false ) const;	// Shader Resource View => Read-Only Input
	ID3D11RenderTargetView*		GetRTV( U32 _MipLevelIndex=0, U32 _ArrayStart=0, U32 _ArraySize=0 ) const;												// Render Target View => Write-Only Output
	ID3D11UnorderedAccessView*	GetUAV( U32 _MipLevelIndex=0, U32 _ArrayStart=0, U32 _ArraySize=0 ) const;												// Unordered Access View => Read/Write
	ID3D11DepthStencilView*		GetDSV( U32 _ArrayStart=0, U32 _ArraySize=0 ) const;																	// Depth Stencil View => Write-Only Depth Stencil Output

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

	// Used by the Device for the default backbuffer
	Texture2D( Device& _Device, ID3D11Texture2D& _Texture, const IPixelFormatDescriptor& _Format );

	// Texture access by the CPU
	void		CopyFrom( Texture2D& _SourceTexture );
	D3D11_MAPPED_SUBRESOURCE&	Map( U32 _MipLevelIndex, U32 _ArrayIndex );
	void		UnMap( U32 _MipLevelIndex, U32 _ArrayIndex );

#if defined(_DEBUG) || !defined(GODCOMPLEX)
	// I/O for staging textures
	void		Save( const char* _pFileName );
	void		Load( const char* _pFileName );

	// Creates an immutable texture from a POM file
	Texture2D( Device& _Device, const TextureFilePOM& _POM, bool _bUnOrderedAccess=false );
#endif

public:
	static void	NextMipSize( U32& _Width, U32& _Height );
	static U32	ComputeMipLevelsCount( U32 _Width, U32 _Height, U32 _MipLevelsCount );
	U32			CalcSubResource( U32 _MipLevelIndex, U32 _ArrayIndex );

private:
	// _bStaging, true if this is a staging texture (i.e. CPU accessible as read/write)
	// _bUnOrderedAccess, true if the texture can also be used as a UAV (Random access read/write from a compute shader)
	// _pMipDescriptors, if not NULL then the row pitch & depth pitch will be read from this array for each mip level
	//
	void		Init( const void* const* _ppContent, bool _bStaging=false, bool _bUnOrderedAccess=false, TextureFilePOM::MipDescriptor* _pMipDescriptors=NULL );
};

