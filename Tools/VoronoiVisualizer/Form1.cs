using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Renderer;
using ImageUtility;
using SharpMath;
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
		struct CB_Mesh {
			public float4		m_Color;
		};

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct SB_Neighbor {
			public float3		m_Position;
			public float3		m_Color;
		}

		ConstantBuffer< CB_Camera >		m_CB_Camera;
		ConstantBuffer< CB_Mesh >		m_CB_Mesh;
		StructuredBuffer< SB_Neighbor >	m_SB_Neighbors;
		Shader							m_Shader_RenderCellPlanes;
		Shader							m_Shader_RenderCellMesh;
		Shader							m_Shader_RenderCellMesh_Opaque;
		Shader							m_Shader_PostProcess;
		Shader							m_Shader_PostProcess2;
		Primitive						m_Prim_Quad;

		Texture2D[]			m_RT_WorldPositions = new Texture2D[2];

		Hammersley			m_hammersley = new Hammersley();
		float3[]			m_NeighborPositions = null;
		float3[]			m_NeighborColors = null;

		Camera				m_Camera = new Camera();
		CameraManipulator	m_CameraManipulator = new CameraManipulator();

		public Form1()
		{
			InitializeComponent();

			m_Device = new Device();
			m_Device.Init( panel1.Handle, false, false );

			m_Shader_RenderCellPlanes = new Shader( m_Device, new System.IO.FileInfo( "RenderCellPlanes.hlsl" ), VERTEX_FORMAT.T2, "VS", null, "PS", null );
			m_Shader_RenderCellMesh = new Shader( m_Device, new System.IO.FileInfo( "RenderCellMesh.hlsl" ), VERTEX_FORMAT.P3N3, "VS", null, "PS", null );
			m_Shader_RenderCellMesh_Opaque = new Shader( m_Device, new System.IO.FileInfo( "RenderCellMesh_Opaque.hlsl" ), VERTEX_FORMAT.P3N3, "VS", null, "PS", null );
			m_Shader_PostProcess = new Shader( m_Device, new System.IO.FileInfo( "PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			m_Shader_PostProcess2 = new Shader( m_Device, new System.IO.FileInfo( "PostProcess.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

			VertexT2[]	Vertices = new VertexT2[4] {
				new VertexT2() { UV = new float2( 0, 0 ) },
				new VertexT2() { UV = new float2( 0, 1 ) },
				new VertexT2() { UV = new float2( 1, 0 ) },
				new VertexT2() { UV = new float2( 1, 1 ) },
			};
			m_Prim_Quad = new Primitive( m_Device, 4, VertexT2.FromArray( Vertices ), null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.T2 );


			m_RT_WorldPositions[0] = new Texture2D( m_Device, (uint) panel1.Width, (uint) panel1.Height, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_RT_WorldPositions[1] = new Texture2D( m_Device, (uint) panel1.Width, (uint) panel1.Height, 1, 1, PIXEL_FORMAT.RGBA32F, COMPONENT_FORMAT.AUTO, false, false, null );
			m_Device.Clear( m_RT_WorldPositions[0], float4.Zero );
			m_Device.Clear( m_RT_WorldPositions[1], float4.Zero );

			// Setup camera
			m_CB_Camera = new ConstantBuffer< CB_Camera >( m_Device, 0 );
			m_CB_Mesh = new ConstantBuffer< CB_Mesh >( m_Device, 1 );

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
			if ( m_Prim_CellFaces != null ) {
				m_Prim_CellFaces.Dispose();
				m_Prim_CellEdges.Dispose();
			}

			m_RT_WorldPositions[0].Dispose();
			m_RT_WorldPositions[1].Dispose();

			m_Shader_RenderCellMesh_Opaque.Dispose();
			m_Shader_RenderCellMesh.Dispose();
			m_Shader_RenderCellPlanes.Dispose();
			m_Prim_Quad.Dispose();
			m_SB_Neighbors.Dispose();
			m_CB_Mesh.Dispose();
			m_CB_Camera.Dispose();
			m_Device.Dispose();

			base.OnFormClosing( e );
		}

		bool	m_bHasRendered = false;
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
			if ( checkBoxRenderCell.Checked && m_Prim_CellFaces != null ) {
				// Render mesh
				if ( m_Shader_RenderCellMesh.Use() ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ADDITIVE );

					// Render faces
					m_CB_Mesh.m.m_Color = new float4( 0.2f, 0.2f, 0.2f, 1 );
					m_CB_Mesh.UpdateData();
					m_Prim_CellFaces.Render( m_Shader_RenderCellMesh );

					// Render faces
					m_Device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
					m_CB_Mesh.m.m_Color = new float4( 1, 1, 0, 1 );
					m_CB_Mesh.UpdateData();
					m_Prim_CellEdges.Render( m_Shader_RenderCellMesh );
				}


				// Render again
				if ( m_Shader_RenderCellMesh_Opaque.Use() ) {
					m_Device.SetRenderTarget( m_RT_WorldPositions[0], m_Device.DefaultDepthStencil );
					m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_DEPTH_LESS_EQUAL, BLEND_STATE.DISABLED );

					m_Prim_CellFaces.Render( m_Shader_RenderCellMesh );
				}
			} else {
				// Render planes
				if ( m_Shader_RenderCellPlanes.Use() ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );

					m_SB_Neighbors.SetInput( 0 );

					m_Prim_Quad.RenderInstanced( m_Shader_RenderCellPlanes, (uint) m_SB_Neighbors.m.Length );
				}
			}

			// Post-Process
			if ( checkBoxRenderCell.Checked ) {
				if ( m_Shader_PostProcess.Use() ) {
					m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
					m_Device.SetRenderTarget( m_RT_WorldPositions[1], null );
					m_RT_WorldPositions[0].SetPS( 0 );
					m_Device.RenderFullscreenQuad( m_Shader_PostProcess );

					Texture2D	Temp = m_RT_WorldPositions[0];
					m_RT_WorldPositions[0] = m_RT_WorldPositions[1];
					m_RT_WorldPositions[1] = Temp;
				}

				if ( m_Shader_PostProcess2.Use() ) {
					m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
					m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.ADDITIVE );
					m_RT_WorldPositions[0].SetPS( 0 );
					m_Device.RenderFullscreenQuad( m_Shader_PostProcess2 );
				}
			}

			m_Device.Present( false );
			m_bHasRendered = true;
		}

		private void integerTrackbarControlNeighborsCount_ValueChanged( IntegerTrackbarControl _Sender, int _FormerValue )
		{
			int	NeighborsCount = integerTrackbarControlNeighborsCount.Value;
			if ( m_SB_Neighbors != null )
				m_SB_Neighbors.Dispose();
			m_SB_Neighbors = new StructuredBuffer< SB_Neighbor >( m_Device, (uint) NeighborsCount, true );

			float3[]	directions = null;
			if ( radioButtonHammersley.Checked ) {
				double[,]	samples = m_hammersley.BuildSequence( NeighborsCount, 2 );
				directions = m_hammersley.MapSequenceToSphere( samples );
			} else {
				Random	TempRNG = new Random();
				directions = new float3[NeighborsCount];
				for ( int i=0; i < NeighborsCount; i++ ) {
					directions[i] = new float3( 2.0f * (float) TempRNG.NextDouble() - 1.0f, 2.0f * (float) TempRNG.NextDouble() - 1.0f, 2.0f * (float) TempRNG.NextDouble() - 1.0f );
					directions[i].Normalize();
				}
			}

			Random	RNG = new Random( 1 );

			m_NeighborPositions = new float3[NeighborsCount];
			m_NeighborColors = new float3[NeighborsCount];
			for ( int NeighborIndex=0; NeighborIndex < NeighborsCount; NeighborIndex++ ) {
				float	Radius = 2.0f;	// Make that random!
				m_NeighborPositions[NeighborIndex] = Radius * new float3( directions[NeighborIndex].x, directions[NeighborIndex].y, directions[NeighborIndex].z );

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
			if ( !m_bHasRendered )
				return;
			m_bHasRendered = false;

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

			if ( checkBoxRenderCell.Checked ) {
				buttonBuildCell_Click( this, EventArgs.Empty );
				Application.DoEvents();
			}
		}

		private void checkBoxRenderCell_CheckedChanged( object sender, EventArgs e )
		{
			if ( m_Prim_CellFaces == null )
				buttonBuildCell_Click( this, EventArgs.Empty );
		}

		#region Cell Building

		class	CellPolygon {

			float3		m_P;
			float3		m_T;
			float3		m_B;
			float3		m_N;
			public float3[]	m_Vertices = null;

			public CellPolygon( float3 _P, float3 _N ) {

				m_P = _P;
				m_N = _N;
				m_T = (float3.UnitY.Cross( m_N )).Normalized;
				m_B = m_N.Cross( m_T );

				// Start with 4 vertices
				const float	R = 10.0f;
				m_Vertices = new float3[] {
					m_P + R * (-m_T + m_B),
					m_P + R * (-m_T - m_B),
					m_P + R * ( m_T - m_B),
					m_P + R * ( m_T + m_B),
				};
			}

			// Cut polygon with a new plane, yielding a new polygon
			public void	Cut( float3 _P, float3 _N ) {
				List< float3 >	NewVertices = new List< float3 >();
				for ( int EdgeIndex=0; EdgeIndex < m_Vertices.Length; EdgeIndex++ ) {
					float3	P0 = m_Vertices[EdgeIndex+0];
					float3	P1 = m_Vertices[(EdgeIndex+1)%m_Vertices.Length];
					float	Dot0 = (P0 - _P).Dot( _N );
					float	Dot1 = (P1 - _P).Dot( _N );
					bool	InFront0 = Dot0 >= 0.0f;
					bool	InFront1 = Dot1 >= 0.0f;
					if ( !InFront0 && !InFront1 )
						continue;	// This edge is completely behind the cutting plane, skip it entirely

					if ( InFront0 && InFront1 ) {
						// This edge is completely in front of the cutting plane, add P1
						NewVertices.Add( P1 );
					} else {
						// The edge intersects the plane
						float3	D = P1 - P0;
						float	t = -Dot0 / D.Dot( _N );
						float3	I = P0 + t * D;
						NewVertices.Add( I );		// Add intersection no matter what
						if ( InFront1 )
							NewVertices.Add( P1 );	// Since the edge is entering the plane, also add end point
					}
				}

				m_Vertices = NewVertices.ToArray();
			}
		}

		float		m_AreaMin = float.MaxValue;
		float		m_AreaMax = -float.MaxValue;
		float		m_AreaAvg = 0.0f;
		Primitive	m_Prim_CellFaces = null;
		Primitive	m_Prim_CellEdges = null;
		private void buttonBuildCell_Click( object sender, EventArgs e )
		{
			if ( m_Prim_CellFaces != null ) {
				m_Prim_CellFaces.Dispose();
				m_Prim_CellEdges.Dispose();
				m_Prim_CellFaces = null;
				m_Prim_CellEdges = null;
			}

			float		AreaThresholdLow = m_AreaMin + 0.1f * (m_AreaAvg - m_AreaMin);
			float		AreaThresholdHigh = m_AreaMax - 0.1f * (m_AreaMax - m_AreaAvg);
// 			float		AreaThresholdLow = m_AreaAvg / 1.5f;
// 			float		AreaThresholdHigh = 1.5f * m_AreaAvg;

			List<VertexP3N3>	vertices = new List<VertexP3N3>();
			List<uint>			indices_Faces = new List<uint>();
			List<uint>			Indices_Edges = new List<uint>();

			float	AreaMin = float.MaxValue;
			float	AreaMax = -float.MaxValue;
			float	TotalArea = 0.0f;

			for ( int FaceIndex=0; FaceIndex < m_NeighborPositions.Length; FaceIndex++ ) {
				float3	P = m_NeighborPositions[FaceIndex];
				float3	N = -P.Normalized;	// Pointing inward

				// Build the polygon by cutting it with all other neighbors
				CellPolygon	Polygon = new CellPolygon( P, N );
				for ( int NeighborIndex=0; NeighborIndex < m_NeighborPositions.Length; NeighborIndex++ )
					if ( NeighborIndex != FaceIndex ) {
						float3	Pn = m_NeighborPositions[NeighborIndex];
						float3	Nn = -Pn.Normalized;	// Pointing inward
						Polygon.Cut( Pn, Nn );
					}

				// Append vertices & indices for both faces & edges
				uint	VertexOffset = (uint) vertices.Count;
				uint	VerticesCount = (uint) Polygon.m_Vertices.Length;

				float	PolygonArea = 0.0f;
				for ( uint FaceTriangleIndex=0; FaceTriangleIndex < VerticesCount-2; FaceTriangleIndex++ ) {
					indices_Faces.Add( VertexOffset + 0 );
					indices_Faces.Add( VertexOffset + 1 + FaceTriangleIndex );
					indices_Faces.Add( VertexOffset + 2 + FaceTriangleIndex );

					float	Area = 0.5f * (Polygon.m_Vertices[2 + FaceTriangleIndex] - Polygon.m_Vertices[0]).Cross( Polygon.m_Vertices[1 + FaceTriangleIndex] - Polygon.m_Vertices[0] ).Length;
					PolygonArea += Area;
				}
				AreaMin = Math.Min( AreaMin, PolygonArea );
				AreaMax = Math.Max( AreaMax, PolygonArea );
				TotalArea += PolygonArea;

				for ( uint VertexIndex=0; VertexIndex < VerticesCount; VertexIndex++ ) {
					Indices_Edges.Add( VertexOffset + VertexIndex );
					Indices_Edges.Add( VertexOffset + (VertexIndex+1) % VerticesCount );
				}

				float3	Color = PolygonArea < AreaThresholdLow ? new float3( 1, 0, 0 ) : PolygonArea > AreaThresholdHigh ? new float3( 0, 1, 0 ) : new float3( 1, 1, 0 );

				foreach ( float3 Vertex in Polygon.m_Vertices ) {
					vertices.Add( new VertexP3N3() { P = Vertex, N = Color } );
				}

			}

			m_AreaMin = AreaMin;
			m_AreaMax = AreaMax;
			m_AreaAvg = TotalArea / m_NeighborPositions.Length;
			labelStats.Text = "Area min: " + m_AreaMin + "\r\n"
							+ "Area max: " + m_AreaMax + "\r\n"
							+ "Area average: " + m_AreaAvg + "\r\n"
							+ "Area total: " + TotalArea + "\r\n";

			m_Prim_CellFaces = new Primitive( m_Device, (uint) vertices.Count, VertexP3N3.FromArray( vertices.ToArray() ), indices_Faces.ToArray(), Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3 );
			m_Prim_CellEdges = new Primitive( m_Device, (uint) vertices.Count, VertexP3N3.FromArray( vertices.ToArray() ), Indices_Edges.ToArray(), Primitive.TOPOLOGY.LINE_LIST, VERTEX_FORMAT.P3N3 );
		}

		#endregion

		private void buttonDumpDirections_Click( object sender, EventArgs e )
		{
			string	Text = "{\r\n";
			for ( int NeighborIndex=0; NeighborIndex < m_NeighborPositions.Length; NeighborIndex++ ) {
				float3	D = m_NeighborPositions[NeighborIndex].Normalized;
				Text += "float3( " + D.x + "f, " + D.y + "f, " + D.z + "f ),\r\n";
			}
			Text += "};\r\n";

			Clipboard.SetText( Text );
		}
	}
}
