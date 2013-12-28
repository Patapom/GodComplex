#include "../../GodComplex.h"
#include "EffectGlobalIllum2.h"

#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectGlobalIllum2::EffectGlobalIllum2( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera ) : m_ErrorCode( 0 ), m_Device( _Device ), m_RTTarget( _RTHDR ), m_ScreenQuad( _ScreenQuad )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
 	CHECK_MATERIAL( m_pMatRender = CreateMaterial( IDR_SHADER_GI_RENDER_SCENE, "./Resources/Shaders/GIRenderScene.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 1 );
 	CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_GI_RENDER_CUBEMAP, "./Resources/Shaders/GIRenderCubeMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 2 );
 	CHECK_MATERIAL( m_pMatRenderNeighborProbe = CreateMaterial( IDR_SHADER_GI_RENDER_NEIGHBOR_PROBE, "./Resources/Shaders/GIRenderNeighborProbe.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
 	CHECK_MATERIAL( m_pMatPostProcess = CreateMaterial( IDR_SHADER_GI_POST_PROCESS, "./Resources/Shaders/GIPostProcess.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 10 );

	//////////////////////////////////////////////////////////////////////////
	// Create the textures
	{
		TextureFilePOM	POM( "./Resources/Scenes/GITest1/pata_diff_colo.pom" );
		m_pTexWalls = new Texture2D( _Device, POM );
	}

	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_Scene = new CB<CBScene>( _Device, 10 );
 	m_pCB_Object = new CB<CBObject>( _Device, 11 );
 	m_pCB_Material = new CB<CBMaterial>( _Device, 12 );
	m_pCB_Probe = new CB<CBProbe>( _Device, 10 );
	m_pCB_Splat = new CB<CBSplat>( _Device, 10 );


	//////////////////////////////////////////////////////////////////////////
	// Create the lights & probes structured buffers
	m_pSB_Lights = new SB<LightStruct>( m_Device, MAX_LIGHTS, true );
	m_pSB_RuntimeProbes = NULL;

	//////////////////////////////////////////////////////////////////////////
	// Create the scene
	m_bDeleteSceneTags = false;
	m_Scene.Load( IDR_SCENE_GI, *this );

	//////////////////////////////////////////////////////////////////////////
	// Start precomputation
	PreComputeProbes();
}
Texture2D*	ppRTCubeMap[3];

EffectGlobalIllum2::~EffectGlobalIllum2()
{
	delete[] m_pProbes;

	m_bDeleteSceneTags = true;
	m_Scene.ClearTags( *this );

	delete m_pSB_RuntimeProbes;
	delete m_pSB_Lights;

	delete m_pCB_Splat;
	delete m_pCB_Probe;
	delete m_pCB_Material;
	delete m_pCB_Object;
	delete m_pCB_Scene;

	delete m_pTexWalls;

	delete m_pMatPostProcess;
	delete m_pMatRenderNeighborProbe;
	delete m_pMatRenderCubeMap;
	delete m_pMatRender;

//###
delete ppRTCubeMap[2];
delete ppRTCubeMap[1];
delete ppRTCubeMap[0];
}


void	EffectGlobalIllum2::Render( float _Time, float _DeltaTime )
{
	// Setup scene data
	m_pCB_Scene->m.LightsCount = MAX_LIGHTS;
	m_pCB_Scene->m.ProbesCount = m_ProbesCount;
	m_pCB_Scene->UpdateData();

	//////////////////////////////////////////////////////////////////////////
	// Animate lights, encode them into SH and inject them into each probe
	m_pSB_Lights->m[0].Color.Set( 100, 100, 100 );
//	m_pSB_Lights->m[0].Position.Set( 0.0f, 0.2f, 4.0f * sinf( 0.4f * _Time ) );	// Move along the corridor
	m_pSB_Lights->m[0].Position.Set( 0.5f * sinf( 0.3f * _Time ), 0.5f + 0.1f * cosf( 1.0f * _Time ), 4.0f * sinf( 0.1f * _Time ) );	// Move along the corridor
	m_pSB_Lights->m[0].Radius = 0.1f;
	m_pSB_Lights->Write();
	m_pSB_Lights->SetInput( 8, true );

	NjFloat3	pSHAmbient[9];
	memset( pSHAmbient, 0, 9*sizeof(NjFloat3) );	// No ambient at the moment...

// 	double		pTestAmbient[9];
// 	BuildSHCosineLobe( NjFloat3( -1, 1, 0 ).Normalize(), pTestAmbient );
// 	for ( int i=0; i < 9; i++ )
// 		pSHAmbient[i] = 5.0f * NjFloat3( 0.7, 0.9, 1.0 ) * float(pTestAmbient[i]);


	for ( int ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

		// Encode all lights into probe
		memset( Probe.pSHLight, 0, 9*sizeof(NjFloat3) );
		double	pSHLight[9];

		for ( int LightIndex=0; LightIndex < MAX_LIGHTS; LightIndex++ )
		{
			const LightStruct&	Light = m_pSB_Lights->m[LightIndex];

			// Approximate a SH cone
			NjFloat3	Probe2Light = Light.Position - Probe.pSceneProbe->m_Local2World.GetRow(3);
			float		DistanceProbe2Light = Probe2Light.Length();
//			float		InvDistance = 1.0f / DistanceProbe2Light;
float		InvDistance = 1.0f / MAX( 0.5f, DistanceProbe2Light );
			Probe2Light = Probe2Light * InvDistance;

			float		HalfAngle = asinf( MIN( 1.0f, Light.Radius / DistanceProbe2Light ) );

			BuildSHSmoothCone( Probe2Light, HalfAngle, pSHLight );

			// Accumulate to probe
			float		LightFactor = InvDistance * InvDistance;
			for ( int i=0; i < 9; i++ )
			{
				float	SH = float( pSHLight[i] );
				Probe.pSHLight[i].x += LightFactor * Light.Color.x * SH;
				Probe.pSHLight[i].y += LightFactor * Light.Color.y * SH;
				Probe.pSHLight[i].z += LightFactor * Light.Color.z * SH;
			}
		}

		// Apply the product of the accumulated light SH and the probe's environment response to get the light bounce
		Probe.ComputeLightBounce( pSHAmbient );
	}

	// Swap bounced light buffers & write to the runtime structured buffer
	for ( int ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];
		RuntimeProbe&	Runtime = m_pSB_RuntimeProbes->m[ProbeIndex];

		Probe.SwapBuffers();

		// Write the result to the probe structured buffer
//		memcpy( Runtime.pSHBounce, Probe.pSHBouncedLight0, 9*sizeof(NjFloat3) );

memcpy( Runtime.pSHBounce, Probe.pSHBounce, 9*sizeof(NjFloat3) );
memcpy( Runtime.pSHLight, Probe.pSHLight, 9*sizeof(NjFloat3) );

	}

	m_pSB_RuntimeProbes->Write();
	m_pSB_RuntimeProbes->SetInput( 9, true );


	//////////////////////////////////////////////////////////////////////////
	// 1] Render the scene
// 	m_Device.ClearRenderTarget( m_RTTarget, NjFloat4::Zero );

 	m_Device.SetRenderTarget( m_RTTarget, &m_Device.DefaultDepthStencil() );
	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

	m_Scene.Render( *this );


	//////////////////////////////////////////////////////////////////////////
	// 2] Post-process the result
	USING_MATERIAL_START( *m_pMatPostProcess )

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_Disabled, m_Device.m_pBS_Disabled );
	m_Device.SetRenderTarget( m_Device.DefaultRenderTarget() );

	m_RTTarget.SetPS( 10 );

	m_pCB_Splat->m.dUV = m_Device.DefaultRenderTarget().GetdUV();
	m_pCB_Splat->UpdateData();

	m_ScreenQuad.Render( M );

	USING_MATERIAL_END

	m_RTTarget.RemoveFromLastAssignedSlots();
}


void	EffectGlobalIllum2::ProbeStruct::ComputeLightBounce( const NjFloat3 _pSHAmbient[9] )
{
	// 1] Accumulate neighbor lighting by considering them as point light sources of their own
	NjFloat3	pSHNeighborsLight[9];
	for ( int i=0; i < 9; i++ )
		pSHNeighborsLight[0].Set( 0, 0, 0 );

	for ( int NeighborIndex=0; NeighborIndex < NeighborsCount; NeighborIndex++ )
	{
		NeighborLink&	Link = pNeighborLinks[NeighborIndex];

		// The neighbor's contribution to our probe is simply its previous frame's SH bounced light multiplied by what we perceive of it
		NjFloat3	pSHNeighborLight[9];
		SH::Product3( Link.pNeighbor->pSHBouncedLight1, Link.pSHLink, pSHNeighborLight );

		// That we accumulate for each neighbor
		for ( int i=0; i < 9; i++ )
			pSHNeighborsLight[i] = pSHNeighborsLight[i] + pSHNeighborLight[i];
//pSHNeighborsLight[i] = pSHNeighborsLight[i] + 10.0f * pSHNeighborLight[i];
	}

	// 2] Perform the product of direct accumulated light with indirect environment bounce
	// This will give us the bounced indirect lighting
	NjFloat3	pSHBouncedLight[9];
	SH::Product3( pSHLight, pSHBounce, pSHBouncedLight );

	// 3] Perform the product of direct ambient light with direct environment mask and accumulate with indirect lighting
	NjFloat3	pSHOccludedAmbientLight[9];
	SH::Product3( _pSHAmbient, pSHOcclusion, pSHOccludedAmbientLight );

	// 4] Generate this frame's total lighting
	for ( int i=0; i < 9; i++ )
		pSHBouncedLight1[i] = pSHNeighborsLight[i] + pSHBouncedLight[i] + pSHOccludedAmbientLight[i];
}

void	EffectGlobalIllum2::ProbeStruct::SwapBuffers()
{
	for ( int i=0; i < 9; i++ )
	{
		NjFloat3	Temp = pSHBouncedLight0[i];
		pSHBouncedLight0[i] = pSHBouncedLight1[i];
		pSHBouncedLight1[i] = Temp;
	}
}

void	EffectGlobalIllum2::PreComputeProbes()
{
	const float		Z_INFINITY = 1e6f;
	const float		Z_INFINITY_TEST = 0.99f * Z_INFINITY;

	ppRTCubeMap[0] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain albedo
	ppRTCubeMap[1] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain normal + distance
	ppRTCubeMap[2] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, 6, PixelFormatR32_UINT::DESCRIPTOR, 1, NULL );	// Will contain probe ID (WARNING ==> NOT a cubemap!)
	Texture2D*	pRTCubeMapDepth = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );

	Texture2D*	ppRTCubeMapStaging[3];
	ppRTCubeMapStaging[0] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );		// Will contain albedo
	ppRTCubeMapStaging[1] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );		// Will contain normal + distance
	ppRTCubeMapStaging[2] = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, 6, PixelFormatR32_UINT::DESCRIPTOR, 1, NULL, true );		// Will contain probe ID (WARNING ==> NOT a cubemap!)


	//////////////////////////////////////////////////////////////////////////
	// Allocate probes
	m_ProbesCount = 0;
	Scene::Node*	pSceneProbe = NULL;
	while ( pSceneProbe = m_Scene.ForEach( Scene::Node::PROBE, pSceneProbe ) )
	{
		m_ProbesCount++;
	}
	m_pProbes = new ProbeStruct[m_ProbesCount];

	pSceneProbe = NULL;
	m_ProbesCount = 0;
	while ( pSceneProbe = m_Scene.ForEach( Scene::Node::PROBE, pSceneProbe ) )
	{
		m_pProbes[m_ProbesCount].pSceneProbe = (Scene::Probe*) pSceneProbe;
		m_pProbes[m_ProbesCount].NeighborsCount = 0;	// No neighbor at the moment

		m_ProbesCount++;
	}

	// Also allocate runtime probes structured buffer
	m_pSB_RuntimeProbes = new SB<RuntimeProbe>( m_Device, m_ProbesCount, true );
	for ( int ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
		m_pSB_RuntimeProbes->m[ProbeIndex].Position = m_pProbes[ProbeIndex].pSceneProbe->m_Local2World.GetRow( 3 );



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
	NjFloat3	SideAt[6] = 
	{
		NjFloat3(  1, 0, 0 ),
		NjFloat3( -1, 0, 0 ),
		NjFloat3( 0,  1, 0 ),
		NjFloat3( 0, -1, 0 ),
		NjFloat3( 0, 0,  1 ),
		NjFloat3( 0, 0, -1 ),
	};
	NjFloat3	SideRight[6] = 
	{
		NjFloat3( 0, 0, -1 ),
		NjFloat3( 0, 0,  1 ),
		NjFloat3(  1, 0, 0 ),
		NjFloat3(  1, 0, 0 ),
		NjFloat3(  1, 0, 0 ),
		NjFloat3( -1, 0, 0 ),
	};

	NjFloat4x4	SideWorld2Proj[6];
	NjFloat4x4	Side2Local[6];
	NjFloat4x4	Camera2Proj = NjFloat4x4::ProjectionPerspective( 0.5f * PI, 1.0f, 0.01f, 1000.0f );
	for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
	{
		NjFloat4x4	Camera2Local;
		Camera2Local.SetRow( 0, SideRight[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 1, SideAt[CubeFaceIndex] ^ SideRight[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 2, SideAt[CubeFaceIndex], 0 );
		Camera2Local.SetRow( 3, NjFloat3::Zero, 1 );

		Side2Local[CubeFaceIndex] = Camera2Local;

		NjFloat4x4	Local2Camera = Camera2Local.Inverse();
		NjFloat4x4	Local2Proj = Local2Camera * Camera2Proj;
		SideWorld2Proj[CubeFaceIndex] = Local2Proj;
	}

	// Create the special CB for cube map projections
	struct	CBCubeMapCamera
	{
		NjFloat4x4	Camera2World;
		NjFloat4x4	World2Proj;
	};
	CB<CBCubeMapCamera>*	pCBCubeMapCamera = new CB<CBCubeMapCamera>( m_Device, 9, true );

	//////////////////////////////////////////////////////////////////////////
	// Render every probe as a cube map & process
	//
	for ( int ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
//for ( int ProbeIndex=0; ProbeIndex < 1; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

		// Clear the temporary counters of every other probe to check if it can be our neighbor
		for ( int NeighborProbeIndex=0; NeighborProbeIndex < m_ProbesCount; NeighborProbeIndex++ )
		{
			m_pProbes[NeighborProbeIndex].__TempNeighborCounter = 0;
			m_pProbes[NeighborProbeIndex].__TempSumSolidAngle = 0.0;
		}


		//////////////////////////////////////////////////////////////////////////
		// 1] Render Albedo + Normal + Z + Neighborhood

		// Clear cube map
		m_Device.ClearRenderTarget( *ppRTCubeMap[0], NjFloat4::Zero );
		m_Device.ClearRenderTarget( *ppRTCubeMap[1], NjFloat4( 0, 0, 0, Z_INFINITY ) );	// We clear distance to infinity here
		m_Device.ClearRenderTarget( *ppRTCubeMap[2], NjFloat4::Zero );					// 0 is the invalid ID

		NjFloat4x4	ProbeLocal2World = Probe.pSceneProbe->m_Local2World;
		ProbeLocal2World.Normalize();

		NjFloat4x4	ProbeWorld2Local = ProbeLocal2World.Inverse();

		// Render the 6 faces
		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
			// Update cube map face camera transform
			NjFloat4x4	World2Proj = ProbeWorld2Local * SideWorld2Proj[CubeFaceIndex];

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;
			pCBCubeMapCamera->m.World2Proj = World2Proj;
			pCBCubeMapCamera->UpdateData();

			// Render the scene into the specific cube map faces
			m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

			ID3D11RenderTargetView*	ppViews[2] = {
				ppRTCubeMap[0]->GetTargetView( 0, CubeFaceIndex, 1 ),
				ppRTCubeMap[1]->GetTargetView( 0, CubeFaceIndex, 1 )
			};
			m_Device.SetRenderTargets( CUBE_MAP_SIZE, CUBE_MAP_SIZE, 2, ppViews, pRTCubeMapDepth->GetDepthStencilView() );

			// Clear depth
			m_Device.ClearDepthStencil( *pRTCubeMapDepth, 1.0f, 0, true, false );

			// Render scene
			Scene::Node*	pMesh = NULL;
			while ( pMesh = m_Scene.ForEach( Scene::Node::MESH, pMesh ) )
			{
				RenderMesh( (Scene::Mesh&) *pMesh, m_pMatRenderCubeMap );
			}

			// Render neighborhood for each probe
			// The idea here is simply to build a 3D voronoi cell by splatting the planes passing through all other probes
			//	with their normal set to the direction from the other probe to the current probe.
			// Splatting a new plane and accounting for the depth buffer will let visible pixels from the plane show up
			//	and write the ID of the probe.
			//
			// Reading back the cube map will indicate the solid angle perceived by each probe to each of its neighbors
			//	so we can create a linked list of neighbor probes, of their visibilities and solid angle
			//
			m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );
			m_Device.SetRenderTarget( CUBE_MAP_SIZE, CUBE_MAP_SIZE, *ppRTCubeMap[2]->GetTargetView( 0, CubeFaceIndex, 1 ), pRTCubeMapDepth->GetDepthStencilView() );

			m_pCB_Probe->m.CurrentProbePosition = Probe.pSceneProbe->m_Local2World.GetRow( 3 );

			USING_MATERIAL_START( *m_pMatRenderNeighborProbe )

#if 1
			for ( int NeighborProbeIndex=0; NeighborProbeIndex < m_ProbesCount; NeighborProbeIndex++ )
				if ( NeighborProbeIndex != ProbeIndex )
				{
					ProbeStruct&	NeighborProbe = m_pProbes[NeighborProbeIndex];

					m_pCB_Probe->m.NeighborProbeID = 1 + NeighborProbeIndex;
					m_pCB_Probe->m.NeighborProbePosition = NeighborProbe.pSceneProbe->m_Local2World.GetRow( 3 );
					m_pCB_Probe->UpdateData();

					m_ScreenQuad.Render( M );
				}
#else
//###DEBUG
const int	ThetaCount = 5;
const int	PhiCount = 5;
int		PipoProbeIndex = 0;
for ( int ThetaIndex=0; ThetaIndex < ThetaCount; ThetaIndex++ )
{
	float	Theta = PI * (ThetaIndex+0.5f) / ThetaCount;
	for ( int PhiIndex=0; PhiIndex < PhiCount; PhiIndex++ )
	{
		float	Phi = TWOPI * (PhiIndex+0.0f) / PhiCount;

		NjFloat3	ProbeDirection;
		ProbeDirection.x = sinf(Phi)*sinf(Theta);
		ProbeDirection.y = cosf(Theta);
		ProbeDirection.z = cos(Phi)*sinf(Theta);

		const float	Distance = 0.5f;
		m_pCB_Probe->m.NeighborProbeID = 1 + PipoProbeIndex++;

		m_pCB_Probe->m.NeighborProbePosition = m_pCB_Probe->m.CurrentProbePosition + Distance * ProbeDirection;
		m_pCB_Probe->UpdateData();

		m_ScreenQuad.Render( M );
	}
}
//###DEBUG
#endif
			USING_MATERIAL_END
		}

		//////////////////////////////////////////////////////////////////////////
		// 2] Read back cube map and create the SH coefficients
		ppRTCubeMapStaging[0]->CopyFrom( *ppRTCubeMap[0] );
		ppRTCubeMapStaging[1]->CopyFrom( *ppRTCubeMap[1] );
		ppRTCubeMapStaging[2]->CopyFrom( *ppRTCubeMap[2] );


		double	dA = 4.0 / (CUBE_MAP_SIZE*CUBE_MAP_SIZE);	// Cube face is supposed to be in [-1,+1], yielding a 2x2 square units
		double	SumSolidAngle = 0.0;

		double	pSHBounce[3*9];
		double	pSHAmbient[9];
		memset( pSHBounce, 0, 3*9*sizeof(double) );
		memset( pSHAmbient, 0, 9*sizeof(double) );

		for ( int CubeFaceIndex=0; CubeFaceIndex < 6; CubeFaceIndex++ )
		{
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceAlbedo = ppRTCubeMapStaging[0]->Map( 0, CubeFaceIndex );
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceGeometry = ppRTCubeMapStaging[1]->Map( 0, CubeFaceIndex );
			D3D11_MAPPED_SUBRESOURCE&	MappedFaceNeighborhood = ppRTCubeMapStaging[2]->Map( 0, CubeFaceIndex );

			// Update cube map face camera transform
			NjFloat4x4	Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;

			pCBCubeMapCamera->m.Camera2World = Side2Local[CubeFaceIndex] * ProbeLocal2World;

			NjFloat3	View( 0, 0, 1 );
			for ( int Y=0; Y < CUBE_MAP_SIZE; Y++ )
			{
				NjFloat4*	pScanlineAlbedo = (NjFloat4*) ((U8*) MappedFaceAlbedo.pData + Y * MappedFaceAlbedo.RowPitch);
				NjFloat4*	pScanlineGeometry = (NjFloat4*) ((U8*) MappedFaceGeometry.pData + Y * MappedFaceGeometry.RowPitch);
				U32*		pScanlineNeighboorhood = (U32*) ((U8*) MappedFaceNeighborhood.pData + Y * MappedFaceNeighborhood.RowPitch);

				View.y = 1.0f - 2.0f * (0.5f + Y) / CUBE_MAP_SIZE;
				for ( int X=0; X < CUBE_MAP_SIZE; X++ )
				{
					NjFloat4	Albedo = *pScanlineAlbedo++;
					NjFloat4	Geometry = *pScanlineGeometry++;
					U32			NeighborID = *pScanlineNeighboorhood++;

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

					// Account for neighbor
					if ( NeighborID > 0 )
					{
						ASSERT( NeighborID <= U32(m_ProbesCount), "Probe ID out of range!" );
						m_pProbes[NeighborID-1].__TempNeighborCounter++;
						m_pProbes[NeighborID-1].__TempSumSolidAngle += SolidAngle;
					}

					// Check if we hit an obstacle, in which case we should accumulate direct ambient lighting
					if ( Geometry.w > Z_INFINITY_TEST )
					{	// No obstacle means direct lighting from the ambient sky...
						NjFloat3	ViewWorld = NjFloat4( View, 0.0f ) * Camera2World;	// View vector in world space
						ViewWorld.Normalize();

						// Accumulate SH coefficients in that direction, weighted by the solid angle
 						double	pSHCoeffs[9];
 						BuildSHCoeffs( ViewWorld, pSHCoeffs );
						for ( int i=0; i < 9; i++ )
							pSHAmbient[i] += SolidAngle * pSHCoeffs[i];

						continue;
					}

					// Build SH cosine lobe in the direction of the surface normal
					NjFloat3	Normal( Geometry.x, Geometry.y, Geometry.z );
					double		pCosineLobe[9];
					BuildSHCosineLobe( Normal, pCosineLobe );

					// Accumulate lobe, weighted by solid angle and albedo
					double	R = SolidAngle * Albedo.x;
					double	G = SolidAngle * Albedo.y;
					double	B = SolidAngle * Albedo.z;
					pSHBounce[3*0+0] += pCosineLobe[0] * R;
					pSHBounce[3*0+1] += pCosineLobe[0] * G;
					pSHBounce[3*0+2] += pCosineLobe[0] * B;
					pSHBounce[3*1+0] += pCosineLobe[1] * R;
					pSHBounce[3*1+1] += pCosineLobe[1] * G;
					pSHBounce[3*1+2] += pCosineLobe[1] * B;
					pSHBounce[3*2+0] += pCosineLobe[2] * R;
					pSHBounce[3*2+1] += pCosineLobe[2] * G;
					pSHBounce[3*2+2] += pCosineLobe[2] * B;
					pSHBounce[3*3+0] += pCosineLobe[3] * R;
					pSHBounce[3*3+1] += pCosineLobe[3] * G;
					pSHBounce[3*3+2] += pCosineLobe[3] * B;
					pSHBounce[3*4+0] += pCosineLobe[4] * R;
					pSHBounce[3*4+1] += pCosineLobe[4] * G;
					pSHBounce[3*4+2] += pCosineLobe[4] * B;
					pSHBounce[3*5+0] += pCosineLobe[5] * R;
					pSHBounce[3*5+1] += pCosineLobe[5] * G;
					pSHBounce[3*5+2] += pCosineLobe[5] * B;
					pSHBounce[3*6+0] += pCosineLobe[6] * R;
					pSHBounce[3*6+1] += pCosineLobe[6] * G;
					pSHBounce[3*6+2] += pCosineLobe[6] * B;
					pSHBounce[3*7+0] += pCosineLobe[7] * R;
					pSHBounce[3*7+1] += pCosineLobe[7] * G;
					pSHBounce[3*7+2] += pCosineLobe[7] * B;
					pSHBounce[3*8+0] += pCosineLobe[8] * R;
					pSHBounce[3*8+1] += pCosineLobe[8] * G;
					pSHBounce[3*8+2] += pCosineLobe[8] * B;
				}
			}

			ppRTCubeMapStaging[0]->UnMap( 0, CubeFaceIndex );
			ppRTCubeMapStaging[1]->UnMap( 0, CubeFaceIndex );
			ppRTCubeMapStaging[2]->UnMap( 0, CubeFaceIndex );
		}

		//////////////////////////////////////////////////////////////////////////
		// 3] Store indirect and direct ambient
		double	Normalizer = 1.0 / SumSolidAngle;
		for ( int i=0; i < 9; i++ )
		{
			Probe.pSHBounce[i].Set( float( Normalizer * pSHBounce[3*i+0] ), float( Normalizer * pSHBounce[3*i+1] ), float( Normalizer * pSHBounce[3*i+2] ) );
			Probe.pSHOcclusion[i] = float( Normalizer * pSHAmbient[i] );
		}

		//////////////////////////////////////////////////////////////////////////
		// 4] Sort largest contributing neighbors
		for ( int NeighborProbeIndex=0; NeighborProbeIndex < m_ProbesCount; NeighborProbeIndex++ )
			if ( NeighborProbeIndex != ProbeIndex )
			{
				ProbeStruct&	NeighborProbe = m_pProbes[NeighborProbeIndex];
				if ( NeighborProbe.__TempNeighborCounter == 0 )
					continue;	// Not a neighbor...
				if ( NeighborProbe.__TempSumSolidAngle < 8.0 * PI / 180 )	// Reject under 8 ster-degrees
					continue;	// Too low to contribute...

				// Sort insert the probe in our list of neighbors
				int ExistingNeighborIndex = 0;
				for ( ; ExistingNeighborIndex < Probe.NeighborsCount; ExistingNeighborIndex++ )
				{
					if ( Probe.pNeighborLinks[ExistingNeighborIndex].SolidAngle < NeighborProbe.__TempSumSolidAngle )
						break;	// We found the insertion point!
				}

				if ( ExistingNeighborIndex >= MAX_NEIGHBOR_PROBES )
					continue;	// This neighbor is not influent enough to insert anywhere and our list is full anyway....

				// Shift existing entries
				int		MoveSize = (MAX_NEIGHBOR_PROBES-1-ExistingNeighborIndex) * sizeof(ProbeStruct::NeighborLink);
				memcpy_s( &Probe.pNeighborLinks[ExistingNeighborIndex+1], MoveSize, &Probe.pNeighborLinks[ExistingNeighborIndex], MoveSize );

				// Insert the neighbor
				Probe.pNeighborLinks[ExistingNeighborIndex].pNeighbor = &NeighborProbe;
				Probe.pNeighborLinks[ExistingNeighborIndex].SolidAngle = NeighborProbe.__TempSumSolidAngle;

				// Safe increment the amount of neighbors
				Probe.NeighborsCount = MIN( int(MAX_NEIGHBOR_PROBES), Probe.NeighborsCount+1 );
			}

		//////////////////////////////////////////////////////////////////////////
		// 5] Process neighbor links
		Probe.ProbeInfluenceDistance = Probe.NeighborsCount > 0 ? 0.0f : 10.0f;

		NjFloat3	ProbePosition = Probe.pSceneProbe->m_Local2World.GetRow( 3 );
		for ( int NeighborProbeIndex=0; NeighborProbeIndex < Probe.NeighborsCount; NeighborProbeIndex++ )
		{
			ProbeStruct::NeighborLink&	NeighborProbeLink = Probe.pNeighborLinks[NeighborProbeIndex];
			NjFloat3					NeighborPosition = NeighborProbeLink.pNeighbor->pSceneProbe->m_Local2World.GetRow( 3 );

			// Compute distance
			NjFloat3	ToNeighbor = NeighborPosition - ProbePosition;
			NeighborProbeLink.Distance = ToNeighbor.Length();
			ToNeighbor = ToNeighbor / NeighborProbeLink.Distance;

			// Update probe distance to account for the maximum neighbor distance so we make sure we contribute and reach all neighbors
			Probe.ProbeInfluenceDistance = MAX( Probe.ProbeInfluenceDistance, NeighborProbeLink.Distance );

			// Retrieve an approximate cone angle for the perceived solid angle of that probe
			//	ConeSolidAngle = 2PI * (1 - cos(a)) where a is the half cone angle
			//
			float	ConeHalfAngle = float( acos( 1.0 - NeighborProbeLink.SolidAngle / TWOPI ) );
			double	pSHCone[9];
			BuildSHSmoothCone( ToNeighbor, ConeHalfAngle, pSHCone );
			for ( int i=0; i < 9; i++ )
				NeighborProbeLink.pSHLink[i] = float( pSHCone[i] );
		}


#if 1
// Save to disk
// const char*	ppFileNames[2*6] = {
// 	"ProbeX+_Albedo.pom",
// 	"ProbeX-_Albedo.pom",
// 	"ProbeY+_Albedo.pom",
// 	"ProbeY-_Albedo.pom",
// 	"ProbeZ+_Albedo.pom",
// 	"ProbeZ-_Albedo.pom",
// 
// 	"ProbeX+_Geometry.pom",
// 	"ProbeX-_Geometry.pom",
// 	"ProbeY+_Geometry.pom",
// 	"ProbeY-_Geometry.pom",
// 	"ProbeZ+_Geometry.pom",
// 	"ProbeZ-_Geometry.pom",
// };

ppRTCubeMapStaging[0]->Save( "Probe_Albedo.pom" );
ppRTCubeMapStaging[1]->Save( "Probe_Geometry.pom" );

#endif

	}

	// Update runtime probe influence distances
	for ( int ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
		m_pSB_RuntimeProbes->m[ProbeIndex].ProbeInfluenceDistance = m_pProbes[ProbeIndex].ProbeInfluenceDistance;


	//////////////////////////////////////////////////////////////////////////
	// Release
#if 1
m_Device.RemoveRenderTargets();
ppRTCubeMap[0]->SetPS( 64 );
ppRTCubeMap[1]->SetPS( 65 );
ppRTCubeMap[2]->SetPS( 66 );
#endif

	delete pCBCubeMapCamera;

	delete ppRTCubeMapStaging[2];
	delete ppRTCubeMapStaging[1];
	delete ppRTCubeMapStaging[0];

	delete pRTCubeMapDepth;
//###
// 	delete ppRTCubeMap[2];
// 	delete ppRTCubeMap[1];
// 	delete ppRTCubeMap[0];
}

// Builds the 9 SH coefficient for the specified direction
// (We're already accounting for the fact we're Y-up here)
//
void	EffectGlobalIllum2::BuildSHCoeffs( const NjFloat3& _Direction, double _Coeffs[9] )
{
	const double	f0 = 0.5 / sqrt(PI);
	const double	f1 = sqrt(3.0) * f0;
	const double	f2 = sqrt(15.0) * f0;
	const double	f3 = sqrt(5.0) * 0.5 * f0;

	_Coeffs[0] = f0;
	_Coeffs[1] = -f1 * _Direction.x;
	_Coeffs[2] = f1 * _Direction.y;
	_Coeffs[3] = -f1 * _Direction.z;
	_Coeffs[4] = f2 * _Direction.x * _Direction.z;
	_Coeffs[5] = -f2 * _Direction.x * _Direction.y;
	_Coeffs[6] = f3 * (3.0 * _Direction.y*_Direction.y - 1.0);
	_Coeffs[7] = -f2 * _Direction.z * _Direction.y;
	_Coeffs[8] = f2 * 0.5 * (_Direction.z*_Direction.z - _Direction.x*_Direction.x);
}

// Builds a spherical harmonics cosine lobe
// (from "Stupid SH Tricks")
// (We're already accounting for the fact we're Y-up here)
//
void	EffectGlobalIllum2::BuildSHCosineLobe( const NjFloat3& _Direction, double _Coeffs[9] )
{
	const NjFloat3 ZHCoeffs = NjFloat3(
		0.88622692545275801364908374167057f,	// sqrt(PI) / 2
		1.0233267079464884884795516248893f,		// sqrt(PI / 3)
		0.49541591220075137666812859564002f		// sqrt(5PI) / 8
		);
	ZHRotate( _Direction, ZHCoeffs, _Coeffs );
}

// Builds a spherical harmonics cone lobe (same as for a spherical light source subtending a cone of half angle a)
// (from "Stupid SH Tricks")
//
void	EffectGlobalIllum2::BuildSHCone( const NjFloat3& _Direction, float _HalfAngle, double _Coeffs[9] )
{
	double	a = _HalfAngle;
	double	c = cos( a );
	double	s = sin( a );
	NjFloat3 ZHCoeffs = NjFloat3(
			float( 1.7724538509055160272981674833411 * (1 - c)),				// sqrt(PI) (1 - cos(a))
			float( 1.5349900619197327327193274373339 * (s * s)),				// 0.5 sqrt(3PI) sin(a)^2
			float( 1.9816636488030055066725143825601 * (c * (1 - c) * (1 + c)))	// 0.5 sqrt(5PI) cos(a) (1-cos(a)) (cos(a)+1)
		);
	ZHRotate( _Direction, ZHCoeffs, _Coeffs );
}

// Builds a spherical harmonics smooth cone lobe
// The light source intensity is 1 at theta=0 and 0 at theta=half angle
// (from "Stupid SH Tricks")
//
void	EffectGlobalIllum2::BuildSHSmoothCone( const NjFloat3& _Direction, float _HalfAngle, double _Coeffs[9] )
{
	double	a = _HalfAngle;
	float	One_a3 = 1.0f / float(a*a*a);
	double	c = cos( a );
	double	s = sin( a );
	NjFloat3 ZHCoeffs = One_a3 * NjFloat3(
			float( 1.7724538509055160272981674833411 * (a * (6.0*(1+c) + a*a) - 12*s) ),					// sqrt(PI) (a^3 + 6a - 12*sin(a) + 6*cos(a)*a) / a^3
			float( 0.76749503095986636635966371866695 * (a * (a*a + 3*c*c) - 3*c*s) ),						// 0.25 sqrt(3PI) (a^3 - 3*cos(a)*sin(a) + 3*cos(a)^2*a) / a^3
			float( 0.44036969973400122370500319612446 * (-6.0*a -2*c*c*s -9.0*c*a + 14.0*s + 3*c*c*c*a))	// 1/9 sqrt(5PI) (-6a - 2*cos(a)^2*sin(a) - 9*cos(a)*a + 14*sin(a) + 3*cos(a)^3*a) / a^3
		);
	ZHRotate( _Direction, ZHCoeffs, _Coeffs );
}

// Rotates ZH coefficients in the specified direction (from "Stupid SH Tricks")
// Rotating ZH comes to evaluating scaled SH in the given direction.
// The scaling factors for each band are equal to the ZH coefficients multiplied by sqrt( 4PI / (2l+1) )
//
void	EffectGlobalIllum2::ZHRotate( const NjFloat3& _Direction, const NjFloat3& _ZHCoeffs, double _Coeffs[9] )
{
	double	cl0 = 3.5449077018110320545963349666823 * _ZHCoeffs.x;	// sqrt(4PI)
	double	cl1 = 2.0466534158929769769591032497785 * _ZHCoeffs.y;	// sqrt(4PI/3)
	double	cl2 = 1.5853309190424044053380115060481 * _ZHCoeffs.z;	// sqrt(4PI/5)

	double	f0 = cl0 * 0.28209479177387814347403972578039;	// 0.5 / sqrt(PI);
	double	f1 = cl1 * 0.48860251190291992158638462283835;	// 0.5 * sqrt(3.0/PI);
	double	f2 = cl2 * 1.0925484305920790705433857058027;	// 0.5 * sqrt(15.0/PI);
	_Coeffs[0] = f0;
	_Coeffs[1] = -f1 * _Direction.x;
	_Coeffs[2] = f1 * _Direction.y;
	_Coeffs[3] = -f1 * _Direction.z;
	_Coeffs[4] = f2 * _Direction.x * _Direction.z;
	_Coeffs[5] = -f2 * _Direction.x * _Direction.y;
	_Coeffs[6] = f2 * 0.28209479177387814347403972578039 * (3.0 * _Direction.y*_Direction.y - 1.0);
	_Coeffs[7] = -f2 * _Direction.z * _Direction.y;
	_Coeffs[8] = f2 * 0.5f * (_Direction.z*_Direction.z - _Direction.x*_Direction.x);
}

void*	EffectGlobalIllum2::TagMaterial( const Scene::Material& _Material ) const
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	switch ( _Material.m_ID )
	{
	case 0:	return m_pMatRender;
	case 1:	return m_pMatRender;
	default:
		ASSERT( false, "Unsupported material!" );
	}
	return NULL;
}
void*	EffectGlobalIllum2::TagTexture( const Scene::Material::Texture& _Texture ) const
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
void*	EffectGlobalIllum2::TagNode( const Scene::Node& _Node ) const
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	return NULL;
}
void*	EffectGlobalIllum2::TagPrimitive( const Scene::Mesh& _Mesh, const Scene::Mesh::Primitive& _Primitive ) const
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

	return pPrim;
}

void	EffectGlobalIllum2::RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride ) const
{
	// Upload the object's CB
	memcpy( &m_pCB_Object->m.Local2World, &_Mesh.m_Local2World, sizeof(NjFloat4x4) );
//m_pCB_Object->m.Local2World = NjFloat4x4::Identity;
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
		m_pCB_Material->m.DiffuseColor = SceneMaterial.m_DiffuseAlbedo;
		m_pCB_Material->m.HasDiffuseTexture = pTexDiffuseAlbedo != NULL;
		m_pCB_Material->m.SpecularColor = SceneMaterial.m_SpecularAlbedo;
		m_pCB_Material->m.HasSpecularTexture = pTexSpecularAlbedo != NULL;
		m_pCB_Material->m.SpecularExponent = SceneMaterial.m_SpecularExponent.x;
		m_pCB_Material->UpdateData();

		// Render
		pMat->Use();
		pPrim->Render( *pMat );
	}
}
