#include "stdafx.h"
#include "ShaderCompiler.h"

#include "FileServer.h"

#include <D3Dcompiler.h>
#include <D3D11Shader.h>

bool	ShaderCompiler::ms_LoadFromBinary = false;

#ifdef WARNING_AS_ERROR
	bool	ShaderCompiler::ms_warningsAsError = true;
#else
	bool	ShaderCompiler::ms_warningsAsError = false;
#endif

ID3DBlob*   ShaderCompiler::CompileShader( IFileServer& _fileServer, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint, const BString& _target, bool _isComputeShader ) {
	if ( ms_LoadFromBinary )
		return LoadPreCompiledShader( _fileServer, _shaderFileName, _macros, _entryPoint );

	// Load shader code
	LPCVOID	shaderCode = NULL;
	U32		shaderCodeSize;
	HRESULT	fileError = _fileServer.Open( D3D_INCLUDE_LOCAL, _shaderFileName, NULL, &shaderCode, &shaderCodeSize );
	if ( fileError != S_OK ) {
		#if defined(_DEBUG) || defined(DEBUG_SHADER)
			MessageBoxA( NULL, "Failed to open shader source file!", "Shader File Error!", MB_OK | MB_ICONERROR );
			ASSERT( false, "Shader file opening error!" );
		#endif
		return NULL;
	}

	// Pre-process
	ID3DBlob*   codeTextBlob = NULL;
	ID3DBlob*   errorsBlob = NULL;
	D3DPreprocess( shaderCode, shaderCodeSize, NULL, _macros, &_fileServer, &codeTextBlob, &errorsBlob );

	// Free source code
	_fileServer.Close( shaderCode );

	// Check for pre-processing errors
	if ( errorsBlob != NULL ) {
		#if defined(_DEBUG) || defined(DEBUG_SHADER)
			MessageBoxA( NULL, (LPCSTR) errorsBlob->GetBufferPointer(), "Shader PreProcess Error!", MB_OK | MB_ICONERROR );
			ASSERT( false, "Shader preprocess error!" );
		#endif
		return NULL;
	}

	// Perform actual compilation
	U32 Flags1 = 0, Flags2 = 0;
	#if (defined(_DEBUG) && !defined(SAVE_SHADER_BLOB_TO)) || defined(RENDERDOC) || defined(NSIGHT)
		Flags1 |= D3DCOMPILE_DEBUG;
		Flags1 |= D3DCOMPILE_SKIP_OPTIMIZATION;
//		Flags1 |= D3DCOMPILE_WARNINGS_ARE_ERRORS;
		Flags1 |= D3DCOMPILE_PREFER_FLOW_CONTROL;

//Flags1 |= _bComputeShader ? D3DCOMPILE_OPTIMIZATION_LEVEL1 : D3DCOMPILE_OPTIMIZATION_LEVEL3;

	#else
		if ( _isComputeShader )
			Flags1 |= D3DCOMPILE_OPTIMIZATION_LEVEL1;	// Seems to "optimize" (i.e. strip) the important condition line that checks for threadID before writing to concurrent targets => This leads to "race condition" errors
		else
			Flags1 |= D3DCOMPILE_OPTIMIZATION_LEVEL3;
	#endif
//		Flags1 |= D3DCOMPILE_ENABLE_STRICTNESS;
//		Flags1 |= D3DCOMPILE_IEEE_STRICTNESS;		// D3D9 compatibility, clamps precision to usual float32 but may prevent internal optimizations by the video card. Better leave it disabled!
		Flags1 |= D3DCOMPILE_PACK_MATRIX_ROW_MAJOR;	// MOST IMPORTANT FLAG!

	LPCVOID	pCodePointer = codeTextBlob->GetBufferPointer();
	size_t	CodeSize = codeTextBlob->GetBufferSize();
	size_t	CodeLength = strlen( (char*) pCodePointer );

	ID3DBlob*   codeBlob = NULL;
				errorsBlob = NULL;
	D3DCompile( pCodePointer, CodeSize, _shaderFileName, _macros, &_fileServer, _entryPoint, _target, Flags1, Flags2, &codeBlob, &errorsBlob );

	#if defined(_DEBUG) || defined(DEBUG_SHADER)
		bool	hasWarningOrErrors = errorsBlob != NULL;	// Represents warnings and errors
		bool	hasErrors = codeBlob == NULL;			// Surely an error if no shader is returned!
		if ( hasWarningOrErrors && (ms_warningsAsError || hasErrors) ) {
			const char*	pErrorText = (LPCSTR) errorsBlob->GetBufferPointer();
			MessageBoxA( NULL, pErrorText, "Shader Compilation Error!", MB_OK | MB_ICONERROR );
			ASSERT( errorsBlob == NULL, "Shader compilation error!" );
			return NULL;
		} else {
			ASSERT( codeBlob != NULL, "Shader compilation failed => No error provided but didn't output any shader either!" );
		}
	#endif

	// Save the binary blob to disk
	#if defined(SAVE_SHADER_BLOB_TO) && !defined(RENDERDOC) && !defined(NSIGHT)
		if ( codeBlob != NULL ) {
			SaveBinaryBlob( _shaderFileName, _macros, _entryPoint, *codeBlob );
		}
	#endif

	return codeBlob;
}

