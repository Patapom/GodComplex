
#include "ComputeShader.h"
#include "ConstantBuffer.h"
#include "StructuredBuffer.h"
#include "Shader.h"

#include <stdio.h>
#include <io.h>

#include <D3Dcompiler.h>
#include <D3D11Shader.h>

ComputeShader*	ComputeShader::ms_pCurrentShader = NULL;

ComputeShader::ComputeShader( Device& _Device, const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, ID3DInclude* _pIncludeOverride )
	: Component( _Device )
	, m_pCS( NULL )
	, m_pShaderPath( NULL )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{
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
		ASSERT( pFile != NULL, "Compute Shader file not found => You can ignore this assert but compute shader file will NOT be watched for modification!" );
		fclose( pFile );

		// Register as a watched shader
		ms_WatchedShaders.Add( _pShaderFileName, this );

#ifndef COMPUTE_SHADER_COMPILE_AT_RUNTIME
		m_LastShaderModificationTime = GetFileModTime( _pShaderFileName );
#endif
	}
#endif

	m_pEntryPointCS = _pEntryPoint;

	if ( _pMacros != NULL )
	{
		D3D_SHADER_MACRO*	pMacro = _pMacros;
		while ( pMacro->Name != NULL )
			pMacro++;

		int	MacrosCount = int( 1 + pMacro - _pMacros );
		m_pMacros = new D3D_SHADER_MACRO[MacrosCount];
		memcpy( m_pMacros, _pMacros, MacrosCount*sizeof(D3D_SHADER_MACRO) );
	}
	else
		m_pMacros = NULL;

#ifdef COMPUTE_SHADER_COMPILE_THREADED
	// Create the mutex for compilation exclusivity
	m_hCompileMutex = CreateMutexA( NULL, false, m_pShaderFileName );
	if ( m_hCompileMutex == NULL )
		m_hCompileMutex = OpenMutexA( SYNCHRONIZE, false, m_pShaderFileName );	// Try and reuse any existing mutex
	ASSERT( m_hCompileMutex != 0, "Failed to create compilation mutex!" );
#endif

#ifndef COMPUTE_SHADER_COMPILE_AT_RUNTIME
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	ASSERT( false, "The COMPUTE_SHADER_COMPILE_THREADED option should only work in pair with the COMPUTE_SHADER_COMPILE_AT_RUNTIME option! (i.e. You CANNOT define COMPUTE_SHADER_COMPILE_THREADED without defining COMPUTE_SHADER_COMPILE_AT_RUNTIME at the same time!)" );
#endif

	// Compile immediately
	CompileShaders( _pShaderCode );
#endif
}

ComputeShader::ComputeShader( Device& _Device, const char* _pShaderFileName, ID3DBlob* _pCS )
	: Component( _Device )
	, m_pCS( NULL )
	, m_pShaderPath( NULL )
	, m_pIncludeOverride( NULL )
	, m_bHasErrors( false )
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	, m_LastShaderModificationTime( 0 )
#endif
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	, m_hCompileThread( 0 )
#endif
{

#if defined(_DEBUG) || !defined(GODCOMPLEX)
	m_pShaderFileName = CopyString( _pShaderFileName );
#endif
#ifndef GODCOMPLEX
	m_pShaderPath = GetShaderPath( _pShaderFileName );
	m_Pointer2FileName.Add( NULL, m_pShaderPath );
#endif

	ASSERT( _pCS != NULL, "You can't provide a NULL CS blob!" );

	CompileShaders( NULL, _pCS );
}

ComputeShader::~ComputeShader()
{
#ifdef COMPUTE_SHADER_COMPILE_THREADED
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
	if ( m_pCS != NULL ) { m_pCS->Release(); m_pCS = NULL; }
	if ( m_pMacros != NULL ) { delete[] m_pMacros; m_pMacros = NULL; }
}

