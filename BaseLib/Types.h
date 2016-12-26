#pragma once

#ifdef _DEBUG
	#include <assert.h>
	#define ASSERT( condition, text ) assert( (condition) || !text )
	#define ASSERT_RETURN_FALSE( condition, text ) assert( (condition) || !text ) return false
	#define RELEASE_ASSERT( condition, text ) assert( (condition) || !text )
#else
	#include <crtdefs.h>
	#define ASSERT( condition, text )	(condition)
	#define ASSERT_RETURN_FALSE( condition, text ) return false
	#define RELEASE_ASSERT( condition, text ) assert( (condition) || !text )
#endif

#ifndef NULL
	#define NULL    0
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


//////////////////////////////////////////////////////////////////////////
//
#define SAFE_DELETE( a )		if ( (a) != NULL ) { delete (a); (a) = NULL; }
#define SAFE_DELETE_ARRAY( a )	if ( (a) != NULL ) { delete[] (a); (a) = NULL; }
#define SAFE_RELEASE( a )		if ( (a) != NULL ) { (a)->Release(); delete (a); (a) = NULL; }

#ifndef GODCOMPLEX
template<typename T> void		SafeDelete__( T*& _pBuffer ) {
	if ( _pBuffer == NULL )
		return;
	delete _pBuffer;
	_pBuffer = NULL;
}

template<typename T> void		SafeDeleteArray__( T*& _pBuffer ) {
	if ( _pBuffer == NULL )
		return;
	delete[] _pBuffer;
	_pBuffer = NULL;
}

//#else
// //For the GodComplex intro, don't use templates! That makes the exe fat!
// static void		SafeDelete__( void*& _pBuffer )
// {
// 	if ( _pBuffer == 0 )
// 		return;
// 	delete _pBuffer;
// 	_pBuffer = 0;
// }
// 
// static void		SafeDeleteArray__( void*& _pBuffer )
// {
// 	if ( _pBuffer == 0 )
// 		return;
// 	delete[] _pBuffer;
// 	_pBuffer = 0;
// }
// #define delete a )			SafeDelete__( (void*&) (a) );
// #define delete[] a )	SafeDeleteArray__( (void*&) (a) );

#endif

template<typename T> void	Swap( T& a, T& b ) { T temp = a; a = b; b = temp; }

#include "BString.h"
#include "Containers/Hashtable.h"
#include "Containers/List.h"

#include "ASMHelpers.h"
#include "Math/Math.h"
#include "Math/Random.h"
#include "Math/SH.h"
#include "PixelFormats/PixelFormats.h"
#include "Utility/tweakval.h"
