// RendererManaged.h

#pragma once

#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace Renderer {

	ref class Texture3D;

	// Wraps a 3D texture view (SRV, RTV, DSV or UAV)
	public ref class	View3D : public IView {
	internal:
		Texture3D^	m_Owner;
		UInt32		m_MipLevelStart;
		UInt32		m_MipLevelsCount;
		UInt32		m_SliceStart;
		UInt32		m_SlicesCount;
		bool		m_AsArray;

		View3D( Texture3D^ _Owner, UInt32 _MipLevelStart, UInt32 _MipLevelsCount, UInt32 _SliceStart, UInt32 _SlicesCount ) : m_Owner( _Owner ), m_MipLevelStart( _MipLevelStart ), m_MipLevelsCount( _MipLevelsCount ), m_SliceStart( _SliceStart ), m_SlicesCount( _SlicesCount ), m_AsArray( false ) {}
		View3D( Texture3D^ _Owner, UInt32 _MipLevelStart, UInt32 _MipLevelsCount, UInt32 _SliceStart, UInt32 _SlicesCount, bool _AsArray ) : m_Owner( _Owner ), m_MipLevelStart( _MipLevelStart ), m_MipLevelsCount( _MipLevelsCount ), m_SliceStart( _SliceStart ), m_SlicesCount( _SlicesCount ), m_AsArray( _AsArray ) {}

	public:

		virtual property UInt32	Width				{ UInt32 get(); }
		virtual property UInt32	Height				{ UInt32 get(); }
		virtual property UInt32	ArraySizeOrDepth	{ UInt32 get(); }

		virtual property ::ID3D11ShaderResourceView*	SRV { ::ID3D11ShaderResourceView*	get(); }
		virtual property ::ID3D11RenderTargetView*		RTV { ::ID3D11RenderTargetView*		get(); }
		virtual property ::ID3D11UnorderedAccessView*	UAV { ::ID3D11UnorderedAccessView*	get(); }
		virtual property ::ID3D11DepthStencilView*		DSV { ::ID3D11DepthStencilView*		get() { throw gcnew Exception( "3D Textures cannot be used as depth stencil buffers!" ); } }
	};

	// Wraps a 3D texture
	public ref class Texture3D {
	public:

		::Texture3D*	m_pTexture;

	public:

		property UInt32	Width			{ UInt32 get() { return m_pTexture->GetWidth(); } }
		property UInt32	Height			{ UInt32 get() { return m_pTexture->GetHeight(); } }
		property UInt32	Depth			{ UInt32 get() { return m_pTexture->GetDepth(); } }
		property UInt32	MipLevelsCount	{ UInt32 get() { return m_pTexture->GetMipLevelsCount(); } }

	public:

		Texture3D( Device^ _device, UInt32 _width, UInt32 _height, UInt32 _depth, UInt32 _mipLevelsCount, PIXEL_FORMAT _pixelFormat, bool _staging, bool _UAV, cli::array<PixelsBuffer^>^ _mipLevelsContent );
		~Texture3D() {
 			delete m_pTexture;
		}

		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture3D^ _Source ) {
			m_pTexture->CopyFrom( *_Source->m_pTexture );
		}

		PixelsBuffer^	MapRead( int _mipLevelIndex ) {
			D3D11_MAPPED_SUBRESOURCE&	mappedResource = m_pTexture->Map( _mipLevelIndex );
			return gcnew PixelsBuffer( mappedResource, _mipLevelIndex, 0, true );
		}

		PixelsBuffer^	MapWrite( int _mipLevelIndex ) {
			D3D11_MAPPED_SUBRESOURCE&	mappedResource = m_pTexture->Map( _mipLevelIndex );
			return gcnew PixelsBuffer( mappedResource, _mipLevelIndex, 0, false );
		}

		void			UnMap( PixelsBuffer^ _mappedSubResource ) {
			if ( !_mappedSubResource->m_readOnly ) {
				// Write back buffer to mapped sub-resource for upload
				_mappedSubResource->WriteToMappedSubResource();
			}
			m_pTexture->UnMap( _mappedSubResource->m_mappedMipLevelIndex );
			delete _mappedSubResource;
		}

		// Views
		View3D^		GetView()				{ return GetView( 0, 0, 0, 0 ); }
		View3D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _sliceStart, UInt32 _slicesCount ) { return gcnew View3D( this, _mipLevelStart, _mipLevelsCount, _sliceStart, _slicesCount ); }
		View3D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _sliceStart, UInt32 _slicesCount, bool _asArray ) { return gcnew View3D( this, _mipLevelStart, _mipLevelsCount, _sliceStart, _slicesCount, _asArray ); }

		// Uploads the texture to the shader
		void		Set( UInt32 _slotIndex )	{ Set( _slotIndex, nullptr ); }
		void		SetVS( UInt32 _slotIndex )	{ SetVS( _slotIndex, nullptr ); }
		void		SetHS( UInt32 _slotIndex )	{ SetHS( _slotIndex, nullptr ); }
		void		SetDS( UInt32 _slotIndex )	{ SetDS( _slotIndex, nullptr ); }
		void		SetGS( UInt32 _slotIndex )	{ SetGS( _slotIndex, nullptr ); }
		void		SetPS( UInt32 _slotIndex )	{ SetPS( _slotIndex, nullptr ); }
		void		SetCS( UInt32 _slotIndex )	{ SetCS( _slotIndex, nullptr ); }

		void		Set( UInt32 _slotIndex, View3D^ _view );
		void		SetVS( UInt32 _slotIndex, View3D^ _view );
		void		SetHS( UInt32 _slotIndex, View3D^ _view );
		void		SetDS( UInt32 _slotIndex, View3D^ _view );
		void		SetGS( UInt32 _slotIndex, View3D^ _view );
		void		SetPS( UInt32 _slotIndex, View3D^ _view );
		void		SetCS( UInt32 _slotIndex, View3D^ _view );
		void		RemoveFromLastAssignedSlots()	{ m_pTexture->RemoveFromLastAssignedSlots(); }

		// Upload the texture as a UAV for a compute shader
		void		SetCSUAV( int _slotIndex )		{ m_pTexture->SetCSUAV( _slotIndex ); }
		void		RemoveFromLastAssignedSlotUAV()	{ m_pTexture->RemoveFromLastAssignedSlotUAV(); }

	internal:

		Texture3D( const ::Texture3D& _existingTexture ) {
			m_pTexture = const_cast< ::Texture3D* >( &_existingTexture );
		}
	};
}
