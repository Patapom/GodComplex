#include "../../GodComplex.h"
#include "EffectDOF.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectDOF::EffectDOF( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera ) : m_ErrorCode( 0 ), m_Device( _Device ), m_RTTarget( _RTHDR ), m_ScreenQuad( _ScreenQuad )
{
#ifdef SHADERTOY
 	CHECK_MATERIAL( m_pMatShadertoy = CreateMaterial( IDR_SHADER_SHADERTOY, "./Resources/Shaders/Shadertoy.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	
#else
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	D3D_SHADER_MACRO	pMacros[] = { { "USE_SHADOW_MAP", "1" }, { NULL, NULL } };
 	CHECK_MATERIAL( m_pMatRender = CreateMaterial( IDR_SHADER_DOF_RENDER_SCENE, "./Resources/Shaders/DOFRenderScene.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS", pMacros ), 1 );
 	CHECK_MATERIAL( m_pMatRenderCube = CreateMaterial( IDR_SHADER_DOF_RENDER_CUBE, "./Resources/Shaders/DOFRenderCube.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS", pMacros ), 1 );

// 	D3D_SHADER_MACRO	pMacros2[] = { { "EMISSIVE", "1" }, { NULL, NULL } };
// 	CHECK_MATERIAL( m_pMatRenderEmissive = CreateMaterial( IDR_SHADER_GI_RENDER_SCENE, "./Resources/Shaders/GIRenderScene2.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS", pMacros2 ), 2 );
//  	CHECK_MATERIAL( m_pMatRenderLights = CreateMaterial( IDR_SHADER_GI_RENDER_LIGHTS, "./Resources/Shaders/GIRenderLights.hlsl", VertexFormatP3N3::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
//  	CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_GI_RENDER_CUBEMAP, "./Resources/Shaders/GIRenderCubeMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 4 );
//  	CHECK_MATERIAL( m_pMatRenderNeighborProbe = CreateMaterial( IDR_SHADER_GI_RENDER_NEIGHBOR_PROBE, "./Resources/Shaders/GIRenderNeighborProbe.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 5 );
//  	CHECK_MATERIAL( m_pMatRenderShadowMap = CreateMaterial( IDR_SHADER_GI_RENDER_SHADOW_MAP, "./Resources/Shaders/GIRenderShadowMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, NULL ), 6 );

	{
		D3D_SHADER_MACRO	pMacros2[] = { { "TYPE", "2" }, { NULL, NULL } };
  		CHECK_MATERIAL( m_pMatDownsampleMin = CreateMaterial( IDR_SHADER_DOF_DOWNSAMPLE, "./Resources/Shaders/DOFDownsample.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS", pMacros2 ), 9 );
	}
	{
		D3D_SHADER_MACRO	pMacros2[] = { { "TYPE", "1" }, { NULL, NULL } };
  		CHECK_MATERIAL( m_pMatDownsampleMax = CreateMaterial( IDR_SHADER_DOF_DOWNSAMPLE, "./Resources/Shaders/DOFDownsample.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS", pMacros2 ), 9 );
	}
	{
		D3D_SHADER_MACRO	pMacros2[] = { { "TYPE", "3" }, { NULL, NULL } };
	  	CHECK_MATERIAL( m_pMatDownsampleAvg = CreateMaterial( IDR_SHADER_DOF_DOWNSAMPLE, "./Resources/Shaders/DOFDownsample.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS", pMacros2 ), 9 );
	}
  	CHECK_MATERIAL( m_pMatComputeFuzzinessNear = CreateMaterial( IDR_SHADER_DOF_COMPUTE_FUZZINESS, "./Resources/Shaders/DOFComputeFuzziness.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS_Near" ), 10 );
  	CHECK_MATERIAL( m_pMatComputeFuzzinessFar = CreateMaterial( IDR_SHADER_DOF_COMPUTE_FUZZINESS, "./Resources/Shaders/DOFComputeFuzziness.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS_Far" ), 11 );
  	CHECK_MATERIAL( m_pMatComputeFuzzinessBlur = CreateMaterial( IDR_SHADER_DOF_COMPUTE_FUZZINESS, "./Resources/Shaders/DOFComputeFuzziness.hlsl", VertexFormatPt4::DESCRIPTOR, "VS_Blur", NULL, "PS_Blur" ), 12 );

  	CHECK_MATERIAL( m_pMatDOFNear = CreateMaterial( IDR_SHADER_DOF_COMPUTE, "./Resources/Shaders/DOFCompute.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS_Near" ), 15 );
  	CHECK_MATERIAL( m_pMatDOFFar = CreateMaterial( IDR_SHADER_DOF_COMPUTE, "./Resources/Shaders/DOFCompute.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS_Far" ), 16 );
  	CHECK_MATERIAL( m_pMatDOFCombine = CreateMaterial( IDR_SHADER_DOF_COMPUTE, "./Resources/Shaders/DOFCompute.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS_Combine" ), 17 );

// 	// Compute Shaders
// 	CHECK_MATERIAL( m_pCSUpdateProbe = CreateComputeShader( IDR_SHADER_GI_UPDATE_PROBE, "./Resources/Shaders/GIUpdateProbe.hlsl", "CS" ), 20 );


	//////////////////////////////////////////////////////////////////////////
	// Create the textures
	{
		TextureFilePOM	POM( "./Resources/Scenes/GITest1/pata_diff_colo.pom" );
		m_pTexWalls = new Texture2D( _Device, POM );
	}

	// Create the shadow map
	m_pRTShadowMap = new Texture2D( _Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );


	// Create the render targets for DOF
	U32	W = m_RTTarget.GetWidth();
	U32	H = m_RTTarget.GetHeight();
	U32	HalfWidth = (W+1) >> 1;
	U32	HalfHeight = (H+1) >> 1;
	U32	QuarterWidth = (HalfWidth+1) >> 1;
	U32	QuarterHeight = (HalfHeight+1) >> 1;

	m_pRTDOFMask = new Texture2D( m_Device, HalfWidth, HalfHeight, 1, PixelFormatRG16F::DESCRIPTOR, 2, NULL );
	m_pRTDepthAlphaBuffer = new Texture2D( m_Device, W, H, DepthStencilFormatD32F::DESCRIPTOR );
	m_pRTDOF = new Texture2D( m_Device, HalfWidth, HalfHeight, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pRTDownsampledHDRTarget = new Texture2D( m_Device, HalfWidth, HalfHeight, 1, PixelFormatRGBA16F::DESCRIPTOR, 2, NULL );

// 	ScopedTempImage	hrDiv4RenderDest( MRB_RENDER_DOF, ARK_FORMAT_RGBA16_FLOAT, QuarterWidth, QuarterHeight );
//	ScopedTempImage	dofMaskDiv4RenderDest( MRB_RENDER_DOF, ARK_FORMAT_RG16_FLOAT, QuarterWidth, QuarterHeight );
	ScopedTempImage	dofHrDiv2RenderDest( MRB_RENDER_DOF, ARK_FORMAT_RGBA16_FLOAT, HalfWidth, HalfHeight );


	m_pRTTemp = new Texture2D( m_Device, QuarterWidth, QuarterHeight, 2, PixelFormatR16F::DESCRIPTOR, 1, NULL );

// 		opts.Format(ARK_FORMAT_RG16_FLOAT).Width(HalfWidth).Height(HalfHeight).NumLevels(1).BindType(TB_RENDER_TARGET);
// 		idImage * dofMaskDiv2Buffer = globalImages->ScratchImage( va( "_DOF_MASK_DIV2_%p", _hDC ), opts );
// 		pCtxt->m_dofMaskDiv2RenderDest->CreateFromImages( dofMaskDiv2Buffer, NULL, NULL ADDITIONAL_CREATE_FROM_IMAGES_PARAMS );
// 
// 		opts.Format(ARK_FORMAT_D32_FLOAT).Width(width).Height(height).NumLevels(1).BindType(TB_DEPTH_STENCIL_TARGET); //ftournade: temporary fix fo MS - format should be ARK_FORMAT_D24S8_TYPELES
// 		idImage * depthAlphaBuffer = globalImages->ScratchImage( va( "_DepthAlphaBuffer_%p", _hDC ), opts );
// 		pCtxt->m_depthAlphaBufferRenderDest->CreateFromImages( NULL, depthAlphaBuffer, depthAlphaBuffer ADDITIONAL_CREATE_FROM_IMAGES_PARAMS );
// 
// 		opts.Format(ARK_FORMAT_RGBA16_FLOAT).Width(HalfWidth).Height(HalfHeight).NumLevels(1).BindType(TB_RENDER_TARGET);
// 		idImage * dofHrDiv2Buffer = globalImages->ScratchImage( va( "_DOF_HR_DIV2_%p", _hDC ), opts );
// 		pCtxt->m_dofDiv2RenderDest->CreateFromImages( dofHrDiv2Buffer, NULL, NULL ADDITIONAL_CREATE_FROM_IMAGES_PARAMS );
// 
// 		opts.Format(ARK_FORMAT_RGBA16_FLOAT).Width(HalfWidth).Height(HalfHeight).NumLevels(1).BindType(TB_RENDER_TARGET);
// 		idImage * hrDiv2Buffer = globalImages->ScratchImage( va( "_HRBuffer_DIV2_%p", _hDC ), opts );
// 		pCtxt->m_hrBufferDiv2RenderDest->CreateFromImages( hrDiv2Buffer, NULL, NULL ADDITIONAL_CREATE_FROM_IMAGES_PARAMS );


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_General = new CB<CBGeneral>( _Device, 9, true );
	m_pCB_Scene = new CB<CBScene>( _Device, 10 );
 	m_pCB_Object = new CB<CBObject>( _Device, 11 );
 	m_pCB_Material = new CB<CBMaterial>( _Device, 12 );
	m_pCB_Splat = new CB<CBSplat>( _Device, 10 );

	m_pCB_Scene->m.DynamicLightsCount = 0;
	m_pCB_Scene->m.StaticLightsCount = 0;
	m_pCB_Scene->m.ProbesCount = 0;



	//////////////////////////////////////////////////////////////////////////
	// Create the scene
	m_bDeleteSceneTags = false;
// 	m_TotalFacesCount = 0;
// 	m_TotalPrimitivesCount = 0;
	m_Scene.Load( IDR_SCENE_GI, *this );

// 	// Upload static lights once and for all
// 	m_pSB_LightsStatic->Write( m_pCB_Scene->m.StaticLightsCount );
// 	m_pSB_LightsStatic->SetInput( 7, true );

	// Update once so it's ready when we pre-compute probes
	m_pCB_Scene->UpdateData();


	//////////////////////////////////////////////////////////////////////////
	// Create our cube primitive for testing DOF
	m_pPrimCube = new Primitive( _Device, VertexFormatP3N3G3B3T2::DESCRIPTOR );
	GeometryBuilder::BuildCube( 15, 15, 15, *m_pPrimCube );


	//////////////////////////////////////////////////////////////////////////
	// Start precomputation
//	PreComputeProbes();
#endif
}

EffectDOF::~EffectDOF()
{
#ifdef SHADERTOY
 	delete m_pMatShadertoy;
#else
	delete m_pPrimCube;

	m_bDeleteSceneTags = true;
	m_Scene.ClearTags( *this );

	delete m_pCB_Splat;
	delete m_pCB_Material;
	delete m_pCB_Object;
	delete m_pCB_Scene;
	delete m_pCB_General;

	delete m_pRTTemp;

	delete m_pRTDownsampledHDRTarget;
	delete m_pRTDOF;
	delete m_pRTDepthAlphaBuffer;
	delete m_pRTDOFMask;

	delete m_pRTShadowMap;
	delete m_pTexWalls;

// 	delete m_pCSUpdateProbe;
 
 	delete m_pMatCombine;
	delete m_pMatDownsampleAvg;
	delete m_pMatDownsampleMax;
	delete m_pMatDownsampleMin;
 	delete m_pMatComputeFuzzinessBlur;
 	delete m_pMatComputeFuzzinessFar;
 	delete m_pMatComputeFuzzinessNear;
// 	delete m_pCSComputeShadowMapBounds;
// 	delete m_pMatRenderShadowMap;
// 	delete m_pMatRenderNeighborProbe;
// 	delete m_pMatRenderCubeMap;
// 	delete m_pMatRenderLights;
	delete m_pMatRenderCube;
	delete m_pMatRender;
#endif
}

void	EffectDOF::Render( float _Time, float _DeltaTime )
{
#ifdef SHADERTOY

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	D3D11_VIEWPORT	Viewport;
	Viewport.TopLeftX = 1010;
	Viewport.TopLeftY = 509;
	Viewport.Width = 1;
	Viewport.Height = 1;
	Viewport.MinDepth = 0.0f;
	Viewport.MaxDepth = 1.0f;
//  	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), NULL, &Viewport );
 	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget() );

	USING_MATERIAL_START( *m_pMatShadertoy )

	m_ScreenQuad.Render( M );

	USING_MATERIAL_END

#else
	// Setup general data
	m_pCB_General->m.ShowIndirect = gs_WindowInfos.pKeys[VK_RETURN] == 0;
	m_pCB_General->UpdateData();

	// Setup scene data
	m_pCB_Scene->m.DynamicLightsCount = 0;
	m_pCB_Scene->m.ProbesCount = 0;
	m_pCB_Scene->UpdateData();


	//////////////////////////////////////////////////////////////////////////
	// 1] Render the scene
// 	m_Device.ClearRenderTarget( m_RTTarget, NjFloat4::Zero );

 	m_Device.SetRenderTarget( m_RTTarget, &m_Device.DefaultDepthStencil() );
	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

	m_Scene.Render( *this );


 	//////////////////////////////////////////////////////////////////////////
	// 2] Render the cube
	m_Device.SetStates( m_Device.m_pRS_CullNone, NULL, NULL );

	USING_MATERIAL_START( *m_pMatRenderCube )

	float		Phi = _Time; 
	float		Theta = 0.33f * _Time; 

	float3	P( 0.0f, 1.0f, 0.0f );
	float3	Axis( sinf(Theta)*sinf(Phi), sinf(Theta)*cosf(Phi), cosf(Theta) );
	float4	R = float4::QuatFromAngleAxis( _Time, Axis );
	float3	S = 0.25f * float3::One;

	m_pCB_Object->m.Local2World.PRS( P, R, S );
	m_pCB_Object->UpdateData();

	m_pPrimCube->Render( M );

	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 3] Depth of Field
	U32	W = m_RTTarget.GetWidth();
	U32	H = m_RTTarget.GetHeight();
	U32	HalfWidth = (W+1) >> 1;
	U32	HalfHeight = (H+1) >> 1;
	U32	QuarterWidth = (HalfWidth+1) >> 1;
	U32	QuarterHeight = (HalfHeight+1) >> 1;

	// 3.1) Start by downsampling buffers
	Downsample( HalfWidth, HalfHeight, m_RTTarget.GetShaderView(), m_pRTDownsampledHDRTarget->GetTargetView( 0, 0, 1 ), AVG );
	Downsample( QuarterWidth, QuarterHeight, m_pRTDownsampledHDRTarget->GetShaderView( 0, 1, 0, 1 ), m_pRTDownsampledHDRTarget->GetTargetView( 1, 0, 1 ), AVG );

//	DownscaleBuffer( *tr.GetCurrentWindowContext()->m_hdrBufferRenderDest->GetColorImage()->GetView(), *tr.GetCurrentWindowContext()->m_hrBufferDiv2RenderDest->GetColorImage()->GetView(), downscaleType::AVG );

	// 3.2) Compute near/far fuzziness
	RenderFuzziness();

	// 3.3) Render actual DOF
	RenderDOF();


// 	//////////////////////////////////////////////////////////////////////////
// 	// 2] Render the lights
// 	USING_MATERIAL_START( *m_pMatRenderLights )
// 
// 	m_pPrimSphere->RenderInstanced( M, 1 );	// Only show point light, no sun light
// 
// 	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 3] Post-process the result
	USING_MATERIAL_START( *m_pMatCombine )

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );
	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget() );

	m_RTTarget.SetPS( 10 );

