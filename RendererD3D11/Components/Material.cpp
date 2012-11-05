
#include "Material.h"
#include "ConstantBuffer.h"

#include <stdio.h>
#include <io.h>

#include "D3Dcompiler.h"
#include "D3D11Shader.h"


Material::Material( Device& _Device, const IVertexFormatDescriptor& _Format, const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride )
	: Component( _Device )
	, m_Format( _Format )
	, m_pVertexLayout( NULL )
	, m_pVS( NULL )
	, m_pHS( NULL )
	, m_pDS( NULL )
	, m_pGS( NULL )
	, m_pPS( NULL )
	, m_pShaderPath( NULL )
	, m_LastShaderModificationTime( 0 )
#ifdef MATERIAL_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
	ASSERT( _pShaderCode != NULL, "Shader code is NULL!" );

	m_pIncludeOverride = _pIncludeOverride;
	m_bHasErrors = false;

	// Store the default NULL pointer to point to the shader path
	m_pShaderFileName = CopyString( _pShaderFileName );
#ifndef GODCOMPLEX
	m_pShaderPath = GetShaderPath( _pShaderFileName );
	m_Pointer2FileName.Add( NULL, m_pShaderPath );
#endif

#ifdef _DEBUG
	if ( _pShaderFileName != NULL )
	{
		// Just ensure the file exists !
		FILE*	pFile;
		fopen_s( &pFile, _pShaderFileName, "rb" );
		ASSERT( pFile != NULL, "Shader file not found => You can ignore this assert but shader file will NOT be watched for modification!" );
		if ( pFile != NULL )
		{
			fclose( pFile );

			// Register as a watched shader
			ms_WatchedShaders.Add( _pShaderFileName, this );

#ifndef MATERIAL_COMPILE_AT_RUNTIME
			m_LastShaderModificationTime = GetFileModTime( _pShaderFileName );
#endif
		}
	}
#endif

	m_pEntryPointVS = _pEntryPointVS;
	m_pEntryPointHS = _pEntryPointHS;
	m_pEntryPointDS = _pEntryPointDS;
	m_pEntryPointGS = _pEntryPointGS;
	m_pEntryPointPS = _pEntryPointPS;

	if ( _pMacros != NULL )
	{
		D3D_SHADER_MACRO*	pMacro = _pMacros;
		while ( pMacro->Name != NULL )
			pMacro++;

		int	MacrosCount = 1 + pMacro - _pMacros;
		m_pMacros = new D3D_SHADER_MACRO[MacrosCount];
		memcpy( m_pMacros, _pMacros, MacrosCount*sizeof(D3D_SHADER_MACRO) );
	}
	else
		m_pMacros = NULL;

#ifdef MATERIAL_COMPILE_THREADED
	// Create the mutex for compilation exclusivity
	m_hCompileMutex = CreateMutexA( NULL, false, m_pShaderFileName );
	ASSERT( m_hCompileMutex != 0, "Failed to create compilation mutex !" );
#endif

#ifndef MATERIAL_COMPILE_AT_RUNTIME
#ifdef MATERIAL_COMPILE_THREADED
	ASSERT( false, "The MATERIAL_COMPILE_THREADED option should only work in pair with the MATERIAL_COMPILE_AT_RUNTIME option ! (i.e. You CANNOT define MATERIAL_COMPILE_THREADED without defining MATERIAL_COMPILE_AT_RUNTIME at the same time !)" );
#endif

	// Compile immediately
	CompileShaders( _pShaderCode );
#endif
}

Material::~Material()
{
#ifdef MATERIAL_COMPILE_THREADED
	// Destroy mutex
	CloseHandle( m_hCompileMutex );
#endif

#ifdef _DEBUG
	// Unregister as a watched shader
	if ( m_pShaderFileName != NULL )
	{
		ms_WatchedShaders.Remove( m_pShaderFileName );
		delete[] m_pShaderFileName;
	}
#endif

	if ( m_pShaderPath != NULL ) { delete[] m_pShaderPath; m_pShaderPath = NULL; }
	if ( m_pVertexLayout != NULL ) { m_pVertexLayout->Release(); m_pVertexLayout = NULL; }
	if ( m_pVS != NULL ) { m_pVS->Release(); m_pVS = NULL; }
	if ( m_pHS != NULL ) { m_pHS->Release(); m_pHS = NULL; }
	if ( m_pDS != NULL ) { m_pDS->Release(); m_pDS = NULL; }
	if ( m_pGS != NULL ) { m_pGS->Release(); m_pGS = NULL; }
	if ( m_pPS != NULL ) { m_pPS->Release(); m_pPS = NULL; }
	if ( m_pMacros != NULL ) { delete[] m_pMacros; m_pMacros = NULL; }
}

