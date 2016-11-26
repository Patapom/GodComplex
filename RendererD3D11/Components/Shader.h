#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"

#define WATCH_SHADER_MODIFICATIONS	// Define this to reload shaders from disk if they changed (comment to ship a demo with embedded shaders)
#define MATERIAL_REFRESH_CHANGES_INTERVAL	500

//#define AUTHORIZE_MULTITHREADED_COMPILATION	// Define this to allow multithreaded compilation at runtime

#if !defined(GODCOMPLEX) && defined(AUTHORIZE_MULTITHREADED_COMPILATION)
	// This is useful only for applications, not demos !
	#define MATERIAL_COMPILE_AT_RUNTIME	// Define this to start compiling shaders at runtime and avoid blocking (useful for debugging)
										// If you enable that option then the shader will start compiling as soon as WatchShaderModifications() is called on the material

	#define MATERIAL_COMPILE_THREADED	// Define this to launch shader compilation in different threads (compiles much faster but shaders are not immediately ready!)

#endif

//#define WARNING_AS_ERRORS			// Also report warnings in the message box

//#define __DEBUG_UPLOAD_ONLY_ONCE	// If defined, then the constants & textures will be uploaded only once (once the material is compiled)
									// This allows to test the importance of constants/texture uploads in the performance of the application
									// Obviously, if you switch textures & render targets often this will give you complete crap results !


#if defined(_DEBUG) || !defined(GODCOMPLEX)
	// Define this to save the binary blobs for each shader (only works in DEBUG mode)
	// NOTE: in RELEASE, the blobs are embedded as resources and read from binary so they need to have been saved to
	#ifdef GODCOMPLEX
		#define SAVE_SHADER_BLOB_TO		"./Resources/Shaders/Binary/"
	#else
//		#define SAVE_SHADER_BLOB_TO		"./Binary/"
		#define SAVE_SHADER_BLOB_TO		""
	#endif
#endif	// _DEBUG

#ifdef GODCOMPLEX
#define USE_BINARY_BLOBS			// Define this to use pre-compiled binary blobs resources rather than text files
#endif


class ConstantBuffer;

#define USING_MATERIAL_START( shader )	\
{									\
	Shader&	M = shader;				\
	M.Use();						\

// #define USING_MATERIAL_END	\
// M.GetDevice().RemoveRenderTargets(); /* Just to ensure we don't leave any attached RT we may need later as a texture !*/	\
// }
#define USING_MATERIAL_END	\
}


class Shader : public Component, ID3DInclude
{
public:		// NESTED TYPES

#ifdef ENABLE_SHADER_REFLECTION
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

		~ShaderConstants();

		void	Enumerate( ID3DBlob& _ShaderBlob );
		int		GetConstantBufferIndex( const char* _pBufferName ) const;
		int		GetShaderResourceViewIndex( const char* _pTextureName ) const;
	};
#endif


private:	// FIELDS

	const IVertexFormatDescriptor&	m_Format;

	const char*				m_pShaderFileName;
	const char*				m_pShaderPath;
	ID3DInclude*			m_pIncludeOverride;

	D3D_SHADER_MACRO*		m_pMacros;

	ID3D11InputLayout*		m_pVertexLayout;

	const char*				m_pEntryPointVS;
	ID3D11VertexShader*		m_pVS;

	const char*				m_pEntryPointHS;
	ID3D11HullShader*		m_pHS;

	const char*				m_pEntryPointDS;
	ID3D11DomainShader*		m_pDS;

	const char*				m_pEntryPointGS;
	ID3D11GeometryShader*	m_pGS;

	const char*				m_pEntryPointPS;
	ID3D11PixelShader*		m_pPS;

	bool					m_bHasErrors;

 	#ifdef ENABLE_SHADER_REFLECTION
		ShaderConstants			m_VSConstants;
		ShaderConstants			m_HSConstants;
		ShaderConstants			m_DSConstants;
		ShaderConstants			m_GSConstants;
		ShaderConstants			m_PSConstants;
 	#endif

	BaseLib::Dictionary<const char*>	m_Pointer2FileName;

public:
	static bool				ms_LoadFromBinary;	// A flag you can set to force loading from binary files without having to write a specific code for that
												// Use the helper class ScopedForceMaterialsLoadFromBinary below


