#include "stdafx.h"

#include "Device.h"
#include "Components/Component.h"
#include "Components/Texture2D.h"
#include "Components/Texture3D.h"
#include "Components/StructuredBuffer.h"
#include "Components/States.h"

Device::Device()
	: m_device( NULL )
	, m_deviceContext( NULL )
	, m_swapChain( NULL )
	, m_componentsStackTop( NULL )
	, m_pCurrentMaterial( NULL )
	, m_pCurrentRasterizerState( NULL )
	, m_pCurrentDepthStencilState( NULL )
	, m_pCurrentBlendState( NULL )
	, m_blendFactors( 1, 1, 1, 1 )
	, m_blendMasks( ~0 )
	, m_stencilRef( 0 )
	, m_framesCount( 0 )
	, m_queryDisjoint( NULL )
	, m_queryFrameBegin( NULL )
	, m_queryFrameEnd( NULL )
	, m_lastQuery( NULL ) {
}

int		Device::ComponentsCount() const {
	int			count = -2 - m_statesCount;	// Start without counting for our internal back buffer & depth stencil components
	Component*	current = m_componentsStackTop;
	while ( current != NULL ) {
		count++;
		current = current->m_previous;
	}

	return count;
}

bool	Device::Init( HWND _Handle, bool _Fullscreen, bool _sRGB ) {
	RECT	rect = { 0, 0, 0, 0 };
	if ( !GetWindowRect( _Handle, &rect ) )
		throw "Failed to retrieve window dimensions to initialize device!";
	
	int	width = rect.right - rect.left;
	int	height = rect.bottom - rect.top;

	return Init( width, height, _Handle, _Fullscreen, _sRGB );
}

