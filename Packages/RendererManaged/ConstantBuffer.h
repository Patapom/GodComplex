// RendererManaged.h

#pragma once

#include "Device.h"

using namespace System;

namespace Renderer {

	// Wraps a constant buffer used to provide uniforms to the shaders
	// Constant buffers are created with a fixed slot index since we consider they don't change from one call to another given the CB is usually assigned
	//	to a single shader at a time (i.e. PerDraw parameters), or to all shaders at once (i.e. PerView parameters)
	// It's easy to make up for this if it poses any problem by simply adding a property accessor to modify the slot index...
	///
	generic<typename T> public ref class ConstantBuffer {
	private:

		int					m_slotIndex;
		::ConstantBuffer*	m_pConstantBuffer;

	public:
		T					m;

	public:

		ConstantBuffer( Device^ _device, int _slotIndex ) {
			m_slotIndex = _slotIndex;
			m_pConstantBuffer = new ::ConstantBuffer( *_device->m_pDevice, System::Runtime::InteropServices::Marshal::SizeOf( T::typeid ) );
		}

		~ConstantBuffer() {
			delete m_pConstantBuffer;
		}

		void	UpdateData() {
			pin_ptr<T>	ptr = &m;
 			m_pConstantBuffer->UpdateData( ptr );

			m_pConstantBuffer->Set( m_slotIndex );
		}
	};
}
