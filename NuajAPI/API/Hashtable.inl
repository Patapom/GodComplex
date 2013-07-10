#ifdef _DEBUG

//////////////////////////////////////////////////////////////////////////
// String version
template<typename T> DictionaryString<T>::DictionaryString( int _Size ) : m_EntriesCount( 0 )
{
	m_Size = _Size;
	m_ppTable = new Node*[m_Size];
	memset( m_ppTable, 0, m_Size*sizeof(Node*) );
}
template<typename T> DictionaryString<T>::~DictionaryString()
{
	for ( int i=0; i < m_Size; i++ )
	{
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL )
		{
			Node*	pOld = pNode;
			pNode = pNode->pNext;
 
			delete pOld->pKey;
			pOld->pKey = NULL;

			delete pOld;
		}
	}

	delete[] m_ppTable;
}

template<typename T> T*	DictionaryString<T>::Get( const char* _pKey ) const
{
	if ( !m_EntriesCount )
		return NULL;

	U32		idx = Hash( _pKey ) % m_Size;
	Node*	pNode = m_ppTable[idx];
	while ( pNode != NULL )
	{
		if ( !strncmp( _pKey, pNode->pKey, HT_MAX_KEYLEN ) )
			return &pNode->Value;
 
		pNode = pNode->pNext;
	}
 
	return NULL;
}

template<typename T> T&	DictionaryString<T>::Add( const char* _pKey )
{
	U32		idx = Hash( _pKey ) % m_Size;
 
	Node*	pNode = new Node();

	int		KeyLength = strnlen( _pKey, HT_MAX_KEYLEN ) + 1;
	pNode->pKey = new char[KeyLength];
	strcpy_s( pNode->pKey, KeyLength, _pKey );
 
	pNode->pNext = m_ppTable[idx];
	m_ppTable[idx] = pNode;

	m_EntriesCount++;

	return pNode->Value;
}

template<typename T> T&	DictionaryString<T>::AddUnique( const char* _pKey )
{
	T*	pExisting = Get( _pKey );
	if ( pExisting != NULL )
		return *pExisting;

	return Add( _pKey );
}

template<typename T> void	DictionaryString<T>::Add( const char* _pKey, const T& _Value )
{
	T&	Value = Add( _pKey );
		Value = _Value;
}

template<typename T> void	DictionaryString<T>::AddUnique( const char* _pKey, const T& _Value )
{
	T&	Value = AddUnique( _pKey );
		Value = _Value;
}

template<typename T> void	DictionaryString<T>::Remove( const char* _pKey )
{
	U32		idx = Hash( _pKey ) % m_Size;
 
	Node*	pPrevious = NULL;
	Node*	pCurrent = m_ppTable[idx];
	while ( pCurrent != NULL )
	{
		if ( !strncmp( _pKey, pCurrent->pKey, HT_MAX_KEYLEN ) )
		{
			if ( pPrevious != NULL )
				pPrevious->pNext = pCurrent->pNext;	// Link over...
			else
				m_ppTable[idx] = pCurrent->pNext;	// We replaced the root key...
 
			delete pCurrent->pKey;
			delete pCurrent;
 
			m_EntriesCount--;

			return;
		}
 
		pPrevious = pCurrent;
		pCurrent = pCurrent->pNext;
	}
}

template<typename T> U32	DictionaryString<T>::Hash( const char* _pKey )
{
  /* djb2 */
  U32 hash = 5381;
  int c;
 
  while ( c = *_pKey++ )
    hash = ((hash << 5) + hash) + c;
 
  return hash;
}

template<typename T> U32	DictionaryString<T>::Hash( U32 _Key )
{
	U32	hash = 5381;

	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + _Key;

  return hash;
}

template<typename T> void	DictionaryString<T>::ForEach( VisitorDelegate _pDelegate, void* _pUserData )
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

//////////////////////////////////////////////////////////////////////////
// U32 specific version
//
#ifdef _DEBUG
template<typename T> int	Dictionary<T>::ms_MaxCollisionsCount = 0;
#endif

template<typename T> Dictionary<T>::Dictionary( int _Size ) : m_EntriesCount( 0 )
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
	if ( !m_EntriesCount )
		return NULL;

	U32		idx = _Key % m_Size;
	Node*	pNode = m_ppTable[idx];

#ifdef _DEBUG
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
#else
	while ( pNode != NULL )
	{
		if ( _Key == pNode->Key )
			return &pNode->Value;
 
		pNode = pNode->pNext;
	}
#endif

	return NULL;
}

template<typename T> T&	Dictionary<T>::Add( U32 _Key )
{
	U32		idx = _Key % m_Size;
 
	Node*	pNode = new Node();
	pNode->Key = _Key;
	pNode->pNext = m_ppTable[idx];	// Here, we could add a check for m_ppTable[idx] == NULL to ensure no collision...
	m_ppTable[idx] = pNode;

	m_EntriesCount++;

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

			m_EntriesCount--;

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
