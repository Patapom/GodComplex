#include "stdafx.h"
#include "MinimizeBFGS.h"

using namespace MathSolvers;

BFGS::BFGS()
	: m_coefficientsCount( 0 )
	, m_model( nullptr )
	, m_maxIterations( 200 )
	, m_tolX( 1.0e-8 )
 	, m_tolGradient( 1.0e-8 )
 	, m_functionMinimum( DBL_MAX )
	, m_iterationsCount( 0 )
	, m_evalCallsCount( 0 )
	, m_evalGradientCallsCount( 0 ) {
}
BFGS::~BFGS() {
}

void	BFGS::Minimize( IModel& _model ) {
	m_model = &_model;
	m_coefficientsCount = m_model->getParameters().length;

	m_previousX.Init( m_coefficientsCount );
	m_currentX.Init( m_coefficientsCount );
	m_optimum.Init( m_coefficientsCount );

	// Start from model's initial parameters
	m_model->getParameters().CopyTo( m_previousX );

	VectorD	direction( m_coefficientsCount ); // x_k+1 = x_k + alpha_k*direction_k
	VectorD	tempDirection( m_coefficientsCount ); // Used temporarily by LinearSearch()

	VectorD	gradient( m_coefficientsCount );
	VectorD	previousGradient( m_coefficientsCount );

	MatrixD	hessian( m_coefficientsCount, m_coefficientsCount ); // inverse Hessian approximation

	VectorD	pi( m_coefficientsCount );  // p_i = x_i+1 - x_i
	VectorD	qi( m_coefficientsCount );  // q_i = Gradient_i+1 - Gradient_i
	VectorD	Dqi( m_coefficientsCount ); // Dq_i = |D_i|.q_i:

	m_evalCallsCount = m_evalGradientCallsCount = 0; // count of function and gradient evaluations

	// Perform initial evaluation
	m_functionMinimum = m_model->Eval( m_previousX );	// Initial error
	EvalGradient( m_previousX, previousGradient );		// Initial gradient
	m_evalCallsCount++;

	hessian.Clear();
	for ( int d = 0; d < m_coefficientsCount; ++d ) {
		// initialize Hessian to a unit matrix:
		hessian[d][d] = 1.0;

		// set initial direction to opposite of the starting gradient (since Hessian is just a unit matrix):
		direction[d] = -previousGradient[d];
	}
 
	// perform a max of 'maxiterations' of quasi-Newton iteration steps
	double	temp1, temp2;
		
	m_iterationsCount = 0;
	while ( ++m_iterationsCount < m_maxIterations ) {
		// do the line search in the current direction:
		double	newMinimum = LinearSearch( m_functionMinimum, previousGradient, m_previousX, direction, m_currentX, tempDirection );	// this updates _functionMinimum and x
		m_functionMinimum = newMinimum;

		// Notify of new optimal values
		m_currentX.CopyTo( m_optimum );
		m_model->setParameters( m_optimum );

		// if the current point shift (relative to current position) is below tolerance, we're done:
		double	delta = 0.0;
		for ( int d=0; d < m_coefficientsCount; ++d ) {
			pi[d] = m_currentX[d] - m_previousX[d];

			temp2 = abs( pi[d] ) / MAX( abs( m_currentX[d] ), 1.0 );
			if ( temp2 > delta )
				delta = temp2;
		}
		if ( delta < m_tolX )
			break;

		// Get the current gradient:			// TODO: use 1 extra _functionMinimum eval gradient version?
		EvalGradient( m_currentX, gradient );
   
		// if the current gradient (normalized by the current x and _functionMinimum) is below tolerance, we're done:
		delta = 0.0;
		temp1 = MAX( m_functionMinimum, 1.0 );
		for ( int d=0; d < m_coefficientsCount; ++d ) {
			temp2 = abs( gradient[d] ) * MAX( abs( m_currentX[d] ), 1.0 ) / temp1;
			if ( temp2 > delta )
				delta = temp2;
		}
		if ( delta < m_tolGradient )
			break;

		// Compute q_i = Gradient_i+1 - Gradient_i:
		for ( int d=0; d < m_coefficientsCount; ++d )
			qi[d] = gradient[d] - previousGradient[d];

		// Compute Dq_i = |D_i|.q_i:
		for ( int m=0; m < m_coefficientsCount; ++m ) {
			Dqi[m] = 0.0;
			for ( int n=0; n < m_coefficientsCount; ++n )
				Dqi[m] += hessian[m][n] * qi[n];
		}

		// compute p_i.q_i and q_i.Dq_i:
		double	piqi = 0.0;
		double	qiDqi = 0.0;
		double	pi_norm = 0.0, qi_norm = 0.0;

		for ( int d=0; d < m_coefficientsCount; ++d ) {
			temp1 = qi[d];
			temp2 = pi[d];

			piqi += temp2 * temp1;
			qiDqi += temp1 * Dqi[d];

			qi_norm += temp1 * temp1;
			pi_norm += temp2 * temp2;
		}

		// Update Hessian using BFGS formula:
		// note that we should not update Hessian when successive pi's are almost linearly dependent;
		//	this can be ensured by checking pi.qi = pi|H|pi, which ought to be positive enough if H is positive definite.
		double	ZERO_PRODUCT = 1.0e-8;
		if ( piqi > ZERO_PRODUCT * sqrt( qi_norm * pi_norm ) ) {
			// re-use qi vector to compute v in Bertsekas:
			for ( int d=0; d < m_coefficientsCount; ++d )
				qi[d] = pi[d] / piqi - Dqi[d] / qiDqi;

			for ( int m=0; m < m_coefficientsCount; ++m ) {
				for ( int n=m; n < m_coefficientsCount; ++n ) {
					hessian[m][n] += pi[m] * pi[n] / piqi - Dqi[m] * Dqi[n] / qiDqi + qiDqi * qi[m] * qi[n];
					hessian[n][m] = hessian[m][n];
				}
			}
		}

		// set current direction for the next iteration as -|Hessian|.Gradient 
		for ( int m=0; m < m_coefficientsCount; ++m ) {
			direction[m] = 0.0;
			for ( int n = 0; n < m_coefficientsCount; ++n )
				direction[m] -= hessian[m][n] * gradient[n];
		}

		// update current point and current gradient for the next iteration:
		if ( m_iterationsCount < m_maxIterations-1 ) {	// keep the 'x contains the latest point' invariant for the post-loop copy below
			gradient.Swap( previousGradient );
			m_currentX.Swap( m_previousX );
		}
	}

	// Copy final parameters
	m_currentX.CopyTo( m_optimum );
	m_model->setParameters( m_optimum );

// 	DeleteVector( Dqi );
// 	DeleteVector( qi );
// 	DeleteVector( pi );
// 	DeleteMatrix( hessian );
// 	DeleteVector( previousGradient );
// 	DeleteVector( gradient );
// 	DeleteVector( tempDirection );
// 	DeleteVector( direction );
// 	DeleteVector( m_optimum );
// 	DeleteVector( m_currentX );
// 	DeleteVector( m_previousX );
}

