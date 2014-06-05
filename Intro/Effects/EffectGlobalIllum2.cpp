//////////////////////////////////////////////////////////////////////////
// How to properly use that code sample and make your own scenes:
//
// 1) Creating the scene (I'm using Maya and the FBX SDK v2014.2)
//	_ Prepare a scene in Maya:
//		_ I'm only supporting standard lambert/phong materials here since using advanced shaders is not the purpose of this code
//		_ Place point and spot lights in your scene: they will be considered static lighting
//		_ Place locators in your scene at relevant positions: they will be considered light probes.
//			=> It's important to place them where light bounces are important, near the walls (but not too near)
//				and areas of quick lighting changes, near colored surfaces to capture color bleeding adequately
//		_ Use a non-zero "Incandescence" value in materials to hint that the material will be used as a dynamic area light
//	_ Export as FBX
//	_ Convert to GCX (God Complex scene format)
//		=> Using the Tools > FBX2GCX you can convert a standard FBX scene into a GCX
//	_ Patch the code to make it load your scene
//		=> Taking model below with the SCENE_SPONZA define, create the same pattern for your scene (i.e. create PROBES_PATH, USE_PER_VERTEX_PROBE_ID, etc.)
//		=> Create a disk folder where probe infos will be stored, make PROBES_PATH point to it (usually, a "Probes" sub-directory in the scene's directory)
//		=> Text-edit the GodComplex.rc resource file, locate the line where IDR_SCENE_GI is defined, copy/paste/comment and patch with your scene's path
//
// 2) Converting textures
//	_ Place any textures for your scene anywhere you like (preferably in a "Textures" folder in the scene's directory)
//		=> Prefer using PNGs! Otherwise, batch convert any format to PNG using photoshop or anything
//	_ Converting to GCX scene, if successful, should have shown a popup dialog with a list of texture names
//		=> Copy that list of texture names (ending with the .POM extension)
//		=> Paste it in the code below where you can see similar lists
//		=> You just hinted the code of which texture corresponds to which texture ID in the GCX scene
//	_ Convert your textures to POM
//		=> Using the Tools > PNG2POM you can batch convert your PNGs into POM format
//		=> Target directory should be in a "TexturesPOM" folder in the scene's directory, to match the paths you copied in the code
//
// 3) Generating probe cube maps
//	_ Make sure USE_WHITE_TEXTURES is commented (i.e. undefined) because you want to use your scene's textures to get proper color bounces
//	_ Make sure LOAD_PROBES is commented (i.e. undefined): this will hint the code that we're actually COMPUTING the probes for the first time
//	_ Run
//		=> If successful, this should take some time to load the scene, render the probes' cube maps, then it should display the scene
//			using plain direct lighting only
//		=> Probe cube maps are now ready to be analyzed and processed to generate probe informations that will be used for indirect lighting
//		=> Go to the directory pointed by PROBES_PATH and make sure you have as many Probe??.POM files as probes you placed in your scene
//
// 4) Processing probe cube maps and generating probe infos
//	_ Launch the Tools > ProbeSHEncoder project
//	_ Use Main Menu > File > Batch Encode
//		=> Point it to the directory where the probe cube maps have been saved, prefer using the same directory as target
//	_ Wait until all probes have been converted
//		=> You should now have an equal number of .PROBESET files
//	_ Click the Main Menu > Tools > Encode Face Probes action
//		=> Locate the .PIM file that should have been generated at the same place as the .PROBESET files
//		=> Locate your .GCX scene
//	_ It should show a dialog with informations on what went right/wrong in your scene
//		=> Typically, faces/primitives that don't have probe informations are faces/primitives too far away from any probe and disconnected from other faces/primitives
//		=> After that, you just generated an additional vertex stream for each of your scene's vertices that contains the index of the closest probe
//	_ You now have all the informations needed to render the scene
//
// 5) Final result
//	_ Make sure LOAD_PROBES is not commented (i.e. defined): this will hint the code that we're not LOADING and USING the probes for indirect lighting
//	_ Run
//		=> Use WASD/QSDZ to navigate the scene, shift to speed up
//	_ Also run the Tools > ControlPanelGlobalIllumination project
//		=> This will provide you with the options to control the demo, like enabling/disabling Sun, Sky, dynamic light, emissive area lights, etc.
//	_ Enjoy...
//
//
// More questions? contact.patapom[at]patapom.com
//
//////////////////////////////////////////////////////////////////////////
//
#include "../../GodComplex.h"
#include "EffectGlobalIllum2.h"

//#define SCENE_CORRIDOR		// Simple corridor
#define SCENE_CITY			// City
//#define SCENE_SPONZA		// Sponza Atrium

//#define	LOAD_PROBES			// Define this to load probes instead of computing them
//#define USE_WHITE_TEXTURES	// Define this to use a single white texture for the entire scene (low patate machines)
#define	USE_NORMAL_MAPS			// Define this to use normal maps

// Scene selection (also think about changing the scene in the .RC!)
#ifdef SCENE_CORRIDOR
#define PROBES_PATH				".\\Resources\\Scenes\\GITest1\\ProbeSets\\GITest1_10Probes\\"
#ifdef LOAD_PROBES	// Can't use that until it's been baked!
#define USE_PER_VERTEX_PROBE_ID	".\\Resources\\Scenes\\GITest1_ProbeID.vertexStream.U16"
#endif

#elif defined(SCENE_SPONZA)
#define PROBES_PATH				".\\Resources\\Scenes\\Sponza\\Probes\\"
#ifdef LOAD_PROBES	// Can't use that until it's been baked!
#define USE_PER_VERTEX_PROBE_ID	".\\Resources\\Scenes\\Sponza\\Sponza_ProbeID.vertexStream.U16"
#endif

#elif defined(SCENE_CITY)
//#define TEXTURES_PATH			"..\\Arkane\\TexturesPOM\\"
#define TEXTURES_PATH			".\\Resources\\Scenes\\Arkane\\TexturesPOM\\"
//#define PROBES_PATH				"..\\Arkane\\Probes\\City\\"
#define PROBES_PATH				".\\Resources\\Scenes\\Arkane\\Probes\\City\\"
#ifdef LOAD_PROBES	// Can't use that until it's been baked!
//#define USE_PER_VERTEX_PROBE_ID	"..\\Arkane\\City_ProbeID.vertexStream.U16"
//#define USE_PER_VERTEX_PROBE_ID	"..\\Arkane\\City_ProbeID.vertexStream.U16"
#define USE_PER_VERTEX_PROBE_ID	".\\Resources\\Scenes\\Arkane\\City_ProbeID.vertexStream.U16"
#endif

#endif


#define CHECK_MATERIAL( pMaterial, ErrorCode )		if ( (pMaterial)->HasErrors() ) m_ErrorCode = ErrorCode;