public:	 // PROPERTIES

	bool				HasErrors() const	{ return m_bHasErrors; }
	ID3D11InputLayout*  GetVertexLayout()
	{
		if ( !Lock() )
			return NULL;	// Probably compiling...

		ID3D11InputLayout*	pResult = m_pVertexLayout;

		Unlock();

		return pResult;
	}

	const IVertexFormatDescriptor&	GetFormat()	{ return m_Format; }


public:	 // METHODS

	Shader( Device& _Device, const char* _pShaderFileName, const IVertexFormatDescriptor& _Format, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride );
	Shader( Device& _Device, const char* _pShaderFileName, const IVertexFormatDescriptor& _Format, ID3DBlob* _pVS, ID3DBlob* _pHS, ID3DBlob* _pDS, ID3DBlob* _pGS, ID3DBlob* _pPS );
	~Shader();

	void			SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer );
	void			SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData );
	#ifdef ENABLE_SHADER_REFLECTION
		bool			SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer );
		bool			SetTexture( const char* _pTextureName, ID3D11ShaderResourceView* _pData );
	#endif

	// Must call this before using the material
	// Returns false if the shader cannot be used (like when it's in error state)
	bool			Use();

	// Static shader compilation helper (also used by ComputeShader)
	static ID3DBlob*	CompileShader( const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget, ID3DInclude* _pInclude, bool _bComputeShader=false );


public:	// ID3DInclude Members

    STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
    STDMETHOD(Close)( THIS_ LPCVOID _pData );

private:

	void			CompileShaders( const char* _pShaderCode, ID3DBlob* _pVS=NULL, ID3DBlob* _pHS=NULL, ID3DBlob* _pDS=NULL, ID3DBlob* _pGS=NULL, ID3DBlob* _pPS=NULL );

	const char*		CopyString( const char* _pShaderFileName ) const;
#ifndef GODCOMPLEX
	const char*		GetShaderPath( const char* _pShaderFileName ) const;
#endif

	// Returns true if the shaders are safe to access (i.e. have been compiled and no other thread is accessing them)
	// WARNING: Calling this will take ownership of the mutex if the function returns true ! You thus must call Unlock() later...
	bool			Lock() const;
	void			Unlock() const;

	//////////////////////////////////////////////////////////////////////////
	// Threaded compilation
#ifdef MATERIAL_COMPILE_THREADED
	HANDLE			m_hCompileThread;
	HANDLE			m_hCompileMutex;

	void			StartThreadedCompilation();
public:
	void			RebuildShader();
#endif



public:
	//////////////////////////////////////////////////////////////////////////
	// Binary Blobs
#ifdef SAVE_SHADER_BLOB_TO
	// Helper to reload a compiled binary blob and build the material from it
	static Shader*	CreateFromBinaryBlob( Device& _Device, const char* _pShaderFileName, const IVertexFormatDescriptor& _Format, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS );

	static void			SaveBinaryBlob( const char* _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, ID3DBlob& _Blob );
	static ID3DBlob*	LoadBinaryBlob( const char* _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint );	// NOTE: It's the caller's responsibility to release the blob!
	static void			BuildMacroSignature( char _pSignature[1024], D3D_SHADER_MACRO* _pMacros );
#endif

	// After .FXBIN files are processed by the ConcatenateShader project (Tools.sln), they are packed together in
	//	a single aggregate containing all the entry points for a given original HLSL file.
	// Each binary blob (FXBIN file) can be retrieved using this helper method...
	static ID3DBlob*	LoadBinaryBlobFromAggregate( const U8* _pAggregate, const char* _pEntryPoint );

private:
	//////////////////////////////////////////////////////////////////////////
	// Shader auto-reload on change mechanism

	// The dictionary of watched materials
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	static BaseLib::DictionaryString<Shader*>	ms_WatchedShaders;
	time_t			m_LastShaderModificationTime;
	time_t			GetFileModTime( const char* _pFileName );
#endif

public:
	// Call this every time you need to rebuild shaders whose code has changed
	static void		WatchShadersModifications();
	void			WatchShaderModifications();
	void			ForceRecompile();	// Called externally by the IncludesManager if an include file was changed
};

class	ScopedForceMaterialsLoadFromBinary
{
public:
	ScopedForceMaterialsLoadFromBinary()	{ Shader::ms_LoadFromBinary = true; }
	~ScopedForceMaterialsLoadFromBinary()	{ Shader::ms_LoadFromBinary = false; }
};