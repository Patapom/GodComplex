using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RendererManaged;
using System.IO;
using Nuaj.Cirrus.Utility;

namespace GloubiBoule
{
	public partial class GloubiForm : Form
	{
		#region CONSTANTS

		const int	VOLUME_SIZE = 128;
		const int	NOISE_SIZE = 64;

		#endregion

		#region NESTED TYPES

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
		private struct CB_PostProcess {
			public float2		_ScreenSize;
		}

		#endregion

		#region FIELDS

		private Device				m_Device = new Device();

		Primitive					m_Prim_Sphere;
		Primitive					m_Prim_Cube;
		Primitive					m_Prim_Quad;

		Texture2D					m_Tex_HeightMap;
		Texture2D					m_Tex_Scattering;
		Texture2D					m_Tex_AccumPhotonCube;
		Texture3D					m_Tex_AccumPhoton3D;
		Texture3D					m_Tex_Noise;

		ComputeShader				m_Shader_DeformSphere;
		ComputeShader				m_Shader_TracePhotons;
		Shader						m_Shader_Room;
		Shader						m_Shader_RenderSphere;
		Shader						m_Shader_RayMarcher;
		Shader						m_Shader_PostProcess;

		Camera						m_Camera;
		CameraManipulator			m_Manipulator;

		ConstantBuffer<CB_Camera>		m_CB_Camera;
		ConstantBuffer<CB_PostProcess>	m_CB_PostProcess;

		#endregion

