// This is the main DLL file.
#include "stdafx.h"

#include "Texture2D.h"
#include "Texture3D.h"
#include "Shader.h"

void	Renderer::Device::Init( System::IntPtr _windowHandle, bool _fullScreen, bool _sRGBRenderTarget ) {
	if ( _windowHandle == IntPtr::Zero )
		throw gcnew Exception( "Invalid window handle!" );
	m_pDevice->Exit();
	if ( !m_pDevice->Init( (HWND) _windowHandle.ToInt32(), _fullScreen, _sRGBRenderTarget ) )
		throw gcnew Exception( "Failed to initialize the DirectX device with DX11 level!" );

	m_defaultTarget = gcnew Renderer::Texture2D( m_pDevice->DefaultRenderTarget() );
	m_defaultDepthStencil = gcnew Renderer::Texture2D( m_pDevice->DefaultDepthStencil() );

	// Build the fullscreen quad
	float	pVertices[4*4] = {
		-1.0f, 1.0f, 0.0f, 1.0f,
		-1.0f, -1.0f, 0.0f, 1.0f,
		1.0f, 1.0f, 0.0f, 1.0f,
		1.0f, -1.0f, 0.0f, 1.0f,
	};
	m_pScreenQuad = new Primitive( *m_pDevice, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP, VertexFormatPt4::DESCRIPTOR );
}

void	Renderer::Device::Clear( float4 _clearColor ) {
	m_pDevice->ClearRenderTarget( m_pDevice->DefaultRenderTarget(), bfloat4( _clearColor.x, _clearColor.y, _clearColor.z, _clearColor.w ) );
}

void	Renderer::Device::Clear( Texture2D^ _RenderTarget, float4 _clearColor ) {
	m_pDevice->ClearRenderTarget( *_RenderTarget->m_texture
		, bfloat4( _clearColor.x, _clearColor.y, _clearColor.z, _clearColor.w ) );
}

void	Renderer::Device::Clear( Texture3D^ _RenderTarget, float4 _clearColor ) {
	m_pDevice->ClearRenderTarget( *_RenderTarget->m_texture, bfloat4( _clearColor.x, _clearColor.y, _clearColor.z, _clearColor.w ) );
}

void	Renderer::Device::ClearDepthStencil( Texture2D^ _RenderTarget, float _Z, byte _Stencil, bool _ClearDepth, bool _ClearStencil ) {
	m_pDevice->ClearDepthStencil( *_RenderTarget->m_texture, _Z, _Stencil, _ClearDepth, _ClearStencil );
}

void	Renderer::Device::SetRenderStates( RASTERIZER_STATE _RS, DEPTHSTENCIL_STATE _DS, BLEND_STATE _BS ) {
	::RasterizerState*	pRS = NULL;
	switch ( _RS ) {
		case RASTERIZER_STATE::NOCHANGE: break;
		case RASTERIZER_STATE::CULL_NONE:	pRS = m_pDevice->m_pRS_CullNone; break;
		case RASTERIZER_STATE::CULL_BACK:	pRS = m_pDevice->m_pRS_CullBack; break;
		case RASTERIZER_STATE::CULL_FRONT:	pRS = m_pDevice->m_pRS_CullFront; break;
		case RASTERIZER_STATE::WIREFRAME:	pRS = m_pDevice->m_pRS_WireFrame; break;
		default: throw gcnew Exception( "Unsupported rasterizer state!" );
	}

	::DepthStencilState*	pDS = NULL;
	switch ( _DS ) {
		case DEPTHSTENCIL_STATE::NOCHANGE: break;
		case DEPTHSTENCIL_STATE::DISABLED:					pDS = m_pDevice->m_pDS_Disabled; break;
		case DEPTHSTENCIL_STATE::READ_DEPTH_LESS_EQUAL:		pDS = m_pDevice->m_pDS_ReadLessEqual; break;
		case DEPTHSTENCIL_STATE::READ_WRITE_DEPTH_LESS:		pDS = m_pDevice->m_pDS_ReadWriteLess; break;
		case DEPTHSTENCIL_STATE::READ_WRITE_DEPTH_GREATER:	pDS = m_pDevice->m_pDS_ReadWriteGreater; break;
		default: throw gcnew Exception( "Unsupported depth stencil state!" );
	}

	::BlendState*	pBS = NULL;
	switch ( _BS ) {
		case BLEND_STATE::NOCHANGE: break;
		case BLEND_STATE::DISABLED:				pBS = m_pDevice->m_pBS_Disabled; break;
		case BLEND_STATE::ALPHA_BLEND:			pBS = m_pDevice->m_pBS_AlphaBlend; break;
		case BLEND_STATE::PREMULTIPLIED_ALPHA:	pBS = m_pDevice->m_pBS_PremultipliedAlpha; break;
		case BLEND_STATE::ADDITIVE:				pBS = m_pDevice->m_pBS_Additive; break;
		default: throw gcnew Exception( "Unsupported blend state!" );
	}

	m_pDevice->SetStates( pRS, pDS, pBS );
}

void	Renderer::Device::SetRenderTarget( Texture2D^ _RenderTarget, Texture2D^ _DepthStencilTarget ) {
	m_pDevice->SetRenderTarget( *_RenderTarget->m_texture, _DepthStencilTarget != nullptr ? _DepthStencilTarget->m_texture : NULL );
}

void	Renderer::Device::SetRenderTargets( UInt32 _width, UInt32 _height, cli::array<IView^>^ _renderTargetViews, Texture2D^ _depthStencilTarget ) {
	if ( _renderTargetViews == nullptr )
		throw gcnew Exception( "Invalid render targets array!" );

// 	ppRenderTargets = new ::ID3D11RenderTargetView*[_RenderTargetViews->Length];
	for ( int i=0; i < _renderTargetViews->Length; i++ )
		m_ppRenderTargetViews[i] = _renderTargetViews[i]->RTV;

	m_pDevice->SetRenderTargets( _width, _height, _renderTargetViews->Length, m_ppRenderTargetViews, _depthStencilTarget != nullptr ? _depthStencilTarget->m_texture->GetDSV() : NULL );

//	delete[] ppRenderTargets;
}

void	Renderer::Device::RenderFullscreenQuad( Shader^ _shader ) {
	_shader->m_pShader->Use();
	m_pScreenQuad->Render( *_shader->m_pShader );
}

ImageUtility::ImagesMatrix^	Renderer::Device::DDSCompress( ImageUtility::ImagesMatrix^ _sourceImage, ImageUtility::ImagesMatrix::COMPRESSION_TYPE _compressionType, ImageUtility::COMPONENT_FORMAT _componentFormat ) {
	ImageUtility::ImagesMatrix^	result = gcnew ImageUtility::ImagesMatrix();
	reinterpret_cast< ImageUtilityLib::ImagesMatrix* >( result->NativeObject.ToPointer() )->DDSCompress( *reinterpret_cast< ImageUtilityLib::ImagesMatrix* >( _sourceImage->NativeObject.ToPointer() ), ImageUtilityLib::ImagesMatrix::COMPRESSION_TYPE( _compressionType ), BaseLib::COMPONENT_FORMAT( _componentFormat ), reinterpret_cast< void* >( &m_pDevice->DXDevice() ) );
	return result;
}
