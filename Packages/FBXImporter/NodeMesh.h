// Contains the Mesh node class
//
#pragma managed
#pragma once

#include "Helpers.h"
#include "Layers.h"
#include "Nodes.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;


namespace FBXImporter
{
	//////////////////////////////////////////////////////////////////////////
	// A Mesh node
	//
	public ref class		NodeMesh : public NodeWithAttribute
	{
	public:		// NESTED TYPES

		[System::Diagnostics::DebuggerDisplayAttribute( "[{Vertex0}, {Vertex1}, {Vertex2}] PolyIndex={PolygonIndex}" )]
		ref class	Triangle
		{
		public:
			int		Vertex0;
			int		Vertex1;
			int		Vertex2;

			// Cumulated polygon indices to address BY_POLYGON_VERTEX mapped infos directly in the layer elements
			int		PolygonVertex0;
			int		PolygonVertex1;
			int		PolygonVertex2;

			// The original polygon informations
			int		PolygonIndex;
			int		PolygonVertexOffset;	// The polygon vertex offset (so we can subtract it to the above polygon vertices to retrieve the original polygon vertex index)

		public:
			Triangle( int _V0, int _V1, int _V2, int _PolygonVertex0, int _PolygonVertex1, int _PolygonVertex2, int _PolygonIndex, int _PolygonVertexOffset ) :
				Vertex0( _V0 ), Vertex1( _V1 ), Vertex2( _V2 ),
				PolygonVertex0( _PolygonVertex0 ), PolygonVertex1( _PolygonVertex1 ), PolygonVertex2( _PolygonVertex2 ),
				PolygonIndex( _PolygonIndex ), PolygonVertexOffset( _PolygonVertexOffset )
			{
			}

			// When the mesh is composed only of triangle, this constructor is easier to use...
			Triangle( int _V0, int _V1, int _V2, int _TriangleIndex ) : Vertex0( _V0 ), Vertex1( _V1 ), Vertex2( _V2 )
			{
				PolygonVertex0 = 3 * _TriangleIndex + 0;
				PolygonVertex1 = 3 * _TriangleIndex + 1;
				PolygonVertex2 = 3 * _TriangleIndex + 2;

				PolygonIndex = _TriangleIndex;
				PolygonVertexOffset = 3 * _TriangleIndex;
			}

// 			// Gets the original polygon vertices as given by the FBX
// 			int		GetPolygonVertexIndex0()	{ return PolygonVertex0 - PolygonVertexOffset; }
// 			int		GetPolygonVertexIndex1()	{ return PolygonVertex1 - PolygonVertexOffset; }
// 			int		GetPolygonVertexIndex2()	{ return PolygonVertex2 - PolygonVertexOffset; }
		};


	protected:	// FIELDS

		SharpMath::BoundingBox^		m_BBox;
		int							m_PolygonsCount;
		SharpMath::float4x4			m_Pivot;

		List<Layer^>^				m_Layers;	// The list of layers

		cli::array<Triangle^>^		m_Triangles;
		cli::array<SharpMath::float3^>^	m_Vertices;
		cli::array<int>^			m_PolygonVertexOffsets;

		int							m_PolygonVerticesCount;	// The total amount of polygon vertices

	public:		// PROPERTIES

		property SharpMath::BoundingBox^	LocalBoundingBox {
			SharpMath::BoundingBox^			get()	{ return m_BBox; }
		}

		property SharpMath::BoundingBox^	WorldBoundingBox {
			SharpMath::BoundingBox^		get() {
				SharpMath::float4x4	Mesh2World = m_Pivot * m_LocalTransform;
				SharpMath::BoundingBox^		WorldBBox = SharpMath::BoundingBox::Empty;
										WorldBBox->Grow( m_BBox, Mesh2World );

				return WorldBBox;
			}
		}

		property SharpMath::float4x4	Pivot {
			SharpMath::float4x4			get()	{ return m_Pivot; }
		}

		property cli::array<Triangle^>^		Triangles
		{
			cli::array<Triangle^>^		get()	{ return m_Triangles; }
		}

		property int						TrianglesCount
		{
			int							get()	{ return m_Triangles->Length; }
		}

		property cli::array<SharpMath::float3^>^	Vertices
		{
			cli::array<SharpMath::float3^>^	get()	{ return m_Vertices; }
		}

		property int						VerticesCount
		{
			int							get()	{ return m_Vertices->Length; }
		}

		property cli::array<Layer^>^		Layers
		{
			cli::array<Layer^>^			get()	{ return m_Layers->ToArray(); }
		}

		property int						PolygonsCount
		{
			int							get()	{ return m_PolygonsCount; }
		}

		property int						PolygonVerticesCount
		{
			int							get()	{ return m_PolygonVerticesCount; }
		}


	public:		// METHODS

		NodeMesh( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode );

		// Gets the index to a control point given the triangle and its internal index
		int		GetControlPointIndex( int _TriangleIndex, int _TriangleVertexIndex );

		// Gets the absolute index to a polygon vertex index given the polygon and its internal index
//		int		GetAbsolutePolygonVertexIndex( int _PolygonIndex, int _PolygonVertexIndex );
	};
}
