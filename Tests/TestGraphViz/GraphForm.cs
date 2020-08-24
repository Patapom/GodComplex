#define RENDER_GRAPH_PROPER
//#define BUILD_FONTS

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

namespace TestGraphViz
{
	public partial class GraphForm : Form
	{
		#region CONSTANTS

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public uint			_nodesCount;
			public uint			_resX;
			public uint			_resY;
			public float		_maxMass;

			public float2		_cameraCenter;
			public float2		_cameraSize;

			public uint			_hoveredNodeIndex;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Simulation {
			public float		_deltaTime;
			public float		_springConstant;
			public float		_dampingConstant;
			public float		_restDistance;

			public float4		_K;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Text {
			public float2		_position;
			public float2		_right;
			public float2		_up;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct SB_NodeSim {
			public float2		m_position;
			public float2		m_velocity;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct SB_NodeInfo {
			public float		m_mass;
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
		private ConstantBuffer<CB_Simulation>	m_CB_Simulation = null;
		private ConstantBuffer<CB_Text>			m_CB_Text = null;

		private ComputeShader					m_shader_ComputeForces = null;
		private ComputeShader					m_shader_Simulate = null;
#if RENDER_GRAPH_PROPER
		private Shader							m_shader_RenderGraphNode = null;
		private Shader							m_shader_RenderGraphLink = null;
#else
		private Shader							m_shader_RenderGraph = null;
#endif
		private Shader							m_shader_RenderText = null;

		private StructuredBuffer<SB_NodeInfo>	m_SB_Nodes = null;
		private StructuredBuffer<uint>			m_SB_Links = null;
		private StructuredBuffer<uint>			m_SB_LinkSources = null;
		private StructuredBuffer<float2>		m_SB_Forces = null;
		private StructuredBuffer<SB_NodeSim>[]	m_SB_NodeSims = new StructuredBuffer<SB_NodeSim>[2];
 
 		private Texture2D						m_tex_FalseColors = null;

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

		#endregion

		#region METHODS

		public GraphForm() {
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				m_device.Init( panelOutput.Handle, false, true );
			} catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "Heat Wave Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			try {
				//////////////////////////////////////////////////////////////////////////
				// Load the graph we need to simulate
				m_graph = new ProtoParser.Graph();

//				FileInfo	file = new FileInfo( "../../../AI/Projects/Semantic Memory/Tests/Birds Database/Tools/ProtoParser/Concepts.graph" );
				FileInfo	file = new FileInfo( "../../../AI/Projects/Semantic Memory/Tests/Birds Database/Tools/ProtoParser/TestGraph.graph" );
				if ( !file.Exists )
					file = new FileInfo( "./Graphs/TestGraph.graph" );

				using ( FileStream S = file.OpenRead() )
					using ( BinaryReader R = new BinaryReader( S ) )
						m_graph.Read( R );

				ProtoParser.Neuron[]	neurons = m_graph.Neurons;
				m_nodesCount = (uint) neurons.Length;

/*
m_nodesCount = 2;
neurons = new ProtoParser.Neuron[2];
neurons[0] = new ProtoParser.Neuron();
neurons[1] = new ProtoParser.Neuron();
neurons[0].LinkChild( neurons[1] );
//*/


				//////////////////////////////////////////////////////////////////////////
				m_CB_Main = new ConstantBuffer<CB_Main>( m_device, 0 );
				m_CB_Simulation = new ConstantBuffer<CB_Simulation>( m_device, 1 );
				m_CB_Text = new ConstantBuffer<CB_Text>( m_device, 2 );

				m_shader_ComputeForces = new ComputeShader( m_device, new FileInfo( "./Shaders/SimulateGraph.hlsl" ), "CS" );
				m_shader_Simulate = new ComputeShader( m_device, new FileInfo( "./Shaders/SimulateGraph.hlsl" ), "CS2" );
	#if RENDER_GRAPH_PROPER
				m_shader_RenderGraphNode = new Shader( m_device, new FileInfo( "./Shaders/RenderGraph2.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
				m_shader_RenderGraphLink = new Shader( m_device, new FileInfo( "./Shaders/RenderGraph2.hlsl" ), VERTEX_FORMAT.Pt4, "VS2", null, "PS2" );
	#else
				m_shader_RenderGraph = new Shader( m_device, new FileInfo( "./Shaders/RenderGraph.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
	#endif

				m_shader_RenderText = new Shader( m_device, new FileInfo( "./Shaders/RenderText.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );

				// Build node info
				m_SB_Nodes = new StructuredBuffer<SB_NodeInfo>( m_device, m_nodesCount, true );

				m_neuron2ID = new Dictionary<ProtoParser.Neuron, uint>( neurons.Length );

				float	maxMass = 0.0f;
				for ( int neuronIndex=0; neuronIndex < m_nodesCount; neuronIndex++ ) {
					ProtoParser.Neuron	N = neurons[neuronIndex];
					m_neuron2ID[N] = (uint) neuronIndex;

					uint	linksCount = (uint) (N.ParentsCount + N.ChildrenCount + N.FeaturesCount);
	//				m_SB_Nodes.m[neuronIndex].m_mass = (1 + 10.0f * linksCount) / (0.1f + N.Distance2Root);			// Works with S=1e4 D=-1e3
	//				m_SB_Nodes.m[neuronIndex].m_mass = (1 + 1.0f * linksCount) / (0.01f + 0.0f * N.Distance2Root);	// Works with S=1e4 D=-1e3
	//				m_SB_Nodes.m[neuronIndex].m_mass = (1 + 0.1f * linksCount) / (0.1f + N.Distance2Root);			// Works with S=10 D=-10
					m_SB_Nodes.m[neuronIndex].m_mass = 100.0f * (1 + 1.0f * linksCount);	// Works with S=1e4 D=-1e3
					m_SB_Nodes.m[neuronIndex].m_linkOffset = m_totalLinksCount;
					m_SB_Nodes.m[neuronIndex].m_linksCount = linksCount;
					m_SB_Nodes.m[neuronIndex].m_flags = 0U;

					maxMass = Mathf.Max( maxMass, m_SB_Nodes.m[neuronIndex].m_mass );

					m_totalLinksCount += linksCount;
				}

				m_SB_Nodes.m[0].m_mass = 1e4f;

				m_SB_Nodes.Write();

				// Build node links
				m_SB_Links = new StructuredBuffer<uint>( m_device, m_totalLinksCount, true );
				m_SB_LinkSources = new StructuredBuffer<uint>( m_device, m_totalLinksCount, true );
				m_totalLinksCount = 0;
				for ( int neuronIndex=0; neuronIndex < m_nodesCount; neuronIndex++ ) {
					ProtoParser.Neuron	N = neurons[neuronIndex];
					foreach ( ProtoParser.Neuron O in N.Parents ) {
						m_SB_LinkSources.m[m_totalLinksCount] = (uint) neuronIndex;
						m_SB_Links.m[m_totalLinksCount++] = m_neuron2ID[O];
					}
					foreach ( ProtoParser.Neuron O in N.Children ) {
						m_SB_LinkSources.m[m_totalLinksCount] = (uint) neuronIndex;
						m_SB_Links.m[m_totalLinksCount++] = m_neuron2ID[O];
					}
					foreach ( ProtoParser.Neuron O in N.Features ) {
						m_SB_LinkSources.m[m_totalLinksCount] = (uint) neuronIndex;
						m_SB_Links.m[m_totalLinksCount++] = m_neuron2ID[O];
					}
				}
				m_SB_Links.Write();
				m_SB_LinkSources.Write();

				// Setup initial CB
				m_CB_Main.m._nodesCount = m_nodesCount;
				m_CB_Main.m._resX = (uint) panelOutput.Width;
				m_CB_Main.m._resY = (uint) panelOutput.Height;
				m_CB_Main.m._maxMass = maxMass;
				m_CB_Main.m._cameraCenter = float2.Zero;
				m_CB_Main.m._cameraSize.Set( 10.0f, 10.0f );
				m_CB_Main.m._hoveredNodeIndex = ~0U;
				m_CB_Main.UpdateData();

				// Initialize sim buffers
				m_SB_Forces = new StructuredBuffer<float2>( m_device, m_nodesCount * m_nodesCount, false );
				m_SB_NodeSims[0] = new StructuredBuffer<SB_NodeSim>( m_device, m_nodesCount, true );
				m_SB_NodeSims[1] = new StructuredBuffer<SB_NodeSim>( m_device, m_nodesCount, true );

				buttonReset_Click( null, EventArgs.Empty );

	//			m_shader_HeatDiffusion = new Shader( m_device, new FileInfo( "./Shaders/HeatDiffusion.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );

	// 			m_tex_Search = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, false, false, null );
	// 			m_tex_Search_Staging = new Texture2D( m_device, (uint) GRAPH_SIZE, (uint) GRAPH_SIZE, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.UNORM, true, false, null );

				// Load false colors
	//			using ( ImageUtility.ImageFile sourceImage = new ImageUtility.ImageFile( new FileInfo( "../../Images/Gradients/Viridis.png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG ) ) {
				using ( ImageUtility.ImageFile sourceImage = new ImageUtility.ImageFile( new FileInfo( "../../Images/Gradients/Magma.png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG ) ) {
					ImageUtility.ImageFile convertedImage = new ImageUtility.ImageFile();
					convertedImage.ConvertFrom( sourceImage, ImageUtility.PIXEL_FORMAT.BGRA8 );
					using ( ImageUtility.ImagesMatrix image = new ImageUtility.ImagesMatrix( convertedImage, ImageUtility.ImagesMatrix.IMAGE_TYPE.sRGB ) )
						m_tex_FalseColors = new Texture2D( m_device, image, ImageUtility.COMPONENT_FORMAT.UNORM_sRGB );
				}

				// Prepare font atlas
				m_SB_Text = new StructuredBuffer<SB_Letter>( m_device, 1024U, true );
				BuildFont();

				Application.Idle += Application_Idle;
			} catch ( Exception _e ) {
				m_device = null;
				MessageBox.Show( "Failed to initialize shaders!\n\n" + _e.Message, "Heat Wave Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}
		}

		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null )
				return;

			m_CB_Simulation.m._deltaTime = floatTrackbarControlDeltaTime.Value;
			m_CB_Simulation.m._springConstant = floatTrackbarControlSpringConstant.Value;
			m_CB_Simulation.m._dampingConstant = floatTrackbarControlDampingConstant.Value;
			m_CB_Simulation.m._restDistance = floatTrackbarControlRestDistance.Value;
			m_CB_Simulation.m._K.Set( floatTrackbarControlK0.Value, floatTrackbarControlK1.Value, floatTrackbarControlK2.Value, floatTrackbarControlK3.Value );
			m_CB_Simulation.UpdateData();


// 			Point	clientPos = panelOutput.PointToClient( Control.MousePosition );
// 			m_CB_Main.m.mousePosition.Set( GRAPH_SIZE * (float) clientPos.X / panelOutput.Width, GRAPH_SIZE * (float) clientPos.Y / panelOutput.Height );
// 			m_CB_Main.m.mouseButtons = (uint) ((((Control.MouseButtons & MouseButtons.Left) != 0) ? 1 : 0)
// //											| (((Control.MouseButtons & MouseButtons.Middle) != 0) ? 2 : 0)
// 											| (m_plotSource ? 2 : 0)
// 											| (((Control.MouseButtons & MouseButtons.Right) != 0) ? 4 : 0)
// 											| (Control.ModifierKeys == Keys.Shift ? 8 : 0));
// 			m_CB_Main.m.diffusionCoefficient = floatTrackbarControlDiffusionCoefficient.Value;
// 			m_CB_Main.m.flags = (uint) (
// 									  (checkBoxShowSearch.Checked ? 1 : 0)
// 
// 									  // 2 bits to select 4 display modes
// 									| (radioButtonShowNormalizedSpace.Checked ? 2 : 0)
// 									| (radioButtonShowResultsSpace.Checked ? 4 : 0)
// 
// 									| (checkBoxShowLog.Checked ? 8 : 0)
// 								);
// 			m_CB_Main.m.sourceIndex = (uint) integerTrackbarControlSimulationSourceIndex.Value;
// 			m_CB_Main.m.sourcesCount = (uint) m_simulationHotSpots.Count;
// 			m_CB_Main.m.resultsConfinementDistance = floatTrackbarControlResultsSpaceConfinement.Value;

			//////////////////////////////////////////////////////////////////////////
			// Perform simulation
			if ( checkBoxRun.Checked && m_shader_ComputeForces.Use() ) {

				// Compute forces
				m_SB_Nodes.SetInput( 1 );
				m_SB_Links.SetInput( 2 );

				m_SB_NodeSims[0].SetInput( 0 );		// Input positions & velocities
				m_SB_NodeSims[1].SetOutput( 0 );	// Output positions & velocities

				m_SB_Forces.SetOutput( 1 );			// Simulation forces

				uint	groupsCount = (m_nodesCount + 255) >> 8;
				m_shader_ComputeForces.Dispatch( groupsCount, 1, 1 );

				// Apply forces
				m_shader_Simulate.Use();
				m_shader_Simulate.Dispatch( groupsCount, 1, 1 );

				m_SB_NodeSims[0].RemoveFromLastAssignedSlots();
				m_SB_NodeSims[1].RemoveFromLastAssignedSlotUAV();
				m_SB_Forces.RemoveFromLastAssignedSlotUAV();

				// Swap simulation buffers
				StructuredBuffer<SB_NodeSim>	temp = m_SB_NodeSims[0];
				m_SB_NodeSims[0] = m_SB_NodeSims[1];
				m_SB_NodeSims[1] = temp;
			}


			//////////////////////////////////////////////////////////////////////////
			// Render
#if RENDER_GRAPH_PROPER
			if ( m_shader_RenderGraphLink.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS, BLEND_STATE.DISABLED );
				m_device.SetRenderTarget( m_device.DefaultTarget, m_device.DefaultDepthStencil );

				m_device.Clear( float4.Zero );
				m_device.ClearDepthStencil( m_device.DefaultDepthStencil, 1.0f, 0, true, false );

				m_SB_NodeSims[0].SetInput( 0 );
				m_SB_Nodes.SetInput( 1 );
				m_SB_Links.SetInput( 2 );
				m_SB_LinkSources.SetInput( 3 );

				m_tex_FalseColors.SetPS( 4 );

				m_device.ScreenQuad.RenderInstanced( m_shader_RenderGraphLink, m_totalLinksCount );
			}

			if ( m_shader_RenderGraphNode.Use() ) {
				m_device.SetRenderStates( RASTERIZER_STATE.NOCHANGE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.NOCHANGE );
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_SB_NodeSims[0].SetInput( 0 );
				m_SB_Nodes.SetInput( 1 );
				m_SB_Links.SetInput( 2 );
				m_SB_LinkSources.SetInput( 3 );

				m_tex_FalseColors.SetPS( 4 );

				m_device.ScreenQuad.RenderInstanced( m_shader_RenderGraphNode, m_nodesCount );
			}
#else
			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			if ( m_shader_RenderGraph.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );

				m_SB_NodeSims[0].SetInput( 0 );
				m_SB_Nodes.SetInput( 1 );

				m_tex_FalseColors.SetPS( 4 );

				m_device.RenderFullscreenQuad( m_shader_RenderGraph );
			}
#endif

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

			//////////////////////////////////////////////////////////////////////////
			// Auto-center camera
			if ( checkBoxAutoCenter.Checked ) {
				m_SB_NodeSims[0].Read();

				float2	minPos = new float2( float.MaxValue, float.MaxValue );
				float2	maxPos = new float2( -float.MaxValue, -float.MaxValue );
				for ( int i=0; i < m_nodesCount; i++ ) {
					float2	P = m_SB_NodeSims[0].m[i].m_position;
					if ( float.IsNaN( P.x ) || float.IsNaN( P.y ) )
						continue;

					minPos.Min( P );
					maxPos.Max( P );
				}

				if ( minPos.x > maxPos.x || minPos.y > maxPos.y ) {
					minPos.Set( -10, -10 );
					maxPos.Set(  10,  10 );
				}

				float2	size = maxPos - minPos;
				if ( size.x * panelOutput.Height > size.y * panelOutput.Width ) {
					// Fit horizontally
					size.y = size.x * panelOutput.Height / panelOutput.Width;
				} else {
					// Fit vertically
					size.x = size.y * panelOutput.Width / panelOutput.Height;
				}

				m_CB_Main.m._cameraSize = 1.05f * size;
				m_CB_Main.m._cameraCenter = 0.5f * (minPos + maxPos);

				m_CB_Main.UpdateData();
			}

			m_device.Present( false );
		}

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

 				m_tex_FalseColors.Dispose();

				m_SB_Forces.Dispose();
				m_SB_NodeSims[1].Dispose();
				m_SB_NodeSims[0].Dispose();
				m_SB_Links.Dispose();
				m_SB_Nodes.Dispose();

				m_shader_RenderText.Dispose();
#if RENDER_GRAPH_PROPER
				m_shader_RenderGraphLink.Dispose();
				m_shader_RenderGraphNode.Dispose();
#else
				m_shader_RenderGraph.Dispose();
#endif
				m_shader_Simulate.Dispose();
				m_shader_ComputeForces.Dispose();

				m_CB_Text.Dispose();
				m_CB_Simulation.Dispose();
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

#if BUILD_FONTS
			using ( Font F = new Font( this.Font.FontFamily, 36.0f ) ) {

				// Build small bitmap and write each character
				int	width = 0;
				int	maxheight = 0;
				using ( Bitmap B = new Bitmap( 70, 70, System.Drawing.Imaging.PixelFormat.Format32bppArgb ) ) {
					using ( Graphics G = Graphics.FromImage( B ) ) {

						for ( int i=0; i < charSet.Length; i++ ) {
							string	s = charSet.Substring( i, 1 );
// 							G.FillRectangle( Brushes.Black, 0, 0, B.Width, B.Height );
// 							G.DrawString( s, F, Brushes.White, new PointF( 0, 0 ) );

							SizeF	tempSize = G.MeasureString( s, F );
							m_fontRectangles[i] = new Rectangle( width, 0, (int) Mathf.Ceiling( tempSize.Width ), (int) Mathf.Ceiling( tempSize.Height ) );
							if ( m_fontRectangles[i].Width > B.Width || m_fontRectangles[i].Height > B.Height )
								throw new Exception( "Fonts are too big, expand bitmap size or reduce font size!" );

							width += m_fontRectangles[i].Width;
							maxheight = Math.Max( maxheight, m_fontRectangles[i].Height );
						}
					}
				}

				// Build the final bitmap
				using ( Bitmap B = new Bitmap( width, maxheight, System.Drawing.Imaging.PixelFormat.Format32bppArgb ) ) {
					using ( Graphics G = Graphics.FromImage( B ) ) {
						G.FillRectangle( Brushes.Black, 0, 0, B.Width, B.Height );

						for ( int i=0; i < charSet.Length; i++ ) {
							string	s = charSet.Substring( i, 1 );
							G.DrawString( s, F, Brushes.White, new PointF( m_fontRectangles[i].X, m_fontRectangles[i].Y ) );
						}

						using ( ImageUtility.ImageFile file = new ImageUtility.ImageFile( B, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) ) ) {
							file.Save( new FileInfo( "Atlas.png" ), ImageUtility.ImageFile.FILE_FORMAT.PNG );
						}
					}
				}
			}

			// Write char sizes
			using ( FileStream S = new FileInfo( "Atlas.rect" ).Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					for ( int i=0; i < m_fontRectangles.Length; i++ ) {
						W.Write( m_fontRectangles[i].X );
						W.Write( m_fontRectangles[i].Y );
						W.Write( m_fontRectangles[i].Width );
						W.Write( m_fontRectangles[i].Height );
					}
				}
#endif

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

		#endregion

		#region EVENT HANDLERS

		private void buttonReset_Click( object sender, EventArgs e ) {
			m_CB_Main.m._cameraSize = 10.0f * float2.One;
			m_CB_Main.m._cameraCenter = float2.Zero;
			m_CB_Main.UpdateData();

#if true
			for ( int neuronIndex=0; neuronIndex < m_nodesCount; neuronIndex++ ) {
				m_SB_NodeSims[0].m[neuronIndex].m_position.Set( 0.5f * m_CB_Main.m._cameraSize.x * (2.0f * (float) SimpleRNG.GetUniform() - 1.0f), 0.5f * m_CB_Main.m._cameraSize.y * (2.0f * (float) SimpleRNG.GetUniform() - 1.0f) );	// In a size 2 square
//m_SB_NodeSims[0].m[neuronIndex].m_position.Set( 2.0f * neuronIndex - 1.0f, 0.0f );
				m_SB_NodeSims[0].m[neuronIndex].m_velocity.Set( 0, 0 );
			}
			m_SB_NodeSims[0].Write();
#else
			for ( int neuronIndex=0; neuronIndex < m_nodesCount; neuronIndex++ ) {
				float	a = Mathf.TWOPI * neuronIndex / m_nodesCount;
				m_SB_NodeSims[0].m[neuronIndex].m_position.Set( Mathf.Cos( a ), Mathf.Sin( a ) );	// Set on a unit circle
				m_SB_NodeSims[0].m[neuronIndex].m_velocity.Set( 0, 0 );
			}
			m_SB_NodeSims[0].Write();
#endif
		}

		private void buttonResetAll_Click( object sender, EventArgs e ) {
		}

		private void buttonResetObstacles_Click( object sender, EventArgs e ) {
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		private void checkBoxRun_CheckedChanged( object sender, EventArgs e ) {
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

		float2	Client2Pos( Point _clientPos ) {
			if ( m_CB_Main == null )
				return float2.Zero;

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
			m_SB_NodeSims[0].Read();

			m_displayText = null;
			m_CB_Main.m._hoveredNodeIndex = ~0U;
			for ( int nodeIndex=0; nodeIndex < m_nodesCount; nodeIndex++ ) {
				float2	nodePosition = m_SB_NodeSims[0].m[nodeIndex].m_position;
				float2	delta = nodePosition - mousePosition;
				float	sqDistance = delta.Dot( delta );
				if ( sqDistance > sqNodeRadius )
					continue;

				// Found it!
				ProtoParser.Neuron	selectedNeuron = m_graph[nodeIndex];
				m_displayText = selectedNeuron.m_name != null ? selectedNeuron.m_name : selectedNeuron.Parents[0].m_name + "()";
				m_selectedNodePosition = nodePosition;

				m_CB_Main.m._hoveredNodeIndex = (uint) nodeIndex;
				break;
			}

			m_CB_Main.UpdateData();
		}

		#endregion

		ProtoParser.Neuron[]	m_selection = new ProtoParser.Neuron[0];
		ProtoParser.Neuron[]	Selection {
			get { return m_selection; }
			set {
				if ( value == null )
					value = new ProtoParser.Neuron[0];

				if ( value.Length == m_selection.Length ) {
					bool	hasChanged = false;
					foreach ( ProtoParser.Neuron newSelection in value ) {
						bool	found = false;
						foreach ( ProtoParser.Neuron currentSelection in m_selection ) {
							if ( newSelection == currentSelection ) {
								found = true;
								break;
							}
						}
						if ( !found ) {
							hasChanged = true;
							break;
						}
					}
					if ( !hasChanged )
						return;	// No change...
				}

				// Clear children check box
				bool	oldCheckBoxStatus = checkBoxShowChildren.Checked;
				checkBoxShowChildren.Checked = false;

				// Update selection and node flags
				m_selection = value;

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

				// Restore check box status
				checkBoxShowChildren.Checked = oldCheckBoxStatus;
			}
		}

// 		void	SelectNeuronsAndHierarchy( ProtoParser.Neuron[] _neurons ) {
// 			if ( _neurons == null ) {
// 				Selection = null;
// 			}
// 
// 			List<ProtoParser.Neuron>	neurons = new List<ProtoParser.Neuron>( _neurons );
// 			foreach ( ProtoParser.Neuron N in _neurons ) {
// 				ProtoParser.Neuron parent = N;
// 				while ( parent.ParentsCount > 0 ) {
// 					parent = parent.Parents[0];
// 					neurons.Add( parent );
// 				}
// 			}
// 
// 			Selection = neurons.ToArray();
// 		}

		private void textBoxSearch_TextChanged( object sender, EventArgs e ) {
			try {
				ProtoParser.Neuron[]	results = m_graph.FindNeurons( textBoxSearch.Text );

				if ( results == null || results.Length == 0 ) {
					ProtoParser.Neuron	single = m_graph.FindNeuron( textBoxSearch.Text );
					if ( single != null ) {
						results = new ProtoParser.Neuron[] { single };
					} else {
						results = new ProtoParser.Neuron[0];
					}
				}

				if ( results.Length > 0 ) {
					labelSearchResults.Text = results.Length > 1 ? "Multiple results:\n" : "Single result:\n";
					foreach ( ProtoParser.Neuron result in results )
						labelSearchResults.Text += "	" + result.FullName + "\n";
				} else {
					labelSearchResults.Text = "No result.\n";
				}

//				SelectNeuronsAndHierarchy( results );
				Selection = results;

			} catch ( Exception _e ) {
				labelSearchResults.Text = "Error during search: " + _e.Message;
//				SelectNeuronsAndHierarchy( new ProtoParser.Neuron[0] );
				Selection = new ProtoParser.Neuron[0];
			}
		}

		private void checkBoxShowChildren_CheckedChanged( object sender, EventArgs e ) {
			if ( checkBoxShowChildren.Checked ) {
				foreach ( ProtoParser.Neuron N in Selection ) {
					foreach ( ProtoParser.Neuron child in N.Children ) {
						m_SB_Nodes.m[(int) m_neuron2ID[child]].m_flags |= 0x4U;
					}
				}
			} else {
				foreach ( ProtoParser.Neuron N in Selection ) {
					foreach ( ProtoParser.Neuron child in N.Children ) {
						m_SB_Nodes.m[(int) m_neuron2ID[child]].m_flags &= ~0x4U;
					}
				}
			}
			m_SB_Nodes.Write();
		}

		private void buttonSave_Click( object sender, EventArgs e ) {
			if ( saveFileDialog1.ShowDialog( this ) != DialogResult.OK )
				return;

			using ( FileStream S = new FileInfo( saveFileDialog1.FileName ).Create() )
				using ( BinaryWriter W = new BinaryWriter( S ) ) {
					// Write bounding box
					W.Write( m_CB_Main.m._cameraCenter.x - 0.5f * m_CB_Main.m._cameraSize.x );
					W.Write( m_CB_Main.m._cameraCenter.y - 0.5f * m_CB_Main.m._cameraSize.y );
					W.Write( m_CB_Main.m._cameraCenter.x + 0.5f * m_CB_Main.m._cameraSize.x );
					W.Write( m_CB_Main.m._cameraCenter.y + 0.5f * m_CB_Main.m._cameraSize.y );

					// Write nodes count
					W.Write( m_SB_NodeSims[0].m.Length );

					// Write node positions
					m_SB_NodeSims[0].Read();

					for ( int i=0; i < m_SB_NodeSims[0].m.Length; i++ ) {
						W.Write( m_SB_NodeSims[0].m[i].m_position.x );
						W.Write( m_SB_NodeSims[0].m[i].m_position.y );
					}
				}
		}
	}
}
