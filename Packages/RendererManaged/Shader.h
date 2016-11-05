// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"
#include "ShaderFile.h"
#include "VertexFormats.h"

using namespace System;

namespace RendererManaged {

	public ref class Shader
	{
	internal:

		::Shader*	m_pShader;

	public:

		Shader( Device^ _Device, ShaderFile^ _ShaderFile, VERTEX_FORMAT _Format, String^ _EntryPointVS, String^ _EntryPointGS, String^ _EntryPointPS, cli::array<ShaderMacro^>^ _Macros )
		{
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderFileName->FullName ).ToPointer();
			const char*	ShaderCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderSourceCode ).ToPointer();
			const char*	EntryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointVS ).ToPointer();
			const char*	EntryPointHS = NULL;	//TODO?
			const char*	EntryPointDS = NULL;	//TODO?
			const char*	EntryPointGS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointGS ).ToPointer();
			const char*	EntryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointPS ).ToPointer();

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

			IVertexFormatDescriptor*	pDescriptor = GetDescriptor( _Format );

			m_pShader = new ::Shader( *_Device->m_pDevice, ShaderFileName, *pDescriptor, ShaderCode, pMacros, EntryPointVS, EntryPointHS, EntryPointDS, EntryPointGS, EntryPointPS, NULL );

			delete[] pMacros;
		}

		~Shader()
		{
			delete m_pShader;
		}

		bool	Use()
		{
			return m_pShader != nullptr ? m_pShader->Use() : false;
		}

		static Shader^	CreateFromBinaryBlob( Device^ _Device, FileInfo^ _ShaderFileName, VERTEX_FORMAT _Format, String^ _EntryPointVS, String^ _EntryPointGS, String^ _EntryPointPS )
		{
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFileName->FullName ).ToPointer();
			const char*	EntryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointVS ).ToPointer();
			const char*	EntryPointHS = NULL;	//TODO?
			const char*	EntryPointDS = NULL;	//TODO?
			const char*	EntryPointGS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointGS ).ToPointer();
			const char*	EntryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointPS ).ToPointer();

			IVertexFormatDescriptor*	pDescriptor = NULL;
			switch ( _Format )
			{
			case VERTEX_FORMAT::Pt4:	pDescriptor = &VertexFormatPt4::DESCRIPTOR; break;
			}
			if ( pDescriptor == NULL )
				throw gcnew Exception( "Unsupported vertex format!" );

			::Shader*	pShader = ::Shader::CreateFromBinaryBlob( *_Device->m_pDevice, ShaderFileName, *pDescriptor, NULL, EntryPointVS, EntryPointHS, EntryPointDS, EntryPointGS, EntryPointPS );

			return gcnew Shader( _Device, pShader );
		}

	private:
		Shader( Device^ _Device, ::Shader* _pShader )
		{
			m_pShader = _pShader;
		}
	};
}
