//////////////////////////////////////////////////////////////////////////
// Implementation of Singular Value Decomposition
// SVD is generally used to solve large systems of equations, possibly over-determined (i.e. more equations than unknowns)
// From Numerical Recipes, chapter 2.6 (NOTE: I rewrote the code so it uses 0-based vectors and matrices!)
// (NR stole it themselves from Golub: http://people.duke.edu/~hpgavin/SystemID/References/Golub+Reinsch-NM-1970.pdf)
//
// The Singular Value Decomposition decomposes a matrix A into a product of 3 matrices U, w and V such as:
//	A = U.w.V^T
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
//	w is a n*n diagonal matrix
//
//	• The columns of U are called the "left-singular vectors" and are the set of orthonormal eigen-vectors of A.A^T
//	• The columns of V are called the "right-singular vectors" and are the set of orthonormal eigen-vectors of A^T.A
//	• The non-zero diagonal entries of W are called the "singular values" and are the square roots of the non-zero eigen values of both A.A^T and A^T.A
//
// ===============================================================================================
// USAGES:
//
// • The singular value decomposition can be used for computing the pseudoinverse of a matrix.
//		Indeed, the pseudo-inverse of A = U.w.V^T is A^-1 = V.w^-1.U^T where w^-1 is created by replacing each non-zero diagonal elements by its reciprocal
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
//
#pragma once

#include "Matrix.h"

namespace MathSolvers {

	class SVD {
	public:

	private:	// FIELDS


	public:		// PROPERTIES

	public:		// METHODS

		void	Decompose( int m, int n, float** a, float w[], float** v );

	};

}	// namespace MathSolvers