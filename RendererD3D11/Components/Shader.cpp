
#include "Shader.h"
#include "ConstantBuffer.h"

#include <stdio.h>
#include <io.h>

#include "D3Dcompiler.h"
#include "D3D11Shader.h"

bool	Shader::ms_LoadFromBinary = false;

Shader::Shader( Device& _Device, const char* _pShaderFileName, const IVertexFormatDescriptor& _Format, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride )
	: Component( _Device )
	, m_Format( _Format )
	, m_pVertexLayout( NULL )
	, m_pVS( NULL )
	, m_pHS( NULL )
	, m_pDS( NULL )
	, m_pGS( NULL )
	, m_pPS( NULL )
	, m_pShaderPath( NULL )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef MATERIAL_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
	ASSERT( _pShaderCode != NULL, "Shader code is NULL!" );

	m_pIncludeOverride = _pIncludeOverride;
	m_bHasErrors = false;

	// Store the default NULL pointer to point to the shader path
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	m_pShaderFileName = CopyString( _pShaderFileName );
#endif
#ifndef GODCOMPLEX
	m_pShaderPath = GetShaderPath( _pShaderFileName );
	m_Pointer2FileName.Add( NULL, m_pShaderPath );
#endif

#if defined(_DEBUG) && defined(WATCH_SHADER_MODIFICATIONS)
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

Shader::Shader( Device& _Device, const char* _pShaderFileName, const IVertexFormatDescriptor& _Format, ID3DBlob* _pVS, ID3DBlob* _pHS, ID3DBlob* _pDS, ID3DBlob* _pGS, ID3DBlob* _pPS )
	: Component( _Device )
	, m_Format( _Format )
	, m_pVertexLayout( NULL )
	, m_pVS( NULL )
	, m_pHS( NULL )
	, m_pDS( NULL )
	, m_pGS( NULL )
	, m_pPS( NULL )
	, m_pEntryPointVS( NULL )
	, m_pEntryPointHS( NULL )
	, m_pEntryPointDS( NULL )
	, m_pEntryPointGS( NULL )
	, m_pEntryPointPS( NULL )
	, m_pShaderPath( NULL )
	, m_pIncludeOverride( NULL )
	, m_bHasErrors( false )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef MATERIAL_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
	ASSERT( _pVS != NULL, "You can't provide a NULL VS blob!" );
	CompileShaders( NULL, _pVS, _pHS, _pDS, _pGS, _pPS );
}

Shader::~Shader()
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

	if ( m_pShaderPath != NULL )	{ delete[] m_pShaderPath; m_pShaderPath = NULL; }
	if ( m_pVertexLayout != NULL )	{ m_pVertexLayout->Release(); m_pVertexLayout = NULL; }
	if ( m_pVS != NULL )			{ m_pVS->Release(); m_pVS = NULL; }
	if ( m_pHS != NULL )			{ m_pHS->Release(); m_pHS = NULL; }
	if ( m_pDS != NULL )			{ m_pDS->Release(); m_pDS = NULL; }
	if ( m_pGS != NULL )			{ m_pGS->Release(); m_pGS = NULL; }
	if ( m_pPS != NULL )			{ m_pPS->Release(); m_pPS = NULL; }
	if ( m_pMacros != NULL )		{ delete[] m_pMacros; m_pMacros = NULL; }
}

void	Shader::CompileShaders( const char* _pShaderCode, ID3DBlob* _pVS, ID3DBlob* _pHS, ID3DBlob* _pDS, ID3DBlob* _pGS, ID3DBlob* _pPS )
{
	// Release any pre-existing shader
	if ( m_pVertexLayout != NULL )	{ m_pVertexLayout->Release(); m_pVertexLayout = NULL; }
	if ( m_pVS != NULL )			{ m_pVS->Release(); m_pVS = NULL; }
	if ( m_pHS != NULL )			{ m_pHS->Release(); m_pHS = NULL; }
	if ( m_pDS != NULL )			{ m_pDS->Release(); m_pDS = NULL; }
	if ( m_pGS != NULL )			{ m_pGS->Release(); m_pGS = NULL; }
	if ( m_pPS != NULL )			{ m_pPS->Release(); m_pPS = NULL; }

	bool	bHasErrors = false;

	//////////////////////////////////////////////////////////////////////////
	// Compile the compulsory vertex shader
	ASSERT( _pVS != NULL || m_pEntryPointVS != NULL, "Invalid VertexShader entry point !" );
#ifdef DIRECTX10
	ID3DBlob*   pShader = _pVS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointVS, "vs_4_0", this ) : _pVS;
