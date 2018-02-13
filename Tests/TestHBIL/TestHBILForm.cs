//#define BRUTE_FORCE_HBIL		// 25ms at 1280x720... :D
//#define RENDER_IN_DEPTH_STENCIL	// If defined, use the depth-stencil (single mip level) to render, instead of multi-mip RT (this RT allows larger sample footprints when gathering radiance and accelerates the HBIL pass)
#define BILATERAL_PUSH_PULL

//#define SCENE_LIBRARY
#define SCENE_CORNELL
//#define SCENE_HEIGHTFIELD

//////////////////////////////////////////////////////////////////////////
// Horizon-Based Indirect Lighting Demo
//////////////////////////////////////////////////////////////////////////
//
// IDEAS / #TODOS:
//	✓ Use push/pull (with bilateral) to fill in reprojected radiance voids!!!
//	✓ float	GetBilateralWeight( Z0, Z1, radius, ref sqHypotenuse ) => Outside of unit sphere???
//	✓ Use radius² as progression
//		=> Doesn't provide any significant improvement
//	✓ Compute AO value!!
//	✓ Emissive surfaces???
//	✓ Keep previous radiance in case we reject height sample but accept radiance, and don't want to interpolate foreground radiance? Will that even occur?
//	✓ Use normal dot product weighting anyway?? It looks closer to ground truth in the ground truth simulator! Check it!
//		=> Nope. Looks better with actual bent normal.
//
//	• Sample mips for larger footprint (only if mip is bilateral filtered!)
//	• Write interleaved sampling + reconstruction based on bilateral weight (store it some place? Like alpha somewhere?)
//	• Keep failed reprojected pixels into some "surrounding buffer", some sort of paraboloid-projected buffer containing off-screen values???
//		=> Exponential decay of off-screen values with decay rate depending on camera's linear velocity?? (fun idea!)
//	• Use W linear attenuation as in HBAO?
//	• Advance in local camera space but using screen-space steps
//		=> Must be a linear combination of vector so that advancing 2 pixels equals advancing N meters in camera space...
//
// BUGS:
//	• Horrible noise in reprojection buffer ==> OLD->NEW Camera transform???
//
//
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

		const uint	SHADOW_MAP_SIZE = 512;		// Caution! Must match value declared in Lighting.hlsl!

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float2		_resolution;
			public float		_time;
			public float		_deltaTime;
			public uint			_flags;
			public uint			_sourceRadianceIndex;
			public uint			_debugMipIndex;
			public float		_environmentIntensity;
			public float		_forcedAlbedo;
			public float		_coneAngleBias;
			public float		_exposure;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;

			// Reprojection matrix
			public float4x4		_PrevCamera2Camera;
			public float4x4		_Camera2PrevCamera;
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
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_Shadow {
			public uint		_faceIndex;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		internal struct	CB_HBIL {
			public float4	_bilateralValues;
			public float	_gatherSphereMaxRadius_m;	// Maximum radius (in meters) of the IL gather sphere
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
		private ConstantBuffer<CB_DebugCone>	m_CB_DebugCone = null;

		private ComputeShader		m_shader_ReprojectRadiance = null;

		private ComputeShader		m_shader_Push = null;
		private ComputeShader		m_shader_Pull = null;
		#if !BILATERAL_PUSH_PULL
			private ComputeShader		m_shader_Push_FirstPass = null;
			private ComputeShader		m_shader_Pull_LastPass = null;
		#endif
		private Shader				m_shader_RenderScene_DepthGBufferPass = null;
		private ComputeShader		m_shader_DownSampleDepth = null;
		private Shader				m_shader_AddEmissive = null;
		private Shader				m_shader_RenderScene_Shadow = null;
		private Shader				m_shader_ComputeHBIL = null;
		private Shader				m_shader_ComputeLighting = null;
		private Shader				m_shader_PostProcess = null;

		// G-Buffer
		private Texture2D			m_tex_albedo = null;
		private Texture2D			m_tex_normal = null;
		private Texture2D			m_tex_motionVectors = null;
		private Texture2D			m_tex_emissive = null;
		private Texture2D			m_tex_depthWithMips = null;

		// Shadow map
		private Texture2D			m_tex_shadow = null;

		// HBIL Results
		private Texture2D			m_tex_bentCone = null;
		private Texture2D			m_tex_radiance = null;
		private Texture2D			m_tex_sourceRadiance_PUSH = null;
		private Texture2D			m_tex_sourceRadiance_PULL = null;
		private Texture2D			m_tex_finalRender = null;
		private uint				m_radianceSourceSliceIndex = 0;

		// Regular textures
		private Texture2D			m_tex_BlueNoise = null;
			// Dummy textures with pre-computed heights and normals used to debug the computation
		private Texture2D			m_tex_texDebugHeights = null;
		private Texture2D			m_tex_texDebugNormals = null;

		#if SCENE_CORNELL	
			private Texture2D			m_tex_tomettesAlbedo = null;
			private Texture2D			m_tex_tomettesNormal = null;
			private Texture2D			m_tex_tomettesRoughness = null;
			private Texture2D			m_tex_concreteAlbedo = null;
			private Texture2D			m_tex_concreteNormal = null;
			private Texture2D			m_tex_concreteRoughness = null;

		#endif

		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

		// DEBUG
		private ComputeHBIL			m_softwareHBILComputer = null;	// For DEBUG purposes
		private Primitive			m_primCylinder;
		private Shader				m_shader_RenderDebugCone = null;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_stopWatch = new System.Diagnostics.Stopwatch();
		private double				m_ticks2Seconds;
		private float				m_startTime = 0;
		private float				m_lastDisplayTime = 0;
		private uint				m_framesCount = 0;

		#endregion

		#region METHODS

		public TestHBILForm() {
			InitializeComponent();
		}

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _fileName, COMPONENT_FORMAT _componentFormat ) {
			ImagesMatrix	images = null;
			if ( _fileName.Extension.ToLower() == ".dds" ) {
				images = ImageFile.DDSLoadFile( _fileName );
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
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
			m_CB_SH = new ConstantBuffer<CB_SH>( m_device, 2 );
			m_CB_PushPull = new ConstantBuffer<CB_PushPull>( m_device, 3 );
			m_CB_DownSample = new ConstantBuffer<CB_DownSample>( m_device, 3 );
			m_CB_Shadow = new ConstantBuffer<CB_Shadow>( m_device, 3 );
			m_CB_HBIL = new ConstantBuffer<CB_HBIL>( m_device, 3 );
			m_CB_DebugCone = new ConstantBuffer<CB_DebugCone>( m_device, 3 );

			try {
				// Reprojection shaders
 				m_shader_ReprojectRadiance = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection2.hlsl" ), "CS_Reproject", null );
 				m_shader_Push = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection2.hlsl" ), "CS_Push", null );
				m_shader_Pull = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection2.hlsl" ), "CS_Pull", null );
				#if !BILATERAL_PUSH_PULL
 					m_shader_Push_FirstPass = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection2.hlsl" ), "CS_Push", new ShaderMacro[] { new ShaderMacro( "FIRST_PASS", "1" ) } );
 					m_shader_Pull_LastPass = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection2.hlsl" ), "CS_Pull", new ShaderMacro[] { new ShaderMacro( "LAST_PASS", "1" ) } );
				#endif

				// Scene rendering & lighting
				List< ShaderMacro >	macros = new List<ShaderMacro>();
				#if RENDER_IN_DEPTH_STENCIL
					macros.Add( new ShaderMacro( "USE_DEPTH_STENCIL", "1" ) );
				#endif
				#if SCENE_LIBRARY
					macros.Add( new ShaderMacro( "SCENE_TYPE", "0" ) );
				#elif SCENE_CORNELL
					macros.Add( new ShaderMacro( "SCENE_TYPE", "1" ) );
				#elif SCENE_HEIGHTFIELD
					macros.Add( new ShaderMacro( "SCENE_TYPE", "2" ) );
				#endif

				m_shader_RenderScene_DepthGBufferPass = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_RenderGBuffer", macros.ToArray() );
				m_shader_RenderScene_Shadow = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_RenderShadow", macros.ToArray() );
				m_shader_ComputeLighting = new Shader( m_device, new System.IO.FileInfo( "Shaders/Scene/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_Light", macros.ToArray() );

				// HBIL
				#if BRUTE_FORCE_HBIL
 					m_shader_ComputeHBIL = new Shader( m_device, new System.IO.FileInfo( "Shaders/BruteForce/ComputeHBIL.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				#else
 					m_shader_ComputeHBIL = new Shader( m_device, new System.IO.FileInfo( "Shaders/ComputeHBIL.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				#endif

				// Stuff
				m_shader_AddEmissive = new Shader( m_device, new System.IO.FileInfo( "Shaders/AddEmissive.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_DownSampleDepth = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/DownSampleDepth.hlsl" ), "CS", null );
				m_shader_PostProcess = new Shader( m_device, new System.IO.FileInfo( "Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_RenderDebugCone = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderDebugCone.hlsl" ), VERTEX_FORMAT.P3, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "HBIL Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			// Create buffers
			m_tex_albedo = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_normal = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.SNORM, false, false, null );
			m_tex_emissive = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_motionVectors = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_depthWithMips = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.R16F, COMPONENT_FORMAT.AUTO, false, true, null );
//			m_tex_depthWithMips = new Texture2D( m_device, W, H, 1, 0, DEPTH_STENCIL_FORMAT.D32 );	// Can't have UAV flag so can't use CS for mip downsampling

			// Create shadow map
			m_tex_shadow = new Texture2D( m_device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 6, 1, DEPTH_STENCIL_FORMAT.D32 );

			// Create HBIL buffers
			m_tex_bentCone = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.SNORM, false, false, null );
			m_tex_radiance = new Texture2D( m_device, W, H, 2, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_sourceRadiance_PUSH = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, true, null );
			m_tex_sourceRadiance_PULL = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, true, null );

			m_tex_finalRender = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGB10A2, COMPONENT_FORMAT.UNORM, false, false, null );

			// Create textures
			using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/BlueNoise64x64.png" ) ) )
				using ( ImageFile Imono = new ImageFile() ) {
					Imono.ConvertFrom( I, PIXEL_FORMAT.R8 );
					m_tex_BlueNoise = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Imono }} ), COMPONENT_FORMAT.UNORM );
				}

			using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/heights.png" ) ) )
				using ( ImageFile Imono = new ImageFile() ) {
					Imono.ConvertFrom( I, PIXEL_FORMAT.R8 );
					m_tex_texDebugHeights = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Imono }} ), COMPONENT_FORMAT.UNORM );
				}

			using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/normals.png" ) ) ) {
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

			#if SCENE_CORNELL
				// Tomettes Floor
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/tomettes_basecolor.png" ) ) )
					using ( ImageFile Isecure = new ImageFile() ) {
						Isecure.ConvertFrom( I, PIXEL_FORMAT.BGRA8 );
						m_tex_tomettesAlbedo = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Isecure }} ), COMPONENT_FORMAT.UNORM );
					}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/tomettes_normal.png" ) ) ) {
					float4[]				scanline = new float4[I.Width];
					Renderer.PixelsBuffer	sourceNormalMap = new  Renderer.PixelsBuffer( I.Width*I.Height*4 );
					using ( System.IO.BinaryWriter Wr = sourceNormalMap.OpenStreamWrite() )
						for ( int Y=0; Y < I.Height; Y++ ) {
							I.ReadScanline( (uint) Y, scanline );
							for ( int X=0; X < I.Width; X++ ) {
								Wr.Write( (sbyte) Mathf.Clamp( 256.0f * scanline[X].x - 128.0f, -128, 127 ) );
								Wr.Write( (sbyte) Mathf.Clamp( 256.0f * scanline[X].y - 128.0f, -128, 127 ) );
								Wr.Write( (sbyte) Mathf.Clamp( 256.0f * scanline[X].z - 128.0f, -128, 127 ) );
								Wr.Write( (byte) 255 );
							}
						}

					m_tex_tomettesNormal = new Renderer.Texture2D( m_device, I.Width, I.Height, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.SNORM, false, false, new Renderer.PixelsBuffer[] { sourceNormalMap } );
				}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/tomettes_roughness.png" ) ) )
					using ( ImageFile Imono = new ImageFile() ) {
						Imono.ConvertFrom( I, PIXEL_FORMAT.R8 );
						m_tex_tomettesRoughness = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Imono }} ), COMPONENT_FORMAT.UNORM );
					}

				// Concrete Walls
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/concrete_diffuse.png" ) ) )
					using ( ImageFile Isecure = new ImageFile() ) {
						Isecure.ConvertFrom( I, PIXEL_FORMAT.BGRA8 );
						m_tex_concreteAlbedo = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Isecure }} ), COMPONENT_FORMAT.UNORM );
					}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/concrete_normal.png" ) ) ) {
					float4[]				scanline = new float4[I.Width];
					Renderer.PixelsBuffer	sourceNormalMap = new  Renderer.PixelsBuffer( I.Width*I.Height*4 );
					using ( System.IO.BinaryWriter Wr = sourceNormalMap.OpenStreamWrite() )
						for ( int Y=0; Y < I.Height; Y++ ) {
							I.ReadScanline( (uint) Y, scanline );
							for ( int X=0; X < I.Width; X++ ) {
								Wr.Write( (sbyte) Mathf.Clamp( 256.0f * scanline[X].x - 128.0f, -128, 127 ) );
								Wr.Write( (sbyte) Mathf.Clamp( 256.0f * scanline[X].y - 128.0f, -128, 127 ) );
								Wr.Write( (sbyte) Mathf.Clamp( 256.0f * scanline[X].z - 128.0f, -128, 127 ) );
								Wr.Write( (byte) 255 );
							}
						}

					m_tex_concreteNormal = new Renderer.Texture2D( m_device, I.Width, I.Height, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.SNORM, false, false, new Renderer.PixelsBuffer[] { sourceNormalMap } );
				}
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Textures/concrete_roughness.png" ) ) )
					using ( ImageFile Imono = new ImageFile() ) {
						Imono.ConvertFrom( I, PIXEL_FORMAT.R8 );
						m_tex_concreteRoughness = new Texture2D( m_device, new ImagesMatrix( new ImageFile[,] {{ Imono }} ), COMPONENT_FORMAT.UNORM );
					}
			#endif

			// Setup camera
			// BEWARE: Must correspond to TAN_HALF_FOV declared in global.hlsl!
			m_camera.CreatePerspectiveCamera( 2.0f * Mathf.Atan( 0.6f ), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_manipulator.Attach( panelOutput, m_camera );
//			m_manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );
			#if SCENE_LIBRARY
				m_manipulator.InitializeCamera( new float3( 0, 5.0f, -4.5f ), new float3( 0, 0, 0 ), float3.UnitY );	// Library
			#elif SCENE_CORNELL
				m_manipulator.InitializeCamera( new float3( 0, -0.7f, -6.0f ), new float3( 0, -0.7f, 0 ), float3.UnitY );
				floatTrackbarControlEnvironmentIntensity.Value = 0.25f;
			#elif SCENE_HEIGHTFIELD
				m_manipulator.InitializeCamera( new float3( 0, 3.0f, 0.01f ), new float3( 0, 0, 0 ), float3.UnitY );
			#endif
			m_manipulator.EnableMouseAction += m_manipulator_EnableMouseAction;

			// Setup environment SH coefficients
			InitEnvironmentSH();
			UpdateSH();

			// Create software computer for debugging purposes
			m_softwareHBILComputer = new ComputeHBIL( m_device );
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
			m_startTime = GetGameTime();
			m_lastDisplayTime = m_startTime;

			m_camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			Camera_CameraTransformChanged( m_camera, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_device == null )
				return;

			m_primCylinder.Dispose();
			m_softwareHBILComputer.Dispose();

			m_tex_finalRender.Dispose();

			m_tex_sourceRadiance_PULL.Dispose();
			m_tex_sourceRadiance_PUSH.Dispose();
			m_tex_radiance.Dispose();
			m_tex_bentCone.Dispose();

			#if SCENE_CORNELL
				m_tex_tomettesRoughness.Dispose();
				m_tex_tomettesNormal.Dispose();
				m_tex_tomettesAlbedo.Dispose();

				m_tex_concreteAlbedo.Dispose();
				m_tex_concreteNormal.Dispose();
				m_tex_concreteRoughness.Dispose();
			#endif

			m_tex_texDebugNormals.Dispose();
			m_tex_texDebugHeights.Dispose();
			m_tex_BlueNoise.Dispose();
			m_tex_shadow.Dispose();
			m_tex_depthWithMips.Dispose();
			m_tex_motionVectors.Dispose();
			m_tex_emissive.Dispose();
			m_tex_normal.Dispose();
			m_tex_albedo.Dispose();

			m_shader_RenderDebugCone.Dispose();
			m_shader_AddEmissive.Dispose();
			m_shader_PostProcess.Dispose();
			m_shader_ComputeHBIL.Dispose();
			m_shader_DownSampleDepth.Dispose();
			m_shader_ComputeLighting.Dispose();
			m_shader_RenderScene_Shadow.Dispose();
			m_shader_RenderScene_DepthGBufferPass.Dispose();
			m_shader_Pull.Dispose();
			m_shader_Push.Dispose();
			#if !BILATERAL_PUSH_PULL
				m_shader_Pull_LastPass.Dispose();
				m_shader_Push_FirstPass.Dispose();
			#endif
			m_shader_ReprojectRadiance.Dispose();

			m_CB_DebugCone.Dispose();
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

		float4x4	m_previousFrameCamera2World = float4x4.Identity;
		float		m_currentTime;

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null || checkBoxPause.Checked )
				return;

			m_device.PerfBeginFrame();

			// Setup global data
			float	newCurrentTime = GetGameTime() - m_startTime;
			float	deltaTime = newCurrentTime - m_currentTime;
			m_currentTime = newCurrentTime;

			if ( checkBoxAnimate.Checked ) {
				m_CB_Main.m._deltaTime = deltaTime;
				m_CB_Main.m._time = newCurrentTime;
			} else {
				m_CB_Main.m._deltaTime = 0.0f;
			}
			m_CB_Main.m._flags = 0U;
			m_CB_Main.m._flags |= checkBoxEnableHBIL.Checked ? 1U : 0;
			m_CB_Main.m._flags |= checkBoxEnableBentNormal.Checked ? 2U : 0;
			m_CB_Main.m._flags |= checkBoxEnableConeVisibility.Checked ? 4U : 0;
			m_CB_Main.m._flags |= checkBoxEnableBentNormalDirect.Checked ? 0x8U : 0;
			m_CB_Main.m._flags |= checkBoxEnableConeVisibilityDirect.Checked ? 0x10U : 0;
			m_CB_Main.m._flags |= checkBoxMonochrome.Checked ? 0x20U : 0;
			m_CB_Main.m._flags |= checkBoxForceAlbedo.Checked ? 0x40U : 0;
			m_CB_Main.m._flags |= radioButtonPULL.Checked ? 0x100U : 0;
			m_CB_Main.m._sourceRadianceIndex = 0;
			m_CB_Main.m._debugMipIndex = (uint) integerTrackbarControlDebugMip.Value;
			m_CB_Main.m._environmentIntensity = floatTrackbarControlEnvironmentIntensity.Value;
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

			// Finalize camera matrices
			{
				float4x4	previousCamera2World = m_previousFrameCamera2World;
				float4x4	currentCamera2World = m_CB_Camera.m._Camera2World;
// 				float3		currentX = currentCamera2World.r0.xyz;
// 				float3		currentY = currentCamera2World.r1.xyz;
// 				float3		currentZ = currentCamera2World.r2.xyz;
// 
// 				float3		deltaPosition = currentCamera2World.r3.xyz - previousCamera2World.r3.xyz;
// 				float3		prevCamera2CurrentCamera_Position = deltaPosition;
// 				float3		prevX = previousCamera2World.r0.xyz;
// 				float3		newX = new float3( prevX.Dot( currentX ) );

				if ( !checkBoxFreezePrev2CurrentCamMatrix.Checked ) {
					m_CB_Camera.m._PrevCamera2Camera = previousCamera2World * m_CB_Camera.m._World2Camera;
					m_CB_Camera.m._Camera2PrevCamera = m_CB_Camera.m._Camera2World * previousCamera2World.Inverse;
				}
				m_CB_Camera.UpdateData();
			}

			m_previousFrameCamera2World = m_CB_Camera.m._Camera2World;

			#if RENDER_IN_DEPTH_STENCIL
				Texture2D	targetDepthStencil = m_device.DefaultDepthStencil;
			#else
				Texture2D	targetDepthStencil = m_tex_depthWithMips;
			#endif

			//////////////////////////////////////////////////////////////////////////
			// =========== Reproject Last Frame Radiance ===========
			//
			m_device.PerfSetMarker( 0 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_ReprojectRadiance.Use() ) {
				m_device.Clear( m_tex_sourceRadiance_PUSH, float4.Zero );

				m_tex_radiance.GetView( 0, 1, m_radianceSourceSliceIndex, 1 ).SetCS( 0 );	// Source radiance from last frame
				targetDepthStencil.SetCS( 1 );												// Source depth buffer from last frame
				m_tex_motionVectors.SetCS( 2 );												// Motion vectors for dynamic objects
				m_tex_BlueNoise.SetCS( 3 );
				m_tex_sourceRadiance_PUSH.SetCSUAV( 0 );

				m_shader_ReprojectRadiance.Dispatch( m_tex_sourceRadiance_PUSH.Width >> 4, m_tex_sourceRadiance_PUSH.Height >> 4, 1 );

				m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlotUAV();
				m_tex_motionVectors.RemoveFromLastAssignedSlots();
				m_tex_radiance.RemoveFromLastAssignedSlots();
				targetDepthStencil.RemoveFromLastAssignedSlots();
			}

