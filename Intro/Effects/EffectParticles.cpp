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
	PixelFormatRGBA16F*	pInitialPosition = new PixelFormatRGBA16F[TEXTURE_SIZE*TEXTURE_SIZE];
	PixelFormatRGBA16F*	pScanline = pInitialPosition;

	int		TotalCount = TEXTURE_SIZE*TEXTURE_SIZE;
	int		ParticlesPerDimension = U32(floor(powf( float(TotalCount), 1.0f/3.0f )));
	float	CubeSize = 1.0f;

	for ( int Y=0; Y < TEXTURE_SIZE; Y++ )
		for ( int X=0; X < TEXTURE_SIZE; X++, pScanline++ )
		{
			int		ParticleIndex = TEXTURE_SIZE*Y+X;
			int		ParticleZ = ParticleIndex / (ParticlesPerDimension*ParticlesPerDimension);
					ParticleIndex -= ParticlesPerDimension*ParticlesPerDimension*ParticleZ;

			int		ParticleY = ParticleIndex / ParticlesPerDimension;
					ParticleIndex -= ParticlesPerDimension*ParticleY;

			int		ParticleX = ParticleIndex;

			pScanline->R = CubeSize * (float(ParticleX) / (ParticlesPerDimension-1) - 0.5f);
			pScanline->G = CubeSize * (float(ParticleY) / (ParticlesPerDimension-1) - 0.5f) + 1.5f;
			pScanline->B = CubeSize * (float(ParticleZ) / (ParticlesPerDimension-1) - 0.5f);
			pScanline->A = 0.0f;
		}

	void*	ppContent[1];
			ppContent[0] = (void*) pInitialPosition;

	Texture2D*	pTemp = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, ppContent );

	m_pRTParticlePositions[0] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pRTParticlePositions[1] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pRTParticlePositions[2] = new Texture2D( gs_Device, TEXTURE_SIZE, TEXTURE_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

	m_pRTParticlePositions[0]->CopyFrom( *pTemp );
	m_pRTParticlePositions[1]->CopyFrom( *pTemp );
	m_pRTParticlePositions[2]->CopyFrom( *pTemp );

	delete pTemp;
	delete[] pInitialPosition;


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Render = new CB<CBRender>( gs_Device, 10 );
}

EffectParticles::~EffectParticles()
{
	delete m_pCB_Render;

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
	
		gs_Device.SetRenderTarget( *m_pRTParticlePositions[2] );
 		gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );

		m_pCB_Render->m.dUV = m_pRTParticlePositions[2]->GetdUV();
		m_pCB_Render->UpdateData();

		m_pRTParticlePositions[0]->SetPS( 10 );
		m_pRTParticlePositions[1]->SetPS( 11 );

		gs_pPrimQuad->Render( *m_pMatCompute );

		Texture2D*	pTemp = m_pRTParticlePositions[0];
		m_pRTParticlePositions[0] = m_pRTParticlePositions[1];
		m_pRTParticlePositions[1] = m_pRTParticlePositions[2];
		m_pRTParticlePositions[2] = pTemp;

		USING_MATERIAL_END
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Render the particles
	{	USING_MATERIAL_START( *m_pMatDisplay )

		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget() );
		gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );

		m_pRTParticlePositions[1]->SetVS( 10 );

		m_pPrimParticle->RenderInstanced( *m_pMatDisplay, TEXTURE_SIZE*TEXTURE_SIZE );

		USING_MATERIAL_END
	}

}