// ===========================================
// Compute the gradient using finite differences
void	BFGS::EvalGradient( VectorD& _params, VectorD& _gradient ) {
	double	EPS = 1e-6;
	for ( int i=0; i < m_coefficientsCount; i++ ) {
		double	oldCoeff = _params[i];

		_params[i] -= EPS;
		m_model->Constrain( _params );		// Pom: constrain!
		double	parmMin = _params[i];

		double	offsetValueNeg = m_model->Eval( _params );

		_params[i] = oldCoeff + EPS;
		m_model->Constrain( _params );		// Pom: constrain!
		double	parmMax = _params[i];

		double	offsetValuePos = m_model->Eval( _params );

		_params[i] = oldCoeff;

		double	delta = parmMax - parmMin;
		double	derivative = delta > 0.0 ? (offsetValuePos - offsetValueNeg) / delta : 0.0;

		_gradient[i] = derivative;
	}

	m_evalCallsCount += 2*m_coefficientsCount;
	m_evalGradientCallsCount++;
}

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
double	BFGS::LinearSearch( double _functionMinimum, VectorD& _gradient, VectorD& x, VectorD& _direction, VectorD& _xout, VectorD& _tempDirection ) {
	double	ZERO = 1.0E-10;
	double	SIGMA = 1.0E-4;
	double	BETA = 0.5;

	// [Armijo rule]
//	int		dLimit = x.Length;
	int		dLimit = m_coefficientsCount;

	// Compute direction normalizer:
	double	dnorm = 0.0;
	for ( int d=0; d < dLimit; ++d ) {
		_tempDirection[d] = _direction[d];
		dnorm += _tempDirection[d]*_tempDirection[d];
	}
	dnorm = sqrt( dnorm );
	if ( dnorm <= ZERO )
		throw "Direction is a zero vector!";

	// normalize direction (to avoid making the initial step too big):
	for ( int d=0; d < dLimit; ++d )
		_tempDirection[d] /= dnorm;

	// compute _Gradient * Direction (normalized):
	double	p = 0.0;
	for ( int d=0; d < dLimit; ++d )
		p += _gradient[d] * _tempDirection[d];
	if ( p >= 0.0 )
		throw "Direction is not a descent direction!";

	double	alpha = 1.0; // initial step size
	for ( int i=0; ; ++i ) {
		// take the step:
		for ( int d=0; d < dLimit; ++d )
			_xout[d] = x[d] + alpha * _tempDirection[d];

		// Pom: constrain!
		m_model->Constrain( _xout );

		double	fx_alpha = m_model->Eval( _xout );
		if ( _isnan( fx_alpha ) )
			throw "Linear search eval returned NaN!";

//				System.out.println (i + " _FunctionMinimum = " + fx_alpha);

		if ( fx_alpha < _functionMinimum + SIGMA * alpha * p )
			return fx_alpha;

		if ( i == 0 ) {
			alpha = 0.5 * p / (p + _functionMinimum - fx_alpha);	// first step: do quadratic approximation along the direction line and set alpha to be the minimizer of that approximation:
		} else {
			alpha *= BETA; // reduce the step
		}

		if ( alpha < ZERO ) {
			// prevent alpha from becoming too small
			if ( fx_alpha > _functionMinimum ) {
				for ( int d=0; d < dLimit; ++d )
					_xout[d] = x[d];

				// Pom: constrain!
				m_model->Constrain( _xout );

				return _functionMinimum;
			} else {
				return fx_alpha;
			}
		}
	}
}

