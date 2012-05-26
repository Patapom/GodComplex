#include "Primitive.h"

Primitive::Primitive( Device& _Device, int _VerticesCount, void* _pVertices, int _IndicesCount, U16* _pIndices, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format ) : Component( _Device )
	, m_VerticesCount( _VerticesCount )
	, m_IndicesCount( _IndicesCount )
	, m_Format( _Format )
	, m_Topology( _Topology )
	, m_pVB( NULL )
	, m_pIB( NULL )
{
	{   // Create the vertex buffer
		D3D11_BUFFER_DESC   Desc;
		Desc.ByteWidth = _VerticesCount * _Format.Size();
		Desc.Usage = D3D11_USAGE_IMMUTABLE;
		Desc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
		Desc.CPUAccessFlags = 0;
		Desc.MiscFlags = 0;
		Desc.StructureByteStride = 0;

		D3D11_SUBRESOURCE_DATA  InitData;
		InitData.pSysMem = _pVertices;
		InitData.SysMemPitch = 0;
		InitData.SysMemSlicePitch = 0;

		Check( m_Device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pVB ) );
	}

	if ( _pIndices != NULL )
	{   // Create the index buffer
		D3D11_BUFFER_DESC   Desc;
		Desc.ByteWidth = _IndicesCount * sizeof(U16);		 // For now, we only support U16 primitives
		Desc.Usage = D3D11_USAGE_IMMUTABLE;
		Desc.BindFlags = D3D11_BIND_INDEX_BUFFER;
		Desc.CPUAccessFlags = 0;
		Desc.MiscFlags = 0;
		Desc.StructureByteStride = 0;

		D3D11_SUBRESOURCE_DATA  InitData;
		InitData.pSysMem = _pIndices;
		InitData.SysMemPitch = 0;
		InitData.SysMemSlicePitch = 0;

		Check( m_Device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pIB ) );
	}

	switch ( m_Topology )
	{
	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST:
		m_FacesCount = _pIndices != NULL ? _IndicesCount / 3 : _VerticesCount / 3;
		break;

	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP:
		m_FacesCount = _pIndices != NULL ? _IndicesCount - 2 : _VerticesCount - 2;
		break;

	default:
		ASSERT( FALSE, "Unsupported primitive type !" );
	}
}

Primitive::~Primitive()
{
	ASSERT( m_pVB != NULL, "Invalid vertex buffer to destroy !" );

	m_pVB->Release(); m_pVB = NULL;
	if ( m_pIB != NULL ) m_pIB->Release(); m_pIB = NULL;
}

void	Primitive::Render( Material& _Material )
{
	m_Device.DXContext().IASetInputLayout( _Material.GetVertexLayout() );
	m_Device.DXContext().IASetPrimitiveTopology( m_Topology );

	U32 StrideOffset = 0;
	m_Device.DXContext().IASetVertexBuffers( 0, 1, &m_pVB, &StrideOffset, &StrideOffset );
	if ( m_pIB != NULL )
		m_Device.DXContext().IASetIndexBuffer( m_pIB, DXGI_FORMAT_R16_UINT, 0 );
	else
		m_Device.DXContext().IASetIndexBuffer( NULL, DXGI_FORMAT_UNKNOWN, 0 );

	if ( m_pIB != NULL )
		m_Device.DXContext().DrawIndexed( m_IndicesCount, 0, 0 );
	else
		m_Device.DXContext().Draw( m_VerticesCount, 0 );
}
