//////////////////////////////////////////////////////////////////////////
// Horizon-Based Indirect Lighting Demo
//////////////////////////////////////////////////////////////////////////
//
// If you're familiar with the Maya or Unity camera then my camera manipulator works the same
// Otherwise:
//
//	No key pressed:
//		• Left button = Orbit about target
//		• Middle button = Pan both camera and target
//		• Right button = Zoom in/out on target
//
//	LShift pressed (first person view):
//		• Left button = Forward/backward + look left/right
//		• Middle button = Pan both camera and target
//		• Right button = Look around
//
//	LAlt pressed (Light control):
//		• Left button = Rotate light direction (in scenes with ambient only, rotates the SH environment. In scenes with directional light, rotates the directional)
//		• Middle button = <NOTHING>
//		• Right button = <NOTHING>
//
//	LControl pressed (Debug):
//		• Any button = Attempts to read back buffers for debugging purpose. Crashes at the moment since it's been ages since I've used this tool.
//
//
// Shortcut keys:
//		• <SPACE> = Toggles HBIL
//		• R = Reloads modified shaders
//		• A = Toggles reprojection matrix copy ON/OFF (used for debugging scene reprojection, otherwise it's quite useless)
//
//
// ------------------------- DONE ------------------------- 
//	✓ Use push/pull (with bilateral) to fill in reprojected radiance voids!!!
//	✓ float	GetBilateralWeight( Z0, Z1, radius, ref sqHypotenuse ) => Outside of unit sphere???
//	✓ Use radius² as progression
//		=> Doesn't provide any significant improvement
//	✓ Compute AO value!!
//	✓ Emissive surfaces???
//	✓ Keep previous radiance in case we reject height sample but accept radiance, and don't want to interpolate foreground radiance? Will that even occur?
//	✓ Use normal dot product weighting anyway?? It looks closer to ground truth in the ground truth simulator! Check it!
//		=> Nope. Looks better with actual bent normal.
//	✓ Advance in local camera space but using screen-space steps
//		=> Must be a linear combination of vector so that advancing 2 pixels equals advancing N meters in camera space...
//	✓ Sample mips for larger footprint for IRRADIANCE sampling (only if mip is bilateral filtered!)
//		=> Works super fine!
//	✓ Sample mips for larger footprint for DEPTH sampling (only if mip is bilateral filtered!)
//		=> DOESN'T WORK AT ALL! Nasty silhouettes appear around objects, I have no immediate idea on how to fix that...
//	✓ Write interleaved sampling
//	✓ Reconstruct interleaved sampling based on bilateral weight (store it some place? Like alpha somewhere?)
//		=> Actually NOT DONE, but the TAA takes care of the reconstruction
//	✓ Write and use mips for split irradiance
//		=> Moreover, use already computed mips from full-res source! Don't recompute mips. This way, we could hope to achieve more quality since we'll sample more pixels from more slices...
//		=> Maybe it's a general idea we could use? => Use mips to spread pixels "cross slices"
//			► Actually, it brings back the ugly bayer pattern so don't!!
//			► Moreover, there is no runtime gain except a larger cost when splitting + creating the mips!
//
// ------------------------- DONE ------------------------- 
//
// #TODOS:
//	• Decrease sample weights with distance for AO
//		=> I find we get better results with closer radius
//	• Try and make the "STEP_SIZE_FACTORS" work! They give a goddamned good AO result, but such a poor HBIL result... What a shame! :'(
//	• Try to use spiral sampling pattern in the manner of scalable ambient obscurance (http://research.nvidia.com/sites/default/files/pubs/2012-06_Scalable-Ambient-Obscurance/McGuire12SAO.pdf)
//		=> At the cost of cache coherence I suppose... :'(
//	• Try all this in quarter res + upsampling!
//
// IDEAS:
//	• Keep failed reprojected pixels into some "surrounding buffer", some sort of paraboloid-projected buffer containing off-screen values???
//		=> Exponential decay of off-screen values with decay rate depending on camera's linear velocity?? (fun idea!)
//	• Even better: render the remaining 5 sides of a cube map around the camera (each frame?!) and integrate the off-screen radiance from it!
//		Or default to the nearest IL environment probe to estimate far-field, as explained in the paper... :/
//	• Cut if outside screen + blend to 0 at grazing angles
//	• Linearly interpolate Ld in the integral? (use previous / current normal to estimate a bicubic shape)
//	• Re-use Dishonored's pre-computed BRDF weight that depends on roughness for irradiance estimate?
//
// BUGS:
//	• Horrible noise in reprojection buffer
//		==> Due to race condition for 2 pixels wanting to reproject at the same place
//		==> Tried to use a poor man's ZBuffer as uint + interlockedMin(), reduced the noise a little but not completely for some reason I can't explain... :/
//
//////////////////////////////////////////////////////////////////////////
//


// Distance field scenes
// These are all procedural scene that don't require actual meshes, but some of them use the textures stored in the "./Textures" folder
//
#define SCENE_LIBRARY					// From @leondenise. Lit only by a single set of SH coefficients (useful to test AO from bent cones)
//#define SCENE_INFINITE_ROOMS			// From @leondenise. Lit by both ambient SH and a strong directional Sun.
//#define SCENE_CORNELL					// Simple Cornell box. Lit by a single point light on the ceiling and a weak environment light. (textures from Geoffrey Rosin) (can you find the hidden emissive quad? :D)
//#define SCENE_CORNELL_USE_ASBESTOS	// Same but with different floor texture
//#define SCENE_HEIGHTFIELD				// Simple height field. Lit by a super strong Sun. (stolen from Shadertoy version of iQ's "Elevated")

// 3D scenes
// These are OBJ scenes that are NOT stored on github. You need to download and install them from the appropriate sources.
// Read the README files in the "./Scenes/" directory for more information...
//
//#define SCENE_SPONZA					// Crytek Sponza scene with a super strong Sun. Downloaded from https://casual-effects.com/data/ and installed in the "./Scenes/" folder
//#define SCENE_SPONZA_POINT_LIGHT		// Same but with a strong moving point light and a weak Sun. (check "Animate" to make the point light move about)
//#define SCENE_SIBENIK					// Sibenik cathedral with a strong point light. Downloaded from https://casual-effects.com/data/ and installed in the "./Scenes/" folder
//#define SCENE_SCHAEFFER				// Corridor scene by Austin Shaeffer. Not provided. You should contact @SchaefferAustin to use it (textures are quite big too so couldn't upload them).


//#define BRUTE_FORCE_HBIL		// Nice but at least 50ms at 1280x720 on easy scenes... Serves as "ground-truth" :D
//#define RECOMPOSE				// Define this to recompose the split buffers into a full-res buffer (uses 2ms whereas we can totally sample directly from split buffers at lighting time)!
//#define SINGLE_DIRECTION		// Define to use a single direction instead of 2 when computing interleaved HBIL values (obviously faster, but also a lot uglier =))

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using SharpMath;
using ImageUtility;
using Renderer;
using Nuaj.Cirrus.Utility;
using Nuaj.Cirrus;

namespace TestHBIL {
	public partial class TestHBILForm : Form {

		#region CONSTANTS

//		const float		TAN_HALF_FOV = Mathf.Tan( Mathf.ToRad( 60.0f ) / 2.0f );	// BEWARE: Must correspond to TAN_HALF_FOV declared in global.hlsl!
		readonly float	TAN_HALF_FOV = 0.6f;										// BEWARE: Must correspond to TAN_HALF_FOV declared in global.hlsl!
		const float		Z_NEAR = 0.01f;
		const float		Z_FAR = 100.0f;												// BEWARE: Must correspond to Z_FAR declared in global.hlsl!

		const uint		SHADOW_MAP_SIZE_DIRECTIONAL = 1024;							// Caution! Must match value declared in Lighting.hlsl!
		const uint		SHADOW_MAP_SIZE_POINT = 512;										// Caution! Must match value declared in Lighting.hlsl!

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float2		_resolution;
			public float		_time;
			public float		_deltaTime;
			public float4		_debugValues;
			public float4		_mouseUVs;
			public uint			_flags;
			public uint			_framesCount;
			public uint			_debugMipIndex;
			public float		_environmentIntensity;
			public float		_sunIntensity;
			public float		_forcedAlbedo;
			public float		_coneAngleBias;
			public float		_exposure;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_camera2World;
			public float4x4		_world2Camera;
			public float4x4		_proj2World;
			public float4x4		_world2Proj;
			public float4x4		_camera2Proj;
			public float4x4		_proj2Camera;

			public float4		_ZNearFar_Q_Z;
			public float4		_subPixelOffsets;
//			public float4		_subPixelOffset;	// Camera jittering

