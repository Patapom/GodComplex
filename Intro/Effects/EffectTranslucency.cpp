#include "../../GodComplex.h"
#include "EffectTranslucency.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

void	TweakSphere( NjFloat3& _Position, NjFloat3& _Normal, NjFloat3& _Tangent, NjFloat2& _UV, void* _pUserData )
{
	Noise&	N = *((Noise*) _pUserData);

	_Position = _Position + _Normal * 0.2f * N.Perlin( 0.01f * _Position );	// Add perlin
}

EffectTranslucency::EffectTranslucency() : m_ErrorCode( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_TEST_DISPLAY, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );


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
	// Create the constant buffers
	m_pCB_Object = new CB<CBObject>( gs_Device );
}

EffectTranslucency::~EffectTranslucency()
{
	delete m_pCB_Object;

	delete m_pPrimSphereInternal;
	delete m_pPrimSphereExternal;

 	delete m_pMatDisplay;
}

void	EffectTranslucency::Render( float _Time, float _DeltaTime )
{
	gs_Device.SetStates( *gs_Device.m_pRS_CullBack, *gs_Device.m_pDS_ReadWriteLess, *gs_Device.m_pBS_Disabled );

	{	USING_MATERIAL_START( *m_pMatDisplay )

		static float	ObjectAngle = 0.0f;
		ObjectAngle += _TV(0.5f) * _DeltaTime;

		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( ObjectAngle, NjFloat3::UnitY ), NjFloat3::One );
		m_pCB_Object->UpdateData();
		m_pCB_Object->Set( 1 );

		m_pPrimSphereInternal->Render( *m_pMatDisplay );

		USING_MATERIAL_END
	}
}