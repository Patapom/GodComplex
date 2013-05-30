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
		const char*		pPath;			// The path to include/resolve as written in the HLSL file
		const char*		pFullPath;		// The true path on disk from current working directory
		U16				ResourceID;		// The equivalent resource ID encoding the shader file
	};

	static IncludePair	m_pIncludeFiles[] =	{ REGISTERED_INCLUDE_FILES };
}
//
//////////////////////////////////////////////////////////////////////////

static class	IncludesManager : public ID3DInclude
{
#ifdef _DEBUG
public:	

	static const int	MAX_DEPENDENCIES = 64;	// A maximum of 64 shaders per include file

	struct Dependencies 
	{
		mutable time_t	LastModificationTime;							// Last time the include file was modified
		int				Count;											// Amount of dependencies
		const char**	ppDependencies;									// List of dependencies
	};
	Dependencies*						m_pDependencies;				// The list of dependencies for each include file
	DictionaryString<Material*>			m_pShaderName2Material;			// A map from shader file to material
	DictionaryString<ComputeShader*>	m_pShaderName2ComputeShader;	// A map from shader file to material

	const char*							m_pCurrentShaderFileName;		// The name of the shader currently being compiled
#endif

public:

#ifdef _DEBUG
	IncludesManager() : m_pDependencies(NULL) {}
#else
	IncludesManager() {}
#endif
	~IncludesManager();

