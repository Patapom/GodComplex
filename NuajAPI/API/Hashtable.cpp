#include "Hashtable.h"
#include <stdlib.h>
#include <string.h>

#ifdef _DEBUG

//////////////////////////////////////////////////////////////////////////
// String version
DictionaryString::DictionaryString( int _Size )
{
	m_Size = _Size;
	m_ppTable = new Node*[m_Size];
	memset( m_ppTable, 0, m_Size*sizeof(Node*) );
}
DictionaryString::~DictionaryString()
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

void*	DictionaryString::Get( char* _pKey ) const
{
	U32		idx = Hash( _pKey ) % m_Size;
	Node*	pNode = m_ppTable[idx];
	while ( pNode != NULL )
	{
		if ( !strncmp( _pKey, pNode->pKey, HT_MAX_KEYLEN ) )
			return pNode->pValue;
 
		pNode = pNode->pNext;
	}
 
	return NULL;
}

void	DictionaryString::Add( char* _pKey, void* _pValue )
{
	U32		idx = Hash( _pKey ) % m_Size;
 
	Node*	pNode = new Node();
	pNode->pValue = _pValue;
	pNode->pKey = new char[strnlen( _pKey, HT_MAX_KEYLEN) + 1];
	strcpy( pNode->pKey, _pKey );
 
	pNode->pNext = m_ppTable[idx];
	m_ppTable[idx] = pNode;
}

void	DictionaryString::Remove( char* _pKey )
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
 
			return;
		}
 
		pPrevious = pCurrent;
		pCurrent = pCurrent->pNext;
	}
}

U32	DictionaryString::Hash( char* _pKey ) const
{
  /* djb2 */
  U32 hash = 5381;
  int c;
 
  while ( c = *_pKey++ )
    hash = ((hash << 5) + hash) + c;
 
  return hash;
}

U32	DictionaryString::Hash( U32 _Key ) const
{
	U32	hash = 5381;

	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + (_Key & 0xFF);	_Key >>= 8;
	hash = ((hash << 5) + hash) + _Key;

  return hash;
}
#endif


//////////////////////////////////////////////////////////////////////////
// U32 version
//
#ifdef _DEBUG
int	DictionaryU32::ms_MaxCollisionsCount = 0;
#endif

DictionaryU32::DictionaryU32( int _Size )
{
	m_Size = _Size;
	m_ppTable = new Node*[m_Size];
	memset( m_ppTable, 0, m_Size*sizeof(Node*) );
}
DictionaryU32::~DictionaryU32()
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

void*	DictionaryU32::Get( U32 _Key ) const
{
	U32		idx = _Key % m_Size;
	Node*	pNode = m_ppTable[idx];

#ifdef _DEBUG
	int		CollisionsCount = 0;
	while ( pNode != NULL )
	{
		if ( _Key == pNode->Key )
		{
			ms_MaxCollisionsCount = MAX( ms_MaxCollisionsCount, CollisionsCount );
			return pNode->pValue;
		}
 
		pNode = pNode->pNext;
		CollisionsCount++;
	}
#else
	while ( pNode != NULL )
	{
		if ( _Key == pNode->Key )
			return pNode->pValue;
 
		pNode = pNode->pNext;
	}
#endif
 
	return NULL;
}

void	DictionaryU32::Add( U32 _Key, void* _pValue )
{
	U32		idx = _Key;
 
	Node*	pNode = new Node();
	pNode->Key = _Key;
	pNode->pValue = _pValue;
 
	pNode->pNext = m_ppTable[idx];	// Here, we could add a check for m_ppTable[idx] == NULL to ensure no collision...
	m_ppTable[idx] = pNode;
}

void	DictionaryU32::Remove( U32 _Key )
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

void	DictionaryU32::ForEach( VisitorDelegate _pDelegate )
{
	for ( int i=0; i < m_Size; i++ )
	{
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL )
		{
			(*_pDelegate)( pNode->pValue );
			pNode = pNode->pNext;
		}
	}
}