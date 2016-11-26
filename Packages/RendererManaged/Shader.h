// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"
#include "ShaderFile.h"
#include "VertexFormats.h"

using namespace System;

namespace Renderer {

	// Wraps a simple compiled shader
	//
	public ref class Shader {
	internal:

		::Shader*	m_pShader;

	public:

		Shader( Device^ _device, ShaderFile^ _shaderFile, VERTEX_FORMAT _format, String^ _entryPointVS, String^ _entryPointGS, String^ _entryPointPS, cli::array<ShaderMacro^>^ _macros ) {
			const char*	shaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFile->m_shaderFileName->FullName ).ToPointer();
			const char*	shaderSourceCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFile->m_shaderSourceCode ).ToPointer();
			const char*	entryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointVS ).ToPointer();
			const char*	entryPointHS = NULL;	//TODO?
			const char*	entryPointDS = NULL;	//TODO?
			const char*	entryPointGS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointGS ).ToPointer();
			const char*	entryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointPS ).ToPointer();

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

		Shader( Device^ _device, ShaderBinaryFile^ _shaderFile, VERTEX_FORMAT _format, String^ _entryPointVS, String^ _entryPointGS, String^ _entryPointPS ) {
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _shaderFile->m_shaderFileName->FullName ).ToPointer();
			const char*	entryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointVS ).ToPointer();
			const char*	entryPointHS = NULL;	//TODO?
			const char*	entryPointDS = NULL;	//TODO?
			const char*	entryPointGS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointGS ).ToPointer();
			const char*	entryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _entryPointPS ).ToPointer();

			IVertexFormatDescriptor*	descriptor = GetDescriptor( _format );

			m_pShader = ::Shader::CreateFromBinaryBlob( *_device->m_pDevice, ShaderFileName, *descriptor, NULL, entryPointVS, entryPointHS, entryPointDS, entryPointGS, entryPointPS );
		}

		~Shader() {
			delete m_pShader;
		}

		// Must be called prior calling Render()!
		bool	Use() {
			return m_pShader != nullptr ? m_pShader->Use() : false;
		}
	};
}
