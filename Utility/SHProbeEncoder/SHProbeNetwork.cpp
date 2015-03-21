#include "../../GodComplex.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

SHProbeNetwork::SHProbeNetwork() 
	: m_pDevice( NULL )
	, m_ErrorCode( 0 )
	, m_ProbesCount( 0 )
	, m_MaxProbesCount( 0 )
	, m_pProbes( NULL )
	, m_pPrimProbeIDs( NULL )
	, m_ProbeUpdateIndex( 0 ) {
}

SHProbeNetwork::~SHProbeNetwork() {
	Exit();
}

void	SHProbeNetwork::Init( Device& _Device, Primitive& _ScreenQuad ) {

	m_pDevice = &_Device;
	m_pScreenQuad = &_ScreenQuad;

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Probe = new CB<CBProbe>( _Device, 10 );
	m_pCB_UpdateProbes = new CB<CBUpdateProbes>( _Device, 10 );

	//////////////////////////////////////////////////////////////////////////
	// Create the probes structured buffers
	m_pSB_RuntimeProbes = NULL;
	m_pSB_RuntimeProbeNetworkInfos = NULL;

	m_pSB_RuntimeProbeUpdateInfos = new SB<RuntimeProbeUpdateInfo>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME, true );
	m_pSB_RuntimeProbeSamples = new SB<RuntimeProbeUpdateSampleInfo>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME*SHProbeEncoder::PROBE_SAMPLES_COUNT, true );
	m_pSB_RuntimeProbeEmissiveSurfaces = new SB<RuntimeProbeUpdateEmissiveSurfaceInfo>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME*SHProbeEncoder::MAX_PROBE_EMISSIVE_SURFACES, true );

	// Create the static SH coefficients for each sample
	m_pSB_RuntimeProbeSamplesSH = new SB<SHCoeffs1>( *m_pDevice, SHProbeEncoder::PROBE_SAMPLES_COUNT, true );
	for ( int SampleIndex=0; SampleIndex < SHProbeEncoder::PROBE_SAMPLES_COUNT; SampleIndex++ ) {
		const double*	SH = m_ProbeEncoder.GetSampleSHCoefficients( SampleIndex );
		for ( int SHCoeffIndex=0; SHCoeffIndex < 9; SHCoeffIndex++ )
			m_pSB_RuntimeProbeSamplesSH->m[SampleIndex].pSH[SHCoeffIndex] = float( SH[SHCoeffIndex] );
	}
	m_pSB_RuntimeProbeSamplesSH->Write();

	m_ppSB_RuntimeSHStatic[0] = NULL;
	m_ppSB_RuntimeSHStatic[1] = NULL;
	m_pSB_RuntimeSHAmbient = NULL;
	m_pSB_RuntimeSHDynamic = NULL;
	m_pSB_RuntimeSHDynamicSun = NULL;
	m_pSB_RuntimeSHFinal = NULL;


	//////////////////////////////////////////////////////////////////////////
	// Create shaders
	{
ScopedForceMaterialsLoadFromBinary		bisou;

		CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_GI_RENDER_CUBEMAP, "./Resources/Shaders/GIRenderCubeMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 0 );
 		CHECK_MATERIAL( m_pMatRenderNeighborProbe = CreateMaterial( IDR_SHADER_GI_RENDER_NEIGHBOR_PROBE, "./Resources/Shaders/GIRenderNeighborProbe.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
	}

	{
// This one is REALLY heavy! So build it once and reload it from binary forever again
ScopedForceMaterialsLoadFromBinary		bisou;

		CHECK_MATERIAL( m_pCSUpdateProbeDynamicSH = CreateComputeShader( IDR_SHADER_GI_UPDATE_PROBE, "./Resources/Shaders/GIUpdateProbe.hlsl", "CS" ), 2 );
	}

	{
ScopedForceMaterialsLoadFromBinary	bisou;

 		CHECK_MATERIAL( m_pCSAccumulateProbeSH = CreateComputeShader( IDR_SHADER_GI_UPDATE_PROBE, "./Resources/Shaders/GIUpdateProbe.hlsl", "CS_AccumulateSH" ), 3 );
	}
}

void	SHProbeNetwork::Exit() {
	m_ProbesCount = 0;
	SAFE_DELETE_ARRAY( m_pProbes );

	delete m_pPrimProbeIDs;

	delete m_ppSB_RuntimeSHStatic[0];
	delete m_ppSB_RuntimeSHStatic[1];
	delete m_pSB_RuntimeSHAmbient;
	delete m_pSB_RuntimeSHDynamic;
	delete m_pSB_RuntimeSHDynamicSun;
	delete m_pSB_RuntimeSHFinal;

	delete m_pCSUpdateProbeDynamicSH;
	delete m_pMatRenderNeighborProbe;
	delete m_pMatRenderCubeMap;

	delete m_pSB_RuntimeProbeSamplesSH;

	delete m_pSB_RuntimeProbeEmissiveSurfaces;
	delete m_pSB_RuntimeProbeSamples;
	delete m_pSB_RuntimeProbeUpdateInfos;
	delete m_pSB_RuntimeProbeNetworkInfos;
	delete m_pSB_RuntimeProbes;

	delete m_pCB_UpdateProbes;
	delete m_pCB_Probe;
}

void	SHProbeNetwork::PreAllocateProbes( int _ProbesCount ) {
	m_MaxProbesCount = _ProbesCount;
	m_pProbes = new SHProbe[m_MaxProbesCount];
}

void	SHProbeNetwork::AddProbe( Scene::Probe& _Probe ) {
	ASSERT( m_ProbesCount < m_MaxProbesCount, "Probes count out of range!" );
	m_pProbes[m_ProbesCount].ProbeID = m_ProbesCount;
	m_pProbes[m_ProbesCount].pSceneProbe = &_Probe;
	m_ProbesCount++;
}

void	SHProbeNetwork::UpdateDynamicProbes( DynamicUpdateParms& _Parms ) {
//	ASSERT( m_ProbesCount <= MAX_PROBE_UPDATES_PER_FRAME, "Increase max probes update per frame! Or write the time-sliced updater you promised!" );

	U32		ProbeUpdatesCount = MIN( _Parms.MaxProbeUpdatesPerFrame, U32(m_ProbesCount) );

	// Prepare constant buffer for update
	m_pCB_UpdateProbes->m.SunBoost = _Parms.BounceFactorSun;
	m_pCB_UpdateProbes->m.SkyBoost = _Parms.BounceFactorSky;
	m_pCB_UpdateProbes->m.DynamicLightsBoost = _Parms.BounceFactorDynamic;
	m_pCB_UpdateProbes->m.StaticLightingBoost = _Parms.BounceFactorStatic;
	m_pCB_UpdateProbes->m.EmissiveBoost = _Parms.BounceFactorEmissive;
	m_pCB_UpdateProbes->m.NeighborProbesContributionBoost = _Parms.BounceFactorNeighbors;
// 	for ( int i=0; i < 9; i++ )
// 		m_pCB_UpdateProbes->m.AmbientSH[i] = float4( _Parms.AmbientSkySH[i], 0 );	// Update one by one because of float3 padding

	m_pCB_UpdateProbes->UpdateData();


#if 1	// Hardware update
	// We prepare the update structures for each probe and send them to the compute shader
	// . The compute shader will then evaluate lighting for all the samples of each probe, use their contribution to weight
	//		each sample's SH coefficients that will be added together to form the indirect lighting SH coefficients.
	// . Then it will compute the product of ambient sky SH and occlusion SH for the probe to add the contribution of the occluded sky
	// . It will also add the emissive surfaces' SH weighted by the intensity of the emissive materials at the time (diffuse area lighting).
	// . Finally, it will estimate the neighbor's "perceived visibility" and propagate their SH via a product of their SH with the
	//		neighbor visibility mask. This way we get additional light bounces from probe to probe.
	//
	// Basically for every probe update, we perform 1(sky)+4(neighbor) expensive SH products and compute lighting for at most 128 samples in the scene
	//
//TODO: Handle a proper stack of probes to update

	// Prepare the buffer of probe update infos and sampling point infos
	RuntimeProbeUpdateSampleInfo*	pSampleUpdateInfos = m_pSB_RuntimeProbeSamples->m;
	int		TotalEmissiveSurfacesCount = 0;
	for ( U32 ProbeUpdateIndex=0; ProbeUpdateIndex < ProbeUpdatesCount; ProbeUpdateIndex++ ) {
		// Simple at the moment, when we have the update stack we'll have to fetch the index from it...
//		int			ProbeIndex = ProbeUpdateIndex;

		// Still simple: we update N probes each frame in sequence, next frame we'll update the next N ones...
		int			ProbeIndex = (m_ProbeUpdateIndex + ProbeUpdateIndex) % m_ProbesCount;

		SHProbe&	Probe = m_pProbes[ProbeIndex];

		// Fill the probe update infos
		RuntimeProbeUpdateInfo&	ProbeUpdateInfos = m_pSB_RuntimeProbeUpdateInfos->m[ProbeUpdateIndex];

		ProbeUpdateInfos.Index = ProbeIndex;
		ProbeUpdateInfos.EmissiveSurfacesStart = TotalEmissiveSurfacesCount;
		ProbeUpdateInfos.EmissiveSurfacesCount = Probe.EmissiveSurfacesCount;

		// Copy neighbor info
		ProbeUpdateInfos.NeighborProbeIDs[0] = Probe.pNeighborProbeInfos[0].ProbeID;
		ProbeUpdateInfos.NeighborProbeIDs[1] = Probe.pNeighborProbeInfos[1].ProbeID;
		ProbeUpdateInfos.NeighborProbeIDs[2] = Probe.pNeighborProbeInfos[2].ProbeID;
		ProbeUpdateInfos.NeighborProbeIDs[3] = Probe.pNeighborProbeInfos[3].ProbeID;
		for( int i=0; i < 9; i++ ) {
			ProbeUpdateInfos.SHConvolution[i].x = Probe.pNeighborProbeInfos[0].SH[i];
			ProbeUpdateInfos.SHConvolution[i].y = Probe.pNeighborProbeInfos[1].SH[i];
			ProbeUpdateInfos.SHConvolution[i].z = Probe.pNeighborProbeInfos[2].SH[i];
			ProbeUpdateInfos.SHConvolution[i].w = Probe.pNeighborProbeInfos[3].SH[i];
		}

		// Fill the samples update infos
		SHProbe::Sample*	pSample = Probe.pSamples;
		for ( U32 SampleIndex=0; SampleIndex < SHProbeEncoder::PROBE_SAMPLES_COUNT; SampleIndex++, pSample++, pSampleUpdateInfos++ ) {
			pSampleUpdateInfos->Position = pSample->Position;
			pSampleUpdateInfos->Normal = pSample->Normal;
			pSampleUpdateInfos->Albedo = pSample->SHFactor * pSample->Albedo;
			pSampleUpdateInfos->Radius = pSample->Radius;
		}

		// Fill the emissive surface update infos
		for ( U32 EmissiveSurfaceIndex=0; EmissiveSurfaceIndex < Probe.EmissiveSurfacesCount; EmissiveSurfaceIndex++ ) {
			SHProbe::EmissiveSurface				EmissiveSurface = Probe.pEmissiveSurfaces[EmissiveSurfaceIndex];
			RuntimeProbeUpdateEmissiveSurfaceInfo&	EmissiveSetUpdateInfos = m_pSB_RuntimeProbeEmissiveSurfaces->m[TotalEmissiveSurfacesCount+EmissiveSurfaceIndex];

			ASSERT( EmissiveSurface.pEmissiveMaterial != NULL, "Invalid emissive material!" );
			EmissiveSetUpdateInfos.EmissiveColor = EmissiveSurface.pEmissiveMaterial->m_EmissiveColor;

			memcpy_s( EmissiveSetUpdateInfos.SH, sizeof(EmissiveSetUpdateInfos.SH), EmissiveSurface.pSHEmissive, 9*sizeof(float) );
		}

		TotalEmissiveSurfacesCount += Probe.EmissiveSurfacesCount;
	}

	// =========================================================
	// Do the update!
	{
		USING_COMPUTESHADER_START( *m_pCSUpdateProbeDynamicSH )

		m_pSB_RuntimeSHFinal->SetInput( 9, true );	// Feed last frame's SH for neighbor bounce

		m_pSB_RuntimeProbeUpdateInfos->Write( ProbeUpdatesCount );
		m_pSB_RuntimeProbeUpdateInfos->SetInput( 10 );

		m_pSB_RuntimeProbeSamples->Write( ProbeUpdatesCount * SHProbeEncoder::PROBE_SAMPLES_COUNT );
		m_pSB_RuntimeProbeSamples->SetInput( 11 );

		m_pSB_RuntimeProbeEmissiveSurfaces->Write( TotalEmissiveSurfacesCount );
		m_pSB_RuntimeProbeEmissiveSurfaces->SetInput( 12 );

		m_pSB_RuntimeProbeSamplesSH->SetInput( 13 );

		m_pSB_RuntimeSHDynamic->SetOutput( 0 );
		m_pSB_RuntimeSHDynamicSun->SetOutput( 1 );

		M.Dispatch( ProbeUpdatesCount, 1, 1 );

		m_pSB_RuntimeSHFinal->RemoveFromLastAssignedSlots();	// So we can bind it as output later

		USING_COMPUTE_SHADER_END
	}

	// Advance probe update index
	if ( m_ProbesCount > 0 )
		m_ProbeUpdateIndex = (m_ProbeUpdateIndex + ProbeUpdatesCount) % m_ProbesCount;

#else
	// Software update (no shadows!)
	for ( int ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		// Clear light accumulation for the probe
		Probe.ClearLightBounce( pSHAmbient );

		// Iterate on each patch and compute energy level
		for ( U32 SetIndex=0; SetIndex < Probe.SurfacesCount; SetIndex++ )
		{
			const SHProbe::Surface&	Set = Probe.pSurfaces[SetIndex];

			// Compute irradiance for every sample
			float3	SetIrradiance = float3::Zero;
			for ( U32 SampleIndex=0; SampleIndex < Set.SamplesCount; SampleIndex++ )
			{
				const SHProbe::Surface::Sample&	Sample = Set.pSamples[SampleIndex];

				// Compute irradiance from every light
				for ( int LightIndex=0; LightIndex < m_pCB_Scene->m.DynamicLightsCount; LightIndex++ )
				{
					const LightStruct&	Light = m_pSB_LightsDynamic->m[LightIndex];

					// Compute light vector
					float3	Set2Light = Light.Position - Sample.Position;
					float		DistanceProbe2Light = Set2Light.Length();
					float		InvDistance = 1.0f / DistanceProbe2Light;
					Set2Light = Set2Light * InvDistance;

					float		NdotL = MAX( 0.0f, Set2Light | Sample.Normal );
					float3	LightIrradiance = Light.Color * NdotL * InvDistance * InvDistance;	// I=E.(N.L)/r²

					SetIrradiance = SetIrradiance + LightIrradiance;
				}
			}

			// Average lighting
			SetIrradiance = SetIrradiance / float(Set.SamplesCount);

			// Transform this into SH
			float3	pSetSH[9];
			for ( int i=0; i < 9; i++ )
				pSetSH[i] = SetIrradiance * Set.pSHBounce[i];	// Simply irradiance * (Rho/PI) encoded as SH

			// Accumulate to total SH for the probe
			Probe.AccumulateLightBounce( pSetSH );
		}
	}

	// Write to the runtime structured buffer
	for ( int ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		SHProbe&	Probe = m_pProbes[ProbeIndex];
		RuntimeProbe&	Runtime = m_pSB_RuntimeProbes->m[ProbeIndex];

		// Write the result to the probe structured buffer
		memcpy( Runtime.pSHBounce, Probe.pSHBouncedLight, 9*sizeof(float3) );
	}

	m_pSB_RuntimeProbes->Write();

#endif

	// =========================================================
	// Perform the final accumulation of all the SH sources
	{
		USING_COMPUTESHADER_START( *m_pCSAccumulateProbeSH )

		m_ppSB_RuntimeSHStatic[0]->SetInput( 10 );
		m_pSB_RuntimeSHAmbient->SetInput( 11 );
		m_pSB_RuntimeSHDynamic->SetInput( 12 );
		m_pSB_RuntimeSHDynamicSun->SetInput( 13 );

		m_pSB_RuntimeSHFinal->SetOutput( 0 );

		int	GroupsCount = (m_ProbesCount + 0xFF) >> 8;	// 256 threads per group
		M.Dispatch( GroupsCount, 1, 1 );

		m_pSB_RuntimeSHDynamic->RemoveFromLastAssignedSlots();	// So we can bind them as output for next frame update
		m_pSB_RuntimeSHDynamicSun->RemoveFromLastAssignedSlots();

		USING_COMPUTE_SHADER_END
	}

	// =========================================================
	// Setup the input buffers for scene rendering
	m_pSB_RuntimeProbes->SetInput( 8, true );
	m_pSB_RuntimeSHFinal->SetInput( 9, true );
}

U32	SHProbeNetwork::GetNearestProbe( const float3& _wsPosition ) const {
	float					ProbeDistance;
	const SHProbe* const*	ppNearestProbe = m_ProbeOctree.FetchNearest( _wsPosition, ProbeDistance );
	U32						probeID = ppNearestProbe != NULL ? (*ppNearestProbe)->ProbeID : 0xFFFFFFFFU;
	return probeID;
}

void	SHProbeNetwork::PreComputeProbes( const char* _pPathToProbes, IRenderSceneDelegate& _RenderScene, Scene& _Scene, U32 _TotalFacesCount ) {

	const float		Z_INFINITY = 1e6f;
	const float		Z_INFINITY_TEST = 0.99f * Z_INFINITY;

	if ( m_pRTCubeMap == NULL ) {
		m_pRTCubeMap = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, -6 * 4, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain albedo (cube 0) + (normal + distance) (cube 1) + (static lighting + emissive surface index) (cube 2) + Probe IDs (cube 3)
	}
	Texture2D*	pRTCubeMapDepth = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );
	Texture2D*	pRTCubeMapStaging = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, -6 * 4, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );


	//////////////////////////////////////////////////////////////////////////
	// Prepare the cube map face transforms
	// Here are the transform to render the 6 faces of a cube map
	// Remember the +Z face is not oriented the same way as our Z vector: http://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
	//
	//
	//		^ +Y
	//		|   +Z  (our actual +Z faces the other way!)
	//		|  /
	//		| /
	//		|/
	//		o------> +X
	//
	//
	float3	SideAt[6] = 
	{
		float3(  1, 0, 0 ),
		float3( -1, 0, 0 ),
		float3( 0,  1, 0 ),
		float3( 0, -1, 0 ),
		float3( 0, 0,  1 ),
		float3( 0, 0, -1 ),
	};
	float3	SideRight[6] = 
	{
		float3( 0, 0, -1 ),
		float3( 0, 0,  1 ),
		float3(  1, 0, 0 ),
		float3(  1, 0, 0 ),
		float3(  1, 0, 0 ),
		float3( -1, 0, 0 ),
	};

	float4x4	SideWorld2Proj[6];
	float4x4	Side2Local[6];
	float4x4	Camera2Proj = float4x4::ProjectionPerspective( 0.5f * PI, 1.0f, 0.01f, 1000.0f );
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
	{
		float4x4	Camera2Local;
		Camera2Local.SetRow( 0, SideRight[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 1, SideAt[CubeFaceIndex] ^ SideRight[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 2, SideAt[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 3, float3::Zero, 1 );

		Side2Local[CubeFaceIndex] = Camera2Local;

		float4x4	Local2Camera = Camera2Local.Inverse();
		float4x4	Local2Proj = Local2Camera * Camera2Proj;
		SideWorld2Proj[CubeFaceIndex] = Local2Proj;
	}

	// Create the special CB for cube map projections
	struct	CBCubeMapCamera
	{
		float4x4	Camera2World;
		float4x4	World2Proj;
	};
	CB<CBCubeMapCamera>*	pCBCubeMapCamera = new CB<CBCubeMapCamera>( *m_pDevice, 8, true );


	//////////////////////////////////////////////////////////////////////////
	// Initialize probe influences for each face
	m_ProbeInfluencePerFace.Init( _TotalFacesCount );
	m_ProbeInfluencePerFace.SetCount( _TotalFacesCount );
	ProbeInfluence*	pInfluence = &m_ProbeInfluencePerFace[0];
	for ( U32 FaceIndex=0; FaceIndex < _TotalFacesCount; FaceIndex++, pInfluence++ ) {
		pInfluence->ProbeID = ~0UL;
		pInfluence->Influence = 0.0;
	}


	//////////////////////////////////////////////////////////////////////////
	// Render every probe as a cube map & process
	//
	char	pTemp[1024];

	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		// Clear cube maps
		m_pDevice->ClearRenderTarget( *m_pRTCubeMap->GetRTV( 0, 6*0, 6 ), float4::Zero );
		m_pDevice->ClearRenderTarget( *m_pRTCubeMap->GetRTV( 0, 6*1, 6 ), float4( 0, 0, 0, Z_INFINITY ) );	// We clear distance to infinity here

		float4	Bisou = float4::Zero;
		((U32&) Bisou.w) = 0xFFFFFFFFUL;
		m_pDevice->ClearRenderTarget( *m_pRTCubeMap->GetRTV( 0, 6*2, 6 ), Bisou );	// Clear emissive surface ID to -1 (invalid) and static color to 0
		((U32&) Bisou.x) = 0xFFFFFFFFUL;
		m_pDevice->ClearRenderTarget( *m_pRTCubeMap->GetRTV( 0, 6*3, 6 ), Bisou );	// Clear probe ID to -1 (invalid)

		float4x4	ProbeLocal2World = Probe.pSceneProbe->m_Local2World;
		ProbeLocal2World.Normalize();

ProbeLocal2World = float4x4::Identity;
ProbeLocal2World.SetRow( 3, Probe.pSceneProbe->m_Local2World.GetRow( 3 ) );

		ASSERT( ProbeLocal2World.GetRow(0).LengthSq() > 0.999f && ProbeLocal2World.GetRow(1).LengthSq() > 0.999f && ProbeLocal2World.GetRow(2).LengthSq() > 0.999f, "Not identity! If not identity then transform probe patch positions/normals/etc. by probe matrix!" );

		float4x4	ProbeWorld2Local = ProbeLocal2World.Inverse();

		// Render the 6 faces
		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
			// Update cube map face camera transform
			float4x4	World2Proj = ProbeWorld2Local * SideWorld2Proj[CubeFaceIndex];

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;
			pCBCubeMapCamera->m.World2Proj = World2Proj;
			pCBCubeMapCamera->UpdateData();

			m_pDevice->ClearDepthStencil( *pRTCubeMapDepth, 1.0f, 0, true, false );

			//////////////////////////////////////////////////////////////////////////
			// 1] Render Albedo + Normal + Distance + Static lit + Emissive Mat ID
			m_pDevice->SetStates( m_pDevice->m_pRS_CullFront, m_pDevice->m_pDS_ReadWriteLess, m_pDevice->m_pBS_Disabled );

			ID3D11RenderTargetView*	ppViews[3] = {
				m_pRTCubeMap->GetRTV( 0, 6*0+CubeFaceIndex, 1 ),
				m_pRTCubeMap->GetRTV( 0, 6*1+CubeFaceIndex, 1 ),
				m_pRTCubeMap->GetRTV( 0, 6*2+CubeFaceIndex, 1 )
			};
			m_pDevice->SetRenderTargets( SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, 3, ppViews, pRTCubeMapDepth->GetDSV() );

			// Render scene
			_RenderScene( *m_pMatRenderCubeMap );


			//////////////////////////////////////////////////////////////////////////
			// 2] Render neighborhood for each probe
			// The idea here is simply to build a 3D voronoi cell by splatting the planes passing through all other probes
			//	with their normal set to the direction from the other probe to the current probe.
			// Splatting a new plane and accounting for the depth buffer will let visible pixels from the plane show up
			//	and write the ID of the probe.
			//
			// Reading back the cube map will indicate the solid angle perceived by each probe to each of its neighbors
			//	so we can create a linked list of neighbor probes, of their visibilities and solid angle
			//
			m_pDevice->SetStates( m_pDevice->m_pRS_CullNone, m_pDevice->m_pDS_ReadWriteLess, m_pDevice->m_pBS_Disabled );
			m_pDevice->SetRenderTarget( SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, *m_pRTCubeMap->GetRTV( 0, 6*3+CubeFaceIndex, 1 ), pRTCubeMapDepth->GetDSV() );

			m_pCB_Probe->m.CurrentProbePosition = Probe.pSceneProbe->m_Local2World.GetRow( 3 );

			USING_MATERIAL_START( *m_pMatRenderNeighborProbe )

			for ( U32 NeighborProbeIndex=0; NeighborProbeIndex < m_ProbesCount; NeighborProbeIndex++ )
				if ( NeighborProbeIndex != ProbeIndex )
				{
					SHProbe&	NeighborProbe = m_pProbes[NeighborProbeIndex];

					m_pCB_Probe->m.NeighborProbeID = NeighborProbeIndex;
					m_pCB_Probe->m.NeighborProbePosition = NeighborProbe.pSceneProbe->m_Local2World.GetRow( 3 );
					m_pCB_Probe->UpdateData();

					m_pScreenQuad->Render( M );
				}

			USING_MATERIAL_END
		}


		//////////////////////////////////////////////////////////////////////////
		// 3] Read back cube map and create the various dynamic samples & static SH coefficients
		pRTCubeMapStaging->CopyFrom( *m_pRTCubeMap );

#if 0	// Save to disk for processing by the external ProbeSHEncoder tool (not needed anymore since we're doing everything in here now)
		sprintf_s( pTemp, "%sProbe%02d.pom", _pPathToProbes, ProbeIndex );
		pRTCubeMapStaging->Save( pTemp );
#endif

		m_ProbeEncoder.EncodeProbeCubeMap( *pRTCubeMapStaging, ProbeIndex, m_ProbesCount, _TotalFacesCount );

		// Save probe results
		sprintf_s( pTemp, "%sProbe%02d.probeset", _pPathToProbes, ProbeIndex );
		m_ProbeEncoder.Save( pTemp );

#ifdef _DEBUG
		// Save probe debug pixels (can be analyzed with the external tool found in Tools.sln => GIProbesDebugger)
		sprintf_s( pTemp, "%sProbe%02d.probepixels", _pPathToProbes, ProbeIndex );
		m_ProbeEncoder.SavePixels( pTemp );
#endif

		//////////////////////////////////////////////////////////////////////////
		// 4] Collate per-face probe influence for the secondary vertex stream
		const double*	pNewInfluence = &m_ProbeEncoder.GetProbeInfluences()[0];
		ProbeInfluence*	pCurrentInfluence = &m_ProbeInfluencePerFace[0];
		for ( U32 FaceIndex=0; FaceIndex < _TotalFacesCount; FaceIndex++, pCurrentInfluence++, pNewInfluence++ ) {
			if ( *pNewInfluence > pCurrentInfluence->Influence ) {
				pCurrentInfluence->Influence = *pNewInfluence;
				pCurrentInfluence->ProbeID = Probe.ProbeID;
			}
		}
	}

	delete pCBCubeMapCamera;


	//////////////////////////////////////////////////////////////////////////
	// Save the final probe influences
	BuildProbeInfluenceVertexStream( _Scene, _pPathToProbes );


	//////////////////////////////////////////////////////////////////////////
	// Release
#if 1
m_pDevice->RemoveRenderTargets();
m_pRTCubeMap->SetPS( 64 );
#endif

	delete pRTCubeMapStaging;

	delete pRTCubeMapDepth;

//### Keep it for debugging!
// 	delete m_pRTCubeMap;
}

