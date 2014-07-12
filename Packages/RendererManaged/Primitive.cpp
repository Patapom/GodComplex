// This is the main DLL file.

#include "stdafx.h"

#include "Primitive.h"
#include "ByteBuffer.h"
#include "Shader.h"

namespace RendererManaged {

	Primitive::Primitive( Device^ _Device, int _VerticesCount, ByteBuffer^ _Vertices, cli::array<UInt32>^ _Indices, TOPOLOGY _Topology, VERTEX_FORMAT _VertexFormat )
	{
		D3D11_PRIMITIVE_TOPOLOGY	Topology = D3D11_PRIMITIVE_TOPOLOGY_UNDEFINED;
		switch ( _Topology )
		{
		case TOPOLOGY::POINT_LIST:		Topology = D3D11_PRIMITIVE_TOPOLOGY_POINTLIST; break;
		case TOPOLOGY::TRIANGLE_LIST:	Topology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST; break;
		case TOPOLOGY::TRIANGLE_STRIP:	Topology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP; break;
		default:
			throw gcnew Exception( "Unsupported topology!" );
		}

		// Build vertices and indices arrays
		cli::pin_ptr<Byte>	PinnedVertices = &_Vertices->m_Buffer[0];
		void*	pVertices = PinnedVertices;

		cli::pin_ptr<UInt32>	PinnedIndices;
		if ( _Indices != nullptr )
			PinnedIndices = &_Indices[0];

		IVertexFormatDescriptor*	pDescriptor = GetDescriptor( _VertexFormat );

		m_pPrimitive = new ::Primitive( *_Device->m_pDevice, _VerticesCount, pVertices, _Indices != nullptr ? _Indices->Length : 0, PinnedIndices, Topology, *pDescriptor );
	}

	void	Primitive::Render( Shader^ _Shader )
	{
		m_pPrimitive->Render( *_Shader->m_pShader );
	}

	void	Primitive::RenderInstanced( Shader^ _Shader, int _InstancesCount )
	{
		m_pPrimitive->RenderInstanced( *_Shader->m_pShader, _InstancesCount );
	}
	void	Primitive::RenderInstanced( Shader^ _Shader, int _InstancesCount, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset )
	{
		m_pPrimitive->RenderInstanced( *_Shader->m_pShader, _InstancesCount, _StartVertex, _VerticesCount, _StartIndex, _IndicesCount, _BaseVertexOffset );
	}

}