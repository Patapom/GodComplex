#include "stdafx.h"

#include "Shader.h"
#include "ConstantBuffer.h"
#include "..\Utility\FileServer.h"
#include "..\Utility\ShaderCompiler.h"


Shader::Shader( Device& _device, const BString& _shaderFileName, const IVertexFormatDescriptor& _format, D3D_SHADER_MACRO* _macros, const BString& _entryPointVS, const BString& _entryPointHS, const BString& _entryPointDS, const BString& _entryPointGS, const BString& _entryPointPS, IFileServer* _fileServerOverride )
	: Component( _device )
	, m_format( _format )
	, m_shaderFileName( _shaderFileName )
	, m_hasErrors( false )
	, m_vertexLayout( NULL )
	, m_pVS( NULL )
	, m_pHS( NULL )
	, m_pDS( NULL )
	, m_pGS( NULL )
	, m_pPS( NULL )
	, m_entryPointVS( _entryPointVS )
	, m_entryPointHS( _entryPointHS )
	, m_entryPointDS( _entryPointDS )
	, m_entryPointGS( _entryPointGS )
	, m_entryPointPS( _entryPointPS )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef MATERIAL_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
	m_fileServer = _fileServerOverride != NULL ? _fileServerOverride : &DiskFileServer::singleton;

	#if defined(_DEBUG) && defined(WATCH_SHADER_MODIFICATIONS)
		if ( !_shaderFileName.IsEmpty() ) {
			// Just ensure the file exists !
			FILE*	pFile;
			fopen_s( &pFile, _shaderFileName, "rb" );
			ASSERT( pFile != NULL, "Shader file not found => You can ignore this assert but shader file will NOT be watched for modification!" );
			if ( pFile != NULL ) {
				fclose( pFile );

				// Register as a watched shader
				ms_WatchedShaders.Add( _shaderFileName, this );

				#ifndef MATERIAL_COMPILE_AT_RUNTIME
					m_LastShaderModificationTime = GetFileModTime( _shaderFileName );
				#endif
			}
		}
	#endif

	if ( _macros != NULL ) {
		D3D_SHADER_MACRO*	pMacro = _macros;
		while ( pMacro->Name != NULL )
			pMacro++;

		int	MacrosCount = int( 1 + pMacro - _macros );
		m_macros = new D3D_SHADER_MACRO[MacrosCount];
		memcpy( m_macros, _macros, MacrosCount*sizeof(D3D_SHADER_MACRO) );
	} else {
		m_macros = NULL;
	}

	#ifdef MATERIAL_COMPILE_THREADED
		// Create the mutex for compilation exclusivity
		m_hCompileMutex = CreateMutexA( NULL, false, m_shaderFileName );
		ASSERT( m_hCompileMutex != 0, "Failed to create compilation mutex!" );
	#endif

	#ifndef MATERIAL_COMPILE_AT_RUNTIME
		#ifdef MATERIAL_COMPILE_THREADED
			ASSERT( false, "The MATERIAL_COMPILE_THREADED option should only work in pair with the MATERIAL_COMPILE_AT_RUNTIME option! (i.e. You CANNOT define MATERIAL_COMPILE_THREADED without defining MATERIAL_COMPILE_AT_RUNTIME at the same time!)" );
		#endif

		// Compile immediately
		CompileShaders();
	#endif
}

Shader::Shader( Device& _device, const BString& _shaderFileName, const IVertexFormatDescriptor& _Format, ID3DBlob* _pVS, ID3DBlob* _pHS, ID3DBlob* _pDS, ID3DBlob* _pGS, ID3DBlob* _pPS )
	: Component( _device )
	, m_format( _Format )
	, m_shaderFileName( _shaderFileName )
	, m_vertexLayout( NULL )
	, m_pVS( NULL )
	, m_pHS( NULL )
	, m_pDS( NULL )
	, m_pGS( NULL )
	, m_pPS( NULL )
	, m_entryPointVS( NULL )
	, m_entryPointHS( NULL )
	, m_entryPointDS( NULL )
	, m_entryPointGS( NULL )
	, m_entryPointPS( NULL )
	, m_fileServer( NULL )
	, m_macros( NULL )
	, m_hasErrors( false )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef MATERIAL_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
	ASSERT( _pVS != NULL, "You can't provide a NULL VS blob!" );
	CompileShaders( _pVS, _pHS, _pDS, _pGS, _pPS );
}

Shader::~Shader() {
	#ifdef MATERIAL_COMPILE_THREADED
		// Destroy mutex
		CloseHandle( m_hCompileMutex );
	#endif

	#ifdef _DEBUG
		// Unregister as a watched shader
		if ( !m_shaderFileName.IsEmpty() ) {
			ms_WatchedShaders.Remove( m_shaderFileName );
			delete[] m_shaderFileName;
		}
	#endif

	SAFE_RELEASE( m_vertexLayout );
	SAFE_RELEASE( m_pVS );
	SAFE_RELEASE( m_pHS );
	SAFE_RELEASE( m_pDS );
	SAFE_RELEASE( m_pGS );
	SAFE_RELEASE( m_pPS );
	SAFE_DELETE_ARRAY( m_macros );
}

