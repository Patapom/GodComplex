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
	m_ProbeEncoder.m_pOwner = this;

	m_pDevice = &_Device;
	m_pScreenQuad = &_ScreenQuad;

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Probe = new CB<CBProbe>( _Device, 10 );
	m_pCB_UpdateProbes = new CB<CBUpdateProbes>( _Device, 10 );

	//////////////////////////////////////////////////////////////////////////
	// Create the probes structured buffers
	m_pSB_RuntimeProbes = NULL;
	m_pSB_ProbeNeighbors = NULL;
	m_pSB_RuntimeProbeNetworkInfos = NULL;

	m_pSB_RuntimeProbeUpdateInfos = new SB<RuntimeProbeUpdateInfo>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME, true );
	m_pSB_RuntimeProbeSamples = new SB<RuntimeProbeUpdateSampleInfo>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME*SHProbe::SAMPLES_COUNT, true );
	m_pSB_RuntimeProbeEmissiveSurfaces = new SB<RuntimeProbeUpdateEmissiveSurfaceInfo>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME*SHProbe::MAX_EMISSIVE_SURFACES, true );

	// Create the static SH coefficients for each sample
	m_pSB_RuntimeProbeSamplesSH = new SB<SHCoeffs1>( *m_pDevice, SHProbe::SAMPLES_COUNT, true );
	for ( int SampleIndex=0; SampleIndex < SHProbe::SAMPLES_COUNT; SampleIndex++ ) {
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

	delete m_pSB_ProbeNeighbors;

	delete m_pCB_UpdateProbes;
	delete m_pCB_Probe;
}

void	SHProbeNetwork::PreAllocateProbes( int _ProbesCount ) {
	m_MaxProbesCount = _ProbesCount;
	m_pProbes = new SHProbe[m_MaxProbesCount];
}

void	SHProbeNetwork::AddProbe( Scene::Probe& _Probe ) {
	ASSERT( m_ProbesCount < m_MaxProbesCount, "Probes count out of range!" );
	m_pProbes[m_ProbesCount].m_ProbeID = m_ProbesCount;
	m_pProbes[m_ProbesCount].m_pSceneProbe = &_Probe;
	m_pProbes[m_ProbesCount].m_wsPosition = float3( _Probe.m_Local2World.GetRow(3) );	// Cache probe position as we're going to use it a lot!
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
		ProbeUpdateInfos.EmissiveSurfacesCount = Probe.m_EmissiveSurfacesCount;

		// Copy neighbor info
		ProbeUpdateInfos.NeighborProbeIDs[0] = Probe.m_NeighborProbes[0].ProbeID;
		ProbeUpdateInfos.NeighborProbeIDs[1] = Probe.m_NeighborProbes[1].ProbeID;
		ProbeUpdateInfos.NeighborProbeIDs[2] = Probe.m_NeighborProbes[2].ProbeID;
		ProbeUpdateInfos.NeighborProbeIDs[3] = Probe.m_NeighborProbes[3].ProbeID;
		for( int i=0; i < 9; i++ ) {
			ProbeUpdateInfos.SHConvolution[i].x = Probe.m_NeighborProbes[0].SH[i];
			ProbeUpdateInfos.SHConvolution[i].y = Probe.m_NeighborProbes[1].SH[i];
			ProbeUpdateInfos.SHConvolution[i].z = Probe.m_NeighborProbes[2].SH[i];
			ProbeUpdateInfos.SHConvolution[i].w = Probe.m_NeighborProbes[3].SH[i];
		}

		// Fill the samples update infos
		SHProbe::Sample*	pSample = Probe.m_pSamples;
		for ( U32 SampleIndex=0; SampleIndex < SHProbe::SAMPLES_COUNT; SampleIndex++, pSample++, pSampleUpdateInfos++ ) {
			pSampleUpdateInfos->Position = pSample->Position;
			pSampleUpdateInfos->Normal = pSample->Normal;
			pSampleUpdateInfos->Albedo = pSample->SHFactor * pSample->Albedo;
			pSampleUpdateInfos->Radius = pSample->Radius;
		}

		// Fill the emissive surface update infos
		for ( U32 EmissiveSurfaceIndex=0; EmissiveSurfaceIndex < Probe.m_EmissiveSurfacesCount; EmissiveSurfaceIndex++ ) {
			const SHProbe::EmissiveSurface&			EmissiveSurface = Probe.m_pEmissiveSurfaces[EmissiveSurfaceIndex];
			RuntimeProbeUpdateEmissiveSurfaceInfo&	EmissiveSetUpdateInfos = m_pSB_RuntimeProbeEmissiveSurfaces->m[TotalEmissiveSurfacesCount+EmissiveSurfaceIndex];

			ASSERT( _Parms.pQueryMaterial != NULL, "Invalid material query functor!" );
			Scene::Material*	pEmissiveMaterial = (*_Parms.pQueryMaterial)( EmissiveSurface.MaterialID );
			ASSERT( pEmissiveMaterial != NULL, "Invalid emissive material!" );
			EmissiveSetUpdateInfos.EmissiveColor = pEmissiveMaterial->m_EmissiveColor;

			memcpy_s( EmissiveSetUpdateInfos.SH, sizeof(EmissiveSetUpdateInfos.SH), EmissiveSurface.pSH, 9*sizeof(float) );
		}

		TotalEmissiveSurfacesCount += Probe.m_EmissiveSurfacesCount;
	}

	// =========================================================
	// Do the update!
	{
		USING_COMPUTESHADER_START( *m_pCSUpdateProbeDynamicSH )

		m_pSB_RuntimeSHFinal->SetInput( 8, true );	// Feed last frame's SH for neighbor bounce

		m_pSB_RuntimeProbeUpdateInfos->Write( ProbeUpdatesCount );
		m_pSB_RuntimeProbeUpdateInfos->SetInput( 10 );

		m_pSB_RuntimeProbeSamples->Write( ProbeUpdatesCount * SHProbe::SAMPLES_COUNT );
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
	m_pSB_RuntimeProbes->SetInput( 7, true );
	m_pSB_RuntimeSHFinal->SetInput( 8, true );
	m_pSB_ProbeNeighbors->SetInput( 9, true );
}