m_pRTDownsampledHDRTarget->SetPS( 64 );

	m_pCB_Splat->m.dUV = m_Device.DefaultRenderTarget().GetdUV();
	m_pCB_Splat->UpdateData();

	m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	m_RTTarget.RemoveFromLastAssignedSlots();
#endif
}

#ifndef SHADERTOY

void	EffectDOF::Downsample( int _TargetWidth, int _TargetHeight, const ID3D11ShaderResourceView& _Source, const ID3D11RenderTargetView& _Target, DOWNSAMPLE_TYPE _Type ) const
{
	Material*	pMaterial = NULL;
	switch ( _Type )
	{
	case 0:	pMaterial = m_pMatDownsampleMin; break;
	case 1:	pMaterial = m_pMatDownsampleMax; break;
	case 2:	pMaterial = m_pMatDownsampleAvg; break;
	}

	USING_MATERIAL_START( *pMaterial )

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );
	m_Device.SetRenderTarget( _TargetWidth, _TargetHeight, _Target );

	ID3D11ShaderResourceView*	pTarget = &_Source;
	m_Device.DXContext().PSSetShaderResources( 10, 1, &pTarget );

//	m_pCB_Splat->m.dUV = _Source.GetdUV();
	m_pCB_Splat->UpdateData();

	m_ScreenQuad.Render( M );

	USING_MATERIAL_END

