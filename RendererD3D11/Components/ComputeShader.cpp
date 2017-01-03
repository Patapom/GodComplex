#include "stdafx.h"

#include "ComputeShader.h"
#include "ConstantBuffer.h"
#include "StructuredBuffer.h"
#include "..\Utility\FileServer.h"
#include "..\Utility\ShaderCompiler.h"

#include <stdio.h>
#include <io.h>

ComputeShader*	ComputeShader::ms_pCurrentShader = NULL;

ComputeShader::ComputeShader( Device& _device, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint, IFileServer* _fileServerOverride )
	: Component( _device )
	, m_shaderFileName( _shaderFileName )
	, m_hasErrors( false )
	, m_entryPointCS( _entryPoint )
	, m_pCS( NULL )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
	m_fileServer = _fileServerOverride != nullptr ? _fileServerOverride : &DiskFileServer::singleton;

	#if defined(_DEBUG) && defined(WATCH_SHADER_MODIFICATIONS)
		if ( !m_shaderFileName.IsEmpty() ) {
			// Just ensure the file exists !
			if ( m_fileServer->Open( D3D_INCLUDE_LOCAL, _shaderFileName, NULL, NULL, NULL ) == S_OK ) {
				// Register as a watched shader
				ms_WatchedShaders.Add( _shaderFileName, this );

				#ifndef COMPUTE_SHADER_COMPILE_AT_RUNTIME
					m_LastShaderModificationTime = m_fileServer->GetFileModTime( _shaderFileName );
				#endif
			} else {
				ASSERT( false, "Compute Shader file not found => You can ignore this assert but compute shader file will NOT be watched for modification!" );
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

	#ifdef COMPUTE_SHADER_COMPILE_THREADED
		// Create the mutex for compilation exclusivity
		m_hCompileMutex = CreateMutexA( NULL, false, m_shaderFileName );
		if ( m_hCompileMutex == NULL )
			m_hCompileMutex = OpenMutexA( SYNCHRONIZE, false, m_shaderFileName );	// Try and reuse any existing mutex
		ASSERT( m_hCompileMutex != 0, "Failed to create compilation mutex!" );
	#endif

	#ifndef COMPUTE_SHADER_COMPILE_AT_RUNTIME
		#ifdef COMPUTE_SHADER_COMPILE_THREADED
			ASSERT( false, "The COMPUTE_SHADER_COMPILE_THREADED option should only work in pair with the COMPUTE_SHADER_COMPILE_AT_RUNTIME option! (i.e. You CANNOT define COMPUTE_SHADER_COMPILE_THREADED without defining COMPUTE_SHADER_COMPILE_AT_RUNTIME at the same time!)" );
		#endif

		// Compile immediately
		CompileShader();
	#endif
}

ComputeShader::ComputeShader( Device& _Device, const BString& _shaderFileName, ID3DBlob* _blobCS )
	: Component( _Device )
	, m_pCS( NULL )
	, m_shaderFileName( _shaderFileName )
	, m_fileServer( NULL )
	, m_macros( NULL )
	, m_hasErrors( false )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
	ASSERT( _blobCS != NULL, "You can't provide a NULL CS blob!" );
	CompileShader( _blobCS );
}

ComputeShader::~ComputeShader() {
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	// Destroy mutex
	CloseHandle( m_hCompileMutex );
#endif

#ifdef _DEBUG
	// Unregister as a watched shader
	if ( !m_shaderFileName.IsEmpty() ) {
		ms_WatchedShaders.Remove( m_shaderFileName );
	}
#endif

	SAFE_RELEASE( m_pCS );
	SAFE_DELETE_ARRAY( m_macros );
}

