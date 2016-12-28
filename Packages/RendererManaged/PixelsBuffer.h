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

		// When a texture is mapped
		D3D11_MAPPED_SUBRESOURCE*	m_mappedSubResource;
		UInt32						m_mappedMipLevelIndex;
		UInt32						m_mappedArrayIndex;
		bool						m_readOnly;

	public:

		property UInt32		RowPitch	{ UInt32 get() { return m_rowPitch; } }
		property UInt32		DepthPitch	{ UInt32 get() { return m_depthPitch; } }

	public:

		PixelsBuffer( UInt32 _contentSize ) : ByteBuffer( _contentSize ), m_mappedSubResource( NULL ) {
		}

	internal:
		PixelsBuffer( D3D11_MAPPED_SUBRESOURCE& _SubResource, UInt32 _mipLevelIndex, UInt32 _arrayIndex, bool _readOnly ) : ByteBuffer( _SubResource.DepthPitch ) {
			m_rowPitch = _SubResource.RowPitch;
			m_depthPitch = _SubResource.DepthPitch;

			m_mappedSubResource = &_SubResource;
			m_mappedMipLevelIndex = _mipLevelIndex;
			m_mappedArrayIndex = _arrayIndex;
			m_readOnly = _readOnly;

			if ( m_readOnly ) {
				// Copy mapped resource now
				System::Runtime::InteropServices::Marshal::Copy( System::IntPtr( _SubResource.pData ), m_Buffer, 0, m_depthPitch );
			}
		}

		// Copies back buffer's content to mapped sub-resources
		void	WriteToMappedSubResource() {
			System::Runtime::InteropServices::Marshal::Copy( m_Buffer, 0, System::IntPtr( m_mappedSubResource->pData ), m_depthPitch );
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
