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

		const int	GRAPH_SIZE = 128;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float2		mousePosition;	// Mouse position in texels
			public uint			mouseButtons;	// Mouse button states (0=left button, 1=middle, 2=right)
			public float		deltaTime;		// Time step
			public float		diffusionCoefficient;
		}

		#endregion

		#region FIELDS

		Device		m_device = new Device();

		private ConstantBuffer<CB_Main>	m_CB_Main = null;
		private Shader					m_shader_RenderHeatMap = null;
		private Shader					m_shader_HeatDiffusion = null;
		private Shader					m_shader_DrawObstacles = null;
//		private Texture2D				m_tex_HeatMap_Staging = null;
		private Texture2D				m_tex_HeatMap0 = null;
		private Texture2D				m_tex_HeatMap1 = null;
		private Texture2D				m_tex_Obstacles0 = null;
		private Texture2D				m_tex_Obstacles1 = null;

		private Texture2D				m_tex_Obstacles_Staging = null;

		private Texture2D				m_tex_FalseColors = null;

		#endregion

		#region METHODS

		public GraphForm()
		{
			InitializeComponent();

//			GraphSeparabilityTest();
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

			m_shader_HeatDiffusion = new Shader( m_device, new System.IO.FileInfo( "./Shaders/HeatDiffusion.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			m_shader_RenderHeatMap = new Shader( m_device, new System.IO.FileInfo( "./Shaders/RenderHeatMap.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			m_shader_DrawObstacles = new Shader( m_device, new System.IO.FileInfo( "./Shaders/DrawObstacles.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

//			m_tex_HeatMap_Staging = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB, true, false, null );
			m_tex_HeatMap0 = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_HeatMap1 = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_Obstacles0 = new Texture2D( m_device, (uint) GRAPH_SIZE + 2, (uint) GRAPH_SIZE + 2, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_Obstacles1 = new Texture2D( m_device, (uint) GRAPH_SIZE + 2, (uint) GRAPH_SIZE + 2, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_Obstacles_Staging = new Texture2D( m_device, (uint) GRAPH_SIZE + 2, (uint) GRAPH_SIZE + 2, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, true, false, null );
			m_tex_Obstacles_Staging.WritePixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryWriter W ) => {
				bool	obstacle = _X == 0 || _Y == 0 || _X == GRAPH_SIZE+1 || _Y == GRAPH_SIZE+1;
				if ( obstacle )
					W.Write( 0x000000FFU );
				else
					W.Write( 0x00000000U );
			} );
			buttonResetObstacles_Click( null, EventArgs.Empty );

			// Load false colors
			using ( ImageUtility.ImageFile sourceImage = new ImageUtility.ImageFile( new System.IO.FileInfo( "../../Images/Gradients/Magma.png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG ) ) {
				ImageUtility.ImageFile convertedImage = new ImageUtility.ImageFile();
				convertedImage.ConvertFrom( sourceImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
				using ( ImageUtility.ImagesMatrix image = new ImageUtility.ImagesMatrix( convertedImage, ImageUtility.ImagesMatrix.IMAGE_TYPE.sRGB ) )
					m_tex_FalseColors = new Texture2D( m_device, image, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
			}

//			BuildGraph();

			Application.Idle += Application_Idle;
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			Point	clientPos = panelOutput.PointToClient( Control.MousePosition );
			m_CB_Main.m.mousePosition.Set( GRAPH_SIZE * (float) clientPos.X / panelOutput.Width, GRAPH_SIZE * (float) clientPos.Y / panelOutput.Height );
			m_CB_Main.m.mouseButtons = (uint) ((((Control.MouseButtons & MouseButtons.Left) != 0) ? 1 : 0)
											| (((Control.MouseButtons & MouseButtons.Middle) != 0) ? 2 : 0)
											| (((Control.MouseButtons & MouseButtons.Right) != 0) ? 4 : 0)
											| (Control.ModifierKeys == Keys.Shift ? 8 : 0));
			m_CB_Main.m.deltaTime = floatTrackbarControlDeltaTime.Value;
			m_CB_Main.m.diffusionCoefficient = floatTrackbarControlDiffusionCoefficient.Value;
			m_CB_Main.UpdateData();


			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			//////////////////////////////////////////////////////////////////////////
			// Draw Obstacles
			if ( m_shader_DrawObstacles.Use() ) {
				m_device.SetRenderTarget( m_tex_Obstacles1, null );
				m_tex_Obstacles0.SetPS( 0 );
				m_device.RenderFullscreenQuad( m_shader_DrawObstacles );
			}

			//////////////////////////////////////////////////////////////////////////
			// Perform heat diffusion test
			if ( m_shader_HeatDiffusion.Use() ) {
				m_device.SetRenderTarget( m_tex_HeatMap1, null );

				m_tex_HeatMap0.SetPS( 0 );
				m_tex_Obstacles1.SetPS( 1 );

				m_device.RenderFullscreenQuad( m_shader_HeatDiffusion );
			}

			//////////////////////////////////////////////////////////////////////////
			// Render
			if ( m_shader_RenderHeatMap.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_HeatMap1.SetPS( 0 );
				m_tex_Obstacles1.SetPS( 1 );
				m_tex_FalseColors.SetPS( 2 );

				m_device.RenderFullscreenQuad( m_shader_RenderHeatMap );
			}

			m_device.Present( false );


			// Swap
			Texture2D	temp = m_tex_HeatMap0;
			m_tex_HeatMap0 = m_tex_HeatMap1;
			m_tex_HeatMap1 = temp;

			temp = m_tex_Obstacles0;
			m_tex_Obstacles0 = m_tex_Obstacles1;
			m_tex_Obstacles1 = temp;
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
			const float		TIME_STEP = 0.1f;
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
			int	minConnections = int.MaxValue;
			int	maxConnections = -int.MaxValue;
			for ( int nodeIndex=0; nodeIndex < NODES_COUNT; nodeIndex++ ) {
				int	degree = 0;
				for ( int columnIndex=0; columnIndex < nodeIndex; columnIndex++ )
					degree += laplacian[(uint) nodeIndex, (uint) columnIndex] != 0.0f ? 1 : 0;
				for ( int columnIndex=nodeIndex+1; columnIndex < NODES_COUNT; columnIndex++ )
					degree += laplacian[(uint) nodeIndex, (uint) columnIndex] != 0.0f ? 1 : 0;

				laplacian[(uint) nodeIndex, (uint) nodeIndex] = degree;

				edgesCount += degree;
				minConnections = Math.Min( minConnections, degree );
				maxConnections = Math.Max( maxConnections, degree );
			}
			edgesCount >>= 1;	// All edges are accounted twice...

			//////////////////////////////////////////////////////////////////////////
			// 2] Compute full heat diffusion
			float[][]	results_full = ComputeHeatDiffusionFull( laplacian, HEAT_DIFFUSION, TIME_STEP, ITERATIONS_COUNT );

			//////////////////////////////////////////////////////////////////////////
			// 3] Compute iterative diffusion
			float[][]	results_Euler = ComputeHeatDiffusionEuler( laplacian, HEAT_DIFFUSION, TIME_STEP, ITERATIONS_COUNT );

			//////////////////////////////////////////////////////////////////////////
			// 4] Compare errors between methods
			float[]		errors = new float[ITERATIONS_COUNT];
			float		delta;
			for ( uint iterationIndex=0; iterationIndex < ITERATIONS_COUNT; iterationIndex++ ) {
				float[]	temps0 = results_full[iterationIndex];
				float[]	temps1 = results_Euler[iterationIndex];

				float	sqError = 0.0f;
				for ( uint nodeIndex=0; nodeIndex < NODES_COUNT; nodeIndex++ ) {
					delta = temps0[nodeIndex] - temps1[nodeIndex];
					sqError += delta * delta;
				}
				errors[iterationIndex] = Mathf.Sqrt( sqError );
			}
		}

		float[][]	ComputeHeatDiffusionFull( MathSolvers.MatrixF _laplacian, float _diffusionCoefficient, float _timeStep, uint _iterationsCount ) {
			uint	nodesCount = _laplacian.ColumnsCount;

			// 1] Compute eigen vectors using singular value decomposition
			//
			MathSolvers.SVD		SVD = new MathSolvers.SVD( _laplacian );
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


			// 2] Apply heat to node 0 and compute diffusion
			//
			float[][]	results = new float[_iterationsCount][];

			float[]		heatsSource = new float[nodesCount];
			float[]		eigenHeatsSource = new float[nodesCount];
			float[]		eigenHeatsTarget = new float[nodesCount];

			heatsSource[0] = 1;

			// 3.1) Transform heat vector into eigen-space: Phi' = trans(V) * Phi
			for ( int i=0; i < nodesCount; i++ ) {
				float	sum = 0.0f;
				for ( int j=0; j < nodesCount; j++ )
					sum += eigenVectors[j,i] *  heatsSource[j];
				eigenHeatsSource[i] = sum;
			}

			// 3.2) Apply diffusion iteratively (note that we could obviously reach the desired time immediately,
			//		there's no use for iterative computation here but it's only there to compare against Euler integration)
			//
			for ( uint iterationIndex=0; iterationIndex < _iterationsCount; iterationIndex++ ) {
				float	time = _timeStep * (iterationIndex+1);
				for ( int i=0; i < nodesCount; i++ ) {
					float	lambda = eigenValues[i];
					float	phi_0 = eigenHeatsSource[i];
					float	phi_t = phi_0 * Mathf.Exp( -_diffusionCoefficient * lambda * time );
					eigenHeatsTarget[i] = phi_t;
				}

				// Transform eigen-heat vector back into graph-space: Phi = V * Phi'
				float[]	heatsTarget = new float[nodesCount];
				float	totalHeat = 0.0f;	// This should equal to initial heat, no loss!
				for ( int i=0; i < nodesCount; i++ ) {
					float	sum = 0.0f;
					for ( int j=0; j < nodesCount; j++ )
						sum += eigenVectors[i,j] * eigenHeatsTarget[j];
					heatsTarget[i] = sum;
					totalHeat += sum;
				}

				// Copy each iteration
				results[iterationIndex] = heatsTarget;
			}

			return results;
		}

		float[][]	ComputeHeatDiffusionEuler( MathSolvers.MatrixF _laplacian, float _diffusionCoefficient, float _timeStep, uint _iterationsCount ) {
			uint	nodesCount = _laplacian.ColumnsCount;

// 			// 1] Pre-compute many little laplacian matrices for each node
// 			MathSolvers.MatrixF[]	laplacians = new MathSolvers.MatrixF[nodesCount];
// 			MathSolvers.MatrixF[]	eigenVectorss = new MathSolvers.MatrixF[nodesCount];
// 			MathSolvers.VectorF[]	eigenValuess = new MathSolvers.VectorF[nodesCount];
// 			for ( uint nodeIndex=0; nodeIndex < nodesCount; nodeIndex++ ) {
// 
// 				// 1.1) Count the amount of neighbors
// 				uint	j = 0;
// 				uint	neighborsCount = 0;
// 				for ( ; j < nodeIndex; j++ )
// 					if ( _laplacian[nodeIndex,j] != 0.0f )
// 						neighborsCount++;
// 				for ( j++; j < nodesCount; j++ )
// 					if ( _laplacian[nodeIndex,j] != 0.0f )
// 						neighborsCount++;
// 
// 				// 1.2) Create a tiny matrix for the current node and its neighbors only
// 				MathSolvers.MatrixF	laplacian = new MathSolvers.MatrixF( 1+neighborsCount, 1+neighborsCount );
// 				laplacians[nodeIndex] = laplacian;
// 				laplacian[0,0] = neighborsCount;	// Write degree for central node
// 				for ( j=0; j < neighborsCount; j++ ) {
// 					laplacian[j,j] = 1;				// Connected to central node only
// 					laplacian[0,j] = -1;
// 					laplacian[j,0] = -1;
// 				}
// 
// 				// 1.3) Compute eigen vectors for the tiny matrix
// 				MathSolvers.SVD	SVD = new MathSolvers.SVD( laplacian );
// 				SVD.Decompose();
// 				eigenVectorss[nodeIndex] = SVD.U;
// 				eigenValuess[nodeIndex] = SVD.w;
// 			}

			// 1] Precompute neighbor indices for each node
			List<uint>	tempNeighborIndices = new List<uint>( (int) nodesCount );
			uint[][]	neighborIndicess = new uint[nodesCount][];
			for ( uint nodeIndex=0; nodeIndex < nodesCount; nodeIndex++ ) {

				tempNeighborIndices.Clear();

				// Collect neighbor indices
				uint	j = 0;
				for ( ; j < nodeIndex; j++ )
					if ( _laplacian[nodeIndex,j] != 0.0f )
						tempNeighborIndices.Add( j );
				for ( j++; j < nodesCount; j++ )
					if ( _laplacian[nodeIndex,j] != 0.0f )
						tempNeighborIndices.Add( j );

				neighborIndicess[nodeIndex] = tempNeighborIndices.ToArray();
			}

			// 2] Apply diffusion iteratively using Euler method
			float[][]	results = new float[_iterationsCount][];

			float[][]	heats = new float[2][] {
				new float[nodesCount],
				new float[nodesCount]
			};
			heats[0][0] = 1.0f;

			float	diffusionCoefficient = _diffusionCoefficient * _timeStep;

			float[]	tempVector = new float[nodesCount];
			for ( uint iterationIndex=0; iterationIndex < _iterationsCount; iterationIndex++ ) {

				float[]	sourceHeats = heats[0];
				float[]	targetHeats = heats[1];

				for ( uint nodeIndex=0; nodeIndex < nodesCount; nodeIndex++ ) {
					uint[]	neighborNodeIndices = neighborIndicess[nodeIndex];
					uint	neighborsCount = (uint) neighborNodeIndices.Length;

					float	sourceHeat = sourceHeats[nodeIndex];

					// Compute laplacian for this node
					float	laplacian = -neighborsCount * sourceHeat;
					for ( uint neighborIndex=0; neighborIndex < neighborsCount; neighborIndex++ ) {
						uint	neighborNodeIndex = neighborNodeIndices[neighborIndex];
						laplacian += sourceHeats[neighborNodeIndex];
					}

					// Apply small heat diffusion
					targetHeats[nodeIndex] = sourceHeat + diffusionCoefficient * laplacian;
				}

				// Swap heat buffers
				float[]	temp = heats[0];
				heats[0] = heats[1];
				heats[1] = temp;

				// Copy each iteration
				float[]	result = new float[nodesCount];
				Array.Copy( heats[0], result, nodesCount );
				results[iterationIndex] = result;
			}

			return results;
		}

		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if ( disposing && (components != null) )
			{
				components.Dispose();

				m_tex_FalseColors.Dispose();

				m_tex_Obstacles_Staging.Dispose();
				m_tex_Obstacles1.Dispose();
				m_tex_Obstacles0.Dispose();
				m_tex_HeatMap1.Dispose();
				m_tex_HeatMap0.Dispose();
//				m_tex_HeatMap_Staging.Dispose();
				m_shader_DrawObstacles.Dispose();
				m_shader_HeatDiffusion.Dispose();
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

		private void button1_Click( object sender, EventArgs e ) {
			m_device.Clear( m_tex_HeatMap0, float4.Zero );
		}

		private void buttonResetObstacles_Click( object sender, EventArgs e ) {
			m_tex_Obstacles0.CopyFrom( m_tex_Obstacles_Staging );
		}

		private void checkBox1_CheckedChanged( object sender, EventArgs e ) {
			timer1.Enabled = checkBoxRun.Checked;
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		#endregion
	}
}
