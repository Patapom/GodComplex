// This is the main DLL file.

#include "stdafx.h"
#include "Device.h"
#include "Texture2D.h"
#include "Shader.h"

void	RendererManaged::Device::Init( System::IntPtr _WindowHandle, int _WindowWidth, int _WindowHeight, bool _FullScreen, bool _sRGBRenderTarget )
{
	m_pDevice->Exit();
	m_pDevice->Init( _WindowWidth, _WindowHeight, (HWND) _WindowHandle.ToInt32(), _FullScreen, _sRGBRenderTarget );

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

void	RendererManaged::Device::SetRenderStates( RASTERIZER_STATE _RS, DEPTHSTENCIL_STATE _DS, BLEND_STATE _BS )
{
	::RasterizerState*	pRS = NULL;
	switch ( _RS )
	{
	case RASTERIZER_STATE::CULL_NONE:	pRS = m_pDevice->m_pRS_CullNone; break;
	}

	::DepthStencilState*	pDS = NULL;
	switch ( _DS )
	{
	case DEPTHSTENCIL_STATE::DISABLED:						pDS = m_pDevice->m_pDS_Disabled; break;
	case DEPTHSTENCIL_STATE::READ_DEPTH_LESS_EQUAL:			pDS = m_pDevice->m_pDS_ReadLessEqual; break;
	case DEPTHSTENCIL_STATE::READ_WRITE_DEPTH_LESS_EQUAL:	pDS = m_pDevice->m_pDS_ReadWriteLess; break;
	}

	::BlendState*	pBS = NULL;
	switch ( _BS )
	{
	case BLEND_STATE::DISABLED:	pBS = m_pDevice->m_pBS_Disabled; break;
	}

	m_pDevice->SetStates( pRS, pDS, pBS );
}

void	RendererManaged::Device::SetRenderTarget( Texture2D^ _RenderTarget, Texture2D^ _DepthStencilTarget )
{
	m_pDevice->SetRenderTarget( *_RenderTarget->m_pTexture, _DepthStencilTarget != nullptr ? _DepthStencilTarget->m_pTexture : NULL );
}

void	RendererManaged::Device::RenderFullscreenQuad( Shader^ _Shader )
{
	_Shader->m_pShader->Use();
	m_pQuad->Render( *_Shader->m_pShader );
}
