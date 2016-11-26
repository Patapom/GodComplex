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
		int			m_MipLevelStart;
		int			m_MipLevelsCount;
		int			m_SliceStart;
		int			m_SlicesCount;
		bool		m_AsArray;

		View3D( Texture3D^ _Owner, int _MipLevelStart, int _MipLevelsCount, int _SliceStart, int _SlicesCount ) : m_Owner( _Owner ), m_MipLevelStart( _MipLevelStart ), m_MipLevelsCount( _MipLevelsCount ), m_SliceStart( _SliceStart ), m_SlicesCount( _SlicesCount ), m_AsArray( false ) {}
		View3D( Texture3D^ _Owner, int _MipLevelStart, int _MipLevelsCount, int _SliceStart, int _SlicesCount, bool _AsArray ) : m_Owner( _Owner ), m_MipLevelStart( _MipLevelStart ), m_MipLevelsCount( _MipLevelsCount ), m_SliceStart( _SliceStart ), m_SlicesCount( _SlicesCount ), m_AsArray( _AsArray ) {}

	public:

		virtual property int	Width				{ int get(); }
		virtual property int	Height				{ int get(); }
		virtual property int	ArraySizeOrDepth	{ int get(); }

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

		property int	Width			{ int get() { return m_pTexture->GetWidth(); } }
		property int	Height			{ int get() { return m_pTexture->GetHeight(); } }
		property int	Depth			{ int get() { return m_pTexture->GetDepth(); } }
		property int	MipLevelsCount	{ int get() { return m_pTexture->GetMipLevelsCount(); } }

	public:

		Texture3D( Device^ _device, int _width, int _height, int _depth, int _mipLevelsCount, PIXEL_FORMAT _pixelFormat, bool _staging, bool _UAV, cli::array<PixelsBuffer^>^ _mipLevelsContent );
		~Texture3D() {
 			delete m_pTexture;
		}

		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture3D^ _Source ) {
			m_pTexture->CopyFrom( *_Source->m_pTexture );
		}

		PixelsBuffer^	Map( int _mipLevelIndex ) {
			D3D11_MAPPED_SUBRESOURCE&	MappedResource = m_pTexture->Map( _mipLevelIndex );
			return gcnew PixelsBuffer( MappedResource );
		}

		void			UnMap( int _mipLevelIndex ) {
			m_pTexture->UnMap( _mipLevelIndex );
		}

		// Views
		View3D^		GetView()				{ return GetView( 0, 0, 0, 0 ); }
		View3D^		GetView( int _mipLevelStart, int _mipLevelsCount, int _sliceStart, int _slicesCount ) { return gcnew View3D( this, _mipLevelStart, _mipLevelsCount, _sliceStart, _slicesCount ); }
		View3D^		GetView( int _mipLevelStart, int _mipLevelsCount, int _sliceStart, int _slicesCount, bool _asArray ) { return gcnew View3D( this, _mipLevelStart, _mipLevelsCount, _sliceStart, _slicesCount, _asArray ); }

		// Uploads the texture to the shader
		void		Set( int _slotIndex )	{ Set( _slotIndex, nullptr ); }
		void		SetVS( int _slotIndex )	{ SetVS( _slotIndex, nullptr ); }
		void		SetHS( int _slotIndex )	{ SetHS( _slotIndex, nullptr ); }
		void		SetDS( int _slotIndex )	{ SetDS( _slotIndex, nullptr ); }
		void		SetGS( int _slotIndex )	{ SetGS( _slotIndex, nullptr ); }
		void		SetPS( int _slotIndex )	{ SetPS( _slotIndex, nullptr ); }
		void		SetCS( int _slotIndex )	{ SetCS( _slotIndex, nullptr ); }

		void		Set( int _slotIndex, View3D^ _view );
		void		SetVS( int _slotIndex, View3D^ _view );
		void		SetHS( int _slotIndex, View3D^ _view );
		void		SetDS( int _slotIndex, View3D^ _view );
		void		SetGS( int _slotIndex, View3D^ _view );
		void		SetPS( int _slotIndex, View3D^ _view );
		void		SetCS( int _slotIndex, View3D^ _view );
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
