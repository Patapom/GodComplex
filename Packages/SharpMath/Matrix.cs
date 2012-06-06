using System;

namespace WMath
{
	/// <summary>
	/// Double-precision NxN matrix class
	/// </summary>
    public class Matrix
	{
		#region FIELDS

		public int			N = 0;
		public double[,]	m = null;

		/// <summary>
		/// Value access
		/// </summary>
		/// <param name="i">Row index in [0,N[</param>
		/// <param name="j">Column index in [0,N[</param>
		/// <returns></returns>
		public double		this[int i, int j]
		{
			get { return m[i,j]; }
			set { m[i,j] = value; }
		}

		#endregion

		#region METHODS

		public						Matrix( int _Dimensions )
		{
			N = _Dimensions;
			m = new double[N,N];
		}

		public						Matrix( Matrix _Other )
		{
			N = _Other.N;
			m = new double[N,N];
			Array.Copy( _Other.m, m, N*N );
		}

		public						Matrix( double[,] _Array )
		{
			if ( _Array.GetLength( 0 ) != _Array.GetLength( 1 ) )
				throw new Exception( "Invalid array ! Must be square !" );

			N = _Array.GetLength( 0 );
			m = new double[N,N];
			Array.Copy( _Array, m, N*N );
		}

		public						Matrix( float[,] _Array )
		{
			if ( _Array.GetLength( 0 ) != _Array.GetLength( 1 ) )
				throw new Exception( "Invalid array ! Must be square !" );

			N = _Array.GetLength( 0 );
			m = new double[N,N];
			Array.Copy( _Array, m, N*N );
		}

		// Multiplication
		static public Matrix		operator*( Matrix _A, Matrix _B )
		{
			if ( _A.N != _B.N )
				throw new Exception( "A and B dimensions mismatch !" );

			int N = _A.N;
			Matrix	R = new Matrix( N );
			for ( int i=0; i < N; i++ )		// Row
				for ( int j=0; j < N; j++ )	// Column
					for ( int k=0; k < N; k++ )
						R[i,j] += _A[i,k] * _B[k,j];

			return R;
		}

		static public double[]		operator*( double[] _V, Matrix _A )
		{
			if ( _V.Length != _A.N )
				throw new Exception( "A and V dimensions mismatch !" );

			int N = _A.N;
			double[]	R = new double[_V.Length];
			for ( int j=0; j < N; j++ )		// Column
				for ( int i=0; i < N; i++ )	// Row
					R[j] += _V[i] * _A.m[i,j];

			return R;
		}

		static public double[]		operator*( Matrix _A, double[] _V )
		{
			if ( _V.Length != _A.N )
				throw new Exception( "A and V dimensions mismatch !" );

			int N = _A.N;
			double[]	R = new double[_V.Length];
			for ( int i=0; i < N; i++ )		// Row
				for ( int j=0; j < N; j++ )	// Column
					R[i] += _V[j] * _A.m[i,j];

			return R;
		}

		// Addition
		static public Matrix		operator+( Matrix _A, Matrix _B )
		{
			if ( _A.N != _B.N )
				throw new Exception( "A and B dimensions mismatch !" );

			int N = _A.N;
			Matrix	R = new Matrix( N );
			for ( int i=0; i < N; i++ )		// Row
				for ( int j=0; j < N; j++ )	// Column
					R[i,j] = _A[i,j] + _B[i,j];

			return R;
		}

		// Subtraction
		static public Matrix		operator-( Matrix _A, Matrix _B )
		{
			if ( _A.N != _B.N )
				throw new Exception( "A and B dimensions mismatch !" );

			int N = _A.N;
			Matrix	R = new Matrix( N );
			for ( int i=0; i < N; i++ )		// Row
				for ( int j=0; j < N; j++ )	// Column
					R[i,j] = _A[i,j] - _B[i,j];

			return R;
		}

		/// <summary>
		/// Solve A.x = y with A and y being known
		/// This methods uses LU decomposition and back substitution
		/// </summary>
		/// <param name="y"></param>
		/// <returns>An array of floats for the result x</returns>
		/// <exception cref="Exception">Throws if the matrix is singular and connot be decomposed</exception>
		public double[]	Solve( double[] y )
		{
			if ( y.Length != N )
				throw new Exception( "Matrix and vector sizes mismatch !" );

			// LU decompose the matrix
			int[]	PivotedIndices = null;
			double	fParity = 1.0f;
			Matrix	LU = LUDecomposition( out PivotedIndices, out fParity );

			// Back substitute
			return LUBackwardSubstitution( LU, y, PivotedIndices );
		}