void	SHProbeNetwork::MeshWithAdjacency::Build( const Scene::Mesh& _Mesh, ProbeInfluence* _pProbeInfluencePerFace ) {

	m_World2Local = _Mesh.m_Local2World.Inverse();

	m_PrimitivesCount = _Mesh.m_PrimitivesCount;
	m_pPrimitives = new Primitive[_Mesh.m_PrimitivesCount];

	int	FaceOffset = 0;
	for ( int PrimitiveIndex=0; PrimitiveIndex < _Mesh.m_PrimitivesCount; PrimitiveIndex++ ) {
		const Scene::Mesh::Primitive&	SourcePrim = _Mesh.m_pPrimitives[PrimitiveIndex];
		m_pPrimitives[PrimitiveIndex].Build( SourcePrim, _pProbeInfluencePerFace + FaceOffset );
		FaceOffset += SourcePrim.m_FacesCount;
	}
}

bool	SHProbeNetwork::MeshWithAdjacency::PropagateProbeInfluences( U32 _PassIndex ) {
	bool	stillSpreading = false;
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ ) {
		Primitive&	P = m_pPrimitives[PrimitiveIndex];
		stillSpreading |= P.PropagateProbeInfluences( _PassIndex );
	}

	return stillSpreading;
}

void	SHProbeNetwork::MeshWithAdjacency::AssignNearestProbe( U32 _ProbesCount, const SHProbe* _pProbes ) {
	// Transform probe positions into local space
	List<float3>	LocalProbePositions;
	LocalProbePositions.Init( _ProbesCount );
	LocalProbePositions.SetCount( _ProbesCount );
	for ( U32 ProbeIndex=0; ProbeIndex < _ProbesCount; ProbeIndex++ ) {
		const SHProbe&	Probe = _pProbes[ProbeIndex];
		float3&			lsProbePosition = LocalProbePositions[ProbeIndex];

		lsProbePosition = Probe.pSceneProbe->m_Local2World.GetRow( 3 ) * m_World2Local;
	}

	// Assign nearest probes
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ ) {
		Primitive&	P = m_pPrimitives[PrimitiveIndex];
		P.AssignNearestProbe( _ProbesCount, &LocalProbePositions[0] );
	}
}

