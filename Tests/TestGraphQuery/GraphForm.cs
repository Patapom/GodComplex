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
using UIUtility;

namespace TestGraphQuery
{
	public partial class GraphForm : Form
	{
		#region CONSTANTS

		const uint	MAX_QUERY_SOURCES = 64;

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public uint			_nodesCount;
			public uint			_sourcesCount;
			public uint			_resX;
			public uint			_resY;

			public float2		_cameraCenter;
			public float2		_cameraSize;

			public float		_diffusionCoefficient;
			public uint			_sourceIndex;
			public uint			_hoveredNodeIndex;
			public uint			_renderFlags;

			public float		_barycentricDistanceTolerance;
			public float		_barycentricBias;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Text {
			public float2		_position;
			public float2		_right;
			public float2		_up;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct SB_NodeInfo {
			public float2		m_position;
			public uint			m_flags;
			public uint			m_linkOffset;
			public uint			m_linksCount;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct SB_Letter {
			public uint			m_letterIndex;
			public float		m_offset;
			public float		m_ratio;
		}

		#endregion

		#region FIELDS

		Device		m_device = new Device();

		private ConstantBuffer<CB_Main>			m_CB_Main = null;
		private ConstantBuffer<CB_Text>			m_CB_Text = null;

		private ComputeShader					m_compute_HeatDiffusion = null;
		private ComputeShader					m_compute_SplatHeatSources = null;

		private Shader							m_shader_RenderGraphNode = null;
		private Shader							m_shader_RenderGraphLink = null;
		private Shader							m_shader_RenderText = null;

		private StructuredBuffer<SB_NodeInfo>	m_SB_Nodes = null;
		private StructuredBuffer<uint>			m_SB_LinkSources = null;
		private StructuredBuffer<uint>			m_SB_LinkTargets = null;

		// Heatwave simulation
		private StructuredBuffer<uint>			m_SB_SourceIndices = null;
		private StructuredBuffer<float>			m_SB_HeatSource = null;
		private StructuredBuffer<float>			m_SB_HeatTarget = null;

		private bool							m_barycentricsDirty = true;
		private StructuredBuffer<float>			m_SB_HeatBarycentrics = null;

		private bool							m_sumDirty = true;
		private StructuredBuffer<float>			m_SB_HeatSum = null;

		// Text display
		private int[]							m_char2Index = new int[512];
		private Rectangle[]						m_fontRectangles = null;
 		private Texture2D						m_tex_FontAtlas = null;
 		private Texture2D						m_tex_FontRectangle = null;
		private StructuredBuffer<SB_Letter>		m_SB_Text = null;

		// Graph
		private ProtoParser.Graph				m_graph = null;
		private uint							m_nodesCount = 0;
		private uint							m_totalLinksCount = 0;
		private Dictionary< ProtoParser.Neuron, uint >	m_neuron2ID = null;

		private Texture2D						m_tex_FalseColors0 = null;
		private Texture2D						m_tex_FalseColors1 = null;

		#endregion

		#region METHODS

		public GraphForm() {
// UnitTestPacking();

			InitializeComponent();
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

			//////////////////////////////////////////////////////////////////////////
			m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
			m_CB_Text = new ConstantBuffer<CB_Text>( m_device, 2 );

			m_compute_HeatDiffusion = new ComputeShader( m_device, new FileInfo( "./Shaders/HeatDiffusion.hlsl" ), "CS" );
			m_compute_SplatHeatSources = new ComputeShader( m_device, new FileInfo( "./Shaders/HeatDiffusion.hlsl" ), "CS2" );

			m_shader_RenderGraphNode = new Shader( m_device, new FileInfo( "./Shaders/RenderGraph.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
			m_shader_RenderGraphLink = new Shader( m_device, new FileInfo( "./Shaders/RenderGraph.hlsl" ), VERTEX_FORMAT.Pt4, "VS2", null, "PS2" );
			m_shader_RenderText = new Shader( m_device, new FileInfo( "./Shaders/RenderText.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS"  );


			//////////////////////////////////////////////////////////////////////////
			// Load the graph we need to run
			m_graph = new ProtoParser.Graph();

			// Load graph
			using ( FileStream S = new FileInfo( "./Graphs/Birds.graph" ).OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) )
					m_graph.Read( R );

			// Load graph node positions (simulated and saved by the TestGraphViz project)
			float2[]	graphNodePositions = null;
			using ( FileStream S = new FileInfo( "./Graphs/Birds.graphpos" ).OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {
					float2	BBoxMin = new float2( R.ReadSingle(), R.ReadSingle() );
					float2	BBoxMax = new float2( R.ReadSingle(), R.ReadSingle() );

					m_CB_Main.m._cameraCenter = 0.5f * (BBoxMin + BBoxMax);
					m_CB_Main.m._cameraSize = BBoxMax - BBoxMin;

					int	nodesCount = R.ReadInt32();
					if ( nodesCount != m_graph.NeuronsCount )
						throw new Exception( "Graph nodes count mismatch!" );

					graphNodePositions = new float2[nodesCount];
					for ( int i=0; i < nodesCount; i++ ) {
						graphNodePositions[i].Set( R.ReadSingle(), R.ReadSingle() );
					}
				}

			ProtoParser.Neuron[]	neurons = m_graph.Neurons;
			m_nodesCount = (uint) neurons.Length;

			// Build node info
			m_SB_Nodes = new StructuredBuffer<SB_NodeInfo>( m_device, m_nodesCount, true );

			m_neuron2ID = new Dictionary<ProtoParser.Neuron, uint>( neurons.Length );

			for ( int neuronIndex=0; neuronIndex < m_nodesCount; neuronIndex++ ) {
				ProtoParser.Neuron	N = neurons[neuronIndex];
				m_neuron2ID[N] = (uint) neuronIndex;

				uint	linksCount = (uint) (N.ParentsCount + N.ChildrenCount + N.FeaturesCount);
				m_SB_Nodes.m[neuronIndex].m_position = graphNodePositions[neuronIndex];
				m_SB_Nodes.m[neuronIndex].m_linkOffset = m_totalLinksCount;
				m_SB_Nodes.m[neuronIndex].m_linksCount = linksCount;
				m_SB_Nodes.m[neuronIndex].m_flags = 0U;

				m_totalLinksCount += linksCount;
			}

			m_SB_Nodes.Write();

			// Build node links
			m_SB_LinkTargets = new StructuredBuffer<uint>( m_device, m_totalLinksCount, true );
			m_SB_LinkSources = new StructuredBuffer<uint>( m_device, m_totalLinksCount, true );
			m_totalLinksCount = 0;
			for ( int neuronIndex=0; neuronIndex < m_nodesCount; neuronIndex++ ) {
				ProtoParser.Neuron	N = neurons[neuronIndex];
				foreach ( ProtoParser.Neuron O in N.Parents ) {
					m_SB_LinkSources.m[m_totalLinksCount] = (uint) neuronIndex;
					m_SB_LinkTargets.m[m_totalLinksCount++] = m_neuron2ID[O];
				}
				foreach ( ProtoParser.Neuron O in N.Children ) {
					m_SB_LinkSources.m[m_totalLinksCount] = (uint) neuronIndex;
					m_SB_LinkTargets.m[m_totalLinksCount++] = m_neuron2ID[O];
				}
				foreach ( ProtoParser.Neuron O in N.Features ) {
					m_SB_LinkSources.m[m_totalLinksCount] = (uint) neuronIndex;
					m_SB_LinkTargets.m[m_totalLinksCount++] = m_neuron2ID[O];
				}
			}
			m_SB_LinkTargets.Write();
			m_SB_LinkSources.Write();


			// Build heat buffers
			uint	elementsCount = m_nodesCount * MAX_QUERY_SOURCES;
			m_SB_SourceIndices = new StructuredBuffer<uint>( m_device, MAX_QUERY_SOURCES, true );
			m_SB_HeatSource = new StructuredBuffer<float>( m_device, elementsCount, true );
			m_SB_HeatTarget = new StructuredBuffer<float>( m_device, elementsCount, true );

			m_SB_HeatBarycentrics = new StructuredBuffer<float>( m_device, elementsCount, true );
			m_SB_HeatSum = new StructuredBuffer<float>( m_device, m_nodesCount, true );

			// Setup initial CB
			m_CB_Main.m._nodesCount = m_nodesCount;
			m_CB_Main.m._sourcesCount = 0;
			m_CB_Main.m._resX = (uint) panelOutput.Width;
			m_CB_Main.m._resY = (uint) panelOutput.Height;
			m_CB_Main.m._diffusionCoefficient = 1.0f;
			m_CB_Main.m._hoveredNodeIndex = ~0U;
			m_CB_Main.m._renderFlags = 0U;
			m_CB_Main.UpdateData();

			// Load false colors
			m_tex_FalseColors0 = LoadFalseColors( new FileInfo( "../../Images/Gradients/Magma.png" ) );
			m_tex_FalseColors1 = LoadFalseColors( new FileInfo( "../../Images/Gradients/Viridis.png" ) );

			// Prepare font atlas
			m_SB_Text = new StructuredBuffer<SB_Letter>( m_device, 1024U, true );
			BuildFont();

			Application.Idle += Application_Idle;
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			m_CB_Main.m._sourceIndex = (uint) integerTrackbarControlShowQuerySourceIndex.Value;
			m_CB_Main.m._renderFlags = 0U;
			m_CB_Main.m._renderFlags |= radioButtonShowBarycentrics.Checked ? 0x1U : 0U;
			m_CB_Main.m._renderFlags |= radioButtonShowResultsBarycentric.Checked ? 0x2U : 0U;
			m_CB_Main.m._renderFlags |= radioButtonShowResultsSum.Checked ? 0x4U : 0U;
			m_CB_Main.m._renderFlags |= checkBoxShowLog.Checked ? 0x10U : 0U;
			m_CB_Main.m._barycentricDistanceTolerance = floatTrackbarControlResultsTolerance.Value;
			m_CB_Main.m._barycentricBias = floatTrackbarControlBarycentricBias.Value;
			m_CB_Main.m._diffusionCoefficient = floatTrackbarControlDiffusionConstant.Value;
			m_CB_Main.UpdateData();


			//////////////////////////////////////////////////////////////////////////
			// Simulate heat wave
			if ( checkBoxRun.Checked && m_queryNodes.Length > 0 && m_compute_HeatDiffusion.Use() ) {

				m_SB_HeatSource.SetInput( 0 );
				m_SB_HeatTarget.SetOutput( 0 );

				m_SB_Nodes.SetInput( 1 );
				m_SB_LinkTargets.SetInput( 2 );

				m_SB_SourceIndices.SetInput( 3 );

				// Simulate heat propagation
				m_compute_HeatDiffusion.Dispatch( (m_nodesCount + 63) >> 6, (uint) m_queryNodes.Length, 1 );

				// Splat heat sources
				m_compute_SplatHeatSources.Use();
				m_compute_SplatHeatSources.Dispatch( ((uint) m_queryNodes.Length + 63) >> 6, 1, 1 );

				// Swap source & target
				StructuredBuffer<float>	temp = m_SB_HeatTarget;
				m_SB_HeatTarget = m_SB_HeatSource;
				m_SB_HeatSource = temp;

				m_barycentricsDirty = true;
				m_sumDirty = true;
			}


			//////////////////////////////////////////////////////////////////////////
			// Render
			m_device.Clear( float4.Zero );
			m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

			m_SB_Nodes.SetInput( 0 );
			m_SB_LinkTargets.SetInput( 1 );
			m_SB_LinkSources.SetInput( 2 );

			m_SB_HeatSource.SetInput( 3 );
			if ( radioButtonShowTemperature.Checked ) {
				m_tex_FalseColors0.Set( 5 );
			} else if ( radioButtonShowBarycentrics.Checked ) {
				GetBarycentricsBuffer().SetInput( 4 );
				m_tex_FalseColors1.Set( 5 );
			} else if ( radioButtonShowResultsBarycentric.Checked ) {
				GetBarycentricsBuffer().SetInput( 4 );
			} else if ( radioButtonShowResultsSum.Checked ) {
				GetSumBuffer().SetInput( 4 );
				m_tex_FalseColors0.Set( 5 );
			}

			if ( m_shader_RenderGraphLink.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

				m_device.ScreenQuad.RenderInstanced( m_shader_RenderGraphLink, m_totalLinksCount );
			}

			if ( m_shader_RenderGraphNode.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.NOCHANGE );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_device.ScreenQuad.RenderInstanced( m_shader_RenderGraphNode, m_nodesCount );
			}


			//////////////////////////////////////////////////////////////////////////
			// Draw some text
			if ( m_displayText != null && m_displayText.Length > 0 && m_shader_RenderText.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_tex_FontAtlas.SetPS( 0 );
				m_tex_FontRectangle.SetVS( 1 );

				// Fill text letters
				int	totalWidth = 0;
				int	totalHeight = (int) m_tex_FontAtlas.Height;
				for ( int i=0; i < m_displayText.Length; i++ ) {
					int			letterIndex = m_char2Index[(int) m_displayText[i]];
					Rectangle	rect = m_fontRectangles[letterIndex];
					m_SB_Text.m[i].m_letterIndex = (uint) letterIndex;
					m_SB_Text.m[i].m_offset = totalWidth;
					m_SB_Text.m[i].m_ratio = rect.Width;
					totalWidth += rect.Width;
				}
					// Normalize
				float	norm = 1.0f / totalWidth;
				for ( int i=0; i < m_displayText.Length; i++ ) {
					m_SB_Text.m[i].m_offset *= norm;
					m_SB_Text.m[i].m_ratio *= norm;
				}
				m_SB_Text.Write();
				m_SB_Text.SetInput( 2 );

				// Setup text rectangle
				const float	textHeight_pixels = 20.0f;	// What we want the text size to be on screen
				float		textWidth_pixels = totalWidth * textHeight_pixels / totalHeight;

				float2		textSize_proj = new float2( 2.0f * textWidth_pixels / panelOutput.Width, 2.0f * textHeight_pixels / panelOutput.Height );

				float2		selectedNodePosition_proj = new float2(  2.0f * (m_selectedNodePosition.x - m_CB_Main.m._cameraCenter.x) / m_CB_Main.m._cameraSize.x,
																	-2.0f * (m_selectedNodePosition.y - m_CB_Main.m._cameraCenter.y) / m_CB_Main.m._cameraSize.y );
				m_CB_Text.m._position.Set( selectedNodePosition_proj.x - 0.5f * textSize_proj.x, selectedNodePosition_proj.y + 1.0f * textSize_proj.y );	// Horizontal centering on node position, vertical offset so it's above
				m_CB_Text.m._right.Set( textSize_proj.x, 0.0f );
				m_CB_Text.m._up.Set( 0.0f, textSize_proj.y );
				m_CB_Text.UpdateData();

				m_device.ScreenQuad.RenderInstanced( m_shader_RenderText, (uint) m_displayText.Length );
			}

			m_device.Present( false );
		}

		#region Normalized Results Space Computation

		float	ComputeLogHeat( float _heat ) {
//return _heat;
//			return 1 + Mathf.Log( Math.Max( 1e-18f, _heat ) );
			return Mathf.Sqrt( -Mathf.Log( Math.Max( 1e-18f, _heat ) ) );	// Following Varadhan
		}

		StructuredBuffer<float>			GetBarycentricsBuffer() {
			if ( !m_barycentricsDirty || m_queryNodes.Length < 2 )
				return m_SB_HeatBarycentrics;

			// Read back simulation & transform into computable log heat
			m_SB_HeatSource.Read();
			for ( int nodeIndex=0; nodeIndex < m_SB_HeatSource.m.Length; nodeIndex++ )
				m_SB_HeatSource.m[nodeIndex] = ComputeLogHeat( m_SB_HeatSource.m[nodeIndex] );

			// Build a matrix of mutual heat values for each simulation at the position of each source
			int			sourcesCount = m_queryNodes.Length;
			Matrix		mutualHeat = new Matrix( sourcesCount );
			for ( int source0=0; source0 < sourcesCount; source0++ ) {
				int	sourceHeatOffset0 = (int) m_nodesCount * source0;

				for ( int source1=0; source1 < sourcesCount; source1++ ) {
					int		sourceNodeIndex = (int) m_neuron2ID[m_queryNodes[source1]];
					float	heat = m_SB_HeatSource.m[sourceHeatOffset0 + sourceNodeIndex];	// Here we read the temperature of source 1 in the simulation space of source 0
					mutualHeat[source1,source0] = heat;
				}
			}

			// Invert so we get the matrix that will help us compute barycentric coordinates
			Matrix		barycentric = mutualHeat.Invert();
//Matrix	test = mutualHeat * barycentric;

			// Apply transform to the fields
			double[]	sourceHeatVector = new double[sourcesCount];
			double[]	barycentricsVector = new double[sourcesCount];
			for ( int nodeIndex=0; nodeIndex < (int) m_nodesCount; nodeIndex++ ) {
				// Build source vector
				for ( int sourceIndex=0; sourceIndex < sourcesCount; sourceIndex++ ) {
					sourceHeatVector[sourceIndex] = m_SB_HeatSource.m[(int) m_nodesCount * sourceIndex + nodeIndex];
				}

				// Transform into barycentrics
				Matrix.Mul( sourceHeatVector, barycentric, barycentricsVector );

				// Write back
				for ( int sourceIndex=0; sourceIndex < sourcesCount; sourceIndex++ ) {
					m_SB_HeatBarycentrics.m[(int) m_nodesCount * sourceIndex + nodeIndex] = (float) barycentricsVector[sourceIndex];
				}
			}

			// Write results
			m_SB_HeatBarycentrics.Write();

			m_barycentricsDirty = false;

			return m_SB_HeatBarycentrics;
		}

		StructuredBuffer<float>			GetSumBuffer() {
			if ( !m_sumDirty || m_queryNodes.Length < 2 )
				return m_SB_HeatSum;

			// Read back simulation & accumulate into each node
			m_SB_HeatSource.Read();

			// Build a matrix of mutual heat values for each simulation at the position of each source
			int		sourcesCount = m_queryNodes.Length;
			float	maxHeat = 0.0f;
			for ( int nodeIndex=0; nodeIndex < m_nodesCount; nodeIndex++ ) {
				float	sumHeat = 0.0f;
				for ( int sourceIndex=0; sourceIndex < sourcesCount; sourceIndex++ )
					sumHeat += m_SB_HeatSource.m[m_nodesCount * sourceIndex + nodeIndex];

				m_SB_HeatSum.m[nodeIndex] = sumHeat;
				maxHeat = Mathf.Max( maxHeat, sumHeat );
			}

			// Normalize
			for ( int nodeIndex=0; nodeIndex < m_nodesCount; nodeIndex++ ) {
				m_SB_HeatSum.m[nodeIndex] = m_SB_HeatSum.m[nodeIndex] / maxHeat;
			}

			// Write results
			m_SB_HeatSum.Write();

			m_sumDirty = false;

			return m_SB_HeatSum;
		}

		#endregion

		#region Junk

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if ( disposing && (components != null) ) {
				components.Dispose();

				m_tex_FontRectangle.Dispose();
				m_tex_FontAtlas.Dispose();
				m_SB_Text.Dispose();

				m_SB_HeatSum.Dispose();
				m_SB_HeatBarycentrics.Dispose();
				m_SB_HeatTarget.Dispose();
				m_SB_HeatSource.Dispose();

				m_tex_FalseColors1.Dispose();
  				m_tex_FalseColors0.Dispose();

				m_SB_SourceIndices.Dispose();

				m_SB_LinkTargets.Dispose();
				m_SB_LinkSources.Dispose();
				m_SB_Nodes.Dispose();

				m_shader_RenderText.Dispose();
				m_shader_RenderGraphLink.Dispose();
				m_shader_RenderGraphNode.Dispose();

				m_compute_HeatDiffusion.Dispose();

				m_CB_Text.Dispose();
				m_CB_Main.Dispose();

				Device	temp = m_device;
				m_device = null;
				temp.Dispose();
			}
			base.Dispose( disposing );
		}

		void	BuildFont() {
			string	charSet = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~éèêëôöàçÔÖÂÊÉœûüù";

			m_fontRectangles = new Rectangle[charSet.Length];
			for ( int charIndex=0; charIndex < charSet.Length; charIndex++ ) {
				char	C = charSet[charIndex];
				int		index = (int) C;
				m_char2Index[index] = charIndex;
			}

			// Load atlas & rectangles
			using ( ImageUtility.ImageFile file = new ImageUtility.ImageFile( new FileInfo( "Atlas.png" ) ) ) {
				ImageUtility.ImageFile file2 = new ImageUtility.ImageFile( file, ImageUtility.PIXEL_FORMAT.RGBA8 );
				using ( ImageUtility.ImagesMatrix M = new ImageUtility.ImagesMatrix( file2, ImageUtility.ImagesMatrix.IMAGE_TYPE.sRGB ) ) {
					m_tex_FontAtlas = new Texture2D( m_device, M, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
				}
			}

			using ( FileStream S = new FileInfo( "Atlas.rect" ).OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) ) {

					// Read both CPU and GPU versions
					float	recW = 1.0f / m_tex_FontAtlas.Width;
					float	recH = 1.0f / m_tex_FontAtlas.Height;
					using ( PixelsBuffer content = new PixelsBuffer( (uint) (16 * m_fontRectangles.Length) ) ) {
						using ( BinaryWriter W = content.OpenStreamWrite() ) {
							for ( int i=0; i < m_fontRectangles.Length; i++ ) {
								m_fontRectangles[i].X = R.ReadInt32();
								m_fontRectangles[i].Y = R.ReadInt32();
								m_fontRectangles[i].Width = R.ReadInt32();
								m_fontRectangles[i].Height = R.ReadInt32();

								W.Write( recW * (float) m_fontRectangles[i].X );
								W.Write( recH * (float) m_fontRectangles[i].Y );
								W.Write( recW * (float) m_fontRectangles[i].Width );
								W.Write( recH * (float) m_fontRectangles[i].Height );
							}
						}

						m_tex_FontRectangle = new Texture2D( m_device, (uint) m_fontRectangles.Length, 1, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
					}
				}

		}

		Texture2D	LoadFalseColors( FileInfo _fileName ) {
			using ( ImageUtility.ImageFile sourceImage = new ImageUtility.ImageFile( _fileName, ImageUtility.ImageFile.FILE_FORMAT.PNG ) ) {
				ImageUtility.ImageFile convertedImage = new ImageUtility.ImageFile();
				convertedImage.ConvertFrom( sourceImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
				using ( ImageUtility.ImagesMatrix image = new ImageUtility.ImagesMatrix( convertedImage, ImageUtility.ImagesMatrix.IMAGE_TYPE.sRGB ) )
					return new Texture2D( m_device, image, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
			}
		}

		#endregion

		#region Pack/Unpack unit testing

		void	UnitTestPacking() {
			for ( ushort i=0; i < 4096; i++ ) {
				for ( ushort j=i; j < 4096; j++ ) {
					uint	V = Pack2( i, j );
					ushort	i2, j2;
					Unpack2( V, out i2, out j2 );
					if ( i2 != i || j2 != j )
						throw new Exception( "Rha mé !" );
				}
			}


			for ( ushort i=0; i < 4096; i++ ) {
				for ( ushort j=i; j < 4096; j++ ) {
					uint	V = Pack( i, j );
					ushort	i2, j2;
					Unpack( V, out i2, out j2 );
					if ( i2 != i || j2 != j )
						throw new Exception( "Rha mé !" );
				}
			}
		}

uint	BitsCount( ushort _value ) {
	uint	count = 0;
	ushort	mask = (ushort) 0x8000U;
	for ( ; count < 16 && (_value & mask) == 0; count++, mask>>=1 );
	return 15 - count;
}
uint	Pack( ushort _a, ushort _b ) {
	uint	bitsCount = 1 + BitsCount( _a );
	uint	result = bitsCount << (32-4);
	if ( bitsCount > 0 )
		result |= (uint) (_a << (12 - (int) bitsCount));
	if ( bitsCount < 12 )
		result |= (uint) ((_b - _a) & (0x0FFFU >> (int) bitsCount));
	return result;
}
void	Unpack( uint _value, out ushort _a, out ushort _b ) {
	uint	bitsCount = _value >> (32-4);
	_a = (ushort) (bitsCount > 0 ? (_value >> (12 - (int) bitsCount)) : 0U);
	_b = (ushort) (bitsCount < 12 ? (_value & (0x0FFFU >> (int) bitsCount)) : 0);
	_b += _a;
}

uint	maxTotalBitsCount = 0;
ushort	maxA, maxB;
uint	Pack2( ushort _a, ushort _b ) {
	_b -= _a;
	uint	bitsCount0 = 1 + BitsCount( _a );
	uint	bitsCount1 = 1 + BitsCount( _b );
//	uint	result = (bitsCount0 << (32-4)) | (bitsCount1 << (32-8));
	uint	result = bitsCount0 << (32-4);
			result |= _a;
			result |= (uint) _b << (int) bitsCount0;

// Check max count
if ( bitsCount0 + bitsCount1 > maxTotalBitsCount ) {
	maxTotalBitsCount = bitsCount0 + bitsCount1;
	maxA = _a;
	maxB = _b;
}

	return result;
}
void	Unpack2( uint _value, out ushort _a, out ushort _b ) {
	uint	bitsCount = _value >> (32-4);
	_a = (ushort) (_value & (0x0FFFU >> (int) (12 - bitsCount)));
	_b = (ushort) (_value >> (int) bitsCount);
	_b += _a;
}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		private void buttonReset_Click( object sender, EventArgs e ) {
			Array.Clear( m_SB_HeatSource.m, 0, m_SB_HeatSource.m.Length );
			m_SB_HeatSource.Write();
			m_barycentricsDirty = true;
			m_sumDirty = true;
		}

		float[]	m_resultScores = new float[0];
		int[]	m_resultNodeIndices = new int[0];

		float	ComputeResult( uint _nodeIndex ) {
			int		sourcesCount = m_queryNodes.Length;

			float	bias = floatTrackbarControlBarycentricBias.Value;
			int		biasSourceTarget = integerTrackbarControlShowQuerySourceIndex.Value;

			float	isoBarycentricCenter0 = (1.0f - bias) / (sourcesCount - bias);	// The ideal center is a vector with all components equal to this value
			float	isoBarycentricCenter1 = Mathf.Lerp( isoBarycentricCenter0, 1.0f, bias );

			float	sqDistance = 0.0f;
			for ( int sourceIndex=0; sourceIndex < sourcesCount; sourceIndex++ ) {
				float	barycentric = m_SB_HeatBarycentrics.m[m_nodesCount * sourceIndex + _nodeIndex];
				float	delta = barycentric - (sourceIndex == biasSourceTarget ? isoBarycentricCenter1 : isoBarycentricCenter0);
				sqDistance += delta * delta;
			}

			return Mathf.Sqrt( sqDistance );
		}

		private void buttonGrabResults_Click( object sender, EventArgs e ) {
			int	sourcesCount = m_queryNodes.Length;
			if ( sourcesCount == 0 ) {
				textBoxSearchResults.Text = "No results.";
				return;
			}

			// Compute barycentrics needed for results
			if ( radioButtonShowResultsBarycentric.Checked ) {
				GetBarycentricsBuffer().Read();

				// Compute node scores
				float	bias = floatTrackbarControlBarycentricBias.Value;
				int		biasSourceTarget = integerTrackbarControlShowQuerySourceIndex.Value;

				float	isoBarycentricCenter0 = (1.0f - bias) / (sourcesCount - bias);	// The ideal center is a vector with all components equal to this value
				float	isoBarycentricCenter1 = Mathf.Lerp( isoBarycentricCenter0, 1.0f, bias );

				m_resultScores = new float[m_nodesCount];
				m_resultNodeIndices = new int[m_nodesCount];
				for ( int nodeIndex=0; nodeIndex < m_nodesCount; nodeIndex++ ) {

					float	sqDistance = 0.0f;
					for ( int sourceIndex=0; sourceIndex < sourcesCount; sourceIndex++ ) {
						float	barycentric = m_SB_HeatBarycentrics.m[m_nodesCount * sourceIndex + nodeIndex];
						float	delta = barycentric - (sourceIndex == biasSourceTarget ? isoBarycentricCenter1 : isoBarycentricCenter0);
						sqDistance += delta * delta;
					}

					m_resultScores[nodeIndex] = Mathf.Sqrt( sqDistance );
					m_resultNodeIndices[nodeIndex] = nodeIndex;
				}
			} else {
				GetSumBuffer().Read();

				// Compute node scores
				m_resultScores = new float[m_nodesCount];
				m_resultNodeIndices = new int[m_nodesCount];
				for ( int nodeIndex=0; nodeIndex < m_nodesCount; nodeIndex++ ) {
					m_resultScores[nodeIndex] = 1.0f - m_SB_HeatSum.m[nodeIndex];	// Lowest scores are better!
					m_resultNodeIndices[nodeIndex] = nodeIndex;
				}
			}

//Il faut aussi isoler les résultats du type qu'on veut !

			// Dump results
			string	results = "";

			if ( Selection.Length > 0 ) {
				// Show selected nodes first
				results += "Results for selection:\r\n";
				foreach ( ProtoParser.Neuron selectedNode in Selection ) {
					float	score = m_resultScores[m_neuron2ID[selectedNode]];
					results += selectedNode + (selectedNode.m_value is ProtoParser.NeuronValue ? "( " + ((selectedNode.m_value as ProtoParser.NeuronValue).m_valueMean != null ? (selectedNode.m_value as ProtoParser.NeuronValue).m_valueMean : "<null>") + " )" : "") + " - " + score.ToString( "G4" ) + "\r\n";
				}

				results += "\r\n";
			}

			// Sort scores & dump the N first
			Array.Sort( m_resultScores, m_resultNodeIndices );

			for ( int i=0; i < integerTrackbarControlSignificantResultsCount.Value; i++ ) {
				float				score = m_resultScores[i];
				ProtoParser.Neuron	N = m_graph.Neurons[m_resultNodeIndices[i]];
				results += N + (N.m_value is ProtoParser.NeuronValue ? "( " + ((N.m_value as ProtoParser.NeuronValue).m_valueMean != null ? (N.m_value as ProtoParser.NeuronValue).m_valueMean : "<null>") + " )" : "") + " - " + score.ToString( "G4" ) + "\r\n";
			}
			textBoxSearchResults.Text = results;
		}

		private void panelOutput_MouseDown( object sender, MouseEventArgs e ) {
// 			if ( e.Button == MouseButtons.Middle ) {
// 				// Add a new hotspot
// 				AddHotSpot( new Point( e.X * GRAPH_SIZE / panelOutput.Width, e.Y * GRAPH_SIZE / panelOutput.Height ) );
// 
// 				// Authorize source plotting ONLY when we successfully registered a new point
// 				m_plotSource = true;
// 			}
		}

		#region Hovering

		float2	Client2Pos( Point _clientPos ) {
			return new float2(	m_CB_Main.m._cameraCenter.x + ((float) _clientPos.X / panelOutput.Width - 0.5f) * m_CB_Main.m._cameraSize.x,
								m_CB_Main.m._cameraCenter.y - (0.5f - (float) _clientPos.Y / panelOutput.Height) * m_CB_Main.m._cameraSize.y );
		}

		string	m_displayText = null;
		float2	m_selectedNodePosition;
		private void panelOutput_MouseMove( object sender, MouseEventArgs e ) {
			// Transform mouse position into node space
			float2	mousePosition = Client2Pos( e.Location );
			float	nodeRadius = 0.01f * 0.5f * (m_CB_Main.m._cameraSize.x + m_CB_Main.m._cameraSize.y);	// Radius adapts to camera size
			float	sqNodeRadius = nodeRadius * nodeRadius;

			// Identify any node under the mouse
			m_SB_Nodes.Read();

			m_displayText = null;
			m_CB_Main.m._hoveredNodeIndex = ~0U;
			for ( int nodeIndex=0; nodeIndex < m_nodesCount; nodeIndex++ ) {
				float2	nodePosition = m_SB_Nodes.m[nodeIndex].m_position;
				float2	delta = nodePosition - mousePosition;
				float	sqDistance = delta.Dot( delta );
				if ( sqDistance > sqNodeRadius )
					continue;

				// Found it!
				ProtoParser.Neuron	selectedNeuron = m_graph[nodeIndex];
				if ( selectedNeuron.m_name != null )
					m_displayText = selectedNeuron.m_name;
				else
					m_displayText = "*" + selectedNeuron.Parents[0].m_name;

				if ( selectedNeuron.m_value is ProtoParser.NeuronValue ) {
					// Show value
					m_displayText += "( ";
					m_displayText += (selectedNeuron.m_value as ProtoParser.NeuronValue).m_valueMean != null ? (selectedNeuron.m_value as ProtoParser.NeuronValue).m_valueMean : "<null>";
					m_displayText += " )";
				}

				// Show value
				if ( radioButtonShowTemperature.Checked ) {
					m_SB_HeatSource.Read();
					float	heat = m_SB_HeatSource.m[m_nodesCount * integerTrackbarControlShowQuerySourceIndex.Value + m_neuron2ID[selectedNeuron]];
					m_displayText += " = " + heat.ToString( "G4" );
				} else if ( radioButtonShowBarycentrics.Checked ) {
					GetBarycentricsBuffer();
					float	heat = m_SB_HeatBarycentrics.m[m_nodesCount * integerTrackbarControlShowQuerySourceIndex.Value + m_neuron2ID[selectedNeuron]];
					m_displayText += " = " + heat.ToString( "G4" );
				} else if ( radioButtonShowResultsBarycentric.Checked ) {
					GetBarycentricsBuffer();
					float	heat = ComputeResult( m_neuron2ID[selectedNeuron] );
					m_displayText += " = " + heat.ToString( "G4" );
				} else if ( radioButtonShowResultsSum.Checked ) {
					float	heatSum = GetSumBuffer().m[m_neuron2ID[selectedNeuron]];
					m_displayText += " = " + heatSum.ToString( "G4" );
				}

				m_selectedNodePosition = nodePosition;

				m_CB_Main.m._hoveredNodeIndex = (uint) nodeIndex;
				break;
			}

			m_CB_Main.UpdateData();
		}

		#endregion

		#region Query/Selection Input

		ProtoParser.Neuron[]	m_queryNodes = new ProtoParser.Neuron[0];
		ProtoParser.Neuron[]	QueryNodes {
			get { return m_queryNodes; }
			set {
				if ( value == null )
					value = new ProtoParser.Neuron[0];

				m_queryNodes = value;

				// Update the amount of sources in the main CB
				m_CB_Main.m._sourcesCount = (uint) m_queryNodes.Length;
				m_CB_Main.UpdateData();

				// Set the source indices for the splatting shader
				for ( int i=0; i < m_queryNodes.Length; i++ ) {
					uint	neuronID = m_neuron2ID[m_queryNodes[i]];
					m_SB_SourceIndices.m[i] = neuronID;
				}
				m_SB_SourceIndices.Write();

				// Reset simulation
				buttonReset_Click( null, EventArgs.Empty );

				// Update UI
				if ( m_queryNodes.Length > 1 ) {
					integerTrackbarControlShowQuerySourceIndex.Enabled = true;
					integerTrackbarControlShowQuerySourceIndex.RangeMax = m_queryNodes.Length - 1;
					integerTrackbarControlShowQuerySourceIndex.VisibleRangeMax = m_queryNodes.Length - 1;
					integerTrackbarControlShowQuerySourceIndex.Value = Math.Min( integerTrackbarControlShowQuerySourceIndex.RangeMax-1, integerTrackbarControlShowQuerySourceIndex.Value );
				} else {
					integerTrackbarControlShowQuerySourceIndex.Enabled = false;
					integerTrackbarControlShowQuerySourceIndex.Value = 0;
					integerTrackbarControlShowQuerySourceIndex.RangeMax = 1;
					integerTrackbarControlShowQuerySourceIndex.VisibleRangeMax = 1;
				}
			}
		}

		private void textBoxSearch_TextChanged( object sender, EventArgs e ) {
			try {
				string[]	queries = textBoxSearch.Text.Split( '\n' );

				if ( queries.Length > 0 ) {
					string	message = "";

					List<ProtoParser.Neuron>	queryNodes = new List<ProtoParser.Neuron>();
					foreach ( string query in queries ) {
						try {
							ProtoParser.Neuron	N = ProcessQuery( query );
							if ( N == null )
								continue;

							queryNodes.Add( N );

							message += "OK: " + N + "\r\n";
						} catch ( Exception _e ) {
							message += "Error on \"" + query + "\": " + _e.Message + "\r\n";
						}
					}

					QueryNodes = queryNodes.ToArray();

					message += "Total queries: " + queryNodes.Count + "\r\n";

					textBoxProcessedQuery.Text = message;

				} else {
					textBoxProcessedQuery.Text = "No query source.";
				}

			} catch ( Exception _e ) {
				QueryNodes = null;
				textBoxProcessedQuery.Text = "Error during search: " + _e.Message;
			}
		}

		ProtoParser.Neuron[]	m_selection = new ProtoParser.Neuron[0];
		ProtoParser.Neuron[]	Selection {
			get { return m_selection; }
			set {
				if ( value == null )
					value = new ProtoParser.Neuron[0];

				m_selection = value;

				// Update selected flags
				for ( int nodeIndex=0; nodeIndex < (int) m_nodesCount; nodeIndex++ )
					m_SB_Nodes.m[nodeIndex].m_flags = 0x0U;
 
				for ( int i=0; i < m_nodesCount; i++ )
					m_SB_Nodes.m[i].m_flags = 0U;

				foreach ( ProtoParser.Neuron selectedNeuron in m_selection ) {
					// Select neuron
					m_SB_Nodes.m[m_neuron2ID[selectedNeuron]].m_flags |= 1U;

					// Select parents
					ProtoParser.Neuron parent = selectedNeuron;
					while ( parent.ParentsCount > 0 ) {
						parent = parent.Parents[0];
						m_SB_Nodes.m[m_neuron2ID[parent]].m_flags |= 2U;
					}
				}

				m_SB_Nodes.Write();
			}
		}

		private void textBoxSelection_TextChanged( object sender, EventArgs e ) {
			try {
				string[]	queries = textBoxSelection.Text.Split( '\n', ',', ' ', ';' );

				if ( queries.Length > 0 ) {
					string	message = "";

					List<ProtoParser.Neuron>	selectedNodes = new List<ProtoParser.Neuron>();
					for ( int i=0; i < queries.Length; i++ ) {
						string	query = queries[i].Trim();
						if ( query.StartsWith( "\"" ) ) {
//							query = query.Remove( 0, 1 );	// Remove "
							while ( !query.EndsWith( "\"" ) && i < queries.Length-1 ) {
								query += " " + queries[++i];
								query = query.Trim();
							}
// 							if ( query.Length > 0 )
// 								query = query.Remove( query.Length-1, 1 );	// Remove last "
						}

						try {
							ProtoParser.Neuron	N = ProcessQuery( query );
							if ( N == null )
								continue;

							selectedNodes.Add( N );

							message += "OK: " + N + "\r\n";
						} catch ( Exception _e ) {
							message += "Error on \"" + query + "\": " + _e.Message + "\r\n";
						}
					}

					Selection = selectedNodes.ToArray();

					message += "Total selection: " + selectedNodes.Count + "\r\n";

					textBoxProcessedSelection.Text = message;

				} else {
					textBoxProcessedSelection.Text = "Empty.";
				}

			} catch ( Exception _e ) {
				Selection = null;
				textBoxProcessedSelection.Text = "Error during selection: " + _e.Message;
			}
		}

		ProtoParser.Neuron	ProcessQuery( string _query ) {
			_query = _query.Trim();

			ProtoParser.Neuron	single = m_graph.FindNeuron( _query );
			return single;
		}

		#endregion

		private void buttonSave_Click( object sender, EventArgs e ) {
// 			if ( saveFileDialog1.ShowDialog( this ) != DialogResult.OK )
// 				return;
// 
// 			using ( FileStream S = new FileInfo( saveFileDialog1.FileName ).Create() )
// 				using ( BinaryWriter W = new BinaryWriter( S ) ) {
// 					// Write bounding box
// 					W.Write( m_CB_Main.m._cameraCenter.x - 0.5f * m_CB_Main.m._cameraSize.x );
// 					W.Write( m_CB_Main.m._cameraCenter.y - 0.5f * m_CB_Main.m._cameraSize.y );
// 					W.Write( m_CB_Main.m._cameraCenter.x + 0.5f * m_CB_Main.m._cameraSize.x );
// 					W.Write( m_CB_Main.m._cameraCenter.y + 0.5f * m_CB_Main.m._cameraSize.y );
// 
// 					// Write nodes count
// 					W.Write( m_SB_NodeSims[0].m.Length );
// 
// 					// Write node positions
// 					m_SB_NodeSims[0].Read();
// 
// 					for ( int i=0; i < m_SB_NodeSims[0].m.Length; i++ ) {
// 						W.Write( m_SB_NodeSims[0].m[i].m_position.x );
// 						W.Write( m_SB_NodeSims[0].m[i].m_position.y );
// 					}
// 				}
		}

		#endregion
	}
}
