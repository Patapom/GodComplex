#pragma once
#include "Renderer.h"

class Component;
class Texture2D;

class Device
{
private:	// FIELDS

	ID3D11Device*			m_pDevice;
	ID3D11DeviceContext*	m_pDeviceContext;

	IDXGISwapChain*			m_pSwapChain;

	Texture2D*				m_pDefaultRenderTarget;	// The back buffer to render to the screen
	Texture2D*				m_pDefaultDepthStencil;	// The default depth stencil

	Component*				m_pComponentsStack;


public:	 // PROPERTIES

	bool					IsInitialized() const	{ return m_pDeviceContext != NULL; }
	int						ComponentsCount() const;

	ID3D11Device*			DXDevice()  { return m_pDevice; }
	ID3D11DeviceContext*	DXContext() { return m_pDeviceContext; }

	const Texture2D&		DefaultRenderTarget() const	{ return *m_pDefaultRenderTarget; }
	const Texture2D&		DefaultDepthStencil() const	{ return *m_pDefaultDepthStencil; }


public:	 // METHODS

	Device();
	~Device();

	void	Init( int _Width, int _Height, HWND _Handle, bool _Fullscreen, bool _sRGB );
	void	Exit();

	// Helpers
	void	SetRenderTarget( const Texture2D& _Target, Texture2D* _pDepthStencil=NULL );

private:

	void	RegisterComponent( Component& _Component );
	void	UnRegisterComponent( Component& _Component );
	void	Check( HRESULT _Result );

	friend class Component;
};

