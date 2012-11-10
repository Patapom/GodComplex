#include "../../GodComplex.h"
#include "EffectRoom.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectRoom::EffectRoom( Texture2D& _RTTarget ) : m_ErrorCode( 0 ), m_RTTarget( _RTTarget )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_ROOM_DISPLAY, VertexFormatP3N3G3T3T3::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatDisplayEmissive = CreateMaterial( IDR_SHADER_ROOM_DISPLAY, VertexFormatP3N3G3T3T3::DESCRIPTOR, "VS", NULL, "PS_Emissive" ), 2 );

	//////////////////////////////////////////////////////////////////////////
	// Build the room geometry & compute lightmaps
	{
		TextureBuilder	TB( 2048, 1024 );
		BuildRoomTextures( TB );
		BuildRoom( TB );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
 	m_pCB_Object = new CB<CBObject>( gs_Device, 10 );
 	m_pCB_Tesselate = new CB<CBTesselate>( gs_Device, 10 );


	// Build animation parameters
	float	LightUpTimeBase = 2.0f;
	float	LightUpTimeVariance = 0.1f;
	m_LightUpTime.Set( _frand( LightUpTimeBase-LightUpTimeVariance, LightUpTimeBase+LightUpTimeVariance ), _frand( LightUpTimeBase-LightUpTimeVariance, LightUpTimeBase+LightUpTimeVariance ), _frand( LightUpTimeBase-LightUpTimeVariance, LightUpTimeBase+LightUpTimeVariance ), _frand( LightUpTimeBase-LightUpTimeVariance, LightUpTimeBase+LightUpTimeVariance ) );
	m_LightFailTimer = NjFloat4::Zero;

// 	// Test tessealation shader
// 	{
// 		CHECK_MATERIAL( m_pMatTestTesselation = CreateMaterial( IDR_SHADER_ROOM_TESSELATION, VertexFormatP3T2::DESCRIPTOR, "VS", "HS", "DS", NULL, "PS" ), 3 );
// 		CHECK_MATERIAL( m_pCSTest = CreateComputeShader( IDR_SHADER_ROOM_TEST_COMPUTE, "CS" ), 4 );
// 		struct	Pipo
// 		{
// 			U32			Constant;
// 			NjFloat3	Color;
// 		};
// 
// 		SB<Pipo>	Output;
// 		Output.Init( gs_Device, 32*32, false );
// 		Output.SetOutput( 0 );
// 
// 		m_pCSTest->Use();
// 		m_pCSTest->Run( 2, 2, 1 );
// 
// 		Output.Read();
// 		delete m_pCSTest );
// 	}
// 
// 	float VERT_OFFSET = 0.2f;
// 	VertexFormatP3T2	pVertices[4] =
// 	{
// // 		{ NjFloat3( -1.0f, VERT_OFFSET+1.0f, 0.0f ), NjFloat2( 0.0f, 0.0f ) },	// Top-left
// // 		{ NjFloat3( -1.0f, VERT_OFFSET-1.0f, 0.0f ), NjFloat2( 0.0f, 1.0f ) },	// Bottom-left
// // 		{ NjFloat3( 1.0f, VERT_OFFSET-1.0f, 0.0f ), NjFloat2( 1.0f, 1.0f ) },	// Bottom-right
// // 		{ NjFloat3( 1.0f, VERT_OFFSET+1.0f, 0.0f ), NjFloat2( 1.0f, 0.0f ) },	// Top-right
// 
// 		{ NjFloat3( -1.0f, VERT_OFFSET+0.0f, -1.0f ), NjFloat2( 0.0f, 0.0f ) },	// Top-left
// 		{ NjFloat3( -1.0f, VERT_OFFSET+0.0f, 1.0f ), NjFloat2( 0.0f, 1.0f ) },	// Bottom-left
// 		{ NjFloat3( 1.0f, VERT_OFFSET+0.0f, 1.0f ), NjFloat2( 1.0f, 1.0f ) },	// Bottom-right
// 		{ NjFloat3( 1.0f, VERT_OFFSET+0.0f, -1.0f ), NjFloat2( 1.0f, 0.0f ) },	// Top-right
// 	};
// 
// //	Primitive*	pPrim = new Primitive( gs_Device, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_4_CONTROL_POINT_PATCH, VertexFormatP3T2::DESCRIPTOR );
// 	m_pPrimTesselatedQuad = new Primitive( gs_Device, 4, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_4_CONTROL_POINT_PATCHLIST, VertexFormatP3T2::DESCRIPTOR );
}

