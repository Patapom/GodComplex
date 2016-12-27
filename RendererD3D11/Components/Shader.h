#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"

#define WATCH_SHADER_MODIFICATIONS	// Define this to reload shaders from disk if they changed (comment to ship a demo with embedded shaders)
#define MATERIAL_REFRESH_CHANGES_INTERVAL	500

//#define AUTHORIZE_MULTITHREADED_COMPILATION	// Define this to allow multithreaded compilation at runtime

#if !defined(GODCOMPLEX) && defined(AUTHORIZE_MULTITHREADED_COMPILATION)
	// This is useful only for applications, not demos!
	#define MATERIAL_COMPILE_AT_RUNTIME	// Define this to start compiling shaders at runtime and avoid blocking (useful for debugging)
										// If you enable that option then the shader will start compiling as soon as WatchShaderModifications() is called on the material

	#define MATERIAL_COMPILE_THREADED	// Define this to launch shader compilation in different threads (compiles much faster but shaders are not immediately ready!)

#endif

#ifdef GODCOMPLEX
#define USE_BINARY_BLOBS			// Define this to use pre-compiled binary blobs resources rather than text files
#endif


class ConstantBuffer;
class IFileServer;

#define USING_MATERIAL_START( shader )	\
{									\
	Shader&	M = shader;				\
	M.Use();						\

// #define USING_MATERIAL_END	\
// M.GetDevice().RemoveRenderTargets(); /* Just to ensure we don't leave any attached RT we may need later as a texture !*/	\
// }
#define USING_MATERIAL_END	\
}


class Shader : public Component {
public:		// NESTED TYPES

#ifdef ENABLE_SHADER_REFLECTION
	class	ShaderConstants {
	public:	// NESTED TYPES

		struct BindingDesc {
			char*	pName;
			int		Slot;

			~BindingDesc();

			void	SetName( const BString& _name );
		};

	public:

		DictionaryString<BindingDesc*>	m_ConstantBufferName2Descriptor;
		DictionaryString<BindingDesc*>	m_TextureName2Descriptor;

		~ShaderConstants();

		void	Enumerate( ID3DBlob& _ShaderBlob );
		int		GetConstantBufferIndex( const BString& _bufferName ) const;
		int		GetShaderResourceViewIndex( const BString& _textureName ) const;
	};
#endif

private:	// FIELDS

	const IVertexFormatDescriptor&	m_format;

	BString					m_shaderFileName;
	IFileServer*			m_fileServer;

	D3D_SHADER_MACRO*		m_macros;

	ID3D11InputLayout*		m_vertexLayout;

	BString					m_entryPointVS;
	ID3D11VertexShader*		m_pVS;

	BString					m_entryPointHS;
	ID3D11HullShader*		m_pHS;

	BString					m_entryPointDS;
	ID3D11DomainShader*		m_pDS;

	BString					m_entryPointGS;
	ID3D11GeometryShader*	m_pGS;

	BString					m_entryPointPS;
	ID3D11PixelShader*		m_pPS;

	bool					m_hasErrors;

 	#ifdef ENABLE_SHADER_REFLECTION
		ShaderConstants			m_VSConstants;
		ShaderConstants			m_HSConstants;
		ShaderConstants			m_DSConstants;
		ShaderConstants			m_GSConstants;
		ShaderConstants			m_PSConstants;
 	#endif


public:	 // PROPERTIES

	bool				HasErrors() const	{ return m_hasErrors; }
	ID3D11InputLayout*  GetVertexLayout() {
		if ( !Lock() )
			return NULL;	// Probably compiling...

		ID3D11InputLayout*	pResult = m_vertexLayout;

		Unlock();

		return pResult;
	}

	const IVertexFormatDescriptor&	GetFormat()	{ return m_format; }


public:	 // METHODS

	Shader( Device& _device, const BString& _pShaderFileName, const IVertexFormatDescriptor& _Format, D3D_SHADER_MACRO* _pMacros, const BString& _pEntryPointVS, const BString& _pEntryPointHS, const BString& _pEntryPointDS, const BString& _pEntryPointGS, const BString& _pEntryPointPS, IFileServer* _fileServerOverride );
	Shader( Device& _device, const BString& _pShaderFileName, const IVertexFormatDescriptor& _Format, ID3DBlob* _pVS, ID3DBlob* _pHS, ID3DBlob* _pDS, ID3DBlob* _pGS, ID3DBlob* _pPS );
	~Shader();

	void				SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer );
	void				SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData );
	#ifdef ENABLE_SHADER_REFLECTION
		bool			SetConstantBuffer( const BString& _pBufferName, ConstantBuffer& _Buffer );
		bool			SetTexture( const BString& _pTextureName, ID3D11ShaderResourceView* _pData );
	#endif

	// Must call this before using the material
	// Returns false if the shader cannot be used (like when it's in error state)
	bool				Use();

// 	// Static shader compilation helper (also used by ComputeShader)
// 	static ID3DBlob*	CompileShader( ID3DInclude* _pInclude, const String& _pShaderFileName, const String& _pShaderCode, D3D_SHADER_MACRO* _pMacros, const String& _pEntryPoint, const String& _pTarget, bool _bComputeShader=false );
// 
// 
// public:	// ID3DInclude Members
// 
//     STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
//     STDMETHOD(Close)( THIS_ LPCVOID _pData );

private:
	// Compiles all shaders from shader file
	void			CompileShaders( ID3DBlob* _pVS=NULL, ID3DBlob* _pHS=NULL, ID3DBlob* _pDS=NULL, ID3DBlob* _pGS=NULL, ID3DBlob* _pPS=NULL );
// 
// 	const String&		CopyString( const String& _pShaderFileName ) const;

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
	// Helper to reload a compiled binary blob and build the material from it
	static Shader*		CreateFromBinaryBlob( Device& _device, const BString& _shaderFileName, const IVertexFormatDescriptor& _format, D3D_SHADER_MACRO* _macros, const BString& _entryPointVS, const BString& _entryPointHS, const BString& _entryPointDS, const BString& _entryPointGS, const BString& _entryPointPS );


// #ifdef SAVE_SHADER_BLOB_TO
// 	static void			SaveBinaryBlob( const String& _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const String& _pEntryPoint, ID3DBlob& _Blob );
// 	static ID3DBlob*	LoadBinaryBlob( const String& _pShaderFileName, D3D_SHADER_MACRO* _pMacros, const String& _pEntryPoint );	// NOTE: It's the caller's responsibility to release the blob!
// 	static void			BuildMacroSignature( char _pSignature[1024], D3D_SHADER_MACRO* _pMacros );
// #endif
// 
// 	// After .FXBIN files are processed by the ConcatenateShader project (Tools.sln), they are packed together in
// 	//	a single aggregate containing all the entry points for a given original HLSL file.
// 	// Each binary blob (FXBIN file) can be retrieved using this helper method...
// 	static ID3DBlob*	LoadBinaryBlobFromAggregate( const U8* _pAggregate, const String& _pEntryPoint );

private:
	//////////////////////////////////////////////////////////////////////////
	// Shader auto-reload on change mechanism

	// The dictionary of watched materials
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	static BaseLib::DictionaryString<Shader*>	ms_WatchedShaders;
	time_t			m_LastShaderModificationTime;
//	time_t			GetFileModTime( const BString& _fileName );
#endif

public:
	// Call this every time you need to rebuild shaders whose code has changed
	static void		WatchShadersModifications();
	void			WatchShaderModifications();
	void			ForceRecompile();	// Called externally by the IncludesManager if an include file was changed
};
