#ifdef _DEBUG

//////////////////////////////////////////////////////////////////////////
// Live Video Capture from a webcam
// Interesting code grabbed from http://www.geekpage.jp/en/programming/directshow/
// Cool thread http://xboxforums.create.msdn.com/forums/t/102261.aspx
// Blog on implementation for DX9: http://blog.yezhucn.com/dnwmt/vmr_d3d.htm
// Supplying a custom allocator/presenter: http://msdn.microsoft.com/en-us/library/windows/desktop/dd407172(v=vs.85).aspx
// Implementation of the allocator/presenter from: http://msdn.microsoft.com/en-us/subscriptions/ms787744(v=vs.85).aspx
// Sharing DX9/DXGI surfaces: http://msdn.microsoft.com/en-us/library/ee913554.aspx
//
#pragma once

#include <DShow.h>
#include <d3d9.h>
#include <Vmr9.h>

class Device;
class ConstantBuffer;
template<typename> class CB;

class	Video : public IVMRImagePresenter9, IVMRSurfaceAllocator9, IVMRMonitorConfig9, IVMRImagePresenterConfig9
{
public:		// NESTED TYPES

	typedef void	EnumerateDelegate( int _DeviceIndex, const BSTR& _FriendlyName, const BSTR& _DevicePath, IMoniker* _pMoniker, void* _pUserData );

private:	// FIELDS

//	Device&					m_Device;
//	CCritSec				m_ObjectLock;
    long					m_RefCount;

	IDirect3D9Ex*			m_pD3D;
	IDirect3DDevice9Ex*		m_pDevice;

	IGraphBuilder*			m_pGraphBuilder;
	IBaseFilter*			m_pSourceDevice;
	IMediaControl*			m_pMediaControl;
	IBaseFilter*			m_pVMR9;
	ICaptureGraphBuilder2*	m_pCaptureGraphBuilder2;

	// VMR9 Configuration
	IVMRSurfaceAllocatorNotify9*	m_pVMRAllocatorNotify;
	IDirect3DSurface9*				m_pSurface;

public:		// PROPERTIES
 
public:		// METHODS

	Video( Device& _Device, HWND _hWnd );
	~Video();

	void			Init( int _DeviceIndex );
	void			Play();
	void			Pause();
	void			Stop();
	void			Exit();

	void			EnumerateDevices( EnumerateDelegate _pDeviceEnumerator, void* _pUserData );

public:	// IUnknown

	virtual HRESULT STDMETHODCALLTYPE	QueryInterface( REFIID riid, __RPC__deref_out void __RPC_FAR *__RPC_FAR* ppvObject );
	virtual ULONG STDMETHODCALLTYPE		AddRef();
	virtual ULONG STDMETHODCALLTYPE		Release();

public:	// IVMRSurfaceAllocator9
	virtual HRESULT STDMETHODCALLTYPE	InitializeDevice( DWORD_PTR dwUserID, VMR9AllocationInfo* lpAllocInfo, DWORD* lpNumBuffers);
	virtual HRESULT STDMETHODCALLTYPE	TerminateDevice( DWORD_PTR dwID );
	virtual HRESULT STDMETHODCALLTYPE	GetSurface( DWORD_PTR dwUserID, DWORD SurfaceIndex, DWORD SurfaceFlags, IDirect3DSurface9** lplpSurface );
	virtual HRESULT STDMETHODCALLTYPE	AdviseNotify( IVMRSurfaceAllocatorNotify9* lpIVMRSurfAllocNotify );
  
public:	// IVMRImagePresenter9
	virtual HRESULT STDMETHODCALLTYPE	StartPresenting( DWORD_PTR dwUserID );
	virtual HRESULT STDMETHODCALLTYPE	StopPresenting( DWORD_PTR dwUserID );
	virtual HRESULT STDMETHODCALLTYPE	PresentImage( DWORD_PTR dwUserID, VMR9PresentationInfo* lpPresInfo );

public:	// IVMRMonitorConfig9
	virtual HRESULT STDMETHODCALLTYPE	SetMonitor( UINT uDev );
	virtual HRESULT STDMETHODCALLTYPE	GetMonitor( UINT* puDev );
	virtual HRESULT STDMETHODCALLTYPE	SetDefaultMonitor( UINT uDev );
	virtual HRESULT STDMETHODCALLTYPE	GetDefaultMonitor( UINT* puDev );
	virtual HRESULT STDMETHODCALLTYPE	GetAvailableMonitors( VMR9MonitorInfo* pInfo, DWORD dwMaxInfoArraySize, DWORD* pdwNumDevices );

public:	// IVMRImagePresenterConfig9
	virtual HRESULT STDMETHODCALLTYPE	SetRenderingPrefs( DWORD dwRenderFlags );
	virtual HRESULT STDMETHODCALLTYPE	GetRenderingPrefs( DWORD* dwRenderFlags );

private:
	IBaseFilter*	QueryMediaSourceDevice( int _DeviceIndex );
	HRESULT			EnumerateDevices( REFGUID _Category, IEnumMoniker** _ppEnum );
	void			ConfigureVMR9();
	void			ReleaseVMR9();
};

#endif