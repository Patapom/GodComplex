// This is the main DLL file.

#include "stdafx.h"

#include "Shader.h"

bool	Renderer::Shader::LoadFromBinary::get() {
	return ShaderCompiler::ms_loadFromBinary;
}
void	Renderer::Shader::LoadFromBinary::set( bool value ) {
	ShaderCompiler::ms_loadFromBinary = value;
}

bool	Renderer::Shader::WarningAsError::get() {
	return ::ShaderCompiler::ms_warningsAsError;
}
void	Renderer::Shader::WarningAsError::set( bool value ) {
	::ShaderCompiler::ms_warningsAsError = value;
}

bool	Renderer::Shader::AssertOnSaveBinaryBlobFailed::get() {
	return ::ShaderCompiler::ms_assertOnSaveBinaryBlobFailed;
}
void	Renderer::Shader::AssertOnSaveBinaryBlobFailed::set( bool value ) {
	::ShaderCompiler::ms_assertOnSaveBinaryBlobFailed = value;
}

void	Renderer::Shader::Init( Device^ _device, System::IO::FileInfo^ _shaderFileName, VERTEX_FORMAT _format, String^ _entryPointVS, String^ _entryPointGS, String^ _entryPointPS, FileServer^ _fileServerOverride, cli::array<ShaderMacro^>^ _macros ) {
	const char*	shaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFileName->FullName ).ToPointer();
	const char*	entryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointVS ).ToPointer();
	const char*	entryPointHS = NULL;	//TODO?
	const char*	entryPointDS = NULL;	//TODO?
	const char*	entryPointGS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointGS ).ToPointer();
	const char*	entryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointPS ).ToPointer();

// 	const char*	shaderSourceCode = nullptr;
// 	if ( !Shader::LoadFromBinary ) {
// 		if ( !_shaderFileName->Exists )
// 			throw gcnew Exception( "Shader file \"" + _shaderFileName + "\" does not exist!" );
// 
// 		StreamReader^	R = _shaderFileName->OpenText();
// 		String^	stringShaderSourceCode = R->ReadToEnd();
// 		delete R;
// 
// 		shaderSourceCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( stringShaderSourceCode ).ToPointer();
// 	}

	D3D_SHADER_MACRO*	pMacros = NULL;
	if ( _macros != nullptr && _macros->Length > 0 ) {
		int i=0;
		pMacros = new D3D_SHADER_MACRO[_macros->Length + 1];
		for ( ; i < _macros->Length; i++ ) {
			pMacros[i].Name = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _macros[i]->name ).ToPointer();
			pMacros[i].Definition = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _macros[i]->value ).ToPointer();

		}
		pMacros[i].Name = NULL;
		pMacros[i].Definition = NULL;
	}

	IVertexFormatDescriptor*	descriptor = GetDescriptor( _format );

	m_pShader = new ::Shader( *_device->m_pDevice, shaderFileName, *descriptor, pMacros, entryPointVS, entryPointHS, entryPointDS, entryPointGS, entryPointPS, _fileServerOverride != nullptr ? _fileServerOverride->m_server : NULL );

	delete[] pMacros;
}
