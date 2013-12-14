// Contains the Mesh node class
//
#pragma managed
#pragma once

#include "Helpers.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;

namespace FBXImporter
{
	ref class	Layer;
	ref class	Triangle;

	//////////////////////////////////////////////////////////////////////////
	// The base layer element class hosting the actual informations about the geometry
	// A layer element is typically an array of data (bool, int, vector2, vector3, color, etc.)
	//	that is mapped in a specific way to the owner mesh's polygons
	// This is used to generically encode Positions, Normals, UV Sets, Vertex Colors and so on.
	//
	[System::Diagnostics::DebuggerDisplayAttribute( "Name={Name} Type={ElementType} Mapping={MappingType} Index={Index}" )]
	public ref class	LayerElement
	{
	public:		// NESTED TYPES

		enum class	ELEMENT_TYPE
		{
			UNDEFINED,
			NORMAL,
			BINORMAL,
			TANGENT,
			MATERIAL,
			POLYGON_GROUP,
			UV,
			VERTEX_COLOR,
			SMOOTHING,
			VERTEX_CREASE,
			EDGE_CREASE,
			HOLE,
			USER_DATA,
			VISIBILITY,

			DIFFUSE_TEXTURES,
			EMISSIVE_TEXTURES,
			EMISSIVE_FACTOR_TEXTURES,
			AMBIENT_TEXTURES,
			AMBIENT_FACTOR_TEXTURES,
			DIFFUSE_FACTOR_TEXTURES,
			SPECULAR_TEXTURES,
			NORMALMAP_TEXTURES,
			SPECULAR_FACTOR_TEXTURES,
			SHININESS_TEXTURES,
			BUMP_TEXTURES,
			TRANSPARENT_TEXTURES,
			TRANSPARENCY_FACTOR_TEXTURES,
			REFLECTION_TEXTURES,
			REFLECTION_FACTOR_TEXTURES,
			DISPLACEMENT_TEXTURES,

			// Custom element type, not in the FBX SDK
			POSITION,
		};

		// Determines how the element is mapped on a surface.
		// - NONE                  The mapping is undetermined.
		// - BY_CONTROL_POINT      There will be one mapping coordinate for each surface control point/vertex.
		// - BY_TRIANGLE_VERTEX    There will be one mapping coordinate for each vertex, for each triangle it is part of.
		// 							This means that a vertex will have as many mapping coordinates as triangles it is part of.
		// - BY_TRIANGLE           There can be only one mapping coordinate for the whole triangle.
		// - BY_EDGE               There will be one mapping coordinate for each unique edge in the mesh.
		// 							This is meant to be used with smoothing layer elements.
		// - ALL_SAME              There can be only one mapping coordinate for the whole surface.
		//
		enum class	MAPPING_TYPE
		{
			NONE,
			BY_CONTROL_POINT,
			BY_TRIANGLE_VERTEX,
			BY_TRIANGLE,
			BY_EDGE,			// Not supported !
			ALL_SAME,
		};

		// Determines how the mapping information is stored in the array of coordinates.
		//  - DIRECT              This indicates that the mapping information for the n'th element is found in the n'th place of 
		// 							FbxLayerElementTemplate::mDirectArray.
		//  - INDEX,              This symbol is kept for backward compatibility with FBX v5.0 files. In FBX v6.0 and higher, 
		// 							this symbol is replaced with eINDEX_TO_DIRECT.
		//  - INDEX_TO_DIRECT     This indicates that the FbxLayerElementTemplate::mIndexArray
		// 							contains, for the n'th element, an index in the FbxLayerElementTemplate::mDirectArray
		// 							array of mapping elements. eINDEX_TO_DIRECT is usually useful to store coordinates
		// 							for eBY_TRIANGLE_VERTEX mapping mode elements. Since the same coordinates are usually
		// 							repeated a large number of times, it saves spaces to store the coordinate only one time
		// 							and refer to them with an index. Materials and Textures are also referenced with this
		// 							mode and the actual Material/Texture can be accessed via the FbxLayerElementTemplate::mDirectArray
		//
		enum class	REFERENCE_TYPE
		{
			DIRECT,
			INDEX,
			INDEX_TO_DIRECT
		};

	protected:	// FIELDS

		Layer^					m_Owner;

		String^					m_Name;

		ELEMENT_TYPE			m_ElementType;
		MAPPING_TYPE			m_MappingMode;
		REFERENCE_TYPE			m_ReferenceMode;

		int						m_Index;		// The semantic index of this layer element (e.g. UV Set #0 => Index=0, UV Set #1 => Index=1, etc.)

		// Cached array conversion
		cli::array<Object^>^	m_CachedArray;


	public:		// PROPERTIES

		property ELEMENT_TYPE	ElementType
		{
			ELEMENT_TYPE	get()		{ return m_ElementType; }
			void			set( ELEMENT_TYPE _Value )	{ m_ElementType = _Value; }
		}

		property String^		Name
		{
			String^			get()		{ return m_Name; }
		}

		property MAPPING_TYPE	MappingType
		{
			MAPPING_TYPE	get()		{ return m_MappingMode; }
			void			set( MAPPING_TYPE _Value )	{ m_MappingMode = _Value; }
		}

		property REFERENCE_TYPE	ReferenceType
		{
			REFERENCE_TYPE	get()		{ return m_ReferenceMode; }
			void			set( REFERENCE_TYPE _Value )	{ m_ReferenceMode = _Value; }
		}

		property int			Index
		{
			int				get()		{ return m_Index; }
		}
		

	public:		// METHODS

		LayerElement( Layer^ _Owner, FbxLayerElement* _pLayerElement, FbxLayerElement::EType _ElementType ) : m_Owner( _Owner ), m_CachedArray( nullptr )
		{
			m_Name = Helpers::GetString( _pLayerElement->GetName() );
			m_ElementType = static_cast<ELEMENT_TYPE>( _ElementType );
			m_MappingMode = static_cast<MAPPING_TYPE>( _pLayerElement->GetMappingMode() );
			m_ReferenceMode = static_cast<REFERENCE_TYPE>( _pLayerElement->GetReferenceMode() );

			// Try and determine the element's index based on its name
			m_Index = 0;	// Default is 0
			System::Text::RegularExpressions::Regex^	RX = gcnew System::Text::RegularExpressions::Regex( ".*_(\\d*)", System::Text::RegularExpressions::RegexOptions::IgnoreCase | System::Text::RegularExpressions::RegexOptions::Singleline );
			System::Text::RegularExpressions::Match^	Match = RX->Match( Name );
			if ( Match != nullptr && Match->Groups->Count == 2 )
				if ( Int32::TryParse( Match->Groups[1]->Value, m_Index ) )
					m_Index--;	// Index naming convention starts at one!

			// Cache the layer element array
			BuildArray( _pLayerElement );
		}

		// This constructor is used for custom creation of a layer element (i.e. procedural meshes)
		LayerElement( String^ _Name, ELEMENT_TYPE _ElementType, MAPPING_TYPE _MappingMode, int _SemanticIndex ) : m_Owner( nullptr ), m_CachedArray( nullptr )
		{
			m_Name = _Name;
			m_ElementType = _ElementType;
			m_MappingMode = _MappingMode;
			m_Index = _SemanticIndex;
		}

		// Sets the array of collapsed data
		// NOTE: You must understand the layer element format and provide the correct array as if returned by the ToArray() method!
		//
		void	SetArrayOfData( cli::array<Object^>^ _Array )
		{
			m_CachedArray = _Array;
		}

		// Converts the layer element to an array, given the mesh's triangles
		// Depending on the mapping type, the returned array will contain :
		//	_ As many elements as control points if mapped as "BY_CONTROL_POINT"
		//	_ 3 * the amount of triangles if mapped as "BY_TRIANGLE_VERTEX" (assuming we only support triangles)
		//	_ As many as triangles if mapped as "BY_TRIANGLE"
		//	_ 1 element if mapped as "ALL_SAME"
		//
		// Note: the mapping type "BY_EDGE" is not supported and will throw an exception!
		//
		// Also, depending on the ELEMENT_TYPE, the returned objects in the array will be :
		//	_ Points for POSITION
		//	_ Vectors for NORMAL, BINORMAL, TANGENT and UVW
		//	_ Vector2D for UV
		//	_ int for MATERIAL
		//	_ uint32 for SMOOTHING
		//	_ Vector4D for VERTEX_COLOR
		//
		// Note: any other type is not supported
		//
		cli::array<Object^>^		ToArray()	{ return m_CachedArray; }

		// Compares 2 layers elements and returns true if they are equal
		bool	Compare( LayerElement^ _Other );


		// Gets the element for the requested triangle vertex
		// NOTE: This will alwayscorrectly return the element for a given polygon vertex, despite of the mapping type.
		//
		//	This means it will return always the same value if mapping is ALL_SAME, will return the same value
		//	for all vertices of a same polygon if mapping is BY_TRIANGLE, will return a common value for all
		//	vertices referencing the same control point if mapping is BY_CONTROL_POINT, etc.
		//
		Object^			GetElementByTriangleVertex( int _TriangleIndex, int _TriangleVertexIndex );

	protected:
		
		void			BuildArray( FbxLayerElement* _pLayerElement );

		// Gets the layer element's element at the given index
		Object^			GetElementAt( FbxLayerElement* _pLayerElement, int _Index );

	};
}