			// Previous frame matrices
			public float4x4		_previousWorld2Proj;
//			public float4x4		_previousWorld2Camera;
			public float4x4		_previousCamera2Camera;
			public float4x4		_camera2PreviousCamera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_SH {
			public float4	_SH0;
			public float4	_SH1;
			public float4	_SH2;
			public float4	_SH3;
			public float4	_SH4;
			public float4	_SH5;
			public float4	_SH6;
			public float4	_SH7;
			public float4	_SH8;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_PushPull {
			public uint		_sizeX;
			public uint		_sizeY;
			public float2	_bilateralDepths;
			public float	_preferedDepth;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_DownSample {
			public uint		_sizeX;
			public uint		_sizeY;
			public uint		_passIndex;
			float			__PAD;
			public float4	_bilateralValues;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_Shadow {
			// Point light
			public float3		_wsLightPosition;			// Point light world-space position
			public float		_pointLightZFar;			// Near/Far clip distances

			// Directional light
			public float4x4		_directionalShadowMap2World;
			public uint			_faceIndex;					// Point face index or cascade slice index (render-time only)
		}

		#if BRUTE_FORCE_HBIL
			[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
			internal struct	CB_HBIL {
				public float4	_bilateralValues;
				public float	_gatherSphereMaxRadius_m;		// Maximum radius (in meters) of the IL gather sphere
				public float	_gatherSphereMaxRadius_pixels;	// Maximum radius (in pixels) of the IL gather sphere
				public float	_temporalAttenuationFactor;
			}
		#else
			[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
			internal struct	CB_HBIL {
				public uint		_targetResolutionX;
				public uint		_targetResolutionY;
				public float2	_csDirection;

				public uint		_renderPassIndexX;
				public uint		_renderPassIndexY;
				public uint		_renderPassIndexCount;
				public float	_gatherSphereMaxRadius_m;		// Maximum radius (in meters) of the IL gather sphere

				public float4	_bilateralValues;

				public float	_gatherSphereMaxRadius_pixels;	// Maximum radius (in pixels) of the IL gather sphere
				public float	_temporalAttenuationFactor;
				public uint		_jitterOffset;
			}
		#endif

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_Object {
			public float4x4		_object2World;
			public float4x4		_previousObject2World;	// From previous frame
			public float		_F0;					// Object's F0
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_TAA {
			public float2	_TAADither;
			public float	_TAAAmount;					// Default = 0.1
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_DebugCone {
			public float3	_wsConePosition;
			public float	_coneAngle;
			public float3	_wsConeDirection;
			public float	_coneStdDeviation;
			public uint		_flags;
		}

		#endregion

		#region FIELDS

		private Device				m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_SH>			m_CB_SH = null;
		private ConstantBuffer<CB_PushPull>		m_CB_PushPull = null;
		private ConstantBuffer<CB_DownSample>	m_CB_DownSample = null;
		private ConstantBuffer<CB_Shadow>		m_CB_Shadow = null;
		private ConstantBuffer<CB_HBIL>			m_CB_HBIL = null;
		private ConstantBuffer<CB_Object>		m_CB_Object = null;
		private ConstantBuffer<CB_TAA>			m_CB_TAA = null;
		private ConstantBuffer<CB_DebugCone>	m_CB_DebugCone = null;

		private ComputeShader		m_shader_ReprojectRadiance = null;

		private ComputeShader		m_shader_Push = null;
		private ComputeShader		m_shader_Pull = null;
		private Shader				m_shader_RenderScene_DepthGBufferPass = null;
		private ComputeShader		m_shader_DownSampleDepth = null;
		private ComputeShader		m_shader_CopyDepthStencil = null;
		private ComputeShader		m_shader_SplitDepth = null;
		private ComputeShader		m_shader_SplitRadiance = null;
		private ComputeShader		m_shader_SplitNormal = null;
		private ComputeShader		m_shader_DownSampleSplitRadiance = null;
		private ComputeShader		m_shader_Recompose = null;
		private Shader				m_shader_AddEmissive = null;
		private Shader				m_shader_RenderScene_ShadowPoint = null;
		private Shader				m_shader_RenderScene_ShadowDirectional = null;
		private Shader				m_shader_ComputeHBIL = null;
		private Shader				m_shader_ComputeLighting = null;
		private ComputeShader		m_shader_TAA = null;
		private Shader				m_shader_PostProcess = null;

		// G-Buffer
		private Texture2D			m_tex_albedo = null;
		private Texture2D			m_tex_normal = null;
//		private Texture2D			m_tex_motionVectors_Scatter = null;
		private Texture2D			m_tex_motionVectors_Gather = null;
		private Texture2D			m_tex_emissive = null;
		private Texture2D			m_tex_depthWithMips = null;

		// Shadow maps
		private Texture2D			m_tex_shadowDirectional = null;
		private bool				m_directionalShadowDirty = true;
		private Texture2D			m_tex_shadowPoint = null;

		// HBIL Results
		#if !BRUTE_FORCE_HBIL
			private Texture2D			m_tex_splitDepth = null;
			private Texture2D			m_tex_splitNormal = null;
			private Texture2D			m_tex_splitRadiance = null;
			private Texture2D			m_tex_splitBentCone = null;
			private Texture2D			m_tex_splitIrradiance = null;
		#endif

		private Texture2D			m_tex_bentCone = null;
		private Texture2D			m_tex_radiance0 = null;
		private Texture2D			m_tex_radiance1 = null;

		private Texture2D			m_tex_sourceRadiance_PUSH = null;
		private Texture2D			m_tex_sourceRadiance_PULL = null;
		private Texture2D			m_tex_reprojectedDepthBuffer = null;
		private Texture2D			m_tex_finalRender0 = null;
		private Texture2D			m_tex_finalRender1 = null;

		// TAA
		private Texture2D			m_tex_TAAHistory0 = null;
		private Texture2D			m_tex_TAAHistory1 = null;

		// Regular textures
		private Texture2D			m_tex_blueNoise = null;
		private float[,]			m_tex_blueNoise_CPU = null;
		private uint[,]				m_tex_blueNoise_4x4 = new uint[4,4];


			// Dummy textures with pre-computed heights and normals used to debug the computation
		#if SCENE_HEIGHTFIELD
			private Texture2D			m_tex_texDebugHeights = null;
			private Texture2D			m_tex_texDebugNormals = null;
		#endif

		#if SCENE_CORNELL || SCENE_CORNELL_USE_ASBESTOS
			private Texture2D			m_tex_tomettesAlbedo = null;
			private Texture2D			m_tex_tomettesNormal = null;
			private Texture2D			m_tex_tomettesRoughness = null;
			private Texture2D			m_tex_concreteAlbedo = null;
			private Texture2D			m_tex_concreteNormal = null;
			private Texture2D			m_tex_concreteRoughness = null;
		#endif

		#if SCENE_SPONZA || SCENE_SPONZA_POINT_LIGHT || SCENE_SIBENIK || SCENE_SCHAEFFER
			private ObjSceneUtility.SceneObj	m_sceneOBJ = null;
		#endif

		// TAA
		private float2				m_subPixelJitter;

		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

		// DEBUG
		private ComputeHBIL_SoftwareDebug			m_softwareHBILComputer = null;	// For DEBUG purposes
		private Primitive			m_primCylinder;
		private Shader				m_shader_RenderDebugCone = null;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_stopWatch = new System.Diagnostics.Stopwatch();
		private double				m_ticks2Seconds;
//		private float				m_startTime = 0;
		private float				m_lastDisplayTime = 0;
		private uint				m_framesCount = 0;

		#endregion

		#region METHODS

		public TestHBILForm() {
			InitializeComponent();
		}

		#region Init/Exit

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			uint	W = (uint) panelOutput.Width;
			uint	H = (uint) panelOutput.Height;

			try {
				m_device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "HBIL Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
			m_CB_Main.m._resolution.Set( W, H );
			m_CB_Main.m._time = 0.0f;
			m_CB_Main.m._deltaTime = 0.016f;	// You wish! :D
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
			m_CB_SH = new ConstantBuffer<CB_SH>( m_device, 2 );
			m_CB_PushPull = new ConstantBuffer<CB_PushPull>( m_device, 3 );
			m_CB_DownSample = new ConstantBuffer<CB_DownSample>( m_device, 3 );
			m_CB_Shadow = new ConstantBuffer<CB_Shadow>( m_device, 4 );
			m_CB_HBIL = new ConstantBuffer<CB_HBIL>( m_device, 3 );
			m_CB_Object = new ConstantBuffer<CB_Object>( m_device, 3 );
			m_CB_TAA = new ConstantBuffer<CB_TAA>( m_device, 3 );
			m_CB_DebugCone = new ConstantBuffer<CB_DebugCone>( m_device, 3 );

			try {
				List< ShaderMacro >	macrosList = new List<ShaderMacro>();
				#if SCENE_LIBRARY
					macrosList.Add( new ShaderMacro( "SCENE_TYPE", "0" ) );
				#elif SCENE_INFINITE_ROOMS
					macrosList.Add( new ShaderMacro( "SCENE_TYPE", "1" ) );
				#elif SCENE_CORNELL || SCENE_CORNELL_USE_ASBESTOS
					#if SCENE_CORNELL_USE_ASBESTOS
						macrosList.Add( new ShaderMacro( "USE_ASBESTOS", "1" ) );
					#endif
					macrosList.Add( new ShaderMacro( "SCENE_TYPE", "2" ) );
				#elif SCENE_HEIGHTFIELD
					macrosList.Add( new ShaderMacro( "SCENE_TYPE", "3" ) );
				#elif SCENE_SPONZA|| SCENE_SPONZA_POINT_LIGHT
					macrosList.Add( new ShaderMacro( "SCENE_TYPE", "4" ) );
				#elif SCENE_SIBENIK
					macrosList.Add( new ShaderMacro( "SCENE_TYPE", "5" ) );
				#elif SCENE_SCHAEFFER
					macrosList.Add( new ShaderMacro( "SCENE_TYPE", "7" ) );
				#endif

				#if SCENE_SPONZA_POINT_LIGHT
					macrosList.Add( new ShaderMacro( "SPONZA_POINT_LIGHT", "1" ) );
				#endif

				#if RECOMPOSE || BRUTE_FORCE_HBIL
					macrosList.Add( new ShaderMacro( "USE_RECOMPOSED_BUFFER", "1" ) );	// If set, will use the recomposed buffers. Otherwise, will sample split buffers
				#endif
				#if SINGLE_DIRECTION
					macrosList.Add( new ShaderMacro( "USE_SINGLE_DIRECTION", "1" ) );
				#endif

				ShaderMacro[]	macros = macrosList.ToArray();

				// Reprojection shaders
 				m_shader_ReprojectRadiance = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Reproject", macros );
 				m_shader_Push = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Push", macros );
				m_shader_Pull = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Pull", macros );

				// Scene rendering & lighting
				#if !SCENE_SPONZA && !SCENE_SPONZA_POINT_LIGHT && !SCENE_SIBENIK && !SCENE_SCHAEFFER
					// Ray-traced / Ray-marched scenes
					m_shader_RenderScene_DepthGBufferPass = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_RenderGBuffer", macros );
					m_shader_RenderScene_ShadowPoint = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_ShadowPoint", macros );
					m_shader_RenderScene_ShadowDirectional = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_ShadowDirectional", macros );
				#else
					// Actual 3D scenes
					m_shader_RenderScene_DepthGBufferPass = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene3D.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS_RenderGBuffer", macros );
					m_shader_RenderScene_ShadowPoint = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene3D.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS_ShadowPoint", null, null, macros );
					m_shader_RenderScene_ShadowDirectional = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene3D.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS_ShadowDirectional", null, null, macros );
				#endif

				m_shader_ComputeLighting = new Shader( m_device, new System.IO.FileInfo( "Shaders/ComputeLighting.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", macros );

				// HBIL
				#if BRUTE_FORCE_HBIL
 					m_shader_ComputeHBIL = new Shader( m_device, new System.IO.FileInfo( "Shaders/HBIL/ComputeHBIL_BruteForce.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", macros );
				#else
 					m_shader_ComputeHBIL = new Shader( m_device, new System.IO.FileInfo( "Shaders/HBIL/ComputeHBIL_Interleaved.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", macros );
				#endif

				// TAA
				m_shader_TAA = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/TAA/TemporalAA.hlsl" ), "CS", macros );

				// General Stuff
				m_shader_AddEmissive = new Shader( m_device, new System.IO.FileInfo( "Shaders/AddEmissive.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", macros );
				m_shader_DownSampleDepth = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/DownSampleDepth.hlsl" ), "CS", macros );
				m_shader_CopyDepthStencil = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/DownSampleDepth.hlsl" ), "CS_Copy", macros );
 				m_shader_SplitDepth = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/SplitBuffers.hlsl" ), "CS_SplitFloat1", macros );
 				m_shader_SplitRadiance = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/SplitBuffers.hlsl" ), "CS_SplitFloat3", macros );
				m_shader_SplitNormal = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/SplitBuffers.hlsl" ), "CS_SplitNormal", macros );
				m_shader_DownSampleSplitRadiance = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/SplitBuffers.hlsl" ), "CS_DownSample", macros );
				m_shader_Recompose = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/RecomposeBuffers.hlsl" ), "CS", macros );
				m_shader_PostProcess = new Shader( m_device, new System.IO.FileInfo( "Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", macros );
				m_shader_RenderDebugCone = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderDebugCone.hlsl" ), VERTEX_FORMAT.P3, "VS", null, "PS", macros );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "HBIL Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			// Create buffers
			m_tex_albedo = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.UNORM_sRGB, false, false, null );
			m_tex_normal = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.SNORM, false, false, null );
			m_tex_emissive = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );
//			m_tex_motionVectors_Scatter = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_motionVectors_Gather = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RG16F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_depthWithMips = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.R16F, COMPONENT_FORMAT.AUTO, false, true, null );
//			m_tex_depthWithMips = new Texture2D( m_device, W, H, 1, 0, DEPTH_STENCIL_FORMAT.D32 );	// Can't have UAV flag so can't use CS for mip downsampling

			// Create shadow maps
			m_tex_shadowDirectional = new Texture2D( m_device, SHADOW_MAP_SIZE_DIRECTIONAL, SHADOW_MAP_SIZE_DIRECTIONAL, 1, 1, DEPTH_STENCIL_FORMAT.D32 );
			m_tex_shadowPoint = new Texture2D( m_device, SHADOW_MAP_SIZE_POINT, SHADOW_MAP_SIZE_POINT, 6, 1, DEPTH_STENCIL_FORMAT.D32 );

			// Create HBIL full-res buffers
			#if BRUTE_FORCE_HBIL
				m_tex_bentCone = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
				m_tex_radiance0 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
				m_tex_radiance1 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
				m_tex_sourceRadiance_PUSH = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_sourceRadiance_PULL = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_reprojectedDepthBuffer = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.R32, COMPONENT_FORMAT.UINT, false, true, null );

				m_tex_finalRender0 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_finalRender1 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );
			#else
				m_tex_bentCone = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.SNORM, false, true, null );
				m_tex_radiance0 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_radiance1 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_sourceRadiance_PUSH = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_sourceRadiance_PULL = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_reprojectedDepthBuffer = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.R32, COMPONENT_FORMAT.UINT, false, true, null );

				// Create HBIL quarter-res buffers
				uint	qW = W >> 2; if ( 4*qW != W ) throw new Exception( "Must be integer multiple of 4!" );
				uint	qH = H >> 2; if ( 4*qH != H ) throw new Exception( "Must be integer multiple of 4!" );
				m_tex_splitDepth = new Texture2D( m_device, qW, qH, 16, 1, PIXEL_FORMAT.R16F, COMPONENT_FORMAT.AUTO, false, true, null );
//m_tex_splitDepth = new Texture2D( m_device, qW, qH, 16, 1, PIXEL_FORMAT.R32F, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_splitNormal = new Texture2D( m_device, qW, qH, 16, 1, PIXEL_FORMAT.RG8, COMPONENT_FORMAT.SNORM, false, true, null );
//				m_tex_splitRadiance = new Texture2D( m_device, qW, qH, 16, 0, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );	// Use several mips to sample irradiance with a larger footprint => turns out it brings nothing, no time gain or quality!
				m_tex_splitRadiance = new Texture2D( m_device, qW, qH, 16, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );	// Use only a single mip
				m_tex_splitIrradiance = new Texture2D( m_device, qW, qH, 16, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );
//				m_tex_splitBentCone = new Texture2D( m_device, qW, qH, 16, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, true, null );		// Can't use R11G11B10: We need the alpha slot!
				m_tex_splitBentCone = new Texture2D( m_device, qW, qH, 16, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.SNORM, false, true, null );		// Can't use R11G11B10: We need the alpha slot!

// 				m_tex_finalRender0 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGB10A2, COMPONENT_FORMAT.UNORM, false, true, null );
// 				m_tex_finalRender1 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGB10A2, COMPONENT_FORMAT.UNORM, false, true, null );
				m_tex_finalRender0 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );
				m_tex_finalRender1 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );
			#endif

			// TAA
			m_tex_TAAHistory0 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );
			m_tex_TAAHistory1 = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, true, null );
			m_device.Clear( m_tex_TAAHistory0, float4.UnitW );
			m_device.Clear( m_tex_TAAHistory1, float4.UnitW );

			//////////////////////////////////////////////////////////////////////////
			// Create scene
			#if SCENE_SPONZA|| SCENE_SPONZA_POINT_LIGHT
				m_sceneOBJ = new ObjSceneUtility.SceneObj();
				m_sceneOBJ.LoadOBJFile( new System.IO.FileInfo( "./Scenes/Casual-Effects.com/sponza/sponza.obj" ), true );
//ObjSceneUtility.SceneObj.Material.ms_diffuseIssRGB = false;		// Don't treat diffuse textures as sRGB
//ObjSceneUtility.SceneObj.Material.ms_fastLoadDefaultColor = true;	// Fast load!
//ObjSceneUtility.SceneObj.Material.ms_loadTexturesAsDDS = false;		// Reload and build mips every time
//ObjSceneUtility.SceneObj.Material.ms_saveTexturesAsDDS = false;		// Don't cache DDS

				float	S = 0.01f;	// Scale factor
// 				float4x4	local2World = new float4x4(		// Rotate by 90° about X
// 					new float4( S, 0, 0, 0 ),
// 					new float4( 0, 0, -S, 0 ),
// 					new float4( 0, S, 0, 0 ),
// 					new float4( 0, 0, 0, 1 )
// 				);
				float4x4	local2World = new float4x4(
					new float4( S, 0, 0, 0 ),
					new float4( 0, S, 0, 0 ),
					new float4( 0, 0, S, 0 ),
					new float4( 0, 0, 0, 1 )
				);

				m_sceneOBJ.CreatePrimitivesAndTextures( m_device, local2World );
			#elif SCENE_SIBENIK
				m_sceneOBJ = new ObjSceneUtility.SceneObj();
				m_sceneOBJ.LoadOBJFile( new System.IO.FileInfo( "./Scenes/Casual-Effects.com/sibenik/sibenik.obj" ), true );
//ObjSceneUtility.SceneObj.Material.ms_diffuseIssRGB = false;			// Don't treat diffuse textures as sRGB
//ObjSceneUtility.SceneObj.Material.ms_fastLoadDefaultColor = true;		// Fast load!
//ObjSceneUtility.SceneObj.Material.ms_loadTexturesAsDDS = false;		// Reload and build mips every time
//ObjSceneUtility.SceneObj.Material.ms_saveTexturesAsDDS = false;		// Don't cache DDS

				float	S = 1.0f;	// Scale factor
// 				float4x4	local2World = new float4x4(		// Rotate by 90° about X
// 					new float4( S, 0, 0, 0 ),
// 					new float4( 0, 0, -S, 0 ),
// 					new float4( 0, S, 0, 0 ),
// 					new float4( 0, 0, 0, 1 )
// 				);
				float4x4	local2World = new float4x4(
					new float4( S, 0, 0, 0 ),
					new float4( 0, S, 0, 0 ),
					new float4( 0, 0, S, 0 ),
					new float4( 0, 0, 0, 1 )
				);

				m_sceneOBJ.CreatePrimitivesAndTextures( m_device, local2World );
			#elif SCENE_SCHAEFFER
				m_sceneOBJ = new ObjSceneUtility.SceneObj();
				m_sceneOBJ.LoadOBJFile( new System.IO.FileInfo( "./Scenes/Schaeffer/testScene.obj" ), true );
//ObjSceneUtility.SceneObj.Material.ms_diffuseIssRGB = false;		// Don't treat diffuse textures as sRGB
//ObjSceneUtility.SceneObj.Material.ms_fastLoadDefaultColor = true;	// Fast load!
//ObjSceneUtility.SceneObj.Material.ms_loadTexturesAsDDS = false;		// Reload and build mips every time
//ObjSceneUtility.SceneObj.Material.ms_saveTexturesAsDDS = false;		// Don't cache DDS

				float	S = 1.0f;	// Scale factor
// 				float4x4	local2World = new float4x4(		// Rotate by 90° about X
// 					new float4( S, 0, 0, 0 ),
// 					new float4( 0, 0, -S, 0 ),
// 					new float4( 0, S, 0, 0 ),
// 					new float4( 0, 0, 0, 1 )
// 				);
				float4x4	local2World = new float4x4(
					new float4( S, 0, 0, 0 ),
					new float4( 0, S, 0, 0 ),
					new float4( 0, 0, S, 0 ),
					new float4( 0, 0, 0, 1 )
				);

				m_sceneOBJ.CreatePrimitivesAndTextures( m_device, local2World );
			#endif


			//////////////////////////////////////////////////////////////////////////
			// Create textures
			using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/BlueNoise64x64_16bits.png" ) ) )
				using ( ImageFile Imono = new ImageFile() ) {
					Imono.ConvertFrom( I, PIXEL_FORMAT.R16 );
					m_tex_blueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Imono }} ), COMPONENT_FORMAT.UNORM );

					// Also keep a CPU version
					m_tex_blueNoise_CPU = new float[Imono.Width,Imono.Height];
					Imono.ReadPixels( ( uint _X, uint _Y, ref float4 _color ) => { m_tex_blueNoise_CPU[_X,_Y] = _color.x; } );

// 					// Generate a blue noise sampling pattern for shadows
// 					uint		SAMPLES_COUNT = 64;
// 					float2[]	samplingPositions = new float2[SAMPLES_COUNT+1];
// 					float		factor = Imono.Width * Imono.Height - 1;	// A value of 1 will yield the total amount pixels - 1
// 					uint		sampleStep = (uint) Math.Floor( factor / SAMPLES_COUNT );
// 					uint		storedSamplesCount = 0;
// 					for ( uint Y=0; Y < Imono.Height; Y++ )
// 						for ( uint X=0; X < Imono.Width; X++ ) {
// 							float	V = factor * m_tex_BlueNoise_CPU[X,Y];
// 							uint	pixelIndex = (uint) Mathf.Floor( V );
// 							if ( pixelIndex < SAMPLES_COUNT ) {
// 								uint	sampleIndex = pixelIndex;
// 								samplingPositions[sampleIndex] = new float2( X / (0.5f * Imono.Width) - 1.0f, Y / (0.5f * Imono.Height) - 1.0f );
// 								storedSamplesCount++;
// 							}
// 						}
// 
// 					string	code_HLSL = "	float2	SHADOW_SAMPLES[64] = {\r\n";
// 					string	code_Mathematica = "	shadowSamples = {\r\n";
// 					for ( uint sampleIndex=0; sampleIndex < SAMPLES_COUNT; sampleIndex++ ) {
// 						code_HLSL += "float2( " + samplingPositions[sampleIndex].x + ", " + samplingPositions[sampleIndex].y + " ), ";
// 						code_Mathematica += "{ " + samplingPositions[sampleIndex].x + ", " + samplingPositions[sampleIndex].y + " }, ";
// 					}
// 					code_HLSL += "};\r\n";
// 					code_Mathematica += "};\r\n";

