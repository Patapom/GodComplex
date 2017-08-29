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

		property UInt32	VerticesCount	{ UInt32 get() { return m_pPrimitive->GetVerticesCount(); } }
		property UInt32	IndicesCount	{ UInt32 get() { return m_pPrimitive->GetIndicesCount(); } }
		property UInt32	FacesCount		{ UInt32 get() { return m_pPrimitive->GetFacesCount(); } }

	public:

		Primitive( Device^ _device, UInt32 _verticesCount, ByteBuffer^ _vertices, cli::array<UInt32>^ _indices, TOPOLOGY _topology, VERTEX_FORMAT _vertexFormat ) { Build( _device, _verticesCount, _vertices, _indices, _topology, _vertexFormat, false, false ); }
		Primitive( Device^ _device, UInt32 _verticesCount, ByteBuffer^ _vertices, cli::array<UInt32>^ _indices, TOPOLOGY _topology, VERTEX_FORMAT _vertexFormat, bool _allowSRV, bool _allowUAV )  { Build( _device, _verticesCount, _vertices, _indices, _topology, _vertexFormat, _allowSRV, _allowUAV ); }
		~Primitive() { delete m_pPrimitive; }

		void	Render( Shader^ _shader );
		void	RenderInstanced( Shader^ _shader, UInt32 _instancesCount );
		void	RenderInstanced( Shader^ _shader, UInt32 _instancesCount, UInt32 _startVertex, UInt32 _verticesCount, UInt32 _startIndex, UInt32 _indicesCount, UInt32 _baseVertexOffset );

		void	VBSetCS( U32 _slotIndex )		{ m_pPrimitive->VBSetCS( _slotIndex ); }		// Sets the vertex buffer as a SRV for a compute shader
		void	IBSetCS( U32 _slotIndex )		{ m_pPrimitive->IBSetCS( _slotIndex ); }		// Sets the index buffer as a SRV for a compute shader
		void	VBSetCS_UAV( U32 _slotIndex )	{ m_pPrimitive->VBSetCS_UAV( _slotIndex ); }	// Sets the vertex buffer as an UAV for a compute shader
		void	IBSetCS_UAV( U32 _slotIndex )	{ m_pPrimitive->IBSetCS_UAV( _slotIndex ); }	// Sets the index buffer as an UAV for a compute shader

	private:
		void	Build( Device^ _device, UInt32 _verticesCount, ByteBuffer^ _vertices, cli::array<UInt32>^ _indices, TOPOLOGY _topology, VERTEX_FORMAT _vertexFormat, bool _allowSRV, bool _allowUAV );
	};
}
