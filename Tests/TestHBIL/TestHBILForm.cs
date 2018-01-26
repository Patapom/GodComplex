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
			public float2		iResolution;
			public float		iGlobalTime;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Light {
			public float3		_AreaLightX;
			public float		_AreaLightScaleX;
			public float3		_AreaLightY;
			public float		_AreaLightScaleY;
			public float3		_AreaLightZ;
			public float		_AreaLightDiffusion;
			public float3		_AreaLightT;
			public float		_AreaLightIntensity;
			public float4		_AreaLightTexDimensions;	// XY=Texture size, ZW=1/XY
			public float3		_ProjectionDirectionDiff;	// Closer to portal when diffusion increases
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_ShadowMap {
			public float2		_ShadowOffsetXY;			// XY offset in local light space where to place the shadow
			public float2		_ShadowZFar;				// X=Far clip distance for the shadow, Y=1/X
			public float		_InvShadowMapSize;			// 1/Size of the shadow map
			public float		_KernelSize;				// Size of the filtering kernel
			public float2		_HardeningFactor;			// Hardening factor for the sigmoïd
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Object {
			public float4x4		_Local2World;
			public float4x4		_World2Local;
			public float3		_DiffuseAlbedo;
			public float		_Gloss;
			public float3		_SpecularTint;
			public float		_Metal;
			public UInt32		_UseTexture;
			public UInt32		_FalseColors;
			public float		_FalseColorsMaxRange;
		}

		#endregion

		#region FIELDS

		private Device		m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
// 		private ConstantBuffer<CB_Light>		m_CB_Light = null;
// 		private ConstantBuffer<CB_ShadowMap>	m_CB_ShadowMap = null;
// 		private ConstantBuffer<CB_Object>		m_CB_Object = null;

//		private Shader		m_shader_RenderShadowMap = null;
//		private Shader		m_shader_RenderAreaLight = null;
		private Shader		m_shader_RenderScene_DepthPass = null;

		private Texture2D	m_tex_ShadowMap = null;
		private Texture2D	m_tex_motionVectors = null;

		private Texture2D	m_tex_BlueNoise = null;

		private Camera				m_camera = new Camera();
		private CameraManipulator	m_manipulator = new CameraManipulator();

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_stopWatch = new System.Diagnostics.Stopwatch();
		private double						m_ticks2Seconds;
		public float						m_startTime = 0;
		public float						m_currentTime = 0;
		public float						m_deltaTime = 0;		// Delta time used for the current frame

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

		#region Primitives

/*
		private void	BuildPrimitives() {
			{
				VertexPt4[]	Vertices = new VertexPt4[4];
				Vertices[0] = new VertexPt4() { Pt = new float4( -1, +1, 0, 1 ) };	// Top-Left
				Vertices[1] = new VertexPt4() { Pt = new float4( -1, -1, 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexPt4() { Pt = new float4( +1, +1, 0, 1 ) };	// Top-Right
				Vertices[3] = new VertexPt4() { Pt = new float4( +1, -1, 0, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexPt4.FromArray( Vertices );

				m_Prim_Quad = new Primitive( m_Device, (uint) Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
			}

			{
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[4];
				Vertices[0] = new VertexP3N3G3B3T2() { P = new float3( -1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 0 ) };	// Top-Left
				Vertices[1] = new VertexP3N3G3B3T2() { P = new float3( -1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexP3N3G3B3T2() { P = new float3( +1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 0 ) };	// Top-Right
				Vertices[3] = new VertexP3N3G3B3T2() { P = new float3( +1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Rectangle = new Primitive( m_Device, (uint) Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
			}

			{	// Build the sphere
				const int	W = 41;
				const int	H = 22;
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[W*H];
				for ( int Y=0; Y < H; Y++ ) {
					double	Theta = Math.PI * Y / (H-1);
					float	CosTheta = (float) Math.Cos( Theta );
					float	SinTheta = (float) Math.Sin( Theta );
					for ( int X=0; X < W; X++ ) {
						double	Phi = 2.0 * Math.PI * X / (W-1);
						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float3	N = new float3( SinTheta * SinPhi, CosTheta, SinTheta * CosPhi );
						float3	T = new float3( CosPhi, 0.0f, -SinPhi );
						float3	B = N.Cross( T );

						Vertices[W*Y+X] = new VertexP3N3G3B3T2() {
							P = N,
							N = N,
							T = T,
							B = B,
							UV = new float2( 2.0f * X / W, 1.0f * Y / H )
						};
					}
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				uint[]		Indices = new uint[(H-1) * (2*W+2)-2];
				int			IndexCount = 0;
				for ( int Y=0; Y < H-1; Y++ ) {
					for ( int X=0; X < W; X++ ) {
						Indices[IndexCount++] = (uint) ((Y+0) * W + X);
						Indices[IndexCount++] = (uint) ((Y+1) * W + X);
					}
					if ( Y < H-2 ) {
						Indices[IndexCount++] = (uint) ((Y+1) * W - 1);
						Indices[IndexCount++] = (uint) ((Y+1) * W + 0);
					}
				}

				m_Prim_Sphere = new Primitive( m_Device, (uint) Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
			}

			{	// Build the cube
				float3[]	Normals = new float3[6] {
					-float3.UnitX,
					float3.UnitX,
					-float3.UnitY,
					float3.UnitY,
					-float3.UnitZ,
					float3.UnitZ,
				};

				float3[]	Tangents = new float3[6] {
					float3.UnitZ,
					-float3.UnitZ,
					float3.UnitX,
					-float3.UnitX,
					-float3.UnitX,
					float3.UnitX,
				};

				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[6*4];
				uint[]		Indices = new uint[2*6*3];

				for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ ) {
					float3	N = Normals[FaceIndex];
					float3	T = Tangents[FaceIndex];
					float3	B = N.Cross( T );

					Vertices[4*FaceIndex+0] = new VertexP3N3G3B3T2() {
						P = N - T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 0 )
					};
					Vertices[4*FaceIndex+1] = new VertexP3N3G3B3T2() {
						P = N - T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 1 )
					};
					Vertices[4*FaceIndex+2] = new VertexP3N3G3B3T2() {
						P = N + T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 1 )
					};
					Vertices[4*FaceIndex+3] = new VertexP3N3G3B3T2() {
						P = N + T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 0 )
					};

					Indices[2*3*FaceIndex+0] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+1] = (uint) (4*FaceIndex+1);
					Indices[2*3*FaceIndex+2] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+3] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+4] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+5] = (uint) (4*FaceIndex+3);
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Cube = new Primitive( m_Device, (uint) Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
			}
		}
*/

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

//			BuildPrimitives();

			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
			m_CB_Main.m.iResolution.Set( W, H );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_device, 1 );
//			m_CB_Light = new ConstantBuffer<CB_Light>( m_device, 2 );
//			m_CB_ShadowMap = new ConstantBuffer<CB_ShadowMap>( m_Device, 3 );
//			m_CB_Object = new ConstantBuffer<CB_Object>( m_Device, 4 );

// 			try {
// 				m_shader_RenderAreaLight = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderAreaLight.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
// 			} catch ( Exception _e ) {
// 				MessageBox.Show( "Shader \"RenderAreaLight\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
// 				m_shader_RenderAreaLight = null;
// 			}
// 
// 			try {
// 				m_shader_RenderShadowMap = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderShadowMap.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
// 			} catch ( Exception _e ) {
// 				MessageBox.Show( "Shader \"RenderShadow\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
// 				m_shader_RenderShadowMap = null;
// 			}

			try {
				m_shader_RenderScene_DepthPass = new Shader( m_device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"RenderScene\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_shader_RenderScene_DepthPass = null;
			}

			// Create buffers
			m_tex_motionVectors = new Texture2D( m_device, W, H, 1, 1, PIXEL_FORMAT.RG8, COMPONENT_FORMAT.SNORM, false, false, null );

			// Create textures
			using ( ImageFile I = new ImageFile( new FileInfo( "blueNoise64.png" ) ) )
			m_tex_BlueNoise = Image2Texture( )

			// Setup camera
			m_camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_manipulator.Attach( panelOutput, m_camera );
			m_manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
			m_ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_stopWatch.Start();
			m_startTime = GetGameTime();

			m_camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			Camera_CameraTransformChanged( m_camera, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_device == null )
				return;

// 			if ( m_shader_RenderShadowMap != null ) {
// 				m_shader_RenderShadowMap.Dispose();
// 			}
// 			if ( m_shader_RenderAreaLight != null ) {
// 				m_shader_RenderAreaLight.Dispose();
// 			}
			if ( m_shader_RenderScene_DepthPass != null ) {
				m_shader_RenderScene_DepthPass.Dispose();
			}

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
//			m_CB_Light.Dispose();
//			m_CB_ShadowMap.Dispose();
//			m_CB_Object.Dispose();

// 			m_Prim_Quad.Dispose();
// 			m_Prim_Rectangle.Dispose();
// 			m_Prim_Sphere.Dispose();
// 			m_Prim_Cube.Dispose();

			m_tex_ShadowMap.Dispose();

			m_device.Exit();

			base.OnFormClosed( e );
		}

		#endregion

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_stopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_ticks2Seconds);
			return Time;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			// Setup global data
			m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			if ( checkBoxAnimate.Checked )
				m_CB_Main.m.iGlobalTime = GetGameTime() - m_startTime;
			m_CB_Main.UpdateData();

