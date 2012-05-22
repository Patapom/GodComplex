#include "../GodComplex.h"

const U8*	LoadResourceBinary( U16 _ResourceID, const char* _pResourceType, U32* _pResourceSize )
{
	HRSRC	hResource = FindResourceA( NULL, MAKEINTRESOURCE( _ResourceID ), _pResourceType );
	ASSERT( hResource != NULL );	// Couldn't find resource!

	// Optional: retrieve the size of the resource
	if ( _pResourceSize != NULL )
		*_pResourceSize = SizeofResource( NULL, hResource );

	// Load and lock the resource
	HGLOBAL	hBinary = LoadResource( NULL, hResource );
	LPVOID	pData = LockResource( hBinary );
	ASSERT( pData != NULL );	// Couldn't lock resource!

	return (U8*) pData;
}

char*	LoadResourceShader( U16 _ResourceID )
{
	U32	dwSize;
	const U8*	pData = LoadResourceBinary( _ResourceID, "SHADER", &dwSize );

	// Copy it and append the missing NULL character terminator
	char*	pShaderSource = new char[dwSize+1];

	ASM_memcpy( pShaderSource, pData, dwSize );
	pShaderSource[dwSize] = '\0';	// Add the NULL terminator

	return pShaderSource;
}
