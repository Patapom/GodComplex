//////////////////////////////////////////////////////////////////////////
// 
//
#pragma once

namespace MathSolvers {

	// Helper fitting class implementing BFGS optimization
	// http://en.wikipedia.org/wiki/BFGS_method
	// Implementation stolen from http://code.google.com/p/vladium/source/browse/#svn%2Ftrunk%2Foptlib%2Fsrc%2Fcom%2Fvladium%2Futil%2Foptimize
	//
	class BFGS {
	public:
		// Interface to the model to minimize
		class IModel abstract {
		public:
			virtual U32			getParametersCount() const abstract;

			// Gets or sets the free parameters used by the model
			virtual double*		getParameters() const abstract;
			virtual void		setParameters( double value[] ) abstract;

			// Evaluates the model given a set of parameters
			// <returns>The difference between the model's estimate and the measured data</returns>
			virtual double		Eval( double _newParameters[] ) abstract;

			// Applies constraints to the array of parameters
			// <param name="_Parameters"></param>
			virtual void		Constrain( double _parameters[] ) abstract;
		};

		typedef float	(*ProgressCallback_t)( float _progress );


	private:	// FIELDS

		int			m_coefficientsCount;		// Cached amount of coefficients used by the model
		IModel*		m_model;					// Pointer to the model to minimize

		int			m_maxIterations;			// User-specified maximum amount of iterations of the algorithm
		double		m_tolX;						// User-specified tolerance for target minimum
 		double		m_tolGradient;				// User-specified tolerance for gradient progression

		double*		m_previousX;				// Previous set of parameters
		double*		m_currentX;					// Current set of parameters
		double*		m_optimum;					// Current optimum set of parameters
 		double		m_functionMinimum;			// Current function minimum
		int			m_iterationsCount;			// Current amount of iterations performed by the algorithm
		int			m_evalCallsCount;			// (STATS) Amount of model evaluations called to reach minimum
		int			m_evalGradientCallsCount;	// (STATS) Amount of gradient evaluations called to reach minimum


	public:	// PROPERTIES
		// Gets or sets the maximum amount of iterations performed by the algorithm
		int		getMaxIterations() const			{ return m_maxIterations; }
		void	setMaxIterations( int value )		{ m_maxIterations = value; }

		// Gets or sets the tolerance below which the algorithm succeeds
		double	getSuccessTolerance() const			{ return m_tolX; }
		void	setSuccessTolerance( double value ) { m_tolX = value; }

		// Gets or sets the tolerance of gradient magnitude below which the algorithm succeeds
		double	getGradientSuccessTolerance() const			{ return m_tolGradient; }
		void	setGradientSuccessTolerance( double value )	{ m_tolGradient = value; }

		// Gets the amount of iterations performed by the algorithm
		int		getIterationsCount() const			{ return m_iterationsCount; }

		// Gets the minimum reached by the minimization
		double	getFunctionMinimum() const			{ return m_functionMinimum; }

	public:	// METHODS

		BFGS();
		~BFGS();

		// Performs minimization
		// <param name="_model"></param>
		void	Minimize( IModel& _model );

		// ===========================================
		// Compute the gradient using finite differences
		void	EvalGradient( double _Params[], double _Gradient[] );

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		double	LinearSearch( double _FunctionMinimum, double _Gradient[], double x[], double _direction[], double _xout[], double _tempDirection[] );

	private:
		// ===========================================
		// Helpers
		double*		InitVector( int _length );
		double**	InitMatrix( int _rows, int _columns );
		void		CopyVector( const double* _source, double* _target, int _length );
		void		DeleteVector( double*& _vector );
		void		DeleteMatrix( double**& _matrix );
	};

}	// namespace MathSolvers