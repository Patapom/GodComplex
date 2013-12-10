#include "../../GodComplex.h"
#include "EffectGlobalIllum.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectGlobalIllum::EffectGlobalIllum( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera ) : m_ErrorCode( 0 ), m_Device( _Device ), m_RTTarget( _RTHDR )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatRender = CreateMaterial( IDR_SHADER_GI_RENDER_SCENE, "./Resources/Shaders/GIRenderScene.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_GI_RENDER_CUBEMAP, "./Resources/Shaders/GIRenderCubeMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 2 );

	//////////////////////////////////////////////////////////////////////////
	// Create the textures
	{
		TextureFilePOM	POM( "./Resources/Scenes/GITest1/pata_diff_colo.pom" );
		m_pTexWalls = new Texture2D( _Device, POM );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
 	m_pCB_Object = new CB<CBObject>( gs_Device, 10 );
 	m_pCB_Material = new CB<CBMaterial>( gs_Device, 11 );


	//////////////////////////////////////////////////////////////////////////
	// Create the scene
	m_bDeleteSceneTags = false;
	m_Scene.Load( IDR_SCENE_GI, *this );

	//////////////////////////////////////////////////////////////////////////
	// Start precomputation
	PreComputeProbes();
}

EffectGlobalIllum::~EffectGlobalIllum()
{
	m_bDeleteSceneTags = true;
	m_Scene.ClearTags( *this );

	delete m_pCB_Material;
	delete m_pCB_Object;

	delete m_pTexWalls;

	delete m_pMatRenderCubeMap;
	delete m_pMatRender;
}


void	EffectGlobalIllum::Render( float _Time, float _DeltaTime )
{
	m_Device.ClearRenderTarget( m_Device.DefaultRenderTarget(), NjFloat4::Zero );
	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), &m_Device.DefaultDepthStencil() );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

	m_Scene.Render( *this );
}


void	EffectGlobalIllum::PreComputeProbes()
{
	Texture2D*	ppRTCubeMap[2];
	ppRTCubeMap[0] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );	// Will contain albedo
	ppRTCubeMap[1] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );	// Will contain normal + distance
	Texture2D*	pRTCubeMapDepth = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );

	// Here are the transform to render the 6 faces of a cube map
	NjFloat4x4	SideTransforms[6] =
	{
		NjFloat4x4::RotY( +0.5f * PI ),	// +X (look right)
		NjFloat4x4::RotY( -0.5f * PI ),	// -X (look left)
		NjFloat4x4::RotX( -0.5f * PI ),	// +Y (look up)
		NjFloat4x4::RotX( +0.5f * PI ),	// -Y (look down)
		NjFloat4x4::RotY( +0.0f * PI ),	// +Z (look front) (default)
		NjFloat4x4::RotY( +1.0f * PI ),	// -Z (look back)
	};

	NjFloat4x4	Camera2Proj = NjFloat4x4::ProjectionPerspective( 0.5f * PI, 1.0f, 0.1f, 10000.0f );

	struct	CBCubeMapCamera
	{
		NjFloat4x4	World2Proj;
	};
	CB<CBCubeMapCamera>*	pCBCubeMapCamera = new CB<CBCubeMapCamera>( m_Device, 9, true );

	// Render every probe as a cube map & process
	Scene::Node*	pProbe = NULL;
	while ( pProbe = m_Scene.ForEach( Scene::Node::PROBE, pProbe ) )
	{
		// Clear cube map
		m_Device.ClearRenderTarget( *ppRTCubeMap[0], NjFloat4::Zero );
		m_Device.ClearRenderTarget( *ppRTCubeMap[1], NjFloat4( 0, 0, 0, 1e6f ) );	// We clear distance to infinity here

		NjFloat4x4&	ProbeTransform = pProbe->m_Local2World;

		// Render the 6 faces
		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
			// Update cube map face camera transform
			NjFloat4x4	CubeFaceTransform = SideTransforms[CubeFaceIndex] * ProbeTransform;

			pCBCubeMapCamera->m.World2Proj = CubeFaceTransform * Camera2Proj;
			pCBCubeMapCamera->UpdateData();

			// Render the scene into the specific cube map faces
			ID3D11RenderTargetView*	ppViews[2] = {
				ppRTCubeMap[0]->GetTargetView( 0, CubeFaceIndex, 1 ),
				ppRTCubeMap[1]->GetTargetView( 0, CubeFaceIndex, 1 )
			};
			m_Device.SetRenderTargets( CUBE_MAP_SIZE, CUBE_MAP_SIZE, 2, ppViews, pRTCubeMapDepth->GetDepthStencilView() );

			// Clear depth
			m_Device.ClearDepthStencil( *pRTCubeMapDepth, 1.0f, 0, true, false );

			// Render scene
			Scene::Node*	pMesh = NULL;
			while( pMesh = m_Scene.ForEach( Scene::Node::MESH, pMesh ) )
			{
				RenderMesh( (Scene::Mesh&) *pMesh, m_pMatRenderCubeMap );
			}
		}
	}

	delete pCBCubeMapCamera;

	delete pRTCubeMapDepth;
	delete ppRTCubeMap[1];
	delete ppRTCubeMap[0];
}


