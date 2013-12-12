#include "../../GodComplex.h"
#include "EffectGlobalIllum.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectGlobalIllum::EffectGlobalIllum( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera ) : m_ErrorCode( 0 ), m_Device( _Device ), m_RTTarget( _RTHDR ), m_ScreenQuad( _ScreenQuad )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatRender = CreateMaterial( IDR_SHADER_GI_RENDER_SCENE, "./Resources/Shaders/GIRenderScene.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_GI_RENDER_CUBEMAP, "./Resources/Shaders/GIRenderCubeMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 2 );
 	CHECK_MATERIAL( m_pMatPostProcess = CreateMaterial( IDR_SHADER_GI_POST_PROCESS, "./Resources/Shaders/GIPostProcess.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 10 );

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
	m_pCB_Splat = new CB<CBSplat>( gs_Device, 10 );


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

	delete m_pCB_Splat;
	delete m_pCB_Material;
	delete m_pCB_Object;

	delete m_pTexWalls;

	delete m_pMatPostProcess;
	delete m_pMatRenderCubeMap;
	delete m_pMatRender;
}


void	EffectGlobalIllum::Render( float _Time, float _DeltaTime )
{
	//////////////////////////////////////////////////////////////////////////
	// 1] Render the scene
// 	m_Device.ClearRenderTarget( m_RTTarget, NjFloat4::Zero );

 	m_Device.SetRenderTarget( m_RTTarget, &m_Device.DefaultDepthStencil() );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

	m_Scene.Render( *this );


	//////////////////////////////////////////////////////////////////////////
	// 2] Post-process the result
	USING_MATERIAL_START( *m_pMatPostProcess )

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );
	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget() );

	m_RTTarget.SetPS( 10 );

	m_pCB_Splat->m.dUV = m_Device.DefaultRenderTarget().GetdUV();
	m_pCB_Splat->UpdateData();

	m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	m_RTTarget.RemoveFromLastAssignedSlots();
}


	Texture2D*	ppRTCubeMap[2];
