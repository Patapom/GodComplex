
//////////////////////////////////////////////////////////////////////////
template< typename T >
void	Vector<T>::Init( U32 _length, T* _ptr ) {
	if ( length == _length && _ptr == nullptr )
		return;

	Exit();

	length = _length;
	ownedPtr = _ptr == nullptr;
	m = ownedPtr ? new T[length] : _ptr;
}
template< typename T >
void	Vector<T>::Exit() {
	if ( ownedPtr ) {
		delete[] m;
		ownedPtr = false;
	}
	m = nullptr;
	length = 0;
}
template< typename T >
void	Vector<T>::Clear( T v ) {
	if ( v == 0.0 ) {
		memset( m, 0, length*sizeof(T) );
		return;
	}
	T*	ptr = m;
	for ( U32 i=length; i > 0; i-- )
		*ptr++ = v;
}
template< typename T >
void	Vector<T>::CopyTo( Vector& _target ) const {
	_target.Init( length );
	U32		size = length*sizeof(T);
	memcpy_s( _target.m, size, m, size );
}
template< typename T >
void	Vector<T>::Swap( Vector& _other ) {
	::Swap( length, _other.length );
	::Swap( ownedPtr, _other.ownedPtr );
	::Swap( m, _other.m );
}


//////////////////////////////////////////////////////////////////////////
template< typename T >
void	Matrix<T>::Init( U32 _rows, U32 _columns ) {
	if ( rows == _rows && columns == _columns )
		return;

	Exit();

	rows = _rows;
	columns = _columns;
	m_raw = new T[rows * columns];
	m = new Vector<T>[rows];
	for ( U32 rowIndex=0; rowIndex < rows; rowIndex++ ) {
		m[rowIndex].Init( _columns, &m_raw[columns * rowIndex] );
	}
}
template< typename T >
void	Matrix<T>::Exit() {
	SAFE_DELETE_ARRAY( m_raw );
	SAFE_DELETE_ARRAY( m );
	rows = columns = 0;
}
template< typename T >
void	Matrix<T>::Clear( T v ) {
	if ( v == 0.0 ) {
		memset( m_raw, 0, rows*columns*sizeof(T) );
		return;
	}
	T*	ptr = m_raw;
	for ( U32 i=rows*columns; i > 0; i-- )
		*ptr++ = v;
}
template< typename T >
void	Matrix<T>::CopyTo( Matrix& _target ) const {
	_target.Init( rows, columns );
	U32		size = rows*columns*sizeof(T);
	memcpy_s( _target.m_raw, size, m_raw, size );
}