bool	Device::Init( U32 _width, U32 _height, HWND _handle, bool _fullscreen, bool _sRGB ) {
	// Create a swap chain with 2 back buffers
	DXGI_SWAP_CHAIN_DESC	swapChainDesc;

	// Simple output buffer
	swapChainDesc.BufferDesc.Width = _width;
	swapChainDesc.BufferDesc.Height = _height;
	swapChainDesc.BufferDesc.Format = _sRGB ? DXGI_FORMAT_R8G8B8A8_UNORM_SRGB : DXGI_FORMAT_R8G8B8A8_UNORM;
//	SwapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_STRETCHED;
	swapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_CENTERED;
	swapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
	swapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
	swapChainDesc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	swapChainDesc.BufferUsage = DXGI_USAGE_BACK_BUFFER | DXGI_USAGE_RENDER_TARGET_OUTPUT | DXGI_USAGE_SHADER_INPUT;
//	SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT | DXGI_USAGE_UNORDERED_ACCESS;
	swapChainDesc.BufferCount = 2;

	// No multisampling
	swapChainDesc.SampleDesc.Count = 1;
	swapChainDesc.SampleDesc.Quality = 0;

	swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	swapChainDesc.OutputWindow = _handle;
	swapChainDesc.Windowed = !_fullscreen;
	swapChainDesc.Flags = 0;

	int	featureLevelsCount = 2;
	D3D_FEATURE_LEVEL	FeatureLevels[] = { D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL_11_0 };		// Support D3D11...
	D3D_FEATURE_LEVEL	obtainedFeatureLevel;

	#if defined(_DEBUG) && !defined(NSIGHT)
		UINT	debugFlags = D3D11_CREATE_DEVICE_DEBUG;
		DoubleBufferedQuery::MAX_FRAMES = 8;	// CAUTION!!! Up to 8 frames latency to query results with DEBUG layer active!
	#else
		UINT	debugFlags = 0;
		DoubleBufferedQuery::MAX_FRAMES = 8;	// CAUTION!!! Up to 8 frames latency to query results with DEBUG layer active!
	#endif

 	if ( !Check(
		D3D11CreateDeviceAndSwapChain( NULL, D3D_DRIVER_TYPE_HARDWARE, NULL,
			debugFlags,
			FeatureLevels, featureLevelsCount,
			D3D11_SDK_VERSION,
			&swapChainDesc, &m_swapChain,
			&m_device, &obtainedFeatureLevel, &m_deviceContext ) )
		)
		return false;

	// Store the default render target
	ID3D11Texture2D*	pDefaultRenderSurface;
	m_swapChain->GetBuffer( 0, __uuidof( ID3D11Texture2D ), (void**) &pDefaultRenderSurface );
	ASSERT( pDefaultRenderSurface != NULL, "Failed to retrieve default render surface!" );

	m_pDefaultRenderTarget = new Texture2D( *this, *pDefaultRenderSurface );

	// Create the default depth stencil buffer
	m_pDefaultDepthStencil = new Texture2D( *this, _width, _height, 1, 1, BaseLib::PIXEL_FORMAT::R32F, BaseLib::DEPTH_COMPONENT_FORMAT::DEPTH_ONLY );


	//////////////////////////////////////////////////////////////////////////
	// Enumerate adapters & outputs
	IDXGIFactory*	DXGIFactory;
	if ( !Check( CreateDXGIFactory(__uuidof(IDXGIFactory), (void**) &DXGIFactory ) ) ) {
		return false;
	}
	U32				adaptersCount = 0;
	IDXGIAdapter*	adapter = NULL;
	while ( DXGIFactory->EnumAdapters( adaptersCount, &adapter ) == S_OK ) {
		if ( adapter == NULL ) {
			break;
		}
		adaptersCount++;
	}
	m_adapterOutputs.SetCount( adaptersCount );

	for ( U32 adapterIndex=0; adapterIndex < adaptersCount; adapterIndex++ ) {
		DXGIFactory->EnumAdapters( adapterIndex, &adapter );
		BaseLib::List< AdapterOutput >&	adapterOutputs = m_adapterOutputs[adapterIndex];

		U32				outputIndex = 0;
		IDXGIOutput*	output = NULL;
		while ( adapter->EnumOutputs( outputIndex++, &output ) == S_OK ) {
			if ( output == NULL ) {
				break;
			}

			DXGI_OUTPUT_DESC	outputDesc;
			output->GetDesc( &outputDesc );

			AdapterOutput&	adapterOutput = adapterOutputs.Append();
			adapterOutput.m_output = output;
			adapterOutput.m_adapterIndex = adapterIndex;
			adapterOutput.m_outputIndex = outputIndex-1;
			adapterOutput.m_outputMonitor = outputDesc.Monitor;
			adapterOutput.m_outputRotation = outputDesc.Rotation;
			adapterOutput.m_outputRectangle = outputDesc.DesktopCoordinates;
		}
	}

	DXGIFactory->Release();
 
	//////////////////////////////////////////////////////////////////////////
	// Create default render states
	m_statesCount = 0;
	{
		D3D11_RASTERIZER_DESC	Desc;
		memset( &Desc, 0, sizeof(Desc) );
		Desc.FillMode = D3D11_FILL_SOLID;
        Desc.CullMode = D3D11_CULL_NONE;
        Desc.FrontCounterClockwise = TRUE;
        Desc.DepthBias = D3D11_DEFAULT_DEPTH_BIAS;
        Desc.DepthBiasClamp = D3D11_DEFAULT_DEPTH_BIAS_CLAMP;
        Desc.SlopeScaledDepthBias = D3D11_DEFAULT_SLOPE_SCALED_DEPTH_BIAS;
        Desc.DepthClipEnable = TRUE;
        Desc.ScissorEnable = FALSE;
        Desc.MultisampleEnable = FALSE;
        Desc.AntialiasedLineEnable = FALSE;

		m_pRS_CullNone = new RasterizerState( *this, Desc ); m_statesCount++;

		// Create CullFront state
		Desc.CullMode = D3D11_CULL_FRONT;
		m_pRS_CullFront = new RasterizerState( *this, Desc ); m_statesCount++;

		// Create CullBack state
		Desc.CullMode = D3D11_CULL_BACK;
		m_pRS_CullBack = new RasterizerState( *this, Desc ); m_statesCount++;

		// Create the wireframe state
		Desc.FillMode = D3D11_FILL_WIREFRAME;
        Desc.CullMode = D3D11_CULL_NONE;
		m_pRS_WireFrame = new RasterizerState( *this, Desc ); m_statesCount++;
	}
	{
		D3D11_DEPTH_STENCIL_DESC	Desc;
		memset( &Desc, 0, sizeof(Desc) );
		Desc.DepthEnable = false;
		Desc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
		Desc.DepthFunc = D3D11_COMPARISON_LESS;
		Desc.StencilEnable = false;
		Desc.StencilReadMask = 0xFF;
		Desc.StencilWriteMask = 0xFF;

		m_pDS_Disabled = new DepthStencilState( *this, Desc ); m_statesCount++;

		// Create R/W Less state
		Desc.DepthEnable = true;
		m_pDS_ReadWriteLess = new DepthStencilState( *this, Desc ); m_statesCount++;

		Desc.DepthFunc = D3D11_COMPARISON_GREATER;
		m_pDS_ReadWriteGreater = new DepthStencilState( *this, Desc ); m_statesCount++;

		Desc.DepthFunc = D3D11_COMPARISON_ALWAYS;	// Always write
		m_pDS_WriteAlways = new DepthStencilState( *this, Desc ); m_statesCount++;

		Desc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
		Desc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
		m_pDS_ReadLessEqual = new DepthStencilState( *this, Desc ); m_statesCount++;

		// ============= Stencil operations =============
		Desc.StencilEnable = true;

		// First stencil operation is increment stencil if a back face fails the depth test and decrement it if a front face fails the depth test
		// The net effect is that objects INSIDE a volume will have a stencil != 0
		// Objects in front of a volume will be increased by back faces and decreased by front faces so their stencil == 0
		// Objects behind a volume will keep stencil at 0 because both back & front depth tests succeed
		//
		Desc.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;		// Always pass stencil, only depth is important here!
		Desc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;		// Will always occur
		Desc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;		// Will never occur
		Desc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_DECR;	// If failed, decrement

		Desc.BackFace.StencilFunc = D3D11_COMPARISON_ALWAYS;		// Always pass stencil, only depth is important here!
		Desc.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;		// Will always occur
		Desc.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;		// Will never occur
		Desc.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_INCR;	// If failed, increment

		m_pDS_ReadLessEqual_StencilIncBackDecFront = new DepthStencilState( *this, Desc ); m_statesCount++;

		// Second stencil operation is the one that succeeds if stencil is != 0
		// This is the state we need after the stencil pass that used the state above
		Desc.DepthEnable = false;									// No depth test for this pass!

		Desc.FrontFace.StencilFunc = D3D11_COMPARISON_NOT_EQUAL;	// Pass if stencil is not 0
		Desc.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;		// Don't care
		Desc.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;		// Don't care
		Desc.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;	// Don't care

		Desc.BackFace.StencilFunc = D3D11_COMPARISON_NOT_EQUAL;		// Pass if stencil is not 0
		Desc.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;		// Don't care
		Desc.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;		// Don't care
		Desc.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;	// Don't care

		m_pDS_ReadLessEqual_StencilFailIfZero = new DepthStencilState( *this, Desc ); m_statesCount++;
	}
	{
		D3D11_BLEND_DESC	Desc;
		memset( &Desc, 0, sizeof(Desc) );
		Desc.AlphaToCoverageEnable = false;
		Desc.IndependentBlendEnable = false;
		Desc.RenderTarget[0].BlendEnable = false;
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_COLOR;
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_DEST_COLOR;
		Desc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_ADD;
		Desc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_SRC_ALPHA;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_DEST_ALPHA;
		Desc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_ADD;

		// Special "no blend + no color write" for double speed Z pre-pass
		Desc.RenderTarget[0].RenderTargetWriteMask = 0;	// Write no channels
		m_pBS_ZPrePass = new BlendState( *this, Desc ); m_statesCount++;

		// Disabled
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;	// Write all channels
		m_pBS_Disabled = new BlendState( *this, Desc ); m_statesCount++;

		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_RED;
		m_pBS_Disabled_RedOnly = new BlendState( *this, Desc ); m_statesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_GREEN;
		m_pBS_Disabled_GreenOnly = new BlendState( *this, Desc ); m_statesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_BLUE;
		m_pBS_Disabled_BlueOnly = new BlendState( *this, Desc ); m_statesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALPHA;
		m_pBS_Disabled_AlphaOnly = new BlendState( *this, Desc ); m_statesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;

		// Alpha blending (Dst = SrcAlpha * Src + (1-SrcAlpha) * Dst)
		Desc.RenderTarget[0].BlendEnable = true;
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_ALPHA;
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_INV_SRC_ALPHA;
		m_pBS_AlphaBlend = new BlendState( *this, Desc ); m_statesCount++;

		// Premultiplied alpa (Dst = Src + (1-SrcAlpha)*Dst)
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
		m_pBS_PremultipliedAlpha = new BlendState( *this, Desc ); m_statesCount++;

		// Additive
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ONE;
		m_pBS_Additive = new BlendState( *this, Desc ); m_statesCount++;

		// Max (Dst = max(Src,Dst))
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_MAX;
		Desc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_MAX;
		m_pBS_Max = new BlendState( *this, Desc ); m_statesCount++;

		// Min (Dst = min(Src,Dst))
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_MIN;
		Desc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_MIN;
		m_pBS_Min = new BlendState( *this, Desc ); m_statesCount++;
	}

	//////////////////////////////////////////////////////////////////////////
	// Create default samplers
	D3D11_SAMPLER_DESC	Desc;
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
	Desc.MipLODBias = 0.0f;
	Desc.MaxAnisotropy = 16;
	Desc.ComparisonFunc = D3D11_COMPARISON_NEVER;
	Desc.BorderColor[0] = Desc.BorderColor[2] = 1.0f;	Desc.BorderColor[1] = Desc.BorderColor[3] = 0.0f;
	Desc.MinLOD = -D3D11_FLOAT32_MAX;
	Desc.MaxLOD = D3D11_FLOAT32_MAX;

	m_device->CreateSamplerState( &Desc, &m_ppSamplers[0] );	// Linear Clamp
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
	m_device->CreateSamplerState( &Desc, &m_ppSamplers[1] );	// Point Clamp

	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
	m_device->CreateSamplerState( &Desc, &m_ppSamplers[3] );	// Point Wrap
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
	m_device->CreateSamplerState( &Desc, &m_ppSamplers[2] );	// Linear Wrap

	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_MIRROR;
	m_device->CreateSamplerState( &Desc, &m_ppSamplers[4] );	// Linear Mirror
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
	m_device->CreateSamplerState( &Desc, &m_ppSamplers[5] );	// Point Mirror

	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_BORDER;
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
	Desc.BorderColor[0] = 0.0f;
	Desc.BorderColor[1] = 0.0f;
	Desc.BorderColor[2] = 0.0f;
	Desc.BorderColor[3] = 0.0f;
	m_device->CreateSamplerState( &Desc, &m_ppSamplers[6] );	// Linear Black Border

	// Shadow sampler with comparison
	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
	Desc.Filter = D3D11_FILTER_COMPARISON_MIN_MAG_MIP_LINEAR;
	Desc.ComparisonFunc = D3D11_COMPARISON_LESS_EQUAL;
	m_device->CreateSamplerState( &Desc, &m_ppSamplers[7] );


	// Upload them once and for all
	m_deviceContext->VSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_deviceContext->HSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_deviceContext->DSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_deviceContext->GSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_deviceContext->PSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_deviceContext->CSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );

	return true;
}

