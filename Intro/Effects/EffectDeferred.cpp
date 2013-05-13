#include "../../GodComplex.h"
#include "EffectDeferred.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectDeferred::EffectDeferred() : m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDepthPass = CreateMaterial( IDR_SHADER_DEFERRED_DEPTH_PASS, "./Resources/Shaders/DeferredDepthPass.hlsl", VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, NULL ), 1 );
	CHECK_MATERIAL( m_pMatFillGBuffer = CreateMaterial( IDR_SHADER_DEFERRED_FILL_GBUFFER, "./Resources/Shaders/DeferredFillGBuffer.hlsl", VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatShading_StencilPass = CreateMaterial( IDR_SHADER_DEFERRED_SHADING_STENCIL, "./Resources/Shaders/DeferredShadingStencil.hlsl", VertexFormatP3::DESCRIPTOR, "VS", NULL, NULL ), 1 );
	CHECK_MATERIAL( m_pMatShading = CreateMaterial( IDR_SHADER_DEFERRED_SHADING, "./Resources/Shaders/DeferredShading.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1 );


	//////////////////////////////////////////////////////////////////////////
	// Build the primitives
	{
		m_pPrimSphere = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildSphere( 60, 30, *m_pPrimSphere );
	}

	// Create the render targets
	m_pRTGBuffer = new Texture2D( gs_Device, gs_Device.DefaultRenderTarget().GetWidth(), gs_Device.DefaultRenderTarget().GetHeight(), 2, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Render = new CB<CBRender>( gs_Device, 10 );
	m_pCB_Render->m.DeltaTime.Set( 0, 1 );
}

EffectDeferred::~EffectDeferred()
{
//	delete m_pTexVoronoi;

	delete m_pCB_Render;

	delete m_pRTGBuffer;

	delete m_pPrimSphere;

	delete m_pMatShading;
	delete m_pMatShading_StencilPass;
	delete m_pMatFillGBuffer;
	delete m_pMatDepthPass;
}

void	EffectDeferred::Render( float _Time, float _DeltaTime )
{
/*
	//////////////////////////////////////////////////////////////////////////
	// 1] Render objects in Z pre-pass
	{	USING_MATERIAL_START( *m_pMatFillGBuffer )
	
		ID3D11RenderTargetView*	ppRenderTargets[2] =
		{
			m_pRTGBuffer->GetTargetView( 0, 0, 1 ), m_pRTGBuffer->GetTargetView( 0, 0, 1 )
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

//////////////////////////////////////////////////////////////////////////
// 
		CB<CBObject>*	m_pCB_Object;
		Primitive*		m_pPrimitive;
		Texture2D*		m_pTexDiffuseAO;
		Texture2D*		m_pTexNormal;

Object::Object()
{
	m_pCB_Object = new CB<CBObject>( gs_Device, 10 );
}
Object::~Object()
{
	delete m_pCB_Object;
}

void	Object::SetPrimitive( Primitive& _Primitive, const Texture2D& _TexDiffuseAO, const Texture2D& _TexNormal )
{

}

void	Object::Render( bool _IsDepthPass ) const
{

}

