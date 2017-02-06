#include "stdafx.h"

#include "Device.h"
#include "Components/Component.h"
#include "Components/Texture2D.h"
#include "Components/Texture3D.h"
#include "Components/StructuredBuffer.h"
#include "Components/States.h"

Device::Device()
	: m_pDevice( NULL )
	, m_pDeviceContext( NULL )
	, m_pComponentsStackTop( NULL )
	, m_pCurrentMaterial( NULL )
	, m_pCurrentRasterizerState( NULL )
	, m_pCurrentDepthStencilState( NULL )
	, m_pCurrentBlendState( NULL )
	, m_BlendFactors( 1, 1, 1, 1 )
	, m_BlendMasks( ~0 )
	, m_StencilRef( 0 ) {
}

int		Device::ComponentsCount() const
{
	int			Count = -2 - m_StatesCount;	// Start without counting for our internal back buffer & depth stencil components
	Component*	pCurrent = m_pComponentsStackTop;
	while ( pCurrent != NULL )
	{
		Count++;
		pCurrent = pCurrent->m_previous;
	}

	return Count;
}

bool	Device::Init( HWND _Handle, bool _Fullscreen, bool _sRGB ) {
	RECT	Rect = { 0, 0, 0, 0 };
	if ( !GetWindowRect( _Handle, &Rect ) )
		throw "Failed to retrieve window dimensions to initialize device!";
	
	int	Width = Rect.right - Rect.left;
	int	Height = Rect.bottom - Rect.top;

	return Init( Width, Height, _Handle, _Fullscreen, _sRGB );
}

