#include "../GodComplex.h"

MemoryMappedFile::MemoryMappedFile( int _Size, const char* _pFileName )
{
	m_hFile = CreateFileMapping( INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, _Size, _pFileName );
	ASSERT( m_hFile != INVALID_HANDLE_VALUE, "Failed to create File Mapping!" );

	m_pMappedFile = MapViewOfFile( m_hFile, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0 );

//#define NO_INITIAL_REFRESH
#ifdef NO_INITIAL_REFRESH
// Doing this with another application having the file open will result in no change on our side when calling CheckForChange()
// But we generally want a change the first time we read the file, since it's usually there we initialize new values for our variables...
	m_Checksum = *((U32*) m_pMappedFile);	// First DWORD is the checksum
#else
	m_Checksum = ~*((U32*) m_pMappedFile);	// So there will always be a change...
#endif
}

MemoryMappedFile::~MemoryMappedFile()
{
	UnmapViewOfFile( m_pMappedFile );
	CloseHandle( m_hFile );
}

bool		MemoryMappedFile::CheckForChange()
{
	U32	Checksum = *((U32*) m_pMappedFile);
	if ( Checksum == m_Checksum )
		return false;	// No change...

	m_Checksum = Checksum;
	return true;
}