#else
	ID3DBlob*   pShader = _pVS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointVS, "vs_5_0", this ) : _pVS;
#endif
	if ( pShader != NULL )
	{
		void*	pBuffer = pShader->GetBufferPointer();
		U32		BufferLength = pShader->GetBufferSize();
		Check( m_Device.DXDevice().CreateVertexShader( pBuffer, BufferLength, NULL, &m_pVS ) );
		ASSERT( m_pVS != NULL, "Failed to create vertex shader!" );
#ifndef GODCOMPLEX
		m_VSConstants.Enumerate( *pShader );
#endif
		bHasErrors |= m_pVS == NULL;

		// Create the associated vertex layout
		Check( m_Device.DXDevice().CreateInputLayout( m_Format.GetInputElements(), m_Format.GetInputElementsCount(), pShader->GetBufferPointer(), pShader->GetBufferSize(), &m_pVertexLayout ) );
		ASSERT( m_pVertexLayout != NULL, "Failed to create vertex layout !" );
		bHasErrors |= m_pVertexLayout == NULL;

		pShader->Release();
	}
	else
		bHasErrors = true;

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional hull shader
	if ( !bHasErrors && (m_pEntryPointHS != NULL || _pHS != NULL) )
	{
#ifdef DIRECTX10
		ASSERT( false, "You can't use Hull Shaders if you define DIRECTX10!" );
#endif
		ID3DBlob*   pShader = _pHS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointHS, "hs_5_0", this ) : _pHS;
		if ( pShader != NULL )
		{
			void*	pBuffer = pShader->GetBufferPointer();
			U32		BufferLength = pShader->GetBufferSize();
			Check( m_Device.DXDevice().CreateHullShader( pBuffer, BufferLength, NULL, &m_pHS ) );
			ASSERT( m_pHS != NULL, "Failed to create hull shader!" );
#ifndef GODCOMPLEX
			m_HSConstants.Enumerate( *pShader );
#endif
			bHasErrors |= m_pHS == NULL;

			pShader->Release();
		}
		else
			bHasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional domain shader
	if ( !bHasErrors && (m_pEntryPointDS != NULL || _pDS != NULL) )
	{
#ifdef DIRECTX10
		ASSERT( false, "You can't use Domain Shaders if you define DIRECTX10!" );
#endif
		ID3DBlob*   pShader = _pDS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointDS, "ds_5_0", this ) : _pDS;
		if ( pShader != NULL )
		{
			Check( m_Device.DXDevice().CreateDomainShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pDS ) );
			ASSERT( m_pDS != NULL, "Failed to create domain shader!" );
#ifndef GODCOMPLEX
			m_DSConstants.Enumerate( *pShader );
#endif
			bHasErrors |= m_pDS == NULL;

			pShader->Release();
		}
		else
			bHasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional geometry shader
	if ( !bHasErrors && (m_pEntryPointGS != NULL || _pGS != NULL) )
	{
#ifdef DIRECTX10
		ID3DBlob*   pShader = _pGS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointGS, "gs_4_0", this ) : _pGS;
#else
		ID3DBlob*   pShader = _pGS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointGS, "gs_5_0", this ) : _pGS;
#endif
		if ( pShader != NULL )
		{
			Check( m_Device.DXDevice().CreateGeometryShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pGS ) );
			ASSERT( m_pGS != NULL, "Failed to create geometry shader!" );
#ifndef GODCOMPLEX
			m_GSConstants.Enumerate( *pShader );
