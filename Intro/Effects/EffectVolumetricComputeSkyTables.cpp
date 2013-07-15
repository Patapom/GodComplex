//////////////////////////////////////////////////////////////////////////
// Computes the non-updatable sky table using standard pixel shaders
// Check the new Compute Shader version that can perform time-sliced updates now!
// 

//#define BUILD_SKY_SCATTERING	// Build or load? (warning: the computation shader takes hell of a time to compile!) (but the computation itself takes less than a second! ^^)

#define FILENAME_IRRADIANCE		"./TexIrradiance_64x16.pom"
#define FILENAME_TRANSMITTANCE	"./TexTransmittance_256x64.pom"
#define FILENAME_SCATTERING		"./TexScattering_256x128x32.pom"


struct	CBPreCompute
{
	NjFloat4	dUVW;
	bool		bFirstPass;
	float		AverageGroundReflectance;
	NjFloat2	__PAD1;
};

void	EffectVolumetric::InitSkyTables()
{
#ifdef BUILD_SKY_SCATTERING

	Texture2D*	pRTDeltaIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );			// deltaE (temp)
	Texture3D*	pRTDeltaScatteringRayleigh = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );		// deltaSR (temp)
	Texture3D*	pRTDeltaScatteringMie = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );			// deltaSM (temp)
	Texture3D*	pRTDeltaScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL );				// deltaJ (temp)

	Material*	pMatComputeTransmittance;
	Material*	pMatComputeIrradiance_Single;
	Material*	pMatComputeInScattering_Single;
	Material*	pMatComputeInScattering_Delta;
	Material*	pMatComputeIrradiance_Delta;
	Material*	pMatComputeInScattering_Multiple;
	Material*	pMatMergeInitialScattering;
	Material*	pMatAccumulateIrradiance;
	Material*	pMatAccumulateInScattering;

	CHECK_MATERIAL( pMatComputeInScattering_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Delta" ), 14 );			// inscatterS

	CHECK_MATERIAL( pMatComputeTransmittance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeTransmittance" ), 10 );
	CHECK_MATERIAL( pMatComputeIrradiance_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeIrradiance_Single" ), 11 );				// irradiance1
	CHECK_MATERIAL( pMatComputeIrradiance_Delta = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PreComputeIrradiance_Delta" ), 12 );				// irradianceN
	CHECK_MATERIAL( pMatComputeInScattering_Single = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Single" ), 13 );			// inscatter1
	CHECK_MATERIAL( pMatComputeInScattering_Multiple = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "PreComputeInScattering_Multiple" ), 15 );		// inscatterN
	CHECK_MATERIAL( pMatMergeInitialScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "MergeInitialScattering" ), 16 );					// copyInscatter1
	CHECK_MATERIAL( pMatAccumulateIrradiance = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", NULL, "AccumulateIrradiance" ), 17 );						// copyIrradiance
	CHECK_MATERIAL( pMatAccumulateInScattering = CreateMaterial( IDR_SHADER_VOLUMETRIC_PRECOMPUTE_ATMOSPHERE, VertexFormatPt4::DESCRIPTOR, "VS", "GS", "AccumulateInScattering" ), 18 );					// copyInscatterN


	CB<CBPreCompute>	CB( m_Device, 10 );
	CB.m.bFirstPass = true;
	CB.m.AverageGroundReflectance = 0.1f;

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );


	//////////////////////////////////////////////////////////////////////////
	// Computes transmittance texture T (line 1 in algorithm 4.1)
	USING_MATERIAL_START( *pMatComputeTransmittance )

		m_Device.SetRenderTarget( *m_pRTTransmittance );

		CB.m.dUVW = NjFloat4( m_pRTTransmittance->GetdUV(), 0.0f );
		CB.UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	// Assign to slot 7
	m_Device.RemoveRenderTargets();
	m_ppRTTransmittance[0]->SetVS( 7, true );
	m_ppRTTransmittance[0]->SetPS( 7, true );

	//////////////////////////////////////////////////////////////////////////
	// Computes irradiance texture deltaE (line 2 in algorithm 4.1)
	USING_MATERIAL_START( *pMatComputeIrradiance_Single )

		m_Device.SetRenderTarget( *pRTDeltaIrradiance );

		CB.m.dUVW = NjFloat4( pRTDeltaIrradiance->GetdUV(), 0.0f );
		CB.UpdateData();

		m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	// Assign to slot 13
	m_Device.RemoveRenderTargets();
	pRTDeltaIrradiance->SetPS( 13 );

	// ==================================================
 	// Clear irradiance texture E (line 4 in algorithm 4.1)
	m_Device.ClearRenderTarget( *m_ppRTIrradiance[0], NjFloat4::Zero );

	//////////////////////////////////////////////////////////////////////////
	// Computes single scattering texture deltaS (line 3 in algorithm 4.1)
	// Rayleigh and Mie separated in deltaSR + deltaSM
	USING_MATERIAL_START( *pMatComputeInScattering_Single )

		ID3D11RenderTargetView*	ppTargets[] = { pRTDeltaScatteringRayleigh->GetTargetView( 0, 0, 0 ), pRTDeltaScatteringMie->GetTargetView( 0, 0, 0 ) };
		m_Device.SetRenderTargets( RES_3D_U, RES_3D_COS_THETA_VIEW, 2, ppTargets );

		CB.m.dUVW = pRTDeltaScatteringRayleigh->GetdUVW();
		CB.UpdateData();

		m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

	USING_MATERIAL_END

	// Assign to slot 14 & 15
	m_Device.RemoveRenderTargets();
	pRTDeltaScatteringRayleigh->SetPS( 14 );
	pRTDeltaScatteringMie->SetPS( 15 );

	// ==================================================
	// Merges DeltaScatteringRayleigh & Mie into initial inscatter texture S (line 5 in algorithm 4.1)
	USING_MATERIAL_START( *pMatMergeInitialScattering )

		m_Device.SetRenderTarget( *m_ppRTInScattering[0] );

		CB.m.dUVW = m_ppRTInScattering[0]->GetdUVW();
		CB.UpdateData();

		m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

	USING_MATERIAL_END

	//////////////////////////////////////////////////////////////////////////
	// Loop for each scattering order (line 6 in algorithm 4.1)
	for ( int Order=2; Order <= 4; Order++ )
	{
		// ==================================================
		// Computes deltaJ (line 7 in algorithm 4.1)
		USING_MATERIAL_START( *pMatComputeInScattering_Delta )

			m_Device.SetRenderTarget( *pRTDeltaScattering );

			CB.m.dUVW = pRTDeltaScattering->GetdUVW();
			CB.m.bFirstPass = Order == 2;
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

		USING_MATERIAL_END

		// Assign to slot 16
		m_Device.RemoveRenderTargets();
		pRTDeltaScattering->SetPS( 16 );

		// ==================================================
		// Computes deltaE (line 8 in algorithm 4.1)
		USING_MATERIAL_START( *pMatComputeIrradiance_Delta )

			m_Device.SetRenderTarget( *pRTDeltaIrradiance );

			CB.m.dUVW = NjFloat4( pRTDeltaIrradiance->GetdUV(), 0.0 );
			CB.m.bFirstPass = Order == 2;
			CB.UpdateData();

			m_ScreenQuad.Render( M );

		USING_MATERIAL_END

		// Assign to slot 13 again
		m_Device.RemoveRenderTargets();
		pRTDeltaIrradiance->SetPS( 13 );

		// ==================================================
		// Computes deltaS (line 9 in algorithm 4.1)
		USING_MATERIAL_START( *pMatComputeInScattering_Multiple )

			m_Device.SetRenderTarget( *pRTDeltaScatteringRayleigh );	// Warning: We're re-using Rayleigh slot.
																		// It doesn't matter for next orders where we don't sample from Rayleigh+Mie separately anymore (only done in first pass)

			CB.m.dUVW = pRTDeltaScattering->GetdUVW();
			CB.m.bFirstPass = Order == 2;
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

		USING_MATERIAL_END

		// Assign to slot 14 again
		m_Device.RemoveRenderTargets();
		pRTDeltaScatteringRayleigh->SetPS( 14 );

		// ==================================================
		// Adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
		m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Additive );

		USING_MATERIAL_START( *pMatAccumulateIrradiance )

			m_Device.SetRenderTarget( *m_ppRTIrradiance[0] );

			CB.m.dUVW = NjFloat4( m_ppRTIrradiance[0]->GetdUV(), 0 );
			CB.UpdateData();

			m_ScreenQuad.Render( M );

		USING_MATERIAL_END

		// ==================================================
 		// Adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
		USING_MATERIAL_START( *pMatAccumulateInScattering )

			m_Device.SetRenderTarget( *m_ppRTInScattering[0] );

			CB.m.dUVW = m_ppRTInScattering[0]->GetdUVW();
			CB.UpdateData();

			m_ScreenQuad.RenderInstanced( M, RES_3D_ALTITUDE );

		USING_MATERIAL_END

		m_Device.SetStates( NULL, NULL, m_Device.m_pBS_Disabled );
	}

	// Assign final textures to slots 8 & 9
	m_Device.RemoveRenderTargets();
	m_ppRTInScattering[0]->SetVS( 8, true );
	m_ppRTInScattering[0]->SetPS( 8, true );
	m_ppRTIrradiance[0]->SetVS( 9, true );
	m_ppRTIrradiance[0]->SetPS( 9, true );

	// Release materials & temporary RTs
	delete pMatAccumulateInScattering;
	delete pMatAccumulateIrradiance;
	delete pMatMergeInitialScattering;
	delete pMatComputeInScattering_Multiple;
	delete pMatComputeInScattering_Delta;
	delete pMatComputeInScattering_Single;
	delete pMatComputeIrradiance_Delta;
	delete pMatComputeIrradiance_Single;
	delete pMatComputeTransmittance;

	delete pRTDeltaIrradiance;
	delete pRTDeltaScatteringRayleigh;
	delete pRTDeltaScatteringMie;
	delete pRTDeltaScattering;

	// Save tables
	Texture3D*	pStagingScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	Texture2D*	pStagingTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );
	Texture2D*	pStagingIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true );

	pStagingScattering->CopyFrom( *m_ppRTInScattering[0] );
	pStagingTransmittance->CopyFrom( *m_ppRTTransmittance[0] );
	pStagingIrradiance->CopyFrom( *m_ppRTIrradiance[0] );

	pStagingIrradiance->Save( FILENAME_IRRADIANCE );
	pStagingTransmittance->Save( FILENAME_TRANSMITTANCE );
	pStagingScattering->Save( FILENAME_SCATTERING );

	delete pStagingIrradiance;
	delete pStagingTransmittance;
	delete pStagingScattering;