		/// <summary>
		/// Inverts the current matrix using LU decomposition
		/// </summary>
		/// <returns>The inverse matrix</returns>
		/// <exception cref="Exception">Throws if the matrix is not inversible</exception>
		public Matrix	Invert()
		{
			Matrix	R = new Matrix( N );

			// LU decompose the matrix
			int[]	PivotedIndices = null;
			double	fParity = 1.0f;
			Matrix	LU = LUDecomposition( out PivotedIndices, out fParity );

			// Solve for each column using backward substitution
			double[]	Column = new double[N];
			for ( int j=0; j < N; j++ )
			{
				Array.Clear( Column, 0, N );
				Column[j] = 1.0f;
				double[]	InverseColumn = LUBackwardSubstitution( LU, Column, PivotedIndices );

				for ( int i=0; i < N; i++ )
					R.m[i,j] = InverseColumn[i];
			}

			return R;
		}

		/// <summary>
		/// Computes the determinant of the matrix
		/// </summary>
		/// <returns></returns>
		public double	Determinant()
		{
			// LU decompose the matrix
			int[]	PivotedIndices = new int[N];
			double	d = 1.0f;
			Matrix	LU = LUDecomposition( out PivotedIndices, out d );

			// Determinant is simply the product of the diagonal elements
			// As repeated products can make us lose precision, we simply add the logs
			double	fLogDet = 0.0;
			for ( int i=0; i < N; i++ )
			{
				double	V = LU[i,i];
				fLogDet += Math.Log( Math.Abs( V ) );
				d *= Math.Sign( V );
			}

			return d * Math.Exp( fLogDet );
		}

		/// <summary>
		/// Performs LU decomposition of that matrix
		/// Borrowed from the Numerical Recipes (chapter 2 pp. 46)
		/// </summary>
		/// <param name="_PivotedIndices">The array of pivoted indices. You should access your vector of coefficients to solve with an indirection through that array (cf. the Solve() method)</param>
		/// <param name="_Parity">The parity sign (-1 if indices were pivoted an odd number of times, +1 otherwise) (cf. Solve() to see how to deal with that)</param>
		/// <returns>The LU decomposition of that matrix in the form of a single matrix composed of the 2 L and U matrices as shown below:
		/// | B11 B12 B13 B14 |
		/// | A21 B22 B23 B24 |
		/// | A31 A32 B33 B34 |  with Aii = 1
		/// | A41 A42 A43 B44 |
		/// 
		/// So, finally :
		/// 
		///     |  1   0   0   0  |
		///     | A21  1   0   0  |
		/// L = | A31 A32  1   0  |
		///     | A41 A42 A43  1  |
		///     
		///     | B11 B12 B13 B14 |
		///     |  0  B22 B23 B24 |
		/// U = |  0   0  B33 B34 |
		///     |  0   0   0  B44 |
		/// </returns>
		/// <exception cref="Exception">Throws if the matrix is singular and connot be decomposed</exception>
		public Matrix	LUDecomposition( out int[] _PivotedIndices, out double _Parity )
		{
			Matrix	R = new Matrix( this );
			_PivotedIndices = new int[N];

			/*
			void ludcmp(float **a, int n, int *indx, float *d)
			Given a matrix a[1..n][1..n], this routine replaces it by the LU decomposition of a rowwise
			permutation of itself. a and n are input. a is output, arranged as in equation (2.3.14) above;
			indx[1..n] is an output vector that records the row permutation effected by the partial
			pivoting; d is output as 1 depending on whether the number of row interchanges was even
			or odd, respectively. This routine is used in combination with lubksb to solve linear equations
			or invert a matrix.
			*/

			int		i,imax,j,k;
			double	big,dum,sum,temp;
			double[]	vv = new double[N];	// vv stores the implicit scaling of each row.
			for ( i=0;i<N;i++ )
				vv[i] = 1.0f;

			_Parity = 1.0f;	// No row interchanges yet.
			for ( i=0; i < N; i++ )
			{	// Loop over rows to get the implicit scaling information.
				big = 0.0f;
				for ( j=0; j<N; j++ )
					if ( (temp=Math.Abs(R.m[i,j])) > big )
						big = temp;
			
				if ( big == 0.0 )
					throw new Exception( "Singular matrix !" );

				// No nonzero largest element.
				vv[i] = 1.0f / big;	// Save the scaling.
			}

			for ( j=0; j < N; j++ )
			{	// This is the loop over columns of Crout's method.
				for ( i=0; i < j; i++ )
				{	// This is equation (2.3.12) except for i = j.
					sum = R.m[i,j];
					for ( k=0; k < i; k++ )
						sum -= R.m[i,k] * R.m[k,j];
					R.m[i,j] = sum;
				}
			
				big = 0.0f;	// Initialize for the search for largest pivot element.
				imax = j;
				for ( i=j; i < N; i++ )
				{	// This is i = j of equation (2.3.12) and i = j+1 : ::N
					sum = R.m[i,j];	// of equation (2.3.13).
					for ( k=0; k < j; k++ )
						sum -= R.m[i,k]*R.m[k,j];
					R.m[i,j] = sum;

					if ( (dum=vv[i]*Math.Abs(sum)) >= big )
					{	// Is thefigure of merit for the pivot better than the best so far?
						big = dum;
						imax = i;
					}
				}

				if ( j != imax )
				{	// Do we need to interchange rows?
					for ( k=0; k < N; k++ )
					{	// Yes, do so...
						dum = R.m[imax,k];
						R.m[imax,k] = R.m[j,k];
						R.m[j,k] = dum;
					}
					_Parity = -_Parity;		// ...and change the parity of d.
					vv[imax] = vv[j];		// Also interchange the scale factor.
				}

				_PivotedIndices[j] = imax;
				if ( R.m[j,j] == 0.0 )
					R.m[j,j] = 1e-10f;
				
				// If the pivot element is zero the matrix is singular (at least to the precision of the algorithm).
				// For some applications on singular matrices, it is desirable to substitute 1e-10f for zero.
				if ( j != N-1 )
				{	// Now, finally, divide by the pivot element.
					dum = 1.0f / R.m[j,j];
					for ( i=j+1; i < N; i++ )
						R.m[i,j] *= dum;
				}
			}	// Go back for the next column in the reduction.

// 			// CHECK
// 			Matrix	L = new Matrix( R );
// 			Matrix	U = new Matrix( R );
// 			for ( i=0; i < N; i++ )
// 				for ( j=0; j < N; j++ )
// 					if ( i < j )
// 						L[i,j] = 0.0f;
// 					else if ( i > j )
// 						U[i,j] = 0.0f;
// 					else
// 						L[i,j] = 1.0f;
// 
// 			Matrix	L_times_U = L * U;
// 
// 			// Swap rows
// 			for ( i=0; i < N; i++ )
// 			{
// 				if ( _PivotedIndices[i] != i )
// 				{
// 					for ( j=0; j < N; j++ )
// 					{
// 						dum = L_times_U[_PivotedIndices[i],j];
// 						L_times_U[_PivotedIndices[i],j] = L_times_U[i,j];
// 						L_times_U[i,j] = dum;
// 					}
// 
// 					for ( k=i+1; k < N; k++ )
// 						if  ( _PivotedIndices[k] == _PivotedIndices[i] )
// 							_PivotedIndices[k] = i;
// 						else if ( _PivotedIndices[k] == i )
// 							_PivotedIndices[k] = _PivotedIndices[j];
// 				}
// 			}
// 			// Now, "L_times_U" should be equal to THIS !
// 			Matrix	Zero = L_times_U - this;	// And that should be 0
// 			// CHECK

			return R;
		}