ID3DBlob*	ShaderCompiler::LoadPreCompiledShader( IFileServer& _fileServer, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint ) {
	ASSERT( !_shaderFileName.IsEmpty(), "Can't load binary blob => Invalid shader file name!" );
	ASSERT( !_entryPoint.IsEmpty(), "Can't load binary blob => Invalid entry point name!" );

#if !defined(_DEBUG) && defined(GODCOMPLEX)
	// Load from the aggregate
	return LoadBinaryBlobFromAggregate( _shaderFileName, _macros, _entryPoint );

#else

	// Create the unique binary blob file name
	BString	finalShaderName;
	BuildBinaryBlobFileName( finalShaderName, _shaderFileName, _macros, _entryPoint );

	// Load the binary file
	const U8*	fileContent = NULL;
	U32			fileLength;
	HRESULT		fileError = _fileServer.Open( D3D_INCLUDE_LOCAL, _shaderFileName, NULL, (LPCVOID*) &fileContent, &fileLength );
	if ( fileError != S_OK ) {
		#if defined(_DEBUG) || defined(DEBUG_SHADER)
			MessageBoxA( NULL, "Failed to open shader binary file!", "Shader File Error!", MB_OK | MB_ICONERROR );
			ASSERT( false, "Shader file opening error!" );
		#endif
		return NULL;
	}

	const U8*	currentPtr = fileContent;

	// Read the entry point's length
	int	entryPointLength = *((int*) currentPtr); currentPtr += sizeof(int);
	if ( strncmp( _entryPoint, (const char*) currentPtr, entryPointLength ) ) {
		ASSERT( false, "Entry point names mismatch!" );
		return NULL;
	}
	currentPtr += entryPointLength;

	// Read the blob's length
	int	blobSize = *((int*) currentPtr); currentPtr += sizeof(int);

	// Create a D3DBlob
	ID3DBlob*	shaderBlob = NULL;
	D3DCreateBlob( blobSize, &shaderBlob );

	// Copy the blob's content
	LPVOID	shaderBlobContent = shaderBlob->GetBufferPointer();
	memcpy_s( shaderBlobContent, blobSize, currentPtr, blobSize );

	return shaderBlob;
#endif
}

ID3DBlob*	ShaderCompiler::LoadBinaryBlobFromAggregate( const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint ) {
	const U8*	aggregate = NULL;
	throw "TODO: Find aggregate from shader name + macros signature!";
	return LoadBinaryBlobFromAggregate( aggregate, _entryPoint );
}

ID3DBlob*	ShaderCompiler::LoadBinaryBlobFromAggregate( const U8* _aggregate, const BString& _entryPoint ) {
	U16	blobsCount = *((U16*) _aggregate); _aggregate+=2;	// Amount of blobs in the big blob
	for ( U16 blobIndex=0; blobIndex < blobsCount; blobIndex++ ) {
		int	cmp = strcmp( (char*) _aggregate, _entryPoint );
		int	blobEntryPointLength = int( strlen( (char*) _aggregate ) );
		_aggregate += blobEntryPointLength+1;	// Skip the entry point's name
		if ( !cmp ) {
			// Found it !
			U16	BlobStartOffset = *((U16*) _aggregate); _aggregate+=2;	// Retrieve the jump offset to reach the blob
			_aggregate += BlobStartOffset;									// Go to the blob descriptor

			U16	BlobSize = *((U16*) _aggregate); _aggregate+=2;			// Retrieve the size of the blob

			// Create a D3DBlob
			ID3DBlob*	pResult = NULL;
			D3DCreateBlob( BlobSize, &pResult );

			// Copy our blob content
			void*		pBlobContent = pResult->GetBufferPointer();
			memcpy( pBlobContent, _aggregate, BlobSize );

			// Yoohoo!
			return pResult;
		}

		// Not that blob either... Skip the jump offset...
		_aggregate += 2;
	}

	return NULL;
}

