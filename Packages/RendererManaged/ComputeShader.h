// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"
#include "FileServer.h"

using namespace System;

namespace Renderer {

	public ref class ComputeShader {
	private:

		::ComputeShader*	m_pShader;

	public:

		ComputeShader( Device^ _device, System::IO::FileInfo^ _shaderFileName, String^ _entryPoint, cli::array<ShaderMacro^>^ _macros ) {
			Init( _device, _shaderFileName, _entryPoint, _macros, nullptr );
		}

		ComputeShader( Device^ _device, System::IO::FileInfo^ _shaderFileName, String^ _entryPoint, cli::array<ShaderMacro^>^ _macros, FileServer^ _fileServerOverride ) {
			Init( _device, _shaderFileName, _entryPoint, _macros, _fileServerOverride );
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

	private:
		void	Init( Device^ _device, System::IO::FileInfo^ _shaderFileName, String^ _entryPoint, cli::array<ShaderMacro^>^ _macros, FileServer^ _fileServerOverride );
	};
}