void	SHProbeNetwork::MeshWithAdjacency::RedistributeProbeIDs2Vertices( ProbeInfluence**& _ppProbeInfluences ) const {
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ ) {
		Primitive&	P = m_pPrimitives[PrimitiveIndex];
		P.RedistributeProbeIDs2Vertices( _ppProbeInfluences );
		_ppProbeInfluences += P.m_VerticesCount;	// Make the pointer advance as we're done with that primitive
	}
}

void	SHProbeNetwork::MeshWithAdjacency::Primitive::Build( const Scene::Mesh::Primitive& _Primitive, ProbeInfluence* _pProbeInfluencePerFace ) {

	DictionaryGeneric< EdgeKey, FacePair >	SharedEdges( 3 * _Primitive.m_FacesCount );

	m_FacesCount = _Primitive.m_FacesCount;
	m_pFaces = new Face[m_FacesCount];
	m_VerticesCount = _Primitive.m_VerticesCount;
	m_pVerticesProbeInfluence = new ProbeInfluence*[m_VerticesCount];
	memset( m_pVerticesProbeInfluence, 0, m_VerticesCount*sizeof(ProbeInfluence*) );

	const U32*		pSourceFace = _Primitive.m_pFaces;
	Face*			pTargetFace = m_pFaces;
	ProbeInfluence* pProbeInfluence = _pProbeInfluencePerFace;
	for ( U32 FaceIndex=0; FaceIndex < _Primitive.m_FacesCount; FaceIndex++, pTargetFace++, pProbeInfluence++ ) {
		// Build the face
		pTargetFace->V[0] = *pSourceFace++;
		pTargetFace->V[1] = *pSourceFace++;
		pTargetFace->V[2] = *pSourceFace++;

		const float3&	P0 = ((Scene::Mesh::Primitive::VF_P3N3G3B3T2*) _Primitive.m_pVertices)[pTargetFace->V[0]].P;
		const float3&	P1 = ((Scene::Mesh::Primitive::VF_P3N3G3B3T2*) _Primitive.m_pVertices)[pTargetFace->V[1]].P;
		const float3&	P2 = ((Scene::Mesh::Primitive::VF_P3N3G3B3T2*) _Primitive.m_pVertices)[pTargetFace->V[2]].P;

		pTargetFace->P = (P0 + P1 + P2) / 3.0f;
		pTargetFace->N = (P2 - P0).Cross( P1 - P0 ).Normalize();
		pTargetFace->pProbeInfluence = pProbeInfluence;

		// Build adjacency info
		EdgeKey		Key;
		for ( int EdgeIndex=0; EdgeIndex < 3; EdgeIndex++ ) {
			Key.V0 = pTargetFace->V[EdgeIndex];
			Key.V1 = pTargetFace->V[(EdgeIndex+1)%3];

			FacePair*	ExistingValue = SharedEdges.Get( Key );
			if ( ExistingValue == NULL ) {
				// First face sharing this edge!
				FacePair&	AdjacentFaces = SharedEdges.Add( Key );
				AdjacentFaces.F0 = pTargetFace;
				AdjacentFaces.F1 = NULL;
			} else {
				// Second face! Build adjacency...
				ASSERT( ExistingValue->F1 == NULL, "Non-manifold mesh: more than 2 faces share the same edge!" );
				ExistingValue->F1 = pTargetFace;

				pTargetFace->pAdjacent[EdgeIndex] = ExistingValue->F0;
				ExistingValue->F0->SetAdjacency( Key.V1, Key.V0, pTargetFace );
			}
		}
	}
}

