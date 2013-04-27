#pragma once

#include "Component.h"

#define COMPUTE_SHADER_REFRESH_CHANGES_INTERVAL	500

#ifndef GODCOMPLEX
// This is useful only for applications, not demos !

#define COMPUTE_SHADER_COMPILE_AT_RUNTIME	// Define this to start compiling shaders at runtime and avoid blocking (useful for debugging)
											// If you enable that option then the shader will start compiling as soon as WatchShaderModifications() is called on the material

#define COMPUTE_SHADER_COMPILE_THREADED		// Define this to launch shader compilation in different threads

#endif

class ConstantBuffer;

// #define USING_COMPUTESHADER_START( Mat )	\
// {									\
// 	(Mat).Use();					\
// 	ComputeShader&	M = Mat;
// 
// #define USING_COMPUTESHADER_END	\
// 	M.GetDevice().RemoveRenderTargets(); /* Just to ensure we don't leave any attached RT we may need later as a texture !*/	\
// }


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

	// This is the class that is used to pass values to the shader and read back the results
	class	StructuredBuffer : public Component
	{
	protected:	// FIELDS

		int				m_ElementSize;
		int				m_ElementsCount;
		int				m_Size;

		ID3D11Buffer*   m_pBuffer;
		ID3D11Buffer*   m_pCPUBuffer;

		ID3D11ShaderResourceView*	m_pShaderView;
		ID3D11UnorderedAccessView*  m_pUnorderedAccessView;

		// Structure to keep track of current outputs
		int							m_pAssignedToOutputSlot[D3D11_PS_CS_UAV_REGISTER_COUNT];
		static StructuredBuffer*	ms_ppOutputs[D3D11_PS_CS_UAV_REGISTER_COUNT];


	public:		// PROPERTIES

		int				GetElementSize() const		{ return m_ElementSize; }
		int				GetElementsCount() const	{ return m_ElementsCount; }
		int				GetSize() const				{ return m_Size; }

		ID3D11ShaderResourceView*	GetShaderView()				{ return m_pShaderView; }
		ID3D11UnorderedAccessView*	GetUnorderedAccessView()	{ return m_pUnorderedAccessView; }

	public:		// METHODS

		StructuredBuffer( Device& _Device, int _ElementSize, int _ElementsCount, bool _bWriteable );
		~StructuredBuffer();

		// Read/Write for CPU interchange
		void			Read( void* _pData, int _ElementsCount=-1 ) const;
		void			Write( void* _pData, int _ElementsCount=-1 );

		// Clear of the unordered access view
		void			Clear( U32 _pValue[4] );
		void			Clear( const NjFloat4& _Value );

		// Uploads the buffer to the shader
		void			SetInput( int _SlotIndex );
		void			SetOutput( int _SlotIndex );
	};


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
	// NOTE: Each thread group contains 
	//
	void			Run( int _GroupsCountX, int _GroupsCountY, int _GroupsCountZ );


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

#ifdef COMPUTE_SHADER_COMPILE_THREADED
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
};


//////////////////////////////////////////////////////////////////////////
// Helper class to easily manipulate structured buffers
//
template<typename T> class	SB
{
public:		// FIELDS

	T*					m;

protected:

	ComputeShader::StructuredBuffer*	m_pBuffer;

public:		// PROPERTIES

	int							GetElementSize() const		{ return sizeof(T); }
	int							GetElementsCount() const	{ return m_pBuffer->GetElementsCount(); }
	int							GetSize() const				{ return m_pBuffer->GetSize(); }
	ID3D11ShaderResourceView*	GetShaderView()				{ return m_pBuffer->GetShaderView(); }
	ID3D11UnorderedAccessView*	GetUnorderedAccessView()	{ return m_pBuffer->GetUnorderedAccessView(); }

public:		// METHODS

	SB() : m( NULL ), m_pBuffer( NULL ) {}
	SB( Device& _Device, int _ElementsCount, bool _bWriteable ) : m( NULL ), m_pBuffer( NULL ) { Init( _Device, _ElementsCount, _bWriteable ); }
	~SB()								{ delete m_pBuffer; delete[] m; }

	void	Init( Device& _Device, int _ElementsCount, bool _bWriteable )
	{
		m = new T[_ElementsCount];
		m_pBuffer = new ComputeShader::StructuredBuffer( _Device, sizeof(T), _ElementsCount, _bWriteable );
	}

	void	Read( int _ElementsCount=-1 )	{ m_pBuffer->Read( m, _ElementsCount ); }
	void	Write( int _ElementsCount=-1 )	{ m_pBuffer->Write( m, _ElementsCount ); }
	void	Clear( U32 _pValue[4] )			{ m_pBuffer->Clear( _pValue ); }
	void	Clear( const NjFloat4& _Value )	{ m_pBuffer->Clear( _Value ); }
	void	SetInput( int _SlotIndex )		{ m_pBuffer->SetInput( _SlotIndex ); }
	void	SetOutput( int _SlotIndex )		{ m_pBuffer->SetOutput( _SlotIndex ); }
};

