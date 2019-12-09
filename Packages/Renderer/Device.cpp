// This is the main DLL file.
#include "stdafx.h"

#include "Texture2D.h"
#include "Texture3D.h"
#include "Primitive.h"
#include "Shader.h"

void	Renderer::Device::Init( System::IntPtr _windowHandle, bool _fullScreen, bool _sRGBRenderTarget ) {
	if ( _windowHandle == IntPtr::Zero )
		throw gcnew Exception( "Invalid window handle!" );
	m_pDevice->Exit();
	if ( !m_pDevice->Init( (HWND) _windowHandle.ToInt64(), _fullScreen, _sRGBRenderTarget ) )
		throw gcnew Exception( "Failed to initialize the DirectX device with DX11 level!" );

	m_defaultTarget = gcnew Renderer::Texture2D( m_pDevice->DefaultRenderTarget() );
	m_defaultDepthStencil = gcnew Renderer::Texture2D( m_pDevice->DefaultDepthStencil() );

	// Build the fullscreen quad
	cli::array<VertexPt4>^	vertices = gcnew cli::array<VertexPt4>( 4 );
	vertices[0].Pt.Set( -1, 1, 0, 1 );
	vertices[1].Pt.Set( -1, -1, 0, 1 );
	vertices[2].Pt.Set( 1, 1, 0, 1 );
	vertices[3].Pt.Set( 1, -1, 0, 1 );

	ByteBuffer^	buffVertices = VertexPt4::FromArray( vertices );
	m_screenQuad = gcnew Primitive( this, 4, buffVertices, nullptr, Primitive::TOPOLOGY::TRIANGLE_STRIP, VERTEX_FORMAT::Pt4 );
}

cli::array< cli::array< Renderer::Device::AdapterOutput^ >^ >^	Renderer::Device::AdapterOutputs::get() {
	if ( m_cachedAdapterOutputs == nullptr ) {
		// Cache them all at once
		m_cachedAdapterOutputs = gcnew cli::array<cli::array<AdapterOutput ^> ^>( m_pDevice->m_adapterOutputs.Count() );
		for ( UInt32 adapterIndex=0; adapterIndex < m_pDevice->m_adapterOutputs.Count(); adapterIndex++ ) {
			const BaseLib::List< ::Device::AdapterOutput >&	nativeAdapterOutputs = m_pDevice->m_adapterOutputs[adapterIndex];
			cli::array<AdapterOutput ^>^					adapterOutputs = gcnew cli::array<AdapterOutput ^>( nativeAdapterOutputs.Count() );

			m_cachedAdapterOutputs[adapterIndex] = adapterOutputs;

			for ( UInt32 outputIndex=0; outputIndex < nativeAdapterOutputs.Count(); outputIndex++ ) {
				const ::Device::AdapterOutput&	nativeAdapterOutput = nativeAdapterOutputs[outputIndex];
				adapterOutputs[outputIndex] = gcnew AdapterOutput();
				adapterOutputs[outputIndex]->m_pAdapterOutput = &nativeAdapterOutput;
			}
		}
	}

	return m_cachedAdapterOutputs;
}

void	Renderer::Device::ResizeSwapChain( UInt32 _width, UInt32 _height, bool _sRGB ) {
	m_pDevice->ResizeSwapChain( _width, _height, _sRGB );
}
void	Renderer::Device::ResizeSwapChain( UInt32 _width, UInt32 _height ) {
	m_pDevice->ResizeSwapChain( _width, _height );
}

void	Renderer::Device::SwitchFullScreenState( bool _fullscreen, AdapterOutput^ _targetOutput ) {
	const ::Device::AdapterOutput*	nativeOutput = _targetOutput != nullptr ? _targetOutput->m_pAdapterOutput : NULL;
	m_pDevice->SwitchFullScreenState( _fullscreen, nativeOutput );
}
void	Renderer::Device::SwitchFullScreenState( bool _fullscreen ) {
	SwitchFullScreenState( _fullscreen, nullptr );
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
		case DEPTHSTENCIL_STATE::WRITE_ALWAYS:				pDS = m_pDevice->m_pDS_WriteAlways; break;
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

void	Renderer::Device::SetRenderTargets( cli::array<IView^>^ _renderTargetViews, IView^ _depthStencilTargetView ) {
	U32	W, H;
	U32	renderTargetsCount = 0;
	if ( _renderTargetViews != nullptr && _renderTargetViews->Length > 0 ) {
		W = _renderTargetViews[0]->Width;
		H = _renderTargetViews[0]->Height;
		renderTargetsCount = _renderTargetViews->Length;
		for ( int i=0; i < _renderTargetViews->Length; i++ )
			m_ppRenderTargetViews[i] = _renderTargetViews[i]->RTV;
	} else if ( _depthStencilTargetView != nullptr ) {
		W = _depthStencilTargetView->Width;
		H = _depthStencilTargetView->Height;
	} else {
		throw gcnew Exception( "Render target views and depth stencil view cannot both be null!" );
	}
	m_pDevice->SetRenderTargets( W, H, renderTargetsCount, m_ppRenderTargetViews, _depthStencilTargetView != nullptr ? _depthStencilTargetView->DSV : NULL );
}

void	Renderer::Device::RenderFullscreenQuad( Shader^ _shader ) {
	_shader->m_pShader->Use();
	m_screenQuad->Render( _shader );
}

ImageUtility::ImagesMatrix^	Renderer::Device::DDSCompress( ImageUtility::ImagesMatrix^ _sourceImage, ImageUtility::ImagesMatrix::COMPRESSION_TYPE _compressionType, ImageUtility::COMPONENT_FORMAT _componentFormat ) {
	ImageUtility::ImagesMatrix^	result = gcnew ImageUtility::ImagesMatrix();
	reinterpret_cast< ImageUtilityLib::ImagesMatrix* >( result->NativeObject.ToPointer() )->DDSCompress( *reinterpret_cast< ImageUtilityLib::ImagesMatrix* >( _sourceImage->NativeObject.ToPointer() ), ImageUtilityLib::ImagesMatrix::COMPRESSION_TYPE( _compressionType ), BaseLib::COMPONENT_FORMAT( _componentFormat ), reinterpret_cast< void* >( &m_pDevice->DXDevice() ) );
	return result;
}

UInt32	Renderer::Device::AdapterOutput::AdapterIndex::get() {
	return m_pAdapterOutput->m_adapterIndex;
}
UInt32	Renderer::Device::AdapterOutput::OutputIndex::get() {
	return m_pAdapterOutput->m_outputIndex;
}
System::IntPtr	Renderer::Device::AdapterOutput::MonitorHandle::get() {
	return System::IntPtr( m_pAdapterOutput->m_outputMonitor );
}
System::Drawing::Rectangle	Renderer::Device::AdapterOutput::Rectangle::get() {
	System::Drawing::Rectangle	result( m_pAdapterOutput->m_outputRectangle.left, m_pAdapterOutput->m_outputRectangle.top,
										1 + m_pAdapterOutput->m_outputRectangle.right - m_pAdapterOutput->m_outputRectangle.left, 1 + m_pAdapterOutput->m_outputRectangle.bottom - m_pAdapterOutput->m_outputRectangle.top );
	return result;
}
Renderer::Device::AdapterOutput::ROTATION	Renderer::Device::AdapterOutput::Rotation::get() {
	return (ROTATION) m_pAdapterOutput->m_outputRotation;
}
