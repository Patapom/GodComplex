#pragma once

#include "../Types.h"

namespace BaseLib {

// Comparer should return:
//	+1 if a < b 
//	-1 if a > b
//	 0 if a == b
template<typename T> class	IComparer {
public:	virtual int		Compare( const T& a, const T& b ) const = 0;
};

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

	int		Count() const				{ return m_Count; }
	void	SetCount( U32 _Count )		{ ASSERT( _Count <= m_Size, "Count exceeds allocated size!" ); m_Count = _Count; }
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
	void		AppendUnique( const T& _Value );
	T&			Append();
	T*			Ptr() { return m_pList; }
	const T*	Ptr() const { return m_pList; }
	U32			IndexOf( const T& _Value ) const;
	void		RemoveAt( U32 _Index );
	bool		Remove( const T& _Value );
	void		Clear()	{ m_Count = 0; }

	void		Sort( const IComparer<T>& _Comparer );

private:
	void		Allocate( U32 _NewCount );
};

#include "List.inl"

}	// namespace BaseLib