#include "../../GodComplex.h"
#include "EffectRoom.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectRoom::EffectRoom( Texture2D& _RTTarget ) : m_ErrorCode( 0 ), m_RTTarget( _RTTarget )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_ROOM_DISPLAY, VertexFormatP3N3G3T2T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );

	//////////////////////////////////////////////////////////////////////////
	// Build the room geometry
	BuildRoom();

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
 	m_pCB_Object = new CB<CBObject>( gs_Device, 10 );
}

EffectRoom::~EffectRoom()
{
 	delete m_pCB_Object;

 	delete m_pMatDisplay;

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

void	EffectRoom::BuildRoom()
{
	// Create the room DISPLAY geometry
	float	RoomWidth = ROOM_HEIGHT * GOLDEN_RATIO;	// The width & Depth of the room

	{	// Build actual display geometry
		NjFloat3	pPositions[8] =
		{
			NjFloat3( -RoomWidth, 0.0f, RoomWidth ),
			NjFloat3( +RoomWidth, 0.0f, RoomWidth ),
			NjFloat3( +RoomWidth, ROOM_HEIGHT, RoomWidth ),
			NjFloat3( -RoomWidth, ROOM_HEIGHT, RoomWidth ),

			NjFloat3( -RoomWidth, 0.0f, -RoomWidth ),
			NjFloat3( +RoomWidth, 0.0f, -RoomWidth ),
			NjFloat3( +RoomWidth, ROOM_HEIGHT, -RoomWidth ),
			NjFloat3( -RoomWidth, ROOM_HEIGHT, -RoomWidth ),
		};

		VertexFormatP3N3G3T2T2	pVertices[4*6] =
		{
			// Front
			{ pPositions[1], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 3, NjFloat2( 0, 1 ) ) },
			{ pPositions[0], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 3, NjFloat2( 1, 1 ) ) },
			{ pPositions[3], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 3, NjFloat2( 1, 0 ) ) },
			{ pPositions[2], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 3, NjFloat2( 0, 0 ) ) },
			// Back
			{ pPositions[4],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 4, NjFloat2( 0, 1 ) ) },
			{ pPositions[5],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 4, NjFloat2( 1, 1 ) ) },
			{ pPositions[6],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 4, NjFloat2( 1, 0 ) ) },
			{ pPositions[7],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 4, NjFloat2( 0, 0 ) ) },

			// Left
			{ pPositions[0],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 0, 0 ), LightUV( 1, NjFloat2( 0, 1 ) ) },
			{ pPositions[4],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 1, 0 ), LightUV( 1, NjFloat2( 1, 1 ) ) },
			{ pPositions[7],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 1, 1 ), LightUV( 1, NjFloat2( 1, 0 ) ) },
			{ pPositions[3],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 0, 1 ), LightUV( 1, NjFloat2( 0, 0 ) ) },
			// Right
			{ pPositions[5], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 0, 0 ), LightUV( 2, NjFloat2( 0, 1 ) ) },
			{ pPositions[1], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 1, 0 ), LightUV( 2, NjFloat2( 1, 1 ) ) },
			{ pPositions[2], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 1, 1 ), LightUV( 2, NjFloat2( 1, 0 ) ) },
			{ pPositions[6], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 0, 1 ), LightUV( 2, NjFloat2( 0, 0 ) ) },

			// Floor
			{ pPositions[0],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 0, NjFloat2( 0, 1 ) ) },
			{ pPositions[1],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 0, NjFloat2( 1, 1 ) ) },
			{ pPositions[5],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 0, NjFloat2( 1, 0 ) ) },
			{ pPositions[4],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 0, NjFloat2( 0, 0 ) ) },
			// Ceiling
			{ pPositions[7], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 5, NjFloat2( 0, 0 ) ) },
			{ pPositions[6], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 5, NjFloat2( 0, 0 ) ) },
			{ pPositions[2], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 5, NjFloat2( 0, 0 ) ) },
			{ pPositions[3], -NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 5, NjFloat2( 0, 0 ) ) },
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
}

static const int	LIGHT_SOURCES_COUNT = 1;		// For the moment, only one light which is the ceiling !
static const int	THETA_SUBDIVISIONS_COUNT = 32;	// Generate 32 rays along theta, twice as much along phi
static const int	PHI_SUBDIVISIONS_COUNT = 2*THETA_SUBDIVISIONS_COUNT;