					// Generate 4x4 blue noise to use instead of Bayer
					m_tex_blueNoise_4x4[0,0] = 1;
					for ( uint i=1; i < 16; i++ ) {
						// Compute distances
						float	newValueMaxSqDistance = 0.0f;
						int		newValueX = -1, newValueY = -1;
						for ( int Y0=0; Y0 < 4; Y0++ ) {
							for ( int X0=0; X0 < 4; X0++ ) {
								float	maxSqDistance = float.MaxValue;
								for ( int DY=-2; DY < 2 && maxSqDistance > 0.0f; DY++ ) {
									int	Y1 = (Y0 + DY + 4) & 3;
									for ( int DX=-1; DX < 2 && maxSqDistance > 0.0f; DX++ ) {
										int	X1 = (X0 + DX + 4) & 3;
										if ( m_tex_blueNoise_4x4[X1,Y1] != 0 ) {
											// Pixel is occupied, update maximum distance
											float	sqDistance = DX*DX + DY*DY;
											maxSqDistance = Math.Min( maxSqDistance, sqDistance );
										}
									}
								}
								if ( maxSqDistance > newValueMaxSqDistance ) {
									// New best position where to place the new value
									newValueMaxSqDistance = maxSqDistance;
									newValueX = X0;
									newValueY = Y0;
								}
							}
						}

						m_tex_blueNoise_4x4[newValueX,newValueY] = 1+i;
					}
				}

			#if SCENE_HEIGHTFIELD
				// Load height map for heightfield
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/heights.png" ) ) )
					using ( ImageFile Imono = new ImageFile() ) {
						Imono.ConvertFrom( I, PIXEL_FORMAT.R8 );
						m_tex_texDebugHeights = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Imono }} ), COMPONENT_FORMAT.UNORM );
					}

				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/normals.png" ) ) ) {
//				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/SponzaNormal.png" ) ) ) {
					float4[]				scanline = new float4[W];
					Renderer.PixelsBuffer	sourceNormalMap = new  Renderer.PixelsBuffer( W*H*4*4 );
					using ( System.IO.BinaryWriter Wr = sourceNormalMap.OpenStreamWrite() )
						for ( int Y=0; Y < I.Height; Y++ ) {
							I.ReadScanline( (uint) Y, scanline );
							for ( int X=0; X < I.Width; X++ ) {
								float	Nx = 2.0f * scanline[X].x - 1.0f;
								float	Ny = 2.0f * scanline[X].y - 1.0f;
								float	Nz = 2.0f * scanline[X].z - 1.0f;
								Wr.Write( Nx );
								Wr.Write( Ny );
								Wr.Write( Nz );
								Wr.Write( 1.0f );
							}
						}

					m_tex_texDebugNormals = new Renderer.Texture2D( m_device, I.Width, I.Height, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { sourceNormalMap } );
				}
			#endif

			#if SCENE_CORNELL || SCENE_CORNELL_USE_ASBESTOS
				// Tomettes Floor
				#if !SCENE_CORNELL_USE_ASBESTOS
					string[]	floorTextureNames = new string[] {
						"Textures/tomettes_basecolor.png",
						"Textures/tomettes_normal.png",
						"Textures/tomettes_roughness.png",
					};
				#else
					// Asbestos Floor
					string[]	floorTextureNames = new string[] {
						"Textures/asbestos_basecolor.png",
						"Textures/asbestos_normal.png",
						"Textures/asbestos_roughness.png",
					};
				#endif

				// Floor
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( floorTextureNames[0] ) ) ) {
					using ( ImagesMatrix mips = new ImagesMatrix( new ImageFile( I, PIXEL_FORMAT.RGBA8 ), ImagesMatrix.IMAGE_TYPE.sRGB ) ) {
						m_tex_tomettesAlbedo = new Texture2D( m_device, mips, COMPONENT_FORMAT.UNORM_sRGB );
					}
				}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( floorTextureNames[1] ) ) ) {
					using ( ImagesMatrix mips = new ImagesMatrix( new ImageFile( I, PIXEL_FORMAT.RGBA8 ), ImagesMatrix.IMAGE_TYPE.NORMAL_MAP ) ) {
						mips.MakeSigned();
						m_tex_tomettesNormal = new Texture2D( m_device, mips, COMPONENT_FORMAT.SNORM );
					}
				}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( floorTextureNames[2] ) ) ) {
					using ( ImagesMatrix mips = new ImagesMatrix( new ImageFile( I, PIXEL_FORMAT.R8 ), ImagesMatrix.IMAGE_TYPE.LINEAR ) ) {
						m_tex_tomettesRoughness = new Texture2D( m_device, mips, COMPONENT_FORMAT.UNORM );
					}
				}

				// Concrete Walls
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/concrete_diffuse.png" ) ) ) {
					using ( ImagesMatrix mips = new ImagesMatrix( new ImageFile( I, PIXEL_FORMAT.RGBA8 ), ImagesMatrix.IMAGE_TYPE.sRGB ) ) {
						m_tex_concreteAlbedo = new Texture2D( m_device, mips, COMPONENT_FORMAT.UNORM_sRGB );
					}
				}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/concrete_normal.png" ) ) ) {
					using ( ImagesMatrix mips = new ImagesMatrix( new ImageFile( I, PIXEL_FORMAT.RGBA8 ), ImagesMatrix.IMAGE_TYPE.NORMAL_MAP ) ) {
						mips.MakeSigned();
						m_tex_concreteNormal = new Texture2D( m_device, mips, COMPONENT_FORMAT.SNORM );
					}
				}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/concrete_roughness.png" ) ) ) {
					using ( ImagesMatrix mips = new ImagesMatrix( new ImageFile( I, PIXEL_FORMAT.R8 ), ImagesMatrix.IMAGE_TYPE.sRGB ) ) {
						m_tex_concreteRoughness = new Texture2D( m_device, mips, COMPONENT_FORMAT.UNORM );
					}
				}
			#endif

			// Setup camera
			m_camera.CreatePerspectiveCamera( 2.0f * Mathf.Atan( TAN_HALF_FOV ), (float) panelOutput.Width / panelOutput.Height, Z_NEAR, Z_FAR );
			m_manipulator.Attach( panelOutput, m_camera );
			m_manipulator.EnableMouseAction += m_manipulator_EnableMouseAction;

			#if SCENE_LIBRARY
				m_manipulator.InitializeCamera( new float3( 0, 5.0f, -4.5f ), new float3( 0, 0, 0 ), float3.UnitY );	// Library
				checkBoxForceAlbedo.Checked = true;
				floatTrackbarControlAlbedo.Value = 0.7f;
			#elif SCENE_INFINITE_ROOMS
				float3	P = new float3( 3.6635196208953857f, 1.7375000715255737f, -4.879035472869873f );
				float3	D = new float3( -0.42554745078086853f, -0.70710641145706177f, -0.56472104787826538f );
				m_manipulator.InitializeCamera( P, P + D, float3.UnitY );	// Library
				floatTrackbarControlEnvironmentIntensity.Value = 0.4f;
				floatTrackbarControlSunIntensity.Value = 5.0f;
			#elif SCENE_CORNELL || SCENE_CORNELL_USE_ASBESTOS
				m_manipulator.InitializeCamera( new float3( 0, -0.7f, -6.0f ), new float3( 0, -0.7f, 0 ), float3.UnitY );
				floatTrackbarControlEnvironmentIntensity.Value = 0.1f;
				floatTrackbarControlSunIntensity.Value = 2.0f;
			#elif SCENE_HEIGHTFIELD
//				m_manipulator.InitializeCamera( new float3( 0, 3.0f, 0.01f ), new float3( 0, 0, 0 ), float3.UnitY );
				float3	P = new float3( 2.2706992626190186f, 1.3530584573745728f, -1.4188917875289917f );
				float3	D = new float3( -0.75689566135406494f, -0.45101827383041382f, 0.472960501909256f );
				m_manipulator.InitializeCamera( P, P + D, float3.UnitY );	// Library
				checkBoxForceAlbedo.Checked = true;
				checkBoxMonochrome.Checked = true;
				floatTrackbarControlAlbedo.Value = 0.66f;
				floatTrackbarControlEnvironmentIntensity.Value = 0.04f;
			#elif SCENE_SPONZA || SCENE_SPONZA_POINT_LIGHT
// 				float3	P = new float3( 12.5715694f, 0.9832124f, -3.20435619f );	// Near lion head's location
// 				float3	D = new float3( -0.5008525f, 0.207912222f, 0.84019f );
				#if SCENE_SPONZA_POINT_LIGHT
					float3	P = new float3( -3.7771854400634766f, 1.400185227394104f, 3.33648306131362915f );	// In the corner where the light is starting
				#else
					float3	P = new float3( 9.7771854400634766f, 1.400185227394104f, -0.33648306131362915f );	// Viewing the main alley
				#endif
				float3	D = new float3( -1, 0, 0 );
				m_manipulator.InitializeCamera( P, P + D, float3.UnitY );
				floatTrackbarControlEnvironmentIntensity.Value = 0.01f;
				#if SCENE_SPONZA_POINT_LIGHT
					floatTrackbarControlSunIntensity.Value = 1.0f;	// Weak Sun, let the point light show
				#else
					floatTrackbarControlSunIntensity.Value = 100.0f;
				#endif
				floatTrackbarControlExposure.Value = 0.41f;
			#elif SCENE_SIBENIK
				float3	P = new float3( 17.0f, -7.0f, 0.0f );
				float3	D = new float3( -1, -0.5f, 0 );
				m_manipulator.InitializeCamera( P, P + D, float3.UnitY );
				floatTrackbarControlEnvironmentIntensity.Value = 0.05f;
				floatTrackbarControlSunIntensity.Value = 10.0f;
				floatTrackbarControlExposure.Value = 1.0f;
			#elif SCENE_SCHAEFFER
// 				float3	P = new float3( 0.0f, -7.0f, 0.0f );
// 				float3	D = new float3( 0, 0.0f, -1.0f );
				float3	P = new float3( 1.243434f, -7.711112f, 7.611919f );
 				float3	D = new float3( -0.4783995f, 0.03490078f, -0.8774485f );
				m_manipulator.InitializeCamera( P, P + D, float3.UnitY );
				floatTrackbarControlEnvironmentIntensity.Value = 0.01f;
				floatTrackbarControlSunIntensity.Value = 150.0f;
				floatTrackbarControlExposure.Value = 0.5f;
			#endif

			// Setup environment SH coefficients
			InitEnvironmentSH();
			m_lightRotation = (float3x3) m_lightQuat;
			UpdateSH();

			// Create software computer for debugging purposes
			m_softwareHBILComputer = new ComputeHBIL_SoftwareDebug( m_device );
			{
				VertexP3[]	vertices = new VertexP3[2*100];
				uint[]		indices = new uint[2*3*100];
				for ( uint i=0; i < 100; i++ ) {
					float	a = Mathf.PI * i / 50;
					vertices[i] = new VertexP3() { P = new float3( Mathf.Cos( a ), Mathf.Sin( a ), 0 ) };
					vertices[100+i] = new VertexP3() { P = new float3( Mathf.Cos( a ), Mathf.Sin( a ), 1 ) };
					uint	Ni = (i+1) % 100;
					indices[2*3*i+0] = i;
					indices[2*3*i+1] = 100+i;
					indices[2*3*i+2] = 100+Ni;
					indices[2*3*i+3] = i;
					indices[2*3*i+4] = 100+Ni;
					indices[2*3*i+5] = Ni;
				}
				m_primCylinder = new Primitive( m_device, 2*100, VertexP3.FromArray( vertices ), indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3 );
			}

			// Start game time
			m_ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_stopWatch.Start();
			m_lastDisplayTime = GetGameTime();

			m_camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			Camera_CameraTransformChanged( m_camera, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_device == null )
				return;

			m_primCylinder.Dispose();
			m_softwareHBILComputer.Dispose();

			m_tex_TAAHistory1.Dispose();
			m_tex_TAAHistory0.Dispose();

			m_tex_finalRender1.Dispose();
			m_tex_finalRender0.Dispose();

			#if !BRUTE_FORCE_HBIL
				m_tex_splitIrradiance.Dispose();
				m_tex_splitBentCone.Dispose();
				m_tex_splitNormal.Dispose();
				m_tex_splitRadiance.Dispose();
				m_tex_splitDepth.Dispose();
			#endif

			m_tex_reprojectedDepthBuffer.Dispose();
			m_tex_sourceRadiance_PULL.Dispose();
			m_tex_sourceRadiance_PUSH.Dispose();
			m_tex_radiance1.Dispose();
			m_tex_radiance0.Dispose();
			m_tex_bentCone.Dispose();

			#if SCENE_SPONZA || SCENE_SPONZA_POINT_LIGHT || SCENE_SIBENIK || SCENE_SCHAEFFER
				m_sceneOBJ.Dispose();
			#endif


			#if SCENE_CORNELL || SCENE_CORNELL_USE_ASBESTOS
				m_tex_tomettesRoughness.Dispose();
				m_tex_tomettesNormal.Dispose();
				m_tex_tomettesAlbedo.Dispose();

				m_tex_concreteAlbedo.Dispose();
				m_tex_concreteNormal.Dispose();
				m_tex_concreteRoughness.Dispose();
			#endif

			#if SCENE_HEIGHTFIELD
				m_tex_texDebugNormals.Dispose();
				m_tex_texDebugHeights.Dispose();
			#endif

			m_tex_blueNoise.Dispose();
			m_tex_shadowDirectional.Dispose();
			m_tex_shadowPoint.Dispose();
			m_tex_depthWithMips.Dispose();
			m_tex_motionVectors_Gather.Dispose();
