// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"
#include "VertexFormats.h"
#include "FileServer.h"

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
			bool	get();// { return ShaderCompiler::ms_LoadFromBinary; }
			void	set( bool value );// { ShaderCompiler::ms_LoadFromBinary = value; }
		}

		// A flag you can set to treat warnings as errors
		static property bool	WarningAsError {
			bool	get();// { return ::ShaderCompiler::ms_warningsAsError; }
			void	set( bool value );// { ::ShaderCompiler::ms_warningsAsError = value; }
		}

	public:

		Shader( Device^ _device, System::IO::FileInfo^ _shaderFileName, VERTEX_FORMAT _format, String^ _entryPointVS, String^ _entryPointGS, String^ _entryPointPS, cli::array<ShaderMacro^>^ _macros ) {
			Init( _device, _shaderFileName, _format, _entryPointVS, _entryPointGS, _entryPointPS, _macros, nullptr );
		}

		Shader( Device^ _device, System::IO::FileInfo^ _shaderFileName, VERTEX_FORMAT _format, String^ _entryPointVS, String^ _entryPointGS, String^ _entryPointPS, cli::array<ShaderMacro^>^ _macros, FileServer^ _fileServerOverride ) {
			Init( _device, _shaderFileName, _format, _entryPointVS, _entryPointGS, _entryPointPS, _macros, _fileServerOverride );
		}

		~Shader() {
			delete m_pShader;
		}

		// Must be called prior calling Render()!
		bool	Use() {
			return m_pShader != nullptr ? m_pShader->Use() : false;
		}

	private:
		void	Init( Device^ _device, System::IO::FileInfo^ _shaderFileName, VERTEX_FORMAT _format, String^ _entryPointVS, String^ _entryPointGS, String^ _entryPointPS, cli::array<ShaderMacro^>^ _macros, FileServer^ _fileServerOverride );
	};

	public ref class	ScopedForceShadersLoadFromBinary {
		::ScopedForceShadersLoadFromBinary*	m_nativeObject;
	public:
		ScopedForceShadersLoadFromBinary() { m_nativeObject = new ::ScopedForceShadersLoadFromBinary(); }
		~ScopedForceShadersLoadFromBinary() { delete m_nativeObject; }
	};
}