EffectRoom::~EffectRoom()
{
 	delete m_pCB_Object;
 	delete m_pCB_Tesselate;

 	delete m_pMatDisplay;
 	delete m_pMatDisplayEmissive;

	delete m_pPrimRoom;
	delete m_pPrimRoomLights;

	delete m_pTexWalls;
	delete m_pTexLightMaps;

// 	delete m_pMatTestTesselation;
//	delete m_pPrimTesselatedQuad;
}

float	EffectRoom::AnimateFailure( float& _TimerTillFailure, float& _TimeSinceFailure, float& _FailureDuration, float _FailMinTime, float _FailDeltaTime, float _DeltaTime )
{
	float	OldTimer = _TimerTillFailure;
	_TimerTillFailure -= _DeltaTime;
	if ( _TimerTillFailure >= 0.0f )
		return 1.0f;	// Still okay!

	// Animate failure
	if ( OldTimer >= 0.0f )
	{
		_TimeSinceFailure = 0.0f;					// Reset failure animation time
		_FailureDuration = _frand( 1.0f, 2.0f );	// ...and failure duration
	}
	else
		_TimeSinceFailure += _DeltaTime;

	float	t = _TimeSinceFailure / _FailureDuration;
	if ( t > 1.0f )
		_TimerTillFailure = _FailMinTime + _frand() * _FailDeltaTime;	// Finished failure animation => Restart light timer until next failure

	return SATURATE( 10.0f * sinf( 10.0f * t*t ) );

	t = 2.0f * t - 1.0f;
//	return expf( -t*t );
	return 0.0f;
	return 1.0f - SATURATE( 1.0f - 8.0f * expf( -powf(1.1f-abs(t), 8.0f) ) );
}

void	EffectRoom::Render( float _Time, float _DeltaTime )
{
	//////////////////////////////////////////////////////////////////////////
	// Animate lights
	float		LightMaxIntensity = 10.0f;
	float		LightIntensity = LightMaxIntensity * (1.0f - expf( -1.0f * _Time ));
	NjFloat4	LightIntensities = LightIntensity * NjFloat4::One;

	NjFloat4	FailTimeMin( 1.0f, 10.0f, 0.2f, 5.0f );
	NjFloat4	FailTimeDelta( 4.0f, 5.0f, 1.0f, 15.0f );

		// Check for failures
	LightIntensities.x *= AnimateFailure( m_LightFailTimer.x, m_LightFailureTimer.x, m_LightFailureDuration.x, FailTimeMin.x, FailTimeDelta.x, _DeltaTime );
	LightIntensities.y *= AnimateFailure( m_LightFailTimer.y, m_LightFailureTimer.y, m_LightFailureDuration.y, FailTimeMin.y, FailTimeDelta.y, _DeltaTime );
	LightIntensities.z *= AnimateFailure( m_LightFailTimer.z, m_LightFailureTimer.z, m_LightFailureDuration.z, FailTimeMin.z, FailTimeDelta.z, _DeltaTime );
	LightIntensities.w *= AnimateFailure( m_LightFailTimer.w, m_LightFailureTimer.w, m_LightFailureDuration.w, FailTimeMin.w, FailTimeDelta.w, _DeltaTime );

	m_pCB_Object->m.LightColor0.Set( LightIntensities.x, LightIntensities.x, LightIntensities.x, 0 );
	m_pCB_Object->m.LightColor1.Set( LightIntensities.y, LightIntensities.y, LightIntensities.y, 0 );
	m_pCB_Object->m.LightColor2.Set( LightIntensities.x, LightIntensities.x, LightIntensities.x, 0 );
	m_pCB_Object->m.LightColor3.Set( LightIntensities.w, LightIntensities.w, LightIntensities.w, 0 );

	//////////////////////////////////////////////////////////////////////////
	// Display the room
	//
 	gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4::Zero );
	gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
	gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );

	m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( _TV(0.1f) * _Time, NjFloat3::UnitY ), NjFloat3::One );
	m_pCB_Object->UpdateData();

	{	USING_MATERIAL_START( *m_pMatDisplay )
		m_pTexLightMaps->SetPS( 10 );
		m_pTexWalls->SetPS( 11 );
		m_pPrimRoom->Render( *m_pMatDisplay );
		USING_MATERIAL_END
	}
	{	USING_MATERIAL_START( *m_pMatDisplayEmissive )
		m_pPrimRoomLights->Render( *m_pMatDisplayEmissive );
		USING_MATERIAL_END
	}