//*			//////////////////////////////////////////////////////////////////////////
			// =========== Apply Push-Pull to Reconstruct Missing Pixels ===========
			//
			m_device.PerfSetMarker( 1 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			{
				const uint	MAX_MIP = 11;

				m_CB_PushPull.m._bilateralDepths.Set( floatTrackbarControlBilateralDepthDeltaMin.Value, floatTrackbarControlBilateralDepthDeltaMax.Value );
				m_CB_PushPull.m._bilateralDepths.x *= m_CB_PushPull.m._bilateralDepths.x;
				m_CB_PushPull.m._bilateralDepths.y *= m_CB_PushPull.m._bilateralDepths.y;
				m_CB_PushPull.m._preferedDepth = floatTrackbarControlHarmonicPreferedDepth.Value;

				#if !BILATERAL_PUSH_PULL
					ComputeShader	currentCS = m_shader_Push_FirstPass;
				#else
					ComputeShader	currentCS = m_shader_Push;
				#endif
				if ( currentCS.Use() ) {
					for ( uint mipLevel=1; mipLevel < MAX_MIP; mipLevel++ ) {
						View2D	targetView = null;
						if ( mipLevel == MAX_MIP-1 )
							targetView = m_tex_sourceRadiance_PULL.GetView( mipLevel, 1, 0, 1 );		// Write last pass to last mip of PULL texture!
						else
							targetView = m_tex_sourceRadiance_PUSH.GetView( mipLevel, 1, 0, 1 );
						targetView.SetCSUAV( 0 );
						m_tex_sourceRadiance_PUSH.GetView( mipLevel-1, 1, 0, 1 ).SetCS( 0 );

						m_CB_PushPull.m._sizeX = targetView.Width;
						m_CB_PushPull.m._sizeY = targetView.Height;
						m_CB_PushPull.UpdateData();

						currentCS.Dispatch( (m_CB_PushPull.m._sizeX+15) >> 4, (m_CB_PushPull.m._sizeY+15) >> 4, 1 );

						#if !BILATERAL_PUSH_PULL
							if ( currentCS == m_shader_Push_FirstPass ) {
								// Use regular push shader for other passes
								currentCS = m_shader_Push;
								currentCS.Use();
							}
						#endif
					}
				}

				// Start pulling phase
				currentCS = m_shader_Pull;
 				if ( currentCS.Use() ) {
					for ( uint mipLevel=MAX_MIP-1; mipLevel > 0; mipLevel-- ) {
						View2D	targetView = m_tex_sourceRadiance_PULL.GetView( mipLevel-1, 1, 0, 1 );	// Write to current mip of PULL texture
						targetView.SetCSUAV( 0 );
						m_tex_sourceRadiance_PULL.GetView( mipLevel, 1, 0, 1 ).SetCS( 0 );				// Read from previous mip of PULL texture
						m_tex_sourceRadiance_PUSH.GetView( mipLevel-1, 1, 0, 1 ).SetCS( 3 );			// Read from current mip of PUSH texture (because we can't read and write from a RWTexture other than UINT type!)

						m_CB_PushPull.m._sizeX = targetView.Width;
						m_CB_PushPull.m._sizeY = targetView.Height;
						m_CB_PushPull.UpdateData();

						currentCS.Dispatch( (m_CB_PushPull.m._sizeX+15) >> 4, (m_CB_PushPull.m._sizeY+15) >> 4, 1 );

						#if !BILATERAL_PUSH_PULL
							if ( mipLevel == 2 ) {
								// Use last pass shader
								currentCS = m_shader_Pull_LastPass;
								currentCS.Use();
							}
						#endif
					}

					m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlots();
					m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlots();
					m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlotUAV();
 				}
			}
//*/

			//////////////////////////////////////////////////////////////////////////
			// =========== Render Depth / G-Buffer Pass  ===========
			// We're actually doing all in one here...
			//
			m_device.PerfSetMarker( 2 );
			#if RENDER_IN_DEPTH_STENCIL
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.WRITE_ALWAYS, BLEND_STATE.DISABLED );
			#else
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
			#endif

			if ( m_shader_RenderScene_DepthGBufferPass.Use() ) {
				#if RENDER_IN_DEPTH_STENCIL
					m_device.SetRenderTargets( new IView[] { m_tex_albedo.GetView( 0, 1, 0, 1 ), m_tex_normal.GetView( 0, 1, 0, 1 ), m_tex_emissive.GetView( 0, 1, 0, 1 ), m_tex_motionVectors.GetView( 0, 1, 0, 1 ) }, targetDepthStencil );
				#else
					m_device.SetRenderTargets( new IView[] { m_tex_albedo.GetView( 0, 1, 0, 1 ), m_tex_normal.GetView( 0, 1, 0, 1 ), m_tex_emissive.GetView( 0, 1, 0, 1 ), m_tex_motionVectors.GetView( 0, 1, 0, 1 ), targetDepthStencil.GetView( 0, 1, 0, 1 ) }, null );
				#endif

				#if SCENE_HEIGHTFIELD
					m_tex_texDebugHeights.SetPS( 32 );
					m_tex_texDebugNormals.SetPS( 33 );
				#elif SCENE_CORNELL
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

			// Downsample depth-stencil
			m_device.PerfSetMarker( 3 );
			#if !RENDER_IN_DEPTH_STENCIL
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
			#endif

			//////////////////////////////////////////////////////////////////////////
			// =========== Add Emissive Map to Reprojected Radiance ===========
			// (this way it's used as indirect radiance source in this frame as well)
			//
			if ( m_shader_AddEmissive.Use() ) {

// WARNING: #TODO! Do reprojection + push/pull phase WITH added emissive radiance from this frame so the emissive is pulled down the mips as well!
// Unfortunately that means doing the radiance reprojection AFTER the G-buffer rendering

				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ADDITIVE );
				m_device.SetRenderTarget( m_tex_sourceRadiance_PULL, null );
				m_tex_emissive.SetPS( 0 );
				m_device.RenderFullscreenQuad( m_shader_AddEmissive );
			}

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute Shadow Map ===========
			m_device.PerfSetMarker( 4 );
			#if SCENE_CORNELL || SCENE_LIBRARY
				if ( m_shader_RenderScene_Shadow.Use() ) {
					m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.WRITE_ALWAYS, BLEND_STATE.DISABLED );

					m_tex_shadow.RemoveFromLastAssignedSlots();
					for ( uint faceIndex=0; faceIndex < 6; faceIndex++ ) {
						m_device.SetRenderTargets( null, m_tex_shadow.GetView( 0, 1, faceIndex, 1 ) );
						
						m_CB_Shadow.m._faceIndex = faceIndex;
						m_CB_Shadow.UpdateData();

						m_device.RenderFullscreenQuad( m_shader_RenderScene_Shadow );
					}
				}
			#endif

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute Bent Cone Map and Irradiance Bounces  ===========
			// 
			m_device.PerfSetMarker( 5 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			#if BRUTE_FORCE_HBIL
				if ( m_shader_ComputeHBIL.Use() ) {
					m_device.SetRenderTargets( new IView[] { m_tex_radiance.GetView( 0, 1, 1-m_radianceSourceSliceIndex, 1 ), m_tex_bentCone.GetView( 0, 1, 0, 1 ) }, null );

//					m_tex_radiance.GetView( 0, 1, m_radianceSourceSliceIndex, 1 ).SetPS( 0 );	// Reprojected source radiance from last frame
					m_tex_sourceRadiance_PULL.SetPS( 0 );	// Reprojected source radiance
					m_tex_normal.SetPS( 1 );
					targetDepthStencil.SetPS( 2 );
					m_tex_BlueNoise.SetPS( 3 );

m_tex_texDebugHeights.SetPS( 32 );
m_tex_texDebugNormals.SetPS( 33 );

					m_device.RenderFullscreenQuad( m_shader_ComputeHBIL );

					m_radianceSourceSliceIndex = 1-m_radianceSourceSliceIndex;
	
					m_tex_radiance.RemoveFromLastAssignedSlots();
				}
			#else
				if ( m_shader_ComputeHBIL.Use() ) {
					m_device.SetRenderTargets( new IView[] { m_tex_radiance.GetView( 0, 1, 1-m_radianceSourceSliceIndex, 1 ), m_tex_bentCone.GetView( 0, 1, 0, 1 ) }, null );

					m_CB_HBIL.m._gatherSphereMaxRadius_m = floatTrackbarControlGatherSphereRadius.Value;
					m_CB_HBIL.m._bilateralValues.Set( floatTrackbarControlBilateral0.Value, floatTrackbarControlBilateral1.Value, floatTrackbarControlBilateral2.Value, floatTrackbarControlBilateral3.Value );
					m_CB_HBIL.UpdateData();

					targetDepthStencil.SetPS( 0 );
					m_tex_normal.SetPS( 1 );
					m_tex_sourceRadiance_PULL.SetPS( 2 );	// Reprojected + reconstructed source radiance from last frame with all mips
					m_tex_BlueNoise.SetPS( 3 );

					m_device.RenderFullscreenQuad( m_shader_ComputeHBIL );

					m_radianceSourceSliceIndex = 1-m_radianceSourceSliceIndex;

					m_tex_radiance.RemoveFromLastAssignedSlots();
				}
			#endif


// DEBUG RGB10A2 and R11G11B10_FLOAT formats
// Texture2D	pipoCPU = new Texture2D( m_device, m_tex_radiance.Width, m_tex_radiance.Height, 2, 1, PIXEL_FORMAT.R11G11B10, COMPONENT_FORMAT.AUTO, true, false, null );
// pipoCPU.CopyFrom( m_tex_radiance );
// uint[,]	pixels = new uint[m_tex_radiance.Width,m_tex_radiance.Height];
// pipoCPU.ReadPixels( 0, m_radianceSourceSliceIndex, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
// 	pixels[_X,_Y] = _R.ReadUInt32();
// } );

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute lighting & finalize radiance  ===========
			// 
			m_device.PerfSetMarker( 6 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_ComputeLighting.Use() ) {
				m_device.SetRenderTargets( new IView[] { m_tex_radiance.GetView( 0, 1, 1-m_radianceSourceSliceIndex, 1 ), m_tex_finalRender.GetView( 0, 1, 0, 1 ) }, null );

				m_tex_albedo.SetPS( 0 );
				m_tex_normal.SetPS( 1 );
				m_tex_emissive.SetPS( 2 );
				targetDepthStencil.SetPS( 3 );

				m_tex_shadow.SetPS( 6 );

				m_tex_radiance.GetView( 0, 1, m_radianceSourceSliceIndex, 1 ).SetPS( 8 );
				m_tex_bentCone.SetPS( 9 );

				m_device.RenderFullscreenQuad( m_shader_ComputeLighting );

				m_radianceSourceSliceIndex = 1-m_radianceSourceSliceIndex;

				m_tex_radiance.RemoveFromLastAssignedSlots();
			}
//*/
			//////////////////////////////////////////////////////////////////////////
			// =========== Post-Process ===========
			m_device.PerfSetMarker( 7 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_PostProcess.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_motionVectors.SetPS( 4 );

				m_tex_radiance.SetPS( 8 );
				m_tex_finalRender.SetPS( 10 );
				m_tex_sourceRadiance_PUSH.SetPS( 11 );
				m_tex_sourceRadiance_PULL.SetPS( 12 );

				m_device.RenderFullscreenQuad( m_shader_PostProcess );

				targetDepthStencil.RemoveFromLastAssignedSlots();
				m_tex_emissive.RemoveFromLastAssignedSlots();
				m_tex_motionVectors.RemoveFromLastAssignedSlots();
				m_tex_normal.RemoveFromLastAssignedSlots();
				m_tex_albedo.RemoveFromLastAssignedSlots();

				m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlots();
				m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlots();
				m_tex_radiance.RemoveFromLastAssignedSlots();
				m_tex_bentCone.RemoveFromLastAssignedSlots();
				m_tex_finalRender.RemoveFromLastAssignedSlots();
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
			// Update information box
			if ( m_currentTime - m_lastDisplayTime < 1.0f )
				return;
			m_lastDisplayTime = m_currentTime;

			double	timeReprojection = m_device.PerfGetMilliSeconds( 0 );
			double	timePushPull = m_device.PerfGetMilliSeconds( 1 );
			double	timeRenderGBuffer = m_device.PerfGetMilliSeconds( 2 );
			double	timeDownSampleDepth = m_device.PerfGetMilliSeconds( 3 );
			double	timeShadow = m_device.PerfGetMilliSeconds( 4 );
			double	timeHBIL = m_device.PerfGetMilliSeconds( 5 );
			double	timeComputeLighting = m_device.PerfGetMilliSeconds( 6 );
			double	timePostProcess = m_device.PerfGetMilliSeconds( 7 );

			float	totalTime = m_currentTime - m_startTime;
			textBoxInfo.Text = "" //"Total Time = " + totalTime + " seconds\r\n"
							 + "Average frame time = " + (1000.0f * totalTime / m_framesCount).ToString( "G6" ) + " ms\r\n"
							 + "\r\n"
							 + "Reprojection: " + timeReprojection.ToString( "G4" ) + " ms\r\n"
							 + "Push-Pull: " + timePushPull.ToString( "G4" ) + " ms\r\n"
							 + "G-Buffer Rendering: " + timeRenderGBuffer.ToString( "G4" ) + " ms\r\n"
							 + "Shadow Map: " + timeShadow.ToString( "G4" ) + " ms\r\n"
							 + "DownSample Depth: " + timeDownSampleDepth.ToString( "G4" ) + " ms\r\n"
							 + "HBIL: " + timeHBIL.ToString( "G4" ) + " ms\r\n"
							 + "Lighting: " + timeComputeLighting.ToString( "G4" ) + " ms\r\n"
							 + "Post-Processing: " + timePostProcess.ToString( "G4" ) + " ms\r\n"
							 + "Total frame time = " + totalFrameTime.ToString( "G6" ) + " ms\r\n"
							 + "\r\n"
							 ;
		}

		public float	GetGameTime() {
			long	ticks = m_stopWatch.ElapsedTicks;
			float	time = (float) (ticks * m_ticks2Seconds);
			return time;
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			if ( m_device != null )
				m_device.ReloadModifiedShaders();
		}

		private void buttonClear_Click( object sender, EventArgs e ) {
			m_device.Clear( m_tex_radiance, float4.Zero );
		}

		#region Camera & Environment Light Manipulation

		bool m_manipulator_EnableMouseAction( MouseEventArgs _e ) {
			return (Control.ModifierKeys & Keys.Alt) == 0;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;
		}

		Quat			m_lightQuat = new Quat( new AngleAxis( 0.0f, float3.UnitZ ) );
		float3x3		m_lightRotation = float3x3.Identity;
// 		float3x3		m_lightRotation = (float3x3) m_lightQuat;

		float3[]		m_lightSH = new float3[9];
		double[,]		m_SHRotation = new double[9,9];
		float3[]		m_rotatedLightSH = new float3[9];

		MouseButtons	m_buttonDown = MouseButtons.None;
		Point			m_buttonDownPosition;
		Quat			m_buttonDownLightQuat;

		private void panelOutput_MouseDown( object sender, MouseEventArgs e ) {
			if ( Control.ModifierKeys == Keys.Control )
				DebugHBIL( (uint) e.X, (uint) e.Y );

			if ( m_manipulator_EnableMouseAction( e ) )
				return;	// Don't do anything if camera manipulator is enabled

			m_buttonDownPosition = e.Location;
			m_buttonDownLightQuat = new Quat( m_lightQuat );
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
				Quat	rotY = new Quat( new AngleAxis( 0.01f * Dx, float3.UnitY ) );
				Quat	rotX = new Quat( new AngleAxis( 0.01f * Dy, float3.UnitX ) );
				m_lightQuat = m_buttonDownLightQuat * rotX * rotY;
				m_lightRotation = (float3x3) m_lightQuat;
				UpdateSH();
			}
		}

		void	UpdateSH() {
			SphericalHarmonics.SHFunctions.BuildRotationMatrix( m_lightRotation, m_SHRotation, 3 );
			SphericalHarmonics.SHFunctions.Rotate( m_lightSH, m_SHRotation, m_rotatedLightSH, 3 );
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
		}

		private void checkBoxPause_CheckedChanged( object sender, EventArgs e ) {
			if ( checkBoxPause.Checked )
				m_stopWatch.Stop();
			else
				m_stopWatch.Start();
		}

		private void TestHBILForm_KeyDown( object sender, KeyEventArgs e ) {
			if ( e.KeyCode == Keys.A )
				checkBoxFreezePrev2CurrentCamMatrix.Checked ^= true;
		}

		private void	DebugHBIL( uint _X, uint _Y ) {
			m_softwareHBILComputer.Setup( m_tex_depthWithMips, m_tex_normal, m_tex_sourceRadiance_PULL );
			m_softwareHBILComputer.GatherSphereMaxRadius_m = floatTrackbarControlGatherSphereRadius.Value;
			m_softwareHBILComputer.Camera2World = m_CB_Camera.m._Camera2World;
			m_softwareHBILComputer.World2Proj = m_CB_Camera.m._World2Proj;
			m_softwareHBILComputer.Compute( _X, _Y );
		}

		#endregion
	}
}
