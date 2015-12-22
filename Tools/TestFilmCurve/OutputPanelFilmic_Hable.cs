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
	public partial class OutputPanelFilmic_Hable : Panel
	{
		protected Bitmap	m_Bitmap = null;

		protected float		m_ScaleX = 1.0f;
		protected float		m_ScaleY = 1.0f;
		protected float		m_WhitePoint = 10.0f;
		protected float		m_A = 0.15f;
		protected float		m_B = 0.5f;
		protected float		m_C = 0.1f;
		protected float		m_D = 0.2f;
		protected float		m_E = 0.02f;
		protected float		m_F = 0.3f;

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

		public float	WhitePoint
		{
			get { return m_WhitePoint; }
			set { m_WhitePoint = value; UpdateBitmap(); }
		}

		public float	A
		{
			get { return m_A; }
			set { m_A = value; UpdateBitmap(); }
		}

		public float	B
		{
			get { return m_B; }
			set { m_B = value; UpdateBitmap(); }
		}

		public float	C
		{
			get { return m_C; }
			set { m_C = value; UpdateBitmap(); }
		}

		public float	D
		{
			get { return m_D; }
			set { m_D = value; UpdateBitmap(); }
		}

		public float	E
		{
			get { return m_E; }
			set { m_E = value; UpdateBitmap(); }
		}

		public float	F
		{
			get { return m_F; }
			set { m_F = value; UpdateBitmap(); }
		}

		public bool		ShowDebugLuminance {
			get { return m_ShowDebugLuminance; }
			set { m_ShowDebugLuminance = value; UpdateBitmap(); }
		}

		public float	DebugLuminance {
			get { return m_DebugLuminance; }
			set { m_DebugLuminance = value; if ( m_ShowDebugLuminance ) UpdateBitmap(); }
		}

		public OutputPanelFilmic_Hable()
		{
			InitializeComponent();
		}

		public OutputPanelFilmic_Hable( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

// float A = 0.15;
// float B = 0.50;
// float C = 0.10;
// float D = 0.20;
// float E = 0.02;
// float F = 0.30;
// float W = 11.2;
// 
// float3 Uncharted2Tonemap(float3 x)
// {
//    return ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;
// }
// 
// float4 ps_main( float2 texCoord  : TEXCOORD0 ) : COLOR
// {
//    float3 texColor = tex2D(Texture0, texCoord );
//    texColor *= 16;  // Hardcoded Exposure Adjustment
// 
//    float ExposureBias = 2.0f;
//    float3 curr = Uncharted2Tonemap(ExposureBias*texColor);
// 
//    float3 whiteScale = 1.0f/Uncharted2Tonemap(W);
//    float3 color = curr*whiteScale;
// 
//    float3 retColor = pow(color,1/2.2);
//    return float4(retColor,1);
// }

		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null || m_ScaleX <= 1.0f )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				G.DrawLine( Pens.Black, 10, 0, 10, Height );
				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );

				for ( int i=1; i <= 10; i++ ) {
					float	height = 0.02f * (1.0f + (i==1 ? 1 : 0));
					DrawLine( G, Pens.Black, LogLuminance2Client( 0.01f * i ), 0.0f, LogLuminance2Client( 0.01f * i ), height );
					DrawLine( G, Pens.Black, LogLuminance2Client( 0.1f * i ), 0.0f, LogLuminance2Client( 0.1f * i ), height );
					DrawLine( G, Pens.Black, LogLuminance2Client( 1.0f * i ), 0.0f, LogLuminance2Client( 1.0f * i ), height );
					DrawLine( G, Pens.Black, LogLuminance2Client( 10.0f * i ), 0.0f, LogLuminance2Client( 10.0f * i ), height );
				}

				DrawLine( G, Pens.LightGray, 10, 1.0f, Width, 1.0f );

				float	Xw = LogLuminance2Client( m_WhitePoint );
				DrawLine( G, Pens.LightGreen, Xw, 0.0f, Xw, 1.0f );

// 				if ( m_ShowDebugLuminance ) {
// 					float	Xd = LogLuminance2Client( m_DebugLuminance );
// 					DrawLine( G, Pens.Gold, Xd, 0.0f, Xd, 1.0f );
// 				}
 
				float	py = GetFilmicCurve( Client2LogLuminance( 10 ) );
				for ( int X=11; X < Width; X++ ) {
					float	y = GetFilmicCurve( Client2LogLuminance( X ) );
					DrawLine( G, Pens.Black, X-1, py, X, y );
					py = y;
				}

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

		float		Client2LogLuminance( float _ClientX ) {
			float	x = (float) (_ClientX - 10) / (Width - 20);	// in [0,1]
			return (float) Math.Pow( 10.0, -2.0 + (2.0 + Math.Log10( m_ScaleX )) * x );
		}

		float		LogLuminance2Client( float _Luminance ) {
			return 10.0f + (Width-20) * (float) ((2.0 + Math.Log10( _Luminance )) / (2.0 + Math.Log10( m_ScaleX )));
		}
 
// 		float		Client2LogLuminance( float _ClientX ) {
// 			float	x = (float) (_ClientX - 10) / (Width - 20);	// in [0,1]
// 			double	logScale = Math.Log10( m_ScaleX );
// 			x = (float) Math.Pow( 10.0, logScale * x );
// 			return x;
// 		}
// 
// 		float		LogLuminance2Client( float _logLuminance ) {
// 			double	logScale = Math.Log10( m_ScaleX );
// 			float	x = (float) (Math.Log10( _logLuminance ) / logScale);
// 			return 10.0f + (Width-20) * x;
// 		}

		protected void		DrawLine( Graphics G, Pen P, float x0, float y0, float x1, float y1 ) {
			float	Y0 = Height - 10 - (Height-20) * m_ScaleY * y0;
			float	Y1 = Height - 10 - (Height-20) * m_ScaleY * y1;
			G.DrawLine( P, x0, Y0, x1, Y1 );
		}

		protected float		Filmic( float x )
		{
//			x = (float) Math.Log10( 1.0 + x );
			x = (float) Math.Pow( 10.0, x-2.0 );

// 			const float A = 0.15f;
// 			const float B = 0.50f;
// 			const float C = 0.10f;
// 			const float D = 0.20f;
// 			const float E = 0.02f;
// 			const float F = 0.30f;
//			const float W = 11.2f;

			return ((x * (m_A*x + m_C*m_B) + m_D*m_E) / (x * (m_A*x + m_B) + m_D*m_F)) - m_E / m_F;
		}
		protected float		GetFilmicCurve( float x )
		{
			float	y = Filmic( x ) / Filmic( m_WhitePoint );
			if ( float.IsInfinity( y ) || float.IsNaN( y ) )
				y = 0.0f;
			return y;
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
