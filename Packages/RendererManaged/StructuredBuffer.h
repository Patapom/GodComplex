// RendererManaged.h

#pragma once

#include "Device.h"

using namespace System;
using namespace SharpMath;

namespace RendererManaged {

	generic<typename T> public ref class StructuredBuffer
	{
	private:

		::StructuredBuffer*	m_pStructuredBuffer;

	public:
		cli::array<T>^		m;

	public:

		StructuredBuffer( Device^ _Device, int _ElementsCount, bool _Writeable )
		{
			Init( _Device, _ElementsCount, _Writeable );
		}

		~StructuredBuffer()
		{
			delete m_pStructuredBuffer;
		}

		void	Init(  Device^ _Device, int _ElementsCount, bool _Writeable )
		{
			m = gcnew array<T>( _ElementsCount );
			m_pStructuredBuffer = new ::StructuredBuffer( *_Device->m_pDevice, System::Runtime::InteropServices::Marshal::SizeOf( T::typeid ), _ElementsCount, _Writeable );
		}

		void	Read() { Read( -1 ); }
		void	Read( int _ElementsCount )
		{
			cli::pin_ptr<T>	Bisou = &m[0];
			m_pStructuredBuffer->Read( Bisou, _ElementsCount );
		}
		void	Write() { Write( -1 ); }
		void	Write( int _ElementsCount )
		{
			cli::pin_ptr<T>	Bisou = &m[0];
			m_pStructuredBuffer->Write( Bisou, _ElementsCount );
		}
		void	Clear( float4 _Value ) {
			bfloat4	value( _Value.x, _Value.y, _Value.z, _Value.w );
			m_pStructuredBuffer->Clear( value );
		}
		void	SetInput( int _SlotIndex )
		{
			m_pStructuredBuffer->SetInput( _SlotIndex );
		}
		void	SetOutput( int _SlotIndex )		{ m_pStructuredBuffer->SetOutput( _SlotIndex ); }
 		void	RemoveFromLastAssignedSlots()	{ m_pStructuredBuffer->RemoveFromLastAssignedSlots(); }
	};
}
