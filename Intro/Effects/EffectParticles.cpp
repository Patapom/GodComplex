#include "../../GodComplex.h"
#include "EffectParticles.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectParticles::EffectParticles() : m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatCompute = CreateMaterial( IDR_SHADER_PARTICLES_COMPUTE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_PARTICLES_DISPLAY, VertexFormatP3::DESCRIPTOR, "VS", "GS", "PS" ), 2 );

	//////////////////////////////////////////////////////////////////////////
	// Build the awesome particle primitive
	{
		VertexFormatP3	Vertices;
		m_pPrimParticle = new Primitive( gs_Device, 1, &Vertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_POINTLIST, VertexFormatP3::DESCRIPTOR );
	}

	//////////////////////////////////////////////////////////////////////////
	// Build textures & render targets
	PixelFormatRGBA32F*	pInitialPosition = new PixelFormatRGBA32F[TEXTURE_SIZE*TEXTURE_SIZE];
	PixelFormatRGBA32F*	pInitialRotation = new PixelFormatRGBA32F[TEXTURE_SIZE*TEXTURE_SIZE];
	PixelFormatRGBA32F*	pScanlinePosition = pInitialPosition;
	PixelFormatRGBA32F*	pScanlineRotation = pInitialRotation;

	int		TotalCount = TEXTURE_SIZE*TEXTURE_SIZE;
	int		ParticlesPerDimension = U32(floor(powf( float(TotalCount), 1.0f/3.0f )));
	float	CubeSize = 1.0f;

	for ( int Y=0; Y < TEXTURE_SIZE; Y++ )
		for ( int X=0; X < TEXTURE_SIZE; X++, pScanlinePosition++, pScanlineRotation++ )
		{
// 			int		ParticleIndex = TEXTURE_SIZE*Y+X;
// 			int		ParticleZ = ParticleIndex / (ParticlesPerDimension*ParticlesPerDimension);
// 					ParticleIndex -= ParticlesPerDimension*ParticlesPerDimension*ParticleZ;
// 
// 			int		ParticleY = ParticleIndex / ParticlesPerDimension;
// 					ParticleIndex -= ParticlesPerDimension*ParticleY;
// 
// 			int		ParticleX = ParticleIndex;
// 
// 			pScanlinePosition->R = CubeSize * (float(ParticleX) / (ParticlesPerDimension-1) - 0.5f);
// 			pScanlinePosition->G = CubeSize * (float(ParticleY) / (ParticlesPerDimension-1) - 0.5f) + 1.5f;
// 			pScanlinePosition->B = CubeSize * (float(ParticleZ) / (ParticlesPerDimension-1) - 0.5f);
// 			pScanlinePosition->A = 0.0f;
// 
// 			pScanlineRotation->R = 0;
// 			pScanlineRotation->G = 0;
// 			pScanlineRotation->B = 1;
// 			pScanlineRotation->A = 0.0f;

			float	R = 0.5f;
			float	r = 0.2f;

			float	Alpha = TWOPI * X / TEXTURE_SIZE;
			float	Beta = TWOPI * Y / TEXTURE_SIZE;

			NjFloat3	T( cosf(Alpha), 0.0f, sinf(Alpha) );
			NjFloat3	Center = NjFloat3( 0, 0.5, 0 ) + R * T;
			NjFloat3	N( -T.z, 0, T.x );
			NjFloat3	B = T ^ N;

			NjFloat3	Dir = cosf(Beta) * T + sinf(Beta) * B;
			NjFloat3	Pos = Center + r * Dir;

			pScanlinePosition->R = Pos.x;
			pScanlinePosition->G = Pos.y;
			pScanlinePosition->B = Pos.z;
			pScanlinePosition->A = 0.0f;

			pScanlineRotation->R = Dir.x;
			pScanlineRotation->G = Dir.y;
			pScanlineRotation->B = Dir.z;
			pScanlineRotation->A = 0.0f;
		}

	void*	ppContentPosition[1];
			ppContentPosition[0] = (void*) pInitialPosition;
	void*	ppContentRotation[1];
			ppContentRotation[0] = (void*) pInitialRotation;

	Texture2D*	pTempPosition = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, ppContentPosition );
	Texture2D*	pTempRotation = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, ppContentRotation );

	m_pRTParticlePositions[0] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );
	m_pRTParticlePositions[1] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );
	m_pRTParticlePositions[2] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );

	m_pRTParticlePositions[0]->CopyFrom( *pTempPosition );
	m_pRTParticlePositions[1]->CopyFrom( *pTempPosition );
	m_pRTParticlePositions[2]->CopyFrom( *pTempPosition );

	m_pRTParticleRotations[0] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );
	m_pRTParticleRotations[1] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );
	m_pRTParticleRotations[2] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );

	m_pRTParticleRotations[0]->CopyFrom( *pTempRotation );
	m_pRTParticleRotations[1]->CopyFrom( *pTempRotation );
	m_pRTParticleRotations[2]->CopyFrom( *pTempRotation );

	delete pTempPosition;
	delete pTempRotation;
	delete[] pInitialPosition;
	delete[] pInitialRotation;


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Render = new CB<CBRender>( gs_Device, 10 );
	m_pCB_Render->m.DeltaTime.Set( 0, 1 );

	//////////////////////////////////////////////////////////////////////////
	// Build our voronoï texture & our initial positions & data
	{
		TextureBuilder		TB( 1024, 1024 );
		VertexFormatPt4		pVertices[EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT];
		BuildVoronoiTexture( TB, pVertices );

		TextureBuilder::ConversionParams	Conv =
		{
			0,		// int		PosR;
			-1,		// int		PosG;
			-1,		// int		PosB;
			-1,		// int		PosA;

					// Position of the height & roughness fields
			-1,		// int		PosHeight;
			-1,		// int		PosRoughness;

					// Position of the Material ID
			-1,		// int		PosMatID;

					// Position of the normal fields
			1.0f,	// float	NormalFactor;	// Factor to apply to the height to generate the normals
			-1,		// int		PosNormalX;
			-1,		// int		PosNormalY;
			-1,		// int		PosNormalZ;

					// Position of the AO field
			1.0f,	// float	AOFactor;		// Factor to apply to the height to generate the AO
			-1,		// int		PosAO;
		};

		m_pTexVoronoi = TB.CreateTexture( PixelFormatR16F::DESCRIPTOR, Conv );

//		m_pPrimParticle = new Primitive( gs_Device, EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT, pVertices, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_POINTLIST, VertexFormatPt4::DESCRIPTOR );
	}
}