//////////////////////////////////////////////////////////////////////////
// Auto-saving binary blobs 
#ifdef SAVE_SHADER_BLOB_TO

void	ShaderCompiler::BuildBinaryBlobFileName( BString& _blobFileName, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint ) {
	// Build unique macros signature
	BString	macrosSignature;
	BuildMacroSignature( macrosSignature, _macros );

	// Build filename
	const char*	pFileName = strrchr( _shaderFileName, '/' );
	if ( pFileName == NULL )
		pFileName = strrchr( _shaderFileName, '\\' );
	ASSERT( pFileName != NULL, "Can't retrieve last '/'!" );
	int		FileNameIndex = int( 1+pFileName - _shaderFileName );

	const char*	pExtension = strrchr( _shaderFileName, '.' );
	ASSERT( pExtension != NULL, "Can't retrieve extension!" );
	int		ExtensionIndex = int( pExtension - _shaderFileName );

	char	pShaderPath[1024];
	memcpy( pShaderPath, _shaderFileName, FileNameIndex );
	pShaderPath[FileNameIndex] = '\0';	// End the path name here

	char	pFileNameWithoutExtension[1024];
	memcpy( pFileNameWithoutExtension, pFileName+1, ExtensionIndex-FileNameIndex );
	pFileNameWithoutExtension[ExtensionIndex-FileNameIndex] = '\0';	// End the file name here

//	sprintf_s( _blobFileName, 4096, "%s%s%s.%s.fxbin", SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, pMacrosSignature, _entryPoint );
	_blobFileName.Format( "%s%s%s%s.%s.fxbin", pShaderPath, SAVE_SHADER_BLOB_TO, pFileNameWithoutExtension, macrosSignature, _entryPoint );
}

void	ShaderCompiler::BuildMacroSignature( BString& _signature, D3D_SHADER_MACRO* _macros ) {
	char	temp[1024];
	char*	pCurrent = temp;
	while ( _macros != NULL && _macros->Name != NULL ) {
		*pCurrent++ = '_';
		strcpy_s( pCurrent, 1024-(pCurrent-temp), _macros->Name );
		pCurrent += strlen( _macros->Name );
		*pCurrent++ = '=';
		strcpy_s( pCurrent, 1024-(pCurrent-temp), _macros->Definition );
		pCurrent += strlen( _macros->Definition );
		_macros++;
	}
	*pCurrent = '\0';

	_signature = temp;
}

void	ShaderCompiler::SaveBinaryBlob( const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint, ID3DBlob& _Blob ) {
	ASSERT( !_shaderFileName.IsEmpty(), "Can't save binary blob => Invalid shader file name!" );
	ASSERT( !_entryPoint.IsEmpty(), "Can't save binary blob => Invalid entry point name!" );

	// Create the unique binary blob file name
	BString	finalShaderName;
	BuildBinaryBlobFileName( finalShaderName, _shaderFileName, _macros, _entryPoint );

	// Create the binary file
	FILE*	pFile;
	fopen_s( &pFile, finalShaderName, "wb" );
	ASSERT( pFile != NULL, "Can't create binary shader file!" );

	// Write the entry point's length
	int	Length = int( strlen( _entryPoint )+1 );
	fwrite( &Length, sizeof(int), 1, pFile );

	// Write the entry point name
	fwrite( _entryPoint, 1, Length, pFile );

	// Write the blob's length
	Length = int( _Blob.GetBufferSize() );
//	ASSERT( Length < 65536, "Shader length doesn't fit on 16 bits!" );
	fwrite( &Length, sizeof(int), 1, pFile );

	// Write the blob's content
	LPCVOID	pCodePointer = _Blob.GetBufferPointer();
	fwrite( pCodePointer, 1, Length, pFile );

	// We're done!
	fclose( pFile );
}

#endif	// #ifdef SAVE_SHADER_BLOB_TO
