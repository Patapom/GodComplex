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
	public partial class OutputPanel2 : Panel
	{
		private Bitmap	m_Bitmap = null;

		private double	m_Theta = 0;
		public double	Theta
		{
			get { return m_Theta; }
			set {
				m_Theta = value;
				UpdateBitmap();
			}
		}

		public OutputPanel2( IContainer container )
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

				// Draw function
				float	Px = 10.0f;
				float	Py = 10.0f;
				for ( int X=1; X < Width-20; X++ )
				{
					float	x = (float) X / (Width-20);
					float	y = 1.0f - x*x;

					float	Cx = 10.0f + X;
					float	Cy = 10.0f + (Height-20.0f) * (1.0f - y);

					G.DrawLine( Pens.Black, Px, Py, Cx, Cy );

					Px = Cx;
					Py = Cy;
				}

				// Draw normal vector
				G.DrawLine( Pens.Black, 10, Height-10, 10 + (Width-20) * (float) Math.Sin( m_Theta ), Height-10 - (Height-20) * (float) Math.Cos( m_Theta ) );

				// Draw intersection
				// We try and find the intersection of the 2nd order polynomial:
				//	y = 1-x²
				//
				// with the normal:
				//	x = sin(Theta).t
				//	y = cos(Theta).t
				//
				// So we're looking for:
				//
				//	cos(Theta).t = 1 - sin²(Theta).t²
				//
				// Or:
				//
				//	1 - cos(Theta).t - sin²(Theta).t² = 0
				//
				double[]	Roots = solvePolynomial( 1.0, -Math.Cos( m_Theta ), -Math.Sin( m_Theta )*Math.Sin( m_Theta ), 0, 0 );

				double	t = Roots[0];
				float	Ix = (float) (Math.Sin(Theta) * t);
				float	Iy = (float) (Math.Cos(Theta) * t);

				PointF	I = new PointF( 10 + (Width-20) * Ix, Height-10 - (Height-20) * Iy );
 				G.FillEllipse( Brushes.Red, I.X - 4, I.Y - 4, 8, 8 );

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
			var	OneOver2c = 0.5 / c;

			return	new double[] { OneOver2c * (-b - Delta), OneOver2c * (-b + Delta) };
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
