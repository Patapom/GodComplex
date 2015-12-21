using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestFilmicCurve
{
	public partial class OutputPanelFilmic_Insomniac : Panel
	{
		protected Bitmap	m_Bitmap = null;

		protected float		m_ScaleX = 1.0f;
		protected float		m_ScaleY = 1.0f;
		protected float		m_BlackPoint = 0.0f;
		protected float		m_WhitePoint = 10.0f;
		protected float		m_JunctionPoint = 0.5f;
		protected float		m_ToeStrength = 0.0f;
		protected float		m_ShoulderStrength = 0.0f;
		private float		k;

		protected bool		m_ShowDebugLuminance = false;
		protected float		m_DebugLuminance = 1.0f;

		public float	ScaleX
		{
			get { return m_ScaleX; }
			set { m_ScaleX = value; UpdateBitmap(); }
		}

		public float	ScaleY
		{
			get { return m_ScaleY; }
			set { m_ScaleY = value; UpdateBitmap(); }
		}

		public float	BlackPoint
		{
			get { return m_BlackPoint; }
			set { m_BlackPoint = value; UpdateBitmap(); }
		}

		public float	WhitePoint
		{
			get { return m_WhitePoint; }
			set { m_WhitePoint = value; UpdateBitmap(); }
		}

		public float	JunctionPoint
		{
			get { return m_JunctionPoint; }
			set { m_JunctionPoint = value; UpdateBitmap(); }
		}

		public float	ToeStrength
		{
			get { return m_ToeStrength; }
			set { m_ToeStrength = value; UpdateBitmap(); }
		}

		public float	ShoulderStrength
		{
			get { return m_ShoulderStrength; }
			set { m_ShoulderStrength = value; UpdateBitmap(); }
		}

		public bool		ShowDebugLuminance {
			get { return m_ShowDebugLuminance; }
			set { m_ShowDebugLuminance = value; UpdateBitmap(); }
		}

		public float	DebugLuminance {
			get { return m_DebugLuminance; }
			set { m_DebugLuminance = value; if ( m_ShowDebugLuminance ) UpdateBitmap(); }
		}

		public OutputPanelFilmic_Insomniac()
		{
			InitializeComponent();
		}

		public OutputPanelFilmic_Insomniac( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null || m_ScaleX <= 1.0f )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				G.DrawLine( Pens.Black, 10, 0, 10, Height );
				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );

				// Compute junction factor
				k = (1.0f - m_ToeStrength) * (m_JunctionPoint - m_BlackPoint) / ((1.0f - m_ShoulderStrength) * (m_WhitePoint - m_JunctionPoint) + (1.0f - m_ToeStrength) * (m_JunctionPoint - m_BlackPoint));

				float	py = Filmic( Client2Luminance( 10 ) );
				for ( int X=11; X < Width; X++ ) {
					float	y = Filmic( Client2Luminance( X ) );
					DrawLine( G, Pens.Black, X-1, py, X, y );
					py = y;
				}

				DrawLine( G, Pens.LightGray, 10, 1.0f, Width, 1.0f );

				float	Xw = Luminance2Client( m_WhitePoint );
				DrawLine( G, Pens.LightGreen, Xw, 0.0f, Xw, 1.0f );

// 				if ( m_ShowDebugLuminance ) {
// 					float	Xd = LogLuminance2Client( m_DebugLuminance );
// 					DrawLine( G, Pens.Gold, Xd, 0.0f, Xd, 1.0f );
// 				}
// 
// 				float	py = GetFilmicCurve( Client2LogLuminance( 10 ) );
// 				for ( int X=11; X < Width; X++ ) {
// 					float	y = GetFilmicCurve( Client2LogLuminance( X ) );
// 					DrawLine( G, Pens.Black, X-1, py, X, y );
// 					py = y;
// 				}

			}

			Invalidate();
		}

		float		Client2Luminance( float _ClientX ) {
			float	x = (float) (_ClientX - 10) / (Width - 20);	// in [0,1]
			return m_ScaleX * x;
		}

		float		Luminance2Client( float _Luminance ) {
			return 10.0f + (Width-20) * (_Luminance / m_ScaleX);
		}

		protected void		DrawLine( Graphics G, Pen P, float x0, float y0, float x1, float y1 ) {
			float	Y0 = Height - 10 - (Height-20) * m_ScaleY * y0;
			float	Y1 = Height - 10 - (Height-20) * m_ScaleY * y1;
			G.DrawLine( P, x0, Y0, x1, Y1 );
		}

		protected float		Filmic( float x ) {
			if ( x < m_JunctionPoint ) {
				return k * (1.0f - m_ToeStrength) * (x - m_BlackPoint) / ((m_JunctionPoint - m_BlackPoint) - m_ToeStrength * (x - m_BlackPoint));
			} else {
				return k + (1.0f - k) * (x - m_JunctionPoint) / ((1.0f - m_ShoulderStrength) * (m_WhitePoint - m_JunctionPoint) + m_ShoulderStrength * (x - m_JunctionPoint));
			}
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

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
	}
}