bool	SHProbeNetwork::MeshWithAdjacency::Primitive::PropagateProbeInfluences( U32 _PassIndex ) {
	bool	stillSpreading = false;
	Face*	pFace = m_pFaces;
	for ( int FaceIndex=0; FaceIndex < m_FacesCount; FaceIndex++, pFace++ ) {
		stillSpreading |= pFace->RecursePropagateProbeInfluences( _PassIndex );
	}

	return stillSpreading;
}

void	SHProbeNetwork::MeshWithAdjacency::Primitive::AssignNearestProbe( U32 _ProbesCount, const float3* _LocalProbePositions ) {
	Face*	pFace = m_pFaces;
	for ( int FaceIndex=0; FaceIndex < m_FacesCount; FaceIndex++, pFace++ ) {
		if ( pFace->pProbeInfluence->ProbeID != ~0UL )
			continue;

		float	NearestProbeSqDistance = FLT_MAX;
		U32		NearestProbeIndex = ~0UL;
		for ( U32 ProbeIndex=0; ProbeIndex < _ProbesCount; ProbeIndex++ ) {
			float	SqDistance = (_LocalProbePositions[ProbeIndex] - pFace->P).LengthSq();
			if ( SqDistance < NearestProbeSqDistance ) {
				NearestProbeSqDistance = SqDistance;
				NearestProbeIndex = ProbeIndex;
			}
		}
		pFace->pProbeInfluence->ProbeID = NearestProbeIndex;
	}
}

