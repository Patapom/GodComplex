// This is the main DLL file.

#include "stdafx.h"

#include "NodeMesh.h"
#include "Scene.h"

using namespace	FBXImporter;

NodeMesh::NodeMesh( Scene^ _ParentScene, Node^ _Parent, FbxNode* _pNode ) : NodeWithAttribute( _ParentScene, _Parent, _pNode )
{
	FbxMesh*	pMesh = _pNode->GetMesh();

	pMesh->ComputeBBox();				// Compute the bounding box

	m_BBox = gcnew WMath::BoundingBox( Helpers::ToPoint3( pMesh->BBoxMin.Get() ), Helpers::ToPoint3( pMesh->BBoxMax.Get() ) );
	m_PolygonsCount = pMesh->GetPolygonCount();


	//////////////////////////////////////////////////////////////////////////
	// Build the array of vertices

	// Doesn't work this stuff ! It splits all triangles and creates 3*TrianglesCount vertices, what a lousy piece of shit !
// int	VertexCountBefore = pMesh->GetControlPointsCount();
// //			pMesh->SplitPoints( FbxLayerElement::eDIFFUSE_TEXTURES );
// 			pMesh->SplitPoints( FbxLayerElement::eSMOOTHING );
// int	VertexCountAfter = pMesh->GetControlPointsCount();
// 
//	pMesh->ComputeVertexNormals();	// Compute the vertex normals

	FbxVector4*	pControlPoints = pMesh->GetControlPoints();
	if ( pControlPoints == NULL )
		throw gcnew Exception( "List of control points for mesh \"" + Name + "\" is not initialized!" );

	m_Vertices = gcnew cli::array<WMath::Point^>( pMesh->GetControlPointsCount() );

	switch ( m_ParentScene->UpAxis )
	{
	case Scene::UP_AXIS::X:
		throw gcnew Exception( "X as Up Axis is not supported!" );
		break;

	case Scene::UP_AXIS::Y:
		for ( int VertexIndex=0; VertexIndex < pMesh->GetControlPointsCount(); VertexIndex++ )
			m_Vertices[VertexIndex] = gcnew WMath::Point( (float) pControlPoints[VertexIndex][0], (float) pControlPoints[VertexIndex][2], -(float) pControlPoints[VertexIndex][1] );
		break;

	case Scene::UP_AXIS::Z:
		for ( int VertexIndex=0; VertexIndex < pMesh->GetControlPointsCount(); VertexIndex++ )
			m_Vertices[VertexIndex] = gcnew WMath::Point( (float) pControlPoints[VertexIndex][0], (float) pControlPoints[VertexIndex][1], (float) pControlPoints[VertexIndex][2] );
		break;
	}


	//////////////////////////////////////////////////////////////////////////
	// Build the array of faces
	List<Triangle^>^	Triangles = gcnew List<Triangle^>();
	List<int>^			PolygonVertexOffsets = gcnew List<int>();

	int	PolygonVertexOffset = 0;
	for ( int PolygonIndex=0; PolygonIndex < pMesh->GetPolygonCount(); PolygonIndex++ )
	{
		// We convert polygons into triangles assuming they are CONVEX !
		// (I don't intend to support concave polygon splitting any time soon!)
		// (If that bothers people, they should simply convert to triangle meshes before exporting)
		// (sorry but that's how it is)
		int		PolySize = pMesh->GetPolygonSize( PolygonIndex );
		for ( int TriangleIndex=0; TriangleIndex < PolySize-2; TriangleIndex++ )
		{
			Triangle^	T = gcnew Triangle(	pMesh->GetPolygonVertex( PolygonIndex, 0 ),
											pMesh->GetPolygonVertex( PolygonIndex, 1 + TriangleIndex ),
											pMesh->GetPolygonVertex( PolygonIndex, 2 + TriangleIndex ),

											// Cumulated polygon indices to address BY_POLYGON_VERTEX mapped infos directly in the layer elements
											PolygonVertexOffset + 0,
											PolygonVertexOffset + 1 + TriangleIndex,
											PolygonVertexOffset + 2 + TriangleIndex,

											// Store the polygon index, and the polygon vertex offset (so we can subtract it to the above polygon offsets to retrieve the original polygon vertex index)
											PolygonIndex,
											PolygonVertexOffset
										  );

			Triangles->Add( T );
		}

		PolygonVertexOffsets->Add( PolygonVertexOffset );
		PolygonVertexOffset += PolySize;
	}

	m_Triangles = Triangles->ToArray();
	m_PolygonVertexOffsets = PolygonVertexOffsets->ToArray();
	m_PolygonVerticesCount = PolygonVertexOffset;


	//////////////////////////////////////////////////////////////////////////
	// Build layers referencing the vertices
	m_Layers = gcnew List<Layer^>();
	for ( int LayerIndex=0; LayerIndex < pMesh->GetLayerCount(); LayerIndex++ )
	{
		Layer^	L = gcnew Layer( this, pMesh->GetLayer( LayerIndex ) );
		m_Layers->Add( L );
	}


	//////////////////////////////////////////////////////////////////////////
	// Cache pivot
	//
	FbxAMatrix	Pivot;
	pMesh->GetPivot( Pivot );

	// FBX Usually exports the pivot as the identity matrix, we need the actual geometric pivot!
	// It seems we can retrieve it from the "GeometricXxXxX" properties...

// 	return Helpers::ToMatrix( Pivot );


	//////////////////////////////////////////////////////////////////////////
	// Retrieve the PRS values
	//
	ObjectProperty^	PropP = FindProperty( "GeometricTranslation" );
	WMath::Point^	Trans = PropP != nullptr ? (WMath::Point^) (WMath::Vector^) PropP->Value : gcnew WMath::Point( 0.0f, 0.0f, 0.0f );

	ObjectProperty^	PropR = FindProperty( "GeometricRotation" );
	WMath::Vector^	RotXYZ = (float) Math::PI / 180.0f * (PropR != nullptr ? (WMath::Vector^) PropR->Value : gcnew WMath::Vector( 0.0f, 0.0f, 0.0f ));

	ObjectProperty^	PropS = FindProperty( "GeometricScaling" );
	WMath::Vector^	Scale = PropS != nullptr ? (WMath::Vector^) PropS->Value : gcnew WMath::Vector( 1.0f, 1.0f, 1.0f );


	//////////////////////////////////////////////////////////////////////////
	// Build the pivot matrix
	//
	m_Pivot = gcnew WMath::Matrix4x4();
	m_Pivot->MakeIdentity();

	switch ( m_ParentScene->UpAxis )
	{
		case Scene::UP_AXIS::X:
			throw gcnew Exception( "X as Up Axis is not supported!" );
			break;

		case Scene::UP_AXIS::Y:
		{
			WMath::Matrix3x3^	RotPYR = gcnew WMath::Matrix3x3();
								RotPYR->FromEuler( gcnew WMath::Vector( RotXYZ->x, RotXYZ->z, -RotXYZ->y ) );

			m_Pivot->SetRotation( RotPYR );
			m_Pivot->Scale( gcnew WMath::Vector( Scale->x, Scale->z, Scale->y ) );
			m_Pivot->SetTrans( gcnew WMath::Point( Trans->x, Trans->z, -Trans->y ) );
			break;
		}

		case Scene::UP_AXIS::Z:
		{
			WMath::Matrix3x3^	RotPYR = gcnew WMath::Matrix3x3();
								RotPYR->FromEuler( RotXYZ );

			m_Pivot->SetRotation( RotPYR );
			m_Pivot->Scale( Scale );
			m_Pivot->SetTrans( Trans );
			break;
		}
	}
}

int	NodeMesh::GetControlPointIndex( int _TriangleIndex, int _TriangleVertexIndex )
{
	switch ( _TriangleVertexIndex )
	{
	case	0:
		return	m_Triangles[_TriangleIndex]->Vertex0;
	case	1:
		return	m_Triangles[_TriangleIndex]->Vertex1;
	case	2:
		return	m_Triangles[_TriangleIndex]->Vertex2;
	}
	
	throw gcnew Exception( "Triangle vertex index out of range!" );
}

// int	NodeMesh::GetAbsolutePolygonVertexIndex( int _PolygonIndex, int _PolygonVertexIndex )
// {
// 	return	m_PolygonVertexOffsets[_PolygonIndex] + _PolygonVertexIndex;
// }
