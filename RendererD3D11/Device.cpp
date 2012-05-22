#include "Device.h"
#include "Components/Component.h"

Device::Device() : m_pDevice( NULL ), m_pDeviceContext( NULL ), m_pComponentsStack( NULL )
{
}

Device::~Device()
{
	Exit();
}

void	Device::Init( int _Width, int _Height, HWND _Handle, bool _Fullscreen, bool _sRGB )
{
 	DXGI_RATIONAL   RefreshRate;
 	RefreshRate.Numerator = 60;
 	RefreshRate.Denominator = 1;
 
	// Simple output buffer
	DXGI_MODE_DESC ModeDesc;
	ModeDesc.Width = _Width;
	ModeDesc.Height = _Height;
	ModeDesc.Scaling = DXGI_MODE_SCALING_STRETCHED;
	ModeDesc.RefreshRate = RefreshRate;
	ModeDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	ModeDesc.Format = _sRGB ? DXGI_FORMAT_R8G8B8A8_UNORM_SRGB : DXGI_FORMAT_R8G8B8A8_UNORM;

	// No multisampling
	DXGI_SAMPLE_DESC	SampleDesc;
	SampleDesc.Count = 1;
	SampleDesc.Quality = 0;

	// Create a swap chain with 1 back buffer
	DXGI_SWAP_CHAIN_DESC	SwapChainDesc;
	SwapChainDesc.BufferDesc = ModeDesc;
	SwapChainDesc.SampleDesc = SampleDesc;
	SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	SwapChainDesc.BufferCount = 1;
	SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	SwapChainDesc.OutputWindow = _Handle;
	SwapChainDesc.Windowed = !_Fullscreen;
	SwapChainDesc.Flags = 0;

	D3D_FEATURE_LEVEL	   pFeatureLevels[] = { D3D_FEATURE_LEVEL_11_0, D3D_FEATURE_LEVEL_10_1, D3D_FEATURE_LEVEL_10_0 };
	D3D_FEATURE_LEVEL	   ObtainedFeatureLevel;

 	Check( D3D11CreateDeviceAndSwapChain( NULL, D3D_DRIVER_TYPE_HARDWARE, NULL,
#ifndef _DEBUG
		D3D11_CREATE_DEVICE_SINGLETHREADED,
#else
		D3D11_CREATE_DEVICE_SINGLETHREADED | D3D11_CREATE_DEVICE_DEBUG,
#endif
		pFeatureLevels, 1,  // Support D3D11 only...
		D3D11_SDK_VERSION,
		&SwapChainDesc, &m_pSwapChain,
		&m_pDevice, &ObtainedFeatureLevel, &m_pDeviceContext ) );
}

void	Device::Exit()
{
	if ( m_pDevice == NULL )
		return; // Already released !

	// Dispose of all the registered components in order
	while ( m_pComponentsStack != NULL )
		delete m_pComponentsStack;  // DIE !!

	m_pDevice->Release(); m_pDevice = NULL; m_pDeviceContext = NULL;
}

void	Device::RegisterComponent( Component& _Component )
{
	// Attach to the end of the list
	if ( m_pComponentsStack != NULL )
		m_pComponentsStack->m_pNext = &_Component;
	_Component.m_pPrevious = m_pComponentsStack;

	m_pComponentsStack = &_Component;
}

void	Device::UnRegisterComponent( Component& _Component )
{
	// Link over
	if ( _Component.m_pPrevious != NULL )
		_Component.m_pPrevious->m_pNext = _Component.m_pNext;
	if ( _Component.m_pNext != NULL )
		_Component.m_pNext->m_pPrevious = _Component.m_pPrevious;
	else
		m_pComponentsStack = _Component.m_pPrevious;	// We were the top of the stack !
}

void	Device::Check( HRESULT _Result )
{
	ASSERT( _Result == S_OK );
}

