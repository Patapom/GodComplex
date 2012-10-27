#include "../../GodComplex.h"
#include "EffectRoom.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectRoom::EffectRoom( Texture2D& _RTTarget ) : m_ErrorCode( 0 ), m_RTTarget( _RTTarget )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_ROOM_DISPLAY, VertexFormatP3N3G3T2T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_RENDER_CUBEMAP, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PS" ), 1 );

	//////////////////////////////////////////////////////////////////////////
	// Build the room geometry
//	BuildRoom();

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
 	m_pCB_Object = new CB<CBObject>( gs_Device, 10 );
}

EffectRoom::~EffectRoom()
{
 	delete m_pCB_Object;

	delete m_pMatRenderCubeMap;
 	delete m_pMatDisplay;

	delete m_pRTGeometry;
	delete m_pRTMaterial;
	delete m_pCubeMapCamera;

	delete m_pPrimRoom;
	delete m_pTexLightmap;
}

void	EffectRoom::Render( float _Time, float _DeltaTime )
{
	{	USING_MATERIAL_START( *m_pMatDisplay )

		gs_Device.SetRenderTarget( m_RTTarget, &gs_Device.DefaultDepthStencil() );
		gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, NULL );

		m_pTexLightmap->SetPS( 10 );

		// Render the room
		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( _TV(1.0f) * _Time, NjFloat3::UnitY ), NjFloat3::One );
		m_pCB_Object->UpdateData();

		m_pPrimRoom->Render( *m_pMatDisplay );

		USING_MATERIAL_END
	}
}