static void		ReleaseQueryObject( int _entryIndex, Device::DoubleBufferedQuery*& _query, void* _pUserData ) {
	SAFE_DELETE( _query );
}

void	Device::Exit() {
	if ( m_device == NULL )
		return; // Already released !

	// Dispose of all the registered components in reverse order (we should only be left with default targets & states if you were clean)
	while ( m_componentsStackTop != NULL )
		delete m_componentsStackTop;  // DIE !!

	// Dispose of samplers
	for ( int SamplerIndex=0; SamplerIndex < SAMPLERS_COUNT; SamplerIndex++ )
		m_ppSamplers[SamplerIndex]->Release();

	m_swapChain->Release();

	m_deviceContext->ClearState();
	m_deviceContext->Flush();

	m_deviceContext->Release(); m_deviceContext = NULL;
	m_device->Release(); m_device = NULL;

	// Patapom [18/01/30] Release performance queries
	SAFE_DELETE( m_queryDisjoint );
	SAFE_DELETE( m_queryFrameBegin );
	SAFE_DELETE( m_queryFrameEnd );
	m_performanceQueries.ForEach( ReleaseQueryObject, NULL );
}

void	Device::ResizeSwapChain( U32 _width,  U32 _height ) {
	DXGI_SWAP_CHAIN_DESC	desc;
	m_swapChain->GetDesc( &desc );

	bool	issRGB = desc.BufferDesc.Format == DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
	ResizeSwapChain( _width, _height, issRGB );
}
void	Device::ResizeSwapChain( U32 _width,  U32 _height, bool _sRGB ) {

	DXGI_SWAP_CHAIN_DESC	desc;
	m_swapChain->GetDesc( &desc );

	bool	issRGB = desc.BufferDesc.Format == DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
	if ( _width == desc.BufferDesc.Width && _height == desc.BufferDesc.Height && _sRGB == issRGB ) {
		return;	// No change
	}

	// Resize swap chain
	m_swapChain->ResizeBuffers( 2, _width, _height, _sRGB ? DXGI_FORMAT_R8G8B8A8_UNORM_SRGB : DXGI_FORMAT_R8G8B8A8_UNORM, 0 );

	// Resize the default target
	ID3D11Texture2D*	pDefaultRenderSurface;
	m_swapChain->GetBuffer( 0, __uuidof( ID3D11Texture2D ), (void**) &pDefaultRenderSurface );
	ASSERT( pDefaultRenderSurface != NULL, "Failed to retrieve default render surface!" );

	m_pDefaultRenderTarget->WrapExistingTexture( *pDefaultRenderSurface );

	// Resize the default depth stencil buffer
	SAFE_DELETE( m_pDefaultDepthStencil );
	m_pDefaultDepthStencil = new Texture2D( *this, _width, _height, 1, 1, BaseLib::PIXEL_FORMAT::R32F, BaseLib::DEPTH_COMPONENT_FORMAT::DEPTH_ONLY );
}

