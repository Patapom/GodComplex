//////////////////////////////////////////////////////////////////////////
// Fitter class for Linearly-Transformed Cosines
// From "Real-Time Polygonal-Light Shading with Linearly Transformed Cosines" (https://eheitzresearch.wordpress.com/415-2/)
// This is a C# re-implementation of the code provided by Heitz et al.
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace TestMSBRDF.LTC
{
	/// <summary>
	/// Downhill simplex solver:
	/// http://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method#One_possible_variation_of_the_NM_algorithm
	/// Using the termination criterion from Numerical Recipes in C++ (3rd Ed.)
	/// </summary>
	public class NelderMead {

		// standard coefficients from Nelder-Mead
		const float reflect  = 1.0f;
		const float expand   = 2.0f;
		const float contract = 0.5f;
		const float shrink   = 0.5f;

		int			DIM;
		int			NB_POINTS;
		float[][]	s;
		float[]		f;

		public delegate float	ObjectiveFunctionDelegate( float[] _parameters );

		public NelderMead( int _dimensions ) {
			DIM = _dimensions;
			NB_POINTS = _dimensions+1;
			s = new float[NB_POINTS][];
			for ( int i=0; i < NB_POINTS; i++  )
				s[i] = new float[_dimensions];
			f = new float[NB_POINTS];
		}

		public float	FindFit( float[] _pmin, float[] _start, float _delta, float _tolerance, int _maxIterations, ObjectiveFunctionDelegate _objectiveFn ) {

			// initialise simplex
			Mov( s[0], _start );
			for (int i = 1; i < NB_POINTS; i++) {
				Mov( s[i], _start );
				s[i][i - 1] += _delta;
			}

			// evaluate function at each point on simplex
			for ( int i = 0; i < NB_POINTS; i++ )
				f[i] = _objectiveFn( s[i] );

			float[]	o = new float[DIM];	// Centroid
			float[]	r = new float[DIM];	// Reflection
			float[]	c = new float[DIM];	// Contraction
			float[]	e = new float[DIM];	// Expansion

			int lo = 0, hi, nh;
			for ( int j = 0; j < _maxIterations; j++ ) {
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
				}

				// stop if we've reached the required tolerance level
				float a = Mathf.Abs(f[lo]);
				float b = Mathf.Abs(f[hi]);
				if (2.0f*Mathf.Abs(a - b) < (a + b)*_tolerance)
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

				// reflection
				for (int i = 0; i < DIM; i++)
					r[i] = o[i] + reflect*(o[i] - s[hi][i]);

				float fr = _objectiveFn(r);
				if (fr < f[nh]) {
					if (fr < f[lo]) {
						// expansion
						for (int i = 0; i < DIM; i++)
							e[i] = o[i] + expand*(o[i] - s[hi][i]);

						float fe = _objectiveFn(e);
						if (fe < fr) {
							Mov( s[hi], e );
							f[hi] = fe;
							continue;
						}
					}

					Mov( s[hi], r );
					f[hi] = fr;
					continue;
				}

				// contraction
				for (int i = 0; i < DIM; i++)
					c[i] = o[i] - contract*(o[i] - s[hi][i]);

				float fc = _objectiveFn(c);
				if (fc < f[hi]) {
					Mov( s[hi], c );
					f[hi] = fc;
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
			}

			// return best point and its value
			Mov( _pmin, s[lo] );
			return f[lo];
		}

		void Mov( float[] r, float[] v ) {//, int dim ) {
			for (int i = 0; i < DIM; ++i)
				r[i] = v[i];
		}

		void Set( float[] r, float v ) {
			for (int i = 0; i < DIM; ++i)
				r[i] = v;
		}

		void Add( float[] r, float[] v ) {
			for (int i = 0; i < DIM; ++i)
				r[i] += v[i];
		}

	}
}
