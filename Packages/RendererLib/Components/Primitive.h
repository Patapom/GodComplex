#pragma once

#ifdef GODCOMPLEX
#define SUPPORT_GEO_BUILDERS
#endif

#include "Component.h"
#include "../Structures/VertexFormats.h"
#include "Shader.h"

#ifdef SUPPORT_GEO_BUILDERS
#include "../../Procedural/GeometryBuilder.h"
class Primitive : public Component, public GeometryBuilder::IGeometryWriter
#else
class Primitive : public Component
#endif
{
private:	// NESTED TYPES

	static const int		MAX_BOUND_VERTEX_STREAMS = 8;

private:	// FIELDS

	const IVertexFormatDescriptor&	m_format;
	D3D11_PRIMITIVE_TOPOLOGY		m_topology;

	ID3D11Buffer*					m_VB;
	ID3D11Buffer*					m_IB;
	U32								m_verticesCount;
	U32								m_indicesCount;
	U32								m_facesCount;
	U32								m_stride;

	// Render parameters
	U32								m_boundVertexStreamsCount;
	U32								m_strides[MAX_BOUND_VERTEX_STREAMS];
	U32								m_offsets[MAX_BOUND_VERTEX_STREAMS];
	ID3D11Buffer*					m_vertexBuffers[MAX_BOUND_VERTEX_STREAMS];
	CompositeVertexFormatDescriptor	m_compositeFormat;	// This format is used only if we have more than a single vertex stream for the primitive
#ifdef _DEBUG
	Primitive*						m_boundPrimitives[MAX_BOUND_VERTEX_STREAMS];	// The primitive that contains the vertex stream to bind to our primitive
#endif

	// Custom views for compute shader access
	mutable ID3D11ShaderResourceView*	m_cachedVB_SRV;
	mutable ID3D11UnorderedAccessView*	m_cachedVB_UAV;
	mutable ID3D11ShaderResourceView*	m_cachedIB_SRV;
	mutable ID3D11UnorderedAccessView*	m_cachedIB_UAV;

public:	 // PROPERTIES

	U32				GetVerticesCount() const	{ return m_verticesCount; }
	U32				GetIndicesCount() const		{ return m_indicesCount; }
	U32				GetFacesCount() const		{ return m_facesCount; }

public:	 // METHODS

	// _allowSRV, _allowUAV, if true then the bind flags allow the VB and IB to be used as SRV or UAV by a compute shader (see VBSetCS and IBSetCS)
	Primitive( Device& _device, U32 _verticesCount, const void* _vertices, U32 _indicesCount, const U32* _indices, D3D11_PRIMITIVE_TOPOLOGY _topology, const IVertexFormatDescriptor& _format, bool _allowSRV=false, bool _allowUAV=false, bool _makeStructuredBuffer=false );
	Primitive( Device& _device, U32 _verticesCount, U32 _indicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _format, bool _allowSRV=false, bool _allowUAV=false, bool _makeStructuredBuffer=false );	// Used to build dynamic buffers
	Primitive( Device& _device, const IVertexFormatDescriptor& _Format );	// Used by geometry builders
	~Primitive();

	void			Render( Shader& _material );
	void			Render( Shader& _material, int _startVertex, int _verticesCount, int _startIndex, int _indicesCount, int _baseVertexOffset );
	void			RenderInstanced( Shader& _material, U32 _InstancesCount );
	void			RenderInstanced( Shader& _material, U32 _InstancesCount, U32 _startVertex, U32 _verticesCount, U32 _startIndex, U32 _indicesCount, U32 _baseVertexOffset );

	void			UpdateDynamic( void* _vertices, U16* _indices, int _verticesCount=-1, int _indicesCount=-1 );

	// Binds additional vertex streams from another primitive
	// This allows, for example, to add a separate vertex buffer to this primitive's VB
	// Example:
	//	struct VS_IN {
	//		float3	Position : POSITION;	// Comes from this primitive
	//		uint	SomeOtherInfo : INFO;	// Comes from a 2nd vertex buffer
	//	};
	//
	// Just create a primitive with the P3 vertex format, and bind it a primitive 
	//
	//	_startIndex, the index of the start vertex
	//
	void			BindVertexStream( U32 _streamIndex, Primitive& _boundPrimitive, int _startIndex=0 );

	void			VBSetCS( U32 _slotIndex ) const;		// Sets the vertex buffer as a SRV for a compute shader
	void			IBSetCS( U32 _slotIndex ) const;		// Sets the index buffer as a SRV for a compute shader
	void			VBSetCS_UAV( U32 _slotIndex ) const;	// Sets the vertex buffer as an UAV for a compute shader
	void			IBSetCS_UAV( U32 _slotIndex ) const;	// Sets the index buffer as an UAV for a compute shader

#ifdef SUPPORT_GEO_BUILDERS
	// IGeometryWriter implementation
	virtual void	CreateBuffers( int _verticesCount, int _indicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _vertices, void*& _indices );
	virtual void	AppendVertex( void*& _pVertex, const bfloat3& _Position, const bfloat3& _Normal, const bfloat3& _Tangent, const bfloat3& _BiTangent, const bfloat2& _UV );
	virtual void	AppendIndex( void*& _pIndex, int _Index );
	virtual void	Finalize( void* _vertices, void* _indices );
#endif

private:

	void			Build( const void* _vertices, const U32* _indices, bool _dynamic, bool _allowSRV, bool _allowUAV, bool _makeStructuredBuffer );
};

