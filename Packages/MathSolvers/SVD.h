//////////////////////////////////////////////////////////////////////////
// Implementation of Singular Value Decomposition
// SVD is generally used to solve large systems of equations, possibly over-determined (i.e. more equations than unknowns)
// From Numerical Recipes, chapter 2.6 (NOTE: I rewrote the code so it uses 0-based vectors and matrices!)
// (NR stole it themselves from Golub: http://people.duke.edu/~hpgavin/SystemID/References/Golub+Reinsch-NM-1970.pdf)
//
// The Singular Value Decomposition decomposes a matrix A into a product of 3 matrices U, w and V such as:
//	A = U.W.V^T
//
//	|		|	|		|
//	|		|	|		|	
//	|		|	|		|	| w		|	|		|
//	|	A	| = |	U	| *	|   w	| *	|  V^T	|
//	|		|	|		|	|     w	|	|		|
//	|		|	|		|	
//	|		|	|		|
//
//	A is a m*n matrix where m >= n
//	U is a m*n orthonormal matrix
//	V is a n*n orthonormal matrix
//	W is a n*n diagonal matrix
//
//	• The columns of U are called the "left-singular vectors" and are the set of orthonormal eigen-vectors of A.A^T
//	• The columns of V are called the "right-singular vectors" and are the set of orthonormal eigen-vectors of A^T.A
//	• The non-zero diagonal entries of W are called the "singular values" and are the square roots of the non-zero eigen values of both A.A^T and A^T.A
//
// ===============================================================================================
// USAGES:
//
// • The singular value decomposition can be used for computing the pseudoinverse of a matrix.
//		Indeed, the pseudo-inverse of A = U.W.V^T is A^-1 = V.W^-1.U^T where W^-1 is created by replacing each non-zero diagonal elements by its reciprocal
//
// • Least-squares solution to an overdetermined set of linear equations A.x = b
//		The least-squares solution vector x is obtained by x = A^-1.b = V.W^-1.U^T.b
//		In general, the matrix W will not be singular and no diagonal element w will need to be set to 0
//
// • Solving homogeneous linear equations
//		A set of homogeneous linear equations can be written as A.x = 0 for a matrix A and vector x.
//		A typical situation is that A is known and a non-zero x is to be determined which satisfies the equation.
//		Such an x belongs to A's null space and is sometimes called a (right) null vector of A.
//		The vector x can be characterized as a right-singular vector corresponding to a singular value of A that is zero.
//		This observation means that if A is a square matrix and has no vanishing singular value, the equation has no non-zero x as a solution.
//		It also means that if there are several vanishing singular values, any linear combination of the corresponding right-singular vectors is a valid solution.
//		Analogously to the definition of a (right) null vector, a non-zero x satisfying x∗A = 0, with x∗ denoting the conjugate transpose of x, is called a left null vector of A.
//
// • Constructing an orthonormal basis
//		Suppose that you have N vectors in M-dimensional vector space, with N <= M then the N vectors span a subspace of the full vector space.
//		Often you want to construct an orthonormal set of N vectors that span the same subspace.
//		The textbook way to do this is by Gram-Schmidt orthogonalization, starting with one vector and then expanding the subspace one dimension at a time.
//		Numerically, however, because of the build-up of roundoff errors, naïve Gram-Schmidt orthogonalization is terrible.
//		Instead, form an M × N matrix A whose N columns are your vectors. Run the matrix through SVD: the columns of the matrix U  are your desired orthonormal basis vectors.
//
#pragma once

#include "Matrix.h"

namespace MathSolversLib {

	class SVD {
	public:		// FIELDS

		MatrixF		A;	// The source matrix to decompose
		MatrixF		U;	// Matrix of left-singular vectors
		VectorF		w;	// Singular-values of diagonal matrix W
		MatrixF		V;	// Matrix of right-singular vectors (WARNING! NOT the transpose V^T)

	public:		// METHODS

		SVD();
		SVD( U32 _rows, U32 _columns ) {
			Init( _rows, _columns );
		}
		SVD( const MatrixF& _A ) {
			Init( _A );
		}

		void	Init( U32 _rows, U32 _columns );
		void	Init( const MatrixF& _A );

		// Given a matrix a[1..m][1..n], this routine computes its singular value decomposition, A = U · W · V^T
		//	The matrix U replaces A on output.
		//	The diagonal matrix of singular values W is output as a vector w[1..n].
		//	The matrix V (not the transpose V^T) is output as v[1..n][1..n].
		//
		void	Decompose();

		// Solves A.x = b
		// NOTE: Decompose must have been called first so the U,W and V matrices are computed!
		void	Solve( const VectorF& b, VectorF& x );
	};

}	// namespace MathSolvers