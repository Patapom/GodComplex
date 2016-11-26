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

		Primitive( Device^ _device, int _verticesCount, ByteBuffer^ _vertices, cli::array<UInt32>^ _indices, TOPOLOGY _topology, VERTEX_FORMAT _vertexFormat );
		~Primitive() {
			delete m_pPrimitive;
		}

		void	Render( Shader^ _shader );
		void	RenderInstanced( Shader^ _shader, int _instancesCount );
		void	RenderInstanced( Shader^ _shader, int _instancesCount, int _startVertex, int _verticesCount, int _startIndex, int _indicesCount, int _baseVertexOffset );
	};
}
