#include "../../../GodComplex.h"
#include "EffectScene.h"
#include "Scene.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectScene::EffectScene( Device& _Device, Scene& _Scene, Primitive& _ScreenQuad ) : m_Device( _Device ), m_Scene( _Scene ), m_ScreenQuad( _ScreenQuad ), m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDepthPass = CreateMaterial( IDR_SHADER_SCENE_DEPTH_PASS, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, NULL ), 1 );
	CHECK_MATERIAL( m_pMatFillGBuffer = CreateMaterial( IDR_SHADER_SCENE_FILL_GBUFFER, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatShading = CreateMaterial( IDR_SHADER_SCENE_SHADING, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1 );

	//////////////////////////////////////////////////////////////////////////
	// Create the render targets
	int		W = gs_Device.DefaultRenderTarget().GetWidth();
	int		H = gs_Device.DefaultRenderTarget().GetHeight();

	m_pDepthStencil = new Texture2D( m_Device, W, H, DepthStencilFormatD32F::DESCRIPTOR );
	m_pRT = new Texture2D( m_Device, W, H, 4, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Render = new CB<CBRender>( m_Device, 10 );
	m_pCB_Render->m.DeltaTime.Set( 0, 1 );
}

EffectScene::~EffectScene()
{
	delete m_pCB_Render;

	delete m_pRT;
	delete m_pDepthStencil;

	delete m_pMatShading;
 	delete m_pMatFillGBuffer;
	delete m_pMatDepthPass;
}

void	EffectScene::Render( float _Time, float _DeltaTime )
{
	int		W = gs_Device.DefaultRenderTarget().GetWidth();
	int		H = gs_Device.DefaultRenderTarget().GetHeight();

	m_pCB_Render->m.dUV.Set( 1.0f / W, 1.0f / H, 0.0f );
	m_pCB_Render->m.DeltaTime.Set( _Time, _DeltaTime );

	// Update scene once
	m_Scene.Update( _Time, _DeltaTime );

	//////////////////////////////////////////////////////////////////////////
	// 1] Render scene in depth pre-pass
	{	USING_MATERIAL_START( *m_pMatDepthPass )

	 	gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_ZPrePass );

		m_Device.ClearDepthStencil( *m_pDepthStencil, 1.0f, 0 );

		ID3D11RenderTargetView*	ppRenderTargets[1] = { NULL };	// No render target => boost!
		gs_Device.SetRenderTargets( W, H, 0, ppRenderTargets, m_pDepthStencil->GetDepthStencilView() );
		m_Scene.Render( M, true );

		USING_MATERIAL_END
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Render the scene in our first G-Buffer
	{	USING_MATERIAL_START( *m_pMatFillGBuffer )

	 	gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadLessEqual, gs_Device.m_pBS_Disabled );

// 		ID3D11RenderTargetView*	ppRenderTargets[2] =
// 		{
// 			m_pRT->GetTargetView( 0, 0, 1 ), m_pRT->GetTargetView( 0, 0, 1 )
// 		};
// 		gs_Device.SetRenderTargets( gs_Device.DefaultRenderTarget().GetWidth(), gs_Device.DefaultRenderTarget().GetHeight(), 2, ppRenderTargets, gs_Device.DefaultDepthStencil().GetDepthStencilView() );
 		gs_Device.SetRenderTarget( *m_pRT, m_pDepthStencil );
 
		m_Scene.Render( M );

		USING_MATERIAL_END
	}

	//////////////////////////////////////////////////////////////////////////
	// 3] Apply shading using my Pom materials! ^^
	{	USING_MATERIAL_START( *m_pMatShading )

	 	gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );
 		gs_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), &m_Device.DefaultDepthStencil() );
 
		m_pRT->SetPS( 10 );
		m_pDepthStencil->SetPS( 11 );

		m_pCB_Render->UpdateData();
		m_ScreenQuad.Render( M );

		USING_MATERIAL_END
	}
}
