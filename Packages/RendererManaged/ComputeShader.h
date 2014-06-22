// RendererManaged.h

#pragma once
#include "RendererManaged.h"
#include "ShaderMacros.h"
#include "ShaderFile.h"

using namespace System;

namespace RendererManaged {

	public ref class ComputeShader
	{
	private:

		::ComputeShader*	m_pShader;

	public:

		ComputeShader( Device^ _Device, ShaderFile^ _ShaderFile, String^ _EntryPoint, ShaderMacros^ _Macros )
		{
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderFileName->FullName ).ToPointer();
			const char*	ShaderCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderSourceCode ).ToPointer();
			const char*	EntryPoint = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPoint ).ToPointer();

			D3D_SHADER_MACRO*	pMacros = NULL;
			if ( _Macros != nullptr )
			{
				int i=0;
				pMacros = new D3D_SHADER_MACRO[_Macros->m_Macros->Length + 1];
				for ( ; i < _Macros->m_Macros->Length; i++ )
				{
					pMacros[i].Name = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _Macros->m_Macros[i]->Name ).ToPointer();
					pMacros[i].Definition = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _Macros->m_Macros[i]->Value ).ToPointer();

				}
				pMacros[i].Name = NULL;
				pMacros[i].Definition = NULL;
			}

			m_pShader = new ::ComputeShader( *_Device->m_pDevice, ShaderFileName, ShaderCode, pMacros, EntryPoint, NULL );

			delete[] pMacros;
		}

		~ComputeShader()
		{
			delete m_pShader;
		}
	};
}