bool	Device::Init( U32 _width, U32 _height, HWND _handle, bool _fullscreen, bool _sRGB ) {
	// Create a swap chain with 2 back buffers
	DXGI_SWAP_CHAIN_DESC	SwapChainDesc;

	// Simple output buffer
	SwapChainDesc.BufferDesc.Width = _width;
	SwapChainDesc.BufferDesc.Height = _height;
	SwapChainDesc.BufferDesc.Format = _sRGB ? DXGI_FORMAT_R8G8B8A8_UNORM_SRGB : DXGI_FORMAT_R8G8B8A8_UNORM;
//	SwapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_STRETCHED;
	SwapChainDesc.BufferDesc.Scaling = DXGI_MODE_SCALING_CENTERED;
	SwapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
	SwapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
	SwapChainDesc.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	SwapChainDesc.BufferUsage = DXGI_USAGE_BACK_BUFFER | DXGI_USAGE_RENDER_TARGET_OUTPUT | DXGI_USAGE_SHADER_INPUT;
//	SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT | DXGI_USAGE_UNORDERED_ACCESS;
	SwapChainDesc.BufferCount = 2;

	// No multisampling
	SwapChainDesc.SampleDesc.Count = 1;
	SwapChainDesc.SampleDesc.Quality = 0;

	SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	SwapChainDesc.OutputWindow = _handle;
	SwapChainDesc.Windowed = !_fullscreen;
	SwapChainDesc.Flags = 0;

	int	FeatureLevelsCount = 2;
	D3D_FEATURE_LEVEL	FeatureLevels[] = { D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL_11_0 };		// Support D3D11...
	D3D_FEATURE_LEVEL	ObtainedFeatureLevel;

	#if defined(_DEBUG) && !defined(NSIGHT)
		UINT	DebugFlags = D3D11_CREATE_DEVICE_DEBUG;
	#else
		UINT	DebugFlags = 0;
	#endif

 	if ( !Check(
		D3D11CreateDeviceAndSwapChain( NULL, D3D_DRIVER_TYPE_HARDWARE, NULL,
			DebugFlags,
			FeatureLevels, FeatureLevelsCount,
			D3D11_SDK_VERSION,
			&SwapChainDesc, &m_pSwapChain,
			&m_pDevice, &ObtainedFeatureLevel, &m_pDeviceContext ) )
		)
		return false;

	// Store the default render target
	ID3D11Texture2D*	pDefaultRenderSurface;
	m_pSwapChain->GetBuffer( 0, __uuidof( ID3D11Texture2D ), (void**) &pDefaultRenderSurface );
	ASSERT( pDefaultRenderSurface != NULL, "Failed to retrieve default render surface !" );

//	m_pDefaultRenderTarget = new Texture2D( *this, *pDefaultRenderSurface, BaseLib::PF_RGBA8::Descriptor, _sRGB ? BaseLib::COMPONENT_FORMAT::UNORM_sRGB : BaseLib::COMPONENT_FORMAT::UNORM );
	m_pDefaultRenderTarget = new Texture2D( *this, *pDefaultRenderSurface );

	// Create the default depth stencil buffer
	m_pDefaultDepthStencil = new Texture2D( *this, _width, _height, 1, BaseLib::PF_D32::Descriptor );


	//////////////////////////////////////////////////////////////////////////
	// Create default render states
	m_StatesCount = 0;
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

		m_pRS_CullNone = new RasterizerState( *this, Desc ); m_StatesCount++;

		// Create CullFront state
		Desc.CullMode = D3D11_CULL_FRONT;
		m_pRS_CullFront = new RasterizerState( *this, Desc ); m_StatesCount++;

		// Create CullBack state
		Desc.CullMode = D3D11_CULL_BACK;
		m_pRS_CullBack = new RasterizerState( *this, Desc ); m_StatesCount++;

		// Create the wireframe state
		Desc.FillMode = D3D11_FILL_WIREFRAME;
        Desc.CullMode = D3D11_CULL_NONE;
		m_pRS_WireFrame = new RasterizerState( *this, Desc ); m_StatesCount++;
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

		m_pDS_Disabled = new DepthStencilState( *this, Desc ); m_StatesCount++;

		// Create R/W Less state
		Desc.DepthEnable = true;
		m_pDS_ReadWriteLess = new DepthStencilState( *this, Desc ); m_StatesCount++;

		Desc.DepthFunc = D3D11_COMPARISON_GREATER;
		m_pDS_ReadWriteGreater = new DepthStencilState( *this, Desc ); m_StatesCount++;

		Desc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
		Desc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
		m_pDS_ReadLessEqual = new DepthStencilState( *this, Desc ); m_StatesCount++;

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

		m_pDS_ReadLessEqual_StencilIncBackDecFront = new DepthStencilState( *this, Desc ); m_StatesCount++;

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

		m_pDS_ReadLessEqual_StencilFailIfZero = new DepthStencilState( *this, Desc ); m_StatesCount++;
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
		m_pBS_ZPrePass = new BlendState( *this, Desc ); m_StatesCount++;

		// Disabled
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;	// Write all channels
		m_pBS_Disabled = new BlendState( *this, Desc ); m_StatesCount++;

		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_RED;
		m_pBS_Disabled_RedOnly = new BlendState( *this, Desc ); m_StatesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_GREEN;
		m_pBS_Disabled_GreenOnly = new BlendState( *this, Desc ); m_StatesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_BLUE;
		m_pBS_Disabled_BlueOnly = new BlendState( *this, Desc ); m_StatesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALPHA;
		m_pBS_Disabled_AlphaOnly = new BlendState( *this, Desc ); m_StatesCount++;
		Desc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;

		// Alpha blending (Dst = SrcAlpha * Src + (1-SrcAlpha) * Dst)
		Desc.RenderTarget[0].BlendEnable = true;
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_SRC_ALPHA;
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_INV_SRC_ALPHA;
		m_pBS_AlphaBlend = new BlendState( *this, Desc ); m_StatesCount++;

		// Premultiplied alpa (Dst = Src + (1-SrcAlpha)*Dst)
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
		m_pBS_PremultipliedAlpha = new BlendState( *this, Desc ); m_StatesCount++;

		// Additive
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ONE;
		m_pBS_Additive = new BlendState( *this, Desc ); m_StatesCount++;

		// Max (Dst = max(Src,Dst))
		Desc.RenderTarget[0].SrcBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].SrcBlendAlpha = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlend = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].DestBlendAlpha = D3D11_BLEND_ONE;
		Desc.RenderTarget[0].BlendOp = D3D11_BLEND_OP_MAX;
		Desc.RenderTarget[0].BlendOpAlpha = D3D11_BLEND_OP_MAX;
		m_pBS_Max = new BlendState( *this, Desc ); m_StatesCount++;
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

	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[0] );	// Linear Clamp
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[1] );	// Point Clamp

	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[3] );	// Point Wrap
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[2] );	// Linear Wrap

	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_MIRROR;
	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[4] );	// Linear Mirror
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[5] );	// Point Mirror

	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_BORDER;
	Desc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
	Desc.BorderColor[0] = 0.0f;
	Desc.BorderColor[1] = 0.0f;
	Desc.BorderColor[2] = 0.0f;
	Desc.BorderColor[3] = 0.0f;
	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[6] );	// Linear Black Border

	// Shadow sampler with comparison
	Desc.AddressU = Desc.AddressV = Desc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
	Desc.Filter = D3D11_FILTER_COMPARISON_MIN_MAG_MIP_LINEAR;
	Desc.ComparisonFunc = D3D11_COMPARISON_LESS_EQUAL;
	m_pDevice->CreateSamplerState( &Desc, &m_ppSamplers[7] );


	// Upload them once and for all
	m_pDeviceContext->VSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_pDeviceContext->HSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_pDeviceContext->DSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_pDeviceContext->GSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_pDeviceContext->PSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );
	m_pDeviceContext->CSSetSamplers( 0, SAMPLERS_COUNT, m_ppSamplers );

	return true;
}