//			m_tex_motionVectors_Scatter.Dispose();
			m_tex_emissive.Dispose();
			m_tex_normal.Dispose();
			m_tex_albedo.Dispose();

			m_shader_RenderDebugCone.Dispose();
			m_shader_AddEmissive.Dispose();
			m_shader_PostProcess.Dispose();
			m_shader_TAA.Dispose();
			m_shader_ComputeHBIL.Dispose();
			m_shader_Recompose.Dispose();
			m_shader_DownSampleSplitRadiance.Dispose();
			m_shader_SplitNormal.Dispose();
			m_shader_SplitRadiance.Dispose();
			m_shader_SplitDepth.Dispose();
			m_shader_CopyDepthStencil.Dispose();
			m_shader_DownSampleDepth.Dispose();
			m_shader_ComputeLighting.Dispose();
			if ( m_shader_RenderScene_ShadowDirectional != null )
				m_shader_RenderScene_ShadowDirectional.Dispose();
			m_shader_RenderScene_ShadowPoint.Dispose();
			m_shader_RenderScene_DepthGBufferPass.Dispose();
			m_shader_Pull.Dispose();
			m_shader_Push.Dispose();
			m_shader_ReprojectRadiance.Dispose();

			m_CB_DebugCone.Dispose();
			m_CB_TAA.Dispose();
			m_CB_Object.Dispose();
			m_CB_HBIL.Dispose();
			m_CB_Shadow.Dispose();
			m_CB_DownSample.Dispose();
			m_CB_PushPull.Dispose();
			m_CB_SH.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Main.Dispose();

			m_device.Exit();

			base.OnFormClosed( e );
		}

		#endregion

		float		m_currentTime;

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null || checkBoxPause.Checked )
				return;

			m_device.PerfBeginFrame();

			// Setup global data
			float	newCurrentTime = GetGameTime();
			float	deltaTime = newCurrentTime - m_currentTime;
			m_currentTime = newCurrentTime;

			if ( checkBoxAnimate.Checked ) {
				m_CB_Main.m._deltaTime = deltaTime;
				m_CB_Main.m._time = newCurrentTime;
			} else {
//				m_CB_Main.m._deltaTime = 0.016f;	// You wish we run @60FPS dude! :D
			}

			m_CB_Main.m._debugValues.Set( floatTrackbarControlBilateral0.Value, floatTrackbarControlBilateral1.Value, floatTrackbarControlBilateral2.Value, floatTrackbarControlBilateral3.Value );

			Point	clientLocation = panelOutput.PointToClient( Control.MousePosition );
			m_CB_Main.m._mouseUVs.Set( (clientLocation.X + 0.5f) / panelOutput.Width, (clientLocation.Y + 0.5f) / panelOutput.Height, (m_buttonDownPosition.X + 0.5f) / panelOutput.Width, (m_buttonDownPosition.Y + 0.5f) / panelOutput.Height );

			m_CB_Main.m._flags = 0U;
			m_CB_Main.m._flags |= checkBoxEnableHBIL.Checked ? 1U : 0;
			m_CB_Main.m._flags |= checkBoxEnableBentNormal.Checked ? 2U : 0;
			m_CB_Main.m._flags |= checkBoxEnableConeVisibility.Checked ? 4U : 0;
			m_CB_Main.m._flags |= checkBoxEnableBentNormalDirect.Checked ? 0x8U : 0;
			m_CB_Main.m._flags |= checkBoxEnableConeVisibilityDirect.Checked ? 0x10U : 0;
			m_CB_Main.m._flags |= checkBoxMonochrome.Checked ? 0x20U : 0;
			m_CB_Main.m._flags |= checkBoxForceAlbedo.Checked ? 0x40U : 0;
			m_CB_Main.m._flags |= radioButtonPULL.Checked ? 0x100U : 0;
			m_CB_Main.m._flags |= checkBoxShowAO.Checked ? 0x200U : 0;
			m_CB_Main.m._flags |= checkBoxShowIrradiance.Checked ? 0x400U : 0;
			m_CB_Main.m._flags |= checkBoxEnablePushPull.Checked ? 0x1000U : 0;
			m_CB_Main.m._framesCount = m_framesCount;
			m_CB_Main.m._debugMipIndex = (uint) integerTrackbarControlDebugMip.Value;
			m_CB_Main.m._environmentIntensity = floatTrackbarControlEnvironmentIntensity.Value;
			m_CB_Main.m._sunIntensity = floatTrackbarControlSunIntensity.Value;
			m_CB_Main.m._forcedAlbedo = floatTrackbarControlAlbedo.Value;
			m_CB_Main.m._coneAngleBias = floatTrackbarControlConeAngleBias.Value;
			m_CB_Main.m._exposure = floatTrackbarControlExposure.Value;
			m_CB_Main.UpdateData();

			m_CB_SH.m._SH0.Set( m_rotatedLightSH[0], 0 );
			m_CB_SH.m._SH1.Set( m_rotatedLightSH[1], 0 );
			m_CB_SH.m._SH2.Set( m_rotatedLightSH[2], 0 );
			m_CB_SH.m._SH3.Set( m_rotatedLightSH[3], 0 );
			m_CB_SH.m._SH4.Set( m_rotatedLightSH[4], 0 );
			m_CB_SH.m._SH5.Set( m_rotatedLightSH[5], 0 );
			m_CB_SH.m._SH6.Set( m_rotatedLightSH[6], 0 );
			m_CB_SH.m._SH7.Set( m_rotatedLightSH[7], 0 );
			m_CB_SH.m._SH8.Set( m_rotatedLightSH[8], 0 );
			m_CB_SH.UpdateData();

			//////////////////////////////////////////////////////////////////////////
			// Finalize camera matrices
			// Prepare sub-pixel projection
			SetupCameraMatrices();

			// Set remaining parameters
			m_CB_Camera.m._ZNearFar_Q_Z.Set( Z_NEAR, Z_FAR, Z_FAR / (Z_FAR - Z_NEAR), 0.0f );

			m_CB_Camera.UpdateData();

			//////////////////////////////////////////////////////////////////////////
			// =========== Reproject Last Frame Radiance ===========
			//
			m_device.PerfSetMarker( 0 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( checkBoxEnablePushPull.Checked && m_shader_ReprojectRadiance.Use() ) {
				m_device.Clear( m_tex_sourceRadiance_PUSH, float4.Zero );
				m_device.Clear( m_tex_reprojectedDepthBuffer, 4294967000.0f * float4.One );	// Clear to Z Far

				m_tex_radiance0.SetCS( 0 );				// Source radiance from last frame
				m_tex_depthWithMips.SetCS( 1 );			// Source depth buffer from last frame
//				m_tex_motionVectors_Scatter.SetCS( 2 );	// Motion vectors for dynamic objects
				m_tex_blueNoise.SetCS( 3 );
				m_tex_sourceRadiance_PUSH.SetCSUAV( 0 );
				m_tex_reprojectedDepthBuffer.SetCSUAV( 1 );

				m_shader_ReprojectRadiance.Dispatch( m_tex_sourceRadiance_PUSH.Width >> 4, m_tex_sourceRadiance_PUSH.Height >> 4, 1 );

				m_tex_reprojectedDepthBuffer.RemoveFromLastAssignedSlotUAV();
				m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlotUAV();
//				m_tex_motionVectors_Scatter.RemoveFromLastAssignedSlots();
				m_tex_radiance0.RemoveFromLastAssignedSlots();
				m_tex_depthWithMips.RemoveFromLastAssignedSlots();
			}

			//////////////////////////////////////////////////////////////////////////
			// =========== Render Depth / G-Buffer Pass  ===========
			// We're actually doing all in one here...
			//
			m_device.PerfSetMarker( 10 );

			#if !SCENE_SPONZA && !SCENE_SPONZA_POINT_LIGHT && !SCENE_SIBENIK && !SCENE_SCHAEFFER
				// Ray-traced / Ray-marched Scenes
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				if ( m_shader_RenderScene_DepthGBufferPass.Use() ) {
					m_device.SetRenderTargets( new IView[] { m_tex_albedo.GetView( 0, 1, 0, 1 ), m_tex_normal.GetView( 0, 1, 0, 1 ), m_tex_emissive.GetView( 0, 1, 0, 1 ), m_tex_motionVectors_Gather.GetView( 0, 1, 0, 1 ), m_tex_depthWithMips.GetView( 0, 1, 0, 1 ) }, null );

					#if SCENE_HEIGHTFIELD
						m_tex_texDebugHeights.SetPS( 32 );
						m_tex_texDebugNormals.SetPS( 33 );
					#elif SCENE_CORNELL || SCENE_CORNELL_USE_ASBESTOS
						m_tex_tomettesAlbedo.SetPS( 32 );
						m_tex_tomettesNormal.SetPS( 33 );
						m_tex_tomettesRoughness.SetPS( 34 );
						m_tex_concreteAlbedo.SetPS( 35 );
						m_tex_concreteNormal.SetPS( 36 );
						m_tex_concreteRoughness.SetPS( 37 );
					#endif

					m_device.RenderFullscreenQuad( m_shader_RenderScene_DepthGBufferPass );
					m_device.RemoveRenderTargets();
				}
			#else
				// Actual 3D scenes
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

				if ( m_shader_RenderScene_DepthGBufferPass.Use() ) {
// Optional
// 					m_device.Clear( m_tex_splitIrradiance, float4.Zero );
// 					m_device.Clear( m_tex_splitBentCone, float4.Zero );

					m_device.Clear( m_tex_albedo, float4.Zero );
					m_device.Clear( m_tex_normal, float4.Zero );
					m_device.Clear( m_tex_motionVectors_Gather, float4.Zero );
					m_device.Clear( m_tex_emissive, 3.0f / 255.0f * new float4( 135, 206, 235, 0.0f ) );

					m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );
					m_device.SetRenderTargets( new IView[] { m_tex_albedo.GetView( 0, 1, 0, 1 ), m_tex_normal.GetView( 0, 1, 0, 1 ), m_tex_emissive.GetView( 0, 1, 0, 1 ), m_tex_motionVectors_Gather.GetView( 0, 1, 0, 1 ) }, m_device.DefaultDepthStencil.GetView() );

					// Render all meshes
					uint	surfacesCount = 0;
					uint	verticesCount = 0;
					uint	facesCount = 0;

					m_sceneOBJ.Render( m_shader_RenderScene_DepthGBufferPass, ( ObjSceneUtility.SceneObj.Mesh.Surface _surface ) => {

						// Meshes in OBJ scenes are always (??) centered on the origin
						m_CB_Object.m._object2World = _surface.m_owner.m_local2World;
						m_CB_Object.m._previousObject2World = _surface.m_owner.m_previousFrameLocal2World;
						m_CB_Object.m._F0 = 0.04f;	// Full dielectric
						m_CB_Object.UpdateData();
						
						// Setup textures
						_surface.m_material.m_textureDiffuse.Set( 32 );
						_surface.m_material.m_textureNormal.Set( 33 );
						_surface.m_material.m_textureSpecular.Set( 34 );
						_surface.m_material.m_textureEmissive.Set( 35 );

						verticesCount += (uint) _surface.m_vertices.Length;
						facesCount += (uint) _surface.m_faces.Length;
						surfacesCount++;

						return true;
					} );

					m_device.RemoveRenderTargets();
				}

				// Copy depth stencil to texture
				if ( m_shader_CopyDepthStencil.Use() ) {
					m_device.DefaultDepthStencil.SetCS( 0 );
					m_tex_depthWithMips.GetView( 0, 1, 0, 1 ).SetCSUAV( 0 );
					m_shader_CopyDepthStencil.Dispatch( (m_device.DefaultDepthStencil.Width+15) >> 4, (m_device.DefaultDepthStencil.Height+15) >> 4, 1 );
					m_tex_depthWithMips.RemoveFromLastAssignedSlotUAV();
				}
			#endif

			// Downsample depth-stencil
			m_device.PerfSetMarker( 20 );
			if ( m_shader_DownSampleDepth.Use() ) {
				for ( uint mipLevel=1; mipLevel < m_tex_depthWithMips.MipLevelsCount; mipLevel++ ) {
					View2D	targetView = m_tex_depthWithMips.GetView( mipLevel, 1, 0, 1 );
					targetView.SetCSUAV( 0 );
					m_tex_depthWithMips.GetView( mipLevel-1, 1, 0, 1 ).SetCS( 0 );

					m_CB_DownSample.m._sizeX = targetView.Width;
					m_CB_DownSample.m._sizeY = targetView.Height;
					m_CB_DownSample.UpdateData();

					m_shader_DownSampleDepth.Dispatch( (m_CB_DownSample.m._sizeX+15) >> 4, (m_CB_DownSample.m._sizeY+15) >> 4, 1 );
				}

				m_tex_depthWithMips.RemoveFromLastAssignedSlots();
				m_tex_depthWithMips.RemoveFromLastAssignedSlotUAV();
			}

			//////////////////////////////////////////////////////////////////////////
			// =========== Add Emissive Map to Reprojected Radiance ===========
			// (this way it's used as indirect radiance source in this frame as well)
			//
			if ( m_shader_AddEmissive.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ADDITIVE );
				m_device.SetRenderTargets( new View2D[] { m_tex_sourceRadiance_PUSH.GetView( 0, 1, 0, 1 ) }, null );

				m_tex_emissive.SetPS( 0 );

				m_device.RenderFullscreenQuad( m_shader_AddEmissive );

				m_device.RemoveRenderTargets();	// We need the mip 0 available as SRV right afterward
			}

