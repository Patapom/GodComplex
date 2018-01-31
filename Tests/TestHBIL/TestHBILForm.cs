//////////////////////////////////////////////////////////////////////////
// Horizon-Based Indirect Lighting Demo
//////////////////////////////////////////////////////////////////////////
//
// IDEAS / #TODOS:
//	✓
//	• Use push/pull (with bilateral) to fill in reprojected radiance voids!!!
//	• float	GetBilateralWeight( Z0, Z1, radius, ref sqHypotenuse ) => Outside of unit sphere???
//	• Use radius² as progression + sample mips for larger footprint (only if mip is bilateral filtered!)
//	• Keep previous radiance in case we reject height sample but accept radiance, and don't want to interpolate foreground radiance? Will that even occur?
//	• Write interleaved sampling + reconstruction based on bilateral weight (store it some place? Like alpha somewhere?)
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
		}

		#endregion

		#region FIELDS

		private Device				m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_SH>			m_CB_SH = null;
		private ConstantBuffer<CB_PushPull>		m_CB_PushPull = null;

		private Shader				m_shader_RenderScene_DepthGBufferPass = null;
		private ComputeShader		m_shader_ReprojectRadiance = null;
		private ComputeShader		m_shader_Push_FirstPass = null;
		private ComputeShader		m_shader_Push = null;
		private ComputeShader		m_shader_Pull = null;
		private ComputeShader		m_shader_Pull_LastPass = null;
		private Shader				m_shader_ComputeHBIL = null;
		private Shader				m_shader_ComputeLighting = null;
		private Shader				m_shader_PostProcess = null;

		// G-Buffer
		private Texture2D			m_tex_albedo = null;
		private Texture2D			m_tex_normal = null;
		private Texture2D			m_tex_motionVectors = null;

		// HBIL Results
		private Texture2D			m_tex_bentCone = null;
		private Texture2D			m_tex_radiance = null;
		private Texture2D			m_tex_sourceRadiance_PUSH = null;
		private Texture2D			m_tex_sourceRadiance_PULL = null;
		private Texture2D			m_tex_finalRender = null;
		private uint				m_radianceSourceSliceIndex = 0;

		private Texture2D			m_tex_BlueNoise = null;

		// Dummy textures with pre-computed heights and normals used to debug the computation
		private Texture2D			m_tex_texDebugHeights = null;
		private Texture2D			m_tex_texDebugNormals = null;

		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

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

			try {
				m_shader_RenderScene_DepthGBufferPass = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_Depth", null );
 //				m_shader_ReprojectRadiance = new Shader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS_Reproject", null );
 				m_shader_ReprojectRadiance = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Reproject", null );
 				m_shader_Push_FirstPass = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Push", new ShaderMacro[] { new ShaderMacro( "FIRST_PASS", "1" ) } );
 				m_shader_Push = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Push", null );
 				m_shader_Pull = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Pull", new ShaderMacro[] { new ShaderMacro( "LAST_PASS", "1" ) } );
				m_shader_Pull_LastPass = new ComputeShader( m_device, new System.IO.FileInfo( "Shaders/ComputeReprojection.hlsl" ), "CS_Pull", null );
 				m_shader_ComputeHBIL = new Shader( m_device, new System.IO.FileInfo( "Shaders/ComputeHBIL.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_ComputeLighting = new Shader( m_device, new System.IO.FileInfo( "Shaders/ComputeLighting.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_shader_PostProcess = new Shader( m_device, new System.IO.FileInfo( "Shaders/PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "HBIL Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			// Create buffers
			m_tex_albedo = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA8, COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_normal = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_motionVectors = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA16F, COMPONENT_FORMAT.AUTO, false, false, null );

			// Create HBIL buffers
			m_tex_bentCone = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_radiance = new Texture2D( m_device, W, H, 2, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_sourceRadiance_PUSH = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );
			m_tex_sourceRadiance_PULL = new Texture2D( m_device, W, H, 1, 0, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, true, null );

			m_tex_finalRender = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );

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
				Renderer.PixelsBuffer	SourceNormalMap = new  Renderer.PixelsBuffer( W*H*4*4 );
				using ( System.IO.BinaryWriter Wr = SourceNormalMap.OpenStreamWrite() )
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

				m_tex_texDebugNormals = new Renderer.Texture2D( m_device, I.Width, I.Height, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new Renderer.PixelsBuffer[] { SourceNormalMap } );
			}

			// Setup camera
			m_camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_manipulator.Attach( panelOutput, m_camera );
//			m_manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );
			m_manipulator.InitializeCamera( new float3( 0, 5.0f, -4.5f ), new float3( 0, 0, 0 ), float3.UnitY );
			m_manipulator.EnableMouseAction += m_manipulator_EnableMouseAction;

			// Setup environment SH coefficients
			InitEnvironmentSH();
			UpdateSH();

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

			m_tex_finalRender.Dispose();

			m_tex_sourceRadiance_PULL.Dispose();
			m_tex_sourceRadiance_PUSH.Dispose();
			m_tex_radiance.Dispose();
			m_tex_bentCone.Dispose();

			m_tex_texDebugNormals.Dispose();
			m_tex_texDebugHeights.Dispose();
			m_tex_BlueNoise.Dispose();
			m_tex_motionVectors.Dispose();
			m_tex_normal.Dispose();
			m_tex_albedo.Dispose();

			m_shader_PostProcess.Dispose();
			m_shader_ComputeLighting.Dispose();
			m_shader_ComputeHBIL.Dispose();
			m_shader_Pull_LastPass.Dispose();
			m_shader_Pull.Dispose();
			m_shader_Push.Dispose();
			m_shader_Push_FirstPass.Dispose();
			m_shader_ReprojectRadiance.Dispose();
			m_shader_RenderScene_DepthGBufferPass.Dispose();

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
			m_CB_Main.m._flags |= checkBoxMonochrome.Checked ? 8U : 0;
			m_CB_Main.m._flags |= checkBoxForceAlbedo.Checked ? 0x10U : 0;
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

				m_CB_Camera.m._PrevCamera2Camera = previousCamera2World * m_CB_Camera.m._World2Camera;
				m_CB_Camera.m._Camera2PrevCamera = m_CB_Camera.m._Camera2World * previousCamera2World.Inverse;
				m_CB_Camera.UpdateData();
			}

			m_previousFrameCamera2World = m_CB_Camera.m._Camera2World;

			//////////////////////////////////////////////////////////////////////////
			// =========== Reproject Last Frame Radiance ===========
			//
			m_device.PerfSetMarker( 0 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_ReprojectRadiance.Use() ) {
				m_device.Clear( m_tex_sourceRadiance_PUSH, float4.Zero );

				m_tex_radiance.GetView( 0, 1, m_radianceSourceSliceIndex, 1 ).SetCS( 0 );	// Source radiance from last frame
				m_device.DefaultDepthStencil.SetCS( 1 );									// Source depth buffer from last frame
				m_tex_motionVectors.SetCS( 2 );												// Motion vectors for dynamic objects
				m_tex_sourceRadiance_PUSH.SetCSUAV( 0 );

				m_shader_ReprojectRadiance.Dispatch( m_tex_sourceRadiance_PUSH.Width >> 4, m_tex_sourceRadiance_PUSH.Height >> 4, 1 );

				m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlotUAV();
				m_tex_motionVectors.RemoveFromLastAssignedSlots();
				m_tex_radiance.RemoveFromLastAssignedSlots();
				m_device.DefaultDepthStencil.RemoveFromLastAssignedSlots();
			}

			//////////////////////////////////////////////////////////////////////////
			// =========== Render Depth / G-Buffer Pass  ===========
			// We're actually doing all in one here...
			//
			m_device.PerfSetMarker( 1 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.WRITE_ALWAYS, BLEND_STATE.DISABLED );

			if ( m_shader_RenderScene_DepthGBufferPass.Use() ) {
				m_device.SetRenderTargets( new IView[] { m_tex_albedo.GetView( 0, 1, 0, 1 ), m_tex_normal.GetView( 0, 1, 0, 1 ), m_tex_motionVectors.GetView( 0, 1, 0, 1 ) }, m_device.DefaultDepthStencil );
				m_device.RenderFullscreenQuad( m_shader_RenderScene_DepthGBufferPass );
			}

//*			//////////////////////////////////////////////////////////////////////////
			// =========== Apply Push-Pull ===========
			//
			m_device.PerfSetMarker( 2 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			{
				const uint	MAX_MIP = 11;
				uint	mipLevel = 1;
// 				uint	W = m_tex_sourceRadiance.Width;
// 				uint	H = m_tex_sourceRadiance.Height;

				if ( m_shader_Push_FirstPass.Use() ) {
					ComputeShader	currentCS = m_shader_Push_FirstPass;
					for ( ; mipLevel < MAX_MIP; mipLevel++ ) {
//						W = (W+1) >> 1;
//						H = (H+1) >> 1;
//						W = Math.Max( 1, W >> 1 );
//						H = Math.Max( 1, H >> 1 );
						View2D	targetView = m_tex_sourceRadiance_PUSH.GetView( mipLevel, 1, 0, 1 );
						targetView.SetCSUAV( 0 );
						m_tex_sourceRadiance_PUSH.GetView( mipLevel-1, 1, 0, 1 ).SetCS( 0 );
// 						if ( targetView.Width != W || targetView.Height != H )
// 							throw new Exception( );

						m_CB_PushPull.m._sizeX = targetView.Width;
						m_CB_PushPull.m._sizeY = targetView.Height;
						m_CB_PushPull.UpdateData();

						currentCS.Dispatch( (m_CB_PushPull.m._sizeX+15) >> 4, (m_CB_PushPull.m._sizeY+15) >> 4, 1 );
						if ( currentCS == m_shader_Push_FirstPass ) {
							// Use regular push shader for other passes
							currentCS = m_shader_Push;
							currentCS.Use();
						}
					}
				}
 				if ( m_shader_Pull.Use() ) {
					ComputeShader	currentCS = m_shader_Pull;
					for (  mipLevel--; mipLevel > 0; mipLevel-- ) {
						View2D	targetView = m_tex_sourceRadiance_PULL.GetView( mipLevel-1, 1, 0, 1 );	// Write to current mip of PULL texture
						targetView.SetCSUAV( 0 );
						m_tex_sourceRadiance_PUSH.GetView( mipLevel, 1, 0, 1 ).SetCS( 0 );				// Read from previous mip of PULL texture
						m_tex_sourceRadiance_PUSH.GetView( mipLevel-1, 1, 0, 1 ).SetCS( 3 );			// Read from current mip of PUSH texture (because we can't read and write from a RWTexture other than UINT type!)

						m_CB_PushPull.m._sizeX = targetView.Width;
						m_CB_PushPull.m._sizeY = targetView.Height;
						m_CB_PushPull.UpdateData();

						currentCS.Dispatch( (m_CB_PushPull.m._sizeX+15) >> 4, (m_CB_PushPull.m._sizeY+15) >> 4, 1 );
						if ( mipLevel == 2 ) {
							// Use last pass shader
							currentCS = m_shader_Pull_LastPass;
							currentCS.Use();
						}
					}

					m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlots();
					m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlots();
					m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlotUAV();
 				}
			}
//*/

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute Bent Cone Map and Irradiance Bounces  ===========
			// 
			m_device.PerfSetMarker( 3 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_ComputeHBIL.Use() ) {
				m_device.SetRenderTargets( new IView[] { m_tex_radiance.GetView( 0, 1, 1-m_radianceSourceSliceIndex, 1 ), m_tex_bentCone.GetView( 0, 1, 0, 1 ) }, null );

//				m_tex_radiance.GetView( 0, 1, m_radianceSourceSliceIndex, 1 ).SetPS( 0 );	// Reprojected source radiance from last frame
				m_tex_sourceRadiance_PULL.SetPS( 0 );	// Reprojected source radiance
				m_tex_normal.SetPS( 1 );
				m_device.DefaultDepthStencil.SetPS( 2 );
				m_tex_BlueNoise.SetPS( 3 );

m_tex_texDebugHeights.SetPS( 32 );
m_tex_texDebugNormals.SetPS( 33 );

				m_device.RenderFullscreenQuad( m_shader_ComputeHBIL );

				m_radianceSourceSliceIndex = 1-m_radianceSourceSliceIndex;

				m_tex_radiance.RemoveFromLastAssignedSlots();
			}

			//////////////////////////////////////////////////////////////////////////
			// =========== Compute lighting & finalize radiance  ===========
			// 
			m_device.PerfSetMarker( 4 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_ComputeLighting.Use() ) {
				m_device.SetRenderTargets( new IView[] { m_tex_radiance.GetView( 0, 1, 1-m_radianceSourceSliceIndex, 1 ), m_tex_finalRender.GetView( 0, 1, 0, 1 ) }, null );

				m_tex_albedo.SetPS( 0 );
				m_tex_normal.SetPS( 1 );
				m_tex_motionVectors.SetPS( 2 );
				m_device.DefaultDepthStencil.SetPS( 3 );

				m_tex_radiance.GetView( 0, 1, m_radianceSourceSliceIndex, 1 ).SetPS( 8 );
				m_tex_bentCone.SetPS( 9 );

				m_device.RenderFullscreenQuad( m_shader_ComputeLighting );

				m_radianceSourceSliceIndex = 1-m_radianceSourceSliceIndex;

				m_tex_radiance.RemoveFromLastAssignedSlots();
			}
//*/
			//////////////////////////////////////////////////////////////////////////
			// =========== Post-Process ===========
			m_device.PerfSetMarker( 5 );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_PostProcess.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_radiance.SetPS( 8 );
				m_tex_finalRender.SetPS( 10 );
				m_tex_sourceRadiance_PUSH.SetPS( 11 );
				m_tex_sourceRadiance_PULL.SetPS( 12 );

				m_device.RenderFullscreenQuad( m_shader_PostProcess );

				m_device.DefaultDepthStencil.RemoveFromLastAssignedSlots();
				m_tex_motionVectors.RemoveFromLastAssignedSlots();
				m_tex_normal.RemoveFromLastAssignedSlots();
				m_tex_albedo.RemoveFromLastAssignedSlots();

				m_tex_sourceRadiance_PUSH.RemoveFromLastAssignedSlots();
				m_tex_sourceRadiance_PULL.RemoveFromLastAssignedSlots();
				m_tex_radiance.RemoveFromLastAssignedSlots();
				m_tex_bentCone.RemoveFromLastAssignedSlots();
				m_tex_finalRender.RemoveFromLastAssignedSlots();
			}

			// Show!
			m_device.Present( false );
			m_device.PerfEndFrame();
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
			double	timeRenderGBuffer = m_device.PerfGetMilliSeconds( 1 );
			double	timePushPull = m_device.PerfGetMilliSeconds( 2 );
			double	timeHBIL = m_device.PerfGetMilliSeconds( 3 );
			double	timeComputeLighting = m_device.PerfGetMilliSeconds( 4 );
			double	timePostProcess = m_device.PerfGetMilliSeconds( 5 );

			float	totalTime = m_currentTime - m_startTime;
			textBoxInfo.Text = "Total Time = " + totalTime + "s\r\n"
							 + "Average frame time = " + (1000.0f * totalTime / m_framesCount).ToString( "G6" ) + " ms\r\n"
							 + "\r\n"
							 + "Reprojection: " + timeReprojection.ToString( "G4" ) + " ms\r\n"
							 + "G-Buffer Rendering: " + timeRenderGBuffer.ToString( "G4" ) + " ms\r\n"
							 + "Push-Pull: " + timePushPull.ToString( "G4" ) + " ms\r\n"
							 + "HBIL: " + timeHBIL.ToString( "G4" ) + " ms\r\n"
							 + "Lighting: " + timeComputeLighting.ToString( "G4" ) + " ms\r\n"
							 + "Post-Processing: " + timePostProcess.ToString( "G4" ) + " ms\r\n"
							 + "\r\n"
							 ;

			// Update window text
//			Text = "Test HBIL Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
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

		#endregion
	}
}
