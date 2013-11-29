#pragma once

#ifdef GODCOMPLEX
#define SUPPORT_GEO_BUILDERS
#endif

#include "Component.h"
#include "../Structures/VertexFormats.h"
#include "Material.h"

#ifdef SUPPORT_GEO_BUILDERS
#include "../../Procedural/GeometryBuilder.h"
class Primitive : public Component, public GeometryBuilder::IGeometryWriter
#else
class Primitive : public Component
#endif
{
private:	// FIELDS

	const IVertexFormatDescriptor&	m_Format;
	D3D11_PRIMITIVE_TOPOLOGY		m_Topology;

	ID3D11Buffer*					m_pVB;
	ID3D11Buffer*					m_pIB;
	int								m_VerticesCount;
	int								m_IndicesCount;
	int								m_FacesCount;
	U32								m_Stride;


public:	 // METHODS

	Primitive( Device& _Device, int _VerticesCount, const void* _pVertices, int _IndicesCount, const U32* _pIndices, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format );
	Primitive( Device& _Device, const IVertexFormatDescriptor& _Format );	// Used by geometry builders
	Primitive( Device& _Device, int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format );	// Used to build dynamic buffers
	~Primitive();

	void			Render( Material& _Material );
	void			Render( Material& _Material, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset );
	void			RenderInstanced( Material& _Material, int _InstancesCount );
	void			RenderInstanced( Material& _Material, int _InstancesCount, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset );

	void			UpdateDynamic( void* _pVertices, U16* _pIndices, int _VerticesCount=-1, int _IndicesCount=-1 );

#ifdef SUPPORT_GEO_BUILDERS
	// IGeometryWriter implementation
	virtual void	CreateBuffers( int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _pVertices, void*& _pIndices );
	virtual void	AppendVertex( void*& _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat3& _BiTangent, const NjFloat2& _UV );
	virtual void	AppendIndex( void*& _pIndex, int _Index );
	virtual void	Finalize( void* _pVertices, void* _pIndices );
#endif

private:

	void			Build( const void* _pVertices, const U32* _pIndices, bool _bDynamic );
};

