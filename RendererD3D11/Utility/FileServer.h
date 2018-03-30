#pragma once

#include <d3d11.h>

#define WATCH_SHADER_MODIFICATIONS	// Define this to reload shaders from disk if they changed (comment to ship a demo with embedded shaders)
#define MATERIAL_REFRESH_CHANGES_INTERVAL	500

#if defined(_DEBUG) || !defined(GODCOMPLEX)
	// Define this to save the binary blobs for each shader (only works in DEBUG mode)
	// NOTE: in RELEASE, the blobs are embedded as resources and read from binary so they need to have been saved to
	#ifdef GODCOMPLEX
		#define SAVE_SHADER_BLOB_TO		"./Resources/Shaders/Binary/"
	#else
//		#define SAVE_SHADER_BLOB_TO		"./Binary/"		// Save to "Binary" sub folder
		#define SAVE_SHADER_BLOB_TO		""				// Save into the same folder
	#endif
#endif


//////////////////////////////////////////////////////////////////////////
// Declares the interface to a file server
// 
class IFileServer : public ID3DInclude {
public:
	// We inherit the ID3DInclude interface so you must implement these 2 methods as well:
	// NOTE: The Open/Close methods must support _ppData == NULL, in which case the file is only opened and closed immediately (used for test purpose)!
	//
//	STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
//	STDMETHOD(Close)( THIS_ LPCVOID _pData );

	// Gets the time at which the specified file was last modified
	virtual time_t			GetFileModTime( const BString& _fileName ) const abstract;
};

// static U32	GetHash( const void* _key ) {
// 	U64		v = U64( _key );
// 	U64		v2 = v;
// 			v2 >>= 32;
// 	return U32( v2 ^ v );
// }
// static S32	Compare( const void* const& _a, const void*& _b ) {
// 	return _a < _b ? -1 : (_a > _b ? 1 : 0);
// }

typedef struct _iobuf FILE;

//////////////////////////////////////////////////////////////////////////
// Generic disk file server loading files from disk
// 
class DiskFileServer : public IFileServer {
private:	// FIELDS

	// This dictionary is used to keep track of the various base shader paths that that were encountered by the file server
	BaseLib::DictionaryGeneric< BString, BString >	m_collectedDirectories;

public:

	static DiskFileServer	singleton;

private:	 // METHODS

	DiskFileServer();

public:	// ID3DInclude Members

    STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
    STDMETHOD(Close)( THIS_ LPCVOID _pData );

		// IFileServer Members
	time_t			GetFileModTime( const BString& _fileName ) const override;

private:

	// Attempts to find the shader file using already collected directories
	FILE*			FindShaderFile( const BString& _partialFileName );
public:
	FILE*			FindShaderFile( const BString& _directory, const BString& _partialFileName );
};
