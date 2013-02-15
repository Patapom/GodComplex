#ifdef _DEBUG

#include "../GodComplex.h"

Video::Video( Device& _Device, HWND _hWnd )
	: m_RefCount				( 1 )
//	, m_Device					( _Device )
	, m_pGraphBuilder			( NULL )
	, m_pMediaControl			( NULL )
	, m_pVMR9					( NULL )
	, m_pCaptureGraphBuilder2	( NULL )
{
	HRESULT	hr = CoInitializeEx( NULL, COINIT_MULTITHREADED );
	ASSERT( SUCCEEDED(hr), "Failed to initialize COM for video recording!" );

	// Create a D3D9 device!
	ASSERT( SUCCEEDED( Direct3DCreate9Ex( D3D_SDK_VERSION, &m_pD3D ) ), "Failed to create D3D9 Device!" );

	D3DPRESENT_PARAMETERS	Params;
	Params.BackBufferFormat = D3DFMT_UNKNOWN;// D3DFMT_A8R8G8B8;
	Params.BackBufferCount = 0;
	Params.BackBufferWidth = 0;
	Params.BackBufferHeight = 0;
	Params.EnableAutoDepthStencil = false;
	Params.AutoDepthStencilFormat = D3DFMT_UNKNOWN;// D3DFMT_D24S8;
	Params.MultiSampleType = D3DMULTISAMPLE_NONE;
	Params.MultiSampleQuality = 0;
	Params.SwapEffect = D3DSWAPEFFECT_DISCARD;
	Params.PresentationInterval = D3DPRESENT_INTERVAL_IMMEDIATE;//D3DPRESENT_RATE_DEFAULT;
	Params.FullScreen_RefreshRateInHz = 0;
	Params.Windowed = true;
	Params.hDeviceWindow = _hWnd;
	Params.Flags = D3DPRESENTFLAG_DEVICECLIP | D3DPRESENTFLAG_VIDEO;
	hr = m_pD3D->CreateDeviceEx( D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, _hWnd, 0*D3DCREATE_MULTITHREADED | D3DCREATE_HARDWARE_VERTEXPROCESSING, &Params, NULL, &m_pDevice );
	ASSERT( SUCCEEDED( hr ), "Failed to create D3D9 Device!" );

//	DisplayMode _mode = _d3d.GetAdapterDisplayMode(0);
}
Video::~Video()
{
	m_pDevice->Release();
	m_pD3D->Release();

	CoUninitialize();
}

// Most code from http://www.geekpage.jp/en/programming/directshow/vmr9.php
void	Video::Init( int _DeviceIndex )
{
	HRESULT	hr;

	// Create FilterGraph
	ASSERT( SUCCEEDED( hr = CoCreateInstance( CLSID_FilterGraph, NULL, CLSCTX_INPROC, IID_IGraphBuilder, (LPVOID*) &m_pGraphBuilder) ), "Failed to create FilterGraph instance!" );

	// Retrieve media source
	m_pSourceDevice = QueryMediaSourceDevice( _DeviceIndex );
	ASSERT( SUCCEEDED( hr = m_pGraphBuilder->AddFilter( m_pSourceDevice, L"WebCam" ) ), "Failed to add webcam device as source filter!" );

	// Prepare VMR9 filter
	ASSERT( SUCCEEDED( hr = CoCreateInstance( CLSID_VideoMixingRenderer9, 0, CLSCTX_INPROC_SERVER, IID_IBaseFilter, (LPVOID*) &m_pVMR9 ) ), "Failed to create VMR9 instance!" );
	ConfigureVMR9();

	// Add VMR9 filter to Graph
	ASSERT( SUCCEEDED( hr = m_pGraphBuilder->AddFilter( m_pVMR9, L"VMR9" ) ), "Failed to add VMR9 as rendering filter!" );

	// Create GraphBuilder, CaptureGraphBuilder2
	ASSERT( SUCCEEDED( hr = CoCreateInstance( CLSID_CaptureGraphBuilder2, NULL, CLSCTX_INPROC, IID_ICaptureGraphBuilder2, (LPVOID*) &m_pCaptureGraphBuilder2 ) ), "Failed to create the CaptureGraphBuilder instance!" );

	// Set FilterGraph for CaptureGraphBuilder2
	ASSERT( SUCCEEDED( hr = m_pCaptureGraphBuilder2->SetFiltergraph( m_pGraphBuilder ) ), "Failed to set filter graph!" );

	// Build Graph
	ASSERT( SUCCEEDED( hr = m_pCaptureGraphBuilder2->RenderStream( 0, 0, m_pSourceDevice, 0, m_pVMR9 ) ), "Failed to setup render stream!" );

	// Get MediaControl Interface
	ASSERT( SUCCEEDED( hr = m_pGraphBuilder->QueryInterface( IID_IMediaControl, (LPVOID*) &m_pMediaControl ) ), "Failed to retrieve MediaControl instance!" );
}