// Compiles all individual shaders from the same shader file
void	Shader::CompileShaders( ID3DBlob* _blobVS, ID3DBlob* _blobHS, ID3DBlob* _blobDS, ID3DBlob* _blobGS, ID3DBlob* _blobPS ) {
	m_hasErrors = false;

	ID3D11VertexShader*		pVS = NULL;
	ID3D11InputLayout*		pVertexLayout = NULL;
	ID3D11HullShader*		pHS = NULL;
	ID3D11DomainShader*		pDS = NULL;
	ID3D11GeometryShader*	pGS = NULL;
	ID3D11PixelShader*		pPS = NULL;

	//////////////////////////////////////////////////////////////////////////
	// Compile the compulsory vertex shader
	ID3DBlob*   blobVS = _blobVS;
	if ( blobVS == NULL ) {
 		ASSERT( !m_entryPointVS.IsEmpty(), "Invalid VertexShader entry point!" );
		blobVS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointVS, "vs_5_0" );
	}
	if ( blobVS != NULL ) {
		Check( m_device.DXDevice().CreateVertexShader( blobVS->GetBufferPointer(), blobVS->GetBufferSize(), NULL, &pVS ) );
		ASSERT( pVS != NULL, "Failed to create vertex shader!" );
		m_hasErrors |= pVS == NULL;

		if ( !m_hasErrors ) {
			// Create the associated vertex layout
			Check( m_device.DXDevice().CreateInputLayout( m_format.GetInputElements(), m_format.GetInputElementsCount(), blobVS->GetBufferPointer(), blobVS->GetBufferSize(), &pVertexLayout ) );
			ASSERT( pVertexLayout != NULL, "Failed to create vertex layout!" );
			m_hasErrors |= pVertexLayout == NULL;
		}
	} else {
		m_hasErrors = true;
	}

// 	ASSERT( _blobVS != NULL || m_pEntryPointVS != NULL, "Invalid VertexShader entry point!" );
// 	blobVS = _blobVS == NULL ? CompileShader( m_shaderFileName, _shaderCode, m_pMacros, m_pEntryPointVS, "vs_5_0", this ) : _blobVS;

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional hull shader
	ID3DBlob*   blobHS = _blobHS;
	if ( blobHS == NULL && !m_hasErrors ) {
 		ASSERT( !m_entryPointHS.IsEmpty(), "Invalid HullShader entry point!" );
		blobHS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointHS, "hs_5_0" );
		if ( blobHS != NULL ) {
			Check( m_device.DXDevice().CreateHullShader( blobHS->GetBufferPointer(), blobHS->GetBufferSize(), NULL, &pHS ) );
			ASSERT( pHS != NULL, "Failed to create hull shader!" );
			m_hasErrors |= pHS == NULL;
		}
		else {
			m_hasErrors = true;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional domain shader
	ID3DBlob*   blobDS = _blobDS;
	if ( !m_hasErrors && blobDS == NULL ) {
 		ASSERT( !m_entryPointDS.IsEmpty(), "Invalid DomainShader entry point!" );
		blobDS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointDS, "ds_5_0" );
		if ( blobDS != NULL ) {
			Check( m_device.DXDevice().CreateDomainShader( blobDS->GetBufferPointer(), blobDS->GetBufferSize(), NULL, &pDS ) );
			ASSERT( pDS != NULL, "Failed to create domain shader!" );
			m_hasErrors |= pDS == NULL;
		}
		else
			m_hasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional geometry shader
	ID3DBlob*   blobGS = _blobGS;
	if ( !m_hasErrors && blobGS == NULL ) {
 		ASSERT( !m_entryPointGS.IsEmpty(), "Invalid GeometryShader entry point!" );
		blobGS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointGS, "gs_5_0" );
		if ( blobGS != NULL ) {
			Check( m_device.DXDevice().CreateGeometryShader( blobGS->GetBufferPointer(), blobGS->GetBufferSize(), NULL, &pGS ) );
			ASSERT( pGS != NULL, "Failed to create geometry shader!" );
			m_hasErrors |= pGS == NULL;
		}
		else
			m_hasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional pixel shader
	ID3DBlob*   blobPS = _blobPS;
	if ( !m_hasErrors && blobPS == NULL ) {
 		ASSERT( !m_entryPointPS.IsEmpty(), "Invalid PixelShader entry point!" );

		#ifdef _DEBUG
			// CSO TEST
			// We use a special pre-compiled CSO read from file here
			if ( !m_entryPointPS.IsEmpty() && m_entryPointPS[0] == 1 ) {
				U32	BufferSize = *((U32*) (((const char*) m_entryPointPS)+1));
				U8*	pBufferPointer = (U8*) ((const char*) m_entryPointPS)+5;
				Check( m_device.DXDevice().CreatePixelShader( pBufferPointer, BufferSize, NULL, &m_pPS ) );
				ASSERT( m_pPS != NULL, "Failed to create pixel shader!" );
				return;
			}
			// CSO TEST
		#endif

		blobPS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointPS, "ps_5_0" );
		if ( blobPS != NULL ) {
			Check( m_device.DXDevice().CreatePixelShader( blobPS->GetBufferPointer(), blobPS->GetBufferSize(), NULL, &pPS ) );
			ASSERT( pPS != NULL, "Failed to create pixel shader!" );
			m_hasErrors |= pPS == NULL;
		}
		else
			m_hasErrors = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Only replace actual member pointers once everything compiled successfully
	if ( !m_hasErrors ) {
		// Release any pre-existing shader
		SAFE_RELEASE( m_vertexLayout );
		SAFE_RELEASE( m_pVS );
		SAFE_RELEASE( m_pHS );
		SAFE_RELEASE( m_pDS );
		SAFE_RELEASE( m_pGS );
		SAFE_RELEASE( m_pPS );

		// Replace with brand new shaders
		m_vertexLayout = pVertexLayout;
		m_pVS = pVS;
		m_pHS = pHS;
		m_pDS = pDS;
		m_pGS = pGS;
		m_pPS = pPS;

		// Enumerate constants
		#ifdef ENABLE_SHADER_REFLECTION
			if ( blobVS != NULL )
				m_VSConstants.Enumerate( *blobVS );
			if ( blobHS != NULL )
				m_HSConstants.Enumerate( *blobHS );
			if ( blobDS != NULL )
				m_DSConstants.Enumerate( *blobDS );
			if ( blobGS != NULL )
				m_GSConstants.Enumerate( *blobGS );
			if ( blobPS != NULL )
				m_PSConstants.Enumerate( *blobPS );
		#endif
	}

	if ( blobVS != NULL )
		blobVS->Release();
	if ( blobHS != NULL )
		blobHS->Release();
	if ( blobDS != NULL )
		blobDS->Release();
	if ( blobGS != NULL )
		blobGS->Release();
	if ( blobPS != NULL )
		blobPS->Release();
}

