//////////////////////////////////////////////////////////////////////////
// Inspired by http://http.developer.nvidia.com/GPUGems/gpugems_ch18.html
//
// All code translated from the BRDF framework by David K. McAllister
// Available from http://sbrdf.cs.unc.edu/Code/index.html
// 
// The Vector & Matrix types are the simplest possible and don't perform (almost) any check regarding dimensions matching
//
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace BRDFLafortuneFitting
{
	public class LevenbergMarquardt
	{
		#region CONSTANTS

		public const double	EPSILON = double.Epsilon;

		#endregion

		#region NESTED TYPES

		public interface	IChatte
		{
			/// <summary>
			/// Evaluates the function to minimize for a given set of points and the current jacobian
			/// </summary>
			/// <param name="_Point"></param>
			/// <param name="_Jacobian"></param>
			/// <returns></returns>
			double		Eval( Vector _Point, Matrix _Jacobian );
		}

		public class	Estimator
		{

		}

		public class	Vector
		{
			public double[]		m;
			public int			Size;

			public double	this[int i]
			{
				get { return m[i]; }
				set { m[i] = value; }
			}

			public	Vector( int _Size )
			{
				m = new double[_Size];
				Size = _Size;
			}

			public	Vector( Vector _Source )
			{
				m = new double[_Source.Size];
				Size = _Source.Size;
				for ( int i=0; i < Size; i++ )
					m[i] = _Source.m[i];
			}

			public double	Dot( Vector v )
			{
				double	Result = 0.0;
				for ( int i=0; i < Size; i++ )
					Result += m[i] * v.m[i];

				return Result;
			}

			public static Vector	operator+( Vector a, Vector b )
			{
				Vector	R = new Vector( a );
				for ( int i=0; i < a.Size; i++ )
					R.m[i] += b.m[i];
				return R;
			}

			// Dot product
			public static double	operator*( Vector a, Vector b )
			{
				double	R = 0.0;
				for ( int i=0; i < a.Size; i++ )
					R += a.m[i] * b.m[i];
				return R;
			}

			public static Vector	operator*( double a, Vector b )
			{
				Vector	R = new Vector( b );
				for ( int i=0; i < b.Size; i++ )
					R.m[i] *= a;
				return R;
			}

			public static Vector	operator*( Vector a, double b )
			{
				Vector	R = new Vector( a );
				for ( int i=0; i < a.Size; i++ )
					R.m[i] *= b;
				return R;
			}
		}

		public class	Matrix
		{
			public double[,]	m;
			public int			Columns = 0, Rows = 0;

			public double	this[int i, int j]
			{
				get { return m[i,j]; }
				set { m[i,j] = value; }
			}

			// Builds an empty matrix
			public Matrix( int _Columns, int _Rows )
			{
				m = new double[_Rows, _Columns];
				Columns = _Columns;
				Rows = _Rows;
			}

			// Copies a matrix
			public Matrix( Matrix _Source )
			{
				m = new double[_Source.Rows, _Source.Columns];
				Columns = _Source.Columns;
				Rows = _Source.Rows;
			}

			// <summary>
			// Builds the matrix from the outer product of 2 vectors
			// </summary>
			// <param name="a"></param>
			// <param name="b"></param>
			public Matrix( Vector a, Vector b )
			{
				m = new double[a.Size, a.Size];
				Columns = Rows = a.Size;

				for( int i=0; i < a.Size; i++ )
					for( int j=0; j < b.Size; j++ )
						m[i,j] = a[i]*b[j];
			}

			// Solve least square problem by QR-decomposition:
			//  minimize ||M*x - b||.  Returns x.
			//
			// The method works only for well-determined (#rows = #columns) and over-determined (#rows > #columns) systems.
			//
			public Vector	SolveLeastSquareQR( Vector b )
			{
				int		l = Rows > Columns ? Columns : Rows;

				//////////////////////////////////////////////////////////////////////////
				// QR-decompose this = q*r
				Matrix		Q = new Matrix( Rows, Columns );
				Matrix		R = new Matrix( Columns, Columns );
				
				// Make vectors x out of columns. Vectors g will populate q-matrix.
				Vector[]	x = new Vector[Columns];
				Vector[]	g = new Vector[Columns];

				for ( int k=0; k < Columns; k++ )
				{ 
					x[k] = new Vector( Rows );
					g[k] = new Vector( Rows );
					for( int i=0; i < Rows; i++ )
						x[k].m[i] = m[i,k];
				}

				// The main loop
				for ( int k=0; k < Columns; k++ )
				{
					// Calculate r(k,k): norm of x[k]
					R.m[k,k] = Math.Sqrt( x[k].Dot( x[k] ) );
					if ( R.m[k,k] == 0.0 )
					{
						for ( int i=0; i < Rows; i++ )
							g[k].m[i] = 0.0;
					}
					else 
					{	// set g-s
						for ( int i=0; i < Rows; i++ )
						{
							double	temp = 1.0 / R.m[k,k];
							g[k].m[i] = x[k].m[i] * temp;
						}
					}

					// Calculate non-diagonal elements of r-matrix
					for ( int j=k+1; j < Columns; j++ )
					{
						R.m[j,k] = 0.0;
						for ( int i=0; i < Rows; i++ )
							R.m[k,j] += x[j].m[i] * g[k].m[i];
					}  

					// Reset x-s
					for ( int j=k+1; j < Columns; j++ ) 
						for ( int i=0; i < Rows; i++ )
							x[j].m[i] -= R.m[k,j] * g[k].m[i];
				}

				// Make q out of g-s
				for( int i=0; i < Rows; i++ )
					for( int k=0; k < Columns; k++ )
						Q.m[i,k] = g[k].m[i]; 


				//////////////////////////////////////////////////////////////////////////
				// Solve R*x = Q.setToAdjoint()*b
				Q = Q.GetTranspose();
				Vector	y = Q * b;
				Vector	Result = R.SolveBackwardSubstitution( y );
				return Result;
			}

			public void		SetToIdentity()
			{
				Array.Clear( m, 0, m.Length );
				int	Min = Math.Min( Rows, Columns );
				for ( int i=0; i < Min; i++ )
					m[i,i] = 1.0;
			}

			public Matrix	GetTranspose()
			{
				Matrix	R = new Matrix( Columns, Rows );
				for ( int i=0; i < Rows; i++ )
					for ( int j=0; j < Columns; j++ )
						R.m[j,i] = m[i,j];

				return R;
			}

			public Vector	SolveBackwardSubstitution( Vector b )
			{
				if ( Rows != b.Size )
					throw new Exception( "Can't solve!" );
				if ( !IsUpperDiagonal )
					throw new Exception( "Can't solve!" );

				Vector	R = new Vector( Rows );
				R[Rows-1] = b[Rows-1] / m[Rows-1,Rows-1];
				for ( int i=Rows-2; i >= 0; i-- )
				{
					double	temp = 0.0;
					for ( int k=i+1; k < Rows; k++ )
						temp += R[k] * m[i,k]; 
				
					double	Div = m[i,i];
					if ( Div != 0.0 ) 
						R[i] = (b[i] - temp) / Div;
				}

				return R;
			}

			public bool		IsUpperDiagonal
			{
				get
				{
					double	mx = 0.0;
					for ( int i=0; i < Rows; i++)
						for( int j=i+1; j < Rows; j++ )
							mx = Math.Max( mx, Math.Abs( m[j,i] ) );

					double	eps = 2.0 * EPSILON * (mx + 1.0);
					for ( int i=0; i < Rows; i++ )
						for ( int j=i+1; j < Rows; j++ )
							if ( Math.Abs( m[j,i] ) > eps )
								return false;

					return true;
				}
			}

			public Matrix	GetLeftHouseholderMatrix( int k )
			{
				return GetLeftHouseholderMatrix( k, 0 );
			}
			// Follow closely "Introduction to Numerical Analysis" by Stoer and Bulirish.
			public Matrix		GetLeftHouseholderMatrix( int k, int z )
			{
				// The Householder matrix to be built
				Matrix	H = new Matrix( Rows, Rows );
						H.SetToIdentity();

				// x is built from the kth row of the matrix, out of last m-k elements
				Vector	x = new Vector( Rows - k - z );
				for ( int i=0; i < x.Size; i++ )
					x[i] = m[k+i+z, k];

				// Phase of x[0]: x[0] = |x[0]|*Phase
				double	Phase = x[0];
				double	ModX0 = Math.Abs( Phase );

				// Norm of x
				double sig = Math.Sqrt( x.Dot( x ) );
				if( sig == 0.0 )
					return H;

				// Vector used for buiding the projector
				Vector	u = new Vector( x );
				u[0] += Phase*sig;

				// Renorm factor
				double	beta = 1.0 / (sig*(sig+ModX0));

				// Householder projector: right lower block of Householder matrix
				Matrix	p = new Matrix( x.Size, x.Size );
						p.SetToIdentity();

				// Direct product u*u
				Matrix	uu = new Matrix( u, u );
						uu *= beta;

				p -= uu;

				// Build the projector part
				for( int i=k+z; i < Rows; i++ )
					for( int j=k+z; j < Rows; j++ )
						H.m[i,j] = p.m[i-(k+z), j-(k+z)];

				return H;
			}

			public static Matrix	operator+( Matrix a, Matrix b )
			{
				Matrix	R = new Matrix( a );
				for ( int i=0; i < a.Rows; i++ )
					for ( int j=0; j < a.Columns; j++ )
						R.m[i,j] += b.m[i,j];

				return R;
			}

			public static Matrix	operator-( Matrix a, Matrix b )
			{
				Matrix	R = new Matrix( a );
				for ( int i=0; i < a.Rows; i++ )
					for ( int j=0; j < a.Columns; j++ )
						R.m[i,j] -= b.m[i,j];

				return R;
			}

			public static Matrix	operator*( double a, Matrix b )
			{
				Matrix	R = new Matrix( b );
				for ( int i=0; i < b.Rows; i++ )
					for ( int j=0; j < b.Columns; j++ )
						R.m[i,j] *= a;

				return R;
			}

			public static Matrix	operator*( Matrix a, double b )
			{
				Matrix	R = new Matrix( a );
				for ( int i=0; i < a.Rows; i++ )
					for ( int j=0; j < a.Columns; j++ )
						R.m[i,j] *= b;

				return R;
			}

			public static Vector	operator*( Matrix a, Vector b )
			{
				if ( a.Columns != b.Size )
					throw new Exception( "Can't multiply!" );

				Vector	R = new Vector( a.Rows );
				for ( int i=0; i < a.Rows; i++ )
				{
					double	C = 0.0;
					for ( int j=0; j < a.Columns; j++ )
						C += a.m[i,j] * b.m[j];
					R.m[i] = C;
				}

				return R;
			}

			public static Vector	operator*( Vector a, Matrix b )
			{
				if ( b.Rows != a.Size )
					throw new Exception( "Can't multiply!" );

				Vector	R = new Vector( b.Columns );
				for ( int j=0; j < b.Columns; j++ )
				{
					double	C = 0.0;
					for ( int i=0; i < b.Rows; i++ )
						C += b.m[i,j] * a.m[i];
					R.m[j] = C;
				}

				return R;
			}

			public static Matrix	operator*( Matrix a, Matrix b )
			{
				if ( a.Columns != b.Rows )
					throw new Exception( "Can't multiply!" );

				Matrix	R = new Matrix( a.Rows, a.Columns );
				int i, j, k;
				for ( i=0; i < a.Rows; i++ )
					for ( j=0; j < a.Columns; j++ )
					{
						double	C = 0.0;
						for ( k=0; k < a.Columns; k++ )
							C += a.m[i,k] * b.m[k,j];
						R.m[i,j] = C;
					}

				return R;
			}
		}

		public class	Constraints
		{
			protected Vector	m_Min = null;
			protected Vector	m_Max = null;

			public Constraints( int _Size )
			{
				m_Min = new Vector( _Size );
				m_Max = new Vector( _Size );

				for ( int i=0; i < _Size; i++ )
				{
					m_Min[i] = -double.MaxValue;
					m_Max[i] = +double.MaxValue;
				}
			}

			public void		SetConstraint( int _Index, double _Min, double _Max )
			{
				m_Min[_Index] = _Min;
				m_Max[_Index] = _Max;
			}
		}

		#endregion

		#region FIELDS

		protected int			m_Dimension = 0;				// Dimension of the dataset to solve parameters for
		protected int			m_ParametersCount = 0;			// Dimension of the vector of parameters to solve
		protected Constraints	m_Constraints = null;			// Constraints for the parameters

		protected int			m_MaxIterations;
		protected int			m_IterationsCount;

		protected IChatte		m_Estimator = null;

		// General fitting data
		protected Matrix		m_Jacobian = null;				// The jacobian matrix of size Dimension X ParametersCount
		protected Matrix		m_JacobianEx = null;			// The extended jacobian matrix of size (ParametersCount+Dimension) X Dimension

		protected Vector		m_InitialPoint = null;			// The point used as the initial guess.
		protected Vector		m_MinPoint = null;				// Coordinates yielding the minimum value of the function.
		protected Vector		m_PreviousMinPoint = null;		// Vector holding the previous point.
		protected Vector		m_TempPoint = null;				// Possible value of the argument in the next step.
		protected Vector		m_Scale = null;					// The scale lengths used for the initial guess.
		protected Vector		m_Direction = null;				// Direction used for line search or otherwise.

		protected double		m_MinValue;						// The minimum (presumably optimum) value of the function. 
		protected double		m_PreviousMinValue;				// The "previous" value of m_MinValue.

		// Specific Levenberg-Marquardt data
		protected double		ro = 1.0;						// Ratio of m_Reduction to m_PredictedReduction.
		protected double		b = 0.0001;						// Auxialliary number used in criterion for stepping.
		protected double		m_Lambda = 0.1;					// Levenberg-Marquardt parameter.
		protected double		m_InitialLambda = 0.1;

		protected Vector		m_Delta = null;					// The step in arguments.
		protected Vector		m_Diagonal = null;				// Scaling diagonal matrix, stored as a vector.
		protected Vector		m_FunctionValues = null;		// The vector of function values (size = ParametersCount).
		protected Vector		m_FunctionValuesEx = null;		// The extended vector of function values (size = ParametersCount+Dimension).
 
		protected double		m_Reduction;					// Actual m_Reduction of the function value in the step.
		protected double		m_PredictedReduction;			// Predicted by linear model m_Reduction.

		// Tolerance & Accuracy Measurements
		protected double		m_AbsoluteGradTolerance;		// Absolute gradient tolerance.
		protected double		m_AchievedGradTolerance;		// Achieved gradient tolerance.
		protected Vector		m_AbsoluteTolerance;			// Absolute tolerance (one for each dimension).
		protected Vector		m_RelativeTolerance;			// Relative tolerance (one for each dimension).
		protected Vector		m_TotalTolerance;				// Total specified tolerance at convergence (one for each dimension).
		protected Vector		m_AchievedTolerance;			// Tolerance achieved at convergence.

		protected double		m_RelativeAccuracy;				// Relative accuracy for the optimization.
		protected double		m_AbsoluteAccuracy;				// Absolute accuracy for the optimization.
		protected double		m_TotalAccuracy;				// Total specified accuracy at convergence.
		protected double		m_AchievedAccuracy;				// Accuracy achieved at convergence.

		protected int 			m_RepeatedConvergenceFailures;	// Number of repeated resets due to convergence failure.
		protected int 			m_RepeatedLineSearchFailures;	// Number of repeated resets due to line search failure.

		#endregion

		#region PROPERTIES

		public int	Dimension
		{
			get { return m_Dimension; }	// TODO!
		}

		public Constraints	VectorConstraints
		{
			get { return m_Constraints; }
		}

		public double	AbsoluteTolerance
		{
			set { for ( int i=0; i < m_AbsoluteTolerance.Size; i++ ) m_AbsoluteTolerance[i] = value; }
		}

		public double	RelativeTolerance
		{
			set { for ( int i=0; i < m_RelativeTolerance.Size; i++ ) m_RelativeTolerance[i] = value; }
		}

		// Determine whether or not the desired accuracy or tolerance was achieved.
		public bool		IsSolved
		{
			get { return IsToleranceAchieved || IsAccuracyAchieved; }
		}
	
		// Check for achieved accuracy.
		public bool		IsAccuracyAchieved
		{
			get
			{
				double	Factor = Math.Min( 1.0, Math.Abs( m_MinValue ) );

				double	TotalAccuracy = m_AbsoluteAccuracy + m_RelativeAccuracy * Factor;
				double	AchievedAccuracy = Math.Abs( m_MinValue - m_PreviousMinValue );

				return AchievedAccuracy <= TotalAccuracy;
			}
		}

		public bool		IsToleranceAchieved
		{
			get
			{
				Vector	DiffPoint = new Vector( Dimension);
				for ( int i=0; i < Dimension; i++ )
					DiffPoint[i] = m_MinPoint[i] - m_PreviousMinPoint[i];

				double	Factor = 0.0;
				bool	Result = true;
				for ( int i=0; i < Dimension; i++ )
				{
					Factor = Math.Min( 1.0, Math.Abs( m_MinPoint[i] ) );

					m_TotalTolerance[i] = m_AbsoluteTolerance[i] + m_RelativeTolerance[i] * Factor;
					m_AchievedTolerance[i] = Math.Abs( DiffPoint[i] );
					if ( m_AchievedTolerance[i] >= m_TotalTolerance[i] )
						Result = false;
				}

				return Result;
			}
		}

		#endregion

		#region METHODS

		public LevenbergMarquardt()
		{
		}

		/// <summary>
		/// Initializes the solver
		/// </summary>
		/// <param name="_Dimension">Size of the dataset to solve against (i.e. amount of data points to fit against)</param>
		/// <param name="_ParametersCount">Size of the vector to solve for (i.e. amount of unknowns to fit)</param>
		/// <param name="_MaxIterations">Maximum amount of iterations to perform (20 iters significantly increases error vs. 100, a standard value is 45)</param>
		public void		Init( int _Dimension, int _ParametersCount, int _MaxIterations )
		{
			m_Dimension = _Dimension;
			m_ParametersCount = _ParametersCount;
			m_MaxIterations = _MaxIterations;

			m_Constraints = new Constraints( m_ParametersCount );

			m_Delta = new Vector( m_Dimension );
 			m_Diagonal = new Vector( m_Dimension );
			m_Jacobian = new Matrix( m_ParametersCount, m_Dimension );
			m_JacobianEx = new Matrix( m_Dimension+m_ParametersCount, m_Dimension );

			m_TempPoint = new Vector( m_Dimension );
		}

		/// <summary>
		/// Solves the problem
		/// </summary>
		/// <param name="_InitialGuess"></param>
		/// <param name="_Estimator"></param>
		public void		Solve( Vector _InitialGuess, IChatte _Estimator )
		{
			SetData( _InitialGuess );
			m_Estimator = _Estimator;

			m_IterationsCount = 0;
			Reset();
			Complete();
		}

		protected void	SetData( Vector _Point )
		{
		}

		// Convert from the original argument x (specified component only) to the corresponding component of the transformed argument y.
		// As y varies from -inf to +inf, x varies from the specified lower bound (if any) to the specified upper bound (if any).
		protected double	convertToTransformedArg( Vector x, int i )
		{
			double	y = 0.0;
 
// 			// the lower bound has been set
// 			if (lowerBoolVector[i])
// 			{
// 				// the upper bound has also been set
// 				if (upperBoolVector[i])
// 				{
// 					if ( x[i]>=upperBounds[i] || x[i]<=lowerBounds[i] )
// 					{
// 						cerr << endl;
// 						cerr << "Problem in TxBounds::convertToTransformedArg():";
// 						cerr << endl;
// 						cerr << "  x[" << i << "] = " << x[i] << endl;
// 						cerr << "  l[" << i << "] = " << lowerBounds[i] << endl;
// 						cerr << "  u[" << i << "] = " << upperBounds[i] << endl;
// 						cerr << endl;
// 					}
// 
// 					if ( x[i] > xMid[i] )
// 						y = alphaSq[i] * log( alphaSq[i] / (upperBounds[i]-x[i]) );
// 					else
// 						y = alphaSq[i] * log( (x[i]-lowerBounds[i]) / alphaSq[i] );
// 				}
// 				else
// 				{	// the upper bound has NOT been set
// 
// 					if (x[i]<=lowerBounds[i])
// 					{
// 						cerr << endl;
// 						cerr << "Problem in TxBounds::convertToTransformedArg():";
// 						cerr << endl;
// 						cerr << "  x[" << i << "] = " << x[i] << endl;
// 						cerr << "  l[" << i << "] = " << lowerBounds[i] << endl;
// 						cerr << endl;
// 					}
// 
// 					if ( x[i] >= lowerBounds[i]+alphaSq[i] )
// 						y = x[i] - lowerBounds[i] - alphaSq[i];
// 					else
// 						y = alphaSq[i] * log( (x[i]-lowerBounds[i]) / alphaSq[i] );
// 				}
// 			}
// 			else
// 			{	// the lower bound has NOT been set
// 				if ( upperBoolVector[i] )
// 				{	// the upper bound HAS been set
// 					if (x[i]>=upperBounds[i])
// 					{
// 						cerr << endl;
// 						cerr << "Problem in TxBounds::convertToTransformedArg():";
// 						cerr << endl;
// 						cerr << "  x[" << i << "] = " << x[i] << endl;
// 						cerr << "  u[" << i << "] = " << upperBounds[i] << endl;
// 						cerr << endl;
// 					}
// 
// 				if ( x[i] <= upperBounds[i]-alphaSq[i] )
// 					y = x[i] - upperBounds[i] + alphaSq[i];
// 				else
// 					y = alphaSq[i] * log( alphaSq[i] / (upperBounds[i]-x[i]) );
// 				}
// 				else
// 					y = x[i];		// NEITHER the upper bound NOR the lower bound have been set
// 			}

			return y;
		}

		protected void	Reset()
		{
		   if ( m_IterationsCount == 0 )
			   m_Lambda = m_InitialLambda;

			for ( int i=0; i < m_Dimension; i++ )
			{
				m_MinPoint[i] = m_InitialPoint[i];
				m_PreviousMinPoint[i] = m_MinPoint[i] - 1.0;
			}

			// Calculate the function value and the jacobian
			m_MinValue = m_Estimator.Eval( m_MinPoint, m_Jacobian );
			m_PreviousMinValue = m_MinValue + 1;

			// Calculate initial scaling factor
			for ( int i=0; i < m_Dimension; i++ )
			{
				double	Diag = 0.0;
				for ( int j=0; j < m_ParametersCount; j++ )
					Diag += m_Jacobian[j,i] * m_Jacobian[j,i];
				Diag = Math.Sqrt( Diag );
				if ( Diag < Math.Sqrt( EPSILON ) )
					Diag = 1.0;

				m_Diagonal[i] = Diag;
			}
		}

		protected void	Complete()
		{
			// Iterate through the minimization loop.
			for ( m_IterationsCount = 0; m_IterationsCount < m_MaxIterations; m_IterationsCount++ )
			{
				// Take one minimization step and prepare for next.
				try
				{
					Step();
				}
				// Take care of any required error handling.
				catch ( Exception _e )
				{
// 					osx << "Exception caught:  " << __FILE__ << ":" << __LINE__ << "\n";
// 					osx << "in void complete()\n";
// 					osx << "  --now passing exception to handleExceptions().\n\n";
// 					handleExceptions(osx);
				}

				// Check for success criterion.
				if( IsSolved )
				{
					m_IterationsCount += 1;
					return;
				}

				// Prepare for the next optimization step
				try
				{
					PrepareStep();
				}
				catch ( Exception _e )
				{
// 					osx << "Exception caught:  " << __FILE__ << ":" << __LINE__ << "\n";
// 					osx << "in void complete()\n";
// 					osx << "  --now passing exception to handleExceptions().\n\n";
// 					handleExceptions(osx);
				}
			}
		}

		protected void	PrepareStep()
		{
			// updateTrustRegion uses "old" m_MinPoint, that is why goes first
			UpdateTrustRegion();

			// If the new point is good, move
			UpdateMin();

			// updateScaling uses a new point, that is why goes last
			UpdateScaling();
		}

		protected void	Step()
		{
			// Evaluate the functions values and jacobian.
			m_MinValue = m_Estimator.Eval( m_MinPoint, m_Jacobian );

			// Find extended matrix
			for ( int i=0; i < m_ParametersCount; i++ )
				for ( int j=0; j < m_Dimension; j++ )
					m_JacobianEx[i,j] = m_Jacobian[i,j];

			double	SqrtLambda = Math.Sqrt( m_Lambda );
			for ( int i=0; i < m_Dimension; i++ )
				m_JacobianEx[i+m_ParametersCount,i] = SqrtLambda * m_Diagonal.m[i];

			// Find extended vector of function values
			for ( int i=0; i < m_ParametersCount; i++ )
			{
				double	Value = m_Estimator.GetCalcFunctionValues()[i];
				m_FunctionValues.m[i] = Value;
				m_FunctionValuesEx.m[i] = -Value;	// !! Mind the NEGATIVE sign here !!
			}

			// Solve min ||jacExt*x+funValsExt||
//			if(isQR)
				m_Delta = m_JacobianEx.SolveLeastSquareQR( m_FunctionValuesEx );
// 			else
// 				m_Delta = jacExt->solveLstSqrSVD(-m_FunctionValuesEx);

			// Candidate for new minimizer
			for ( int i=0; i < m_Dimension; i++)
				m_TempPoint.m[i] = m_MinPoint[i] + m_Delta[i];

			// Calculate ro : showing how expansion works for the function
			Vector	Temp = m_Jacobian * m_Delta;
			m_PredictedReduction = -(Temp * m_FunctionValues) - Temp*Temp*0.5;
			m_Reduction = m_MinValue - m_Estimator.Glou( m_TempPoint );
			if ( m_PredictedReduction == m_Reduction )
				ro = 1;
			else
				ro = m_Reduction/m_PredictedReduction;
		}

/*		protected void	UpdateJacobianBroyden( ArgVecType& v, Matrix& jac )
		{

		// Use finite difference if v is the initial point
			if(!isInitialized)
			{
				try
				{
					resetJac(v);
				}
				catch  (TxBadEvalExcept& ex)
				{
				ex << "Exception caught:  " << __FILE__ << ":" << __LINE__ << "\n";
				ex << "in TxVectorFunctor::getBroydJacobian"
					<< "(const ArgVecType&, Matrix&)\n";
				ex << "  --rethrowing now.\n\n";
				throw;
				}
				catch (...) {
				TxBadEvalExcept ex;
				ex << "Unknown exception caught:  " << __FILE__ << ":" << __LINE__ << "\n"
					<< "in TxVectorFunctor::getBroydJacobian"; 
				ex << "(const ArgVecType&, Matrix&)\n"
					<< "  --throwing TxBadEvalExcept now.\n\n";
				throw ex;
				}

				jac = jacobian;
				return;
			}

			int i;

		// Calculate the difference between new and old points
			for(i=0; i<Dimension; i++)
				del[i] = v[i] - point[i];
			double del2 = del*del;

		// If we have not moved, return old jacobian
			if(del2 == double(0.) )
			{
				jac = jacobian;
				return;
			}
 
		// If v is not the initial point, use Broyden formula
 
			for(i=0; i<m_ParametersCount; i++)
				y[i] = static_cast<double>(values[i] - prevValues[i]);
 
		// Broyden formula
			Vector temp = (y - jacobian*del)/(del2);
			jac = jacobian + Matrix(temp, del);

		// For exception handling
			try{
				if(jac != jac) throw;
			}
			catch  (TxBadEvalExcept& ex) {
				ex << "Exception caught:  " << __FILE__ << ":" << __LINE__ << "\n";
				ex << "in TxVectorFunctor::getBroydJacobian"
					<< "(const ArgVecType&, Matrix&)\n";
				ex << "Jacobian is a NAN.\n"; 
				ex << "  --rethrowing now.\n\n";
				throw;
			}
 
		// Reset the data
			jacobian = jac;
			point = v;
			prevValues = values; 
		}

		// values are known!
		template <class RetVecType, class ArgVecType> void TxVectorFunctor<RetVecType, ArgVecType>::resetJac(const ArgVecType& v) throw(TxBadEvalExcept)
		{
		   ArgVecType u = v;
		   point = v;
		   int i, j;
		   TXSTD::vector<RetVecType> temp(Dimension);

		// Calculate derivs
		   for (i=0; i<Dimension; i++)
			{
			 totDiff = absDiff + relDiff *
						  Math.Abs( u[i] );
			 u[i] += totDiff;
			 temp[i] = getFunctionValues(u);
			 for(j=0; j<m_ParametersCount; j++)
			 {
			   jacobian(j, i) = ( temp[i][j] - values[j] )/totDiff;
			 }
		// Return to previous value
			 u[i] -= totDiff;
		   }

		// For exception handling
		   try{
			  if(jacobian != jacobian) throw;
		   }
		   catch  (TxBadEvalExcept& ex) {
			   ex << "Exception caught:  " << __FILE__ << ":" << __LINE__ << "\n";
			   ex << "in TxVectorFunctor::resetJac(const ArgVecType&)\n";
			   ex << "Jacobian is a NAN.\n";
			   ex << "  --rethrowing now.\n\n";
			   throw;
		   }

		   if( (m_ParametersCount == Dimension) && (isInvJacobianNeeded) )
			{
			  invJacobian = jacobian;
			  invJacobian.invert();
		   }

		   isInitialized = true;
		   prevValues = values;
		}


		template <class RetVecType, class ArgVecType> typename TxUnaryContainerTraits<RetVecType>::ValueType TxVectorFunctor<RetVecType, ArgVecType>::evaluate(const ArgVecType& yarg, Matrix& jac) throw(TxBadEvalExcept)
		{
		  double res;

		  if(anyBoundsSet)
		  {
		//Do transformation for bounds
			ArgVecType x(Dimension);
			x = convertToOriginalArg(yarg);

		// Calculate function and jacobian in the new point
			try
			{
			  res = internFunction(x);
			  if (isBroyden) getBroydJacobian(x, jac);
			  else getRealJacobian(x, jac);    
			}
			catch (TxBadEvalExcept& ex) {
			  ex << "Exception caught:  " << __FILE__ << ":" << __LINE__ << "\n";
			  ex << "in evaluate(const ArgVecType& x, Matrix& jac)\n";
			  ex << "  --throwing TxBadEvalExcept now.\n\n";
			  throw ex;
			}

		//Finish transformation for the jacobian
			ArgVecType dydx = getOriginalArgDerivative(yarg);
			int i,j;
			for (i=0; i<m_ParametersCount; i++) 
			  for(j=0; j<Dimension; j++)
				jac(i,j) = jac(i,j)*dydx[j];
			}
			else {
			  try {
			  res = internFunction(yarg);
			  if (isBroyden) getBroydJacobian(yarg, jac);
			  else getRealJacobian(yarg, jac);
			  }
			  catch (TxBadEvalExcept& ex) {
				ex << "Exception caught:  " << __FILE__ << ":" << __LINE__ << "\n";
				ex << "in evaluate(const ArgVecType& x, Matrix& jac)\n";
				ex << "  --throwing TxBadEvalExcept now.\n\n";
				throw ex;
			  }
			}

		  return res;
		}
*/

		protected void	UpdateMin()
		{
			// Update the point
			if ( b < ro )
				return;

			for ( int i=0; i < m_Dimension; i++ )
			{
				m_PreviousMinPoint[i] = m_MinPoint[i];
				m_MinPoint[i] += m_Delta[i];
			}
			m_PreviousMinValue = m_MinValue;
		}

		protected void	UpdateScaling()
		{
			Vector	dv = new Vector( m_Dimension );
			for ( int i=0; i<m_Dimension; i++)
			{
				for ( int j=0; j < m_ParametersCount; j++ )
					dv[i] += m_Jacobian[j,i] * m_Jacobian[j,i];
			
				dv[i] = Math.Sqrt( dv[i] );
			}

			for ( int i=0; i<m_Dimension; i++ )
				m_Diagonal[i] = Math.Max( m_Diagonal[i], dv[i] );
		}

		protected void	UpdateTrustRegion()
		{
			if ( ro < 0.25 )
				m_Lambda *= 4;

			if ( ro > 0.75 )
				m_Lambda *= .5;
		}

		#endregion
	}
}