void	Video::Play()
{
	ASSERT( m_pMediaControl != NULL, "MediaControl not initialized! Did you forget to call Init()?" );
	m_pMediaControl->Run();
}

void	Video::Pause()
{
	ASSERT( m_pMediaControl != NULL, "MediaControl not initialized! Did you forget to call Init()?" );
	m_pMediaControl->Pause();
}

void	Video::Stop()
{
	ASSERT( m_pMediaControl != NULL, "MediaControl not initialized! Did you forget to call Init()?" );
	m_pMediaControl->Stop();
}

void	Video::Exit()
{
	ReleaseVMR9();

	m_pSourceDevice->Release();
	m_pVMR9->Release();
	m_pMediaControl->Release();
	m_pCaptureGraphBuilder2->Release();
	m_pGraphBuilder->Release();
}

// Code from http://msdn.microsoft.com/en-us/library/windows/desktop/dd377566(v=vs.85).aspx
void	Video::EnumerateDevices( EnumerateDelegate _pDeviceEnumerator, void* _pUserData )
{
	IEnumMoniker*	pEnumerator = NULL;
	ASSERT( SUCCEEDED( EnumerateDevices( CLSID_VideoInputDeviceCategory, &pEnumerator ) ), "Failed enumerating devices!" );

	VARIANT	varFriendlyName, varDevicePath;
	VariantInit( &varFriendlyName );
	VariantInit( &varDevicePath );

	int			DeviceIndex = 0;
	IMoniker*	pMoniker = NULL;
	while ( pEnumerator->Next( 1, &pMoniker, NULL ) == S_OK )
	{
		IPropertyBag*	pPropBag;
		HRESULT hr = pMoniker->BindToStorage( 0, 0, IID_PPV_ARGS( &pPropBag ) );
		if ( FAILED(hr) )
		{
			pMoniker->Release();
			continue;  
		} 

		// Get friendly name.
		hr = pPropBag->Read( L"FriendlyName", &varFriendlyName, 0 );
		if ( !SUCCEEDED(hr) )
			continue;

		hr = pPropBag->Read( L"DevicePath", &varDevicePath, 0 );
		if ( !SUCCEEDED(hr) )
			continue;

		// Notify delegate
		(*_pDeviceEnumerator)( DeviceIndex++, varFriendlyName.bstrVal, varDevicePath.bstrVal, pMoniker, _pUserData );

		pPropBag->Release();
		pMoniker->Release();
	}

	pEnumerator->Release();
}

namespace
{
	struct	__QueryDeviceStruct
	{
		int				DeviceIndex;
		IBaseFilter*	pSourceDevice;
	};
	void	QueryDeviceEnumerator( int _DeviceIndex, const BSTR& _FriendlyName, const BSTR& _DevicePath, IMoniker* _pMoniker, void* _pUserData )
	{
		__QueryDeviceStruct&	Params = *((__QueryDeviceStruct*) _pUserData);
		if ( _DeviceIndex != Params.DeviceIndex )
			return;

		ASSERT( SUCCEEDED( _pMoniker->BindToObject( 0, 0, IID_IBaseFilter, (void**) &Params.pSourceDevice ) ), "Failed to query base filter!" );
	}
}
IBaseFilter*	Video::QueryMediaSourceDevice( int _DeviceIndex )
{
	__QueryDeviceStruct	Params;
	Params.DeviceIndex = _DeviceIndex;
	Params.pSourceDevice = NULL;
	EnumerateDevices( QueryDeviceEnumerator, &Params );
	ASSERT( Params.pSourceDevice != NULL, "Failed to query the proper source device!" );
	return Params.pSourceDevice;
}

HRESULT	Video::EnumerateDevices( REFGUID _Category, IEnumMoniker** _ppEnum )
{
    // Create the System Device Enumerator.
    ICreateDevEnum *pDevEnum;
    HRESULT	hr = CoCreateInstance( CLSID_SystemDeviceEnum, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS( &pDevEnum ) );
    ASSERT( SUCCEEDED(hr), "Failed to enumerate video devices!" );

	// Create an enumerator for the category.
    hr = pDevEnum->CreateClassEnumerator( _Category, _ppEnum, 0 );
    if ( hr == S_FALSE )
        hr = VFW_E_NOT_FOUND;  // The category is empty. Treat as an error.

	pDevEnum->Release();

	return hr;
}

