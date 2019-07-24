// MathSolversManaged.h

#pragma once

using namespace System;

#include "Matrix.h"

namespace MathSolvers {

	public ref class SVD {
		MathSolversLib::SVD*	m_nativeSVD;

	public:

		MatrixF^		U;	// Matrix of left-singular vectors
		VectorF^		w;	// Singular-values of diagonal matrix W
		MatrixF^		V;	// Matrix of right-singular vectors (WARNING! NOT the transpose V^T)

	public:
		SVD( MatrixF^ _matrix );
		~SVD() { delete m_nativeSVD; }

		void	Decompose();
		void	Solve( VectorF^ b, VectorF^ x );
	};
}
