#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"

class ConstantBuffer;

class Material : public Component, ID3DInclude
{
protected:  // NESTED TYPES

	class	ShaderConstants
	{
	public:
		int		m_ConstantBuffersCount;
		char**	m_ppConstantBufferNames;
		int		m_ShaderResourceViewsCount;
		char**	m_ppShaderResourceViewNames;

		ShaderConstants() : m_ConstantBuffersCount( 0 ), m_ppConstantBufferNames( NULL ), m_ShaderResourceViewsCount( 0 ), m_ppShaderResourceViewNames( NULL ) {}
		~ShaderConstants();

		void	Enumerate( ID3DBlob& _ShaderBlob );
		int		GetBufferIndex( const char* _pBufferName ) const;
		int		GetShaderResourceViewIndex( const char* _pTextureName ) const;
	};


private:	// FIELDS

	const VertexFormatDescriptor&   m_Format;
	ID3DInclude*			m_pIncludeOverride;

	ID3D11InputLayout*		m_pVertexLayout;

	ID3D11VertexShader*		m_pVS;
	ShaderConstants			m_VSConstants;

	ID3D11GeometryShader*	m_pGS;
	ShaderConstants			m_GSConstants;

	ID3D11PixelShader*		m_pPS;
	ShaderConstants			m_PSConstants;

public:	 // PROPERTIES

	ID3D11InputLayout*  GetVertexLayout()	{ return m_pVertexLayout; }

public:	 // METHODS

	Material( Device& _Device, const VertexFormatDescriptor& _Format, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride );
	~Material();

	void			SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer );
	void			SetTexture( const char* _pTextureName, ID3D11ShaderResourceView* _pData );

	void			Use();

public:	// ID3DInclude Members

    STDMETHOD(Open)( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes );
    STDMETHOD(Close)( THIS_ LPCVOID _pData );

private:

	ID3DBlob*   CompileShader( const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget );
};