#endif
			bHasErrors |= m_pGS == NULL;

			pShader->Release();
		}
		else
			bHasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional pixel shader
	if ( !bHasErrors && (m_pEntryPointPS != NULL || _pPS != NULL) )
	{

#ifdef _DEBUG
// CSO TEST
// We use a special pre-compiled CSO read from file here
if ( m_pEntryPointPS != NULL && *m_pEntryPointPS == 1 )
{
	U32	BufferSize = *((U32*) (m_pEntryPointPS+1));
	U8*	pBufferPointer = (U8*) m_pEntryPointPS+5;
	Check( m_Device.DXDevice().CreatePixelShader( pBufferPointer, BufferSize, NULL, &m_pPS ) );
	ASSERT( m_pPS != NULL, "Failed to create pixel shader!" );
	return;
}
// TEST
#endif

#ifdef DIRECTX10
		ID3DBlob*   pShader = _pPS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointPS, "ps_4_0", this ) : _pPS;
#else
		ID3DBlob*   pShader = _pPS == NULL ? CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointPS, "ps_5_0", this ) : _pPS;
#endif
		if ( pShader != NULL )
		{
			Check( m_Device.DXDevice().CreatePixelShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pPS ) );
			ASSERT( m_pPS != NULL, "Failed to create pixel shader!" );
#ifndef GODCOMPLEX
			m_PSConstants.Enumerate( *pShader );
#endif
			bHasErrors |= m_pPS == NULL;

			pShader->Release();
		}
		else
			bHasErrors = true;
	}

	m_bHasErrors = bHasErrors;
}

#if defined(SURE_DEBUG) || !defined(GODCOMPLEX)

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

ID3DBlob*   Shader::CompileShader( const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget, ID3DInclude* _pInclude, bool _bComputeShader )
{
#ifdef SAVE_SHADER_BLOB_TO
	// Check if we're forced to load from binary...
	if ( ms_LoadFromBinary )
		return LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPoint );
#endif

	ID3DBlob*   pCodeText;
	ID3DBlob*   pCode;
	ID3DBlob*   pErrors;


//_pShaderCode = pTestShader;


	D3DPreprocess( _pShaderCode, strlen(_pShaderCode), NULL, _pMacros, _pInclude, &pCodeText, &pErrors );
#if defined(_DEBUG) || defined(DEBUG_SHADER)
	if ( pErrors != NULL ) {
		MessageBoxA( NULL, (LPCSTR) pErrors->GetBufferPointer(), "Shader PreProcess Error !", MB_OK | MB_ICONERROR );
		ASSERT( pErrors == NULL, "Shader preprocess error !" );
	}
#endif

	U32 Flags1 = 0;
#if (defined(_DEBUG) && !defined(SAVE_SHADER_BLOB_TO)) || defined(NSIGHT)
		Flags1 |= D3DCOMPILE_DEBUG;
		Flags1 |= D3DCOMPILE_SKIP_OPTIMIZATION;
//		Flags1 |= D3DCOMPILE_WARNINGS_ARE_ERRORS;
		Flags1 |= D3DCOMPILE_PREFER_FLOW_CONTROL;
#else
	if ( _bComputeShader )
		Flags1 |= D3DCOMPILE_OPTIMIZATION_LEVEL1;	// Seems to "optimize" (i.e. strip) the important condition line that checks for threadID before writing to concurrent targets => This leads to "race condition" errors
	else
		Flags1 |= D3DCOMPILE_OPTIMIZATION_LEVEL3;
#endif
//		Flags1 |= D3DCOMPILE_ENABLE_STRICTNESS;
//		Flags1 |= D3DCOMPILE_IEEE_STRICTNESS;		// D3D9 compatibility, clamps precision to usual float32 but may prevent internal optimizations by the video card. Better leave it disabled!
		Flags1 |= D3DCOMPILE_PACK_MATRIX_ROW_MAJOR;	// MOST IMPORTANT FLAG!

	U32 Flags2 = 0;

	LPCVOID	pCodePointer = pCodeText->GetBufferPointer();
	size_t	CodeSize = pCodeText->GetBufferSize();
	size_t	CodeLength = strlen( (char*) pCodePointer );

	D3DCompile( pCodePointer, CodeSize, _pShaderFileName, _pMacros, _pInclude, _pEntryPoint, _pTarget, Flags1, Flags2, &pCode, &pErrors );
