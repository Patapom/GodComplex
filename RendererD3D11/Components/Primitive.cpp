#include "stdafx.h"

#include "Primitive.h"

Primitive::Primitive( Device& _device, U32 _verticesCount, const void* _vertices, U32 _indicesCount, const U32* _indices, D3D11_PRIMITIVE_TOPOLOGY _topology, const IVertexFormatDescriptor& _format, bool _allowSRV, bool _allowUAV ) : Component( _device )
	, m_verticesCount( _verticesCount )
	, m_indicesCount( _indicesCount )
	, m_format( _format )
	, m_topology( _topology )
	, m_VB( NULL )
	, m_IB( NULL )
	, m_boundVertexStreamsCount( 0 )
	, m_cachedVB_SRV( NULL )
	, m_cachedVB_UAV( NULL )
	, m_cachedIB_SRV( NULL )
	, m_cachedIB_UAV( NULL )
{
	m_stride = _format.Size();
	Build( _vertices, _indices, false, _allowSRV, _allowUAV );
}

Primitive::Primitive( Device& _device, const IVertexFormatDescriptor& _format ) : Component( _device )
	, m_format( _format )
	, m_verticesCount( 0 )
	, m_indicesCount( 0 )
	, m_topology( D3D11_PRIMITIVE_TOPOLOGY_UNDEFINED )
	, m_VB( NULL )
	, m_IB( NULL )
	, m_boundVertexStreamsCount( 0 )
	, m_cachedVB_SRV( NULL )
	, m_cachedVB_UAV( NULL )
	, m_cachedIB_SRV( NULL )
	, m_cachedIB_UAV( NULL )
{
	m_stride = _format.Size();
	// Deferred construction...
}

Primitive::Primitive( Device& _device, U32 _verticesCount, U32 _indicesCount, D3D11_PRIMITIVE_TOPOLOGY _topology, const IVertexFormatDescriptor& _format, bool _allowSRV, bool _allowUAV ) : Component( _device )
	, m_verticesCount( _verticesCount )
	, m_indicesCount( _indicesCount )
	, m_format( _format )
	, m_topology( _topology )
	, m_VB( NULL )
	, m_IB( NULL )
	, m_boundVertexStreamsCount( 0 )
	, m_cachedVB_SRV( NULL )
	, m_cachedVB_UAV( NULL )
	, m_cachedIB_SRV( NULL )
	, m_cachedIB_UAV( NULL )
{
	m_stride = _format.Size();
	Build( NULL, NULL, true, _allowSRV, _allowUAV );
}

Primitive::~Primitive() {
	ASSERT( m_VB != NULL, "Invalid vertex buffer to destroy !" );

	SAFE_RELEASE( m_cachedVB_SRV );
	SAFE_RELEASE( m_cachedVB_UAV );
	SAFE_RELEASE( m_cachedIB_SRV );
	SAFE_RELEASE( m_cachedIB_UAV );

	m_VB->Release(); m_VB = NULL;
	if ( m_IB != NULL ) m_IB->Release(); m_IB = NULL;
}

void	Primitive::Render( Shader& _material ) {
	Render( _material, 0, m_verticesCount, 0, m_indicesCount, 0 );
}
void	Primitive::Render( Shader& _material, int _startVertex, int _verticesCount, int _startIndex, int _indicesCount, int _baseVertexOffset ) {
	ASSERT( m_device.CurrentMaterial() == &_material, "Attempting to render with a material that is not the currently used material!" );

	ID3D11InputLayout*	layout = _material.GetVertexLayout();
	if ( layout == NULL )
		return;	// Material is not initialied yet...

	// Ensure material & primitive use the same vertex format
	const IVertexFormatDescriptor&	primitiveFormat = m_boundVertexStreamsCount == 1 ? m_format : m_compositeFormat;
//	ASSERT( PrimitiveFormat == _material.GetFormat(), "Material and Primitive must use the same vertex format !" );
	ASSERT( _material.GetFormat().IsSubset( primitiveFormat ), "Material and Primitive must use a compatible vertex format!" );

	m_device.DXContext().IASetInputLayout( layout );
	m_device.DXContext().IASetPrimitiveTopology( m_topology );

	U32 Offset = 0;
//	m_Device.DXContext().IASetVertexBuffers( 0, 1, &m_pVB, &m_Stride, &Offset );
	m_device.DXContext().IASetVertexBuffers( 0, m_boundVertexStreamsCount, m_vertexBuffers, m_strides, m_offsets );

	if ( m_IB != NULL ) {
		m_device.DXContext().IASetIndexBuffer( m_IB, DXGI_FORMAT_R32_UINT, 0 );
		m_device.DXContext().DrawIndexed( _indicesCount, _startIndex, _baseVertexOffset );
	} else {
		m_device.DXContext().IASetIndexBuffer( NULL, DXGI_FORMAT_UNKNOWN, 0 );
		m_device.DXContext().Draw( _verticesCount, _startVertex );
	}
}

