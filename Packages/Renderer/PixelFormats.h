// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace Renderer {
	// Wraps the most useful DXGI formats used for depth stencil buffers
	//
	public enum class	DEPTH_STENCIL_FORMAT {
		D16,
		D32,
		D24S8,
	};

	static void	GetDescriptor( DEPTH_STENCIL_FORMAT _format, BaseLib::PIXEL_FORMAT& _pixelFormat, BaseLib::DEPTH_COMPONENT_FORMAT& _componentFormat ) {
 		switch ( _format ) {
			case DEPTH_STENCIL_FORMAT::D16:		_pixelFormat = BaseLib::PIXEL_FORMAT::R16F; _componentFormat = BaseLib::DEPTH_COMPONENT_FORMAT::DEPTH_ONLY; break;
			case DEPTH_STENCIL_FORMAT::D32:		_pixelFormat = BaseLib::PIXEL_FORMAT::R32F; _componentFormat = BaseLib::DEPTH_COMPONENT_FORMAT::DEPTH_ONLY; break;
			case DEPTH_STENCIL_FORMAT::D24S8:	_pixelFormat = BaseLib::PIXEL_FORMAT::R32F; _componentFormat = BaseLib::DEPTH_COMPONENT_FORMAT::DEPTH_STENCIL; break;

			default:	throw gcnew Exception( "Unsupported depth stencil format!" );
 		}
	}
}
