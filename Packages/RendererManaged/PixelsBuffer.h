// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;
using namespace System::IO;

namespace RendererManaged {

	public ref class PixelsBuffer
	{
	internal:

		cli::array<System::Byte>^	m_Buffer;
		System::IO::MemoryStream^	m_Stream;

		int							m_RowPitch;
		int							m_DepthPitch;

	public:

		property int		RowPitch	{ int get() { return m_RowPitch; } }
		property int		DepthPitch	{ int get() { return m_DepthPitch; } }

	public:

		PixelsBuffer( int _ContentSize )
		{
			m_Buffer = gcnew array<System::Byte>( _ContentSize );
		}
		~PixelsBuffer()
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

	internal:
		PixelsBuffer( D3D11_MAPPED_SUBRESOURCE& _SubResource )
		{
			m_RowPitch = _SubResource.RowPitch;
			m_DepthPitch = _SubResource.DepthPitch;

			m_Buffer = gcnew array<System::Byte>( _SubResource.DepthPitch );
			System::Runtime::InteropServices::Marshal::Copy( System::IntPtr( _SubResource.pData ), m_Buffer, 0, m_DepthPitch );
		}
	};
}