void	Device::SwitchFullScreenState( bool _fullscreen, const AdapterOutput* _targetOutput ) {
	IDXGIOutput*	output = _targetOutput != NULL ? _targetOutput->m_output : NULL;
	m_swapChain->SetFullscreenState( _fullscreen, _fullscreen ? output : NULL );
}

void	Device::ClearRenderTarget( const Texture2D& _Target, const bfloat4& _Color )
{
	ClearRenderTarget( *_Target.GetRTV(), _Color );
}

void	Device::ClearRenderTarget( const Texture3D& _Target, const bfloat4& _Color )
{
	ClearRenderTarget( *_Target.GetRTV(), _Color );
}

void	Device::ClearRenderTarget( ID3D11RenderTargetView& _TargetView, const bfloat4& _Color )
{
	m_deviceContext->ClearRenderTargetView( &_TargetView, &_Color.x );
}

void	Device::ClearDepthStencil( const Texture2D& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth, bool _bClearStencil )
{
	ClearDepthStencil( *_DepthStencil.GetDSV(), _Z, _Stencil, _bClearDepth, _bClearStencil );
}
void	Device::ClearDepthStencil( ID3D11DepthStencilView& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth, bool _bClearStencil )
{
	m_deviceContext->ClearDepthStencilView( &_DepthStencil, (_bClearDepth ? D3D11_CLEAR_DEPTH : 0) | (_bClearStencil ? D3D11_CLEAR_STENCIL : 0), _Z, _Stencil );
}

