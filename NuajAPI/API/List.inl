
template<typename T> List<T>::List()
	: m_pList( NULL )
	, m_Size( 0 )
	, m_Count( 0 )
{

}

template<typename T> List<T>::List( U32 _InitialSize )
	: m_pList( NULL )
	, m_Size( 0 )
	, m_Count( 0 )
{
	Init( _InitialSize );
}

template<typename T> void	List<T>::Init( U32 _Size )
{
	delete[] m_pList;

	m_Size = _Size;
	m_pList = new T[m_Size];
	m_Count = 0;
}

template<typename T> List<T>::~List()
{
	delete[] m_pList;
}

template<typename T> T&			List<T>::operator[]( U32 _Index )
{
	ASSERT( _Index < m_Count, "Index out of range!" );
	return m_pList[_Index];
}

template<typename T> const T&	List<T>::operator[]( U32 _Index ) const
{
	ASSERT( _Index < m_Count, "Index out of range!" );
	return m_pList[_Index];
}

template<typename T> void		List<T>::Append( const T& _Value ) {
	Allocate( m_Count+1 );
	memcpy( &m_pList[m_Count-1], &_Value, sizeof(T) );
}

template<typename T> T&			List<T>::Append() {
	Allocate( m_Count+1 );
	return m_pList[m_Count-1];
}

template<typename T> T&			List<T>::Insert( U32 _Index ) {
	if ( _Index == m_Count )
		return Append( _Index );

	ASSERT( _Index < m_Count, "Index out of range!" );
	Allocate( m_Count+1 );

	memcpy( &m_pList[_Index+1], &m_pList[_Index], (m_Count-1-_Index)*sizeof(T) );
	return m_pList[_Index];
}

template<typename T> U32		List<T>::IndexOf( const T& _Value ) const {
	for ( U32 i=0; i < m_Count; i++ ) {
		if ( m_pList[i] == _Value ) {
			return i;
		}
	}
	return ~0UL;
}

template<typename T> void		List<T>::RemoveAt( U32 _Index ) {
	ASSERT( _Index < m_Count, "Index out of range!" );
	memcpy( &m_pList[_Index], &m_pList[_Index+1], (m_Count-_Index-1)*sizeof(T) );
}

template<typename T> bool		List<T>::Remove( const T& _Value ) {
	U32	Index = IndexOf( _Value );
	if ( Index == ~0UL )
		return false;

	RemoveAt( Index );
	return true;
}

template<typename T> void		List<T>::Allocate( U32 _NewCount ) {
	if ( _NewCount <= m_Size )
	{	// No need to re-allocate...
		m_Count = _NewCount;
		return;
	}

	T*	pOldList = m_pList;
	int	OldSize = m_Size;

	if ( m_Size != 0 )
		m_Size *= 2;
	else
		m_Size = 8;	// Arbitrary...

	m_pList = new T[m_Size];
	memset( m_pList, 0, m_Size*sizeof(T) );
	if ( pOldList != NULL )
		memcpy( m_pList, pOldList, OldSize*sizeof(T) );
	m_Count = _NewCount;

	delete[] pOldList;
}

// Poor man's version: bubble sort!
template<typename T> void	List<T>::Sort( const IComparer<T>& _Comparer ) {
	if ( m_Size == 0 )
		return;

	T	temp;
	for ( U32 i=0; i < m_Size-1; i++ ) {
		T&	a = m_pList[i];
		for ( U32 j=i; j < m_Size; j++ ) {
			T&	b = m_pList[j];
			if ( _Comparer.Compare( a, b ) < 0 ) {
				// Swap since a > b and should be placed afterward...
				memcpy_s( &temp, sizeof(T), &a, sizeof(T) );
				memcpy_s( &a, sizeof(T), &b, sizeof(T) );
				memcpy_s( &b, sizeof(T), &temp, sizeof(T) );
			}
		}
	}
}
