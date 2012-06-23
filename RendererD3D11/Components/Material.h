#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"

#define REFRESH_CHANGES_INTERVAL	500

class ConstantBuffer;

#define USING_MATERIAL_START( Mat )	\
{									\
	(Mat).Use();					\
	Material&	M = Mat;

#define USING_MATERIAL_END	\
	gs_Device.RemoveRenderTargets(); /* Just to ensure we don't leave any attached RT we may need later as a texture !*/	\
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

	const char*				m_pEntryPointGS;
	ID3D11GeometryShader*	m_pGS;

	const char*				m_pEntryPointPS;
	ID3D11PixelShader*		m_pPS;

	bool					m_bHasErrors;

#ifndef GODCOMPLEX
	ShaderConstants			m_VSConstants;
	ShaderConstants			m_GSConstants;
	ShaderConstants			m_PSConstants;

	Dictionary<const char*>	m_Pointer2FileName;
#endif


public:	 // PROPERTIES

	bool				HasErrors() const	{ return m_bHasErrors; }
	ID3D11InputLayout*  GetVertexLayout()	{ return m_pVertexLayout; }

public:	 // METHODS

	Material( Device& _Device, const IVertexFormatDescriptor& _Format, const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride );
	~Material();

	void			SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer );
	void			SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData );
#ifndef GODCOMPLEX
	bool			SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer );
	bool			SetTexture( const char* _pTextureName, ID3D11ShaderResourceView* _pData );
#endif

	void			Use();

public:	// ID3DInclude Members

    STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
    STDMETHOD(Close)( THIS_ LPCVOID _pData );

private:

	void			CompileShaders( const char* _pShaderCode );
	ID3DBlob*		CompileShader( const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget );
#ifndef GODCOMPLEX
	const char*		GetShaderPath( const char* _pShaderFileName ) const;
#endif

	//////////////////////////////////////////////////////////////////////////
	// Shader auto-reload on change mechanism
#ifdef _DEBUG
private:
	// The dictionary of watched materials
	static DictionaryString<Material*>	ms_WatchedShaders;
	time_t			m_LastShaderModificationTime;
	time_t			GetFileModTime( const char* _pFileName );

public:
	// Call this every time you need to rebuild shaders whose code has changed
	static void		WatchShadersModifications();
	void			WatchShaderModifications();
#endif
};

