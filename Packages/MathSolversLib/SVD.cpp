#include "stdafx.h"
#include "SVD.h"

using namespace MathSolversLib;

void	SVD::Init( U32 _rows, U32 _columns ) {
	A.Init( _rows, _columns );
	U.Init( _rows, _columns );
	w.Init( _columns );
	V.Init( _columns, _columns );
}
void	SVD::Init( const MatrixF& _A ) {
	_A.CopyTo( A );
	U.Init( _A.rows, _A.columns );
	w.Init( _A.columns );
	V.Init( _A.columns, _A.columns );
}

//////////////////////////////////////////////////////////////////////////
// We know that A was decomposed into U.w.V^T where U and V are orthonormal matrices and w a diagonal matrix with (theoretically) non null components
//	so A^-1 = V.1/w.U^T
// Since A.x = b it ensues that x = A^-1.b
//
void	SVD::Solve( const VectorF& b, VectorF& x ) {
	if ( b.length != A.rows )
		throw "Vector b length and matrix A's rows count mismatch!";

	U32	unknownsCount = A.columns;
	U32	equationsCount = A.rows;

	VectorF	tempX( x.length );

	// 1) Perform 1/w * U^T * b
	for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) {
		float	r = 0.0f;
		for ( U32 i=0; i < equationsCount; i++ ) {
			const float	Uterm = U[i][rowIndex];
			const float	bterm = b[i];
			r += Uterm * bterm;
		}

		// Multiply by 1/w
		const float	wterm = w[rowIndex];
		float		recW = fabs( wterm ) > 1e-6f ? 1.0f / wterm : 0.0f;	// We shouldn't ever have 0 values because of the overdetermined system of equations but let's be careful anyway!
		tempX[rowIndex] = r * recW;
	}

	// 2) Perform V * (1/w * U^T * b)
	for ( U32 rowIndex=0; rowIndex < unknownsCount; rowIndex++ ) {
		float	r = 0.0f;
		for ( U32 i=0; i < unknownsCount; i++ ) {
			const float	Vterm = V[rowIndex][i];
			const float	tempXterm = tempX[i];
			r += Vterm * tempXterm;
		}
		x[rowIndex] = r;	// This is our final results!
	}
}

//////////////////////////////////////////////////////////////////////////
// Performs the actual Singular Value Decomposition

// Computes (a2 + b2)^1/2 without destructive underflow or overflow.
static float pythag(float a, float b) {
	float	absa = fabs(a);
	float	absb = fabs(b);
	if ( absa > absb )
		return absa*sqrt( 1.0f + SQR(absb/absa) );
	else
		return absb == 0.0f ? 0.0f : absb*sqrt( 1.0f + SQR(absa/absb) );
}

template<class T>
inline T SIGN(const T &a, const T &b) { return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a); }

static float maxarg1,maxarg2;
#define FMAX(a,b) (maxarg1=(a),maxarg2=(b),(maxarg1) > (maxarg2) ?\
        (maxarg1) : (maxarg2))

static int iminarg1,iminarg2;
#define IMIN(a,b) (iminarg1=(a),iminarg2=(b),(iminarg1) < (iminarg2) ?\
        (iminarg1) : (iminarg2))

void	SVD::Decompose() {
	// Copy A to U as SVD works in place
	A.CopyTo( U );

	int		m = U.rows;
	int		n = U.columns;

	VectorF	residualValues( n );
	residualValues.Clear();

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
				scale += fabs( U[k][i] );

			if ( scale != 0.0f ) {
				for ( int k=i; k < m; k++ ) {
					U[k][i] /= scale;
					s += U[k][i] * U[k][i];
				}
				f = U[i][i];
				g = -SIGN( sqrtf(s), f );
				h = f*g - s;
				U[i][i] = f-g;
				for ( int j=l; j < n; j++ ) {
					s = 0.0f;
					for ( int k = i; k < m; k++)
						s += U[k][i] * U[k][j];

					f = s/h;
					for ( int k=i; k < m; k++ )
						U[k][j] += f*U[k][i];
				}
				for ( int k=i; k < m; k++ )
					U[k][i] *= scale;
			}
		}
		w[i] = scale * g;

		g = 0.0f;
		s = 0.0f;
		scale = 0.0f;
		if ( i < m && l < n ) {
			for ( int k=l; k < n; k++ )
				scale += fabs( U[i][k] );

			if ( scale != 0.0f ) {
				for ( int k=l; k < n; k++ ) {
					U[i][k] /= scale;
					s += U[i][k] * U[i][k];
				}
				f = U[i][l];
				g = -SIGN( sqrtf(s), f );
				h = f * g - s;
				U[i][l] = f - g;
				for ( int k=l; k < n; k++ )
					residualValues[k] = U[i][k] / h;

				for ( int j=l; j < m; j++ ) {
					s = 0.0f;
					for ( int k=l; k < n; k++ )
						s += U[j][k] * U[i][k];

					for ( int k=l; k < n; k++ )
						U[j][k] += s * residualValues[k];
				}
				for ( int k=l; k < n; k++ )
					U[i][k] *= scale;
			}
		}
		anorm = FMAX( anorm, (fabs(w[i])+fabs(residualValues[i])) );
	}

	// Accumulation of right-hand transformations
	V[n-1][n-1] = 1.0f;
	g = residualValues[n-1];

	for ( int i=n-2; i >= 0; i-- ) {
		int	l = i+1;
		if ( g != 0.0f ) {
			for ( int j=l; j < n; j++ )
				V[j][i] = (U[i][j] / U[i][l]) / g;	// Double division to avoid possible underflow

			for ( int j=l; j < n; j++) {
				float	s = 0.0f;
				for ( int k=l; k < n; k++ )
					s += U[i][k] * V[k][j];
				for ( int k=l; k < n; k++ )
					V[k][j] += s * V[k][i];
			}
		}
		for ( int j=l; j < n; j++ )
			V[i][j] = V[j][i] = 0.0f;

		V[i][i] = 1.0f;
		g = residualValues[i];
	}

	// Accumulation of left-hand transformations
	for ( int i=IMIN(m,n)-1; i >= 0; i-- ) {
		int	l = i+1;
		g = w[i];
		for ( int j=l; j < n; j++ )
			U[i][j] = 0.0f;
		if ( g != 0.0f ) {
			g = 1.0f / g;
			for ( int j=l; j < n; j++ ) {
				float	s = 0.0f;
				for ( int k=l; k < m; k++ )
					s += U[k][i] * U[k][j];

				f = (s / U[i][i]) * g;
				for ( int k=i; k < m; k++ )
					U[k][j] += f * U[k][i];
			}
		}
		for ( int j=i; j < m; j++ )
			U[j][i] *= g;

		++U[i][i];
	}

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
						float	y = U[j][nm];
						float	z = U[j][i];
						U[j][nm] = y*c + z*s;
						U[j][i]  = z*c - y*s;
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
						V[j][k] = -V[j][k];
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
					x = V[jj][j];
					z = V[jj][i];
					V[jj][j] = x*c + z*s;
					V[jj][i] = z*c - x*s;
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
					y = U[jj][j];
					z = U[jj][i];
					U[jj][j] = y*c + z*s;
					U[jj][i] = z*c - y*s;
				}

// sprintf( debugString, "j = %d -> z = %f - f = %f - x = %f\r\n", j, z, f, x );
// OutputDebugStringA( debugString );
			}
			residualValues[l] = 0.0f;
			residualValues[k] = f;
			w[k] = x;
		}	// for ( ; iterationsCount <= 30; iterationsCount++ ) 
	}	// for ( int k=n-1; k >= 0; k-- ) 
}

