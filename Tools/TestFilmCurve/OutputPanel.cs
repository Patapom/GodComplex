using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestGradientPNG
{
	public partial class OutputPanel : Panel
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

		public OutputPanel()
		{
			InitializeComponent();
		}

		public OutputPanel( IContainer container )
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
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				G.DrawLine( Pens.Black, 10, 0, 10, Height );
				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );

				float	x = 0.0f;
				float	y = GetFilmicCurve( x );
				for ( int X=10; X < Width; X++ )
				{
					float	px = x;
					float	py = y;
					x = (float) X / (Width - 20);
					y = m_ScaleY * GetFilmicCurve( m_ScaleX * x );

					DrawLine( G, px, py, x, y );
				}
			}

			Invalidate();
		}

		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1 )
		{
			float	X0 = 10 + (Width-20) * x0;
			float	Y0 = Height - 10 - (Height-20) * y0;
			float	X1 = 10 + (Width-20) * x1;
			float	Y1 = Height - 10 - (Height-20) * y1;
			G.DrawLine( Pens.Black, X0, Y0, X1, Y1 );
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
