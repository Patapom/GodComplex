// This is the main DLL file.

#include "stdafx.h"

#include "SVD.h"

using namespace MathSolvers;

SVD::SVD( MatrixF^ _matrix ) {
	m_nativeSVD = new MathSolversLib::SVD( _matrix->RowsCount, _matrix->ColumnsCount );
	m_nativeSVD->Init( *_matrix->m_nativeMatrix );

	U = gcnew MatrixF( m_nativeSVD->U );
	w = gcnew VectorF( m_nativeSVD->w );
	V = gcnew MatrixF( m_nativeSVD->V );
}

void	SVD::Decompose() {
	m_nativeSVD->Decompose();
}

void	SVD::Solve( VectorF^ b, VectorF^ x ) {
	m_nativeSVD->Solve( *b->m_nativeVector, *x->m_nativeVector );
}