//	m_RTTarget.RemoveFromLastAssignedSlots();


// 	GL_SetDefaultState();
// 	const arkRenderProgView* progDownsample = NULL;
// 
// 	idassert(outRT.GetImage() || !"[CRASH] No color buffer, use DownscaleDepthBuffer");
// 
// 	switch(outRT.GetImage()->GetOpts().Format().Layout()) {
// 	case ARK_GFX_FORMAT_LAYOUT_16_16:
// 		progDownsample = tr.m_progDownscaleMinG16R16;
// 		break;
// 	default:
// 		switch(downType) {
// 		case downscaleType::MIN: progDownsample = tr.m_progDownscaleMinARGB; break;
// 		case downscaleType::MAX: progDownsample = tr.m_progDownscaleMaxARGB; break;
// 		case downscaleType::AVG: progDownsample = tr.m_progDownscaleAvgARGB; break;
// 		default: idassert(0);
// 		}
// 		
// 	}
// 	idassert(progDownsample);
// 
// 	const int width = inImage.GetImage()->GetView()->GetWidth();
// 	const int height = inImage.GetImage()->GetView()->GetHeight();
// 	const float ratioX = 1.f;//(width/outRT.GetWidth()) * 0.5f;
// 	const float ratioY = 1.f;//(width/outRT.GetHeight()) * 0.5f;
// 
// 	renderThreadParmState->SetParmVector( tr.m_rpDownscaleBufferInTexelSize, ratioX/width, ratioY/height, 0.f, 0.f);
// 	renderThreadParmState->SetParmImage( tr.m_rpDownscaleBufferIn, inImage.GetImage() );	
// 	GL_SetRenderDestination( &outRT );
// 	GL_DrawElements( progDownsample, tr.unitSquareTris, 0 );
}

