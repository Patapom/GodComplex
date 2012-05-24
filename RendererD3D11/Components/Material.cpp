
#include "Material.h"
#include "ConstantBuffer.h"

#include <stdio.h>
#include "D3Dcompiler.h"
#include "D3D11Shader.h"


Material::Material( Device& _Device, const VertexFormatDescriptor& _Format, const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPointVS, const char* _pEntryPointGS, const char* _pEntryPointPS, ID3DInclude* _pIncludeOverride ) : Component( _Device ), m_Format( _Format )
{
	m_pIncludeOverride = _pIncludeOverride;

	// Compile the compulsory vertex shader
	ASSERT( _pEntryPointVS != NULL, "Invalid VertexShader entry point !" );
	ID3DBlob*   pShader = CompileShader( _pShaderCode, _pMacros, _pEntryPointVS, "vs_4_0" );
	Check( m_Device.DXDevice()->CreateVertexShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pVS ) );
	ASSERT( m_pVS != NULL, "Failed to create vertex shader !" );
	m_VSConstants.Enumerate( *pShader );

	// Create the vertex layout
	Check( m_Device.DXDevice()->CreateInputLayout( _Format.GetInputElements(), _Format.GetInputElementsCount(), pShader->GetBufferPointer(), pShader->GetBufferSize(), &m_pVertexLayout ) );

	// Compile the optional geometry shader
	if ( _pEntryPointGS != NULL )
	{
		ID3DBlob*   pShader = CompileShader( _pShaderCode, _pMacros, _pEntryPointGS, "gs_4_0" );
		Check( m_Device.DXDevice()->CreateGeometryShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pGS ) );
		ASSERT( m_pGS != NULL, "Failed to create geometry shader !" );
		m_GSConstants.Enumerate( *pShader );
	}
	else
		m_pGS = NULL;

	// Compile the optional pixel shader
	if ( _pEntryPointPS != NULL )
	{
		ID3DBlob*   pShader = CompileShader( _pShaderCode, _pMacros, _pEntryPointPS, "ps_4_0" );
		Check( m_Device.DXDevice()->CreatePixelShader( pShader->GetBufferPointer(), pShader->GetBufferSize(), NULL, &m_pPS ) );
		ASSERT( m_pPS != NULL, "Failed to create pixel shader !" );
		m_PSConstants.Enumerate( *pShader );
	}
	else
		m_pPS = NULL;
}

Material::~Material()
{
	ASSERT( m_pVertexLayout != NULL, "Invalid Vertex Layout to destroy !" );

	m_pVertexLayout->Release(); m_pVertexLayout = NULL;
	m_pVS->Release(); m_pVS = NULL;
	if ( m_pGS != NULL ) m_pGS->Release(); m_pGS = NULL;
	if ( m_pPS != NULL ) m_pPS->Release(); m_pPS = NULL;
}

void	Material::Use()
{
	m_Device.DXContext()->IASetInputLayout( m_pVertexLayout );

	m_Device.DXContext()->VSSetShader( m_pVS, NULL, 0 );
	if ( m_pGS != NULL )
		m_Device.DXContext()->GSSetShader( m_pGS, NULL, 0 );
	if ( m_pPS != NULL )
		m_Device.DXContext()->PSSetShader( m_pPS, NULL, 0 );
}

ID3DBlob*   Material::CompileShader( const char* _pShaderCode, D3D_SHADER_MACRO* _pMacros, const char* _pEntryPoint, const char* _pTarget )
{
	ID3DBlob*   pCodeText;
	ID3DBlob*   pCode;
	ID3DBlob*   pErrors;

	Check( D3DPreprocess( _pShaderCode, strlen(_pShaderCode), NULL, _pMacros, this, &pCodeText, &pErrors ) );
	ASSERT( pErrors == NULL, "Shader preprocess error !" );

	U32 Flags1 = 0;
#ifdef _DEBUG
		Flags1 |= D3D10_SHADER_DEBUG;
		Flags1 |= D3D10_SHADER_SKIP_OPTIMIZATION;
#else
		Flags1 |= D3D10_SHADER_OPTIMIZATION_LEVEL3;
#endif
		Flags1 |= D3D10_SHADER_ENABLE_STRICTNESS;
		Flags1 |= D3D10_SHADER_IEEE_STRICTNESS;

	U32 Flags2 = 0;

	Check( D3DCompile( pCodeText->GetBufferPointer(), pCodeText->GetBufferSize(), NULL, _pMacros, this, _pEntryPoint, _pTarget, Flags1, Flags2, &pCode, &pErrors ) );
	ASSERT( pErrors == NULL, "Shader compilation error !" );
	ASSERT( pCode != NULL, "Shader compilation failed => No error provided but didn't output any shader either !" );

	return pCode;
}

#ifndef FORBID_SHADER_INCLUDE

HRESULT	Material::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes )
{
	FILE*	pFile;
	fopen_s( &pFile, _pFileName, "r" );

	fseek( pFile, 0, SEEK_END );
	*_pBytes = ftell( pFile );
	fseek( pFile, 0, SEEK_SET );

	*_ppData = new char[*_pBytes];
	fread_s( const_cast<void*>( *_ppData ), *_pBytes, 1, *_pBytes, pFile );

	fclose( pFile );

	return S_OK;
}