void	Device::SetRenderTarget( const Texture2D& _Target, const Texture2D* _pDepthStencil, const D3D11_VIEWPORT* _pViewport )
{
	ID3D11RenderTargetView*	pTargetView = _Target.GetRTV( 0, 0, 0 );
	ID3D11DepthStencilView*	pDepthStencilView = _pDepthStencil != NULL ? _pDepthStencil->GetDSV() : NULL;

	SetRenderTargets( _Target.GetWidth(), _Target.GetHeight(), 1, &pTargetView, pDepthStencilView, _pViewport );
}

void	Device::SetRenderTarget( const Texture3D& _Target, const Texture2D* _pDepthStencil, const D3D11_VIEWPORT* _pViewport )
{
	ID3D11RenderTargetView*	pTargetView = _Target.GetRTV( 0, 0, 0 );
	ID3D11DepthStencilView*	pDepthStencilView = _pDepthStencil != NULL ? _pDepthStencil->GetDSV() : NULL;

	SetRenderTargets( _Target.GetWidth(), _Target.GetHeight(), 1, &pTargetView, pDepthStencilView, _pViewport );
}

void	Device::SetRenderTarget( U32 _Width, U32 _Height, const ID3D11RenderTargetView& _Target, ID3D11DepthStencilView* _pDepthStencil, const D3D11_VIEWPORT* _pViewport ) {
	const ID3D11RenderTargetView*	pTargetView = &_Target;
	SetRenderTargets( _Width, _Height, 1, (ID3D11RenderTargetView* const*) &pTargetView, _pDepthStencil, _pViewport );
}