void	Primitive::RenderInstanced( Shader& _material, U32 _instancesCount ) {
	RenderInstanced( _material, _instancesCount, 0, m_verticesCount, 0, m_indicesCount, 0 );
}
void	Primitive::RenderInstanced( Shader& _material, U32 _instancesCount, U32 _startVertex, U32 _verticesCount, U32 _startIndex, U32 _indicesCount, U32 _baseVertexOffset ) {
	ASSERT( m_device.CurrentMaterial() == &_material, "Attempting to render with a material that is not the currently used material!" );

	ID3D11InputLayout*	layout = _material.GetVertexLayout();
	if ( layout == NULL )
		return;	// Material is not initialied yet...

	// Ensure material & primitive use the same vertex format
	const IVertexFormatDescriptor&	primitiveFormat = m_boundVertexStreamsCount == 1 ? m_format : m_compositeFormat;
//	ASSERT( PrimitiveFormat == _material.GetFormat(), "Material and Primitive must use the same vertex format !" );
	ASSERT( _material.GetFormat().IsSubset( primitiveFormat ), "Material and Primitive must use a compatible vertex format!" );

	m_device.DXContext().IASetInputLayout( layout );
	m_device.DXContext().IASetPrimitiveTopology( m_topology );

	U32 Offset = 0;
//	m_Device.DXContext().IASetVertexBuffers( 0, 1, &m_pVB, &m_Stride, &Offset );
	m_device.DXContext().IASetVertexBuffers( 0, m_boundVertexStreamsCount, m_vertexBuffers, m_strides, m_offsets );

	if ( m_IB != NULL ) {
		m_device.DXContext().IASetIndexBuffer( m_IB, DXGI_FORMAT_R32_UINT, 0 );
		m_device.DXContext().DrawIndexedInstanced( _indicesCount, _instancesCount, _startIndex, _baseVertexOffset, 0 );
	} else {
		m_device.DXContext().IASetIndexBuffer( NULL, DXGI_FORMAT_UNKNOWN, 0 );
		m_device.DXContext().DrawInstanced( _verticesCount, _instancesCount, _startVertex, 0 );
	}
}

