
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

template<typename T> void		List<T>::Append( const T& _Value )
{
	Allocate( m_Count+1 );
	memcpy( &m_pList[m_Count-1], &_Value, sizeof(T) );
}

template<typename T> T&			List<T>::Append()
{
	Allocate( m_Count+1 );
	return m_pList[m_Count-1];
}

template<typename T> T&			List<T>::Insert( U32 _Index )
{
	if ( _Index == m_Count )
		return Append( _Index );

	ASSERT( _Index < m_Count, "Index out of range!" );
	Allocate( m_Count+1 );

	memcpy( &m_pList[_Index+1], &m_pList[_Index], (m_Count-1-_Index)*sizeof(T) );
	return m_pList[_Index];
}

template<typename T> void		List<T>::RemoveAt( U32 _Index )
{
	ASSERT( _Index < m_Count, "Index out of range!" );
	memcpy( &m_pList[_Index], &m_pList[_Index+1], (m_Count-_Index-1)*sizeof(T) );
}

template<typename T> void		List<T>::Allocate( U32 _NewCount )
{
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