void	Device::Exit()
{
	if ( m_pDevice == NULL )
		return; // Already released !

	// Dispose of all the registered components in reverse order (we should only be left with default targets & states if you were clean)
	while ( m_pComponentsStackTop != NULL )
		delete m_pComponentsStackTop;  // DIE !!

	// Dispose of samplers
	for ( int SamplerIndex=0; SamplerIndex < SAMPLERS_COUNT; SamplerIndex++ )
		m_ppSamplers[SamplerIndex]->Release();

	m_pSwapChain->Release();

	m_pDeviceContext->ClearState();
	m_pDeviceContext->Flush();

	m_pDeviceContext->Release(); m_pDeviceContext = NULL;
	m_pDevice->Release(); m_pDevice = NULL;
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
	m_pDeviceContext->ClearRenderTargetView( &_TargetView, &_Color.x );
}

void	Device::ClearDepthStencil( const Texture2D& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth, bool _bClearStencil )
{
	ClearDepthStencil( *_DepthStencil.GetDSV(), _Z, _Stencil, _bClearDepth, _bClearStencil );
}
void	Device::ClearDepthStencil( ID3D11DepthStencilView& _DepthStencil, float _Z, U8 _Stencil, bool _bClearDepth, bool _bClearStencil )
{
	m_pDeviceContext->ClearDepthStencilView( &_DepthStencil, (_bClearDepth ? D3D11_CLEAR_DEPTH : 0) | (_bClearStencil ? D3D11_CLEAR_STENCIL : 0), _Z, _Stencil );
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
		m_pDeviceContext->RSSetViewports( 1, &Viewport );
	}
	else
		m_pDeviceContext->RSSetViewports( 1, _pViewport );

	m_pDeviceContext->OMSetRenderTargets( _TargetsCount, _ppTargets, _pDepthStencil );
}

void	Device::RemoveRenderTargets() {
	static ID3D11RenderTargetView*	ppEmpty[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, };
	m_pDeviceContext->OMSetRenderTargets( 8, ppEmpty, NULL );
}

void	Device::RemoveUAVs() {
	static ID3D11UnorderedAccessView*	ppEmpty[8] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, };
	UINT	pInitialCount[8] = { -1 };
	m_pDeviceContext->OMSetRenderTargetsAndUnorderedAccessViews( D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, NULL, NULL, 0, 8, ppEmpty, pInitialCount );
	m_pDeviceContext->CSSetUnorderedAccessViews( 0, 8, ppEmpty, pInitialCount );
}

