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

		UInt32						m_rowPitch;
		UInt32						m_depthPitch;

	public:

		property UInt32		RowPitch	{ UInt32 get() { return m_rowPitch; } }
		property UInt32		DepthPitch	{ UInt32 get() { return m_depthPitch; } }

	public:

		PixelsBuffer( UInt32 _contentSize ) : ByteBuffer( _contentSize ) {
		}

	internal:
		PixelsBuffer( D3D11_MAPPED_SUBRESOURCE& _SubResource ) : ByteBuffer( _SubResource.DepthPitch ) {
			m_rowPitch = _SubResource.RowPitch;
			m_depthPitch = _SubResource.DepthPitch;

			System::Runtime::InteropServices::Marshal::Copy( System::IntPtr( _SubResource.pData ), m_Buffer, 0, m_depthPitch );
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
