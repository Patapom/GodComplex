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

		private Shader		m_Shader_Christmas = null;
		private Texture2D	m_Tex_Christmas = null;
		private Primitive	m_Prim_Quad = null;

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
			VertexP3[]	Vertices = new VertexP3[4];
			Vertices[0] = new VertexP3() { P = new float3( -1, +1, 0 ) };	// Top-Left
			Vertices[1] = new VertexP3() { P = new float3( -1, -1, 0 ) };	// Bottom-Left
			Vertices[2] = new VertexP3() { P = new float3( +1, +1, 0 ) };	// Top-Right
			Vertices[3] = new VertexP3() { P = new float3( +1, -1, 0 ) };	// Bottom-Right

			ByteBuffer	VerticesBuffer = VertexP3.FromArray( Vertices );

			m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3 );
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
			m_Shader_Christmas = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/Christmas.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );;
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			m_Shader_Christmas.Dispose();
			m_Prim_Quad.Dispose();
			m_Tex_Christmas.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			m_Device.Clear( new float4( 0, 0, 0, 0 ) );

			// Post render command

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
