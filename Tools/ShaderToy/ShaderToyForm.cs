using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using RendererManaged;

namespace ShaderToy
{
	public partial class ShaderToyForm : Form
	{
		private Device		m_Device = new Device();

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
			public float		iGlobalTime;
		}

		private ConstantBuffer<CB_Main>	m_CB_Main = null;
		private Shader		m_Shader_Christmas = null;
		private Texture2D	m_Tex_Christmas = null;
		private Primitive	m_Prim_Quad = null;

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartGameTime = 0;
		public float						m_CurrentGameTime = 0;
		public float						m_DeltaTime = 0;		// Delta time used for the current frame

		public ShaderToyForm()
		{
			InitializeComponent();

			Application.Idle += new EventHandler( Application_Idle );
		}

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _FileName )
		{
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				return Image2Texture( S );
		}
		public unsafe Texture2D	Image2Texture( System.IO.Stream _Stream )
		{
			int		W, H;
			byte[]	Content = null;
			using ( Bitmap B = Bitmap.FromStream( _Stream ) as Bitmap )
			{
				W = B.Width;
				H = B.Height;
				Content = new byte[W*H*4];

				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
				for ( int Y=0; Y < H; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;
					int		Offset = 4*W*Y;
					for ( int X=0; X < W; X++, Offset+=4 )
					{
						Content[Offset+2] = *pScanline++;	// B
						Content[Offset+1] = *pScanline++;	// G
						Content[Offset+0] = *pScanline++;	// R
						Content[Offset+3] = *pScanline++;	// A
					}
				}
				B.UnlockBits( LockedBitmap );
			}
			return Image2Texture( W, H, Content );
		}
		public Texture2D	Image2Texture( int _Width, int _Height, byte[] _Content )
		{
			using ( PixelsBuffer Buff = new PixelsBuffer( _Content.Length ) )
			{
				using ( System.IO.BinaryWriter W = Buff.OpenStreamWrite() )
					W.Write( _Content );

				return Image2Texture( _Width, _Height, Buff );
			}
		}
		public Texture2D	Image2Texture( int _Width, int _Height, PixelsBuffer _Content )
		{
			return new Texture2D( m_Device, _Width, _Height, 1, 1, PIXEL_FORMAT.RGBA8_UNORM_sRGB, false, false, new PixelsBuffer[] { _Content } );
		}

		#endregion

		private void	BuildQuad()
		{
			VertexPt4[]	Vertices = new VertexPt4[4];
			Vertices[0] = new VertexPt4() { Pt = new float4( -1, +1, 0, 1 ) };	// Top-Left
			Vertices[1] = new VertexPt4() { Pt = new float4( -1, -1, 0, 1 ) };	// Bottom-Left
			Vertices[2] = new VertexPt4() { Pt = new float4( +1, +1, 0, 1 ) };	// Top-Right
			Vertices[3] = new VertexPt4() { Pt = new float4( +1, -1, 0, 1 ) };	// Bottom-Right

			ByteBuffer	VerticesBuffer = VertexPt4.FromArray( Vertices );

			m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try
			{
				m_Device.Init( panelOutput.Handle, false, false );
			}
			catch ( Exception _e )
			{
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			BuildQuad();
			m_Tex_Christmas = Image2Texture( new System.IO.FileInfo( "christmas.jpg" ) );

			try
			{
//				m_Shader_Christmas = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Christmas.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
				m_Shader_Christmas = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Airlight.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;

				m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader failed to compile!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_Christmas = null;
			}

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartGameTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			if ( m_Shader_Christmas != null ) {
				m_CB_Main.Dispose();
				m_Shader_Christmas.Dispose();
			}
			m_Prim_Quad.Dispose();
			m_Tex_Christmas.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime()
		{
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Post render command
			if ( m_Shader_Christmas != null ) {
				m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
				m_CB_Main.m.iGlobalTime = GetGameTime() - m_StartGameTime;
				m_CB_Main.UpdateData();

				m_Tex_Christmas.SetPS( 0 );

				m_Shader_Christmas.Use();
				m_Prim_Quad.Render( m_Shader_Christmas );
			} else {
				m_Device.Clear( new float4( 1.0f, 0, 0, 0 ) );
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
	}
}
