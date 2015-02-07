#include "../../GodComplex.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

SHProbeNetwork::SHProbeNetwork() 
	: m_pDevice( NULL )
	, m_ErrorCode( 0 )
	, m_ProbesCount( 0 )
	, m_MaxProbesCount( 0 )
	, m_pProbes( NULL )
	, m_ProbeUpdateIndex( 0 )
{

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

	m_pSB_RuntimeProbeUpdateInfos = new SB<RuntimeProbeUpdateInfos>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME, true );
	m_pSB_RuntimeProbeSetInfos = new SB<RuntimeProbeUpdateSetInfos>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME*MAX_PROBE_SETS, true );
	m_pSB_RuntimeProbeEmissiveSetInfos = new SB<RuntimeProbeUpdateEmissiveSetInfos>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME*MAX_PROBE_EMISSIVE_SETS, true );
	m_pSB_RuntimeSamplingPointInfos = new SB<RuntimeSamplingPointInfos>( *m_pDevice, MAX_PROBE_UPDATES_PER_FRAME * MAX_SET_SAMPLES, true );

	//////////////////////////////////////////////////////////////////////////
	// Create shaders
	{
//ScopedForceMaterialsLoadFromBinary		bisou;

		CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_GI_RENDER_CUBEMAP, "./Resources/Shaders/GIRenderCubeMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
 		CHECK_MATERIAL( m_pMatRenderNeighborProbe = CreateMaterial( IDR_SHADER_GI_RENDER_NEIGHBOR_PROBE, "./Resources/Shaders/GIRenderNeighborProbe.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 11 );
	}

	{
// This one is REALLY heavy! So build it once and reload it from binary forever again
//ScopedForceMaterialsLoadFromBinary		bisou;
		// Compute Shaders
 		CHECK_MATERIAL( m_pCSUpdateProbe = CreateComputeShader( IDR_SHADER_GI_UPDATE_PROBE, "./Resources/Shaders/GIUpdateProbe.hlsl", "CS" ), 1 );
	}
}

void	SHProbeNetwork::Exit() {
	m_ProbesCount = 0;
	SAFE_DELETE_ARRAY( m_pProbes );

	delete m_pCSUpdateProbe;
	delete m_pMatRenderNeighborProbe;
	delete m_pMatRenderCubeMap;

	delete m_pSB_RuntimeSamplingPointInfos;
	delete m_pSB_RuntimeProbeEmissiveSetInfos;
	delete m_pSB_RuntimeProbeSetInfos;
	delete m_pSB_RuntimeProbeUpdateInfos;
	delete m_pSB_RuntimeProbeNetworkInfos;
	delete m_pSB_RuntimeProbes;

	delete m_pCB_UpdateProbes;
	delete m_pCB_Probe;
}

void	SHProbeNetwork::AddProbe( Scene::Probe& _Probe ) {
	ASSERT( m_ProbesCount < m_MaxProbesCount, "Probes count out of range!" );
	m_pProbes[m_ProbesCount].ProbeID = m_ProbesCount;
	m_pProbes[m_ProbesCount].pSceneProbe = &_Probe;
	m_ProbesCount++;
}

void	SHProbeNetwork::UpdateDynamicProbes( DynamicUpdateParms& _Parms ) {
//	ASSERT( m_ProbesCount <= MAX_PROBE_UPDATES_PER_FRAME, "Increase max probes update per frame! Or write the time-sliced updater you promised!" );

	// Prepare constant buffer for update
	for ( int i=0; i < 9; i++ )
		m_pCB_UpdateProbes->m.AmbientSH[i] = float4( _Parms.AmbientSkySH[i], 0 );	// Update one by one because of float3 padding

	m_pCB_UpdateProbes->m.AmbientSH[8].w = _Parms.BounceFactorSun.x;	// Last padding hides one of our variables in its W component...
	m_pCB_UpdateProbes->m.SkyBoost = _Parms.BounceFactorSky.x;
	m_pCB_UpdateProbes->m.DynamicLightsBoost = _Parms.BounceFactorDynamic.x;
	m_pCB_UpdateProbes->m.StaticLightingBoost = _Parms.BounceFactorStatic.x;
	m_pCB_UpdateProbes->m.EmissiveBoost = _Parms.BounceFactorEmissive.x;
	m_pCB_UpdateProbes->m.NeighborProbesContributionBoost = _Parms.BounceFactorNeighbors.x;

	m_pCB_UpdateProbes->UpdateData();


#if 1	// Hardware update
	// We prepare the update structures for each probe and send this to the compute shader
	// . The compute shader will then evaluate lighting for all the sampling points for the probe, use their contribution to weight
	//		each set's SH coefficients that will be added together to form the indirect lighting SH coefficients.
	// . Then it will compute the product of ambient sky SH and occlusion SH for the probe to add the contribution of the occluded sky
	// . It will also add the emissive sets' SH weighted by the intensity of the emissive materials at the time (diffuse area lighting).
	// . Finally, it will estimate the neighbor's "perceived visibility" and propagate their SH via a product of their SH with the
	//		neighbor visibility mask. This way we get additional light bounces from probe to probe.
	//
	// Basically for every probe update, we perform 1(sky)+4(neighbor) expensive SH products and compute lighting for 64 points in the scene
	//
	U32		ProbeUpdatesCount = MIN( _Parms.MaxProbeUpdatesPerFrame, U32(m_ProbesCount) );
//TODO: Handle a proper stack of probes to update

	// Prepare the buffer of probe update infos and sampling point infos
	int		TotalSamplingPointsCount = 0;
	int		TotalSetsCount = 0;
	int		TotalEmissiveSetsCount = 0;
	for ( U32 ProbeUpdateIndex=0; ProbeUpdateIndex < ProbeUpdatesCount; ProbeUpdateIndex++ )
	{
//		int		ProbeIndex = ProbeUpdateIndex;	// Simple at the moment, when we have the update stack we'll have to fetch the index from it...

		// Still simple: we update N probes each frame in sequence, next frame we'll update the next N ones...
		int		ProbeIndex = (m_ProbeUpdateIndex + ProbeUpdateIndex) % m_ProbesCount;

		SHProbe&	Probe = m_pProbes[ProbeIndex];

		// Fill the probe update infos
		RuntimeProbeUpdateInfos&	ProbeUpdateInfos = m_pSB_RuntimeProbeUpdateInfos->m[ProbeUpdateIndex];

		ProbeUpdateInfos.Index = ProbeIndex;
		ProbeUpdateInfos.SetsStart = TotalSetsCount;
		ProbeUpdateInfos.SetsCount = Probe.SetsCount;
		ProbeUpdateInfos.EmissiveSetsStart = TotalEmissiveSetsCount;
		ProbeUpdateInfos.EmissiveSetsCount = Probe.EmissiveSetsCount;
		memcpy_s( ProbeUpdateInfos.SHStatic, sizeof(ProbeUpdateInfos.SHStatic), Probe.pSHBounceStatic, 9*sizeof(float3) );
		memcpy_s( ProbeUpdateInfos.SHOcclusion, sizeof(ProbeUpdateInfos.SHOcclusion), Probe.pSHOcclusion, 9*sizeof(float) );

		// Copy
		for( int i=0; i < 9; i++ )
		{
			ProbeUpdateInfos.NeighborProbeSH[i].x = Probe.pNeighborProbeInfos[0].SH[i];
			ProbeUpdateInfos.NeighborProbeSH[i].y = Probe.pNeighborProbeInfos[1].SH[i];
			ProbeUpdateInfos.NeighborProbeSH[i].z = Probe.pNeighborProbeInfos[2].SH[i];
			ProbeUpdateInfos.NeighborProbeSH[i].w = Probe.pNeighborProbeInfos[3].SH[i];
		}

		ProbeUpdateInfos.SamplingPointsStart = TotalSamplingPointsCount;

		// Fill the set update infos
		int	SetSamplingPointsCount = 0;
		for ( U32 SetIndex=0; SetIndex < Probe.SetsCount; SetIndex++ )
		{
			SHProbe::SetInfos		Set = Probe.pSetInfos[SetIndex];
			RuntimeProbeUpdateSetInfos&	SetUpdateInfos = m_pSB_RuntimeProbeSetInfos->m[TotalSetsCount+SetIndex];

			SetUpdateInfos.SamplingPointsStart = SetSamplingPointsCount;
			SetUpdateInfos.SamplingPointsCount = Set.SamplesCount;
			memcpy_s( SetUpdateInfos.SH, sizeof(SetUpdateInfos.SH), Set.pSHBounce, 9*sizeof(float3) );

			// Copy sampling points (fortunately it's the same static & runtime structures)
			memcpy_s( &m_pSB_RuntimeSamplingPointInfos->m[TotalSamplingPointsCount], Set.SamplesCount*sizeof(RuntimeSamplingPointInfos), Set.pSamples, Set.SamplesCount*sizeof(SHProbe::SetInfos::Sample) );

			TotalSamplingPointsCount += Set.SamplesCount;
			SetSamplingPointsCount += Set.SamplesCount;
		}

		// Fill the emissive set update infos
		for ( U32 EmissiveSetIndex=0; EmissiveSetIndex < Probe.EmissiveSetsCount; EmissiveSetIndex++ )
		{
			SHProbe::EmissiveSetInfos		EmissiveSet = Probe.pEmissiveSetInfos[EmissiveSetIndex];
			RuntimeProbeUpdateEmissiveSetInfos&	EmissiveSetUpdateInfos = m_pSB_RuntimeProbeEmissiveSetInfos->m[TotalEmissiveSetsCount+EmissiveSetIndex];

			ASSERT( EmissiveSet.pEmissiveMaterial != NULL, "Invalid emissive material!" );
			EmissiveSetUpdateInfos.EmissiveColor = EmissiveSet.pEmissiveMaterial->m_EmissiveColor;

			memcpy_s( EmissiveSetUpdateInfos.SH, sizeof(EmissiveSetUpdateInfos.SH), EmissiveSet.pSHEmissive, 9*sizeof(float) );
		}

		TotalSetsCount += Probe.SetsCount;
		TotalEmissiveSetsCount += Probe.EmissiveSetsCount;

		ProbeUpdateInfos.SamplingPointsCount = TotalSamplingPointsCount - ProbeUpdateInfos.SamplingPointsStart;	// Total amount of sampling points for the probe
	}

	// Do the update!
	USING_COMPUTESHADER_START( *m_pCSUpdateProbe )

	m_pSB_RuntimeProbeUpdateInfos->Write( ProbeUpdatesCount );
	m_pSB_RuntimeProbeUpdateInfos->SetInput( 10 );

	m_pSB_RuntimeProbeSetInfos->Write( TotalSetsCount );
	m_pSB_RuntimeProbeSetInfos->SetInput( 11 );

	m_pSB_RuntimeProbeEmissiveSetInfos->Write( TotalEmissiveSetsCount );
	m_pSB_RuntimeProbeEmissiveSetInfos->SetInput( 12 );

	m_pSB_RuntimeSamplingPointInfos->Write( TotalSamplingPointsCount );
	m_pSB_RuntimeSamplingPointInfos->SetInput( 13 );

	m_pSB_RuntimeProbes->RemoveFromLastAssignedSlots();
	m_pSB_RuntimeProbes->SetOutput( 0 );

	M.Dispatch( ProbeUpdatesCount, 1, 1 );

	USING_COMPUTE_SHADER_END

	m_pSB_RuntimeProbes->SetInput( 9, true );

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

		// Iterate on each set and compute energy level
		for ( U32 SetIndex=0; SetIndex < Probe.SetsCount; SetIndex++ )
		{
			const SHProbe::SetInfos&	Set = Probe.pSetInfos[SetIndex];

			// Compute irradiance for every sample
			float3	SetIrradiance = float3::Zero;
			for ( U32 SampleIndex=0; SampleIndex < Set.SamplesCount; SampleIndex++ )
			{
				const SHProbe::SetInfos::Sample&	Sample = Set.pSamples[SampleIndex];

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
	m_pSB_RuntimeProbes->SetInput( 9, true );

#endif

}

U32	SHProbeNetwork::GetNearestProbe( const float3& _wsPosition ) const {
	float					ProbeDistance;
	const SHProbe* const*	ppNearestProbe = m_ProbeOctree.FetchNearest( _wsPosition, ProbeDistance );
	U32						probeID = ppNearestProbe != NULL ? (*ppNearestProbe)->ProbeID : 0xFFFFFFFFU;
	return probeID;
}

void	SHProbeNetwork::SHProbe::ClearLightBounce( const float3 _pSHAmbient[9] )
{
	// 1] Perform the product of direct ambient light with direct environment mask and accumulate with indirect lighting
	float3	pSHOccludedAmbientLight[9];
	SH::Product3( _pSHAmbient, pSHOcclusion, pSHOccludedAmbientLight );

	// 2] Initialize bounced light with ambient SH + static lighting SH
	for ( int i=0; i < 9; i++ )
		pSHBouncedLight[i] = pSHBounceStatic[i] + pSHOccludedAmbientLight[i];
}

void	SHProbeNetwork::SHProbe::AccumulateLightBounce( const float3 _pSHSet[9] )
{
	// Simply accumulate dynamic set lighting to bounced light
	for ( int i=0; i < 9; i++ )
		pSHBouncedLight[i] = pSHBouncedLight[i] + _pSHSet[i];
}

void	SHProbeNetwork::PreComputeProbes( const char* _pPathToProbes, IRenderSceneDelegate& _RenderScene ) {

	const float		Z_INFINITY = 1e6f;
	const float		Z_INFINITY_TEST = 0.99f * Z_INFINITY;

	m_pRTCubeMap = new Texture2D( *m_pDevice, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6 * 4, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain albedo (cube 0) + (normal + distance) (cube 1) + (static lighting + emissive surface index) (cube 2) + Probe IDs (cube 3)
	Texture2D*	pRTCubeMapDepth = new Texture2D( *m_pDevice, CUBE_MAP_SIZE, CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );
	Texture2D*	pRTCubeMapStaging = new Texture2D( *m_pDevice, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6 * 4, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );


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
	// Render every probe as a cube map & process
	//
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
//for ( int ProbeIndex=0; ProbeIndex < 1; ProbeIndex++ )
	{
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

		ASSERT( ProbeLocal2World.GetRow(0).LengthSq() > 0.999f && ProbeLocal2World.GetRow(1).LengthSq() > 0.999f && ProbeLocal2World.GetRow(2).LengthSq() > 0.999f, "Not identity! If not identity then transform probe set positions/normals/etc. by probe matrix!" );

		float4x4	ProbeWorld2Local = ProbeLocal2World.Inverse();

		// Render the 6 faces
		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
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
			m_pDevice->SetRenderTargets( CUBE_MAP_SIZE, CUBE_MAP_SIZE, 3, ppViews, pRTCubeMapDepth->GetDSV() );

			// Render scene
			_RenderScene( *m_pMatRenderCubeMap );


			//////////////////////////////////////////////////////////////////////////
			// 2]  Render neighborhood for each probe
			// The idea here is simply to build a 3D voronoi cell by splatting the planes passing through all other probes
			//	with their normal set to the direction from the other probe to the current probe.
			// Splatting a new plane and accounting for the depth buffer will let visible pixels from the plane show up
			//	and write the ID of the probe.
			//
			// Reading back the cube map will indicate the solid angle perceived by each probe to each of its neighbors
			//	so we can create a linked list of neighbor probes, of their visibilities and solid angle
			//
			m_pDevice->SetStates( m_pDevice->m_pRS_CullNone, m_pDevice->m_pDS_ReadWriteLess, m_pDevice->m_pBS_Disabled );
			m_pDevice->SetRenderTarget( CUBE_MAP_SIZE, CUBE_MAP_SIZE, *m_pRTCubeMap->GetRTV( 0, 6*3+CubeFaceIndex, 1 ), pRTCubeMapDepth->GetDSV() );

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
		// 3] Read back cube map and create the SH coefficients
		pRTCubeMapStaging->CopyFrom( *m_pRTCubeMap );

#if 1	// Save to disk for processing by the ProbeSHEncoder tool
		char	pTemp[1024];
		sprintf_s( pTemp, "%s/Probe%02d.pom", _pPathToProbes, ProbeIndex );
		pRTCubeMapStaging->Save( pTemp );
#endif


/* TODO! At the moment we only read back sets from disk that were computed by the probe SH encoder tool (in Tools.sln)
	But when the probe SH encoder tool is complete, I'll have to re-write it in C++ for in-place probe encoding...
	---------------------------------------------------------------------------------------------------------------------

		double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
		double	SumSolidAngle = 0.0;

		double	pSHOcclusion[9];
		memset( pSHOcclusion, 0, 9*sizeof(double) );

		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceAlbedo = pRTCubeMapStaging->Map( 0, CubeFaceIndex );
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceGeometry = pRTCubeMapStaging->Map( 0, 6+CubeFaceIndex );

			// Update cube map face camera transform
			float4x4	Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;

			float3	View( 0, 0, 1 );
			for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			{
				float4*	pScanlineAlbedo = (float4*) ((U8*) MappedFaceAlbedo.pData + Y * MappedFaceAlbedo.RowPitch);
				float4*	pScanlineGeometry = (float4*) ((U8*) MappedFaceGeometry.pData + Y * MappedFaceGeometry.RowPitch);

				View.y = 1.0f - 2.0f * (0.5f + Y) / CUBE_MAP_SIZE;
				for ( int X=0; X < CUBE_MAP_SIZE; X++ )
				{
					float4	Albedo = *pScanlineAlbedo++;
					float4	Geometry = *pScanlineGeometry++;

					// Rebuild view direction
					View.x = 2.0f * (0.5f + X) / CUBE_MAP_SIZE - 1.0f;

					// Retrieve the cube map texel's solid angle (from http://people.cs.kuleuven.be/~philip.dutre/GI/TotalCompendium.pdf)
					// dw = cos(Theta).dA / r²
					// cos(Theta) = Adjacent/Hypothenuse = 1/r
					//
					float	SqDistance2Texel = View.LengthSq();
					float	Distance2Texel = sqrtf( SqDistance2Texel );

					double	SolidAngle = dA / (Distance2Texel * SqDistance2Texel);
					SumSolidAngle += SolidAngle;	// CHECK! => Should amount to 4PI at the end of the iteration...

					// Check if we hit an obstacle, in which case we should accumulate direct ambient lighting
					if ( Geometry.w > Z_INFINITY_TEST )
					{	// No obstacle means direct lighting from the ambient sky...
						float3	ViewWorld = float4( View, 0.0f ) * Camera2World;	// View vector in world space
						ViewWorld.Normalize();

						// Accumulate SH coefficients in that direction, weighted by the solid angle
 						double	pSHCoeffs[9];
 						BuildSHCoeffs( ViewWorld, pSHCoeffs );
						for ( int i=0; i < 9; i++ )
							pSHOcclusion[i] += SolidAngle * pSHCoeffs[i];

						continue;
					}
				}
			}

			pRTCubeMapStaging->UnMap( 0, 6*0+CubeFaceIndex );
			pRTCubeMapStaging->UnMap( 0, 6*1+CubeFaceIndex );
		}

		//////////////////////////////////////////////////////////////////////////
		// 3] Store direct ambient and indirect reflection of static lights on static geometry
// DON'T NORMALIZE		double	Normalizer = 4*PI / SumSolidAngle;
		for ( int i=0; i < 9; i++ )
		{
			Probe.pSHOcclusion[i] = float( Normalizer * pSHOcclusion[i] );

// TODO! At the moment we don't compute static SH coeffs
Probe.pSHBounceStatic[i] = float3::Zero;
		}

		//////////////////////////////////////////////////////////////////////////
		// 4] Compute solid sets for that probe
		// This part is really important as it will attempt to isolate the important geometric zones near the probe to
		//	approximate them using simple planar impostors that will be lit instead of the entire probe's pixels
		// Each solid set is then lit by dynamic lights in real-time and all pixels belonging to the set add their SH
		//	contribution to the total SH of the probe, this allows us to perform dynamic light bounce on the scene cheaply!
		//

*/
	}

	delete pCBCubeMapCamera;

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

static void	CopyProbeNetworkConnection( int _EntryIndex, SHProbeNetwork::RuntimeProbeNetworkInfos& _Value, void* _pUserData );

void	SHProbeNetwork::LoadProbes( const char* _pPathToProbes, IQueryMaterial& _QueryMaterial, const float3& _SceneBBoxMin, const float3& _SceneBBoxMax ) {

	FILE*	pFile = NULL;
	char	pTemp[1024];

	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		// Read numbered probe
		sprintf_s( pTemp, "%s/Probe%02d.probeset", _pPathToProbes, ProbeIndex );
		fopen_s( &pFile, pTemp, "rb" );
		if ( pFile == NULL )
		{	// Not ready yet (happens for first time computation!)
			Probe.SetsCount = Probe.EmissiveSetsCount = 0;
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
		for ( int i=0; i < 9; i++ )
		{
			fread_s( &Probe.pSHBounceStatic[i].x, sizeof(Probe.pSHBounceStatic[i].x), sizeof(float), 1, pFile );
			fread_s( &Probe.pSHBounceStatic[i].y, sizeof(Probe.pSHBounceStatic[i].y), sizeof(float), 1, pFile );
			fread_s( &Probe.pSHBounceStatic[i].z, sizeof(Probe.pSHBounceStatic[i].z), sizeof(float), 1, pFile );
		}

		// Read occlusion SH
		for ( int i=0; i < 9; i++ )
			fread_s( &Probe.pSHOcclusion[i], sizeof(Probe.pSHOcclusion[i]), sizeof(float), 1, pFile );

		// Read the amount of dynamic sets
		U32	SetsCount = 0;
		fread_s( &SetsCount, sizeof(SetsCount), sizeof(U32), 1, pFile );
		Probe.SetsCount = MIN( MAX_PROBE_SETS, SetsCount );	// Don't read more than we can chew!

		// Read the sets
		SHProbe::SetInfos	DummySet;
		for ( U32 SetIndex=0; SetIndex < SetsCount; SetIndex++ )
		{
			SHProbe::SetInfos&	S = SetIndex < Probe.SetsCount ? Probe.pSetInfos[SetIndex] : DummySet;	// We load into a useless set if out of range...

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

			fread_s( &S.Albedo.x, sizeof(S.Albedo.x), sizeof(float), 1, pFile );
			fread_s( &S.Albedo.y, sizeof(S.Albedo.y), sizeof(float), 1, pFile );
			fread_s( &S.Albedo.z, sizeof(S.Albedo.z), sizeof(float), 1, pFile );

			// Transform set's position/normal by probe's LOCAL=>WORLD
			S.Position = float3( Probe.pSceneProbe->m_Local2World.GetRow(3) ) + S.Position;
// 			NjFloat3	wsSetNormal = Set.Normal;
// 			NjFloat3	wsSetTangent = Set.Tangent;
// 			NjFloat3	wsSetBiTangent = Set.BiTangent;
// TODO: Handle non-identity matrices! Let's go fast for now...
// ARGH! That also means possibly rotating the SH!
// Let's just force the probes to be axis-aligned, shall we??? :) (lazy man talking) (no, seriously, it makes sense after all)

			// Read SH coefficients
			for ( int i=0; i < 9; i++ )
			{
				fread_s( &S.pSHBounce[i].x, sizeof(S.pSHBounce[i].x), sizeof(float), 1, pFile );
				fread_s( &S.pSHBounce[i].y, sizeof(S.pSHBounce[i].y), sizeof(float), 1, pFile );
				fread_s( &S.pSHBounce[i].z, sizeof(S.pSHBounce[i].z), sizeof(float), 1, pFile );
			}

			// Read the samples
			fread_s( &S.SamplesCount, sizeof(S.SamplesCount), sizeof(U32), 1, pFile );
			ASSERT( S.SamplesCount < MAX_SET_SAMPLES, "Too many samples for that set!" );
			for ( U32 SampleIndex=0; SampleIndex < S.SamplesCount; SampleIndex++ )
			{
				SHProbe::SetInfos::Sample&	Sample = S.pSamples[SampleIndex];

				// Read position
				fread_s( &Sample.Position.x, sizeof(Sample.Position.x), sizeof(float), 1, pFile );
				fread_s( &Sample.Position.y, sizeof(Sample.Position.y), sizeof(float), 1, pFile );
				fread_s( &Sample.Position.z, sizeof(Sample.Position.z), sizeof(float), 1, pFile );

				// Read normal
				fread_s( &Sample.Normal.x, sizeof(Sample.Normal.x), sizeof(float), 1, pFile );
				fread_s( &Sample.Normal.y, sizeof(Sample.Normal.y), sizeof(float), 1, pFile );
				fread_s( &Sample.Normal.z, sizeof(Sample.Normal.z), sizeof(float), 1, pFile );

				// Read disk radius
				fread_s( &Sample.Radius, sizeof(Sample.Radius), sizeof(float), 1, pFile );


				// Transform set's position/normal by probe's LOCAL=>WORLD
				Sample.Position = float3( Probe.pSceneProbe->m_Local2World.GetRow(3) ) + Sample.Position;
//				NjFloat3	wsSetNormal = Sample.Normal;
// TODO: Handle non-identity matrices! Let's go fast for now...
// ARGH! That also means possibly rotating the SH!
// Let's just force the probes to be axis-aligned, shall we??? :) (lazy man talking) (no, seriously, it makes sense after all)

			}
		}

		// Read the amount of emissive sets
		U32	EmissiveSetsCount;
		fread_s( &EmissiveSetsCount, sizeof(EmissiveSetsCount), sizeof(U32), 1, pFile );
		Probe.EmissiveSetsCount = MIN( MAX_PROBE_EMISSIVE_SETS, EmissiveSetsCount );	// Don't read more than we can chew!

		// Read the sets
		SHProbe::EmissiveSetInfos	DummyEmissiveSet;
		for ( U32 SetIndex=0; SetIndex < EmissiveSetsCount; SetIndex++ )
		{
			SHProbe::EmissiveSetInfos&	S = SetIndex < Probe.EmissiveSetsCount ? Probe.pEmissiveSetInfos[SetIndex] : DummyEmissiveSet;	// We load into a useless set if out of range...

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

		for ( U32 NeighborProbeIndex=0; NeighborProbeIndex < NeighborProbesCount; NeighborProbeIndex++ )
		{
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
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		m_pSB_RuntimeProbes->m[ProbeIndex].Position = Probe.pSceneProbe->m_Local2World.GetRow( 3 );
		m_pSB_RuntimeProbes->m[ProbeIndex].Radius = Probe.MaxDistance;
//		m_pSB_RuntimeProbes->m[ProbeIndex].Radius = Probe.MeanDistance;

// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[0] = Probe.pNeighborProbeInfos[0].ProbeID;
// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[1] = Probe.pNeighborProbeInfos[1].ProbeID;
// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[2] = Probe.pNeighborProbeInfos[2].ProbeID;
// 		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[3] = Probe.pNeighborProbeInfos[3].ProbeID;

		ASSERT( Probe.pNeighborProbeInfos[0].ProbeID == ~0 || Probe.pNeighborProbeInfos[0].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
		ASSERT( Probe.pNeighborProbeInfos[1].ProbeID == ~0 || Probe.pNeighborProbeInfos[1].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
		ASSERT( Probe.pNeighborProbeInfos[2].ProbeID == ~0 || Probe.pNeighborProbeInfos[2].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
		ASSERT( Probe.pNeighborProbeInfos[3].ProbeID == ~0 || Probe.pNeighborProbeInfos[3].ProbeID < 65535, "Too many probes to be encoded into a U16!" );
		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[0] = Probe.pNeighborProbeInfos[0].ProbeID;
		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[1] = Probe.pNeighborProbeInfos[1].ProbeID;
		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[2] = Probe.pNeighborProbeInfos[2].ProbeID;
		m_pSB_RuntimeProbes->m[ProbeIndex].NeighborProbeIDs[3] = Probe.pNeighborProbeInfos[3].ProbeID;
	}
	m_pSB_RuntimeProbes->Write();


	//////////////////////////////////////////////////////////////////////////
	// Build the probes' octree
	float	MaxDimension = (_SceneBBoxMax - _SceneBBoxMin).Max();

	m_ProbeOctree.Init( _SceneBBoxMin, MaxDimension, 4.0f, m_ProbesCount );
	int		MaxNodesCount = 0;
	int		TotalNodesCount = 0;
	U32		MaxNodesProbeIndex = ~0;
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
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
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		SHProbe&	Probe = m_pProbes[ProbeIndex];

		for ( int NeighborProbeIndex=0; NeighborProbeIndex < MAX_PROBE_NEIGHBORS; NeighborProbeIndex++ )
		{
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
