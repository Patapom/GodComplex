// RendererManaged.h

#pragma once
#include "RendererManaged.h"

using namespace System;
using namespace System::IO;

namespace RendererManaged {

	public ref class ShaderFile
	{
	public:

		FileInfo^	m_ShaderFileName;
		String^		m_ShaderSourceCode;

	public:

		ShaderFile( FileInfo^ _ShaderFileName )
		{
			m_ShaderFileName = _ShaderFileName;

			StreamReader^	R = _ShaderFileName->OpenText();
			m_ShaderSourceCode = R->ReadToEnd();
			delete R;
		}
	};
}
