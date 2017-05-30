// RendererManaged.h
#pragma once

#include "RenderStates.h"

using namespace System;

namespace Renderer {

	ref class Shader;
	ref class Texture2D;
	ref class Texture3D;

	// Texture view interface
	public interface class	IView {
	public:
		virtual property UInt32							Width				{ UInt32 get() = 0; }
		virtual property UInt32							Height				{ UInt32 get() = 0; }
		virtual property UInt32							ArraySizeOrDepth	{ UInt32 get() = 0; }
		virtual property ::ID3D11ShaderResourceView*	SRV					{ ::ID3D11ShaderResourceView*	get() = 0; }
		virtual property ::ID3D11RenderTargetView*		RTV					{ ::ID3D11RenderTargetView*		get() = 0; }
		virtual property ::ID3D11UnorderedAccessView*	UAV					{ ::ID3D11UnorderedAccessView*	get() = 0; }
		virtual property ::ID3D11DepthStencilView*		DSV					{ ::ID3D11DepthStencilView*		get() = 0; }
	};

	// Main device class from which everything else is derived
	public ref class Device {
	internal:

		::Device*		m_pDevice;
 		Texture2D^		m_defaultTarget;
 		Texture2D^		m_defaultDepthStencil;

		// We offer the ever useful fullscreen quad
		::Primitive*	m_pScreenQuad;

		// We keep the list of the last 8 assigned RTVs
		::ID3D11RenderTargetView**	m_ppRenderTargetViews;

	public:

		property Texture2D^		DefaultTarget {
			Texture2D^	get() { return m_defaultTarget; }
		}

		property Texture2D^		DefaultDepthStencil {
			Texture2D^	get() { return m_defaultDepthStencil; }
		}

		Device() {
			m_pDevice = new ::Device();
 			m_defaultTarget = nullptr;
 			m_defaultDepthStencil = nullptr;
			m_ppRenderTargetViews = new ::ID3D11RenderTargetView*[8];
			memset( m_ppRenderTargetViews, 0, 8*sizeof(::ID3D11RenderTargetView*) );
		}
		~Device() {
//			delete m_pQuad;
			SAFE_DELETE_ARRAY( m_ppRenderTargetViews );
			m_pDevice->Exit();
			delete m_pDevice;
			m_pDevice = NULL;
		}

 		void	Init( System::IntPtr _WindowHandle, bool _FullScreen, bool _sRGBRenderTarget );
		void	Exit() {
			m_pDevice->Exit();
		}

		void	Clear( SharpMath::float4 _clearColor );
		void	Clear( Texture2D^ _RenderTarget, SharpMath::float4 _clearColor );
		void	Clear( Texture3D^ _RenderTarget, SharpMath::float4 _clearColor );
		void	ClearDepthStencil( Texture2D^ _RenderTarget, float _Z, byte _Stencil, bool _ClearDepth, bool _ClearStencil );

		void	SetRenderStates( RASTERIZER_STATE _RS, DEPTHSTENCIL_STATE _DS, BLEND_STATE _BS );
		void	SetRenderTarget( Texture2D^ _RenderTarget, Texture2D^ _DepthStencilTarget );
		void	SetRenderTargets( cli::array<IView^>^ _RenderTargetViews, Texture2D^ _DepthStencilTarget );

		void	RemoveRenderTargets()	{ m_pDevice->RemoveRenderTargets(); }
		void	RemoveUAVs()			{ m_pDevice->RemoveUAVs(); }

		void	RenderFullscreenQuad( Shader^ _Shader );

		// Compresses the source image to the appropriate format using GPU acceleration
		ImageUtility::ImagesMatrix^	DDSCompress( ImageUtility::ImagesMatrix^ _sourceImage, ImageUtility::ImagesMatrix::COMPRESSION_TYPE _compressionType, ImageUtility::COMPONENT_FORMAT _componentFormat );

		void	Present( bool _flushCommands ) {
			if ( _flushCommands )
				m_pDevice->DXContext().Flush();

			m_pDevice->DXSwapChain().Present( 0, 0 );
		}

		void	ReloadModifiedShaders() {
			// Reload modified shaders
			::Shader::WatchShadersModifications();
			::ComputeShader::WatchShadersModifications();
		}
	};
}
