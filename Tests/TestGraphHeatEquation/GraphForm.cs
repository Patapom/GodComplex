using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Renderer;
using SharpMath;
using Nuaj.Cirrus.Utility;
using Nuaj.Cirrus;

namespace TestGraphHeatEquation
{
	public partial class GraphForm : Form
	{
		#region CONSTANTS

		const int	GRAPH_SIZE = 64;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float2		iResolution;
			public float		tanHalfFOV;
			public float		iGlobalTime;
		}

		#endregion

		#region FIELDS

		Device		m_device = new Device();

		private ConstantBuffer<CB_Main>	m_CB_Main = null;
		private Shader					m_shader_RenderHeatMap = null;
		private Texture2D				m_tex_HeatMap_Staging = null;
		private Texture2D				m_tex_HeatMap = null;

		#endregion

		#region METHODS

		public GraphForm()
		{
			InitializeComponent();

			GraphSeparabilityTest();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput.Handle, false, true );
			}
			catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Heat Wave Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );

			m_shader_RenderHeatMap = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderHeatMap.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

			m_tex_HeatMap_Staging = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB, true, false, null );
			m_tex_HeatMap = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB, false, false, null );

			BuildGraph();

			Application.Idle += Application_Idle;
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			if ( !m_shader_RenderHeatMap.Use() )
				return;

			m_device.SetRenderTarget( m_device.DefaultTarget, null );
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			m_tex_HeatMap.SetPS( 0 );

			m_device.RenderFullscreenQuad( m_shader_RenderHeatMap );

			m_device.Present( false );
		}

		#region Graph Building

		void	BuildGraph() {

			// Build random obstacles
			const int	OBSTACLES_COUNT = 1000;

			uint[,]		obstaclesMap = new uint[GRAPH_SIZE,GRAPH_SIZE];
			Random		RNG = new Random( 1 );
			int			X, Y;
			for ( int i=0; i < OBSTACLES_COUNT; i++ ) {
				X = RNG.Next( GRAPH_SIZE );
				Y = RNG.Next( GRAPH_SIZE );
				obstaclesMap[X,Y] = ~0U;
			}

			// Count valid nodes
			uint	nodesCount = 0;	// Must equal at least GRAPH_SIZE*GRAPH_SIZE - OBSTACLES_COUNT when exiting
			for ( Y=0; Y < GRAPH_SIZE; Y++ ) {
				for ( X=0; X < GRAPH_SIZE; X++ ) {
					if ( obstaclesMap[X,Y] == ~0U )
						continue;

					obstaclesMap[X,Y] = nodesCount++;
				}
			}

			// Build the Laplacian matrix
			MathSolvers.MatrixF	laplacianMatrix = new MathSolvers.MatrixF( nodesCount, nodesCount );
//			float[,]	laplacianMatrix = new float[nodesCount,nodesCount];
			int[,]		dXY = new int[8,2] {
				{ -1, -1 },
				{ -1, 0 },
				{ -1, +1 },
				{  0, +1 },
				{ +1, +1 },
				{ +1,  0 },
				{ +1, -1 },
				{  0, -1 },
			};

			X = 0;
			Y = 0;
			for ( int nodeIndex=0; nodeIndex < nodesCount; nodeIndex++ ) {
				// Skip obstacles
				while ( obstaclesMap[X,Y] == ~0U ) {
					X++;
					if ( X >= GRAPH_SIZE ) {
						X = 0;
						Y++;
					}
				}

				// Collect connections
				int	degree = 0;
				for ( int neighborIndex=0; neighborIndex < 8; neighborIndex++ ) {
					int	Nx = X + dXY[neighborIndex,0];
					int	Ny = Y + dXY[neighborIndex,1];
					if ( Nx < 0 || Nx >= GRAPH_SIZE || Ny < 0 || Ny >= GRAPH_SIZE )
						continue;	// Out of graph...

					uint	neighborNodeIndex = obstaclesMap[Nx,Ny];
					if ( neighborNodeIndex == ~0U )
						continue;	// Obstacle...

					laplacianMatrix[(uint) nodeIndex, (uint) neighborNodeIndex] = -1;
					laplacianMatrix[(uint) neighborNodeIndex, (uint) nodeIndex] = -1;
					degree++;
				}

				laplacianMatrix[(uint) nodeIndex, (uint) nodeIndex] = degree;	// At most 8
			}

// 			//////////////////////////////////////////////////////////////////////////
// 			// Compute eigen vectors using singular value decomposition
// 			//
// 			MathSolvers.SVD		SVD = new MathSolvers.SVD( laplacianMatrix );
// 			SVD.Decompose();
// 
// 			MathSolvers.MatrixF	test = new MathSolvers.MatrixF( SVD.U.RowsCount, SVD.U.ColumnsCount );
// 			for ( uint r=0; r < test.RowsCount; r++ ) {
// 				for ( uint c=0; c < test.ColumnsCount; c++ ) {
// 
// 					float	sum = 0.0f;
// 					for ( uint i=0; i < test.RowsCount; i++ ) {
// 						sum += SVD.U[r,i] * SVD.V[i,c];
// 					}
// 				}
// 			}
		}

		//////////////////////////////////////////////////////////////////////////
		// Here we're going to test if the problem of heat diffusion across a graph is separable into multiple small sub-problems
		//
		//	• We first create a small graph of ~100 nodes
		//	• Next we build the full graph Laplacian matrix and compute the eigen vectors
		//	• Then we apply heat diffusion along time
		//
		//	• For the 2nd part, we separate each node into its own little sub-Laplacian matrix with only the connections to its direct neighbors
		//	• We compute the eigen vectors/values for each small matrix
		//	• We apply heat diffusion interatively node by node and we compare the results
		//
		void		GraphSeparabilityTest() {

			const int		NODES_COUNT = 100;
			const int		MAX_NEIGHBORS_COUNT = 6;

			const float		HEAT_DIFFUSION = 0.8f;
			const float		TIME_STEP = 0.01f;
			const int		ITERATIONS_COUNT = 100;

			//////////////////////////////////////////////////////////////////////////
			// 1] Construct a graph with 100 nodes
			MathSolvers.MatrixF	laplacian = new MathSolvers.MatrixF( (uint) NODES_COUNT, (uint) NODES_COUNT );

			Random	RNG = new Random( 1 );
			int	tentativeEdgesCount = 0;
			for ( int nodeIndex=0; nodeIndex < NODES_COUNT; nodeIndex++ ) {
				int	neighborsCount = 1 + RNG.Next( MAX_NEIGHBORS_COUNT );
				for ( int neighborIndex=0; neighborIndex < neighborsCount; neighborIndex++ ) {
					int	neighborNodeIndex = RNG.Next( NODES_COUNT - 1 );
						neighborNodeIndex = (nodeIndex + 1 + neighborNodeIndex) % NODES_COUNT;

					laplacian[(uint) nodeIndex, (uint) neighborNodeIndex] = -1;
					laplacian[(uint) neighborNodeIndex, (uint) nodeIndex] = -1;
				}

				tentativeEdgesCount += neighborsCount;
			}

			// Retrieve the amount of connections
			int	edgesCount = 0;
			for ( int nodeIndex=0; nodeIndex < NODES_COUNT; nodeIndex++ ) {
				int	degree = 0;
				for ( int columnIndex=0; columnIndex < nodeIndex; columnIndex++ )
					degree += laplacian[(uint) nodeIndex, (uint) columnIndex] != 0.0f ? 1 : 0;
				for ( int columnIndex=nodeIndex+1; columnIndex < NODES_COUNT; columnIndex++ )
					degree += laplacian[(uint) nodeIndex, (uint) columnIndex] != 0.0f ? 1 : 0;

				laplacian[(uint) nodeIndex, (uint) nodeIndex] = degree;

				edgesCount += degree;
			}
			edgesCount >>= 1;	// All edges are accounted twice...

			//////////////////////////////////////////////////////////////////////////
			// 2] Compute eigen vectors using singular value decomposition
			//
			MathSolvers.SVD		SVD = new MathSolvers.SVD( laplacian );
			SVD.Decompose();

			float[,]	eigenVectors = SVD.U.AsArray;
			float[]		eigenValues = SVD.w.AsArray;

// Here we make sure U and V are transposed of each other and U is an orthonormal basis
// 			float[,]	test = new float[SVD.U.RowsCount,SVD.U.ColumnsCount];
// 			float[,]	test2 = new float[SVD.U.RowsCount,SVD.U.ColumnsCount];
// 			float[,]	test3 = new float[SVD.U.RowsCount,SVD.U.ColumnsCount];
// 			for ( uint r=0; r < SVD.U.RowsCount; r++ ) {
// 				for ( uint c=0; c < SVD.U.ColumnsCount; c++ ) {
// 
// 					float	sum = 0.0f;
// 					float	sum2 = 0.0f;
// 					for ( uint i=0; i < SVD.U.RowsCount; i++ ) {
// 						sum += SVD.U[r,i] * SVD.V[i,c];
// 						sum2 += SVD.U[r,i] * SVD.V[c,i];
// 					}
// 
// 					test[r,c] = sum;
// 					test2[r,c] = sum2;
// 					test3[r,c] = SVD.U[r,c] - SVD.V[r,c];
// 				}
// 			}

// Check eigen vectors are all orthonormal
// 			float[,]	test4 = new float[SVD.U.RowsCount,SVD.U.ColumnsCount];
// 			for ( uint r=0; r < SVD.U.RowsCount; r++ ) {
// 				for ( uint c=0; c < SVD.U.ColumnsCount; c++ ) {
// 
// 					float	sum = 0.0f;
// 					for ( uint i=0; i < SVD.U.RowsCount; i++ ) {
// 						sum += SVD.U[r,i] * SVD.U[c,i];
// 					}
// 					if ( r == c ) {
// 						if ( Mathf.Abs( sum - 1 ) > 1e-4f )
// 							throw new Exception( "Not 1 along diagonal!" );
// 						else
// 							sum = 1;
// 					} else {
// 						if ( Mathf.Abs( sum ) > 1e-4f )
// 							throw new Exception( "Not 0 off diagonal!" );
// 						else
// 							sum = 0;
// 					}
// 					test4[r,c] = sum;
// 				}
// 			}

// Check recomposed matrix equals original matrix
//			float[,]	test5 = new float[SVD.U.RowsCount,SVD.U.ColumnsCount];
// 			for ( uint r=0; r < SVD.U.RowsCount; r++ ) {
// 				for ( uint c=0; c < SVD.U.ColumnsCount; c++ ) {
// 
// 					float	sum = 0.0f;
// 					for ( uint i=0; i < SVD.U.RowsCount; i++ ) {
// 						sum += eigenVectors[r,i] * eigenValues[i] * eigenVectors[c,i];
// 					}
// 					sum -= laplacian[r,c];
// 					test5[r,c] = sum;
// 				}
// 			}


			//////////////////////////////////////////////////////////////////////////
			// 3] Apply heat to node 0 and compute diffusion
			//
			float[][]	heats = new float[2][] {
				new float[NODES_COUNT],
				new float[NODES_COUNT]
			};

			heats[0][0] = 1;

			// 3.1) Transform heat vector into eigen-space: Phi' = V * Phi
			for ( int i=0; i < NODES_COUNT; i++ ) {
				float	sum = 0.0f;
				for ( int j=0; j < NODES_COUNT; j++ )
					sum += eigenVectors[i,j] *  heats[0][i];
//					sum += eigenVectors[j,i] *  heats[0][i];
				heats[1][i] = sum;
			}

			// 3.2) Apply diffusion over total diffusion time t = ITERATIONS_COUNT * TIME_STEP
			float	totalTime = ITERATIONS_COUNT * TIME_STEP;
			for ( int i=0; i < NODES_COUNT; i++ ) {
				float	lambda = eigenValues[i];
//				float	lambda = eigenValues[i] > 0.0 ? 1.0f / eigenValues[i] : 0.0f;
				float	phi_0 = heats[1][i];
				float	phi_t = phi_0 * Mathf.Exp( -HEAT_DIFFUSION * lambda * totalTime );
				heats[0][i] = phi_t;
			}

			// 3.1) Transform eigen-heat vector back into graph-space: Phi = Phi' * V
			for ( int i=0; i < NODES_COUNT; i++ ) {
				float	sum = 0.0f;
				for ( int j=0; j < NODES_COUNT; j++ )
					sum += heats[0][i] * eigenVectors[j,i];
				heats[1][i] = sum;
			}

			float[]	result_full = new float[NODES_COUNT];
			heats[1].CopyTo( result_full, 0 );
		}

		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();

				m_tex_HeatMap.Dispose();
				m_tex_HeatMap_Staging.Dispose();
				m_shader_RenderHeatMap.Dispose();
				m_CB_Main.Dispose();

				Device	temp = m_device;
				m_device = null;
				temp.Dispose();
			}
			base.Dispose( disposing );
		}

		#endregion

		#region EVENT HANDLERS

		private void button1_Click( object sender, EventArgs e )
		{

		}

		private void checkBox1_CheckedChanged( object sender, EventArgs e )
		{

		}

		private void floatTrackbarControl1_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{

		}

		#endregion
	}
}