void	SHProbeNetwork::MeshWithAdjacency::Primitive::RedistributeProbeIDs2Vertices( ProbeInfluence** _ppProbeInfluences ) const {
	Face*	pFace = m_pFaces;
	for ( int FaceIndex=0; FaceIndex < m_FacesCount; FaceIndex++, pFace++ ) {
		ProbeInfluence*	pFaceProbeInfluence = pFace->pProbeInfluence;

		for ( int FaceVertexIndex=0; FaceVertexIndex < 3; FaceVertexIndex++ ) {
			U32					VertexIndex = pFace->V[FaceVertexIndex];
			ProbeInfluence*&	pVertexProbeInfluence = _ppProbeInfluences[VertexIndex];
			if ( pVertexProbeInfluence == NULL || pVertexProbeInfluence->Influence < pFaceProbeInfluence->Influence ) {
				pVertexProbeInfluence = pFaceProbeInfluence;	// This face provides a better influence for the vertex!
			}
		}
	}
}

bool	SHProbeNetwork::MeshWithAdjacency::Primitive::Face::RecursePropagateProbeInfluences( U32 _PassIndex ) {
	if ( LastVisitIndex == _PassIndex )
		return false;
	LastVisitIndex = _PassIndex;	// Visited!
	
	bool	spreading = false;

	for ( int EdgeIndex=0; EdgeIndex < 3; EdgeIndex++ ) {
		if ( pAdjacent[EdgeIndex] == NULL || pAdjacent[EdgeIndex]->LastVisitIndex == _PassIndex )
			continue;

		Face&	Adjacent = *pAdjacent[EdgeIndex];
		if ( Adjacent.pProbeInfluence->ProbeID == pProbeInfluence->ProbeID )
			continue;

		// Compute new influence
		static const float	DISTANCE_FALLOFF_FACTOR = -1.3862943611198906188344642429164f;			// ln( 0.25 ) so 1m away gets 1/4 the influence
		static const float	ANGULAR_FALLOFF_FACTOR = 0.5f * -0.30102999566398119521373889472449f;	// ln( 0.5 ) so a 90° face gets 1/2 the influence

		float	DistanceBetweenFaces = (Adjacent.P - P).Length();
		float	DotNormalBetweenFaces = Adjacent.N.Dot( N );
		float	FalloffDistance = expf( DISTANCE_FALLOFF_FACTOR * DistanceBetweenFaces );
		float	FalloffAngle = expf( ANGULAR_FALLOFF_FACTOR * (1.0f - DotNormalBetweenFaces) );
		double	Falloff = FalloffDistance * FalloffAngle;

		double	ReducedInfluence0 = 0.0f;
		if ( pProbeInfluence->ProbeID != ~0UL ) {
			ReducedInfluence0 = pProbeInfluence->Influence * Falloff;
		}
		double	ReducedInfluence1 = 0.0f;
		if ( Adjacent.pProbeInfluence->ProbeID != ~0UL ) {
			ReducedInfluence1 = Adjacent.pProbeInfluence->Influence * Falloff;
		}

		if ( ReducedInfluence0 > Adjacent.pProbeInfluence->Influence ) {
			// Spread from face to adjacent face
			Adjacent.pProbeInfluence->Influence = ReducedInfluence0;
			Adjacent.pProbeInfluence->ProbeID = pProbeInfluence->ProbeID;
			spreading = true;
		} else if ( ReducedInfluence1 > pProbeInfluence->Influence ) {
			// Spread from adjacent face to this face
			pProbeInfluence->Influence = ReducedInfluence1;
			pProbeInfluence->ProbeID = Adjacent.pProbeInfluence->ProbeID;
			spreading = true;
		}
	}

	return spreading;
}

