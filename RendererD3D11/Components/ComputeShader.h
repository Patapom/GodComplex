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

#define USING_COMPUTESHADER_START( Shader )	\
{									\
	ComputeShader&	M = Shader;		\
	M.Use();						\

// #define USING_MATERIAL_END	\
// M.GetDevice().RemoveRenderTargets(); /* Just to ensure we don't leave any attached RT we may need later as a texture !*/	\
// }
#define USING_COMPUTE_SHADER_END	\
}


class ComputeShader : public Component, ID3DInclude
{
public:		// NESTED TYPES

#ifndef GODCOMPLEX
	class	ShaderConstants
	{
	public:	// NESTED TYPES

		struct BindingDesc
		{
			char*	pName;
			int		Slot;
#ifdef __DEBUG_UPLOAD_ONLY_ONCE
			bool	bUploaded;
#endif

			~BindingDesc();

			void	SetName( const char* _pName );
		};

	public:

		DictionaryString<BindingDesc*>	m_ConstantBufferName2Descriptor;
		DictionaryString<BindingDesc*>	m_TextureName2Descriptor;
		DictionaryString<BindingDesc*>	m_StructuredBufferName2Descriptor;
		DictionaryString<BindingDesc*>	m_UAVName2Descriptor;

		~ShaderConstants();

		void	Enumerate( ID3DBlob& _ShaderBlob );
		int		GetConstantBufferIndex( const char* _pBufferName ) const;
		int		GetShaderResourceViewIndex( const char* _pTextureName ) const;
		int		GetStructuredBufferIndex( const char* _pBufferName ) const;
		int		GetUnorderedAccesViewIndex( const char* _pUAVName ) const;
		
	};
#endif


private:	// FIELDS

	const char*				m_pShaderFileName;
	const char*				m_pShaderPath;
	ID3DInclude*			m_pIncludeOverride;

	D3D_SHADER_MACRO*		m_pMacros;

	const char*				m_pEntryPointCS;
	ID3D11ComputeShader*	m_pCS;

	bool					m_bHasErrors;

#ifndef GODCOMPLEX
	ShaderConstants			m_CSConstants;

	Dictionary<const char*>	m_Pointer2FileName;
#endif

	static ComputeShader*	ms_pCurrentShader;


public:	 // PROPERTIES

	bool				HasErrors() const	{ return m_bHasErrors; }

public:	 // METHODS

	ComputeShader( Device& _Device, const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, ID3DInclude* _pIncludeOverride );
	ComputeShader( Device& _Device, const char* _pShaderFileName, ID3DBlob* _pCS );
	~ComputeShader();

	void			SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer );
	void			SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData );
	void			SetStructuredBuffer( int _BufferSlot, StructuredBuffer& _Buffer );
	void			SetUnorderedAccessView( int _BufferSlot, StructuredBuffer& _Buffer );
#ifndef GODCOMPLEX
	bool			SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer );
	bool			SetTexture( const char* _pTextureName, ID3D11ShaderResourceView* _pData );
	bool			SetStructuredBuffer( const char* _pBufferName, StructuredBuffer& _Buffer );
	bool			SetUnorderedAccessView( const char* _pBufferName, StructuredBuffer& _Buffer );
#endif

	void			Use();

	// Runs the compute shader using as many thread groups as necessary
	//	_GroupsCountXYZ, the amount of thread groups to run the shader on (up to 65535)
	//
	// NOTE: Each thread group runs a number of threads as indicated in the shader
	//
	void			Dispatch( int _GroupsCountX, int _GroupsCountY, int _GroupsCountZ );


public:	// ID3DInclude Members

    STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
    STDMETHOD(Close)( THIS_ LPCVOID _pData );

private:

	void			CompileShaders( const char* _pShaderCode, ID3DBlob* _pCS=NULL );

	const char*		CopyString( const char* _pShaderFileName ) const;
#ifndef GODCOMPLEX
	const char*		GetShaderPath( const char* _pShaderFileName ) const;
#endif


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
	static ComputeShader*	CreateFromBinaryBlob( Device& _Device, const char* _pShaderFileName, const char* _pEntryPoint );


private:
	//////////////////////////////////////////////////////////////////////////
	// Shader auto-reload on change mechanism
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	// The dictionary of watched materials
	static DictionaryString<ComputeShader*>	ms_WatchedShaders;
	time_t			m_LastShaderModificationTime;
	time_t			GetFileModTime( const char* _pFileName );
#endif

public:
	// Call this every time you need to rebuild shaders whose code has changed
	static void		WatchShadersModifications();
	void			WatchShaderModifications();
	void			ForceRecompile();	// Called externally by the IncludesManager if an include file was changed
};
