#define FILTER_EXP_SHADOW_MAP
#define USE_COMPUTE_SHADER_FOR_BRDF_INTEGRATION

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using Renderer;
using SharpMath;
using Nuaj.Cirrus.Utility;
using Nuaj.Cirrus;

namespace TestHBIL {
	public partial class TestHBILForm : Form {

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
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

		private Device		m_Device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Camera>		m_CB_Camera = null;
		private ConstantBuffer<CB_Light>		m_CB_Light = null;
		private ConstantBuffer<CB_ShadowMap>	m_CB_ShadowMap = null;
		private ConstantBuffer<CB_Object>		m_CB_Object = null;

//		private Shader		m_Shader_RenderShadowMap = null;
//		private Shader		m_Shader_RenderAreaLight = null;
		private Shader		m_Shader_RenderScene = null;

		private Texture2D	m_Tex_ShadowMap = null;


		private Camera				m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartTime = 0;
		public float						m_CurrentTime = 0;
		public float						m_DeltaTime = 0;		// Delta time used for the current frame

		#endregion

		#region METHODS

		public TestHBILForm() {
			InitializeComponent();
		}

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _fileName, ImageUtility.COMPONENT_FORMAT _componentFormat ) {
			ImageUtility.ImagesMatrix	images = null;
			if ( _fileName.Extension.ToLower() == ".dds" ) {
				images = ImageUtility.ImageFile.DDSLoadFile( _fileName );
			} else {
				ImageUtility.ImageFile	image = new ImageUtility.ImageFile( _fileName );
				if ( image.PixelFormat != ImageUtility.PIXEL_FORMAT.BGRA8 ) {
					ImageUtility.ImageFile	badImage = image;
					image = new ImageUtility.ImageFile();
					image.ConvertFrom( badImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
					badImage.Dispose();
				}
				images = new ImageUtility.ImagesMatrix( new ImageUtility.ImageFile[1,1] { { image } } );
			}
			return new Texture2D( m_Device, images, _componentFormat );
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

		#region Open/Close

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_Device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e ) {
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "HBIL Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

//			BuildPrimitives();

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Light = new ConstantBuffer<CB_Light>( m_Device, 2 );
//			m_CB_ShadowMap = new ConstantBuffer<CB_ShadowMap>( m_Device, 3 );
//			m_CB_Object = new ConstantBuffer<CB_Object>( m_Device, 4 );

// 			try {
// 				m_Shader_RenderAreaLight = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderAreaLight.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
// 			} catch ( Exception _e ) {
// 				MessageBox.Show( "Shader \"RenderAreaLight\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
// 				m_Shader_RenderAreaLight = null;
// 			}
// 
// 			try {
// 				m_Shader_RenderShadowMap = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderShadowMap.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
// 			} catch ( Exception _e ) {
// 				MessageBox.Show( "Shader \"RenderShadow\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
// 				m_Shader_RenderShadowMap = null;
// 			}

			try {
				m_Shader_RenderScene = new Shader( m_Device, new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			} catch ( Exception _e ) {
				MessageBox.Show( "Shader \"RenderScene\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderScene = null;
			}

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartTime = GetGameTime();

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnFormClosed( FormClosedEventArgs e ) {
			if ( m_Device == null )
				return;

// 			if ( m_Shader_RenderShadowMap != null ) {
// 				m_Shader_RenderShadowMap.Dispose();
// 			}
// 			if ( m_Shader_RenderAreaLight != null ) {
// 				m_Shader_RenderAreaLight.Dispose();
// 			}
			if ( m_Shader_RenderScene != null ) {
				m_Shader_RenderScene.Dispose();
			}

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Light.Dispose();
			m_CB_ShadowMap.Dispose();
			m_CB_Object.Dispose();

// 			m_Prim_Quad.Dispose();
// 			m_Prim_Rectangle.Dispose();
// 			m_Prim_Sphere.Dispose();
// 			m_Prim_Cube.Dispose();

			m_Tex_ShadowMap.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		#endregion

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime() {
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		void RenderScene( Shader _Shader ) {

			// Render a floor plane
			if ( _Shader == m_Shader_RenderScene ) {
				m_CB_Object.m._Local2World.BuildRotRightHanded( float3.Zero, float3.UnitY, float3.UnitX );
				m_CB_Object.m._Local2World.Scale( new float3( 16.0f, 16.0f, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 1, 1 );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.93f );
				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
				m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
				m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
				m_CB_Object.UpdateData();

				m_Prim_Rectangle.Render( _Shader );
			}

			// Render the sphere
			m_CB_Object.m._Local2World.BuildRotRightHanded( new float3( 0, 0.5f, 1.0f ), new float3( 0, 0.5f, 2 ), float3.UnitY );
//			m_CB_Object.m._Local2World.MakeLookAt( new float3( 0, 0.3f, 1.0f ), new float3( 0, 0.3f, 2 ), float3.UnitY );
			m_CB_Object.m._Local2World.Scale( new float3( 0.5f, 0.5f, 0.5f ) );
			m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
			m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 0.8f, 0.5f );
			m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.93f );
 			m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
 			m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
			m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
			m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
			m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
			m_CB_Object.UpdateData();

			m_Prim_Sphere.Render( _Shader );

			// Render the tiny cubes
			for ( int CubeIndex=0; CubeIndex < 4; CubeIndex++ ) {

				float	X = -1.0f + 2.0f * CubeIndex / 3;
//				float	Y = 0.1f + 1.0f * (float) Math.Abs( Math.Sin( m_CB_Main.m.iGlobalTime + CubeIndex ));
				float	Y = 0.1f + (float) Math.Max( 0.0, Math.Sin( m_CB_Main.m.iGlobalTime + CubeIndex ));

//				m_CB_Object.m._Local2World.MakeLookAt( new float3( 1.0f, 0.1f, 0.0f ), new float3( 1.0f, 0.1f, 1 ), float3.UnitY );
				m_CB_Object.m._Local2World.BuildRotRightHanded( new float3( X, Y, 0.0f ), new float3( X, Y, 1 ), float3.UnitY );
				m_CB_Object.m._Local2World.Scale( new float3( 0.1f, 0.1f, 0.1f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 1, 1 );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.92f );
 				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
 				m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
				m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
				m_CB_Object.UpdateData();

				m_Prim_Cube.Render( _Shader );
			}
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null )
				return;

			// Setup global data
			m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			if ( checkBoxAnimate.Checked )
				m_CB_Main.m.iGlobalTime = GetGameTime() - m_StartTime;
			m_CB_Main.UpdateData();

			// Setup area light buffer
			float		LighOffsetX = 1.2f;
			float		SizeX = floatTrackbarControlLightScaleX.Value;
			float		SizeY = 1.0f;
			float		RollAngle = (float) (Math.PI * floatTrackbarControlLightRoll.Value / 180.0);
			float3		LightPosition = new float3( LighOffsetX + floatTrackbarControlLightPosX.Value, 1.0f + floatTrackbarControlLightPosY.Value, -1.0f + floatTrackbarControlLightPosZ.Value );
			float3		LightTarget = new float3( LightPosition.x + floatTrackbarControlLightTargetX.Value, LightPosition.y + floatTrackbarControlLightTargetY.Value, LightPosition.z + 2.0f + floatTrackbarControlLightTargetZ.Value );
			float3		LightUp = new float3( (float) Math.Sin( -RollAngle ), (float) Math.Cos( RollAngle ), 0.0f );
			float4x4	AreaLight2World = new float4x4(); 
						AreaLight2World.BuildRotRightHanded( LightPosition, LightTarget, LightUp );

			float4x4	World2AreaLight = AreaLight2World.Inverse;

			double		Phi = Math.PI * floatTrackbarControlProjectionPhi.Value / 180.0;
			double		Theta = Math.PI * floatTrackbarControlProjectionTheta.Value / 180.0;
			float3		Direction = new float3( (float) (Math.Sin(Theta) * Math.Sin(Phi)), (float) (Math.Sin(Theta) * Math.Cos(Phi)), (float) Math.Cos( Theta ) );

			const float	DiffusionMin = 1e-2f;
			const float	DiffusionMax = 1000.0f;
//			float		Diffusion_Diffuse = DiffusionMin / (DiffusionMin / DiffusionMax + floatTrackbarControlProjectionDiffusion.Value);
			float		Diffusion_Diffuse = DiffusionMax + (DiffusionMin - DiffusionMax) * (float) Math.Pow( floatTrackbarControlProjectionDiffusion.Value, 0.01f );

//			float3		LocalDirection_Diffuse = (float3) (new float4( Diffusion_Diffuse * Direction, 0 ) * World2AreaLight);
			float3		LocalDirection_Diffuse = Diffusion_Diffuse * Direction;

			m_CB_Light.m._AreaLightX = (float3) AreaLight2World[0];
			m_CB_Light.m._AreaLightY = (float3) AreaLight2World[1];
			m_CB_Light.m._AreaLightZ = (float3) AreaLight2World[2];
			m_CB_Light.m._AreaLightT = (float3) AreaLight2World[3];
			m_CB_Light.m._AreaLightScaleX = SizeX;
			m_CB_Light.m._AreaLightScaleY = SizeY;
			m_CB_Light.m._AreaLightDiffusion = floatTrackbarControlProjectionDiffusion.Value;
			m_CB_Light.m._AreaLightIntensity = floatTrackbarControlLightIntensity.Value;
			m_CB_Light.m._AreaLightTexDimensions = new float4( m_Tex_AreaLight.Width, m_Tex_AreaLight.Height, 1.0f / m_Tex_AreaLight.Width, 1.0f / m_Tex_AreaLight.Height );
			m_CB_Light.m._ProjectionDirectionDiff = LocalDirection_Diffuse;
			m_CB_Light.UpdateData();


			// =========== Render shadow map ===========
//			float	KernelSize = 16.0f * floatTrackbarControlProjectionDiffusion.Value;
			float	KernelSize = floatTrackbarControlKernelSize.Value;

//			float	ShadowZFar = (float) Math.Sqrt( 2.0 ) * m_Camera.Far;
			float	ShadowZFar = 10.0f;
			m_CB_ShadowMap.m._ShadowOffsetXY = (float2) Direction;
			m_CB_ShadowMap.m._ShadowZFar = new float2( ShadowZFar, 1.0f / ShadowZFar );
			m_CB_ShadowMap.m._KernelSize = KernelSize;
			m_CB_ShadowMap.m._InvShadowMapSize = 1.0f / m_Tex_ShadowMap.Width;
			m_CB_ShadowMap.m._HardeningFactor = new float2( floatTrackbarControlHardeningFactor.Value, floatTrackbarControlHardeningFactor2.Value );
			m_CB_ShadowMap.UpdateData();

			if ( m_Shader_RenderShadowMap != null && m_Shader_RenderShadowMap.Use() ) {
				m_Tex_ShadowMap.RemoveFromLastAssignedSlots();

				m_Device.SetRenderTargets( null, m_Tex_ShadowMap );
#if FILTER_EXP_SHADOW_MAP
// 				m_Device.ClearDepthStencil( m_Tex_ShadowMap, 0.0f, 0, true, false );
// 				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_GREATER, BLEND_STATE.DISABLED );	// For exp shadow map, the Z order is reversed
				m_Device.ClearDepthStencil( m_Tex_ShadowMap, 1.0f, 0, true, false );
				m_Device.SetRenderStates( checkBoxCullFront.Checked ? RASTERIZER_STATE.CULL_BACK : RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );	// For exp shadow map, the Z order is reversed
#else
				m_Device.ClearDepthStencil( m_Tex_ShadowMap, 1.0f, 0, true, false );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
#endif
				RenderScene( m_Shader_RenderShadowMap );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowMap.SetPS( 2 );
			}

#if FILTER_EXP_SHADOW_MAP
			if ( m_Shader_FilterShadowMapH != null ) {
//				m_Tex_ShadowMapFiltered[1].RemoveFromLastAssignedSlots();

				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				// Filter horizontally
				m_Device.SetRenderTarget( m_Tex_ShadowMapFiltered[0], null );
				m_Shader_FilterShadowMapH.Use();
				m_Prim_Quad.Render( m_Shader_FilterShadowMapH );

				// Filter vertically
				m_Device.SetRenderTarget( m_Tex_ShadowMapFiltered[1], null );
				m_Tex_ShadowMapFiltered[0].SetPS( 2 );

				m_Shader_FilterShadowMapV.Use();
				m_Prim_Quad.Render( m_Shader_FilterShadowMapV );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowMapFiltered[1].SetPS( 2 );
			}
#else
			if ( m_Shader_BuildSmoothie != null && m_Shader_BuildSmoothie.Use() ) {
				m_Tex_ShadowSmoothie.RemoveFromLastAssignedSlots();

				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				// Render the (silhouette + Z) RG16 buffer
				m_Device.SetRenderTarget( m_Tex_ShadowSmoothie, null );

				m_Prim_Quad.Render( m_Shader_BuildSmoothie );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowSmoothie.SetPS( 3 );

				// Build distance field
				m_Device.SetRenderTarget( m_Tex_ShadowSmoothiePou[0], null );
				m_Shader_BuildSmoothieDistanceFieldH.Use();
				m_Prim_Quad.Render( m_Shader_BuildSmoothieDistanceFieldH );

// m_Device.RemoveRenderTargets();
// m_Tex_ShadowSmoothiePou[0].SetPS( 3 );

				m_Device.SetRenderTarget( m_Tex_ShadowSmoothiePou[1], null );
				m_Tex_ShadowSmoothiePou[0].SetPS( 0 );
				m_Shader_BuildSmoothieDistanceFieldV.Use();
				m_Prim_Quad.Render( m_Shader_BuildSmoothieDistanceFieldV );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowSmoothiePou[1].SetPS( 3 );
			}
#endif

			// =========== Render scene ===========
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

//			m_Tex_AreaLightSAT.SetPS( 0 );
//			m_Tex_AreaLight3D.SetPS( 0 );
//			m_Tex_AreaLightSATFade.SetPS( 1 );
			m_Tex_AreaLight.SetPS( 4 );
			m_Tex_FalseColors.SetPS( 6 );
// 			m_Tex_GlossMap.SetPS( 7 );
// 			m_Tex_Normal.SetPS( 8 );

			// Render the area light itself
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
			if ( m_Shader_RenderAreaLight != null && m_Shader_RenderAreaLight.Use() ) {

				m_CB_Object.m._Local2World = AreaLight2World;
				m_CB_Object.m._Local2World.Scale( new float3( SizeX, SizeY, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._UseTexture = checkBoxUseTexture.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColors = checkBoxFalseColors.Checked ? 1U : 0U;
				m_CB_Object.m._FalseColorsMaxRange = floatTrackbarControlFalseColorsRange.Value;
				m_CB_Object.UpdateData();

				m_Prim_Rectangle.Render( m_Shader_RenderAreaLight );
			} else {
				m_Device.Clear( new float4( 1, 0, 0, 0 ) );
			}


			// Render the scene
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.NOCHANGE, BLEND_STATE.NOCHANGE );
			if ( m_Shader_RenderScene != null && m_Shader_RenderScene.Use() ) {
				RenderScene( m_Shader_RenderScene );
			} else {
				m_Device.Clear( new float4( 1, 1, 0, 0 ) );
			}


			// Show!
			m_Device.Present( false );


			// Update window text
//			Text = "Zombizous Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}

		private void buttonRebuildBRDF_Click( object sender, EventArgs e ) {
			ComputeBRDFIntegral( new System.IO.FileInfo( "BRDF0_64x64.bin" ), 64 );
//			ComputeBRDFIntegralImportanceSampling( new System.IO.FileInfo( "BRDF1_64x64.bin" ), 64 );

			if ( m_Tex_BRDFIntegral != null )
				m_Tex_BRDFIntegral.Dispose();
			m_Tex_BRDFIntegral = BuildBRDFTexture( new System.IO.FileInfo( "BRDF0_64x64.bin" ), 64 );
			m_Tex_BRDFIntegral.SetPS( 5 );
		}

		#endregion
	}
}