// 	//////////////////////////////////////////////////////////////////////////
// 	// Test the tesselation!
// 	{
// 		USING_MATERIAL_START( *m_pMatTestTesselation );
// 
// //		gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );
// 		gs_Device.SetStates( gs_Device.m_pRS_WireFrame, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );
// 		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
// 
// 		gs_Device.ClearRenderTarget( gs_Device.DefaultRenderTarget(), NjFloat4::Zero );
// 
// 		m_pCB_Tesselate->m.dUV = gs_Device.DefaultRenderTarget().GetdUV();
// 		m_pCB_Tesselate->m.TesselationFactors.x = _TV( 64.0f );	// Edge tesselation
// 		m_pCB_Tesselate->m.TesselationFactors.y = _TV( 64.0f );	// Inside tesselation
//  		m_pCB_Tesselate->UpdateData();
// 
// 		m_pPrimTesselatedQuad->Render( M );
// 
// 		USING_MATERIAL_END
// 	}
}

void	EffectRoom::BuildRoom( const TextureBuilder& _TB )
{
	//////////////////////////////////////////////////////////////////////////
	// Generate the input informations
	NjFloat2	pSizes[6] =
	{
		NjFloat2( ROOM_SIZE, ROOM_SIZE ),
		NjFloat2( ROOM_SIZE, ROOM_SIZE ),
		NjFloat2( ROOM_SIZE, ROOM_HEIGHT ),
		NjFloat2( ROOM_SIZE, ROOM_HEIGHT ),
		NjFloat2( ROOM_SIZE, ROOM_HEIGHT ),
		NjFloat2( ROOM_SIZE, ROOM_HEIGHT ),
	};
	int			pIntSizes[2*6] =
	{
		LIGHTMAP_SIZE, LIGHTMAP_SIZE,
		LIGHTMAP_SIZE, LIGHTMAP_SIZE,
		LIGHTMAP_SIZE, LIGHTMAP_SIZE/2,
		LIGHTMAP_SIZE, LIGHTMAP_SIZE/2,
		LIGHTMAP_SIZE, LIGHTMAP_SIZE/2,
		LIGHTMAP_SIZE, LIGHTMAP_SIZE/2,
	};
	NjFloat3	pCenters[6] =
	{
		NjFloat3( 0.0f, ROOM_HEIGHT, 0.0f ),
		NjFloat3( 0.0f, 0.0f, 0.0f ),
		NjFloat3( -0.5f * ROOM_SIZE, 0.5f * ROOM_HEIGHT, 0.0f ),
		NjFloat3( +0.5f * ROOM_SIZE, 0.5f * ROOM_HEIGHT, 0.0f ),
		NjFloat3( 0.0f, 0.5f * ROOM_HEIGHT, -0.5f * ROOM_SIZE ),
		NjFloat3( 0.0f, 0.5f * ROOM_HEIGHT, +0.5f * ROOM_SIZE ),
	};
	NjFloat3	pNormals[6] =
	{
		NjFloat3( 0.0f, -1.0f, 0.0f ),
		NjFloat3( 0.0f, +1.0f, 0.0f ),
		NjFloat3( +1.0f, 0.0f, 0.0f ),
		NjFloat3( -1.0f, 0.0f, 0.0f ),
		NjFloat3( 0.0f, 0.0f, +1.0f ),
		NjFloat3( 0.0f, 0.0f, -1.0f ),
	};
	NjFloat3	pTangents[6] =
	{
		NjFloat3( -1.0f, 0.0f, 0.0f ),
		NjFloat3( +1.0f, 0.0f, 0.0f ),
		NjFloat3( 0.0f, 0.0f, +1.0f ),
		NjFloat3( 0.0f, 0.0f, -1.0f ),
		NjFloat3( -1.0f, 0.0f, 0.0f ),
		NjFloat3( +1.0f, 0.0f, 0.0f ),
	};
	NjFloat3	pBiTangents[6] =
	{
		NjFloat3( 0.0f, 0.0f, +1.0f ),
		NjFloat3( 0.0f, 0.0f, +1.0f ),
		NjFloat3( 0.0f, +1.0f, 0.0f ),
		NjFloat3( 0.0f, +1.0f, 0.0f ),
		NjFloat3( 0.0f, +1.0f, 0.0f ),
		NjFloat3( 0.0f, +1.0f, 0.0f ),
	};
	static const float	SCALED_V = 0.5f - 1.0f / LIGHTMAP_SIZE;			// So we don't sample other texels from adjacent faces...
	static const float	SCALED_OFFSET = 0.5f + 1.0f / LIGHTMAP_SIZE;
	NjFloat4	pLightMapUVs[6] = 
	{
		NjFloat4( 0.0f, 0.0f, 1.0f, 1.0f ),
		NjFloat4( 0.0f, 0.0f, 1.0f, 1.0f ),
		NjFloat4( 0.0f, 0.0f, 1.0f, SCALED_V ),
		NjFloat4( 0.0f, SCALED_OFFSET, 1.0f, SCALED_V ),
		NjFloat4( 0.0f, 0.0f, 1.0f, SCALED_V ),
		NjFloat4( 0.0f, SCALED_OFFSET, 1.0f, SCALED_V ),
	};
	float		pLightMapArrayIndex[6] =
	{
		0, 1,
		2, 2,
		3, 3
	};

	//////////////////////////////////////////////////////////////////////////
	// Generate the primitives
	{
		VertexFormatP3N3G3T3T3	pVertices[4*6];
		U16						pIndices[6*6];
		for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
		{
			NjFloat2	Size = pSizes[FaceIndex];
			NjFloat3	C = pCenters[FaceIndex];
			NjFloat3	N = pNormals[FaceIndex];
			NjFloat3	T = pTangents[FaceIndex];
			NjFloat3	B = pBiTangents[FaceIndex];
			NjFloat4	UV = pLightMapUVs[FaceIndex];
			float		ArrayIndex = pLightMapArrayIndex[FaceIndex];

			float		TextureIndex = FaceIndex < 2 ? 0.0f : 1.0f;	// Use wall texture for walls...

			// Top-left corner
			pVertices[4*FaceIndex+0].Position = C - 0.5f * Size.x * T + 0.5f * Size.y * B;
			pVertices[4*FaceIndex+0].Normal = N;
			pVertices[4*FaceIndex+0].Tangent = T;
			pVertices[4*FaceIndex+0].UV.Set( 0.0f, 0.0f, TextureIndex );
			pVertices[4*FaceIndex+0].UV2.Set( UV.x + UV.z * 0.0f, UV.y + UV.w * 1.0f, ArrayIndex );

			// Bottom-left corner
			pVertices[4*FaceIndex+1].Position = C - 0.5f * Size.x * T - 0.5f * Size.y * B;
			pVertices[4*FaceIndex+1].Normal = N;
			pVertices[4*FaceIndex+1].Tangent = T;
			pVertices[4*FaceIndex+1].UV.Set( 0.0f, 1.0f, TextureIndex );
			pVertices[4*FaceIndex+1].UV2.Set( UV.x + UV.z * 0.0f, UV.y + UV.w * 0.0f, ArrayIndex );

			// Bottom-right corner
			pVertices[4*FaceIndex+2].Position = C + 0.5f * Size.x * T - 0.5f * Size.y * B;
			pVertices[4*FaceIndex+2].Normal = N;
			pVertices[4*FaceIndex+2].Tangent = T;
			pVertices[4*FaceIndex+2].UV.Set( 1.0f, 1.0f, TextureIndex );
			pVertices[4*FaceIndex+2].UV2.Set( UV.x + UV.z * 1.0f, UV.y + UV.w * 0.0f, ArrayIndex );

			// Top-right corner
			pVertices[4*FaceIndex+3].Position = C + 0.5f * Size.x * T + 0.5f * Size.y * B;
			pVertices[4*FaceIndex+3].Normal = N;
			pVertices[4*FaceIndex+3].Tangent = T;
			pVertices[4*FaceIndex+3].UV.Set( 1.0f, 0.0f, TextureIndex );
			pVertices[4*FaceIndex+3].UV2.Set( UV.x + UV.z * 1.0f, UV.y + UV.w * 1.0f, ArrayIndex );

			// Build indices
			pIndices[6*FaceIndex+0] = 4 * FaceIndex + 0;
			pIndices[6*FaceIndex+1] = 4 * FaceIndex + 1;
			pIndices[6*FaceIndex+2] = 4 * FaceIndex + 2;
			pIndices[6*FaceIndex+3] = 4 * FaceIndex + 0;
			pIndices[6*FaceIndex+4] = 4 * FaceIndex + 2;
			pIndices[6*FaceIndex+5] = 4 * FaceIndex + 3;
		}

		m_pPrimRoom = new Primitive( gs_Device, 24, pVertices, 36, pIndices, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, VertexFormatP3N3G3T3T3::DESCRIPTOR );
	}

	{	// Lights
		VertexFormatP3N3G3T3T3	pVertices[4*4];
		U16						pIndices[4*6];

		NjFloat2				LightSize( 1.0f, 8.0f );	// Neons are 1x8 m²
		float					FirstLightPosX = -0.5f * ROOM_SIZE + (ROOM_SIZE - 7 * LightSize.x) / 2.0f;
		float					LightPosY = ROOM_HEIGHT - 1e-3f;
		float					LightPosZ = -0.5f * ROOM_SIZE + (ROOM_SIZE - LightSize.y) / 2.0f;

		for ( int LightIndex=0; LightIndex < 4; LightIndex++ )
		{
			float	LightPosX = FirstLightPosX + 2.0f * LightSize.x * LightIndex;

			pVertices[4*LightIndex+0].Position.Set( LightPosX+0*LightSize.x, LightPosY, LightPosZ+0*LightSize.y );
			pVertices[4*LightIndex+0].Normal.Set( 0, -1, 0 );
			pVertices[4*LightIndex+0].UV.Set( 0, 0, 0 );
			pVertices[4*LightIndex+0].UV2.Set( 0, 0, float(LightIndex) );

			pVertices[4*LightIndex+1].Position.Set( LightPosX+1*LightSize.x, LightPosY, LightPosZ+0*LightSize.y );
			pVertices[4*LightIndex+1].Normal.Set( 0, -1, 0 );
			pVertices[4*LightIndex+1].UV.Set( 1, 0, 0 );
			pVertices[4*LightIndex+1].UV2.Set( 1, 0, float(LightIndex) );

			pVertices[4*LightIndex+2].Position.Set( LightPosX+1*LightSize.x, LightPosY, LightPosZ+1*LightSize.y );
			pVertices[4*LightIndex+2].Normal.Set( 0, -1, 0 );
			pVertices[4*LightIndex+2].UV.Set( 1, 1, 0 );
			pVertices[4*LightIndex+2].UV2.Set( 1, 1, float(LightIndex) );

			pVertices[4*LightIndex+3].Position.Set( LightPosX+0*LightSize.x, LightPosY, LightPosZ+1*LightSize.y );
			pVertices[4*LightIndex+3].Normal.Set( 0, -1, 0 );
			pVertices[4*LightIndex+3].UV.Set( 0, 1, 0 );
			pVertices[4*LightIndex+3].UV2.Set( 0, 1, float(LightIndex) );

			// Build indices
			pIndices[6*LightIndex+0] = 4*LightIndex+0;
			pIndices[6*LightIndex+1] = 4*LightIndex+1;
			pIndices[6*LightIndex+2] = 4*LightIndex+2;
			pIndices[6*LightIndex+3] = 4*LightIndex+0;
			pIndices[6*LightIndex+4] = 4*LightIndex+2;
			pIndices[6*LightIndex+5] = 4*LightIndex+3;
		}

		m_pPrimRoomLights = new Primitive( gs_Device, 16, pVertices, 24, pIndices, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, VertexFormatP3N3G3T3T3::DESCRIPTOR );
	}


	//////////////////////////////////////////////////////////////////////////
	// Allocate the input & output buffers
	struct LightMapInfos
	{
		NjFloat3	Position;
		U32			Seed0;
		NjFloat3	Normal;
		U32			Seed1;
		NjFloat3	Tangent;
		U32			Seed2;
		NjFloat3	BiTangent;
		U32			Seed3;
	};

	struct	LightMapResult
	{
		NjFloat4	Irradiance;
	};

	SB<LightMapInfos>*	ppLMInfos[6];
	SB<LightMapResult>*	ppResults0[6];
	SB<LightMapResult>*	ppResults1[6];
 	SB<LightMapResult>*	ppAccumResults[6];
	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		int		W = pIntSizes[2*FaceIndex+0];
		int		H = pIntSizes[2*FaceIndex+1];
		int		Size = W*H;

		ppLMInfos[FaceIndex] = new SB<LightMapInfos>( gs_Device, Size, true );
		ppResults0[FaceIndex] = new SB<LightMapResult>( gs_Device, Size, false );
		ppResults1[FaceIndex] = new SB<LightMapResult>( gs_Device, Size, false );
		ppAccumResults[FaceIndex] = new SB<LightMapResult>( gs_Device, Size, false );
	}

	//////////////////////////////////////////////////////////////////////////
	// Build the normal map & height map from the wall texture
	TextureBuilder	TBNormalHeight( LIGHTMAP_SIZE, LIGHTMAP_SIZE/2 );
					TBNormalHeight.CopyFrom( _TB );	// Scale down...
	int				Dummy;
	NjFloat4**		ppWallTextureNormals = (NjFloat4**) TBNormalHeight.Convert( PixelFormatRGBA32F::DESCRIPTOR, TextureBuilder::CONV_NxNyNzH, Dummy );


	//////////////////////////////////////////////////////////////////////////
	// Generate the input information for each texel of the light map
	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		int			W = pIntSizes[2*FaceIndex+0];
		int			H = pIntSizes[2*FaceIndex+1];

		NjFloat2	Size = pSizes[FaceIndex];
		NjFloat3&	C = pCenters[FaceIndex];
		NjFloat3&	N = pNormals[FaceIndex];
		NjFloat3&	T = pTangents[FaceIndex];
		NjFloat3&	B = pBiTangents[FaceIndex];

		static const float	SMALL_OFFSET = 0.01f;

		LightMapInfos*	pDest = ppLMInfos[FaceIndex]->m;
		for ( int Y=0; Y < H; Y++ )
		{
			float	fY = CLAMP( float(Y), SMALL_OFFSET, (H-1)-SMALL_OFFSET ) / (H-1) - 0.5f;		// in ]-0.5,0.5[
					fY *= Size.y;																	// in ]-0.5*Size,+0.5*Size[

			NjFloat4*	pScanlineWallTexture = ppWallTextureNormals[0] + LIGHTMAP_SIZE * Y;
			for ( int X=0; X < W; X++, pDest++, pScanlineWallTexture++ )
			{
				float	fX = CLAMP( float(X), SMALL_OFFSET, (W-1)-SMALL_OFFSET ) / (W-1) - 0.5f;	// in ]-0.5,0.5[
						fX *= Size.x;																// in ]-0.5*Size,+0.5*Size[

				if ( FaceIndex < 2 )
				{	// Flat floor & ceiling
					pDest->Position = C + fX * T + fY * B;

					pDest->Normal = N;
					pDest->Tangent = T;
					pDest->BiTangent = B;
				}
				else
				{	// Offset position & orientation based on normal map and height
					float		Height = 1.0f * pScanlineWallTexture->w;
					NjFloat3	Normal( 2.0f * (pScanlineWallTexture->x - 0.5f), 2.0f * (pScanlineWallTexture->y - 0.5f), 2.0f * (pScanlineWallTexture->z - 0.5f) );
					NjFloat3	WSNormal = Normal.x * T + Normal.y * B + Normal.z * N;	// World space

					pDest->Position = C + fX * T + fY * B + Height * N;

					// Rotate local tangent space with normal map
					NjFloat4x4	Rot = NjFloat4x4::Rot( N, WSNormal );
					pDest->Normal = NjFloat4( N, 0.0f ) * Rot;
					pDest->Tangent = NjFloat4( T, 0.0f ) * Rot;
					pDest->BiTangent = NjFloat4( B, 0.0f ) * Rot;
				}

				pDest->Seed0 = 128;
				pDest->Seed1 = 129;
				pDest->Seed2 = 130;
				pDest->Seed3 = 131;
			}
		}

		// Write to buffer
		ppLMInfos[FaceIndex]->Write();
	}

