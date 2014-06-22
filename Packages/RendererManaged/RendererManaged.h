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
#pragma managed

using namespace System;

namespace RendererManaged {

	public ref class Device
	{
	internal:

		::Device*		m_pDevice;

	public:

		Device()
		{
			m_pDevice = new ::Device();
		}
		~Device()
		{
			m_pDevice->Exit();
			delete m_pDevice;
		}

		void	Init( System::IntPtr _WindowHandle, int _WindowWidth, int _WindowHeight, bool _FullScreen, bool _sRGBRenderTarget )
		{
			m_pDevice->Exit();
			m_pDevice->Init( _WindowWidth, _WindowHeight, (HWND) _WindowHandle.ToInt32(), _FullScreen, _sRGBRenderTarget );
		}

		void	Exit()
		{
			m_pDevice->Exit();
		}

		void	Clear( System::Drawing::Color _ClearColor )
		{
			m_pDevice->ClearRenderTarget( m_pDevice->DefaultRenderTarget(), ::float4( _ClearColor.R / 255.0f, _ClearColor.G / 255.0f, _ClearColor.B / 255.0f, 1.0f ) );
		}

		void	Present()
		{
			m_pDevice->DXSwapChain().Present( 0, 0 );
		}
	};
}
