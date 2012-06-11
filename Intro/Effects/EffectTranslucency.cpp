#include "../../GodComplex.h"
#include "EffectTranslucency.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

void	TweakSphereExternal( NjFloat3& _Position, NjFloat3& _Normal, NjFloat3& _Tangent, NjFloat2& _UV, void* _pUserData )
{
	Noise&	N = *((Noise*) _pUserData);

	_Position = _Position + _Normal * 0.2f * (-1.0f + N.Perlin( 0.0005f * _Position ));	// Add perlin inward
}

void	TweakTorusInternal( NjFloat3& _Position, NjFloat3& _Normal, NjFloat3& _Tangent, NjFloat2& _UV, void* _pUserData )
{
	Noise&	N = *((Noise*) _pUserData);

//	_Position = 0.4f * (_Position + _Normal * 0.5f * N.Perlin( 0.01f * _Position ));	// Scale down and add perlin
	_Position = 0.3f * (_Position + 0.1f * N.PerlinVector( 0.001f * _Position ));
}

EffectTranslucency::EffectTranslucency( Primitive& _Quad, Texture2D& _RTTarget ) : m_ErrorCode( 0 ), m_Quad( _Quad ), m_RTTarget( _RTTarget )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	CHECK_MATERIAL( m_pMatDisplay = CreateMaterial( IDR_SHADER_TRANSLUCENCY_DISPLAY, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	CHECK_MATERIAL( m_pMatBuildZBuffer = CreateMaterial( IDR_SHADER_TRANSLUCENCY_BUILD_ZBUFFER, VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 2 );

	D3D_SHADER_MACRO	pMacros[2] = { { "TARGET_SIZE", "128" }, { NULL, NULL } };
	CHECK_MATERIAL( m_pMatDiffusion = CreateMaterial( IDR_SHADER_TRANSLUCENCY_DIFFUSION, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS", pMacros ), 3 );

	//////////////////////////////////////////////////////////////////////////
	// Build some sphere primitives
	{
		GeometryBuilder::MapperSpherical	Mapper( 4.0f, 2.0f );

		Noise	N( 1 );

		m_pPrimTorusInternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildTorus( 80, 50, 1.0f, 0.2f, *m_pPrimTorusInternal, Mapper, TweakTorusInternal, &N );

		m_pPrimSphereExternal = new Primitive( gs_Device, VertexFormatP3N3G3T2::DESCRIPTOR );
		GeometryBuilder::BuildSphere( 80, 50, *m_pPrimSphereExternal, Mapper, TweakSphereExternal, &N );
	}

	//////////////////////////////////////////////////////////////////////////
	// Build textures & render targets
	m_pRTZBuffer = new Texture2D( gs_Device, DIFFUSION_SIZE, DIFFUSION_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_pDepthStencil = new Texture2D( gs_Device, DIFFUSION_SIZE, DIFFUSION_SIZE, DepthStencilFormatD32F::DESCRIPTOR );

	m_ppRTDiffusion[0] = new Texture2D( gs_Device, DIFFUSION_SIZE, DIFFUSION_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );
	m_ppRTDiffusion[1] = new Texture2D( gs_Device, DIFFUSION_SIZE, DIFFUSION_SIZE, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Object = new CB<CBObject>( gs_Device, 1 );
	m_pCB_Diffusion = new CB<CBDiffusion>( gs_Device, 1 );
	m_pCB_Pass = new CB<CBPass>( gs_Device, 2 );
}

EffectTranslucency::~EffectTranslucency()
{
	delete m_pCB_Pass;
	delete m_pCB_Diffusion;
	delete m_pCB_Object;

	delete m_ppRTDiffusion[1];
	delete m_ppRTDiffusion[0];
	delete m_pDepthStencil;
	delete m_pRTZBuffer;

	delete m_pPrimTorusInternal;
	delete m_pPrimSphereExternal;

	delete m_pMatDiffusion;
 	delete m_pMatBuildZBuffer;
 	delete m_pMatDisplay;
}

void	EffectTranslucency::Render( float _Time, float _DeltaTime )
{
	NjFloat3	TorusPosition = NjFloat3(
		0.8f * sinf( _TV(0.95f) * _Time ) * sinf( _TV(0.7f) * _Time ),
		0.8f * sinf( _TV(0.83f) * _Time ) * sinf( _TV(-0.19f) * _Time ),
		0.2f * cosf( _TV(0.5f) * _Time ) * sinf( _TV(-0.17f) * _Time ) );							// Oscillate within the quiche

	NjFloat4	TorusRotation = NjFloat4::QuatFromAngleAxis( _TV(1.35f) * _Time, NjFloat3::UnitY );	// Rotate, too

	m_EmissivePower = SATURATE( -4.0f * sinf( _TV(0.5f) * _Time ) );

	//////////////////////////////////////////////////////////////////////////
	// 1] Render the internal & external objects into the RGBA ZBuffer
	gs_Device.ClearRenderTarget( *m_pRTZBuffer, NjFloat4( 2.0f, 0.0f, 2.0f, 0.0f ) );
	gs_Device.SetRenderTarget( *m_pRTZBuffer, m_pDepthStencil );

	{	USING_MATERIAL_START( *m_pMatBuildZBuffer )

		// === Render external object ===
		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( 0.0f, NjFloat3::UnitY ), NjFloat3::One );
		m_pCB_Object->UpdateData();

			// Front
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 1.0f, 0 );
 		gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled_RedOnly );
		m_pPrimSphereExternal->Render( *m_pMatBuildZBuffer );

			// Back
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 0.0f, 0 );
 		gs_Device.SetStates( gs_Device.m_pRS_CullFront, gs_Device.m_pDS_ReadWriteGreater, gs_Device.m_pBS_Disabled_GreenOnly );
		m_pPrimSphereExternal->Render( *m_pMatBuildZBuffer );

		// === Render rotating internal object ===
		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( TorusPosition, TorusRotation, NjFloat3::One );
		m_pCB_Object->UpdateData();

			// Front
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 1.0f, 0 );
 		gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, gs_Device.m_pBS_Disabled_BlueOnly );
		m_pPrimTorusInternal->Render( *m_pMatBuildZBuffer );

			// Back
		gs_Device.ClearDepthStencil( *m_pDepthStencil, 0.0f, 0 );
 		gs_Device.SetStates( gs_Device.m_pRS_CullFront, gs_Device.m_pDS_ReadWriteGreater, gs_Device.m_pBS_Disabled_AlphaOnly );
		m_pPrimTorusInternal->Render( *m_pMatBuildZBuffer );

		USING_MATERIAL_END
	}

	//////////////////////////////////////////////////////////////////////////
	// 2] Render the irradiance map ?

	// LATER... Not sure it's necessary

	//////////////////////////////////////////////////////////////////////////
	// 3] Perform diffusion
	gs_Device.SetStates( gs_Device.m_pRS_CullNone, gs_Device.m_pDS_Disabled, gs_Device.m_pBS_Disabled );

	{	USING_MATERIAL_START( *m_pMatDiffusion )

		// Clear original irradiance map
		gs_Device.ClearRenderTarget( *m_ppRTDiffusion[0], NjFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) );

		// Setup our global diffusion parameters
		float	BBoxSize = _TV(0.002f);	// Size of the BBox containing our objects, in meter

		m_pCB_Diffusion->m.BBoxSize = BBoxSize;
		m_pCB_Diffusion->m.SliceThickness = BBoxSize / DIFFUSION_PASSES_COUNT;	// Size of a single slice
		m_pCB_Diffusion->m.TexelSize = BBoxSize / DIFFUSION_SIZE;				// Size of a single texel
		m_pCB_Diffusion->m.ExtinctionCoeff = _TV(1000.0f) * NjFloat3( 0.8f, 0.85f, 1.0f );
		m_pCB_Diffusion->m.Albedo = _TV(0.8f) * NjFloat3::One;

		NjFloat3	ScatteringAnisotropy = _TV(0.0f) * NjFloat3::One;
		float		PhaseFactor = _TV(4.6f);
		m_pCB_Diffusion->m.Phase0 = PhaseFactor * ComputePhase( ScatteringAnisotropy, 0, 1, m_pCB_Diffusion->m.TexelSize, m_pCB_Diffusion->m.SliceThickness );
		m_pCB_Diffusion->m.Phase1 = PhaseFactor * ComputePhase( ScatteringAnisotropy, 1, 8, m_pCB_Diffusion->m.TexelSize, m_pCB_Diffusion->m.SliceThickness );
		m_pCB_Diffusion->m.Phase2 = PhaseFactor * ComputePhase( ScatteringAnisotropy, 2, 12, m_pCB_Diffusion->m.TexelSize, m_pCB_Diffusion->m.SliceThickness );

//		m_pCB_Diffusion->m.ExternalLight = _TV(2.0f) * NjFloat3( 1.0f, 1.0f, 1.0f );
		m_pCB_Diffusion->m.ExternalLight = _TV(1.4f) * (1.4f - m_EmissivePower) * NjFloat3( 1.0f, 1.0f, 1.0f );
		m_pCB_Diffusion->m.InternalEmissive = _TV(10.0f) * m_EmissivePower * NjFloat3( 1.0f, 0.8f, 0.2f );

		m_pCB_Diffusion->UpdateData();

		m_pRTZBuffer->SetPS( 0 );

		// Diffuse light through multiple passes
		for ( int PassIndex=0; PassIndex <= DIFFUSION_PASSES_COUNT; PassIndex++ )
		{
			gs_Device.SetRenderTarget( *m_ppRTDiffusion[1] );

			m_pCB_Pass->m.PassIndex = float(PassIndex);
			m_pCB_Pass->m.CurrentZ = 2.0f * PassIndex / DIFFUSION_PASSES_COUNT;
			m_pCB_Pass->m.NextZ = 2.0f * (PassIndex+1) / DIFFUSION_PASSES_COUNT;
			m_pCB_Pass->UpdateData();

			m_ppRTDiffusion[0]->SetPS( 1 );
			m_Quad.Render( *m_pMatDiffusion );

			// Swap irradiance maps
			Texture2D*	pTemp = m_ppRTDiffusion[0];
			m_ppRTDiffusion[0] = m_ppRTDiffusion[1];
			m_ppRTDiffusion[1] = pTemp;
		}

		USING_MATERIAL_END
	}

	//////////////////////////////////////////////////////////////////////////
	// 4] Finally, render the object with a planar mapping of the irradiance map
	{	USING_MATERIAL_START( *m_pMatDisplay )

		gs_Device.SetRenderTarget( m_RTTarget, &gs_Device.DefaultDepthStencil() );
		gs_Device.SetStates( gs_Device.m_pRS_CullBack, gs_Device.m_pDS_ReadWriteLess, NULL );

		m_ppRTDiffusion[0]->SetPS( 0 );

		// Render the sphere
		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( NjFloat3::Zero, NjFloat4::QuatFromAngleAxis( _TV(0.0f) * _Time, NjFloat3::UnitY ), NjFloat3::One );
		m_pCB_Object->m.EmissiveColor = NjFloat4::Zero;
		m_pCB_Object->UpdateData();

		m_pPrimSphereExternal->Render( *m_pMatDisplay );

		// Render the torus
		m_pCB_Object->m.Local2World = NjFloat4x4::PRS( TorusPosition, TorusRotation, NjFloat3::One );
		m_pCB_Object->m.EmissiveColor = NjFloat4( 0.0f * NjFloat3::One + m_pCB_Diffusion->m.InternalEmissive, 1.0f );
		m_pCB_Object->UpdateData();

		m_pPrimTorusInternal->Render( *m_pMatDisplay );

		USING_MATERIAL_END
	}
}