// ===========================================
// Useful private functions
// double*	BFGS::InitVector( int _length ) {
// 	double*	m = new double[_length];
// 	memset( m, 0, _length*sizeof(double) );
// 	return m;
// }
// 
// double**	BFGS::InitMatrix( int _rows, int _columns ) {
// 	U8*		raw = new U8[2*sizeof(int) + _rows*sizeof(double*) + _rows*_columns*sizeof(double)];
// 
// 	*((int*) (raw+0)) = _rows;
// 	*((int*) (raw+sizeof(int))) = _columns;
// 
// 	double*	m = (double*) (raw + 2 * sizeof(int) + _rows*sizeof(double*));
// 	memset( m, 0, _rows*_columns*sizeof(double) );
// 
// 	double**	rows = (double**) (raw + 2 * sizeof(int));
// 	for ( int i=0; i < _rows; i++ )// 		rows[i] = &m[_columns*i];// 
// 	return rows;
// }
// 
// void	BFGS::CopyVector( const double* _source, double* _target, int _length ) {
// 	memcpy_s( _target, _length*sizeof(double), _source, _length*sizeof(double) );
// }
// 
// void	BFGS::DeleteVector( double*& _vector ) {
// 	SAFE_DELETE_ARRAY( _vector );
// }
// 
// void	BFGS::DeleteMatrix( double**& _matrix ) {
// 	if ( _matrix == nullptr )
// 		return;
// 
// 	U8*	raw = ((U8*) _matrix) - 2*sizeof(int);	// Address of the large buffer that was allocated is actually 2 ints before the matrix pointer itself
// 	delete[] raw;
// 
// 	_matrix = nullptr;
// }
