// RendererManaged.h

#pragma once

#include "Device.h"

using namespace System;
using namespace SharpMath;

namespace Renderer {

	// Wraps a structured buffer
	//
	generic<typename T> public ref class StructuredBuffer {
	private:

		::StructuredBuffer*	m_pStructuredBuffer;
		Object^				m_tag;

	public:
		cli::array<T>^		m;

		property Object^	Tag { Object^ get() { return m_tag; } void set( Object^ _value ) { m_tag = _value; } }

	public:

		StructuredBuffer( Device^ _device, UInt32 _elementsCount, bool _writeable ) {
			Init( _device, _elementsCount, _writeable );
		}

		~StructuredBuffer() {
			delete m_pStructuredBuffer;
		}

		void	Init(  Device^ _device, UInt32 _elementsCount, bool _writeable ) {
			m = gcnew array<T>( _elementsCount );
			m_pStructuredBuffer = new ::StructuredBuffer( *_device->m_pDevice, System::Runtime::InteropServices::Marshal::SizeOf( T::typeid ), _elementsCount, _writeable );
		}

		void	Read() { Read( ~0U ); }
		void	Read( UInt32 _elementsCount ) {
			pin_ptr<T>	ptr = &m[0];
			m_pStructuredBuffer->Read( ptr, _elementsCount );
		}
		void	Write() { Write( ~0U ); }
		void	Write( UInt32 _elementsCount ) {
			pin_ptr<T>	ptr = &m[0];
			m_pStructuredBuffer->Write( ptr, _elementsCount );
		}
		void	Clear( float4 _Value ) {
			bfloat4	value( _Value.x, _Value.y, _Value.z, _Value.w );
			m_pStructuredBuffer->Clear( value );
		}
		void	SetInput( int _SlotIndex ) {
			m_pStructuredBuffer->SetInput( _SlotIndex );
		}
		void	SetOutput( int _SlotIndex )		{ m_pStructuredBuffer->SetOutput( _SlotIndex ); }
 		void	RemoveFromLastAssignedSlots()	{ m_pStructuredBuffer->RemoveFromLastAssignedSlots(); }
	};
}