void	EffectDOF::RenderFuzziness() const
{
	// Get temporary buffer
	int	W = m_pRTDOFMask->GetWidth();// tr.GetCurrentWindowContext()->m_dofMaskDiv2RenderDest->GetTargetWidth();
	int	H = m_pRTDOFMask->GetHeight();//tr.GetCurrentWindowContext()->m_dofMaskDiv2RenderDest->GetTargetHeight();
	int	HalfWidth = (W+1) >> 1;
	int	HalfHeight = (H+1) >> 1;


// 	ScopedTempImage	tempDofMaskTemp0( MRB_ARK_SSAO, ARK_FORMAT_R16_FLOAT, HalfWidth, HalfHeight );
// 	ScopedTempImage	tempDofMaskTemp1( MRB_ARK_SSAO, ARK_FORMAT_R16_FLOAT, HalfWidth, HalfHeight );

	//compute fuzziness Near
//	renderThreadParmState->SetParmImage( tr.m_rpDofZbufferDownsampled, 	tr.GetCurrentWindowContext()->m_downsampledDepthBuffer->GetView( 0, 3 )->GetImage() );	

	// 1] Compute near fuzziness
	USING_MATERIAL_START( *m_pMatComputeFuzzinessNear )

	m_Device.SetRenderTarget( m_pRTTemp->GetWidth(), m_pRTTemp->GetHeight(), *m_pRTTemp->GetTargetView( 0, 0, 1 ) );

//	_Source.SetPS( 10 );

//	m_pCB_Splat->m.dUV = _Source.GetdUV();
	m_pCB_Splat->UpdateData();

	m_ScreenQuad.Render( M );

	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 2] Blur it
	USING_MATERIAL_START( *m_pMatComputeFuzzinessBlur )

