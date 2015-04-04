using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;
using Nuaj.Cirrus.Utility;

namespace SHZHConvolutionViewer
{
	public partial class Form1 : Form
	{
		Device				m_Device = null;

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Camera {
//			public float4x4		m_World2Camera;
			public float4x4		m_World2Proj;
		};

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Mesh {
			public float4		m_SH0;
			public float4		m_SH1;

			public float		m_SH2;
			public float3		m_ZH;

			public float4		m_resultSH0;
			public float4		m_resultSH1;

			public float		m_resultSH2;
			public uint			m_flags;
		};

		ConstantBuffer< CB_Camera >		m_CB_Camera;
		ConstantBuffer< CB_Mesh >		m_CB_Mesh;
		Shader							m_Shader_RenderMesh;
		Primitive						m_Prim_Sphere;

		Camera							m_Camera = new Camera();
		CameraManipulator				m_CameraManipulator = new CameraManipulator();

		public Form1()
		{
			InitializeComponent();

			m_Device = new Device();
			m_Device.Init( panelOutput1.Handle, false, false );

			m_Shader_RenderMesh = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "./Shaders/RenderMesh.hlsl" ) ), VERTEX_FORMAT.P3, "VS", null, "PS", null );

			{
				List<VertexP3>	Vertices = new List<VertexP3>();
				List<uint>		Indices = new List<uint>();

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
						Vertices.Add( new VertexP3() { P=N } );
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

				m_Prim_Sphere = new Primitive( m_Device, Vertices.Count, VertexP3.FromArray( Vertices.ToArray() ), Indices.ToArray(), Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3 );
			}

			// Setup camera
			m_CB_Camera = new ConstantBuffer< CB_Camera >( m_Device, 0 );
			m_CB_Mesh = new ConstantBuffer< CB_Mesh >( m_Device, 1 );

			m_Camera.CreatePerspectiveCamera( 120.0f * (float) Math.PI / 180.0f, (float) panelOutput1.Width / panelOutput1.Height, 0.01f, 40.0f );
			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			m_CameraManipulator.Attach( panelOutput1, m_Camera );
			m_CameraManipulator.InitializeCamera( -2.0f * float3.UnitZ, float3.Zero, float3.UnitY );

			Application.Idle += new EventHandler( Application_Idle );
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e )
		{
			m_CB_Camera.m.m_World2Proj = m_Camera.World2Camera * m_Camera.Camera2Proj;
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			m_Shader_RenderMesh.Dispose();
			m_Prim_Sphere.Dispose();
			m_CB_Mesh.Dispose();
			m_CB_Camera.Dispose();
			m_Device.Dispose();

			base.OnFormClosing( e );
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Update camera
			m_CB_Camera.UpdateData();

			// Clear
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );
			m_Device.Clear( float4.Zero );

			// Render
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

			if ( m_Shader_RenderMesh.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

				m_CB_Mesh.m.m_Color = new float4( 0.2f, 0.2f, 0.2f, 1 );
				m_CB_Mesh.UpdateData();

				m_Prim_Sphere.Render( m_Shader_RenderMesh );
			}

			m_Device.Present( false );
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}
	}
}