#if defined(_DEBUG) || defined(DEBUG_SHADER)
#ifdef WARNING_AS_ERRORS
	if ( pErrors != NULL )
#else
	if ( pCode == NULL && pErrors != NULL )
#endif
	{
		const char*	pErrorText = (LPCSTR) pErrors->GetBufferPointer();
		MessageBoxA( NULL, pErrorText, "Shader Compilation Error!", MB_OK | MB_ICONERROR );
		ASSERT( pErrors == NULL, "Shader compilation error!" );
	}
	else
		ASSERT( pCode != NULL, "Shader compilation failed => No error provided but didn't output any shader either!" );
#endif

// Save the binary blob to disk
#if defined(SAVE_SHADER_BLOB_TO) && !defined(NSIGHT)
	SaveBinaryBlob( _pShaderFileName, _pMacros, _pEntryPoint, *pCode );
#endif

	return pCode;
}

#else	// #if !defined(_DEBUG) && defined(GODCOMPLEX)

ID3DBlob*   Shader::CompileShader( const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget, ID3DInclude* _pInclude, bool _bComputeShader )
{
	U8*	pBigBlob = (U8*) _pShaderCode;	// Actually a giant blob...
	return LoadBinaryBlobFromAggregate( pBigBlob, _pEntryPoint );
}

#endif	// #ifdef _DEBUG

bool	Shader::Use()
{
	if ( HasErrors() )
		return false;	// Can't use a shader in error state!

	if ( !Lock() )
		return false;	// Someone else is locking it !

	ASSERT( m_pVertexLayout != NULL, "Can't use a material with an invalid vertex layout!" );

	m_Device.DXContext().IASetInputLayout( m_pVertexLayout );
	m_Device.DXContext().VSSetShader( m_pVS, NULL, 0 );
	m_Device.DXContext().HSSetShader( m_pHS, NULL, 0 );
	m_Device.DXContext().DSSetShader( m_pDS, NULL, 0 );
	m_Device.DXContext().GSSetShader( m_pGS, NULL, 0 );
	m_Device.DXContext().PSSetShader( m_pPS, NULL, 0 );

	m_Device.m_pCurrentMaterial = this;

	Unlock();

	return true;
}

HRESULT	Shader::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes )
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

HRESULT	Shader::Close( THIS_ LPCVOID _pData )
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

void	Shader::SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer )
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

void	Shader::SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData )
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

bool	Shader::Lock() const
{
#ifdef MATERIAL_COMPILE_THREADED
	return WaitForSingleObject( m_hCompileMutex, 0 ) == WAIT_OBJECT_0;
#else
	return true;
#endif
}
void	Shader::Unlock() const
{
#ifdef MATERIAL_COMPILE_THREADED
	ASSERT( ReleaseMutex( m_hCompileMutex ), "Failed to release mutex !" );
#endif
}

const char*	Shader::CopyString( const char* _pShaderFileName ) const
{
	if ( _pShaderFileName == NULL )
		return NULL;

	int		Length = strlen(_pShaderFileName)+1;
	char*	pResult = new char[Length];
	memcpy( pResult, _pShaderFileName, Length );

	return pResult;
}


// When compiling normally (i.e. not for the GodComplex 64K intro), allow strings to access shader variables
//
#ifndef GODCOMPLEX

bool	Shader::SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer )
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

bool	Shader::SetTexture( const char* _pBufferName, ID3D11ShaderResourceView* _pData )
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

static void	DeleteBindingDescriptors( int _EntryIndex, Shader::ShaderConstants::BindingDesc*& _pValue, void* _pUserData )
{
	delete _pValue;
}
Shader::ShaderConstants::~ShaderConstants()
{
	m_ConstantBufferName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
	m_TextureName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
}
void	Shader::ShaderConstants::Enumerate( ID3DBlob& _ShaderBlob )
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

