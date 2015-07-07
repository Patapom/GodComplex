// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace RendererManaged {

	public enum class	PIXEL_FORMAT
	{
		UNKNOWN,
		R8_UNORM,
		RGBA8_UNORM,
		RGBA8_UNORM_sRGB,
		R16_FLOAT,
		R16_UNORM,
		RG16_FLOAT,
		RG16_UNORM,
		RGBA16_FLOAT,
		R32_FLOAT,
		RG32_FLOAT,
		RGBA32_FLOAT,
		BC3_UNORM,
		BC3_UNORM_sRGB,
	};

	static ::IPixelFormatDescriptor*	GetDescriptor( PIXEL_FORMAT _Format )
	{
 		IPixelFormatDescriptor*	pDescriptor = NULL;
 		switch ( _Format )
 		{
 		case PIXEL_FORMAT::R8_UNORM:		pDescriptor = &PixelFormatR8::DESCRIPTOR; break;
 		case PIXEL_FORMAT::RGBA8_UNORM:		pDescriptor = &PixelFormatRGBA8::DESCRIPTOR; break;
 		case PIXEL_FORMAT::RGBA8_UNORM_sRGB:pDescriptor = &PixelFormatRGBA8_sRGB::DESCRIPTOR; break;
		case PIXEL_FORMAT::R16_FLOAT:		pDescriptor = &PixelFormatR16F::DESCRIPTOR; break;
		case PIXEL_FORMAT::R16_UNORM:		pDescriptor = &PixelFormatR16_UNORM::DESCRIPTOR; break;
		case PIXEL_FORMAT::RG16_FLOAT:		pDescriptor = &PixelFormatRG16F::DESCRIPTOR; break;
		case PIXEL_FORMAT::RG16_UNORM:		pDescriptor = &PixelFormatRG16_UNORM::DESCRIPTOR; break;
		case PIXEL_FORMAT::RGBA16_FLOAT:	pDescriptor = &PixelFormatRGBA16F::DESCRIPTOR; break;
		case PIXEL_FORMAT::R32_FLOAT:		pDescriptor = &PixelFormatR32F::DESCRIPTOR; break;
		case PIXEL_FORMAT::RG32_FLOAT:		pDescriptor = &PixelFormatRG32F::DESCRIPTOR; break;
		case PIXEL_FORMAT::RGBA32_FLOAT:	pDescriptor = &PixelFormatRGBA32F::DESCRIPTOR; break;
		case PIXEL_FORMAT::BC3_UNORM:		pDescriptor = &PixelFormatBC3_UNORM::DESCRIPTOR; break;
		case PIXEL_FORMAT::BC3_UNORM_sRGB:	pDescriptor = &PixelFormatBC3_UNORM_sRGB::DESCRIPTOR; break;

		default:	throw gcnew Exception( "Unsupported pixel format!" );
 		}

		return pDescriptor;
	}

	public enum class	DEPTH_STENCIL_FORMAT
	{
		D32,
		D24S8,
	};

	static ::IDepthStencilFormatDescriptor*	GetDescriptor( DEPTH_STENCIL_FORMAT _Format )
	{
 		IDepthStencilFormatDescriptor*	pDescriptor = NULL;
 		switch ( _Format )
 		{
		case DEPTH_STENCIL_FORMAT::D32:				pDescriptor = &DepthStencilFormatD32F::DESCRIPTOR; break;
		case DEPTH_STENCIL_FORMAT::D24S8:			pDescriptor = &DepthStencilFormatD24S8::DESCRIPTOR; break;

		default:	throw gcnew Exception( "Unsupported depth stencil format!" );
 		}

		return pDescriptor;
	}
}