EffectParticles::~EffectParticles()
{
	delete m_pTexVoronoi;

	delete m_pCB_Render;

	delete m_pRTParticleRotations[2];
	delete m_pRTParticleRotations[1];
	delete m_pRTParticleRotations[0];

	delete m_pRTParticlePositions[2];
	delete m_pRTParticlePositions[1];
	delete m_pRTParticlePositions[0];

	delete m_pPrimParticle;

	delete m_pMatCompute;
 	delete m_pMatDisplay;
}

void	EffectParticles::Render( float _Time, float _DeltaTime )
{
	//////////////////////////////////////////////////////////////////////////
	// 1] Update particles' positions
	{	USING_MATERIAL_START( *m_pMatCompute )
	
		ID3D11RenderTargetView*	ppRenderTargets[2] =
		{
			m_pRTParticlePositions[2]->GetTargetView( 0, 0, 1 ), m_pRTParticleRotations[2]->GetTargetView( 0, 0, 1 )
		};
		gs_Device.SetRenderTargets( m_pRTParticlePositions[2]->GetWidth(), m_pRTParticlePositions[2]->GetHeight(), 2, ppRenderTargets );
 		gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );

		m_pCB_Render->m.dUV = m_pRTParticlePositions[2]->GetdUV();
		m_pCB_Render->m.DeltaTime.x = 10.0f * _DeltaTime;
		m_pCB_Render->UpdateData();

		m_pRTParticlePositions[0]->SetPS( 10 );
		m_pRTParticlePositions[1]->SetPS( 11 );
		m_pRTParticleRotations[0]->SetPS( 12 );
		m_pRTParticleRotations[1]->SetPS( 13 );

		gs_pPrimQuad->Render( *m_pMatCompute );

		Texture2D*	pTemp = m_pRTParticlePositions[0];
		m_pRTParticlePositions[0] = m_pRTParticlePositions[1];
		m_pRTParticlePositions[1] = m_pRTParticlePositions[2];
		m_pRTParticlePositions[2] = pTemp;

		pTemp = m_pRTParticleRotations[0];
		m_pRTParticleRotations[0] = m_pRTParticleRotations[1];
		m_pRTParticleRotations[1] = m_pRTParticleRotations[2];
		m_pRTParticleRotations[2] = pTemp;

		USING_MATERIAL_END
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Render the particles
	{	USING_MATERIAL_START( *m_pMatDisplay )

		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
		gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );

		m_pRTParticlePositions[1]->SetVS( 10 );
		m_pRTParticleRotations[1]->SetVS( 11 );

		m_pPrimParticle->RenderInstanced( *m_pMatDisplay, TEXTURE_SIZE*TEXTURE_SIZE );

		USING_MATERIAL_END
	}

	// Keep delta time for next time
	m_pCB_Render->m.DeltaTime.y = 1.0f;//_DeltaTime;
}