void	EffectRoom::RenderLightmap( IntroProgressDelegate& _Delegate )
{
	float	RoomWidth = ROOM_HEIGHT * GOLDEN_RATIO;	// The width & Depth of the room
	float	HalfWidth = 0.5f * RoomWidth;
	float	HalfHeight = 0.5f * ROOM_HEIGHT;

	// Create the room's ray-trace geometry
	RayTracer	Tracer;

	RayTracer::Quad	pQuads[6] =
	{
		{ NjFloat3( 0.0f, ROOM_HEIGHT, 0.0f ), -NjFloat3::UnitY, NjFloat3::UnitX, NjFloat2( RoomWidth, RoomWidth ), 10 },			// Ceiling

		{ NjFloat3( 0.0f, 0.0f, 0.0f ), NjFloat3::UnitY, NjFloat3::UnitX, NjFloat2( RoomWidth, RoomWidth ), 0 },					// Floor
		{ NjFloat3( -HalfWidth, HalfHeight, 0.0f ), NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( RoomWidth, ROOM_HEIGHT ), 1 },		// Left
		{ NjFloat3( +HalfWidth, HalfHeight, 0.0f ), -NjFloat3::UnitX, NjFloat3::UnitZ, NjFloat2( RoomWidth, ROOM_HEIGHT ), 2 },		// Right
		{ NjFloat3( 0.0f, HalfHeight, +HalfWidth ), -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( RoomWidth, ROOM_HEIGHT ), 3 },	// Front
		{ NjFloat3( 0.0f, HalfHeight, -HalfWidth ), NjFloat3::UnitZ, NjFloat3::UnitX, NjFloat2( RoomWidth, ROOM_HEIGHT ), 4 },		// Back
	};

	Tracer.InitGeometry( 6, pQuads );

	// Create the lightmap's inverse WORLD textures
	// Indeed, each texel in the lightmap is mapped to a unique position and has a unique normal from which to cast rays
	// We need to build these "inverse textures" and we do that by simply drawing the quads in a texture similar to the lightmap, writing WORLD position and normal...
	NjFloat2	AspectRatio = GetLightMapAspectRatios();
	int			LightMapWidth = LIGHTMAP_SIZE;
	int			LightMapHeight = floorf( LIGHTMAP_SIZE * AspectRatio.x );
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

	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 0, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 0, NjFloat2( 1, 1 ) ), pQuads[1] );	// Floor
	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 1, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 1, NjFloat2( 1, 1 ) ), pQuads[2] );	// Left
 	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 2, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 2, NjFloat2( 1, 1 ) ), pQuads[3] );	// Right
 	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 3, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 3, NjFloat2( 1, 1 ) ), pQuads[4] );	// Front
 	DrawQuad( DrawPosition, DrawNormal, DrawTangent, LightMapSize * LightUV( 4, NjFloat2( 0, 0 ) ), LightMapSize * LightUV( 4, NjFloat2( 1, 1 ) ), pQuads[5] );	// Back

	// Generate an amount of unbiased rays uniformly spread across the 2PI hemisphere
	_randpushseed();
	_srand( RAND_DEFAULT_SEED_U, RAND_DEFAULT_SEED_V );

	int			RaysCount = PHI_SUBDIVISIONS_COUNT*THETA_SUBDIVISIONS_COUNT;
	NjFloat3*	pRays = new NjFloat3[RaysCount];
	for ( int Y=0; Y < THETA_SUBDIVISIONS_COUNT; Y++ )
	{
		NjFloat3*	pRay = pRays + PHI_SUBDIVISIONS_COUNT * Y;
		for ( int X=0; X < PHI_SUBDIVISIONS_COUNT; X++, pRay++ )
		{
			float	Phi = TWOPI * (X+_frandStrict()) / PHI_SUBDIVISIONS_COUNT;
			float	Theta = acosf( sqrtf( (Y+_frandStrict()) / THETA_SUBDIVISIONS_COUNT ) );

			pRay->Set(
					sinf(Theta) * sinf(Phi),
					cosf(Theta),
					sinf(Theta) * cosf(Phi)
				);
		}
	}

	_randpopseed();

	// Perform actual rendering
	TextureBuilder*	ppTBLightMaps0[LIGHT_SOURCES_COUNT];
	TextureBuilder*	ppTBLightMaps1[LIGHT_SOURCES_COUNT];

	for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
	{
		ppTBLightMaps0[LightIndex] = new TextureBuilder( LightMapWidth, LightMapHeight );
		ppTBLightMaps1[LightIndex] = new TextureBuilder( LightMapWidth, LightMapHeight );
	}

	RenderDirect( Tracer, TBWorldPosition, TBWorldNormal, TBWorldTangent, RaysCount, pRays, ppTBLightMaps0 );

	delete[] pRays;

	// Finally, build the lightmap with its content
	m_pTexLightmap = new Texture2D( gs_Device, LightMapWidth, LightMapHeight, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, TBWorldPosition.Convert( PixelFormatRGBA16F::DESCRIPTOR ) );
}

