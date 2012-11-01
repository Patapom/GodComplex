#include "ConstantBuffer.h"

ConstantBuffer::ConstantBuffer( Device& _Device, int _Size, void* _pData ) : Component( _Device )
{
	m_Size = _Size;

	// Pad to 16
	_Size = (_Size+15) & ~0xF;
	m_PaddedSize = _Size;

	// Create the vertex buffer
	D3D11_BUFFER_DESC   Desc;
	Desc.ByteWidth = _Size;
	Desc.Usage = _pData != NULL ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DYNAMIC;
	Desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	Desc.CPUAccessFlags = _pData == NULL ? D3D11_CPU_ACCESS_WRITE : 0;
	Desc.MiscFlags = 0;
	Desc.StructureByteStride = 0;

	if ( _pData != NULL )
	{
		D3D11_SUBRESOURCE_DATA  InitData;
		InitData.pSysMem = _pData;
		InitData.SysMemPitch = 0;
		InitData.SysMemSlicePitch = 0;

		Check( m_Device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pBuffer ) );
	}
	else
		Check( m_Device.DXDevice().CreateBuffer( &Desc, NULL, &m_pBuffer ) );
}

ConstantBuffer::~ConstantBuffer()
{
	ASSERT( m_pBuffer != NULL, "Invalid constant buffer to destroy !" );
	m_pBuffer->Release(); m_pBuffer = NULL;
}

void	ConstantBuffer::UpdateData( const void* _pData )
{
	D3D11_MAPPED_SUBRESOURCE	SubResource;
	m_Device.DXContext().Map( m_pBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &SubResource );

	memcpy( SubResource.pData, _pData, m_Size );

	m_Device.DXContext().Unmap( m_pBuffer, 0 );
}

void	ConstantBuffer::Set( int _SlotIndex )
{
	m_Device.DXContext().VSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	m_Device.DXContext().HSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	m_Device.DXContext().DSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	m_Device.DXContext().GSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	m_Device.DXContext().PSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
	m_Device.DXContext().CSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
}
void	ConstantBuffer::SetVS( int _SlotIndex )
{
	m_Device.DXContext().VSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
}
void	ConstantBuffer::SetHS( int _SlotIndex )
{
	m_Device.DXContext().HSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
}
void	ConstantBuffer::SetDS( int _SlotIndex )
{
	m_Device.DXContext().DSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
}
void	ConstantBuffer::SetGS( int _SlotIndex )
{
	m_Device.DXContext().GSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
}
void	ConstantBuffer::SetPS( int _SlotIndex )
{
	m_Device.DXContext().PSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
}
void	ConstantBuffer::SetCS( int _SlotIndex )
{
	m_Device.DXContext().CSSetConstantBuffers( _SlotIndex, 1, &m_pBuffer );
}
