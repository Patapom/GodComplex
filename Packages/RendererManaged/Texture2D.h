// RendererManaged.h

#pragma once
#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace RendererManaged {

	public ref class Texture2D
	{
	internal:

		::Texture2D*	m_pTexture;

	public:

		Texture2D( Device^ _Device, int _Width, int _Height, int _ArraySize, int _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _MipLevelsContent )
		{
 			IPixelFormatDescriptor*	pDescriptor = NULL;
 			switch ( _PixelFormat )
 			{
 			case PIXEL_FORMAT::RGBA8_UNORM:		pDescriptor = &PixelFormatRGBA8::DESCRIPTOR; break;
			case PIXEL_FORMAT::R16_FLOAT:		pDescriptor = &PixelFormatR16F::DESCRIPTOR; break;
			case PIXEL_FORMAT::RG16_FLOAT:		pDescriptor = &PixelFormatRG16F::DESCRIPTOR; break;
			case PIXEL_FORMAT::RGBA16_FLOAT:	pDescriptor = &PixelFormatRGBA16F::DESCRIPTOR; break;
			case PIXEL_FORMAT::R32_FLOAT:		pDescriptor = &PixelFormatR32F::DESCRIPTOR; break;
			case PIXEL_FORMAT::RG32_FLOAT:		pDescriptor = &PixelFormatRG32F::DESCRIPTOR; break;
			case PIXEL_FORMAT::RGBA32_FLOAT:	pDescriptor = &PixelFormatRGBA32F::DESCRIPTOR; break;
 			}
 			if ( pDescriptor == NULL )
 				throw gcnew Exception( "Unsupported pixel format!" );

			void**	ppContent = NULL;
			if ( _MipLevelsContent != nullptr )
			{
				ppContent = new void*[_MipLevelsCount];
				cli::pin_ptr<Byte>	Bisou;
				for ( int MipLevelIndex=0; MipLevelIndex < _MipLevelsCount; MipLevelIndex++ )
				{
					Bisou = &_MipLevelsContent[MipLevelIndex]->m_Buffer[0];
					ppContent[MipLevelIndex] = Bisou;
				}
			}

			m_pTexture = new ::Texture2D( *_Device->m_pDevice, _Width, _Height, _ArraySize, *pDescriptor, _MipLevelsCount, ppContent, _Staging, _UAV );

			delete[] ppContent;
		}

		~Texture2D()
		{
 			delete m_pTexture;
		}

		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture2D^ _Source )
		{
			m_pTexture->CopyFrom( *_Source->m_pTexture );
		}

		PixelsBuffer^	Map( int _MipLevelIndex, int _ArrayIndex )
		{
			D3D11_MAPPED_SUBRESOURCE&	MappedResource = m_pTexture->Map( _MipLevelIndex, _ArrayIndex );
			return gcnew PixelsBuffer( MappedResource );
		}

		void			UnMap( int _MipLevelIndex, int _ArrayIndex )
		{
			m_pTexture->UnMap( _MipLevelIndex, _ArrayIndex );
		}

		// Uploads the texture to the shader
		void		Set( int _SlotIndex )			{ m_pTexture->Set( _SlotIndex, true ); }
		void		SetVS( int _SlotIndex )			{ m_pTexture->Set( _SlotIndex, true ); }
		void		SetHS( int _SlotIndex )			{ m_pTexture->Set( _SlotIndex, true ); }
		void		SetDS( int _SlotIndex )			{ m_pTexture->Set( _SlotIndex, true ); }
		void		SetGS( int _SlotIndex )			{ m_pTexture->Set( _SlotIndex, true ); }
		void		SetPS( int _SlotIndex )			{ m_pTexture->Set( _SlotIndex, true ); }
		void		SetCS( int _SlotIndex )			{ m_pTexture->Set( _SlotIndex, true ); }
		void		RemoveFromLastAssignedSlots()	{ m_pTexture->RemoveFromLastAssignedSlots(); }

		// Upload the texture as a UAV for a compute shader
		void		SetCSUAV( int _SlotIndex )		{ m_pTexture->SetCSUAV( _SlotIndex ); }
		void		RemoveFromLastAssignedSlotUAV()	{ m_pTexture->RemoveFromLastAssignedSlotUAV(); }

	internal:

		Texture2D( const ::Texture2D& _ExistingTexture )
		{
			m_pTexture = const_cast< ::Texture2D* >( &_ExistingTexture );
		}
	};
}
