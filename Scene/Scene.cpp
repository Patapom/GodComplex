#include "../GodComplex.h"
#include "Scene.h"


Scene::Scene()
	: m_pROOT( NULL )
	, m_MaterialsCount( 0 )
	, m_ppMaterials( NULL )
{
}
Scene::~Scene()
{
	delete m_pROOT;

	for ( int MaterialIndex=0; MaterialIndex < m_MaterialsCount; MaterialIndex++ )
		delete m_ppMaterials[MaterialIndex];
	delete[] m_ppMaterials;
	m_ppMaterials = NULL;
	m_MaterialsCount = 0;
}

void	Scene::Load( U16 _SceneResourceID, const ISceneTagger& _SceneTagger )
{
	U32			SceneSize = 0;
	const U8*	pData = LoadResourceBinary( _SceneResourceID, "SCENE", &SceneSize );

	U32		Version = ReadU32( pData );	// Should be "GCX1"
	ASSERT( Version == 0x31584347L, "Unsupported scene version!" );

	// ==== Read Materials ====
	//
	m_MaterialsCount = ReadU16( pData );
	m_ppMaterials = new Material*[m_MaterialsCount];
	for ( int MaterialIndex=0; MaterialIndex < m_MaterialsCount; MaterialIndex++ )
	{
		Material*	pMaterial = new Material();
		m_ppMaterials[MaterialIndex] = pMaterial;

		pMaterial->Init( pData, _SceneTagger );
	}

	// ==== Read Node Hierarchy ====
	//
	m_pROOT = CreateNode( NULL, pData, _SceneTagger );
}

void	Scene::ClearTags( const ISceneTagger& _SceneTagClearer )
{
	for ( int MaterialIndex=0; MaterialIndex < m_MaterialsCount; MaterialIndex++ )
		m_ppMaterials[MaterialIndex]->Exit( _SceneTagClearer );

	m_pROOT->Exit( _SceneTagClearer );
}

void	Scene::Render( const ISceneRenderer& _SceneRenderer ) const
{
	Render( m_pROOT, _SceneRenderer );
}

void	Scene::Render( const Node* _pNode, const ISceneRenderer& _SceneRenderer ) const
{
	ASSERT( _pNode != NULL, "Invalid node!" );

	if ( _pNode->m_Type == Node::MESH )
		_SceneRenderer.RenderMesh( (const Mesh&) *_pNode, NULL );

	// Render children
	for ( int ChildIndex=0; ChildIndex < _pNode->m_ChildrenCount; ChildIndex++ )
		Render( _pNode->m_ppChildren[ChildIndex], _SceneRenderer );
}


Scene::Node*	Scene::ForEach( Node::TYPE _Type, Node* _pPrevious, int _StartAtChild )
{
	if ( _pPrevious == NULL )
		_pPrevious = m_pROOT;
	
	// Search in children first
	for ( int ChildIndex=_StartAtChild; ChildIndex < _pPrevious->m_ChildrenCount; ChildIndex++ )
	{
		Scene::Node*	pChild = _pPrevious->m_ppChildren[ChildIndex];
		if ( pChild->m_Type == _Type )
			return pChild;

		// Look in the child's children...
		Scene::Node*	pMatch = ForEach( _Type, pChild );
		if ( pMatch != NULL )
			return pMatch;	// Found a match in one of the children
	}

	// If we couldn't find any match in the children, go back to parent to find this node among its siblings and continue to the next sibling
//	return FindNextNodeOfType( _Type, _pPrevious );
	Node*	pParent = _pPrevious->m_pParent;
	while ( pParent != NULL )
	{
		for ( int SiblingIndex=0; SiblingIndex < pParent->m_ChildrenCount; SiblingIndex++ )
		{
			Node*	pSibling = pParent->m_ppChildren[SiblingIndex];
			if ( pSibling == _pPrevious )
				return ForEach( _Type, pParent, SiblingIndex+1 );	// We found the previous node in its parent's children (i.e. among its siblings), continue search from there...
		}

		// Keep climbing...
		_pPrevious = pParent;
		pParent = pParent->m_pParent;
	}

	return NULL;
}


