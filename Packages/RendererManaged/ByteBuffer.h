// ByteBuffer.h

#pragma once
//#include "Device.h"

using namespace System;
using namespace System::IO;

namespace Renderer {

	public ref class ByteBuffer {
	internal:

		cli::array<System::Byte>^	m_Buffer;
		System::IO::MemoryStream^	m_Stream;

	public:

		ByteBuffer( UInt32 _ContentSize, UInt32 _clearValue ) {
			m_Buffer = gcnew array<System::Byte>( _ContentSize );
			if ( _clearValue == 0 )
				return;
			UInt32	count = _ContentSize >> 2;
			byte	A = (_clearValue >> 24) & 0xFF;
			byte	R = (_clearValue >> 16) & 0xFF;
			byte	G = (_clearValue >> 8) & 0xFF;
			byte	B = (_clearValue >> 0) & 0xFF;
			UInt32	offset = 0;
			for ( UInt32 i=0; i < count; i++ ) {
				m_Buffer[offset++] = B;
				m_Buffer[offset++] = G;
				m_Buffer[offset++] = R;
				m_Buffer[offset++] = A;
			}
			switch ( _ContentSize & 3 ) {
			case 3:
				m_Buffer[offset++] = B;
			case 2:
				m_Buffer[offset++] = G;
			case 1:
				m_Buffer[offset++] = R;
			}
		}
		~ByteBuffer() {
			delete m_Stream;
			delete m_Buffer;
		}

		System::IO::BinaryReader^	OpenStreamRead() {
			if ( m_Stream != nullptr )
				throw gcnew Exception( "Stream is already opened!" );

			m_Stream = gcnew System::IO::MemoryStream( m_Buffer, false );
			return gcnew System::IO::BinaryReader( m_Stream );
		}

		System::IO::BinaryWriter^	OpenStreamWrite() {
			if ( m_Stream != nullptr )
				throw gcnew Exception( "Stream is already opened!" );

			m_Stream = gcnew System::IO::MemoryStream( m_Buffer, true );
			return gcnew System::IO::BinaryWriter( m_Stream );
		}

		void	CloseStream() {
			if ( m_Stream == nullptr )
				throw gcnew Exception( "Stream is not opened!" );

			delete m_Stream;
			m_Stream = nullptr;
		}
	};
}