// #if defined(SURE_DEBUG) || !defined(GODCOMPLEX)
// 
// // Embedded shader for debug & testing...
// // static char*	pTestShader =
// // 	"struct VS_IN\r\n" \
// // 	"{\r\n" \
// // 	"	float4	__Position : SV_POSITION;\r\n" \
// // 	"};\r\n" \
// // 	"\r\n" \
// // 	"VS_IN	VS( VS_IN _In ) { return _In; }\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"\r\n" \
// // 	"";
// 
// ID3DBlob*   Shader::CompileShader( ID3DInclude* _pInclude, const String& _pShaderFileName, const String& _shaderCode, D3D_SHADER_MACRO* _pMacros, const String& _pEntryPoint, const String& _pTarget, bool _bComputeShader ) {
// 
// 	#ifdef SAVE_SHADER_BLOB_TO
// 		// Check if we're forced to load from binary...
// 		if ( ms_LoadFromBinary )
// 			return LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPoint );
// 	#endif
// 
// 	ID3DBlob*   pCodeText;
// 	ID3DBlob*   pCode;
// 	ID3DBlob*   pErrors;
// 
// //_shaderCode = pTestShader;
// 
// 
// 	D3DPreprocess( _shaderCode, strlen(_shaderCode), NULL, _pMacros, _pInclude, &pCodeText, &pErrors );
// 	#if defined(_DEBUG) || defined(DEBUG_SHADER)
// 		if ( pErrors != NULL ) {
// 			MessageBoxA( NULL, (LPCSTR) pErrors->GetBufferPointer(), "Shader PreProcess Error!", MB_OK | MB_ICONERROR );
// 			ASSERT( pErrors == NULL, "Shader preprocess error!" );
// 		}
// 	#endif
// 
// 	U32 Flags1 = 0, Flags2 = 0;
// 	#if (defined(_DEBUG) && !defined(SAVE_SHADER_BLOB_TO)) || defined(RENDERDOC) || defined(NSIGHT)
// 		Flags1 |= D3DCOMPILE_DEBUG;
// 		Flags1 |= D3DCOMPILE_SKIP_OPTIMIZATION;
// //		Flags1 |= D3DCOMPILE_WARNINGS_ARE_ERRORS;
// 		Flags1 |= D3DCOMPILE_PREFER_FLOW_CONTROL;
// 
// //Flags1 |= _bComputeShader ? D3DCOMPILE_OPTIMIZATION_LEVEL1 : D3DCOMPILE_OPTIMIZATION_LEVEL3;
// 
// 	#else
// 		if ( _bComputeShader )
// 			Flags1 |= D3DCOMPILE_OPTIMIZATION_LEVEL1;	// Seems to "optimize" (i.e. strip) the important condition line that checks for threadID before writing to concurrent targets => This leads to "race condition" errors
// 		else
// 			Flags1 |= D3DCOMPILE_OPTIMIZATION_LEVEL3;
// 	#endif
// //		Flags1 |= D3DCOMPILE_ENABLE_STRICTNESS;
// //		Flags1 |= D3DCOMPILE_IEEE_STRICTNESS;		// D3D9 compatibility, clamps precision to usual float32 but may prevent internal optimizations by the video card. Better leave it disabled!
// 		Flags1 |= D3DCOMPILE_PACK_MATRIX_ROW_MAJOR;	// MOST IMPORTANT FLAG!
// 
// 	LPCVOID	pCodePointer = pCodeText->GetBufferPointer();
// 	size_t	CodeSize = pCodeText->GetBufferSize();
// 	size_t	CodeLength = strlen( (char*) pCodePointer );
// 
// 	D3DCompile( pCodePointer, CodeSize, _pShaderFileName, _pMacros, _pInclude, _pEntryPoint, _pTarget, Flags1, Flags2, &pCode, &pErrors );
// 
// 	#if defined(_DEBUG) || defined(DEBUG_SHADER)
// 		bool	hasWarningOrErrors = pErrors != NULL;	// Represents warnings and errors
// 		bool	hasErrors = pCode == NULL;				// Surely an error if no shader is returned!
// 		if ( hasWarningOrErrors && (ms_warningsAsError || hasErrors) ) {
// 			const char*	pErrorText = (LPCSTR) pErrors->GetBufferPointer();
// 			MessageBoxA( NULL, pErrorText, "Shader Compilation Error!", MB_OK | MB_ICONERROR );
// 			ASSERT( pErrors == NULL, "Shader compilation error!" );
// 			return NULL;
// 		} else {
// 			ASSERT( pCode != NULL, "Shader compilation failed => No error provided but didn't output any shader either!" );
// 		}
// 	#endif
// 
// 	// Save the binary blob to disk
// 	#if defined(SAVE_SHADER_BLOB_TO) && !defined(RENDERDOC) && !defined(NSIGHT)
// 		if ( pCode != NULL ) {
// 			SaveBinaryBlob( _pShaderFileName, _pMacros, _pEntryPoint, *pCode );
// 		}
// 	#endif
// 
// 	return pCode;
// }
// 
// #else	// #if !defined(_DEBUG) && defined(GODCOMPLEX)
// 
// ID3DBlob*   Shader::CompileShader( const char* _pShaderFileName, const char* _shaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget, ID3DInclude* _pInclude, bool _bComputeShader ) {
// 	U8*	pBigBlob = (U8*) _shaderCode;	// Actually a giant blob...
// 	return LoadBinaryBlobFromAggregate( pBigBlob, _pEntryPoint );
// }
// 
// #endif	// #ifdef _DEBUG

