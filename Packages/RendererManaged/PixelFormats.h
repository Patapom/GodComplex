// RendererManaged.h

#pragma once
#include "Device.h"

using namespace System;

namespace RendererManaged {

	public enum class	PIXEL_FORMAT
	{
		RGBA8_UNORM,
		RGBA8_UNORM_sRGB,
		R16_FLOAT,
		RG16_FLOAT,
		RGBA16_FLOAT,
		R32_FLOAT,
		RG32_FLOAT,
		RGBA32_FLOAT,
	};

	static ::IPixelFormatDescriptor*	GetDescriptor( PIXEL_FORMAT _Format )
	{
 		IPixelFormatDescriptor*	pDescriptor = NULL;
 		switch ( _Format )
 		{
 		case PIXEL_FORMAT::RGBA8_UNORM:		pDescriptor = &PixelFormatRGBA8::DESCRIPTOR; break;
 		case PIXEL_FORMAT::RGBA8_UNORM_sRGB:pDescriptor = &PixelFormatRGBA8_sRGB::DESCRIPTOR; break;
		case PIXEL_FORMAT::R16_FLOAT:		pDescriptor = &PixelFormatR16F::DESCRIPTOR; break;
		case PIXEL_FORMAT::RG16_FLOAT:		pDescriptor = &PixelFormatRG16F::DESCRIPTOR; break;
		case PIXEL_FORMAT::RGBA16_FLOAT:	pDescriptor = &PixelFormatRGBA16F::DESCRIPTOR; break;
		case PIXEL_FORMAT::R32_FLOAT:		pDescriptor = &PixelFormatR32F::DESCRIPTOR; break;
		case PIXEL_FORMAT::RG32_FLOAT:		pDescriptor = &PixelFormatRG32F::DESCRIPTOR; break;
		case PIXEL_FORMAT::RGBA32_FLOAT:	pDescriptor = &PixelFormatRGBA32F::DESCRIPTOR; break;
		default:	throw gcnew Exception( "Unsupported pixel format!" );
 		}

		return pDescriptor;
	}
}