void	Shader::ShaderConstants::BindingDesc::SetName( const char* _pName )
{
	int		NameLength = strlen(_pName)+1;
	pName = new char[NameLength];
	strcpy_s( pName, NameLength+1, _pName );
}
Shader::ShaderConstants::BindingDesc::~BindingDesc()
{
//	delete[] pName;	// This makes a heap corruption, I don't know why and I don't give a fuck about these C++ problems... (certainly some shit about allocating memory from a DLL and releasing it from another one or something like this) (which I don't give a shit about either BTW)
}

int		Shader::ShaderConstants::GetConstantBufferIndex( const char* _pBufferName ) const
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

int		Shader::ShaderConstants::GetShaderResourceViewIndex( const char* _pTextureName ) const
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

const char*	Shader::GetShaderPath( const char* _pShaderFileName ) const
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

#endif	// #ifndef GODCOMPLEX


//////////////////////////////////////////////////////////////////////////
// Shader rebuild on modifications mechanism...
#if defined(_DEBUG) || !defined(GODCOMPLEX)

#include <sys/types.h>
#include <sys/stat.h>

DictionaryString<Shader*>	Shader::ms_WatchedShaders;

static void	WatchShader( int _EntryIndex, Shader*& _Value, void* _pUserData )	{ _Value->WatchShaderModifications(); }

void		Shader::WatchShadersModifications()
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
	Shader*	pMaterial = (Shader*) _pData;
	pMaterial->RebuildShader();
}
#endif

void		Shader::WatchShaderModifications()
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

#ifdef MATERIAL_COMPILE_THREADED

	m_LastShaderModificationTime = LastModificationTime;

	// We're up to date
	Unlock();

	ASSERT( m_hCompileThread == 0, "Compilation thread already exists !" );

	DWORD	ThreadID;
    m_hCompileThread = CreateThread( NULL, 0, (LPTHREAD_START_ROUTINE) ThreadCompileMaterial, this, 0, &ThreadID );
    SetThreadPriority( m_hCompileThread, THREAD_PRIORITY_HIGHEST );
}

void		Shader::RebuildShader()
{
	DWORD	ErrorCode = WaitForSingleObject( m_hCompileMutex, 30000 );
#ifdef _DEBUG
	ASSERT( ErrorCode == WAIT_OBJECT_0, "Failed shader rebuild after 30 seconds waiting for access !" );
#else
	if ( ErrorCode != WAIT_OBJECT_0 )
		ExitProcess( -1 );	// Brutal fail!
#endif

#endif
 
	// Reload file
	FILE*	pFile = NULL;
	fopen_s( &pFile, m_pShaderFileName, "rb" );
//	ASSERT( pFile != NULL, "Failed to open shader file !" );
	if ( pFile == NULL )
	{	// Failed! Unlock but don't update time stamp so we try again next time...
		Unlock();
		return;
	}

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

#ifdef MATERIAL_COMPILE_THREADED
	// Close the thread once we're done !
	if ( m_hCompileThread )
		CloseHandle( m_hCompileThread );
	m_hCompileThread = 0;
#else
	m_LastShaderModificationTime = LastModificationTime;
#endif

	// Release the mutex: it's now safe to access the shader !
	Unlock();
}

void		Shader::ForceRecompile()
{
	m_LastShaderModificationTime--;	// So we're sure it will be recompiled on next watch!
}

time_t		Shader::GetFileModTime( const char* _pFileName )
{	
	struct _stat statInfo;
	_stat( _pFileName, &statInfo );

	return statInfo.st_mtime;
}

#endif	// #if defined(_DEBUG) || !defined(GODCOMPLEX)


#ifdef SAVE_SHADER_BLOB_TO

