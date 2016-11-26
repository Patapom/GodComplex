// RendererManaged.h

#pragma once

#include "Device.h"

using namespace System;
using namespace System::IO;

namespace Renderer {

	// Wraps an existing shader file and its source code, used by the Shader class for compiling the shader
	// 
	public ref class ShaderFile {
	public:

		FileInfo^	m_shaderFileName;
		String^		m_shaderSourceCode;

	public:

		ShaderFile( FileInfo^ _ShaderFileName ) {
			if ( _ShaderFileName == nullptr || !_ShaderFileName->Exists )
				throw gcnew Exception( "Shader file \"" + _ShaderFileName + "\" does not exist!" );

			m_shaderFileName = _ShaderFileName;

			StreamReader^	R = _ShaderFileName->OpenText();
			m_shaderSourceCode = R->ReadToEnd();
			delete R;
		}
	};

	// Wraps an existing binary shader file and its compiled byte code, used by the Shader class for using the shader
	// 
	public ref class ShaderBinaryFile {
	public:

		FileInfo^			m_shaderFileName;
		cli::array<Byte>^	m_shaderBlob;

	public:

		ShaderBinaryFile( FileInfo^ _ShaderFileName ) {
			if ( _ShaderFileName == nullptr || !_ShaderFileName->Exists )
				throw gcnew Exception( "Shader file \"" + _ShaderFileName + "\" does not exist!" );

			m_shaderFileName = _ShaderFileName;

			FileStream^	R = _ShaderFileName->OpenRead();

			m_shaderBlob = gcnew cli::array<Byte>( (int) R->Length );
			R->Read( m_shaderBlob, 0, (int) R->Length );
			delete R;
		}
	};
}