// This part is quite optional, actually reprojection from last frame is not compulsory:
//	• If you don't do it then you will simply get a single indirect bounce
//	• You could totally re-use the history buffer from the TAA, the fact that it contains both specular and diffuse pixels instead of just diffuse is quite unnoticeable when you integrate radiance
//		Although it's not perfect and requires a little fiddling with some kind of "magic coefficient", it's totally doable and very nice to see some sort of "infinite bounce" as it adds a lot the the resulting image...
//
//*			//////////////////////////////////////////////////////////////////////////
			// =========== Apply Push-Pull to Reconstruct Missing Pixels ===========
			//
			m_device.PerfSetMarker( 30 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			Texture2D	sourceRadiance = m_tex_radiance0;
			if ( checkBoxEnablePushPull.Checked ) {
				uint	maxMip = m_tex_sourceRadiance_PULL.MipLevelsCount;

				m_CB_PushPull.m._bilateralDepths.Set( floatTrackbarControlBilateralDepthDeltaMin.Value, floatTrackbarControlBilateralDepthDeltaMax.Value );
				m_CB_PushPull.m._bilateralDepths.x *= m_CB_PushPull.m._bilateralDepths.x;
				m_CB_PushPull.m._bilateralDepths.y *= m_CB_PushPull.m._bilateralDepths.y;
				m_CB_PushPull.m._preferedDepth = floatTrackbarControlHarmonicPreferedDepth.Value;

				if ( m_shader_Push.Use() ) {
					for ( uint mipLevel=1; mipLevel < maxMip; mipLevel++ ) {
						View2D	targetView = null;
						if ( mipLevel == maxMip-1 )
							targetView = m_tex_sourceRadiance_PULL.GetView( mipLevel, 1, 0, 1 );		// Write last pass to last mip of PULL texture!
						else
							targetView = m_tex_sourceRadiance_PUSH.GetView( mipLevel, 1, 0, 1 );
						targetView.SetCSUAV( 0 );
						m_tex_sourceRadiance_PUSH.GetView( mipLevel-1, 1, 0, 1 ).SetCS( 0 );

						m_CB_PushPull.m._sizeX = targetView.Width;
						m_CB_PushPull.m._sizeY = targetView.Height;
						m_CB_PushPull.UpdateData();

						m_shader_Push.Dispatch( (m_CB_PushPull.m._sizeX+15) >> 4, (m_CB_PushPull.m._sizeY+15) >> 4, 1 );
					}
				}

				// Start pulling phase
				if ( m_shader_Pull.Use() ) {
					for ( uint mipLevel=maxMip-1; mipLevel > 0; mipLevel-- ) {
						View2D	targetView = m_tex_sourceRadiance_PULL.GetView( mipLevel-1, 1, 0, 1 );	// Write to current mip of PULL texture
						targetView.SetCSUAV( 0 );
						m_tex_sourceRadiance_PULL.GetView( mipLevel, 1, 0, 1 ).SetCS( 0 );				// Read from previous mip of PULL texture
						m_tex_sourceRadiance_PUSH.GetView( mipLevel-1, 1, 0, 1 ).SetCS( 3 );			// Read from current mip of PUSH texture (because we can't read and write from a RWTexture other than UINT type!)

						m_CB_PushPull.m._sizeX = targetView.Width;
						m_CB_PushPull.m._sizeY = targetView.Height;
						m_CB_PushPull.UpdateData();

						m_shader_Pull.Dispatch( (m_CB_PushPull.m._sizeX+15) >> 4, (m_CB_PushPull.m._sizeY+15) >> 4, 1 );
					}

					m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlots();
					m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlots();
					m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlotUAV();
 				}

				// Use reconstructed pushed/pulled radiance as source
				sourceRadiance = m_tex_sourceRadiance_PULL;	// Reprojected + reconstructed source radiance from last frame with all mips
			}
//*/

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute Shadow Map ===========
			m_device.PerfSetMarker( 40 );
			#if SCENE_SPONZA || SCENE_SPONZA_POINT_LIGHT || SCENE_SIBENIK || SCENE_SCHAEFFER
				if ( m_shader_RenderScene_ShadowDirectional.Use() && m_directionalShadowDirty ) {
					m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
					m_device.ClearDepthStencil( m_tex_shadowDirectional, 1, 0, true, false );

					m_tex_shadowDirectional.RemoveFromLastAssignedSlots();
					m_device.SetRenderTargets( null, m_tex_shadowDirectional.GetView( 0, 1, 0, 1 ) );
						
					m_CB_Shadow.m._faceIndex = 0;
					m_CB_Shadow.UpdateData();

					m_sceneOBJ.Render( m_shader_RenderScene_ShadowDirectional, ( ObjSceneUtility.SceneObj.Mesh.Surface _surface ) => {
						// Meshes in OBJ scenes are always (??) centered on the origin
						m_CB_Object.m._object2World = _surface.m_owner.m_local2World;
						m_CB_Object.m._previousObject2World = _surface.m_owner.m_previousFrameLocal2World;
						m_CB_Object.UpdateData();
						return true;
					} );

					m_directionalShadowDirty = false;	// Cached!
				}

				// Compute point light shadow
				#if SCENE_SPONZA_POINT_LIGHT
					if ( m_shader_RenderScene_ShadowPoint.Use() ) {
						m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
						m_device.ClearDepthStencil( m_tex_shadowPoint, 1, 0, true, false );

						const float	LIGHT_HEIGHT = 2.0f;	// Ground level
//						const float	LIGHT_HEIGHT = 6.4f;	// 1st Floor

						float3[]	lightPath = new float3[] {
							new float3( -11.785601615905762f, LIGHT_HEIGHT, 3.7845859527587891f ),
							new float3( -11.848102569580078f, LIGHT_HEIGHT, -4.5890488624572754f ),
							new float3( 11.094511985778809f, LIGHT_HEIGHT, -4.5890488624572754f ),
							new float3( 11.144411087036133f, LIGHT_HEIGHT, 3.7845859527587891f ),
							// Duplicate
							new float3( -11.785601615905762f, LIGHT_HEIGHT, 3.7845859527587891f ),
						};
						float	l = 8.3736348152160645f;
						float	L = 23.0f;
						float[]		pathLengths = new float[] {
							0,
							l,
							l + L,
							l + L + l,
							l + L + l + L,
//							l + L + l + L + l,
						};
 
						float	LIGHT_VELOCITY = 1.0f;
						float	currentLength = (m_CB_Main.m._time * LIGHT_VELOCITY) % pathLengths[4];
						for ( uint pathIndex=0; pathIndex < 4; pathIndex++ ) {
							if ( currentLength >= pathLengths[pathIndex] && currentLength <= pathLengths[pathIndex+1] ) {
								float	t = (currentLength - pathLengths[pathIndex]) / (pathLengths[pathIndex+1] - pathLengths[pathIndex] );
								m_CB_Shadow.m._wsLightPosition = lightPath[pathIndex] + (lightPath[pathIndex+1] - lightPath[pathIndex]) * t;
								break;
							}
						}

						m_CB_Shadow.m._pointLightZFar = 50.0f;

						m_tex_shadowPoint.RemoveFromLastAssignedSlots();
						for ( uint faceIndex=0; faceIndex < 6; faceIndex++ ) {
							m_device.SetRenderTargets( null, m_tex_shadowPoint.GetView( 0, 1, faceIndex, 1 ) );
						
							m_CB_Shadow.m._faceIndex = faceIndex;
							m_CB_Shadow.UpdateData();

							m_sceneOBJ.Render( m_shader_RenderScene_ShadowPoint, ( ObjSceneUtility.SceneObj.Mesh.Surface _surface ) => {
								// Meshes in OBJ scenes are always (??) centered on the origin
								m_CB_Object.m._object2World = _surface.m_owner.m_local2World;
								m_CB_Object.UpdateData();
								return true;
							} );
						}
					}
				#elif SCENE_SIBENIK
					if ( m_shader_RenderScene_ShadowPoint.Use() ) {
						m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
						m_device.ClearDepthStencil( m_tex_shadowPoint, 1, 0, true, false );

						m_CB_Shadow.m._wsLightPosition = new float3( 7.0f, -4.0f, 0.0f );
						m_CB_Shadow.m._pointLightZFar = 50.0f;

						m_tex_shadowPoint.RemoveFromLastAssignedSlots();
						for ( uint faceIndex=0; faceIndex < 6; faceIndex++ ) {
							m_device.SetRenderTargets( null, m_tex_shadowPoint.GetView( 0, 1, faceIndex, 1 ) );
						
							m_CB_Shadow.m._faceIndex = faceIndex;
							m_CB_Shadow.UpdateData();

							m_sceneOBJ.Render( m_shader_RenderScene_ShadowPoint, ( ObjSceneUtility.SceneObj.Mesh.Surface _surface ) => {
								// Meshes in OBJ scenes are always (??) centered on the origin
								m_CB_Object.m._object2World = _surface.m_owner.m_local2World;
								m_CB_Object.UpdateData();
								return true;
							} );
						}
					}
				#endif

			#elif SCENE_INFINITE_ROOMS || SCENE_HEIGHTFIELD
				if ( m_shader_RenderScene_ShadowDirectional.Use() && (m_directionalShadowDirty || checkBoxAnimate.Checked) ) {
					m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.WRITE_ALWAYS, BLEND_STATE.DISABLED );
					m_device.ClearDepthStencil( m_tex_shadowDirectional, 1, 0, true, false );

					m_tex_shadowDirectional.RemoveFromLastAssignedSlots();
					m_device.SetRenderTargets( null, m_tex_shadowDirectional.GetView( 0, 1, 0, 1 ) );
						
					m_CB_Shadow.m._faceIndex = 0;
					m_CB_Shadow.UpdateData();

					m_device.RenderFullscreenQuad( m_shader_RenderScene_ShadowDirectional );

					m_directionalShadowDirty = false;	// Cached!
				}
			#elif SCENE_CORNELL || SCENE_CORNELL_USE_ASBESTOS || SCENE_LIBRARY
				if ( m_shader_RenderScene_ShadowPoint.Use() ) {
					m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.WRITE_ALWAYS, BLEND_STATE.DISABLED );

					m_tex_shadowPoint.RemoveFromLastAssignedSlots();
					for ( uint faceIndex=0; faceIndex < 6; faceIndex++ ) {
						m_device.SetRenderTargets( null, m_tex_shadowPoint.GetView( 0, 1, faceIndex, 1 ) );
						
						m_CB_Shadow.m._faceIndex = faceIndex;
						m_CB_Shadow.m._pointLightZFar = 20.0f;
						m_CB_Shadow.UpdateData();

						m_device.RenderFullscreenQuad( m_shader_RenderScene_ShadowPoint );
					}
				}
			#endif

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute Bent Cone Map and Irradiance Bounces  ===========
			// 
			m_device.PerfSetMarker( 50 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			#if BRUTE_FORCE_HBIL
				if ( m_shader_ComputeHBIL.Use() ) {
					m_device.SetRenderTargets( new IView[] { m_tex_radiance1.GetView( 0, 1, 0, 1 ), m_tex_bentCone.GetView( 0, 1, 0, 1 ) }, null );

					m_CB_HBIL.m._gatherSphereMaxRadius_m = floatTrackbarControlGatherSphereRadius_meters.Value;
					m_CB_HBIL.m._gatherSphereMaxRadius_pixels = floatTrackbarControlGatherSphereRadius_pixels.Value;
					m_CB_HBIL.m._bilateralValues.Set( floatTrackbarControlBilateral0.Value, floatTrackbarControlBilateral1.Value, floatTrackbarControlBilateral2.Value, floatTrackbarControlBilateral3.Value );

// 					float	temporalAttenuation = 0.01f;	// 1% of the original value after 1 second
					float	temporalAttenuation = floatTrackbarControlBilateral0.Value;
					m_CB_HBIL.m._temporalAttenuationFactor = Mathf.Exp( Mathf.Log( temporalAttenuation ) * deltaTime );

					m_CB_HBIL.UpdateData();

					m_tex_depthWithMips.SetPS( 0 );
					m_tex_normal.SetPS( 1 );
					sourceRadiance.SetPS( 2 );
					m_tex_blueNoise.SetPS( 3 );

					m_device.RenderFullscreenQuad( m_shader_ComputeHBIL );

//					m_tex_radiance1.RemoveFromLastAssignedSlots();

					Texture2D	temp = m_tex_radiance0;
					m_tex_radiance0 = m_tex_radiance1;
					m_tex_radiance1 = temp;
				}

			#else
				// 1] Split buffers into 4x4 tiny quarter res buffers

// This motherfucker compiler doesn't work with compute shaders taking 16 samples and dispatching them to 16 slices! We need to do 16 unique dispatches instead! :'(
// 				if ( m_shader_SplitDepth.Use() ) {
// 					m_CB_DownSample.m._sizeX = m_tex_splitDepth.Width;
// 					m_CB_DownSample.m._sizeY = m_tex_splitDepth.Height;
// 					m_CB_DownSample.UpdateData();
// 
// 					uint	groupsCountX = (m_CB_DownSample.m._sizeX + 7) >> 3;
// 					uint	groupsCountY = (m_CB_DownSample.m._sizeY + 7) >> 3;
// 
// 					// Split source depth
// 					m_tex_depthWithMips.SetCS( 0 );
// 					m_tex_splitDepth.SetCSUAV( 0 );
// 					m_shader_SplitDepth.Dispatch( groupsCountX, groupsCountY, 1 );
// 
// 					// Split source normal
// 					m_shader_SplitRadiance.Use();
// 					m_tex_normal.SetCS( 1 );
// 					m_tex_splitNormal.SetCSUAV( 1 );
// 					m_shader_SplitRadiance.Dispatch( groupsCountX, groupsCountY, 1 );
// 
// 					// Split source radiance
// 					sourceRadiance.SetCS( 1 );
// 					m_tex_splitRadiance.SetCSUAV( 1 );
// 					m_shader_SplitRadiance.Dispatch( groupsCountX, groupsCountY, 1 );
// 
// 					m_tex_splitRadiance.RemoveFromLastAssignedSlotUAV();
// 					m_tex_splitNormal.RemoveFromLastAssignedSlotUAV();
// 					m_tex_splitDepth.RemoveFromLastAssignedSlotUAV();
// 				}

// Instead, we have to call 16 times dispatch and take a single sample to store it into a single slice at a time
// It's shameful but I don't have time to bother understanding why!!
				if ( m_shader_SplitDepth.Use() ) {
					m_CB_DownSample.m._sizeX = m_tex_splitDepth.Width;
					m_CB_DownSample.m._sizeY = m_tex_splitDepth.Height;

					uint	groupsCountX = (m_CB_DownSample.m._sizeX + 7) >> 3;
					uint	groupsCountY = (m_CB_DownSample.m._sizeY + 7) >> 3;

					// Split source depth
					m_tex_depthWithMips.SetCS( 0 );
					m_tex_splitDepth.SetCSUAV( 0 );
					for ( uint passIndex=0; passIndex < 16; passIndex++ ) {
						m_CB_DownSample.m._passIndex = passIndex;
						m_CB_DownSample.UpdateData();
						m_shader_SplitDepth.Dispatch( groupsCountX, groupsCountY, 1 );
					}

					// Split source normal
					m_shader_SplitNormal.Use();
					m_tex_normal.SetCS( 1 );
					m_tex_splitNormal.SetCSUAV( 2 );
					for ( uint passIndex=0; passIndex < 16; passIndex++ ) {
						m_CB_DownSample.m._passIndex = passIndex;
						m_CB_DownSample.UpdateData();
						m_shader_SplitNormal.Dispatch( groupsCountX, groupsCountY, 1 );
					}

					// Split source radiance
					m_shader_SplitRadiance.Use();
					sourceRadiance.SetCS( 1 );
					m_tex_splitRadiance.SetCSUAV( 1 );
					for ( uint passIndex=0; passIndex < 16; passIndex++ ) {
						m_CB_DownSample.m._passIndex = passIndex;
						m_CB_DownSample.UpdateData();
						m_shader_SplitRadiance.Dispatch( groupsCountX, groupsCountY, 1 );
					}

// This uses the mips from full-res texture
// I thought it would be a good idea as it would favor exchange of values between interleaved samples but it turns out it reveals back
//	the ugly Bayer pattern that is so conveniently removed by the TAA
//
// 					for ( uint mipLevelIndex=0; mipLevelIndex < m_tex_splitRadiance.MipLevelsCount; mipLevelIndex++ ) {
// 						View2D	sourceMip = sourceRadiance.GetView( mipLevelIndex, 1, 0, 1 );
// 						View2D	targetMip = m_tex_splitRadiance.GetView( mipLevelIndex, 1, 0, 16 );
// 
// 						m_CB_DownSample.m._sizeX = targetMip.Width;
// 						m_CB_DownSample.m._sizeY = targetMip.Height;
// 
// 						groupsCountX = (m_CB_DownSample.m._sizeX + 7) >> 3;
// 						groupsCountY = (m_CB_DownSample.m._sizeY + 7) >> 3;
// 
// 						sourceMip.SetCS( 1 );	// Reprojected + reconstructed source radiance from last frame with all mips
// 						targetMip.SetCSUAV( 1 );
// 						for ( uint passIndex=0; passIndex < 16; passIndex++ ) {
// 							m_CB_DownSample.m._passIndex = passIndex;
// 							m_CB_DownSample.UpdateData();
// 							m_shader_SplitRadiance.Dispatch( groupsCountX, groupsCountY, 1 );
// 						}
// 					}

// I tried splitting then building the mips from individual splits
// The result is the same, even worse since the radiance doesn't seem to spread anymore...
// Moreover, the gain in ms is inexistant (even worse: the cost of the splitting+mips goes up! :D)
// 
// 					// Split mip 0
// 					View2D	sourceMip = sourceRadiance.GetView( 0, 1, 0, 1 );
// 					View2D	targetMip = m_tex_splitRadiance.GetView( 0, 1, 0, 16 );
// 					sourceMip.SetCS( 1 );	// Reprojected + reconstructed source radiance from last frame with all mips
// 					targetMip.SetCSUAV( 1 );
// 					for ( uint passIndex=0; passIndex < 16; passIndex++ ) {
// 						m_CB_DownSample.m._passIndex = passIndex;
// 						m_CB_DownSample.UpdateData();
// 						m_shader_SplitRadiance.Dispatch( groupsCountX, groupsCountY, 1 );
// 					}
// 					m_tex_splitRadiance.RemoveFromLastAssignedSlotUAV();
// 
// 					// Build mips
// 					m_shader_DownSampleSplitRadiance.Use();
// 					for ( uint mipLevelIndex=1; mipLevelIndex < m_tex_splitRadiance.MipLevelsCount; mipLevelIndex++ ) {
// 						sourceMip = m_tex_splitRadiance.GetView( mipLevelIndex-1, 1, 0, 1 );
// 						targetMip = m_tex_splitRadiance.GetView( mipLevelIndex, 1, 0, 1 );
// 
// 						m_CB_DownSample.m._sizeX = targetMip.Width;
// 						m_CB_DownSample.m._sizeY = targetMip.Height;
// 
// 						groupsCountX = (m_CB_DownSample.m._sizeX + 7) >> 3;
// 						groupsCountY = (m_CB_DownSample.m._sizeY + 7) >> 3;
// 
// 						sourceMip.SetCS( 2 );
// 						targetMip.SetCSUAV( 1 );
// 						for ( uint passIndex=0; passIndex < 16; passIndex++ ) {
// 							m_CB_DownSample.m._passIndex = passIndex;
// 							m_CB_DownSample.UpdateData();
// 							m_shader_DownSampleSplitRadiance.Dispatch( groupsCountX, groupsCountY, 1 );
// 						}
// 						m_tex_splitRadiance.RemoveFromLastAssignedSlotUAV();
// 					}

					m_tex_depthWithMips.RemoveFromLastAssignedSlots();
					m_tex_normal.RemoveFromLastAssignedSlots();
					sourceRadiance.RemoveFromLastAssignedSlots();
					m_tex_splitRadiance.RemoveFromLastAssignedSlotUAV();
					m_tex_splitNormal.RemoveFromLastAssignedSlotUAV();
					m_tex_splitDepth.RemoveFromLastAssignedSlotUAV();
				}

				// 2] Compute 4x4 small vignettes
				m_device.PerfSetMarker( 51 );
				if ( m_shader_ComputeHBIL.Use() ) {
					m_CB_HBIL.m._targetResolutionX = m_tex_splitRadiance.Width;
					m_CB_HBIL.m._targetResolutionY = m_tex_splitRadiance.Height;
					m_CB_HBIL.m._gatherSphereMaxRadius_m = floatTrackbarControlGatherSphereRadius_meters.Value;
					m_CB_HBIL.m._bilateralValues.Set( floatTrackbarControlBilateral0.Value, floatTrackbarControlBilateral1.Value, floatTrackbarControlBilateral2.Value, floatTrackbarControlBilateral3.Value );
					m_CB_HBIL.m._gatherSphereMaxRadius_pixels = floatTrackbarControlGatherSphereRadius_pixels.Value / 4.0f;

// 					float	temporalAttenuation = 0.01f;	// 1% of the original value after 1 second
					float	temporalAttenuation = floatTrackbarControlBilateral0.Value;
//					m_CB_HBIL.m._temporalAttenuationFactor = Mathf.Pow( temporalAttenuation, deltaTime );
					m_CB_HBIL.m._temporalAttenuationFactor = Mathf.Exp( Mathf.Log( temporalAttenuation ) * deltaTime );

					m_tex_splitDepth.SetPS( 0 );
					m_tex_splitNormal.SetPS( 1 );
					m_tex_splitRadiance.SetPS( 2 );
					m_tex_blueNoise.SetPS( 3 );

					m_CB_HBIL.m._jitterOffset = (m_CB_HBIL.m._jitterOffset + 53) % 67;

					uint	passIndex = 0;
					for ( uint Y=0; Y < 4; Y++ ) {
						for ( uint X=0; X < 4; X++, passIndex++ ) {
							m_device.SetRenderTargets( new IView[] { m_tex_splitIrradiance.GetView( 0, 1, passIndex, 1 ), m_tex_splitBentCone.GetView( 0, 1, passIndex, 1 ) }, null );

							//////////////////////////////////////////////////////////////////////////
							// Here, the goal of this big mess of a code is to find a pattern for phi, the rotation angle for our 2 orthogonal sampling directions
							//	• Phi must cover the entire PI/2/(4*4) angles in the most disorderly fashion possible from one pass to the next (a pass targets an interleaved rendered buffer)
							//		=> For this I'm using a 4x4 Bayer pattern (B4) although I'm really not sure it's the best solution
							//	• The sub-offset angle for Phi in the PI/2/(4*4) from one frame to the next must also be chosen in the most disorderly fashion possible
							//		and at the same time loop fast enough to account for the few frames of TAA history (here I'm using 9 frames IIRC)
							//		and at the same time be sufficiently irrational as to cover all the possible angle and leave no holes (i.e. biased distribution)
							//
							// Overall, the code you see below, uncommented, has been selected as "good enough" after enough experimentation and you can find the same code in the shader
							//	so basically we don't really need to pass the direction vector in the constant buffer anymore... =)
							//

							// This changes pattern quite fast but needs to be faster to cycle entirely before TAA itself cycles
// 							uint	dX = m_framesCount & 3;
// 							uint	dY = m_framesCount >> 2;
// 							float	phi = B4( X + dX, Y + dY ) * 0.5f * Mathf.PI / 16.0f;	// Use Bayer for maximum discrepancy between pixels

//							float	phi = (B4( X, Y ) + Bayer1D_16( m_framesCount ) / 16.0f) * 0.5f * Mathf.PI / 16.0f;	// Use Bayer for maximum discrepancy between pixels

							float	TAAOffset = checkBoxEnableTAA.Checked ? Mathf.Sqrt( 7.0f ) * m_framesCount : 0.0f;	// This will cycle very fast
							#if SINGLE_DIRECTION
								float	phi = (B4( X, Y ) + TAAOffset) * Mathf.PI / 16.0f;	// Use Bayer for maximum discrepancy between pixels
//								float	phi = (m_tex_blueNoise_4x4[X,Y]-1 + TAAOffset) * Mathf.PI / 16.0f;	// Use blue noise for maximum discrepancy between pixels
							#else
//								float	phi = (4*Y+X + TAAOffset) * 0.5f * Mathf.PI / 16.0f;	// Regular pattern, for testing
//								float	phi = (m_tex_blueNoise_4x4[X,Y]-1 + TAAOffset) * 0.5f * Mathf.PI / 16.0f;	// Use blue noise for maximum discrepancy between pixels
								float	phi = (B4( X, Y ) / 16.0f + TAAOffset) * 0.5f * Mathf.PI;	// Use Bayer for maximum discrepancy between pixels
							#endif

//							uint	noiseX = checkBoxEnableTAA.Checked ? (X + m_framesCount) : X;
//							uint	noiseY = checkBoxEnableTAA.Checked ? (Y + 3*m_framesCount) : Y;
//							float	phi = m_tex_BlueNoise_CPU[noiseX & 0x3F, noiseY & 0x3F] * 0.5f * Mathf.PI / 16.0f;

							//////////////////////////////////////////////////////////////////////////

							m_CB_HBIL.m._renderPassIndexX = X;
							m_CB_HBIL.m._renderPassIndexY = Y;
							m_CB_HBIL.m._renderPassIndexCount = passIndex;
							m_CB_HBIL.m._csDirection.Set( Mathf.Cos( phi ), Mathf.Sin( phi ) );
							m_CB_HBIL.UpdateData();

							m_device.RenderFullscreenQuad( m_shader_ComputeHBIL );
						}
					}

					m_device.RemoveRenderTargets();
				}

				// 3] Recompose results
				// Optional, preferable but costs a lot of bandwidth for not much more quality in the end.
				// The idea would be to take the 4x4 samples from each interleaved sampling split-buffer and re-assemble them a bit in the manner of bilateral filtering,
				//	i.e. choosing and weighting the individual samples depending on their coherence with the central pixel...
				//
				m_device.PerfSetMarker( 52 );

				#if RECOMPOSE
					if ( m_shader_Recompose.Use() ) {
						m_CB_DownSample.m._sizeX = m_tex_splitDepth.Width;
						m_CB_DownSample.m._sizeY = m_tex_splitDepth.Height;
						m_CB_DownSample.m._bilateralValues.Set( floatTrackbarControlBilateral0.Value, floatTrackbarControlBilateral1.Value, floatTrackbarControlBilateral2.Value, floatTrackbarControlBilateral3.Value );
						m_CB_DownSample.UpdateData();

						m_tex_splitIrradiance.SetCS( 0 );
						m_tex_splitBentCone.SetCS( 1 );
						m_tex_depthWithMips.SetCS( 2 );
						m_tex_normal.SetCS( 3 );
						sourceRadiance.SetCS( 4 );

						m_tex_radiance0.SetCSUAV( 0 );
						m_tex_bentCone.SetCSUAV( 1 );

						m_shader_Recompose.Dispatch( (m_CB_DownSample.m._sizeX+7) >> 3, (m_CB_DownSample.m._sizeY+7) >> 3, 1 );

						m_tex_radiance0.RemoveFromLastAssignedSlotUAV();
						m_tex_bentCone.RemoveFromLastAssignedSlotUAV();
					}
				#endif

			#endif	// #if !BRUTE_FORCE_HBIL

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute lighting & finalize radiance  ===========
			// 
			m_device.PerfSetMarker( 60 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_ComputeLighting.Use() ) {
				m_device.SetRenderTargets( new IView[] { m_tex_radiance1.GetView( 0, 1, 0, 1 ), m_tex_finalRender1.GetView( 0, 1, 0, 1 ) }, null );

				m_tex_albedo.SetPS( 0 );
				m_tex_normal.SetPS( 1 );
				m_tex_emissive.SetPS( 2 );
				m_tex_depthWithMips.SetPS( 3 );
				m_tex_blueNoise.SetPS( 4 );

				m_tex_shadowPoint.SetPS( 6 );
				m_tex_shadowDirectional.SetPS( 7 );

				// Provide our dearly computed HBIL buffers to the lighting pass
				#if RECOMPOSE || BRUTE_FORCE_HBIL
					m_tex_radiance0.SetPS( 8 );
					m_tex_bentCone.SetPS( 9 );
				#else
					m_tex_splitIrradiance.SetPS( 8 );
					m_tex_splitBentCone.SetPS( 9 );
				#endif

				m_device.RenderFullscreenQuad( m_shader_ComputeLighting );

				m_device.RemoveRenderTargets();
				m_tex_radiance0.RemoveFromLastAssignedSlots();

				Texture2D	temp = m_tex_radiance0;
				m_tex_radiance0 = m_tex_radiance1;
				m_tex_radiance1 = temp;

				temp = m_tex_finalRender0;
				m_tex_finalRender0 = m_tex_finalRender1;
				m_tex_finalRender1 = temp;
			}
//*/

			//////////////////////////////////////////////////////////////////////////
			// =========== Temporal Anti-Aliasing ===========
			m_device.PerfSetMarker( 70 );

			if ( checkBoxEnableTAA.Checked && m_shader_TAA.Use() ) {
				m_tex_finalRender0.SetCS( 0 );
				m_tex_motionVectors_Gather.SetCS( 1 );
				m_tex_TAAHistory0.SetCS( 2 );
				m_tex_finalRender1.SetCSUAV( 0 );
				m_tex_TAAHistory1.SetCSUAV( 1 );

				float	dx = (float) SimpleRNG.GetUniform() * 2.0f - 1.0f;
				float	dy = (float) SimpleRNG.GetUniform() * 2.0f - 1.0f;
				m_CB_TAA.m._TAADither.Set( dx, dy );
				m_CB_TAA.m._TAAAmount = 0.1f;
				m_CB_TAA.UpdateData();

// 				if( tr.GetTemporalAAHistoryClearFlag() ) {
// 					graphicContext.ClearRenderTarget( *pHistoryView_read );
// 				}

				m_shader_TAA.Dispatch( (m_tex_radiance1.Width + 15) >> 4, (m_tex_radiance1.Height + 15) >> 4, 1 );

				m_tex_TAAHistory1.RemoveFromLastAssignedSlotUAV();
				m_tex_finalRender1.RemoveFromLastAssignedSlotUAV();

				// Swap history & HDR buffers
				Texture2D	temp = m_tex_TAAHistory0;
				m_tex_TAAHistory0 = m_tex_TAAHistory1;
				m_tex_TAAHistory1 = temp;

				temp = m_tex_finalRender0;
				m_tex_finalRender0 = m_tex_finalRender1;
				m_tex_finalRender1 = temp;
			}


			//////////////////////////////////////////////////////////////////////////
			// =========== Post-Process ===========
			m_device.PerfSetMarker( 80 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_PostProcess.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

//				m_tex_motionVectors_Scatter.SetPS( 4 );
				m_tex_motionVectors_Gather.SetPS( 5 );

				m_tex_radiance0.SetPS( 8 );
				m_tex_radiance1.SetPS( 9 );
				m_tex_bentCone.SetPS( 10 );
				m_tex_finalRender0.SetPS( 11 );

				// DEBUG PUSH/PULL
				m_tex_sourceRadiance_PUSH.SetPS( 12 );
				m_tex_sourceRadiance_PULL.SetPS( 13 );

				// DEBUG SPLIT BUFFERS
				#if !BRUTE_FORCE_HBIL
					m_tex_splitDepth.SetPS( 20 );
					m_tex_splitNormal.SetPS( 21 );
					m_tex_splitRadiance.SetPS( 22 );
					m_tex_splitIrradiance.SetPS( 23 );
					m_tex_splitBentCone.SetPS( 24 );
				#endif

				m_device.DefaultDepthStencil.SetPS( 30 );

				m_device.RenderFullscreenQuad( m_shader_PostProcess );

				m_tex_depthWithMips.RemoveFromLastAssignedSlots();
				m_tex_emissive.RemoveFromLastAssignedSlots();
//				m_tex_motionVectors_Scatter.RemoveFromLastAssignedSlots();
				m_tex_motionVectors_Gather.RemoveFromLastAssignedSlots();
				m_tex_normal.RemoveFromLastAssignedSlots();
				m_tex_albedo.RemoveFromLastAssignedSlots();

				m_tex_radiance0.RemoveFromLastAssignedSlots();
				m_tex_radiance1.RemoveFromLastAssignedSlots();
				m_tex_bentCone.RemoveFromLastAssignedSlots();
				m_tex_finalRender0.RemoveFromLastAssignedSlots();

				m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlots();
				m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlots();

				#if !BRUTE_FORCE_HBIL
					m_tex_splitDepth.RemoveFromLastAssignedSlots();
					m_tex_splitNormal.RemoveFromLastAssignedSlots();
					m_tex_splitRadiance.RemoveFromLastAssignedSlots();
					m_tex_splitIrradiance.RemoveFromLastAssignedSlots();
					m_tex_splitBentCone.RemoveFromLastAssignedSlots();
				#endif

				m_device.DefaultDepthStencil.RemoveFromLastAssignedSlots();
			}

			//////////////////////////////////////////////////////////////////////////
			// Debug cone
			if ( m_shader_RenderDebugCone.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
				m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );
				m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

				m_CB_DebugCone.m._wsConePosition = m_softwareHBILComputer.WorldSpaceConePosition;
				m_CB_DebugCone.m._wsConeDirection = m_softwareHBILComputer.WorldSpaceConeDirection;
				m_CB_DebugCone.m._coneAngle = m_softwareHBILComputer.AverageConeAngle;
				m_CB_DebugCone.m._coneStdDeviation = m_softwareHBILComputer.StandardDeviation;
				m_CB_DebugCone.m._flags = 0;
				m_CB_DebugCone.UpdateData();
				m_primCylinder.Render( m_shader_RenderDebugCone );

				m_CB_DebugCone.m._flags = 1;
				m_CB_DebugCone.UpdateData();
				m_primCylinder.Render( m_shader_RenderDebugCone );
			}

			// Show!
			m_device.Present( false );
			double	totalFrameTime = m_device.PerfEndFrame();
			m_framesCount++;

			//////////////////////////////////////////////////////////////////////////
			// Update auto camera rotation
			if ( checkBoxAutoRotateCamera.Checked ) {
				float4x4	camera2World = m_camera.Camera2World;
				float4x4	rot = float4x4.Identity;
							rot.BuildRotationY( floatTrackbarControlCameraRotateSpeed.Value * deltaTime );
				m_camera.Camera2World = camera2World * rot;
			}


			//////////////////////////////////////////////////////////////////////////
			// Accumulate time measures
			m_sumTimeReprojection += m_device.PerfGetMilliSeconds( 0 );
			m_sumTimeRenderGBuffer += m_device.PerfGetMilliSeconds( 10 );
			m_sumTimeDownSampleDepth += m_device.PerfGetMilliSeconds( 20 );
			m_sumTimePushPull += m_device.PerfGetMilliSeconds( 30 );
			m_sumTimeShadow += m_device.PerfGetMilliSeconds( 40 );
			m_sumTimeHBIL += m_device.PerfGetMilliSeconds( 50, 60 );
			#if BRUTE_FORCE_HBIL
				m_sumTimeHBIL_split += 0;
				m_sumTimeHBIL_render += m_device.PerfGetMilliSeconds( 50, 60 );
				m_sumTimeHBIL_recompose += 0;
			#else
				m_sumTimeHBIL_split += m_device.PerfGetMilliSeconds( 50, 51 );
				m_sumTimeHBIL_render += m_device.PerfGetMilliSeconds( 51, 52 );
				m_sumTimeHBIL_recompose += m_device.PerfGetMilliSeconds( 52 );
			#endif
			m_sumTimeComputeLighting += m_device.PerfGetMilliSeconds( 60 );
			m_sumTimeTAA += m_device.PerfGetMilliSeconds( 70 );
			m_sumTimePostProcess += m_device.PerfGetMilliSeconds( 80 );
			m_sumTimeCounter++;


			//////////////////////////////////////////////////////////////////////////
			// Update information box
			if ( m_currentTime - m_lastDisplayTime < 1.0f )
				return;
			m_lastDisplayTime = m_currentTime;

			// Finalize average measures
			m_sumTimeReprojection /= m_sumTimeCounter;
			m_sumTimeRenderGBuffer /= m_sumTimeCounter;
			m_sumTimeDownSampleDepth /= m_sumTimeCounter;
			m_sumTimePushPull /= m_sumTimeCounter;
			m_sumTimeShadow /= m_sumTimeCounter;
			m_sumTimeHBIL /= m_sumTimeCounter;
			m_sumTimeHBIL_split /= m_sumTimeCounter;
			m_sumTimeHBIL_render /= m_sumTimeCounter;
			m_sumTimeHBIL_recompose /= m_sumTimeCounter;
			m_sumTimeComputeLighting /= m_sumTimeCounter;
			m_sumTimeTAA /= m_sumTimeCounter;
			m_sumTimePostProcess /= m_sumTimeCounter;

