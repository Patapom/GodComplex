// RendererManaged.h

#pragma once

#include "Device.h"

using namespace System;

namespace RendererManaged {

	public enum class	RASTERIZER_STATE
	{
		NOCHANGE,

		CULL_NONE,
		CULL_BACK,
		CULL_FRONT,
		WIREFRAME,
	};

	public enum class	DEPTHSTENCIL_STATE
	{
		NOCHANGE,

		DISABLED,
		READ_DEPTH_LESS_EQUAL,
		READ_WRITE_DEPTH_LESS,
		READ_WRITE_DEPTH_GREATER,
	};

	public enum class	BLEND_STATE
	{
		NOCHANGE,

		DISABLED,
		ALPHA_BLEND,
		PREMULTIPLIED_ALPHA,
		ADDITIVE,
	};
}
