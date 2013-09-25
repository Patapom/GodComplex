// Contains the Mesh node class
//
#pragma managed
#pragma once

#include "LayerElements.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;

namespace FBXImporter
{
	ref class	NodeMesh;

	//////////////////////////////////////////////////////////////////////////
	// The base layer class containing several layer elements
	//
	public ref class	Layer
	{
	public:		// NESTED TYPES

	protected:	// FIELDS

		NodeMesh^				m_Owner;

		List<LayerElement^>^	m_Elements;

	public:		// PROPERTIES

		property NodeMesh^		Owner
		{
			NodeMesh^	get()	{ return m_Owner; }
		}

		property cli::array<LayerElement^>^	Elements
		{
			cli::array<LayerElement^>^	get()	{ return m_Elements->ToArray(); }
		}

	public:		// METHODS

		Layer( NodeMesh^ _Owner, FbxLayer* _pLayer ) : m_Owner( _Owner )
		{
			// Build layer elements
			m_Elements = gcnew List<LayerElement^>();
			for ( int LayerElementType=int(FbxLayerElement::eNormal); LayerElementType < FbxLayerElement::eTypeCount; LayerElementType++ )
			{
				FbxLayerElement::EType	ElementType = FbxLayerElement::EType( LayerElementType );
				FbxLayerElement*	pElement = _pLayer->GetLayerElementOfType( ElementType );
				if ( pElement == NULL )
					continue;	// Non-existing element...

				LayerElement^	LE = gcnew LayerElement( this, pElement, ElementType );
				m_Elements->Add( LE );
			}
		}

		// Adds another element to the list
		//
		void	AddElement( LayerElement^ _Element )
		{
			m_Elements->Add( _Element );
		}
	};
}