	STDMETHOD(Open)(THIS_ D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes)
	{
		// Search for include file
		int				FilesCount = sizeof(m_pIncludeFiles) / sizeof(IncludePair);
		IncludePair*	pPair = m_pIncludeFiles;
		for ( int FileIndex=0; FileIndex < FilesCount; FileIndex++, pPair++ )
			if ( !strcmp( pFileName, pPair->pPath ) )
			{	// Found it !
				*ppData = LoadResourceBinary( pPair->ResourceID, "SHADER", pBytes );	// We read the file WITHOUT the trailing '\0' character !

#ifdef SURE_DEBUG
				if ( m_pCurrentShaderFileName != NULL )
				{	// Add a dependency on that include
					Dependencies&	D = m_pDependencies[FileIndex];
					D.LastModificationTime = GetFileModTime( pPair->pFullPath );

					bool	bAlreadyThere = false;
					for ( int i=0; i < D.Count; i++ )
						if ( !strcmp( D.ppDependencies[i], m_pCurrentShaderFileName ) )
						{	// We already have this file listed as dependency...
							// This occurs with nested includes if a includes b and include c which also includes b, b will be included twice and a will be added twice...
							bAlreadyThere = true;
							break;
						}

					if ( !bAlreadyThere )
						D.ppDependencies[D.Count++] = m_pCurrentShaderFileName;
				}
#endif

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

	void	SetCurrentlyCompilingShader( const char* _pShaderFileName );
	void	RegisterMaterial( const char* _pShaderFileName, Material& _Material );
	void	RegisterComputeShader( const char* _pShaderFileName, ComputeShader& _ComputeShader );

#ifdef SURE_DEBUG
	// Call this to rebuild dependent shaders if the include file has changed
	void	WatchIncludeModifications() const;

private:
	time_t	GetFileModTime( const char* _pFileName ) const;
#endif

} gs_IncludesManager;

struct	ShaderResource
{
	int			ResourceID;
	const char*	pShaderFileName;
};

Material*		CreateMaterial( U16 _ShaderResourceID, const char* _pFileName, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros )
{
	return CreateMaterial( _ShaderResourceID, _pFileName, _Format, _pEntryPointVS, NULL, NULL, _pEntryPointGS, _pEntryPointPS, _pMacros );
}

Material*		CreateMaterial( U16 _ShaderResourceID, const char* _pFileName, const IVertexFormatDescriptor& _Format, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, D3D_SHADER_MACRO* _pMacros )
{
	const char*	pFileName = _pFileName;

	U32		CodeSize = 0;
	char*	pShaderCode = LoadResourceShader( _ShaderResourceID, CodeSize );
	ASSERT( pShaderCode != NULL, "Failed to load shader resource !" );

	gs_IncludesManager.SetCurrentlyCompilingShader( pFileName );
	Material*	pResult = new Material( gs_Device, _Format, pFileName, pShaderCode, _pMacros, _pEntryPointVS, _pEntryPointHS, _pEntryPointDS, _pEntryPointGS, _pEntryPointPS, &gs_IncludesManager );
	gs_IncludesManager.RegisterMaterial( pFileName, *pResult );

	delete pShaderCode;	// We musn't forget to delete this temporary buffer !

	return pResult;
}

ComputeShader*	CreateComputeShader( U16 _ShaderResourceID, const char* _pFileName, const char* _pEntryPoint, D3D_SHADER_MACRO* _pMacros )
{
	const char*	pFileName = _pFileName;

	U32		CodeSize = 0;
	char*	pShaderCode = LoadResourceShader( _ShaderResourceID, CodeSize );
	ASSERT( pShaderCode != NULL, "Failed to load shader resource !" );

	gs_IncludesManager.SetCurrentlyCompilingShader( pFileName );
	ComputeShader*	pResult = new ComputeShader( gs_Device, pFileName, pShaderCode, _pMacros, _pEntryPoint, &gs_IncludesManager );
	gs_IncludesManager.RegisterComputeShader( pFileName, *pResult );

	delete pShaderCode;	// We musn't forget to delete this temporary buffer !

	return pResult;
}

#ifdef SURE_DEBUG
void	WatchIncludesModifications()
{
	static int	LastTime = -1;
	int			CurrentTime = timeGetTime();
	if ( LastTime >= 0 && (CurrentTime - LastTime) < 500 )
		return;	// Too soon to check !

	// Update last check time
	LastTime = CurrentTime;

	gs_IncludesManager.WatchIncludeModifications();
}
#endif

// Totally experimental
const char*		LoadCSO( const char* _pCSOPath )
{
	FILE*	pFile = fopen( _pCSOPath, "rb" );
	ASSERT( pFile != NULL, "Invalid file!" );

	fseek( pFile, 0, SEEK_END );
	U32	FileSize = ftell( pFile );
	fseek( pFile, 0, SEEK_SET );

	U8*		pRAW = new U8[5+FileSize];

	fread_s( pRAW+5, FileSize, 1, FileSize, pFile );
	fclose( pFile );

	pRAW[0] = 1;	// Indicator of a CSO file
	*((U32*) (pRAW+1)) = FileSize;

	return (const char*) pRAW;
}


//////////////////////////////////////////////////////////////////////////
// Handles include dependencies
//
void	IncludesManager::SetCurrentlyCompilingShader( const char* _pShaderFileName )
{
#ifdef _DEBUG
	int		IncludesCount = sizeof(m_pIncludeFiles) / sizeof(IncludePair);
	if ( m_pDependencies == NULL )
	{
		m_pDependencies = new Dependencies[IncludesCount];
		for ( int IncludeFileIndex=0; IncludeFileIndex < IncludesCount; IncludeFileIndex++ )
		{
			Dependencies&	D = m_pDependencies[IncludeFileIndex];
			D.Count = 0;
			D.ppDependencies = new const char*[MAX_DEPENDENCIES];
		}
	}

	// Store current shader's name
	m_pCurrentShaderFileName = _pShaderFileName;
#endif
}

void	IncludesManager::RegisterMaterial( const char* _pShaderFileName, Material& _Material )
{
#ifdef _DEBUG
	m_pShaderName2Material.AddUnique( _pShaderFileName, &_Material );
#endif
}

void	IncludesManager::RegisterComputeShader( const char* _pShaderFileName, ComputeShader& _ComputeShader )
{
#ifdef _DEBUG
	m_pShaderName2ComputeShader.AddUnique( _pShaderFileName, &_ComputeShader );
#endif
}


IncludesManager::~IncludesManager()
{
#ifdef _DEBUG
	int		IncludesCount = sizeof(m_pIncludeFiles) / sizeof(IncludePair);
	for ( int IncludeFileIndex=0; IncludeFileIndex < IncludesCount; IncludeFileIndex++ )
	{
		Dependencies&	D = m_pDependencies[IncludeFileIndex];
		delete[] D.ppDependencies;
	}
	delete[] m_pDependencies;
#endif
}

#ifdef SURE_DEBUG

#include <sys/stat.h>

void	IncludesManager::WatchIncludeModifications() const
{
	int				IncludesCount = sizeof(m_pIncludeFiles) / sizeof(IncludePair);
	IncludePair*	pPair = m_pIncludeFiles;
	for ( int IncludeFileIndex=0; IncludeFileIndex < IncludesCount; IncludeFileIndex++, pPair++ )
	{
		const Dependencies&	D = m_pDependencies[IncludeFileIndex];

		time_t	LastModificationTime = GetFileModTime( pPair->pFullPath );
		if ( LastModificationTime <= D.LastModificationTime )
			continue;	// No change...

		D.LastModificationTime = LastModificationTime;	// Update last checked time...

		// Iterate on all dependencies and force recompilation
		for ( int DependencyIndex=0; DependencyIndex < D.Count; DependencyIndex++ )
		{
			const char*	pShaderFile = D.ppDependencies[DependencyIndex];
			Material**	ppMaterial = m_pShaderName2Material.Get( pShaderFile );
			if ( ppMaterial != NULL )
			{	// Recompile that material...
				(*ppMaterial)->ForceRecompile();
				continue;
			}

			// Look for a compute shader then?
			ComputeShader**	ppCS = m_pShaderName2ComputeShader.Get( pShaderFile );
			ASSERT( ppCS != NULL, "Failed to retrieve actual dependency material/compute shader from shader file name! (Did you forget to register the material/compute shader after compilation?)" );

			(*ppCS)->ForceRecompile();
		}
	}
}

time_t	IncludesManager::GetFileModTime( const char* _pFileName ) const
{
	struct _stat statInfo;
	ASSERT( !_stat( _pFileName, &statInfo ), "Can't obtain status on include file: Check full path to file actually exists!" );

	return statInfo.st_mtime;
}

#endif