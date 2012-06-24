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
	// Build textures & render targets
	NjFloat2	AspectRatio = GetLightMapAspectRatios();
	int			LightMapHeight = floorf( LIGHTMAP_SIZE * AspectRatio.x );

	m_pTexLightmap = new Texture2D( gs_Device, LIGHTMAP_SIZE, LightMapHeight, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );


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

//		m_pTexLightmap->SetPS( 10 );

		// Render the room
		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( _TV(0.1f) * _Time, NjFloat3::UnitY ), NjFloat3::One );
// 		m_pCB_Object->m.EmissiveColor = NjFloat4::Zero;
// 		m_pCB_Object->m.NoiseOffset = SphereNoise;
		m_pCB_Object->UpdateData();

		m_pPrimRoom->Render( *m_pMatDisplay );

		USING_MATERIAL_END
	}
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
			{ pPositions[1], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 4, NjFloat2( 0, 0 ) ) },
			{ pPositions[0], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 4, NjFloat2( 1, 0 ) ) },
			{ pPositions[3], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 4, NjFloat2( 1, 1 ) ) },
			{ pPositions[2], -NjFloat3::UnitZ, -NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 4, NjFloat2( 0, 1 ) ) },
			// Back
			{ pPositions[4],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 5, NjFloat2( 0, 0 ) ) },
			{ pPositions[5],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 5, NjFloat2( 1, 0 ) ) },
			{ pPositions[6],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 5, NjFloat2( 1, 1 ) ) },
			{ pPositions[7],  NjFloat3::UnitZ,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 5, NjFloat2( 0, 1 ) ) },

			// Left
			{ pPositions[0],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 0, 0 ), LightUV( 1, NjFloat2( 0, 0 ) ) },
			{ pPositions[4],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 1, 0 ), LightUV( 1, NjFloat2( 1, 0 ) ) },
			{ pPositions[7],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 1, 1 ), LightUV( 1, NjFloat2( 1, 1 ) ) },
			{ pPositions[3],  NjFloat3::UnitX, -NjFloat3::UnitZ, NjFloat2( 0, 1 ), LightUV( 1, NjFloat2( 0, 1 ) ) },
			// Right
			{ pPositions[5], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 0, 0 ), LightUV( 2, NjFloat2( 0, 0 ) ) },
			{ pPositions[1], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 1, 0 ), LightUV( 2, NjFloat2( 1, 0 ) ) },
			{ pPositions[2], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 1, 1 ), LightUV( 2, NjFloat2( 1, 1 ) ) },
			{ pPositions[6], -NjFloat3::UnitX,  NjFloat3::UnitZ, NjFloat2( 0, 1 ), LightUV( 2, NjFloat2( 0, 1 ) ) },

			// Floor
			{ pPositions[0],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 0 ), LightUV( 0, NjFloat2( 0, 0 ) ) },
			{ pPositions[1],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 0 ), LightUV( 0, NjFloat2( 1, 0 ) ) },
			{ pPositions[5],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 1, 1 ), LightUV( 0, NjFloat2( 1, 1 ) ) },
			{ pPositions[4],  NjFloat3::UnitY,  NjFloat3::UnitX, NjFloat2( 0, 1 ), LightUV( 0, NjFloat2( 0, 1 ) ) },
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

void	EffectRoom::RenderLightmap( IntroProgressDelegate& _Delegate )
{
	// Create the room's ray-trace geometry

}
