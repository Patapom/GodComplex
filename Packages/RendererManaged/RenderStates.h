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
	};

	public enum class	DEPTHSTENCIL_STATE
	{
		NOCHANGE,

		DISABLED,
		READ_DEPTH_LESS_EQUAL,
		READ_WRITE_DEPTH_LESS_EQUAL,
	};

	public enum class	BLEND_STATE
	{
		NOCHANGE,

		DISABLED,
		ADDITIVE,
		ALPHA_BLEND,
	};
}