//*
	//////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	ComputeShader*	pCSComputeLightMapDirect;
	CHECK_MATERIAL( pCSComputeLightMapDirect = CreateComputeShader( IDR_SHADER_ROOM_BUILD_LIGHTMAP, "CS_Direct" ), 5 );
	ComputeShader*	pCSComputeLightMapIndirect;
	CHECK_MATERIAL( pCSComputeLightMapIndirect = CreateComputeShader( IDR_SHADER_ROOM_BUILD_LIGHTMAP, "CS_Indirect" ), 6 );

	struct	CBRender
	{
		U32		LightMapSizeX;
		U32		LightMapSizeY;
		U32		PassIndex;
		U32		PassesCount;
		float	RadianceWeight;
	};
	CB<CBRender>	CB_Render( gs_Device, 10 );

	pCSComputeLightMapDirect->Use();

	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		ppLMInfos[FaceIndex]->SetInput( 0 );
		ppResults1[FaceIndex]->SetOutput( 0 );
		ppAccumResults[FaceIndex]->SetOutput( 1 );

		CB_Render.m.LightMapSizeX = pIntSizes[2*FaceIndex+0];
		CB_Render.m.LightMapSizeY = pIntSizes[2*FaceIndex+1];
		CB_Render.m.PassIndex = 0;
		CB_Render.m.PassesCount = 1;
		CB_Render.m.RadianceWeight = 1.0f;
		CB_Render.UpdateData();

		pCSComputeLightMapDirect->Run( CB_Render.m.LightMapSizeX, CB_Render.m.LightMapSizeY, 1 );

