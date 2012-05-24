#include "Device.h"
#include "Components/Component.h"
#include "Components/Texture2D.h"

Device::Device() : m_pDevice( NULL ), m_pDeviceContext( NULL ), m_pComponentsStack( NULL )
{
}

Device::~Device()
{
	Exit();
}

int		Device::ComponentsCount() const
{
	int			Count = -2;	// Start without counting for our internal back buffer & depth stencil components
	Component*	pCurrent = m_pComponentsStack;
	while ( pCurrent != NULL )
	{
		Count++;
		pCurrent = pCurrent->m_pNext;
	}

	return Count;
}

void	Device::Init( int _Width, int _Height, HWND _Handle, bool _Fullscreen, bool _sRGB )
{
	// Create a swap chain with 1 back buffer
	DXGI_SWAP_CHAIN_DESC	SwapChainDesc;

	// Simple output buffer
	SwapChainDesc.BufferDesc.Width = _Width;
	SwapChainDesc.BufferDesc.Height = _Height;
	SwapChainDesc.BufferDesc.Format = _sRGB ? DXGI_FORMAT_R8G8B8A8_UNORM_SRGB : DXGI_FORMAT_R8G8B8A8_UNORM;
//	SwapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_STRETCHED;
	SwapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_CENTERED;
	SwapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
	SwapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
	SwapChainDesc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	SwapChainDesc.BufferUsage = DXGI_USAGE_BACK_BUFFER | DXGI_USAGE_RENDER_TARGET_OUTPUT;
//	SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT | DXGI_USAGE_UNORDERED_ACCESS;
	SwapChainDesc.BufferCount = 1;

	// No multisampling
	SwapChainDesc.SampleDesc.Count = 1;
	SwapChainDesc.SampleDesc.Quality = 0;

	SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	SwapChainDesc.OutputWindow = _Handle;
	SwapChainDesc.Windowed = !_Fullscreen;
	SwapChainDesc.Flags = 0;

//	D3D_FEATURE_LEVEL	pFeatureLevels[] = { D3D_FEATURE_LEVEL_11_0 };	// Support D3D11 only...
	D3D_FEATURE_LEVEL	pFeatureLevels[] = { D3D_FEATURE_LEVEL_10_0 };	// Support D3D10 only...
	D3D_FEATURE_LEVEL	ObtainedFeatureLevel;

 	Check
	(
		D3D11CreateDeviceAndSwapChain( NULL, D3D_DRIVER_TYPE_HARDWARE, NULL,
#ifdef _DEBUG
			D3D11_CREATE_DEVICE_DEBUG,
#else
			0,
#endif
			pFeatureLevels, 1,
			D3D11_SDK_VERSION,
			&SwapChainDesc, &m_pSwapChain,
			&m_pDevice, &ObtainedFeatureLevel, &m_pDeviceContext )
	);

	// Store the default render target
	ID3D11Texture2D*	pDefaultRenderSurface;
	m_pSwapChain->GetBuffer( 0, IID_ID3D11Texture2D, (void**) &pDefaultRenderSurface );
	ASSERT( pDefaultRenderSurface != NULL, "Failed to retrieve default render surface !" );
	m_pDefaultRenderTarget = new Texture2D( *this, *pDefaultRenderSurface, PixelFormatRGBA8::DESCRIPTOR );

	// Create the default depth stencil buffer
	m_pDefaultDepthStencil = new Texture2D( *this, _Width, _Height, DepthStencilFormatD32F::DESCRIPTOR );
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

void	Device::SetRenderTarget( const Texture2D& _Target, Texture2D* _pDepthStencil )
{
	ID3D11RenderTargetView*	pTargetView = _Target.GetTargetView( 0, 0, 0 );
	ID3D11DepthStencilView*	pDepthStencilView = _pDepthStencil != NULL ? _pDepthStencil->GetDepthStencilView() : NULL;

	m_pDeviceContext->OMSetRenderTargets( 1, &pTargetView, pDepthStencilView );
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
	if ( _Result != S_OK )
		PostQuitMessage( _Result );
//	ASSERT( _Result == S_OK );
}