Scene::Node*	Scene::CreateNode( Node* _pParent, const U8*& _pData, const ISceneTagger& _SceneTagger )
{
	Node*		pResult = NULL;
	Node::TYPE	NodeType = (Node::TYPE) *_pData;
	switch ( NodeType )
	{
	case Node::GENERIC:
		pResult = new Node( *this, _pParent );
		break;
	case Node::LIGHT:
		pResult = new Light( *this, _pParent );
		break;
	case Node::CAMERA:
		pResult = new Camera( *this, _pParent );
		break;
	case Node::MESH:
		pResult = new Mesh( *this, _pParent );
		break;

		// Special nodes
	case Node::PROBE:
		pResult = new Probe( *this, _pParent );
		break;

	default:
		ASSERT( false, "Unsupported node type!" );
	}

	// Init and tag the node
	pResult->Init( _pData, _SceneTagger );

	// Process children
	pResult->m_ChildrenCount = ReadU16( _pData );
	if ( pResult->m_ChildrenCount > 0 )
	{
		pResult->m_ppChildren = new Node*[pResult->m_ChildrenCount];
		for ( int ChildIndex=0; ChildIndex < pResult->m_ChildrenCount; ChildIndex++ )
		{
			Node*	pChild = CreateNode( pResult, _pData, _SceneTagger );
			pResult->m_ppChildren[ChildIndex] = pChild;
		}
	}

	return pResult;
}

U32	Scene::ReadU16( const U8*& _pData, bool _IsID )
{
	U32		Result = *((U16*) _pData);
	_pData += sizeof(U16);

	if ( _IsID && Result == 0xFFFFL )
		Result = ~0;

	return Result;
}
U32	Scene::ReadU32( const U8*& _pData )
{
	U32		Result = *((U32*) _pData);
	_pData += sizeof(U32);
	return Result;
}
float Scene::ReadF32( const U8*& _pData )
{
	float	Result = *((float*) _pData);
	_pData += sizeof(float);
	return Result;
}
void Scene::ReadEndMaterialMarker( const U8*& _pData )
{
	U16		EndMarker = ReadU16( _pData );
	ASSERT( EndMarker == 0x1234, "Failed to reach end material marker!" );
}
void Scene::ReadEndNodeMarker( const U8*& _pData )
{
	U16		EndMarker = ReadU16( _pData );
	ASSERT( EndMarker == 0xABCD, "Failed to reach end node marker!" );
}


//////////////////////////////////////////////////////////////////////////
// SCENE NESTED TYPES
Scene::Node::Node( Scene& _Owner, Node* _pParent )
	: m_Owner( _Owner )
	, m_pParent( _pParent )
	, m_ChildrenCount( 0 )
	, m_ppChildren( NULL )
	, m_pTag( NULL )
{
}