//////////////////////////////////////////////////////////////////////////
// Load from pre-compiled binary blob (useful for heavy shaders that never change)
//
Shader*	Shader::CreateFromBinaryBlob( Device& _Device, const char* _pShaderFileName, const IVertexFormatDescriptor& _Format, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS )
{
	ID3DBlob*	pVS = _pEntryPointVS != NULL ? LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPointVS ) : NULL;
	ID3DBlob*	pHS = _pEntryPointHS != NULL ? LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPointHS ) : NULL;
	ID3DBlob*	pDS = _pEntryPointDS != NULL ? LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPointDS ) : NULL;
	ID3DBlob*	pGS = _pEntryPointGS != NULL ? LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPointGS ) : NULL;
	ID3DBlob*	pPS = _pEntryPointPS != NULL ? LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPointPS ) : NULL;

	Shader*	pResult = new Shader( _Device, _pShaderFileName, _Format, pVS, pHS, pDS, pGS, pPS );

// No need: the blob was released by the constructor!
// 	pPS->Release();
// 	pGS->Release();
// 	pDS->Release();
// 	pHS->Release();
// 	pVS->Release();

	return pResult;
}

void	Shader::BuildMacroSignature( char _pSignature[1024], D3D_SHADER_MACRO* _pMacros ) {
	char*	pCurrent = _pSignature;
	while ( _pMacros != NULL && _pMacros->Name != NULL ) {
		*pCurrent++ = '_';
		strcpy_s( pCurrent, 1024-(pCurrent-_pSignature), _pMacros->Name );
		pCurrent += strlen( _pMacros->Name );
		*pCurrent++ = '=';
		strcpy_s( pCurrent, 1024-(pCurrent-_pSignature), _pMacros->Definition );
		pCurrent += strlen( _pMacros->Definition );
		_pMacros++;
	}
	*pCurrent = '\0';
}

void	Shader::SaveBinaryBlob( const char* _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, ID3DBlob& _Blob )
{
	ASSERT( _pShaderFileName != NULL, "Can't save binary blob => Invalid shader file name!" );
	ASSERT( _pEntryPoint != NULL, "Can't save binary blob => Invalid entry point name!" );

	// Build unique macros signature
	char	pMacrosSignature[1024];
	BuildMacroSignature( pMacrosSignature, _pMacros );

	// Build filename
	const char*	pFileName = strrchr( _pShaderFileName, '/' );
	if ( pFileName == NULL )
		pFileName = strrchr( _pShaderFileName, '\\' );
	ASSERT( pFileName != NULL, "Can't retrieve last '/' !" );
	int		FileNameIndex = 1+pFileName - _pShaderFileName;
	const char*	pExtension = strrchr( _pShaderFileName, '.' );
	ASSERT( pExtension != NULL, "Can't retrieve extension!" );
	int		ExtensionIndex = pExtension - _pShaderFileName;
	char	pFileNameWithoutExtension[1024];
	memcpy( pFileNameWithoutExtension, pFileName+1, ExtensionIndex-FileNameIndex );
	pFileNameWithoutExtension[ExtensionIndex-FileNameIndex] = '\0';	// End the file name here

	char	pFinalShaderName[1024];
	sprintf_s( pFinalShaderName, 1024, "%s%s%s.%s.fxbin", SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, pMacrosSignature, _pEntryPoint );

	// Create the binary file
	FILE*	pFile;
	fopen_s( &pFile, pFinalShaderName, "wb" );
	ASSERT( pFile != NULL, "Can't create binary shader file!" );

	// Write the entry point's length
	int	Length = strlen( _pEntryPoint )+1;
	fwrite( &Length, sizeof(int), 1, pFile );

	// Write the entry point name
	fwrite( _pEntryPoint, 1, Length, pFile );

	// Write the blob's length
	Length = _Blob.GetBufferSize();
//	ASSERT( Length < 65536, "Shader length doesn't fit on 16 bits!" );
	fwrite( &Length, sizeof(int), 1, pFile );

	// Write the blob's content
	LPCVOID	pCodePointer = _Blob.GetBufferPointer();
	fwrite( pCodePointer, 1, Length, pFile );

	// We're done!
	fclose( pFile );
}

