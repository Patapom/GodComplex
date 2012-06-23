#pragma once

#ifdef _DEBUG
#include <assert.h>
#define ASSERT( condition, text ) assert( condition || !text )
#define ASSERT_RETURN_FALSE( condition, text ) assert( condition || !text ) return false
#else
#define ASSERT( condition, text )
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


#include "../Math/Math.h"