void	Video::ConfigureVMR9()
{
	HRESULT	hr;

	IVMRFilterConfig9*	pVMRConfig;
	ASSERT( SUCCEEDED( hr = m_pVMR9->QueryInterface( IID_IVMRFilterConfig9, (LPVOID*) &pVMRConfig ) ), "Failed to retrieve VMR9 config!" );
	pVMRConfig->SetRenderingMode( VMR9Mode_Renderless );
	pVMRConfig->SetNumberOfStreams( 1 );
	pVMRConfig->Release();

	ASSERT( SUCCEEDED( hr = m_pVMR9->QueryInterface( IID_IVMRSurfaceAllocatorNotify9, (LPVOID*) &m_pVMRAllocatorNotify ) ), "Failed to retrieve VMR9 surface allocator!" );
	AdviseNotify( m_pVMRAllocatorNotify );
	ASSERT( SUCCEEDED( hr = m_pVMRAllocatorNotify->AdviseSurfaceAllocator( 0x1234, this ) ), "Failed to replace the allocator/renderer for VRM9!" );		// Use our class as interface to allocator/presenter
}

void		Video::ReleaseVMR9()
{
	m_pVMRAllocatorNotify->Release();
}

HRESULT	Video::QueryInterface( REFIID riid, void** ppvObject )
{
	if ( riid == IID_IVMRImagePresenter9 )
	{
		*ppvObject = static_cast<IVMRImagePresenter9*>( this );
		AddRef();
		return S_OK;
	}
	else if ( riid == IID_IVMRSurfaceAllocatorEx9 )
	{
		*ppvObject = static_cast<IVMRSurfaceAllocator9*>( this );
		AddRef();
		return S_OK;
	}
	else if ( riid == IID_IVMRMonitorConfig9 )
	{
		*ppvObject = static_cast<IVMRMonitorConfig9*>( this );
		AddRef();
		return S_OK;
	}
	else if ( riid == IID_IVMRImagePresenterConfig9 )
	{
		*ppvObject = static_cast<IVMRImagePresenterConfig9*>( this );
		AddRef();
		return S_OK;
	}
	else if ( riid == IID_IUnknown )
	{
		*ppvObject = static_cast<IUnknown*>( static_cast<IVMRSurfaceAllocator9*>( this ) );
		AddRef();
		return S_OK;    
	}

	return E_NOINTERFACE;
}
ULONG		Video::AddRef()
{
	return InterlockedIncrement( &m_RefCount );
}
ULONG		Video::Release()
{
	return InterlockedDecrement( &m_RefCount );
}

// IVMRSurfaceAllocator9
HRESULT	Video::InitializeDevice( DWORD_PTR dwUserID, VMR9AllocationInfo* lpAllocInfo, DWORD* lpNumBuffers )
{
	HRESULT	hr;
	ASSERT( SUCCEEDED( hr = m_pVMRAllocatorNotify->AllocateSurfaceHelper( lpAllocInfo, lpNumBuffers, &m_pSurface ) ), "Failed to allocate surface for rendering!" );

	return S_OK;
}
HRESULT	Video::TerminateDevice( DWORD_PTR dwID )
{
	if ( m_pSurface != NULL )
		m_pSurface->Release();
	m_pSurface = NULL;
	return S_OK;
}
HRESULT	Video::GetSurface( DWORD_PTR dwUserID, DWORD SurfaceIndex, DWORD SurfaceFlags, IDirect3DSurface9** lplpSurface )
{
	*lplpSurface = m_pSurface;
	return S_OK;
}
HRESULT	Video::AdviseNotify( IVMRSurfaceAllocatorNotify9* lpIVMRSurfAllocNotify )
{
	HRESULT		hr;
	HMONITOR	hMonitor = m_pD3D->GetAdapterMonitor( D3DADAPTER_DEFAULT );
	ASSERT( SUCCEEDED( hr = m_pVMRAllocatorNotify->SetD3DDevice( m_pDevice, hMonitor ) ), "Failed to assign D3D Device to VRM9!" );

	return S_OK;
}

// IVMRImagePresenter9
HRESULT	Video::StartPresenting( DWORD_PTR dwUserID )
{
	return S_OK;
}
HRESULT	Video::StopPresenting( DWORD_PTR dwUserID )
{
	return S_OK;
}
HRESULT	Video::PresentImage( DWORD_PTR dwUserID, VMR9PresentationInfo* lpPresInfo )
{
	return S_OK;
}

// IVMRMonitorConfig9
HRESULT	Video::SetMonitor( UINT uDev )
{
	return S_OK;
}
HRESULT	Video::GetMonitor( UINT* puDev )
{
	return S_OK;
}
HRESULT	Video::SetDefaultMonitor( UINT uDev )
{
	return S_OK;
}
HRESULT	Video::GetDefaultMonitor( UINT* puDev )
{
	return S_OK;
}
HRESULT	Video::GetAvailableMonitors( VMR9MonitorInfo* pInfo, DWORD dwMaxInfoArraySize, DWORD* pdwNumDevices )
{
	return S_OK;
}

// IVMRImagePresenterConfig9
HRESULT Video::SetRenderingPrefs( DWORD dwRenderFlags )
{
	return S_OK;
}
HRESULT Video::GetRenderingPrefs( DWORD* dwRenderFlags )
{
	return S_OK;
}

#endif