// 		ppResults1[FaceIndex]->Read();	// CHECK

		// Swap
		SB<LightMapResult>*	pTemp = ppResults0[FaceIndex];
		ppResults0[FaceIndex] = ppResults1[FaceIndex];
		ppResults1[FaceIndex] = pTemp;
	}


	//////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting
	pCSComputeLightMapIndirect->Use();

	for ( int BounceIndex=0; BounceIndex < ROOM_BOUNCES_COUNT; BounceIndex++ )
	{
		// Upload previous pass's results
		for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
			ppResults0[FaceIndex]->SetInput( 4+FaceIndex );

		// Run
		for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
		{
			ppLMInfos[FaceIndex]->SetInput( 0 );

			ppResults1[FaceIndex]->Clear( NjFloat4::Zero );
			ppResults1[FaceIndex]->SetOutput( 0 );
			ppAccumResults[FaceIndex]->SetOutput( 1 );

			CB_Render.m.LightMapSizeX = pIntSizes[2*FaceIndex+0];
			CB_Render.m.LightMapSizeY = pIntSizes[2*FaceIndex+1];
			CB_Render.m.RadianceWeight = 1.0f / ROOM_RAY_GROUPS_COUNT;
			CB_Render.m.PassesCount = ROOM_RAY_GROUPS_COUNT;

			for ( int PassIndex=0; PassIndex < ROOM_RAY_GROUPS_COUNT; PassIndex++ )
			{
				CB_Render.m.PassIndex = PassIndex;
				CB_Render.UpdateData();

				pCSComputeLightMapIndirect->Run( CB_Render.m.LightMapSizeX, CB_Render.m.LightMapSizeY, 1 );
			}

//			ppResults1[FaceIndex]->Read();	// CHECK

			// Swap
			SB<LightMapResult>*	pTemp = ppResults0[FaceIndex];
			ppResults0[FaceIndex] = ppResults1[FaceIndex];
			ppResults1[FaceIndex] = pTemp;
		}
	}

	delete pCSComputeLightMapIndirect;
	delete pCSComputeLightMapDirect;


	//////////////////////////////////////////////////////////////////////////
	// Build the final light map
	NjHalf4*	ppContent[4];
				ppContent[0] = new NjHalf4[LIGHTMAP_SIZE * LIGHTMAP_SIZE];
				ppContent[1] = new NjHalf4[LIGHTMAP_SIZE * LIGHTMAP_SIZE];
				ppContent[2] = new NjHalf4[LIGHTMAP_SIZE * LIGHTMAP_SIZE];
				ppContent[3] = new NjHalf4[LIGHTMAP_SIZE * LIGHTMAP_SIZE];

	// The first 2 maps (ceiling and floor) are easy
	for ( int FaceIndex=0; FaceIndex < 2; FaceIndex++ )
	{
		ppAccumResults[FaceIndex]->Read();	// Retrieve the accumulated results for that face

		LightMapResult*	pSource = ppAccumResults[FaceIndex]->m;
		NjHalf4*		pDest = ppContent[FaceIndex];
		for ( int Y=0; Y < LIGHTMAP_SIZE; Y++ )
			for ( int X=0; X < LIGHTMAP_SIZE; X++, pSource++, pDest++ )
			{
				pDest->x = pSource->Irradiance.x;
				pDest->y = pSource->Irradiance.y;
				pDest->z = pSource->Irradiance.z;
				pDest->w = pSource->Irradiance.w;
			}
	}

	// The next 4 maps (walls) need to be packed 2 by 2
	for ( int FaceIndex=2; FaceIndex < 6; FaceIndex++ )
	{
		ppAccumResults[FaceIndex]->Read();	// Retrieve the accumulated results for that face

		LightMapResult*	pSource = ppAccumResults[FaceIndex]->m;

		int				TargetFaceIndex = 2 + ((FaceIndex-2) >> 1);
		int				TargetFaceOffsetY = ((FaceIndex-2) & 1) * LIGHTMAP_SIZE/2;
		NjHalf4*		pDest = ppContent[TargetFaceIndex] + TargetFaceOffsetY*LIGHTMAP_SIZE;
		for ( int Y=0; Y < LIGHTMAP_SIZE/2; Y++ )
			for ( int X=0; X < LIGHTMAP_SIZE; X++, pSource++, pDest++ )
			{
				pDest->x = pSource->Irradiance.x;
				pDest->y = pSource->Irradiance.y;
				pDest->z = pSource->Irradiance.z;
				pDest->w = pSource->Irradiance.w;
			}
	}

	m_pTexLightMaps = new Texture2D( gs_Device, LIGHTMAP_SIZE, LIGHTMAP_SIZE, 4, PixelFormatRGBA16F::DESCRIPTOR, 1, (void**) ppContent );

	delete ppContent[0];
	delete ppContent[1];
	delete ppContent[2];
	delete ppContent[3];
