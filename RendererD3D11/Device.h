#pragma once
#include "Renderer.h"

//#define DIRECTX11	// Define this to use DX11, otherwise DX10 will be used

class Component;
class Material;
class Texture2D;
class RasterizerState;
class DepthStencilState;
class BlendState;

class Device
{
	static const int	SAMPLERS_COUNT = 6;

private:	// FIELDS

	ID3D11Device*			m_pDevice;
	ID3D11DeviceContext*	m_pDeviceContext;
	IDXGISwapChain*			m_pSwapChain;

	Texture2D*				m_pDefaultRenderTarget;	// The back buffer to render to the screen
	Texture2D*				m_pDefaultDepthStencil;	// The default depth stencil

	ID3D11SamplerState*		m_ppSamplers[SAMPLERS_COUNT];

	Component*				m_pComponentsStackTop;	// Remember this is the stack TOP so access the components using their m_pNext pointer to reach back to the bottom

	Material*				m_pCurrentMaterial;		// The currently used material
	RasterizerState*		m_pCurrentRasterizerState;
	DepthStencilState*		m_pCurrentDepthStencilState;
	BlendState*				m_pCurrentBlendState;

	int						m_StatesCount;

public:

	RasterizerState*		m_pRS_CullNone;
	RasterizerState*		m_pRS_CullBack;
	RasterizerState*		m_pRS_CullFront;

	DepthStencilState*		m_pDS_Disabled;
	DepthStencilState*		m_pDS_ReadWriteLess;
	DepthStencilState*		m_pDS_ReadWriteGreater;

	BlendState*				m_pBS_Disabled;
	BlendState*				m_pBS_Disabled_RedOnly;
	BlendState*				m_pBS_Disabled_GreenOnly;
	BlendState*				m_pBS_Disabled_BlueOnly;
	BlendState*				m_pBS_Disabled_AlphaOnly;
	BlendState*				m_pBS_AlphaBlend;
	BlendState*				m_pBS_PremultipliedAlpha;


public:	 // PROPERTIES

	bool					IsInitialized() const		{ return m_pDeviceContext != NULL; }
	int						ComponentsCount() const;

	ID3D11Device&			DXDevice()					{ return *m_pDevice; }
	ID3D11DeviceContext&	DXContext()					{ return *m_pDeviceContext; }
	IDXGISwapChain&			DXSwapChain()				{ return *m_pSwapChain; }

	const Texture2D&		DefaultRenderTarget() const	{ return *m_pDefaultRenderTarget; }
	const Texture2D&		DefaultDepthStencil() const	{ return *m_pDefaultDepthStencil; }

	Material*				CurrentMaterial()			{ return m_pCurrentMaterial; }


public:	 // METHODS

	Device();
//	~Device();	// Don't declare a destructor since the Device exists as a static singleton instance : in release mode, this implies calling some annoying atexit() function that will yield a link error !
				// Simply don't forget to call Exit() at the end of your program and that should do the trick...

	void	Init( int _Width, int _Height, HWND _Handle, bool _Fullscreen, bool _sRGB );
	void	Exit();

	// Helpers
	void	ClearRenderTarget( const Texture2D& _Target, const NjFloat4& _Color );
	void	ClearDepthStencil( const Texture2D& _DepthStencil, float _Z, U8 _Stencil );
	void	SetRenderTarget( const Texture2D& _Target, const Texture2D* _pDepthStencil=NULL, D3D11_VIEWPORT* _pViewport=NULL );
	void	SetRenderTargets( int _Width, int _Height, int _TargetsCount, ID3D11RenderTargetView** _ppTargets, ID3D11DepthStencilView* _pDepthStencil=NULL, D3D11_VIEWPORT* _pViewport=NULL );
	void	RemoveRenderTargets();
	void	SetStates( RasterizerState* _pRasterizerState, DepthStencilState* _pDepthStencilState, BlendState* _pBlendState );

private:

	void	RegisterComponent( Component& _Component );
	void	UnRegisterComponent( Component& _Component );
	void	Check( HRESULT _Result );

	friend class Component;
};