NjFloat3	EffectTranslucency::ComputePhase( const NjFloat3& _Anisotropy, int _PixelDistance, int _SamplesCount, float _TexelSize, float _SliceThickness )
{
	// Imagine receiving light from a point above you offset by a distance d
	//
	//                    0      Source
	// -------------------*--------*-------------
	//                    |      /
	//                    |    /
	//                    |  /
	//                    |/
	// -------------------*----------------------
	//                 Receiver
	//
	// The source and receiver make an angle with the vertical, diffusion direction.
	// This is that angle we compute the phase for.
	//
	float		Theta = atanf( _PixelDistance * _TexelSize / _SliceThickness );
	float		CosTheta = cosf( Theta );

	NjFloat3	P;
	P.x = (1.0f - _Anisotropy.x*_Anisotropy.x) * powf( 1.0f + _Anisotropy.x*_Anisotropy.x - 2.0f*_Anisotropy.x*CosTheta, -1.5f );
	P.y = (1.0f - _Anisotropy.y*_Anisotropy.y) * powf( 1.0f + _Anisotropy.y*_Anisotropy.y - 2.0f*_Anisotropy.y*CosTheta, -1.5f );
	P.z = (1.0f - _Anisotropy.z*_Anisotropy.z) * powf( 1.0f + _Anisotropy.z*_Anisotropy.z - 2.0f*_Anisotropy.z*CosTheta, -1.5f );
	P = INV4PI * P;	// Normalize over hemisphere

	// Now, compute the weight to give this phase function
	// The weight is simply the solid angle covered by all the texels using the phase function
	// This solid angle is a spherical band between current and next pixel angles, divided by the amount of samples taken in that band
	//
	float		NextTheta = atanf( (1+_PixelDistance) * _TexelSize / _SliceThickness );
	float		CosNextTheta = cosf( NextTheta );

	float		SolidAngle = TWOPI * (CosTheta - CosNextTheta);	// Solid angle covered by the spherical band in [Theta,NextTheta]
	SolidAngle /= _SamplesCount;								// Solid angle of a single sample in that band

	return SolidAngle * P;
}