EffectGlobalIllum2::EffectGlobalIllum2( Device& _Device, Texture2D& _RTHDR, Primitive& _ScreenQuad, Camera& _Camera )
	: m_ErrorCode( 0 )
	, m_Device( _Device )
	, m_RTTarget( _RTHDR )
	, m_ScreenQuad( _ScreenQuad )
	, m_pVertexStreamProbeIDs( NULL )
	, m_pPrimProbeIDs( NULL )
	, m_ProbeUpdateIndex( 0 )
{
	//////////////////////////////////////////////////////////////////////////
	// Create the materials
	m_SceneVertexFormatDesc.AggregateVertexFormat( VertexFormatP3N3G3B3T2::DESCRIPTOR );

	{
// Main scene rendering is quite heavy so we prefer to reload it from binary instead
//ScopedForceMaterialsLoadFromBinary		bisou;

#ifdef USE_PER_VERTEX_PROBE_ID
		D3D_SHADER_MACRO	pMacros[] = { { "USE_SHADOW_MAP", "1" }, { "PER_VERTEX_PROBE_ID", "1" }, { NULL, NULL } };
		m_SceneVertexFormatDesc.AggregateVertexFormat( VertexFormatU32::DESCRIPTOR );
#else
		D3D_SHADER_MACRO	pMacros[] = { { "USE_SHADOW_MAP", "1" }, { NULL, NULL } };
#endif
 		CHECK_MATERIAL( m_pMatRender = CreateMaterial( IDR_SHADER_GI_RENDER_SCENE, "./Resources/Shaders/GIRenderScene2.hlsl", m_SceneVertexFormatDesc, "VS", NULL, "PS", pMacros ), 1 );

		D3D_SHADER_MACRO	pMacros2[] = { { "EMISSIVE", "1" }, { NULL, NULL } };
		CHECK_MATERIAL( m_pMatRenderEmissive = CreateMaterial( IDR_SHADER_GI_RENDER_SCENE, "./Resources/Shaders/GIRenderScene2.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS", pMacros2 ), 2 );
//		CHECK_MATERIAL( m_pMatRenderEmissive = CreateMaterial( IDR_SHADER_GI_RENDER_SCENE, "./Resources/Shaders/GIRenderScene2.hlsl", m_SceneVertexFormatDesc, "VS", NULL, "PS", pMacros2 ), 2 );
	}
	{
ScopedForceMaterialsLoadFromBinary		bisou;

 		CHECK_MATERIAL( m_pMatRenderCubeMap = CreateMaterial( IDR_SHADER_GI_RENDER_CUBEMAP, "./Resources/Shaders/GIRenderCubeMap.hlsl", VertexFormatP3N3G3B3T2::DESCRIPTOR, "VS", NULL, "PS" ), 3 );
 		CHECK_MATERIAL( m_pMatRenderShadowMap = CreateMaterial( IDR_SHADER_GI_RENDER_SHADOW_MAP, "./Resources/Shaders/GIRenderShadowMap.hlsl", VertexFormatP3::DESCRIPTOR, "VS", NULL, NULL ), 4 );
 		CHECK_MATERIAL( m_pMatRenderShadowMapPoint = CreateMaterial( IDR_SHADER_GI_RENDER_SHADOW_MAP, "./Resources/Shaders/GIRenderShadowMap.hlsl", VertexFormatP3::DESCRIPTOR, "VS2", "GS", NULL ), 5 );

 		CHECK_MATERIAL( m_pMatPostProcess = CreateMaterial( IDR_SHADER_GI_POST_PROCESS, "./Resources/Shaders/GIPostProcess.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 6 );
 		CHECK_MATERIAL( m_pMatRenderLights = CreateMaterial( IDR_SHADER_GI_RENDER_LIGHTS, "./Resources/Shaders/GIRenderLights.hlsl", VertexFormatP3N3::DESCRIPTOR, "VS", NULL, "PS" ), 7 );
 		CHECK_MATERIAL( m_pMatRenderDynamic = CreateMaterial( IDR_SHADER_GI_RENDER_DYNAMIC, "./Resources/Shaders/GIRenderDynamic.hlsl", VertexFormatP3N3G3T2::DESCRIPTOR, "VS", NULL, "PS" ), 8 );
 		CHECK_MATERIAL( m_pMatRenderDebugProbes = CreateMaterial( IDR_SHADER_GI_RENDER_DEBUG_PROBES, "./Resources/Shaders/GIRenderDebugProbes.hlsl", VertexFormatP3N3::DESCRIPTOR, "VS", NULL, "PS" ), 9 );
 		CHECK_MATERIAL( m_pMatRenderDebugProbesNetwork = CreateMaterial( IDR_SHADER_GI_RENDER_DEBUG_PROBES, "./Resources/Shaders/GIRenderDebugProbes.hlsl", VertexFormatP3::DESCRIPTOR, "VS_Network", "GS_Network", "PS_Network" ), 10 );
 		CHECK_MATERIAL( m_pMatRenderNeighborProbe = CreateMaterial( IDR_SHADER_GI_RENDER_NEIGHBOR_PROBE, "./Resources/Shaders/GIRenderNeighborProbe.hlsl", VertexFormatPt4::DESCRIPTOR, "VS", NULL, "PS" ), 11 );
	}
	{
// This one is REALLY heavy! So build it once and reload it from binary forever again
ScopedForceMaterialsLoadFromBinary		bisou;
		// Compute Shaders
 		CHECK_MATERIAL( m_pCSUpdateProbe = CreateComputeShader( IDR_SHADER_GI_UPDATE_PROBE, "./Resources/Shaders/GIUpdateProbe.hlsl", "CS" ), 20 );
	}

m_pCSComputeShadowMapBounds = NULL;	// TODO!


	//////////////////////////////////////////////////////////////////////////
	// Create the textures
	{
#ifndef	USE_WHITE_TEXTURES

		const char*	ppTextureFileNames[] = {

#ifdef SCENE_CORRIDOR
		"./Resources/Scenes/GITest1/pata_diff_colo.pom",

#elif defined(SCENE_SPONZA)
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_thorn_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_thorn_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_thorn_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\vase_plant.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\vase_plant.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\vase_round.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\vase_round_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\background.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\background_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\spnza_bricks_a_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_arch_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_ceiling_a_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_column_a_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_column_a_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_floor_a_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_column_c_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_column_c_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_details_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_column_b_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_column_b_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_flagpole_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_fabric_green_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_fabric_blue_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_fabric_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_curtain_blue_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_curtain_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_curtain_green_diff.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\chain_texture.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\chain_texture.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\chain_texture_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\vase_hanging.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\vase_dif.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\vase_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\lion.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\lion_ddn.pom",
".\\Resources\\Scenes\\Sponza\\TexturesPOM\\sponza_roof_diff.pom",
#elif defined(SCENE_CITY)
"floor_tiles_ornt_int_01_d.pom",
"floor_tiles_ornt_int_01_n.pom",
"floor_tiles_ornt_int_01_s.pom",
"macadam_01_d.pom",
"macadam_01_n.pom",
"macadam_01_s.pom",
"wooden_cobble_01_d.pom",
"wooden_cobble_01_n.pom",
"wooden_cobble_01_s.pom",
"crate_01_d.pom",
"crate_01_n.pom",
"crate_01_s.pom",
"tarp_02_d.pom",
"tarp_02_n.pom",
"tarp_02_s.pom",
"rich_small_wall_clean_01_d.pom",
"rich_small_wall_clean_01_s.pom",
"concrete_02_d.pom",
"sand_01_d.pom",
"sand_01_n.pom",
"sand_01_s.pom",
"shop_poster_01_d.pom",
"shop_poster_01_n.pom",
"shop_poster_01_s.pom",
"moss_02_d.pom",
"rich_medium_stone_01_d.pom",
"rich_medium_stone_01_n.pom",
"glass_01_d.pom",
"rich_medium_details_01_d.pom",
"rich_medium_details_01_n.pom",
"rich_medium_details_01_s.pom",
"rich_small_details_03_d.pom",
"rich_small_details_03_n.pom",
"rich_small_details_03_s.pom",
"rich_small_details_01_d.pom",
"rich_small_details_01_s.pom",
"rich_small_details_02_d.pom",
"rich_small_details_02_s.pom",
"wood_05_d.pom",
"white_stone_01_d.pom",
"white_stone_01_n.pom",
"white_stone_01_s.pom",
"door_blocker_iron_01_d.pom",
"door_blocker_iron_01_n.pom",
"door_blocker_iron_01_s.pom",
"safe_shop_sign_01_d.pom",
"safe_shop_sign_01_n.pom",
"safe_shop_sign_01_s.pom",
"safe_shop_sign_02_d.pom",
"safe_shop_sign_02_n.pom",
"safe_shop_sign_02_s.pom",
"shop_doordeco_01_d.pom",
"shop_doordeco_01_n.pom",
"shop_doordeco_01_s.pom",
"safe_shop_door_frame_01_d.pom",
"safe_shop_door_frame_01_n.pom",
"safe_shop_door_frame_01_s.pom",
"rich_medium_details_02_d.pom",
"rich_medium_details_02_n.pom",
"rich_medium_details_02_s.pom",
"shop_workshopwall_01_d.pom",
"shop_workshopwall_01_n.pom",
"shop_workshopwall_01_s.pom",
"shop_workshopwall_02_d.pom",
"shop_workshopwall_02_n.pom",
"shop_workshopwall_02_s.pom",
"shop_ceiling_01_d.pom",
"shop_ceiling_01_n.pom",
"shop_ceiling_01_s.pom",
"shop_painting_01_d.pom",
"shop_painting_01_n.pom",
"shop_painting_01_s.pom",
"shop_light_wall_01_d.pom",
"shop_light_wall_01_n.pom",
"shop_light_wall_01_s.pom",
"shop_pillar_01_d.pom",
"shop_pillar_01_n.pom",
"shop_pillar_01_s.pom",
"shop_flat_01_d.pom",
"shop_flat_01_n.pom",
"shop_flat_01_s.pom",
"shop_stairs_02_d.pom",
"shop_stairs_02_n.pom",
"shop_stairs_02_s.pom",
"shopbookcase_02_d.pom",
"shopbookcase_02_n.pom",
"shopbookcase_02_s.pom",
"shopcounter_01_d.pom",
"shopcounter_01_n.pom",
"shopcounter_01_s.pom",
"over_desk_lamp_02_d.pom",
"over_desk_lamp_02_n.pom",
"over_desk_lamp_02_s.pom",
"shop_box_kit_01_d.pom",
"shop_box_kit_01_n.pom",
"shop_box_kit_01_s.pom",
"modular_stairs_01_d.pom",
"modular_stairs_01_n.pom",
"modular_stairs_01_s.pom",
"metal_crate_01_d.pom",
"metal_crate_01_n.pom",
"metal_crate_01_s.pom",
"safe_shop_metal_01_d.pom",
"safe_shop_metal_01_n.pom",
"safe_shop_metal_01_s.pom",
"safe_shop_wood_01_d.pom",
"safe_shop_wood_01_n.pom",
"safe_shop_wood_01_s.pom",
"window_d.pom",
"streetlamp_01_d.pom",
"streetlamp_01_n.pom",
"streetlamp_01_s.pom",
#endif
		};

		m_TexturesCount = sizeof(ppTextureFileNames) / sizeof(const char*);
		m_ppTextures = new Texture2D*[m_TexturesCount];

		static char	pTemp[1024];
		for ( int TextureIndex=0; TextureIndex < m_TexturesCount; TextureIndex++ )
		{
			const char*	pTextureFileName = ppTextureFileNames[TextureIndex];
			sprintf_s( pTemp, "%s%s", TEXTURES_PATH, pTextureFileName );

			TextureFilePOM	POM( pTemp );
			m_ppTextures[TextureIndex] = new Texture2D( _Device, POM );
		}

#else	//#ifndef	USE_WHITE_TEXTURES

		m_TexturesCount = 2;
		m_ppTextures = new Texture2D*[m_TexturesCount];

		const float	Albedo = 0.5f;

		TextureBuilder	White( 1, 1 );
		White.Clear( Pixel( float4( Albedo, Albedo, Albedo, 1 ) ) );
		m_ppTextures[0] = White.CreateTexture( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA );

		TextureBuilder	NormalZ( 1, 1 );
		NormalZ.Clear( Pixel( float4( 0.5f, 0.5f, 1, 1 ) ) );
		m_ppTextures[1] = NormalZ.CreateTexture( PixelFormatRGBA8::DESCRIPTOR, TextureBuilder::CONV_RGBA );
#endif

		// Load the dynamic objects' normal map
		{
			TextureFilePOM	DynamicNormal( "./Resources/Images/Normal.POM" );
			m_pTexDynamicNormalMap = new Texture2D( m_Device, DynamicNormal );
		}
	}

	// Create the shadow map
	m_pRTShadowMap = new Texture2D( _Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );
	m_pRTShadowMapPoint = new Texture2D( _Device, SHADOW_MAP_POINT_SIZE, SHADOW_MAP_POINT_SIZE, DepthStencilFormatD32F::DESCRIPTOR, 6 );


	//////////////////////////////////////////////////////////////////////////
	// Create the constant buffers
	m_pCB_General = new CB<CBGeneral>( _Device, 8, true );
	m_pCB_Scene = new CB<CBScene>( _Device, 9, true );
 	m_pCB_Object = new CB<CBObject>( _Device, 10 );
	m_pCB_DynamicObject = new CB<CBDynamicObject>( _Device, 10 );
 	m_pCB_Material = new CB<CBMaterial>( _Device, 11 );
	m_pCB_Probe = new CB<CBProbe>( _Device, 10 );
	m_pCB_Splat = new CB<CBSplat>( _Device, 10 );
	m_pCB_ShadowMap = new CB<CBShadowMap>( _Device, 2, true );
	m_pCB_ShadowMapPoint = new CB<CBShadowMapPoint>( _Device, 3, true );
	m_pCB_UpdateProbes = new CB<CBUpdateProbes>( _Device, 10 );

	m_pCB_Scene->m.DynamicLightsCount = 0;
	m_pCB_Scene->m.StaticLightsCount = 0;
	m_pCB_Scene->m.ProbesCount = 0;


	//////////////////////////////////////////////////////////////////////////
	// Create the lights & probes structured buffers
	m_pSB_LightsStatic = new SB<LightStruct>( m_Device, MAX_LIGHTS, true );
	m_pSB_LightsDynamic = new SB<LightStruct>( m_Device, MAX_LIGHTS, true );
	m_pSB_RuntimeProbes = NULL;
	m_pSB_RuntimeProbeNetworkInfos = NULL;

	m_pSB_RuntimeProbeUpdateInfos = new SB<RuntimeProbeUpdateInfos>( m_Device, MAX_PROBE_UPDATES_PER_FRAME, true );
	m_pSB_RuntimeProbeSetInfos = new SB<RuntimeProbeUpdateSetInfos>( m_Device, MAX_PROBE_UPDATES_PER_FRAME*MAX_PROBE_SETS, true );
	m_pSB_RuntimeProbeEmissiveSetInfos = new SB<RuntimeProbeUpdateEmissiveSetInfos>( m_Device, MAX_PROBE_UPDATES_PER_FRAME*MAX_PROBE_EMISSIVE_SETS, true );
	m_pSB_RuntimeSamplingPointInfos = new SB<RuntimeSamplingPointInfos>( m_Device, MAX_PROBE_UPDATES_PER_FRAME * MAX_SET_SAMPLES, true );


#ifdef USE_PER_VERTEX_PROBE_ID

	//////////////////////////////////////////////////////////////////////////
	// Load the vertex stream containing U32-packed probe IDs for each vertex
	FILE*	pFile = NULL;
	fopen_s( &pFile, USE_PER_VERTEX_PROBE_ID, "rb" );
	ASSERT( pFile != NULL, "Vertex stream for probe IDs file not found!" );
	fseek( pFile, 0, SEEK_END );
	int	FileSize = int(ftell( pFile ));
	fseek( pFile, 0, SEEK_SET );

	m_VertexStreamProbeIDsLength = FileSize / sizeof(U32);
	m_pVertexStreamProbeIDs = new U32[m_VertexStreamProbeIDsLength];
	fread_s( m_pVertexStreamProbeIDs, FileSize, sizeof(U32), m_VertexStreamProbeIDsLength, pFile );

	fclose( pFile );

	m_pPrimProbeIDs = new Primitive( _Device, m_VertexStreamProbeIDsLength, m_pVertexStreamProbeIDs, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_POINTLIST, VertexFormatU32::DESCRIPTOR );

#endif


	//////////////////////////////////////////////////////////////////////////
	// Create the scene
	m_bDeleteSceneTags = false;
	m_TotalFacesCount = 0;
	m_TotalVerticesCount = 0;
	m_TotalPrimitivesCount = 0;
	m_EmissiveMaterialsCount = 0;
	m_Scene.Load( IDR_SCENE_GI, *this );

	// Upload static lights once and for all
	m_pSB_LightsStatic->Write( m_pCB_Scene->m.StaticLightsCount );
	m_pSB_LightsStatic->SetInput( 7, true );

	// Update once so it's ready when we pre-compute probes
	m_pCB_Scene->UpdateData();


	// Cache meshes & probes since my ForEach function is slow as hell!! ^^
	{
		m_MeshesCount = 0;
		m_ProbesCount = 0;

		class	VisitorCountNodes : public Scene::IVisitor
		{
			EffectGlobalIllum2&	m_Owner;
		public:
			VisitorCountNodes( EffectGlobalIllum2& _Owner ) : m_Owner( _Owner ) {}
			void	HandleNode( Scene::Node& _Node ) override
			{
				if ( _Node.m_Type == Scene::Node::MESH )
					m_Owner.m_MeshesCount++;
				else if ( _Node.m_Type == Scene::Node::PROBE )
					m_Owner.m_ProbesCount++;
			}
		}	Visitor0( *this );
		m_Scene.ForEach( Visitor0 );

		m_ppCachedMeshes = new Scene::Mesh*[m_MeshesCount];
		m_pProbes = new ProbeStruct[m_ProbesCount];

		m_MeshesCount = 0;
		m_ProbesCount = 0;

		class	VisitorStoreNodes : public Scene::IVisitor
		{
			EffectGlobalIllum2&	m_Owner;
		public:
			int		m_PrimitivesCount;
			int		m_VerticesCount;
			int		m_FacesCount;
			VisitorStoreNodes( EffectGlobalIllum2& _Owner ) : m_Owner( _Owner ), m_PrimitivesCount( 0 ), m_FacesCount( 0 ), m_VerticesCount( 0 ) {}
			void	HandleNode( Scene::Node& _Node ) override
			{
				if ( _Node.m_Type == Scene::Node::MESH )
				{
					Scene::Mesh*	pMesh = (Scene::Mesh*) &_Node;
					m_Owner.m_ppCachedMeshes[m_Owner.m_MeshesCount++] = pMesh;
					m_PrimitivesCount += pMesh->m_PrimitivesCount;
					for ( int i=0; i < pMesh->m_PrimitivesCount; i++ )
					{
						m_VerticesCount += pMesh->m_pPrimitives[i].m_VerticesCount;
						m_FacesCount += pMesh->m_pPrimitives[i].m_FacesCount;
					}
				}
				else if ( _Node.m_Type == Scene::Node::PROBE )
				{
					m_Owner.m_pProbes[m_Owner.m_ProbesCount].ProbeID = m_Owner.m_ProbesCount;
					m_Owner.m_pProbes[m_Owner.m_ProbesCount].pSceneProbe = (Scene::Probe*) &_Node;
					m_Owner.m_ProbesCount++;
				}
			}
		}	Visitor1( *this );
		m_Scene.ForEach( Visitor1 );

		int		VerticesCount = Visitor1.m_VerticesCount;
		int		FacesCount = Visitor1.m_FacesCount;
	}


	//////////////////////////////////////////////////////////////////////////
	// Create our sphere primitive for displaying lights & probes
	m_pPrimSphere = new Primitive( _Device, VertexFormatP3N3G3T2::DESCRIPTOR );
	GeometryBuilder::BuildSphere( 40, 10, *m_pPrimSphere );

	// Create the dummy point primitive for the debug drawing of the probes network
	float3	Point;
	m_pPrimPoint = new Primitive( _Device, 1, &Point, 0, NULL, D3D11_PRIMITIVE_TOPOLOGY_POINTLIST, VertexFormatP3::DESCRIPTOR );


	//////////////////////////////////////////////////////////////////////////
	// Start precomputation/loading of probes
	PreComputeProbes();


	//////////////////////////////////////////////////////////////////////////
	// Build the ambient sky SH using the CIE overcast sky model
	//
	const int	MAX_THETA = 40;
	double		dPhidTheta = (PI / MAX_THETA) * (2*PI / (2*MAX_THETA));

	double		SumSHCoeffs[3*9];
	memset( SumSHCoeffs, 0, 3*9*sizeof(double) );

	double		SHCoeffs[9];
	double		SumSolidAngle = 0.0;
	for ( int ThetaIndex=0; ThetaIndex < MAX_THETA; ThetaIndex++ )
	{
		float	Theta = PI * (0.5f+ThetaIndex) / MAX_THETA;
		for ( int PhiIndex=0; PhiIndex < 2*MAX_THETA; PhiIndex++ )
		{
			float		Phi = PI * PhiIndex / MAX_THETA;
			float3	Direction( sinf(Phi) * sinf(Theta), cosf(Theta), cosf(Phi)*sinf(Theta) );
			SH::BuildSHCoeffs_YUp( Direction, SHCoeffs );

			float3	SkyColor = (1.0f + 2.0f * MAX( -0.5f, cosf(Theta) )) / 3.0f * float3::One;
//			float3	SkyColor = 1.0f;	// Simple ambient sky...

			double		SolidAngle = sinf(Theta) * dPhidTheta;
			SumSolidAngle += SolidAngle;

			for ( int l=0; l < 3; l++ )
			{
				float3	FilteredIntensity = SkyColor * expf( -(PI * l / 3.0f) * (PI * l / 3.0f) / 2.0f );
				for ( int m=-l; m <= l; m++ )
				{
					int		CoeffIndex = l*(l+1)+m;
					double	SHCoeff = SHCoeffs[CoeffIndex] * SolidAngle;
					SumSHCoeffs[3*CoeffIndex+0] += FilteredIntensity.x * SHCoeff;
					SumSHCoeffs[3*CoeffIndex+1] += FilteredIntensity.y * SHCoeff;
					SumSHCoeffs[3*CoeffIndex+2] += FilteredIntensity.z * SHCoeff;
				}
			}
		}
	}

	for ( int i=0; i < 9; i++ )
		m_pSHAmbientSky[i] = float( SKY_INTENSITY * INV4PI * SumSHCoeffs[3*i+0] ) * float3( 0.64f, 0.79f, 1.0f );


	//////////////////////////////////////////////////////////////////////////
	// Initialize the memory mapped file for remote control (control panel is available through the Tools/ControlPanelGlobalIllumination project)
	//
#ifdef _DEBUG
	m_pMMF = new MMF<ParametersBlock>( "GlobalIllumination" );
	ParametersBlock	Params = {
		1, // WILL BE MARKED AS CHANGED!			// U32		Checksum;

		// Atmosphere Params
		false,				// U32		EnableSun;
		DEG2RAD( 60.0f ),	// float	SunTheta;
		0.0f,				// float	SunPhi;
		SUN_INTENSITY,		// float	SunIntensity;
		// 
		false,				// U32		EnableSky;
		SKY_INTENSITY,		// float	SkyIntensity;
		0.64f, 0.79f, 1.0f,	// float	SkyColorR, G, B;
		// 
		// Dynamic lights params
		true,				// U32		EnablePointLight;
		true,				// U32		AnimatePointLight;
		100.0f,				// float	PointLightIntensity;
		1.0f, 1.0f, 1.0f,	// float	PointLightColorR, G, B;
		// 
		// Static lighting params
		true,				// U32		EnableStaticLighting;
		// 
		// Emissive params
		false,				// U32		EnableEmissiveMaterials;
		4.0f,				// float	EmissiveIntensity;
		1.0f, 0.95f, 0.5f,	// float	EmissiveColorR, G, B;
		//		
		// Dynamic objects
		0,					// U32		DynamicObjectsCount;
		// 
		// Bounce params
		100.0f,				// float	BounceFactorSun;
		100.0f,				// float	BounceFactorSky;
		100.0f,				// float	BounceFactorPoint;
		100.0f,				// float	BounceFactorStaticLights;
		100.0f,				// float	BounceFactorEmissive;
		//
		// Neighborhood
		true,				// U32		EnableNeighborsRedistribution;
		1.0f,				// float	NeighborProbesContributionBoost;
		//
		// Probes update
		16,					// U32		MaxProbeUpdatesPerFrame;
		//
		// Misc
		false,				// U32		ShowDebugProbes;
		false,				// U32		ShowDebugProbes;
		1.0f,				// float	DebugProbesIntensity;
	};
	ParametersBlock&	MappedParams = m_pMMF->GetMappedMemory();

	// Copy our default params only if the checksum is 0 (meaning the control panel isn't loaded and hasn't set any value yet)
	if ( MappedParams.Checksum == 0 )
		MappedParams = Params;

#endif
}
Texture2D*	pRTCubeMap;

EffectGlobalIllum2::~EffectGlobalIllum2()
{
	delete[] m_pProbes;

	delete m_pPrimPoint;
	delete m_pPrimSphere;

	delete m_pPrimProbeIDs;
	delete[] m_pVertexStreamProbeIDs;

	delete[] m_ppCachedMeshes;
	m_MeshesCount = 0;

	m_bDeleteSceneTags = true;
	m_Scene.ClearTags( *this );

	delete m_pSB_RuntimeSamplingPointInfos;
	delete m_pSB_RuntimeProbeEmissiveSetInfos;
	delete m_pSB_RuntimeProbeSetInfos;
	delete m_pSB_RuntimeProbeUpdateInfos;
	delete m_pSB_RuntimeProbeNetworkInfos;
	delete m_pSB_RuntimeProbes;
	delete m_pSB_LightsDynamic;
	delete m_pSB_LightsStatic;

	delete m_pCB_UpdateProbes;
	delete m_pCB_ShadowMapPoint;
	delete m_pCB_ShadowMap;
	delete m_pCB_Splat;
	delete m_pCB_Probe;
	delete m_pCB_Material;
	delete m_pCB_DynamicObject;
	delete m_pCB_Object;
	delete m_pCB_Scene;
	delete m_pCB_General;

	delete m_pRTShadowMapPoint;
	delete m_pRTShadowMap;

	delete m_pTexDynamicNormalMap;
	for ( int TextureIndex=0; TextureIndex < m_TexturesCount; TextureIndex++ )
		delete m_ppTextures[TextureIndex];
	delete[] m_ppTextures;

	delete m_pCSUpdateProbe;

	delete m_pMatPostProcess;
	delete m_pCSComputeShadowMapBounds;
	delete m_pMatRenderShadowMapPoint;
	delete m_pMatRenderShadowMap;
	delete m_pMatRenderNeighborProbe;
	delete m_pMatRenderCubeMap;
	delete m_pMatRenderDebugProbesNetwork;
	delete m_pMatRenderDebugProbes;
	delete m_pMatRenderDynamic;
	delete m_pMatRenderLights;
	delete m_pMatRenderEmissive;
	delete m_pMatRender;

//###
delete pRTCubeMap;
}

float		AnimateLightTime0 = 0.0f;
float		AnimateDynamicObjects = 0.0f;

#define RENDER_SUN	1

void	EffectGlobalIllum2::Render( float _Time, float _DeltaTime )
{
	// Setup general data
	m_pCB_General->m.ShowIndirect = gs_WindowInfos.pKeys[VK_RETURN] == 0;
	m_pCB_General->m.ShowOnlyIndirect = gs_WindowInfos.pKeys[VK_BACK] == 0;
	m_pCB_General->m.ShowWhiteDiffuse = gs_WindowInfos.pKeys[VK_DELETE] != 0;
	m_pCB_General->m.ShowVertexProbeID = gs_WindowInfos.pKeys[VK_INSERT] != 0;
	m_pCB_General->m.Ambient = !m_pCB_General->m.ShowIndirect && m_CachedCopy.EnableSky ? 0.25f * float3( 0.64f, 0.79f, 1.0f ) : float3::Zero;
	m_pCB_General->UpdateData();

	// Setup scene data
	m_pCB_Scene->m.DynamicLightsCount = 1 + RENDER_SUN;
	m_pCB_Scene->m.ProbesCount = m_ProbesCount;
	m_pCB_Scene->UpdateData();


	//////////////////////////////////////////////////////////////////////////
	// Update from memory mapped file
#ifdef _DEBUG
	if ( m_pMMF->CheckForChange() )
		m_CachedCopy = m_pMMF->GetMappedMemory();
#endif


	//////////////////////////////////////////////////////////////////////////
	// Animate lights

		// ============= Point light =============
	bool	ShowLight0 = m_CachedCopy.EnablePointLight != 0;
	if ( ShowLight0 && m_CachedCopy.AnimatePointLight )
		AnimateLightTime0 += _DeltaTime;

	if ( ShowLight0 )
		m_pSB_LightsDynamic->m[0].Color = m_CachedCopy.PointLightIntensity * float3( m_CachedCopy.PointLightColorR, m_CachedCopy.PointLightColorG, m_CachedCopy.PointLightColorB );
	else
		m_pSB_LightsDynamic->m[0].Color.Set( 0, 0, 0 );

	m_pSB_LightsDynamic->m[0].Type = Scene::Light::POINT;
	m_pSB_LightsDynamic->m[0].Parms.Set( 0.1f, 0.1f, 0, 0 );

#ifdef SCENE_CORRIDOR
	// CORRIDOR ANIMATION (simple straight line)

//	m_pSB_LightsDynamic->m[0].Position.Set( 0.0f, 0.2f, 4.0f * sinf( 0.4f * AnimateLightTime0 ) );	// Move along the corridor
	m_pSB_LightsDynamic->m[0].Position.Set( 0.75f * sinf( 1.0f * AnimateLightTime0 ), 0.5f + 0.3f * cosf( 1.0f * AnimateLightTime0 ), 4.0f * sinf( 0.3f * AnimateLightTime0 ) );	// Move along the corridor

#else
	// PATH ANIMATION (follow curve)
	static bool	bPathPreComputed = false;
#ifdef SCENE_SPONZA
	const float	TOTAL_PATH_TIME = 40.0f;	// Total time to walk the path
	const bool	PING_PONG = false;
	const int	PATH_NODES_COUNT = 5;
	const float	Y = 180.0f;	// Ground floor
//	const float	Y = 550.0f;	// First floor

	static float3	pPath[] = {
		0.01f * float3( -1229.0f, Y, -462.0f ),
		0.01f * float3( -1229.0f, Y, 392.0f ),
		0.01f * float3( 1075.0f, Y, 392.0f ),
		0.01f * float3( 1075.0f, Y, -462.0f ),
		0.01f * float3( -1229.0f, Y, -462.0f ),
	};
#elif defined(SCENE_CITY)
	const float	TOTAL_PATH_TIME = 20.0f;	// Total time to walk the path
	const bool	PING_PONG = true;
	const int	PATH_NODES_COUNT = 4;
	static float3	pPath[] = {
		0.01f * float3( 470.669f, 25.833f, -573.035f ),	// Street exterior
		0.01f * float3( 470.669f, 25.833f, 1263.286f ),	// Shop interior
		0.01f * float3( 876.358f, 25.833f, 1263.286f ),	// Shop interior
		0.01f * float3( 918.254f, 25.833f, 3848.391f ),	// Shop yard
	};
#endif
	static float	pPathSegmentsLength[PATH_NODES_COUNT];
	static float	TotalPathLength = 0.0f;

	if ( !bPathPreComputed )
	{	// Precompute path lengths
		pPathSegmentsLength[0] = 0.0f;
		for ( int PathNodeIndex=1; PathNodeIndex < PATH_NODES_COUNT; PathNodeIndex++ )
		{
			TotalPathLength += (pPath[PathNodeIndex] - pPath[PathNodeIndex-1]).Length();
			pPathSegmentsLength[PathNodeIndex] = TotalPathLength;
		}
		bPathPreComputed = true;
	}

	float	PathTime = PING_PONG ? TOTAL_PATH_TIME - abs( fmodf( AnimateLightTime0, 2.0f * TOTAL_PATH_TIME ) - TOTAL_PATH_TIME ) : fmodf( AnimateLightTime0, TOTAL_PATH_TIME );
	float	PathLength = PathTime * TotalPathLength / TOTAL_PATH_TIME;
	for ( int PathNodeIndex=0; PathNodeIndex < PATH_NODES_COUNT-1; PathNodeIndex++ )
	{
		if ( PathLength >= pPathSegmentsLength[PathNodeIndex] && PathLength <= pPathSegmentsLength[PathNodeIndex+1] )
		{
			float	t = (PathLength - pPathSegmentsLength[PathNodeIndex]) / (pPathSegmentsLength[PathNodeIndex+1] - pPathSegmentsLength[PathNodeIndex]);
			m_pSB_LightsDynamic->m[0].Position = pPath[PathNodeIndex] + t * (pPath[PathNodeIndex+1] - pPath[PathNodeIndex]);
			break;
		}
	}

#endif

	if ( ShowLight0 )
		RenderShadowMapPoint( m_pSB_LightsDynamic->m[0].Position, 30.0f );
	else
	{
		m_pCB_ShadowMapPoint->UpdateData();
		m_pRTShadowMapPoint->Set( 3, true );
	}


	// ============= Sun light =============
#if RENDER_SUN	
	{
		bool	ShowLight1 = m_CachedCopy.EnableSun != 0;

		float	SunTheta = m_CachedCopy.SunTheta;
		float	SunPhi = m_CachedCopy.SunPhi;
		float3	SunDirection( sinf(SunTheta) * sinf(SunPhi), cosf(SunTheta), sinf(SunTheta) * cosf(SunPhi) );

		if ( ShowLight1 )
			m_pSB_LightsDynamic->m[1].Color = m_CachedCopy.SunIntensity * float3( 1.0f, 0.990f, 0.950f );
		else
			m_pSB_LightsDynamic->m[1].Color.Set( 0, 0, 0 );

		m_pSB_LightsDynamic->m[1].Type = Scene::Light::DIRECTIONAL;
		m_pSB_LightsDynamic->m[1].Direction = SunDirection;
		m_pSB_LightsDynamic->m[1].Parms = float4::Zero;

		// Render directional shadow map for Sun simulation
		RenderShadowMap( SunDirection );
	}
#else

	// Set shadow map to something, otherwise DX pisses me off with warnings...
	m_pCB_ShadowMap->UpdateData();
	m_pRTShadowMap->Set( 2, true );

#endif

	m_pSB_LightsDynamic->Write( 2 );
	m_pSB_LightsDynamic->SetInput( 8, true );


	// Update emissive materials
	if ( m_EmissiveMaterialsCount > 0 )
	{
		bool	ShowLight2 = m_CachedCopy.EnableEmissiveMaterials != 0;

		float3	EmissiveColor = float3::Zero;
		if ( ShowLight2 )
		{
// //			float	Intensity = 10.0f * MAX( 0.0f, sinf( 4.0f * (AnimateLightTime2 + 0.5f * _frand()) ) );
// 			float	Intensity = 4.0f * MAX( 0.0f, sinf( 4.0f * (AnimateLightTime2 + 0.0f * _frand()) ) );
			float	Intensity = m_CachedCopy.EmissiveIntensity;
			EmissiveColor = Intensity * float3( m_CachedCopy.EmissiveColorR, m_CachedCopy.EmissiveColorG, m_CachedCopy.EmissiveColorB );
		}

		for ( int EmissiveMaterialIndex=0; EmissiveMaterialIndex < m_EmissiveMaterialsCount; EmissiveMaterialIndex++ )
			m_ppEmissiveMaterials[EmissiveMaterialIndex]->m_EmissiveColor = EmissiveColor;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update dynamic probes
//	ASSERT( m_ProbesCount <= MAX_PROBE_UPDATES_PER_FRAME, "Increase max probes update per frame! Or write the time-sliced updater you promised!" );

	// Prepare constant buffer for update
	float3	pSHAmbient[9];
	memset( pSHAmbient, 0, 9*sizeof(float3) );

	if ( m_CachedCopy.EnableSky )
	{
		// Use CIE sky
//		memcpy_s( pSHAmbient, sizeof(pSHAmbient), m_pSHAmbientSky, sizeof(m_pSHAmbientSky) );

		// Simple ambient sky term
		float	SH0 = 0.28209479177387814347403972578039f;	// DC coeff for SH is 1/(2*sqrt(PI))
		pSHAmbient[0] = SH0 * m_CachedCopy.SkyIntensity * float3( m_CachedCopy.SkyColorR, m_CachedCopy.SkyColorG, m_CachedCopy.SkyColorB );
	}

	for ( int i=0; i < 9; i++ )
		m_pCB_UpdateProbes->m.AmbientSH[i] = float4( pSHAmbient[i], 0 );	// Update one by one because of float3 padding

	m_pCB_UpdateProbes->m.AmbientSH[8].w = m_CachedCopy.BounceFactorSun;	// Last padding hides one of our variables in its W component...
	m_pCB_UpdateProbes->m.SkyBoost = m_CachedCopy.BounceFactorSky;
	m_pCB_UpdateProbes->m.DynamicLightsBoost = m_CachedCopy.BounceFactorPoint;
	m_pCB_UpdateProbes->m.StaticLightingBoost = m_CachedCopy.EnableStaticLighting != 0 ? m_CachedCopy.BounceFactorStaticLights : 0.0f;
	m_pCB_UpdateProbes->m.EmissiveBoost = m_CachedCopy.BounceFactorEmissive;
	m_pCB_UpdateProbes->m.NeighborProbesContributionBoost = m_CachedCopy.EnableNeighborsRedistribution ? m_CachedCopy.NeighborProbesContributionBoost : 0.0f;

	m_pCB_UpdateProbes->UpdateData();


#if 1	// Hardware update
	// We prepare the update structures for each probe and send this to the compute shader
	// . The compute shader will then evaluate lighting for all the sampling points for the probe, use their contribution to weight
	//		each set's SH coefficients that will be added together to form the indirect lighting SH coefficients.
	// . Then it will compute the product of ambient sky SH and occlusion SH for the probe to add the contribution of the occluded sky
	// . It will also add the emissive sets' SH weighted by the intensity of the emissive materials at the time (diffuse area lighting).
	// . Finally, it will estimate the neighbor's "perceived visibility" and propagate their SH via a product of their SH with the
	//		neighbor visibility mask (SH product). This way we get additional light bounces from probe to probe.
	//
	// Basically for every probe update, we perform 1(sky)+4(neighbor) expensive SH products and compute lighting for 64 points in the scene
	//
	U32		ProbeUpdatesCount = MIN( m_CachedCopy.MaxProbeUpdatesPerFrame, U32(m_ProbesCount) );
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

		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

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
			ProbeStruct::SetInfos		Set = Probe.pSetInfos[SetIndex];
			RuntimeProbeUpdateSetInfos&	SetUpdateInfos = m_pSB_RuntimeProbeSetInfos->m[TotalSetsCount+SetIndex];

			SetUpdateInfos.SamplingPointsStart = SetSamplingPointsCount;
			SetUpdateInfos.SamplingPointsCount = Set.SamplesCount;
			memcpy_s( SetUpdateInfos.SH, sizeof(SetUpdateInfos.SH), Set.pSHBounce, 9*sizeof(float3) );

			// Copy sampling points (fortunately it's the same static & runtime structures)
			memcpy_s( &m_pSB_RuntimeSamplingPointInfos->m[TotalSamplingPointsCount], Set.SamplesCount*sizeof(RuntimeSamplingPointInfos), Set.pSamples, Set.SamplesCount*sizeof(ProbeStruct::SetInfos::Sample) );

			TotalSamplingPointsCount += Set.SamplesCount;
			SetSamplingPointsCount += Set.SamplesCount;
		}

		// Fill the emissive set update infos
		for ( U32 EmissiveSetIndex=0; EmissiveSetIndex < Probe.EmissiveSetsCount; EmissiveSetIndex++ )
		{
			ProbeStruct::EmissiveSetInfos		EmissiveSet = Probe.pEmissiveSetInfos[EmissiveSetIndex];
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
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

		// Clear light accumulation for the probe
		Probe.ClearLightBounce( pSHAmbient );

		// Iterate on each set and compute energy level
		for ( U32 SetIndex=0; SetIndex < Probe.SetsCount; SetIndex++ )
		{
			const ProbeStruct::SetInfos&	Set = Probe.pSetInfos[SetIndex];

			// Compute irradiance for every sample
			float3	SetIrradiance = float3::Zero;
			for ( U32 SampleIndex=0; SampleIndex < Set.SamplesCount; SampleIndex++ )
			{
				const ProbeStruct::SetInfos::Sample&	Sample = Set.pSamples[SampleIndex];

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
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];
		RuntimeProbe&	Runtime = m_pSB_RuntimeProbes->m[ProbeIndex];

		// Write the result to the probe structured buffer
		memcpy( Runtime.pSHBounce, Probe.pSHBouncedLight, 9*sizeof(float3) );
	}

	m_pSB_RuntimeProbes->Write();
	m_pSB_RuntimeProbes->SetInput( 9, true );

#endif


	//////////////////////////////////////////////////////////////////////////
	// 1] Render the scene
 	m_Device.ClearRenderTarget( m_RTTarget, m_CachedCopy.EnableSky ? 1.0f * float4( 0.64f, 0.79f, 1.0f, 0.0f ) : float4::Zero );

 	m_Device.SetRenderTarget( m_RTTarget, &m_Device.DefaultDepthStencil() );
	m_Device.SetStates( m_Device.m_pRS_CullBack, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

	m_Scene.Render( *this );


	//////////////////////////////////////////////////////////////////////////
	// 2] Render the lights
	USING_MATERIAL_START( *m_pMatRenderLights )

	m_pPrimSphere->RenderInstanced( M, 1 );	// Only show point light, no sun light

	USING_MATERIAL_END


	//////////////////////////////////////////////////////////////////////////
	// 3] Render the dynamic objects
	if ( m_CachedCopy.DynamicObjectsCount > 0 )
	{
		AnimateDynamicObjects += 0.05f * _DeltaTime;
		float	t = abs( fmodf( AnimateDynamicObjects, 2.0f ) - 1.0f );

		USING_MATERIAL_START( *m_pMatRenderDynamic )

		m_pTexDynamicNormalMap->SetPS( 11 );

		for ( U32 DynamicObjectIndex=0; DynamicObjectIndex < m_CachedCopy.DynamicObjectsCount; DynamicObjectIndex++ )
		{
			DynamicObject&	DynObj = m_pDynamicObjects[DynamicObjectIndex];

			// Update object's position
			// TODO!
			m_pCB_DynamicObject->m.Position = DynObj.PositionStart + (DynObj.PositionEnd - DynObj.PositionStart) * t;

			// Retrieve nearest probe
			float				ProbeDistance;
			const ProbeStruct* const*	ppNearestProbe = m_ProbeOctree.FetchNearest( m_pCB_DynamicObject->m.Position, ProbeDistance );
			m_pCB_DynamicObject->m.ProbeID = ppNearestProbe != NULL ? (*ppNearestProbe)->ProbeID : 0xFFFFFFFFU;

			m_pCB_DynamicObject->UpdateData();

			m_pPrimSphere->Render( M );
		}

		USING_MATERIAL_END
	}


	//////////////////////////////////////////////////////////////////////////
	// 4] Render the debug probes
	if ( m_CachedCopy.ShowDebugProbes != 0 )
	{
		USING_MATERIAL_START( *m_pMatRenderDebugProbes )

		m_pPrimSphere->RenderInstanced( M, m_ProbesCount );

		USING_MATERIAL_END
	}

	if ( m_CachedCopy.ShowDebugProbesNetwork != 0 )
	{
		USING_MATERIAL_START( *m_pMatRenderDebugProbesNetwork )

		m_pPrimPoint->RenderInstanced( M, m_pSB_RuntimeProbeNetworkInfos->GetElementsCount() );

		USING_MATERIAL_END
	}


	//////////////////////////////////////////////////////////////////////////
	// 5] Post-process the result
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


void	EffectGlobalIllum2::ProbeStruct::ClearLightBounce( const float3 _pSHAmbient[9] )
{
	// 1] Perform the product of direct ambient light with direct environment mask and accumulate with indirect lighting
	float3	pSHOccludedAmbientLight[9];
	SH::Product3( _pSHAmbient, pSHOcclusion, pSHOccludedAmbientLight );

	// 2] Initialize bounced light with ambient SH + static lighting SH
	for ( int i=0; i < 9; i++ )
		pSHBouncedLight[i] = pSHBounceStatic[i] + pSHOccludedAmbientLight[i];
}

void	EffectGlobalIllum2::ProbeStruct::AccumulateLightBounce( const float3 _pSHSet[9] )
{
	// Simply accumulate dynamic set lighting to bounced light
	for ( int i=0; i < 9; i++ )
		pSHBouncedLight[i] = pSHBouncedLight[i] + _pSHSet[i];
}

static void	CopyProbeNetworkConnection( int _EntryIndex, EffectGlobalIllum2::RuntimeProbeNetworkInfos& _Value, void* _pUserData );

void	EffectGlobalIllum2::PreComputeProbes()
{

#ifdef LOAD_PROBES	// Define this to load probe sets from disk

	FILE*	pFile = NULL;
	char	pTemp[1024];

	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

		// Read numbered probe
		sprintf_s( pTemp, PROBES_PATH "Probe%02d.probeset", ProbeIndex );
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
		ProbeStruct::SetInfos	DummySet;
		for ( U32 SetIndex=0; SetIndex < SetsCount; SetIndex++ )
		{
			ProbeStruct::SetInfos&	S = SetIndex < Probe.SetsCount ? Probe.pSetInfos[SetIndex] : DummySet;	// We load into a useless set if out of range...

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
				ProbeStruct::SetInfos::Sample&	Sample = S.pSamples[SampleIndex];

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
		ProbeStruct::EmissiveSetInfos	DummyEmissiveSet;
		for ( U32 SetIndex=0; SetIndex < EmissiveSetsCount; SetIndex++ )
		{
			ProbeStruct::EmissiveSetInfos&	S = SetIndex < Probe.EmissiveSetsCount ? Probe.pEmissiveSetInfos[SetIndex] : DummyEmissiveSet;	// We load into a useless set if out of range...

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
			ASSERT( EmissiveMatID < U32(m_Scene.m_MaterialsCount), "Material ID out of range!" );
			S.pEmissiveMaterial = m_Scene.m_ppMaterials[EmissiveMatID];

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

#else	// Compute probes instead

	const float		Z_INFINITY = 1e6f;
	const float		Z_INFINITY_TEST = 0.99f * Z_INFINITY;

				pRTCubeMap = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6 * 4, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL );	// Will contain albedo (cube 0) + (normal + distance) (cube 1) + (static lighting + emissive surface index) (cube 2) + Probe IDs (cube 3)
	Texture2D*	pRTCubeMapDepth = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, DepthStencilFormatD32F::DESCRIPTOR );
	Texture2D*	pRTCubeMapStaging = new Texture2D( m_Device, CUBE_MAP_SIZE, CUBE_MAP_SIZE, -6 * 4, PixelFormatRGBA32F::DESCRIPTOR, 1, NULL, true );


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
	CB<CBCubeMapCamera>*	pCBCubeMapCamera = new CB<CBCubeMapCamera>( m_Device, 8, true );


	//////////////////////////////////////////////////////////////////////////
	// Render every probe as a cube map & process
	//
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
//for ( int ProbeIndex=0; ProbeIndex < 1; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

		// Clear cube maps
		m_Device.ClearRenderTarget( *pRTCubeMap->GetTargetView( 0, 6*0, 6 ), float4::Zero );
		m_Device.ClearRenderTarget( *pRTCubeMap->GetTargetView( 0, 6*1, 6 ), float4( 0, 0, 0, Z_INFINITY ) );	// We clear distance to infinity here

		float4	Bisou = float4::Zero;
		((U32&) Bisou.w) = 0xFFFFFFFFUL;
		m_Device.ClearRenderTarget( *pRTCubeMap->GetTargetView( 0, 6*2, 6 ), Bisou );	// Clear emissive surface ID to -1 (invalid) and static color to 0
		((U32&) Bisou.x) = 0xFFFFFFFFUL;
		m_Device.ClearRenderTarget( *pRTCubeMap->GetTargetView( 0, 6*3, 6 ), Bisou );	// Clear probe ID to -1 (invalid)

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

			m_Device.ClearDepthStencil( *pRTCubeMapDepth, 1.0f, 0, true, false );

			//////////////////////////////////////////////////////////////////////////
			// 1] Render Albedo + Normal + Distance + Static lit + Emissive Mat ID
			m_Device.SetStates( m_Device.m_pRS_CullFront, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

			ID3D11RenderTargetView*	ppViews[3] = {
				pRTCubeMap->GetTargetView( 0, 6*0+CubeFaceIndex, 1 ),
				pRTCubeMap->GetTargetView( 0, 6*1+CubeFaceIndex, 1 ),
				pRTCubeMap->GetTargetView( 0, 6*2+CubeFaceIndex, 1 )
			};
			m_Device.SetRenderTargets( CUBE_MAP_SIZE, CUBE_MAP_SIZE, 3, ppViews, pRTCubeMapDepth->GetDepthStencilView() );

			// Render scene
			for ( U32 MeshIndex=0; MeshIndex < m_MeshesCount; MeshIndex++ )
				RenderMesh( *m_ppCachedMeshes[MeshIndex], m_pMatRenderCubeMap, true );


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
			m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );
			m_Device.SetRenderTarget( CUBE_MAP_SIZE, CUBE_MAP_SIZE, *pRTCubeMap->GetTargetView( 0, 6*3+CubeFaceIndex, 1 ), pRTCubeMapDepth->GetDepthStencilView() );

			m_pCB_Probe->m.CurrentProbePosition = Probe.pSceneProbe->m_Local2World.GetRow( 3 );

			USING_MATERIAL_START( *m_pMatRenderNeighborProbe )

			for ( U32 NeighborProbeIndex=0; NeighborProbeIndex < m_ProbesCount; NeighborProbeIndex++ )
				if ( NeighborProbeIndex != ProbeIndex )
				{
					ProbeStruct&	NeighborProbe = m_pProbes[NeighborProbeIndex];

					m_pCB_Probe->m.NeighborProbeID = NeighborProbeIndex;
					m_pCB_Probe->m.NeighborProbePosition = NeighborProbe.pSceneProbe->m_Local2World.GetRow( 3 );
					m_pCB_Probe->UpdateData();

					m_ScreenQuad.Render( M );
				}

			USING_MATERIAL_END
		}


		//////////////////////////////////////////////////////////////////////////
		// 3] Read back cube map and create the SH coefficients
		pRTCubeMapStaging->CopyFrom( *pRTCubeMap );

#if 1	// Save to disk for processing by the ProbeSHEncoder tool
		char	pTemp[1024];
		sprintf_s( pTemp, PROBES_PATH "Probe%02d.pom", ProbeIndex );
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
		double	Normalizer = 1.0 / SumSolidAngle;
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
m_Device.RemoveRenderTargets();
pRTCubeMap->SetPS( 64 );
#endif

	delete pRTCubeMapStaging;

	delete pRTCubeMapDepth;

//### Keep it for debugging!
// 	delete pRTCubeMap;


#endif


	//////////////////////////////////////////////////////////////////////////
	// Allocate runtime probes structured buffer & copy static infos
	m_pSB_RuntimeProbes = new SB<RuntimeProbe>( m_Device, m_ProbesCount, true );
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

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
	m_SceneBBoxMin = float3::MaxFlt;
	m_SceneBBoxMax = -float3::MaxFlt;
	for ( U32 MeshIndex=0; MeshIndex < m_MeshesCount; MeshIndex++ )
	{
		m_SceneBBoxMin = m_SceneBBoxMin.Min( m_ppCachedMeshes[MeshIndex]->m_GlobalBBoxMin );
		m_SceneBBoxMax = m_SceneBBoxMax.Max( m_ppCachedMeshes[MeshIndex]->m_GlobalBBoxMax );
	}

	float	MaxDimension = (m_SceneBBoxMax - m_SceneBBoxMin).Max();

	m_ProbeOctree.Init( m_SceneBBoxMin, MaxDimension, 4.0f, m_ProbesCount );
	int		MaxNodesCount = 0;
	int		TotalNodesCount = 0;
	U32		MaxNodesProbeIndex = ~0;
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

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
	// Build initial positions for dynamic dummy objects
	float3	BBoxCenter = 0.5f * (m_SceneBBoxMin + m_SceneBBoxMax);
	float3	BBoxHalfSize = BBoxCenter - m_SceneBBoxMin;
			BBoxHalfSize = 0.6f * BBoxHalfSize;
	float3	BBoxMin, BBoxMax;
			BBoxMin.x = BBoxCenter.x - BBoxHalfSize.x;
			BBoxMin.y = BBoxCenter.y - BBoxHalfSize.y;
			BBoxMin.z = BBoxCenter.z - BBoxHalfSize.z;
			BBoxMax.x = BBoxCenter.x + BBoxHalfSize.x;
			BBoxMax.y = BBoxCenter.y + BBoxHalfSize.y;
			BBoxMax.z = BBoxCenter.z + BBoxHalfSize.z;

	for ( int DynamicObjectIndex=0; DynamicObjectIndex < MAX_DYNAMIC_OBJECTS; DynamicObjectIndex++ )
	{
		m_pDynamicObjects[DynamicObjectIndex].PositionStart.x = _frand( BBoxMin.x, BBoxMax.x );
		m_pDynamicObjects[DynamicObjectIndex].PositionStart.y = _frand( BBoxMin.y, BBoxMax.y );
		m_pDynamicObjects[DynamicObjectIndex].PositionStart.z = _frand( BBoxMin.z, BBoxMax.z );

		m_pDynamicObjects[DynamicObjectIndex].PositionEnd.x = _frand( BBoxMin.x, BBoxMax.x );
		m_pDynamicObjects[DynamicObjectIndex].PositionEnd.y = _frand( BBoxMin.y, BBoxMax.y );
		m_pDynamicObjects[DynamicObjectIndex].PositionEnd.z = _frand( BBoxMin.z, BBoxMax.z );

		m_pDynamicObjects[DynamicObjectIndex].Interpolation = 0.0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Build the probes network debug mesh
	Dictionary<RuntimeProbeNetworkInfos>	Connections;
	for ( U32 ProbeIndex=0; ProbeIndex < m_ProbesCount; ProbeIndex++ )
	{
		ProbeStruct&	Probe = m_pProbes[ProbeIndex];

		for ( int NeighborProbeIndex=0; NeighborProbeIndex < MAX_PROBE_NEIGHBORS; NeighborProbeIndex++ )
		{
			ProbeStruct::NeighborProbeInfos&	NeighborInfos = Probe.pNeighborProbeInfos[NeighborProbeIndex];
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
			ProbeStruct&	NeighborProbe = m_pProbes[NeighborInfos.ProbeID];
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
	m_pSB_RuntimeProbeNetworkInfos = new SB<RuntimeProbeNetworkInfos>( m_Device, ProbeConnectionsCount, true );
	Connections.ForEach( CopyProbeNetworkConnection, m_pSB_RuntimeProbeNetworkInfos->m );

	m_pSB_RuntimeProbeNetworkInfos->Write();
	m_pSB_RuntimeProbeNetworkInfos->SetInput( 16 );
}

static void	CopyProbeNetworkConnection( int _EntryIndex, EffectGlobalIllum2::RuntimeProbeNetworkInfos& _Value, void* _pUserData )
{
	EffectGlobalIllum2::RuntimeProbeNetworkInfos*	_pTarget = (EffectGlobalIllum2::RuntimeProbeNetworkInfos*) _pUserData;
	memcpy_s( &_pTarget[_EntryIndex], sizeof(EffectGlobalIllum2::RuntimeProbeNetworkInfos), &_Value, sizeof(EffectGlobalIllum2::RuntimeProbeNetworkInfos) );
}

//////////////////////////////////////////////////////////////////////////
// Computes the shadow map infos and render the shadow map itself
//
void	EffectGlobalIllum2::RenderShadowMap( const float3& _SunDirection )
{
	//////////////////////////////////////////////////////////////////////////
	// Build a nice transform
	float3	X = (float3::UnitY ^_SunDirection).Normalize();	// Assuming the Sun is never vertical here!
	float3	Y = _SunDirection ^ X;

	m_pCB_ShadowMap->m.Light2World.SetRow( 0, X );
	m_pCB_ShadowMap->m.Light2World.SetRow( 1, Y );
	m_pCB_ShadowMap->m.Light2World.SetRow( 2, -_SunDirection );
	m_pCB_ShadowMap->m.Light2World.SetRow( 3, float3::Zero, 1 );	// Temporary

	m_pCB_ShadowMap->m.World2Light = m_pCB_ShadowMap->m.Light2World.Inverse();

	// Find appropriate bounds
	float3		BBoxMin = float3::MaxFlt;
	float3		BBoxMax = -float3::MaxFlt;
	for ( U32 MeshIndex=0; MeshIndex < m_MeshesCount; MeshIndex++ )
	{
		Scene::Mesh*	pMesh = m_ppCachedMeshes[MeshIndex];
		float4x4	Mesh2Light = pMesh->m_Local2World * m_pCB_ShadowMap->m.World2Light;

		// Transform the 8 corners of the mesh's BBox into light space and grow the light's bbox
		const float3&	MeshBBoxMin = ((Scene::Mesh&) *pMesh).m_LocalBBoxMin;
		const float3&	MeshBBoxMax = ((Scene::Mesh&) *pMesh).m_LocalBBoxMax;
		for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
		{
			float3	D;
			D.x = float(CornerIndex & 1);
			D.y = float((CornerIndex >> 1) & 1);
			D.z = float((CornerIndex >> 2) & 1);

			float3	CornerLocal = MeshBBoxMin + D * (MeshBBoxMax - MeshBBoxMin);
			float3	CornerLight = float4( CornerLocal, 1 ) * Mesh2Light;

			BBoxMin = BBoxMin.Min( CornerLight );
			BBoxMax = BBoxMax.Max( CornerLight );
		}
	}

	// Recenter & scale transform accordingly
	float3	Center = float4( 0.5f * (BBoxMin + BBoxMax), 1.0f ) * m_pCB_ShadowMap->m.Light2World;	// Center in world space
	float3	Delta = BBoxMax - BBoxMin;
				Center = Center + 0.5f * Delta.z * _SunDirection;	// Center is now stuck to the bounds' Zmin
	m_pCB_ShadowMap->m.Light2World.SetRow( 3, Center, 1 );

	m_pCB_ShadowMap->m.Light2World.Scale( float3( 0.5f * Delta.x, 0.5f * Delta.y, Delta.z ) );


	// Finalize constant buffer
	m_pCB_ShadowMap->m.World2Light = m_pCB_ShadowMap->m.Light2World.Inverse();
	m_pCB_ShadowMap->m.BoundsMin = BBoxMin;
	m_pCB_ShadowMap->m.BoundsMax = BBoxMax;

	m_pCB_ShadowMap->UpdateData();



//CHECK => All corners should be in [(-1,-1,0),(+1,+1,1)]
// BBoxMin = 1e6f * NjFloat3::One;
// BBoxMax = -1e6f * NjFloat3::One;
// pMesh = NULL;
// while ( (pMesh = m_Scene.ForEach( Scene::Node::MESH, pMesh )) != NULL )
// {
// 	NjFloat4x4	Mesh2Light = pMesh->m_Local2World * m_pCB_ShadowMap->m.World2Light;
// 
// 	// Transform the 8 corners of the mesh's BBox into light space and grow the light's bbox
// 	const NjFloat3&	MeshBBoxMin = ((Scene::Mesh&) *pMesh).m_BBoxMin;
// 	const NjFloat3&	MeshBBoxMax = ((Scene::Mesh&) *pMesh).m_BBoxMax;
// 	for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
// 	{
// 		NjFloat3	D;
// 		D.x = float(CornerIndex & 1);
// 		D.y = float((CornerIndex >> 1) & 1);
// 		D.z = float((CornerIndex >> 2) & 1);
// 
// 		NjFloat3	CornerLocal = MeshBBoxMin + D * (MeshBBoxMax - MeshBBoxMin);
// 		NjFloat3	CornerLight = NjFloat4( CornerLocal, 1 ) * Mesh2Light;
// 
// 		BBoxMin = BBoxMin.Min( CornerLight );
// 		BBoxMax = BBoxMax.Max( CornerLight );
// 	}
// }
//CHECK


	//////////////////////////////////////////////////////////////////////////
	// Perform actual rendering
	USING_MATERIAL_START( *m_pMatRenderShadowMap )

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

	m_pRTShadowMap->RemoveFromLastAssignedSlots();
	m_Device.ClearDepthStencil( *m_pRTShadowMap, 1.0f, 0, true, false );
	m_Device.SetRenderTargets( m_pRTShadowMap->GetWidth(), m_pRTShadowMap->GetHeight(), 0, NULL, m_pRTShadowMap->GetDepthStencilView() );

	for ( U32 MeshIndex=0; MeshIndex < m_MeshesCount; MeshIndex++ )
		RenderMesh( *m_ppCachedMeshes[MeshIndex], &M, false );

	USING_MATERIAL_END

	// Assign the shadow map to shaders
	m_Device.RemoveRenderTargets();
	m_pRTShadowMap->Set( 2, true );
}

void	EffectGlobalIllum2::RenderShadowMapPoint( const float3& _Position, float _FarClipDistance )
{
	m_pCB_ShadowMapPoint->m.Position = _Position;
	m_pCB_ShadowMapPoint->m.FarClipDistance = _FarClipDistance;
	m_pCB_ShadowMapPoint->UpdateData();

	//////////////////////////////////////////////////////////////////////////
	// Perform actual rendering
	USING_MATERIAL_START( *m_pMatRenderShadowMapPoint )

	m_Device.SetStates( m_Device.m_pRS_CullNone, m_Device.m_pDS_ReadWriteLess, m_Device.m_pBS_Disabled );

	m_pRTShadowMapPoint->RemoveFromLastAssignedSlots();
	m_Device.ClearDepthStencil( *m_pRTShadowMapPoint, 1.0f, 0, true, false );
	m_Device.SetRenderTargets( m_pRTShadowMapPoint->GetWidth(), m_pRTShadowMapPoint->GetHeight(), 0, NULL, m_pRTShadowMapPoint->GetDepthStencilView() );

	for ( U32 MeshIndex=0; MeshIndex < m_MeshesCount; MeshIndex++ )
		RenderMesh( *m_ppCachedMeshes[MeshIndex], &M, false );

	USING_MATERIAL_END

	// Assign the shadow map to shaders
	m_Device.RemoveRenderTargets();
	m_pRTShadowMapPoint->Set( 3, true );
}

#pragma region Scene Rendering

//////////////////////////////////////////////////////////////////////////
// Scene Rendering
//
// Each scene material will require a tag at creation & destruction time: we simply assign the runtime render material as tag
void*	EffectGlobalIllum2::TagMaterial( const Scene& _Owner, Scene::Material& _Material )
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

// 	switch ( _Material.m_ID )
// 	{
// 	case 0:
// 	case 1:
// 	case 2:
// 
// 		if ( _Material.m_EmissiveColor.Max() > 1e-4f )
// 			return m_pMatRenderEmissive;	// Special emissive materials!
// 
// 		return m_pMatRender;
// 
// 	default:
// 		ASSERT( false, "Unsupported material!" );
// 	}

	
	if ( _Material.m_EmissiveColor.Max() > 1e-4f )
	{
		ASSERT( m_EmissiveMaterialsCount < 100, "Too many emissive materials!" );
		m_ppEmissiveMaterials[m_EmissiveMaterialsCount++] = &_Material;
		return m_pMatRenderEmissive;	// Special rendering for emissive materials!
	}

#ifdef _DEBUG
	OutputDebugString( "New scene material tagged!\n" );
#endif

	return m_pMatRender;
}
void*	EffectGlobalIllum2::TagTexture( const Scene& _Owner, Scene::Material::Texture& _Texture )
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	if ( _Texture.m_ID == ~0 )
		return NULL;	// Invalid textures are not mapped

//return m_ppTextures[0];

#ifndef	USE_WHITE_TEXTURES
	ASSERT( int(_Texture.m_ID) < m_TexturesCount, "Unsupported texture!" );
	return m_ppTextures[_Texture.m_ID];
#else
	return m_ppTextures[0];
#endif
}

// Each scene node will require a tag at creation & destruction time: we simply keep the light nodes and add them as static lights
void*	EffectGlobalIllum2::TagNode( const Scene& _Owner, Scene::Node& _Node )
{
	if ( m_bDeleteSceneTags )
	{
		return NULL;
	}

	if ( _Node.m_Type == Scene::Node::LIGHT )
	{	// Add another static light
		Scene::Light&	SourceLight = (Scene::Light&) _Node;
		LightStruct&	TargetLight = m_pSB_LightsStatic->m[m_pCB_Scene->m.StaticLightsCount++];

		TargetLight.Type = SourceLight.m_LightType;
		TargetLight.Position = SourceLight.m_Local2World.GetRow( 3 );
		TargetLight.Direction = -SourceLight.m_Local2World.GetRow( 2 ).Normalize();
		TargetLight.Color = SourceLight.m_Intensity * SourceLight.m_Color;
		TargetLight.Parms.Set( 10.0f, 11.0f, cosf( SourceLight.m_HotSpot ), cosf( SourceLight.m_Falloff ) );
	}

	return NULL;
}

// Each scene mesh's primitive will require a tag at creation & destruction time: we create an actual runtime rendering primitive
void*	EffectGlobalIllum2::TagPrimitive( const Scene& _Owner, Scene::Mesh& _Mesh, Scene::Mesh::Primitive& _Primitive )
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

#ifdef USE_PER_VERTEX_PROBE_ID
	// Bind it additional buffer infos
	pPrim->BindVertexStream( 1, *m_pPrimProbeIDs, m_TotalVerticesCount );	// We access a small portion of the buffer that only concerns this primitive's vertices
#endif

	// Tag the primitive with the face offset
	pPrim->m_pTag = (void*) m_TotalFacesCount;
	m_pPrimitiveFaceOffset[m_TotalPrimitivesCount] = m_TotalFacesCount;		// Store face offset for each primitive
	m_pPrimitiveVertexOffset[m_TotalPrimitivesCount] = m_TotalVerticesCount;// Sotre vertex offset also
	m_TotalVerticesCount += pPrim->GetVerticesCount();						// Increase total amount of vertices
	m_TotalFacesCount += pPrim->GetFacesCount();							// Increase total amount of faces
	m_TotalPrimitivesCount++;

#ifdef _DEBUG
	OutputDebugString( "New scene primitive tagged!\n" );
#endif

	return pPrim;
}

// Mesh rendering: we render each of the mesh's primitive in turn
void	EffectGlobalIllum2::RenderMesh( const Scene::Mesh& _Mesh, Material* _pMaterialOverride, bool _SetMaterial )
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
		if ( _SetMaterial )
		{
			Texture2D*	pTexDiffuseAlbedo = (Texture2D*) SceneMaterial.m_TexDiffuseAlbedo.m_pTag;
			if ( pTexDiffuseAlbedo != NULL )
				pTexDiffuseAlbedo->SetPS( 10 );
			else
				m_ppTextures[0]->SetPS( 10 );

			Texture2D*	pTexNormal = NULL;
#ifdef USE_NORMAL_MAPS
	#ifdef USE_WHITE_TEXTURES
			pTexNormal = m_ppTextures[1];
	#else
			pTexNormal = (Texture2D*) SceneMaterial.m_TexNormal.m_pTag;
	#endif
#endif
			if ( pTexNormal != NULL )
				pTexNormal->SetPS( 11 );
			else
				m_ppTextures[0]->SetPS( 11 );

			Texture2D*	pTexSpecularAlbedo = (Texture2D*) SceneMaterial.m_TexSpecularAlbedo.m_pTag;
			if ( pTexSpecularAlbedo != NULL )
				pTexSpecularAlbedo->SetPS( 12 );
			else
				m_ppTextures[0]->SetPS( 12 );

			// Upload the primitive's material CB
			m_pCB_Material->m.ID = SceneMaterial.m_ID;
			m_pCB_Material->m.DiffuseAlbedo = SceneMaterial.m_DiffuseAlbedo;
			m_pCB_Material->m.HasDiffuseTexture = pTexDiffuseAlbedo != NULL;
			m_pCB_Material->m.SpecularAlbedo = SceneMaterial.m_SpecularAlbedo;
			m_pCB_Material->m.HasSpecularTexture = pTexSpecularAlbedo != NULL;
			m_pCB_Material->m.EmissiveColor = SceneMaterial.m_EmissiveColor;
			m_pCB_Material->m.SpecularExponent = SceneMaterial.m_SpecularExponent.x;
			m_pCB_Material->m.FaceOffset = U32(pPrim->m_pTag);
			m_pCB_Material->m.HasNormalTexture = pTexNormal != NULL;
			m_pCB_Material->UpdateData();

			pMat->Use();
		}

		// Render
		pPrim->Render( *pMat );
	}
}

#pragma endregion