void	Primitive::Build( const void* _vertices, const U32* _indices, bool _dynamic, bool _allowSRV, bool _allowUAV ) {
//	ASSERT( m_VerticesCount <= 65536, "Time to upgrade to U32 indices!" );

	{   // Create the vertex buffer
		D3D11_BUFFER_DESC   desc;
		desc.ByteWidth = m_verticesCount * m_stride;
		desc.Usage = _dynamic ? D3D11_USAGE_DYNAMIC : (_allowUAV ? D3D11_USAGE_DEFAULT : D3D11_USAGE_IMMUTABLE);
		desc.BindFlags = D3D11_BIND_VERTEX_BUFFER | (_allowSRV ? D3D11_BIND_SHADER_RESOURCE : 0) | (_allowUAV ? D3D11_BIND_UNORDERED_ACCESS : 0);
		desc.CPUAccessFlags = _dynamic ? D3D11_CPU_ACCESS_WRITE : 0;
		desc.MiscFlags = 0;
		desc.StructureByteStride = 0;

		if ( !_dynamic ) {
			D3D11_SUBRESOURCE_DATA	initData;
			initData.pSysMem = _vertices;
			initData.SysMemPitch = 0;
			initData.SysMemSlicePitch = 0;

			Check( m_device.DXDevice().CreateBuffer( &desc, &initData, &m_VB ) );
		} else {
			Check( m_device.DXDevice().CreateBuffer( &desc, NULL, &m_VB ) );
		}

		// Initialize as if we had only one bound vertex stream
#ifdef _DEBUG
		m_boundPrimitives[0] = this;	// We're the first and only bound primitive at the time
#endif
		m_boundVertexStreamsCount = 1;
		m_vertexBuffers[0] = m_VB;
		m_strides[0] = m_stride;
		m_offsets[0] = 0;
		m_compositeFormat.AggregateVertexFormat( m_format );
	}

	if ( _indices != NULL ) {
		// Create the index buffer
		D3D11_BUFFER_DESC   desc;
//		desc.ByteWidth = m_IndicesCount * sizeof(U16);		 // For now, we only support U16 primitives
		desc.ByteWidth = m_indicesCount * sizeof(U32);
		desc.Usage = _dynamic ? D3D11_USAGE_DYNAMIC : (_allowUAV ? D3D11_USAGE_DEFAULT : D3D11_USAGE_IMMUTABLE);
		desc.BindFlags = D3D11_BIND_INDEX_BUFFER | (_allowSRV ? D3D11_BIND_SHADER_RESOURCE : 0) | (_allowUAV ? D3D11_BIND_UNORDERED_ACCESS : 0);;
		desc.CPUAccessFlags = _dynamic ? D3D11_CPU_ACCESS_WRITE : 0;
		desc.MiscFlags = 0;
		desc.StructureByteStride = 0;

		if ( !_dynamic ) {
			D3D11_SUBRESOURCE_DATA	initData;
			initData.pSysMem = _indices;
			initData.SysMemPitch = 0;
			initData.SysMemSlicePitch = 0;

			Check( m_device.DXDevice().CreateBuffer( &desc, &initData, &m_IB ) );
		} else {
			Check( m_device.DXDevice().CreateBuffer( &desc, NULL, &m_IB ) );
		}
	}

	switch ( m_topology ) {
	case D3D11_PRIMITIVE_TOPOLOGY_POINTLIST:
		m_facesCount = _indices != NULL ? m_indicesCount : m_verticesCount;
		break;

	case D3D11_PRIMITIVE_TOPOLOGY_LINELIST:
		m_facesCount = _indices != NULL ? m_indicesCount / 2 : m_verticesCount / 2;
		break;

	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST:
		m_facesCount = _indices != NULL ? m_indicesCount / 3 : m_verticesCount / 3;
		break;

	case D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP:
		m_facesCount = _indices != NULL ? m_indicesCount - 2 : m_verticesCount - 2;
		break;

	default:
		if ( m_topology >= D3D11_PRIMITIVE_TOPOLOGY_1_CONTROL_POINT_PATCHLIST && m_topology <= D3D11_PRIMITIVE_TOPOLOGY_32_CONTROL_POINT_PATCHLIST ) {
			// For patches, it depends on the amount of control points
			int	ControlPointsPerPatch = 1 + (m_topology - D3D11_PRIMITIVE_TOPOLOGY_1_CONTROL_POINT_PATCHLIST);
			m_facesCount = (_indices != NULL ? m_indicesCount : m_verticesCount) / ControlPointsPerPatch;
		} else {
			ASSERT( FALSE, "Unsupported primitive type !" );
		}
	}
}

void	Primitive::UpdateDynamic( void* _vertices, U16* _indices, int _verticesCount, int _indicesCount ) {
	ASSERT( _vertices != NULL, "Invalid vertices !" );
	{
		D3D11_MAPPED_SUBRESOURCE	subResource;
		Device::Check( m_device.DXContext().Map( m_VB, 0, D3D11_MAP_WRITE_DISCARD, 0, &subResource ) );
		memcpy( subResource.pData, _vertices, (_verticesCount != -1 ? _verticesCount : m_verticesCount) * m_stride );
		m_device.DXContext().Unmap( m_VB, 0 );
	}

	if ( _indices != NULL ) {
		D3D11_MAPPED_SUBRESOURCE	subResource;
		Device::Check( m_device.DXContext().Map( m_IB, 0, D3D11_MAP_WRITE_DISCARD, 0, &subResource ) );
		memcpy( subResource.pData, _vertices, (_indicesCount != -1 ? _indicesCount : m_indicesCount) * sizeof(U16) );
		m_device.DXContext().Unmap( m_IB, 0 );
	}
}

void	Primitive::BindVertexStream( U32 _streamIndex, Primitive& _boundPrimitive, int _startIndex ) {
	ASSERT( _streamIndex < MAX_BOUND_VERTEX_STREAMS, "Stream index out of range!" );
	ASSERT( _streamIndex <= m_boundVertexStreamsCount, "Stream index out of range! You must bind streams sequentially!" );
	m_boundVertexStreamsCount = MAX( m_boundVertexStreamsCount, _streamIndex+1 );

#ifdef _DEBUG
	m_boundPrimitives[_streamIndex] = &_boundPrimitive;
#endif
	m_vertexBuffers[_streamIndex] = _boundPrimitive.m_VB;
	m_strides[_streamIndex] = _boundPrimitive.m_stride;
	m_offsets[_streamIndex] = _startIndex * _boundPrimitive.m_format.Size();	// The offset is in BYTES!

	// Aggregate vertex format into our composite format
	// This format will be used at runtime to compare with rendering material format instead of the simple original format the primitive was constructed with...
	m_compositeFormat.AggregateVertexFormat( _boundPrimitive.m_format );
}