void	Device::SetRenderTargets( U32 _Width, U32 _Height, U32 _TargetsCount, ID3D11RenderTargetView* const * _ppTargets, ID3D11DepthStencilView* _pDepthStencil, const D3D11_VIEWPORT* _pViewport ) {
	if ( _pViewport == NULL ) {
		// Use default viewport
		D3D11_VIEWPORT	Viewport;
		Viewport.TopLeftX = 0;
		Viewport.TopLeftY = 0;
		Viewport.Width = float(_Width);
		Viewport.Height = float(_Height);
		Viewport.MinDepth = 0.0f;
		Viewport.MaxDepth = 1.0f;
		m_deviceContext->RSSetViewports( 1, &Viewport );
	}
	else
		m_deviceContext->RSSetViewports( 1, _pViewport );

	m_deviceContext->OMSetRenderTargets( _TargetsCount, _ppTargets, _pDepthStencil );
}

void	Device::RemoveRenderTargets() {
	static ID3D11RenderTargetView*	ppEmpty[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, };
	m_deviceContext->OMSetRenderTargets( 8, ppEmpty, NULL );
}

void	Device::RemoveUAVs() {
	static ID3D11UnorderedAccessView*	ppEmpty[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, };
	UINT	pInitialCount[8] = { ~0U };
	m_deviceContext->OMSetRenderTargetsAndUnorderedAccessViews( D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, NULL, NULL, 0, 8, ppEmpty, pInitialCount );
	m_deviceContext->CSSetUnorderedAccessViews( 0, 8, ppEmpty, pInitialCount );
}

void	Device::SetStates( RasterizerState* _pRasterizerState, DepthStencilState* _pDepthStencilState, BlendState* _pBlendState )
{
	if ( _pRasterizerState != NULL && _pRasterizerState != m_pCurrentRasterizerState )
	{
		m_deviceContext->RSSetState( _pRasterizerState->m_pState );
		m_pCurrentRasterizerState = _pRasterizerState;
	}

	if ( _pDepthStencilState != NULL && _pDepthStencilState != m_pCurrentDepthStencilState )
	{
		m_deviceContext->OMSetDepthStencilState( _pDepthStencilState->m_pState, m_stencilRef );
		m_pCurrentDepthStencilState = _pDepthStencilState;
	}

	if ( _pBlendState != NULL && _pBlendState != m_pCurrentBlendState )
	{
		m_deviceContext->OMSetBlendState( _pBlendState->m_pState, &m_blendFactors.x, m_blendMasks );
		m_pCurrentBlendState = _pBlendState;
	}
}

void	Device::SetStatesReferences( const bfloat4& _BlendFactors, U32 _BlendSampleMask, U8 _StencilRef )
{
	m_blendFactors = _BlendFactors;
	m_blendMasks = _BlendSampleMask;
	m_stencilRef = _StencilRef;
}

