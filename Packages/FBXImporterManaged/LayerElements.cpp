// This is the main DLL file.

#include "stdafx.h"

#include "LayerElements.h"
#include "Layers.h"
#include "NodeMesh.h"
#include "Scene.h"

using namespace FBXImporter;

void	LayerElement::BuildArray( FbxLayerElement* _pLayerElement )
{
	m_CachedArray = nullptr;

	//////////////////////////////////////////////////////////////////////////
	// Build the array first
	int		ElementsCount = 0;
	switch ( MappingType )
	{
	case MAPPING_TYPE::BY_CONTROL_POINT:
		ElementsCount = m_Owner->Owner->VerticesCount;
		break;

	case MAPPING_TYPE::BY_TRIANGLE:
		ElementsCount = m_Owner->Owner->TrianglesCount;
		break;

	case MAPPING_TYPE::BY_TRIANGLE_VERTEX:
		ElementsCount = 3 * m_Owner->Owner->Triangles->Length;
		break;

	case MAPPING_TYPE::ALL_SAME:
		ElementsCount = 1;
		break;

	case MAPPING_TYPE::BY_EDGE:
//		throw gcnew Exception( "Mapping type \"BY_EDGE\" is not supported!" );
		return;
	}
	m_CachedArray = gcnew cli::array<Object^>( ElementsCount );

	//////////////////////////////////////////////////////////////////////////
	// Fill it up with the appropriate objects
	switch ( MappingType )
	{
	case MAPPING_TYPE::BY_CONTROL_POINT:
	case MAPPING_TYPE::ALL_SAME:
		for ( int ElementIndex=0; ElementIndex < ElementsCount; ElementIndex++ )
			m_CachedArray[ElementIndex] = GetElementAt( _pLayerElement, ElementIndex );
		break;

	case MAPPING_TYPE::BY_TRIANGLE:
		{	// Here, we must remap polygons data to triangles data
			int	ElementIndex = 0;
			for ( int FaceIndex=0; FaceIndex < m_Owner->Owner->Triangles->Length; FaceIndex++ )
			{
				NodeMesh::Triangle^	Face = m_Owner->Owner->Triangles[FaceIndex];

				m_CachedArray[ElementIndex++] = GetElementAt( _pLayerElement, Face->PolygonIndex );
			}
		}
		break;

	case MAPPING_TYPE::BY_TRIANGLE_VERTEX:
		{	// Here, we must remap polygons data to triangles data
			int	ElementIndex = 0;
			for ( int FaceIndex=0; FaceIndex < m_Owner->Owner->Triangles->Length; FaceIndex++ )
			{
				NodeMesh::Triangle^	Face = m_Owner->Owner->Triangles[FaceIndex];

				m_CachedArray[ElementIndex++] = GetElementAt( _pLayerElement, Face->PolygonVertex0 );
				m_CachedArray[ElementIndex++] = GetElementAt( _pLayerElement, Face->PolygonVertex1 );
				m_CachedArray[ElementIndex++] = GetElementAt( _pLayerElement, Face->PolygonVertex2 );
			}
		}
		break;
	}
}

Object^		LayerElement::GetElementByTriangleVertex( int _TriangleIndex, int _TriangleVertexIndex )
{
// 	switch ( m_MappingMode )
// 	{
// 	case MAPPING_TYPE::BY_CONTROL_POINT:
//		return	GetElementAt( m_Owner->Owner->GetControlPointIndex( _TriangleIndex, _TriangleVertexIndex ) );
// 
// 	case MAPPING_TYPE::BY_TRIANGLE:
// 		return	GetElementAt( _TriangleIndex );
// 
// 	case MAPPING_TYPE::BY_TRIANGLE_VERTEX:
// 		return	GetElementAt( 3 * _TriangleIndex + _TriangleVertexIndex );
// 
// 	case MAPPING_TYPE::ALL_SAME:
// 		return	GetElementAt( 0 );
// 
// 	case MAPPING_TYPE::BY_EDGE:
// 		throw gcnew Exception( "Mapping type \"BY_EDGE\" is not supported!" );
// 	}

	switch ( m_MappingMode )
	{
	case MAPPING_TYPE::BY_CONTROL_POINT:
		return	m_CachedArray[m_Owner->Owner->GetControlPointIndex( _TriangleIndex, _TriangleVertexIndex )];

	case MAPPING_TYPE::BY_TRIANGLE:
		return	m_CachedArray[_TriangleIndex];

	case MAPPING_TYPE::BY_TRIANGLE_VERTEX:
		return	m_CachedArray[3 * _TriangleIndex + _TriangleVertexIndex];

	case MAPPING_TYPE::ALL_SAME:
		return	m_CachedArray[0];

	case MAPPING_TYPE::BY_EDGE:
		throw gcnew Exception( "Mapping type \"BY_EDGE\" is not supported!" );
	}

	return	nullptr;
}