		public GloubiForm()
		{
			InitializeComponent();

			Application.Idle += new EventHandler( Application_Idle );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try
			{
				m_Device.Init( panelOutput.Handle, false, true );
			} catch ( Exception _e ) {
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			try {
//				m_ShaderDownsample = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Downsample.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
//				m_Shader_RayMarcher = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/TestMSBRDF2.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				m_Shader_PostProcess = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/PostProcess.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			}
			catch ( Exception _e ) {
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}

			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_PostProcess = new ConstantBuffer<CB_PostProcess>( m_Device, 2 );

//			m_Tex_HeightMap = new Texture2D( m_Device );
			m_Tex_Scattering = new Texture2D( m_Device, panelOutput.Width, panelOutput.Height, 2, 1, PIXEL_FORMAT.RGBA16_FLOAT, false, false, null );
			m_Tex_AccumPhotonCube = new Texture2D( m_Device, 256, 256, -6, 1, PIXEL_FORMAT.RGBA16_FLOAT, false, true, null );;
			m_Tex_AccumPhoton3D = new Texture3D( m_Device, VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE, 1, PIXEL_FORMAT.R32_UINT, false, true, null );;
			m_Tex_Noise = BuildNoiseTexture();

			BuildPrimitives();

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, -2.5f ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
// 			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
// 			m_StopWatch.Start();
// 			m_StartGameTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			m_Prim_Cube.Dispose();
			m_Prim_Sphere.Dispose();
			m_Prim_Quad.Dispose();

			m_Tex_Noise.Dispose();
			m_Tex_AccumPhoton3D.Dispose();
			m_Tex_AccumPhotonCube.Dispose();
			m_Tex_Scattering.Dispose();
			m_Tex_HeightMap.Dispose();

			m_CB_PostProcess.Dispose();
			m_CB_Camera.Dispose();

			m_Shader_PostProcess.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		#region Primitives

		void	BuildPrimitives() {

			{	// Post-process quad
				List<VertexPt4>	Vertices = new List<VertexPt4>();
				Vertices.Add( new VertexPt4() { Pt=new float4( -1, +1, 0, 1 ) } );
				Vertices.Add( new VertexPt4() { Pt=new float4( -1, -1, 0, 1 ) } );
				Vertices.Add( new VertexPt4() { Pt=new float4( +1, +1, 0, 1 ) } );
				Vertices.Add( new VertexPt4() { Pt=new float4( +1, -1, 0, 1 ) } );
				m_Prim_Quad = new Primitive( m_Device, 4, VertexPt4.FromArray( Vertices.ToArray() ), null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
			}

			{	// Sphere Primitive
				List<VertexP3N3G3T2>	Vertices = new List<VertexP3N3G3T2>();
				List<uint>				Indices = new List<uint>();

				const int		SUBDIVS_THETA = 80;
				const int		SUBDIVS_PHI = 160;
				for ( int Y=0; Y <= SUBDIVS_THETA; Y++ ) {
					double	Theta = Y * Math.PI / SUBDIVS_THETA;
					float	CosTheta = (float) Math.Cos( Theta );
					float	SinTheta = (float) Math.Sin( Theta );
					for ( int X=0; X <= SUBDIVS_PHI; X++ ) {
						double	Phi = X * 2.0 * Math.PI / SUBDIVS_PHI;
						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float3	N = new float3( SinTheta*SinPhi, CosTheta, SinTheta*CosPhi );
						float3	T = new float3( CosPhi, 0.0f, -SinPhi );
						float2	UV = new float2( (float) X / SUBDIVS_PHI, (float) Y / SUBDIVS_THETA );
						Vertices.Add( new VertexP3N3G3T2() { P=N, N=N, T=T, UV=UV } );
					}
				}

				for ( int Y=0; Y < SUBDIVS_THETA; Y++ ) {
					int	CurrentLineOffset = Y * (SUBDIVS_PHI+1);
					int	NextLineOffset = (Y+1) * (SUBDIVS_PHI+1);
					for ( int X=0; X <= SUBDIVS_PHI; X++ ) {
						Indices.Add( (uint) (CurrentLineOffset + X) );
						Indices.Add( (uint) (NextLineOffset + X) );
					}
					if ( Y < SUBDIVS_THETA-1 ) {
						Indices.Add( (uint) (NextLineOffset - 1) );	// Degenerate triangle to end the line
						Indices.Add( (uint) NextLineOffset );		// Degenerate triangle to start the next line
					}
				}

				m_Prim_Sphere = new Primitive( m_Device, Vertices.Count, VertexP3N3G3T2.FromArray( Vertices.ToArray() ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3T2 );
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

				m_Prim_Cube = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
			}
		}

		#endregion

		#region  Noise Generation

		Texture3D	BuildNoiseTexture() {

			PixelsBuffer	Content = new PixelsBuffer( NOISE_SIZE*NOISE_SIZE*NOISE_SIZE*16 );

			WMath.SimpleRNG.SetSeed( 521288629, 362436069 );

			float4	V = float4.Zero;
			using ( BinaryWriter W = Content.OpenStreamWrite() ) {
				for ( int Z=0; Z < NOISE_SIZE; Z++ )
					for ( int Y=0; Y < NOISE_SIZE; Y++ )
						for ( int X=0; X < NOISE_SIZE; X++ ) {
							V.Set( (float) WMath.SimpleRNG.GetUniform(), (float) WMath.SimpleRNG.GetUniform(), (float) WMath.SimpleRNG.GetUniform(), (float) WMath.SimpleRNG.GetUniform() );
							W.Write( V.x );
							W.Write( V.y );
							W.Write( V.z );
							W.Write( V.w );
						}
			}

			return new Texture3D( m_Device, NOISE_SIZE, NOISE_SIZE, NOISE_SIZE, 1, PIXEL_FORMAT.RGBA8_UNORM, false, false, new PixelsBuffer[] { Content } );
		}

		#endregion

		void Camera_CameraTransformChanged( object sender, EventArgs e ) {

			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Proj2Camera * m_CB_Camera.m._Camera2World;

			m_CB_Camera.UpdateData();
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_Device == null )
				return;

// 			float	lastGameTime = m_CurrentGameTime;
// 			m_CurrentGameTime = GetGameTime();
// 			
// 			if ( m_CurrentGameTime - m_StartFPSTime > 1.0f ) {
// 				m_AverageFrameTime = (m_CurrentGameTime - m_StartFPSTime) / Math.Max( 1, m_SumFrames );
// 				m_SumFrames = 0;
// 				m_StartFPSTime = m_CurrentGameTime;
// 			}
// 			m_SumFrames++;

			Camera_CameraTransformChanged( m_Camera, EventArgs.Empty );

// 			// Render opaque
// 			if ( m_Shader != null ) {
// 				m_Device.SetRenderTargets( m_Tex_TempBuffer.Width, m_Tex_TempBuffer.Height, new IView[] { m_Tex_TempBuffer.GetView( 0, 1, 0, 1 ), m_Tex_TempBuffer.GetView( 0, 1, 1, 1 ) }, null );
// 				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
// 
// // 				m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
// // 				m_CB_Main.m.iGlobalTime = m_CurrentGameTime - m_StartGameTime;
// // 				m_CB_Main.m._WeightMultiplier = floatTrackbarControlWeightMultiplier.Value;
// // 				m_CB_Main.m._ShowWeights = (uint) ((checkBoxShowWeights.Checked ? 1 : 0) | (checkBoxSmoothStep.Checked ? 2 : 0) | (checkBoxShowOrder3.Checked ? 4 : 0) | (checkBoxShowOnlyMS.Checked ? 8 : 0));
// // 				m_CB_Main.m._DebugParm = floatTrackbarControlParm.Value;
// // 				m_CB_Main.UpdateData();
// 
// 				m_Shader.Use();
// 				m_Prim_Quad.Render( m_Shader );
// 			} else {
// 				m_Device.Clear( new float4( 1.0f, 0, 0, 0 ) );
// 			}

// 			// Perform buffer downsampling
// 			if ( m_ShaderDownsample.Use() ) {
// 				for ( int mipLevel=1; mipLevel < m_Tex_TempBuffer.MipLevelsCount; mipLevel++ ) {
// 					View2D	sourceView = m_Tex_TempBuffer.GetView( mipLevel-1, 1, 0, 0 );
// 					View2D	targetView0 = m_Tex_TempBuffer.GetView( mipLevel, 1, 0, 1 );
// 					View2D	targetView1 = m_Tex_TempBuffer.GetView( mipLevel, 1, 1, 1 );
// 					m_Device.SetRenderTargets( targetView0.Width, targetView0.Height, new IView[] { targetView0, targetView1 }, null );
// 					m_Tex_TempBuffer.Set( 0, sourceView );
// 					m_Prim_Quad.Render( m_ShaderDownsample );
// 				}
// 			}

			// Post-process
			if ( m_Shader_PostProcess.Use() ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, null );

// 				m_Tex_TempBuffer2.SetPS( 0 );

				m_Prim_Quad.Render( m_Shader_PostProcess );

//				m_Tex_TempBuffer.RemoveFromLastAssignedSlots();
			}

			// Show!
			m_Device.Present( false );

			// Update window text
//			Text = "ShaderToy - Avg. Frame Time " + (1000.0f * m_AverageFrameTime).ToString( "G5" ) + " ms (" + (1.0f / m_AverageFrameTime).ToString( "G5" ) + " FPS)";
		}
	}
}
