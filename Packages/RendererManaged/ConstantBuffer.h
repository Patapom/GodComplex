// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace RendererManaged {

	generic<typename T> public ref class ConstantBuffer
	{
	private:

		int					m_SlotIndex;
		::ConstantBuffer*	m_pConstantBuffer;

	public:
		T					m;

	public:

		ConstantBuffer( Device^ _Device, int _SlotIndex )
		{
			m_SlotIndex = _SlotIndex;
			m_pConstantBuffer = new ::ConstantBuffer( *_Device->m_pDevice, System::Runtime::InteropServices::Marshal::SizeOf( T::typeid ) );
		}

		~ConstantBuffer()
		{
			delete m_pConstantBuffer;
		}

		void	UpdateData()
		{
			cli::pin_ptr<T>	Bisou = &m;
 				m_pConstantBuffer->UpdateData( Bisou );

			m_pConstantBuffer->Set( m_SlotIndex );
		}
	};
}
