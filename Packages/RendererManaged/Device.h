// RendererManaged.h
#pragma once

#pragma unmanaged
#include "../../RendererD3D11/Device.h"
#include "../../RendererD3D11/Components/Texture2D.h"
#include "../../RendererD3D11/Components/Texture3D.h"
#include "../../RendererD3D11/Components/StructuredBuffer.h"
#include "../../RendererD3D11/Components/Shader.h"
#include "../../RendererD3D11/Components/ComputeShader.h"
#include "../../RendererD3D11/Components/ConstantBuffer.h"
#include "../../RendererD3D11/Components/Primitive.h"
#include "../../RendererD3D11/Components/States.h"

#include "../../RendererD3D11/Structures/PixelFormats.h"
#include "../../RendererD3D11/Structures/VertexFormats.h"
#pragma managed

#include "RenderStates.h"
#include "MathStructs.h"

using namespace System;

namespace RendererManaged {

	ref class Shader;
	ref class Texture2D;
	ref class Texture3D;

	// Texture view interface
	public interface class	IView
	{
	public:
		virtual property int							Width				{ int get() = 0; }
		virtual property int							Height				{ int get() = 0; }
		virtual property int							ArraySizeOrDepth	{ int get() = 0; }
		virtual property ::ID3D11ShaderResourceView*	SRV					{ ::ID3D11ShaderResourceView*	get() = 0; }
		virtual property ::ID3D11RenderTargetView*		RTV					{ ::ID3D11RenderTargetView*		get() = 0; }
		virtual property ::ID3D11UnorderedAccessView*	UAV					{ ::ID3D11UnorderedAccessView*	get() = 0; }
		virtual property ::ID3D11DepthStencilView*		DSV					{ ::ID3D11DepthStencilView*		get() = 0; }
	};

	// Main device
	public ref class Device
	{
	internal:

		::Device*		m_pDevice;
 		Texture2D^		m_DefaultTarget;
 		Texture2D^		m_DefaultDepthStencil;

		// We offer the ever useful fullscreen quad
		::Primitive*	m_pQuad;

	public:

		property Texture2D^		DefaultTarget
		{
			Texture2D^	get() { return m_DefaultTarget; }
		}

		property Texture2D^		DefaultDepthStencil
		{
			Texture2D^	get() { return m_DefaultDepthStencil; }
		}

		Device()
		{
			m_pDevice = new ::Device();
 			m_DefaultTarget = nullptr;
 			m_DefaultDepthStencil = nullptr;
		}
		~Device()
		{
//			delete m_pQuad;

			m_pDevice->Exit();
			delete m_pDevice;
			m_pDevice = NULL;
		}

 		void	Init( System::IntPtr _WindowHandle, bool _FullScreen, bool _sRGBRenderTarget );
		void	Exit() {
			m_pDevice->Exit();
		}

		void	Clear( RendererManaged::float4 _ClearColor );
		void	Clear( Texture2D^ _RenderTarget, RendererManaged::float4 _ClearColor );
		void	Clear( Texture3D^ _RenderTarget, RendererManaged::float4 _ClearColor );
		void	ClearDepthStencil( Texture2D^ _RenderTarget, float _Z, byte _Stencil, bool _ClearDepth, bool _ClearStencil );

		void	SetRenderStates( RASTERIZER_STATE _RS, DEPTHSTENCIL_STATE _DS, BLEND_STATE _BS );
		void	SetRenderTarget( Texture2D^ _RenderTarget, Texture2D^ _DepthStencilTarget );
		void	SetRenderTargets( int _Width, int _Height, cli::array<IView^>^ _RenderTargetViews, Texture2D^ _DepthStencilTarget );

		void	RemoveRenderTargets()	{ m_pDevice->RemoveRenderTargets(); }
		void	RemoveUAVs()			{ m_pDevice->RemoveUAVs(); }

		void	RenderFullscreenQuad( Shader^ _Shader );

		void	Present( bool _FlushCommands )
		{
			if ( _FlushCommands )
				m_pDevice->DXContext().Flush();

			m_pDevice->DXSwapChain().Present( 0, 0 );
		}

		void	ReloadModifiedShaders()
		{
			// Reload modified shaders
			::Shader::WatchShadersModifications();
			::ComputeShader::WatchShadersModifications();
		}
	};
}
