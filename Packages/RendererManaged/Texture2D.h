// RendererManaged.h

#pragma once
#include "Device.h"

#include "PixelFormats.h"

using namespace System;

namespace RendererManaged {

	public ref class Texture2D
	{
	internal:

		::Texture2D*	m_pTexture;

	public:

		Texture2D( Device^ _Device, int _Width, int _Height, int _ArraySize, int _MipsLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV )
		{
 			IPixelFormatDescriptor*	pDescriptor = NULL;
 			switch ( _PixelFormat )
 			{
 			case PIXEL_FORMAT::RGBA8_UNORM:		pDescriptor = &PixelFormatRGBA8::DESCRIPTOR; break;
			case PIXEL_FORMAT::R16_FLOAT:		pDescriptor = &PixelFormatR16F::DESCRIPTOR; break;
			case PIXEL_FORMAT::RG16_FLOAT:		pDescriptor = &PixelFormatRG16F::DESCRIPTOR; break;
			case PIXEL_FORMAT::R32_FLOAT:		pDescriptor = &PixelFormatR32F::DESCRIPTOR; break;
			case PIXEL_FORMAT::RG32_FLOAT:		pDescriptor = &PixelFormatRG32F::DESCRIPTOR; break;
			case PIXEL_FORMAT::RGBA32_FLOAT:	pDescriptor = &PixelFormatRGBA32F::DESCRIPTOR; break;
 			}
 			if ( pDescriptor == NULL )
 				throw gcnew Exception( "Unsupported pixel format!" );

			m_pTexture = new ::Texture2D( *_Device->m_pDevice, _Width, _Height, _ArraySize, *pDescriptor, _MipsLevelsCount, NULL, _Staging, _UAV );
		}

		~Texture2D()
		{
 			delete m_pTexture;
		}

	internal:

		Texture2D( const ::Texture2D& _ExistingTexture )
		{
			m_pTexture = const_cast< ::Texture2D* >( &_ExistingTexture );
		}
	};
}
