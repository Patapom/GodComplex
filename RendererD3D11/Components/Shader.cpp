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
			if ( m_fileServer->Open( D3D_INCLUDE_LOCAL, _shaderFileName, NULL, NULL, NULL ) == S_OK ) {
				// Register as a watched shader
				ms_WatchedShaders.Add( _shaderFileName, this );

				#ifndef MATERIAL_COMPILE_AT_RUNTIME
					m_LastShaderModificationTime = m_fileServer->GetFileModTime( _shaderFileName );
				#endif
			} else {
				ASSERT( false, "Shader file not found => You can ignore this assert but shader file will NOT be watched for modification!" );
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
	if ( _blobVS == NULL ) {
 		ASSERT( !m_entryPointVS.IsEmpty(), "Invalid VertexShader entry point!" );
		_blobVS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointVS, "vs_5_0" );
		m_hasErrors = _blobVS == NULL;
	}
	if ( _blobVS != NULL ) {
		Check( m_device.DXDevice().CreateVertexShader( _blobVS->GetBufferPointer(), _blobVS->GetBufferSize(), NULL, &pVS ) );
		ASSERT( pVS != NULL, "Failed to create vertex shader!" );
		m_hasErrors |= pVS == NULL;

		if ( !m_hasErrors ) {
			// Create the associated vertex layout
			Check( m_device.DXDevice().CreateInputLayout( m_format.GetInputElements(), m_format.GetInputElementsCount(), _blobVS->GetBufferPointer(), _blobVS->GetBufferSize(), &pVertexLayout ) );
			ASSERT( pVertexLayout != NULL, "Failed to create vertex layout!" );
			m_hasErrors |= pVertexLayout == NULL;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional hull shader
	if ( !m_hasErrors ) {
		if ( _blobHS == NULL && !m_entryPointHS.IsEmpty() ) {
			_blobHS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointHS, "hs_5_0" );
			m_hasErrors = _blobHS == NULL;
		}
		if ( _blobHS != NULL ) {
			Check( m_device.DXDevice().CreateHullShader( _blobHS->GetBufferPointer(), _blobHS->GetBufferSize(), NULL, &pHS ) );
			ASSERT( pHS != NULL, "Failed to create hull shader!" );
			m_hasErrors |= pHS == NULL;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional domain shader
	if ( !m_hasErrors ) {
		if ( _blobDS == NULL && !m_entryPointDS.IsEmpty() ) {
			_blobDS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointDS, "ds_5_0" );
			m_hasErrors = _blobDS == NULL;
		}
		if ( _blobDS != NULL ) {
			Check( m_device.DXDevice().CreateDomainShader( _blobDS->GetBufferPointer(), _blobDS->GetBufferSize(), NULL, &pDS ) );
			ASSERT( pDS != NULL, "Failed to create domain shader!" );
			m_hasErrors |= pDS == NULL;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional geometry shader
	if ( !m_hasErrors ) {
		if ( _blobGS == NULL && !m_entryPointGS.IsEmpty() ) {
			_blobGS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointGS, "gs_5_0" );
			m_hasErrors = _blobGS == NULL;
		}
		if ( _blobGS != NULL ) {
			Check( m_device.DXDevice().CreateGeometryShader( _blobGS->GetBufferPointer(), _blobGS->GetBufferSize(), NULL, &pGS ) );
			ASSERT( pGS != NULL, "Failed to create geometry shader!" );
			m_hasErrors |= pGS == NULL;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// Compile the optional pixel shader
	if ( !m_hasErrors ) {
		if ( _blobPS == NULL && !m_entryPointPS.IsEmpty() ) {
// 			#ifdef _DEBUG
// 				// CSO TEST
// 				// We use a special pre-compiled CSO read from file here
// 				if ( m_entryPointPS[0] == 1 ) {
// 					U32	BufferSize = *((U32*) (((const char*) m_entryPointPS)+1));
// 					U8*	pBufferPointer = (U8*) ((const char*) m_entryPointPS)+5;
// 					Check( m_device.DXDevice().CreatePixelShader( pBufferPointer, BufferSize, NULL, &m_pPS ) );
// 					ASSERT( m_pPS != NULL, "Failed to create pixel shader!" );
// 					return;
// 				}
// 				// CSO TEST
// 			#endif

			_blobPS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointPS, "ps_5_0" );
			m_hasErrors = _blobPS == NULL;
		}
		if ( _blobPS != NULL ) {
			Check( m_device.DXDevice().CreatePixelShader( _blobPS->GetBufferPointer(), _blobPS->GetBufferSize(), NULL, &pPS ) );
			ASSERT( pPS != NULL, "Failed to create pixel shader!" );
			m_hasErrors |= pPS == NULL;
		}
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
		pVertexLayout = NULL;
		pVS = NULL;
		pHS = NULL;
		pDS = NULL;
		pGS = NULL;
		pPS = NULL;

		// Enumerate constants
		#ifdef ENABLE_SHADER_REFLECTION
			if ( _blobVS != NULL )
				m_VSConstants.Enumerate( *_blobVS );
			if ( _blobHS != NULL )
				m_HSConstants.Enumerate( *_blobHS );
			if ( _blobDS != NULL )
				m_DSConstants.Enumerate( *_blobDS );
			if ( _blobGS != NULL )
				m_GSConstants.Enumerate( *_blobGS );
			if ( _blobPS != NULL )
				m_PSConstants.Enumerate( *_blobPS );
		#endif
	}

	SAFE_RELEASE( pVertexLayout );
	SAFE_RELEASE( pVS );
	SAFE_RELEASE( pHS );
	SAFE_RELEASE( pDS );
	SAFE_RELEASE( pGS );
	SAFE_RELEASE( pPS );

	SAFE_RELEASE( _blobVS );
	SAFE_RELEASE( _blobHS );
	SAFE_RELEASE( _blobDS );
	SAFE_RELEASE( _blobGS );
	SAFE_RELEASE( _blobPS );
}

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
	time_t	lastModificationTime = m_fileServer->GetFileModTime( m_shaderFileName );
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

	HRESULT		error = m_fileServer->Open( D3D_INCLUDE_TYPE::D3D_INCLUDE_LOCAL, m_shaderFileName, NULL, NULL, NULL );
	if ( error != S_OK ) {
		// Failed! Unlock but don't update time stamp so we try again next time...
		Unlock();
		return;
	}

	// Compile
	CompileShaders();

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

#endif	// #if defined(_DEBUG) && defined(SAVE_SHADER_BLOB_TO)