void	EffectRoom::RenderDirect( RayTracer& _Tracer, TextureBuilder& _Positions, TextureBuilder& _Normals, TextureBuilder& _Tangents, int _RaysCount, NjFloat3* _pRays, TextureBuilder** _ppLightMaps )
{
	int	W = _Positions.GetWidth();
	int	H = _Positions.GetHeight();

	NjFloat4*		ppTarget[LIGHT_SOURCES_COUNT];

	RayTracer::Ray	Ray;
	NjFloat3		Normal, Tangent;

	for ( int Y=0; Y < H; Y++ )
	{
		NjFloat4*	pPosition = _Positions.GetMips()[0] + W * Y;
		NjFloat4*	pNormal = _Normals.GetMips()[0] + W * Y;
		NjFloat4*	pTangent = _Normals.GetMips()[0] + W * Y;
		for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
			ppTarget[LightIndex] = _ppLightMaps[LightIndex]->GetMips()[0] + W * Y;

		for ( int X=0; X < W; X++ )
		{
			Ray.Position.x = pPosition->x;
			Ray.Position.y = pPosition->y;
			Ray.Position.z = pPosition->z;

			Normal.x = pNormal->x;
			Normal.y = pNormal->y;
			Normal.z = pNormal->z;

			Tangent.x = pTangent->x;
			Tangent.y = pTangent->y;
			Tangent.z = pTangent->z;



			for ( int LightIndex=0; LightIndex < LIGHT_SOURCES_COUNT; LightIndex++ )
			{
				ppTarget[LightIndex]++;
			}

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
	_Pixel.RGBA.x = Position.x;
	_Pixel.RGBA.y = Position.y;
	_Pixel.RGBA.z = Position.z;
	_Pixel.RGBA.w = 1.0f;	// So we know the texel is not empty !
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
//           Floor      Left Right Front Back
//   | +----------------+----+----+----+----+
//	 | |                |    |    |    |    |
//	 | |                |    |    |    |    |
//	 | |                |    |    |    |    |
//	 | |                |    |    |    |    |
// U | |                |    |    |    |    |
//	 | |                |    |    |    |    |
//	 | |                |    |    |    |    |
//	 | |                |    |    |    |    |
//	 | |                |    |    |    |    |
//   V +----------------+----+----+----+----+
//     ------------------------------------->
//                         V
//
// This function returns the size of the Floor and of the Height reported to a unit width
// i.e.  X=Floor/(Floor+4*Height)  Y=Height/(Floor+4*Height)
//
NjFloat2	EffectRoom::GetLightMapAspectRatios()
{
	float	FloorSize = 1.0f;						// Assuming floor size is the unity
	float	RoomHeight = FloorSize / GOLDEN_RATIO;	// We retrieve the height of the room

	float	Width = FloorSize + 4.0f * RoomHeight;	// This is the width of the light map

	// Then, we simply need to renormalize to obtain the ratios
	return NjFloat2( FloorSize / Width, RoomHeight / Width );
}

// This returns the UV coordinates of the room's faces inside the lightmap
//
NjFloat2	EffectRoom::LightUV( int _FaceIndex, const NjFloat2& _UV )
{
	NjFloat2	AspectRatios = GetLightMapAspectRatios();

	NjFloat2	pFaceLightUVStart[6] =
	{
		NjFloat2( 0, 0 ),
		NjFloat2( AspectRatios.x, 0 ),
		NjFloat2( AspectRatios.x + 1.0f * AspectRatios.y, 0 ),
		NjFloat2( AspectRatios.x + 2.0f * AspectRatios.y, 0 ),
		NjFloat2( AspectRatios.x + 3.0f * AspectRatios.y, 0 ),
		NjFloat2( 0, 0 ),	// Dummy => This face has no mapping in the lightmap since it's the ceiling which IS the light source !
	};
	NjFloat2	pFaceLightUVEnd[6] =
	{
		NjFloat2( AspectRatios.x, 1 ),
		NjFloat2( AspectRatios.x + 1.0f * AspectRatios.y, 1 ),
		NjFloat2( AspectRatios.x + 2.0f * AspectRatios.y, 1 ),
		NjFloat2( AspectRatios.x + 3.0f * AspectRatios.y, 1 ),
		NjFloat2( AspectRatios.x + 4.0f * AspectRatios.y, 1 ),	// Should be equal to (1,1)
		NjFloat2( 0, 0 ),	// Dummy => This face has no mapping in the lightmap since it's the ceiling which IS the light source !
	};

	NjFloat2	UV = pFaceLightUVStart[_FaceIndex];
	NjFloat2	dUV = pFaceLightUVEnd[_FaceIndex] - UV;

	return NjFloat2( UV.x + _UV.y * dUV.x, UV.y + _UV.x * dUV.y );
}
