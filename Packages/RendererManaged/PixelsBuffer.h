// PixelsBuffer.h
#pragma once


#pragma unmanaged
#include "../../RendererD3D11/Device.h"
#pragma managed

#include "ByteBuffer.h"

using namespace System;
using namespace System::IO;

namespace RendererManaged {

	public ref class PixelsBuffer : public ByteBuffer
	{
	internal:

		int							m_RowPitch;
		int							m_DepthPitch;

	public:

		property int		RowPitch	{ int get() { return m_RowPitch; } }
		property int		DepthPitch	{ int get() { return m_DepthPitch; } }

	public:

		PixelsBuffer( int _ContentSize ) : ByteBuffer( _ContentSize )
		{
		}

	internal:
		PixelsBuffer( D3D11_MAPPED_SUBRESOURCE& _SubResource ) : ByteBuffer( _SubResource.DepthPitch )
		{
			m_RowPitch = _SubResource.RowPitch;
			m_DepthPitch = _SubResource.DepthPitch;

			System::Runtime::InteropServices::Marshal::Copy( System::IntPtr( _SubResource.pData ), m_Buffer, 0, m_DepthPitch );
		}
	};
}
