// Primitive.h

#pragma once
#include "Device.h"
#include "VertexFormats.h"

using namespace System;

namespace RendererManaged {

	ref class Shader;
	ref class ByteBuffer;

	public ref class Primitive
	{
	public:

		enum class	TOPOLOGY
		{
			POINT_LIST,
			LINE_LIST,
			TRIANGLE_LIST,
			TRIANGLE_STRIP,
		};

	private:

		::Primitive*		m_pPrimitive;

	public:

		Primitive( Device^ _Device, int _VerticesCount, ByteBuffer^ _Vertices, cli::array<UInt32>^ _Indices, TOPOLOGY _Topology, VERTEX_FORMAT _VertexFormat );
		~Primitive()
		{
			delete m_pPrimitive;
		}

		void	Render( Shader^ _Shader );
		void	RenderInstanced( Shader^ _Shader, int _InstancesCount );
		void	RenderInstanced( Shader^ _Shader, int _InstancesCount, int _StartVertex, int _VerticesCount, int _StartIndex, int _IndicesCount, int _BaseVertexOffset );
	};
}