void	Device::SetStates( RasterizerState* _pRasterizerState, DepthStencilState* _pDepthStencilState, BlendState* _pBlendState )
{
	if ( _pRasterizerState != NULL && _pRasterizerState != m_pCurrentRasterizerState )
	{
		m_pDeviceContext->RSSetState( _pRasterizerState->m_pState );
		m_pCurrentRasterizerState = _pRasterizerState;
	}

	if ( _pDepthStencilState != NULL && _pDepthStencilState != m_pCurrentDepthStencilState )
	{
		m_pDeviceContext->OMSetDepthStencilState( _pDepthStencilState->m_pState, m_StencilRef );
		m_pCurrentDepthStencilState = _pDepthStencilState;
	}

	if ( _pBlendState != NULL && _pBlendState != m_pCurrentBlendState )
	{
		m_pDeviceContext->OMSetBlendState( _pBlendState->m_pState, &m_BlendFactors.x, m_BlendMasks );
		m_pCurrentBlendState = _pBlendState;
	}
}

void	Device::SetStatesReferences( const bfloat4& _BlendFactors, U32 _BlendSampleMask, U8 _StencilRef )
{
	m_BlendFactors = _BlendFactors;
	m_BlendMasks = _BlendSampleMask;
	m_StencilRef = _StencilRef;
}

void	Device::SetScissorRect( const D3D11_RECT* _pScissor )
{
	D3D11_RECT	Full = {
		0, 0,
		DefaultRenderTarget().GetWidth(),
		DefaultRenderTarget().GetHeight()
	};
	m_pDeviceContext->RSSetScissorRects( 1, _pScissor != NULL ? _pScissor : &Full );
}

void	Device::RemoveShaderResources( int _SlotIndex, int _SlotsCount, U32 _ShaderStages )
{
	static bool							ViewsInitialized = false;
	static ID3D11ShaderResourceView*	ppNULL[128];
	if ( !ViewsInitialized )
	{
		memset( ppNULL, NULL, _SlotsCount*sizeof(ID3D11ShaderResourceView*) );
		ViewsInitialized = true;
	}

	if ( (_ShaderStages & SSF_VERTEX_SHADER) != 0 )
		m_pDeviceContext->VSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_HULL_SHADER) != 0 )
		m_pDeviceContext->HSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_DOMAIN_SHADER) != 0 )
		m_pDeviceContext->DSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_GEOMETRY_SHADER) != 0 )
		m_pDeviceContext->GSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_PIXEL_SHADER) != 0 )
		m_pDeviceContext->PSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_COMPUTE_SHADER) != 0 )
		m_pDeviceContext->CSSetShaderResources( _SlotIndex, _SlotsCount, ppNULL );
	if ( (_ShaderStages & SSF_COMPUTE_SHADER_UAV) != 0 )
	{
		U32	UAVInitCount = -1;
		m_pDeviceContext->CSSetUnorderedAccessViews( _SlotIndex, _SlotsCount, (ID3D11UnorderedAccessView**) ppNULL, &UAVInitCount );
	}
}

void	Device::RegisterComponent( Component& _Component )
{
	// Attach to the end of the list
	if ( m_pComponentsStackTop != NULL )
		m_pComponentsStackTop->m_next = &_Component;
	_Component.m_previous = m_pComponentsStackTop;

	m_pComponentsStackTop = &_Component;
}

void	Device::UnRegisterComponent( Component& _Component )
{
	// Link over
	if ( _Component.m_previous != NULL )
		_Component.m_previous->m_next = _Component.m_next;
	if ( _Component.m_next != NULL )
		_Component.m_next->m_previous = _Component.m_previous;
	else
		m_pComponentsStackTop = _Component.m_previous;	// We were the top of the stack !
}

bool	Device::Check( HRESULT _Result )
{
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
 