//*/

	for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
	{
		delete ppLMInfos[FaceIndex];
		delete ppResults0[FaceIndex];
		delete ppResults1[FaceIndex];
		delete ppAccumResults[FaceIndex];
	}
}

//////////////////////////////////////////////////////////////////////////
// Build the texture used for the walls
namespace RoomFillers
{
	void	FillRoundedRect( const DrawUtils::DrawInfos& _Infos, Pixel& _Pixel )
	{
		if ( _Infos.Distance < -0.5f )
		{	// We're outside the rounded rectangle, paint a shiny dark material
			_Pixel.RGBA.Set( 0.05f, 0.05f, 0.05f, 1.0f );
			_Pixel.Height = 0.0f;
			_Pixel.Roughness = 0.0f;
			_Pixel.MatID = 1;
			return;
		}

		// Inside the rectangle, rapidly increase height to its maximum and paint with a diffuse white material
		float	t = SATURATE( (_Infos.Distance - (-0.5f)) / 0.5f );
		float	MaxHeight = 0.25f;				// 5 centimeters
		float	MinHeight = MaxHeight * 0.8f;	// We start with 80% of the max height

		_Pixel.RGBA.Set( 1.0f, 1.0f, 1.0f, 1.0f );	// Should it be the actual reflectance used by the light map computer?
		_Pixel.Height = LERP( MinHeight, MaxHeight, sinf( HALFPI * t ) );	// Rounded height
		_Pixel.Roughness = 1.0f;
		_Pixel.MatID = 0;
	}
}

void	EffectRoom::BuildRoomTextures( TextureBuilder& _TB )
{
	{
		_TB.Clear( Pixel( NjFloat4::Zero ) );

		DrawUtils	DU;
		DU.SetupSurface( _TB );
		for ( int Y=0; Y < 4; Y++ )
			for ( int X=0; X < 4; X++ )
				DU.DrawRectangle( 512.0f*X, 256.0f*Y, 512, 256, 20, 1.0f, RoomFillers::FillRoundedRect, NULL );

		m_pTexWalls = _TB.CreateTexture( PixelFormatRGBA16F::DESCRIPTOR, TextureBuilder::CONV_RGBA_NxNyHR_M );
	}
}
