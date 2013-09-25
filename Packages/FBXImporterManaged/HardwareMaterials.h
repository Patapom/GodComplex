// Contains Material classes
//
#pragma managed
#pragma once

#include "Helpers.h"
#include "Materials.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;


namespace FBXImporter
{
	//////////////////////////////////////////////////////////////////////////
	// Represents the base class for a hardware material (HLSL or CGFX)
	//
	public ref class		MaterialHardwareShader : public Material
	{
	public:		// NESTED TYPES

		ref class	TableEntry : public ObjectProperty
		{
		protected:	// FIELDS

			const FbxBindingTableEntry*	m_pEntry;

		public:		// METHODS

			TableEntry( MaterialHardwareShader^ _Owner, const FbxBindingTableEntry& _Entry, FbxProperty& _Property ) : ObjectProperty( _Owner, _Property ), m_pEntry( &_Entry )
			{
			}
		};


	protected:	// FIELDS

		String^						m_RelativeURL;
		String^						m_TechniqueName;
		String^						m_CodeRelativeURL;
		String^						m_CodeEntryTag;

		cli::array<TableEntry^>^	m_Entries;

	public:		// PROPERTIES

		[DescriptionAttribute( "Relative URL of file containing the shader implementation description" )]
		// 
		property String^		RelativeURL
		{
			String^		get()	{ return m_RelativeURL; }
		}

		[DescriptionAttribute( "Identifies the shader to use in previous decription's URL" )]
		//
		property String^		TechniqueName
		{
			String^		get()	{ return m_TechniqueName; }
		}

		[DescriptionAttribute( "Relative URL of file containing the shader implementation code" )]
		//
		property String^		CodeRelativeURL
		{
			String^		get()	{ return m_CodeRelativeURL; }
		}

		[DescriptionAttribute( "Identifies the shader function entry to use in previous code's URL" )]
		//
		property String^		CodeEntryTag
		{
			String^		get()	{ return m_CodeEntryTag; }
		}

		[DescriptionAttribute( "Gets the list of shader entries (i.e. shader parameters)" )]
		//
		property cli::array<TableEntry^>^	ShaderEntries
		{
			cli::array<TableEntry^>^	get()	{ return m_Entries; }
		}


	public:		// METHODS

		MaterialHardwareShader( Scene^ _ParentScene, FbxSurfaceMaterial* _pMaterial, const FbxImplementation* _pImplementation ) : Material( _ParentScene, _pMaterial )
		{
			const FbxBindingTable*		pRootTable = _pImplementation->GetRootTable();

			m_RelativeURL = Helpers::GetString( pRootTable->DescRelativeURL.Get() );
			m_TechniqueName = Helpers::GetString( pRootTable->DescTAG.Get() );
			m_CodeRelativeURL = Helpers::GetString( pRootTable->CodeRelativeURL.Get() );
			m_CodeEntryTag = Helpers::GetString( pRootTable->CodeTAG.Get() );


			// Build the table entries
			List<TableEntry^>^	Entries = gcnew List<TableEntry^>();
			for ( int EntryIndex=0; EntryIndex < (int) pRootTable->GetEntryCount(); EntryIndex++ )
			{
				const FbxBindingTableEntry&	Entry = pRootTable->GetEntry( EntryIndex );

				// Retrieve the property for that entry
				const char* pEntrySrcType = Entry.GetEntryType( true );

				FbxProperty	FbxProp;
				if ( strcmp( FbxPropertyEntryView::sEntryType, pEntrySrcType ) == 0 )
				{   
					FbxProp = _pMaterial->FindPropertyHierarchical( Entry.GetSource() );
					if( !FbxProp.IsValid() )
						FbxProp = _pMaterial->RootProperty.FindHierarchical( Entry.GetSource() );
				}
				else if( strcmp( FbxConstantEntryView::sEntryType, pEntrySrcType ) == 0 )
					FbxProp = _pImplementation->GetConstants().FindHierarchical( Entry.GetSource() );

				if ( !FbxProp.IsValid() )
					continue;	// Invalid property :(

				Entries->Add( gcnew TableEntry( this, Entry, FbxProp ) );
			}

			m_Entries = Entries->ToArray();
		}
	};

	// Represents a HSLS material
	//
	public ref class		MaterialHLSL : public MaterialHardwareShader
	{
	protected:	// FIELDS


	public:		// PROPERTIES


	public:		// METHODS

		MaterialHLSL( Scene^ _ParentScene, FbxSurfaceMaterial* _pMaterial, const FbxImplementation* _pImplementation ) : MaterialHardwareShader( _ParentScene, _pMaterial, _pImplementation )
		{
		}
	};

	// Represents a CGFX Material
	//
	public ref class		MaterialCGFX : public MaterialHardwareShader
	{
	protected:	// FIELDS


	public:		// PROPERTIES


	public:		// METHODS

		MaterialCGFX( Scene^ _ParentScene, FbxSurfaceMaterial* _pMaterial, const FbxImplementation* _pImplementation ) : MaterialHardwareShader( _ParentScene, _pMaterial, _pImplementation )
		{
		}
	};
}
