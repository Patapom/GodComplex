#pragma once

//////////////////////////////////////////////////////////////////////////
#include "Types.h"

class NjIPrimitive;
class NjITexture;
class NjITextureView;
class NjIShader;
class NjIConstantBuffer;
class NjITextureBuffer;

enum NJ_TEXTURE_FORMAT
{
//	NJ_TEXTURE_FORMAT_ARGB,
//	NJ_TEXTURE_FORMAT_ARGB_sRGB,
	NJ_TEXTURE_FORMAT_ABGR16F,
//	NJ_TEXTURE_FORMAT_RG16F,
	NJ_TEXTURE_FORMAT_R32F,
};

enum NJ_PRIMITIVE_TOPOLOGY
{
	NJ_PRIMITIVE_TRIANGLE_LIST,
	NJ_PRIMITIVE_TRIANGLE_STRIP,
//	NJ_PRIMITIVE_POINT_LIST
};

enum NJ_VERTEX_FORMAT
{
	NJ_VERTEX_PT4,					// Point Transformed in clip space
	NJ_VERTEX_P3,					// Point
// 	NJ_VERTEX_P3N3,					// Point & normal
// 	NJ_VERTEX_P3T2,					// Point & UV
	NJ_VERTEX_P3N3G3T2,				// Point & normal & tangent & UV
};

enum NJ_RENDER_STATE
{
	NJ_RENDER_STATE_NO_CULLING_NO_Z_NO_BLENDING,	// No culling, Z read/write disabled, Blending disabled
};

enum NJ_SAMPLER_STATE
{
	NJ_SAMPLER_STATE_POINT_WRAP,	// AddressUVW = WRAP	---- MAG/MIN/MIPFILTER = POINT
	NJ_SAMPLER_STATE_POINT_CLAMP,	// AddressUVW = CLAMP   ---- MAG/MIN/MIPFILTER = POINT
	NJ_SAMPLER_STATE_POINT_MIRROR,	// AddressUVW = MIRROR  ---- MAG/MIN/MIPFILTER = POINT

	NJ_SAMPLER_STATE_LINEAR_WRAP,	// AddressUVW = WRAP	---- MAG/MIN/MIPFILTER = LINEAR
	NJ_SAMPLER_STATE_LINEAR_CLAMP,	// AddressUVW = CLAMP   ---- MAG/MIN/MIPFILTER = LINEAR
	NJ_SAMPLER_STATE_LINEAR_MIRROR,	// AddressUVW = MIRROR  ---- MAG/MIN/MIPFILTER = LINEAR
};

struct NJ_MACRO
{
	const char* pKey;
	const char* pValue;
};

struct NjViewport
{
	int		x, y, width, height;
	float	zmin, zmax;

	NjViewport() : x(0), y(0), width(0), height(0), zmin(0.0f), zmax(1.0f)	{}
};

//////////////////////////////////////////////////////////////////////////
// NjIDisposable objects
class NjIDisposable
{
public:
	virtual	~NjIDisposable() {}

	// Disposes of the object
	virtual void	Dispose() = 0;

};

//////////////////////////////////////////////////////////////////////////
// Engine interface you need to implement on your end
//
class NjIEngine : public NjIDisposable
{
public: // MEMORY

	// Allocates memory whose scope is Nuaj' lifetime
	virtual void*	Allocate( size_t _Size ) = 0;

	// Releases memory
	virtual void	Delete( void* _pMemory ) = 0;


public: // TEXTURES & RENDER TARGETS

	virtual NjITexture*	CreateTexture2D( const char* _pDebugName, int _Width, int _Height, NJ_TEXTURE_FORMAT _Format, int _MipLevelsCount, const void* const* _ppContent ) = 0;
//	virtual NjITexture*	CreateTexture2DArray( const char* _pDebugName, int _Width, int _Height, int _ArraySize, NJ_TEXTURE_FORMAT _Format, int _MipLevelsCount, const void* const* _ppContent ) = 0;
	virtual NjITexture*	CreateTexture3D( const char* _pDebugName, int _Width, int _Height, int _Depth, NJ_TEXTURE_FORMAT _Format, int _MipLevelsCount, const void* const* _ppContent ) = 0;

	virtual NjITexture*	CreateRenderTarget2D( const char* _pDebugName, int _Width, int _Height, NJ_TEXTURE_FORMAT _Format, int _MipLevelsCount, bool _CPUReadBack, bool _IsTemporary ) = 0;
//	virtual NjITexture*	CreateRenderTarget2DArray( const char* _pDebugName, int _Width, int _Height, int _ArraySize, NJ_TEXTURE_FORMAT _Format, int _MipLevelsCount, bool _CPUReadBack, bool _IsTemporary ) = 0;
	virtual NjITexture*	CreateRenderTarget3D( const char* _pDebugName, int _Width, int _Height, int _Depth, NJ_TEXTURE_FORMAT _Format, int _MipLevelsCount, bool _CPUReadBack, bool _IsTemporary ) = 0;

	// Sets the multiple render targets and optional _Depth stencil buffer
	// If _pViewport is NULL then the render target's full viewport is used with default ZMin/Max=0/1
	virtual void		SetRenderTargets( int _RenderTargetsCount, NjITextureView** _ppRenderTargets, NjITextureView* _pDepthStencil, const NjViewport* _pViewport ) = 0;


public: // GEOMETRY