// Macro to get an element at the specified index (supports direct and by index addressing)
//
#define GET_ELEMENT( _pLayerElement, _Index, _Call )						\
	if ( ReferenceType == REFERENCE_TYPE::DIRECT )							\
		Result = _Call( _pLayerElement->GetDirectArray().GetAt( _Index ) );	\
	else if (  ReferenceType == REFERENCE_TYPE::INDEX						\
			|| ReferenceType == REFERENCE_TYPE::INDEX_TO_DIRECT )			\
	{																		\
		int id = _pLayerElement->GetIndexArray().GetAt( _Index );			\
		Result = _Call( _pLayerElement->GetDirectArray().GetAt( id ) );		\
	}

Object^	LayerElement::GetElementAt( FbxLayerElement* _pLayerElement, int _Index )
{
	FbxLayerElementTemplate<int>*			pElementInt = NULL;
	FbxLayerElementTemplate<FbxVector2>*	pElementVector2 = NULL;
	FbxLayerElementTemplate<FbxVector4>*	pElementVector4 = NULL;
	FbxLayerElementTemplate<FbxColor>*		pElementColor = NULL;
	FbxLayerElementMaterial*				pElementMaterial = NULL;

	Scene::UP_AXIS	UpAxis = m_Owner->Owner->ParentScene->UpAxis;

	Object^	Result = nullptr;

	switch ( m_ElementType )
	{
	// VECTORS
	case	ELEMENT_TYPE::NORMAL:
	case	ELEMENT_TYPE::TANGENT:
	case	ELEMENT_TYPE::BINORMAL:
		pElementVector4 = static_cast<FbxLayerElementTemplate<FbxVector4>*>( _pLayerElement );
		GET_ELEMENT( pElementVector4, _Index, Helpers::ToVector4 )

		switch ( UpAxis )
		{
		case Scene::UP_AXIS::X:
			throw gcnew Exception( "X as Up Axis is not supported!" );
			break;
		case Scene::UP_AXIS::Y:
			{
				WMath::Vector4D^	Temp = dynamic_cast<WMath::Vector4D^>( Result );
				Result = gcnew WMath::Vector( Temp->x, Temp->z, -Temp->y );
			}
			break;
		case Scene::UP_AXIS::Z:
			{
				WMath::Vector4D^	Temp = dynamic_cast<WMath::Vector4D^>( Result );
				Result = gcnew WMath::Vector( Temp->x, Temp->y, Temp->z );
			}
			break;
		}

		break;

	case	ELEMENT_TYPE::UV:
		pElementVector2 = static_cast<FbxLayerElementTemplate<FbxVector2>*>( _pLayerElement );
		GET_ELEMENT( pElementVector2, _Index, Helpers::ToVector2 )
		break;

	// INTs
	case	ELEMENT_TYPE::SMOOTHING:
		pElementInt = static_cast<FbxLayerElementTemplate<int>*>( _pLayerElement );
		GET_ELEMENT( pElementInt, _Index, int )
		break;

	// VECTOR4D's
	case	ELEMENT_TYPE::VERTEX_COLOR:
		pElementColor = static_cast<FbxLayerElementTemplate<FbxColor>*>( _pLayerElement );
		GET_ELEMENT( pElementColor, _Index, Helpers::ToVector4 )
		break;

	// MATERIALs
	case	ELEMENT_TYPE::MATERIAL:
		pElementMaterial = static_cast<FbxLayerElementMaterial*>( _pLayerElement );

		// For materials, direct mapping is obsolete, only material indices are supported
		if ( ReferenceType == REFERENCE_TYPE::DIRECT )
			throw gcnew Exception( "Materials mapped with DIRECT mode are not supported anymore! Are you using the latest FBX exporter version ?" );

		int	MaterialIndex = pElementMaterial->GetIndexArray().GetAt( _Index );
		return	m_Owner->Owner->ResolveMaterial( MaterialIndex );
	}

	if ( Result == nullptr )
		throw gcnew Exception( "Unsupported Element Type \"" + ElementType.ToString() + "\" ! " );

	return	Result;
}

