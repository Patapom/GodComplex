using System;
using System.Collections.Generic;

namespace WMath
{
	/// <summary>
	///  Helper fitting class implementing BFGS optimization
	///  http://en.wikipedia.org/wiki/BFGS_method
	///  Implementation stolen from http://code.google.com/p/vladium/source/browse/#svn%2Ftrunk%2Foptlib%2Fsrc%2Fcom%2Fvladium%2Futil%2Foptimize
	/// </summary>
	public class BFGS {

		public interface Model {
			/// <summary>
			/// Gets or sets the free parameters used by the model
			/// </summary>
			double[]	Parameters	{ get; set; }

			/// <summary>
			/// Evaluates the model given a set of parameters
			/// </summary>
			/// <returns>The difference between the model's estimate and the measured data</returns>
			double		Eval( double[] _NewParameters );

			/// <summary>
			/// Applies constraints to the array of parameters
			/// </summary>
			/// <param name="_Parameters"></param>
			void		Constrain( double[] _Parameters );
		}

		public delegate float	ProgressCallback( float _progress );

		int			m_coefficientsCount;
		Model		m_model;

		int			m_maxIterations = 200;
		double		m_tolX = 1.0e-8;
 		double		m_tolGradient = 1.0e-8;

		double[]	m_previousX;		// Previous set of parameters
		double[]	m_currentX;			// Current set of parameters
		double[]	m_optimum = null;
 		double		m_functionMinimum = 1e38;
		int			m_iterationsCount = 0;
		int			m_evalCallsCount = 0;
		int			m_evalGradientCallsCount = 0;

		/// <summary>
		/// Gets or sets the maximum amount of iterations performed by the algorithm
		/// </summary>
		public int		MaxIterations {
			get { return m_maxIterations; }
			set { m_maxIterations = value; }
		}

		/// <summary>
		/// Gets or sets the tolerance below which the algorithm succeeds
		/// </summary>
		public double	SuccessTolerance {
			get { return m_tolX; }
			set { m_tolX = value; }
		}

		/// <summary>
		/// Gets or sets the tolerance of gradient magnitude below which the algorithm succeeds
		/// </summary>
		public double	GradientSuccessTolerance {
			get { return m_tolGradient; }
			set { m_tolGradient = value; }
		}

		/// <summary>
		/// Gets the amount of iterations performed by the algorithm
		/// </summary>
		public int		IterationsCount {
			get { return m_iterationsCount; }
		}

		/// <summary>
		/// Gets the minimum reached by the minimization
		/// </summary>
		public double	FunctionMinimum {
			get { return m_functionMinimum; }
		}