void	Material::CompileShaders( const char* _pShaderCode )
{
	// Release any pre-existing shader
	if ( m_pVertexLayout != NULL ) m_pVertexLayout->Release();
	if ( m_pVS != NULL )	m_pVS->Release();
	if ( m_pHS != NULL )	m_pHS->Release();
	if ( m_pDS != NULL )	m_pDS->Release();
	if ( m_pGS != NULL )	m_pGS->Release();
	if ( m_pPS != NULL )	m_pPS->Release();

	//////////////////////////////////////////////////////////////////////////
	// Compile the compulsory vertex shader
	ASSERT( m_pEntryPointVS != NULL, "Invalid VertexShader entry point !" );
	ID3DBlob*   pShader = CompileShader( _pShaderCode, m_pMacros, m_pEntryPointVS, "vs_4_0" );
	if ( pShader != NULL )
	{
		Check( m_Device.DXDevice().CreateVertexShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pVS ) );
		ASSERT( m_pVS != NULL, "Failed to create vertex shader !" );
#ifndef GODCOMPLEX
		m_VSConstants.Enumerate( *pShader );
#endif
		m_bHasErrors |= m_pVS == NULL;

		// Create the associated vertex layout
		Check( m_Device.DXDevice().CreateInputLayout( m_Format.GetInputElements(), m_Format.GetInputElementsCount(), pShader->GetBufferPointer(), pShader->GetBufferSize(), &m_pVertexLayout ) );
		ASSERT( m_pVertexLayout != NULL, "Failed to create vertex layout !" );
		m_bHasErrors |= m_pVertexLayout == NULL;

		pShader->Release();
	}
	else
		m_bHasErrors = true;

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional hull shader
	if ( !m_bHasErrors && m_pEntryPointHS != NULL )
	{
		ID3DBlob*   pShader = CompileShader( _pShaderCode, m_pMacros, m_pEntryPointHS, "hs_5_0" );
		if ( pShader != NULL )
		{
			Check( m_Device.DXDevice().CreateHullShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pHS ) );
			ASSERT( m_pHS != NULL, "Failed to create hull shader !" );
#ifndef GODCOMPLEX
			m_HSConstants.Enumerate( *pShader );
#endif
			m_bHasErrors |= m_pHS == NULL;

			pShader->Release();
		}
		else
			m_bHasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional domain shader
	if ( !m_bHasErrors && m_pEntryPointDS != NULL )
	{
		ID3DBlob*   pShader = CompileShader( _pShaderCode, m_pMacros, m_pEntryPointDS, "ds_5_0" );
		if ( pShader != NULL )
		{
			Check( m_Device.DXDevice().CreateDomainShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pDS ) );
			ASSERT( m_pDS != NULL, "Failed to create domain shader !" );
#ifndef GODCOMPLEX
			m_DSConstants.Enumerate( *pShader );
#endif
			m_bHasErrors |= m_pDS == NULL;

			pShader->Release();
		}
		else
			m_bHasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional geometry shader
	if ( !m_bHasErrors && m_pEntryPointGS != NULL )
	{
		ID3DBlob*   pShader = CompileShader( _pShaderCode, m_pMacros, m_pEntryPointGS, "gs_4_0" );
		if ( pShader != NULL )
		{
			Check( m_Device.DXDevice().CreateGeometryShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pGS ) );
			ASSERT( m_pGS != NULL, "Failed to create geometry shader !" );
#ifndef GODCOMPLEX
			m_GSConstants.Enumerate( *pShader );
#endif
			m_bHasErrors |= m_pGS == NULL;

			pShader->Release();
		}
		else
			m_bHasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional pixel shader
	if ( !m_bHasErrors && m_pEntryPointPS != NULL )
	{
		ID3DBlob*   pShader = CompileShader( _pShaderCode, m_pMacros, m_pEntryPointPS, "ps_4_0" );
		if ( pShader != NULL )
		{
			Check( m_Device.DXDevice().CreatePixelShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pPS ) );
			ASSERT( m_pPS != NULL, "Failed to create pixel shader !" );
#ifndef GODCOMPLEX
			m_PSConstants.Enumerate( *pShader );
#endif
			m_bHasErrors |= m_pPS == NULL;

			pShader->Release();
		}
		else
			m_bHasErrors = true;
	}
}