bool	Shader::Use() {
	if ( HasErrors() )
		return false;	// Can't use a shader in error state!

	if ( !Lock() )
		return false;	// Someone else is locking it !

	ASSERT( m_vertexLayout != NULL, "Can't use a material with an invalid vertex layout!" );

	m_device.DXContext().IASetInputLayout( m_vertexLayout );
	m_device.DXContext().VSSetShader( m_pVS, NULL, 0 );
	m_device.DXContext().HSSetShader( m_pHS, NULL, 0 );
	m_device.DXContext().DSSetShader( m_pDS, NULL, 0 );
	m_device.DXContext().GSSetShader( m_pGS, NULL, 0 );
	m_device.DXContext().PSSetShader( m_pPS, NULL, 0 );

	m_device.m_pCurrentMaterial = this;

	Unlock();

	return true;
}
 
// HRESULT	Shader::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes ) {
// 	if ( m_fileServer != NULL )
// 		return m_fileServer->Open( _IncludeType, _pFileName, _pParentData, _ppData, _pBytes );
// 
// #ifndef GODCOMPLEX
// 	const char**	ppShaderPath = m_Pointer2FileName.Get( U32(_pParentData) );
// 	ASSERT( ppShaderPath != NULL, "Failed to retrieve data pointer!" );
// 	const char*	pShaderPath = *ppShaderPath;
// 
// 	char	pFullName[4096];
// 	sprintf_s( pFullName, 4096, "%s%s", pShaderPath, _pFileName );
// 
// 	FILE*	pFile;
// 	fopen_s( &pFile, pFullName, "rb" );
// 	ASSERT( pFile != NULL, "Include file not found!" );
// 
// 	fseek( pFile, 0, SEEK_END );
// 	U32	Size = ftell( pFile );
// 	fseek( pFile, 0, SEEK_SET );
// 
// 	char*	pBuffer = new char[Size];
// 	fread_s( pBuffer, Size, 1, Size, pFile );
// //	pBuffer[Size] = '\0';
// 
// 	*_pBytes = Size;
// 	*_ppData = pBuffer;
// 
// 	fclose( pFile );
// 
// 	// Register this shader's path as attached to the data pointer
// 	const char*	pIncludedShaderPath = GetShaderPath( pFullName );
// 	m_Pointer2FileName.Add( U32(*_ppData), pIncludedShaderPath );
// #else
// 	ASSERT( false, "You MUST provide an ID3DINCLUDE override when compiling with the GODCOMPLEX option!" );
// #endif
// 
// 	return S_OK;
// }
// 
// HRESULT	Shader::Close( THIS_ LPCVOID _pData ) {
// 	if ( m_fileServer != NULL )
// 		return m_fileServer->Close( _pData );
// 
// #ifndef GODCOMPLEX
// 	// Remove entry from dictionary
// 	const char**	ppShaderPath = m_Pointer2FileName.Get( U32(_pData) );
// 	ASSERT( ppShaderPath != NULL, "Failed to retrieve data pointer!" );
// 	delete[] *ppShaderPath;
// 	m_Pointer2FileName.Remove( U32(_pData) );
// 
// 	// Delete file content
// 	delete[] _pData;
// #endif
// 
// 	return S_OK;
// }

