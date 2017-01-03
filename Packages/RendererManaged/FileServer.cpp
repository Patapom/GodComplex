#include "stdafx.h"
#include "FileServer.h"

using namespace System;

namespace Renderer {

 	#pragma region Generic Server

	class GenericServer : public IFileServer {
	public:
		typedef HRESULT	(*OpenFileDelegate)( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes );
		typedef HRESULT	(*CloseFileDelegate)( LPCVOID pData );
		typedef time_t	(*GetFileModTimeDelegate)( const BString& _fileName );

	private:

		OpenFileDelegate		m_openFileDelegate;
		CloseFileDelegate		m_closeFileDelegate;
		GetFileModTimeDelegate	m_getFileModTimeDelegate;

	public:
		GenericServer( OpenFileDelegate _openFileDelegate, CloseFileDelegate _closeFileDelegate, GetFileModTimeDelegate _getFileModTimeDelegate )
			: m_openFileDelegate( _openFileDelegate )
			, m_closeFileDelegate( _closeFileDelegate )
			, m_getFileModTimeDelegate( _getFileModTimeDelegate ) {
		}

		STDMETHOD(Open)(THIS_ D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes) {
			return (*m_openFileDelegate)( IncludeType, pFileName, pParentData, ppData, pBytes );
		}
		STDMETHOD(Close)(THIS_ LPCVOID pData) {
			return (*m_closeFileDelegate)( pData );
		}
		virtual time_t	GetFileModTime( const BString& _fileName ) const override {
			return (*m_getFileModTimeDelegate)( _fileName );
		}
	};

	#pragma endregion

	FileServer::FileServer( System::IO::DirectoryInfo^ _baseDirectory ) {
		m_baseDirectory = _baseDirectory;

		OpenFileDelegate^		openDelegate = gcnew OpenFileDelegate( this, &FileServer::Disk_OpenFile );
		CloseFileDelegate^		closeDelegate = gcnew CloseFileDelegate( this, &FileServer::Disk_CloseFile );
		GetFileModTimeDelegate^	getFileModDelegate = gcnew GetFileModTimeDelegate( this, &FileServer::Disk_GetFileModTime );
		InitServer( openDelegate, closeDelegate, getFileModDelegate );
	}

	FileServer::FileServer( System::Resources::ResourceManager^ _manager ) {
		m_manager = _manager;

		OpenFileDelegate^		openDelegate = gcnew OpenFileDelegate( this, &FileServer::ResourceManager_OpenFile );
		CloseFileDelegate^		closeDelegate = gcnew CloseFileDelegate( this, &FileServer::ResourceManager_CloseFile );
		GetFileModTimeDelegate^	getFileModDelegate = gcnew GetFileModTimeDelegate( this, &FileServer::ResourceManager_GetFileModTime );
		InitServer( openDelegate, closeDelegate, getFileModDelegate );
	}

	FileServer::~FileServer() {
		// Release references to delegates
		m_handleOpenDelegate.Free();
		m_handleCloseDelegate.Free();
		m_handleFileModDelegate.Free();
	}

	void	FileServer::InitServer( OpenFileDelegate^ _openFileDelegate, CloseFileDelegate^ _closeFileDelegate, GetFileModTimeDelegate^ _getFileModDelegate ) {
		m_handleOpenDelegate = System::Runtime::InteropServices::GCHandle::Alloc( _openFileDelegate );
		m_handleCloseDelegate = System::Runtime::InteropServices::GCHandle::Alloc( _closeFileDelegate );
		m_handleFileModDelegate = System::Runtime::InteropServices::GCHandle::Alloc( _getFileModDelegate );
		
		IntPtr	ptrOpenDelegate = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate( _openFileDelegate );
		IntPtr	ptrCloseDelegate = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate( _closeFileDelegate );
		IntPtr	ptrFileModDelegate = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate( _getFileModDelegate );

		m_server = new GenericServer(	reinterpret_cast< GenericServer::OpenFileDelegate >( ptrOpenDelegate.ToPointer() ),
										reinterpret_cast< GenericServer::CloseFileDelegate >( ptrCloseDelegate.ToPointer() ),
										reinterpret_cast< GenericServer::GetFileModTimeDelegate >( ptrFileModDelegate.ToPointer() )
									);
	}

 	#pragma region Disk Server

 	#include <sys/stat.h>

