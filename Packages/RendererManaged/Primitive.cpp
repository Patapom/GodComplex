// This is the main DLL file.

#include "stdafx.h"

#include "Primitive.h"
#include "ByteBuffer.h"
#include "Shader.h"

namespace Renderer {

	void	Primitive::Build( Device^ _device, UInt32 _verticesCount, ByteBuffer^ _vertices, cli::array<UInt32>^ _indices, TOPOLOGY _topology, VERTEX_FORMAT _vertexFormat, bool _allowSRV, bool _allowUAV ) {

		D3D11_PRIMITIVE_TOPOLOGY	topology = D3D11_PRIMITIVE_TOPOLOGY_UNDEFINED;
		switch ( _topology ) {
		case TOPOLOGY::POINT_LIST:		topology = D3D11_PRIMITIVE_TOPOLOGY_POINTLIST; break;
		case TOPOLOGY::LINE_LIST:		topology = D3D11_PRIMITIVE_TOPOLOGY_LINELIST; break;
		case TOPOLOGY::TRIANGLE_LIST:	topology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST; break;
		case TOPOLOGY::TRIANGLE_STRIP:	topology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP; break;
		default:
			throw gcnew Exception( "Unsupported topology!" );
		}

		// Build vertices and indices arrays
		pin_ptr<Byte>	pinnedVertices = &_vertices->m_Buffer[0];
		void*	pVertices = pinnedVertices;

		pin_ptr<UInt32>	pinnedIndices;
		if ( _indices != nullptr )
			pinnedIndices = &_indices[0];

		IVertexFormatDescriptor*	descriptor = GetDescriptor( _vertexFormat );

		m_pPrimitive = new ::Primitive( *_device->m_pDevice, _verticesCount, pVertices, _indices != nullptr ? _indices->Length : 0, pinnedIndices, topology, *descriptor, _allowSRV, _allowUAV );
	}

	void	Primitive::Render( Shader^ _shader ) {
		m_pPrimitive->Render( *_shader->m_pShader );
	}
	void	Primitive::RenderInstanced( Shader^ _shader, UInt32 _instancesCount ) {
		m_pPrimitive->RenderInstanced( *_shader->m_pShader, _instancesCount );
	}
	void	Primitive::RenderInstanced( Shader^ _shader, UInt32 _instancesCount, UInt32 _startVertex, UInt32 _verticesCount, UInt32 _startIndex, UInt32 _indicesCount, UInt32 _baseVertexOffset ) {
		m_pPrimitive->RenderInstanced( *_shader->m_pShader, _instancesCount, _startVertex, _verticesCount, _startIndex, _indicesCount, _baseVertexOffset );
	}

}