#pragma once
#include "Renderer.h"

class Component;

class Device
{
private:	// FIELDS

	ID3D11Device*			m_pDevice;
	ID3D11DeviceContext*	m_pDeviceContext;

	IDXGISwapChain*			m_pSwapChain;

	Component*				m_pComponentsStack;


public:	 // PROPERTIES

	ID3D11Device*			DXDevice()  { return m_pDevice; }
	ID3D11DeviceContext*	DXContext() { return m_pDeviceContext; }


public:	 // METHODS

	Device();
	~Device();

	void	Init( int _Width, int _Height, HWND _Handle, bool _Fullscreen, bool _sRGB );
	void	Exit();

private:

	void	RegisterComponent( Component& _Component );
	void	UnRegisterComponent( Component& _Component );
	void	Check( HRESULT _Result );

	friend class Component;
};