#else
	// Load tables
	Texture2D*	pStagingTransmittance = new Texture2D( m_Device, TRANSMITTANCE_W, TRANSMITTANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );
	Texture3D*	pStagingScattering = new Texture3D( m_Device, RES_3D_U, RES_3D_COS_THETA_VIEW, RES_3D_ALTITUDE, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );
	Texture2D*	pStagingIrradiance = new Texture2D( m_Device, IRRADIANCE_W, IRRADIANCE_H, 1, PixelFormatRGBA16F::DESCRIPTOR, 1, NULL, true, true );

// This includes a dependency on disk files... Useless if we recompute them!
// #if 0
// 	BuildTransmittanceTable( TRANSMITTANCE_W, TRANSMITTANCE_H, *pStagingTransmittance );
// #else
// 	pStagingTransmittance->Load( FILENAME_TRANSMITTANCE );
// #endif
// 
// 	pStagingIrradiance->Load( FILENAME_IRRADIANCE );
// 	pStagingScattering->Load( FILENAME_SCATTERING );

	m_ppRTTransmittance[0]->CopyFrom( *pStagingTransmittance );
	m_ppRTInScattering[0]->CopyFrom( *pStagingScattering );
	m_ppRTIrradiance[0]->CopyFrom( *pStagingIrradiance );

	delete pStagingIrradiance;
	delete pStagingTransmittance;
	delete pStagingScattering;

	m_ppRTTransmittance[0]->SetVS( 7, true );
	m_ppRTTransmittance[0]->SetPS( 7, true );
	m_ppRTInScattering[0]->SetVS( 8, true );
	m_ppRTInScattering[0]->SetPS( 8, true );
	m_ppRTIrradiance[0]->SetVS( 9, true );
	m_ppRTIrradiance[0]->SetPS( 9, true );

#endif
}