void*	EffectGlobalIllum::TagMaterial( const Scene::Material& _Material ) const
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	switch ( _Material.m_ID )
	{
	case 0:	return m_pMatRender;
	case 1:	return m_pMatRender;
	default:
		ASSERT( false, "Unsupported material!" );
	}
	return NULL;
}
void*	EffectGlobalIllum::TagTexture( const Scene::Material::Texture& _Texture ) const
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	switch ( _Texture.m_ID )
	{
	case 0:		return m_pTexWalls;
	case ~0:	return NULL;	// Invalid textures are not mapped
	default:
		ASSERT( false, "Unsupported texture!" );
	}
	return NULL;
}
void*	EffectGlobalIllum::TagNode( const Scene::Node& _Node ) const
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	return NULL;
}
void*	EffectGlobalIllum::TagPrimitive( const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive ) const
{
	if ( m_bDeleteSceneTags )
	{	// Delete the primitive
		Primitive*	pPrim = (Primitive*) _Primitive.m_pTag;	// We need to cast it as a primitive first so the destructor gets called
		delete pPrim;
		return NULL;
	}

	// Create an actual rendering primitive
	IVertexFormatDescriptor*	pVertexFormat = NULL;
	switch ( _Primitive.m_VertexFormat )
	{
	case Scene::Mesh::Primitive::P3N3G3B3T2:	pVertexFormat = &VertexFormatP3N3G3B3T2::DESCRIPTOR;
	}
	ASSERT( pVertexFormat != NULL, "Unsupported vertex format!" );

	Primitive*	pPrim = new Primitive( m_Device, _Primitive.m_VerticesCount, _Primitive.m_pVertices, 3*_Primitive.m_FacesCount, _Primitive.m_pFaces, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, *pVertexFormat );

	return pPrim;
}

void	EffectGlobalIllum::RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride ) const
{
	// Upload the object's CB
	memcpy( &m_pCB_Object->m.Local2World, &_Mesh.m_Local2World, sizeof(NjFloat4x4) );
//m_pCB_Object->m.Local2World = NjFloat4x4::Identity;
	m_pCB_Object->UpdateData();

	for ( int PrimitiveIndex=0; PrimitiveIndex < _Mesh.m_PrimitivesCount; PrimitiveIndex++ )
	{
		Scene::Mesh::Primitive&	ScenePrimitive = _Mesh.m_pPrimitives[PrimitiveIndex];
		Scene::Material&		SceneMaterial = *ScenePrimitive.m_pMaterial;

		Material*	pMat = _pMaterialOverride == NULL ? (Material*) SceneMaterial.m_pTag : _pMaterialOverride;
		if ( pMat == NULL )
			continue;	// Unsupported material!
		Primitive*	pPrim = (Primitive*) ScenePrimitive.m_pTag;
		if ( pPrim == NULL )
			continue;	// Unsupported primitive!

		// Upload textures
		Texture2D*	pTexDiffuseAlbedo = (Texture2D*) SceneMaterial.m_TexDiffuseAlbedo.m_pTag;
		if ( pTexDiffuseAlbedo != NULL )
			pTexDiffuseAlbedo->SetPS( 10 );
		Texture2D*	pTexSpecularAlbedo = (Texture2D*) SceneMaterial.m_TexSpecularAlbedo.m_pTag;
		if ( pTexSpecularAlbedo != NULL )
			pTexSpecularAlbedo->SetPS( 11 );

		// Upload the primitive's material CB
		m_pCB_Material->m.DiffuseColor = SceneMaterial.m_DiffuseAlbedo;
		m_pCB_Material->m.HasDiffuseTexture = pTexDiffuseAlbedo != NULL;
		m_pCB_Material->m.SpecularColor = SceneMaterial.m_SpecularAlbedo;
		m_pCB_Material->m.HasSpecularTexture = pTexSpecularAlbedo != NULL;
		m_pCB_Material->m.SpecularExponent = SceneMaterial.m_SpecularExponent.x;
		m_pCB_Material->UpdateData();

		// Render
		pMat->Use();
		pPrim->Render( *pMat );
	}
}