	// Creates a nice primitive to render with
	// NOTE: indicesCount can be 0 and indices can be NULL, in which case the provided primitive is obviously a non-indexed primitive
	virtual NjIPrimitive*	CreatePrimitive( const char* _pDebugName, int _VerticesCount, void* _pVertices, int _IndicesCount, U16* _pIndices, NJ_PRIMITIVE_TOPOLOGY _Topology, NJ_VERTEX_FORMAT _Format ) = 0;


public: // RENDER STATES

	// There's only one really...
	virtual void		SetRenderStates( NJ_RENDER_STATE _RenderState ) = 0;


public: // SHADERS

	// Creates & compiles a shader from a shader ID + a NULL-terminated array of macros (or NULL if no macro)
	virtual NjIShader*	CreateShader( const char* _pDebugName, NjResourceID _ShaderID, NJ_MACRO* _pCompilerMacros ) = 0;

	// Creates a constant buffer to feed a shader with
	// NOTE: If _IsDynamic is false then _pInitData cannot be NULL and must be filled with the immutable constant buffer
	virtual NjIConstantBuffer*	CreateConstantBuffer( const char* _pDebugName, bool _IsDynamic, int _BufferSize, void* _pInitData ) = 0;
};

//////////////////////////////////////////////////////////////////////////
//
class NjIPrimitive : public NjIDisposable
{
public: // METHODS

	// Render this primitive using the provided shader
	virtual void	Render( NjIShader& _Shader ) = 0;
};

//////////////////////////////////////////////////////////////////////////
//
class NjITexture : public NjIDisposable
{
public: // PROPERTIES

	virtual int		GetWidth() const = 0;
	virtual int		GetHeight() const = 0;
	virtual int		GetArraySize() const = 0;	// Valid only for Array textures/RTs
	virtual int		GetDepth() const = 0;		// Valid only for 3D textures/RTs
	virtual int		GetMipLevelsCount() const = 0;

public: // METHODS

	// Views
	// NOTE: if _MipLevelsCount==0 then you should include ALL mip levels
	// NOTE: if _ArraySize==0 then you should include ALL array slices
	// NOTE: A typical view query with all parameters equal to 0 means returning the complete texture/render target view with all its mips and arrays
	// NOTE: For textures created with the "_IsTemporary" flag, Shader/Target views can be queried EVEN if the texture has not been physically allocated !
	virtual NjITextureView*	GetShaderView( int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize ) = 0;
	virtual NjITextureView*	GetRenderTargetView( int _MipLevelIndex, int _ArrayStart, int _ArraySize ) = 0;

	// Allocate/Free render target (Temporary render targets only)
	// NOTE: For textures created with the "_IsTemporary" flag, Shader/Target views can be queried EVEN if the texture has not been physically allocated !
	virtual void	AllocateTemp() = 0;
	// NOTE: Dispose() can be called without having called FreeTemp(), in which case you should obviously free the temporary texture as well
	virtual void	FreeTemp() = 0;

	// Locking (Render targets only)
	virtual void*	LockRead( int _MipLevelIndex, int& _RowPitch, int& _DepthPitch ) = 0;
	virtual void	Unlock() = 0;
};

class NjITextureView : public NjIDisposable
{
public: // METHODS

	virtual int		GetWidth() const = 0;
	virtual int		GetHeight() const = 0;

	// Return true if there is need for mapping correction (i.e. DirectX 9 mode) (cf. http://msdn.microsoft.com/en-us/library/windows/desktop/bb219690(v=vs.85).aspx)
	virtual bool	MustCorrectTexelToPixelMapping() const = 0;
};


//////////////////////////////////////////////////////////////////////////
//
class NjIShader : public NjIDisposable
{
public: // METHODS

	// Sends data to the specified constant buffer
	// Returns true if the constant buffer was indeed required by the shader
	virtual bool	SetConstantBuffer( const char* _pBufferName, NjIConstantBuffer& _Buffer ) = 0;
	virtual void	SetConstantBuffer( int _BufferSlot, NjIConstantBuffer& _Buffer ) = 0;

	// Sets the texture by name
	// Shouldn't crash if the texture doesn't exist !
	virtual bool	SetTexture( const char* _pTextureName, NjITextureView* _pTexture, NJ_SAMPLER_STATE _SamplerState ) = 0;
	virtual void	SetTexture( int _BufferSlot, NjITextureView* _pTexture, NJ_SAMPLER_STATE _SamplerState ) = 0;

//	 // Gets a texture sampler by name
//	 // Returns NULL if name does not exist
//	 virtual NjITextureSampler*   GetTextureSamplerByName( const char* name ) = 0;

	// Use this shader for rendering
	virtual void	Use() = 0;
};

class NjIConstantBuffer : public NjIDisposable
{
public: // METHODS

	// Updates the constants
	// NOTE: Valid only if the buffer was created with _IsDynamic = true
	virtual void	Update( void* _pData ) = 0;

};

class NjITextureSampler : public NjIDisposable
{
public: // METHODS

	// Sends the texture to the GPU
	virtual void	Set( NjITextureView* _pTexture, NJ_SAMPLER_STATE _SamplerState ) = 0;

};
