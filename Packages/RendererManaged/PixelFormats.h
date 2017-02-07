// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace Renderer {

	// Wraps the most useful DXGI formats used for textures and render targets
	//
	public enum class	PIXEL_FORMAT {
		UNKNOWN,
		R8_UNORM,
		RGBA8_UNORM,
		RGBA8_UNORM_sRGB,
		R16_FLOAT,
		R16_UNORM,
		RG16_FLOAT,
		RG16_UNORM,
		RGBA16_FLOAT,
		RGBA16_UNORM,
		RGBA16_UINT,
		R32_UINT,
		R32_FLOAT,
		RG32_FLOAT,
		RGBA32_FLOAT,
		RGBA32_UINT,
// 		BC3_UNORM,
// 		BC3_UNORM_sRGB,
	};

	static void	GetDescriptor( PIXEL_FORMAT _Format, BaseLib::IPixelAccessor*& _pixelFormat, BaseLib::COMPONENT_FORMAT& _componentFormat ) {
		_pixelFormat = NULL;
		_componentFormat = BaseLib::COMPONENT_FORMAT::AUTO;
		switch ( _Format ) {
			case PIXEL_FORMAT::R8_UNORM:		_pixelFormat = &BaseLib::PF_R8::Descriptor;	break;
			case PIXEL_FORMAT::RGBA8_UNORM:		_pixelFormat = &BaseLib::PF_RGBA8::Descriptor; break;
			case PIXEL_FORMAT::RGBA8_UNORM_sRGB:_pixelFormat = &BaseLib::PF_RGBA8::Descriptor;	_componentFormat = BaseLib::COMPONENT_FORMAT::UNORM_sRGB;break;
			case PIXEL_FORMAT::R16_FLOAT:		_pixelFormat = &BaseLib::PF_R16F::Descriptor; break;
			case PIXEL_FORMAT::R16_UNORM:		_pixelFormat = &BaseLib::PF_R16::Descriptor; break;
			case PIXEL_FORMAT::RG16_FLOAT:		_pixelFormat = &BaseLib::PF_RG16F::Descriptor; break;
			case PIXEL_FORMAT::RG16_UNORM:		_pixelFormat = &BaseLib::PF_RG16::Descriptor; break;
			case PIXEL_FORMAT::RGBA16_FLOAT:	_pixelFormat = &BaseLib::PF_RGBA16F::Descriptor; break;
			case PIXEL_FORMAT::RGBA16_UNORM:	_pixelFormat = &BaseLib::PF_RGBA16::Descriptor; break;
			case PIXEL_FORMAT::RGBA16_UINT:		_pixelFormat = &BaseLib::PF_RGBA16::Descriptor;	_componentFormat = BaseLib::COMPONENT_FORMAT::UINT;	break;
			case PIXEL_FORMAT::R32_FLOAT:		_pixelFormat = &BaseLib::PF_R32F::Descriptor;	break;
			case PIXEL_FORMAT::R32_UINT:		_pixelFormat = &BaseLib::PF_R32::Descriptor; _componentFormat = BaseLib::COMPONENT_FORMAT::UINT; break;
			case PIXEL_FORMAT::RG32_FLOAT:		_pixelFormat = &BaseLib::PF_RG32F::Descriptor; break;
			case PIXEL_FORMAT::RGBA32_FLOAT:	_pixelFormat = &BaseLib::PF_RGBA32F::Descriptor; break;
			case PIXEL_FORMAT::RGBA32_UINT:		_pixelFormat = &BaseLib::PF_RGBA32::Descriptor;	_componentFormat = BaseLib::COMPONENT_FORMAT::UINT; break;
// 			case PIXEL_FORMAT::BC3_UNORM:		_pixelFormat = &PF_BC3::Descriptor;	_componentFormat = BaseLib::COMPONENT_FORMAT::UNORM;	break;
// 			case PIXEL_FORMAT::BC3_UNORM_sRGB:	_pixelFormat = &PF_BC3::Descriptor;	_componentFormat = BaseLib::COMPONENT_FORMAT::UNORM_sRGB;	break;

			default:	throw gcnew Exception( "Unsupported pixel format!" );
		}
	}

	// Wraps the most useful DXGI formats used for depth stencil buffers
	//
	public enum class	DEPTH_STENCIL_FORMAT {
		D16,
		D32,
		D24S8,
	};

	static BaseLib::IDepthAccessor*	GetDescriptor( DEPTH_STENCIL_FORMAT _format ) {
 		switch ( _format ) {
			case DEPTH_STENCIL_FORMAT::D32:		return &BaseLib::PF_D32::Descriptor;
			case DEPTH_STENCIL_FORMAT::D24S8:	return &BaseLib::PF_D24S8::Descriptor;

			default:	throw gcnew Exception( "Unsupported depth stencil format!" );
 		}

		return NULL;
	}
}
