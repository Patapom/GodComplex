#include "../GodComplex.h"

static U8*		gs_pMemoryBuffer = NULL;
static U8*		gs_pMemoryBufferAligned = NULL;
static size_t	gs_MemoryOffset = 0;

void	AllocateMemoryPool()
{
	gs_pMemoryBuffer = (U8*) GlobalAlloc( GMEM_ZEROINIT, MEMORY_POOL_SIZE );
	ASSERT( gs_pMemoryBuffer != NULL );

	gs_pMemoryBufferAligned = (U8*) (U32(gs_pMemoryBuffer+31) & (~31));
}

void	FreeMemoryPool()
{
	ASSERT( gs_MemoryOffset > 0 );	// If this assert fires up then it means you didn't even use the memory pool ! Don't bother allocating 64Mb for nothing then ! ^^
	GlobalFree( gs_pMemoryBuffer );
}

void*	Alloc( size_t _Size )
{
	ASSERT( gs_pMemoryBuffer != NULL );

	void*	pResult = gs_pMemoryBufferAligned;
	gs_pMemoryBufferAligned += gs_MemoryOffset;

	return pResult;
}
