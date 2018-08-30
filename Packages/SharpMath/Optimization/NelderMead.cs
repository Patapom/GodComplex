using System;
using System.Collections.Generic;

namespace SharpMath
{
	/// <summary>
	/// Downhill simplex solver:
	/// http://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method#One_possible_variation_of_the_NM_algorithm
	/// Using the termination criterion from Numerical Recipes in C++ (3rd Ed.)
	/// </summary>
	public class NelderMead {

		// standard coefficients from Nelder-Mead
		const double reflect  = 1.0f;
		const double expand   = 2.0f;
		const double contract = 0.5f;
		const double shrink   = 0.5f;

		int			DIM;
		int			NB_POINTS;
		double[][]	s;
		double[]	f;

		public int	m_lastIterationsCount;

		public delegate double	ObjectiveFunctionDelegate( double[] _parameters );

		public NelderMead( int _dimensions ) {
			DIM = _dimensions;
			NB_POINTS = _dimensions+1;
			s = new double[NB_POINTS][];
			for ( int i=0; i < NB_POINTS; i++  )
				s[i] = new double[_dimensions];
			f = new double[NB_POINTS];

		}


//public System.IO.StreamWriter	log = new System.IO.FileInfo( "log.txt" ).CreateText();


		public double	FindFit( double[] _pmin, double[] _start, double _delta, double _tolerance, int _maxIterations, ObjectiveFunctionDelegate _objectiveFn ) {

			// initialise simplex
			Mov( s[0], _start );
			for (int i = 1; i < NB_POINTS; i++) {
				Mov( s[i], _start );
				s[i][i - 1] += _delta;
			}

			// evaluate function at each point on simplex
//log.WriteLine( "Init" );
			for ( int i = 0; i < NB_POINTS; i++ ) {
				f[i] = _objectiveFn( s[i] );
//log.WriteLine( "f[{0}] = {1}", i, f[i] );
			}
//			if ( f[0] < _tolerance ) {
//				// Already at a minimum!
//				Mov( _pmin, _start );
//				return f[0];
//			}

			double[]	o = new double[DIM];	// Centroid
			double[]	r = new double[DIM];	// Reflection
			double[]	c = new double[DIM];	// Contraction
			double[]	e = new double[DIM];	// Expansion

			int lo = 0, hi, nh;
			for ( m_lastIterationsCount = 0; m_lastIterationsCount < _maxIterations; m_lastIterationsCount++ ) {
//log.WriteLine();
//log.WriteLine( "===================================" );
//log.WriteLine(  "Iteration #" + m_lastIterationsCount );

				// find lowest, highest and next highest
				lo = hi = nh = 0;
				for ( int i = 1; i < NB_POINTS; i++ ) {
					if (f[i] < f[lo])
						lo = i;
					if (f[i] > f[hi]) {
						nh = hi;
						hi = i;
					} else if (f[i] > f[nh])
						nh = i;

//log.WriteLine( "f[{0}] = {1}", i, f[i] );
				}

				// stop if we've reached the required tolerance level
				double a = Mathf.Abs(f[lo]);
				double b = Mathf.Abs(f[hi]);
				if ( 2.0*Mathf.Abs(a - b) < (a + b)*_tolerance )// || a + b == 0.0 )
					break;

				// compute centroid (excluding the worst point)
				Set( o, 0.0f );
				for ( int i = 0; i < NB_POINTS; i++) {
					if ( i == hi )
						continue;
					Add( o, s[i] );
				}

				for (int i = 0; i < DIM; i++)
					o[i] /= DIM;

//log.WriteLine( "centroid = {{ {0}, {1}, {2} }}", o[0], o[1], o[2] );

				// reflection
				for (int i = 0; i < DIM; i++)
					r[i] = o[i] + reflect*(o[i] - s[hi][i]);

				double fr = _objectiveFn(r);

//log.WriteLine( "reflection = {{ {0}, {1}, {2} }}", r[0], r[1], r[2] );
//log.WriteLine( "reflection error = " + fr );

				if (fr < f[nh]) {
					if (fr < f[lo]) {
						// expansion
						for (int i = 0; i < DIM; i++)
							e[i] = o[i] + expand*(o[i] - s[hi][i]);

						double fe = _objectiveFn(e);

//log.WriteLine( "expansion = {{ {0}, {1}, {2} }}", e[0], e[1], e[2] );
//log.WriteLine( "expansion error = " + fe );

						if (fe < fr) {
							Mov( s[hi], e );
							f[hi] = fe;
//log.WriteLine( "CHOSE EXPANSION" );
							continue;
						}
					}

					Mov( s[hi], r );
					f[hi] = fr;
//log.WriteLine( "CHOSE REFLECTION" );
					continue;
				}

				// contraction
				for (int i = 0; i < DIM; i++)
					c[i] = o[i] - contract*(o[i] - s[hi][i]);

				double fc = _objectiveFn(c);

//log.WriteLine( "contraction = {{ {0}, {1}, {2} }}", c[0], c[1], c[2] );
//log.WriteLine( "contraction error = " + fc );

				if (fc < f[hi]) {
					Mov( s[hi], c );
					f[hi] = fc;
//log.WriteLine( "CHOSE CONTRACTION" );
					continue;
				}

				// reduction
				for (int k = 0; k < NB_POINTS; k++) {
					if (k == lo)
						continue;
					for (int i = 0; i < DIM; i++)
						s[k][i] = s[lo][i] + shrink*(s[k][i] - s[lo][i]);
					f[k] = _objectiveFn(s[k]);
				}
//log.WriteLine( "CHOSE REDUCTION" );
			}

			// return best point and its value
			Mov( _pmin, s[lo] );

// log.WriteLine();
// log.WriteLine();
// log.WriteLine( "===================================" );
// log.WriteLine( "Exiting after " + m_lastIterationsCount + " iterations" );
// log.WriteLine( "Result = {{ {0}, {1}, {2} }}", _pmin[0], _pmin[1], _pmin[2] );// log.WriteLine( "Error = " + f[lo] );

			return f[lo];
		}

		void Mov( double[] r, double[] v ) {
			for (int i = 0; i < DIM; ++i)
				r[i] = v[i];
		}

		void Set( double[] r, double v ) {
			for (int i = 0; i < DIM; ++i)
				r[i] = v;
		}

		void Add( double[] r, double[] v ) {
			for (int i = 0; i < DIM; ++i)
				r[i] += v[i];
		}
	}
}
