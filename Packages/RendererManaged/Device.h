// RendererManaged.h

#pragma once

#pragma unmanaged
#include "../../RendererD3D11/Device.h"
#include "../../RendererD3D11/Components/Texture2D.h"
#include "../../RendererD3D11/Components/Texture3D.h"
#include "../../RendererD3D11/Components/StructuredBuffer.h"
#include "../../RendererD3D11/Components/Material.h"
#include "../../RendererD3D11/Components/ComputeShader.h"
#include "../../RendererD3D11/Components/ConstantBuffer.h"
#include "../../RendererD3D11/Components/Primitive.h"
#include "../../RendererD3D11/Components/States.h"

#include "../../RendererD3D11/Structures/PixelFormats.h"
#include "../../RendererD3D11/Structures/VertexFormats.h"
#pragma managed

#include "RenderStates.h"

using namespace System;

namespace RendererManaged {

	ref class Texture2D;
	ref class Shader;

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
			delete m_pQuad;

			m_pDevice->Exit();
			delete m_pDevice;
			m_pDevice = NULL;
		}

 		void	Init( System::IntPtr _WindowHandle, int _WindowWidth, int _WindowHeight, bool _FullScreen, bool _sRGBRenderTarget );

		void	Exit()
		{
			m_pDevice->Exit();
		}

		void	Clear( System::Drawing::Color _ClearColor )
		{
			m_pDevice->ClearRenderTarget( m_pDevice->DefaultRenderTarget(), ::float4( _ClearColor.R / 255.0f, _ClearColor.G / 255.0f, _ClearColor.B / 255.0f, 1.0f ) );
		}

		void	SetRenderStates( RASTERIZER_STATE _RS, DEPTHSTENCIL_STATE _DS, BLEND_STATE _BS );
		void	SetRenderTarget( Texture2D^ _RenderTarget, Texture2D^ _DepthStencilTarget );
		void	RenderFullscreenQuad( Shader^ _Shader );

		void	Present()
		{
			m_pDevice->DXSwapChain().Present( 0, 0 );
		}
	};
}