void	SHProbeNetwork::BuildProbeInfluenceVertexStream( Scene& _Scene, const char* _pPathToStreamFile ) {

	//////////////////////////////////////////////////////////////////////////
	// Start by building adjacency structures between primitives' faces
	List< MeshWithAdjacency >	Meshes;
	Meshes.Init( _Scene.m_MeshesCount );

	class MeshVisitor : public Scene::IVisitor {
	public:
		List< MeshWithAdjacency >*	m_Meshes;
		ProbeInfluence*				m_ProbeInfluencePerFace;
		U32							m_TotalFacesCount;
		U32							m_TotalVerticesCount;
		virtual void	HandleNode( Scene::Node& _Node ) override {
			if ( _Node.m_Type != Scene::Node::MESH )
				return;
			
			Scene::Mesh&		SourceMesh = (Scene::Mesh&) _Node;
			MeshWithAdjacency&	TargetMesh = m_Meshes->Append();
			TargetMesh.Build( SourceMesh, m_ProbeInfluencePerFace + m_TotalFacesCount );

			// Accumulate vertices/faces count
			for ( int PrimitiveIndex=0; PrimitiveIndex < SourceMesh.m_PrimitivesCount; PrimitiveIndex++ ) {
				Scene::Mesh::Primitive&	P = SourceMesh.m_pPrimitives[PrimitiveIndex];
				m_TotalFacesCount += P.m_FacesCount;
				m_TotalVerticesCount += P.m_VerticesCount;
			}
		}
	} visitor;
	visitor.m_TotalFacesCount = 0;
	visitor.m_TotalVerticesCount = 0;
	visitor.m_Meshes = &Meshes;
	visitor.m_ProbeInfluencePerFace = &m_ProbeInfluencePerFace[0];
	_Scene.ForEach( visitor );

	//////////////////////////////////////////////////////////////////////////
	// Propagate best probe indices by adjacency
	U32		PassIndex = 0;
	bool	stilSpreading = true;
	while ( stilSpreading ) {

		stilSpreading = false;
		for ( int MeshIndex=0; MeshIndex < Meshes.GetCount(); MeshIndex++ ) {
			MeshWithAdjacency&	M = Meshes[MeshIndex];
			stilSpreading |= M.PropagateProbeInfluences( ++PassIndex );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Assign nearest probes to faces without influence (isolated faces)
	for ( int MeshIndex=0; MeshIndex < Meshes.GetCount(); MeshIndex++ ) {
		MeshWithAdjacency&	M = Meshes[MeshIndex];
		M.AssignNearestProbe( m_ProbesCount, m_pProbes );
	}

	//////////////////////////////////////////////////////////////////////////
	// Redistribute to vertices, choosing the best probe influence each time
	ProbeInfluence**	pProbeInfluencePerVertex = new ProbeInfluence*[visitor.m_TotalVerticesCount];
	memset( pProbeInfluencePerVertex, 0, visitor.m_TotalVerticesCount*sizeof(ProbeInfluence*) );

	{
		ProbeInfluence**	ppInfluence = pProbeInfluencePerVertex;
		for ( int MeshIndex=0; MeshIndex < Meshes.GetCount(); MeshIndex++ ) {
			MeshWithAdjacency&	M = Meshes[MeshIndex];
			M.RedistributeProbeIDs2Vertices( ppInfluence );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Load the vertex stream containing U32-packed probe IDs for each vertex
	{
		char	pTemp[1024];
		sprintf_s( pTemp, "%sScene.vertexStream.U16", _pPathToStreamFile );

		FILE*	pFile = NULL;
		fopen_s( &pFile, pTemp, "wb" );
		ASSERT( pFile != NULL, "Can't create vertex stream for probe IDs!" );

		fwrite( &visitor.m_TotalVerticesCount, sizeof(U32), 1, pFile );

		ProbeInfluence**	ppInfluence = pProbeInfluencePerVertex;
		for ( U32 VertexIndex=0; VertexIndex < visitor.m_TotalVerticesCount; VertexIndex++, ppInfluence++ ) {
			ASSERT( ppInfluence != NULL, "Yikes!" );
			fwrite( &(*ppInfluence)->ProbeID, sizeof(U32), 1, pFile );
		}

		fclose( pFile );
	}

	SAFE_DELETE_ARRAY( pProbeInfluencePerVertex );
}

static void	CopyProbeNetworkConnection( int _EntryIndex, SHProbeNetwork::RuntimeProbeNetworkInfos& _Value, void* _pUserData );

void	SHProbeNetwork::LoadProbes( const char* _pPathToProbes, IQueryMaterial& _QueryMaterial, const float3& _SceneBBoxMin, const float3& _SceneBBoxMax ) {

	FILE*	pFile = NULL;
	char	pTemp[1024];

	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		// Read numbered probe
		sprintf_s( pTemp, "%s/Probe%02d.probeset", _pPathToProbes, ProbeIndex );
		fopen_s( &pFile, pTemp, "rb" );
		if ( pFile == NULL ) {
			// Not ready yet (happens for first time computation!)
			memset( Probe.pSamples, 0, SHProbeEncoder::PROBE_SAMPLES_COUNT*sizeof(SHProbe::Sample) );
			Probe.EmissiveSurfacesCount = 0;
			continue;
		}
//		ASSERT( pFile != NULL, "Can't find probeset test file!" );

		// Read the boundary infos
		fread_s( &Probe.MeanDistance, sizeof(Probe.MeanDistance), sizeof(float), 1, pFile );
		fread_s( &Probe.MeanHarmonicDistance, sizeof(Probe.MeanHarmonicDistance), sizeof(float), 1, pFile );
		fread_s( &Probe.MinDistance, sizeof(Probe.MinDistance), sizeof(float), 1, pFile );
		fread_s( &Probe.MaxDistance, sizeof(Probe.MaxDistance), sizeof(float), 1, pFile );

		fread_s( &Probe.BBoxMin.x, sizeof(Probe.BBoxMin.x), sizeof(float), 1, pFile );
		fread_s( &Probe.BBoxMin.y, sizeof(Probe.BBoxMin.y), sizeof(float), 1, pFile );
		fread_s( &Probe.BBoxMin.z, sizeof(Probe.BBoxMin.z), sizeof(float), 1, pFile );
		fread_s( &Probe.BBoxMax.x, sizeof(Probe.BBoxMax.x), sizeof(float), 1, pFile );
		fread_s( &Probe.BBoxMax.y, sizeof(Probe.BBoxMax.y), sizeof(float), 1, pFile );
		fread_s( &Probe.BBoxMax.z, sizeof(Probe.BBoxMax.z), sizeof(float), 1, pFile );

		// Read static SH
		for ( int i=0; i < 9; i++ ) {
			fread_s( &Probe.pSHStaticLighting[i].x, sizeof(Probe.pSHStaticLighting[i].x), sizeof(float), 1, pFile );
			fread_s( &Probe.pSHStaticLighting[i].y, sizeof(Probe.pSHStaticLighting[i].y), sizeof(float), 1, pFile );
			fread_s( &Probe.pSHStaticLighting[i].z, sizeof(Probe.pSHStaticLighting[i].z), sizeof(float), 1, pFile );
		}

		// Read occlusion SH
		for ( int i=0; i < 9; i++ )
			fread_s( &Probe.pSHOcclusion[i], sizeof(Probe.pSHOcclusion[i]), sizeof(float), 1, pFile );

		// Read the samples
		for ( U32 SampleIndex=0; SampleIndex < SHProbeEncoder::PROBE_SAMPLES_COUNT; SampleIndex++ ) {
			SHProbe::Sample&	S = Probe.pSamples[SampleIndex];

			// Read position, normal, albedo
			fread_s( &S.Position.x, sizeof(S.Position.x), sizeof(float), 1, pFile );
			fread_s( &S.Position.y, sizeof(S.Position.y), sizeof(float), 1, pFile );
			fread_s( &S.Position.z, sizeof(S.Position.z), sizeof(float), 1, pFile );

			fread_s( &S.Normal.x, sizeof(S.Normal.x), sizeof(float), 1, pFile );
			fread_s( &S.Normal.y, sizeof(S.Normal.y), sizeof(float), 1, pFile );
			fread_s( &S.Normal.z, sizeof(S.Normal.z), sizeof(float), 1, pFile );

			fread_s( &S.Tangent.x, sizeof(S.Tangent.x), sizeof(float), 1, pFile );
			fread_s( &S.Tangent.y, sizeof(S.Tangent.y), sizeof(float), 1, pFile );
			fread_s( &S.Tangent.z, sizeof(S.Tangent.z), sizeof(float), 1, pFile );

			fread_s( &S.BiTangent.x, sizeof(S.BiTangent.x), sizeof(float), 1, pFile );
			fread_s( &S.BiTangent.y, sizeof(S.BiTangent.y), sizeof(float), 1, pFile );
			fread_s( &S.BiTangent.z, sizeof(S.BiTangent.z), sizeof(float), 1, pFile );

			fread_s( &S.Radius, sizeof(S.Radius), sizeof(float), 1, pFile );

			fread_s( &S.Albedo.x, sizeof(S.Albedo.x), sizeof(float), 1, pFile );
			fread_s( &S.Albedo.y, sizeof(S.Albedo.y), sizeof(float), 1, pFile );
			fread_s( &S.Albedo.z, sizeof(S.Albedo.z), sizeof(float), 1, pFile );

			fread_s( &S.F0.x, sizeof(S.F0.x), sizeof(float), 1, pFile );
			fread_s( &S.F0.y, sizeof(S.F0.y), sizeof(float), 1, pFile );
			fread_s( &S.F0.z, sizeof(S.F0.z), sizeof(float), 1, pFile );

			fread_s( &S.SHFactor, sizeof(S.SHFactor), sizeof(float), 1, pFile );
			
// No need: can be regenerated at runtime from normal direction
// 			// Read SH coefficients
// 			for ( int i=0; i < 9; i++ ) {
// 				fread_s( &S.pSHBounce[i], sizeof(S.pSHBounce[i]), sizeof(float), 1, pFile );
// 			}

			// Transform sample's position/normal by probe's LOCAL=>WORLD
			S.Position = float3( Probe.pSceneProbe->m_Local2World.GetRow(3) ) + S.Position;
// 			NjFloat3	wsSetNormal = Set.Normal;
// 			NjFloat3	wsSetTangent = Set.Tangent;
// 			NjFloat3	wsSetBiTangent = Set.BiTangent;
// TODO: Handle non-identity matrices! Let's go fast for now...
// ARGH! That also means possibly rotating the SH!
// Let's just force the probes to be axis-aligned, shall we??? :) (lazy man talking) (no, seriously, it makes sense after all)
		}

		// Read the amount of emissive surfaces
		U32	EmissiveSurfacesCount;
		fread_s( &EmissiveSurfacesCount, sizeof(EmissiveSurfacesCount), sizeof(U32), 1, pFile );
		Probe.EmissiveSurfacesCount = MIN( SHProbeEncoder::MAX_PROBE_EMISSIVE_SURFACES, EmissiveSurfacesCount );	// Don't read more than we can chew!

		// Read the surfaces
		SHProbe::EmissiveSurface	DummyEmissiveSurface;
		for ( U32 SampleIndex=0; SampleIndex < EmissiveSurfacesCount; SampleIndex++ ) {
			SHProbe::EmissiveSurface&	S = SampleIndex < Probe.EmissiveSurfacesCount ? Probe.pEmissiveSurfaces[SampleIndex] : DummyEmissiveSurface;	// We load into a useless surface if out of range...

			// Read position, normal
			fread_s( &S.Position.x, sizeof(S.Position.x), sizeof(float), 1, pFile );
			fread_s( &S.Position.y, sizeof(S.Position.y), sizeof(float), 1, pFile );
			fread_s( &S.Position.z, sizeof(S.Position.z), sizeof(float), 1, pFile );

			fread_s( &S.Normal.x, sizeof(S.Normal.x), sizeof(float), 1, pFile );
			fread_s( &S.Normal.y, sizeof(S.Normal.y), sizeof(float), 1, pFile );
			fread_s( &S.Normal.z, sizeof(S.Normal.z), sizeof(float), 1, pFile );

			fread_s( &S.Tangent.x, sizeof(S.Tangent.x), sizeof(float), 1, pFile );
			fread_s( &S.Tangent.y, sizeof(S.Tangent.y), sizeof(float), 1, pFile );
			fread_s( &S.Tangent.z, sizeof(S.Tangent.z), sizeof(float), 1, pFile );

			fread_s( &S.BiTangent.x, sizeof(S.BiTangent.x), sizeof(float), 1, pFile );
			fread_s( &S.BiTangent.y, sizeof(S.BiTangent.y), sizeof(float), 1, pFile );
			fread_s( &S.BiTangent.z, sizeof(S.BiTangent.z), sizeof(float), 1, pFile );

			// Read emissive material ID
			U32	EmissiveMatID;
			fread_s( &EmissiveMatID, sizeof(EmissiveMatID), sizeof(U32), 1, pFile );
			S.pEmissiveMaterial = _QueryMaterial( EmissiveMatID );

			// Read SH coefficients
			for ( int i=0; i < 9; i++ )
				fread_s( &S.pSHEmissive[i], sizeof(S.pSHEmissive[i]), sizeof(float), 1, pFile );
		}

		// Read the amount of neighbor probes & distance infos
		U32	NeighborProbesCount;
		fread_s( &NeighborProbesCount, sizeof(NeighborProbesCount), sizeof(U32), 1, pFile );
		NeighborProbesCount = MIN( MAX_PROBE_NEIGHBORS, NeighborProbesCount );

		fread_s( &Probe.NearestProbeDistance, sizeof(Probe.NearestProbeDistance), sizeof(float), 1, pFile );
		fread_s( &Probe.FarthestProbeDistance, sizeof(Probe.FarthestProbeDistance), sizeof(float), 1, pFile );

		for ( U32 NeighborProbeIndex=0; NeighborProbeIndex < NeighborProbesCount; NeighborProbeIndex++ ) {
			fread_s( &Probe.pNeighborProbeInfos[NeighborProbeIndex].ProbeID, sizeof(Probe.pNeighborProbeInfos[NeighborProbeIndex].ProbeID), sizeof(U32), 1, pFile );
			fread_s( &Probe.pNeighborProbeInfos[NeighborProbeIndex].Distance, sizeof(Probe.pNeighborProbeInfos[NeighborProbeIndex].Distance), sizeof(float), 1, pFile );
			fread_s( &Probe.pNeighborProbeInfos[NeighborProbeIndex].SolidAngle, sizeof(Probe.pNeighborProbeInfos[NeighborProbeIndex].SolidAngle), sizeof(float), 1, pFile );
			fread_s( &Probe.pNeighborProbeInfos[NeighborProbeIndex].Direction.x, sizeof(Probe.pNeighborProbeInfos[NeighborProbeIndex].Direction), sizeof(float3), 1, pFile );
			for ( int i=0; i < 9; i++ )
				fread_s( &Probe.pNeighborProbeInfos[NeighborProbeIndex].SH[i], sizeof(Probe.pNeighborProbeInfos[NeighborProbeIndex].SH[i]), sizeof(float), 1, pFile );
		}
		for ( U32 NeighborProbeIndex=NeighborProbesCount; NeighborProbeIndex < MAX_PROBE_NEIGHBORS; NeighborProbeIndex++ )
			Probe.pNeighborProbeInfos[NeighborProbeIndex].ProbeID = ~0;	// Invalid ID

		fclose( pFile );
	}


	//////////////////////////////////////////////////////////////////////////
	// Allocate runtime probes structured buffer & copy static infos
	m_pSB_RuntimeProbes = new SB<RuntimeProbe>( *m_pDevice, m_ProbesCount, true );
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		m_pSB_RuntimeProbes->m[ProbeIndex].Position = Probe.pSceneProbe->m_Local2World.GetRow( 3 );
		m_pSB_RuntimeProbes->m[ProbeIndex].Radius = Probe.MaxDistance;
//		m_pSB_RuntimeProbes->m[ProbeIndex].Radius = Probe.MeanDistance;


// 		ASSERT( Probe.pNeighborProbeInfos[0].ProbeID == ~0UL || Probe.pNeighborProbeInfos[0].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
// 		ASSERT( Probe.pNeighborProbeInfos[1].ProbeID == ~0UL || Probe.pNeighborProbeInfos[1].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
// 		ASSERT( Probe.pNeighborProbeInfos[2].ProbeID == ~0UL || Probe.pNeighborProbeInfos[2].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
// 		ASSERT( Probe.pNeighborProbeInfos[3].ProbeID == ~0UL || Probe.pNeighborProbeInfos[3].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[0] = Probe.pNeighborProbeInfos[0].ProbeID;
// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[1] = Probe.pNeighborProbeInfos[1].ProbeID;
// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[2] = Probe.pNeighborProbeInfos[2].ProbeID;
// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[3] = Probe.pNeighborProbeInfos[3].ProbeID;
	}
	m_pSB_RuntimeProbes->Write();


	m_ppSB_RuntimeSHStatic[0] = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, true );
	m_ppSB_RuntimeSHStatic[1] = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, true );
	m_pSB_RuntimeSHAmbient = new SB<SHCoeffs1>( *m_pDevice, m_ProbesCount, true );
	m_pSB_RuntimeSHDynamic = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, false );
	m_pSB_RuntimeSHDynamicSun = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, false );
	m_pSB_RuntimeSHFinal = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, false );

	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		for ( int SHCoeffIndex=0; SHCoeffIndex < 9; SHCoeffIndex++ ) {
			m_ppSB_RuntimeSHStatic[0]->m[ProbeIndex].pSH[SHCoeffIndex] = Probe.pSHStaticLighting[SHCoeffIndex];
			m_ppSB_RuntimeSHStatic[1]->m[ProbeIndex].pSH[SHCoeffIndex] = Probe.pSHStaticLighting[SHCoeffIndex];

			m_pSB_RuntimeSHAmbient->m[ProbeIndex].pSH[SHCoeffIndex] = Probe.pSHOcclusion[SHCoeffIndex];

// 			m_pSB_RuntimeSHDynamic->m[ProbeIndex].pSH[SHCoeffIndex] = float3::Zero;
// 			m_pSB_RuntimeSHDynamicSun->m[ProbeIndex].pSH[SHCoeffIndex] = float3::Zero;
		}
	}

	m_ppSB_RuntimeSHStatic[0]->Write();
	m_ppSB_RuntimeSHStatic[1]->Write();
	m_pSB_RuntimeSHAmbient->Write();