		/// <summary>
		/// Performs Forward/Backward substitution of a vector of unknowns with a LU decomposed matrix
		/// to find the solution of a system of linear equations A.x = b
		/// </summary>
		/// <param name="_LUDecomposition"></param>
		/// <param name="_B"></param>
		/// <param name="_PivotedIndices"></param>
		/// <returns>The array of solutions "x"</returns>
		public double[]	LUBackwardSubstitution( Matrix _LUDecomposition, double[] _B, int[] _PivotedIndices )
		{
			double[]	R = new double[N];
			Array.Copy( _B, R, N );
/*
void lubksb(float **a, int n, int *indx, float b[])
Solves the set of n linear equations AX = B. Here a[1..n][1..n] is input, not as the matrix
A but rather as its LU decomposition, determined by the routine ludcmp. indx[1..n] is input
as the permutation vector returned by ludcmp. b[1..n] is input as the right-hand side vector
B, and returns with the solution vector X. a, n, and indx are not modified by this routine
and can be left in place for successive calls with different right-hand sides b. This routine takes
into account the possibility that b will begin with many zero elements, so it is efficient for use
in matrix inversion.
*/
			int i, ii=-1, ip, j;
			double sum;

			// We need to solve Ax = (LU)x = L(Ux) = b (2.3.3)
			// We first solve Ly = b (2.3.4) by forward substitution
			// Then we solve Ux = y (2.3.5) by backward substitution

			// Forward substitution with lower matrix L
			for ( i=0; i < N; i++ )
			{	// When ii is set to a positive value, it will become the index of the first nonvanishing element of b.
				// We now do the forward substitution, equation (2.3.6). The only new wrinkle is to unscramble the permutation as we go.
				ip = _PivotedIndices[i];
				sum = R[ip];
				R[ip] = R[i];

				if ( ii >= 0 )
					for ( j=ii; j <= i-1; j++ )
						sum -= _LUDecomposition.m[i,j] * R[j];
				else if ( sum != 0.0f )
					ii = i;	// A nonzero element was encountered, so from now on we will have to do the sums in the loop above.

				R[i] = sum;
			}

			// Backward substitution with upper matrix U
			for ( i=N-1; i >= 0; i-- )
			{	// Now we do the backsubstitution, equation (2.3.7).
				sum = R[i];
				for ( j=i+1; j < N; j++ )
					sum -= _LUDecomposition.m[i,j] * R[j];

				R[i] = sum / _LUDecomposition.m[i,i];	// Store a component of the solution vector X.
			}	// All done!

			return R;
		}

		#endregion
	}
}
