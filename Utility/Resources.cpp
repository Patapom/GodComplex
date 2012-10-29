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

	memcpy( pShaderSource, pData, _CodeSize-1 );
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

	static IncludePair	m_pIncludeFiles[] =	{ REGISTERED_INCLUDE_FILES };
}
//
//////////////////////////////////////////////////////////////////////////

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
				*ppData = LoadResourceBinary( pPair->ResourceID, "SHADER", pBytes );	// We read the file WITHOUT the trailing '\0' character !
				return S_OK;
			}

		ASSERT( false, "Shader include file not found !" );
		return S_FALSE;
	}

	STDMETHOD(Close)(THIS_ LPCVOID pData)
	{
//		delete pData;	// No need to release the resource !
		return S_OK;
	}

} gs_IncludesManager;

struct	ShaderResource
{
	int			ResourceID;
	const char*	pShaderFileName;
};

Material*	CreateMaterial( U16 _ShaderResourceID, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros )
{
	return CreateMaterial( _ShaderResourceID, _Format, _pEntryPointVS, NULL, NULL, _pEntryPointGS, _pEntryPointPS, _pMacros );
}

Material*	CreateMaterial( U16 _ShaderResourceID, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros )
{
	const char*	pFileName = NULL;

#ifdef _DEBUG
	// To support automatic reloading of shader changes, you need to register the shader file name here
	ShaderResource	pShaderResources[] =
	{
		REGISTERED_SHADER_FILES
	};

	int	ShadersCount = sizeof(pShaderResources) / sizeof(ShaderResource);
	for ( int ShaderIndex=0; ShaderIndex < ShadersCount; ShaderIndex++ )
		if ( _ShaderResourceID == pShaderResources[ShaderIndex].ResourceID )
		{
			pFileName = pShaderResources[ShaderIndex].pShaderFileName;
			break;
		}
#endif

	U32		CodeSize = 0;
	char*	pShaderCode = LoadResourceShader( _ShaderResourceID, CodeSize );
	ASSERT( pShaderCode != NULL, "Failed to load shader resource !" );

	Material*	pResult = new Material( gs_Device, _Format, pFileName, pShaderCode, _pMacros, _pEntryPointVS, _pEntryPointHS, _pEntryPointDS, _pEntryPointGS, _pEntryPointPS, &gs_IncludesManager );

	delete pShaderCode;	// We musn't forget to delete this temporary buffer !

	return pResult;
}

ComputeShader*	CreateComputeShader( U16 _ShaderResourceID, const char* _pEntryPoint, D3D_SHADER_MACRO* _pMacros )
{
	const char*	pFileName = NULL;

#ifdef _DEBUG
	// To support automatic reloading of shader changes, you need to register the shader file name here
	ShaderResource	pShaderResources[] =
	{
		REGISTERED_SHADER_FILES
	};

	int	ShadersCount = sizeof(pShaderResources) / sizeof(ShaderResource);
	for ( int ShaderIndex=0; ShaderIndex < ShadersCount; ShaderIndex++ )
		if ( _ShaderResourceID == pShaderResources[ShaderIndex].ResourceID )
		{
			pFileName = pShaderResources[ShaderIndex].pShaderFileName;
			break;
		}
#endif

	U32		CodeSize = 0;
	char*	pShaderCode = LoadResourceShader( _ShaderResourceID, CodeSize );
	ASSERT( pShaderCode != NULL, "Failed to load shader resource !" );

	ComputeShader*	pResult = new ComputeShader( gs_Device, pFileName, pShaderCode, _pMacros, _pEntryPoint, &gs_IncludesManager );

	delete pShaderCode;	// We musn't forget to delete this temporary buffer !

	return pResult;
}

