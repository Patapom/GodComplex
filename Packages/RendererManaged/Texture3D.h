// RendererManaged.h

#pragma once

#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace Renderer {

	ref class View3D;

	// Wraps a 3D texture
	[System::Diagnostics::DebuggerDisplayAttribute( "{Width,d}x{Height,d}x{Depth,d}x{MipLevelsCount,d} {Tag}" )]
	public ref class Texture3D {
	public:

		::Texture3D*	m_texture;
		Object^			m_tag;

	public:

		property UInt32	Width			{ UInt32 get() { return m_texture->GetWidth(); } }
		property UInt32	Height			{ UInt32 get() { return m_texture->GetHeight(); } }
		property UInt32	Depth			{ UInt32 get() { return m_texture->GetDepth(); } }
		property UInt32	MipLevelsCount	{ UInt32 get() { return m_texture->GetMipLevelsCount(); } }

		property Object^	Tag { Object^ get() { return m_tag; } void set( Object^ _value ) { m_tag = _value; } }

	public:

		Texture3D( Device^ _device, UInt32 _width, UInt32 _height, UInt32 _depth, UInt32 _mipLevelsCount, ImageUtility::PIXEL_FORMAT _pixelFormat, ImageUtility::COMPONENT_FORMAT _componentFormat, bool _staging, bool _UAV, cli::array<PixelsBuffer^>^ _mipLevelsContent );
		Texture3D( Device^ _device, ImageUtility::ImagesMatrix^ _images, ImageUtility::COMPONENT_FORMAT _componentFormat );
		~Texture3D() {
 			delete m_texture;
		}

		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture3D^ _Source ) {
			m_texture->CopyFrom( *_Source->m_texture );
		}

		PixelsBuffer^	MapRead( int _mipLevelIndex ) {
			const D3D11_MAPPED_SUBRESOURCE&	mappedResource = m_texture->MapRead( _mipLevelIndex );
			return gcnew PixelsBuffer( mappedResource, _mipLevelIndex, 0, true );
		}

		PixelsBuffer^	MapWrite( int _mipLevelIndex ) {
			const D3D11_MAPPED_SUBRESOURCE&	mappedResource = m_texture->MapWrite( _mipLevelIndex );
			return gcnew PixelsBuffer( mappedResource, _mipLevelIndex, 0, false );
		}

		void			UnMap( PixelsBuffer^ _mappedSubResource ) {
			if ( !_mappedSubResource->m_readOnly ) {
				// Write back buffer to mapped sub-resource for upload
				_mappedSubResource->WriteToMappedSubResource();
			}
			m_texture->UnMap( _mappedSubResource->m_mappedMipLevelIndex );
			delete _mappedSubResource;
		}

		// Views
		View3D^		GetView()	{ return GetView( 0, 0, 0, 0 ); }
		View3D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _sliceStart, UInt32 _slicesCount );
		View3D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _sliceStart, UInt32 _slicesCount, bool _asArray );

		// Uploads the texture to the shader
		void		Set( UInt32 _slotIndex );
		void		SetVS( UInt32 _slotIndex );
		void		SetHS( UInt32 _slotIndex );
		void		SetDS( UInt32 _slotIndex );
		void		SetGS( UInt32 _slotIndex );
		void		SetPS( UInt32 _slotIndex );
		void		SetCS( UInt32 _slotIndex );
		void		RemoveFromLastAssignedSlots()	{ m_texture->RemoveFromLastAssignedSlots(); }

		// Upload the texture as a UAV for a compute shader
		void		SetCSUAV( int _slotIndex );
		void		RemoveFromLastAssignedSlotUAV()	{ m_texture->RemoveFromLastAssignedSlotUAV(); }

	internal:

		Texture3D( const ::Texture3D& _existingTexture ) {
			m_texture = const_cast< ::Texture3D* >( &_existingTexture );
		}
	};

	// Wraps a 3D texture view (SRV, RTV, DSV or UAV)
	public ref class	View3D : public IView {
	internal:
		Texture3D^	m_owner;
		UInt32		m_mipLevelStart;
		UInt32		m_mipLevelsCount;
		UInt32		m_sliceStart;
		UInt32		m_slicesCount;
		bool		m_asArray;

		View3D( Texture3D^ _Owner, UInt32 _MipLevelStart, UInt32 _MipLevelsCount, UInt32 _SliceStart, UInt32 _SlicesCount ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _MipLevelsCount ), m_sliceStart( _SliceStart ), m_slicesCount( _SlicesCount ), m_asArray( false ) {}
		View3D( Texture3D^ _Owner, UInt32 _MipLevelStart, UInt32 _MipLevelsCount, UInt32 _SliceStart, UInt32 _SlicesCount, bool _AsArray ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _MipLevelsCount ), m_sliceStart( _SliceStart ), m_slicesCount( _SlicesCount ), m_asArray( _AsArray ) {}

	public:

		virtual property UInt32	Width				{ UInt32 get(); }
		virtual property UInt32	Height				{ UInt32 get(); }
		virtual property UInt32	ArraySizeOrDepth	{ UInt32 get(); }

		virtual property ::ID3D11ShaderResourceView*	SRV { ::ID3D11ShaderResourceView*	get(); }
		virtual property ::ID3D11RenderTargetView*		RTV { ::ID3D11RenderTargetView*		get(); }
		virtual property ::ID3D11UnorderedAccessView*	UAV { ::ID3D11UnorderedAccessView*	get(); }
		virtual property ::ID3D11DepthStencilView*		DSV { ::ID3D11DepthStencilView*		get() { throw gcnew Exception( "3D Textures cannot be used as depth stencil buffers!" ); } }

	public:	// Setters to shader inputs
		void		Set( UInt32 _slotIndex );
		void		SetVS( UInt32 _slotIndex );
		void		SetHS( UInt32 _slotIndex );
		void		SetDS( UInt32 _slotIndex );
		void		SetGS( UInt32 _slotIndex );
		void		SetPS( UInt32 _slotIndex );
		void		SetCS( UInt32 _slotIndex );
		void		SetCSUAV( UInt32 _slotIndex );
	};
}
