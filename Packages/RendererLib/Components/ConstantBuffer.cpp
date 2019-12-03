#include "stdafx.h"

#include "ConstantBuffer.h"

ConstantBuffer::ConstantBuffer( Device& _device, int _size, void* _pData, bool _isConstantBuffer )
	: Component( _device )
	, m_pShaderResourceView( NULL )
{
	m_IsConstantBuffer = _isConstantBuffer;
	m_Size = _size;

	// Pad to 16
	_size = (_size+15) & ~0xF;
	m_PaddedSize = _size;

	// Create the vertex buffer
	D3D11_BUFFER_DESC   Desc;
	Desc.ByteWidth = _size;
	Desc.Usage = _pData != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DYNAMIC;
	Desc.BindFlags = m_IsConstantBuffer ? D3D11_BIND_CONSTANT_BUFFER : D3D11_BIND_SHADER_RESOURCE;
	Desc.CPUAccessFlags = _pData == NULL ? D3D11_CPU_ACCESS_WRITE : 0;
	Desc.MiscFlags = 0;
	Desc.StructureByteStride = 0;

	if ( _pData != NULL )
	{
		D3D11_SUBRESOURCE_DATA  InitData;
		InitData.pSysMem = _pData;
		InitData.SysMemPitch = 0;
		InitData.SysMemSlicePitch = 0;

		Check( m_device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pBuffer ) );
	}
	else
		Check( m_device.DXDevice().CreateBuffer( &Desc, NULL, &m_pBuffer ) );

	// Create the shader resource view if it's a tbuffer
	if ( m_IsConstantBuffer )
		return;

	D3D11_SHADER_RESOURCE_VIEW_DESC	ViewDesc;
	ViewDesc.Format = DXGI_FORMAT_R32G32B32A32_UINT;
	ViewDesc.ViewDimension = D3D11_SRV_DIMENSION_BUFFER;
	ViewDesc.Buffer.ElementOffset = 0;
	ViewDesc.Buffer.ElementWidth = _size >> 4;

	Check( m_device.DXDevice().CreateShaderResourceView( m_pBuffer, &ViewDesc, &m_pShaderResourceView ) );
}

ConstantBuffer::~ConstantBuffer() {
	ASSERT( m_pBuffer != NULL, "Invalid constant buffer to destroy!" );
	m_pBuffer->Release(); m_pBuffer = NULL;

	if ( m_pShaderResourceView )
		m_pShaderResourceView->Release();
	m_pShaderResourceView = NULL;
}

void	ConstantBuffer::UpdateData( const void* _pData ) {
	D3D11_MAPPED_SUBRESOURCE	SubResource;
	m_device.DXContext().Map( m_pBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &SubResource );

	memcpy( SubResource.pData, _pData, m_Size );

	m_device.DXContext().Unmap( m_pBuffer, 0 );
}

void	ConstantBuffer::Set( int _SlotIndex ) {
	SetVS( _SlotIndex );
	SetHS( _SlotIndex );
	SetDS( _SlotIndex );
	SetGS( _SlotIndex );
	SetPS( _SlotIndex );
	SetCS( _SlotIndex );
}
void	ConstantBuffer::SetVS( int _SlotIndex ) {
	if ( m_IsConstantBuffer )
		m_device.DXContext().VSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	else
		m_device.DXContext().VSSetShaderResources( _SlotIndex, 1, &m_pShaderResourceView );
}
void	ConstantBuffer::SetHS( int _SlotIndex ) {
	if ( m_IsConstantBuffer )
		m_device.DXContext().HSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	else
		m_device.DXContext().HSSetShaderResources( _SlotIndex, 1, &m_pShaderResourceView );
}
void	ConstantBuffer::SetDS( int _SlotIndex ) {
	if ( m_IsConstantBuffer )
		m_device.DXContext().DSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	else
		m_device.DXContext().DSSetShaderResources( _SlotIndex, 1, &m_pShaderResourceView );
}
void	ConstantBuffer::SetGS( int _SlotIndex ) {
	if ( m_IsConstantBuffer )
		m_device.DXContext().GSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	else
		m_device.DXContext().GSSetShaderResources( _SlotIndex, 1, &m_pShaderResourceView );
}
void	ConstantBuffer::SetPS( int _SlotIndex ) {
	if ( m_IsConstantBuffer )
		m_device.DXContext().PSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	else
		m_device.DXContext().PSSetShaderResources( _SlotIndex, 1, &m_pShaderResourceView );
}
void	ConstantBuffer::SetCS( int _SlotIndex ) {
	if ( m_IsConstantBuffer )
		m_device.DXContext().CSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	else
		m_device.DXContext().CSSetShaderResources( _SlotIndex, 1, &m_pShaderResourceView );
}
