#ifdef _DEBUG

//////////////////////////////////////////////////////////////////////////
// U32 specific version
//
template<typename T> int	Dictionary<T>::ms_MaxCollisionsCount = 0;

template<typename T> Dictionary<T>::Dictionary( int _Size )
{
	m_Size = _Size;
	m_ppTable = new Node*[m_Size];
	memset( m_ppTable, 0, m_Size*sizeof(Node*) );
}
template<typename T> Dictionary<T>::~Dictionary()
{
	for ( int i=0; i < m_Size; i++ )
	{
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL )
		{
			Node*	pOld = pNode;
			pNode = pNode->pNext;

			delete pOld;
		}
	}

	delete[] m_ppTable;
}

template<typename T> T*	Dictionary<T>::Get( U32 _Key ) const
{
	U32		idx = _Key % m_Size;
	Node*	pNode = m_ppTable[idx];

	int		CollisionsCount = 0;
	while ( pNode != NULL )
	{
		if ( _Key == pNode->Key )
		{
			ms_MaxCollisionsCount = MAX( ms_MaxCollisionsCount, CollisionsCount );
			return &pNode->Value;
		}
 
		pNode = pNode->pNext;
		CollisionsCount++;
	}
 
	return NULL;
}

template<typename T> T&	Dictionary<T>::Add( U32 _Key )
{
	U32		idx = _Key % m_Size;
 
	Node*	pNode = new Node();
	pNode->Key = _Key;
	pNode->pNext = m_ppTable[idx];	// Here, we could add a check for m_ppTable[idx] == NULL to ensure no collision...
	m_ppTable[idx] = pNode;

	return pNode->Value;
}

template<typename T> T&	Dictionary<T>::Add( U32 _Key, const T& _Value )
{
	T&	Value = Add( _Key );
	memcpy( &Value, &_Value, sizeof(T) );

	return Value;
}

template<typename T> void	Dictionary<T>::Remove( U32 _Key )
{
	U32		idx = _Key % m_Size;
 
	Node*	pPrevious = NULL;
	Node*	pCurrent = m_ppTable[idx];
	while ( pCurrent != NULL )
	{
		if ( _Key == pCurrent->Key )
		{
			if ( pPrevious != NULL )
				pPrevious->pNext = pCurrent->pNext;	// Link over...
			else
				m_ppTable[idx] = pCurrent->pNext;	// We replaced the root key...
 
			delete pCurrent;
 			return;
		}
 
		pPrevious = pCurrent;
		pCurrent = pCurrent->pNext;
	}
}

template<typename T> void	Dictionary<T>::ForEach( VisitorDelegate _pDelegate, void* _pUserData )
{
	for ( int i=0; i < m_Size; i++ )
	{
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL )
		{
			(*_pDelegate)( pNode->Value, _pUserData );
			pNode = pNode->pNext;
		}
	}
}

#endif