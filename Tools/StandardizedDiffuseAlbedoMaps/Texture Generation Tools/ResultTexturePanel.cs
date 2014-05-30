using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class ResultTexturePanel : Panel
	{
		private Bitmap		m_Bitmap = null;
		private Bitmap		m_TextureBitmap = null;
		private Bitmap2.ColorProfile	m_sRGBProfile = new Bitmap2.ColorProfile( Bitmap2.ColorProfile.STANDARD_PROFILE.sRGB );


		private CalibratedTexture	m_CalibratedTexture = null;
		public unsafe CalibratedTexture	CalibratedTexture
		{
			get { return m_CalibratedTexture; }
			set {
				m_CalibratedTexture = value;
 
				if ( m_CalibratedTexture != null && m_CalibratedTexture.Texture != null )
				{
					int		W = m_CalibratedTexture.Texture.Width;
					int		H = m_CalibratedTexture.Texture.Height;
					if ( m_TextureBitmap == null || m_TextureBitmap.Width != W || m_TextureBitmap.Height != H )
					{
						if ( m_TextureBitmap != null )
							m_TextureBitmap.Dispose();

						m_TextureBitmap = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					}

					// Convert to RGB first
					float4[,]	ContentXYZ = m_CalibratedTexture.Texture.ContentXYZ;
					float4[,]	ContentRGB = new float4[ContentXYZ.GetLength(0),ContentXYZ.GetLength(1)];
					m_sRGBProfile.XYZ2RGB( ContentXYZ, ContentRGB );

					// Fill pixels
					BitmapData	LockedBitmap = m_TextureBitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					byte	R, G, B;
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0 + LockedBitmap.Stride * Y;
						for ( int X=0; X < W; X++ )
						{
							R = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].x ) );
							G = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].y ) );
							B = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].z ) );
							*pScanline++ = B;
							*pScanline++ = G;
							*pScanline++ = R;
							*pScanline++ = 0xFF;
						}
					}
					m_TextureBitmap.UnlockBits( LockedBitmap );
				}
				else
				{
					if ( m_TextureBitmap != null )
						m_TextureBitmap.Dispose();
					m_TextureBitmap = null;
				}

				UpdateBitmap();
			}
		}

		public RectangleF	ImageClientRectangle		{ get { return ImageClientRect(); } }

		public ResultTexturePanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		public void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				using ( SolidBrush B = new SolidBrush( Color.Black ) )
					G.FillRectangle( B, 0, 0, W, H );

				if ( m_TextureBitmap != null )
				{
					// Draw thumbnail
					RectangleF	ClientRect = ImageClientRect();
					G.DrawImage( m_TextureBitmap, ClientRect, new RectangleF( 0, 0, m_TextureBitmap.Width, m_TextureBitmap.Height ), GraphicsUnit.Pixel );
				}
			}

			Invalidate();
		}

		private RectangleF		ImageClientRect()
		{
			int		SizeX = m_TextureBitmap.Width;
			int		SizeY = m_TextureBitmap.Height;

			int		WidthIfVertical = SizeX * Height / SizeY;	// Client width of the image if fitting vertically
			int		HeightIfHorizontal = SizeY * Width / SizeX;	// Client height of the image if fitting horizontally

			if ( WidthIfVertical > Width )
			{	// Fit horizontally
				return new RectangleF( 0, 0.5f * (Height-HeightIfHorizontal), Width, HeightIfHorizontal );
			}
			else
			{	// Fit vertically
				return new RectangleF( 0.5f * (Width-WidthIfVertical), 0, WidthIfVertical, Height );
			}
		}

		private PointF	Client2ImageUV( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new PointF( (_Position.X - ImageRect.X) / ImageRect.Width, (_Position.Y - ImageRect.Y) / ImageRect.Height );
		}

		private PointF	ImageUV2Client( PointF _Position )
		{
			RectangleF	ImageRect = ImageClientRect();
			return new PointF( _Position.X * ImageRect.Width + ImageRect.X, _Position.Y * ImageRect.Height + ImageRect.Y );
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();
			m_Bitmap = null;

			if ( Width > 0 && Height > 0 )
				m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			UpdateBitmap();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );
		}
	}
}
