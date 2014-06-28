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

		::Material*	m_pShader;

	public:

		Shader( Device^ _Device, ShaderFile^ _ShaderFile, VERTEX_FORMAT _Format, String^ _EntryPointVS, String^ _EntryPointPS, ShaderMacros^ _Macros )
		{
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderFileName->FullName ).ToPointer();
			const char*	ShaderCode = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFile->m_ShaderSourceCode ).ToPointer();
			const char*	EntryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointVS ).ToPointer();
			const char*	EntryPointHS = NULL;	//TODO?
			const char*	EntryPointDS = NULL;	//TODO?
			const char*	EntryPointGS = NULL;	//TODO?
			const char*	EntryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointPS ).ToPointer();


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

			IVertexFormatDescriptor*	pDescriptor = NULL;
			switch ( _Format )
			{
			case VERTEX_FORMAT::Pt4:	pDescriptor = &VertexFormatPt4::DESCRIPTOR; break;
			}
			if ( pDescriptor == NULL )
				throw gcnew Exception( "Unsupported vertex format!" );

			m_pShader = new ::Material( *_Device->m_pDevice, ShaderFileName, *pDescriptor, ShaderCode, pMacros, EntryPointVS, EntryPointHS, EntryPointDS, EntryPointGS, EntryPointPS, NULL );

			delete[] pMacros;
		}

		~Shader()
		{
			delete m_pShader;
		}

		static Shader^	CreateFromBinaryBlob( Device^ _Device, VERTEX_FORMAT _Format, FileInfo^ _ShaderFileName, String^ _EntryPointVS, String^ _EntryPointPS )
		{
			const char*	ShaderFileName = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _ShaderFileName->FullName ).ToPointer();
			const char*	EntryPointVS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointVS ).ToPointer();
			const char*	EntryPointHS = NULL;	//TODO?
			const char*	EntryPointDS = NULL;	//TODO?
			const char*	EntryPointGS = NULL;	//TODO?
			const char*	EntryPointPS = (const char*) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi( _EntryPointPS ).ToPointer();

			IVertexFormatDescriptor*	pDescriptor = NULL;
			switch ( _Format )
			{
			case VERTEX_FORMAT::Pt4:	pDescriptor = &VertexFormatPt4::DESCRIPTOR; break;
			}
			if ( pDescriptor == NULL )
				throw gcnew Exception( "Unsupported vertex format!" );

			::Material*	pShader = ::Material::CreateFromBinaryBlob( *_Device->m_pDevice, ShaderFileName, *pDescriptor, EntryPointVS, EntryPointHS, EntryPointDS, EntryPointGS, EntryPointPS );

			return gcnew Shader( _Device, pShader );
		}

	private:
		Shader( Device^ _Device, ::Material* _pShader )
		{
			m_pShader = _pShader;
		}
	};
}