void	Material::Use()
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	if ( m_pVertexLayout != NULL )
	{
		m_Device.DXContext().IASetInputLayout( m_pVertexLayout );
		m_Device.DXContext().VSSetShader( m_pVS, NULL, 0 );
		m_Device.DXContext().HSSetShader( m_pHS, NULL, 0 );
		m_Device.DXContext().DSSetShader( m_pDS, NULL, 0 );
		m_Device.DXContext().GSSetShader( m_pGS, NULL, 0 );
		m_Device.DXContext().PSSetShader( m_pPS, NULL, 0 );
	}

	Unlock();
}

// Embedded shader for debug & testing...
// static char*	pTestShader =
// 	"struct VS_IN\r\n" \
// 	"{\r\n" \
// 	"	float4	__Position : SV_POSITION;\r\n" \
// 	"};\r\n" \
// 	"\r\n" \
// 	"VS_IN	VS( VS_IN _In ) { return _In; }\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"\r\n" \
// 	"";

ID3DBlob*   Material::CompileShader( const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget )
{
	ID3DBlob*   pCodeText;
	ID3DBlob*   pCode;
	ID3DBlob*   pErrors;


//_pShaderCode = pTestShader;


	D3DPreprocess( _pShaderCode, strlen(_pShaderCode), NULL, _pMacros, this, &pCodeText, &pErrors );
#if defined(_DEBUG) || defined(DEBUG_SHADER)
	if ( pErrors != NULL )
	{
		MessageBoxA( NULL, (LPCSTR) pErrors->GetBufferPointer(), "Shader PreProcess Error !", MB_OK | MB_ICONERROR );
		ASSERT( pErrors == NULL, "Shader preprocess error !" );
	}
#endif

	U32 Flags1 = 0;
#ifdef _DEBUG
		Flags1 |= D3D10_SHADER_DEBUG;
		Flags1 |= D3D10_SHADER_SKIP_OPTIMIZATION;
//		Flags1 |= D3D10_SHADER_WARNINGS_ARE_ERRORS;
		Flags1 |= D3D10_SHADER_PREFER_FLOW_CONTROL;
#else
		Flags1 |= D3D10_SHADER_OPTIMIZATION_LEVEL3;
#endif
		Flags1 |= D3D10_SHADER_ENABLE_STRICTNESS;
		Flags1 |= D3D10_SHADER_IEEE_STRICTNESS;
		Flags1 |= D3D10_SHADER_PACK_MATRIX_ROW_MAJOR;	// MOST IMPORTANT FLAG !

	U32 Flags2 = 0;

	LPCVOID	pCodePointer = pCodeText->GetBufferPointer();
	size_t	CodeSize = pCodeText->GetBufferSize();
	size_t	CodeLength = strlen( (char*) pCodePointer );

	D3DCompile( pCodePointer, CodeSize, NULL, _pMacros, this, _pEntryPoint, _pTarget, Flags1, Flags2, &pCode, &pErrors );
#if defined(_DEBUG) || defined(DEBUG_SHADER)
	if ( pCode == NULL && pErrors != NULL )
	{
		MessageBoxA( NULL, (LPCSTR) pErrors->GetBufferPointer(), "Shader Compilation Error !", MB_OK | MB_ICONERROR );
		ASSERT( pErrors == NULL, "Shader compilation error !" );
	}
	else
		ASSERT( pCode != NULL, "Shader compilation failed => No error provided but didn't output any shader either !" );
#endif

	return pCode;
}