// 	float	Offsets[DOF_OFFSET_COUNT];
// 	float	Weights[DOF_WEIGHT_COUNT];

	const float	Coeff = 2.5f;
	const float	weights[4] = { 0.3125f * Coeff, 0.234375f * Coeff, 0.09375f * Coeff, 0.015625f * Coeff };

	{	// 2.1] Horizontal blur
		m_Device.SetRenderTarget( m_pRTTemp->GetWidth(), m_pRTTemp->GetHeight(), *m_pRTTemp->GetTargetView( 0, 1, 1 ) );

		ComputeKernel( &m_pCB_Splat->m.Offsets->x, &m_pCB_Splat->m.Weights->x, weights, false );
		ScaleKernel( &m_pCB_Splat->m.Offsets->x, HalfWidth, HalfHeight );

// 		//flush kernel
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset0, _Offsets[ 0], _Offsets[ 1], _Offsets[ 2], _Offsets[ 3] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset1, _Offsets[ 4], _Offsets[ 5], _Offsets[ 6], _Offsets[ 7] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset2, _Offsets[ 8], _Offsets[ 9], _Offsets[10], _Offsets[11] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset3, _Offsets[12], _Offsets[13], _Offsets[14], _Offsets[15] );
// 
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight0, _Weights[ 0], _Weights[ 1], _Weights[ 2], _Weights[ 3] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight1, _Weights[ 4], _Weights[ 5], _Weights[ 6], _Weights[ 7] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight2, _Weights[ 8], _Weights[ 9], _Weights[10], _Weights[11] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight3, _Weights[12], _Weights[13], _Weights[14], _Weights[15] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight4, _Weights[16], _Weights[17], _Weights[18], _Weights[19] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight5, _Weights[20], _Weights[21], _Weights[22], _Weights[23] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight6, _Weights[24], _Weights[25], _Weights[26], _Weights[27] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight7, _Weights[28], _Weights[29], _Weights[30], _Weights[31] );
// 
// 		renderThreadParmState->SetParmImage( tr.m_rpDofNearFuzziness, &tempDofMaskTemp0 );

		m_pCB_Splat->UpdateData();

		m_pRTTemp->SetPS( 10, false, m_pRTTemp->GetShaderView( 0, 1, 0, 1 ) );

		m_ScreenQuad.Render( M );
	}

	{	// 2.2] Vertical blur
		m_Device.SetRenderTarget( m_pRTTemp->GetWidth(), m_pRTTemp->GetHeight(), *m_pRTTemp->GetTargetView( 0, 0, 1 ) );

		ComputeKernel( &m_pCB_Splat->m.Offsets->x, &m_pCB_Splat->m.Weights->x, weights, true );
		ScaleKernel( &m_pCB_Splat->m.Offsets->x, HalfWidth, HalfHeight );

// 		//flush kernel
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset0, _Offsets[ 0], _Offsets[ 1], _Offsets[ 2], _Offsets[ 3] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset1, _Offsets[ 4], _Offsets[ 5], _Offsets[ 6], _Offsets[ 7] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset2, _Offsets[ 8], _Offsets[ 9], _Offsets[10], _Offsets[11] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurOffset3, _Offsets[12], _Offsets[13], _Offsets[14], _Offsets[15] );
// 
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight0, _Weights[ 0], _Weights[ 1], _Weights[ 2], _Weights[ 3] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight1, _Weights[ 4], _Weights[ 5], _Weights[ 6], _Weights[ 7] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight2, _Weights[ 8], _Weights[ 9], _Weights[10], _Weights[11] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight3, _Weights[12], _Weights[13], _Weights[14], _Weights[15] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight4, _Weights[16], _Weights[17], _Weights[18], _Weights[19] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight5, _Weights[20], _Weights[21], _Weights[22], _Weights[23] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight6, _Weights[24], _Weights[25], _Weights[26], _Weights[27] );
// 		renderThreadParmState->SetParmVector( tr.m_rpDofBlurWeight7, _Weights[28], _Weights[29], _Weights[30], _Weights[31] );
// 
// 		//draw
// 		renderThreadParmState->SetParmImage( tr.m_rpDofNearFuzziness, &tempDofMaskTemp1 );

		m_pCB_Splat->UpdateData();

		m_pRTTemp->SetPS( 10, false, m_pRTTemp->GetShaderView( 0, 1, 1, 1 ) );

		m_ScreenQuad.Render( M );
	}

	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 3] Compute far fuzziness & combine it with near fuzziness
	USING_MATERIAL_START( *m_pMatComputeFuzzinessFar )

	m_Device.SetRenderTarget( *m_pRTDOFMask->GetTargetView( 0, 0, 1 ) );

	m_pRTTemp->SetPS( 10, false, m_pRTTemp->GetShaderView( 0, 1, 0, 1 ) );

//	m_pCB_Splat->m.dUV = _Source.GetdUV();
	m_pCB_Splat->UpdateData();

	m_ScreenQuad.Render( M );

	USING_MATERIAL_END

// 	renderThreadParmState->SetParmImage( tr.m_rpDofZbufferDownsampled, 	tr.GetCurrentWindowContext()->m_downsampledDepthBuffer->GetView( 0, 3 )->GetImage() );	
// 	renderThreadParmState->SetParmImage( tr.m_rpDofZbufferAlpha, tr.GetCurrentWindowContext()->m_depthAlphaBufferRenderDest->GetDepthImage()->GetDefaultView()->GetImage() );	
// 	renderThreadParmState->SetParmImage( tr.m_rpDofNearFuzziness, &tempDofMaskTemp0 );
// 	GL_SetRenderDestination( width, height, tr.GetCurrentWindowContext()->m_dofMaskDiv2RenderDest->GetColorImage()->GetDefaultView() );
// 
// 	GL_DrawElements( tr.m_progDofFuzzinessFar, tr.unitSquareTris, 0 );
}

