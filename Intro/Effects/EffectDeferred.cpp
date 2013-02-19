#include "../../GodComplex.h"
#include "EffectDeferred.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectDeferred::EffectDeferred() : m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatFillGBuffer = CreateMaterial( IDR_SHADER_DEFERRED_FILL_GBUFFER, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );


	//////////////////////////////////////////////////////////////////////////
	// Build the primitives
	{
		GeometryBuilder::MapperSpherical	Mapper;
		m_pPrimSphere = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildSphere( 60, 30, *m_pPrimSphere, Mapper );
	}

	//////////////////////////////////////////////////////////////////////////
	// Build our voronoï texture & our initial positions & data
// 	NjFloat2*	pCellCenters = new NjFloat2[EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT];
// 	{
// 		TextureBuilder		TB( 1024, 1024 );
// 		VertexFormatPt4*	pVertices = new VertexFormatPt4[EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT];
// 		BuildVoronoiTexture( TB, pCellCenters, pVertices );
// 
// 		TextureBuilder::ConversionParams	Conv =
// 		{
// 			0,		// int		PosR;
// 			1,		// int		PosG;
// 			-1,		// int		PosB;
// 			-1,		// int		PosA;
// 
// 					// Position of the height & roughness fields
// 			-1,		// int		PosHeight;
// 			-1,		// int		PosRoughness;
// 
// 					// Position of the Material ID
// 			-1,		// int		PosMatID;
// 
// 					// Position of the normal fields
// 			1.0f,	// float	NormalFactor;	// Factor to apply to the height to generate the normals
// 			-1,		// int		PosNormalX;
// 			-1,		// int		PosNormalY;
// 			-1,		// int		PosNormalZ;
// 
// 					// Position of the AO field
// 			1.0f,	// float	AOFactor;		// Factor to apply to the height to generate the AO
// 			-1,		// int		PosAO;
// 		};
// 
// 		m_pTexVoronoi = TB.CreateTexture( PixelFormatRG32F::DESCRIPTOR, Conv );
// 
// 
// 		m_pPrimParticle = new Primitive( gs_Device, EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_POINTLIST, VertexFormatPt4::DESCRIPTOR );
// 
// 		// Build cell centers
// 		for ( int ParticleIndex=0; ParticleIndex < EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT; ParticleIndex++ )
// 			pCellCenters[ParticleIndex].Set( 0.5f * (pVertices[ParticleIndex].Pt.x + pVertices[ParticleIndex].Pt.z), 0.5f * (pVertices[ParticleIndex].Pt.y + pVertices[ParticleIndex].Pt.w) );
// 
// 		delete[] pVertices;
// 	}



	// Create the render targets
	m_pRT = new Texture2D( gs_Device, gs_Device.DefaultRenderTarget().GetWidth(), gs_Device.DefaultRenderTarget().GetHeight(), 2, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Render = new CB<CBRender>( gs_Device, 10 );
	m_pCB_Render->m.DeltaTime.Set( 0, 1 );
}

EffectDeferred::~EffectDeferred()
{
//	delete m_pTexVoronoi;

	delete m_pCB_Render;

	delete m_pRT;

	delete m_pPrimSphere;

	delete m_pMatFillGBuffer;
}

