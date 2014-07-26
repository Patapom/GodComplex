// ByteBuffer.h

#pragma once
//#include "Device.h"

using namespace System;
using namespace System::IO;

namespace RendererManaged {

	public ref class ByteBuffer
	{
	internal:

		cli::array<System::Byte>^	m_Buffer;
		System::IO::MemoryStream^	m_Stream;

	public:

		ByteBuffer( int _ContentSize )
		{
			m_Buffer = gcnew array<System::Byte>( _ContentSize );
		}
		~ByteBuffer()
		{
			delete m_Stream;
			delete m_Buffer;
		}

		System::IO::BinaryReader^	OpenStreamRead()
		{
			if ( m_Stream != nullptr )
				throw gcnew Exception( "Stream is already opened!" );

			m_Stream = gcnew System::IO::MemoryStream( m_Buffer, false );
			return gcnew System::IO::BinaryReader( m_Stream );
		}

		System::IO::BinaryWriter^	OpenStreamWrite()
		{
			if ( m_Stream != nullptr )
				throw gcnew Exception( "Stream is already opened!" );

			m_Stream = gcnew System::IO::MemoryStream( m_Buffer, true );
			return gcnew System::IO::BinaryWriter( m_Stream );
		}

		void	CloseStream()
		{
			if ( m_Stream == nullptr )
				throw gcnew Exception( "Stream is not opened!" );

			delete m_Stream;
		}
	};
}
