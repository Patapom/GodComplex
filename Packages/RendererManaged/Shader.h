// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"
#include "VertexFormats.h"

using namespace System;

namespace Renderer {

	// Wraps a simple compiled shader
	//
	public ref class Shader {
	internal:

		::Shader*	m_pShader;

	public:

		// A flag you can set to force loading from binary files without having to write a specific code for that
		// Use the helper class ScopedForceMaterialsLoadFromBinary below
		static property bool	LoadFromBinary {
			bool	get() { return ::Shader::ms_LoadFromBinary; }
			void	set( bool value ) { ::Shader::ms_LoadFromBinary = value; }
		}

		// A flag you can set to treat warnings as errors
		static property bool	WarningAsError {
			bool	get() { return ::Shader::ms_warningsAsError; }
			void	set( bool value ) { ::Shader::ms_warningsAsError = value; }
		}

	public:

		Shader( Device^ _device, System::IO::FileInfo^ _shaderFileName, VERTEX_FORMAT _format, String^ _entryPointVS, String^ _entryPointGS, String^ _entryPointPS, cli::array<ShaderMacro^>^ _macros ) {
			const char*	shaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFileName->FullName ).ToPointer();
			const char*	entryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointVS ).ToPointer();
			const char*	entryPointHS = NULL;	//TODO?
			const char*	entryPointDS = NULL;	//TODO?
			const char*	entryPointGS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointGS ).ToPointer();
			const char*	entryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointPS ).ToPointer();

			const char*	shaderSourceCode = nullptr;
			if ( !Shader::LoadFromBinary ) {
				if ( !_shaderFileName->Exists )
					throw gcnew Exception( "Shader file \"" + _shaderFileName + "\" does not exist!" );

				StreamReader^	R = _shaderFileName->OpenText();
				String^	stringShaderSourceCode = R->ReadToEnd();
				delete R;

				shaderSourceCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( stringShaderSourceCode ).ToPointer();
			}

			D3D_SHADER_MACRO*	pMacros = NULL;
			if ( _macros != nullptr ) {
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

			m_pShader = new ::Shader( *_device->m_pDevice, shaderFileName, *descriptor, shaderSourceCode, pMacros, entryPointVS, entryPointHS, entryPointDS, entryPointGS, entryPointPS, NULL );

			delete[] pMacros;
		}

		~Shader() {
			delete m_pShader;
		}

		// Must be called prior calling Render()!
		bool	Use() {
			return m_pShader != nullptr ? m_pShader->Use() : false;
		}
	};

	public ref class	ScopedForceMaterialsLoadFromBinary {
		::ScopedForceMaterialsLoadFromBinary*	m_nativeObject;
	public:
		ScopedForceMaterialsLoadFromBinary() { m_nativeObject = new ::ScopedForceMaterialsLoadFromBinary(); }
		~ScopedForceMaterialsLoadFromBinary() { delete m_nativeObject; }
	};
}
