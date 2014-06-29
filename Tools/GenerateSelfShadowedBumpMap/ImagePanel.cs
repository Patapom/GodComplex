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
			AO,
			AO_FROM_RGB
		}

		private Bitmap				m_Bitmap = null;

		private string				m_MessageOnEmpty = null;
		public string				MessageOnEmpty
		{
			get { return m_MessageOnEmpty; }
			set { m_MessageOnEmpty = value; Invalidate(); }
		}

 		private ImageUtility.ColorProfile	m_ProfilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.ColorProfile	m_ProfileLinear = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.Chromaticities.sRGB, ImageUtility.ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );

		private ImageUtility.Bitmap	m_Image = null;
		public ImageUtility.Bitmap	Image
		{
			get { return m_Image; }
			set {
				m_Image = value;
				UpdateBitmap();
			}
		}

		private VIEW_MODE		m_ViewMode = VIEW_MODE.RGB;
		public VIEW_MODE		ViewMode
		{
			get { return m_ViewMode; }
			set
			{
				m_ViewMode = value;
				UpdateBitmap();
			}
		}

		private bool		m_ViewLinear = false;
		public bool			ViewLinear
		{
			get { return m_ViewLinear; }
			set
			{
				m_ViewLinear = value;
				UpdateBitmap();
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

		private unsafe void	UpdateBitmap()
		{
			if ( m_Image == null )
				return;

			// Fill pixel per pixel
			int	W = m_Image.Width;
			int	H = m_Image.Height;
			if ( m_Bitmap != null && (m_Bitmap.Width != W || m_Bitmap.Height != H) )
			{
				m_Bitmap.Dispose();
				m_Bitmap = null;
			}
			if ( m_Bitmap == null )
				m_Bitmap = new Bitmap( W, H, PixelFormat.Format32bppArgb );

			ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
			if ( m_ViewLinear )
				m_ProfileLinear.XYZ2RGB( m_Image.ContentXYZ, ContentRGB );
			else
				m_ProfilesRGB.XYZ2RGB( m_Image.ContentXYZ, ContentRGB );

			BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
			for ( int Y=0; Y < H; Y++ )
			{
				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
				for ( int X=0; X < W; X++ )
				{
					byte	R = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].x ) );
					byte	G = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].y ) );
					byte	B = (byte) Math.Max( 0, Math.Min( 255, 255 * ContentRGB[X,Y].z ) );
					byte	A = (byte) Math.Max( 0, Math.Min( 255, 255 * (m_ViewLinear ? ContentRGB[X,Y].w : ImageUtility.ColorProfile.Linear2sRGB( ContentRGB[X,Y].w )) ) );

					switch ( m_ViewMode )
					{
						case VIEW_MODE.RGB:
							*pScanline++ = B;
							*pScanline++ = G;
							*pScanline++ = R;
							*pScanline++ = 0xFF;
							break;
						case VIEW_MODE.R:
							*pScanline++ = R;
							*pScanline++ = R;
							*pScanline++ = R;
							*pScanline++ = 0xFF;
							break;
						case VIEW_MODE.G:
							*pScanline++ = G;
							*pScanline++ = G;
							*pScanline++ = G;
							*pScanline++ = 0xFF;
							break;
						case VIEW_MODE.B:
							*pScanline++ = B;
							*pScanline++ = B;
							*pScanline++ = B;
							*pScanline++ = 0xFF;
							break;
						case VIEW_MODE.AO:
							*pScanline++ = A;
							*pScanline++ = A;
							*pScanline++ = A;
							*pScanline++ = 0xFF;
							break;
						case VIEW_MODE.AO_FROM_RGB:
							{
								float	LinR = ImageUtility.ColorProfile.sRGB2Linear( ContentRGB[X,Y].x );
								float	LinG = ImageUtility.ColorProfile.sRGB2Linear( ContentRGB[X,Y].y );
								float	LinB = ImageUtility.ColorProfile.sRGB2Linear( ContentRGB[X,Y].z );
								float	LinAO = (float) Math.Sqrt( LinR*LinR + LinG*LinG + LinB*LinB ) * 0.57735026918962576450914878050196f;	// divided by sqrt(3)
								A = (byte) Math.Max( 0, Math.Min( 255, 255 * ImageUtility.ColorProfile.Linear2sRGB( LinAO ) ) );
								*pScanline++ = A;
								*pScanline++ = A;
								*pScanline++ = A;
								*pScanline++ = 0xFF;
							}
							break;
						case VIEW_MODE.RGB_AO:
							{
								float	LinR = ImageUtility.ColorProfile.sRGB2Linear( ContentRGB[X,Y].x );
								float	LinG = ImageUtility.ColorProfile.sRGB2Linear( ContentRGB[X,Y].y );
								float	LinB = ImageUtility.ColorProfile.sRGB2Linear( ContentRGB[X,Y].z );
								float	LinAO = ContentRGB[X,Y].w;
								LinR *= LinAO;
								LinG *= LinAO;
								LinB *= LinAO;
								R = (byte) Math.Max( 0, Math.Min( 255, 255 * ImageUtility.ColorProfile.Linear2sRGB( LinR ) ) );
								G = (byte) Math.Max( 0, Math.Min( 255, 255 * ImageUtility.ColorProfile.Linear2sRGB( LinG ) ) );
								B = (byte) Math.Max( 0, Math.Min( 255, 255 * ImageUtility.ColorProfile.Linear2sRGB( LinB ) ) );
								*pScanline++ = B;
								*pScanline++ = G;
								*pScanline++ = R;
								*pScanline++ = 0xFF;
							}
							break;
					}
				}
			}
			m_Bitmap.UnlockBits( LockedBitmap );

			Refresh();
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
			if ( m_Bitmap != null )
			{
				RectangleF	Rect = ImageClientRect;
				e.Graphics.DrawImage( m_Bitmap, Rect, new RectangleF( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), GraphicsUnit.Pixel );
			}
			else if ( m_MessageOnEmpty != null )
			{
				SizeF	MessageSize = e.Graphics.MeasureString( m_MessageOnEmpty, Font );
				e.Graphics.DrawString( m_MessageOnEmpty, Font, Brushes.White, 0.5f * (Width-MessageSize.Width), 0.5f * (Height-MessageSize.Height) );
			}
		}
	}
}
