#pragma once

#include "Types.h"
#include "ASMHelpers.h"

// Simple list class
template<typename T> class	List
{
protected:	// NESTED TYPES

public:

protected:	// FIELDS

	T*		m_pList;	// List of allocated elements
	U32		m_Size;		// Size of the allocated list
	U32		m_Count;	// Amount of non empty elements

public:		// PROPERTIES

	int		GetCount() const			{ return m_Count; }
	int		GetAllocatedSize() const	{ return m_Size; }


public:		// METHODS

	List();
	List( U32 _InitialSize );
	~List();

	void		Init( U32 _Size );

	T&			operator[]( U32 _Index );
	const T&	operator[]( U32 _Index ) const;
	T&			Insert( U32 _Index );
	void		Append( const T& _Value );
	T&			Append();
	void		RemoveAt( U32 _Index );
	void		Clear()	{ m_Count = 0; }

private:
	void		Allocate( U32 _NewCount );
};

#include "List.inl"