U32	SHProbeNetwork::GetNearestProbe( const float3& _wsPosition ) const {
	float					ProbeDistance;
	const SHProbe* const*	ppNearestProbe = m_ProbeOctree.FetchNearest( _wsPosition, ProbeDistance );
	U32						probeID = ppNearestProbe != NULL ? (*ppNearestProbe)->m_ProbeID : 0xFFFFFFFFU;
	return probeID;
}

void	SHProbeNetwork::PreComputeProbes( const char* _pPathToProbes, IRenderSceneDelegate& _RenderScene, Scene& _Scene, U32 _TotalFacesCount ) {

	const float		Z_INFINITY = 1e6f;
	const float		Z_INFINITY_TEST = 0.99f * Z_INFINITY;

	if ( m_pRTCubeMap == NULL ) {
		m_pRTCubeMap = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, -6 * 3, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );				// Will contain albedo (cube 0) + (normal + distance) (cube 1) + (static lighting + emissive surface index) (cube 2)
	}
	Texture2D*	pRTCubeMapStaging = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, -6 * 3, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );

	Texture2D*	pRTCubeMapNeighbors = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain Neighbor Probe IDs (cube 3)
	Texture2D*	pRTCubeMapNeighborsStaging = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );

	Texture2D*	pRTCubeMapDepth = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR, 6 );
	Texture2D*	pRTCubeMapDepthCopy = new Texture2D( *m_pDevice, SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR, 6 );


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

		m_pCB_Probe->m.CurrentProbePosition = Probe.m_wsPosition;

		// Clear cube maps
		m_pDevice->ClearRenderTarget( *m_pRTCubeMap->GetRTV( 0, 6*0, 6 ), float4::Zero );
		m_pDevice->ClearRenderTarget( *m_pRTCubeMap->GetRTV( 0, 6*1, 6 ), float4( 0, 0, 0, Z_INFINITY ) );	// We clear distance to infinity here

		float4	Bisou = float4::Zero;
		((U32&) Bisou.w) = 0xFFFFFFFFUL;
		m_pDevice->ClearRenderTarget( *m_pRTCubeMap->GetRTV( 0, 6*2, 6 ), Bisou );	// Clear emissive surface ID to -1 (invalid) and static color to 0

		// Setup probe WORLD -> LOCAL transform
		float4x4	ProbeLocal2World = float4x4::Identity;
					ProbeLocal2World.SetRow( 3, Probe.m_wsPosition, 1 );
		float4x4	ProbeWorld2Local = ProbeLocal2World.Inverse();

		// Render the 6 faces
		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
			// Update cube map face camera transform
			float4x4	World2Proj = ProbeWorld2Local * SideWorld2Proj[CubeFaceIndex];

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;
			pCBCubeMapCamera->m.World2Proj = World2Proj;
			pCBCubeMapCamera->UpdateData();

			ID3D11DepthStencilView*	pDSV = pRTCubeMapDepth->GetDSV( CubeFaceIndex, 1 );

			m_pDevice->ClearDepthStencil( *pDSV, 1.0f, 0, true, false );

			//////////////////////////////////////////////////////////////////////////
			// 1] Render Albedo + Normal + Distance + Static lit + Emissive Mat ID
			m_pDevice->SetStates( m_pDevice->m_pRS_CullFront, m_pDevice->m_pDS_ReadWriteLess, m_pDevice->m_pBS_Disabled );

			ID3D11RenderTargetView*	ppViews[3] = {
				m_pRTCubeMap->GetRTV( 0, 6*0+CubeFaceIndex, 1 ),
				m_pRTCubeMap->GetRTV( 0, 6*1+CubeFaceIndex, 1 ),
				m_pRTCubeMap->GetRTV( 0, 6*2+CubeFaceIndex, 1 )
			};
			m_pDevice->SetRenderTargets( SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, 3, ppViews, pDSV );

			// Render scene
			_RenderScene( *m_pMatRenderCubeMap );
		}

		//////////////////////////////////////////////////////////////////////////
		// 2] Render neighborhood for each probe
		// The idea here is simply to build a 3D voronoi cell by splatting the planes passing through all other probes
		//	with their normal set to the direction from the other probe to the current probe.
		// Splatting a new plane and accounting for the depth buffer will only let visible pixels from the plane show up
		//	and write the ID of the probe.
		//
		// Reading back the cube map will indicate the solid angle perceived by each probe to each of its neighbors
		//	so we can create a linked list of neighbor probes, of their visibilities and solid angle
		//
		pRTCubeMapDepthCopy->CopyFrom( *pRTCubeMapDepth );

		((U32&) Bisou.x) = 0xFFFFFFFFUL;
		m_pDevice->ClearRenderTarget( *pRTCubeMapNeighbors->GetRTV( 0, 0, 6 ), Bisou );	// Clear probe ID to -1 (invalid)

		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
			// Update cube map face camera transform
			float4x4	World2Proj = ProbeWorld2Local * SideWorld2Proj[CubeFaceIndex];

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;
			pCBCubeMapCamera->m.World2Proj = World2Proj;
			pCBCubeMapCamera->UpdateData();

			// Render
			m_pDevice->SetStates( m_pDevice->m_pRS_CullNone, m_pDevice->m_pDS_ReadWriteLess, m_pDevice->m_pBS_Disabled );
			m_pDevice->SetRenderTarget( SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, *pRTCubeMapNeighbors->GetRTV( 0, CubeFaceIndex, 1 ), pRTCubeMapDepthCopy->GetDSV( CubeFaceIndex, 1 ) );

			USING_MATERIAL_START( *m_pMatRenderNeighborProbe )

			for ( U32 NeighborProbeIndex=0; NeighborProbeIndex < m_ProbesCount; NeighborProbeIndex++ )
				if ( NeighborProbeIndex != ProbeIndex ) {
					const float3&	NeighborProbePosition = m_pProbes[NeighborProbeIndex].m_wsPosition;

					float	Distance2Neighbor = (NeighborProbePosition - Probe.m_wsPosition).Length();

					m_pCB_Probe->m.NeighborProbeID = NeighborProbeIndex;
					m_pCB_Probe->m.NeighborProbePosition = NeighborProbePosition;
					m_pCB_Probe->m.QuadHalfSize = SATURATE( 0.125f * Distance2Neighbor );	// Will reduce when getting below 8 meters, otherwise renders a constant 2x2m² plane
					m_pCB_Probe->UpdateData();

					m_pScreenQuad->Render( M );
				}

			USING_MATERIAL_END
		}

		// Build neighbors list immediately since we need it for the Voronoï splatting right after
		pRTCubeMapNeighborsStaging->CopyFrom( *pRTCubeMapNeighbors );

		m_ProbeEncoder.BuildProbeNeighborIDs( *pRTCubeMapNeighborsStaging, Probe );


		//////////////////////////////////////////////////////////////////////////
		// 3] Build the Voronoï cells
		// This is without a doubt the most important structure to spread the probes' influence correctly:
		//	1) We render all connections STRICTLY VISIBLE neighbors by splatting large planes in the middle of the connection
		//		=> This will build the planes for the Voronoï cell
		//	2) We read back the neighbor IDs and store their planes into the Voronoï structure associated to the probe
		//		=> The probe's influence will be constrained within the strict influence of this cell
		//	3) We'll use the Voronoï cell's structure later when we'll spread the influence of the probe across the scene
		//
		pRTCubeMapDepthCopy->CopyFrom( *pRTCubeMapDepth );

		((U32&) Bisou.x) = 0xFFFFFFFFUL;
		m_pDevice->ClearRenderTarget( *pRTCubeMapNeighbors->GetRTV( 0, 0, 6 ), Bisou );	// Clear probe ID to -1 (invalid)

		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ ) {
			// Update cube map face camera transform
			float4x4	World2Proj = ProbeWorld2Local * SideWorld2Proj[CubeFaceIndex];

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;
			pCBCubeMapCamera->m.World2Proj = World2Proj;
			pCBCubeMapCamera->UpdateData();

			m_pDevice->SetStates( m_pDevice->m_pRS_CullNone, m_pDevice->m_pDS_ReadWriteLess, m_pDevice->m_pBS_Disabled );
			m_pDevice->SetRenderTarget( SHProbeEncoder::CUBE_MAP_SIZE, SHProbeEncoder::CUBE_MAP_SIZE, *pRTCubeMapNeighbors->GetRTV( 0, CubeFaceIndex, 1 ), pRTCubeMapDepthCopy->GetDSV( CubeFaceIndex, 1 ) );

			USING_MATERIAL_START( *m_pMatRenderNeighborProbe )

			for ( U32 NeighborProbeIndex=0; NeighborProbeIndex < U32(Probe.m_NeighborProbes.GetCount()); NeighborProbeIndex++ ) {
				const SHProbe::NeighborProbeInfo&	NP = Probe.m_NeighborProbes[NeighborProbeIndex];
				if ( NP.DirectlyVisible ) {
					const float3&	NeighborProbePosition = m_pProbes[NP.ProbeID].m_wsPosition;

					float3	CenterPosition = 0.5f * (Probe.m_wsPosition + NeighborProbePosition);
					float	Distance2Neighbor = (NeighborProbePosition - Probe.m_wsPosition).Length();

					m_pCB_Probe->m.NeighborProbeID = NP.ProbeID;
					m_pCB_Probe->m.NeighborProbePosition = CenterPosition;
					m_pCB_Probe->m.QuadHalfSize = min( 100.0f, 2.0f * Distance2Neighbor );
					m_pCB_Probe->UpdateData();

					m_pScreenQuad->Render( M );
				}
			}

			USING_MATERIAL_END
		}

		pRTCubeMapNeighborsStaging->CopyFrom( *pRTCubeMapNeighbors );

		m_ProbeEncoder.BuildProbeVoronoiCell( *pRTCubeMapNeighborsStaging, Probe );


		//////////////////////////////////////////////////////////////////////////
		// 3] Read back cube map and create the various dynamic samples & static SH coefficients
		pRTCubeMapStaging->CopyFrom( *m_pRTCubeMap );

