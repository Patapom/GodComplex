#include "stdafx.h"

#include "FileServer.h"
#include <sys/stat.h>

DiskFileServer	DiskFileServer::singleton;

DiskFileServer::DiskFileServer() {
}

HRESULT	DiskFileServer::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes ) {

	// Test all registered paths to find the shader
	FILE*	pFile = FindShaderFile( _pFileName );
	if ( pFile == NULL )
		return S_FALSE;

	if ( _ppData == NULL ) {
		// Just check file can be opened...
		fclose( pFile );
		return S_OK;
	}

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

	return S_OK;
}

HRESULT	DiskFileServer::Close( THIS_ LPCVOID _pData ) {
	delete[] _pData;	// Delete file content
	return S_OK;
}

time_t		DiskFileServer::GetFileModTime( const BString& _fileName ) const {
	struct _stat statInfo;
	_stat( _fileName, &statInfo );

	return statInfo.st_mtime;
}

struct DelegateData {
	DiskFileServer*	server;
	const BString*	partialFileName;
	FILE*			result;
};
bool	VisitorDelegate( int _entryIndex, const BString& _key, BString& _value, void* _pUserData ) {
	DelegateData&	data = *reinterpret_cast< DelegateData* >( _pUserData );
	data.result = data.server->FindShaderFile( _key, *data.partialFileName );
	return data.result == NULL;	// Continue while not found...
}

FILE*	DiskFileServer::FindShaderFile( const BString& _partialFileName ) {
	DelegateData	data;
	data.server = this;
	data.partialFileName = &_partialFileName;
	data.result = FindShaderFile( "", _partialFileName );
	if ( data.result == NULL )
		m_collectedDirectories.ForEach( VisitorDelegate, &data );	// Search other directories if necessary...

	return data.result;
}

FILE*	DiskFileServer::FindShaderFile( const BString& _directoryName, const BString& _partialFileName ) {
	FILE*	file = NULL;

	// Combine paths
	BString	combinedPath;
	combinedPath.Combine( _directoryName, _partialFileName );
	fopen_s( &file, combinedPath, "rb" );
	if ( file == NULL )
		return NULL;	// Not found in that directory

	// Retrieve directory for registration
	BString	shaderDirectory;
	combinedPath.GetFileDirectory( shaderDirectory );
	shaderDirectory.ToLower();

	const BString*	existingDirectory = m_collectedDirectories.Get( shaderDirectory );
	if ( existingDirectory == NULL ) {
		// Collect new directory
		m_collectedDirectories.Add( shaderDirectory, shaderDirectory );
	}

	return file;
}
