using System;
using System.Collections.Generic;

namespace ImprovedNormalMapDistribution
{
	static class Polynomial
	{
		// Returns the array of roots for any polynomial of degree 0 to 4
		//
		public static double[]	solvePolynomial( double a, double b, double c, double d, double e )
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
		public static double[]	solveLinear( double a, double b )
		{
			return new double[] { -a / b };
		}

		// Returns the array of 2 real roots of a quadratic polynomial  a + b x + c x^2 = 0
		// NOTE: If roots are imaginary, the returned value in the array will be "undefined"
		//
		public static double[]	solveQuadratic( double a, double b, double c )
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
		public static double[]	solveCubic( double a, double b, double c, double d )
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
		public static double[]	solveQuartic( double a, double b, double c, double d, double e )
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
	}
}