#if 0	// Save to disk for processing by the external ProbeSHEncoder tool (not needed anymore since we're doing everything in here now)
		sprintf_s( pTemp, "%sProbe%02d.pom", _pPathToProbes, ProbeIndex );
		pRTCubeMapStaging->Save( pTemp );
#endif

		m_ProbeEncoder.EncodeProbeCubeMap( *pRTCubeMapStaging, Probe, _TotalFacesCount );

		// Save probe results
		{
			sprintf_s( pTemp, "%sProbe%02d.probeset", _pPathToProbes, ProbeIndex );

			FILE*	pFile = NULL;
			fopen_s( &pFile, pTemp, "wb" );
			ASSERT( pFile != NULL, "Locked!" );

			Probe.Save( pFile );

			fclose( pFile );
		}

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
				pCurrentInfluence->ProbeID = Probe.m_ProbeID;
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
	delete pRTCubeMapDepthCopy;
	delete pRTCubeMapDepth;
	delete pRTCubeMapNeighborsStaging;
	delete pRTCubeMapNeighbors;

//### Keep it for debugging!
// 	delete m_pRTCubeMap;
}

void	SHProbeNetwork::MeshWithAdjacency::Build( SHProbeNetwork& _Owner, const Scene::Mesh& _Mesh, ProbeInfluence* _pProbeInfluencePerFace ) {

	m_Local2World = _Mesh.m_Local2World;
	m_World2Local = _Mesh.m_Local2World.Inverse();

	m_PrimitivesCount = _Mesh.m_PrimitivesCount;
	m_pPrimitives = new Primitive[_Mesh.m_PrimitivesCount];

	int	FaceOffset = 0;
	for ( int PrimitiveIndex=0; PrimitiveIndex < _Mesh.m_PrimitivesCount; PrimitiveIndex++ ) {
		const Scene::Mesh::Primitive&	SourcePrim = _Mesh.m_pPrimitives[PrimitiveIndex];
		m_pPrimitives[PrimitiveIndex].Build( _Owner, m_Local2World, SourcePrim, _pProbeInfluencePerFace + FaceOffset );
		FaceOffset += SourcePrim.m_FacesCount;
	}
}

