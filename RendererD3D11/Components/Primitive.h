#pragma once

#include "Component.h"
#include "../Structures/VertexFormats.h"
#include "Material.h"

class Primitive : public Component
{
private:	// FIELDS

	const VertexFormatDescriptor&   m_Format;
	D3D11_PRIMITIVE_TOPOLOGY	m_Topology;

	ID3D11Buffer*   m_pVB;
	ID3D11Buffer*   m_pIB;
	int			 m_FacesCount;


public:	 // METHODS

	Primitive( Device& _Device, int _VerticesCount, void* _pVertices, int _IndicesCount, U16* _pIndices, D3D11_PRIMITIVE_TOPOLOGY _Topology, const VertexFormatDescriptor& _Format );
	~Primitive();

	void			Render( Material& _Material );

private:
};

