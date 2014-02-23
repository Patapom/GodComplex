using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TestFresnel
{
	public partial class OutputPanel : Panel
	{
		protected Bitmap	m_Bitmap = null;

		public enum		FRESNEL_TYPE
		{
			SCHLICK,
			PRECISE,
		}

		protected FRESNEL_TYPE	m_Type = FRESNEL_TYPE.SCHLICK;
		public  FRESNEL_TYPE	FresnelType
		{
			get { return m_Type; }
			set
			{
				m_Type = value;
				UpdateBitmap();
			}
		}

		protected float			m_IOR = 1.0f;
		public float			IOR
		{
			get { return m_IOR; }
			set
			{
				m_IOR = value;
				UpdateBitmap();
			}
		}

		protected Color			m_SpecularTint = Color.White;
		public Color			SpecularTint
		{
			get { return m_SpecularTint; }
			set
			{
				m_SpecularTint = value;
				UpdateBitmap();
			}
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

		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				G.DrawLine( Pens.Black, 10, 0, 10, Height );
				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );

				FresnelEval	Eval = null;
				switch ( m_Type ) 
				{
					case FRESNEL_TYPE.SCHLICK:	Eval = Fresnel_Schlick; PrepareSchlick(); break;
					case FRESNEL_TYPE.PRECISE:	Eval = Fresnel_Precise; PreparePrecise(); break;
				}

				DrawLine( G, 0, 1, 1, 1, Pens.Red );

				float	x = 0.0f;
				float	yr, yg, yb;
				Eval( 1.0f, out yr, out yg, out yb );
				for ( int X=10; X <= Width; X++ )
				{
					float	px = x;
					float	pyr = yr;
					float	pyg = yg;
					float	pyb = yb;
					x = (float) (X-10.0f) / (Width - 10);

					float	CosTheta = (float) Math.Cos( x * 0.5 * Math.PI );	// Cos(theta)

					Eval( CosTheta, out yr, out yg, out yb );

					DrawLine( G, px, pyr, x, yr, Pens.Red );
					DrawLine( G, px, pyg, x, yg, Pens.LimeGreen );
					DrawLine( G, px, pyb, x, yb, Pens.Blue );
				}

				Eval( 1.0f, out yr, out yg, out yb );
				float	F0 = Math.Max( Math.Max( yr, yg ), yb );
				G.DrawString( "F0 = " + F0, Font, Brushes.Black, 12.0f, Height - 30 - (Height-20) * F0 );
			}

			Invalidate();
		}

		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1 )
		{
			DrawLine( G, x0, y0, x1, y1, Pens.Black );
		}
		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1, Pen _Pen )
		{
			float	X0 = 10 + (Width-20) * x0;
			float	Y0 = Height - 10 - (Height-20) * y0;
			float	X1 = 10 + (Width-20) * x1;
			float	Y1 = Height - 10 - (Height-20) * y1;
			G.DrawLine( _Pen, X0, Y0, X1, Y1 );
		}

		protected delegate void		FresnelEval( float x, out float yr, out float yg, out float yb );

		// F0 = ((n2 - n1) / (n2 + n1))²
		// Assuming n1=1 (air)
		// We look for n2 so:
		//	n2 = (1+a)/(1-a) with a = sqrt(F0)
		protected float		F0r;
		protected float		F0g;
		protected float		F0b;
		protected void		PrepareSchlick()
		{
// 			var	IOR = (1+Math.sqrt(this.fresnelF0)) / (1-Math.sqrt(this.fresnelF0));
// 			if ( !isFinite( IOR ) )
// 				IOR = 1e30;	// Simply use a huge number instead...
			float	F0 = (float) Math.Pow( (m_IOR - 1.0) / (m_IOR + 1.0), 2.0 );
			F0r = F0 * m_SpecularTint.R / 255.0f;
			F0g = F0 * m_SpecularTint.G / 255.0f;
			F0b = F0 * m_SpecularTint.B / 255.0f;
		}
		protected void		Fresnel_Schlick( float _CosTheta, out float yr, out float yg, out float yb )
		{
			float	One_Minus_CosTheta = 1.0f - _CosTheta;
			float	One_Minus_CosTheta_Pow5 = One_Minus_CosTheta * One_Minus_CosTheta;
					One_Minus_CosTheta_Pow5 *= One_Minus_CosTheta_Pow5 * One_Minus_CosTheta;

			yr = F0r + (1.0f - F0r) * One_Minus_CosTheta_Pow5;
			yg = F0g + (1.0f - F0g) * One_Minus_CosTheta_Pow5;
			yb = F0b + (1.0f - F0b) * One_Minus_CosTheta_Pow5;
		}

		/// <summary>
		/// Stolen from §5.1 http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf
		/// 
		/// F = 1/2 * (g-c)²/(g+c)² * (1 + (c*(g+c) - 1)² / (c*(g-c) + 1)²)
		/// 
		/// where:
		///		g = sqrt( (n2/n1)² - 1 + c² )
		///		n2 = IOR
		///		n1 = 1 (air)
		///		c = cos(theta)
		///		theta = angle between normal and half vector
		/// </summary>
		protected void		PreparePrecise()
		{
			float	F0 = (float) Math.Pow( (m_IOR - 1.0) / (m_IOR + 1.0), 2.0 );
			F0r = F0 * m_SpecularTint.R / 255.0f;
			F0g = F0 * m_SpecularTint.G / 255.0f;
			F0b = F0 * m_SpecularTint.B / 255.0f;

			F0r = (float) ((1.0+Math.Sqrt( F0r )) / (1.0-Math.Sqrt( F0r )));
			F0g = (float) ((1.0+Math.Sqrt( F0g )) / (1.0-Math.Sqrt( F0g )));
			F0b = (float) ((1.0+Math.Sqrt( F0b )) / (1.0-Math.Sqrt( F0b )));
		}
		protected void		Fresnel_Precise( float _CosTheta, out float yr, out float yg, out float yb )
		{
			float	c = _CosTheta;
// 			double	g = Math.Sqrt( m_IOR*m_IOR - 1.0 + c*c );
// 			float	F = (float) (0.5 * Math.Pow( (g-c) / (g+c), 2.0 ) * (1.0 + Math.Pow( (c*(g+c) - 1) / (c*(g-c) + 1), 2.0 )) );


			double	g = Math.Sqrt( F0r*F0r - 1.0 + c*c );
			yr = (float) (0.5 * Math.Pow( (g-c) / (g+c), 2.0 ) * (1.0 + Math.Pow( (c*(g+c) - 1) / (c*(g-c) + 1), 2.0 )) );
			g = Math.Sqrt( F0g*F0g - 1.0 + c*c );
			yg = (float) (0.5 * Math.Pow( (g-c) / (g+c), 2.0 ) * (1.0 + Math.Pow( (c*(g+c) - 1) / (c*(g-c) + 1), 2.0 )) );
			g = Math.Sqrt( F0b*F0b - 1.0 + c*c );
			yb = (float) (0.5 * Math.Pow( (g-c) / (g+c), 2.0 ) * (1.0 + Math.Pow( (c*(g+c) - 1) / (c*(g-c) + 1), 2.0 )) );

// 			yr = F * F0r;
// 			yg = F * F0g;
// 			yb = F * F0b;
// 			yr = F0r + (1.0f - F0r) * F;
// 			yg = F0g + (1.0f - F0g) * F;
// 			yb = F0b + (1.0f - F0b) * F;
// 			yr = F0r * (1-F) + F;
// 			yg = F0g * (1-F) + F;
// 			yb = F0b * (1-F) + F;
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