void	Scene::Node::Init( const U8*& _pData, const ISceneTagger& _SceneTagger )
{
	m_Type = (TYPE) *_pData++;

	m_Local2Parent.m[4*0+0] = ReadF32( _pData );
	m_Local2Parent.m[4*0+1] = ReadF32( _pData );
	m_Local2Parent.m[4*0+2] = ReadF32( _pData );
	m_Local2Parent.m[4*0+3] = ReadF32( _pData );
	m_Local2Parent.m[4*1+0] = ReadF32( _pData );
	m_Local2Parent.m[4*1+1] = ReadF32( _pData );
	m_Local2Parent.m[4*1+2] = ReadF32( _pData );
	m_Local2Parent.m[4*1+3] = ReadF32( _pData );
	m_Local2Parent.m[4*2+0] = ReadF32( _pData );
	m_Local2Parent.m[4*2+1] = ReadF32( _pData );
	m_Local2Parent.m[4*2+2] = ReadF32( _pData );
	m_Local2Parent.m[4*2+3] = ReadF32( _pData );
	m_Local2Parent.m[4*3+0] = ReadF32( _pData );
	m_Local2Parent.m[4*3+1] = ReadF32( _pData );
	m_Local2Parent.m[4*3+2] = ReadF32( _pData );
	m_Local2Parent.m[4*3+3] = ReadF32( _pData );

	// Retrieve LOCAL => WORLD
	const NjFloat4x4&	Parent2World = m_pParent != NULL ? m_pParent->m_Local2World : NjFloat4x4::Identity;
	m_Local2World = m_Local2Parent * Parent2World;

	InitSpecific( _pData, _SceneTagger );

	ReadEndNodeMarker( _pData );

	// Ask for a tag
	m_pTag = _SceneTagger.TagNode( *this );
}
Scene::Node::~Node()
{
	for ( int ChildIndex=0; ChildIndex < m_ChildrenCount; ChildIndex++ )
		delete m_ppChildren[ChildIndex];
	delete[] m_ppChildren;
	m_ppChildren = NULL;
	m_ChildrenCount = 0;
}

void	Scene::Node::Exit( const ISceneTagger& _SceneTagClearer )
{
	ExitSpecific( _SceneTagClearer );

	m_pTag = _SceneTagClearer.TagNode( *this );

	// Exit on children too
	for ( int ChildIndex=0; ChildIndex < m_ChildrenCount; ChildIndex++ )
		m_ppChildren[ChildIndex]->Exit( _SceneTagClearer );
}

// ==== Light ====
Scene::Light::Light( Scene& _Owner, Node* _pParent )
	: Node( _Owner, _pParent )
	, m_LightType( POINT )
	, m_Intensity( 1.0f )
	, m_HotSpot( 0.0f )
	, m_Falloff( 0.0f )
{
}

void	Scene::Light::InitSpecific( const U8*& _pData, const ISceneTagger& _SceneTagger )
{
	m_LightType = (LIGHT_TYPE) *_pData++;
	m_Color.Set( ReadF32( _pData ), ReadF32( _pData ), ReadF32( _pData ) );
	m_Intensity = ReadF32( _pData );
	m_HotSpot = ReadF32( _pData );
	m_Falloff = ReadF32( _pData );
}


// ==== Camera ====
Scene::Camera::Camera( Scene& _Owner, Node* _pParent )
	: Node( _Owner, _pParent )
	, m_FOV( 0.0f )
{
}

void	Scene::Camera::InitSpecific( const U8*& _pData, const ISceneTagger& _SceneTagger )
{
	m_FOV = ReadF32( _pData );
}


// ==== Mesh ====
Scene::Mesh::Mesh( Scene& _Owner, Node* _pParent )
	: Node( _Owner, _pParent )
	, m_PrimitivesCount( 0 )
	, m_pPrimitives( NULL )
{
}
Scene::Mesh::~Mesh()
{
	delete[] m_pPrimitives;
	m_pPrimitives = NULL;
	m_PrimitivesCount = 0;
}

void	Scene::Mesh::InitSpecific( const U8*& _pData, const ISceneTagger& _SceneTagger )
{
	m_PrimitivesCount = ReadU16( _pData );
	m_pPrimitives = new Primitive[m_PrimitivesCount];
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ )
	{
		Primitive&	P = m_pPrimitives[PrimitiveIndex];
		P.Init( m_Owner, _pData );

		// Tag the primitive
		P.m_pTag = _SceneTagger.TagPrimitive( *this, P );
	}
}

void	Scene::Mesh::ExitSpecific( const ISceneTagger& _SceneTagClearer )
{
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ )
		m_pPrimitives[PrimitiveIndex].m_pTag = _SceneTagClearer.TagPrimitive( *this, m_pPrimitives[PrimitiveIndex] );
}