void	EffectDOF::RenderDOF() const
{
	U32	W = m_RTTarget.GetWidth();
	U32	H = m_RTTarget.GetHeight();
	U32	HalfWidth = (W+1) >> 1;
	U32	HalfHeight = (H+1) >> 1;
	U32	QuarterWidth = (HalfWidth+1) >> 1;
	U32	QuarterHeight = (HalfHeight+1) >> 1;

	// Downsample fuzziness
	Downsample( QuarterWidth, QuarterHeight, m_pRTDOFMask->GetShaderView( 0, 1, 0, 1 ), m_pRTDOFMask->GetTargetView( 1, 0, 1 ), DOWNSAMPLE_TYPE::MIN );


	const float centerX = 0.5f;//(_pViewportState->GetViewportWidth()/2.f + _pViewportState->GetViewportX())/(float)_pRessources->GetPostFullResTexture()->GetWidth();
	const float centerY = 0.5f;//(_pViewportState->GetViewportHeight()/2.f + _pViewportState->GetViewportY())/(float)_pRessources->GetPostFullResTexture()->GetHeight();

	//////////////////////////////////////////////////////////////////////////
	// PASS NEAR
	USING_MATERIAL_START( m_pMat )
	{
		//TODO STENCIL

		renderThreadParmState->SetParmVector(tr.m_rpDofTargetSize, 1.0f / HalfWidth, 1.0f / HalfHeight, 1.0f / QuarterWidth, 1.0f / QuarterHeight );
		renderThreadParmState->SetParmVec2(tr.m_rpDofCenterParams, idVec2(centerX, centerY));

		renderThreadParmState->SetParmImage( tr.m_rpDofDiffuseMap, tr.GetCurrentWindowContext()->m_hrBufferDiv2RenderDest->GetColorImage() );	//PostFX/DOF/diffuseMap
		renderThreadParmState->SetParmImage( tr.m_rpDofDataMap, tr.GetCurrentWindowContext()->m_dofMaskDiv2RenderDest->GetColorImage() );		//PostFX/DOF/dofDataMap
		renderThreadParmState->SetParmImage( tr.m_rpDofBlurredMap, &hrDiv4RenderDest);															//PostFX/DOF/blurredMap
		renderThreadParmState->SetParmImage( tr.m_rpDofBlurredDofMap, &dofMaskDiv4RenderDest);													//PostFX/DOF/dofBlurMap


		GL_SetRenderDestination( HalfWidth, HalfHeight, dofHrDiv2RenderDest->GetDefaultView() );
		GL_DrawElements( GetRenderProgDOF(PASS_NEAR), tr.unitSquareTris, 0 );
	}
	USING_MATERIAL_END

	//////////////////////////////////////////////////////////////////////////
	// PASS FAR
	{
		//TODO STENCIL

		renderThreadParmState->SetParmVector(tr.m_rpDofTargetSize, 1.f/width, 1.f/height, 1.f/widthDiv4, 1.f/heightDiv4 );
		renderThreadParmState->SetParmVec2(tr.m_rpDofCenterParams, idVec2(centerX, centerY));

		renderThreadParmState->SetParmImage( tr.m_rpDofDiffuseMap, tr.GetCurrentWindowContext()->m_hrBufferDiv2RenderDest->GetColorImage() );	//PostFX/DOF/diffuseMap
		renderThreadParmState->SetParmImage( tr.m_rpDofDataMap, tr.GetCurrentWindowContext()->m_dofMaskDiv2RenderDest->GetColorImage() );		//PostFX/DOF/dofDataMap
		renderThreadParmState->SetParmImage( tr.m_rpDofBlurredMap, &hrDiv4RenderDest);															//PostFX/DOF/blurredMap
		renderThreadParmState->SetParmImage( tr.m_rpDofBlurredDofMap, &dofMaskDiv4RenderDest);													//PostFX/DOF/dofBlurMap

		GL_SetRenderDestination( width, height, tr.GetCurrentWindowContext()->m_dofDiv2RenderDest->GetColorImage()->GetDefaultView() );
		GL_DrawElements( GetRenderProgDOF(PASS_FAR), tr.unitSquareTris, 0 );
	}

	//////////////////////////////////////////////////////////////////////////
	// COMBINE
	{
		//Copy DOF near to DOF far buffer
		CopyBuffer(*dofHrDiv2RenderDest->GetDefaultView(), *tr.GetCurrentWindowContext()->m_dofDiv2RenderDest->GetColorImage()->GetDefaultView(), false /*invertTexCoords*/);

		//Composite with the frame buffer
		renderThreadParmState->SetParmImage( tr.m_rpDofFrameBufferMap, tr.GetCurrentWindowContext()->m_hdrBufferRenderDest->GetColorImage());
		renderThreadParmState->SetParmImage( tr.m_rpDofDepthMap, tr.GetCurrentWindowContext()->m_depthAlphaBufferRenderDest->GetDepthImage()->GetDefaultView()->GetImage());
		renderThreadParmState->SetParmImage( tr.m_rpDofMap, tr.GetCurrentWindowContext()->m_dofDiv2RenderDest->GetColorImage());
		GL_SetRenderDestination( tr.GetCurrentWindowContext()->m_hdrBufferRenderDest2->GetColorImage()->GetDefaultView() );
		GL_DrawElements( GetRenderProgCombine(), tr.unitSquareTris, 0 );

		tr.GetCurrentWindowContext()->SwapHDRBuffers();
	}

}

void	EffectDOF::ComputeKernel( float* _pOffsets, float* _pWeights, const float weights[4], bool vertical ) const
{
	float*	offset = _pOffsets;

	if ( vertical )
	{
		for ( int i=0; i < DOF_NB_SAMPLE-1; i++)
		{
			*(offset++) = 0;
			*(offset++) = (float)(i-3);
		}
	}
	else
	{
		for ( int i=0; i < DOF_NB_SAMPLE-1; i++ )
		{
			*(offset++) = (float)(i-3);
			*(offset++) = 0;
		}
	}

	*(offset++) = 0;
	*(offset++) = 0;

	float*	w = _pWeights;
	for ( int i=3; i >= 0; i-- ) // 3, 2, 1, 0
	{
		w[0] = w[1] = w[2] = w[3] = weights[i];
		w += 4;
	}
	for ( int i=1; i < 4; i++ ) // 1, 2, 3
	{
		w[0] = w[1] = w[2] = w[3] = weights[i];
		w += 4;
	}
	w[0] = w[1] = w[2] = w[3] = 0.0f;
}

void	EffectDOF::ScaleKernel( float* _pOffsets, int _Width, int _Height ) const
{
	const float ScaleX = 1.0f / _Width;
	const float ScaleY = 1.0f / _Height;

	float*	off = _pOffsets;
	for ( int i = 0; i < DOF_NB_SAMPLE; i++ )
	{
		*(off++) *= ScaleX;
		*(off++) *= ScaleY;
	}
}