/*

void	EffectRoom::BuildRoom()
{
	// Create the room DISPLAY geometry
	NjFloat3	pPositions[8] =
	{
		NjFloat3( -ROOM_SIZE, 0.0f, ROOM_SIZE ),
		NjFloat3( +ROOM_SIZE, 0.0f, ROOM_SIZE ),
		NjFloat3( +ROOM_SIZE, ROOM_HEIGHT, ROOM_SIZE ),
		NjFloat3( -ROOM_SIZE, ROOM_HEIGHT, ROOM_SIZE ),

		NjFloat3( -ROOM_SIZE, 0.0f, -ROOM_SIZE ),
		NjFloat3( +ROOM_SIZE, 0.0f, -ROOM_SIZE ),
		NjFloat3( +ROOM_SIZE, ROOM_HEIGHT, -ROOM_SIZE ),
		NjFloat3( -ROOM_SIZE, ROOM_HEIGHT, -ROOM_SIZE ),
	};

	VertexFormatP3N3G3T2T2	pVertices[4*6] =
	{
		// Front
		{ pPositions[1], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 4, NjFloat2( 0, 1 ), true ) },
		{ pPositions[0], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 4, NjFloat2( 1, 1 ), true ) },
		{ pPositions[3], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 4, NjFloat2( 1, 0 ), true ) },
		{ pPositions[2], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 4, NjFloat2( 0, 0 ), true ) },
		// Back
		{ pPositions[4],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 5, NjFloat2( 0, 1 ), true ) },
		{ pPositions[5],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 5, NjFloat2( 1, 1 ), true ) },
		{ pPositions[6],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 5, NjFloat2( 1, 0 ), true ) },
		{ pPositions[7],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 5, NjFloat2( 0, 0 ), true ) },

		// Left
		{ pPositions[0],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 0, 0 ), LightUV( 2, NjFloat2( 0, 1 ), true ) },
		{ pPositions[4],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 1, 0 ), LightUV( 2, NjFloat2( 1, 1 ), true ) },
		{ pPositions[7],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 1, 1 ), LightUV( 2, NjFloat2( 1, 0 ), true ) },
		{ pPositions[3],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 0, 1 ), LightUV( 2, NjFloat2( 0, 0 ), true ) },
		// Right
		{ pPositions[5], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 0, 0 ), LightUV( 3, NjFloat2( 0, 1 ), true ) },
		{ pPositions[1], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 1, 0 ), LightUV( 3, NjFloat2( 1, 1 ), true ) },
		{ pPositions[2], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 1, 1 ), LightUV( 3, NjFloat2( 1, 0 ), true ) },
		{ pPositions[6], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 0, 1 ), LightUV( 3, NjFloat2( 0, 0 ), true ) },

		// Floor
		{ pPositions[0],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 1, NjFloat2( 0, 1 ), true ) },
		{ pPositions[1],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 1, NjFloat2( 1, 1 ), true ) },
		{ pPositions[5],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 1, NjFloat2( 1, 0 ), true ) },
		{ pPositions[4],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 1, NjFloat2( 0, 0 ), true ) },
		// Ceiling
		{ pPositions[7], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 0, NjFloat2( 0, 1 ), true ) },
		{ pPositions[6], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 0, NjFloat2( 1, 1 ), true ) },
		{ pPositions[2], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 0, NjFloat2( 1, 0 ), true ) },
		{ pPositions[3], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 0, NjFloat2( 0, 0 ), true ) },
	};

	U16	pIndices[6*6];
	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		pIndices[6*FaceIndex+0] = 4 * FaceIndex + 0;
		pIndices[6*FaceIndex+1] = 4 * FaceIndex + 1;
		pIndices[6*FaceIndex+2] = 4 * FaceIndex + 2;
		pIndices[6*FaceIndex+3] = 4 * FaceIndex + 0;
		pIndices[6*FaceIndex+4] = 4 * FaceIndex + 2;
		pIndices[6*FaceIndex+5] = 4 * FaceIndex + 3;
	}

	m_pPrimRoom = new Primitive( gs_Device, 24, pVertices, 36, pIndices, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, VertexFormatP3N3G3T2T2::DESCRIPTOR );
}

static const int	LIGHT_SOURCES_COUNT = 1;		// For the moment, only one light which is the ceiling !
static const int	THETA_SUBDIVISIONS_COUNT = 32;	// Generate 32 rays along theta, twice as much along phi
static const int	PHI_SUBDIVISIONS_COUNT = 2*THETA_SUBDIVISIONS_COUNT;

static const float	OFF_SURFACE = 0.01f;			// A tiny offset to move the ray off the surface and avoid precision problems

EffectRoom::MaterialDescriptor	EffectRoom::ms_pMaterials[] =
{
	{ -1, 0.1f * NjFloat3( 1, 1, 1 ) },		// #0 Floor material

	{ -1, 0.1f * NjFloat3( 1, 1, 1 ) },		// #1 Left wall material
	{ -1, 0.1f * NjFloat3( 1, 1, 1 ) },		// #2 Right wall material
	{ -1, 0.1f * NjFloat3( 1, 1, 1 ) },		// #3 Front wall material
	{ -1, 0.1f * NjFloat3( 1, 1, 1 ) },		// #4 Back wall material

	{ -1, NjFloat3::Zero },
	{ -1, NjFloat3::Zero },
	{ -1, NjFloat3::Zero },
	{ -1, NjFloat3::Zero },
	{ -1, NjFloat3::Zero },

	{ 0, 4.0f * NjFloat3( 1, 1, 1 ) },		// #10 Ceiling as a light source
};

void	EffectRoom::RenderLightmap( IntroProgressDelegate& _Delegate )
{
	float	HalfWidth = 0.5f * ROOM_SIZE;
	float	HalfHeight = 0.5f * ROOM_HEIGHT;

	// Create the room's ray-trace geometry
	RayTracer	Tracer;

	RayTracer::Quad	pQuads[6] =
	{
		{ NjFloat3( 0.0f, ROOM_HEIGHT, 0.0f ), -NjFloat3::UnitY, NjFloat3::UnitX, NjFloat2( ROOM_SIZE, ROOM_SIZE ), 10 },			// Ceiling

		{ NjFloat3( 0.0f, 0.0f, 0.0f ), NjFloat3::UnitY, NjFloat3::UnitX, NjFloat2( ROOM_SIZE, ROOM_SIZE ), 0 },					// Floor
		{ NjFloat3( -HalfWidth, HalfHeight, 0.0f ), NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( ROOM_SIZE, ROOM_HEIGHT ), 1 },		// Left
		{ NjFloat3( +HalfWidth, HalfHeight, 0.0f ), -NjFloat3::UnitX, NjFloat3::UnitZ, NjFloat2( ROOM_SIZE, ROOM_HEIGHT ), 2 },		// Right
		{ NjFloat3( 0.0f, HalfHeight, +HalfWidth ), -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( ROOM_SIZE, ROOM_HEIGHT ), 3 },	// Front
		{ NjFloat3( 0.0f, HalfHeight, -HalfWidth ), NjFloat3::UnitZ, NjFloat3::UnitX, NjFloat2( ROOM_SIZE, ROOM_HEIGHT ), 4 },		// Back
	};

	Tracer.InitGeometry( 6, pQuads );

	// Create the lightmap's inverse WORLD textures
	// Indeed, each texel in the lightmap is mapped to a unique position and has a unique normal from which to cast rays
	// We need to build these "inverse textures" and we do that by simply drawing the quads in a texture similar to the lightmap, writing WORLD position and normal...
	NjFloat2	AspectRatio = GetLightMapAspectRatios();
	int			LightMapWidth = LIGHTMAP_SIZE;
// 	int			LightMapHeight = floorf( LIGHTMAP_SIZE * AspectRatio.x );
	int			LightMapHeight = LIGHTMAP_SIZE;
	NjFloat2	LightMapSize;
	LightMapSize.x = float(LightMapWidth);
	LightMapSize.y = float(LightMapHeight);

	TextureBuilder	TBWorldPosition( LightMapWidth, LightMapHeight );
	TextureBuilder	TBWorldNormal( LightMapWidth, LightMapHeight );
	TextureBuilder	TBWorldTangent( LightMapWidth, LightMapHeight );

	DrawUtils	DrawPosition;
	DrawPosition.SetupSurface( LightMapWidth, LightMapHeight, TBWorldPosition.GetMips()[0] );
	DrawUtils	DrawNormal;
	DrawNormal.SetupSurface( LightMapWidth, LightMapHeight, TBWorldNormal.GetMips()[0] );
	DrawUtils	DrawTangent;
	DrawTangent.SetupSurface( LightMapWidth, LightMapHeight, TBWorldTangent.GetMips()[0] );

	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 0, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 0, NjFloat2( 1, 1 ) ), pQuads[0] );	// Ceiling
	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 1, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 1, NjFloat2( 1, 1 ) ), pQuads[1] );	// Floor
	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 2, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 2, NjFloat2( 1, 1 ) ), pQuads[2] );	// Left
 	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 3, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 3, NjFloat2( 1, 1 ) ), pQuads[3] );	// Right
 	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 4, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 4, NjFloat2( 1, 1 ) ), pQuads[4] );	// Front
 	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 5, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 5, NjFloat2( 1, 1 ) ), pQuads[5] );	// Back

// 	// Generate an amount of unbiased rays uniformly spread across the 2PI hemisphere
// 	_randpushseed();
// 	_srand( RAND_DEFAULT_SEED_U, RAND_DEFAULT_SEED_V );
// 
// 	int			RaysCount = PHI_SUBDIVISIONS_COUNT*THETA_SUBDIVISIONS_COUNT;
// 	NjFloat3*	pRays = new NjFloat3[RaysCount];
// 	for ( int Y=0; Y < THETA_SUBDIVISIONS_COUNT; Y++ )
// 	{
// 		NjFloat3*	pRay = pRays + PHI_SUBDIVISIONS_COUNT * Y;
// 		for ( int X=0; X < PHI_SUBDIVISIONS_COUNT; X++, pRay++ )
// 		{
// 			float	Phi = TWOPI * (X+_frandStrict()) / PHI_SUBDIVISIONS_COUNT;
// 			float	Theta = asinf( sqrtf( (Y+_frandStrict()) / THETA_SUBDIVISIONS_COUNT ) );
// 
// 			pRay->Set(
// 					sinf(Theta) * sinf(Phi),
// 					cosf(Theta),
// 					sinf(Theta) * cosf(Phi)
// 				);
// 		}
// 	}
// 
// 	_randpopseed();
// 
// 	// Perform actual rendering
// 	TextureBuilder*	ppTBLightMaps0[LIGHT_SOURCES_COUNT];
// 	TextureBuilder*	ppTBLightMaps1[LIGHT_SOURCES_COUNT];
// 
// 	for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
// 	{
// 		ppTBLightMaps0[LightIndex] = new TextureBuilder( LightMapWidth, LightMapHeight );
// 		ppTBLightMaps1[LightIndex] = new TextureBuilder( LightMapWidth, LightMapHeight );
// 	}
// 
// //	RenderDirect( Tracer, TBWorldPosition, TBWorldNormal, TBWorldTangent, RaysCount, pRays, ppTBLightMaps0 );
// 
// 	delete[] pRays;



	//////////////////////////////////////////////////////////////////////////
	// Create the cube map rendering data
	m_pRTMaterial = new Texture2D( gs_Device, LIGHTMAP_CUBEMAP_SIZE, LIGHTMAP_CUBEMAP_SIZE, 6, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pRTGeometry = new Texture2D( gs_Device, LIGHTMAP_CUBEMAP_SIZE, LIGHTMAP_CUBEMAP_SIZE, 6, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pRTStagingMaterial = new Texture2D( gs_Device, LIGHTMAP_CUBEMAP_SIZE, LIGHTMAP_CUBEMAP_SIZE, 6, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	m_pRTStagingGeometry = new Texture2D( gs_Device, LIGHTMAP_CUBEMAP_SIZE, LIGHTMAP_CUBEMAP_SIZE, 6, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	m_pDepthStencilCubeMap = new Texture2D( gs_Device, LIGHTMAP_CUBEMAP_SIZE, LIGHTMAP_CUBEMAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );
	m_pCubeMapCamera = new Camera( gs_Device );

	// Perform actual rendering
// 	TextureBuilder*	ppTBLightMaps0[LIGHT_SOURCES_COUNT];
// 	TextureBuilder*	ppTBLightMaps1[LIGHT_SOURCES_COUNT];
// 	RenderDirect( TBWorldPosition, TBWorldNormal, TBWorldTangent, ppTBLightMaps0 );


	// Finally, build the lightmap with its content
	m_pTexLightmap = new Texture2D( gs_Device, LightMapWidth, LightMapHeight, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, TBWorldPosition.Convert( PixelFormatRGBA16F::DESCRIPTOR ) );

	//////////////////////////////////////////////////////////////////////////
	// Release the shit
	delete m_pCubeMapCamera;
	delete m_pDepthStencilCubeMap;
	delete m_pRTGeometry;
	delete m_pRTMaterial;
	delete m_pRTStagingGeometry;
	delete m_pRTStagingMaterial;
}

void	EffectRoom::RenderDirect( TextureBuilder& _Positions, TextureBuilder& _Normals, TextureBuilder& _Tangents, TextureBuilder** _ppLightMaps )
{
	NjFloat3	Position, Normal, Tangent;
	NjFloat4*	ppTarget[LIGHT_SOURCES_COUNT];

	for ( int Y=0; Y < H; Y++ )
	{
		NjFloat4*	pPosition = _Positions.GetMips()[0] + W * Y;
		NjFloat4*	pNormal = _Normals.GetMips()[0] + W * Y;
		NjFloat4*	pTangent = _Tangents.GetMips()[0] + W * Y;
		for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
			ppTarget[LightIndex] = _ppLightMaps[LightIndex]->GetMips()[0] + W * Y;

		for ( int X=0; X < W; X++ )
		{
			// Initialize the tangential base
			Normal.x = pNormal->x;
			Normal.y = pNormal->y;
			Normal.z = pNormal->z;

			Tangent.x = pTangent->x;
			Tangent.y = pTangent->y;
			Tangent.z = pTangent->z;

			BiTangent = Normal ^ Tangent;

			// Start a little off-surface
			Position.x = pPosition->x + OFF_SURFACE * Normal.x;
			Position.y = pPosition->y + OFF_SURFACE * Normal.y;
			Position.z = pPosition->z + OFF_SURFACE * Normal.z;

			// Reset irradiance !
			for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
				ppTarget[LightIndex]->Set( 0, 0, 0, 0 );

			// Render cube map
			RenderCubeMap( Position, Normal, BiTangent, 0.1f, 20.0f );

			// Read back results
			ReadBack( ppTarget );
		}
	}
}

void	EffectRoom::RenderCubeMap( const NjFloat3& _Position, const NjFloat3& _At, const NjFloat3& _Up, float _Near, float _Far )
{
	m_pCubeMapCamera->SetPerspective( HALFPI, 1.0f, _Near, _Far );

	// Create the side transforms
	NjFloat4x4	SideTransforms[] =
	{
		NjFloat4x4::RotationY( +HALFPI ),	// +X (look right)
		NjFloat4x4::RotationY( -HALFPI ),	// -X (look left)
		NjFloat4x4::RotationX( -HALFPI ),	// +Y (look up)
		NjFloat4x4::RotationX( +HALFPI ),	// -Y (look down)
		NjFloat4x4::Identity,				// +Z (look front) (default)
		NjFloat4x4::RotationY( +PI ),		// -Z (look back)
	};

	// Create the main camera matrix
	m_pCubeMapCamera->LookAt( _Position, _Position + _At, _Up );
	NjFloat4x4&	Camera2World = m_pCubeMapCamera->GetCB().Camera2World;

	NjFloat4x4	DefaultCamera2World = Camera2World;	// Keep a backup of the original transform

	// Setup the identity for the room's transform
	m_pCB_Object->m.Local2World = NjFloat4x4::Identity;
	m_pCB_Object->UpdateData();

	// Start rendering
	USING_MATERIAL_START( *m_pMatRenderCubeMap )
	gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, NULL );

	ID3D11RenderTargetView*	ppTargets[2];
	ID3D11DepthStencilView*	pDepthStencil = m_pDepthStencilCubeMap->GetDepthStencilView();

	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		// Render into the array texture corresponding to the cube map face
		ppTargets[0] = m_pRTMaterial->GetTargetView( 0, FaceIndex, 1 );
		ppTargets[1] = m_pRTGeometry->GetTargetView( 0, FaceIndex, 1 );
		gs_Device.SetRenderTargets( m_pRTMaterial->GetWidth(), m_pRTMaterial->GetHeight(), 2, ppTargets, pDepthStencil );

		// Update camera transform
		NjFloat4x4	Side2World = SideTransforms[FaceIndex] * DefaultCamera2World;
		Camera2World = Side2World;
		m_pCubeMapCamera->UpdateCompositions();
		m_pCubeMapCamera->Upload( 0 );

		// Render the room !
		m_pPrimRoom->Render( *m_pMatDisplay );
	}
	USING_MATERIAL_END
}

void	EffectRoom::ReadBack( NjFloat4** _ppTarget )
{
	m_pRTStagingMaterial->CopyFrom( m_pRTMaterial );
	m_pRTStagingGeometry->CopyFrom( m_pRTGeometry );

	m_pRTStagingMaterial->Map( 0, 0 );
	m_pRTStagingMaterial->UnMap( 0, 0 );
}

void	EffectRoom::RenderDirectOLD( RayTracer& _Tracer, TextureBuilder& _Positions, TextureBuilder& _Normals, TextureBuilder& _Tangents, int _RaysCount, NjFloat3* _pRays, TextureBuilder** _ppLightMaps )
{
	int	W = _Positions.GetWidth();
	int	H = _Positions.GetHeight();

	NjFloat4*		ppTarget[LIGHT_SOURCES_COUNT];

	RayTracer::Ray	Ray;
	NjFloat3		Normal, Tangent, BiTangent;
	float			RadianceFactor = TWOPI / _RaysCount;	// Assuming no bias in the rays' distribution

	for ( int Y=0; Y < H; Y++ )
	{
		NjFloat4*	pPosition = _Positions.GetMips()[0] + W * Y;
		NjFloat4*	pNormal = _Normals.GetMips()[0] + W * Y;
		NjFloat4*	pTangent = _Tangents.GetMips()[0] + W * Y;
		for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
			ppTarget[LightIndex] = _ppLightMaps[LightIndex]->GetMips()[0] + W * Y;

		for ( int X=0; X < W; X++ )
		{
			// Initialize the tangential base
			Normal.x = pNormal->x;
			Normal.y = pNormal->y;
			Normal.z = pNormal->z;

			Tangent.x = pTangent->x;
			Tangent.y = pTangent->y;
			Tangent.z = pTangent->z;

			BiTangent = Normal ^ Tangent;

			// Start a little off-surface
			Ray.Position.x = pPosition->x + OFF_SURFACE * Normal.x;
			Ray.Position.y = pPosition->y + OFF_SURFACE * Normal.y;
			Ray.Position.z = pPosition->z + OFF_SURFACE * Normal.z;

			// Reset irradiance !
			for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
				ppTarget[LightIndex]->Set( 0, 0, 0, 0 );

			NjFloat3*	pRay = _pRays;
			for ( int RayIndex=0; RayIndex < _RaysCount; RayIndex++, pRay++ )
			{
				Ray.Direction = pRay->x * Tangent + pRay->y * Normal + pRay->z * BiTangent;
				if ( !_Tracer.Trace( Ray ) )
				{	// This can't happen since we're in a closed environment !
					ASSERT( false, "WTF?!" );
					return;
				}

				const MaterialDescriptor&	Mat = ms_pMaterials[Ray.pHitQuad->MaterialID];
				if ( Mat.LightSourceIndex < 0 )
					continue;	// We didn't hit the any emissive material !

				float		CosTheta = pRay->y;	// Simple...
				float		CosTheta2 = -(Ray.Direction | Ray.pHitQuad->Normal);	// Assuming light is biased toward normal... Perfect diffuse emitter !
				float		Factor = CosTheta * CosTheta2 * RadianceFactor;
				NjFloat3	Radiance = Factor * Mat.Color;

				NjFloat4&	Irradiance = *ppTarget[Mat.LightSourceIndex];

				Irradiance.x += Factor * Radiance.x;
				Irradiance.y += Factor * Radiance.y;
				Irradiance.z += Factor * Radiance.z;
			}

			// Next texel...
			pPosition++;
			pNormal++;
			pTangent++;
		}
	}
}

struct	 __DUMMY
{
	NjFloat3	Pos, DeltaPosU, DeltaPosV;
	NjFloat3	Normal, Tangent;
};

static void	FillPosition( const DrawUtils::DrawInfos& _Infos, DrawUtils::Pixel& _Pixel )
{
	__DUMMY*	pData = (__DUMMY*) _Infos.pData;

//	_Pixel.RGBA.Set( 1, 0, 0, 0 );
// 	_Pixel.RGBA.x = _Infos.UV.x;
// 	_Pixel.RGBA.y = _Infos.UV.y;
	NjFloat3	Position = pData->Pos + _Infos.UV.x * pData->DeltaPosU + _Infos.UV.y * pData->DeltaPosV;
	_Pixel.Blend( NjFloat4( Position, 1.0f ), _Infos.Coverage );
}

static void	FillNormal( const DrawUtils::DrawInfos& _Infos, DrawUtils::Pixel& _Pixel )
{
	__DUMMY*	pData = (__DUMMY*) _Infos.pData;
	_Pixel.RGBA.x = pData->Normal.x;
	_Pixel.RGBA.y = pData->Normal.y;
	_Pixel.RGBA.z = pData->Normal.z;
}

static void	FillTangent( const DrawUtils::DrawInfos& _Infos, DrawUtils::Pixel& _Pixel )
{
	__DUMMY*	pData = (__DUMMY*) _Infos.pData;
	_Pixel.RGBA.x = pData->Tangent.x;
	_Pixel.RGBA.y = pData->Tangent.y;
	_Pixel.RGBA.z = pData->Tangent.z;
}

void	EffectRoom::DrawQuad( DrawUtils& _DrawPosition, DrawUtils& _DrawNormal, DrawUtils& _DrawTangent, const NjFloat2& _TopLeft, const NjFloat2& _BottomRight, const RayTracer::Quad& _Quad )
{
	float	x = _TopLeft.x;
	float	y = _TopLeft.y;
	float	w = _BottomRight.x - _TopLeft.x;
	float	h = _BottomRight.y - _TopLeft.y;

	__DUMMY	Data;
	Data.Normal = _Quad.Normal;
	Data.Tangent = _Quad.Tangent;

	NjFloat3	QuadTangent = _Quad.Tangent;
	QuadTangent.Normalize();
	NjFloat3	QuadBiTangent = _Quad.Normal ^ _Quad.Tangent;
	QuadBiTangent.Normalize();

	Data.DeltaPosV = _Quad.Size.x * _Quad.Tangent;	// U in Rectangle space actually means V in Room space
	Data.DeltaPosU = -_Quad.Size.y * QuadBiTangent;	// V in Rectangle space actually means U in Room space
	Data.Pos = _Quad.Center - 0.5f * (Data.DeltaPosU + Data.DeltaPosV);

	_DrawPosition.DrawRectangle( x, y, w, h, 1.0f, 0.0f, FillPosition, (void*) &Data );
	_DrawNormal.DrawRectangle( x, y, w, h, 1.0f, 0.0f, FillNormal, (void*) &Data );
	_DrawNormal.DrawRectangle( x, y, w, h, 1.0f, 0.0f, FillTangent, (void*) &Data );
}

// At the moment, the room's lightmap is organized like this :
//
//           Floor      Left  Right 
//   | +----------------+----+----+
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//   | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
// U | +----------------+----+----+
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//   | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//	 | |                |    |    |
//   V +----------------+----+----+
//          Ceiling     Front Back
//     --------------------------->
//                         V
//
// This function returns the size of the Floor and of the Height reported to a unit width
// i.e.  X=Floor/(Floor+2*Height)  Y=Height/(Floor+2*Height)
//
NjFloat2	EffectRoom::GetLightMapAspectRatios()
{
	float	FloorSize = 1.0f;						// Assuming floor size is the unity
	float	RoomHeight = ROOM_HEIGHT / ROOM_SIZE;	// We retrieve the relative height of the room

	float	Width = FloorSize + 2.0f * RoomHeight;	// This is the width of the light map

	// Then, we simply need to renormalize to obtain the ratios
	return NjFloat2( FloorSize / Width, RoomHeight / Width );
}

// This returns the UV coordinates of the room's faces inside the lightmap
//
NjFloat2	EffectRoom::LightUV( int _FaceIndex, const NjFloat2& _UV, bool _bBias )
{
	NjFloat2	AspectRatios = GetLightMapAspectRatios();

	NjFloat2	pFaceLightUVStart[6] =
	{
		NjFloat2( 0, AspectRatios.x ),										// Ceiling
		NjFloat2( 0, 0 ),													// Floor
		NjFloat2( AspectRatios.x, 0 ),										// Left
		NjFloat2( AspectRatios.x + 1.0f * AspectRatios.y, 0 ),				// Right
		NjFloat2( AspectRatios.x, AspectRatios.x ),							// Front
		NjFloat2( AspectRatios.x + 1.0f * AspectRatios.y, AspectRatios.x ),	// Back
	};
	NjFloat2	pFaceLightUVEnd[6] =
	{
		NjFloat2( AspectRatios.x, 1 ),										// Ceiling
		NjFloat2( AspectRatios.x, AspectRatios.x ),							// Floor
		NjFloat2( AspectRatios.x + 1.0f * AspectRatios.y, AspectRatios.x ),	// Left
		NjFloat2( AspectRatios.x + 2.0f * AspectRatios.y, AspectRatios.x ),	// Right
		NjFloat2( AspectRatios.x + 1.0f * AspectRatios.y, 1 ),				// Front
		NjFloat2( AspectRatios.x + 2.0f * AspectRatios.y, 1 ),				// Back (Should be equal to (1,1))
	};

	NjFloat2	UV = pFaceLightUVStart[_FaceIndex];
	NjFloat2	dUV = pFaceLightUVEnd[_FaceIndex] - UV;

	NjFloat2	InsideUV = _UV;
	if ( _bBias )
	{
		InsideUV = InsideUV - 0.5f * NjFloat2::One;
		InsideUV = InsideUV * 0.95f;
		InsideUV = InsideUV + 0.5f * NjFloat2::One;
	}

	return NjFloat2( UV.x + InsideUV.y * dUV.x, UV.y + InsideUV.x * dUV.y );
}
*/