Scene::Mesh::Primitive::Primitive()
	: m_pMaterial( NULL )
	, m_FacesCount( 0 )
	, m_pFaces( NULL )
	, m_VerticesCount( 0 )
	, m_pVertices( NULL )
{
}
Scene::Mesh::Primitive::~Primitive()
{
	delete[] m_pFaces;
	delete[] m_pVertices;
}

void	Scene::Mesh::Primitive::Init( Scene& _Owner, const U8*& _pData )
{
	int	MaterialID = ReadU16( _pData, true );
	ASSERT( MaterialID < _Owner.m_MaterialsCount, "Material ID out of range!" );
	m_pMaterial = _Owner.m_ppMaterials[MaterialID];

	m_FacesCount = ReadU32( _pData );
	m_VerticesCount = ReadU32( _pData );

	// Read indices
	m_pFaces = new U32[3*m_FacesCount];
	if ( m_VerticesCount <= 65536 )
	{
		for ( U32 FaceIndex=0; FaceIndex < m_FacesCount; FaceIndex++ )
		{
			m_pFaces[3*FaceIndex+0] = ReadU16( _pData );
			m_pFaces[3*FaceIndex+1] = ReadU16( _pData );
			m_pFaces[3*FaceIndex+2] = ReadU16( _pData );
		}
	}
	else
	{
		int	IndexBufferSize = 3*m_FacesCount*sizeof(U32);
		memcpy( m_pFaces, _pData, IndexBufferSize );
		_pData += IndexBufferSize;
	}

	// Read vertices
	m_VertexFormat = (VERTEX_FORMAT) *_pData++;
	int	VertexSize = 0;
	switch ( m_VertexFormat )
	{
	case P3N3G3B3T2: VertexSize = (3+3+3+3+2) * sizeof(float); break;
	}

	int		VertexBufferSize = m_VerticesCount * VertexSize;
	m_pVertices = new U8[VertexBufferSize];
	memcpy( m_pVertices, _pData, VertexBufferSize );
	_pData += VertexBufferSize;
}

// ==== Probe ====
Scene::Probe::Probe( Scene& _Owner, Node* _pParent )
	: Node( _Owner, _pParent )
{
}


// ==== Material ====
Scene::Material::Material()
	: m_ID( ~0 )
	, m_pTag( NULL )
{
}

void	Scene::Material::Init( const U8*& _pData, const ISceneTagger& _SceneTagger )
{
	m_ID = ReadU16( _pData, true );

	m_DiffuseAlbedo.x = ReadF32( _pData );
	m_DiffuseAlbedo.y = ReadF32( _pData );
	m_DiffuseAlbedo.z = ReadF32( _pData );
	m_TexDiffuseAlbedo.m_ID = ReadU16( _pData, true );
	m_TexDiffuseAlbedo.m_pTag = _SceneTagger.TagTexture( m_TexDiffuseAlbedo );

	m_SpecularAlbedo.x = ReadF32( _pData );
	m_SpecularAlbedo.y = ReadF32( _pData );
	m_SpecularAlbedo.z = ReadF32( _pData );
	m_TexSpecularAlbedo.m_ID = ReadU16( _pData, true );
	m_TexSpecularAlbedo.m_pTag = _SceneTagger.TagTexture( m_TexSpecularAlbedo );

	m_SpecularExponent.x = ReadF32( _pData );
	m_SpecularExponent.y = ReadF32( _pData );
	m_SpecularExponent.z = ReadF32( _pData );

	ReadEndMaterialMarker( _pData );

	// Ask for a tag
	m_pTag = _SceneTagger.TagMaterial( *this );
}

void	Scene::Material::Exit( const ISceneTagger& _SceneTagClearer )
{
	m_TexDiffuseAlbedo.m_pTag = _SceneTagClearer.TagTexture( m_TexDiffuseAlbedo );
	m_TexSpecularAlbedo.m_pTag = _SceneTagClearer.TagTexture( m_TexSpecularAlbedo );

	m_pTag = _SceneTagClearer.TagMaterial( *this );
}
