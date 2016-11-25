#include "stdafx.h"
#include "SVD.h"

using namespace MathSolvers;

float pythag(float a, float b);
template<class T>
inline T SIGN(const T &a, const T &b) { return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a); }

static float maxarg1,maxarg2;
#define FMAX(a,b) (maxarg1=(a),maxarg2=(b),(maxarg1) > (maxarg2) ?\
        (maxarg1) : (maxarg2))

static int iminarg1,iminarg2;
#define IMIN(a,b) (iminarg1=(a),iminarg2=(b),(iminarg1) < (iminarg2) ?\
        (iminarg1) : (iminarg2))

//char	debugString[4096];

// Given a matrix a[1..m][1..n], this routine computes its singular value decomposition, A = U · W · V^T
//	The matrix U replaces a on output.
//	The diagonal matrix of singular values W is output as a vector w[1..n].
//	The matrix V (not the transpose V^T) is output as v[1..n][1..n].
//
void svdcmp( int m, int n, float** a, float w[], float** v ) {
	float*	residualValues = new float[n];
	memset( residualValues, 0, n*sizeof(float) );

	float	f = 0.0f, g = 0.0f, h = 0.0f;
 	float	scale = 0.0f;
 	float	anorm = 0.0f;

	// Householder reduction to bidiagonal form
	for ( int i=0; i < n; i++ ) {
		int	l = i+1;
		residualValues[i] = scale * g;

		g = 0.0f;
		float	s = 0.0f;
		scale = 0.0f;
		if ( i < m ) {
			for ( int k=i; k < m; k++ )
				scale += fabs( a[k][i] );

			if ( scale != 0.0f ) {
				for ( int k=i; k < m; k++ ) {
					a[k][i] /= scale;
					s += a[k][i] * a[k][i];
				}
				f = a[i][i];
				g = -SIGN( sqrt(s), f );
				h = f*g - s;
				a[i][i] = f-g;
				for ( int j=l; j < n; j++ ) {
					s = 0.0f;
					for ( int k = i; k < m; k++)
						s += a[k][i] * a[k][j];

					f = s/h;
					for ( int k=i; k < m; k++ )
						a[k][j] += f*a[k][i];
				}
				for ( int k=i; k < m; k++ )
					a[k][i] *= scale;
			}
		}
		w[i] = scale * g;

		g = 0.0f;
		s = 0.0f;
		scale = 0.0f;
		if ( i < m && l < n ) {
			for ( int k=l; k < n; k++ )
				scale += fabs( a[i][k] );

			if ( scale != 0.0f ) {
				for ( int k=l; k < n; k++ ) {
					a[i][k] /= scale;
					s += a[i][k] * a[i][k];
				}
				f = a[i][l];
				g = -SIGN( sqrt(s), f );
				h = f * g - s;
				a[i][l] = f - g;
				for ( int k=l; k < n; k++ )
					residualValues[k] = a[i][k] / h;

				for ( int j=l; j < m; j++ ) {
					s = 0.0f;
					for ( int k=l; k < n; k++ )
						s += a[j][k] * a[i][k];

					for ( int k=l; k < n; k++ )
						a[j][k] += s * residualValues[k];
				}
				for ( int k=l; k < n; k++ )
					a[i][k] *= scale;
			}
		}
		anorm = FMAX( anorm, (fabs(w[i])+fabs(residualValues[i])) );
	}

	// Accumulation of right-hand transformations
	v[n-1][n-1] = 1.0f;
	g = residualValues[n-1];

	for ( int i=n-2; i >= 0; i-- ) {
		int	l = i+1;
		if ( g != 0.0f ) {
			for ( int j=l; j < n; j++ )
				v[j][i] = (a[i][j] / a[i][l]) / g;	// Double division to avoid possible underflow

			for ( int j=l; j < n; j++) {
				float	s = 0.0f;
				for ( int k=l; k < n; k++ )
					s += a[i][k] * v[k][j];
				for ( int k=l; k < n; k++ )
					v[k][j] += s * v[k][i];
			}
		}
		for ( int j=l; j < n; j++ )
			v[i][j] = v[j][i] = 0.0f;

		v[i][i] = 1.0f;
		g = residualValues[i];
	}

	// Accumulation of left-hand transformations
	for ( int i=IMIN(m,n)-1; i >= 0; i-- ) {
		int	l = i+1;
		g = w[i];
		for ( int j=l; j < n; j++ )
			a[i][j] = 0.0f;
		if ( g != 0.0f ) {
			g = 1.0f / g;
			for ( int j=l; j < n; j++ ) {
				float	s = 0.0f;
				for ( int k=l; k < m; k++ )
					s += a[k][i] * a[k][j];

				f = (s / a[i][i]) * g;
				for ( int k=i; k < m; k++ )
					a[k][j] += f * a[k][i];
			}
		}
		for ( int j=i; j < m; j++ )
			a[j][i] *= g;

		++a[i][i];
	}

//return;

	// Diagonalization of the bidiagonal form: Loop over singular values, and over allowed iterations
	const int	MAX_ITERATIONS = 30;
//	const int	MAX_ITERATIONS = 1000;

	for ( int k=n-1; k >= 0; k-- ) {
		int	iterationsCount = 1;
		for ( ; iterationsCount <= MAX_ITERATIONS; iterationsCount++ ) {
			int	nm;
			// Test for splitting
			bool	flag = true;
			int		l = k;
			for ( ; l >= 0; l-- ) {
				nm = l-1;
				// Note that rv1[0] is always zero so we eventually break at the very end
				if ( float( fabs(residualValues[l]) + anorm ) == anorm ) {
					flag = false;
					break;
				}
				ASSERT( nm >= 0, "nm in negative range!" );
				if ( float( fabs(w[nm]) + anorm ) == anorm )
					break;
			}
			if ( flag ) {
				// Cancellation of rv1[l], if l > 0.
				float	c = 0.0f;
				float	s = 1.0f;
				for ( int i=l; i < k; i++ ) {
					f = s * residualValues[i];
					residualValues[i] = c * residualValues[i];
					if ( float( fabs(f) + anorm ) == anorm )
						break;

					g = w[i];
					h = pythag( f, g );
					w[i] = h;
					h = 1.0f / h;
					c = g * h;
					s = -f * h;
					for ( int j=0; j < m; j++ ) {
						float	y = a[j][nm];
						float	z = a[j][i];
						a[j][nm] = y*c + z*s;
						a[j][i]  = z*c - y*s;
					}
				}
			}
			float	z = w[k];
			if ( l == k ) {
				// Convergence
				if ( z < 0.0f ) {
					// Singular value is made nonnegative
					w[k] = -z;
					for ( int j=0; j < n; j++ )
						v[j][k] = -v[j][k];
				}
				break;
			}
			if ( iterationsCount >= MAX_ITERATIONS )
				throw "No convergence in svdcmp iterations";

			nm = k-1;
			ASSERT( nm >= 0, "nm in negative range!" );

			float	x = w[l];	// Shift from bottom 2-by-2 minor
			float	y = w[nm];
			g = residualValues[nm];
			h = residualValues[k];
			f = ((y-z)*(y+z)+(g-h)*(g+h)) / (2.0f*h*y);
			g = pythag( f, 1.0f );
			f = ((x-z)*(x+z)+h*((y/(f+SIGN(g,f)))-h))/x;

// sprintf( debugString, "ITERATION %d = f = %f - g = %f - h = %f\r\n", iterationsCount, f, g, h );
// OutputDebugStringA( debugString );

			// Next QR transformation
			float	c = 1.0f;
			float	s = 1.0f;
			for ( int j=l; j <= nm; j++ ) {
				int	i = j+1;
				g = residualValues[i];
				y = w[i];
				h = s*g;
				g = c*g;
				z = pythag( f, h );
				residualValues[j] = z;
				c = f/z;
				s = h/z;
				f = x*c + g*s;	// Rotation with sine/cosine?
				g = g*c - x*s;
				h = y*s;
				y *= c;
				for ( int jj=0; jj < n; jj++ ) {
					x = v[jj][j];
					z = v[jj][i];
					v[jj][j] = x*c + z*s;
					v[jj][i] = z*c - x*s;
				}
				z = pythag( f, h );
				w[j] = z;
				if ( z != 0.0f ) {
					z = 1.0f / z;
					c = f*z;
					s = h*z;
				}
				f = c*g + s*y;
				x = c*y - s*g;
				for ( int jj=0; jj < m; jj++ ) {
					y = a[jj][j];
					z = a[jj][i];
					a[jj][j] = y*c + z*s;
					a[jj][i] = z*c - y*s;
				}

// sprintf( debugString, "j = %d -> z = %f - f = %f - x = %f\r\n", j, z, f, x );
// OutputDebugStringA( debugString );
			}
			residualValues[l] = 0.0f;
			residualValues[k] = f;
			w[k] = x;
		}	// for ( ; iterationsCount <= 30; iterationsCount++ ) 
	}	// for ( int k=n-1; k >= 0; k-- ) 

	delete[] residualValues;
}

