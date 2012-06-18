#include "Primitive.h"

Primitive::Primitive( Device& _Device, int _VerticesCount, void* _pVertices, int _IndicesCount, U16* _pIndices, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format ) : Component( _Device )
	, m_VerticesCount( _VerticesCount )
	, m_IndicesCount( _IndicesCount )
	, m_Format( _Format )
	, m_Topology( _Topology )
	, m_pVB( NULL )
	, m_pIB( NULL )
{
	m_Stride = _Format.Size();
	Build( _pVertices, _pIndices );
}

Primitive::Primitive( Device& _Device, const IVertexFormatDescriptor& _Format ) : Component( _Device )
	, m_Format( _Format )
	, m_VerticesCount( 0 )
	, m_IndicesCount( 0 )
	, m_Topology( D3D11_PRIMITIVE_TOPOLOGY_UNDEFINED )
	, m_pVB( NULL )
	, m_pIB( NULL )
{
	m_Stride = _Format.Size();
	// Deferred construction...
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

	U32 Offset = 0;
	m_Device.DXContext().IASetVertexBuffers( 0, 1, &m_pVB, &m_Stride, &Offset );
	if ( m_pIB != NULL )
		m_Device.DXContext().IASetIndexBuffer( m_pIB, DXGI_FORMAT_R16_UINT, 0 );
	else
		m_Device.DXContext().IASetIndexBuffer( NULL, DXGI_FORMAT_UNKNOWN, 0 );

	if ( m_pIB != NULL )
		m_Device.DXContext().DrawIndexed( m_IndicesCount, 0, 0 );
	else
		m_Device.DXContext().Draw( m_VerticesCount, 0 );
}

#ifdef SUPPORT_GEO_BUILDERS

// IGeometryWriter Implementation
void	Primitive::CreateBuffers( int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _pVertices, void*& _pIndices, int& _VertexStride, int& _IndexStride )
{
	ASSERT( _VerticesCount <= 65536, "Too many vertices !" );	// Time to start accounting for large meshes d00d !

	m_VerticesCount = _VerticesCount;
	m_IndicesCount = _IndicesCount;
	m_Topology = _Topology;

	_VertexStride = m_Format.Size();
	_pVertices = new U8[_VertexStride * m_VerticesCount];

	_IndexStride = sizeof(U16);
	_pIndices = new U16[m_IndicesCount];
}

void	Primitive::WriteVertex( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV )
{
	m_Format.Write( _pVertex, _Position, _Normal, _Tangent, _UV );
}

void	Primitive::WriteIndex( void* _pIndex, int _Index )
{
	ASSERT( _Index < m_VerticesCount, "Index out of range !" );
	*((U16*) _pIndex) = _Index;
}

void	Primitive::Finalize( void* _pVertices, void* _pIndices )
{
	Build( _pVertices, (U16*) _pIndices );

	delete[] _pVertices;
	delete[] _pIndices;
}

void	Primitive::Build( void* _pVertices, U16* _pIndices )
{
	{   // Create the vertex buffer
		D3D11_BUFFER_DESC   Desc;
		Desc.ByteWidth = m_VerticesCount * m_Stride;
		Desc.Usage = D3D11_USAGE_IMMUTABLE;
		Desc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
		Desc.CPUAccessFlags = 0;
		Desc.MiscFlags = 0;
		Desc.StructureByteStride = 0;

		D3D11_SUBRESOURCE_DATA	InitData;
		InitData.pSysMem = _pVertices;
		InitData.SysMemPitch = 0;
		InitData.SysMemSlicePitch = 0;

		Check( m_Device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pVB ) );
	}

	if ( _pIndices != NULL )
	{   // Create the index buffer
		D3D11_BUFFER_DESC   Desc;
		Desc.ByteWidth = m_IndicesCount * sizeof(U16);		 // For now, we only support U16 primitives
		Desc.Usage = D3D11_USAGE_IMMUTABLE;
		Desc.BindFlags = D3D11_BIND_INDEX_BUFFER;
		Desc.CPUAccessFlags = 0;
		Desc.MiscFlags = 0;
		Desc.StructureByteStride = 0;

		D3D11_SUBRESOURCE_DATA	InitData;
		InitData.pSysMem = _pIndices;
		InitData.SysMemPitch = 0;
		InitData.SysMemSlicePitch = 0;

		Check( m_Device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pIB ) );
	}

	switch ( m_Topology )
	{
	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST:
		m_FacesCount = _pIndices != NULL ? m_IndicesCount / 3 : m_VerticesCount / 3;
		break;

	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP:
		m_FacesCount = _pIndices != NULL ? m_IndicesCount - 2 : m_VerticesCount - 2;
		break;

	default:
		ASSERT( FALSE, "Unsupported primitive type !" );
	}
}

#endif