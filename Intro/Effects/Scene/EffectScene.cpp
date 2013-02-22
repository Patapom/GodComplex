#include "../../../GodComplex.h"
#include "EffectScene.h"
#include "Scene.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectScene::EffectScene( Device& _Device, Scene& _Scene, Primitive& _ScreenQuad ) : m_Device( _Device ), m_Scene( _Scene ), m_ScreenQuad( _ScreenQuad ), m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDepthPass = CreateMaterial( IDR_SHADER_SCENE_DEPTH_PASS, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, NULL ), 1 );
	CHECK_MATERIAL( m_pMatBuildLinearZ = CreateMaterial( IDR_SHADER_SCENE_BUILD_LINEARZ, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatFillGBuffer = CreateMaterial( IDR_SHADER_SCENE_FILL_GBUFFER, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatShading = CreateMaterial( IDR_SHADER_SCENE_SHADING, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1 );

	//////////////////////////////////////////////////////////////////////////
	// Create the render targets
	int		W = gs_Device.DefaultRenderTarget().GetWidth();
	int		H = gs_Device.DefaultRenderTarget().GetHeight();

	m_pDepthStencilFront = new Texture2D( m_Device, W, H, DepthStencilFormatD32F::DESCRIPTOR );
	m_pDepthStencilBack = new Texture2D( m_Device, W, H, DepthStencilFormatD32F::DESCRIPTOR );
	m_pRTZBuffer= new Texture2D( m_Device, W, H, 1, PixelFormatRG32F::DESCRIPTOR, 1, NULL );

	m_pRTGBuffer0_2 = new Texture2D( m_Device, W, H, 3, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pRTGBuffer3 = new Texture2D( m_Device, W, H, 1, PixelFormatRGBA16_UINT::DESCRIPTOR, 1, NULL );

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Render = new CB<CBRender>( m_Device, 10 );
	m_pCB_Render->m.DeltaTime.Set( 0, 1 );
}

EffectScene::~EffectScene()
{
	delete m_pCB_Render;

	delete m_pRTGBuffer3;
	delete m_pRTGBuffer0_2;
	delete m_pRTZBuffer;
	delete m_pDepthStencilBack;
	delete m_pDepthStencilFront;

	delete m_pMatShading;
 	delete m_pMatFillGBuffer;
	delete m_pMatBuildLinearZ;
	delete m_pMatDepthPass;
}

void	EffectScene::Render( float _Time, float _DeltaTime, Texture2D* _pTex )
{
	int		W = gs_Device.DefaultRenderTarget().GetWidth();
	int		H = gs_Device.DefaultRenderTarget().GetHeight();

	m_pCB_Render->m.dUV.Set( 1.0f / W, 1.0f / H, 0.0f );
	m_pCB_Render->m.DeltaTime.Set( _Time, _DeltaTime );

	// Update scene once
	m_Scene.Update( _Time, _DeltaTime );

	//////////////////////////////////////////////////////////////////////////
	// 1] Render scene in depth pre-pass in front & back Z Buffers
	USING_MATERIAL_START( *m_pMatDepthPass )

	// Render front faces in front Z buffer
	gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_ZPrePass );

	m_Device.ClearDepthStencil( *m_pDepthStencilFront, 1.0f, 0 );

	ID3D11RenderTargetView*	ppRenderTargets[1] = { NULL };	// No render target => boost!
	gs_Device.SetRenderTargets( W, H, 0, ppRenderTargets, m_pDepthStencilFront->GetDepthStencilView() );
	m_Scene.Render( M, true );

	// Render back faces in back Z buffer
	gs_Device.SetStates( gs_Device.m_pRS_CullFront, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_ZPrePass );

	m_Device.ClearDepthStencil( *m_pDepthStencilBack, 1.0f, 0 );

	gs_Device.SetRenderTargets( W, H, 0, ppRenderTargets, m_pDepthStencilBack->GetDepthStencilView() );
	m_Scene.Render( M, true );

	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 2] Concatenate and linearize front & back Z Buffers
	USING_MATERIAL_START( *m_pMatBuildLinearZ )

	gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );

	gs_Device.SetRenderTarget( *m_pRTZBuffer );

	m_pDepthStencilFront->SetPS( 10 );
	m_pDepthStencilBack->SetPS( 11 );

	m_pCB_Render->UpdateData();
	m_ScreenQuad.Render( M );

	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 3] Render the scene in our first G-Buffer
	USING_MATERIAL_START( *m_pMatFillGBuffer )

	gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadLessEqual, gs_Device.m_pBS_Disabled );

	ID3D11RenderTargetView*	ppRenderTargets[4] =
	{
		m_pRTGBuffer0_2->GetTargetView( 0, 0, 1 ),	// Normal (XY) + Tangent (XY)
		m_pRTGBuffer0_2->GetTargetView( 0, 1, 1 ),	// Diffuse Albedo (XYZ) + Tangent (W)
		m_pRTGBuffer0_2->GetTargetView( 0, 2, 1 ),	// Specular Albedo (XYZ) + Height (W)
		m_pRTGBuffer3->GetTargetView( 0, 0, 1 )		// 4 couples of [Weight,MatId] each packed into a U16
	};
	gs_Device.SetRenderTargets( W, H, 4, ppRenderTargets, m_pDepthStencilFront->GetDepthStencilView() );

	m_Scene.Render( M );

	USING_MATERIAL_END
	

	//////////////////////////////////////////////////////////////////////////
	// 4] Apply shading using my Pom materials! ^^
	USING_MATERIAL_START( *m_pMatShading )

	gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );
	gs_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), &m_Device.DefaultDepthStencil() );

	m_pRTGBuffer0_2->SetPS( 10 );
	m_pRTGBuffer3->SetPS( 11 );
	m_pRTZBuffer->SetPS( 12 );


_pTex->SetPS( 13 );
	

	m_pCB_Render->UpdateData();
	m_ScreenQuad.Render( M );

	USING_MATERIAL_END
}
