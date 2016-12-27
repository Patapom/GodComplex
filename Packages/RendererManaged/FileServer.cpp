#include "stdafx.h"
#include "FileServer.h"
#include <sys/stat.h>

using namespace System;

namespace Renderer {

 	#pragma region Generic Server

	class GenericServer : public IFileServer {
	public:
		typedef void	(*OpenFileDelegate)( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes );
		typedef void	(*CloseFileDelegate)( LPCVOID pData );
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
			(*m_openFileDelegate)( IncludeType, pFileName, pParentData, ppData, pBytes );
			return S_OK;
		}
		STDMETHOD(Close)(THIS_ LPCVOID pData) {
			(*m_closeFileDelegate)( pData );
			return S_OK;
		}

		virtual time_t	GetFileModTime( const BString& _fileName ) const override {
			return (*m_getFileModTimeDelegate)( _fileName );
		}
	};

	#pragma endregion

	FileServer::FileServer( System::IO::DirectoryInfo^ _baseDirectory ) {
// 		m_fileName2AllocatedMemory = gcnew Dictionary< String^, IntPtr >();
		m_baseDirectory = _baseDirectory;

		OpenFileDelegate^	openDelegate = gcnew OpenFileDelegate( this, &FileServer::Disk_OpenFile );
		CloseFileDelegate^	closeDelegate = gcnew CloseFileDelegate( this, &FileServer::Disk_CloseFile );
		GetFileModTimeDelegate^	getFileModDelegate = gcnew GetFileModTimeDelegate( this, &FileServer::Disk_GetFileModTime );
		InitServer( openDelegate, closeDelegate, getFileModDelegate );
	}

	FileServer::FileServer( System::Resources::ResourceManager^ _manager ) {
// 		m_fileName2AllocatedMemory = gcnew Dictionary< String^, IntPtr >();
		m_manager = _manager;

		OpenFileDelegate^	openDelegate = gcnew OpenFileDelegate( this, &FileServer::ResourceManager_OpenFile );
		CloseFileDelegate^	closeDelegate = gcnew CloseFileDelegate( this, &FileServer::ResourceManager_CloseFile );
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
		IntPtr	ptrFileModDelegate = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate( _closeFileDelegate );

		m_server = new GenericServer(	reinterpret_cast< GenericServer::OpenFileDelegate >( ptrOpenDelegate.ToPointer() ),
										reinterpret_cast< GenericServer::CloseFileDelegate >( ptrCloseDelegate.ToPointer() ),
										reinterpret_cast< GenericServer::GetFileModTimeDelegate >( ptrFileModDelegate.ToPointer() )
									);
	}

 	#pragma region Disk Server

// 	#include <sys/stat.h>

	HRESULT	FileServer::Disk_OpenFile( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes ) {
		String^	partialFileName = System::Runtime::InteropServices::Marshal::PtrToStringAnsi( static_cast<IntPtr>( (void*) pFileName ) );  
		String^	shaderFileNameStr = System::IO::Path::Combine( m_baseDirectory->FullName, partialFileName );

		System::IO::FileInfo^	shaderFileName = gcnew System::IO::FileInfo( shaderFileNameStr );
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
			*pBytes = UINT( fileLength );
		} catch ( System::Exception^ ) {
			// Failed to read! Maybe the file is locked?
			return S_FALSE;
		}

// 		// Keep track of allocated memory for that file
// 		m_fileName2AllocatedMemory.Add( partialFileName, ptrFileContent );

		return S_OK;
	}
	HRESULT	FileServer::Disk_CloseFile( LPCVOID pData ) {
		IntPtr	ptrFileContent( (void*) pData );
		System::Runtime::InteropServices::Marshal::FreeHGlobal( ptrFileContent );
		return S_OK;
	}

	time_t	FileServer::Disk_GetFileModTime( const BString& _fileName ) {
		struct _stat statInfo;
		_stat( _fileName, &statInfo );

		return statInfo.st_mtime;
//		return time_t( ~0ULL );
	}

	#pragma endregion

 	#pragma region Resource Manager Server

	HRESULT	FileServer::ResourceManager_OpenFile( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes ) {
		String^	partialFileName = System::Runtime::InteropServices::Marshal::PtrToStringAnsi( static_cast<IntPtr>( (void*) pFileName ) );  
		return S_FALSE;
	}
	HRESULT	FileServer::ResourceManager_CloseFile( LPCVOID pData ) {
		IntPtr	ptrFileContent( (void*) pData );
		System::Runtime::InteropServices::Marshal::FreeHGlobal( ptrFileContent );
		return S_OK;
	}

	time_t	 FileServer::ResourceManager_GetFileModTime( const BString& _fileName ) {
		return time_t( ~0ULL );
	}

	#pragma endregion
}
