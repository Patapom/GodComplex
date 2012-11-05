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
	Build( _pVertices, _pIndices, false );
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

Primitive::Primitive( Device& _Device, int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format ) : Component( _Device )
	, m_VerticesCount( _VerticesCount )
	, m_IndicesCount( _IndicesCount )
	, m_Format( _Format )
	, m_Topology( _Topology )
	, m_pVB( NULL )
	, m_pIB( NULL )
{
	m_Stride = _Format.Size();
	Build( NULL, NULL, true );
}

Primitive::~Primitive()
{
	ASSERT( m_pVB != NULL, "Invalid vertex buffer to destroy !" );

	m_pVB->Release(); m_pVB = NULL;
	if ( m_pIB != NULL ) m_pIB->Release(); m_pIB = NULL;
}

void	Primitive::Render( Material& _Material )
{
	Render( _Material, 0, m_VerticesCount, 0, m_IndicesCount, 0 );
}
void	Primitive::Render( Material& _Material, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset )
{
	ID3D11InputLayout*	pLayout = _Material.GetVertexLayout();
	if ( pLayout == NULL )
		return;	// Material is not initialied yet...

	// Ensure material & primitive use the same vertex format
	ASSERT( m_Format == _Material.GetFormat(), "Material and Primimtive must use the same vertex format !" );

	m_Device.DXContext().IASetInputLayout( pLayout );
	m_Device.DXContext().IASetPrimitiveTopology( m_Topology );

	U32 Offset = 0;
	m_Device.DXContext().IASetVertexBuffers( 0, 1, &m_pVB, &m_Stride, &Offset );
	if ( m_pIB != NULL )
		m_Device.DXContext().IASetIndexBuffer( m_pIB, DXGI_FORMAT_R16_UINT, 0 );
	else
		m_Device.DXContext().IASetIndexBuffer( NULL, DXGI_FORMAT_UNKNOWN, 0 );

	if ( m_pIB != NULL )
		m_Device.DXContext().DrawIndexed( _IndicesCount, _StartIndex, _BaseVertexOffset );
	else
		m_Device.DXContext().Draw( _VerticesCount, _StartVertex );
}

void	Primitive::RenderInstanced( Material& _Material, int _InstancesCount )
{
	RenderInstanced( _Material, _InstancesCount, 0, m_VerticesCount, 0, m_IndicesCount, 0 );
}
void	Primitive::RenderInstanced( Material& _Material, int _InstancesCount, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset )
{
	ID3D11InputLayout*	pLayout = _Material.GetVertexLayout();
	if ( pLayout == NULL )
		return;	// Material is not initialied yet...

	m_Device.DXContext().IASetInputLayout( pLayout );
	m_Device.DXContext().IASetPrimitiveTopology( m_Topology );

	U32 Offset = 0;
	m_Device.DXContext().IASetVertexBuffers( 0, 1, &m_pVB, &m_Stride, &Offset );
	if ( m_pIB != NULL )
		m_Device.DXContext().IASetIndexBuffer( m_pIB, DXGI_FORMAT_R16_UINT, 0 );
	else
		m_Device.DXContext().IASetIndexBuffer( NULL, DXGI_FORMAT_UNKNOWN, 0 );

	if ( m_pIB != NULL )
		m_Device.DXContext().DrawIndexedInstanced( _IndicesCount, _InstancesCount, _StartIndex, _BaseVertexOffset, 0 );
	else
		m_Device.DXContext().DrawInstanced( _VerticesCount, _InstancesCount, _StartVertex, 0 );
}