//////////////////////////////////////////////////////////////////////////
// Computes the shadow map infos and render the shadow map itself
//
// void	EffectDOF::RenderShadowMap( const NjFloat3& _SunDirection )
// {
// 	//////////////////////////////////////////////////////////////////////////
// 	// Build a nice transform
// 	NjFloat3	X = (NjFloat3::UnitY ^_SunDirection).Normalize();	// Assuming the Sun is never vertical here!
// 	NjFloat3	Y = _SunDirection ^ X;
// 
// 	m_pCB_ShadowMap->m.Light2World.SetRow( 0, X );
// 	m_pCB_ShadowMap->m.Light2World.SetRow( 1, Y );
// 	m_pCB_ShadowMap->m.Light2World.SetRow( 2, -_SunDirection );
// 	m_pCB_ShadowMap->m.Light2World.SetRow( 3, NjFloat3::Zero, 1 );	// Temporary
// 
// 	m_pCB_ShadowMap->m.World2Light = m_pCB_ShadowMap->m.Light2World.Inverse();
// 
// 	// Find appropriate bounds
// 	NjFloat3		BBoxMin = 1e6f * NjFloat3::One;
// 	NjFloat3		BBoxMax = -1e6f * NjFloat3::One;
// 	Scene::Node*	pMesh = NULL;
// 	while ( (pMesh = m_Scene.ForEach( Scene::Node::MESH, pMesh )) != NULL )
// 	{
// 		NjFloat4x4	Mesh2Light = pMesh->m_Local2World * m_pCB_ShadowMap->m.World2Light;
// 
// 		// Transform the 8 corners of the mesh's BBox into light space and grow the light's bbox
// 		const NjFloat3&	MeshBBoxMin = ((Scene::Mesh&) *pMesh).m_BBoxMin;
// 		const NjFloat3&	MeshBBoxMax = ((Scene::Mesh&) *pMesh).m_BBoxMax;
// 		for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
// 		{
// 			NjFloat3	D;
// 			D.x = float(CornerIndex & 1);
// 			D.y = float((CornerIndex >> 1) & 1);
// 			D.z = float((CornerIndex >> 2) & 1);
// 
// 			NjFloat3	CornerLocal = MeshBBoxMin + D * (MeshBBoxMax - MeshBBoxMin);
// 			NjFloat3	CornerLight = NjFloat4( CornerLocal, 1 ) * Mesh2Light;
// 
// 			BBoxMin = BBoxMin.Min( CornerLight );
// 			BBoxMax = BBoxMax.Max( CornerLight );
// 		}
// 	}
// 
// 	// Recenter & scale transform accordingly
// 	NjFloat3	Center = NjFloat4( 0.5f * (BBoxMin + BBoxMax), 1.0f ) * m_pCB_ShadowMap->m.Light2World;	// Center in world space
// 	NjFloat3	Delta = BBoxMax - BBoxMin;
// 				Center = Center + 0.5f * Delta.z * _SunDirection;	// Center is now stuck to the bounds' Zmin
// 	m_pCB_ShadowMap->m.Light2World.SetRow( 3, Center, 1 );
// 
// 	m_pCB_ShadowMap->m.Light2World.Scale( NjFloat3( 0.5f * Delta.x, 0.5f * Delta.y, Delta.z ) );
// 
// 
// 	// Finalize constant buffer
// 	m_pCB_ShadowMap->m.World2Light = m_pCB_ShadowMap->m.Light2World.Inverse();
// 	m_pCB_ShadowMap->m.BoundsMin = BBoxMin;
// 	m_pCB_ShadowMap->m.BoundsMax = BBoxMax;
// 
// 	m_pCB_ShadowMap->UpdateData();
// 
// 
// 
// //CHECK => All corners should be in [(-1,-1,0),(+1,+1,1)]
// // BBoxMin = 1e6f * NjFloat3::One;
// // BBoxMax = -1e6f * NjFloat3::One;
// // pMesh = NULL;
// // while ( (pMesh = m_Scene.ForEach( Scene::Node::MESH, pMesh )) != NULL )
// // {
// // 	NjFloat4x4	Mesh2Light = pMesh->m_Local2World * m_pCB_ShadowMap->m.World2Light;
// // 
// // 	// Transform the 8 corners of the mesh's BBox into light space and grow the light's bbox
// // 	const NjFloat3&	MeshBBoxMin = ((Scene::Mesh&) *pMesh).m_BBoxMin;
// // 	const NjFloat3&	MeshBBoxMax = ((Scene::Mesh&) *pMesh).m_BBoxMax;
// // 	for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
// // 	{
// // 		NjFloat3	D;
// // 		D.x = float(CornerIndex & 1);
// // 		D.y = float((CornerIndex >> 1) & 1);
// // 		D.z = float((CornerIndex >> 2) & 1);
// // 
// // 		NjFloat3	CornerLocal = MeshBBoxMin + D * (MeshBBoxMax - MeshBBoxMin);
// // 		NjFloat3	CornerLight = NjFloat4( CornerLocal, 1 ) * Mesh2Light;
// // 
// // 		BBoxMin = BBoxMin.Min( CornerLight );
// // 		BBoxMax = BBoxMax.Max( CornerLight );
// // 	}
// // }
// //CHECK
// 
// 
// 	//////////////////////////////////////////////////////////////////////////
// 	// Perform actual rendering
// 	USING_MATERIAL_START( *m_pMatRenderShadowMap )
// 
// 	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );
// 
// 	m_pRTShadowMap->RemoveFromLastAssignedSlots();
// 	m_Device.ClearDepthStencil( *m_pRTShadowMap, 1.0f, 0, true, false );
// 	m_Device.SetRenderTargets( m_pRTShadowMap->GetWidth(), m_pRTShadowMap->GetHeight(), 0, NULL, m_pRTShadowMap->GetDepthStencilView() );
// 
// 	Scene::Node*	pMesh = NULL;
// 	while ( (pMesh = m_Scene.ForEach( Scene::Node::MESH, pMesh )) != NULL )
// 	{
// 		RenderMesh( (Scene::Mesh&) *pMesh, &M );
// 	}
// 
// 	USING_MATERIAL_END
// 
// 	// Assign the shadow map to shaders
// 	m_Device.RemoveRenderTargets();
// 	m_pRTShadowMap->Set( 2, true );
// }



