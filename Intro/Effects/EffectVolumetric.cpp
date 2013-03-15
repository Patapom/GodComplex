#include "../../GodComplex.h"
#include "EffectVolumetric.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

const float	EffectVolumetric::SCREEN_TARGET_RATIO = 1.0f;

EffectVolumetric::EffectVolumetric( Device& _Device, Primitive& _ScreenQuad ) : m_Device( _Device ), m_ScreenQuad( _ScreenQuad ), m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatDepthWrite = CreateMaterial( IDR_SHADER_VOLUMETRIC_DEPTH_WRITE, VertexFormatP3::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMPUTE_TRANSMITTANCE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 2 );
// 	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_VOLUMETRIC_DISPLAY, VertexFormatP3::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
 	CHECK_MATERIAL( m_pMatCombine = CreateMaterial( IDR_SHADER_VOLUMETRIC_COMBINE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 4 );
	

	//////////////////////////////////////////////////////////////////////////
	// Build the box primitive
	{
		m_pPrimBox = new Primitive( m_Device, VertexFormatP3::DESCRIPTOR );
		GeometryBuilder::BuildCube( 1, 1, 1, *m_pPrimBox );
	}

	//////////////////////////////////////////////////////////////////////////
	// Build textures & render targets
	m_pRTTransmittanceZ = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, PixelFormatRG16F::DESCRIPTOR, 1, NULL );
	m_pRTTransmittanceMap = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 2, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );

	int	W = m_Device.DefaultRenderTarget().GetWidth();
	int	H = m_Device.DefaultRenderTarget().GetHeight();
	m_RenderWidth = int( ceilf( W * SCREEN_TARGET_RATIO ) );
	m_RenderHeight = int( ceilf( H * SCREEN_TARGET_RATIO ) );

	m_pRTRenderZ = new Texture2D( m_Device, m_RenderWidth, m_RenderHeight, 1, PixelFormatRG16F::DESCRIPTOR, 1, NULL );
	m_pRTRender = new Texture2D( m_Device, m_RenderWidth, m_RenderHeight, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Object = new CB<CBObject>( m_Device, 10 );
	m_pCB_Splat = new CB<CBSplat>( m_Device, 10 );
	m_pCB_Shadow = new CB<CBShadow>( m_Device, 11 );


	//////////////////////////////////////////////////////////////////////////
	// Setup our volume & light
	m_Position = NjFloat3( 0.0f, 2.0f, 0.0f );
	m_Rotation = NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY );
	m_Scale = NjFloat3( 1.0f, 2.0f, 1.0f );

	m_LightDirection = NjFloat3( 1, 1, 1 );
	m_LightDirection.Normalize();
}

EffectVolumetric::~EffectVolumetric()
{
	delete m_pCB_Shadow;
	delete m_pCB_Splat;
	delete m_pCB_Object;

	delete m_pRTRender;
	delete m_pRTRenderZ;
	delete m_pRTTransmittanceMap;
	delete m_pRTTransmittanceZ;

	delete m_pPrimBox;

 	delete m_pMatCombine;
 	delete m_pMatDisplay;
 	delete m_pMatComputeTransmittance;
	delete m_pMatDepthWrite;
}

void	EffectVolumetric::Render( float _Time, float _DeltaTime )
{
// DEBUG
m_LightDirection.Set( cosf(_Time), 1, sinf( _Time ) );
// DEBUG


	//////////////////////////////////////////////////////////////////////////
	// 1] Compute transforms
	m_Box2World.PRS( m_Position, m_Rotation, m_Scale );

	NjFloat4x4	Local2Proj;
	ComputeShadowTransform( Local2Proj );

	m_pCB_Shadow->UpdateData();


	//////////////////////////////////////////////////////////////////////////
	// 2] Compute the transmittance function map

	// 2.1] Render front & back depths
	m_Device.ClearRenderTarget( *m_pRTTransmittanceZ, NjFloat4( 1e4f, -1e4f, 0.0f, 0.0f ) );

	USING_MATERIAL_START( *m_pMatDepthWrite )

		m_Device.SetRenderTarget( *m_pRTTransmittanceZ );

		m_pCB_Object->m.Local2Proj = m_Box2World * m_pCB_Shadow->m.World2Shadow;
		m_pCB_Object->m.dUV = m_pRTTransmittanceZ->GetdUV();
		m_pCB_Object->UpdateData();

	 	m_Device.SetStates( m_Device.m_pRS_CullFront, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_RedOnly );
		m_pPrimBox->Render( M );

	 	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled_GreenOnly );
		m_pPrimBox->Render( M );

	USING_MATERIAL_END

	// 2.2] Compute transmittance map
	m_Device.ClearRenderTarget( *m_pRTTransmittanceMap, NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );

	ID3D11RenderTargetView*	ppViews[2] = {
		m_pRTTransmittanceMap->GetTargetView( 0, 0, 1 ),
		m_pRTTransmittanceMap->GetTargetView( 0, 1, 1 ),
	};
	m_Device.SetRenderTargets( m_pRTTransmittanceMap->GetWidth(), m_pRTTransmittanceMap->GetHeight(), 2, ppViews );

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	USING_MATERIAL_START( *m_pMatComputeTransmittance )

		m_pCB_Object->m.Local2Proj;
		m_pCB_Object->m.dUV = m_pRTTransmittanceMap->GetdUV();
		m_pCB_Object->UpdateData();

		m_pRTTransmittanceZ->SetPS( 10 );

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END


