// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"

using namespace System;

namespace Renderer {

	public ref class ComputeShader {
	private:

		::ComputeShader*	m_pShader;

	public:

		ComputeShader( Device^ _device, System::IO::FileInfo^ _shaderFileName, String^ _entryPoint, cli::array<ShaderMacro^>^ _macros ) {
			const char*	shaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFileName->FullName ).ToPointer();
			const char*	entryPoint = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPoint ).ToPointer();

			const char*	shaderSourceCode = nullptr;
			if ( !::Shader::ms_LoadFromBinary ) {
				if ( _shaderFileName->Exists )
					throw gcnew Exception( "Compute shader file \"" + _shaderFileName + "\" does not exist!" );

				System::IO::StreamReader^	R = _shaderFileName->OpenText();
				String^	stringShaderSourceCode = R->ReadToEnd();
				delete R;

				shaderSourceCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( stringShaderSourceCode ).ToPointer();
			}

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

			m_pShader = new ::ComputeShader( *_device->m_pDevice, shaderFileName, shaderSourceCode, macros, entryPoint, NULL );

			delete[] macros;
		}

		~ComputeShader() {
			delete m_pShader;
		}

		// Must be called prior calling Dispatch()!
		bool	Use() {
			return m_pShader->Use();
		}

		void	Dispatch( UInt32 _GroupsCountX, UInt32 _GroupsCountY, UInt32 _GroupsCountZ ) {
			m_pShader->Dispatch( _GroupsCountX, _GroupsCountY, _GroupsCountZ );
		}

	};
}
