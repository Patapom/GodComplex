#pragma once

#include "..\Components\Component.h"

//#define WARNING_AS_ERROR			// Also report warnings in the message box

#if defined(_DEBUG) || !defined(GODCOMPLEX)
	// Define this to save the binary blobs for each shader (only works in DEBUG mode)
	// NOTE: in RELEASE, the blobs are embedded as resources and read from binary so they need to have been saved to first
	#ifdef GODCOMPLEX
		#define SAVE_SHADER_BLOB_TO		"./Resources/Shaders/Binary/"
	#else
//		#define SAVE_SHADER_BLOB_TO		"./Binary/"
		#define SAVE_SHADER_BLOB_TO		""
	#endif
#endif	// _DEBUG

class IFileServer;

class ShaderCompiler {
public:	// FIELDS

	// A flag you can set to treat warnings as errors
	static bool				ms_warningsAsError;

	static bool				ms_LoadFromBinary;	// A flag you can set to force loading from binary files without having to write a specific code for that
												// Use the helper class ScopedForceMaterialsLoadFromBinary below


public:	 // METHODS

	// Compiles the specified shader file
	//	_fileServer, the file server that is capable of providing the shader source code
	//	_shaderFileName, the full path and file name to the shader file to compile
	//	_macros, the set of compilation macros that are associated to the shader
	//	_entryPoint, the shader's entry point function name (e.g. "main")
	//	_target, the shader's target signature (e.g. "vs_5_0", "cs_4_0", etc.)
	// Returns the compiled binary blob or NULL if the compilation failed
	static ID3DBlob*	CompileShader( IFileServer& _fileServer, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint, const BString& _target, bool _isComputeShader=false );


	// Loads an already compiled shader
	//	_fileServer, the file server that is capable of providing the pre-compiled shader blob
	//	_shaderFileName, the full path and file name to the shader blob to load (NOTE: can be the HLSL shader file name, the FXBIN extension and proper signature are automatically setup)
	//	_macros, the set of compilation macros that are associated to the shader
	//	_entryPoint, the shader's entry point function name (e.g. "main")
	// Returns the pre-compiled binary blob or NULL if the loading failed
	// NOTE: It's the caller's responsibility to release the blob!
	static ID3DBlob*	LoadPreCompiledShader( IFileServer& _fileServer, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint );


private:
	//////////////////////////////////////////////////////////////////////////
	// Binary Blobs
	#ifdef SAVE_SHADER_BLOB_TO
 		static void			SaveBinaryBlob( const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint, ID3DBlob& _blob );
		static void			BuildBinaryBlobFileName( BString& _fileName, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint );
		static void			BuildMacroSignature( BString& _signature, D3D_SHADER_MACRO* _macros );
	#endif

	// Loads the blob from a blob aggregate
	// File server loading BINARY BLOB files from an aggregate
	// After .FXBIN files are processed by the ConcatenateShader project (Tools.sln), they are packed together into
	//	a single aggregate containing all the entry points for a given original HLSL file.
	// This method should be able to access a global repository of shader aggregates and return the appopriate binary blob depending on the requested entry point...
	static ID3DBlob*		LoadBinaryBlobFromAggregate( const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint );

	// Each binary blob (FXBIN file) can be retrieved using this helper method...
	static ID3DBlob*		LoadBinaryBlobFromAggregate( const U8* _aggregate, const BString& _entryPoint );
};

class	ScopedForceShadersLoadFromBinary {
	bool	m_formerState;
public:
	ScopedForceShadersLoadFromBinary()	{ m_formerState = ShaderCompiler::ms_LoadFromBinary; ShaderCompiler::ms_LoadFromBinary = true; }
	~ScopedForceShadersLoadFromBinary()	{ ShaderCompiler::ms_LoadFromBinary = m_formerState; }
};
