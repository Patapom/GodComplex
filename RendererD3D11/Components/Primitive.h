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

	const IVertexFormatDescriptor&	m_Format;
	D3D11_PRIMITIVE_TOPOLOGY		m_Topology;

	ID3D11Buffer*					m_pVB;
	ID3D11Buffer*					m_pIB;
	int								m_VerticesCount;
	int								m_IndicesCount;
	int								m_FacesCount;
	U32								m_Stride;

	// Render parameters
	U32								m_BoundVertexStreamsCount;
	U32								m_pStrides[MAX_BOUND_VERTEX_STREAMS];
	U32								m_pOffsets[MAX_BOUND_VERTEX_STREAMS];
	ID3D11Buffer*					m_ppVertexBuffers[MAX_BOUND_VERTEX_STREAMS];
	CompositeVertexFormatDescriptor	m_CompositeFormat;	// This format is used only if we have more than a single vertex stream for the primitive
#ifdef _DEBUG
	Primitive*						m_ppBoundPrimitives[MAX_BOUND_VERTEX_STREAMS];	// The primitive that contains the vertex stream to bind to our primitive
#endif


public:	 // PROPERTIES

	int				GetVerticesCount() const	{ return m_VerticesCount; }
	int				GetIndicesCount() const		{ return m_IndicesCount; }
	int				GetFacesCount() const		{ return m_FacesCount; }


public:	 // METHODS

	Primitive( Device& _Device, int _VerticesCount, const void* _pVertices, int _IndicesCount, const U32* _pIndices, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format );
	Primitive( Device& _Device, const IVertexFormatDescriptor& _Format );	// Used by geometry builders
	Primitive( Device& _Device, int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format );	// Used to build dynamic buffers
	~Primitive();

	void			Render( Shader& _Material );
	void			Render( Shader& _Material, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset );
	void			RenderInstanced( Shader& _Material, int _InstancesCount );
	void			RenderInstanced( Shader& _Material, int _InstancesCount, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset );

	void			UpdateDynamic( void* _pVertices, U16* _pIndices, int _VerticesCount=-1, int _IndicesCount=-1 );

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
	//	_StartIndex, the index of the start vertex
	//
	void			BindVertexStream( U32 _StreamIndex, Primitive& _BoundPrimitive, int _StartIndex=0 );


#ifdef SUPPORT_GEO_BUILDERS
	// IGeometryWriter implementation
	virtual void	CreateBuffers( int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _pVertices, void*& _pIndices );
	virtual void	AppendVertex( void*& _pVertex, const float3& _Position, const float3& _Normal, const float3& _Tangent, const float3& _BiTangent, const float2& _UV );
	virtual void	AppendIndex( void*& _pIndex, int _Index );
	virtual void	Finalize( void* _pVertices, void* _pIndices );
#endif

private:

	void			Build( const void* _pVertices, const U32* _pIndices, bool _bDynamic );
};

