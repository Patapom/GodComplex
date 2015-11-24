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
	public partial class OutputPanel2 : Panel
	{
		private const float	MIN_IOR = 0.1f;		// Minimum plotted IOR
//		private const float	MAX_IOR = 10.0f;	// Maximum plotted IOR

		protected Bitmap	m_Bitmap = null;

		public enum		FRESNEL_TYPE
		{
			SCHLICK,
			PRECISE,
		}

		protected FRESNEL_TYPE	m_Type = FRESNEL_TYPE.SCHLICK;
		public  FRESNEL_TYPE	FresnelType {
			get { return m_Type; }
			set {
				m_Type = value;
				UpdateBitmap();
			}
		}

		protected float			m_IOR = 1.0f;
		public float			IOR {
			get { return m_IOR; }
			set {
				m_IOR = value;
				UpdateBitmap();
			}
		}

		protected float			m_MaxIOR = 10.0f;
		public float			MaxIOR {
			get { return m_MaxIOR; }
			set {
				m_MaxIOR = value;
				UpdateBitmap();
			}
		}

		protected float			m_VerticalScale = 1.0f;
		public float			VerticalScale {
			get { return m_VerticalScale; }
			set {
				m_VerticalScale = value;
				UpdateBitmap();
			}
		}

		public OutputPanel2()
		{
			InitializeComponent();
		}

		public OutputPanel2( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		protected void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) ) {
				G.FillRectangle( Brushes.White, 0, 0, Width, Height );

				G.DrawLine( Pens.Black, 10, 0, 10, Height );
				G.DrawLine( Pens.Black, 0, Height-10, Width, Height-10 );

				FresnelEval	Eval = null;
				switch ( m_Type ) {
					case FRESNEL_TYPE.SCHLICK:	Eval = Fresnel_Schlick; break;
					case FRESNEL_TYPE.PRECISE:	Eval = Fresnel_Precise; break;
				}

				DrawLine( G, 0, 1, 1, 1, Pens.Gray );

				float	x = 0.0f;
				float	yr, yg, yb, yy;
				Eval( MIN_IOR, out yr, out yg, out yb, out yy );
				for ( int X=10; X <= Width; X++ )
				{
					float	px = x;
					float	pyr = yr;
					float	pyg = yg;
					float	pyb = yb;
					float	pyy = yy;
					x = (float) (X-10.0f) / (Width - 10);

					float	CurrentIOR = MIN_IOR + x * (m_MaxIOR - MIN_IOR);

					Eval( CurrentIOR, out yr, out yg, out yb, out yy );

					DrawLine( G, px, pyr, x, yr, Pens.Red );
					DrawLine( G, px, pyg, x, yg, Pens.LimeGreen );
					DrawLine( G, px, pyb, x, yb, Pens.Blue );
					DrawLine( G, px, pyy, x, yy, Pens.Gold );
				}

				Eval( m_IOR, out yr, out yg, out yb, out yy );

				string	T = "Fdr = " + yr.ToString( "G4" ) + "\r\nFdr_in = " + yg.ToString( "G4" ) + "\r\nFdr_a = " + yb.ToString( "G4" );
				SizeF	TextSize = G.MeasureString( T, Font );
				G.DrawString( T, Font, Brushes.Black, Width - TextSize.Width - 12.0f, Height - TextSize.Height - 20 );

				// Show current IOR
				float	x_IOR = (IOR - MIN_IOR) / (m_MaxIOR - MIN_IOR);
				DrawLine( G, x_IOR, 0.0f, x_IOR, 1.0f, Pens.Black );
			}

			Invalidate();
		}

		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1 )
		{
			DrawLine( G, x0, y0, x1, y1, Pens.Black );
		}
		protected void		DrawLine( Graphics G, float x0, float y0, float x1, float y1, Pen _Pen )
		{
			y0 *= m_VerticalScale;
			y1 *= m_VerticalScale;

			float	X0 = 10 + (Width-20) * x0;
			float	Y0 = Height - 10 - (Height-20) * y0;
			float	X1 = 10 + (Width-20) * x1;
			float	Y1 = Height - 10 - (Height-20) * y1;
			G.DrawLine( _Pen, X0, Y0, X1, Y1 );
		}

		protected delegate void		FresnelEval( float x, out float yr, out float yg, out float yb, out float yy );

		/// <summary>
		/// Analytic expression for Fdr as found in 
		/// </summary>
		/// <param name="_eta"></param>
		/// <returns></returns>
		float	FdrAnalytic( float _eta ) {
			float	result;
			if ( _eta >= 1.0f ) {
				result = -1.4399f / (_eta*_eta) + 0.7099f / _eta + 0.6681f + 0.0636f * _eta; 
			} else {
				result = -0.4399f + 0.7099f / _eta - 0.3319f / (_eta * _eta) + 0.0636f / (_eta * _eta * _eta); 
			}
			return result;
		}

		const int		THETA_COUNT = 100;
		const double	DTheta = 0.5 * Math.PI / THETA_COUNT;

		#region Schlick

		/// <summary>
		/// Computes the integral of the Fresnel diffuse reflection
		/// </summary>
		/// <param name="_IOR"></param>
		/// <returns></returns>
		float	FdrIntegral_Schlick( float _IOR ) {

			// F0 = ((n2 - n1) / (n2 + n1))²
			double	F0 = (_IOR-1.0f) / (_IOR+1.0f);
					F0 *= F0;

			double	Fdr = 0.0;
			for ( int i=0; i < THETA_COUNT; i++ ) {
				double	theta = (i+0.5) * DTheta;
				double	sinTheta = Math.Sin( theta );
				double	cosTheta = Math.Sqrt( 1.0 - sinTheta*sinTheta );
				double	dTheta = sinTheta * DTheta;

				double	One_Minus_CosTheta = 1.0 - cosTheta;
				double	One_Minus_CosTheta_Pow5 = One_Minus_CosTheta * One_Minus_CosTheta;
						One_Minus_CosTheta_Pow5 *= One_Minus_CosTheta_Pow5 * One_Minus_CosTheta;
				double	fresnel = F0 + (1.0 - F0) * One_Minus_CosTheta_Pow5;

				double	value = fresnel * cosTheta * dTheta;
//				double	value = fresnel * dTheta;
				Fdr += value;
			}

			Fdr *= 2.0;
//			Fdr *= 2.0 * Math.PI;

			return (float) Fdr;
		}

		protected void		Fresnel_Schlick( float _IOR, out float yr, out float yg, out float yb, out float yy ) {
			yr = FdrIntegral_Schlick( _IOR );
			yg = 1.0f - (1.0f - yr) / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12


			float	eta = 1.0f / _IOR;	// eta is "the relative index of refraction of the medium with the reflected ray to the other medium"
										// So eta = IOR_air / IOR_other

			yb = FdrAnalytic( eta );
			yy = 1.0f - (1.0f - yb) / (eta*eta);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12

//yg = (1.0f - yr) / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12

		}

		#endregion

		#region Precise

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
		/// <summary>
		/// Computes the integral of the Fresnel diffuse reflection
		/// </summary>
		/// <param name="_IOR"></param>
		/// <returns></returns>
		float	FdrIntegral_Precise( float _IOR ) {

			_IOR = Math.Max( 1.0f, _IOR );

			double	Fdr = 0.0;
			for ( int i=0; i < THETA_COUNT; i++ ) {
				double	theta = (i+0.5) * DTheta;
				double	sinTheta = Math.Sin( theta );
				double	cosTheta = Math.Sqrt( 1.0 - sinTheta*sinTheta );
				double	dTheta = sinTheta * DTheta;

// 				double	One_Minus_CosTheta = 1.0 - cosTheta;
// 				double	One_Minus_CosTheta_Pow5 = One_Minus_CosTheta * One_Minus_CosTheta;
// 						One_Minus_CosTheta_Pow5 *= One_Minus_CosTheta_Pow5 * One_Minus_CosTheta;
// 				double	fresnel = F0 + (1.0 - F0) * One_Minus_CosTheta_Pow5;

				double	c = cosTheta;
	 			double	g = Math.Sqrt( _IOR*_IOR - 1.0 + c*c );
				double	fresnel = 0.5 * Math.Pow( (g-c) / (g+c), 2.0 ) * (1.0 + Math.Pow( (c*(g+c) - 1) / (c*(g-c) + 1), 2.0 ));

				double	value = fresnel * cosTheta * dTheta;
//				double	value = fresnel * dTheta;
				Fdr += value;
			}

			Fdr *= 2.0;
//			Fdr *= 2.0 * Math.PI;

			return (float) Fdr;
		}

		protected void		Fresnel_Precise( float _IOR, out float yr, out float yg, out float yb, out float yy ) {
			yr = FdrIntegral_Precise( _IOR );
			yg = 1.0f - (1.0f - yr) / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12


			float	eta = 1.0f / _IOR;	// eta is "the relative index of refraction of the medium with the reflected ray to the other medium"
										// So eta = IOR_air / IOR_other

			yb = FdrAnalytic( eta );
			yy = 1.0f - (1.0f - yb) / (eta*eta);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12


//yg = (1.0f - yr) / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12

		}
		#endregion

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