U32	SHProbeNetwork::MeshWithAdjacency::PropagateProbeInfluences( SHProbeNetwork& _Owner ) {
	U32	spreadsCount = false;
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ ) {
		Primitive&	P = m_pPrimitives[PrimitiveIndex];
		spreadsCount += P.PropagateProbeInfluences( _Owner );
	}

	return spreadsCount;
}

U32	SHProbeNetwork::MeshWithAdjacency::AssignNearestProbe( SHProbeNetwork& _Owner ) {
	U32	isolatedVerticesCount = 0;
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ ) {
		Primitive&	P = m_pPrimitives[PrimitiveIndex];
		isolatedVerticesCount += P.AssignNearestProbe( _Owner );
	}

	return isolatedVerticesCount;
}

void	SHProbeNetwork::MeshWithAdjacency::RedistributeProbeIDs2Vertices( ProbeInfluence const**& _ppProbeInfluences ) const {
	for ( int PrimitiveIndex=0; PrimitiveIndex < m_PrimitivesCount; PrimitiveIndex++ ) {
		Primitive&	P = m_pPrimitives[PrimitiveIndex];
		P.RedistributeProbeIDs2Vertices( _ppProbeInfluences );
		_ppProbeInfluences += P.m_Vertices.GetCount();	// Make the pointer advance as we're done with that primitive
	}
}

SHProbeNetwork::MeshWithAdjacency::Primitive::VertexLink*	SHProbeNetwork::MeshWithAdjacency::Primitive::ms_ppCells[64*64*64];

