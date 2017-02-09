#pragma once

#include "Component.h"
#include "../../Utility/TextureFilePOM.h"

class Texture3D : public Component {
protected:  // CONSTANTS

	static const U32	MAX_TEXTURE_SIZE = 8192;	// Should be enough !
	static const U32	MAX_TEXTURE_POT = 13;

private:	// FIELDS

	U32								m_width;
	U32								m_height;
	U32								m_depth;
	U32								m_mipLevelsCount;

	DXGI_FORMAT						m_format;

	ID3D11Texture3D*				m_texture;

	// Cached resource views
	mutable BaseLib::DictionaryU32	m_cachedSRVs;
	mutable BaseLib::DictionaryU32	m_cachedRTVs;
	mutable BaseLib::DictionaryU32	m_cachedUAVs;
	mutable U32						m_lastAssignedSlots[6];
	mutable U32						m_lastAssignedSlotsUAV;
	mutable D3D11_MAPPED_SUBRESOURCE m_lockedResource;


public:	 // PROPERTIES

	U32								GetWidth() const			{ return m_width; }
	U32								GetHeight() const			{ return m_height; }
	U32								GetDepth() const			{ return m_depth; }
	U32								GetMipLevelsCount() const	{ return m_mipLevelsCount; }
	DXGI_FORMAT						GetFormat() const			{ return m_format; }

	bfloat4							GetdUVW() const				{ return bfloat4( 1.0f / m_width, 1.0f / m_height, 1.0f / m_depth, 0.0f ); }

public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	Texture3D( Device& _device, U32 _width, U32 _height, U32 _depth, U32 _mipLevelsCount, BaseLib::PIXEL_FORMAT _format, BaseLib::COMPONENT_FORMAT _componentFormat, const void* const* _ppContent, bool _staging=false, bool _UAV=false );
	Texture3D( Device& _device, const ImageUtilityLib::ImagesMatrix& _images, BaseLib::COMPONENT_FORMAT _componentFormat=BaseLib::COMPONENT_FORMAT::AUTO );
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
	const D3D11_MAPPED_SUBRESOURCE&	MapRead( U32 _MipLevelIndex ) const;
	const D3D11_MAPPED_SUBRESOURCE&	MapWrite( U32 _MipLevelIndex );
	void		UnMap( U32 _MipLevelIndex ) const;

	// Conversion of a CPU-readable (i.e. staging) texture into an ImagesMatrix
	void		ReadAsImagesMatrix( ImageUtilityLib::ImagesMatrix& _images ) const;

// 	#if defined(_DEBUG) || !defined(GODCOMPLEX)
// 		// I/O for staging textures
// 		void		Save( const char* _pFileName );
// 		void		Load( const char* _pFileName );
// 
// 		// Creates an immutable texture from a POM file
// 		Texture3D( Device& _Device, const TextureFilePOM& _POM, bool _UAV=false );
// 	#endif

public:
	static void	NextMipSize( U32& _Width, U32& _Height, U32& _Depth );
	static U32	ComputeMipLevelsCount( U32 _Width, U32 _Height, U32 _Depth, U32 _MipLevelsCount );

private:
	// _staging, true if this is a staging texture (i.e. CPU accessible as read/write)
	// _UAV, true if the texture can also be used as a UAV (Random access read/write from a compute shader)
	// _pMipDescriptors, if not NULL then the row pitch & depth pitch will be read from this array for each mip level
	//
	void		Init( const void* const* _ppContent, bool _staging=false, bool _UAV=false, TextureFilePOM::MipDescriptor* _pMipDescriptors=NULL );
};