void	ComputeShader::CompileShaders( const char* _pShaderCode, ID3DBlob* _pCS ) {

	//////////////////////////////////////////////////////////////////////////
	// Compile the compute shader
	ASSERT( _pCS != NULL || m_pEntryPointCS != NULL, "Invalid ComputeShader entry point!" );
	ID3DBlob*   pShader = _pCS == NULL ? Shader::CompileShader( m_pShaderFileName, _pShaderCode, m_pMacros, m_pEntryPointCS, "cs_5_0", this, true ) : _pCS;
	if ( pShader == NULL ) {
		m_bHasErrors = true;
		return;
	}

	ID3D11ComputeShader*	tempCS = NULL;
	Check( m_Device.DXDevice().CreateComputeShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &tempCS ) );

	if ( tempCS != NULL ) {
		// SUCCESS! Replace existing shader!
		if ( m_pCS != NULL )
			m_pCS->Release();	// Release any pre-existing shader

		m_pCS = tempCS;

		#ifdef ENABLE_SHADER_REFLECTION
			m_CSConstants.Enumerate( *pShader );
		#endif

		m_bHasErrors = false;	// Not in error state anymore
	} else {
		// ERROR! Don't replace existing shader until errors are fixed...
		m_bHasErrors = true;
		ASSERT( false, "Failed to create compute shader!" );
	}

	pShader->Release();	// Release shader anyway
}

bool	ComputeShader::Use()
{
	if ( !Lock() )
		return false;	// Someone else is locking it !

	m_Device.DXContext().CSSetShader( m_pCS, NULL, 0 );
	ms_pCurrentShader = this;

	Unlock();

	return true;
}

void	ComputeShader::Dispatch( int _GroupsCountX, int _GroupsCountY, int _GroupsCountZ )
{
	ASSERT( ms_pCurrentShader == this, "You must call Use() before calling Run() on a ComputeShader!" );
	if ( !Lock() )
		return;	// Someone else is locking it !

	m_Device.DXContext().Dispatch( _GroupsCountX, _GroupsCountY, _GroupsCountZ );

	Unlock();
}
 
HRESULT	ComputeShader::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes )
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

HRESULT	ComputeShader::Close( THIS_ LPCVOID _pData )
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

void	ComputeShader::SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();
	m_Device.DXContext().CSSetConstantBuffers( _BufferSlot, 1, &pBuffer );

	Unlock();
}

void	ComputeShader::SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	m_Device.DXContext().CSSetShaderResources( _BufferSlot, 1, &_pData );

	Unlock();
}

void	ComputeShader::SetStructuredBuffer( int _BufferSlot, StructuredBuffer& _Buffer )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11ShaderResourceView*	pView = _Buffer.GetShaderView();
	m_Device.DXContext().CSSetShaderResources( _BufferSlot, 1, &pView );

	Unlock();
}

void	ComputeShader::SetUnorderedAccessView( int _BufferSlot, StructuredBuffer& _Buffer )
{
	if ( !Lock() )
		return;	// Someone else is locking it !

	ID3D11UnorderedAccessView*	pUAV = _Buffer.GetUnorderedAccessView();
	U32							UAVInitCount = -1;
	m_Device.DXContext().CSSetUnorderedAccessViews( _BufferSlot, 1, &pUAV, &UAVInitCount );

	Unlock();
}

bool	ComputeShader::Lock() const
{
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	return WaitForSingleObject( m_hCompileMutex, 0 ) == WAIT_OBJECT_0;
#else
	return true;
#endif
}
void	ComputeShader::Unlock() const
{
#ifdef COMPUTE_SHADER_COMPILE_THREADED
	ASSERT( ReleaseMutex( m_hCompileMutex ), "Failed to release mutex !" );
#endif
}

const char*	ComputeShader::CopyString( const char* _pShaderFileName ) const
{
	if ( _pShaderFileName == NULL )
		return NULL;

	int		Length = int( strlen(_pShaderFileName)+1 );
	char*	pResult = new char[Length];
	memcpy( pResult, _pShaderFileName, Length );

	return pResult;
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
			m_Device.DXContext().CSSetConstantBuffers( SlotIndex, 1, &pBuffer );
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
			m_Device.DXContext().CSSetShaderResources( SlotIndex, 1, &_pData );
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
			m_Device.DXContext().CSSetShaderResources( SlotIndex, 1, &pView );
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
			m_Device.DXContext().CSSetUnorderedAccessViews( SlotIndex, 1, &pUAV, &UAVInitCount );
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
#ifdef __DEBUG_UPLOAD_ONLY_ONCE
		(*ppDesc)->bUploaded = false;	// Not uploaded yet !
#endif
	}

	pReflector->Release();
}

void	ComputeShader::ShaderConstants::BindingDesc::SetName( const char* _pName ) {
	int		NameLength = int( strlen(_pName)+1 );
	pName = new char[NameLength];
	strcpy_s( pName, NameLength+1, _pName );
}
ComputeShader::ShaderConstants::BindingDesc::~BindingDesc() {
//	delete[] pName;	// This makes a heap corruption, I don't know why and I don't give a fuck about these C++ problems... (certainly some shit about allocating memory from a DLL and releasing it from another one or something like this) (which I don't give a shit about either BTW)
}

