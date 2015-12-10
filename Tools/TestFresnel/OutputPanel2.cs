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
			TABLE,		// Precomputed table created from the TestAreaLight "precompute BRDF" method
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

		public float[,]			m_Table = null;

		protected float			m_Roughness = 1.0f;
		public float			Roughness {
			get { return m_Roughness; }
			set {
				m_Roughness = value;
				UpdateBitmap();
			}
		}

		protected float			m_PeakFactor = 1.0f;
		public float			PeakFactor {
			get { return m_PeakFactor; }
			set {
				m_PeakFactor = value;
				UpdateBitmap();
			}
		}

		protected bool			m_PlotAgainstF0 = false;
		public bool				PlotAgainstF0 {
			get { return m_PlotAgainstF0; }
			set {
				m_PlotAgainstF0 = value;
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
					case FRESNEL_TYPE.TABLE:	Eval = Fresnel_Table; break;
				}

				DrawLine( G, 0, 1, 1, 1, Pens.Gray );

				float	x = 0.0f;
				float	yr, yg, yb, yy;

				if ( !m_PlotAgainstF0 ) {
					// Plot against IOR
					Eval( MIN_IOR, out yr, out yg, out yb, out yy );
					for ( int X=10; X <= Width; X++ ) {
						float	px = x;
						float	pyr = yr;
						float	pyg = yg;
						float	pyb = yb;
						float	pyy = yy;
						x = (float) (X-10.0f) / (Width - 10);

						float	CurrentIOR = MIN_IOR + x * (m_MaxIOR - MIN_IOR);

						Eval( CurrentIOR, out yr, out yg, out yb, out yy );

						Clamp( ref yr );
						Clamp( ref yg );
						Clamp( ref yb );
						Clamp( ref yy );

						DrawLine( G, px, pyr, x, yr, Pens.Red );
						DrawLine( G, px, pyg, x, yg, Pens.LimeGreen );
						DrawLine( G, px, pyb, x, yb, Pens.Blue );
						DrawLine( G, px, pyy, x, yy, Pens.Gold );
					}

					// Show current IOR
					float	x_IOR = (IOR - MIN_IOR) / (m_MaxIOR - MIN_IOR);
					DrawLine( G, x_IOR, 0.0f, x_IOR, 1.0f, Pens.Black );
				} else {
					// plot against F0
					Eval( Form1.F0_to_IOR( 0.0f ), out yr, out yg, out yb, out yy );
					for ( int X=10; X <= Width; X++ ) {
						float	px = x;
						float	pyr = yr;
						float	pyg = yg;
						float	pyb = yb;
						float	pyy = yy;
						x = (float) (X-10.0f) / (Width - 10);

						float	CurrentIOR = Form1.F0_to_IOR( x );

						Eval( CurrentIOR, out yr, out yg, out yb, out yy );

						Clamp( ref yr );
						Clamp( ref yg );
						Clamp( ref yb );
						Clamp( ref yy );

						DrawLine( G, px, pyr, x, yr, Pens.Red );
						DrawLine( G, px, pyg, x, yg, Pens.LimeGreen );
						DrawLine( G, px, pyb, x, yb, Pens.Blue );
						DrawLine( G, px, pyy, x, yy, Pens.Gold );
					}

					// Show current IOR
					float	x_IOR = Form1.IOR_to_F0( IOR );
					DrawLine( G, x_IOR, 0.0f, x_IOR, 1.0f, Pens.Black );
				}

				// Draw values at current IOR
				Eval( m_IOR, out yr, out yg, out yb, out yy );
				string	T = "R = " + yr.ToString( "G4" ) + "\r\nG = " + yg.ToString( "G4" ) + "\r\nB = " + yb.ToString( "G4" ) + "\r\nA = " + yy.ToString( "G4" );
				SizeF	TextSize = G.MeasureString( T, Font );
				G.DrawString( T, Font, Brushes.Black, Width - TextSize.Width - 12.0f, Height - TextSize.Height - 20 );
			}

			Invalidate();
		}

		void	Clamp( ref float x ) {
			x = Math.Max( -100.0f, Math.Min( 100.0f, x ) );
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
			if ( _eta < 1.0f ) {
				result = -0.4399f + 0.7099f / _eta - 0.3319f / (_eta * _eta) + 0.0636f / (_eta * _eta * _eta); 
			} else {
				result = -1.4399f / (_eta*_eta) + 0.7099f / _eta + 0.6681f + 0.0636f * _eta; 
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
			yr = 1.0f - FdrIntegral_Schlick( _IOR );
			yg = 1.0f - yr / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12

			float	eta = 1.0f / _IOR;	// eta is "the relative index of refraction of the medium with the reflected ray to the other medium"
										// So eta = IOR_air / IOR_other

			yb = 1.0f - FdrAnalytic( eta );
			yy = (1.0f - yb) / (eta*eta);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12

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
			yr = 1.0f - FdrIntegral_Precise( _IOR );
			yg = 1.0f - yr / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12


			float	eta = 1.0f / _IOR;	// eta is "the relative index of refraction of the medium with the reflected ray to the other medium"
										// So eta = IOR_air / IOR_other

			yb = 1.0f - FdrAnalytic( eta );
			yy = 1.0f - yb / (eta*eta);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12


//yg = (1.0f - yr) / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12

		}

		#endregion

		#region Table

		/// <summary>
		/// In the TestAreaLight project we have a Compute Shader that's able to precompute the BRDF integral for our specular model (i.e. Ward)
		/// It creates a 2D table where X represents cos(ThetaV), the view angle and Y represents the roughness from 0.01 to 1 (very rough).
		/// I simply added all the X terms to obtain a single dimension of float2 values depending on roughness.
		/// 
		/// Since the BRDF integral was computed so that F0 could be factored out, for each roughness we obtain 2 values (x,y) that can be recombined as:
		/// 
		///		TotalSpecularReflection = F0 * x + y
		///	
		/// Where F0 is the vertical specular reflection coefficient (or specular tint) directly linked to IOR
		/// We therefore assume that:
		/// 
		///		TotalDiffuseReflection  = 1 - TotalSpecularReflection
		///								= 1 - (F0 * x + y)
		/// 
		/// </summary>
		/// <param name="_IOR"></param>
		/// <param name="yr"></param>
		/// <param name="yg"></param>
		/// <param name="yb"></param>
		/// <param name="yy"></param>
		/// 
		void	BilinearSampleTable( float _roughness, out float _x, out float _y ) {
			_roughness *= 64.0f;
			int		R0 = (int) Math.Floor( _roughness );
			float	r = _roughness - R0;
			R0 = Math.Min( 63, R0 );
			int		R1 = Math.Min( 63, R0+1 );

			_x = (1.0f - r) * m_Table[R0,0] + r * m_Table[R1,0];
			_y = (1.0f - r) * m_Table[R0,1] + r * m_Table[R1,1];
		}

		protected void		Fresnel_Table( float _IOR, out float yr, out float yg, out float yb, out float yy ) {

			float	F0 = Form1.IOR_to_F0( _IOR );

			float	x, y;
			BilinearSampleTable( m_Roughness, out x, out y );


//y *= (1.0f-m_Roughness)*(1.0f-m_Roughness);


			float	TotalSpecularReflection = F0 * x + y;
			float	TotalDiffuseReflection = 1.0f - TotalSpecularReflection;
					TotalDiffuseReflection *= m_PeakFactor;

			yr = TotalDiffuseReflection;

			float	dummy;
			Fresnel_Schlick( _IOR, out yg, out dummy, out dummy, out dummy );
			Fresnel_Precise( _IOR, out yb, out dummy, out dummy, out dummy );

			float	eta = 1.0f / _IOR;	// eta is "the relative index of refraction of the medium with the reflected ray to the other medium"
										// So eta = IOR_air / IOR_other

//			yy = FdrAnalytic( eta );
//			yy = 1.0f - (1.0f - yy) / (eta*eta);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12


			// After fiiting in Mathematica, it turns out that:
			//	TotalSpecularReflection_x ~= 0.831387 + 0.000403213 x - 0.276223 x^2	<= x = roughness
			//	TotalSpecularReflection_y ~= 0.188237 - 0.218206 x + 0.0727132 x^2
// 			x = 0.831387f + 0.000403213f * m_Roughness - 0.276223f * m_Roughness*m_Roughness;
// 			y = 0.188237f - 0.218206f * m_Roughness + 0.0727132f * m_Roughness*m_Roughness;

			// And here's a closer fit with a 3rd degree polynomial:
			//	TotalSpecularReflection_x ~= 0.818887 + 0.156573 x - 0.669738 x^2 + 0.262344 x^3	<= x = roughness
			//	TotalSpecularReflection_y ~= 0.181446 - 0.133354 x - 0.141096 x^2 + 0.142539 x^3
			x = 0.818887f + 0.156573f * m_Roughness - 0.669738f * m_Roughness*m_Roughness + 0.262344f * m_Roughness*m_Roughness*m_Roughness;
			y = 0.181446f - 0.133354f * m_Roughness - 0.141096f * m_Roughness*m_Roughness + 0.142539f * m_Roughness*m_Roughness*m_Roughness;


// Use artificial weakening of base factor when roughness tends towards 0 so we avoid "white veil" due to Schlick approximation
//y *= (1.0f-m_Roughness)*(1.0f-m_Roughness);
y *= 1.0f-m_Roughness*m_Roughness;


			TotalSpecularReflection = F0 * x + y;
			TotalDiffuseReflection = 1.0f - TotalSpecularReflection;
//			TotalDiffuseReflection = (1.0f - TotalSpecularReflection) / (_IOR*_IOR);	// From "Determination of Absorption and Scattering Coefficients for Nonhomogeneous Media 2 - Experiment", Egan, Hilgeman (1973) eq. 12

			TotalDiffuseReflection *= m_PeakFactor;

			yy = TotalDiffuseReflection;
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