void	EffectGlobalIllum::PreComputeProbes()
{
	const float		Z_INFINITY = 1e6f;
	const float		Z_INFINITY_TEST = 0.99f * Z_INFINITY;

	ppRTCubeMap[0] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain albedo
	ppRTCubeMap[1] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain normal + distance
	Texture2D*	pRTCubeMapDepth = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );

	Texture2D*	ppRTCubeMapStaging[2];
	ppRTCubeMapStaging[0] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );	// Will contain albedo
	ppRTCubeMapStaging[1] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );	// Will contain normal + distance

	// Here are the transform to render the 6 faces of a cube map
	NjFloat4x4	SideTransforms[6] =
	{
		NjFloat4x4::RotY( -0.5f * PI ),	// +X (look right)
		NjFloat4x4::RotY( +0.5f * PI ),	// -X (look left)
		NjFloat4x4::RotX( +0.5f * PI ),	// +Y (look up)
		NjFloat4x4::RotX( -0.5f * PI ),	// -Y (look down)
		NjFloat4x4::RotY( +0.0f * PI ),	// +Z (look front) (default)
		NjFloat4x4::RotY( +1.0f * PI ),	// -Z (look back)
	};

	NjFloat4x4	Camera2Proj = NjFloat4x4::ProjectionPerspective( 0.5f * PI, 1.0f, 0.1f, 1000.0f );

	struct	CBCubeMapCamera
	{
		NjFloat4x4	World2Proj;
	};
	CB<CBCubeMapCamera>*	pCBCubeMapCamera = new CB<CBCubeMapCamera>( m_Device, 9, true );

	// Render every probe as a cube map & process
	Scene::Node*	pProbe = NULL;
	while ( pProbe = m_Scene.ForEach( Scene::Node::PROBE, pProbe ) )
	{
		//////////////////////////////////////////////////////////////////////////
		// 1] Render Albedo + Normal + Z

		// Clear cube map
		m_Device.ClearRenderTarget( *ppRTCubeMap[0], NjFloat4::Zero );
		m_Device.ClearRenderTarget( *ppRTCubeMap[1], NjFloat4( 0, 0, 0, Z_INFINITY ) );	// We clear distance to infinity here

		NjFloat4x4	ProbeLocal2World = pProbe->m_Local2World;
		ProbeLocal2World.Normalize();

		NjFloat4x4	ProbeWorld2Local = ProbeLocal2World.Inverse();

		// Render the 6 faces
		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
			// Update cube map face camera transform
			NjFloat4x4	ProbeWorld2Camera = ProbeWorld2Local * SideTransforms[CubeFaceIndex];

			pCBCubeMapCamera->m.World2Proj = ProbeWorld2Camera * Camera2Proj;
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
			while ( pMesh = m_Scene.ForEach( Scene::Node::MESH, pMesh ) )
			{
				RenderMesh( (Scene::Mesh&) *pMesh, m_pMatRenderCubeMap );
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// 2] Read back cube map and create diffuse reflected SH coefficients
		ppRTCubeMapStaging[0]->CopyFrom( *ppRTCubeMap[0] );
		ppRTCubeMapStaging[1]->CopyFrom( *ppRTCubeMap[1] );

		double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
		double	SumSolidAngle = 0.0;

		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
			NjFloat4x4	Camera2ProbeWorld = SideTransforms[CubeFaceIndex] * ProbeLocal2World;

			// Update cube map face camera transform
			NjFloat4x4	ProbeWorld2Camera = ProbeWorld2Local * SideTransforms[CubeFaceIndex];
ProbeWorld2Camera.Inverse();	// CHECK!


			D3D11_MAPPED_SUBRESOURCE&	MappedFaceAlbedo = ppRTCubeMapStaging[0]->Map( 0, CubeFaceIndex );
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceGeometry = ppRTCubeMapStaging[1]->Map( 0, CubeFaceIndex );

			NjFloat3	View;
			double		pSH[3*9];
			memset( pSH, 0, 3*9*sizeof(double) );

			for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			{
				NjFloat4*	pScanlineAlbedo = (NjFloat4*) ((U8*) MappedFaceAlbedo.pData + Y * MappedFaceAlbedo.RowPitch);
				NjFloat4*	pScanlineGeometry = (NjFloat4*) ((U8*) MappedFaceGeometry.pData + Y * MappedFaceGeometry.RowPitch);

				View.y = 1.0f - 2.0f * Y / (CUBE_MAP_SIZE-1);
				for ( int X=0; X < CUBE_MAP_SIZE; X++ )
				{
					NjFloat4	Albedo = *pScanlineAlbedo++;
					NjFloat4	Geometry = *pScanlineGeometry++;

					// Rebuild view direction
					View.x = 2.0f * X / (CUBE_MAP_SIZE-1) - 1.0f;
					View.z = 1.0f;

					// Retrieve the cube map texel's solid angle (from http://people.cs.kuleuven.be/~philip.dutre/GI/TotalCompendium.pdf)
					// dw = cos(Theta).dA / r²
					// cos(Theta) = Adjacent/Hypothenuse = 1/r
					//
					float	SqDistance2Texel = View.LengthSq();
					float	Distance2Texel = sqrtf( SqDistance2Texel );

					double	SolidAngle = dA / (Distance2Texel * SqDistance2Texel);
					SumSolidAngle += SolidAngle;	// CHECK! => Should amount to 4PI at the end of the iteration...

					// Retrieve world position
					// Is this useful?
//					View *= Camera2ProbeWorld;	// View vector in world space


					// Check if we hit an obstacle, in which case we should accumulate indirect lighting
					if ( Geometry.w > Z_INFINITY_TEST )
						continue;	// No obstacle means direct lighting from ambient sky term, which is accounted for somewhere else...

					// Build SH cosine lobe in the direction of the surface normal
					NjFloat3	Normal( Geometry.x, Geometry.y, Geometry.z );
					float		pCosineLobe[9];
					BuildSHCosineLobe( Normal, pCosineLobe );

					// Accumulate lobe, weighted by solid angle and albedo
					double	R = SolidAngle * Albedo.x;
					double	G = SolidAngle * Albedo.y;
					double	B = SolidAngle * Albedo.z;
					pSH[3*0+0] += pCosineLobe[0] * R;
					pSH[3*0+1] += pCosineLobe[0] * G;
					pSH[3*0+2] += pCosineLobe[0] * B;
					pSH[3*1+0] += pCosineLobe[1] * R;
					pSH[3*1+1] += pCosineLobe[1] * G;
					pSH[3*1+2] += pCosineLobe[1] * B;
					pSH[3*2+0] += pCosineLobe[2] * R;
					pSH[3*2+1] += pCosineLobe[2] * G;
					pSH[3*2+2] += pCosineLobe[2] * B;
					pSH[3*3+0] += pCosineLobe[3] * R;
					pSH[3*3+1] += pCosineLobe[3] * G;
					pSH[3*3+2] += pCosineLobe[3] * B;
					pSH[3*4+0] += pCosineLobe[4] * R;
					pSH[3*4+1] += pCosineLobe[4] * G;
					pSH[3*4+2] += pCosineLobe[4] * B;
					pSH[3*5+0] += pCosineLobe[5] * R;
					pSH[3*5+1] += pCosineLobe[5] * G;
					pSH[3*5+2] += pCosineLobe[5] * B;
					pSH[3*6+0] += pCosineLobe[6] * R;
					pSH[3*6+1] += pCosineLobe[6] * G;
					pSH[3*6+2] += pCosineLobe[6] * B;
					pSH[3*7+0] += pCosineLobe[7] * R;
					pSH[3*7+1] += pCosineLobe[7] * G;
					pSH[3*7+2] += pCosineLobe[7] * B;
					pSH[3*8+0] += pCosineLobe[8] * R;
					pSH[3*8+1] += pCosineLobe[8] * G;
					pSH[3*8+2] += pCosineLobe[8] * B;
				}
			}

			ppRTCubeMapStaging[0]->UnMap( 0, CubeFaceIndex );
			ppRTCubeMapStaging[1]->UnMap( 0, CubeFaceIndex );
		}
	}