bool	LayerElement::Compare( LayerElement^ _Other )
{
	if ( _Other == nullptr )
		return	false;

	// 1] Compare types, mapping modes, reference modes & index (fast!)
	if ( m_ElementType != _Other->m_ElementType )
		return	false;	// Not of the same type...
	if ( m_MappingMode != _Other->m_MappingMode )
		return	false;	// Not the same mapping mode...
	if ( m_ReferenceMode != _Other->m_ReferenceMode )
		return	false;	// Not the same reference mode...
	if ( m_Index != _Other->m_Index )
		return	false;	// Not the same index...

	// 2] Compare the array elements one by one
	cli::array<Object^>^	Array0 = ToArray();
	cli::array<Object^>^	Array1 = _Other->ToArray();

	if ( Array0->Length != Array1->Length )
		return	false;	// Not the same amount of elements...
	if ( Array0->Length == 0 )
		return	true;	// 2 equal empty arrays...

	Type^	T = Array0[0]->GetType();
	if ( T != Array1[0]->GetType() )
		return	false;	// Not the same kind of objects... (quite improbable because it shouldn't have passed the ElementType test otherwise!)

	for ( int ElementIndex=0; ElementIndex < Array0->Length; ElementIndex++ )
	{
		Object^	Object0 = Array0[ElementIndex];
		Object^	Object1 = Array1[ElementIndex];

		if ( WMath::Point::typeid->IsAssignableFrom( T ) )
		{
			if ( ((WMath::Point^) Object0) != ((WMath::Point^) Object1) )
				return	false;	// Different !
		}
		else if ( WMath::Vector::typeid->IsAssignableFrom( T ) )
		{
			if ( ((WMath::Vector^) Object0) != ((WMath::Vector^) Object1) )
				return	false;	// Different !
		}
		else if ( WMath::Vector2D::typeid->IsAssignableFrom( T ) )
		{
			if ( ((WMath::Vector2D^) Object0) != ((WMath::Vector2D^) Object1) )
				return	false;	// Different !
		}
		else if ( WMath::Vector4D::typeid->IsAssignableFrom( T ) )
		{
			if ( ((WMath::Vector4D^) Object0) != ((WMath::Vector4D^) Object1) )
				return	false;	// Different !
		}
		else if ( bool::typeid->IsAssignableFrom( T ) )
		{
			if ( (bool) Object0 != (bool) Object1 )
				return	false;	// Different !
		}
		else if ( int::typeid->IsAssignableFrom( T ) )
		{
			if ( (int) Object0 != (int) Object1 )
				return	false;	// Different !
		}
		else if ( float::typeid->IsAssignableFrom( T ) )
		{
			if ( (float) Object0 != (int) Object1 )
				return	false;	// Different !
		}
		else if ( System::String::typeid->IsAssignableFrom( T ) )
		{
			if ( (System::String^) Object0 != (System::String^) Object1 )
				return	false;	// Different !
		}
		else if ( Object0 != Object1 )
			return	false;		// Different !
	}

	return	true;
}