void	Primitive::VBSetCS( U32 _slotIndex ) const {
	if ( m_cachedVB_SRV == NULL ) {
		D3D11_SHADER_RESOURCE_VIEW_DESC	desc;
		desc.Format = DXGI_FORMAT_R32_UINT;
		desc.ViewDimension = D3D11_SRV_DIMENSION_BUFFER;
		desc.Buffer.FirstElement = 0;
		desc.Buffer.NumElements = m_verticesCount * m_stride >> 2;	// Total size in bytes / sizeof(UINT)
		Check( m_device.DXDevice().CreateShaderResourceView( m_VB, &desc, &m_cachedVB_SRV ) );
	}
	m_device.DXContext().CSSetShaderResources( _slotIndex, 1, &m_cachedVB_SRV );
}
void	Primitive::IBSetCS( U32 _slotIndex ) const {
	if ( m_cachedIB_SRV == NULL ) {
		D3D11_SHADER_RESOURCE_VIEW_DESC	desc;
		desc.Format = DXGI_FORMAT_R32_UINT;
		desc.ViewDimension = D3D11_SRV_DIMENSION_BUFFER;
		desc.Buffer.FirstElement = 0;
		desc.Buffer.NumElements = m_indicesCount;
		Check( m_device.DXDevice().CreateShaderResourceView( m_IB, &desc, &m_cachedIB_SRV ) );
	}
	m_device.DXContext().CSSetShaderResources( _slotIndex, 1, &m_cachedIB_SRV );
}
void	Primitive::VBSetCS_UAV( U32 _slotIndex ) const {
	if ( m_cachedVB_UAV == NULL ) {
		D3D11_UNORDERED_ACCESS_VIEW_DESC	desc;
		desc.Format = DXGI_FORMAT_R32_UINT;
		desc.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
		desc.Buffer.FirstElement = 0;
		desc.Buffer.NumElements = m_verticesCount * m_stride >> 2;	// Total size in bytes / sizeof(UINT)
		desc.Buffer.Flags = D3D11_BUFFER_UAV_FLAG_RAW;
		Check( m_device.DXDevice().CreateUnorderedAccessView( m_VB, &desc, &m_cachedVB_UAV ) );
	}
// 	U32	UAVInitCount = -1;
	m_device.DXContext().CSSetUnorderedAccessViews( _slotIndex, 1, &m_cachedVB_UAV, NULL );// &UAVInitCount );
}
void	Primitive::IBSetCS_UAV( U32 _slotIndex ) const {
	if ( m_cachedIB_UAV == NULL ) {
		D3D11_UNORDERED_ACCESS_VIEW_DESC	desc;
		desc.Format = DXGI_FORMAT_R32_UINT;
		desc.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
		desc.Buffer.FirstElement = 0;
		desc.Buffer.NumElements = m_indicesCount;
		desc.Buffer.Flags = 0;	// D3D11_BUFFER_UAV_FLAG_RAW
		Check( m_device.DXDevice().CreateUnorderedAccessView( m_IB, &desc, &m_cachedIB_UAV ) );
	}
// 	U32	UAVInitCount = -1;
	m_device.DXContext().CSSetUnorderedAccessViews( _slotIndex, 1, &m_cachedIB_UAV, NULL );//&UAVInitCount );
}

#ifdef SUPPORT_GEO_BUILDERS

// IGeometryWriter Implementation
void	Primitive::CreateBuffers( int _verticesCount, int _indicesCount, D3D11_PRIMITIVE_TOPOLOGY _topology, void*& _vertices, void*& _indices ) {
	ASSERT( _verticesCount <= 65536, "Too many vertices !" );	// Time to start accounting for large meshes d00d !

	m_verticesCount = _verticesCount;
	m_indicesCount = _indicesCount;
	m_topology = _topology;

	_vertices = new U8[m_verticesCount * m_format.Size()];
	_indices = new U32[m_indicesCount];
}

void	Primitive::AppendVertex( void*& _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV ) {
	m_format.Write( _pVertex, _Position, _Normal, _Tangent, _BiTangent, _UV );
	_pVertex = (void*) ((U8*) _pVertex + m_format.Size());
}

void	Primitive::AppendIndex( void*& _pIndex, int _Index ) {
	ASSERT( _Index < m_verticesCount, "Index out of range !" );
	*((U32*) _pIndex) = _Index;
	_pIndex = (void*) ((U8*) _pIndex + sizeof(U32));
}

void	Primitive::Finalize( void* _vertices, void* _indices ) {
	Build( _vertices, (U32*) _indices, false );

	delete[] _vertices;
	delete[] _indices;
}

#endif