// 	//////////////////////////////////////////////////////////////////////////
// 	// 3] Render the actual volume
// 	m_Device.ClearRenderTarget( *m_pRTRender, NjFloat4( 0.0f, 0.0f, 0.0f, 1.0f ) );
// 
// 	USING_MATERIAL_START( *m_pMatDisplay )
// 
// 		m_pCB_Object->m.dUV = m_pRTRender->GetdUV();
// 		m_pCB_Object->UpdateData();
// 
// 		m_pPrimBox->Render( M );
// 
// 	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 4] Combine with screen
	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget(), NULL );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );

	USING_MATERIAL_START( *m_pMatCombine )

		m_pCB_Splat->m.dUV = m_Device.DefaultRenderTarget().GetdUV();
		m_pCB_Splat->UpdateData();

// DEBUG
m_pRTTransmittanceZ->SetPS( 10 );
m_pRTTransmittanceMap->SetPS( 11 );

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END
}

void	EffectVolumetric::ComputeShadowTransform( NjFloat4x4& _Local2Proj )
{
	// Build basis for directional light
	m_LightDirection.Normalize();
	NjFloat3	X, Y;
	if ( fabs( m_LightDirection.y - 1.0f ) < 1e-3f )
	{	// Special case
		X = NjFloat3::UnitX;
		Y = NjFloat3::UnitZ;
	}
	else
	{
		X = m_LightDirection ^ NjFloat3::UnitY;
		X.Normalize();
		Y = X ^ m_LightDirection;
	}

	// Project the box's corners to the light plane
	NjFloat3	Center = NjFloat3::Zero;
	NjFloat3	CornerLocal, CornerWorld, CornerLight;
	NjFloat3	Min( FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX ), Max( -FLOAT32_MAX, -FLOAT32_MAX, -FLOAT32_MAX );
	for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
	{
		CornerLocal.x = 2.0f * (CornerIndex & 1) - 1.0f;
		CornerLocal.y = 2.0f * ((CornerIndex >> 1) & 1) - 1.0f;
		CornerLocal.z = 2.0f * ((CornerIndex >> 2) & 1) - 1.0f;

		CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Box2World;

		Center = Center + CornerWorld;

		CornerLight.x = CornerWorld | X;
		CornerLight.y = CornerWorld | Y;
		CornerLight.z = CornerWorld | m_LightDirection;

		Min = Min.Min( CornerLight );
		Max = Max.Max( CornerLight );
	}
	Center = Center / 8.0f;

	// Build LIGHT => WORLD transform & inverse
	NjFloat4x4	Light2World;
	Light2World.SetRow( 0, X, 0 );
	Light2World.SetRow( 1, Y, 0 );
	Light2World.SetRow( 2, -m_LightDirection, 0 );
	Light2World.SetRow( 3, Center, 1 );

	NjFloat4x4	World2Light = Light2World.Inverse();

	// Build projection transforms
	NjFloat3	CenterLight = 0.5f * (Min + Max);
	NjFloat3	SizeLight = Max - Min;

	NjFloat4x4	Shadow2Light;
	Shadow2Light.SetRow( 0, NjFloat4( 0.5f * SizeLight.x, 0, 0, 0 ) );
	Shadow2Light.SetRow( 1, NjFloat4( 0, 0.5f * SizeLight.y, 0, 0 ) );
//	Shadow2Light.SetRow( 2, NjFloat4( 0, 0, SizeLight.z, 0 ) );
//	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, -0.5f * SizeLight.z, 1 ) );
	Shadow2Light.SetRow( 2, NjFloat4( 0, 0, 1, 0 ) );
	Shadow2Light.SetRow( 3, NjFloat4( 0, 0, -0.5f * SizeLight.z, 1 ) );

	NjFloat4x4	Light2Shadow = Shadow2Light.Inverse();

	m_pCB_Shadow->m.World2Shadow = World2Light * Light2Shadow;
	m_pCB_Shadow->m.Shadow2World = Shadow2Light * Light2World;
	m_pCB_Shadow->m.ZMax.Set( SizeLight.z, 1.0f / SizeLight.z );

// CHECK
// NjFloat3	CheckMin( FLOAT32_MAX, FLOAT32_MAX, FLOAT32_MAX ), CheckMax( -FLOAT32_MAX, -FLOAT32_MAX, -FLOAT32_MAX );
// NjFloat3	CornerShadow;
// for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
// {
// 	CornerLocal.x = 2.0f * (CornerIndex & 1) - 1.0f;
// 	CornerLocal.y = 2.0f * ((CornerIndex >> 1) & 1) - 1.0f;
// 	CornerLocal.z = 2.0f * ((CornerIndex >> 2) & 1) - 1.0f;
// 
// 	CornerWorld = NjFloat4( CornerLocal, 1 ) * m_Box2World;
// 
// 	CornerShadow = NjFloat4( CornerWorld, 1 ) * m_pCB_Shadow->m.World2Shadow;
// 
// 	CheckMin = CheckMin.Min( CornerShadow );
// 	CheckMax = CheckMax.Max( CornerShadow );
// }
// CHECK

// CHECK
NjFloat3	Test0 = NjFloat4( 0.0f, 0.0f, 0.5f * SizeLight.z, 1.0f ) * m_pCB_Shadow->m.Shadow2World;
NjFloat3	Test1 = NjFloat4( 0.0f, 0.0f, 0.0f, 1.0f ) * m_pCB_Shadow->m.Shadow2World;
NjFloat3	DeltaTest0 = Test1 - Test0;
NjFloat3	Test2 = NjFloat4( 0.0f, 0.0f, SizeLight.z, 1.0f ) * m_pCB_Shadow->m.Shadow2World;
NjFloat3	DeltaTest1 = Test2 - Test0;
// CHECK
}