namespace	// Drawers & Fillers
{
	struct	__PerturbVoronoi
	{
		Noise*				pNoise;
		TextureBuilder*		pVoronoi;
		int					W, H;
		VertexFormatPt4*	pVertices;
	};

	float	CombineDistances( float _pDistances[], int _pCellX[], int _pCellY[], int _pCellZ[] )
	{
		int		ParticleIndex = EFFECT_PARTICLES_COUNT*_pCellY[0] + _pCellX[0];
		return float(ParticleIndex);
//		float	NormalizedIndex = ParticleIndex / 65535.0f;		// Because no matter what, we must generate values in [0,1]
//		return NormalizedIndex;
	}
	void	FillVoronoi( int x, int y, const NjFloat2& _UV, Pixel& _Pixel, void* _pData )
	{
 		Noise&	N = *((Noise*) _pData);

 		float	C = N.Cellular( EFFECT_PARTICLES_COUNT * _UV, CombineDistances, true );	// Simple cellular (NOT Worley !) => Means only 1 point per cell, exactly what we need for a single particle ID

		_Pixel.RGBA.Set( C, C, C, 1.0f );
	}
	void	PerturbVoronoi( int x, int y, const NjFloat2& _UV, Pixel& _Pixel, void* _pData )
	{
 		__PerturbVoronoi&	Params = *((__PerturbVoronoi*) _pData);
		
		// Perturb the UVs a little
		NjFloat2	Disturb = Params.pNoise->PerlinVector( 0.025f * _UV );
		NjFloat2	NewUV = _UV + 0.04f * Disturb;

		// Use POINT SAMPLING to fetch the original color from the voronoï texture
		// (because we're dealing with particle indices here, not colors that can be linearly interpolated!)
		int		X = (Params.W + int(floorf( NewUV.x * Params.W ))) % Params.W;
		int		Y = (Params.H + int(floorf( NewUV.y * Params.H ))) % Params.H;
		Params.pVoronoi->Get( X, Y, 0, _Pixel );

		// Update the particles's Min/Max UVs
// 		int			ParticleIndex = int(_Pixel.RGBA.x);
// 		ASSERT(ParticleIndex < EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT, "WTF?!" );
// 		NjFloat4&	ParticleVertex = Params.pVertices[ParticleIndex].Pt;
// 		ParticleVertex.x = MIN( ParticleVertex.x, NewUV.x );
// 		ParticleVertex.y = MIN( ParticleVertex.y, NewUV.y );
// 		ParticleVertex.z = MAX( ParticleVertex.z, NewUV.x );
// 		ParticleVertex.w = MAX( ParticleVertex.w, NewUV.y );
	}
};

void	EffectParticles::BuildVoronoiTexture( TextureBuilder& _TB, VertexFormatPt4* _pVertices )
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
}
