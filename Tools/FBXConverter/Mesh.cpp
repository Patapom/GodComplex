#include "stdafx.h"

#define SCALE	0.001

Mesh::Mesh( FbxMesh& _Mesh, Node* _pParent ) : Node( *_Mesh.GetNode(), _pParent ), m_Mesh( _Mesh )
{
// // 	DisplayMetaDataConnections(&_Mesh);
// // 	DisplayControlsPoints(&_Mesh);
// 	DisplayMaterialMapping(&_Mesh);
// 	DisplayMaterial(&_Mesh);
// 	DisplayTexture(&_Mesh);
// 	DisplayPolygons(&_Mesh);
// //	DisplayMaterialConnections(&_Mesh);
// // 	DisplayLink(&_Mesh);
// // 	DisplayShape(&_Mesh);

	// Read back vertices
	m_VerticesCount = _Mesh.GetControlPointsCount();
	m_pVertices = new Vertex[m_VerticesCount];
	for ( int VertexIndex=0; VertexIndex < m_VerticesCount; VertexIndex++ )
	{
		FbxVector4	V = _Mesh.GetControlPointAt( VertexIndex );
		m_pVertices[VertexIndex].x = (float) (SCALE * V[0]);
		m_pVertices[VertexIndex].y = (float) (SCALE * V[1]);
		m_pVertices[VertexIndex].z = (float) (SCALE * V[2]);
	}

	// Count streams & store types
	{
		int	StreamsCount = _Mesh.GetElementUVCount();
		for ( int i=0; i < StreamsCount; i++ )
			m_StreamTypes.push_back( UV );
	}
	{
		int	StreamsCount = _Mesh.GetElementNormalCount();
		for ( int i=0; i < StreamsCount; i++ )
			m_StreamTypes.push_back( NORMAL );
	}
	{
		int	StreamsCount = _Mesh.GetElementTangentCount();
		for ( int i=0; i < StreamsCount; i++ )
			m_StreamTypes.push_back( TANGENT );
	}
	{
		int	StreamsCount = _Mesh.GetElementBinormalCount();
		for ( int i=0; i < StreamsCount; i++ )
			m_StreamTypes.push_back( BITANGENT );
	}
	{
		int	StreamsCount = _Mesh.GetElementVertexColorCount();
		for ( int i=0; i < StreamsCount; i++ )
			m_StreamTypes.push_back( VERTEX_COLOR );
	}

	// Build temporary polygons
	m_Polygons.resize( _Mesh.GetPolygonCount() );

	// Treat streams in the same order as we stored the types

	{	//////////////////////////////////////////////////////////////////////////
		// UVs
		for ( int i=0; i < _Mesh.GetElementUVCount(); i++ )
		{
			FbxGeometryElementUV* pUV = _Mesh.GetElementUV( i );

			bool	bDirect = pUV->GetReferenceMode() == FbxGeometryElement::eDirect;

			if ( i == 0 )
			{	// First UV stream
				switch ( pUV->GetMappingMode() )
				{
				case FbxGeometryElement::eByControlPoint:
					for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
					{
						Polygon&	P = m_Polygons[PolygonIndex];

						int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
						for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++ )
						{
							int	PolygonControlPointIndex = _Mesh.GetPolygonVertex( PolygonIndex, PolygonVertexIndex );
							if ( bDirect )
								P.m_UV0.push_back( pUV->GetDirectArray().GetAt( PolygonControlPointIndex ) );
							else
							{
								int id = pUV->GetIndexArray().GetAt( PolygonControlPointIndex );
								P.m_UV0.push_back( pUV->GetDirectArray().GetAt( id ) );
							}
						}
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						int	VertexIndex=0;
						for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
						{
							Polygon&	P = m_Polygons[PolygonIndex];

							int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
							for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++, VertexIndex++ )
							{
								if ( bDirect )
									P.m_UV0.push_back( pUV->GetDirectArray().GetAt( VertexIndex ) );
								else
								{
									int id = pUV->GetIndexArray().GetAt( VertexIndex );
									P.m_UV0.push_back( pUV->GetDirectArray().GetAt( id ) );
								}
							}
						}
					}
					break;
				}
			}
			else if ( i == 1 )
			{	// Second UV stream
				switch ( pUV->GetMappingMode() )
				{
				case FbxGeometryElement::eByControlPoint:
					for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
					{
						Polygon&	P = m_Polygons[PolygonIndex];

						int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
						for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++ )
						{
							int	PolygonControlPointIndex = _Mesh.GetPolygonVertex( PolygonIndex, PolygonVertexIndex );
							if ( bDirect )
								P.m_UV1.push_back( pUV->GetDirectArray().GetAt( PolygonControlPointIndex ) );
							else
							{
								int id = pUV->GetIndexArray().GetAt( PolygonControlPointIndex );
								P.m_UV1.push_back( pUV->GetDirectArray().GetAt( id ) );
							}
						}
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						int	VertexIndex=0;
						for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
						{
							Polygon&	P = m_Polygons[PolygonIndex];

							int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
							for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++, VertexIndex++ )
							{
								if ( bDirect )
									P.m_UV1.push_back( pUV->GetDirectArray().GetAt( VertexIndex ) );
								else
								{
									int id = pUV->GetIndexArray().GetAt( VertexIndex );
									P.m_UV1.push_back( pUV->GetDirectArray().GetAt( id ) );
								}
							}
						}
					}
					break;
				}
			}
			else
			{
				assert( false );	// Handle more streams!
			}
		}
	}
	
	{	//////////////////////////////////////////////////////////////////////////
		// Normals
		for ( int i=0; i < _Mesh.GetElementNormalCount(); i++ )
		{
			FbxGeometryElementNormal* pNormal = _Mesh.GetElementNormal( i );

			bool	bDirect = pNormal->GetReferenceMode() == FbxGeometryElement::eDirect;

			if ( i == 0 )
			{	// First Normal stream
				switch ( pNormal->GetMappingMode() )
				{
				case FbxGeometryElement::eByControlPoint:
					for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
					{
						Polygon&	P = m_Polygons[PolygonIndex];

						int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
						for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++ )
						{
							int	PolygonControlPointIndex = _Mesh.GetPolygonVertex( PolygonIndex, PolygonVertexIndex );
							if ( bDirect )
								P.m_Normals.push_back( pNormal->GetDirectArray().GetAt( PolygonControlPointIndex ) );
							else
							{
								int id = pNormal->GetIndexArray().GetAt( PolygonControlPointIndex );
								P.m_Normals.push_back( pNormal->GetDirectArray().GetAt( id ) );
							}
						}
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						int	VertexIndex=0;
						for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
						{
							Polygon&	P = m_Polygons[PolygonIndex];

							int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
							for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++, VertexIndex++ )
							{
								if ( bDirect )
									P.m_Normals.push_back( pNormal->GetDirectArray().GetAt( VertexIndex ) );
								else
								{
									int id = pNormal->GetIndexArray().GetAt( VertexIndex );
									P.m_Normals.push_back( pNormal->GetDirectArray().GetAt( id ) );
								}
							}
						}
					}
					break;
				}
			}
			else
			{
				assert( false );	// Handle more streams!
			}
		}
	}
	
	{	//////////////////////////////////////////////////////////////////////////
		// Tangents
		for ( int i=0; i < _Mesh.GetElementTangentCount(); i++ )
		{
			FbxGeometryElementTangent* pTangent = _Mesh.GetElementTangent( i );

			bool	bDirect = pTangent->GetReferenceMode() == FbxGeometryElement::eDirect;

			if ( i == 0 )
			{	// First Tangent stream
				switch ( pTangent->GetMappingMode() )
				{
				case FbxGeometryElement::eByControlPoint:
					for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
					{
						Polygon&	P = m_Polygons[PolygonIndex];

						int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
						for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++ )
						{
							int	PolygonControlPointIndex = _Mesh.GetPolygonVertex( PolygonIndex, PolygonVertexIndex );
							if ( bDirect )
								P.m_Tangents.push_back( pTangent->GetDirectArray().GetAt( PolygonControlPointIndex ) );
							else
							{
								int id = pTangent->GetIndexArray().GetAt( PolygonControlPointIndex );
								P.m_Tangents.push_back( pTangent->GetDirectArray().GetAt( id ) );
							}
						}
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						int	VertexIndex=0;
						for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
						{
							Polygon&	P = m_Polygons[PolygonIndex];

							int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
							for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++, VertexIndex++ )
							{
								if ( bDirect )
									P.m_Tangents.push_back( pTangent->GetDirectArray().GetAt( VertexIndex ) );
								else
								{
									int id = pTangent->GetIndexArray().GetAt( VertexIndex );
									P.m_Tangents.push_back( pTangent->GetDirectArray().GetAt( id ) );
								}
							}
						}
					}
					break;
				}
			}
			else
			{
				assert( false );	// Handle more streams!
			}
		}
	}
	
	{	//////////////////////////////////////////////////////////////////////////
		// BiTangents
		for ( int i=0; i < _Mesh.GetElementBinormalCount(); i++ )
		{
			FbxGeometryElementBinormal* pBiTangent = _Mesh.GetElementBinormal( i );

			bool	bDirect = pBiTangent->GetReferenceMode() == FbxGeometryElement::eDirect;

			if ( i == 0 )
			{	// First BiTangent stream
				switch ( pBiTangent->GetMappingMode() )
				{
				case FbxGeometryElement::eByControlPoint:
					for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
					{
						Polygon&	P = m_Polygons[PolygonIndex];

						int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
						for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++ )
						{
							int	PolygonControlPointIndex = _Mesh.GetPolygonVertex( PolygonIndex, PolygonVertexIndex );
							if ( bDirect )
								P.m_BiTangents.push_back( pBiTangent->GetDirectArray().GetAt( PolygonControlPointIndex ) );
							else
							{
								int id = pBiTangent->GetIndexArray().GetAt( PolygonControlPointIndex );
								P.m_BiTangents.push_back( pBiTangent->GetDirectArray().GetAt( id ) );
							}
						}
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						int	VertexIndex=0;
						for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
						{
							Polygon&	P = m_Polygons[PolygonIndex];

							int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
							for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++, VertexIndex++ )
							{
								if ( bDirect )
									P.m_BiTangents.push_back( pBiTangent->GetDirectArray().GetAt( VertexIndex ) );
								else
								{
									int id = pBiTangent->GetIndexArray().GetAt( VertexIndex );
									P.m_BiTangents.push_back( pBiTangent->GetDirectArray().GetAt( id ) );
								}
							}
						}
					}
					break;
				}
			}
			else
			{
				assert( false );	// Handle more streams!
			}
		}
	}
	
	{	//////////////////////////////////////////////////////////////////////////
		// Vertex Colors
		for ( int i=0; i < _Mesh.GetElementVertexColorCount(); i++ )
		{
			FbxGeometryElementVertexColor* pVertexColor = _Mesh.GetElementVertexColor( i );

			bool	bDirect = pVertexColor->GetReferenceMode() == FbxGeometryElement::eDirect;

			if ( i == 0 )
			{	// First Vertex Color stream
				switch ( pVertexColor->GetMappingMode() )
				{
				case FbxGeometryElement::eByControlPoint:
					for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
					{
						Polygon&	P = m_Polygons[PolygonIndex];

						int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
						for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++ )
						{
							int	PolygonControlPointIndex = _Mesh.GetPolygonVertex( PolygonIndex, PolygonVertexIndex );
							if ( bDirect )
								P.m_VertexColors.push_back( pVertexColor->GetDirectArray().GetAt( PolygonControlPointIndex ) );
							else
							{
								int id = pVertexColor->GetIndexArray().GetAt( PolygonControlPointIndex );
								P.m_VertexColors.push_back( pVertexColor->GetDirectArray().GetAt( id ) );
							}
						}
					}
					break;

				case FbxGeometryElement::eByPolygonVertex:
					{
						int	VertexIndex=0;
						for ( int PolygonIndex=0; PolygonIndex < int(m_Polygons.size()); PolygonIndex++ )
						{
							Polygon&	P = m_Polygons[PolygonIndex];

							int	PolygonVerticesCount = _Mesh.GetPolygonSize( PolygonIndex );
							for ( int PolygonVertexIndex=0; PolygonVertexIndex < PolygonVerticesCount; PolygonVertexIndex++, VertexIndex++ )
							{
								if ( bDirect )
									P.m_VertexColors.push_back( pVertexColor->GetDirectArray().GetAt( VertexIndex ) );
								else
								{
									int id = pVertexColor->GetIndexArray().GetAt( VertexIndex );
									P.m_VertexColors.push_back( pVertexColor->GetDirectArray().GetAt( id ) );
								}
							}
						}
					}
					break;
				}
			}
			else
			{
				assert( false );	// Handle more streams!
			}
		}
	}
}

Mesh::~Mesh()
{
	delete[] m_pVertices;
}

Mesh::Material::Material( FbxSurfaceMaterial* _pMaterial ) : m_pMaterial( _pMaterial )
{
	assert( _pMaterial->GetClassId().Is( FbxSurfaceLambert::ClassId ) );

	FbxSurfaceLambert*	pLambert = (FbxSurfaceLambert*) _pMaterial;

	m_Ambient.Set( pLambert->Ambient.Get()[0], pLambert->Ambient.Get()[1], pLambert->Ambient.Get()[2] );
	m_Diffuse.Set( pLambert->Diffuse.Get()[0], pLambert->Diffuse.Get()[1], pLambert->Diffuse.Get()[2] );
	m_Emissive.Set( pLambert->Emissive.Get()[0], pLambert->Emissive.Get()[1], pLambert->Emissive.Get()[2] );
	m_Transparency = (float) pLambert->TransparencyFactor;

	// Add textures...
}
