#include "../../GodComplex.h"
#include "EffectTranslucency.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

void	TweakSphere( NjFloat3& _Position, NjFloat3& _Normal, NjFloat3& _Tangent, NjFloat2& _UV, void* _pUserData )
{
	Noise&	N = *((Noise*) _pUserData);

	_Position = 0.4f * (_Position + _Normal * 0.2f * N.Perlin( 0.01f * _Position ));	// Scale down and add perlin
}

EffectTranslucency::EffectTranslucency() : m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_TEST_DISPLAY, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatBuildZBuffer = CreateMaterial( IDR_SHADER_TRANSLUCENCY_BUILD_ZBUFFER, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 2 );

	//////////////////////////////////////////////////////////////////////////
	// Build some sphere primitives
	{
		GeometryBuilder::MapperSpherical	Mapper( 4.0f, 2.0f );

		Noise	N( 1 );

		m_pPrimSphereInternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildSphere( 20, 10, *m_pPrimSphereInternal, Mapper, TweakSphere, &N );

		m_pPrimSphereExternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildSphere( 20, 10, *m_pPrimSphereExternal, Mapper );
	}

	//////////////////////////////////////////////////////////////////////////
	// Build textures & render targets
	m_pRTZBuffer = new Texture2D( gs_Device, ZBUFFER_SIZE, ZBUFFER_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pDepthStencil = new Texture2D( gs_Device, ZBUFFER_SIZE, ZBUFFER_SIZE, DepthStencilFormatD32F::DESCRIPTOR );

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Object = new CB<CBObject>( gs_Device );
}

EffectTranslucency::~EffectTranslucency()
{
	delete m_pCB_Object;

	delete m_pDepthStencil;
	delete m_pRTZBuffer;

	delete m_pPrimSphereInternal;
	delete m_pPrimSphereExternal;

 	delete m_pMatBuildZBuffer;
 	delete m_pMatDisplay;
}

void	EffectTranslucency::Render( float _Time, float _DeltaTime )
{
	//////////////////////////////////////////////////////////////////////////
	// 1] Render the internal & external objects into the RGBA ZBuffer
	gs_Device.ClearRenderTarget( *m_pRTZBuffer, 1000.0f * NjFloat4::One );	// Clear to ZFar
	gs_Device.SetRenderTarget( *m_pRTZBuffer, m_pDepthStencil );

	{	USING_MATERIAL_START( *m_pMatBuildZBuffer )

		// === Render external object ===
		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ), NjFloat3::One );
		m_pCB_Object->UpdateData();
		m_pCB_Object->Set( 1 );

			// Front
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 1.0f, 0 );
 		gs_Device.SetStates( *gs_Device.m_pRS_CullBack, *gs_Device.m_pDS_ReadWriteLess, *gs_Device.m_pBS_Disabled_RedOnly );
		m_pPrimSphereExternal->Render( *m_pMatDisplay );

			// Back
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 1.0f, 0 );
 		gs_Device.SetStates( *gs_Device.m_pRS_CullFront, *gs_Device.m_pDS_ReadWriteLess, *gs_Device.m_pBS_Disabled_GreenOnly );
		m_pPrimSphereExternal->Render( *m_pMatDisplay );

		// === Render rotating internal object ===
		static float	ObjectAngle = 0.0f;
		ObjectAngle += _TV(0.25f) * _DeltaTime;

		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( ObjectAngle, NjFloat3::UnitY ), NjFloat3::One );
		m_pCB_Object->UpdateData();
		m_pCB_Object->Set( 1 );

			// Front
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 1.0f, 0 );
 		gs_Device.SetStates( *gs_Device.m_pRS_CullBack, *gs_Device.m_pDS_ReadWriteLess, *gs_Device.m_pBS_Disabled_BlueOnly );
		m_pPrimSphereInternal->Render( *m_pMatDisplay );

			// Back
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 1.0f, 0 );
 		gs_Device.SetStates( *gs_Device.m_pRS_CullFront, *gs_Device.m_pDS_ReadWriteLess, *gs_Device.m_pBS_Disabled_AlphaOnly );
		m_pPrimSphereInternal->Render( *m_pMatDisplay );

		USING_MATERIAL_END
	}

// 	{	USING_MATERIAL_START( *m_pMatDisplay )
// 
// 		static float	ObjectAngle = 0.0f;
// 		ObjectAngle += _TV(0.5f) * _DeltaTime;
// 
// 		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( ObjectAngle, NjFloat3::UnitY ), NjFloat3::One );
// 		m_pCB_Object->UpdateData();
// 		m_pCB_Object->Set( 1 );
// 
// 		m_pPrimSphereInternal->Render( *m_pMatDisplay );
// 
// 		USING_MATERIAL_END
// 	}
}