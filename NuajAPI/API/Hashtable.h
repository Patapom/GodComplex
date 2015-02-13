#pragma once

#include "Types.h"
#include "ASMHelpers.h"
#include "../Math/Math.h"

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
#define HT_DEFAULT_SIZE	8192//(1 << 13)	// Default size if 8Kb
#define HT_MAX_KEYLEN	1024

#if defined(_DEBUG) || !defined(GODCOMPLEX)

// Hashtable of strings, only used to access constants & uniforms by name in the shaders in DEBUG mode
template<typename T> class	DictionaryString
{
protected:	// NESTED TYPES

	struct	Node
	{
		struct Node*	pNext;
		char*			pKey;
		T				Value;
	};

public:

	typedef void	(*VisitorDelegate)( int _EntryIndex, T& _Value, void* _pUserData );

protected:	// FIELDS

	Node**	m_ppTable;
	int		m_Size;
	int		m_EntriesCount;

public:		// METHODS

	DictionaryString( int _Size=HT_DEFAULT_SIZE );
	~DictionaryString();

	T*		Get( const char* _pKey ) const;					// retrieve entry
	T&		Add( const char* _pKey );						// store entry
	T&		AddUnique( const char* _pKey );					// store entry
	void	Add( const char* _pKey, const T& _Value );		// store entry
	void	AddUnique( const char* _pKey, const T& _Value );// store entry
	void	Remove( const char* _pKey );					// remove entry
	void	ForEach( VisitorDelegate _pDelegate, void* _pUserData );

public:

	static U32	Hash( const char* _pKey );
	static U32	Hash( U32 _Key );
};

#endif

// Specific dictionary storing explicit typed values
template<typename T> class	Dictionary
{
protected:	// NESTED TYPES

	struct	Node
	{
		struct Node*	pNext;
		U32				Key;
		T				Value;
	};

public:

	typedef void	(*VisitorDelegate)( int _EntryIndex, T& _Value, void* _pUserData );


protected:	// FIELDS

	Node**	m_ppTable;
	int		m_Size;
	int		m_EntriesCount;

#ifdef _DEBUG
public:
	static int	ms_MaxCollisionsCount;	// You can examine this to know if one of the dictionaries has too many collisions (i.e. size too small)
#endif

public:		// METHODS

	Dictionary( int _Size=HT_DEFAULT_SIZE );
	~Dictionary();

	int		GetEntriesCount() const		{return m_EntriesCount; }	// Amount of entries in the dictionary

	T*		Get( U32 _Key ) const;				// retrieve entry
	T&		Add( U32 _Key );					// store entry
	T&		Add( U32 _Key, const T& _Value );	// store entry
	void	Remove( U32 _Key );					// remove entry
	void	Clear();
	void	ForEach( VisitorDelegate _pDelegate, void* _pUserData );
};

// General dictionary storing blind values
class	DictionaryU32
{
protected:	// NESTED TYPES

	struct	Node
	{
		struct Node*	pNext;
		U32				Key;
		void*			pValue;
	};
 
public:

	typedef void	(*VisitorDelegate)( int _EntryIndex, void*& _pValue, void* _pUserData );

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
	void	ForEach( VisitorDelegate _pDelegate, void* _pUserData );
};


#include "Hashtable.inl"
