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

namespace VoronoiVisualizer
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
		struct SB_Neighbor {
			public float3		m_Position;
			public float3		m_Color;
		}

		ConstantBuffer< CB_Camera >		m_CB_Camera;
		StructuredBuffer< SB_Neighbor >	m_SB_Neighbors;
		Shader							m_Shader_RenderCellPlanes;
		Primitive						m_Prim_Quad;

		WMath.Hammersley	m_Hammersley = new WMath.Hammersley();
		float3[]			m_NeighborPositions = null;
		float3[]			m_NeighborColors = null;

		Camera				m_Camera = new Camera();
		CameraManipulator	m_CameraManipulator = new CameraManipulator();

		public Form1()
		{
			InitializeComponent();

			m_Device = new Device();
			m_Device.Init( panel1.Handle, false, false );

			m_Shader_RenderCellPlanes = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "RenderCellPlanes.hlsl" ) ), VERTEX_FORMAT.T2, "VS", null, "PS", null );

			VertexT2[]	Vertices = new VertexT2[4] {
				new VertexT2() { UV = new float2( 0, 0 ) },
				new VertexT2() { UV = new float2( 0, 1 ) },
				new VertexT2() { UV = new float2( 1, 0 ) },
				new VertexT2() { UV = new float2( 1, 1 ) },
			};
			m_Prim_Quad = new Primitive( m_Device, 4, VertexT2.FromArray( Vertices ), null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.T2 );

			// Setup camera
			m_CB_Camera = new ConstantBuffer< CB_Camera >( m_Device, 0 );

			m_Camera.CreatePerspectiveCamera( 120.0f * (float) Math.PI / 180.0f, (float) panel1.Width / panel1.Height, 0.01f, 100.0f );
			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			m_CameraManipulator.Attach( panel1, m_Camera );
			m_CameraManipulator.InitializeCamera( -0.1f * float3.UnitZ, float3.Zero, float3.UnitY );

			// Initalize random neighbors
			integerTrackbarControlNeighborsCount_ValueChanged( integerTrackbarControlNeighborsCount, 0 );

			Application.Idle += new EventHandler( Application_Idle );
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e )
		{
			m_CB_Camera.m.m_World2Proj = m_Camera.World2Camera * m_Camera.Camera2Proj;
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			m_Shader_RenderCellPlanes.Dispose();
			m_Prim_Quad.Dispose();
			m_SB_Neighbors.Dispose();
			m_CB_Camera.Dispose();
			m_Device.Dispose();

			base.OnFormClosing( e );
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			if ( checkBoxSimulate.Checked ) {
// 				// Perform simulation
// 				int			NeighborsCount = m_NeighborPositions.Length;
// 
// 				// Compute pressure forces
// 				float		F = 0.01f * floatTrackbarControlForce.Value;
// 				float3[]	Forces = new float3[NeighborsCount];
// 				for ( int i=0; i < NeighborsCount-1; i++ ) {
// 					float3	D0 = m_NeighborPositions[i].Normalized;
// 					for ( int j=i+1; j < NeighborsCount; j++ ) {
// 						float3	D1 = m_NeighborPositions[j].Normalized;
// 
// 						float3	Dir = (D1 - D0).Normalized;
// 
// 						float	Dot = D0.Dot( D1 ) - 1.0f;	// in [0,-2]
// 						float	Force = F * (float) Math.Exp( Dot );
// 						Forces[i] = Forces[i] - Force * Dir;	// Pushes 0 away from 1
// 						Forces[j] = Forces[j] + Force * Dir;	// Pushes 1 away from 0
// 					}
// 				}
// 
// 				// Apply force
// 				for ( int i=0; i < NeighborsCount; i++ ) {
// 					float3	NewPosition = (m_NeighborPositions[i] + Forces[i]).Normalized;
// 					m_NeighborPositions[i] = NewPosition;
// 					m_SB_Neighbors.m[i].m_Position = NewPosition;
// 				}
// 
// 				// Update
// 				m_SB_Neighbors.Write();
			}

			// Update camera
			m_CB_Camera.UpdateData();

			// Clear
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );
			m_Device.Clear( float4.Zero );

			// Render
			if ( m_Shader_RenderCellPlanes.Use() ) {
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
				m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

				m_SB_Neighbors.SetInput( 0 );

				m_Prim_Quad.RenderInstanced( m_Shader_RenderCellPlanes, m_SB_Neighbors.m.Length );
			}

			m_Device.Present( false );
		}

		private void integerTrackbarControlNeighborsCount_ValueChanged( IntegerTrackbarControl _Sender, int _FormerValue )
		{
			int	NeighborsCount = integerTrackbarControlNeighborsCount.Value;
			if ( m_SB_Neighbors != null )
				m_SB_Neighbors.Dispose();
			m_SB_Neighbors = new StructuredBuffer< SB_Neighbor >( m_Device, NeighborsCount, true );

			WMath.Vector[]	Directions = null;
			if ( radioButtonHammersley.Checked ) {
				double[,]		Samples = m_Hammersley.BuildSequence( NeighborsCount, 2 );
				Directions = m_Hammersley.MapSequenceToSphere( Samples, false );
			} else {
				Random	TempRNG = new Random();
				Directions = new WMath.Vector[NeighborsCount];
				for ( int i=0; i < NeighborsCount; i++ ) {
					Directions[i] = new WMath.Vector( 2.0f * (float) TempRNG.NextDouble() - 1.0f, 2.0f * (float) TempRNG.NextDouble() - 1.0f, 2.0f * (float) TempRNG.NextDouble() - 1.0f );
					Directions[i].Normalize();
				}
			}

			Random	RNG = new Random( 1 );

			m_NeighborPositions = new float3[NeighborsCount];
			m_NeighborColors = new float3[NeighborsCount];
			for ( int NeighborIndex=0; NeighborIndex < NeighborsCount; NeighborIndex++ ) {
				float	Radius = 2.0f;	// Make that random!
				m_NeighborPositions[NeighborIndex] = Radius * new float3( Directions[NeighborIndex].x, Directions[NeighborIndex].y, Directions[NeighborIndex].z );

				float	R = (float) RNG.NextDouble();
				float	G = (float) RNG.NextDouble();
				float	B = (float) RNG.NextDouble();
				m_NeighborColors[NeighborIndex] = new float3( R, G, B );

				m_SB_Neighbors.m[NeighborIndex].m_Position = m_NeighborPositions[NeighborIndex];
				m_SB_Neighbors.m[NeighborIndex].m_Color = m_NeighborColors[NeighborIndex];
			}

			m_SB_Neighbors.Write();	// Upload
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}

		private void radioButtonHammersley_CheckedChanged( object sender, EventArgs e )
		{
			integerTrackbarControlNeighborsCount_ValueChanged( integerTrackbarControlNeighborsCount, 0 );
		}

		private void radioButtonRandom_CheckedChanged( object sender, EventArgs e )
		{
			integerTrackbarControlNeighborsCount_ValueChanged( integerTrackbarControlNeighborsCount, 0 );
		}

		private void checkBoxSimulate_CheckedChanged( object sender, EventArgs e )
		{
			timer1.Enabled = checkBoxSimulate.Checked;
		}

		private void timer1_Tick( object sender, EventArgs e )
		{
			// Perform simulation
			int			NeighborsCount = m_NeighborPositions.Length;

			// Compute pressure forces
			float		F = 0.01f * floatTrackbarControlForce.Value;
			float3[]	Forces = new float3[NeighborsCount];
			for ( int i=0; i < NeighborsCount-1; i++ ) {
				float3	D0 = m_NeighborPositions[i].Normalized;
				for ( int j=i+1; j < NeighborsCount; j++ ) {
					float3	D1 = m_NeighborPositions[j].Normalized;

					float3	Dir = (D1 - D0).Normalized;

					float	Dot = D0.Dot( D1 ) - 1.0f;	// in [0,-2]
					float	Force = F * (float) Math.Exp( Dot );
					Forces[i] = Forces[i] - Force * Dir;	// Pushes 0 away from 1
					Forces[j] = Forces[j] + Force * Dir;	// Pushes 1 away from 0
				}
			}

			// Apply force
			for ( int i=0; i < NeighborsCount; i++ ) {
				float3	NewPosition = (m_NeighborPositions[i] + Forces[i]).Normalized;
				m_NeighborPositions[i] = NewPosition;
				m_SB_Neighbors.m[i].m_Position = NewPosition;
			}

			// Update
			m_SB_Neighbors.Write();
		}
	}
}