/*			//////////////////////////////////////////////////////////////////////////
			// =========== Render shadow map ===========
//			float	KernelSize = 16.0f * floatTrackbarControlProjectionDiffusion.Value;
			float	KernelSize = floatTrackbarControlKernelSize.Value;

//			float	ShadowZFar = (float) Math.Sqrt( 2.0 ) * m_Camera.Far;
			float	ShadowZFar = 10.0f;
			m_CB_ShadowMap.m._ShadowOffsetXY = (float2) Direction;
			m_CB_ShadowMap.m._ShadowZFar = new float2( ShadowZFar, 1.0f / ShadowZFar );
			m_CB_ShadowMap.m._KernelSize = KernelSize;
			m_CB_ShadowMap.m._InvShadowMapSize = 1.0f / m_tex_ShadowMap.Width;
			m_CB_ShadowMap.m._HardeningFactor = new float2( floatTrackbarControlHardeningFactor.Value, floatTrackbarControlHardeningFactor2.Value );
			m_CB_ShadowMap.UpdateData();

			if ( m_shader_RenderShadowMap != null && m_shader_RenderShadowMap.Use() ) {
				m_tex_ShadowMap.RemoveFromLastAssignedSlots();

				m_Device.SetRenderTargets( null, m_tex_ShadowMap );
				RenderScene( m_shader_RenderShadowMap );

				m_Device.RemoveRenderTargets();
				m_tex_ShadowMap.SetPS( 2 );
			}
//*/

			//////////////////////////////////////////////////////////////////////////
			// =========== Render Depth Pass ===========
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_RenderScene_DepthPass.Use() ) {
				m_device.SetRenderTarget( m_tex_motionVectors, m_device.DefaultDepthStencil );
				m_device.RenderFullscreenQuad( m_shader_RenderScene_DepthPass );
			}

			// =========== Render scene ===========
			m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

// 			m_device.Clear( m_device.DefaultTarget, float4.Zero );
// 			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

// 			// Depth pass
// 			if ( m_shader_RenderScene_DepthPass != null && m_shader_RenderScene_DepthPass.Use() ) {
// 				m_device. m_shader_RenderScene.;
// 			} else {
// 				m_device.Clear( new float4( 1, 1, 0, 0 ) );
// 			}


			// Show!
			m_device.Present( false );


			// Update window text
//			Text = "Test HBIL Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			if ( m_device != null )
				m_device.ReloadModifiedShaders();
		}

		#endregion
	}
}
