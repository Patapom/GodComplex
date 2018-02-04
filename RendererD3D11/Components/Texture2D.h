#pragma once

#include "Component.h"
#include "../../Utility/TextureFilePOM.h"

#include "FreeImage.h"
#include "../../Packages/ImageUtilityLib/ImageFile.h"
#include "../../Packages/ImageUtilityLib/ImagesMatrix.h"

class Texture2D : public Component {
protected:	// CONSTANTS

	static const int	MAX_TEXTURE_SIZE = 8192;	// Should be enough!
	static const int	MAX_TEXTURE_POT = 13;

	static const int	HASHTABLE_SIZE = 1 << 13;	// 8Kb per hashtable, 3 hashtable per texture => 24Kb overhead

private:	// FIELDS

	U32								m_width;
	U32								m_height;
	U32								m_arraySize;
	U32								m_mipLevelsCount;

	DXGI_FORMAT						m_format;
	bool							m_isCubeMap;

	ID3D11Texture2D*				m_texture;

	// Cached resource views
	mutable BaseLib::DictionaryU32	m_cachedSRVs;
	mutable BaseLib::DictionaryU32	m_cachedRTVs;
	mutable BaseLib::DictionaryU32	m_cachedUAVs;
	mutable BaseLib::DictionaryU32	m_cachedDSVs;
	mutable U32						m_lastAssignedSlots[6];
	mutable U32						m_lastAssignedSlotsUAV;
	mutable D3D11_MAPPED_SUBRESOURCE m_lockedResource;


public:	 // PROPERTIES

	U32								GetWidth() const			{ return m_width; }
	U32								GetHeight() const			{ return m_height; }
	U32								GetArraySize() const		{ return m_arraySize; }
	U32								GetMipLevelsCount() const	{ return m_mipLevelsCount; }
	bool							IsCubeMap() const			{ return m_isCubeMap; }
	bool							IsDepthFormat() const;
	DXGI_FORMAT						GetFormat() const			{ return m_format; }

	bfloat3							GetdUV() const				{ return bfloat3( 1.0f / m_width, 1.0f / m_height, 0.0f ); }


public:	 // METHODS

	// NOTE: If _ppContents == NULL then the texture is considered a render target !
	// NOTE: If _arraySize is < 0 then a cube map or cube map array is created (WARNING: the array size must be a valid multiple of 6!)
	Texture2D( Device& _device, U32 _width, U32 _height, int _arraySize, U32 _mipLevelsCount, BaseLib::PIXEL_FORMAT _format, BaseLib::COMPONENT_FORMAT _componentFormat, const void* const* _ppContent, bool _staging=false, bool _UAV=false );
	Texture2D( Device& _device, const ImageUtilityLib::ImagesMatrix& _images, BaseLib::COMPONENT_FORMAT _componentFormat=BaseLib::COMPONENT_FORMAT::AUTO );
	Texture2D( Device& _device, U32 _width, U32 _height, U32 _arraySize, U32 _mipLevelsCount, BaseLib::PIXEL_FORMAT _format, BaseLib::DEPTH_COMPONENT_FORMAT _depthComponentFormat );	// This is for creating a depth stencil buffer
	Texture2D( Device& _device, ID3D11Texture2D& _Texture );																										// Used by the Device for the default backbuffer, shouldn't be used otherwise
	~Texture2D();

	// _asArray is used to force the SRV as viewing a Texture2DArray instead of a TextureCube or TextureCubeArray
	ID3D11ShaderResourceView*	GetSRV( U32 _mipLevelStart=0, U32 _mipLevelsCount=0, U32 _arrayStart=0, U32 _arraySize=0, bool _asArray=false ) const;	// Shader Resource View => Read-Only Input
	ID3D11RenderTargetView*		GetRTV( U32 _mipLevelIndex=0, U32 _arrayStart=0, U32 _arraySize=0 ) const;												// Render Target View => Write-Only Output
	ID3D11UnorderedAccessView*	GetUAV( U32 _mipLevelIndex=0, U32 _arrayStart=0, U32 _arraySize=0 ) const;												// Unordered Access View => Read/Write
	ID3D11DepthStencilView*		GetDSV( U32 _arrayStart=0, U32 _arraySize=0 ) const;																	// Depth Stencil View => Write-Only Depth Stencil Output