void	EffectDeferred::Render( float _Time, float _DeltaTime )
{
/*
	//////////////////////////////////////////////////////////////////////////
	// 1] Render objects in Z pre-pass
	{	USING_MATERIAL_START( *m_pMatFillGBuffer )
	
		ID3D11RenderTargetView*	ppRenderTargets[2] =
		{
			m_pRT->GetTargetView( 0, 0, 1 ), m_pRT->GetTargetView( 0, 0, 1 )
		};
		gs_Device.SetRenderTargets( gs_Device.DefaultRenderTarget().GetWidth(), gs_Device.DefaultRenderTarget().GetHeight(), 2, ppRenderTargets, gs_Device.DefaultDepthStencil().GetShaderView() );
 		gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );

// 		m_pCB_Render->m.dUV = m_ppRTParticlePositions[2]->GetdUV();
// 		m_pCB_Render->m.DeltaTime.x = 10.0f * _DeltaTime;
// 		m_pCB_Render->UpdateData();
// 
// 		m_ppRTParticlePositions[0]->SetPS( 10 );
// 		m_ppRTParticlePositions[1]->SetPS( 11 );
// 		m_ppRTParticleNormals[0]->SetPS( 12 );
// 		m_ppRTParticleTangents[0]->SetPS( 13 );
// 
// 		gs_pPrimQuad->Render( *m_pMatCompute );
// 
// 		// Keep delta time for next time
// 		m_pCB_Render->m.DeltaTime.y = _DeltaTime;

		USING_MATERIAL_END
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Render the particles
	{	USING_MATERIAL_START( *m_pMatDisplay )

		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
		gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );

		m_ppRTParticlePositions[1]->SetVS( 10 );
		m_ppRTParticleNormals[0]->SetVS( 11 );
		m_ppRTParticleTangents[0]->SetVS( 12 );
		m_pTexVoronoi->SetPS( 13 );

//		m_pPrimParticle->RenderInstanced( *m_pMatDisplay, EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT );
		m_pPrimParticle->Render( *m_pMatDisplay );

		USING_MATERIAL_END
	}*/
}
/*
namespace	// Drawers & Fillers
{
	struct	__VoronoiInfos
	{
		int					ParticleIndex;
		float				Distance;
	};
	struct	__PerturbVoronoi
	{
		Noise*				pNoise;
		TextureBuilder*		pVoronoi;
		int					W, H;
		VertexFormatPt4*	pVertices;
	};

	float	CombineDistances( float _pDistances[], int _pCellX[], int _pCellY[], int _pCellZ[], void* _pData )
	{
		__VoronoiInfos&	Infos = *((__VoronoiInfos*) _pData);
		Infos.ParticleIndex = EFFECT_PARTICLES_COUNT*_pCellY[0] + _pCellX[0];
		Infos.Distance = sqrtf( _pDistances[0] );
		return 0.0f;
	}
	void	FillVoronoi( int x, int y, const NjFloat2& _UV, Pixel& _Pixel, void* _pData )
	{
 		Noise&	N = *((Noise*) _pData);

		__VoronoiInfos	Infos;
 		N.Cellular( EFFECT_PARTICLES_COUNT * _UV, CombineDistances, &Infos, true );	// Simple cellular (NOT Worley !) => Means only 1 point per cell, exactly what we need for a unique particle ID

		_Pixel.RGBA.Set( float(Infos.ParticleIndex), Infos.Distance, 0, 0 );
	}
	void	PerturbVoronoi( int x, int y, const NjFloat2& _UV, Pixel& _Pixel, void* _pData )
	{
 		__PerturbVoronoi&	Params = *((__PerturbVoronoi*) _pData);
		
		// Perturb the UVs a little
		NjFloat2	Disturb = Params.pNoise->PerlinVector( 0.02f * _UV );
		NjFloat2	NewUV = _UV + 0.0125f * Disturb;

		// Use POINT SAMPLING to fetch the original color from the voronoï texture
		// (because we're dealing with particle indices here, not colors that can be linearly interpolated!)
		int		X = (Params.W + int(floorf( NewUV.x * Params.W ))) % Params.W;
		int		Y = (Params.H + int(floorf( NewUV.y * Params.H ))) % Params.H;
		Params.pVoronoi->Get( X, Y, 0, _Pixel );

		// Update the particles's Min/Max UVs
		int		ParticleIndex = int(_Pixel.RGBA.x);
		ASSERT( ParticleIndex >= 0, "WTF?!" );
		ASSERT( ParticleIndex < EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT, "WTF?!" );
		NjFloat4&	ParticleVertex = Params.pVertices[ParticleIndex].Pt;

// 		if ( ParticleIndex == 0x2F && NewUV.x < 0.4f )
// 			ParticleIndex++;

		// Check if new UV is not too far from existing min/max boundaries, in which case it would indicate a wrap
		// We want to overlap the border instead...
		float	OVERLAP_TOLERANCE = 4.0f / EFFECT_PARTICLES_COUNT;

		NjFloat2	UV = _UV;
		float	DeltaU = UV.x - ParticleVertex.x;
		float	DeltaV = UV.y - ParticleVertex.y;
		if ( DeltaU > OVERLAP_TOLERANCE )
			UV.x -= 1.0f;	// Overlap instead
		if ( DeltaV > OVERLAP_TOLERANCE )
			UV.y -= 1.0f;	// Overlap instead

		DeltaU = ParticleVertex.z - UV.x;
		DeltaV = ParticleVertex.w - UV.y;
		if ( DeltaU > OVERLAP_TOLERANCE )
			UV.x += 1.0f;	// Overlap instead
		if ( DeltaV > OVERLAP_TOLERANCE )
			UV.y += 1.0f;	// Overlap instead

		ParticleVertex.x = MIN( ParticleVertex.x, UV.x );
		ParticleVertex.y = MIN( ParticleVertex.y, UV.y );
		ParticleVertex.z = MAX( ParticleVertex.z, UV.x );
		ParticleVertex.w = MAX( ParticleVertex.w, UV.y );
	}
};

void	EffectDeferred::BuildVoronoiTexture( TextureBuilder& _TB, NjFloat2* _pCellCenters, VertexFormatPt4* _pVertices )
{
	// Build a voronoï pattern
	Noise	N( 1 );
			N.SetCellularWrappingParameters( EFFECT_PARTICLES_COUNT, EFFECT_PARTICLES_COUNT, EFFECT_PARTICLES_COUNT );

	TextureBuilder	TempVoronoi( _TB.GetWidth(), _TB.GetHeight() );
					TempVoronoi.Fill( ::FillVoronoi, &N );

	// Clear the vertices to wrong intervals
	for ( int ParticleIndex=0; ParticleIndex < EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT; ParticleIndex++ )
		_pVertices[ParticleIndex].Pt.Set( +MAX_FLOAT, +MAX_FLOAT, -MAX_FLOAT, -MAX_FLOAT );

	// Perturb the original voronoï with a small noise to break the regular cell patterns
	__PerturbVoronoi	S = { &N, &TempVoronoi, TempVoronoi.GetWidth(), TempVoronoi.GetHeight(), _pVertices };
	_TB.Fill( ::PerturbVoronoi, &S );

	// Reparse vertices to make sure border particles wrap correctly
// 	for ( int VertexIndex=0; VertexIndex < EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT; VertexIndex++ )
// 	{
// 		NjFloat4&	UVs = _pVertices[VertexIndex].Pt;
// 
// 		float		DeltaU = UVs.z - UVs.x;
// 		float		DeltaV = UVs.w - UVs.y;
// 
// 		ASSERT( DeltaU < 0.2f, "WTF?!" );
// 		ASSERT( DeltaV < 0.2f, "WTF?!" );
//  	}

	// Generate the positions of the center of each cell (in UV space)
	for ( int Y=0; Y < EFFECT_PARTICLES_COUNT; Y++ )
		for ( int X=0; X < EFFECT_PARTICLES_COUNT; X++ )
			N.CellularGetCenter( X, Y, _pCellCenters[EFFECT_PARTICLES_COUNT*Y+X], true );
}
*/