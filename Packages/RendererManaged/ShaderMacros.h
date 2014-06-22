// RendererManaged.h

#pragma once
#include "RendererManaged.h"

using namespace System;

namespace RendererManaged {

	public ref class ShaderMacros
	{
	public:

		ref class	Macro
		{
		public:
			String^		Name;
			String^		Value;
		};

		cli::array< Macro^ >^	m_Macros;

	public:

		ShaderMacros( cli::array< Macro^ >^ _Macros )
		{
			m_Macros = _Macros;
		}
	};
}
