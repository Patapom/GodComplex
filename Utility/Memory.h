//////////////////////////////////////////////////////////////////////////
// Memory operators
//
#pragma once

#define MEMORY_POOL_SIZE	(64*1024*1024)	// 64Mb !

void	AllocateMemoryPool();	// Allocates a big chunck of memory that will be used as a memory pool
void	FreeMemoryPool();		// Frees a big chunk of memory
void*	Alloc( size_t _Size );	// Allocates a small buffer (AllocateMemoryPool must have been called first !)

inline void* __cdecl	operator new( size_t _Size )	{ return GlobalAlloc( GMEM_ZEROINIT, _Size ); }
inline void  __cdecl	operator delete( void* p )		{ GlobalFree( p ); }