HRESULT	Material::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes )
{
	if ( m_pIncludeOverride != NULL )
		return m_pIncludeOverride->Open( _IncludeType, _pFileName, _pParentData, _ppData, _pBytes );

#ifndef GODCOMPLEX
	const char**	ppShaderPath = m_Pointer2FileName.Get( U32(_pParentData) );
	ASSERT( ppShaderPath != NULL, "Failed to retrieve data pointer !" );
	const char*	pShaderPath = *ppShaderPath;

	char	pFullName[4096];
	sprintf_s( pFullName, 4096, "%s%s", pShaderPath, _pFileName );

	FILE*	pFile;
	fopen_s( &pFile, pFullName, "rb" );
	ASSERT( pFile != NULL, "Include file not found !" );

	fseek( pFile, 0, SEEK_END );
	U32	Size = ftell( pFile );
	fseek( pFile, 0, SEEK_SET );

	char*	pBuffer = new char[Size];
	fread_s( pBuffer, Size, 1, Size, pFile );
//	pBuffer[Size] = '\0';

	*_pBytes = Size;
	*_ppData = pBuffer;

	fclose( pFile );

	// Register this shader's path as attached to the data pointer
	const char*	pIncludedShaderPath = GetShaderPath( pFullName );
	m_Pointer2FileName.Add( U32(*_ppData), pIncludedShaderPath );
#else
	ASSERT( false, "You MUST provide an ID3DINCLUDE override when compiling with the GODCOMPLEX option !" );
#endif

	return S_OK;
}

HRESULT	Material::Close( THIS_ LPCVOID _pData )
{
	if ( m_pIncludeOverride != NULL )
		return m_pIncludeOverride->Close( _pData );

#ifndef GODCOMPLEX
	// Remove entry from dictionary
	const char**	ppShaderPath = m_Pointer2FileName.Get( U32(_pData) );
	ASSERT( ppShaderPath != NULL, "Failed to retrieve data pointer !" );
	delete[] *ppShaderPath;
	m_Pointer2FileName.Remove( U32(_pData) );

	// Delete file content
	delete[] _pData;
#endif

	return S_OK;
}

void	Material::SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();
	m_Device.DXContext().VSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pHS != NULL )
		m_Device.DXContext().HSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pDS != NULL )
		m_Device.DXContext().DSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pGS != NULL )
		m_Device.DXContext().GSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pPS != NULL )
		m_Device.DXContext().PSSetConstantBuffers( _BufferSlot, 1, &pBuffer );

	Unlock();
}

void	Material::SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	m_Device.DXContext().VSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pHS != NULL )
		m_Device.DXContext().HSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pDS != NULL )
		m_Device.DXContext().DSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pGS != NULL )
		m_Device.DXContext().GSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pPS != NULL )
		m_Device.DXContext().PSSetShaderResources( _BufferSlot, 1, &_pData );

	Unlock();
}


