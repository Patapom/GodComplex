// RendererManaged.h

#pragma once
#include "Device.h"

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
			if ( _ShaderFileName == nullptr || !_ShaderFileName->Exists )
				throw gcnew Exception( "Shader file \"" + _ShaderFileName + "\" does not exist!" );

			m_ShaderFileName = _ShaderFileName;

			StreamReader^	R = _ShaderFileName->OpenText();
			m_ShaderSourceCode = R->ReadToEnd();
			delete R;
		}
	};

	public ref class ShaderBinaryFile
	{
	public:

		FileInfo^			m_ShaderFileName;
		cli::array<Byte>^	m_ShaderBlob;

	public:

		ShaderBinaryFile( FileInfo^ _ShaderFileName )
		{
			if ( _ShaderFileName == nullptr || !_ShaderFileName->Exists )
				throw gcnew Exception( "Shader file \"" + _ShaderFileName + "\" does not exist!" );

			m_ShaderFileName = _ShaderFileName;

			FileStream^	R = _ShaderFileName->OpenRead();

			m_ShaderBlob = gcnew cli::array<Byte>( (int) R->Length );
			R->Read( m_ShaderBlob, 0, (int) R->Length );
			delete R;
		}
	};
}