ID3DBlob*	Shader::LoadBinaryBlob( const char* _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint )
{
	ASSERT( _pShaderFileName != NULL, "Can't load binary blob => Invalid shader file name!" );
	ASSERT( _pEntryPoint != NULL, "Can't load binary blob => Invalid entry point name!" );

	// Build unique macros signature
	char	pMacrosSignature[1024];
	BuildMacroSignature( pMacrosSignature, _pMacros );

	// Build filename
	const char*	pFileName = strrchr( _pShaderFileName, '/' );
	if ( pFileName == NULL )
		pFileName = strrchr( _pShaderFileName, '\\' );
	ASSERT( pFileName != NULL, "Can't retrieve last /!" );
	int		FileNameIndex = 1+pFileName - _pShaderFileName;
	const char*	pExtension = strrchr( _pShaderFileName, '.' );
	ASSERT( pExtension != NULL, "Can't retrieve extension!" );
	int		ExtensionIndex = pExtension - _pShaderFileName;
	char	pFileNameWithoutExtension[1024];
	memcpy( pFileNameWithoutExtension, pFileName+1, ExtensionIndex-FileNameIndex );
	pFileNameWithoutExtension[ExtensionIndex-FileNameIndex] = '\0';	// End the file name here

	char	pFinalShaderName[1024];
	sprintf_s( pFinalShaderName, 1024, "%s%s%s.%s.fxbin", SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, pMacrosSignature, _pEntryPoint );

	// Load the binary file
	FILE*	pFile;
	fopen_s( &pFile, pFinalShaderName, "rb" );
	ASSERT( pFile != NULL, "Can't open binary shader file! (did you compile the shader at least once?)" );

	// Read the entry point's length
	int	Length;
	fread_s( &Length, sizeof(int), sizeof(int), 1, pFile );

	// Read the entry point name
	char	pEntryPointCheck[1024];
	fread_s( pEntryPointCheck, 1024, 1, Length, pFile );
	ASSERT( !strcmp( _pEntryPoint, pEntryPointCheck ), "Entry point names mismatch!" );

	// Read the blob's length
	int	BlobSize;
	fread_s( &BlobSize, sizeof(int), sizeof(int), 1, pFile );

	// Create a D3DBlob
	ID3DBlob*	pResult = NULL;
	D3DCreateBlob( BlobSize, &pResult );

	// Read the blob's content
	LPVOID	pContent = pResult->GetBufferPointer();
	fread_s( pContent, BlobSize, 1, BlobSize, pFile );

	// We're done!
	fclose( pFile );

	return pResult;
}

#endif	// #if defined(_DEBUG) && defined(SAVE_SHADER_BLOB_TO)


ID3DBlob*	Shader::LoadBinaryBlobFromAggregate( const U8* _pAggregate, const char* _pEntryPoint )
{
	U16	BlobsCount = *((U16*) _pAggregate); _pAggregate+=2;	// Amount of blobs in the big blob
	for ( U16 BlobIndex=0; BlobIndex < BlobsCount; BlobIndex++ )
	{
		int	Cmp = strcmp( (char*) _pAggregate, _pEntryPoint );
		int	BlobEntryPointLength = strlen( (char*) _pAggregate );
		_pAggregate += BlobEntryPointLength+1;	// Skip the entry point's name

		if ( !Cmp )
		{	// Found it !
			U16	BlobStartOffset = *((U16*) _pAggregate); _pAggregate+=2;	// Retrieve the jump offset to reach the blob
			_pAggregate += BlobStartOffset;									// Go to the blob descriptor

			U16	BlobSize = *((U16*) _pAggregate); _pAggregate+=2;			// Retrieve the size of the blob

			// Create a D3DBlob
			ID3DBlob*	pResult = NULL;
			D3DCreateBlob( BlobSize, &pResult );

			// Copy our blob content
			void*		pBlobContent = pResult->GetBufferPointer();
			memcpy( pBlobContent, _pAggregate, BlobSize );

			// Yoohoo!
			return pResult;
		}

		// Not that blob either... Skip the jump offset...
		_pAggregate += 2;
	}

	return NULL;
}
