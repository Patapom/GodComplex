#include "../GodComplex.h"

MemoryMappedFile::MemoryMappedFile( int _Size, const char* _pFileName )
{
	m_hFile = CreateFileMapping( INVALID_HANDLE_VALUE, NULL, PAGE_READONLY, 0, _Size, _pFileName );
	ASSERT( m_hFile != INVALID_HANDLE_VALUE, "Failed to create File Mapping!" );

	m_pMappedFile = MapViewOfFile( m_hFile, FILE_MAP_READ, 0, 0, 0 );
	m_Checksum = *((U32*) m_pMappedFile);	// First DWORD is the checksum
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