//////////////////////////////////////////////////////////////////////////
// Scene Rendering
//
void*	EffectDOF::TagMaterial( const Scene& _Owner, const Scene::Material& _Material )
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	switch ( _Material.m_ID )
	{
	case 0:
	case 1:
	case 2:

// 		if ( _Material.m_EmissiveColor.Max() > 1e-4f )
// 			return m_pMatRenderEmissive;	// Special emissive materials!

		return m_pMatRender;

	default:
		ASSERT( false, "Unsupported material!" );
	}
	return NULL;
}
void*	EffectDOF::TagTexture( const Scene& _Owner, const Scene::Material::Texture& _Texture )
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	switch ( _Texture.m_ID )
	{
	case 0:		return m_pTexWalls;
	case ~0:	return NULL;	// Invalid textures are not mapped
	default:
		ASSERT( false, "Unsupported texture!" );
	}
	return NULL;
}
void*	EffectDOF::TagNode( const Scene& _Owner, const Scene::Node& _Node )
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

// 	if ( _Node.m_Type == Scene::Node::LIGHT )
// 	{	// Add another static light
// 		Scene::Light&	SourceLight = (Scene::Light&) _Node;
// 		LightStruct&	TargetLight = m_pSB_LightsStatic->m[m_pCB_Scene->m.StaticLightsCount++];
// 
// 		TargetLight.Type = SourceLight.m_LightType;
// 		TargetLight.Position = SourceLight.m_Local2Parent.GetRow( 3 );
// 		TargetLight.Direction = -SourceLight.m_Local2Parent.GetRow( 2 ).Normalize();
// 		TargetLight.Color = SourceLight.m_Intensity * SourceLight.m_Color;
// 		TargetLight.Parms.Set( 10.0f, 11.0f, cosf( SourceLight.m_HotSpot ), cosf( SourceLight.m_Falloff ) );
// 	}

	return NULL;
}
void*	EffectDOF::TagPrimitive( const Scene& _Owner, const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive )
{
	if ( m_bDeleteSceneTags )
	{	// Delete the primitive
		Primitive*	pPrim = (Primitive*) _Primitive.m_pTag;	// We need to cast it as a primitive first so the destructor gets called
		delete pPrim;
		return NULL;
	}

	// Create an actual rendering primitive
	IVertexFormatDescriptor*	pVertexFormat = NULL;
	switch ( _Primitive.m_VertexFormat )
	{
	case Scene::Mesh::Primitive::P3N3G3B3T2:	pVertexFormat = &VertexFormatP3N3G3B3T2::DESCRIPTOR;
	}
	ASSERT( pVertexFormat != NULL, "Unsupported vertex format!" );

	Primitive*	pPrim = new Primitive( m_Device, _Primitive.m_VerticesCount, _Primitive.m_pVertices, 3*_Primitive.m_FacesCount, _Primitive.m_pFaces, D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, *pVertexFormat );

// 	// Tag the primitive with the face offset
// 	pPrim->m_pTag = (void*) m_TotalFacesCount;
// 	m_pPrimitiveFaceOffset[m_TotalPrimitivesCount++] = m_TotalFacesCount;	// Store face offset for each primitive
// 	m_TotalFacesCount += pPrim->GetFacesCount();							// Increase total amount of faces

	return pPrim;
}

void	EffectDOF::RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride )
{
	// Upload the object's CB
	memcpy( &m_pCB_Object->m.Local2World, &_Mesh.m_Local2World, sizeof(float4x4) );
	m_pCB_Object->UpdateData();

	for ( int PrimitiveIndex=0; PrimitiveIndex < _Mesh.m_PrimitivesCount; PrimitiveIndex++ )
	{
		Scene::Mesh::Primitive&	ScenePrimitive = _Mesh.m_pPrimitives[PrimitiveIndex];
		Scene::Material&		SceneMaterial = *ScenePrimitive.m_pMaterial;

		Material*	pMat = _pMaterialOverride == NULL ? (Material*) SceneMaterial.m_pTag : _pMaterialOverride;
		if ( pMat == NULL )
			continue;	// Unsupported material!
		Primitive*	pPrim = (Primitive*) ScenePrimitive.m_pTag;
		if ( pPrim == NULL )
			continue;	// Unsupported primitive!

		// Upload textures
		Texture2D*	pTexDiffuseAlbedo = (Texture2D*) SceneMaterial.m_TexDiffuseAlbedo.m_pTag;
		if ( pTexDiffuseAlbedo != NULL )
			pTexDiffuseAlbedo->SetPS( 10 );
		Texture2D*	pTexSpecularAlbedo = (Texture2D*) SceneMaterial.m_TexSpecularAlbedo.m_pTag;
		if ( pTexSpecularAlbedo != NULL )
			pTexSpecularAlbedo->SetPS( 11 );

		// Upload the primitive's material CB
		m_pCB_Material->m.ID = SceneMaterial.m_ID;
		m_pCB_Material->m.DiffuseAlbedo = SceneMaterial.m_DiffuseAlbedo;
		m_pCB_Material->m.HasDiffuseTexture = pTexDiffuseAlbedo != NULL;
		m_pCB_Material->m.SpecularAlbedo = SceneMaterial.m_SpecularAlbedo;
		m_pCB_Material->m.HasSpecularTexture = pTexSpecularAlbedo != NULL;
		m_pCB_Material->m.EmissiveColor = SceneMaterial.m_EmissiveColor;
		m_pCB_Material->m.SpecularExponent = SceneMaterial.m_SpecularExponent.x;
		m_pCB_Material->m.FaceOffset = U32(pPrim->m_pTag);
		m_pCB_Material->UpdateData();

		int		Test = sizeof(CBMaterial);

		// Render
		pMat->Use();
		pPrim->Render( *pMat );
	}
}
#endif