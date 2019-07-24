// This is the main DLL file.

#include "stdafx.h"

#include "Matrix.h"

using namespace MathSolvers;

VectorF::VectorF( UInt32 _length ) {
	m_nativeVector = new MathSolversLib::VectorF( _length );
	m_nativeVector->Clear( 0.0f );
}

VectorF::VectorF( cli::array<float>^ _elements ) {
	pin_ptr<float>	elements = &_elements[0];
	m_nativeVector = new MathSolversLib::VectorF( _elements->Length, elements );
}

MatrixF::MatrixF( UInt32 _rowsCount, UInt32 _columnsCount ) {
	m_nativeMatrix = new MathSolversLib::MatrixF( _rowsCount, _columnsCount );
	m_nativeMatrix->Clear( 0.0f );
}

MatrixF::MatrixF( cli::array<float,2>^ _elements ) {
	UInt32	rows = _elements->GetLength( 0 );
	UInt32	columns = _elements->GetLength( 1 );
	m_nativeMatrix = new MathSolversLib::MatrixF( rows, columns );
	for ( UInt32 r=0; r < rows; r++ ) {
		MathSolversLib::VectorF&	row = m_nativeMatrix->m[r];
		for ( UInt32 c=0; c < columns; c++ ) {
			row.m[c] = _elements[r,c];
		}
	}
}
