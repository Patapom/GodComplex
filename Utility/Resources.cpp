#include "../GodComplex.h"

const U8*	LoadResourceBinary( U16 _ResourceID, const char* _pResourceType, U32* _pResourceSize )
{
	HRSRC	hResource = FindResourceA( NULL, MAKEINTRESOURCE( _ResourceID ), _pResourceType );
	ASSERT( hResource != NULL, "Failed to find resource ID !" );	// Couldn't find resource!

	// Optional: retrieve the size of the resource
	if ( _pResourceSize != NULL )
		*_pResourceSize = SizeofResource( NULL, hResource );

	// Load and lock the resource
	HGLOBAL	hBinary = LoadResource( NULL, hResource );
	LPVOID	pData = LockResource( hBinary );
	ASSERT( pData != NULL, "Failed to lock resource for reading !" );	// Couldn't lock resource!

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

//////////////////////////////////////////////////////////////////////////
// Add new include files to this static array below
namespace
{
	struct IncludePair
	{
		const char* pPath;			// The path to include/resolve
		U16			ResourceID;		// The equivalent resource ID encoding the shader file
	};

	static IncludePair	m_pIncludeFiles[] =
	{
		{ "Inc/TestInclude.fx", IDR_SHADER_INCLUDE_TEST },
	};
}


static class	IncludesManager : public ID3DInclude
{
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

		ASSERT( false, "Shader include file not found !" );
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
	ASSERT( pShaderCode != NULL, "Failed to load shader resource !" );

	Material*	pResult = new Material( gs_Device, _Format, pShaderCode, _pMacros, _pEntryPointVS, _pEntryPointGS, _pEntryPointPS, &gs_IncludesManager );
	ASSERT( pResult != NULL, "Failed to create material !" );

	delete pShaderCode;	// We musn't forget to delete this temporary buffer !

	return pResult;
}