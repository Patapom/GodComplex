// RendererManaged.h

#pragma once

#include "Device.h"
#include "ShaderMacros.h"

using namespace System;
using namespace System::Collections::Generic;

namespace Renderer {

	public ref class FileServer {
	private:

		delegate HRESULT	OpenFileDelegate( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes );
		delegate HRESULT	CloseFileDelegate( LPCVOID pData );
		delegate time_t		GetFileModTimeDelegate( const BString& _fileName );

		System::Runtime::InteropServices::GCHandle	m_handleOpenDelegate;
		System::Runtime::InteropServices::GCHandle	m_handleCloseDelegate;
		System::Runtime::InteropServices::GCHandle	m_handleFileModDelegate;

	internal:

		IFileServer*		m_server;

	public:
		// Constructs a FileServer that loads files from the disk from a specific directory
		FileServer( System::IO::DirectoryInfo^ _baseDirectory );

		// Constructs a FileServer that loads resource files from a System::ResourceManager
		FileServer( System::Resources::ResourceManager^ _manager );

		virtual ~FileServer();

	private:

		void		InitServer( OpenFileDelegate^ _openFileDelegate, CloseFileDelegate^ _closeFileDelegate, GetFileModTimeDelegate^ _getFileModDelegate );

		// Disk server
		System::IO::DirectoryInfo^			m_baseDirectory;
		HRESULT		Disk_OpenFile( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes );
		HRESULT		Disk_CloseFile( LPCVOID pData );
		time_t		Disk_GetFileModTime( const BString& _fileName );

		// Resource manager server
		System::Resources::ResourceManager^ m_manager;
		HRESULT		ResourceManager_OpenFile( D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes );
		HRESULT		ResourceManager_CloseFile( LPCVOID pData );
		time_t		ResourceManager_GetFileModTime( const BString& _fileName );
	};
}