int		ComputeShader::ShaderConstants::GetConstantBufferIndex( const char* _pBufferName ) const {
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

int		ComputeShader::ShaderConstants::GetShaderResourceViewIndex( const char* _pTextureName ) const {
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

int		ComputeShader::ShaderConstants::GetStructuredBufferIndex( const char* _pUAVName ) const {
	BindingDesc**	ppValue = m_StructuredBufferName2Descriptor.Get( _pUAVName );

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

int		ComputeShader::ShaderConstants::GetUnorderedAccesViewIndex( const char* _pUAVName ) const {
	BindingDesc**	ppValue = m_UAVName2Descriptor.Get( _pUAVName );

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

#endif	// #ifdef ENABLE_SHADER_REFLECTION

const char*	ComputeShader::GetShaderPath( const char* _pShaderFileName ) const {
	char*	pResult = NULL;
	if ( _pShaderFileName != NULL )
	{
		int	FileNameLength = int( strlen(_pShaderFileName)+1 );
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


//////////////////////////////////////////////////////////////////////////
// Shader rebuild on modifications mechanism...
#if defined(_DEBUG) || !defined(GODCOMPLEX)

#include <sys/types.h>
#include <sys/stat.h>

DictionaryString<ComputeShader*>	ComputeShader::ms_WatchedShaders;

static void	WatchShader( int _EntryIndex, ComputeShader*& _Value, void* _pUserData )	{ _Value->WatchShaderModifications(); }

void		ComputeShader::WatchShadersModifications()
{
	static int	LastTime = -1;
	int			CurrentTime = timeGetTime();
	if ( LastTime >= 0 && (CurrentTime - LastTime) < COMPUTE_SHADER_REFRESH_CHANGES_INTERVAL )
		return;	// Too soon to check !

	// Update last check time
	LastTime = CurrentTime;

	ms_WatchedShaders.ForEach( WatchShader, NULL );
}

#ifdef COMPUTE_SHADER_COMPILE_THREADED
void	ThreadCompileComputeShader( void* _pData )
{
	ComputeShader*	pMaterial = (ComputeShader*) _pData;
	pMaterial->RebuildShader();
}
#endif

void		ComputeShader::WatchShaderModifications()
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

#ifdef COMPUTE_SHADER_COMPILE_THREADED
	ASSERT( m_hCompileThread == 0, "Compilation thread already exists !" );

	DWORD	ThreadID;
    m_hCompileThread = CreateThread( NULL, 0, (LPTHREAD_START_ROUTINE) ThreadCompileComputeShader, this, 0, &ThreadID );
    SetThreadPriority( m_hCompileThread, THREAD_PRIORITY_HIGHEST );
}

void		ComputeShader::RebuildShader()
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

	// Release the mutex: it's now safe to access the shader !
	Unlock();

#ifdef COMPUTE_SHADER_COMPILE_THREADED
	// Close the thread once we're done !
	if ( m_hCompileThread )
		CloseHandle( m_hCompileThread );
	m_hCompileThread = 0;
#endif
}

void		ComputeShader::ForceRecompile()
{
	m_LastShaderModificationTime++;	// So we're sure it will be recompiled on next watch!
}

time_t		ComputeShader::GetFileModTime( const char* _pFileName )
{	
	struct _stat statInfo;
	_stat( _pFileName, &statInfo );

	return statInfo.st_mtime;
}

#endif	// #if defined(_DEBUG) || !defined(GODCOMPLEX)

ComputeShader*	ComputeShader::CreateFromBinaryBlob( Device& _Device, const char* _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint )
{
#ifndef SAVE_SHADER_BLOB_TO
	ASSERT( false, "You can't use that in RELEASE or if binary blobs are not available!" );
	return NULL;
#else
	ASSERT( _pEntryPoint != NULL, "You must provide a valid entry point!" );

//	D3D_SHADER_MACRO
	ID3DBlob*	pCS = Shader::LoadBinaryBlob( _pShaderFileName, _pMacros, _pEntryPoint );

	ComputeShader*	pResult = new ComputeShader( _Device, _pShaderFileName, pCS );

// No need: the blob was released by the constructor!
//	pCS->Release();

	return pResult;
#endif
}
