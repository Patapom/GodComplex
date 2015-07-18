// This is the main DLL file.

#include "stdafx.h"
#include "Texture2D.h"
#include "Texture3D.h"
#include "Shader.h"

void	RendererManaged::Device::Init( System::IntPtr _WindowHandle, bool _FullScreen, bool _sRGBRenderTarget )
{
	m_pDevice->Exit();
	if ( !m_pDevice->Init( (HWND) _WindowHandle.ToInt32(), _FullScreen, _sRGBRenderTarget ) )
		throw gcnew Exception( "Failed to initialize the DirectX device with DX11 level!" );

	m_DefaultTarget = gcnew RendererManaged::Texture2D( m_pDevice->DefaultRenderTarget() );
	m_DefaultDepthStencil = gcnew RendererManaged::Texture2D( m_pDevice->DefaultDepthStencil() );

	// Build the quad
	float	pVertices[4*4] = {
		-1.0f, 1.0f, 0.0f, 1.0f,
		-1.0f, -1.0f, 0.0f, 1.0f,
		1.0f, 1.0f, 0.0f, 1.0f,
		1.0f, -1.0f, 0.0f, 1.0f,
	};
	m_pQuad = new Primitive( *m_pDevice, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, VertexFormatPt4::DESCRIPTOR );
}

void	RendererManaged::Device::Clear( float4 _ClearColor )
{
	m_pDevice->ClearRenderTarget( m_pDevice->DefaultRenderTarget(), ::float4( _ClearColor.x, _ClearColor.y, _ClearColor.z, _ClearColor.w ) );
}

void	RendererManaged::Device::Clear( Texture2D^ _RenderTarget, float4 _ClearColor )
{
	m_pDevice->ClearRenderTarget( *_RenderTarget->m_pTexture, ::float4( _ClearColor.x, _ClearColor.y, _ClearColor.z, _ClearColor.w ) );
}

void	RendererManaged::Device::Clear( Texture3D^ _RenderTarget, float4 _ClearColor )
{
	m_pDevice->ClearRenderTarget( *_RenderTarget->m_pTexture, ::float4( _ClearColor.x, _ClearColor.y, _ClearColor.z, _ClearColor.w ) );
}

void	RendererManaged::Device::ClearDepthStencil( Texture2D^ _RenderTarget, float _Z, byte _Stencil, bool _ClearDepth, bool _ClearStencil )
{
	m_pDevice->ClearDepthStencil( *_RenderTarget->m_pTexture, _Z, _Stencil, _ClearDepth, _ClearStencil );
}

void	RendererManaged::Device::SetRenderStates( RASTERIZER_STATE _RS, DEPTHSTENCIL_STATE _DS, BLEND_STATE _BS )
{
	::RasterizerState*	pRS = NULL;
	switch ( _RS )
	{
	case RASTERIZER_STATE::NOCHANGE: break;
	case RASTERIZER_STATE::CULL_NONE:	pRS = m_pDevice->m_pRS_CullNone; break;
	case RASTERIZER_STATE::CULL_BACK:	pRS = m_pDevice->m_pRS_CullBack; break;
	case RASTERIZER_STATE::CULL_FRONT:	pRS = m_pDevice->m_pRS_CullFront; break;
	default: throw gcnew Exception( "Unsupported rasterizer state!" );
	}

	::DepthStencilState*	pDS = NULL;
	switch ( _DS )
	{
	case DEPTHSTENCIL_STATE::NOCHANGE: break;
	case DEPTHSTENCIL_STATE::DISABLED:					pDS = m_pDevice->m_pDS_Disabled; break;
	case DEPTHSTENCIL_STATE::READ_DEPTH_LESS_EQUAL:		pDS = m_pDevice->m_pDS_ReadLessEqual; break;
	case DEPTHSTENCIL_STATE::READ_WRITE_DEPTH_LESS:		pDS = m_pDevice->m_pDS_ReadWriteLess; break;
	case DEPTHSTENCIL_STATE::READ_WRITE_DEPTH_GREATER:	pDS = m_pDevice->m_pDS_ReadWriteGreater; break;
	default: throw gcnew Exception( "Unsupported depth stencil state!" );
	}

	::BlendState*	pBS = NULL;
	switch ( _BS )
	{
	case BLEND_STATE::NOCHANGE: break;
	case BLEND_STATE::DISABLED:		pBS = m_pDevice->m_pBS_Disabled; break;
	case BLEND_STATE::ALPHA_BLEND:	pBS = m_pDevice->m_pBS_AlphaBlend; break;
	case BLEND_STATE::ADDITIVE:		pBS = m_pDevice->m_pBS_Additive; break;
	default: throw gcnew Exception( "Unsupported blend state!" );
	}

	m_pDevice->SetStates( pRS, pDS, pBS );
}

void	RendererManaged::Device::SetRenderTarget( Texture2D^ _RenderTarget, Texture2D^ _DepthStencilTarget )
{
	m_pDevice->SetRenderTarget( *_RenderTarget->m_pTexture, _DepthStencilTarget != nullptr ? _DepthStencilTarget->m_pTexture : NULL );
}

static ::ID3D11RenderTargetView*	gs_ppRenderTargetViews[8];
void	RendererManaged::Device::SetRenderTargets( int _Width, int _Height, cli::array<IView^>^ _RenderTargetViews, Texture2D^ _DepthStencilTarget )
{
	if ( _RenderTargetViews == nullptr )
		throw gcnew Exception( "Invalid render targets array!" );

// 		ppRenderTargets = new ::ID3D11RenderTargetView*[_RenderTargetViews->Length];
	for ( int i=0; i < _RenderTargetViews->Length; i++ )
		gs_ppRenderTargetViews[i] = _RenderTargetViews[i]->RTV;

	m_pDevice->SetRenderTargets( _Width, _Height, _RenderTargetViews->Length, gs_ppRenderTargetViews, _DepthStencilTarget != nullptr ? _DepthStencilTarget->m_pTexture->GetDSV() : NULL );

//	delete[] ppRenderTargets;
}

void	RendererManaged::Device::RenderFullscreenQuad( Shader^ _Shader )
{
	_Shader->m_pShader->Use();
	m_pQuad->Render( *_Shader->m_pShader );
}
