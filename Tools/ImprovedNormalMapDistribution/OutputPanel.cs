using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImprovedNormalMapDistribution
{
	public partial class OutputPanel : Panel
	{
		private Bitmap	m_Bitmap = null;

		private double	m_NormalX = 0;
		private double	m_NormalY = 0;
		private double	m_NormalZ = 1;

		private int		m_IntersectionsCount = 0;
		private int		m_SumIterationsCount = 0;

		public OutputPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			UpdateBitmap();
		}

		public unsafe void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				G.FillRectangle( Brushes.White, 0, 0, W, H );

				// Draw grid
				const int	SUBDIVS = 50;

				for ( int Y=0; Y <= SUBDIVS; Y++ )
				{
					float	y = -1.0f + 2.0f * Y / SUBDIVS;
					for ( int X=0; X < SUBDIVS; X++ )
					{
						float	x0 = -1.0f + 2.0f * X / SUBDIVS;
						float	x1 = -1.0f + 2.0f * (X+1) / SUBDIVS;

						PointF	P0, P1;
						Transform( x0, y, out P0 );
						Transform( x1, y, out P1 );
						G.DrawLine( Pens.Black, P0, P1 );
					}
				}

				for ( int X=0; X <= SUBDIVS; X++ )
				{
					float	x = -1.0f + 2.0f * X / SUBDIVS;
					for ( int Y=0; Y < SUBDIVS; Y++ )
					{
						float	y0 = -1.0f + 2.0f * Y / SUBDIVS;
						float	y1 = -1.0f + 2.0f * (Y+1) / SUBDIVS;

						PointF	P0, P1;
						Transform( x, y0, out P0 );
						Transform( x, y1, out P1 );
						G.DrawLine( Pens.Black, P0, P1 );
					}
				}

				// Compute normal intersection
				float	Nx, Ny;
//				ComputeIntersectionNewton( out Nx, out Ny );
				ComputeIntersectionExact( out Nx, out Ny );
				PointF	N;
				Transform( Nx, Ny, out N );
				G.FillEllipse( Brushes.Red, N.X - 4, N.Y - 4, 8, 8 );

				G.DrawString( "Average Iterations = " + ((float) m_SumIterationsCount / m_IntersectionsCount).ToString( "G3"), Font, Brushes.Black, 10, 20 );
			}

			Invalidate();
		}

		private void	Transform( float x, float y, out PointF _P )
		{
			float	h = (1.0f - x*x) * (1.0f - y*y);

			x = 0.5f * (1.0f + x);
			y = 0.5f * (1.0f + y);

			float	CornerMinX = 0.1f * Width;
			float	CornerMaxX = 0.9f * Width;
			float	CornerMinY = 0.7f * Height;
			float	W = CornerMaxX - CornerMinX;
			float	H = 0.2f * Height;

			float	Ratio = 0.4f;

			float	X = CornerMinX + (y * Ratio + x * (1.0f - Ratio)) * W;
			float	Y = CornerMinY + (y - x) * H;

			Y -= 0.3f * Height * h;

			_P = new PointF( X, Y );
		}
		
		/// <summary>
		/// Computes the intersection of the normal vector with the curve Z=(1-x²)(1-y²) using Newton-Raphson
		/// 
		/// We can parametrize the Normal path as:
		///		x = Nx.t
		///		y = Ny.t
		///		z = Nz.t
		///		
		/// Injecting this in f(x,y) = (1-x²)(1-y²) we get:
		/// 
		///		f(t) = (1-Nx².t²)(1-Ny².t²)
		///		
		/// To find the intersection with the normal we subtract Nz.t:
		/// 
		///		f(t) = (1-Nx².t²)(1-Ny².t²) - Nz.t
		///		
		/// Expanding into:
		///		
		///		f(t) = Nx².Ny².t^4 - (Nx²+Ny²).t² - Nz.t + 1
		/// 
		/// And try to find the root f(t) = 0, we need the derivative of f(t):
		///		
		///		f'(t) = 4.Nx².Ny².t^3 - 2.(Nx²+Ny²).t - Nz.t
		///		
		///	We perform several iterations of the Newton-Raphson algorithm (http://en.wikipedia.org/wiki/Newton's_method):
		///		
		///	Starting from t0 = 0;
		///	
		///		t1 = t0 - f(t0) / f'(t0)
		///		t2 = t1 - f(t1) / f'(t1)
		///		etc.
		///		
		///	Until f(t) < epsilon ...
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		void	ComputeIntersectionNewton( out float x, out float y )
		{
			double	a = m_NormalX*m_NormalX * m_NormalY*m_NormalY;
			double	b = 0.0;
			double	c = -m_NormalX*m_NormalX - m_NormalY*m_NormalY;
			double	d = -m_NormalZ;
			double	e = 1.0;

			const double	epsilon = 1e-6;
			const int		MAX_ITERATIONS = 10;

			double	t = 0.0;
			int		Iteration = 0;
			while ( Iteration++ < MAX_ITERATIONS )
			{
				double	f = e + t*(d + t*(c + t*(b + t*a)));
				if ( Math.Abs( f ) < epsilon )
					break;

				double	f_prime = d + t*(2*c + t*(3*b + t*4*a));

				double	t1 = t - f / Math.Sign( f_prime ) * Math.Max( 1e-6, Math.Abs( f_prime ) );
				t = t1;
			}

			x = (float) (t * m_NormalX);
			y = (float) (t * m_NormalY);

			m_IntersectionsCount++;
			m_SumIterationsCount += Iteration;
		}

		void	ComputeIntersectionExact( out float x, out float y )
		{
			double	t = 0.0;
//			if ( m_NormalZ < 1.0 )
			{
				double	e = m_NormalX*m_NormalX * m_NormalY*m_NormalY;
				double	d = 0.0;
				double	c = -m_NormalX*m_NormalX - m_NormalY*m_NormalY;
				double	b = -m_NormalZ;
				double	a = 1.0;

				double[]	roots = solvePolynomial( a, b, c, d, e );

				t = Math.Sqrt( 2.0 );
				for ( int i=0; i < roots.Length; i++ )
					if ( !double.IsNaN( roots[i] ) && roots[i] >= 0.0 )
						t = Math.Min( t, roots[i] );
			}

			x = (float) (t * m_NormalX);
			y = (float) (t * m_NormalY);

			m_IntersectionsCount = 1;
		}

		// Returns the array of roots for any polynomial of degree 0 to 4
		//
		double[]	solvePolynomial( double a, double b, double c, double d, double e )
		{
			const double eps = 1e-12;
			if ( Math.Abs( e ) > eps )
				return solveQuartic( a, b, c, d, e );
			else if ( Math.Abs( d ) > eps )
				return solveCubic( a, b, c, d );
			else if ( Math.Abs( c ) > eps )
				return solveQuadratic( a, b, c );

			return solveLinear( a, b );
		}

		// Returns the array of 1 real root of a linear polynomial  a + b x = 0
		//
		double[]	solveLinear( double a, double b )
		{
			return new double[] { -a / b };
		}

		// Returns the array of 2 real roots of a quadratic polynomial  a + b x + c x^2 = 0
		// NOTE: If roots are imaginary, the returned value in the array will be "undefined"
		//
		double[]	solveQuadratic( double a, double b, double c )
		{
			var	Delta = b * b - 4 * a * c;
			if ( Delta < 0.0 )
				return	new double[] { 0, 0 };

			Delta = Math.Sqrt( Delta );
			var	OneOver2a = 0.5 / a;

			return	new double[] { OneOver2a * (-b - Delta), OneOver2a * (-b + Delta) };
		}

		// Returns the array of 3 real roots of a cubic polynomial  a + b x + c x^2 + d x^3 = 0
		// NOTE: If roots are imaginary, the returned value in the array will be "undefined"
		// Code from http://www.codeguru.com/forum/archive/index.php/t-265551.html (pretty much the same as http://mathworld.wolfram.com/CubicFormula.html)
		//
		double[]	solveCubic( double a, double b, double c, double d )
		{
			// Adjust coefficients
			var a1 = c / d;
			var a2 = b / d;
			var a3 = a / d;

			var Q = (a1 * a1 - 3 * a2) / 9;
			var R = (2 * a1 * a1 * a1 - 9 * a1 * a2 + 27 * a3) / 54;
			var Qcubed = Q * Q * Q;
				d = Qcubed - R * R;

			var	Result = new double[3];
			if ( d >= 0 )
			{	// Three real roots
				if ( Q < 0.0 )
					return new double[] { 0, 0, 0 };

				var theta = Math.Acos( R / Math.Sqrt(Qcubed) );
				var sqrtQ = Math.Sqrt( Q );

				Result[0] = -2 * sqrtQ * Math.Cos( theta / 3) - a1 / 3;
				Result[1] = -2 * sqrtQ * Math.Cos( (theta + 2 * Math.PI) / 3 ) - a1 / 3;
				Result[2] = -2 * sqrtQ * Math.Cos( (theta + 4 * Math.PI) / 3 ) - a1 / 3;
			}
			else
			{	// One real root
				var e = Math.Pow( Math.Sqrt( -d ) + Math.Abs( R ), 1.0 / 3.0 );
				if ( R > 0 )
					e = -e;

				Result[0] = Result[1] = Result[2] = (e + Q / e) - a1 / 3.0;
			}

			return	Result;
		}

		// Returns the array of 4 real roots of a quartic polynomial  a + b x + c x^2 + d x^3 + e x^4 = 0
		// NOTE: If roots are imaginary, the returned value in the array will be "undefined"
		// Code from http://mathworld.wolfram.com/QuarticEquation.html
		//
		double[]	solveQuartic( double a, double b, double c, double d, double e )
		{
			// Adjust coefficients
			var a0 = a / e;
			var a1 = b / e;
			var a2 = c / e;
			var a3 = d / e;

			// Find a root for the following cubic equation : y^3 - a2 y^2 + (a1 a3 - 4 a0) y + (4 a2 a0 - a1 ^2 - a3^2 a0) = 0
			var	b0 = 4 * a2 * a0 - a1 * a1 - a3 * a3 * a0;
			var	b1 = a1 * a3 - 4 * a0;
			var	b2 = -a2;
			var	Roots = solveCubic( b0, b1, b2, 1 );
			var	y = Math.Max( Roots[0], Math.Max( Roots[1], Roots[2] ) );

			// Compute R, D & E
			var	R = 0.25 * a3 * a3 - a2 + y;
			if ( R < 0.0 )
				return new double[] { 0, 0, 0, 0 };
			R = Math.Sqrt( R );

			double	D, E;
			if ( R == 0.0 )
			{
// 				D = Math.Sqrt( 0.75 * a3 * a3 - 2 * a2 + 2 * Math.Sqrt( y * y - 4 * a0 ) );
// 				E = Math.Sqrt( 0.75 * a3 * a3 - 2 * a2 - 2 * Math.Sqrt( y * y - 4 * a0 ) );
				D = Math.Sqrt( Math.Max( 0.0, 0.75 * a3 * a3 - 2 * a2 + 2 * Math.Sqrt( Math.Max( 0.0, y * y - 4 * a0 ) ) ) );
				E = Math.Sqrt( Math.Max( 0.0, 0.75 * a3 * a3 - 2 * a2 - 2 * Math.Sqrt( Math.Max( 0.0, y * y - 4 * a0 ) ) ) );
			}
			else
			{
				var	Rsquare = R * R;
				var	Rrec = 1.0 / R;
// 				D = Math.Sqrt( 0.75 * a3 * a3 - Rsquare - 2 * a2 + 0.25 * Rrec * (4 * a3 * a2 - 8 * a1 - a3 * a3 * a3) );
// 				E = Math.Sqrt( 0.75 * a3 * a3 - Rsquare - 2 * a2 - 0.25 * Rrec * (4 * a3 * a2 - 8 * a1 - a3 * a3 * a3) );
				D = Math.Sqrt( Math.Max( 0.0, 0.75 * a3 * a3 - Rsquare - 2 * a2 + 0.25 * Rrec * (4 * a3 * a2 - 8 * a1 - a3 * a3 * a3) ) );
				E = Math.Sqrt( Math.Max( 0.0, 0.75 * a3 * a3 - Rsquare - 2 * a2 - 0.25 * Rrec * (4 * a3 * a2 - 8 * a1 - a3 * a3 * a3) ) );
			}

			// Compute the 4 roots
			var	Result = new double[] {
				-0.25 * a3 + 0.5 * R + 0.5 * D,
				-0.25 * a3 + 0.5 * R - 0.5 * D,
				-0.25 * a3 - 0.5 * R + 0.5 * E,
				-0.25 * a3 - 0.5 * R - 0.5 * E
			};

			return	Result;
		}


		public void		SetNormal( double x, double y, double z )
		{
			m_NormalX = x;
			m_NormalY = y;
			m_NormalZ = z;
			UpdateBitmap();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			UpdateBitmap();
			Invalidate();

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
//				e.Graphics.DrawImage( m_Bitmap, new Rectangle( 0, 0, Width, Height ), 0, 0, m_Bitmap.Width, m_Bitmap.Height, GraphicsUnit.Pixel );
		}
	}
}
