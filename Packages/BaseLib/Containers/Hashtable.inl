#if defined(_DEBUG) || !defined(GODCOMPLEX)

//////////////////////////////////////////////////////////////////////////
// String version
template<typename T> DictionaryString<T>::DictionaryString( int _PowerOfTwoSize ) : m_EntriesCount( 0 )
{
	m_POT = _PowerOfTwoSize;
	m_SizePOT = 1 << m_POT;
	m_ppTable = new Node*[m_SizePOT];
	memset( m_ppTable, 0, m_SizePOT*sizeof(Node*) );
}
template<typename T> DictionaryString<T>::~DictionaryString() {
	for ( int i=0; i < m_SizePOT; i++ ) {
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL ) {
			Node*	pOld = pNode;
			pNode = pNode->pNext;
			delete pOld;
		}
	}

	delete[] m_ppTable;
}

template<typename T> T*	DictionaryString<T>::Get( const BString& _key ) const {
	if ( !m_EntriesCount )
		return NULL;

//	U32		idx = _key.Hash() & (m_SizePOT-1);
	U32		idx = Fibonacci( _key.Hash() );

	Node*	pNode = m_ppTable[idx];
	while ( pNode != NULL ) {
		if ( !BString::Compare( _key, pNode->key, HT_MAX_KEYLEN ) )
			return &pNode->value;
 
		pNode = pNode->pNext;
	}
 
	return NULL;
}

template<typename T> T&	DictionaryString<T>::Add( const BString& _key ) {
//	U32		idx = _key.Hash() & (m_SizePOT-1);
	U32		idx = Fibonacci( _key.Hash() );

 	Node*	pNode = new Node();

	int		keyLength = int( MIN( _key.Length(), HT_MAX_KEYLEN ) + 1 );
	pNode->key = _key;
	pNode->pNext = m_ppTable[idx];
	m_ppTable[idx] = pNode;

	m_EntriesCount++;

	return pNode->value;
}

template<typename T> T&	DictionaryString<T>::AddUnique( const BString& _key ) {
	T*	pExisting = Get( _key );
	if ( pExisting != NULL )
		return *pExisting;

	return Add( _key );
}

template<typename T> void	DictionaryString<T>::Add( const BString& _key, const T& _Value ) {
	T&	Value = Add( _key );
		Value = _Value;
}

template<typename T> void	DictionaryString<T>::AddUnique( const BString& _key, const T& _Value ) {
	T&	Value = AddUnique( _key );
		Value = _Value;
}

