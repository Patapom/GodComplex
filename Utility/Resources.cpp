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

char*	LoadResourceShader( U16 _ResourceID, U32& _CodeSize )
{
	const U8*	pData = LoadResourceBinary( _ResourceID, "SHADER", &_CodeSize );
	_CodeSize++;

	// Copy it and append the missing NULL character terminator
	char*	pShaderSource = new char[_CodeSize];

	ASM_memcpy( pShaderSource, pData, _CodeSize-1 );
	pShaderSource[_CodeSize-1] = '\0';	// Add the NULL terminator

	return pShaderSource;
}

static class	IncludesManager : public ID3DInclude
{
private:

	struct IncludePair
	{
		const char* pPath;			// The path to include/resolve
		U16			ResourceID;		// The equivalent resource ID encoding the shader file
	};

	IncludePair	m_pIncludeFiles[1];
// 	=
// 		{
// 			{ "", IDR_SHADER_POST_FINAL },
// 		};

public:

	STDMETHOD(Open)(THIS_ D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes)
	{
		// Search for include file
		int				FilesCount = sizeof(m_pIncludeFiles) / sizeof(IncludePair);
		IncludePair*	pPair = m_pIncludeFiles;
		for ( int FileIndex=0; FileIndex < FilesCount; FileIndex++, pPair++ )
			if ( !strcmp( pFileName, pPair->pPath ) )
			{	// Found it !
				*ppData = LoadResourceShader( pPair->ResourceID, *pBytes );
				return S_OK;
			}

		ASSERT( false );	// No such file !
		return S_FALSE;
	}

	STDMETHOD(Close)(THIS_ LPCVOID pData)
	{
		delete pData;
		return S_OK;
	}

} gs_IncludesManager;

Material*	CreateMaterial( U16 _ShaderResourceID, const VertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros )
{
	U32		CodeSize = 0;
	char*	pShaderCode = LoadResourceShader( _ShaderResourceID, CodeSize );
	ASSERT( pShaderCode != NULL );

	Material*	pResult = new Material( gs_Device, _Format, pShaderCode, _pMacros, _pEntryPointVS, _pEntryPointGS, _pEntryPointPS, &gs_IncludesManager );
	ASSERT( pResult != NULL );

	delete pShaderCode;

	return pResult;
}