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

using SharpMath;

namespace GenerateSelfShadowedBumpMap
{
	public partial class ImagePanel : Panel {
		public enum		VIEW_MODE {
			RGB,
			RGB_TIMES_AO,
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

		private VIEW_MODE		m_ViewMode = VIEW_MODE.RGB;
		public VIEW_MODE		ViewMode {
			get { return m_ViewMode; }
			set {
				m_ViewMode = value;
				UpdateBitmap();
			}
		}

		private bool		m_viewLinear = false;
		public bool			ViewLinear {
			get { return m_viewLinear; }
			set {
				m_viewLinear = value;
				UpdateBitmap();
			}
		}

		public ImagePanel( IContainer container ) {
			container.Add( this );
			InitializeComponent();
		}

		private ImageUtility.ImageFile	m_sourceImage = null;
		public ImageUtility.ImageFile	Image
		{
			get { return m_sourceImage; }
			set {
				m_sourceImage = value;
				UpdateBitmap();
			}
		}

		private RectangleF		ImageClientRect {
			get {
				int		SizeX = (int) m_sourceImage.Width;
				int		SizeY = (int) m_sourceImage.Height;

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

 		private ImageUtility.ColorProfile	m_ProfilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.ColorProfile	m_ProfileLinear = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.Chromaticities.sRGB, ImageUtility.ColorProfile.GAMMA_CURVE.STANDARD, 1.0f );

		private unsafe void	UpdateBitmap() {
			if ( m_sourceImage == null )
				return;

			// Fill pixel per pixel
			uint	W = m_sourceImage.Width;
			uint	H = m_sourceImage.Height;
			if ( m_Bitmap != null && (m_Bitmap.Width != W || m_Bitmap.Height != H) ) {
				m_Bitmap.Dispose();
				m_Bitmap = null;
			}
			if ( m_Bitmap == null )
				m_Bitmap = new Bitmap( (int) W, (int) H, PixelFormat.Format32bppArgb );

			switch ( m_ViewMode ) {
				case VIEW_MODE.RGB:
					m_sourceImage.AsCustomBitmap( m_Bitmap, ( ref float4 _color ) => {
						if ( !m_viewLinear ) {
							_color.x = ImageUtility.ColorProfile.Linear2sRGB( _color.x );
							_color.y = ImageUtility.ColorProfile.Linear2sRGB( _color.y );
							_color.z = ImageUtility.ColorProfile.Linear2sRGB( _color.z );
						}
						_color.w = 1.0f;
					} );
					break;
				case VIEW_MODE.R:
					m_sourceImage.AsCustomBitmap( m_Bitmap, ( ref float4 _color ) => {
						_color.x = m_viewLinear ? _color.x : ImageUtility.ColorProfile.Linear2sRGB( _color.x );
						_color.y = _color.z = _color.x;
						_color.w = 1.0f;
					} );
					break;
				case VIEW_MODE.G:
					m_sourceImage.AsCustomBitmap( m_Bitmap, ( ref float4 _color ) => {
						_color.y = m_viewLinear ? _color.y : ImageUtility.ColorProfile.Linear2sRGB( _color.y );
						_color.x = _color.z = _color.y;
						_color.w = 1.0f;
					} );
					break;
				case VIEW_MODE.B:
					m_sourceImage.AsCustomBitmap( m_Bitmap, ( ref float4 _color ) => {
						_color.z = m_viewLinear ? _color.z : ImageUtility.ColorProfile.Linear2sRGB( _color.z );
						_color.x = _color.y = _color.z;
						_color.w = 1.0f;
					} );
					break;
				case VIEW_MODE.AO:
					m_sourceImage.AsCustomBitmap( m_Bitmap, ( ref float4 _color ) => {
						_color.x = _color.y = _color.z = m_viewLinear ? _color.w : ImageUtility.ColorProfile.Linear2sRGB( _color.w );
						_color.w = 1.0f;
					} );
					break;
				case VIEW_MODE.AO_FROM_RGB:
					m_sourceImage.AsCustomBitmap( m_Bitmap, ( ref float4 _color ) => {
						float	LinR = ImageUtility.ColorProfile.sRGB2Linear( _color.x );
						float	LinG = ImageUtility.ColorProfile.sRGB2Linear( _color.y );
						float	LinB = ImageUtility.ColorProfile.sRGB2Linear( _color.z );
						float	LinAO = (float) Math.Sqrt( LinR*LinR + LinG*LinG + LinB*LinB ) * 0.57735026918962576450914878050196f;	// divided by sqrt(3)
						_color.x = _color.y = _color.z = ImageUtility.ColorProfile.Linear2sRGB( LinAO );
						_color.w = 1.0f;
					} );
					break;
				case VIEW_MODE.RGB_TIMES_AO:
					m_sourceImage.AsCustomBitmap( m_Bitmap, ( ref float4 _color ) => {
						float	LinR = ImageUtility.ColorProfile.sRGB2Linear( _color.x );
						float	LinG = ImageUtility.ColorProfile.sRGB2Linear( _color.y );
						float	LinB = ImageUtility.ColorProfile.sRGB2Linear( _color.z );
						_color.x = ImageUtility.ColorProfile.Linear2sRGB( _color.w * LinR );
						_color.y = ImageUtility.ColorProfile.Linear2sRGB( _color.w * LinG );
						_color.z = ImageUtility.ColorProfile.Linear2sRGB( _color.w * LinB );
						_color.w = 1.0f;
					} );
					break;
			}

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
			if ( m_Bitmap != null ) {
				RectangleF	Rect = ImageClientRect;
				e.Graphics.DrawImage( m_Bitmap, Rect, new RectangleF( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), GraphicsUnit.Pixel );
			} else if ( m_MessageOnEmpty != null ) {
				SizeF	MessageSize = e.Graphics.MeasureString( m_MessageOnEmpty, Font );
				e.Graphics.DrawString( m_MessageOnEmpty, Font, Brushes.White, 0.5f * (Width-MessageSize.Width), 0.5f * (Height-MessageSize.Height) );
			}
		}
	}
}
