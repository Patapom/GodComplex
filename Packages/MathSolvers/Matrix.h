// MathSolversManaged.h

#pragma once

using namespace System;

namespace MathSolvers {

	public ref class VectorF {
	internal:
		bool						m_externalPointer;
		MathSolversLib::VectorF*	m_nativeVector;

		VectorF( MathSolversLib::VectorF& _nativeVector ) : m_nativeVector( &_nativeVector ), m_externalPointer( true ) {}

	public:
		VectorF() : m_nativeVector( NULL ) {}
		VectorF( UInt32 _length );
		VectorF( cli::array<float>^ _elements );
		~VectorF() { if ( !m_externalPointer ) delete m_nativeVector; }

		property UInt32	Length			{ UInt32 get() { return m_nativeVector->length; } }
		property float	default[UInt32] {
			float		get( UInt32 _index ) { return (*m_nativeVector)[_index]; }
			void		set( UInt32 _index, float _value ) { (*m_nativeVector)[_index] = _value; }
		}
		property cli::array<float>^	AsArray {
			cli::array<float>^	get() {
				cli::array<float>^	result = gcnew cli::array<float>( m_nativeVector->length );
				pin_ptr<float>	elements = &result[0];
				memcpy( (float*) elements, m_nativeVector->m, m_nativeVector->length*sizeof(float) );
				return result;
			}
		}
	};

	public ref class MatrixF {
	internal:
		bool						m_externalPointer;
		MathSolversLib::MatrixF*	m_nativeMatrix;

		MatrixF( MathSolversLib::MatrixF& _nativeMatrix ) : m_nativeMatrix( &_nativeMatrix ), m_externalPointer( true ) {}

	public:
		MatrixF() : m_nativeMatrix( NULL ) {}
		MatrixF( UInt32 _rowsCount, UInt32 _columnsCount );
		MatrixF( cli::array<float,2>^ _elements );
		~MatrixF() { if ( !m_externalPointer ) delete m_nativeMatrix; }

		property UInt32	RowsCount		{ UInt32 get() { return m_nativeMatrix->rows; } }
		property UInt32	ColumnsCount	{ UInt32 get() { return m_nativeMatrix->columns; } }
		property float	default[UInt32,UInt32] {
			float		get( UInt32 _rowIndex, UInt32 _columnIndex ) { return (*m_nativeMatrix)[_rowIndex][_columnIndex]; }
			void		set( UInt32 _rowIndex, UInt32 _columnIndex, float _value ) { (*m_nativeMatrix)[_rowIndex][_columnIndex] = _value; }
		}
		property cli::array<float,2>^	AsArray {
			cli::array<float,2>^	get() {
				int	rows = m_nativeMatrix->rows;
				int	columns = m_nativeMatrix->columns;
				cli::array<float,2>^	result = gcnew cli::array<float,2>( rows, columns );
				float*	p = m_nativeMatrix->m_raw;
				for ( int r=0; r < rows; r++ )
					for ( int c=0; c < columns; c++ )
						result[r,c] = *p++;
				return result;
			}
		}
	};
}