void	ComputeShader::CompileShader( ID3DBlob* _blobCS ) {
	m_hasErrors = false;

	ID3D11ComputeShader*	pCS = NULL;

	//////////////////////////////////////////////////////////////////////////
	// Compile the compute shader
	if ( _blobCS == nullptr ) {
		ASSERT( !m_entryPointCS.IsEmpty(), "Invalid ComputeShader entry point!" );
		_blobCS = ShaderCompiler::CompileShader( *m_fileServer, m_shaderFileName, m_macros, m_entryPointCS, "cs_5_0", true );
		m_hasErrors = _blobCS == NULL;
	}
	if ( _blobCS != NULL ) {
		Check( m_device.DXDevice().CreateComputeShader( _blobCS->GetBufferPointer(), _blobCS->GetBufferSize(), NULL, &pCS ) );
		ASSERT( pCS != NULL, "Failed to create compute shader!" );
		m_hasErrors |= pCS == NULL;
	}

	//////////////////////////////////////////////////////////////////////////
	// Only replace actual member pointers once everything compiled successfully
	if ( !m_hasErrors ) {
		// Release any pre-existing shader
		SAFE_RELEASE( m_pCS );

		// Replace with brand new shaders
		m_pCS = pCS;
		pCS = NULL;

		// Enumerate constants
		#ifdef ENABLE_SHADER_REFLECTION
			m_CSConstants.Enumerate( *m_pCS );
		#endif
	}

	SAFE_RELEASE( pCS );
	SAFE_RELEASE( _blobCS );
}

bool	ComputeShader::Use() {
	if ( !Lock() )
		return false;	// Someone else is locking it !

	m_device.DXContext().CSSetShader( m_pCS, NULL, 0 );
	ms_pCurrentShader = this;

	Unlock();

	return true;
}

void	ComputeShader::Dispatch( U32 _GroupsCountX, U32 _GroupsCountY, U32 _GroupsCountZ ) {
	ASSERT( ms_pCurrentShader == this, "You must call Use() before calling Run() on a ComputeShader!" );
	if ( !Lock() )
		return;	// Someone else is locking it !

	m_device.DXContext().Dispatch( _GroupsCountX, _GroupsCountY, _GroupsCountZ );

	Unlock();
}

void	ComputeShader::SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer ) {
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();
	m_device.DXContext().CSSetConstantBuffers( _BufferSlot, 1, &pBuffer );

	Unlock();
}

void	ComputeShader::SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData ) {
	if ( !Lock() )
		return;	// Someone else is locking it !

	m_device.DXContext().CSSetShaderResources( _BufferSlot, 1, &_pData );

	Unlock();
}

void	ComputeShader::SetStructuredBuffer( int _BufferSlot, StructuredBuffer& _Buffer ) {
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11ShaderResourceView*	pView = _Buffer.GetShaderView();
	m_device.DXContext().CSSetShaderResources( _BufferSlot, 1, &pView );

	Unlock();
}

void	ComputeShader::SetUnorderedAccessView( int _BufferSlot, StructuredBuffer& _Buffer ) {
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11UnorderedAccessView*	pUAV = _Buffer.GetUnorderedAccessView();
	U32							UAVInitCount = -1;
	m_device.DXContext().CSSetUnorderedAccessViews( _BufferSlot, 1, &pUAV, &UAVInitCount );

	Unlock();
}

bool	ComputeShader::Lock() const {
	#ifdef COMPUTE_SHADER_COMPILE_THREADED
		return WaitForSingleObject( m_hCompileMutex, 0 ) == WAIT_OBJECT_0;
	#else
		return true;
	#endif
}
void	ComputeShader::Unlock() const {
	#ifdef COMPUTE_SHADER_COMPILE_THREADED
		ASSERT( ReleaseMutex( m_hCompileMutex ), "Failed to release mutex!" );
	#endif
}

// When compiling normally (i.e. not for the GodComplex 64K intro), allow strings to access shader variables
//
#ifdef ENABLE_SHADER_REFLECTION

bool	ComputeShader::SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer ) {
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = false;
	ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();

	{
		int	SlotIndex = m_CSConstants.GetConstantBufferIndex( _pBufferName );
		if ( SlotIndex != -1 )
			m_device.DXContext().CSSetConstantBuffers( SlotIndex, 1, &pBuffer );
		bUsed |= SlotIndex != -1;
	}

	Unlock();

	return	bUsed;
}

bool	ComputeShader::SetTexture( const char* _pTextureName, ID3D11ShaderResourceView* _pData ) {
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = false;
	{
		int	SlotIndex = m_CSConstants.GetShaderResourceViewIndex( _pTextureName );
		if ( SlotIndex != -1 )
			m_device.DXContext().CSSetShaderResources( SlotIndex, 1, &_pData );
		bUsed |= SlotIndex != -1;
	}

	Unlock();

	return	bUsed;
}