template<typename T> void	DictionaryString<T>::Remove( const BString& _key ) {
//	U32		idx = _key.Hash() & (m_SizePOT-1);
	U32		idx = Fibonacci( _key.Hash() );
 
	Node*	pPrevious = NULL;
	Node*	pCurrent = m_ppTable[idx];
	while ( pCurrent != NULL ) {
		if ( !BString::Compare( _key, pCurrent->key, HT_MAX_KEYLEN ) ) {
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

// template<typename T> U32	DictionaryString<T>::Hash( const String& _key )
// {
//   /* djb2 */
//   U32 hash = 5381;
//   int c;
//  
//   while ( c = *_pKey++ )
//     hash = ((hash << 5) + hash) + c;
//  
//   return hash;
// }

template<typename T> U32	DictionaryString<T>::Hash( U32 _Key )
{
	U32	hash = 5381;

	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + _Key;

  return hash;
}

template<typename T> void	DictionaryString<T>::ForEach( VisitorDelegate _pDelegate, void* _pUserData ) {
	int	EntryIndex = 0;
	for ( int i=0; i < m_SizePOT; i++ ) {
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL ) {
			if ( !(*_pDelegate)( EntryIndex++, pNode->key, pNode->value, _pUserData ) )
				return;	// Stop!
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

template<typename T> Dictionary<T>::Dictionary( int _PowerOfTwoSize ) : m_EntriesCount( 0 ) {
	m_POT = _PowerOfTwoSize;
	m_SizePOT = 1 << m_POT;
	m_ppTable = new Node*[m_SizePOT];
	memset( m_ppTable, 0, m_SizePOT*sizeof(Node*) );
}
template<typename T> Dictionary<T>::~Dictionary() {
	for ( int i=0; i < m_SizePOT; i++ ) {
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL ) {
			Node*	pOld = pNode;
			pNode = pNode->pNext;

			delete pOld;
		}
	}

	delete[] m_ppTable;
}

template<typename T> T*	Dictionary<T>::Get( U32 _Key ) const {
	if ( !m_EntriesCount )
		return NULL;

//	U32		idx = _Key & (m_SizePOT-1);
	U32		idx = Fibonacci( _Key );
	Node*	pNode = m_ppTable[idx];

#ifdef _DEBUG
	int		CollisionsCount = 0;
	while ( pNode != NULL ) {
		if ( _Key == pNode->key ) {
			ms_MaxCollisionsCount = MAX( ms_MaxCollisionsCount, CollisionsCount );
			return &pNode->value;
		}
 
		pNode = pNode->pNext;
		CollisionsCount++;
	}
#else
	while ( pNode != NULL )
	{
		if ( _Key == pNode->key )
			return &pNode->value;
 
		pNode = pNode->pNext;
	}
#endif

	return NULL;
}

template<typename T> T&	Dictionary<T>::Add( U32 _Key ) {
//	U32		idx = _Key & (m_SizePOT-1);
	U32		idx = Fibonacci( _Key );
 
	Node*	pNode = new Node();
	pNode->key = _Key;
	pNode->pNext = m_ppTable[idx];	// Here, we could add a check for m_ppTable[idx] == NULL to ensure no collision...
	m_ppTable[idx] = pNode;

	m_EntriesCount++;

	return pNode->value;
}

template<typename T> T&	Dictionary<T>::Add( U32 _Key, const T& _Value ) {
	T&	Value = Add( _Key );
	memcpy( &Value, &_Value, sizeof(T) );

	return Value;
}

template<typename T> void	Dictionary<T>::Remove( U32 _Key )
{
//	U32		idx = _Key & (m_SizePOT-1);
	U32		idx = Fibonacci( _Key );
 
	Node*	pPrevious = NULL;
	Node*	pCurrent = m_ppTable[idx];
	while ( pCurrent != NULL ) {
		if ( _Key == pCurrent->key ) {
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

template<typename T> void	Dictionary<T>::Clear() {
	// Clear all linked lists of nodes from each head
	for ( int HeadIndex=0; HeadIndex < m_SizePOT; HeadIndex++ ) {
		Node*	pNode = m_ppTable[HeadIndex];
		while ( pNode != NULL ) {
			Node*	pOld = pNode;
			pNode = pNode->pNext;
			delete pOld;
		}
	}
	// Clear heads
	memset( m_ppTable, 0, m_SizePOT*sizeof(Node*) );
}

template<typename T> void	Dictionary<T>::ForEach( VisitorDelegate _pDelegate, void* _pUserData ) {
	int	EntryIndex = 0;
	for ( int i=0; i < m_SizePOT; i++ ) {
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL ) {
			(*_pDelegate)( EntryIndex++, pNode->value, _pUserData );
			pNode = pNode->pNext;
		}
	}
}

//////////////////////////////////////////////////////////////////////////
// Generic version
//
#ifdef _DEBUG
template<typename K, typename T>
int	DictionaryGeneric<K,T>::ms_MaxCollisionsCount = 0;
#endif

template<typename K, typename T>
DictionaryGeneric<K,T>::DictionaryGeneric( int _PowerOfTwoSize ) : m_EntriesCount( 0 )
{
	m_POT = _PowerOfTwoSize;
	m_SizePOT = 1 << m_POT;
	m_ppTable = new Node*[m_SizePOT];
	memset( m_ppTable, 0, m_SizePOT*sizeof(Node*) );
}
template<typename K, typename T>
DictionaryGeneric<K,T>::~DictionaryGeneric()
{
	for ( int i=0; i < m_SizePOT; i++ )
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

template<typename K, typename T>
T*	DictionaryGeneric<K,T>::Get( const K& _Key ) const {
	if ( !m_EntriesCount )
		return NULL;

//	U32		idx = GetHash( _Key ) & (m_SizePOT-1);
	U32		idx = Fibonacci( GetHash( _Key ) );
	Node*	pNode = m_ppTable[idx];

	#ifdef _DEBUG
		int		CollisionsCount = 0;
		while ( pNode != NULL ) {
			if ( Compare( _Key, pNode->key ) == 0 ) {
				ms_MaxCollisionsCount = MAX( ms_MaxCollisionsCount, CollisionsCount );
				return &pNode->value;
			}
 
			pNode = pNode->pNext;
			CollisionsCount++;
		}
	#else
		while ( pNode != NULL ) {
			if ( Compare( _Key, pNode->key ) == 0 )
				return &pNode->value;
 
			pNode = pNode->pNext;
		}
	#endif

	return NULL;
}

template<typename K, typename T>
T&	DictionaryGeneric<K,T>::Add( const K& _key ) {
//	U32		idx = GetHash( _key ) & (m_SizePOT-1);
	U32		idx = Fibonacci( GetHash( _key ) );
 
	Node*	pNode = new Node();
	pNode->key = _key;
	pNode->pNext = m_ppTable[idx];	// Here, we could add a check for m_ppTable[idx] == NULL to ensure no collision...
	m_ppTable[idx] = pNode;

	m_EntriesCount++;

	return pNode->value;
}

template<typename K, typename T>
T&	DictionaryGeneric<K,T>::Add( const K& _key, const T& _value ) {
	T&	value = Add( _key );
	value = _value;

	return value;
}

template<typename K, typename T>
void	DictionaryGeneric<K,T>::Remove( const K& _key ) {
//	U32		idx = GetHash( _key ) & (m_SizePOT-1);
	U32		idx = Fibonacci( GetHash( _key ) );
 
	Node*	pPrevious = NULL;
	Node*	pCurrent = m_ppTable[idx];
	while ( pCurrent != NULL ) {
		if ( Compare( _key, pCurrent->key ) == 0 ) {
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

template<typename K, typename T>
void	DictionaryGeneric<K,T>::Clear() {
	// Clear all linked lists of nodes from each head
	for ( int HeadIndex=0; HeadIndex < m_SizePOT; HeadIndex++ ) {
		Node*	pNode = m_ppTable[HeadIndex];
		while ( pNode != NULL ) {
			Node*	pOld = pNode;
			pNode = pNode->pNext;
			delete pOld;
		}
	}
	// Clear heads
	memset( m_ppTable, 0, m_SizePOT*sizeof(Node*) );
}

template<typename K, typename T>
void	DictionaryGeneric<K,T>::ForEach( VisitorDelegate _pDelegate, void* _pUserData ) {
	int	EntryIndex = 0;
	for ( int i=0; i < m_SizePOT; i++ ) {
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL ) {
			if ( !(*_pDelegate)( EntryIndex++, pNode->key, pNode->value, _pUserData ) )
				return;	// Stop!
			pNode = pNode->pNext;
		}
	}
}
