//////////////////////////////////////////////////////////////////////////
// Memory Mapped Files support
//
#pragma once

//#include <

class MemoryMappedFile
{
public:

	class	IOnFileChanged
	{
		virtual void	OnMemoryMappedFileChanged( MemoryMappedFile& _File ) = 0;
	};

private:

	HANDLE			m_hFile;
	void*			m_pMappedFile;
	U32				m_Checksum;

public:

	MemoryMappedFile( int _Size, const char* _pFileName );
	~MemoryMappedFile();

	void*					GetMappedMemory()		{ return m_pMappedFile; }
	template<class T> T&	GetMappedMemory()		{ return *((T*) m_pMappedFile); }

	// Check for any external change in content...
	bool			CheckForChange();
};