bool	ComputeShader::SetStructuredBuffer( const char* _pBufferName, StructuredBuffer& _Buffer ) {
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = false;
	{
		int	SlotIndex = m_CSConstants.GetStructuredBufferIndex( _pBufferName );
		if ( SlotIndex != -1 )
		{
			ID3D11ShaderResourceView*	pView = _Buffer.GetShaderView();
			m_device.DXContext().CSSetShaderResources( SlotIndex, 1, &pView );
		}
		bUsed |= SlotIndex != -1;
	}

	Unlock();

	return	bUsed;
}

bool	ComputeShader::SetUnorderedAccessView( const char* _pBufferName, StructuredBuffer& _Buffer ) {
	if ( !Lock() )
		return	true;	// Someone else is locking it !

	bool	bUsed = false;
	{
		int	SlotIndex = m_CSConstants.GetUnorderedAccesViewIndex( _pBufferName );
		if ( SlotIndex != -1 )
		{
			ID3D11UnorderedAccessView*	pUAV = _Buffer.GetUnorderedAccessView();
			U32							UAVInitCount = -1;
			m_device.DXContext().CSSetUnorderedAccessViews( SlotIndex, 1, &pUAV, &UAVInitCount );
		}

		bUsed |= SlotIndex != -1;
	}

	Unlock();

	return	bUsed;
}

static void	DeleteBindingDescriptors( int _EntryIndex, ComputeShader::ShaderConstants::BindingDesc*& _pValue, void* _pUserData ) {
	delete _pValue;
}
ComputeShader::ShaderConstants::~ShaderConstants() {
	m_ConstantBufferName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
	m_TextureName2Descriptor.ForEach( DeleteBindingDescriptors, NULL );
}

