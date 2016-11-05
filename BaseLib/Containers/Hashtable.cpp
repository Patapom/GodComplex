#include "Hashtable.h"
//#include <string.h>

using namespace BaseLib;

//////////////////////////////////////////////////////////////////////////
// U32 General version
//
#ifdef _DEBUG
int	DictionaryU32::ms_MaxCollisionsCount = 0;
#endif

DictionaryU32::DictionaryU32( int _Size ) {
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
			if ( CollisionsCount > ms_MaxCollisionsCount )
				ms_MaxCollisionsCount = CollisionsCount;
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
	U32		idx = _Key % m_Size;
 
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

void	DictionaryU32::ForEach( VisitorDelegate _pDelegate, void* _pUserData )
{
	int	EntryIndex = 0;
	for ( int i=0; i < m_Size; i++ )
	{
		Node*	pNode = m_ppTable[i];
		while ( pNode != NULL )
		{
			(*_pDelegate)( EntryIndex++, pNode->pValue, _pUserData );
			pNode = pNode->pNext;
		}
	}
}