void	Device::SetScissorRect( const D3D11_RECT* _pScissor ) {
	D3D11_RECT	Full = {
		0, 0,
		LONG( DefaultRenderTarget().GetWidth()),
		LONG( DefaultRenderTarget().GetHeight())
	};
	m_deviceContext->RSSetScissorRects( 1, _pScissor != NULL ? _pScissor : &Full );
}

void	Device::RemoveShaderResources( int _SlotIndex, int _SlotsCount, U32 _ShaderStages ) {
	static bool							ViewsInitialized = false;
	static ID3D11ShaderResourceView*	ppNULL[128];
	if ( !ViewsInitialized )
	{
		memset( ppNULL, NULL, _SlotsCount*sizeof(ID3D11ShaderResourceView*) );
		ViewsInitialized = true;
	}

	if ( (_ShaderStages & SSF_VERTEX_SHADER) != 0 )
		m_deviceContext->VSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_HULL_SHADER) != 0 )
		m_deviceContext->HSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_DOMAIN_SHADER) != 0 )
		m_deviceContext->DSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_GEOMETRY_SHADER) != 0 )
		m_deviceContext->GSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_PIXEL_SHADER) != 0 )
		m_deviceContext->PSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_COMPUTE_SHADER) != 0 )
		m_deviceContext->CSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_COMPUTE_SHADER_UAV) != 0 )
	{
		U32	UAVInitCount = -1;
		m_deviceContext->CSSetUnorderedAccessViews( _SlotIndex, _SlotsCount, (ID3D11UnorderedAccessView**) ppNULL, &UAVInitCount );
	}
}

//////////////////////////////////////////////////////////////////////////
// Performance queries
U32	Device::DoubleBufferedQuery::ms_currentFrameQueryIndex = 0;
U32	Device::DoubleBufferedQuery::MAX_FRAMES = 2;

Device::DoubleBufferedQuery::DoubleBufferedQuery( Device& _owner, U32 _markerID, D3D11_QUERY _queryType )
	: m_markerID( _markerID )
	, m_nextMarkerID( ~0U ) {

	D3D11_QUERY_DESC	desc;
	desc.MiscFlags = 0;
	desc.Query = _queryType;
	for ( U32 i=0; i < MAX_FRAMES; i++ ) {
		_owner.m_device->CreateQuery( &desc, &m_queries[i] );
	}
}
Device::DoubleBufferedQuery::~DoubleBufferedQuery() {
	for ( U32 i=0; i < MAX_FRAMES; i++ ) {
		SAFE_RELEASE( m_queries[i] );
	}
}
U64		Device::DoubleBufferedQuery::GetTimeStamp( Device& _owner ) {
	_owner.m_deviceContext->GetData( *this, &m_timeStamp, sizeof(UINT64), 0 );
	return m_timeStamp;
}

void	Device::PerfBeginFrame() {
	// Begin disjoint query, and timestamp the beginning of the frame
	if ( m_queryDisjoint == NULL ) {
		m_queryDisjoint = new DoubleBufferedQuery( *this, 0, D3D11_QUERY_TIMESTAMP_DISJOINT );
		m_queryFrameBegin = new DoubleBufferedQuery( *this, 0, D3D11_QUERY_TIMESTAMP );
		m_queryFrameEnd = new DoubleBufferedQuery( *this, ~0U, D3D11_QUERY_TIMESTAMP );
	}
	m_deviceContext->Begin( *m_queryDisjoint );
	m_deviceContext->End( *m_queryFrameBegin );
	m_lastQuery = m_queryFrameBegin;
}
void	Device::PerfSetMarker( U32 _markerID ) {
	ASSERT( m_lastQuery != NULL, "You can't call PerfSetMarker() if you didn't call PerfBeginFrame() first!" );
	DoubleBufferedQuery**	existingQuery = m_performanceQueries.Get( _markerID );
	if ( existingQuery == NULL ) {
		existingQuery = &m_performanceQueries.Add( _markerID, new DoubleBufferedQuery( *this, _markerID, D3D11_QUERY_TIMESTAMP ) );
	}
	m_deviceContext->End( **existingQuery );
	m_lastQuery->m_nextMarkerID = (*existingQuery)->m_markerID;
	m_lastQuery = *existingQuery;
}

