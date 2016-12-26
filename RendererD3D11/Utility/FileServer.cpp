#include "stdafx.h"

#include "FileServer.h"
#include <sys/stat.h>

DiskFileServer::DiskFileServer() {
}

HRESULT	DiskFileServer::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes ) {
	if ( _ppData == NULL ) {
		// Just check file can be opened...
		FILE*	pFile;
		fopen_s( &pFile, _pFileName, "rb" );
		if ( pFile != NULL ) {
			fclose( pFile );
		}
		return S_FALSE;
	}

	// Attempt to retrieve shader path of parent file from parent's data pointer
	const char**	ppShaderPath = _pParentData != NULL ? m_dataPointer2FilePath.Get( _pParentData ) : NULL;
//	ASSERT( ppShaderPath != NULL, "Failed to retrieve data pointer!" );

	// Split file path into directory + file name
	BString	shaderDirectory;
	BString	shaderFileName;
	GetFileDirectory( shaderDirectory, ppShaderPath != NULL ? *ppShaderPath : _pFileName );
	GetFileName( shaderFileName, _pFileName );

	// Recompose file name
	BString	fullName( true, "%s%s", shaderDirectory, shaderFileName );

	FILE*	pFile;
	fopen_s( &pFile, fullName, "rb" );
	ASSERT( pFile != NULL, "Include file not found!" );

	fseek( pFile, 0, SEEK_END );
	U32	Size = ftell( pFile );
	fseek( pFile, 0, SEEK_SET );

	char*	pBuffer = new char[Size];
	fread_s( pBuffer, Size, 1, Size, pFile );

	if ( _pBytes != NULL ) {
		*_pBytes = Size;
	}
	*_ppData = pBuffer;

	fclose( pFile );

	// Register this file's path as attached to the data pointer
	size_t	strLength = strlen(_pFileName)+1;
	char*	copiedFileName = new char[strLength];
	strcpy_s( copiedFileName, strLength, _pFileName );
	m_dataPointer2FilePath.Add( pBuffer, copiedFileName );

	return S_OK;
}

HRESULT	DiskFileServer::Close( THIS_ LPCVOID _pData ) {
	// Remove entry from dictionary
	const char**	ppShaderPath = m_dataPointer2FilePath.Get( _pData );
	ASSERT( ppShaderPath != NULL, "Failed to retrieve data pointer!" );
	delete[] *ppShaderPath;	// Delete the hardcopy of the string we made for registration

	m_dataPointer2FilePath.Remove( _pData );

	// Delete file content
	delete[] _pData;

	return S_OK;
}

time_t		DiskFileServer::GetFileModTime( const BString& _fileName ) const {
	struct _stat statInfo;
	_stat( _fileName, &statInfo );

	return statInfo.st_mtime;
}

void	DiskFileServer::GetFileDirectory( BString& _fileDirectory, const BString& _filePath ) {
	if ( _filePath.IsEmpty() ) {
		_fileDirectory = "\0";
		return;
	}

	_fileDirectory = _filePath;
//	strcpy_s( _fileDirectory, 1024, _filePath );

	// Cut at last slash or anti-slash
	const char*	pLastSlash = strrchr( _fileDirectory, '\\' );
	if ( pLastSlash == NULL )
		pLastSlash = strrchr( _fileDirectory, '/' );
	if ( pLastSlash != NULL ) {
		size_t	end = 1 + pLastSlash - _fileDirectory;
		_fileDirectory[U32(end)] = '\0';
	}
}
void	DiskFileServer::GetFileName( BString& _fileName, const BString& _filePath ) {
	if ( _filePath.IsEmpty() ) {
		_fileName = "\0";
		return;
	}
	const char*	pLastSlash = strrchr( _filePath, '\\' );
	if ( pLastSlash == NULL )
		pLastSlash = strrchr( _filePath, '/' );
	if ( pLastSlash == NULL )
		pLastSlash = _filePath - 1;

	const char*	fileNameStart = pLastSlash + 1;
	_fileName = fileNameStart;
}

