#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"

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


private:	// FIELDS

	const IVertexFormatDescriptor&	m_Format;
	const char*				m_pShaderPath;
	ID3DInclude*			m_pIncludeOverride;

	ID3D11InputLayout*		m_pVertexLayout;

	ID3D11VertexShader*		m_pVS;
	ShaderConstants			m_VSConstants;

	ID3D11GeometryShader*	m_pGS;
	ShaderConstants			m_GSConstants;

	ID3D11PixelShader*		m_pPS;
	ShaderConstants			m_PSConstants;

	bool					m_bHasErrors;

	Dictionary<const char*>	m_Pointer2FileName;


public:	 // PROPERTIES

	bool				HasErrors() const	{ return m_bHasErrors; }
	ID3D11InputLayout*  GetVertexLayout()	{ return m_pVertexLayout; }

public:	 // METHODS

	Material( Device& _Device, const IVertexFormatDescriptor& _Format, const char* _pShaderFileName, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride );
	~Material();

	void			SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer );
	void			SetConstantBuffer( int _BufferSlot, ConstantBuffer& _Buffer );
	void			SetTexture( const char* _pTextureName, ID3D11ShaderResourceView* _pData );
	void			SetTexture( int _BufferSlot, ID3D11ShaderResourceView* _pData );

	void			Use();

public:	// ID3DInclude Members

    STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
    STDMETHOD(Close)( THIS_ LPCVOID _pData );

private:

	ID3DBlob*		CompileShader( const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget );
	const char*		GetShaderPath( const char* _pShaderFileName ) const;
};

