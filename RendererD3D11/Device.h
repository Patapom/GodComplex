#pragma once
#include "Renderer.h"


//#define NSIGHT			// Define this if you're debugging the app using Nvidia Nsight
#define RENDERDOC		// Define this if you're debugging the app using Crytek's RenderDoc

//#define DIRECTX10		// Define this to use DX10, otherwise DX11 will be used
#define TRY_DIRECTX10_1	// Define this to attempt using DX10.1

class Component;
class Shader;
class Texture2D;
class Texture3D;
class RasterizerState;
class DepthStencilState;
class BlendState;

class Device
{
	static const int	SAMPLERS_COUNT = 8;

public:		// NESTED TYPES

	enum	SHADER_STAGE_FLAGS
	{
		SSF_VERTEX_SHADER		= (1 << 0),
		SSF_HULL_SHADER			= (1 << 1),
		SSF_DOMAIN_SHADER		= (1 << 2),
		SSF_GEOMETRY_SHADER		= (1 << 3),
		SSF_PIXEL_SHADER		= (1 << 4),
		SSF_COMPUTE_SHADER		= (1 << 5),
		SSF_COMPUTE_SHADER_UAV	= (1 << 6),		// WARNING: SSF_ALL doesn't include UAVs!

		SSF_ALL					= (1 << 6)-1	// WARNING: SSF_ALL doesn't include UAVs!
	};

private:	// FIELDS

	ID3D11Device*			m_pDevice;
	ID3D11DeviceContext*	m_pDeviceContext;
	IDXGISwapChain*			m_pSwapChain;

	Texture2D*				m_pDefaultRenderTarget;	// The back buffer to render to the screen
	Texture2D*				m_pDefaultDepthStencil;	// The default depth stencil

	ID3D11SamplerState*		m_ppSamplers[SAMPLERS_COUNT];

	Component*				m_pComponentsStackTop;	// Remember this is the stack TOP so access the components using their m_pNext pointer to reach back to the bottom

	Shader*				m_pCurrentMaterial;		// The currently used material
	RasterizerState*		m_pCurrentRasterizerState;
	DepthStencilState*		m_pCurrentDepthStencilState;
	BlendState*				m_pCurrentBlendState;

	int						m_StatesCount;

	// Default blend & stencil refs
	float4				m_BlendFactors;
	U32						m_BlendMasks;
	U8						m_StencilRef;

public:

	RasterizerState*		m_pRS_CullNone;
	RasterizerState*		m_pRS_CullBack;
	RasterizerState*		m_pRS_CullFront;
	RasterizerState*		m_pRS_WireFrame;

	DepthStencilState*		m_pDS_Disabled;
	DepthStencilState*		m_pDS_ReadWriteLess;
	DepthStencilState*		m_pDS_ReadWriteGreater;
		// Write disabled
	DepthStencilState*		m_pDS_ReadLessEqual;
	DepthStencilState*		m_pDS_ReadLessEqual_StencilIncBackDecFront;	// Useful for deferred rendering
	DepthStencilState*		m_pDS_ReadLessEqual_StencilFailIfZero;		// Useful for deferred rendering

	BlendState*				m_pBS_ZPrePass;					// Special double-speed Z Prepass blend mode with no color write (from §3.6.1 http://developer.download.nvidia.com/GPU_Programming_Guide/GPU_Programming_Guide_G80.pdf)
	BlendState*				m_pBS_Disabled;
	BlendState*				m_pBS_Disabled_RedOnly;
	BlendState*				m_pBS_Disabled_GreenOnly;
	BlendState*				m_pBS_Disabled_BlueOnly;
	BlendState*				m_pBS_Disabled_AlphaOnly;
	BlendState*				m_pBS_AlphaBlend;
	BlendState*				m_pBS_PremultipliedAlpha;
	BlendState*				m_pBS_Additive;
	BlendState*				m_pBS_Max;


public:	 // PROPERTIES

	bool					IsInitialized() const		{ return m_pDeviceContext != NULL; }
	int						ComponentsCount() const;

	ID3D11Device&			DXDevice()					{ return *m_pDevice; }
	ID3D11DeviceContext&	DXContext()					{ return *m_pDeviceContext; }
	IDXGISwapChain&			DXSwapChain()				{ return *m_pSwapChain; }

	const Texture2D&		DefaultRenderTarget() const	{ return *m_pDefaultRenderTarget; }
	const Texture2D&		DefaultDepthStencil() const	{ return *m_pDefaultDepthStencil; }

	Shader*				CurrentMaterial()			{ return m_pCurrentMaterial; }


public:	 // METHODS

	Device();
//	~Device();	// Don't declare a destructor since the Device exists as a static singleton instance: in release mode, this implies calling some annoying atexit() function that will yield a link error!
				// Simply don't forget to call Exit() at the end of your program and that should do the trick...

	bool	Init( HWND _Handle, bool _Fullscreen, bool _sRGB );
	bool	Init( U32 _Width, U32 _Height, HWND _Handle, bool _Fullscreen, bool _sRGB );
	void	Exit();

	// Helpers
	void	ClearRenderTarget( const Texture2D& _Target, const float4& _Color );
	void	ClearRenderTarget( const Texture3D& _Target, const float4& _Color );
	void	ClearRenderTarget( ID3D11RenderTargetView& _TargetView, const float4& _Color );
	void	ClearDepthStencil( const Texture2D& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth=true, bool _bClearStencil=true );
	void	ClearDepthStencil( ID3D11DepthStencilView& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth=true, bool _bClearStencil=true );
	void	SetRenderTarget( const Texture2D& _Target, const Texture2D* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetRenderTarget( const Texture3D& _Target, const Texture2D* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetRenderTarget( int _Width, int _Height, const ID3D11RenderTargetView& _Target, ID3D11DepthStencilView* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetRenderTargets( int _Width, int _Height, int _TargetsCount, ID3D11RenderTargetView* const * _ppTargets, ID3D11DepthStencilView* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetStates( RasterizerState* _pRasterizerState, DepthStencilState* _pDepthStencilState, BlendState* _pBlendState );
	void	SetStatesReferences( const float4& _BlendMasks, U32 _BlendSampleMask, U8 _StencilRef );
	void	SetScissorRect( const D3D11_RECT* _pScissor=NULL );

	// Clears the shader resource registers
	// Useful to cleanup textures that may otherwise be considered as required by shaders that don't really need them.
	// Helps to clear up resource contention for draw calls
	void	RemoveShaderResources( int _SlotIndex, int _SlotsCount=1, U32 _ShaderStages=SSF_ALL );

	void	RemoveRenderTargets();
	void	RemoveUAVs();

private:

	void	RegisterComponent( Component& _Component );
	void	UnRegisterComponent( Component& _Component );

public:
	static bool	Check( HRESULT _Result );

	friend class Component;
	friend class Shader;
};

