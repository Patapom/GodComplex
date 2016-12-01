// PixelsBuffer.h
#pragma once

#pragma unmanaged
#include "../../RendererD3D11/Device.h"
#pragma managed

#include "ByteBuffer.h"

using namespace System;
using namespace System::IO;

namespace Renderer {

	public ref class PixelsBuffer : public ByteBuffer {
	internal:

		UInt32						m_RowPitch;
		UInt32						m_DepthPitch;

	public:

		property UInt32		RowPitch	{ UInt32 get() { return m_RowPitch; } }
		property UInt32		DepthPitch	{ UInt32 get() { return m_DepthPitch; } }

	public:

		PixelsBuffer( UInt32 _ContentSize ) : ByteBuffer( _ContentSize ) {
		}

	internal:
		PixelsBuffer( D3D11_MAPPED_SUBRESOURCE& _SubResource ) : ByteBuffer( _SubResource.DepthPitch ) {
			m_RowPitch = _SubResource.RowPitch;
			m_DepthPitch = _SubResource.DepthPitch;

			System::Runtime::InteropServices::Marshal::Copy( System::IntPtr( _SubResource.pData ), m_Buffer, 0, m_DepthPitch );
		}

// Enabling this helper requires to include MathStructs.h but doing so results in Device.h failing to compile!!! ô0
// 		void	FromArray( cli::array<RendererManaged::float4,2>^ _pixels ) {
// 			int	W = _pixels->GetLength( 0 );
// 			int	H = _pixels->GetLength( 1 );
// 
// 			System::IO::BinaryWriter^	Writer = OpenStreamWrite();
// 			for ( int Y=0; Y < H; Y++ )
// 				for ( int X=0; X < W; X++ ) {
// 					float4%	pixel = _pixels[X,Y];
// 					Writer->Write( pixel.x );
// 					Writer->Write( pixel.y );
// 					Writer->Write( pixel.z );
// 					Writer->Write( pixel.w );
// 				}
// 
// 			delete Writer;
// 			CloseStream();
// 		}
	};
}
