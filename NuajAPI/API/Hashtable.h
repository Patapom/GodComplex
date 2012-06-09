#pragma once

#include "Types.h"
#include "../Math/Math.h"
#include "ASMHelpers.h"

// Source: http://blog.2of1.org/2011/07/11/simple-c-hashtable-code/
//
// simple hashtable
// David Kaplan <david[at]2of1.org>, 2011
// 
// mem = (16 + 8 * size + 24 * entries) bytes [64-bit]
// mem = (8 + 4 * size + 12 * entries) bytes [32-bit]
// 
// some sizes for reference [64-bit]
// 2^8  = 16K [probably too small - depending on use]
// 2^16 = 512K [even bigger would be better]
// 2^24 = 128MB [probably a bit too much]
// 2^32 = 32GB [whooooa! watch that heap space!]
// 
// the hashtable uses basic linked-lists for handling collisions
// 
#define HT_DEFAULT_SIZE	8192//1 << 13	// Default size if 8Kb
#define HT_MAX_KEYLEN	50

typedef void	(*VisitorDelegate)( void* _pValue );

#ifdef _DEBUG
class	DictionaryString
{
protected:	// NESTED TYPES

	struct	Node
	{
		struct Node*	pNext;
		char*			pKey;
		void*			pValue;
	};
 
protected:	// FIELDS

	Node**	m_ppTable;
	int		m_Size;

public:		// METHODS

//	DictionaryString();
	DictionaryString( int _Size=HT_DEFAULT_SIZE );
	~DictionaryString();

	void*	Get( char* _pKey ) const;				// retrieve entry
	void	Add( char* _pKey, void* _pValue );	// store entry
	void	Remove( char* _pKey );			// remove entry

public:

	U32		Hash( char* _pKey ) const;
	U32		Hash( U32 _Key ) const;
};
#endif

class	DictionaryU32
{
protected:	// NESTED TYPES

	struct	Node
	{
		struct Node*	pNext;
		U32				Key;
		void*			pValue;
	};
 
protected:	// FIELDS

	Node**	m_ppTable;
	int		m_Size;

#ifdef _DEBUG
public:
	static int	ms_MaxCollisionsCount;	// You can examine this to know if one of the dictionaries has too many collisions (i.e. size too small)
#endif

public:		// METHODS

	DictionaryU32( int _Size=HT_DEFAULT_SIZE );
	~DictionaryU32();

	void*	Get( U32 _Key ) const;				// retrieve entry
	void	Add( U32 _Key, void* _pValue );	// store entry
	void	Remove( U32 _Key );			// remove entry
	void	ForEach( VisitorDelegate _pDelegate );
};