HRESULT	Material::Close( THIS_ LPCVOID pData )
{
	delete[] pData;

	return S_OK;
}

#else

HRESULT	Material::Open( THIS_ D3D_INCLUDE_TYPE _IncludeType, LPCSTR _pFileName, LPCVOID _pParentData, LPCVOID* _ppData, UINT* _pBytes )
{
	ASSERT( m_pIncludeOverride != NULL, "You MUST provide an ID3DINCLUDE override when compiling with the FORBID_SHADER_INCLUDE option !" );
	return m_pIncludeOverride->Open( _IncludeType, _pFileName, _pParentData, _ppData, _pBytes );
}
HRESULT	Material::Close( THIS_ LPCVOID _pData )
{
	return m_pIncludeOverride->Close( _pData );
}

#endif

void	Material::SetConstantBuffer( const char* _pBufferName, ConstantBuffer& _Buffer )
{
	ID3D11Buffer*	pBuffer = _Buffer.GetBuffer();
	{
		int	SlotIndex = m_VSConstants.GetBufferIndex( _pBufferName );
		if ( SlotIndex != -1 )
			m_Device.DXContext()->VSSetConstantBuffers( SlotIndex, 1, &pBuffer );
	}
}

void	Material::SetTexture( const char* _pBufferName, ID3D11ShaderResourceView* _pData )
{
	ID3D11ShaderResourceView*	pView = _pData;
	{
		int	SlotIndex = m_VSConstants.GetShaderResourceViewIndex( _pBufferName );
		if ( SlotIndex != -1 )
			m_Device.DXContext()->VSSetShaderResources( SlotIndex, 1, &pView );
	}
}

Material::ShaderConstants::~ShaderConstants()
{
	if ( m_ppConstantBufferNames != NULL )
	{
		for ( int CBIndex=0; CBIndex < m_ConstantBuffersCount; CBIndex++ )
			delete[] m_ppConstantBufferNames[CBIndex];
		delete[] m_ppConstantBufferNames;
	}
	if ( m_ppShaderResourceViewNames != NULL )
	{
		for ( int SRVIndex=0; SRVIndex < m_ShaderResourceViewsCount; SRVIndex++ )
			delete[] m_ppShaderResourceViewNames[SRVIndex];
		delete[] m_ppShaderResourceViewNames;
	}
}

void	Material::ShaderConstants::Enumerate( ID3DBlob& _ShaderBlob )
{
	ID3D11ShaderReflection*	pReflector = NULL; 
	D3DReflect( _ShaderBlob.GetBufferPointer(), _ShaderBlob.GetBufferSize(), IID_ID3D11ShaderReflection, (void**) &pReflector );

	D3D11_SHADER_DESC	ShaderDesc;
	pReflector->GetDesc( &ShaderDesc );

	// Enumerate constant buffers
	m_ConstantBuffersCount = ShaderDesc.ConstantBuffers;
	m_ppConstantBufferNames = new char*[m_ConstantBuffersCount];
	for ( int CBIndex=0; CBIndex < m_ConstantBuffersCount; CBIndex++ )
	{
		ID3D11ShaderReflectionConstantBuffer*	pCB = pReflector->GetConstantBufferByIndex( CBIndex );

		D3D11_SHADER_BUFFER_DESC	CBDesc;
		pCB->GetDesc( &CBDesc );

		int		NameLength = strlen(CBDesc.Name)+1;
		m_ppConstantBufferNames[CBIndex] = new char[NameLength];
		memcpy( m_ppConstantBufferNames[CBIndex], &CBDesc.Name, NameLength );
	}

	// Enumerate textures
	m_ShaderResourceViewsCount = ShaderDesc.BoundResources;
	m_ppShaderResourceViewNames = new char*[m_ShaderResourceViewsCount];
	for ( int SRVIndex=0; SRVIndex < m_ShaderResourceViewsCount; SRVIndex++ )
	{
		D3D11_SHADER_INPUT_BIND_DESC	SRVDesc;
		pReflector->GetResourceBindingDesc( SRVIndex, &SRVDesc );

		int		NameLength = strlen(SRVDesc.Name)+1;
		m_ppShaderResourceViewNames[SRVIndex] = new char[NameLength];
		memcpy( m_ppShaderResourceViewNames[SRVIndex], &SRVDesc.Name, NameLength );
	}

	pReflector->Release();
}

int		Material::ShaderConstants::GetBufferIndex( const char* _pBufferName ) const
{
	for ( int CBIndex=0; CBIndex < m_ConstantBuffersCount; CBIndex++ )
		if ( !strcmp( _pBufferName, m_ppConstantBufferNames[CBIndex] ) )
			return CBIndex;

	return -1;
}

int		Material::ShaderConstants::GetShaderResourceViewIndex( const char* _pTextureName ) const
{
	for ( int SRVIndex=0; SRVIndex < m_ShaderResourceViewsCount; SRVIndex++ )
		if ( !strcmp( _pTextureName, m_ppShaderResourceViewNames[SRVIndex] ) )
			return SRVIndex;

	return -1;
}
