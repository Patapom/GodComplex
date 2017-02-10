#pragma once

#include <assert.h>
#ifdef _DEBUG
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
typedef signed char			S8;
typedef unsigned char		U8;
typedef signed short		S16;
typedef unsigned short		U16;
typedef unsigned int		U32;
typedef signed int			S32;
typedef unsigned long long	U64;
typedef signed long long	S64;

struct uint2 {
	U32	x, y;
};

struct uint3 {
	U32	x, y, z;
};

struct uint4 {
	U32	x, y, z, w;
};

struct sint2 {
	S32	x, y;
};

struct sint3 {
	S32	x, y, z;
};

struct sint4 {
	S32	x, y, z, w;
};

//////////////////////////////////////////////////////////////////////////
//
#define SAFE_DELETE( a )		if ( (a) != NULL ) { delete (a); (a) = NULL; }
#define SAFE_DELETE_ARRAY( a )	if ( (a) != NULL ) { delete[] (a); (a) = NULL; }
#define SAFE_RELEASE( a )		if ( (a) != NULL ) { (a)->Release(); (a) = NULL; }

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