	HRESULT	FileServer::Disk_OpenFile( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes ) {
		String^	partialFileName = System::Runtime::InteropServices::Marshal::PtrToStringAnsi( static_cast<IntPtr>( (void*) pFileName ) );  
		String^	fileNameNoDir = System::IO::Path::GetFileName( partialFileName );
		String^	shaderFileNameStr = System::IO::Path::Combine( m_baseDirectory->FullName, fileNameNoDir );

		System::IO::FileInfo^	shaderFileName = gcnew System::IO::FileInfo( shaderFileNameStr );
		if ( ppData == NULL ) {
			// Only a test for file's existence
			return shaderFileName->Exists ? S_OK : S_FALSE;
		}

		if ( !shaderFileName->Exists )
			throw gcnew System::IO::FileNotFoundException( "Shader file not found!", shaderFileName->FullName );

		// Read shader file's content (don't assume text! Could be binary shader!)
		try {
			System::IO::FileStream^	S = shaderFileName->OpenRead();
			int				fileLength = int( S->Length );
			array<Byte>^	fileContent = gcnew array<Byte>( fileLength );
			S->Read( fileContent, 0, fileLength );
		
			// Copy to unmanaged memory
			IntPtr	ptrFileContent = System::Runtime::InteropServices::Marshal::AllocHGlobal( fileLength );
			System::Runtime::InteropServices::Marshal::Copy( fileContent, 0, ptrFileContent, fileLength );
		
			*ppData = ptrFileContent.ToPointer();
			if ( pBytes != NULL ) {
				*pBytes = UINT( fileLength );
			}
		} catch ( System::Exception^ ) {
			// Failed to read! Maybe the file is locked?
			return S_FALSE;
		}

// 		// Keep track of allocated memory for that file
// 		m_fileName2AllocatedMemory.Add( partialFileName, ptrFileContent );

		return S_OK;
	}
	HRESULT	FileServer::Disk_CloseFile( LPCVOID pData ) {
		if ( pData != NULL ) {
			IntPtr	ptrFileContent( (void*) pData );
			System::Runtime::InteropServices::Marshal::FreeHGlobal( ptrFileContent );
		}
		return S_OK;
	}

	time_t	FileServer::Disk_GetFileModTime( const BString& _fileName ) {
		String^	partialFileName = System::Runtime::InteropServices::Marshal::PtrToStringAnsi( static_cast<IntPtr>( (void*) (const char*) _fileName ) );  
		String^	fileNameNoDir = System::IO::Path::GetFileName( partialFileName );
		String^	shaderFileNameStr = System::IO::Path::Combine( m_baseDirectory->FullName, fileNameNoDir );

		System::IO::FileInfo^	shaderFileName = gcnew System::IO::FileInfo( shaderFileNameStr );
		if ( !shaderFileName->Exists )
			throw gcnew System::IO::FileNotFoundException( "Shader file not found!", shaderFileName->FullName );

		DateTime	lasModificationTime = shaderFileName->LastWriteTime;

		time_t	result = lasModificationTime.ToFileTime();
		return result;
	}

	#pragma endregion

 	#pragma region Resource Manager Server

	HRESULT	FileServer::ResourceManager_OpenFile( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes ) {
		String^	partialFileName = System::Runtime::InteropServices::Marshal::PtrToStringAnsi( static_cast<IntPtr>( (void*) pFileName ) );

		// Extract filename ourselves since removing the extension using IO:Path sometimes removes too much
		int		indexOfLastSlash = partialFileName->LastIndexOf( "\\" );
		if ( indexOfLastSlash == -1 )
			indexOfLastSlash = partialFileName->LastIndexOf( "/" );
		int		indexOfLastDot = partialFileName->LastIndexOf( "." );
		if ( indexOfLastDot == -1 )
			indexOfLastDot = partialFileName->Length;
		String^	resourceName = partialFileName->Substring( indexOfLastSlash+1, indexOfLastDot - indexOfLastSlash - 1 );

		// Replace any remaining dots by "_" as the resources manager does...
		resourceName = resourceName->Replace( ".", "_" );

		// Retrieve resource, if it exists...
		Object^	resourceObj = m_manager->GetObject( resourceName );
		if ( resourceObj == nullptr )
			return S_FALSE;	// Not found...

		if ( ppData != NULL ) {
			// Read it as a Byte[]
			array<Byte>^	resourceAsByteArray = dynamic_cast< array<Byte>^ >( resourceObj );
			if ( resourceAsByteArray == nullptr )
				throw gcnew Exception( "Resource \"" + resourceName + "\" is not a Byte[] as expected! (did you include it as a text file?)" );

			U32		length = resourceAsByteArray->Length;
			IntPtr	resourcePtr = System::Runtime::InteropServices::Marshal::AllocHGlobal( length );

			System::Runtime::InteropServices::Marshal::Copy( resourceAsByteArray, 0, resourcePtr, length );

			*ppData = resourcePtr.ToPointer();
			if ( pBytes != NULL )
				*pBytes = length;
		}

		return S_OK;
	}
	HRESULT	FileServer::ResourceManager_CloseFile( LPCVOID pData ) {
		IntPtr	ptrFileContent( (void*) pData );
		System::Runtime::InteropServices::Marshal::FreeHGlobal( ptrFileContent );
		return S_OK;
	}

	time_t	 FileServer::ResourceManager_GetFileModTime( const BString& _fileName ) {
		return time_t( ~0ULL );	// Always modified!
	}

	#pragma endregion
}
