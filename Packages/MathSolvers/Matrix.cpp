#include "stdafx.h"
#include "Matrix.h"

using namespace MathSolvers;

//////////////////////////////////////////////////////////////////////////
void	Vector::Init( U32 _length, double* _ptr ) {
	if ( length == _length && _ptr == nullptr )
		return;

	Exit();

	length = _length;
	ownedPtr = _ptr == nullptr;
	m = ownedPtr ? new double[length] : _ptr;
}
void	Vector::Exit() {
	if ( ownedPtr ) {
		delete[] m;
		ownedPtr = false;
	}
	m = nullptr;
	length = 0;
}
void	Vector::Clear( double v ) {
	if ( v == 0.0 ) {
		memset( m, 0, length*sizeof(double) );
		return;
	}
	double*	ptr = m;
	for ( U32 i=length; i > 0; i-- )
		*ptr++ = v;
}
void	Vector::CopyTo( Vector& _target ) const {
	_target.Init( length );
	U32		size = length*sizeof(double);
	memcpy_s( _target.m, size, m, size );
}
void	Vector::Swap( Vector& _other ) {
	::Swap( length, _other.length );
	::Swap( ownedPtr, _other.ownedPtr );
	::Swap( m, _other.m );
}


//////////////////////////////////////////////////////////////////////////
void	Matrix::Init( U32 _rows, U32 _columns ) {
	if ( rows == _rows && columns == _columns )
		return;

	Exit();

	rows = _rows;
	columns = _columns;
	m_raw = new double[rows * columns];
	m = new Vector[rows];
	for ( U32 rowIndex=0; rowIndex < rows; rowIndex++ ) {
		m[rowIndex].Init( _columns, &m_raw[columns * rowIndex] );
	}
}
void	Matrix::Exit() {
	SAFE_DELETE_ARRAY( m_raw );
	SAFE_DELETE_ARRAY( m );
	rows = columns = 0;
}
void	Matrix::Clear( double v ) {
	if ( v == 0.0 ) {
		memset( m_raw, 0, rows*columns*sizeof(double) );
		return;
	}
	double*	ptr = m_raw;
	for ( U32 i=rows*columns; i > 0; i-- )
		*ptr++ = v;
}
void	Matrix::CopyTo( Matrix& _target ) const {
	_target.Init( rows, columns );
	U32		size = rows*columns*sizeof(double);
	memcpy_s( _target.m_raw, size, m_raw, size );
}