void	Shader::SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();
	m_device.DXContext().VSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pHS != NULL )
		m_device.DXContext().HSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pDS != NULL )
		m_device.DXContext().DSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pGS != NULL )
		m_device.DXContext().GSSetConstantBuffers( _BufferSlot, 1, &pBuffer );
	if ( m_pPS != NULL )
		m_device.DXContext().PSSetConstantBuffers( _BufferSlot, 1, &pBuffer );

	Unlock();
}

void	Shader::SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	m_device.DXContext().VSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pHS != NULL )
		m_device.DXContext().HSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pDS != NULL )
		m_device.DXContext().DSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pGS != NULL )
		m_device.DXContext().GSSetShaderResources( _BufferSlot, 1, &_pData );
	if ( m_pPS != NULL )
		m_device.DXContext().PSSetShaderResources( _BufferSlot, 1, &_pData );

	Unlock();
}

bool	Shader::Lock() const {
	#ifdef MATERIAL_COMPILE_THREADED
		return WaitForSingleObject( m_hCompileMutex, 0 ) == WAIT_OBJECT_0;
	#else
		return true;
	#endif
}
void	Shader::Unlock() const {
	#ifdef MATERIAL_COMPILE_THREADED
		ASSERT( ReleaseMutex( m_hCompileMutex ), "Failed to release mutex!" );
	#endif
}

// const char*	Shader::CopyString( const char* _shaderFileName ) const {
// 	if ( _shaderFileName == NULL )
// 		return NULL;
// 
// 	int		Length = int( strlen(_shaderFileName)+1 );
// 	char*	pResult = new char[Length];
// 	memcpy( pResult, _shaderFileName, Length );
// 
// 	return pResult;
// }


// When compiling normally (i.e. not for the GodComplex 64K intro), allow strings to access shader variables
//
#ifdef ENABLE_SHADER_REFLECTION

bool	Shader::SetConstantBuffer( const BString& _pBufferName, ConstantBuffer& _Buffer ) {
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = true;
	if ( m_vertexLayout != NULL )
	{
		bUsed = false;
		ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();

		{
			int	SlotIndex = m_VSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().VSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_HSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().HSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_DSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().DSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_GSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().GSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_PSConstants.GetConstantBufferIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().PSSetConstantBuffers( SlotIndex, 1, &pBuffer );
			bUsed |= SlotIndex != -1;
		}
	}

	Unlock();

	return	bUsed;
}

bool	Shader::SetTexture( const BString& _pBufferName, ID3D11ShaderResourceView* _pData ) {
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = true;
	if ( m_vertexLayout != NULL )
	{
		bUsed = false;
		{
			int	SlotIndex = m_VSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().VSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_HSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().HSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_DSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().DSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_GSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().GSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
		{
			int	SlotIndex = m_PSConstants.GetShaderResourceViewIndex( _pBufferName );
			if ( SlotIndex != -1 )
				m_device.DXContext().PSSetShaderResources( SlotIndex, 1, &_pData );
			bUsed |= SlotIndex != -1;
		}
	}

	Unlock();

	return	bUsed;
}

static void	DeleteBindingDescriptors( int _EntryIndex, Shader::ShaderConstants::BindingDesc*& _pValue, void* _pUserData ) {
	delete _pValue;
}
Shader::ShaderConstants::~ShaderConstants()
{
	m_ConstantBufferName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
	m_TextureName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
}

void	Shader::ShaderConstants::Enumerate( ID3DBlob& _ShaderBlob ) {
	ID3D11ShaderReflection*	pReflector = NULL; 
throw "Reflection doesn't work because of a link error with IID_ID3D11ShaderReflection!";
//	D3DReflect( _ShaderBlob.GetBufferPointer(), _ShaderBlob.GetBufferSize(), IID_ID3D11ShaderReflection, (void**) &pReflector );

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
	}

	pReflector->Release();
}

void	Shader::ShaderConstants::BindingDesc::SetName( const BString& _pName ) {
	int		NameLength = int( strlen(_pName)+1 );
	pName = new char[NameLength];
	strcpy_s( pName, NameLength+1, _pName );
}
Shader::ShaderConstants::BindingDesc::~BindingDesc() {
//	delete[] pName;	// This makes a heap corruption, I don't know why and I don't give a fuck about these C++ problems... (certainly some shit about allocating memory from a DLL and releasing it from another one or something like this) (which I don't give a shit about either BTW)
}

int		Shader::ShaderConstants::GetConstantBufferIndex( const BString& _pBufferName ) const {
	BindingDesc**	ppValue = m_ConstantBufferName2Descriptor.Get( _pBufferName );
	return ppValue != NULL ? (*ppValue)->Slot : -1;
}