	// Uploads the texture to the shader
	void		Set( U32 _slotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _view=NULL ) const;
	void		SetVS( U32 _slotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _view=NULL ) const;
	void		SetHS( U32 _slotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _view=NULL ) const;
	void		SetDS( U32 _slotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _view=NULL ) const;
	void		SetGS( U32 _slotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _view=NULL ) const;
	void		SetPS( U32 _slotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _view=NULL ) const;
	void		SetCS( U32 _slotIndex, bool _bIKnowWhatImDoing=false, ID3D11ShaderResourceView* _view=NULL ) const;
	void		RemoveFromLastAssignedSlots() const;

	// Upload the texture as a UAV for a compute shader
	void		SetCSUAV( U32 _slotIndex, ID3D11UnorderedAccessView* _view=NULL ) const;
	void		RemoveFromLastAssignedSlotUAV() const;

	// Texture access by the CPU
	void		CopyFrom( Texture2D& _SourceTexture );
	const D3D11_MAPPED_SUBRESOURCE&	MapRead( U32 _mipLevelIndex, U32 _arrayIndex ) const;
	const D3D11_MAPPED_SUBRESOURCE&	MapWrite( U32 _mipLevelIndex, U32 _arrayIndex );
	void		UnMap( U32 _mipLevelIndex, U32 _arrayIndex ) const;

	// Conversion of a CPU-readable (i.e. staging) texture into an ImagesMatrix
	void		ReadAsImagesMatrix( ImageUtilityLib::ImagesMatrix& _images ) const;

// 	#if defined(_DEBUG) || !defined(GODCOMPLEX)
// 		// I/O for staging textures
// 		void		Save( const char* _pFileName );
// 		void		Load( const char* _pFileName );
// 
// 		// Creates an immutable texture from a POM file
// 		Texture2D( Device& _device, const TextureFilePOM& _POM, bool _UAV=false );
// 	#endif

public:	// HELPERS

// 	static DXGI_FORMAT	PixelAccessor2DXGIFormat( const BaseLib::IPixelAccessor& _pixelAccessor, BaseLib::COMPONENT_FORMAT _componentFormat );
	enum class DEPTH_ACCESS_TYPE {
		SURFACE_CREATION,	// The DXGI format used when the surface is created
		VIEW_WRITABLE,		// The DXGI format used by a shader to write the depth values (i.e. surface is used as depth stencil buffer)
		VIEW_READABLE,		// The DXGI format used by a shader to read the depth values (i.e. surface is used as a regular texture)
	};
	DXGI_FORMAT			DepthDXGIFormat( DEPTH_ACCESS_TYPE _accessType ) const;

	static void			NextMipSize( U32& _width, U32& _height );
	static U32			ComputeMipLevelsCount( U32 _width, U32 _height, U32 _mipLevelsCount );
	U32					CalcSubResource( U32 _mipLevelIndex, U32 _arrayIndex ) const;
	static bool			IsCompressedFormat( DXGI_FORMAT _format );

private:
	// _staging, true if this is a staging texture (i.e. CPU accessible as read/write)
	// _UAV, true if the texture can also be used as a UAV (Random access read/write from a compute shader)
	// _pMipDescriptors, if not NULL then the row pitch & depth pitch will be read from this array for each mip level
	//
	void		Init( const void* const* _ppContent, bool _staging=false, bool _UAV=false, TextureFilePOM::MipDescriptor* _pMipDescriptors=NULL );
};

