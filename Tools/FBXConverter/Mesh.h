#pragma once
#include "stdafx.h"

class Mesh : public Node
{
public:

	class	Material	// Only lambert for now
	{
		FbxSurfaceMaterial*	m_pMaterial;

		FbxVector4			m_Ambient;
		FbxVector4			m_Diffuse;
		FbxVector4			m_Emissive;
		float				m_Transparency;

//		FbxString			m_DiffuseTextureFileName;

	public:
		Material( FbxSurfaceMaterial* _pMaterial );
	};

	struct	Vertex 
	{
		float	x, y, z;
	};

	enum	STREAM_TYPE : int
	{
		UV,
		NORMAL,
		TANGENT,
		BITANGENT,
		VERTEX_COLOR
	};

	class	Polygon
	{
	public:

		int						m_MatID;
		std::vector<FbxVector2>	m_UV0;
		std::vector<FbxVector2>	m_UV1;
		std::vector<FbxVector4>	m_Normals;
		std::vector<FbxVector4>	m_Tangents;
		std::vector<FbxVector4>	m_BiTangents;
		std::vector<FbxColor>	m_VertexColors;

	};

protected:

	FbxMesh&					m_Mesh;

	std::vector<Material>		m_Materials;

	int							m_VerticesCount;
	Vertex*						m_pVertices;

	std::vector<STREAM_TYPE>	m_StreamTypes;

	std::vector<Polygon>		m_Polygons;

public:
	Mesh( FbxMesh& _Mesh, Node* _pParent );
	~Mesh();
};