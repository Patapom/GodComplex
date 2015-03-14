// RendererManaged.h

#pragma once
#include "Device.h"
#include "ShaderMacros.h"
#include "ShaderFile.h"

using namespace System;

namespace RendererManaged {

	public ref class ComputeShader
	{
	private:

		::ComputeShader*	m_pShader;

	public:

		ComputeShader( Device^ _Device, ShaderFile^ _ShaderFile, String^ _EntryPoint, cli::array<ShaderMacro^>^ _Macros )
		{
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderFileName->FullName ).ToPointer();
			const char*	ShaderCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderSourceCode ).ToPointer();
			const char*	EntryPoint = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPoint ).ToPointer();

			D3D_SHADER_MACRO*	pMacros = NULL;
			if ( _Macros != nullptr )
			{
				int i=0;
				pMacros = new D3D_SHADER_MACRO[_Macros->Length + 1];
				for ( ; i < _Macros->Length; i++ )
				{
					pMacros[i].Name = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _Macros[i]->Name ).ToPointer();
					pMacros[i].Definition = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _Macros[i]->Value ).ToPointer();

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

		void	Use()
		{
			m_pShader->Use();
		}

		void	Dispatch( int _GroupsCountX, int _GroupsCountY, int _GroupsCountZ )
		{
			m_pShader->Dispatch( _GroupsCountX, _GroupsCountY, _GroupsCountZ );
		}

		static ComputeShader^	CreateFromBinaryBlob( Device^ _Device, FileInfo^ _ShaderFileName, String^ _EntryPoint )
		{
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFileName->FullName ).ToPointer();
			const char*	EntryPoint = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPoint ).ToPointer();

			::ComputeShader*	pShader = ::ComputeShader::CreateFromBinaryBlob( *_Device->m_pDevice, ShaderFileName, NULL, EntryPoint );

			return gcnew ComputeShader( _Device, pShader );
		}

	private:
		ComputeShader( Device^ _Device, ::ComputeShader* _ComputeShader )
		{
			m_pShader = _ComputeShader;
		}
	};
}