// 			float	totalTime = m_currentTime - m_startTime;
			textBoxInfo.Text = "" //"Total Time = " + totalTime + " seconds\r\n"
//							 + "Average frame time = " + (1000.0f * totalTime / m_framesCount).ToString( "G6" ) + " ms\r\n"
//							 + "\r\n"
							 + "Reprojection: " + m_sumTimeReprojection.ToString( "G4" ) + " ms\r\n"
							 + "Push-Pull: " + m_sumTimePushPull.ToString( "G4" ) + " ms\r\n"
							 + "G-Buffer Rendering: " + m_sumTimeRenderGBuffer.ToString( "G4" ) + " ms\r\n"
							 + "Shadow Map: " + m_sumTimeShadow.ToString( "G4" ) + " ms\r\n"
							 + "DownSample Depth: " + m_sumTimeDownSampleDepth.ToString( "G4" ) + " ms\r\n"
							 + "HBIL: " + m_sumTimeHBIL.ToString( "G4" ) + " ms (" + (1.729f * m_sumTimeHBIL).ToString( "G4" ) + " ms XB1)\r\n"
							 + " ► Split: " + m_sumTimeHBIL_split.ToString( "G4" ) + " ms - Render: " + m_sumTimeHBIL_render.ToString( "G4" ) + " ms - Recompose: " + m_sumTimeHBIL_recompose.ToString( "G4" ) + "ms\r\n"
							 + "Lighting: " + m_sumTimeComputeLighting.ToString( "G4" ) + " ms\r\n"
							 + "TAA: " + m_sumTimeTAA.ToString( "G4" ) + " ms\r\n"
							 + "Post-Processing: " + m_sumTimePostProcess.ToString( "G4" ) + " ms\r\n"
							 + "Total frame time = " + totalFrameTime.ToString( "G6" ) + " ms\r\n"
							 + "\r\n"
							 ;

			// Clear times
			m_sumTimeReprojection = 0.0;
			m_sumTimeRenderGBuffer = 0.0;
			m_sumTimeDownSampleDepth = 0.0;
			m_sumTimePushPull = 0.0;
			m_sumTimeShadow = 0.0;
			m_sumTimeHBIL = 0.0;
			m_sumTimeHBIL_split = 0.0;
			m_sumTimeHBIL_render = 0.0;
			m_sumTimeHBIL_recompose = 0.0;
			m_sumTimeComputeLighting = 0.0;
			m_sumTimeTAA = 0.0;
			m_sumTimePostProcess = 0.0;
			m_sumTimeCounter = 0;
		}

		int		m_sumTimeCounter = 0;
		double	m_sumTimeReprojection = 0.0;
		double	m_sumTimeRenderGBuffer = 0.0;
		double	m_sumTimeDownSampleDepth = 0.0;
		double	m_sumTimePushPull = 0.0;
		double	m_sumTimeShadow = 0.0;
		double	m_sumTimeHBIL = 0.0;
		#if BRUTE_FORCE_HBIL
			double	m_sumTimeHBIL_split = 0.0;
			double	m_sumTimeHBIL_render = 0.0;
			double	m_sumTimeHBIL_recompose = 0;
		#else
			double	m_sumTimeHBIL_split = 0.0;
			double	m_sumTimeHBIL_render = 0.0;
			double	m_sumTimeHBIL_recompose = 0.0;
		#endif
		double	m_sumTimeComputeLighting = 0.0;
		double	m_sumTimeTAA = 0.0;
		double	m_sumTimePostProcess = 0.0;


		public float	GetGameTime() {
			long	ticks = m_stopWatch.ElapsedTicks;
			float	time = (float) (ticks * m_ticks2Seconds);
			return time;
		}

		#region Bayer

		// Simple recursive Bayer matrix generation
		// We start from the root permutation matrix:
		//	I2 = |1 2|
		//       |3 0|
		//
		// Next, we can recursively obtain successive matrices by applying:
		//  I2n = | 4*In+1 4*In+2 |
		//        | 4*In+3 4*In+0 |
		//
		// Generates the basic 2x2 Bayer permutation matrix:
		//  [1 2]
		//  [3 0]
		// Expects _P in [0,1]
		uint B2( uint _X, uint _Y ) {
			return ((_Y << 1) + _X + 1) & 3;
		}

		// Generates the 4x4 matrix
		// Expects _P any pixel coordinate
		uint B4( uint _X, uint _Y ) {
			return (B2( _X & 1, _Y & 1 ) << 2)
				  + B2( (_X >> 1) & 1, (_Y >> 1) & 1);
		}

		uint	Bayer1D_1( uint _time ) {	// [0,2[
			return 1 - (_time & 1);
		}
		uint	Bayer1D_4( uint _time ) {	// [0,4[
			return Bayer1D_1( _time >> 1 ) + (Bayer1D_1( _time ) << 1);
		}
		uint	Bayer1D_16( uint _time ) {	// [0,16[
			return Bayer1D_4( _time >> 2 ) + (Bayer1D_4( _time & 3 ) << 2);
		}
		uint	Bayer1D_64( uint _time ) {	// [0,63[
			return Bayer1D_16( _time >> 3 ) + (Bayer1D_16( _time & 7 ) << 3);
		}

		#endregion

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _fileName, COMPONENT_FORMAT _componentFormat ) {
			ImagesMatrix	images = null;
			if ( _fileName.Extension.ToLower() == ".dds" ) {
				images = new ImagesMatrix();
				images.DDSLoadFile( _fileName );
			} else {
				ImageFile	image = new ImageFile( _fileName );
				if ( image.PixelFormat != PIXEL_FORMAT.BGRA8 ) {
					ImageFile	badImage = image;
					image = new ImageFile();
					image.ConvertFrom( badImage, PIXEL_FORMAT.BGRA8 );
					badImage.Dispose();
				}
				images = new ImagesMatrix( new ImageFile[1,1] { { image } } );
			}
			return new Texture2D( m_device, images, _componentFormat );
		}

		#endregion

		#region Temporal AA

		//////////////////////////////////////////////////////////////////////////
		// From "Scalable AO" (McGuire)
		//	We observe that the following increase the accuracy of z values computed from a depth buffer on modern GPUs:
		//		1. Compute the modelview-projection matrix at double precision on the host before casting to GPU single precision.
		//		   This comprises three matrix products (projection, camera, and object), divisions, and trigonometric operations.
		//
		//		2. Choose zf = −∞ in the projection matrix [Smi83]. This reduces the number of floating point ALU operations required for the matrix product [UD12].
		//
		//		3. When using column-major matrices (the default in OpenGL), multiply vectors on the left (v'= v^T * P) in the vertex shader. This saves about half a bit of precision.
		//
		//	Note that camera-space positions and normals stored in a G-buffer, such as those by AlchemyAO, also contain error.
		//	The rasterizer interpolates those across a triangle asC/z,nˆ/z, and then divides by z at each pixel, where z itself was interpolated as 1/z with limited fixed-point precision.
		//////////////////////////////////////////////////////////////////////////
		//
		double[,]	m_camera2World_double = new double[4,4] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } };
		double[,]	m_world2Camera_double = new double[4,4];

		double[,]	m_camera2Projection_double = new double[4,4];
		double[,]	m_projection2Camera_double = new double[4,4];
		
		double[,]	m_world2Projection_double = new double[4,4];
		double[,]	m_Projection2World_double = new double[4,4];

		// Previous frame matrices
		double[,]	m_previousFrameCamera2World_double = new double[4,4];
		double[,]	m_previousFrameCamera2Camera_double = new double[4,4];
		double[,]	m_camera2PreviousFrameCamera_double = new double[4,4];

		void	SetupCameraMatrices() {

			//////////////////////////////////////////////////////////////////////////
			// Build previous frame matrices
//			MatrixDouble2Single( m_camera2Projection_double, m_CB_Camera.m._previousCamera2Proj );
			MatrixDouble2Single( m_world2Projection_double, ref m_CB_Camera.m._previousWorld2Proj );

			// Keep previous camera=>world matrix
			Array.Copy( m_camera2World_double, m_previousFrameCamera2World_double, 4*4 );

			// Keep previous jitter offset
			float2	previousSubPixelJitter = m_subPixelJitter;

			//////////////////////////////////////////////////////////////////////////
			// Build this frame's matrices

			// Retrieve our current camera matrix from single-precision
			MatrixSingle2Double( ref m_CB_Camera.m._camera2World, m_camera2World_double );

			// Build projection matrix
			float	aspectRatio = (float) m_device.DefaultTarget.Width / m_device.DefaultTarget.Height;
			float	TAN_HALF_FOV_X = aspectRatio * TAN_HALF_FOV;
			BuildProjectionMatrix( Z_NEAR, Z_FAR, TAN_HALF_FOV_X, TAN_HALF_FOV, (int) m_device.DefaultTarget.Width, (int) m_device.DefaultTarget.Height, m_camera2Projection_double );
			MatrixDoubleInvert( m_camera2Projection_double, m_projection2Camera_double );

			// Compose all other matrices in double-precision
			MatrixDoubleInvert( m_camera2World_double, m_world2Camera_double );
			MatrixDoubleProduct( m_world2Camera_double, m_camera2Projection_double, m_world2Projection_double );
//			MatrixDoubleInvert( m_world2Projection_double, m_Projection2World_double );
			MatrixDoubleProduct( m_projection2Camera_double, m_camera2World_double, m_Projection2World_double );	// More precise than line above since we're doing the product of a "true matrix" and an inverse, rather than inverting a product

			// Setup forward reprojection matrices
			if ( !checkBoxFreezePrev2CurrentCamMatrix.Checked ) {
				MatrixDoubleProduct( m_previousFrameCamera2World_double, m_world2Camera_double, m_previousFrameCamera2Camera_double );
				MatrixDoubleInvert( m_previousFrameCamera2Camera_double, m_camera2PreviousFrameCamera_double );
			}

			//////////////////////////////////////////////////////////////////////////
			// Store back into single-precision
			MatrixDouble2Single( m_world2Camera_double, ref m_CB_Camera.m._world2Camera );
			MatrixDouble2Single( m_camera2Projection_double, ref m_CB_Camera.m._camera2Proj );
			MatrixDouble2Single( m_projection2Camera_double, ref m_CB_Camera.m._proj2Camera );
			MatrixDouble2Single( m_world2Projection_double, ref m_CB_Camera.m._world2Proj );
			MatrixDouble2Single( m_Projection2World_double, ref m_CB_Camera.m._proj2World );
			MatrixDouble2Single( m_previousFrameCamera2Camera_double, ref m_CB_Camera.m._previousCamera2Camera );
			MatrixDouble2Single( m_camera2PreviousFrameCamera_double, ref m_CB_Camera.m._camera2PreviousCamera );

			// Setup jitter canceling vector & current frame jitter offset
			float2	jitterCanceling = (previousSubPixelJitter - m_subPixelJitter) * 2.0f;
			m_CB_Camera.m._subPixelOffsets.Set( jitterCanceling.x, jitterCanceling.y, m_subPixelJitter.x, m_subPixelJitter.y );
		}

		// Fixed array of sub-pixel offsets
		float2[]	m_subPixelOffsets = new float2[] {
			new float2(  0.0625f, -0.1875f ), 
			new float2( -0.0625f,  0.1875f ),
			new float2(  0.3125f,  0.0625f ),
			new float2( -0.1875f, -0.3125f ),
			new float2( -0.3125f,  0.3125f ),
			new float2( -0.4375f,  0.0625f ),
			new float2(  0.1875f,  0.4375f ),
			new float2(  0.4375f, -0.4375f )
		};

		float2		m_subPixelOffset = new float2( 0, 0 );	// Used for HQ screenshots
		float2		m_jitterDivider = new float2( 1, 1 );	// Used for HQ screenshots
		void	BuildProjectionMatrix( float _ZNear, float _ZFar, double _tanHalfFOVX, double _tanHalfFOVY, int _screenWidth, int _screenHeight, double[,] _camera2Projection ) {

			// Random jittering is useful when multiple frames are going to be blended together for motion blurred anti-aliasing.
			float2	jitterOffset = checkBoxEnableTAA.Checked ? m_subPixelOffsets[(int) m_framesCount % m_subPixelOffsets.Length]	// Auto-jitter
															 : m_subPixelOffset;													// Fixed user-specified jitter (for HQ screenshots)

			double	subPixelJitterX = (double) jitterOffset.x / _screenWidth;
			double	subPixelJitterY = (double) jitterOffset.y / _screenHeight;
			m_subPixelJitter.Set( (float) subPixelJitterX, (float) subPixelJitterY );

//			m_CB_Camera.m._subPixelOffset = new float4( m_subPixelOffset.x / _screenWidth, -m_subPixelOffset.y / _screenHeight, m_jitterDivider.x, m_jitterDivider.y );

			// Create the matrix
			_camera2Projection[0,0] = 1.0 / _tanHalfFOVX;
			_camera2Projection[1,0] = 0.0;
			_camera2Projection[2,0] = 2.0 * subPixelJitterX;
			_camera2Projection[3,0] = 0.0;

			_camera2Projection[0,1] = 0.0;
			_camera2Projection[1,1] = 1.0 / _tanHalfFOVY;
			_camera2Projection[2,1] = 2.0 * subPixelJitterY;
			_camera2Projection[3,1] = 0.0;

			if ( _ZFar <= _ZNear ) {
				// this is the far-plane-at-infinity formulation
				_camera2Projection[0,2] = 0.0;
				_camera2Projection[1,2] = 0.0;
				_camera2Projection[2,2] = -1.0;
				_camera2Projection[3,2] = -_ZFar;
			} else {
				_camera2Projection[0,2] = 0.0;
				_camera2Projection[1,2] = 0.0;
// 				// Depth inversion
// 				_camera2Projection[2,2] = -_ZNear / (_ZNear - _ZFar);
// 				_camera2Projection[3,2] = -_ZNear * _ZFar / (_ZNear - _ZFar);
				_camera2Projection[2,2] = _ZFar / (_ZFar - _ZNear);
				_camera2Projection[3,2] = -_ZNear * _ZFar / (_ZFar - _ZNear);
			}

			_camera2Projection[0,3] = 0.0;
			_camera2Projection[1,3] = 0.0;
			_camera2Projection[2,3] = 1.0;
			_camera2Projection[3,3] = 0.0;
		}

		void	MatrixDouble2Single( double[,] _source, ref float4x4 _target ) {
			_target.r0.x = (float) _source[0,0];
			_target.r0.y = (float) _source[0,1];
			_target.r0.z = (float) _source[0,2];
			_target.r0.w = (float) _source[0,3];
			_target.r1.x = (float) _source[1,0];
			_target.r1.y = (float) _source[1,1];
			_target.r1.z = (float) _source[1,2];
			_target.r1.w = (float) _source[1,3];
			_target.r2.x = (float) _source[2,0];
			_target.r2.y = (float) _source[2,1];
			_target.r2.z = (float) _source[2,2];
			_target.r2.w = (float) _source[2,3];
			_target.r3.x = (float) _source[3,0];
			_target.r3.y = (float) _source[3,1];
			_target.r3.z = (float) _source[3,2];
			_target.r3.w = (float) _source[3,3];
		}

		void	MatrixSingle2Double( ref float4x4 _source, double[,] _target ) {
			_target[0,0] = _source.r0.x;
			_target[0,1] = _source.r0.y;
			_target[0,2] = _source.r0.z;
			_target[0,3] = _source.r0.w;
			_target[1,0] = _source.r1.x;
			_target[1,1] = _source.r1.y;
			_target[1,2] = _source.r1.z;
			_target[1,3] = _source.r1.w;
			_target[2,0] = _source.r2.x;
			_target[2,1] = _source.r2.y;
			_target[2,2] = _source.r2.z;
			_target[2,3] = _source.r2.w;
			_target[3,0] = _source.r3.x;
			_target[3,1] = _source.r3.y;
			_target[3,2] = _source.r3.z;
			_target[3,3] = _source.r3.w;
		}

		// c = a * b
		void	MatrixDoubleProduct( double[,] a, double[,] b, double[,] c ) {
			c[0,0] = a[0,0] * b[0,0] + a[0,1] * b[1,0] + a[0,2] * b[2,0] + a[0,3] * b[3,0];
			c[0,1] = a[0,0] * b[0,1] + a[0,1] * b[1,1] + a[0,2] * b[2,1] + a[0,3] * b[3,1];
			c[0,2] = a[0,0] * b[0,2] + a[0,1] * b[1,2] + a[0,2] * b[2,2] + a[0,3] * b[3,2];
			c[0,3] = a[0,0] * b[0,3] + a[0,1] * b[1,3] + a[0,2] * b[2,3] + a[0,3] * b[3,3];

			c[1,0] = a[1,0] * b[0,0] + a[1,1] * b[1,0] + a[1,2] * b[2,0] + a[1,3] * b[3,0];
			c[1,1] = a[1,0] * b[0,1] + a[1,1] * b[1,1] + a[1,2] * b[2,1] + a[1,3] * b[3,1];
			c[1,2] = a[1,0] * b[0,2] + a[1,1] * b[1,2] + a[1,2] * b[2,2] + a[1,3] * b[3,2];
			c[1,3] = a[1,0] * b[0,3] + a[1,1] * b[1,3] + a[1,2] * b[2,3] + a[1,3] * b[3,3];

			c[2,0] = a[2,0] * b[0,0] + a[2,1] * b[1,0] + a[2,2] * b[2,0] + a[2,3] * b[3,0];
			c[2,1] = a[2,0] * b[0,1] + a[2,1] * b[1,1] + a[2,2] * b[2,1] + a[2,3] * b[3,1];
			c[2,2] = a[2,0] * b[0,2] + a[2,1] * b[1,2] + a[2,2] * b[2,2] + a[2,3] * b[3,2];
			c[2,3] = a[2,0] * b[0,3] + a[2,1] * b[1,3] + a[2,2] * b[2,3] + a[2,3] * b[3,3];

			c[3,0] = a[3,0] * b[0,0] + a[3,1] * b[1,0] + a[3,2] * b[2,0] + a[3,3] * b[3,0];
			c[3,1] = a[3,0] * b[0,1] + a[3,1] * b[1,1] + a[3,2] * b[2,1] + a[3,3] * b[3,1];
			c[3,2] = a[3,0] * b[0,2] + a[3,1] * b[1,2] + a[3,2] * b[2,2] + a[3,3] * b[3,2];
			c[3,3] = a[3,0] * b[0,3] + a[3,1] * b[1,3] + a[3,2] * b[2,3] + a[3,3] * b[3,3];
		}

		// b = a^-1
		void	MatrixDoubleInvert( double[,] a, double[,] b ) {
			double	det = MatrixDoubleDeterminant( a );
			if ( Math.Abs( det ) < 1e-28 )
				throw new Exception( "Matrix is not inversible!" );
			det = 1.0 / det;

			b[0,0] = CoFactor( a, 0, 0 ) * det;
			b[0,1] = CoFactor( a, 1, 0 ) * det;
			b[0,2] = CoFactor( a, 2, 0 ) * det;
			b[0,3] = CoFactor( a, 3, 0 ) * det;
			b[1,0] = CoFactor( a, 0, 1 ) * det;
			b[1,1] = CoFactor( a, 1, 1 ) * det;
			b[1,2] = CoFactor( a, 2, 1 ) * det;
			b[1,3] = CoFactor( a, 3, 1 ) * det;
			b[2,0] = CoFactor( a, 0, 2 ) * det;
			b[2,1] = CoFactor( a, 1, 2 ) * det;
			b[2,2] = CoFactor( a, 2, 2 ) * det;
			b[2,3] = CoFactor( a, 3, 2 ) * det;
			b[3,0] = CoFactor( a, 0, 3 ) * det;
			b[3,1] = CoFactor( a, 1, 3 ) * det;
			b[3,2] = CoFactor( a, 2, 3 ) * det;
			b[3,3] = CoFactor( a, 3, 3 ) * det;
		}

		double	MatrixDoubleDeterminant( double[,] a ) {
			return a[0,0] * CoFactor( a, 0, 0 ) + a[0,1] * CoFactor( a, 0, 1 ) + a[0,2] * CoFactor( a, 0, 2 ) + a[0,3] * CoFactor( a, 0, 3 ); 
		}

		double	CoFactor( double[,] a, int _row, int _col ) {
			int		col1 = (_col+1) & 3;
			int		col2 = (_col+2) & 3;
			int		col3 = (_col+3) & 3;
			int		row1 = (_row+1) & 3;
			int		row2 = (_row+2) & 3;
			int		row3 = (_row+3) & 3;
			double	sign = 2.0 * ((_row + _col) & 1) - 1.0;

			double v = (	a[row1,col1] * a[row2,col2] * a[row3,col3] +
							a[row1,col2] * a[row2,col3] * a[row3,col1] +
							a[row1,col3] * a[row2,col1] * a[row3,col2]
						)
					 - (	a[row3,col1] * a[row2,col2] * a[row1,col3] +
							a[row3,col2] * a[row2,col3] * a[row1,col1] +
							a[row3,col3] * a[row2,col1] * a[row1,col2]
						);
			return sign * v;
		}

		#endregion

		private void buttonReload_Click( object sender, EventArgs e ) {
			if ( m_device != null )
				m_device.ReloadModifiedShaders();
			m_directionalShadowDirty = true;
		}

		private void buttonClear_Click( object sender, EventArgs e ) {
			m_device.Clear( m_tex_radiance0, float4.Zero );
			m_device.Clear( m_tex_radiance1, float4.Zero );

			m_device.Clear( m_tex_TAAHistory0, float4.UnitW );
			m_device.Clear( m_tex_TAAHistory1, float4.UnitW );
		}

		#region Camera & Environment Light Manipulation

		bool m_manipulator_EnableMouseAction( MouseEventArgs _e ) {
			return (Control.ModifierKeys & Keys.Alt) == 0;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._camera2World = m_camera.Camera2World;

// This is now done each frame in double-precision by the SetupCameraMatrices() method
// 			m_CB_Camera.m._World2Camera = m_camera.World2Camera;
// 
// 			m_CB_Camera.m._Camera2Proj = m_camera.Camera2Proj;
// 			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;
// 
// 			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
// 			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;
		}

