// Primitive.h

#pragma once

#include "Device.h"
#include "VertexFormats.h"

using namespace System;

namespace Renderer {

	ref class Shader;
	ref class ByteBuffer;

	// Wraps a simple 3D primitive
	//
	public ref class Primitive {
	public:

		enum class	TOPOLOGY {
			POINT_LIST,
			LINE_LIST,
			TRIANGLE_LIST,
			TRIANGLE_STRIP,
		};

	private:

		::Primitive*		m_pPrimitive;

	public:

		Primitive( Device^ _device, UInt32 _verticesCount, ByteBuffer^ _vertices, cli::array<UInt32>^ _indices, TOPOLOGY _topology, VERTEX_FORMAT _vertexFormat );
		~Primitive() {
			delete m_pPrimitive;
		}

		void	Render( Shader^ _shader );
		void	RenderInstanced( Shader^ _shader, UInt32 _instancesCount );
		void	RenderInstanced( Shader^ _shader, UInt32 _instancesCount, UInt32 _startVertex, UInt32 _verticesCount, UInt32 _startIndex, UInt32 _indicesCount, UInt32 _baseVertexOffset );
	};
}