void	ComputeShader::ShaderConstants::Enumerate( ID3DBlob& _ShaderBlob ) {
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

		case D3D_SIT_STRUCTURED:
			ppDesc = &m_StructuredBufferName2Descriptor.Add( BindDesc.Name );
			break;

		case D3D_SIT_UAV_RWTYPED:
		case D3D_SIT_UAV_RWSTRUCTURED:
		case D3D_SIT_UAV_RWBYTEADDRESS:
		case D3D_SIT_UAV_APPEND_STRUCTURED:
		case D3D_SIT_UAV_CONSUME_STRUCTURED:
		case D3D_SIT_UAV_RWSTRUCTURED_WITH_COUNTER:
			ppDesc = &m_UAVName2Descriptor.Add( BindDesc.Name );
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

void	ComputeShader::ShaderConstants::BindingDesc::SetName( const BString& _name ) {
	int		NameLength = int( strlen(_name)+1 );
	pName = new char[NameLength];
	strcpy_s( pName, NameLength+1, _name );
}
ComputeShader::ShaderConstants::BindingDesc::~BindingDesc() {
//	delete[] pName;	// This makes a heap corruption, I don't know why and I don't give a fuck about these C++ problems... (certainly some shit about allocating memory from a DLL and releasing it from another one or something like this) (which I don't give a shit about either BTW)
}

int		ComputeShader::ShaderConstants::GetConstantBufferIndex( const BString& _pBufferName ) const {
	BindingDesc**	ppValue = m_ConstantBufferName2Descriptor.Get( _pBufferName );
	return ppValue != NULL ? (*ppValue)->Slot : -1;
}

int		ComputeShader::ShaderConstants::GetShaderResourceViewIndex( const BString& _pTextureName ) const {
	BindingDesc**	ppValue = m_TextureName2Descriptor.Get( _pTextureName );
	return ppValue != NULL ? (*ppValue)->Slot : -1;
}

int		ComputeShader::ShaderConstants::GetStructuredBufferIndex( const BString& _pUAVName ) const {
	BindingDesc**	ppValue = m_StructuredBufferName2Descriptor.Get( _pUAVName );
	return ppValue != NULL ? (*ppValue)->Slot : -1;
}

int		ComputeShader::ShaderConstants::GetUnorderedAccesViewIndex( const BString& _pUAVName ) const {
	BindingDesc**	ppValue = m_UAVName2Descriptor.Get( _pUAVName );
	return ppValue != NULL ? (*ppValue)->Slot : -1;
}

#endif	// #ifdef ENABLE_SHADER_REFLECTION


//////////////////////////////////////////////////////////////////////////
// Shader rebuild on modifications mechanism...
#if defined(_DEBUG) || !defined(GODCOMPLEX)

#include <sys/types.h>
#include <sys/stat.h>
#include <timeapi.h>

BaseLib::DictionaryString<ComputeShader*>	ComputeShader::ms_WatchedShaders;

static bool	WatchShader( int _EntryIndex, const BString& _key, ComputeShader*& _value, void* _pUserData )	{ _value->WatchShaderModifications(); return true; }

void		ComputeShader::WatchShadersModifications() {
	static int	LastTime = -1;
	int			CurrentTime = timeGetTime();
	if ( LastTime >= 0 && (CurrentTime - LastTime) < COMPUTE_SHADER_REFRESH_CHANGES_INTERVAL )
		return;	// Too soon to check !

	// Update last check time
	LastTime = CurrentTime;

	ms_WatchedShaders.ForEach( WatchShader, NULL );
}

#ifdef COMPUTE_SHADER_COMPILE_THREADED
void	ThreadCompileComputeShader( void* _pData ) {
	ComputeShader*	pMaterial = (ComputeShader*) _pData;
	pMaterial->RebuildShader();
}
#endif

void		ComputeShader::WatchShaderModifications() {
	if ( !Lock() )
		return;	// Someone else is locking it !

	// Check if the shader file changed since last time
	time_t	LastModificationTime = m_fileServer->GetFileModTime( m_shaderFileName );
	if ( LastModificationTime <= m_LastShaderModificationTime ) {
		// No change !
		Unlock();
		return;
	}

	m_LastShaderModificationTime = LastModificationTime;

	// We're up to date
	Unlock();

#ifdef COMPUTE_SHADER_COMPILE_THREADED
	ASSERT( m_hCompileThread == 0, "Compilation thread already exists!" );

	DWORD	ThreadID;
    m_hCompileThread = CreateThread( NULL, 0, (LPTHREAD_START_ROUTINE) ThreadCompileComputeShader, this, 0, &ThreadID );
    SetThreadPriority( m_hCompileThread, THREAD_PRIORITY_HIGHEST );
}

void		ComputeShader::RebuildShader()
{
	DWORD	ErrorCode = WaitForSingleObject( m_hCompileMutex, 30000 );
#ifdef _DEBUG
	ASSERT( ErrorCode == WAIT_OBJECT_0, "Failed shader rebuild after 30 seconds waiting for access!" );
#else
	if ( ErrorCode != WAIT_OBJECT_0 )
		ExitProcess( -1 );	// Failed !
#endif
#endif

	HRESULT		error = m_fileServer->Open( D3D_INCLUDE_TYPE::D3D_INCLUDE_LOCAL, m_shaderFileName, NULL, NULL, NULL );
	if ( error != S_OK ) {
		// Failed! Unlock but don't update time stamp so we try again next time...
		Unlock();
		return;
	}

	// Compile
	CompileShader();

	// Release the mutex: it's now safe to access the shader !
	Unlock();

#ifdef COMPUTE_SHADER_COMPILE_THREADED
	// Close the thread once we're done !
	if ( m_hCompileThread )
		CloseHandle( m_hCompileThread );
	m_hCompileThread = 0;
#endif
}

void		ComputeShader::ForceRecompile() {
	m_LastShaderModificationTime++;	// So we're sure it will be recompiled on next watch!
}

#endif	// #if defined(_DEBUG) || !defined(GODCOMPLEX)

ComputeShader*	ComputeShader::CreateFromBinaryBlob( Device& _device, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint ) {
	ASSERT( !_entryPoint.IsEmpty(), "You must provide a valid entry point!" );

	ID3DBlob*	blobCS = ShaderCompiler::LoadPreCompiledShader( DiskFileServer::singleton, _shaderFileName, _macros, _entryPoint );

	ComputeShader*	pResult = new ComputeShader( _device, _shaderFileName, blobCS );

// No need: the blob was released by the constructor!
//	pCS->Release();

	return pResult;
}