void	Primitive::Build( void* _pVertices, U16* _pIndices, bool _bDynamic )
{
	ASSERT( m_VerticesCount <= 65536, "Time to upgrade to U32 indices !" );

	{   // Create the vertex buffer
		D3D11_BUFFER_DESC   Desc;
		Desc.ByteWidth = m_VerticesCount * m_Stride;
		Desc.Usage = _bDynamic ? D3D11_USAGE_DYNAMIC : D3D11_USAGE_IMMUTABLE;
		Desc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
		Desc.CPUAccessFlags = _bDynamic ? D3D11_CPU_ACCESS_WRITE : 0;
		Desc.MiscFlags = 0;
		Desc.StructureByteStride = 0;

		if ( !_bDynamic )
		{
			D3D11_SUBRESOURCE_DATA	InitData;
			InitData.pSysMem = _pVertices;
			InitData.SysMemPitch = 0;
			InitData.SysMemSlicePitch = 0;

			Check( m_Device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pVB ) );
		}
		else
			Check( m_Device.DXDevice().CreateBuffer( &Desc, NULL, &m_pVB ) );
	}

	if ( _pIndices != NULL )
	{   // Create the index buffer
		D3D11_BUFFER_DESC   Desc;
		Desc.ByteWidth = m_IndicesCount * sizeof(U16);		 // For now, we only support U16 primitives
		Desc.Usage = _bDynamic ? D3D11_USAGE_DYNAMIC : D3D11_USAGE_IMMUTABLE;
		Desc.BindFlags = D3D11_BIND_INDEX_BUFFER;
		Desc.CPUAccessFlags = _bDynamic ? D3D11_CPU_ACCESS_WRITE : 0;
		Desc.MiscFlags = 0;
		Desc.StructureByteStride = 0;

		if ( !_bDynamic )
		{
			D3D11_SUBRESOURCE_DATA	InitData;
			InitData.pSysMem = _pIndices;
			InitData.SysMemPitch = 0;
			InitData.SysMemSlicePitch = 0;

			Check( m_Device.DXDevice().CreateBuffer( &Desc, &InitData, &m_pIB ) );
		}
		else
			Check( m_Device.DXDevice().CreateBuffer( &Desc, NULL, &m_pIB ) );
	}

	switch ( m_Topology )
	{
	case D3D11_PRIMITIVE_TOPOLOGY_POINTLIST:
		m_FacesCount = _pIndices != NULL ? m_IndicesCount : m_VerticesCount;
		break;

	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST:
		m_FacesCount = _pIndices != NULL ? m_IndicesCount / 3 : m_VerticesCount / 3;
		break;

	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP:
		m_FacesCount = _pIndices != NULL ? m_IndicesCount - 2 : m_VerticesCount - 2;
		break;

	default:
		if ( m_Topology >= D3D11_PRIMITIVE_TOPOLOGY_1_CONTROL_POINT_PATCHLIST && m_Topology <= D3D11_PRIMITIVE_TOPOLOGY_32_CONTROL_POINT_PATCHLIST )
		{	// For patches, it depends on the amount of control points
			int	ControlPointsPerPatch = 1 + (m_Topology - D3D11_PRIMITIVE_TOPOLOGY_1_CONTROL_POINT_PATCHLIST);
			m_FacesCount = (_pIndices != NULL ? m_IndicesCount : m_VerticesCount) / ControlPointsPerPatch;
		}
		else
			ASSERT( FALSE, "Unsupported primitive type !" );
	}
}

void	Primitive::UpdateDynamic( void* _pVertices, U16* _pIndices, int _VerticesCount, int _IndicesCount )
{
	ASSERT( _pVertices != NULL, "Invalid vertices !" );
	{
		D3D11_MAPPED_SUBRESOURCE	SubResource;
		Device::Check( m_Device.DXContext().Map( m_pVB, 0, D3D11_MAP_WRITE_DISCARD, 0, &SubResource ) );
		memcpy( SubResource.pData, _pVertices, (_VerticesCount != -1 ? _VerticesCount : m_VerticesCount) * m_Stride );
		m_Device.DXContext().Unmap( m_pVB, 0 );
	}

	if ( _pIndices != NULL )
	{
		D3D11_MAPPED_SUBRESOURCE	SubResource;
		Device::Check( m_Device.DXContext().Map( m_pIB, 0, D3D11_MAP_WRITE_DISCARD, 0, &SubResource ) );
		memcpy( SubResource.pData, _pVertices, (_IndicesCount != -1 ? _IndicesCount : m_IndicesCount) * sizeof(U16) );
		m_Device.DXContext().Unmap( m_pIB, 0 );
	}
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
	Build( _pVertices, (U16*) _pIndices, false );

	delete[] _pVertices;
	delete[] _pIndices;
}

#endif