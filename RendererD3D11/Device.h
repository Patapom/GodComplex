#pragma once
#include "Renderer.h"

class Component;
class Shader;
class Texture2D;
class Texture3D;
class RasterizerState;
class DepthStencilState;
class BlendState;

class Device {
	static const int	SAMPLERS_COUNT = 8;

public:		// NESTED TYPES

	enum	SHADER_STAGE_FLAGS {
		SSF_VERTEX_SHADER		= (1 << 0),
		SSF_HULL_SHADER			= (1 << 1),
		SSF_DOMAIN_SHADER		= (1 << 2),
		SSF_GEOMETRY_SHADER		= (1 << 3),
		SSF_PIXEL_SHADER		= (1 << 4),
		SSF_COMPUTE_SHADER		= (1 << 5),
		SSF_COMPUTE_SHADER_UAV	= (1 << 6),		// WARNING: SSF_ALL doesn't include UAVs!

		SSF_ALL					= (1 << 6)-1	// WARNING: SSF_ALL doesn't include UAVs!
	};

	class DoubleBufferedQuery {
	public:
		static U32	MAX_FRAMES;
		U32							m_markerID;
		U32							m_nextMarkerID;
		ID3D11Query*				m_queries[16];	// Actual queries count is defined by MAX_FRAME (maybe altered at runtime depending on debug state)
		U64							m_timeStamp;

		static U32					ms_currentFrameQueryIndex;
		DoubleBufferedQuery( Device& _owner, U32 _markerID, D3D11_QUERY _queryType );
		~DoubleBufferedQuery();
		operator ID3D11Query*() const	{ return m_queries[ms_currentFrameQueryIndex]; }
		U64			GetTimeStamp( Device& _owner );
	};

private:	// FIELDS

	ID3D11Device*			m_device;
	ID3D11DeviceContext*	m_deviceContext;
	IDXGISwapChain*			m_swapChain;

	Texture2D*				m_pDefaultRenderTarget;	// The back buffer to render to the screen
	Texture2D*				m_pDefaultDepthStencil;	// The default depth stencil

	ID3D11SamplerState*		m_ppSamplers[SAMPLERS_COUNT];

	Component*				m_componentsStackTop;	// Remember this is the stack TOP so access the components using their m_pNext pointer to reach back to the bottom

	Shader*					m_pCurrentMaterial;		// The currently used material
	RasterizerState*		m_pCurrentRasterizerState;
	DepthStencilState*		m_pCurrentDepthStencilState;
	BlendState*				m_pCurrentBlendState;

	int						m_statesCount;

	// Default blend & stencil refs
	bfloat4					m_blendFactors;
	U32						m_blendMasks;
	U8						m_stencilRef;

	// Performance queries
	DoubleBufferedQuery*	m_queryDisjoint;
	DoubleBufferedQuery*	m_queryFrameBegin;
	DoubleBufferedQuery*	m_queryFrameEnd;
	BaseLib::Dictionary<DoubleBufferedQuery*>	m_performanceQueries;
	DoubleBufferedQuery*	m_lastQuery;	// The last querried query :D
	U64						m_queryClockFrequency;
	U64						m_framesCount;

public:

	RasterizerState*		m_pRS_CullNone;
	RasterizerState*		m_pRS_CullBack;
	RasterizerState*		m_pRS_CullFront;
	RasterizerState*		m_pRS_WireFrame;

	DepthStencilState*		m_pDS_Disabled;
	DepthStencilState*		m_pDS_ReadWriteLess;
	DepthStencilState*		m_pDS_ReadWriteGreater;
	DepthStencilState*		m_pDS_WriteAlways;
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

	bool					IsInitialized() const		{ return m_deviceContext != NULL; }
	int						ComponentsCount() const;

	ID3D11Device&			DXDevice()					{ return *m_device; }
	ID3D11DeviceContext&	DXContext()					{ return *m_deviceContext; }
	IDXGISwapChain&			DXSwapChain()				{ return *m_swapChain; }

	const Texture2D&		DefaultRenderTarget() const	{ return *m_pDefaultRenderTarget; }
	const Texture2D&		DefaultDepthStencil() const	{ return *m_pDefaultDepthStencil; }

	Shader*					CurrentMaterial()			{ return m_pCurrentMaterial; }


public:	 // METHODS

	Device();
//	~Device();	// Don't declare a destructor since the Device exists as a static singleton instance: in release mode, this implies calling some annoying atexit() function that will yield a link error!
				// Simply don't forget to call Exit() at the end of your program and that should do the trick...

	bool	Init( HWND _Handle, bool _Fullscreen, bool _sRGB );
	bool	Init( U32 _Width, U32 _Height, HWND _Handle, bool _Fullscreen, bool _sRGB );
	void	Exit();

	// Helpers
	void	ClearRenderTarget( const Texture2D& _Target, const bfloat4& _Color );
	void	ClearRenderTarget( const Texture3D& _Target, const bfloat4& _Color );
	void	ClearRenderTarget( ID3D11RenderTargetView& _TargetView, const bfloat4& _Color );
	void	ClearDepthStencil( const Texture2D& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth=true, bool _bClearStencil=true );
	void	ClearDepthStencil( ID3D11DepthStencilView& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth=true, bool _bClearStencil=true );
	void	SetRenderTarget( const Texture2D& _Target, const Texture2D* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetRenderTarget( const Texture3D& _Target, const Texture2D* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetRenderTarget( U32 _Width, U32 _Height, const ID3D11RenderTargetView& _Target, ID3D11DepthStencilView* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetRenderTargets( U32 _Width, U32 _Height, U32 _TargetsCount, ID3D11RenderTargetView* const * _ppTargets, ID3D11DepthStencilView* _pDepthStencil=NULL, const D3D11_VIEWPORT* _pViewport=NULL );
	void	SetStates( RasterizerState* _pRasterizerState, DepthStencilState* _pDepthStencilState, BlendState* _pBlendState );
	void	SetStatesReferences( const bfloat4& _BlendMasks, U32 _BlendSampleMask, U8 _StencilRef );
	void	SetScissorRect( const D3D11_RECT* _pScissor=NULL );

	// Performance queries
	// Inspired by: http://www.reedbeta.com/blog/gpu-profiling-101/
	void	PerfBeginFrame();				// Must be called at the beginning of the frame
	void	PerfSetMarker( U32 _markerID );	// Must be called at the beginning of your task
	void	PerfEndFrame();					// Must be called AFTER Present()
	// Returns the time (in milliseconds) elapsed between start and end markers (by default, if no end marker is specified then it will use the marker immediately after the start marker, or the end of frame if none exists)
	double	PerfGetMilliSeconds( U32 _markerIDStart, U32 _markerIDEnd=~0U );

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

