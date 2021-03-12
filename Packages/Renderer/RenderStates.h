#pragma once

#include "Device.h"

using namespace System;

namespace Renderer {

	// I chose not to implement an exhaustive list of render states but rather the most used render states
	// You need to modify that list and add the state yourself (in RendererLib::Device) to support additional states...
	//

	public enum class	RASTERIZER_STATE {
		NOCHANGE,

		CULL_NONE,
		CULL_BACK,
		CULL_FRONT,
		WIREFRAME,
	};

	public enum class	DEPTHSTENCIL_STATE {
		NOCHANGE,

		DISABLED,
		READ_DEPTH_LESS_EQUAL,
		READ_WRITE_DEPTH_LESS,
		READ_WRITE_DEPTH_GREATER,
		WRITE_ALWAYS,
	};

	public enum class	BLEND_STATE {
		NOCHANGE,

		DISABLED,
		ALPHA_BLEND,
		PREMULTIPLIED_ALPHA,
		ADDITIVE,
		MAX,
		MIN,
	};
}