// 	m_pSB_RuntimeSHDynamic->Write();
// 	m_pSB_RuntimeSHDynamicSun->Write();


	//////////////////////////////////////////////////////////////////////////
	// Load the vertex stream of probe IDs
	{
		char	pTemp[1024];
		sprintf_s( pTemp, "%sScene.vertexStream.U16", _pPathToProbes );

		FILE*	pFile = NULL;
		fopen_s( &pFile, pTemp, "rb" );
		ASSERT( pFile != NULL, "Vertex stream for probe IDs file not found!" );

		U32	VertexStreamProbeIDsLength;
		fread_s( &VertexStreamProbeIDsLength, sizeof(U32), sizeof(U32), 1, pFile );
		
		U32*	pVertexStreamProbeIDs = new U32[VertexStreamProbeIDsLength];
		fread_s( pVertexStreamProbeIDs, VertexStreamProbeIDsLength*sizeof(U32), sizeof(U32), VertexStreamProbeIDsLength, pFile );

		fclose( pFile );

		// Build the additional vertex stream
		m_pPrimProbeIDs = new Primitive( *m_pDevice, VertexStreamProbeIDsLength, pVertexStreamProbeIDs, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_POINTLIST, VertexFormatU32::DESCRIPTOR );
		SAFE_DELETE_ARRAY( pVertexStreamProbeIDs );
	}


	//////////////////////////////////////////////////////////////////////////
	// Build the probes' octree
	float	MaxDimension = (_SceneBBoxMax - _SceneBBoxMin).Max();

	m_ProbeOctree.Init( _SceneBBoxMin, MaxDimension, 4.0f, m_ProbesCount );
	int		MaxNodesCount = 0;
	int		TotalNodesCount = 0;
	U32		MaxNodesProbeIndex = ~0;
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		float3	Position = Probe.pSceneProbe->m_Local2World.GetRow( 3 );
		int	NodesCount = m_ProbeOctree.Append( Position, Probe.MaxDistance, &Probe );
		TotalNodesCount += NodesCount;
		if ( NodesCount > MaxNodesCount )
		{	// New probe with more octree nodes
			MaxNodesCount = NodesCount;
			MaxNodesProbeIndex = ProbeIndex;
		}
	}

	float	AverageNodesCount = float(TotalNodesCount) / m_ProbesCount;


	//////////////////////////////////////////////////////////////////////////
	// Build the probes network debug mesh
	Dictionary<RuntimeProbeNetworkInfos>	Connections;
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		for ( int NeighborProbeIndex=0; NeighborProbeIndex < MAX_PROBE_NEIGHBORS; NeighborProbeIndex++ ) {
			SHProbe::NeighborProbeInfos&	NeighborInfos = Probe.pNeighborProbeInfos[NeighborProbeIndex];
			if ( NeighborInfos.ProbeID == ~0 )
				continue;
			
			U32	Key = ProbeIndex < NeighborInfos.ProbeID ? ((ProbeIndex & 0xFFFF) | ((NeighborInfos.ProbeID & 0xFFFF) << 16)) : ((NeighborInfos.ProbeID & 0xFFFF) | ((ProbeIndex & 0xFFFF) << 16));

			RuntimeProbeNetworkInfos*	pConnection = Connections.Get( Key );
			if ( pConnection != NULL )
				continue;	// Already eastablished!
			
			pConnection = &Connections.Add( Key );
			pConnection->ProbeIDs[0] = ProbeIndex;
			pConnection->ProbeIDs[1] = NeighborInfos.ProbeID;
			pConnection->NeighborsSolidAngles.x = NeighborInfos.SolidAngle;
			pConnection->NeighborsSolidAngles.y = NeighborInfos.SolidAngle;	// By default, consider solid angles to be equal: both probes perceive the same amount of each other

			// Find us in the neighbor probe's neighborhood
			SHProbe&	NeighborProbe = m_pProbes[NeighborInfos.ProbeID];
			for ( int NeighborNeighborProbeIndex=0; NeighborNeighborProbeIndex < MAX_PROBE_NEIGHBORS; NeighborNeighborProbeIndex++ )
				if ( NeighborProbe.pNeighborProbeInfos[NeighborNeighborProbeIndex].ProbeID == ProbeIndex )
				{	// Found us!
					// Now we can get the exact solid angle!
					pConnection->NeighborsSolidAngles.y = NeighborProbe.pNeighborProbeInfos[NeighborNeighborProbeIndex].SolidAngle;
					break;
				}
		}
	}

	int	ProbeConnectionsCount = Connections.GetEntriesCount();
	if ( ProbeConnectionsCount == 0 )
		return;

	// Create the structured buffer from the flattened dictionary
	m_pSB_RuntimeProbeNetworkInfos = new SB<RuntimeProbeNetworkInfos>( *m_pDevice, ProbeConnectionsCount, true );
	Connections.ForEach( CopyProbeNetworkConnection, m_pSB_RuntimeProbeNetworkInfos->m );
// 	Connections.ForEach( [this]( int _EntryIndex, RuntimeProbeNetworkInfos& _Value, void* _pUserData )
// 		{
// 			RuntimeProbeNetworkInfos*	_pTarget = (RuntimeProbeNetworkInfos*) _pUserData;
// 			memcpy_s( &_pTarget[_EntryIndex], sizeof(RuntimeProbeNetworkInfos), &_Value, sizeof(RuntimeProbeNetworkInfos) );
// 		},
// 		m_pSB_RuntimeProbeNetworkInfos->m );

	m_pSB_RuntimeProbeNetworkInfos->Write();
	m_pSB_RuntimeProbeNetworkInfos->SetInput( 16 );
}

static void	CopyProbeNetworkConnection( int _EntryIndex, SHProbeNetwork::RuntimeProbeNetworkInfos& _Value, void* _pUserData )
{
	SHProbeNetwork::RuntimeProbeNetworkInfos*	_pTarget = (SHProbeNetwork::RuntimeProbeNetworkInfos*) _pUserData;
	memcpy_s( &_pTarget[_EntryIndex], sizeof(SHProbeNetwork::RuntimeProbeNetworkInfos), &_Value, sizeof(SHProbeNetwork::RuntimeProbeNetworkInfos) );
}
