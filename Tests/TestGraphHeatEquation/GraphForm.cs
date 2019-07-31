using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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
			public uint			flags;
			public uint			sourceIndex;
		}

		#endregion

		#region FIELDS

		Device		m_device = new Device();

		private ConstantBuffer<CB_Main>	m_CB_Main = null;
		private Shader					m_shader_RenderHeatMap = null;
		private Shader					m_shader_HeatDiffusion0 = null;
		private Shader					m_shader_HeatDiffusion1 = null;
		private Shader					m_shader_DrawObstacles = null;
		private Texture2D				m_tex_HeatMap_Staging = null;
		private Texture2D				m_tex_HeatMap0 = null;
		private Texture2D				m_tex_HeatMap1 = null;
		private Texture2D				m_tex_Obstacles0 = null;
		private Texture2D				m_tex_Obstacles1 = null;
		private Texture2D				m_tex_Obstacles_Staging = null;

		private Texture2D				m_tex_Search = null;
		private Texture2D				m_tex_Search_Staging = null;

		private Texture2D				m_tex_FalseColors0 = null;
		private Texture2D				m_tex_FalseColors1 = null;

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

			m_shader_HeatDiffusion0 = new Shader( m_device, new FileInfo( "./Shaders/HeatDiffusion.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			m_shader_HeatDiffusion1 = new Shader( m_device, new FileInfo( "./Shaders/HeatDiffusion2.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			m_shader_RenderHeatMap = new Shader( m_device, new FileInfo( "./Shaders/RenderHeatMap.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
			m_shader_DrawObstacles = new Shader( m_device, new FileInfo( "./Shaders/DrawObstacles.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

			m_tex_HeatMap_Staging = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, true, false, null );
			m_tex_HeatMap0 = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_HeatMap1 = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );
			m_tex_Obstacles0 = new Texture2D( m_device, (uint) GRAPH_SIZE + 2, (uint) GRAPH_SIZE + 2, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_Obstacles1 = new Texture2D( m_device, (uint) GRAPH_SIZE + 2, (uint) GRAPH_SIZE + 2, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_Obstacles_Staging = new Texture2D( m_device, (uint) GRAPH_SIZE + 2, (uint) GRAPH_SIZE + 2, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, true, false, null );
			ClearObstacles();
			buttonResetObstacles_Click( null, EventArgs.Empty );

			m_tex_Search = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
			m_tex_Search_Staging = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, true, false, null );

			// Load false colors
			using ( ImageUtility.ImageFile sourceImage = new ImageUtility.ImageFile( new FileInfo( "../../Images/Gradients/Magma.png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG ) ) {
				ImageUtility.ImageFile convertedImage = new ImageUtility.ImageFile();
				convertedImage.ConvertFrom( sourceImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
				using ( ImageUtility.ImagesMatrix image = new ImageUtility.ImagesMatrix( convertedImage, ImageUtility.ImagesMatrix.IMAGE_TYPE.sRGB ) )
					m_tex_FalseColors0 = new Texture2D( m_device, image, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
			}
			using ( ImageUtility.ImageFile sourceImage = new ImageUtility.ImageFile( new FileInfo( "../../Images/Gradients/Viridis.png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG ) ) {
				ImageUtility.ImageFile convertedImage = new ImageUtility.ImageFile();
				convertedImage.ConvertFrom( sourceImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
				using ( ImageUtility.ImagesMatrix image = new ImageUtility.ImagesMatrix( convertedImage, ImageUtility.ImagesMatrix.IMAGE_TYPE.sRGB ) )
					m_tex_FalseColors1 = new Texture2D( m_device, image, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
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
//											| (((Control.MouseButtons & MouseButtons.Middle) != 0) ? 2 : 0)
											| (m_plotSource ? 2 : 0)
											| (((Control.MouseButtons & MouseButtons.Right) != 0) ? 4 : 0)
											| (Control.ModifierKeys == Keys.Shift ? 8 : 0));
			m_CB_Main.m.deltaTime = floatTrackbarControlDeltaTime.Value;
			m_CB_Main.m.diffusionCoefficient = floatTrackbarControlDiffusionCoefficient.Value;
			m_CB_Main.m.flags = (uint) (
									  (checkBoxShowSearch.Checked ? 1 : 0)
									| (radioButtonShowLogHeat.Checked ? 2 : 0)
									| (radioButtonShowVoronoi.Checked ? 4 : 0)
									| (radioButtonShowLaplacian.Checked ? 6 : 0)

									| (radioButtonShowField1.Checked ? 8 : 0)
									| (radioButtonShowSumFields.Checked ? 16 : 0)
								);
			m_CB_Main.m.sourceIndex = (uint) m_simulationHotSpots.Count;	// Always offset by 1 so first source ID=1
			m_CB_Main.UpdateData();

			m_plotSource = false;

			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			//////////////////////////////////////////////////////////////////////////
			// Draw Obstacles
			if ( m_shader_DrawObstacles.Use() ) {
				m_device.SetRenderTarget( m_tex_Obstacles1, null );
				m_tex_Obstacles0.SetPS( 0 );
				m_device.RenderFullscreenQuad( m_shader_DrawObstacles );

				// Swap
				Texture2D	temp = m_tex_Obstacles0;
				m_tex_Obstacles0 = m_tex_Obstacles1;
				m_tex_Obstacles1 = temp;
			}

			//////////////////////////////////////////////////////////////////////////
			// Perform heat diffusion test
			Shader	S = radioButtonDiffusionAlgo0.Checked ? m_shader_HeatDiffusion0 : m_shader_HeatDiffusion1;
			if ( checkBoxRun.Checked && S.Use() ) {
				m_device.SetRenderTarget( m_tex_HeatMap1, null );

				m_tex_HeatMap0.SetPS( 0 );
				m_tex_Obstacles0.SetPS( 1 );

				m_device.RenderFullscreenQuad( S );

				// Swap
				Texture2D	temp = m_tex_HeatMap0;
				m_tex_HeatMap0 = m_tex_HeatMap1;
				m_tex_HeatMap1 = temp;
			}

			//////////////////////////////////////////////////////////////////////////
			// Render
			if ( m_shader_RenderHeatMap.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_HeatMap0.SetPS( 0 );
				m_tex_Obstacles0.SetPS( 1 );
				if ( radioButtonShowLogHeat.Checked )
					m_tex_FalseColors1.SetPS( 2 );
				else
					m_tex_FalseColors0.SetPS( 2 );
				m_tex_Search.SetPS( 3 );

				m_device.RenderFullscreenQuad( m_shader_RenderHeatMap );
			}

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

				m_tex_FalseColors1.Dispose();
				m_tex_FalseColors0.Dispose();

				m_tex_Search_Staging.Dispose();
				m_tex_Search.Dispose();
				m_tex_Obstacles_Staging.Dispose();
				m_tex_Obstacles1.Dispose();
				m_tex_Obstacles0.Dispose();
				m_tex_HeatMap1.Dispose();
				m_tex_HeatMap0.Dispose();
				m_tex_HeatMap_Staging.Dispose();
				m_shader_DrawObstacles.Dispose();
				m_shader_HeatDiffusion1.Dispose();
				m_shader_HeatDiffusion0.Dispose();
				m_shader_RenderHeatMap.Dispose();
				m_CB_Main.Dispose();

				Device	temp = m_device;
				m_device = null;
				temp.Dispose();
			}
			base.Dispose( disposing );
		}

		void	ClearObstacles() {
			m_tex_Obstacles_Staging.WritePixels( 0, 0, ( uint _X, uint _Y, BinaryWriter W ) => {
				bool	obstacle = _X == 0 || _Y == 0 || _X == GRAPH_SIZE+1 || _Y == GRAPH_SIZE+1;
				if ( obstacle )
					W.Write( 0x000000FFU );
				else
					W.Write( 0x00000000U );
			} );
		}

		#endregion

		#region EVENT HANDLERS

		private void buttonReset_Click( object sender, EventArgs e ) {
			m_device.Clear( m_tex_HeatMap0, float4.Zero );
		}

		private void buttonResetObstacles_Click( object sender, EventArgs e ) {
			m_tex_Obstacles0.CopyFrom( m_tex_Obstacles_Staging );
			m_simulationHotSpots.Clear();
			groupBoxSearch.Enabled = false;
		}

		private void checkBox1_CheckedChanged( object sender, EventArgs e ) {
//			timer1.Enabled = checkBoxRun.Checked;
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		#region Simulation

		List< Point >	m_simulationHotSpots = new List<Point>();
		int				m_simulationIteration = -1;

		float4[,]		m_bufferHeat = new float4[GRAPH_SIZE,GRAPH_SIZE];
		uint[,]			m_bufferSearchResults = new uint[GRAPH_SIZE,GRAPH_SIZE];

		int[][]	dXY = new int[8][] {
			new int[] { -1, -1 },
			new int[] {  0, -1 },
			new int[] { +1, -1 },
			new int[] { -1,  0 },
//			new int[] {  0,  0 },
			new int[] { +1,  0 },
			new int[] { -1, +1 },
			new int[] {  0, +1 },
			new int[] { +1, +1 },
		};

		// Simulation data
		enum SEARCH_PHASE {
			TARGET_ID_SEARCH,
			TARGET_ASCENT,
			SOURCE_DESCENT,
			FINISHED
		}

		SEARCH_PHASE	m_searchPhase;
		int				m_simulationX, m_simulationY;
		float			m_simulationRadius = 0.0f;
		bool[,]			m_visited = new bool[GRAPH_SIZE,GRAPH_SIZE];

#if true
		uint			m_targetID;
		float			m_currentGradient;

		private void buttonStepSimulation_Click( object sender, EventArgs e ) {
			if ( m_simulationIteration < 0 ) {
				buttonResetSimulation_Click( sender, e );
			}

			m_simulationIteration++;

			//////////////////////////////////////////////////////////////////////////
			// Local search: We start by finding the target ID nearby our start location and ascend the gradient, all the while descending the source gradient
			float4	current = m_bufferHeat[m_simulationX,m_simulationY];

			switch ( m_searchPhase ) {
				case SEARCH_PHASE.TARGET_ID_SEARCH: {
					// At the moment, assume we're standing on a node with the target ID
					m_targetID = (uint) current.w - 1;	// Keep current secondary ID as target ID
					m_currentGradient = current.x + current.z;

					for ( uint neighborIndex=0; neighborIndex < 8; neighborIndex++ ) {
						int	tempX = m_simulationX + dXY[neighborIndex][0];
						if ( tempX < 0 || tempX >= GRAPH_SIZE )
							continue;
						int	tempY = m_simulationY + dXY[neighborIndex][1];
						if ( tempY < 0 || tempY >= GRAPH_SIZE )
							continue;

						float4	neighborValue = m_bufferHeat[tempX,tempY];
						uint	neighborID = (uint) neighborValue.w - 1;
						if ( neighborID != integerTrackbarControlTargetPosition.Value && m_targetID != 0 )
							continue;	// Not our target ID

						m_simulationX = tempX;
						m_simulationY = tempY;
					}

					m_searchPhase = SEARCH_PHASE.TARGET_ASCENT;

				} break;

				case SEARCH_PHASE.TARGET_ASCENT: {
					// We're descending the source gradient (in field #0) and ascending the target gradient (in field #1)
					float	bestGradient = -1;
					int		bestX = m_simulationX, bestY = m_simulationY;
					for ( uint neighborIndex=0; neighborIndex < 8; neighborIndex++ ) {
						int	tempX = m_simulationX + dXY[neighborIndex][0];
						if ( tempX < 0 || tempX >= GRAPH_SIZE )
							continue;
						int	tempY = m_simulationY + dXY[neighborIndex][1];
						if ( tempY < 0 || tempY >= GRAPH_SIZE )
							continue;

						if ( m_visited[tempX,tempY] )
							continue;	// Don't bother

						float4	neighborValue = m_bufferHeat[tempX,tempY];
						float	gradient = 0*neighborValue.x + neighborValue.z;
						if ( gradient < bestGradient )
							continue;	// Not the ridge!

						bestGradient = gradient;
						bestX = tempX;
						bestY = tempY;
					}

					m_simulationX = bestX;
					m_simulationY = bestY;

					// Write a single pixel
					m_bufferSearchResults[m_simulationX,m_simulationY] = 0x000000FFU;
					m_visited[m_simulationX,m_simulationY] = true;

					// Check we're still following the target ID
					float4	newValue = m_bufferHeat[m_simulationX,m_simulationY];
					uint	newID = (uint) newValue.w - 1;
					if ( newID != m_targetID ) {
						m_targetID = newID;
						m_searchPhase = SEARCH_PHASE.SOURCE_DESCENT;	// New phase!
					}

				} break;

				case SEARCH_PHASE.SOURCE_DESCENT: {
					// We're descending the source gradient (in field #1) and ascending the target gradient (in field #0)
					float	bestGradient = -1;
					int		bestX = m_simulationX, bestY = m_simulationY;
					for ( uint neighborIndex=0; neighborIndex < 8; neighborIndex++ ) {
						int	tempX = m_simulationX + dXY[neighborIndex][0];
						if ( tempX < 0 || tempX >= GRAPH_SIZE )
							continue;
						int	tempY = m_simulationY + dXY[neighborIndex][1];
						if ( tempY < 0 || tempY >= GRAPH_SIZE )
							continue;

						if ( m_visited[tempX,tempY] )
							continue;	// Don't bother

						float4	neighborValue = m_bufferHeat[tempX,tempY];
						float	gradient = neighborValue.x + 0*neighborValue.z;
						if ( gradient < bestGradient )
							continue;	// Not the ridge!

						bestGradient = gradient;
						bestX = tempX;
						bestY = tempY;
					}

					m_simulationX = bestX;
					m_simulationY = bestY;

					// Write a single pixel
					m_bufferSearchResults[m_simulationX,m_simulationY] = 0x000000FFU;
					m_visited[m_simulationX,m_simulationY] = true;

					// Check we're still following the target ID
					float4	newValue = m_bufferHeat[m_simulationX,m_simulationY];
					uint	newID = (uint) newValue.w - 1;
					if ( newID != m_targetID )
						m_searchPhase = SEARCH_PHASE.FINISHED;	// We're done!

				} break;
			}

			// Update search results texture
			if ( sender != null ) {
				UpdateSearchResults();
			}
		}

#else
		private void buttonStepSimulation_Click( object sender, EventArgs e ) {
			if ( m_simulationIteration < 0 ) {
				buttonResetSimulation_Click( sender, e );
			}

			m_simulationIteration++;

			if ( radioButtonSearchAlgo0.Checked ) {
				//////////////////////////////////////////////////////////////////////////
				// Local search: Select second highest value from last search position
				float	largestValue = 0.0f;
				float	secondLargestValue = 0.0f;
				int		sX = m_simulationX, sY = m_simulationY;
				int		X = m_simulationX, Y = m_simulationY;

				for ( int i=0; i < 8; i++ ) {
					int	tempX = m_simulationX + dXY[i][0];
					int	tempY = m_simulationY + dXY[i][1];
					if ( tempX < 0 || tempX >= GRAPH_SIZE )
						continue;
					if ( tempY < 0 || tempY >= GRAPH_SIZE )
						continue;
					if ( m_visited[tempX,tempY] )
						continue;	// Already visited

					float	value = m_bufferHeat[tempX,tempY].x;
					if ( value <= largestValue )
						continue;

					sX = X; sY = Y;
					secondLargestValue = largestValue;
					X = tempX; Y = tempY;
					largestValue = value;
				}

//sX = X; sY = Y;

				// Write a single pixel
				m_bufferSearchResults[sX,sY] = 0x000000FFU;
				m_visited[sX,sY] = true;
m_visited[X,Y] = true;	// Also mark maximum as visited to avoid getting trapped

				m_simulationX = sX;
				m_simulationY = sY;

			} else if ( radioButtonSearchAlgo1.Checked ) {
				//////////////////////////////////////////////////////////////////////////
				// Global search: the search radius is increased and we examine all pixels within the radius and select the one with the highest value
				m_simulationRadius += 1.0f;

				Point	center = m_simulationHotSpots[integerTrackbarControlStartPosition.Value];

				// Walk the circle and find the largest value
				float	largestValue = 0.0f;
				int		X = 0, Y = 0;
				uint	pixelsCount = (uint) Mathf.Ceiling( Mathf.TWOPI * m_simulationRadius );
				for ( uint pixelIndex=0; pixelIndex < pixelsCount; pixelIndex++ ) {
					float	angle = pixelIndex * Mathf.TWOPI / pixelsCount;
					int		tempX = center.X + (int) Mathf.Floor( m_simulationRadius * Mathf.Cos( angle ) );
					int		tempY = center.Y + (int) Mathf.Floor( m_simulationRadius * Mathf.Sin( angle ) );
					if ( tempX < 0 || tempX >= GRAPH_SIZE )
						continue;
					if ( tempY < 0 || tempY >= GRAPH_SIZE )
						continue;

					float	heat = m_bufferHeat[tempX,tempY].x;
					if ( heat <= largestValue )
						continue;

					largestValue = heat;
					X = tempX;
					Y = tempY;
				}

				// Write a single pixel
				m_bufferSearchResults[X,Y] = 0x000000FFU;
			}

			// Update search results texture
			if ( sender != null ) {
				UpdateSearchResults();
			}
		}
#endif

		void	ReadBackHeat() {
			m_tex_HeatMap_Staging.CopyFrom( m_tex_HeatMap0 );
// 			m_bufferHeat = m_tex_HeatMap_Staging.MapRead( 0, 0 );
// 			m_bufferHeatReader = m_bufferHeat.OpenStreamRead();
			m_tex_HeatMap_Staging.ReadPixels( 0, 0, ( uint _X, uint _Y, BinaryReader R ) => { m_bufferHeat[_X,_Y].Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() ); } );
		}

		void	UpdateSearchResults() {
			m_tex_Search_Staging.WritePixels( 0, 0, ( uint _X, uint _Y, BinaryWriter W ) => {
				W.Write( m_bufferSearchResults[_X,_Y] );
			} );
			m_tex_Search.CopyFrom( m_tex_Search_Staging );
		}

		private void integerTrackbarControlStartPosition_ValueChanged( IntegerTrackbarControl _Sender, int _FormerValue ) {
			bool	simulable = integerTrackbarControlStartPosition.Value != integerTrackbarControlTargetPosition.Value;
			buttonStepSimulation.Enabled = simulable;
			buttonRunSimulation.Enabled = simulable;
		}

		private void integerTrackbarControlTargetPosition_ValueChanged( IntegerTrackbarControl _Sender, int _FormerValue ) {
			bool	simulable = integerTrackbarControlStartPosition.Value != integerTrackbarControlTargetPosition.Value;
			buttonStepSimulation.Enabled = simulable;
			buttonRunSimulation.Enabled = simulable;
		}

		private void buttonResetSimulation_Click( object sender, EventArgs e ) {
			m_simulationIteration = 0;
			ReadBackHeat();

			m_searchPhase = SEARCH_PHASE.TARGET_ID_SEARCH;
			m_simulationRadius = 0.0f;
			if ( m_simulationHotSpots.Count > 0 ) {
				m_simulationX = m_simulationHotSpots[integerTrackbarControlStartPosition.Value].X;
				m_simulationY = m_simulationHotSpots[integerTrackbarControlStartPosition.Value].Y;
			}
			Array.Clear( m_visited, 0, GRAPH_SIZE*GRAPH_SIZE );

			// Reset search result
			Array.Clear( m_bufferSearchResults, 0, GRAPH_SIZE*GRAPH_SIZE );
			UpdateSearchResults();
		}

		private void buttonRunSimulation_Click( object sender, EventArgs e ) {
			int	iterationsCount = ((int) Mathf.Sqrt(2) * GRAPH_SIZE);
			for ( int i=0; i < iterationsCount; i++ )
				buttonStepSimulation_Click( null, e );

			UpdateSearchResults();
		}

		bool	m_plotSource = false;
		private void panelOutput_MouseDown( object sender, MouseEventArgs e ) {
			if ( e.Button == MouseButtons.Middle ) {
				// Add a new hotspot
				AddHotSpot( new Point( e.X * GRAPH_SIZE / panelOutput.Width, e.Y * GRAPH_SIZE / panelOutput.Height ) );

				// Authorize source plotting ONLY when we successfully registered a new point
				m_plotSource = true;
			}
		}

		void	AddHotSpot( Point _hotSpotLocation ) {
			m_simulationHotSpots.Add( _hotSpotLocation );

			groupBoxSearch.Enabled = m_simulationHotSpots.Count > 1;

			integerTrackbarControlStartPosition.RangeMax = m_simulationHotSpots.Count - 1;
			integerTrackbarControlStartPosition.VisibleRangeMax = integerTrackbarControlStartPosition.RangeMax;
			integerTrackbarControlStartPosition.Value = 0;

			integerTrackbarControlTargetPosition.RangeMax = m_simulationHotSpots.Count - 1;
			integerTrackbarControlTargetPosition.VisibleRangeMax = integerTrackbarControlTargetPosition.RangeMax;
			integerTrackbarControlTargetPosition.Value = 1;
		}

		#endregion

		#region I/O

		private void buttonLoad_Click( object sender, EventArgs e ) {
//			openFileDialog1.InitialDirectory = Path.GetDirectoryName( Application.ExecutablePath );
			if ( openFileDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			buttonResetObstacles_Click( null, e );

			uint[]		obstacles = new uint[(GRAPH_SIZE+2)*(GRAPH_SIZE+2)];
			float4[,]	simulatedValues = new float4[GRAPH_SIZE,GRAPH_SIZE];

			FileInfo	file = new FileInfo( openFileDialog1.FileName );
			using ( FileStream S = file.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {
					for ( uint i=0; i < obstacles.Length; i++ ) {
						obstacles[i] = R.ReadUInt32();
						if ( (obstacles[i] & 0x00FF0000) != 0 )
							AddHotSpot( new Point( (int) i % (GRAPH_SIZE+2), (int) i / (GRAPH_SIZE+2) ) );
					}
					for ( uint Y=0; Y < GRAPH_SIZE; Y++ )
						for ( uint X=0; X < GRAPH_SIZE; X++ )
							simulatedValues[X,Y].Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
				}

			m_tex_Obstacles_Staging.WritePixels( 0, 0, ( uint _X, uint _Y, BinaryWriter W ) => {
				W.Write( obstacles[(GRAPH_SIZE+2)*_Y+_X] );
			} );
			m_tex_Obstacles0.CopyFrom( m_tex_Obstacles_Staging );

			m_tex_HeatMap_Staging.WritePixels( 0, 0, ( uint _X, uint _Y, BinaryWriter W ) => {
				W.Write( simulatedValues[_X,_Y].x );
				W.Write( simulatedValues[_X,_Y].y );
				W.Write( simulatedValues[_X,_Y].z );
				W.Write( simulatedValues[_X,_Y].w );
			} );
			m_tex_HeatMap0.CopyFrom( m_tex_HeatMap_Staging );

			checkBoxRun.Checked = false;	// Avoid running the freshly-loaded simulation!
			ClearObstacles();
		}

		private void buttonSave_Click( object sender, EventArgs e ) {
//			saveFileDialog1.InitialDirectory = Path.GetDirectoryName( Application.ExecutablePath );
			if ( saveFileDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			uint[]	obstacles = new uint[(GRAPH_SIZE+2)*(GRAPH_SIZE+2)];
			m_tex_Obstacles_Staging.CopyFrom( m_tex_Obstacles0 );
			m_tex_Obstacles_Staging.ReadPixels( 0, 0, ( uint _X, uint _Y, BinaryReader R ) => {
				obstacles[(GRAPH_SIZE+2)*_Y+_X] = R.ReadUInt32();
			} );

			float4[,]	simulatedValues = new float4[GRAPH_SIZE,GRAPH_SIZE];
			m_tex_HeatMap_Staging.CopyFrom( m_tex_HeatMap0 );
			m_tex_HeatMap_Staging.ReadPixels( 0, 0, ( uint _X, uint _Y, BinaryReader R ) => {
				simulatedValues[_X,_Y].Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
			} );

			FileInfo	file = new FileInfo( saveFileDialog1.FileName );
			using ( FileStream S = file.OpenWrite() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					for ( uint i=0; i < obstacles.Length; i++ )
						W.Write( obstacles[i] );
					for ( uint Y=0; Y < GRAPH_SIZE; Y++ )
						for ( uint X=0; X < GRAPH_SIZE; X++ ) {
							W.Write( simulatedValues[X,Y].x );
							W.Write( simulatedValues[X,Y].y );
							W.Write( simulatedValues[X,Y].z );
							W.Write( simulatedValues[X,Y].w );
						}
				}

			ClearObstacles();
		}

		#endregion

		#endregion
	}
}
