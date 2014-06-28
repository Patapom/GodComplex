#define ABS_NORMAL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace GenerateSelfShadowedBumpMap
{
	public partial class ImagePanel : Panel
	{
		private Bitmap				m_Bitmap = null;
		private Bitmap				m_BitmapAlpha = null;

		private ImageUtility.ColorProfile	m_ProfilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );

		private ImageUtility.Bitmap	m_Image = null;
		public unsafe ImageUtility.Bitmap	Image
		{
			get { return m_Image; }
			set {
				m_Image = value;
				
				if ( m_Bitmap != null )
					m_Bitmap.Dispose();
				if ( m_BitmapAlpha != null )
					m_BitmapAlpha.Dispose();
				m_Bitmap = null;
				m_BitmapAlpha = null;

				if ( m_Image != null )
				{	// Fill pixel per pixel
					int	W = m_Image.Width;
					int	H = m_Image.Height;
					m_Bitmap = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					m_BitmapAlpha = new Bitmap( W, H, PixelFormat.Format32bppArgb );

					ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
					m_ProfilesRGB.XYZ2RGB( m_Image.ContentXYZ, ContentRGB );

					BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					BitmapData	LockedBitmapAlpha = m_BitmapAlpha.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
						byte*	pScanlineAlpha = (byte*) LockedBitmapAlpha.Scan0.ToPointer() + LockedBitmap.Stride * Y;
						for ( int X=0; X < W; X++ )
						{
							*pScanline++ = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].z ) );
							*pScanline++ = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].y ) );
							*pScanline++ = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].x ) );
							*pScanline++ = 0xFF;

							byte	A = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].w ) );
							*pScanlineAlpha++ = A;
							*pScanlineAlpha++ = A;
							*pScanlineAlpha++ = A;
							*pScanlineAlpha++ = 0xFF;
						}
					}
					m_BitmapAlpha.UnlockBits( LockedBitmapAlpha );
					m_Bitmap.UnlockBits( LockedBitmap );
				}

				Refresh();
			}
		}

		private bool			m_bShowAO = false;
		public bool				ShowAO
		{
			get { return m_bShowAO; }
			set { m_bShowAO = value; Invalidate(); }
		}

		private RectangleF		ImageClientRect
		{
			get
			{
				int		SizeX = m_Image.Width;
				int		SizeY = m_Image.Height;

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
		}

		public ImagePanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			base.OnSizeChanged( e );
			Invalidate();
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
			{
				RectangleF	Rect = ImageClientRect;
				e.Graphics.DrawImage( m_bShowAO ? m_BitmapAlpha : m_Bitmap, Rect, new RectangleF( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), GraphicsUnit.Pixel );
			}
			else
				e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
		}
	}
}
