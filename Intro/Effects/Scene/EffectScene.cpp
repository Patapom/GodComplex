#include "../../../GodComplex.h"
#include "EffectScene.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectScene::EffectScene( Scene& _Scene ) : m_ErrorCode( 0 ), m_Scene( _Scene )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatFillGBuffer = CreateMaterial( IDR_SHADER_DEFERRED_FILL_GBUFFER, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );

	// Create the render targets
	m_pRT = new Texture2D( gs_Device, gs_Device.DefaultRenderTarget().GetWidth(), gs_Device.DefaultRenderTarget().GetHeight(), 4, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Render = new CB<CBRender>( gs_Device, 10 );
	m_pCB_Render->m.DeltaTime.Set( 0, 1 );
}

EffectScene::~EffectScene()
{
	delete m_pCB_Render;

	delete m_pRT;

	delete m_pMatShading;
 	delete m_pMatFillGBuffer;
	delete m_pMatDepthPass;
}

void	EffectScene::Render( float _Time, float _DeltaTime )
{
 	gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );

	//////////////////////////////////////////////////////////////////////////
	// 1] Render scene in depth pass
	{	USING_MATERIAL_START( *m_pMatDepthPass )
	
// 		ID3D11RenderTargetView*	ppRenderTargets[2] =
// 		{
// 			m_pRT->GetTargetView( 0, 0, 1 ), m_pRT->GetTargetView( 0, 0, 1 )
// 		};
// 		gs_Device.SetRenderTargets( gs_Device.DefaultRenderTarget().GetWidth(), gs_Device.DefaultRenderTarget().GetHeight(), 2, ppRenderTargets, gs_Device.DefaultDepthStencil().GetShaderView() );

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
	// 2] Render the scene in our first G-Buffer
	{	USING_MATERIAL_START( *m_pMatFillGBuffer )

		ID3D11RenderTargetView*	ppRenderTargets[2] =
		{
			m_pRT->GetTargetView( 0, 0, 1 ), m_pRT->GetTargetView( 0, 0, 1 )
		};
		gs_Device.SetRenderTargets( gs_Device.DefaultRenderTarget().GetWidth(), gs_Device.DefaultRenderTarget().GetHeight(), 2, ppRenderTargets, gs_Device.DefaultDepthStencil().GetDepthStencilView() );

// 		gs_Device.SetRenderTarget( gs_Device.DefaultRenderTarget(), &gs_Device.DefaultDepthStencil() );
// 		gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled );
// 
// 		m_ppRTParticlePositions[1]->SetVS( 10 );
// 		m_ppRTParticleNormals[0]->SetVS( 11 );
// 		m_ppRTParticleTangents[0]->SetVS( 12 );
// 		m_pTexVoronoi->SetPS( 13 );
// 
// //		m_pPrimParticle->RenderInstanced( *m_pMatDisplay, EFFECT_PARTICLES_COUNT*EFFECT_PARTICLES_COUNT );
// 		m_pPrimParticle->Render( *m_pMatDisplay );

		USING_MATERIAL_END
	}
}