#ifndef GODCOMPLEX
bool	Material::SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer )
{
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = true;
	if ( m_pVertexLayout != NULL )
	{
		bUsed = false;
		ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();

		{
			int	SlotIndex = m_VSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().VSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_HSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().HSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_DSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().DSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_GSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().GSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_PSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().PSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
	}

	Unlock();

	return	bUsed;
}

bool	Material::SetTexture( const char* _pBufferName, ID3D11ShaderResourceView* _pData )
{
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = true;
	if ( m_pVertexLayout != NULL )
	{
		bUsed = false;
		{
			int	SlotIndex = m_VSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().VSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_HSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().HSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_DSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().DSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_GSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().GSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_PSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_Device.DXContext().PSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
	}

	Unlock();

	return	bUsed;
}

static void	DeleteBindingDescriptors( Material::ShaderConstants::BindingDesc*& _pValue, void* _pUserData )
{
	delete _pValue;
}
Material::ShaderConstants::~ShaderConstants()
{
	m_ConstantBufferName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
	m_TextureName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
}
void	Material::ShaderConstants::Enumerate( ID3DBlob& _ShaderBlob )
{
	ID3D11ShaderReflection*	pReflector = NULL; 
	D3DReflect( _ShaderBlob.GetBufferPointer(), _ShaderBlob.GetBufferSize(), IID_ID3D11ShaderReflection, (void**) &pReflector );

	D3D11_SHADER_DESC	ShaderDesc;
	pReflector->GetDesc( &ShaderDesc );

	// Enumerate bound resources
	for ( int ResourceIndex=0; ResourceIndex < int(ShaderDesc.BoundResources); ResourceIndex++ )
	{
		D3D11_SHADER_INPUT_BIND_DESC	BindDesc;
		pReflector->GetResourceBindingDesc( ResourceIndex, &BindDesc );

		BindingDesc**	ppDesc = NULL;
		switch ( BindDesc.Type )
		{
		case D3D_SIT_TEXTURE:
			ppDesc = &m_TextureName2Descriptor.Add( BindDesc.Name );
			break;

		case D3D_SIT_CBUFFER:
			ppDesc = &m_ConstantBufferName2Descriptor.Add( BindDesc.Name );
			break;
		}
		if ( ppDesc == NULL )
			continue;	// We're not interested in that type !

		*ppDesc = new BindingDesc();
		(*ppDesc)->SetName( BindDesc.Name );
		(*ppDesc)->Slot = BindDesc.BindPoint;
#ifdef __DEBUG_UPLOAD_ONLY_ONCE
		(*ppDesc)->bUploaded = false;	// Not uploaded yet !
#endif
	}

	pReflector->Release();
}

void	Material::ShaderConstants::BindingDesc::SetName( const char* _pName )
{
	int		NameLength = strlen(_pName)+1;
	pName = new char[NameLength];
	strcpy_s( pName, NameLength+1, _pName );
}
Material::ShaderConstants::BindingDesc::~BindingDesc()
{
//	delete[] pName;	// This makes a heap corruption, I don't know why and I don't give a fuck about these C++ problems... (certainly some shit about allocating memory from a DLL and releasing it from another one or something like this) (which I don't give a shit about either BTW)
}

int		Material::ShaderConstants::GetConstantBufferIndex( const char* _pBufferName ) const
{
	BindingDesc**	ppValue = m_ConstantBufferName2Descriptor.Get( _pBufferName );

#ifdef __DEBUG_UPLOAD_ONLY_ONCE
	// Ensure the buffer is uploaded only once !
	if ( ppValue != NULL )
	{
		if ( (*ppValue)->bUploaded )
			return -1;
		(*ppValue)->bUploaded = true;	// Now it has been uploaded ! Don't come back !
	}
#endif

	return ppValue != NULL ? (*ppValue)->Slot : -1;
}

int		Material::ShaderConstants::GetShaderResourceViewIndex( const char* _pTextureName ) const
{
	BindingDesc**	ppValue = m_TextureName2Descriptor.Get( _pTextureName );

#ifdef __DEBUG_UPLOAD_ONLY_ONCE
	// Ensure the texture is uploaded only once !
	if ( ppValue != NULL )
	{
		if ( (*ppValue)->bUploaded )
			return -1;
		(*ppValue)->bUploaded = true;	// Now it has been uploaded ! Don't come back !
	}
#endif

	return ppValue != NULL ? (*ppValue)->Slot : -1;
}
#endif

bool	Material::Lock() const
{
#ifdef MATERIAL_COMPILE_THREADED
	return WaitForSingleObject( m_hCompileMutex, 0 ) == WAIT_OBJECT_0;
#else
	return true;
#endif
}
void	Material::Unlock() const
{
#ifdef MATERIAL_COMPILE_THREADED
	ASSERT( ReleaseMutex( m_hCompileMutex ), "Failed to release mutex !" );
#endif
}

const char*	Material::CopyString( const char* _pShaderFileName ) const
{
	if ( _pShaderFileName == NULL )
		return NULL;

	int		Length = strlen(_pShaderFileName)+1;
	char*	pResult = new char[Length];
	strcpy_s( pResult, Length, _pShaderFileName );

	return pResult;
}

#ifndef GODCOMPLEX
const char*	Material::GetShaderPath( const char* _pShaderFileName ) const
{
	char*	pResult = NULL;
	if ( _pShaderFileName != NULL )
	{
		int	FileNameLength = strlen(_pShaderFileName)+1;
		pResult = new char[FileNameLength];
		strcpy_s( pResult, FileNameLength, _pShaderFileName );

		char*	pLastSlash = strrchr( pResult, '\\' );
		if ( pLastSlash == NULL )
			pLastSlash = strrchr( pResult, '/' );
		if ( pLastSlash != NULL )
			pLastSlash[1] = '\0';
	}

	if ( pResult == NULL )
	{	// Empty string...
		pResult = new char[1];
		pResult = '\0';
		return pResult;
	}

	return pResult;
}
#endif


//////////////////////////////////////////////////////////////////////////
// Shader rebuild on modifications mechanism...
#include <sys/types.h>
#include <sys/stat.h>

DictionaryString<Material*>	Material::ms_WatchedShaders;

static void	WatchShader( Material*& _Value, void* _pUserData )	{ _Value->WatchShaderModifications(); }

void		Material::WatchShadersModifications()
{
	static int	LastTime = -1;
	int			CurrentTime = timeGetTime();
	if ( LastTime >= 0 && (CurrentTime - LastTime) < MATERIAL_REFRESH_CHANGES_INTERVAL )
		return;	// Too soon to check !

	// Update last check time
	LastTime = CurrentTime;

	ms_WatchedShaders.ForEach( WatchShader, NULL );
}

#ifdef MATERIAL_COMPILE_THREADED
void	ThreadCompileMaterial( void* _pData )
{
	Material*	pMaterial = (Material*) _pData;
	pMaterial->RebuildShader();
}
#endif

void		Material::WatchShaderModifications()
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	// Check if the shader file changed since last time
	time_t	LastModificationTime = GetFileModTime( m_pShaderFileName );
	if ( LastModificationTime <= m_LastShaderModificationTime )
	{	// No change !
		Unlock();
		return;
	}

	m_LastShaderModificationTime = LastModificationTime;

	// We're up to date
	Unlock();

#ifdef MATERIAL_COMPILE_THREADED
	ASSERT( m_hCompileThread == 0, "Compilation thread already exists !" );

	DWORD	ThreadID;
    m_hCompileThread = CreateThread( NULL, 0, (LPTHREAD_START_ROUTINE) ThreadCompileMaterial, this, 0, &ThreadID );
    SetThreadPriority( m_hCompileThread, THREAD_PRIORITY_HIGHEST );
}

void		Material::RebuildShader()
{
	DWORD	ErrorCode = WaitForSingleObject( m_hCompileMutex, 30000 );
#ifdef _DEBUG
	ASSERT( ErrorCode == WAIT_OBJECT_0, "Failed shader rebuild after 30 seconds waiting for access !" );
#else
	if ( ErrorCode != WAIT_OBJECT_0 )
		ExitProcess( -1 );	// Failed !
#endif
#endif
 
	// Reload file
	FILE*	pFile = NULL;
	fopen_s( &pFile, m_pShaderFileName, "rb" );
	ASSERT( pFile != NULL, "Failed to open shader file !" );

	fseek( pFile, 0, SEEK_END );
	size_t	FileSize = ftell( pFile );
	fseek( pFile, 0, SEEK_SET );

	char*	pShaderCode = new char[FileSize+1];
	fread_s( pShaderCode, FileSize, 1, FileSize, pFile );
	pShaderCode[FileSize] = '\0';

	fclose( pFile );

	// Compile
	CompileShaders( pShaderCode );

	delete[] pShaderCode;

	// Release the mutex: it's now safe to access the shader !
	Unlock();

#ifdef MATERIAL_COMPILE_THREADED
	// Close the thread once we're done !
	if ( m_hCompileThread )
		CloseHandle( m_hCompileThread );
	m_hCompileThread = 0;
#endif
}

time_t		Material::GetFileModTime( const char* _pFileName )
{	
	struct _stat statInfo;
	_stat( _pFileName, &statInfo );

	return statInfo.st_mtime;
}