static void		QueryTimeStamps( int _entryIndex, Device::DoubleBufferedQuery*& _query, void* _pUserData ) {
	_query->GetTimeStamp( *((Device*) _pUserData) );
}

double	Device::PerfEndFrame() {
	m_deviceContext->End( *m_queryFrameEnd );
	m_deviceContext->End( *m_queryDisjoint );
	m_lastQuery->m_nextMarkerID = m_queryFrameEnd->m_markerID;
	m_lastQuery = NULL;
	m_framesCount++;
	DoubleBufferedQuery::ms_currentFrameQueryIndex = (DoubleBufferedQuery::ms_currentFrameQueryIndex + 1) % DoubleBufferedQuery::MAX_FRAMES;	// Swap!

//	m_queryClockFrequency = 0;
	if ( m_deviceContext->GetData( *m_queryDisjoint, NULL, 0, 0 ) != S_OK )
		return -1.0;
// 	while ( m_deviceContext->GetData( m_queryDisjoint, NULL, 0, 0 ) == S_FALSE ) {
//         Sleep(1);       // Wait a bit, but give other threads a chance to run
//     }

	// Check whether timestamps were disjoint during the last frame
	D3D11_QUERY_DATA_TIMESTAMP_DISJOINT	tsDisjoint;
	if ( m_deviceContext->GetData( *m_queryDisjoint, &tsDisjoint, sizeof(tsDisjoint), 0 ) != S_OK )
		return -1.0;	// Maybe first frame?
	if ( tsDisjoint.Disjoint )
		return - 1.0;
	m_queryClockFrequency = tsDisjoint.Frequency;

	// Get all the timestamps
	m_queryFrameBegin->GetTimeStamp( *this );
	m_queryFrameEnd->GetTimeStamp( *this );
	m_performanceQueries.ForEach( QueryTimeStamps, this );

	U64		timeStampDelta = m_queryFrameEnd->m_timeStamp - m_queryFrameBegin->m_timeStamp;
	double	result = 1000.0 * double( timeStampDelta ) / m_queryClockFrequency;
	return result;
}

double		Device::PerfGetMilliSeconds( U32 _markerIDStart, U32 _markerIDEnd ) {
	DoubleBufferedQuery**	startQuery = m_performanceQueries.Get( _markerIDStart );
	ASSERT( startQuery != NULL, "Invalid start query marker ID!" );

	if ( _markerIDEnd == ~0U )
		_markerIDEnd = (*startQuery)->m_nextMarkerID;
	DoubleBufferedQuery**	endQuery = _markerIDEnd == m_queryFrameEnd->m_markerID ? &m_queryFrameEnd : m_performanceQueries.Get( _markerIDEnd );
	ASSERT( endQuery != NULL, "Invalid end query marker ID!" );

	// Convert to real time
	U64		timeStampDelta = (*endQuery)->m_timeStamp - (*startQuery)->m_timeStamp;
	double	result = 1000.0 * double( timeStampDelta ) / m_queryClockFrequency;
	return result;
}

//////////////////////////////////////////////////////////////////////////
//
void	Device::RegisterComponent( Component& _Component ) {
	// Attach to the end of the list
	if ( m_componentsStackTop != NULL )
		m_componentsStackTop->m_next = &_Component;
	_Component.m_previous = m_componentsStackTop;

	m_componentsStackTop = &_Component;
}

void	Device::UnRegisterComponent( Component& _Component )
{
	// Link over
	if ( _Component.m_previous != NULL )
		_Component.m_previous->m_next = _Component.m_next;
	if ( _Component.m_next != NULL )
		_Component.m_next->m_previous = _Component.m_previous;
	else
		m_componentsStackTop = _Component.m_previous;	// We were the top of the stack !
}

bool	Device::Check( HRESULT _Result ) {
#if defined(_DEBUG) && defined(GODCOMPLEX)
	ASSERT( _Result == S_OK, "DX HRESULT Check failed !" );
	if ( _Result != S_OK )
		PostQuitMessage( _Result );
	return true;
#else
	if ( _Result != S_OK )
		return false;	// So we can put a break point here...

	return true;
#endif
}
 
