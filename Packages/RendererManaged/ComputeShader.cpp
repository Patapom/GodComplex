// This is the main DLL file.

#include "stdafx.h"

#include "ComputeShader.h"

void	Renderer::ComputeShader::Init( Device^ _device, System::IO::FileInfo^ _shaderFileName, String^ _entryPoint, cli::array<ShaderMacro^>^ _macros, FileServer^ _fileServerOverride ) {
	const char*	shaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFileName->FullName ).ToPointer();
	const char*	entryPoint = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPoint ).ToPointer();

	D3D_SHADER_MACRO*	macros = NULL;
	if ( _macros != nullptr ) {
		int i=0;
		macros = new D3D_SHADER_MACRO[_macros->Length + 1];
		for ( ; i < _macros->Length; i++ ) {
			macros[i].Name = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _macros[i]->name ).ToPointer();
			macros[i].Definition = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _macros[i]->value ).ToPointer();

		}
		macros[i].Name = NULL;
		macros[i].Definition = NULL;
	}

	m_pShader = new ::ComputeShader( *_device->m_pDevice, shaderFileName, macros, entryPoint, _fileServerOverride != nullptr ? _fileServerOverride->m_server : NULL );

	delete[] macros;
}
