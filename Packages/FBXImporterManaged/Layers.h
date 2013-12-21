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

				// eUnknown = 0,
				// 
				// //Non-Texture layer element types
				// //Note: Make sure to update static index below if you change this enum!
				// eNormal = 1,
				// eBiNormal = 2,
				// eTangent = 3,
				// eMaterial = 4,
				// ePolygonGroup = 5,
				// eUV = 6,
				// eVertexColor = 7,
				// eSmoothing = 8,
				// eVertexCrease = 9,
				// eEdgeCrease = 10,
				// eHole = 11,
				// eUserData = 12,
				// eVisibility = 13,


				FbxLayerElement*	pElement = _pLayer->GetLayerElementOfType( ElementType );
				if ( pElement == NULL )
					continue;	// Non-existing element...

				const char*	pLayerElementName = pElement->GetName();

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
