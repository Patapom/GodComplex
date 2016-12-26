#pragma once

#include "Component.h"

#define COMPUTE_SHADER_REFRESH_CHANGES_INTERVAL	500

//#define AUTHORIZE_MULTITHREADED_COMPILATION	// Define this to allow multithreaded compilation at runtime

#if !defined(GODCOMPLEX) && defined(AUTHORIZE_MULTITHREADED_COMPILATION)
	// This is useful only for applications, not demos!
	#define COMPUTE_SHADER_COMPILE_AT_RUNTIME	// Define this to start compiling shaders at runtime and avoid blocking (useful for debugging)
												// If you enable that option then the shader will start compiling as soon as WatchShaderModifications() is called on the material

	#define COMPUTE_SHADER_COMPILE_THREADED		// Define this to launch shader compilation in different threads

#endif


class ConstantBuffer;
class StructuredBuffer;
class IFileServer;

#define USING_COMPUTESHADER_START( Shader )	\
{									\
	ComputeShader&	M = Shader;		\
	M.Use();						\

// #define USING_COMPUTE_SHADER_END	\
// M.GetDevice().RemoveRenderTargets(); /* Just to ensure we don't leave any attached RT we may need later as a texture !*/	\
// }
#define USING_COMPUTE_SHADER_END	\
}


class ComputeShader : public Component {
public:		// NESTED TYPES

// Pom (2016-11-01) Because of a fucking link error about IID_ID3D11ShaderReflection we can't use reflection...
// #ifndef GODCOMPLEX
// 	class	ShaderConstants {
// 	public:	// NESTED TYPES
// 
// 		struct BindingDesc {
// 			char*	pName;
// 			int		Slot;
// 
// 			~BindingDesc();
// 
// 			void	SetName( const char* _pName );
// 		};
// 
// 	public:
// 
// 		DictionaryString<BindingDesc*>	m_ConstantBufferName2Descriptor;
// 		DictionaryString<BindingDesc*>	m_TextureName2Descriptor;
// 		DictionaryString<BindingDesc*>	m_StructuredBufferName2Descriptor;
// 		DictionaryString<BindingDesc*>	m_UAVName2Descriptor;
// 
// 		~ShaderConstants();
// 
// 		void	Enumerate( ID3DBlob& _ShaderBlob );
// 		int		GetConstantBufferIndex( const char* _pBufferName ) const;
// 		int		GetShaderResourceViewIndex( const char* _pTextureName ) const;
// 		int		GetStructuredBufferIndex( const char* _pBufferName ) const;
// 		int		GetUnorderedAccesViewIndex( const char* _pUAVName ) const;
// 		
// 	};
// #endif


private:	// FIELDS

	BString					m_shaderFileName;
	IFileServer*			m_fileServer;

	D3D_SHADER_MACRO*		m_macros;

	BString					m_entryPointCS;
	ID3D11ComputeShader*	m_pCS;

	bool					m_hasErrors;

 	#ifdef ENABLE_SHADER_REFLECTION
	 	ShaderConstants			m_CSConstants;
	#endif

	static ComputeShader*	ms_pCurrentShader;


public:	 // PROPERTIES

	bool				HasErrors() const	{ return m_hasErrors; }

public:	 // METHODS

	ComputeShader( Device& _device, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint, IFileServer* _fileServerOverride );
	ComputeShader( Device& _device, const BString& _shaderFileName, ID3DBlob* _blobCS );
	~ComputeShader();

	void			SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer );
	void			SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData );
	void			SetStructuredBuffer( int _BufferSlot, StructuredBuffer& _Buffer );
	void			SetUnorderedAccessView( int _BufferSlot, StructuredBuffer& _Buffer );
#ifndef GODCOMPLEX
	bool			SetConstantBuffer( const BString& _pBufferName, ConstantBuffer& _Buffer );
	bool			SetTexture( const BString& _pTextureName, ID3D11ShaderResourceView* _pData );
	bool			SetStructuredBuffer( const BString& _pBufferName, StructuredBuffer& _Buffer );
	bool			SetUnorderedAccessView( const BString& _pBufferName, StructuredBuffer& _Buffer );
#endif

	bool			Use();

	// Runs the compute shader using as many thread groups as necessary
	//	_GroupsCountXYZ, the amount of thread groups to run the shader on (up to 65535)
	//
	// NOTE: Each thread group runs a number of threads as indicated in the shader
	//
	void			Dispatch( U32 _GroupsCountX, U32 _GroupsCountY, U32 _GroupsCountZ );

private:

	void			CompileShader( ID3DBlob* _pCS=NULL );

// 	#ifndef GODCOMPLEX
// 		const char*		GetShaderPath( const char* _pShaderFileName ) const;
// 	#endif


	// Returns true if the shaders are safe to access (i.e. have been compiled and no other thread is accessing them)
	// WARNING: Calling this will take ownership of the mutex if the function returns true ! You thus must call Unlock() later...
	bool			Lock() const;
	void			Unlock() const;

#ifdef COMPUTE_SHADER_COMPILE_THREADED
	//////////////////////////////////////////////////////////////////////////
	// Threaded compilation
	HANDLE			m_hCompileThread;
	HANDLE			m_hCompileMutex;

	void			StartThreadedCompilation();
public:
	void			RebuildShader();
#endif


public:
	//////////////////////////////////////////////////////////////////////////
	// Binary Blobs

	// Helper to reload a compiled binary blob and build the shader from it
	static ComputeShader*	CreateFromBinaryBlob( Device& _device, const BString& _shaderFileName, D3D_SHADER_MACRO* _macros, const BString& _entryPoint );


private:
	//////////////////////////////////////////////////////////////////////////
	// Shader auto-reload on change mechanism
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	// The dictionary of watched materials
	static BaseLib::DictionaryString<ComputeShader*>	ms_WatchedShaders;
	time_t			m_LastShaderModificationTime;
	time_t			GetFileModTime( const char* _pFileName );
#endif

public:
	// Call this every time you need to rebuild shaders whose code has changed
	static void		WatchShadersModifications();
	void			WatchShaderModifications();
	void			ForceRecompile();	// Called externally by the IncludesManager if an include file was changed
};
