#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"
#include "../../Procedural/GeometryBuilder.h"
#include "Material.h"

class Primitive : public Component, public GeometryBuilder::IGeometryWriter
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

	Primitive( Device& _Device, int _VerticesCount, void* _pVertices, int _IndicesCount, U16* _pIndices, D3D11_PRIMITIVE_TOPOLOGY _Topology, const IVertexFormatDescriptor& _Format );
	Primitive( Device& _Device, const IVertexFormatDescriptor& _Format );
	~Primitive();

	void			Render( Material& _Material );

	// IGeometryWriter implementation
	virtual void	CreateBuffers( int _VerticesCount, int _IndicesCount, D3D11_PRIMITIVE_TOPOLOGY _Topology, void*& _pVertices, void*& _pIndices, int& _VertexStride, int& _IndexStride );
	virtual void	WriteVertex( void* _pVertex, const NjFloat3& _Position, const NjFloat3& _Normal, const NjFloat3& _Tangent, const NjFloat2& _UV );
	virtual void	WriteIndex( void* _pIndex, int _Index );
	virtual void	Finalize( void* _pVertices, void* _pIndices );

private:

	void			Build( void* _pVertices, U16* _pIndices );
};

