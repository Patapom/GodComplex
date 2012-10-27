#pragma once

#ifdef _DEBUG
#include <assert.h>
#define ASSERT( condition, text ) assert( (condition) || !text )
#define ASSERT_RETURN_FALSE( condition, text ) assert( (condition) || !text ) return false
#else
#define ASSERT( condition, text )	(condition)
#define ASSERT_RETURN_FALSE( condition, text ) return false
#endif


//////////////////////////////////////////////////////////////////////////
// Simple types definition
typedef signed char		S8;
typedef unsigned char	U8;
typedef signed short	S16;
typedef unsigned short	U16;
typedef unsigned int	U32;
typedef signed int		S32;
typedef unsigned long	U64;
typedef signed long		S64;

typedef int NjErrorID;
typedef int NjResourceID;


//////////////////////////////////////////////////////////////////////////
//
#ifndef GODCOMPLEX
template<typename T> void		SafeDelete( T*& _pBuffer )
{
	if ( _pBuffer == NULL )
		return;
	delete _pBuffer;
	_pBuffer = NULL;
}

template<typename T> void		SafeDeleteArray( T*& _pBuffer )
{
	if ( _pBuffer == NULL )
		return;
	delete[] _pBuffer;
	_pBuffer = NULL;
}
#else
//For the GodComplex intro, don't use templates! That makes the exe fat!
static void		SafeDelete__( void*& _pBuffer )
{
	if ( _pBuffer == 0 )
		return;
	delete _pBuffer;
	_pBuffer = 0;
}

static void		SafeDeleteArray__( void*& _pBuffer )
{
	if ( _pBuffer == 0 )
		return;
	delete[] _pBuffer;
	_pBuffer = 0;
}
#define SafeDelete( a )			SafeDelete__( (void*&) (a) );
#define SafeDeleteArray( a )	SafeDeleteArray__( (void*&) (a) );
#endif

#include "../Math/Math.h"