void	SHProbeNetwork::MeshWithAdjacency::Primitive::Build( SHProbeNetwork& _Owner, const float4x4& _Local2World, const Scene::Mesh::Primitive& _SourcePrimitive, ProbeInfluence* _pProbeInfluencePerFace ) {

	U32		VerticesCount = _SourcePrimitive.m_VerticesCount;
	U32		FacesCount = _SourcePrimitive.m_FacesCount;
	Scene::Mesh::Primitive::VF_P3N3G3B3T2*	pSourceVertices = (Scene::Mesh::Primitive::VF_P3N3G3B3T2*) _SourcePrimitive.m_pVertices;

	//////////////////////////////////////////////////////////////////////////
	// Create vertices world space positions and build the linked-list of vertices located in a 64x64x64 grid subdividing the local BBox of the primitive
	// (this will be used to quickly weld vertices together)
	m_Vertices.Init( VerticesCount );
	m_Vertices.SetCount( VerticesCount );

	float3	BBoxCellSize = _SourcePrimitive.m_LocalBBoxMax - _SourcePrimitive.m_LocalBBoxMin;
			BBoxCellSize = 1.01f * BBoxCellSize / 64.0f;
	float3	BBoxMin = 0.5f * (_SourcePrimitive.m_LocalBBoxMax + _SourcePrimitive.m_LocalBBoxMin)
					- 32.0f * BBoxCellSize;

	// Create free vertex cells
	m_VertexCells.Init( VerticesCount );
	m_VertexCells.SetCount( VerticesCount );

	// Fill up the 64x64x64 cells with lists of vertices inside each cell
	memset( ms_ppCells, 0, 64*64*64*sizeof(VertexLink*) );

	VertexLink*	pFreeCell = &m_VertexCells[0];
	Scene::Mesh::Primitive::VF_P3N3G3B3T2*	pSourceVertex = pSourceVertices;
	Vertex*									pTargetVertex = &m_Vertices[0];;
	for ( U32 VertexIndex=0; VertexIndex < VerticesCount; VertexIndex++, pSourceVertex++, pTargetVertex++, pFreeCell++ ) {
		float3&	Position = pSourceVertex->P;
		float3	CellPosition = (Position - BBoxMin) / BBoxCellSize;
		U32		iCellPositionX = U32( floorf( CellPosition.x ) );
		U32		iCellPositionY = U32( floorf( CellPosition.y ) );
		U32		iCellPositionZ = U32( floorf( CellPosition.z ) );

		// Link it into its nearest integer cell
		pFreeCell->pNext = ms_ppCells[iCellPositionX+64*(iCellPositionY+64*iCellPositionZ)];
		ms_ppCells[iCellPositionX+64*(iCellPositionY+64*iCellPositionZ)] = pFreeCell;

		pFreeCell->V = VertexIndex;

		// Build world space position to interrogate probes's Voronoï cells
		pTargetVertex->wsPosition = float4( pSourceVertex->P, 1.0f ) * _Local2World;
	}


	//////////////////////////////////////////////////////////////////////////
	// Build faces and assign seed per-vertex probe influences
	const U32*		pSourceFace = _SourcePrimitive.m_pFaces;
	ProbeInfluence* pFaceProbeInfluence = _pProbeInfluencePerFace;
	for ( U32 FaceIndex=0; FaceIndex < FacesCount; FaceIndex++, pFaceProbeInfluence++ ) {
		U32		V[3];
		V[0] = *pSourceFace++;
		V[1] = *pSourceFace++;
		V[2] = *pSourceFace++;

		// Distribute probe influence to face vertices
		if ( pFaceProbeInfluence->ProbeID != ~0UL ) {
			ASSERT( pFaceProbeInfluence->ProbeID < _Owner.m_ProbesCount, "Influencing probe index out of range!" );
			const SHProbe&	InfluencingProbe = _Owner.m_pProbes[pFaceProbeInfluence->ProbeID];

			// Distribute if vertex is inside Voronoï cell and existing influence is lower
			Vertex&				V0 = m_Vertices[V[0]];
			if ( InfluencingProbe.IsInsideVoronoiCell( V0.wsPosition ) && (V0.pInfluence == NULL || V0.pInfluence->Influence < pFaceProbeInfluence->Influence) ) {
				V0.pInfluence = pFaceProbeInfluence;
			}

			Vertex&				V1 = m_Vertices[V[1]];
			if ( InfluencingProbe.IsInsideVoronoiCell( V1.wsPosition ) && (V1.pInfluence == NULL || V1.pInfluence->Influence < pFaceProbeInfluence->Influence) ) {
				V1.pInfluence = pFaceProbeInfluence;
			}

			Vertex&				V2 = m_Vertices[V[2]];
			if ( InfluencingProbe.IsInsideVoronoiCell( V2.wsPosition ) && (V2.pInfluence == NULL || V2.pInfluence->Influence < pFaceProbeInfluence->Influence) ) {
				V2.pInfluence = pFaceProbeInfluence;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Build welded vertices structure and gather largest probe influences
	m_WeldedVertices.Init( VerticesCount );
	m_WeldedVertices.Clear();

	List<bool>	WeldedState( VerticesCount );
	WeldedState.SetCount( VerticesCount );
	memset( &WeldedState[0], 0, VerticesCount*sizeof(bool) );

	pSourceVertex = pSourceVertices;
	pTargetVertex = &m_Vertices[0];
	for ( U32 VertexIndex=0; VertexIndex < VerticesCount; VertexIndex++, pSourceVertex++, pTargetVertex++, pFreeCell++ ) {
		if ( WeldedState[VertexIndex] )
			continue;	// Already welded

		// Found a new vertex to weld
		U32				WeldedVertexIndex = m_WeldedVertices.GetCount();
		WeldedVertex&	NewWeldedVertex = m_WeldedVertices.Append();
		NewWeldedVertex.lsPosition = pSourceVertex->P;
		NewWeldedVertex.wsPosition = pTargetVertex->wsPosition;
		NewWeldedVertex.lsNormal = float3::Zero;
		NewWeldedVertex.SharingVerticesCount = 0;
		NewWeldedVertex.pSharingVertices = NULL;
		NewWeldedVertex.Influence.Influence = 0.0;
		NewWeldedVertex.Influence.ProbeID = ~0UL;	// No valid influence at the moment...

		float3	CellPosition = (pSourceVertex->P - BBoxMin) / BBoxCellSize;
		int		iCellPositionX = int( floorf( CellPosition.x ) );
		int		iCellPositionY = int( floorf( CellPosition.y ) );
		int		iCellPositionZ = int( floorf( CellPosition.z ) );

		// Examine neighbor cells as well (TODO: examine only if the vertex position is close to the cell's edge)
		for ( int Z=-1; Z <= 1; Z++ ) {
			int	iNeighborCellPositionZ = iCellPositionZ+Z;
			if ( iNeighborCellPositionZ < 0 || iNeighborCellPositionZ >= 64 )
				continue;
			for ( int Y=-1; Y <= 1; Y++ ) {
				int	iNeighborCellPositionY = iCellPositionY+Y;
				if ( iNeighborCellPositionY < 0 || iNeighborCellPositionY >= 64 )
					continue;
				for ( int X=-1; X <= 1; X++ ) {
					int	iNeighborCellPositionX = iCellPositionX+X;
					if ( iNeighborCellPositionX < 0 || iNeighborCellPositionX >= 64 )
						continue;

					U32			NeighborCellOffset = iNeighborCellPositionX+64*(iNeighborCellPositionY+64*iNeighborCellPositionZ);
					VertexLink*	pPreviousNeighborCell = NULL;
					VertexLink*	pNeighborCell = ms_ppCells[NeighborCellOffset];
					while ( pNeighborCell != NULL ) {
						float	SqDistance = (pSourceVertices[pNeighborCell->V].P - NewWeldedVertex.lsPosition).LengthSq();
						if ( SqDistance > 0.0001f ) {
							// More than 1cm appart... 
							pPreviousNeighborCell = pNeighborCell;
							pNeighborCell = pNeighborCell->pNext;
							continue;
						}

						// New vertex to weld! (NOTE: it's okay to weld ourselves)
						VertexLink*	pWeldedCell = pNeighborCell;

						// Link over that vertex: it's no longer part of the set of unwelded vertices
						pNeighborCell = pNeighborCell->pNext;
						if ( pPreviousNeighborCell != NULL )
							pPreviousNeighborCell->pNext = pNeighborCell;
						else 
							ms_ppCells[NeighborCellOffset] = pNeighborCell;

						// Link-in this vertex as a new welded vertex
						pWeldedCell->pNext = NewWeldedVertex.pSharingVertices;
						NewWeldedVertex.pSharingVertices = pWeldedCell;

						// Accumulate normals
						NewWeldedVertex.lsNormal = NewWeldedVertex.lsNormal + pSourceVertices[pWeldedCell->V].N;
						NewWeldedVertex.SharingVerticesCount++;

						// Assign new influence if valid...
						const ProbeInfluence*	pInfluence = m_Vertices[pWeldedCell->V].pInfluence;
						if ( pInfluence != NULL && pInfluence->Influence > NewWeldedVertex.Influence.Influence ) {
							NewWeldedVertex.Influence = *pInfluence;
						}

						// Assign welded vertex index to the original vertex
						m_Vertices[pWeldedCell->V].WeldedVertexIndex = WeldedVertexIndex;

						WeldedState[pWeldedCell->V] = true;	// Now welded!
					}
				}
			}
		}
	}

	// Normalize normals
	WeldedVertex*	pWeldedVertex = &m_WeldedVertices[0];
	for ( U32 WeldedVertexIndex=0; WeldedVertexIndex < U32(m_WeldedVertices.GetCount()); WeldedVertexIndex++, pWeldedVertex++ ) {
		pWeldedVertex->lsNormal.Normalize();
	}


	//////////////////////////////////////////////////////////////////////////
	// Build welded vertices adjacency
	pSourceFace = _SourcePrimitive.m_pFaces;
	for ( U32 FaceIndex=0; FaceIndex < FacesCount; FaceIndex++ ) {
		U32		V[3];
		V[0] = *pSourceFace++;
		V[1] = *pSourceFace++;
		V[2] = *pSourceFace++;

		U32		WV[3] = {
			m_Vertices[V[0]].WeldedVertexIndex,
			m_Vertices[V[1]].WeldedVertexIndex,
			m_Vertices[V[2]].WeldedVertexIndex };

		for ( U32 EdgeIndex=0; EdgeIndex < 3; EdgeIndex++ ) {
			U32				V0 = WV[EdgeIndex];
			U32				V1 = WV[(EdgeIndex+1)%3];
			WeldedVertex&	WV0 = m_WeldedVertices[V0];
			WeldedVertex&	WV1 = m_WeldedVertices[V1];

			m_WeldedVertices[V0].AdjacentVertices.AppendUnique( &WV1 );
			m_WeldedVertices[V1].AdjacentVertices.AppendUnique( &WV0 );
		}
	}
}

U32	SHProbeNetwork::MeshWithAdjacency::Primitive::PropagateProbeInfluences( SHProbeNetwork& _Owner ) {
	U32				spreadsCount = 0;
	WeldedVertex*	pVertex = &m_WeldedVertices[0];
	int				VerticesCount = m_WeldedVertices.GetCount();
	for ( int VertexIndex=0; VertexIndex < VerticesCount; VertexIndex++, pVertex++ ) {
		spreadsCount += pVertex->PropagateProbeInfluencesBetweenVertices( _Owner ) ? 1 : 0;
	}

	return spreadsCount;
}

// Assigns the nearest probe to any isolated vertex without probe influence (worst case scenario)
U32	SHProbeNetwork::MeshWithAdjacency::Primitive::AssignNearestProbe( SHProbeNetwork& _Owner ) {
	U32				isolatedVerticesCount = 0;
	WeldedVertex*	pWeldedVertex = &m_WeldedVertices[0];
	int				VerticesCount = m_WeldedVertices.GetCount();
	for ( int VertexIndex=0; VertexIndex < VerticesCount; VertexIndex++, pWeldedVertex++ ) {
		if ( pWeldedVertex->Influence.ProbeID != ~0U )
			continue;

		float	NearestProbeSqDistance = FLT_MAX;
		U32		NearestProbeIndex = ~0UL;
		for ( U32 ProbeIndex=0; ProbeIndex < _Owner.m_ProbesCount; ProbeIndex++ ) {
			float	SqDistance = (_Owner.m_pProbes[ProbeIndex].m_wsPosition - pWeldedVertex->wsPosition).LengthSq();
			if ( SqDistance < NearestProbeSqDistance ) {
				NearestProbeSqDistance = SqDistance;
				NearestProbeIndex = ProbeIndex;
			}
		}
		pWeldedVertex->Influence.ProbeID = NearestProbeIndex;
		isolatedVerticesCount++;
	}

	return isolatedVerticesCount;
}

// Redistributes the probe influences from welded vertices to original vertices
void	SHProbeNetwork::MeshWithAdjacency::Primitive::RedistributeProbeIDs2Vertices( ProbeInfluence const** _ppProbeInfluences ) const {
	const WeldedVertex*	pWeldedVertex = &m_WeldedVertices[0];
	int					VerticesCount = m_WeldedVertices.GetCount();
	for ( int VertexIndex=0; VertexIndex < VerticesCount; VertexIndex++, pWeldedVertex++ ) {
		VertexLink*	pOriginalVertex = pWeldedVertex->pSharingVertices;
		ASSERT( pOriginalVertex != NULL, "How come a welded vertex exists without any original vertex as a source?!" );

		while ( pOriginalVertex != NULL ) {
			if ( _ppProbeInfluences[pOriginalVertex->V] == NULL || pWeldedVertex->Influence.Influence > _ppProbeInfluences[pOriginalVertex->V]->Influence )
				_ppProbeInfluences[pOriginalVertex->V] = &pWeldedVertex->Influence;	// Replace vertex influence by a larger one

			pOriginalVertex = pOriginalVertex->pNext;
		}
	}
}

bool	SHProbeNetwork::MeshWithAdjacency::Primitive::WeldedVertex::PropagateProbeInfluencesBetweenVertices( SHProbeNetwork& _Owner ) {
	static const float	DISTANCE_FALLOFF_FACTOR = -1.3862943611198906188344642429164f;			// ln( 0.25 ) so 1m away gets 1/4 the influence
	static const float	ANGULAR_FALLOFF_FACTOR = 0.5f * -0.30102999566398119521373889472449f;	// ln( 0.5 ) so a 90° face gets 1/2 the influence

	bool			spreading = false;
	WeldedVertex**	ppAdjacentVertex = &AdjacentVertices[0];
	for ( U32 AdjacentVertexIndex=0; AdjacentVertexIndex < U32(AdjacentVertices.GetCount()); AdjacentVertexIndex++, ppAdjacentVertex++ ) {
		WeldedVertex&	AdjacentVertex = **ppAdjacentVertex;
		if ( AdjacentVertex.Influence.ProbeID == Influence.ProbeID )
			continue;	// Both vertices are influenced by the same probe so our work is done here...
 
		float	Distance = (AdjacentVertex.lsPosition - lsPosition).Length();
		float	DotNormalBetweenFaces = AdjacentVertex.lsNormal.Dot( lsNormal );
		float	FalloffDistance = expf( DISTANCE_FALLOFF_FACTOR * Distance );
		float	FalloffAngle = expf( ANGULAR_FALLOFF_FACTOR * (1.0f - DotNormalBetweenFaces) );
		double	Falloff = FalloffDistance * FalloffAngle;

		double	ReducedInfluence0 = -1.0;
		if (	Influence.ProbeID != ~0UL																// Does our vertex has a probe influence?
			&& _Owner.m_pProbes[Influence.ProbeID].IsInsideVoronoiCell( AdjacentVertex.wsPosition ) ) {	// And can that probe influence the adjacent vertex?
			ReducedInfluence0 = Influence.Influence * Falloff;
		}
		double	ReducedInfluence1 = -1.0;
		if (	AdjacentVertex.Influence.ProbeID != ~0UL												// Does adjacent vertex has a probe influence?
			&& _Owner.m_pProbes[AdjacentVertex.Influence.ProbeID].IsInsideVoronoiCell( wsPosition ) ) {	// And can that probe influence our vertex?
			ReducedInfluence1 = AdjacentVertex.Influence.Influence * Falloff;
		}

		if ( ReducedInfluence0 > AdjacentVertex.Influence.Influence ) {
			// Spread from this vertex to adjacent vertex
			AdjacentVertex.Influence.Influence = ReducedInfluence0;
			AdjacentVertex.Influence.ProbeID = Influence.ProbeID;
			spreading = true;
		} else if ( ReducedInfluence1 > Influence.Influence ) {
			// Spread from adjacent vertex to this vertex
			Influence.Influence = ReducedInfluence1;
			Influence.ProbeID = AdjacentVertex.Influence.ProbeID;
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
		SHProbeNetwork&				m_Owner;
		List< MeshWithAdjacency >*	m_Meshes;
		ProbeInfluence*				m_ProbeInfluencePerFace;
		U32							m_TotalFacesCount;
		U32							m_TotalVerticesCount;

		MeshVisitor( SHProbeNetwork& _Owner ) : m_Owner( _Owner ) {}
		virtual void	HandleNode( Scene::Node& _Node ) override {
			if ( _Node.m_Type != Scene::Node::MESH )
				return;
			
			Scene::Mesh&		SourceMesh = (Scene::Mesh&) _Node;
			MeshWithAdjacency&	TargetMesh = m_Meshes->Append();
			TargetMesh.Build( m_Owner, SourceMesh, m_ProbeInfluencePerFace + m_TotalFacesCount );

			// Accumulate vertices/faces count
			for ( int PrimitiveIndex=0; PrimitiveIndex < SourceMesh.m_PrimitivesCount; PrimitiveIndex++ ) {
				Scene::Mesh::Primitive&	P = SourceMesh.m_pPrimitives[PrimitiveIndex];
				m_TotalFacesCount += P.m_FacesCount;
				m_TotalVerticesCount += P.m_VerticesCount;
			}
		}
	} visitor( *this );
	visitor.m_TotalFacesCount = 0;
	visitor.m_TotalVerticesCount = 0;
	visitor.m_Meshes = &Meshes;
	visitor.m_ProbeInfluencePerFace = &m_ProbeInfluencePerFace[0];
	_Scene.ForEach( visitor );

	//////////////////////////////////////////////////////////////////////////
	// Propagate best probe indices by adjacency
	U32		passesCount = 0;
	U32		spreadsCount = 1;
	U32		averageSpreadsCount = 0;
	while ( spreadsCount > 0 ) {
		passesCount++;

		spreadsCount = 0;
		for ( int MeshIndex=0; MeshIndex < Meshes.GetCount(); MeshIndex++ ) {
			MeshWithAdjacency&	M = Meshes[MeshIndex];
			spreadsCount += M.PropagateProbeInfluences( *this );
		}
		averageSpreadsCount += spreadsCount;
	}
	averageSpreadsCount /= (passesCount-1);

	//////////////////////////////////////////////////////////////////////////
	// Assign nearest probes to vertices without influence (isolated vertices)
	U32	isolatedVerticesCount = 0;
	for ( int MeshIndex=0; MeshIndex < Meshes.GetCount(); MeshIndex++ ) {
		MeshWithAdjacency&	M = Meshes[MeshIndex];
		isolatedVerticesCount += M.AssignNearestProbe( *this );
	}

	//////////////////////////////////////////////////////////////////////////
	// Redistribute to vertices, choosing the best probe influence each time
	ProbeInfluence const**	pProbeInfluencePerVertex = new ProbeInfluence const*[visitor.m_TotalVerticesCount];
	memset( pProbeInfluencePerVertex, 0, visitor.m_TotalVerticesCount*sizeof(ProbeInfluence*) );

	{
		ProbeInfluence const**	ppInfluence = pProbeInfluencePerVertex;
		for ( int MeshIndex=0; MeshIndex < Meshes.GetCount(); MeshIndex++ ) {
			MeshWithAdjacency&	M = Meshes[MeshIndex];
			M.RedistributeProbeIDs2Vertices( ppInfluence );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Save the vertex stream containing U32-packed probe IDs for each vertex
	{
		char	pTemp[1024];
		sprintf_s( pTemp, "%sScene.vertexStream.U16", _pPathToStreamFile );

		FILE*	pFile = NULL;
		fopen_s( &pFile, pTemp, "wb" );
		ASSERT( pFile != NULL, "Can't create vertex stream for probe IDs!" );

		fwrite( &visitor.m_TotalVerticesCount, sizeof(U32), 1, pFile );

		ProbeInfluence const**	ppInfluence = pProbeInfluencePerVertex;
		for ( U32 VertexIndex=0; VertexIndex < visitor.m_TotalVerticesCount; VertexIndex++, ppInfluence++ ) {
			ASSERT( ppInfluence != NULL, "Yikes!" );
			fwrite( &(*ppInfluence)->ProbeID, sizeof(U32), 1, pFile );
		}

		fclose( pFile );
	}

	SAFE_DELETE_ARRAY( pProbeInfluencePerVertex );
}

static void	CopyProbeNetworkConnection( int _EntryIndex, SHProbeNetwork::RuntimeProbeNetworkInfos& _Value, void* _pUserData );

void	SHProbeNetwork::LoadProbes( const char* _pPathToProbes, const float3& _SceneBBoxMin, const float3& _SceneBBoxMax ) {

	FILE*	pFile = NULL;
	char	pTemp[1024];

	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		// Read numbered probe
		sprintf_s( pTemp, "%s/Probe%02d.probeset", _pPathToProbes, ProbeIndex );
		fopen_s( &pFile, pTemp, "rb" );
		if ( pFile == NULL ) {
			// Not ready yet (happens for first time computation!)
			memset( Probe.m_pSamples, 0, SHProbe::SAMPLES_COUNT*sizeof(SHProbe::Sample) );
			Probe.m_EmissiveSurfacesCount = 0;
			continue;
		}
//		ASSERT( pFile != NULL, "Can't find probeset test file!" );

		Probe.Load( pFile );

		fclose( pFile );
	}


	//////////////////////////////////////////////////////////////////////////
	// Count total number of neighbors
	U32		TotalNeighborsCount = 0;
	U32		MinNeighborsCount = INT_MAX;
	U32		MaxNeighborsCount = 0;
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];
		U32			NeighborsCount = Probe.m_VoronoiProbes.GetCount();

		TotalNeighborsCount += NeighborsCount;
		MinNeighborsCount = MIN( MinNeighborsCount, NeighborsCount );
		MaxNeighborsCount = MAX( MaxNeighborsCount, NeighborsCount );
	}
	float	AvgNeighborsCount = float(TotalNeighborsCount) / m_ProbesCount;


	//////////////////////////////////////////////////////////////////////////
	// Allocate runtime probes structured buffer
	m_pSB_RuntimeProbes = new SB<RuntimeProbe>( *m_pDevice, m_ProbesCount, true );
	m_pSB_ProbeNeighbors = new SB<ProbeNeighbors>( *m_pDevice, TotalNeighborsCount, true );

	ProbeNeighbors*	pNeighbor = m_pSB_ProbeNeighbors->m;

	TotalNeighborsCount = 0;
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];
		U32			NeighborsCount = Probe.m_VoronoiProbes.GetCount();

		m_pSB_RuntimeProbes->m[ProbeIndex].Position = Probe.m_wsPosition;
		m_pSB_RuntimeProbes->m[ProbeIndex].Radius = Probe.m_MaxDistance;
//		m_pSB_RuntimeProbes->m[ProbeIndex].Radius = Probe.MeanDistance;

		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborsOffset = TotalNeighborsCount;
		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborsCount = NeighborsCount;

		// Write neighbors
		for ( U32 NeighborIndex=0; NeighborIndex < NeighborsCount; NeighborIndex++, pNeighbor++ ) {
			SHProbe::VoronoiProbeInfo&	Neighbor = Probe.m_VoronoiProbes[NeighborIndex];
			pNeighbor->ProbeID = Neighbor.ProbeID;
			pNeighbor->Position = m_pProbes[Neighbor.ProbeID].m_wsPosition;
		}

		TotalNeighborsCount += NeighborsCount;
	}
	m_pSB_RuntimeProbes->Write();
	m_pSB_ProbeNeighbors->Write();


	//////////////////////////////////////////////////////////////////////////
	// Copy static lighting & occlusion info
	m_ppSB_RuntimeSHStatic[0] = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, true );
	m_ppSB_RuntimeSHStatic[1] = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, true );
	m_pSB_RuntimeSHAmbient = new SB<SHCoeffs1>( *m_pDevice, m_ProbesCount, true );
	m_pSB_RuntimeSHDynamic = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, false );
	m_pSB_RuntimeSHDynamicSun = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, false );
	m_pSB_RuntimeSHFinal = new SB<SHCoeffs3>( *m_pDevice, m_ProbesCount, false );

	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ ) {
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		for ( int SHCoeffIndex=0; SHCoeffIndex < 9; SHCoeffIndex++ ) {
			m_ppSB_RuntimeSHStatic[0]->m[ProbeIndex].pSH[SHCoeffIndex] = Probe.m_pSHStaticLighting[SHCoeffIndex];
			m_ppSB_RuntimeSHStatic[1]->m[ProbeIndex].pSH[SHCoeffIndex] = Probe.m_pSHStaticLighting[SHCoeffIndex];

			m_pSB_RuntimeSHAmbient->m[ProbeIndex].pSH[SHCoeffIndex] = Probe.m_pSHOcclusion[SHCoeffIndex];

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

		int	NodesCount = m_ProbeOctree.Append( Probe.m_wsPosition, Probe.m_MaxDistance, &Probe );
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
			SHProbe::NeighborProbeInfo&	NeighborInfos = Probe.m_NeighborProbes[NeighborProbeIndex];
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
				if ( NeighborProbe.m_NeighborProbes[NeighborNeighborProbeIndex].ProbeID == ProbeIndex )
				{	// Found us!
					// Now we can get the exact solid angle!
					pConnection->NeighborsSolidAngles.y = NeighborProbe.m_NeighborProbes[NeighborNeighborProbeIndex].SolidAngle;
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
