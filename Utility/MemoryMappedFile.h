//////////////////////////////////////////////////////////////////////////
// Memory Mapped Files support
//
#pragma once

class MemoryMappedFile
{
private:

	HANDLE			m_hFile;
	void*			m_pMappedFile;
	U32				m_Checksum;

public:

	MemoryMappedFile( int _Size, const char* _pFileName );
	~MemoryMappedFile();

	void*					GetMappedMemory()		{ return m_pMappedFile; }
	template<typename T> T&	GetMappedMemory()		{ return *((T*) m_pMappedFile); }

	// Check for any external change in content...
	bool			CheckForChange();
};

template<typename T> class MMF : protected MemoryMappedFile
{
public:

	MMF( const char* _pFileName ) : MemoryMappedFile( sizeof(T), _pFileName ) {}

	T&		GetMappedMemory()	{ return MemoryMappedFile::GetMappedMemory<T>(); }
	bool	CheckForChange()	{ return MemoryMappedFile::CheckForChange(); }
};