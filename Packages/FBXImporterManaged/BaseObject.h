// Contains base Object class
//
#pragma managed
#pragma once

#include "Helpers.h"
#include "ObjectProperty.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;


namespace FBXImporter
{
	ref class	Scene;

	//////////////////////////////////////////////////////////////////////////
	// Represents the base object used for all FBX objects
	//
	public ref class		BaseObject abstract
	{
	protected:	// FIELDS

		Scene^					m_ParentScene;		// Our parent scene
//		FbxObject*				m_pObject;			// The FBX object we're wrapping

		String^					m_Name;				// Node name

		List<ObjectProperty^>^	m_Properties;
		List<ObjectProperty^>^	m_UserProperties;

// 		cli::array<ObjectProperty^>^	m_Properties;
// 		cli::array<ObjectProperty^>^	m_UserProperties;


	public:		// PROPERTIES

		[BrowsableAttribute( false )]
		property Scene^		ParentScene
		{
			Scene^				get()	{ return m_ParentScene; }
		}

		[DescriptionAttribute( "Gets the name of this object" )]
		property String^	Name
		{
			String^				get()	{ return m_Name; }
		}

		[DescriptionAttribute( "Gives the list of properties tied to this object" )]
		property cli::array<ObjectProperty^>^	Properties
		{
			cli::array<ObjectProperty^>^	get()	{ return m_Properties->ToArray(); }
		}

		[DescriptionAttribute( "Gives the list of user properties tied to this object" )]
		property cli::array<ObjectProperty^>^	UserProperties
		{
			cli::array<ObjectProperty^>^	get()	{ return m_UserProperties->ToArray(); }
		}


	public:		// METHODS

		BaseObject( Scene^ _ParentScene, FbxObject* _pObject ) : m_ParentScene( _ParentScene )//, m_pObject( _pObject )
		{
			m_Name = Helpers::GetString( _pObject->GetName() );
			m_Properties = gcnew List<ObjectProperty^>();
			m_UserProperties = gcnew List<ObjectProperty^>();

			// Build the object's user properties
			AppendProperties( _pObject );
		}

		[BrowsableAttribute( false )]
		virtual String^	ToString() override
		{
			return	Name;
		}

		[DescriptionAttribute( "Tells if the given object is of the specified type" )]
		virtual bool	Is( String^ _ClassName )
		{
			System::Type^	T = System::Type::GetType( _ClassName );
			return	T!= nullptr ? T->IsAssignableFrom( this->GetType() ) : false;
		}

		[DescriptionAttribute( "List the available properties of this object" )]
		String^	ListProperties();

		[DescriptionAttribute( "List the available methods of this object" )]
		String^	ListMethods();

		[DescriptionAttribute( "Finds a property by name" )]
		virtual ObjectProperty^	FindProperty( String^ _Name )
		{
			for ( int PropertyIndex=0; PropertyIndex < m_Properties->Count; PropertyIndex++ )
				if ( m_Properties[PropertyIndex]->Name == _Name )
					return	m_Properties[PropertyIndex];

			return	nullptr;
		}

		[DescriptionAttribute( "Finds a user property by name" )]
		virtual ObjectProperty^	FindUserProperty( String^ _Name )
		{
			for ( int PropertyIndex=0; PropertyIndex < m_UserProperties->Count; PropertyIndex++ )
				if ( m_UserProperties[PropertyIndex]->Name == _Name )
					return	m_UserProperties[PropertyIndex];

			return	nullptr;
		}

		[BrowsableAttribute( false )]
		virtual int		GetHashCode() override
		{
			return	Object::GetHashCode();
		}

		[BrowsableAttribute( false )]
		virtual bool		Equals( Object^ o ) override
		{
			return	Object::Equals( o );
		}

		// Decode and append object properties to this node's properties
		void			AppendProperties( FbxObject* _pObject )
		{
			FbxProperty	Property = _pObject->GetFirstProperty();
			while ( Property.IsValid() )
			{
				ObjectProperty^	Prop = gcnew ObjectProperty( this, Property );
				m_Properties->Add( Prop );
				if ( Property.GetFlag( FbxPropertyAttr::eUser ) )
					m_UserProperties->Add( Prop );

				Property = _pObject->GetNextProperty( Property );
			}
		}
	};
}
