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

		private ImageUtility.ColorProfile	m_ProfilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );

		private ImageUtility.Bitmap	m_Image = null;
		public unsafe ImageUtility.Bitmap	Image
		{
			get { return m_Image; }
			set {
				m_Image = value;
				
				if ( m_Bitmap != null )
					m_Bitmap.Dispose();
				m_Bitmap = null;

				if ( m_Image != null )
				{	// Fill pixel per pixel
					int	W = m_Image.Width;
					int	H = m_Image.Height;
					m_Bitmap = new Bitmap( W, H, PixelFormat.Format32bppArgb );

					ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
					m_ProfilesRGB.XYZ2RGB( m_Image.ContentXYZ, ContentRGB );

					BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
						for ( int X=0; X < W; X++ )
						{
							*pScanline++ = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].z ) );
							*pScanline++ = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].y ) );
							*pScanline++ = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].x ) );
							*pScanline++ = 0xFF;
						}
					}
					m_Bitmap.UnlockBits( LockedBitmap );
				}

				Refresh();
			}
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
				e.Graphics.DrawImage( m_Bitmap, Rect, new RectangleF( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), GraphicsUnit.Pixel );
			}
			else
				e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
		}
	}
}