int		Shader::ShaderConstants::GetShaderResourceViewIndex( const BString& _pTextureName ) const {
	BindingDesc**	ppValue = m_TextureName2Descriptor.Get( _pTextureName );
	return ppValue != NULL ? (*ppValue)->Slot : -1;
}

#endif	// defined(ENABLE_SHADER_REFLECTION)


//////////////////////////////////////////////////////////////////////////
// Shader rebuild on modifications mechanism...
#if defined(_DEBUG) || !defined(GODCOMPLEX)

#include <sys/types.h>
#include <sys/stat.h>
#include <timeapi.h>

BaseLib::DictionaryString<Shader*>	Shader::ms_WatchedShaders;

static void	WatchShader( int _EntryIndex, Shader*& _Value, void* _pUserData )	{ _Value->WatchShaderModifications(); }

void	Shader::WatchShadersModifications() {
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

void	Shader::WatchShaderModifications() {
	if ( !Lock() )
		return;	// Someone else is locking it !

	// Check if the shader file changed since last time
	time_t	lastModificationTime = GetFileModTime( m_shaderFileName );
	if ( lastModificationTime <= m_LastShaderModificationTime ) {
		// No change !
		Unlock();
		return;
	}

	#ifdef MATERIAL_COMPILE_THREADED

		m_LastShaderModificationTime = lastModificationTime;

		// We're up to date
		Unlock();

		ASSERT( m_hCompileThread == 0, "Compilation thread already exists!" );

		DWORD	ThreadID;
		m_hCompileThread = CreateThread( NULL, 0, (LPTHREAD_START_ROUTINE) ThreadCompileMaterial, this, 0, &ThreadID );
		SetThreadPriority( m_hCompileThread, THREAD_PRIORITY_HIGHEST );

// WARNING: I'm closing the method and opening a new one here!
//			(I know that's fugly!)
	}

	void		Shader::RebuildShader() {
		DWORD	ErrorCode = WaitForSingleObject( m_hCompileMutex, 30000 );
		#ifdef _DEBUG
			ASSERT( ErrorCode == WAIT_OBJECT_0, "Failed shader rebuild after 30 seconds waiting for access!" );
		#else
			if ( ErrorCode != WAIT_OBJECT_0 )
				ExitProcess( -1 );	// Brutal fail!
		#endif

	#endif
 
// 	// Reload file
// 	FILE*	pFile = NULL;
// 	fopen_s( &pFile, m_pShaderFileName, "rb" );
// //	ASSERT( pFile != NULL, "Failed to open shader file!" );
// 	if ( pFile == NULL ) {
//		// Failed! Unlock but don't update time stamp so we try again next time...
// 		Unlock();
// 		return;
// 	}
// 
// 	fseek( pFile, 0, SEEK_END );
// 	size_t	FileSize = ftell( pFile );
// 	fseek( pFile, 0, SEEK_SET );
// 
// 	char*	pShaderCode = new char[FileSize+1];
// 	fread_s( pShaderCode, FileSize, 1, FileSize, pFile );
// 	pShaderCode[FileSize] = '\0';
// 
// 	fclose( pFile );

// 	const char*	pShaderCode = NULL;
// 	U32			fileSize = 0;
	HRESULT		error = m_fileServer->Open( D3D_INCLUDE_TYPE::D3D_INCLUDE_LOCAL, m_shaderFileName, NULL, NULL, NULL );
	if ( error != S_OK ) {
		// Failed! Unlock but don't update time stamp so we try again next time...
		Unlock();
		return;
	}

	// Compile
	CompileShaders();

// 	m_fileServer->Close( pShaderCode );

// 	delete[] pShaderCode;

	#ifdef MATERIAL_COMPILE_THREADED
		// Close the thread once we're done !
		if ( m_hCompileThread )
			CloseHandle( m_hCompileThread );
		m_hCompileThread = 0;
	#else
		m_LastShaderModificationTime = lastModificationTime;
	#endif

	// Release the mutex: it's now safe to access the shader !
	Unlock();
}

void		Shader::ForceRecompile() {
	m_LastShaderModificationTime--;	// So we're sure it will be recompiled on next watch!
}

time_t		Shader::GetFileModTime( const BString& _fileName ) {	
	struct _stat statInfo;
	_stat( _fileName, &statInfo );

	return statInfo.st_mtime;
}

#endif	// #if defined(_DEBUG) || !defined(GODCOMPLEX)


#ifdef SAVE_SHADER_BLOB_TO

//////////////////////////////////////////////////////////////////////////
// Load from pre-compiled binary blob (useful for heavy shaders that never change or for final release)
//
Shader*	Shader::CreateFromBinaryBlob( Device& _device, const BString& _shaderFileName, const IVertexFormatDescriptor& _format, D3D_SHADER_MACRO* _macros, const BString& _entryPointVS, const BString& _entryPointHS, const BString& _entryPointDS, const BString& _entryPointGS, const BString& _entryPointPS ) {
	ID3DBlob*	blobVS = !_entryPointVS.IsEmpty() ? ShaderCompiler::LoadPreCompiledShader( DiskFileServer::singleton, _shaderFileName, _macros, _entryPointVS ) : NULL;
	ID3DBlob*	blobHS = !_entryPointHS.IsEmpty() ? ShaderCompiler::LoadPreCompiledShader( DiskFileServer::singleton, _shaderFileName, _macros, _entryPointHS ) : NULL;
	ID3DBlob*	blobDS = !_entryPointDS.IsEmpty() ? ShaderCompiler::LoadPreCompiledShader( DiskFileServer::singleton, _shaderFileName, _macros, _entryPointDS ) : NULL;
	ID3DBlob*	blobGS = !_entryPointGS.IsEmpty() ? ShaderCompiler::LoadPreCompiledShader( DiskFileServer::singleton, _shaderFileName, _macros, _entryPointGS ) : NULL;
	ID3DBlob*	blobPS = !_entryPointPS.IsEmpty() ? ShaderCompiler::LoadPreCompiledShader( DiskFileServer::singleton, _shaderFileName, _macros, _entryPointPS ) : NULL;

	Shader*	result = new Shader( _device, _shaderFileName, _format, blobVS, blobHS, blobDS, blobGS, blobPS );

// No need: the blob was released by the constructor!
// 	pPS->Release();
// 	pGS->Release();
// 	pDS->Release();
// 	pHS->Release();
// 	pVS->Release();

	return result;
}

// void	Shader::BuildMacroSignature( char _pSignature[1024], D3D_SHADER_MACRO* _pMacros ) {
// 	char*	pCurrent = _pSignature;
// 	while ( _pMacros != NULL && _pMacros->Name != NULL ) {
// 		*pCurrent++ = '_';
// 		strcpy_s( pCurrent, 1024-(pCurrent-_pSignature), _pMacros->Name );
// 		pCurrent += strlen( _pMacros->Name );
// 		*pCurrent++ = '=';
// 		strcpy_s( pCurrent, 1024-(pCurrent-_pSignature), _pMacros->Definition );
// 		pCurrent += strlen( _pMacros->Definition );
// 		_pMacros++;
// 	}
// 	*pCurrent = '\0';
// }
// 
// void	Shader::SaveBinaryBlob( const String& _shaderFileName, D3D_SHADER_MACRO* _pMacros, const String& _entryPoint, ID3DBlob& _Blob ) {
// 	ASSERT( _shaderFileName.IsEmpty(), "Can't save binary blob => Empty shader file name!" );
// 	ASSERT( !_entryPoint.IsEmpty(), "Can't save binary blob => Empty entry point name!" );
// 
// 	// Build unique macros signature
// 	char	pMacrosSignature[1024];
// 	BuildMacroSignature( pMacrosSignature, _pMacros );
// 
// 	// Build filename
// 	const char*	pFileName = strrchr( _shaderFileName, '/' );
// 	if ( pFileName == NULL )
// 		pFileName = strrchr( _shaderFileName, '\\' );
// 	ASSERT( pFileName != NULL, "Can't retrieve last '/'!" );
// 	int		FileNameIndex = int( 1+pFileName - _shaderFileName );
// 
// 	const char*	pExtension = strrchr( _shaderFileName, '.' );
// 	ASSERT( pExtension != NULL, "Can't retrieve extension!" );
// 	int		ExtensionIndex = int( pExtension - _shaderFileName );
// 
// 	char	pShaderPath[1024];
// 	memcpy( pShaderPath, _shaderFileName, FileNameIndex );
// 	pShaderPath[FileNameIndex] = '\0';	// End the path name here
// 
// 	char	pFileNameWithoutExtension[1024];
// 	memcpy( pFileNameWithoutExtension, pFileName+1, ExtensionIndex-FileNameIndex );
// 	pFileNameWithoutExtension[ExtensionIndex-FileNameIndex] = '\0';	// End the file name here
// 
// 	char	pFinalShaderName[1024];
// //	sprintf_s( pFinalShaderName, 1024, "%s%s%s.%s.fxbin", SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, pMacrosSignature, _pEntryPoint );
// 	sprintf_s( pFinalShaderName, 1024, "%s%s%s%s.%s.fxbin", pShaderPath, SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, pMacrosSignature, _entryPoint );
// 
// 	// Create the binary file
// 	FILE*	pFile;
// 	fopen_s( &pFile, pFinalShaderName, "wb" );
// 	ASSERT( pFile != NULL, "Can't create binary shader file!" );
// 
// 	// Write the entry point's length
// 	int	Length = int( strlen( _entryPoint )+1 );
// 	fwrite( &Length, sizeof(int), 1, pFile );
// 
// 	// Write the entry point name
// 	fwrite( _entryPoint, 1, Length, pFile );
// 
// 	// Write the blob's length
// 	Length = int( _Blob.GetBufferSize() );
// //	ASSERT( Length < 65536, "Shader length doesn't fit on 16 bits!" );
// 	fwrite( &Length, sizeof(int), 1, pFile );
// 
// 	// Write the blob's content
// 	LPCVOID	pCodePointer = _Blob.GetBufferPointer();
// 	fwrite( pCodePointer, 1, Length, pFile );
// 
// 	// We're done!
// 	fclose( pFile );
// }
// 
// ID3DBlob*	Shader::LoadBinaryBlob( const char* _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint ) {
// 	ASSERT( _pShaderFileName != NULL, "Can't load binary blob => Invalid shader file name!" );
// 	ASSERT( _pEntryPoint != NULL, "Can't load binary blob => Invalid entry point name!" );
// 
// 	// Build unique macros signature
// 	char	pMacrosSignature[1024];
// 	BuildMacroSignature( pMacrosSignature, _pMacros );
// 
// 	// Build filename
// 	const char*	pFileName = strrchr( _pShaderFileName, '/' );
// 	if ( pFileName == NULL )
// 		pFileName = strrchr( _pShaderFileName, '\\' );
// 	ASSERT( pFileName != NULL, "Can't retrieve last /!" );
// 	int		FileNameIndex = int( 1+pFileName - _pShaderFileName );
// 	const char*	pExtension = strrchr( _pShaderFileName, '.' );
// 	ASSERT( pExtension != NULL, "Can't retrieve extension!" );
// 	int		ExtensionIndex = int( pExtension - _pShaderFileName );
// 
// 	char	pShaderPath[1024];
// 	memcpy( pShaderPath, _pShaderFileName, FileNameIndex );
// 	pShaderPath[FileNameIndex] = '\0';	// End the path name here
// 
// 	char	pFileNameWithoutExtension[1024];
// 	memcpy( pFileNameWithoutExtension, pFileName+1, ExtensionIndex-FileNameIndex );
// 	pFileNameWithoutExtension[ExtensionIndex-FileNameIndex] = '\0';	// End the file name here
// 
// 	char	pFinalShaderName[1024];
// //	sprintf_s( pFinalShaderName, 1024, "%s%s%s.%s.fxbin", SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, pMacrosSignature, _pEntryPoint );
// 	sprintf_s( pFinalShaderName, 1024, "%s%s%s%s.%s.fxbin", pShaderPath, SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, pMacrosSignature, _pEntryPoint );
// 
// 	// Load the binary file
// 	FILE*	pFile;
// 	fopen_s( &pFile, pFinalShaderName, "rb" );
// 	ASSERT( pFile != NULL, "Can't open binary shader file! (did you compile the shader at least once?)" );
// 
// 	// Read the entry point's length
// 	int	Length;
// 	fread_s( &Length, sizeof(int), sizeof(int), 1, pFile );
// 
// 	// Read the entry point name
// 	char	pEntryPointCheck[1024];
// 	fread_s( pEntryPointCheck, 1024, 1, Length, pFile );
// 	ASSERT( !strcmp( _pEntryPoint, pEntryPointCheck ), "Entry point names mismatch!" );
// 
// 	// Read the blob's length
// 	int	BlobSize;
// 	fread_s( &BlobSize, sizeof(int), sizeof(int), 1, pFile );
// 
// 	// Create a D3DBlob
// 	ID3DBlob*	pResult = NULL;
// 	D3DCreateBlob( BlobSize, &pResult );
// 
// 	// Read the blob's content
// 	LPVOID	pContent = pResult->GetBufferPointer();
// 	fread_s( pContent, BlobSize, 1, BlobSize, pFile );
// 
// 	// We're done!
// 	fclose( pFile );
// 
// 	return pResult;
// }

#endif	// #if defined(_DEBUG) && defined(SAVE_SHADER_BLOB_TO)


// ID3DBlob*	Shader::LoadBinaryBlobFromAggregate( const U8* _pAggregate, const char* _pEntryPoint ) {
// 	U16	BlobsCount = *((U16*) _pAggregate); _pAggregate+=2;	// Amount of blobs in the big blob
// 	for ( U16 BlobIndex=0; BlobIndex < BlobsCount; BlobIndex++ ) {
// 		int	Cmp = strcmp( (char*) _pAggregate, _pEntryPoint );
// 		int	BlobEntryPointLength = int( strlen( (char*) _pAggregate ) );
// 		_pAggregate += BlobEntryPointLength+1;	// Skip the entry point's name
// 
// 		if ( !Cmp ) {
// 			// Found it !
// 			U16	BlobStartOffset = *((U16*) _pAggregate); _pAggregate+=2;	// Retrieve the jump offset to reach the blob
// 			_pAggregate += BlobStartOffset;									// Go to the blob descriptor
// 
// 			U16	BlobSize = *((U16*) _pAggregate); _pAggregate+=2;			// Retrieve the size of the blob
// 
// 			// Create a D3DBlob
// 			ID3DBlob*	pResult = NULL;
// 			D3DCreateBlob( BlobSize, &pResult );
// 
// 			// Copy our blob content
// 			void*		pBlobContent = pResult->GetBufferPointer();
// 			memcpy( pBlobContent, _pAggregate, BlobSize );
// 
// 			// Yoohoo!
// 			return pResult;
// 		}
// 
// 		// Not that blob either... Skip the jump offset...
// 		_pAggregate += 2;
// 	}
// 
// 	return NULL;
// }
