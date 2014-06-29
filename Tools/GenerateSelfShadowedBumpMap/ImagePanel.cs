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
		public enum		VIEW_MODE
		{
			RGB,
			RGB_AO,
			R,
			G,
			B,
			AO
		}

		private Bitmap				m_BitmapRGB = null;
		private Bitmap				m_BitmapRGB_AO = null;
		private Bitmap				m_BitmapR = null;
		private Bitmap				m_BitmapG = null;
		private Bitmap				m_BitmapB = null;
		private Bitmap				m_BitmapAO = null;

 		private ImageUtility.ColorProfile	m_ProfilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
//		private ImageUtility.ColorProfile	m_ProfilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.Chromaticities.sRGB, ImageUtility.ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );

		private ImageUtility.Bitmap	m_Image = null;
		public unsafe ImageUtility.Bitmap	Image
		{
			get { return m_Image; }
			set {
				m_Image = value;
				
				if ( m_BitmapRGB != null )
				{
					m_BitmapRGB.Dispose();
					m_BitmapRGB_AO.Dispose();
					m_BitmapR.Dispose();
					m_BitmapG.Dispose();
					m_BitmapB.Dispose();
					m_BitmapAO.Dispose();
				}
				m_BitmapRGB = null;
				m_BitmapRGB_AO = null;
				m_BitmapR = null;
				m_BitmapG = null;
				m_BitmapB = null;
				m_BitmapAO = null;

				if ( m_Image != null )
				{	// Fill pixel per pixel
					int	W = m_Image.Width;
					int	H = m_Image.Height;
					m_BitmapRGB = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					m_BitmapRGB_AO = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					m_BitmapR = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					m_BitmapG = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					m_BitmapB = new Bitmap( W, H, PixelFormat.Format32bppArgb );
					m_BitmapAO = new Bitmap( W, H, PixelFormat.Format32bppArgb );

					ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
					m_ProfilesRGB.XYZ2RGB( m_Image.ContentXYZ, ContentRGB );

					BitmapData	LockedBitmapRGB = m_BitmapRGB.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					BitmapData	LockedBitmapRGB_AO = m_BitmapRGB_AO.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					BitmapData	LockedBitmapR = m_BitmapR.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					BitmapData	LockedBitmapG = m_BitmapG.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					BitmapData	LockedBitmapB = m_BitmapB.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					BitmapData	LockedBitmapAO = m_BitmapAO.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanlineRGB = (byte*) LockedBitmapRGB.Scan0.ToPointer() + LockedBitmapRGB.Stride * Y;
						byte*	pScanlineRGB_AO = (byte*) LockedBitmapRGB_AO.Scan0.ToPointer() + LockedBitmapRGB_AO.Stride * Y;
						byte*	pScanlineR = (byte*) LockedBitmapR.Scan0.ToPointer() + LockedBitmapR.Stride * Y;
						byte*	pScanlineG = (byte*) LockedBitmapG.Scan0.ToPointer() + LockedBitmapG.Stride * Y;
						byte*	pScanlineB = (byte*) LockedBitmapB.Scan0.ToPointer() + LockedBitmapB.Stride * Y;
						byte*	pScanlineAO = (byte*) LockedBitmapAO.Scan0.ToPointer() + LockedBitmapRGB.Stride * Y;
						for ( int X=0; X < W; X++ )
						{
							byte	R = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].x ) );
							byte	G = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].y ) );
							byte	B = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].z ) );
							byte	A = (byte) Math.Max( 0, Math.Min( 255, 255 * ImageUtility.ColorProfile.Linear2sRGB( ContentRGB[X,Y].w ) ) );

							*pScanlineRGB++ = B;
							*pScanlineRGB++ = G;
							*pScanlineRGB++ = R;
							*pScanlineRGB++ = 0xFF;

							*pScanlineRGB_AO++ = B;
							*pScanlineRGB_AO++ = G;
							*pScanlineRGB_AO++ = R;
							*pScanlineRGB_AO++ = 0xFF;

							*pScanlineR++ = R;
							*pScanlineR++ = R;
							*pScanlineR++ = R;
							*pScanlineR++ = 0xFF;

							*pScanlineG++ = G;
							*pScanlineG++ = G;
							*pScanlineG++ = G;
							*pScanlineG++ = 0xFF;

							*pScanlineB++ = B;
							*pScanlineB++ = B;
							*pScanlineB++ = B;
							*pScanlineB++ = 0xFF;

							*pScanlineAO++ = A;
							*pScanlineAO++ = A;
							*pScanlineAO++ = A;
							*pScanlineAO++ = 0xFF;
						}
					}
					m_BitmapAO.UnlockBits( LockedBitmapAO );
					m_BitmapB.UnlockBits( LockedBitmapB );
					m_BitmapG.UnlockBits( LockedBitmapG );
					m_BitmapR.UnlockBits( LockedBitmapR );
					m_BitmapRGB_AO.UnlockBits( LockedBitmapRGB_AO );
					m_BitmapRGB.UnlockBits( LockedBitmapRGB );
				}

				Refresh();
			}
		}

		private VIEW_MODE		m_ViewMode = VIEW_MODE.RGB;
		public VIEW_MODE		ViewMode
		{
			get { return m_ViewMode; }
			set { m_ViewMode = value; Invalidate(); }
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

			e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
			if ( m_BitmapRGB != null )
			{
				RectangleF	Rect = ImageClientRect;
				Bitmap		B = null;
				switch ( m_ViewMode )
				{
					case VIEW_MODE.RGB: B = m_BitmapRGB; break;
					case VIEW_MODE.RGB_AO: B = m_BitmapRGB_AO; break;
					case VIEW_MODE.R: B = m_BitmapR; break;
					case VIEW_MODE.G: B = m_BitmapG; break;
					case VIEW_MODE.B: B = m_BitmapB; break;
					case VIEW_MODE.AO: B = m_BitmapAO; break;
				}
				e.Graphics.DrawImage(B, Rect, new RectangleF( 0, 0, m_BitmapRGB.Width, m_BitmapRGB.Height ), GraphicsUnit.Pixel );
			}
		}
	}
}
