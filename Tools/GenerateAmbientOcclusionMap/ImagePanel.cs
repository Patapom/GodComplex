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
	public partial class ImagePanel : Panel
	{
		private Bitmap				m_Bitmap = null;

		private string				m_MessageOnEmpty = null;
		public string				MessageOnEmpty {
			get { return m_MessageOnEmpty; }
			set { m_MessageOnEmpty = value; Invalidate(); }
		}

		private float				m_Brightness = 0.0f;
		private float				m_Contrast = 0.0f;
		private float				m_Gamma = 0.0f;

		public Bitmap		Bitmap {
			get { return m_Bitmap; }
			set {
				m_Bitmap = value;
				UpdateBitmap();
			}
		}

		public float		Brightness {
			get { return m_Brightness; }
			set {
				m_Brightness = value;
				UpdateBitmap();
			}
		}

		public float		Contrast {
			get { return m_Contrast; }
			set {
				m_Contrast = value;
				UpdateBitmap();
			}
		}

		public float		Gamma {
			get { return m_Gamma; }
			set {
				m_Gamma = value;
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

		private RectangleF		ImageClientRect {
			get {
				int		SizeX = m_Bitmap.Width;
				int		SizeY = m_Bitmap.Height;

				int		WidthIfVertical = SizeX * Height / SizeY;	// Client width of the image if fitting vertically
				int		HeightIfHorizontal = SizeY * Width / SizeX;	// Client height of the image if fitting horizontally

				if ( WidthIfVertical > Width ) {
					// Fit horizontally
					return new RectangleF( 0, 0.5f * (Height-HeightIfHorizontal), Width, HeightIfHorizontal );
				}
				else {
					// Fit vertically
					return new RectangleF( 0.5f * (Width-WidthIfVertical), 0, WidthIfVertical, Height );
				}
			}
		}

#if !NO64
 		private ImageUtility.ColorProfile	m_ProfilesRGB = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.ColorProfile	m_ProfileLinear = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR );

//		public ImagePanel( IContainer container )
		public ImagePanel()
		{
//			container.Add( this );
			InitializeComponent();
		}

		public float		ApplyBrightnessContrastGamma( float _Source, float _Brightness, float _Contrast, float _Gamma ) {
			_Source += _Brightness;
			_Source = 0.5f + (_Source - 0.5f) * (1.0f + _Contrast);
			_Source = Math.Max( 0.0f, Math.Min( 1.0f, _Source ) );
			_Source = (float) Math.Pow( _Source, 1.0f + _Gamma );
			return _Source;
		}

		public float4[,]	ApplyBrightnessContrastGamma( float4[,] _Source, float _Brightness, float _Contrast, float _Gamma ) {
			int		W = _Source.GetLength( 0 );
			int		H = _Source.GetLength( 1 );
			float4[,]	Result = new float4[W,H];

			for ( int Y=0; Y < H; Y++ )
				for ( int X=0; X < W; X++ ) {
					float4	RGBA = _Source[X,Y];
					RGBA.x = ApplyBrightnessContrastGamma( RGBA.x, _Brightness, _Contrast, _Gamma );
					RGBA.y = ApplyBrightnessContrastGamma( RGBA.y, _Brightness, _Contrast, _Gamma );
					RGBA.z = ApplyBrightnessContrastGamma( RGBA.z, _Brightness, _Contrast, _Gamma );

					Result[X,Y] = RGBA;
				}

			return Result;
		}
#endif

		protected override void OnSizeChanged( EventArgs e ) {
			base.OnSizeChanged( e );
			Invalidate();
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e ) {
			base.OnPaint( e );

			e.Graphics.FillRectangle( Brushes.Black, 0, 0, Width, Height );
			if ( m_Bitmap != null )
			{
				RectangleF	Rect = ImageClientRect;
				e.Graphics.DrawImage( m_Bitmap, Rect, new RectangleF( 0, 0, m_Bitmap.Width, m_Bitmap.Height ), GraphicsUnit.Pixel );
			}
			else if ( m_MessageOnEmpty != null )
			{
				SizeF	MessageSize = e.Graphics.MeasureString( m_MessageOnEmpty, Font, Width );
				e.Graphics.DrawString( m_MessageOnEmpty, Font, Brushes.White, 0.5f * (Width-MessageSize.Width), 0.5f * (Height-MessageSize.Height) );
			}
		}

		private unsafe void	UpdateBitmap() {
			Refresh();
		}
	}
}
