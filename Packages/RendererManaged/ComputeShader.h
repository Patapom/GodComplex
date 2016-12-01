// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"
#include "ShaderFile.h"

using namespace System;

namespace Renderer {

	public ref class ComputeShader {
	private:

		::ComputeShader*	m_pShader;

	public:

		ComputeShader( Device^ _device, ShaderFile^ _shaderFile, String^ _entryPoint, cli::array<ShaderMacro^>^ _macros ) {
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFile->m_shaderFileName->FullName ).ToPointer();
			const char*	ShaderCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFile->m_shaderSourceCode ).ToPointer();
			const char*	EntryPoint = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPoint ).ToPointer();

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

			m_pShader = new ::ComputeShader( *_device->m_pDevice, ShaderFileName, ShaderCode, macros, EntryPoint, NULL );

			delete[] macros;
		}
		ComputeShader( Device^ _device, System::IO::FileInfo^ _shaderFileName, String^ _entryPoint ) {
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFileName->FullName ).ToPointer();
			const char*	EntryPoint = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPoint ).ToPointer();

			m_pShader = ::ComputeShader::CreateFromBinaryBlob( *_device->m_pDevice, ShaderFileName, NULL, EntryPoint );
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
