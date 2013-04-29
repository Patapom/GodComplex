#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"

#define WATCH_SHADER_MODIFICATIONS	// Define this to reload shaders from disk if they changed
#define MATERIAL_REFRESH_CHANGES_INTERVAL	500

#ifndef GODCOMPLEX
// This is useful only for applications, not demos !

#define MATERIAL_COMPILE_AT_RUNTIME	// Define this to start compiling shaders at runtime and avoid blocking (useful for debugging)
									// If you enable that option then the shader will start compiling as soon as WatchShaderModifications() is called on the material

#define MATERIAL_COMPILE_THREADED	// Define this to launch shader compilation in different threads (compiles much faster but shaders are not immediately ready!)

#endif

//#define WARNING_AS_ERRORS			// Also report warnings in the message box

//#define __DEBUG_UPLOAD_ONLY_ONCE	// If defined, then the constants & textures will be uploaded only once (once the material is compiled)
									// This allows to test the importance of constants/texture uploads in the performance of the application
									// Obviously, if you switch textures & render targets often this will give you complete crap results !


// Define this to save the binary blobs for each shader (only works in DEBUG mode)
#define SAVE_SHADER_BLOB_TO		"./Resources/Shaders/Binary/"

#ifdef GODCOMPLEX
#define USE_BINARY_BLOBS			// Define this to use pre-compiled binary blobs resources rather than text files
#endif


class ConstantBuffer;

#define USING_MATERIAL_START( Mat )	\
{									\
	Material&	M = Mat;			\
	M.Use();						\

// #define USING_MATERIAL_END	\
// M.GetDevice().RemoveRenderTargets(); /* Just to ensure we don't leave any attached RT we may need later as a texture !*/	\
// }
#define USING_MATERIAL_END	\
}


class Material : public Component, ID3DInclude
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

#ifndef GODCOMPLEX
	ShaderConstants			m_VSConstants;
	ShaderConstants			m_HSConstants;
	ShaderConstants			m_DSConstants;
	ShaderConstants			m_GSConstants;
	ShaderConstants			m_PSConstants;

	Dictionary<const char*>	m_Pointer2FileName;
#endif


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

	Material( Device& _Device, const IVertexFormatDescriptor& _Format, const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointHS, const char* _pEntryPointDS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride );
	~Material();

	void			SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer );
	void			SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData );
#ifndef GODCOMPLEX
	bool			SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer );
	bool			SetTexture( const char* _pTextureName, ID3D11ShaderResourceView* _pData );
#endif

	void			Use();

	// Static shader compilation helper
	static ID3DBlob*	CompileShader( const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget, ID3DInclude* _pInclude, bool _bComputeShader=false );


public:	// ID3DInclude Members

    STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
    STDMETHOD(Close)( THIS_ LPCVOID _pData );

private:

	void			CompileShaders( const char* _pShaderCode );

	const char*		CopyString( const char* _pShaderFileName ) const;
#ifndef GODCOMPLEX
	const char*		GetShaderPath( const char* _pShaderFileName ) const;
#endif


	// Returns true if the shaders are safe to access (i.e. have been compiled and no other thread is accessing them)
	// WARNING: Calling this will take ownership of the mutex if the function returns true ! You thus must call Unlock() later...
	bool			Lock() const;
	void			Unlock() const;

#ifdef MATERIAL_COMPILE_THREADED
	//////////////////////////////////////////////////////////////////////////
	// Threaded compilation
	HANDLE			m_hCompileThread;
	HANDLE			m_hCompileMutex;

	void			StartThreadedCompilation();
public:
	void			RebuildShader();
#endif


	//////////////////////////////////////////////////////////////////////////
	// Shader auto-reload on change mechanism
private:
	// The dictionary of watched materials
#if defined(_DEBUG) || !defined(GODCOMPLEX)
	static DictionaryString<Material*>	ms_WatchedShaders;
	time_t			m_LastShaderModificationTime;
	time_t			GetFileModTime( const char* _pFileName );
#endif

public:
	// Call this every time you need to rebuild shaders whose code has changed
	static void		WatchShadersModifications();
	void			WatchShaderModifications();
};