#if 1
m_Device.RemoveRenderTargets();
ppRTCubeMap[0]->SetPS( 64 );
ppRTCubeMap[1]->SetPS( 65 );
#endif

	delete pCBCubeMapCamera;

	delete ppRTCubeMapStaging[1];
	delete ppRTCubeMapStaging[0];

	delete pRTCubeMapDepth;
// 	delete ppRTCubeMap[1];
// 	delete ppRTCubeMap[0];
}


// Builds a spherical harmonics cosine lobe
// (from "Stupid SH Tricks")
//
void	EffectGlobalIllum::BuildSHCosineLobe( const NjFloat3& _Direction, float _Coeffs[9] )
{
	const NjFloat3 ZHCoeffs = NjFloat3(
		0.88622692545275801364908374167057f,	// sqrt(PI) / 2
		1.0233267079464884884795516248893f,		// sqrt(PI / 3)
		0.49541591220075137666812859564002f		// sqrt(5PI) / 8
		);
	ZHRotate( _Direction, ZHCoeffs, _Coeffs );
}

// Rotates ZH coefficients in the specified direction (from "Stupid SH Tricks")
// Rotating ZH comes to evaluating scaled SH in the given direction.
// The scaling factors for each band are equal to the ZH coefficients multiplied by sqrt( 4PI / (2l+1) )
//
void	EffectGlobalIllum::ZHRotate( const NjFloat3& _Direction, const NjFloat3& _ZHCoeffs, float _Coeffs[9] )
{
	float	cl0 = 3.5449077018110320545963349666823f * _ZHCoeffs.x;	// sqrt(4PI)
	float	cl1 = 2.0466534158929769769591032497785f * _ZHCoeffs.y;	// sqrt(4PI/3)
	float	cl2 = 1.5853309190424044053380115060481f * _ZHCoeffs.z;	// sqrt(4PI/5)

	float	f0 = cl0 * 0.28209479177387814347403972578039f;	// 0.5 / sqrt(PI);
	float	f1 = cl1 * 0.48860251190291992158638462283835f;	// 0.5 * sqrt(3.0/PI);
	float	f2 = cl2 * 1.0925484305920790705433857058027f;	// 0.5 * sqrt(15.0/PI);
	_Coeffs[0] = f0;
	_Coeffs[1] = -f1 * _Direction.x;
	_Coeffs[2] = f1 * _Direction.y;
	_Coeffs[3] = -f1 * _Direction.z;
	_Coeffs[4] = f2 * _Direction.x * _Direction.z;
	_Coeffs[5] = -f2 * _Direction.x * _Direction.y;
	_Coeffs[6] = f2 * 0.28209479177387814347403972578039f * (3.0f * _Direction.y*_Direction.y - 1.0f);
	_Coeffs[7] = -f2 * _Direction.z * _Direction.y;
	_Coeffs[8] = f2 * 0.5f * (_Direction.z*_Direction.z - _Direction.x*_Direction.x);
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