//		Quat			m_lightQuat = new Quat( new AngleAxis( 0.0f, float3.UnitZ ) );
		#if SCENE_SPONZA || SCENE_SPONZA_POINT_LIGHT || SCENE_SIBENIK
//			Quat			m_lightQuat = new Quat( new AngleAxis( -0.45f * Mathf.PI, float3.UnitX ) );
			Quat			m_lightQuat = new Quat( 0.5310309f, -0.612917f, -0.3831305f, -0.4422101f );
		#elif SCENE_SCHAEFFER
			Quat			m_lightQuat = new Quat( 0.414818674f, -0.789189041f, -0.210713729f, -0.400881141f );
		#elif SCENE_HEIGHTFIELD
			Quat			m_lightQuat = new Quat( -0.8959564f, 0.0131233633f, 0.4439005f, 0.006501928f );
		#else
			Quat			m_lightQuat = new Quat( new AngleAxis( -0.3f * Mathf.PI, float3.UnitX ) );
		#endif
		float3x3		m_lightRotation = float3x3.Identity;
// 		float3x3		m_lightRotation = (float3x3) m_lightQuat;

		float3[]		m_lightSH = new float3[9];
		double[,]		m_SHRotation = new double[9,9];
		float3[]		m_rotatedLightSH = new float3[9];

		MouseButtons	m_buttonDown = MouseButtons.None;
		Point			m_buttonDownPosition;
		Quat			m_buttonDownLightQuat;
		float3x3		m_buttonDownLightRotation = float3x3.Identity;

		private void panelOutput_MouseDown( object sender, MouseEventArgs e ) {
			if ( Control.ModifierKeys == Keys.Control )
				DebugHBIL( (uint) e.X, (uint) e.Y );

			if ( m_manipulator_EnableMouseAction( e ) )
				return;	// Don't do anything if camera manipulator is enabled

			m_buttonDownPosition = e.Location;
			m_buttonDownLightQuat = new Quat( m_lightQuat );
			m_buttonDownLightRotation = m_lightRotation;
			m_buttonDown |= e.Button;
//			Capture = true;
		}

		private void panelOutput_MouseUp( object sender, MouseEventArgs e ) {
			m_buttonDown &= ~e.Button;
			if ( m_buttonDown == MouseButtons.None )
				Capture = false;
		}

		private void panelOutput_MouseMove( object sender, MouseEventArgs e ) {
			if ( m_buttonDown == MouseButtons.None )
				return;
			if ( m_manipulator_EnableMouseAction( e ) )
				return;	// Don't do anything if camera manipulator is enabled

			if ( (m_buttonDown & System.Windows.Forms.MouseButtons.Left) != 0 ) {
				float	Dx = e.Location.X - m_buttonDownPosition.X;
				float	Dy = e.Location.Y - m_buttonDownPosition.Y;
				Quat	rotY = new Quat( new AngleAxis( -0.01f * Dx, float3.UnitY ) );
//				Quat	rotX = new Quat( new AngleAxis( 0.01f * Dy, float3.UnitX ) );
				Quat	rotX = new Quat( new AngleAxis( 0.01f * Dy, m_buttonDownLightRotation.r0 ) );
				m_lightQuat = rotY * rotX * m_buttonDownLightQuat;
				m_lightRotation = (float3x3) m_lightQuat;
				UpdateSH();
			}
		}

		void	UpdateSH() {
			SphericalHarmonics.SHFunctions.BuildRotationMatrix( m_lightRotation, m_SHRotation, 3 );
			SphericalHarmonics.SHFunctions.Rotate( m_lightSH, m_SHRotation, m_rotatedLightSH, 3 );

			// Update directional shadow map matrix as well
			float3	wsLightDirection = m_lightRotation.r2;
			float3	wsLightRight, wsLightUp;
			wsLightDirection.OrthogonalBasis( out wsLightRight, out wsLightUp );

			#if SCENE_SPONZA || SCENE_SPONZA_POINT_LIGHT || SCENE_SIBENIK
				const float	WIDTH = 50.0f;
				const float	HEIGHT = 50.0f;
				const float	Z_FAR = 80.0f;
				const float	Z_OFFSET = 40.0f;
				float3	CENTER = new float3( 0, 0, 0 );
			#elif SCENE_SCHAEFFER
				const float	WIDTH = 20.0f;
				const float	HEIGHT = 20.0f;
				const float	Z_FAR = 40.0f;
				const float	Z_OFFSET = 20.0f;
				float3	CENTER = new float3( 0, 0, 0 );
			#elif SCENE_INFINITE_ROOMS
				const float	WIDTH = 90.0f;
				const float	HEIGHT = 90.0f;
				const float	Z_FAR = 80.0f;
				const float	Z_OFFSET = 40.0f;
				float3	CENTER = new float3( -30.0f, -15.0f, 0 );
			#elif SCENE_HEIGHTFIELD
				const float	WIDTH = 6.0f;
				const float	HEIGHT = 6.0f;
				const float	Z_FAR = 8.0f;
				const float	Z_OFFSET = 4.0f;
				float3	CENTER = new float3( 0.0f, 0.0f, 0 );
			#else
				const float	WIDTH = 90.0f;
				const float	HEIGHT = 90.0f;
				const float	Z_FAR = 80.0f;
				const float	Z_OFFSET = 40.0f;
				float3	CENTER = new float3( -30.0f, -15.0f, 0 );
			#endif

			m_CB_Shadow.m._directionalShadowMap2World.r0.Set( wsLightRight, WIDTH );
			m_CB_Shadow.m._directionalShadowMap2World.r1.Set( wsLightUp, HEIGHT );
			m_CB_Shadow.m._directionalShadowMap2World.r2.Set( -wsLightDirection, Z_FAR );
			m_CB_Shadow.m._directionalShadowMap2World.r3.Set( CENTER + Z_OFFSET * wsLightDirection, 1 );
			m_directionalShadowDirty = true;
		}

		/// <summary>
		/// Initializes the SH coeffs for the environment
		/// </summary>
		void	InitEnvironmentSH() {
/* Cosine lobe
			float3	dir = float3.UnitZ;

			const float CosineA0 = Mathf.PI;
			const float CosineA1 = (2.0f * Mathf.PI) / 3.0f;
			const float CosineA2 = Mathf.PI * 0.25f;

			// Band 0
			m_lightSH[0] = 0.282095f * CosineA0 * float3.One;

			// Band 1
			m_lightSH[1] = 0.488603f * dir.y * CosineA1 * float3.One;
			m_lightSH[2] = 0.488603f * dir.z * CosineA1 * float3.One;
			m_lightSH[3] = 0.488603f * dir.x * CosineA1 * float3.One;

			// Band 2
			m_lightSH[4] = 1.092548f * dir.x * dir.y * CosineA2 * float3.One;
			m_lightSH[5] = 1.092548f * dir.y * dir.z * CosineA2 * float3.One;
			m_lightSH[6] = 0.315392f * (3.0f * dir.z * dir.z - 1.0f) * CosineA2 * float3.One;
			m_lightSH[7] = 1.092548f * dir.x * dir.z * CosineA2 * float3.One;
			m_lightSH[8] = 0.546274f * (dir.x * dir.x - dir.y * dir.y) * CosineA2 * float3.One;
*/

			// Ennis House
			m_lightSH[0] = new float3( 4.52989505453915f, 4.30646452463535f, 4.51721251492342f );
			m_lightSH[1] = new float3( 0.387870406203612f, 0.384965748870704f, 0.395325521894004f );
			m_lightSH[2] = new float3( 1.05692530696077f, 1.33538156449369f, 1.82393006020369f );
			m_lightSH[3] = new float3( 6.18680912868925f, 6.19927929741711f, 6.6904772608617f );
			m_lightSH[4] = new float3( 0.756169905467733f, 0.681053631625203f, 0.677636982521888f );
			m_lightSH[5] = new float3( 0.170950637080382f, 0.1709443393056f, 0.200437519088333f );
			m_lightSH[6] = new float3( -3.59338856195816f, -3.37861193089806f, -3.30850268192343f );
			m_lightSH[7] = new float3( 2.65318898618603f, 2.97074561577712f, 3.82264536047523f );
			m_lightSH[8] = new float3( 6.07079134655854f, 6.05819330192308f, 6.50325529149908f );

 			// Grace Cathedral
// 			m_lightSH[0] = new float3( 0.933358105849532f, 0.605499186927096f, 0.450999072970855f );
// 			m_lightSH[1] = new float3( 0.0542981143130068f, 0.0409598475963159f, 0.0355377036564806f );
// 			m_lightSH[2] = new float3( 0.914255336642483f, 0.651103534810611f, 0.518065694132826f );
// 			m_lightSH[3] = new float3( 0.238207071886099f, 0.14912965904707f, 0.0912559191766972f );
// 			m_lightSH[4] = new float3( 0.0321476755042544f, 0.0258939812282057f, 0.0324159089991572f );
// 			m_lightSH[5] = new float3( 0.104707893908821f, 0.0756648975030993f, 0.0749934936107284f );
// 			m_lightSH[6] = new float3( 1.27654512826622f, 0.85613828921136f, 0.618241442250845f );
// 			m_lightSH[7] = new float3( 0.473237767573493f, 0.304160108872238f, 0.193304867770535f );
// 			m_lightSH[8] = new float3( 0.143726445535245f, 0.0847402441253633f, 0.0587779174281925f );

			SphericalHarmonics.SHFunctions.FilterHanning( m_lightSH, 1.8f );
		}

		#endregion


		private void checkBoxForceAlbedo_CheckedChanged( object sender, EventArgs e ) {
			floatTrackbarControlAlbedo.Enabled = checkBoxForceAlbedo.Checked;
			checkBoxMonochrome.Enabled = checkBoxForceAlbedo.Checked;
		}

		private void checkBoxPause_CheckedChanged( object sender, EventArgs e ) {
			if ( checkBoxPause.Checked || !checkBoxAnimate.Checked )
				m_stopWatch.Stop();
			else
				m_stopWatch.Start();
		}

		private void TestHBILForm_KeyDown( object sender, KeyEventArgs e ) {
			if ( e.KeyCode == Keys.A )
				checkBoxFreezePrev2CurrentCamMatrix.Checked ^= true;
			if ( e.KeyCode == Keys.R )
				buttonReload_Click( sender, EventArgs.Empty );
			if ( e.KeyCode == Keys.Space )
				checkBoxEnableHBIL.Checked ^= true;
		}

		private void	DebugHBIL( uint _X, uint _Y ) {
			m_softwareHBILComputer.Setup( m_tex_depthWithMips, m_tex_normal, m_tex_sourceRadiance_PULL );
			m_softwareHBILComputer.GatherSphereMaxRadius_m = floatTrackbarControlGatherSphereRadius_meters.Value;
			m_softwareHBILComputer.Camera2World = m_CB_Camera.m._camera2World;
			m_softwareHBILComputer.World2Proj = m_CB_Camera.m._world2Proj;
			m_softwareHBILComputer.Compute( _X, _Y );
		}

		private void checkBoxShowAO_CheckedChanged( object sender, EventArgs e ) {
			buttonClear_Click( sender, e );
		}

		#endregion
	}
}