		/// <summary>
		/// Performs minimization
		/// </summary>
		/// <param name="_model"></param>
		public void	Minimize( Model _model ) {
			m_model = _model;
			m_coefficientsCount = m_model.Parameters.Length;

			m_previousX = InitVector( m_coefficientsCount );
			m_currentX = InitVector( m_coefficientsCount );
			m_optimum = InitVector( m_coefficientsCount );

			// Start from model's initial parameters
			m_model.Parameters.CopyTo( m_previousX, 0 );

			double[]	direction = InitVector( m_coefficientsCount ); // x_k+1 = x_k + alpha_k*direction_k

			double[]	Gradient = InitVector( m_coefficientsCount );
			double[]	PreviousGradient = InitVector( m_coefficientsCount );

			double[,]	Hessian = InitMatrix(m_coefficientsCount,m_coefficientsCount); // inverse Hessian approximation

			double[]	pi = InitVector( m_coefficientsCount );  // p_i = x_i+1 - x_i
			double[]	qi = InitVector( m_coefficientsCount );  // q_i = Gradient_i+1 - Gradient_i
			double[]	Dqi = InitVector( m_coefficientsCount ); // Dq_i = |D_i|.q_i:

			m_evalCallsCount = m_evalGradientCallsCount = 0; // count of function and gradient evaluations

			// Perform initial evaluation
			m_functionMinimum = m_model.Eval( m_previousX );	// Initial error
			EvalGradient( m_previousX, PreviousGradient );		// Initial gradient
			m_evalCallsCount++;

			for ( int d = 0; d < m_coefficientsCount; ++d ) {
				// initialize Hessian to a unit matrix:
				Hessian[d,d] = 1.0;

				// set initial direction to opposite of the starting gradient (since Hessian is just a unit matrix):
				direction[d] = -PreviousGradient[d];
			}
 
			// perform a max of 'maxiterations' of quasi-Newton iteration steps
			double	temp1, temp2;
		
			m_iterationsCount = 0;
			while ( ++m_iterationsCount < m_maxIterations ) {
				// do the line search in the current direction:
				double	newMinimum = LinearSearch( m_functionMinimum, PreviousGradient, m_previousX, direction, m_currentX );	// this updates _functionMinimum and x
				m_functionMinimum = newMinimum;

				// Notify of new optimal values
				m_currentX.CopyTo( m_optimum, 0 );
				m_model.Parameters = m_optimum;

				// if the current point shift (relative to current position) is below tolerance, we're done:
				double	delta = 0.0;
				for ( int d=0; d < m_coefficientsCount; ++d ) {
					pi[d] = m_currentX[d] - m_previousX[d];

					temp2 = Math.Abs( pi[d] ) / Math.Max( Math.Abs( m_currentX[d] ), 1.0 );
					if ( temp2 > delta )
						delta = temp2;
				}
				if ( delta < m_tolX )
					break;

				// Get the current gradient:			// TODO: use 1 extra _functionMinimum eval gradient version?
				EvalGradient( m_currentX, Gradient );
   
				// if the current gradient (normalized by the current x and _functionMinimum) is below tolerance, we're done:
				delta = 0.0;
				temp1 = Math.Max( m_functionMinimum, 1.0 );
				for ( int d=0; d < m_coefficientsCount; ++d ) {
					temp2 = Math.Abs( Gradient[d] ) * Math.Max( Math.Abs( m_currentX[d] ), 1.0 ) / temp1;
					if ( temp2 > delta )
						delta = temp2;
				}
				if ( delta < m_tolGradient )
					break;

				// Compute q_i = Gradient_i+1 - Gradient_i:
				for ( int d=0; d < m_coefficientsCount; ++d )
					qi[d] = Gradient[d] - PreviousGradient[d];

				// Compute Dq_i = |D_i|.q_i:
				for ( int m=0; m < m_coefficientsCount; ++m ) {
					Dqi[m] = 0.0;
					for ( int n=0; n < m_coefficientsCount; ++n )
						Dqi[m] += Hessian[m,n] * qi[n];
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
				if ( piqi > ZERO_PRODUCT * Math.Sqrt(qi_norm * pi_norm) ) {
					// re-use qi vector to compute v in Bertsekas:
					for ( int d=0; d < m_coefficientsCount; ++d )
						qi[d] = pi[d] / piqi - Dqi[d] / qiDqi;

					for ( int m=0; m < m_coefficientsCount; ++m ) {
						for ( int n=m; n < m_coefficientsCount; ++n ) {
							Hessian[m,n] += pi[m] * pi[n] / piqi - Dqi[m] * Dqi[n] / qiDqi + qiDqi * qi[m] * qi[n];
							Hessian[n,m] = Hessian[m,n];
						}
					}
				}

				// set current direction for the next iteration as -|Hessian|.Gradient 
				for ( int m=0; m < m_coefficientsCount; ++m ) {
					direction[m] = 0.0;
					for ( int n = 0; n < m_coefficientsCount; ++n )
						direction[m] -= Hessian[m,n] * Gradient[n];
				}

				// update current point and current gradient for the next iteration:
				if ( m_iterationsCount < m_maxIterations-1 ) {	// keep the 'x contains the latest point' invariant for the post-loop copy below
					double[]	temp = Gradient;
					Gradient = PreviousGradient;
					PreviousGradient = temp;

					temp = m_currentX;
					m_currentX = m_previousX;
					m_previousX = temp;
				}
			}

			// Copy final parameters
			m_currentX.CopyTo( m_optimum, 0 );
			m_model.Parameters = m_optimum;
		}

		// ===========================================
		// Compute the gradient using finite differences
		void	EvalGradient( double[] _Params, double[] _Gradient ) {
			double	EPS = 1e-6;
			double	CentralValue = m_functionMinimum;

			for ( int i=0; i < m_coefficientsCount; i++ ) {
				double	OldCoeff = _Params[i];

				_Params[i] -= EPS;
				m_model.Constrain( _Params );		// Pom: constrain!
				double	parmMin = _Params[i];

				double	OffsetValueNeg = m_model.Eval( _Params );

				_Params[i] += 2*EPS;
				m_model.Constrain( _Params );		// Pom: constrain!
				double	parmMax = _Params[i];

				double	OffsetValuePos = m_model.Eval( _Params );

				_Params[i] = OldCoeff;

//				double	derivative = (OffsetValue - CentralValue) / EPS;
//				double	derivative = (OffsetValuePos - OffsetValueNeg) / (2.0*EPS);
				double	delta = parmMax - parmMin;
				double	derivative = delta > 0.0 ? (OffsetValuePos - OffsetValueNeg) / delta : 0.0;

				_Gradient[i] = derivative;
			}

			m_evalCallsCount += 2*m_coefficientsCount;
			m_evalGradientCallsCount++;
		}

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		double	LinearSearch( double _FunctionMinimum, double[] _Gradient, double[] x, double[] _Direction, double[] _xout ) {
			double	ZERO = 1.0E-10;
			double	SIGMA = 1.0E-4;
			double	BETA = 0.5;

			// [Armijo rule]
			int		dLimit = x.Length;

			// Compute direction normalizer:
			double[]	Direction = InitVector( _Direction.Length );
			double		dnorm = 0.0;
			for ( int d=0; d < dLimit; ++d ) {
				Direction[d] = _Direction[d];
				dnorm += Direction[d]*Direction[d];
			}
			dnorm = Math.Sqrt( dnorm );
			if ( dnorm <= ZERO )
				throw new Exception( "Direction is a zero vector!" );

			// normalize direction (to avoid making the initial step too big):
			for ( int d=0; d < dLimit; ++d )
				Direction[d] /= dnorm;

			// compute _Gradient * Direction (normalized):
			double	p = 0.0;
			for ( int d=0; d < dLimit; ++d )
				p += _Gradient[d] * Direction[d];
			if ( p >= 0.0 )
				throw new Exception( "'Direction is not a descent direction [p = " + p + "]!" );

			double	alpha = 1.0; // initial step size
			for ( int i=0; ; ++i ) {
				// take the step:
				for ( int d=0; d < dLimit; ++d )
					_xout[d] = x[d] + alpha * Direction[d];

				// Pom: constrain!
				m_model.Constrain( _xout );

				double	fx_alpha = m_model.Eval( _xout );
//				System.out.println (i + " _FunctionMinimum = " + fx_alpha);

				if ( fx_alpha < _FunctionMinimum + SIGMA * alpha * p )
					return fx_alpha;

				if ( i == 0 ) {
					alpha = 0.5 * p / (p + _FunctionMinimum - fx_alpha);	// first step: do quadratic approximation along the direction line and set alpha to be the minimizer of that approximation:
				} else {
					alpha *= BETA; // reduce the step
				}

				if ( alpha < ZERO ) {
					// prevent alpha from becoming too small
					if ( fx_alpha > _FunctionMinimum ) {
						for ( int d=0; d < dLimit; ++d )
							_xout[d] = x[d];

						// Pom: constrain!
						m_model.Constrain( _xout );

						return _FunctionMinimum;
					} else {
						return fx_alpha;
					}
				}
			}
		}

		// ===========================================
		// Useful private functions
		double[]	InitVector( int _Length ) {
			double[]	m = new double[_Length];
// 			for ( int i=0; i < _Length; i++ )
// 				m[i] = 0.0;

			return m;
		}

		double[,]	InitMatrix( int _Rows, int _Columns ) {
			double[,]	m = new double[_Rows,_Columns];
// 			for ( int i=0; i < _Rows; i++ )
// 				for ( int j=0; j < _Columns; j++ )
// 					m[i,j] = 0.0;

			return m;
		}

	}
}
