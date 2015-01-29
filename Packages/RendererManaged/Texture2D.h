// RendererManaged.h

#pragma once
#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace RendererManaged {

	ref class Texture2D;

	public ref class	View2D : public IView
	{
	internal:
		Texture2D^	m_Owner;
		int			m_MipLevelStart;
		int			m_MipLevelsCount;
		int			m_ArrayStart;
		int			m_ArraySize;
		bool		m_AsArray;

		View2D( Texture2D^ _Owner, int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize ) : m_Owner( _Owner ), m_MipLevelStart( _MipLevelStart ), m_MipLevelsCount( _MipLevelsCount ), m_ArrayStart( _ArrayStart ), m_ArraySize( _ArraySize ), m_AsArray( false ) {}
		View2D( Texture2D^ _Owner, int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize, bool _AsArray ) : m_Owner( _Owner ), m_MipLevelStart( _MipLevelStart ), m_MipLevelsCount( _MipLevelsCount ), m_ArrayStart( _ArrayStart ), m_ArraySize( _ArraySize ), m_AsArray( _AsArray ) {}

	public:
		virtual property int	Width				{ int get(); }
		virtual property int	Height				{ int get(); }
		virtual property int	ArraySizeOrDepth	{ int get(); }

		virtual property ::ID3D11ShaderResourceView*	SRV { ::ID3D11ShaderResourceView*	get(); }
		virtual property ::ID3D11RenderTargetView*		RTV { ::ID3D11RenderTargetView*		get(); }
		virtual property ::ID3D11UnorderedAccessView*	UAV { ::ID3D11UnorderedAccessView*	get(); }
		virtual property ::ID3D11DepthStencilView*		DSV { ::ID3D11DepthStencilView*		get(); }
	};

	public ref class Texture2D
	{
	internal:

		::Texture2D*	m_pTexture;

	public:

		property int	Width			{ int get() { return m_pTexture->GetWidth(); } }
		property int	Height			{ int get() { return m_pTexture->GetHeight(); } }
		property int	ArraySize		{ int get() { return m_pTexture->GetArraySize(); } }
		property int	MipLevelsCount	{ int get() { return m_pTexture->GetMipLevelsCount(); } }

		void*	GetWrappedtexture()	{ return m_pTexture; }

	public:

		Texture2D( Device^ _Device, int _Width, int _Height, int _ArraySize, int _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _MipLevelsContent );
		Texture2D( Device^ _Device, int _Width, int _Height, int _ArraySize, DEPTH_STENCIL_FORMAT _DepthStencilFormat );
		~Texture2D() {
 			delete m_pTexture;
		}


		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture2D^ _Source ) {
			m_pTexture->CopyFrom( *_Source->m_pTexture );
		}

		PixelsBuffer^	Map( int _MipLevelIndex, int _ArrayIndex ) {
			D3D11_MAPPED_SUBRESOURCE&	MappedResource = m_pTexture->Map( _MipLevelIndex, _ArrayIndex );
			return gcnew PixelsBuffer( MappedResource );
		}

		void			UnMap( int _MipLevelIndex, int _ArrayIndex ) {
			m_pTexture->UnMap( _MipLevelIndex, _ArrayIndex );
		}

		// Views
		View2D^		GetView()				{ return GetView( 0, 0, 0, 0 ); }
		View2D^		GetView( int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize ) { return gcnew View2D( this, _MipLevelStart, _MipLevelsCount, _ArrayStart, _ArraySize ); }
		View2D^		GetView( int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize, bool _AsArray ) { return gcnew View2D( this, _MipLevelStart, _MipLevelsCount, _ArrayStart, _ArraySize, _AsArray ); }

		// Uploads the texture to the shader
		void		Set( int _SlotIndex )	{ Set( _SlotIndex, nullptr ); }
		void		SetVS( int _SlotIndex )	{ SetVS( _SlotIndex, nullptr ); }
		void		SetHS( int _SlotIndex )	{ SetHS( _SlotIndex, nullptr ); }
		void		SetDS( int _SlotIndex )	{ SetDS( _SlotIndex, nullptr ); }
		void		SetGS( int _SlotIndex )	{ SetGS( _SlotIndex, nullptr ); }
		void		SetPS( int _SlotIndex )	{ SetPS( _SlotIndex, nullptr ); }
		void		SetCS( int _SlotIndex )	{ SetCS( _SlotIndex, nullptr ); }

		void		Set( int _SlotIndex, View2D^ _view );
		void		SetVS( int _SlotIndex, View2D^ _view );
		void		SetHS( int _SlotIndex, View2D^ _view );
		void		SetDS( int _SlotIndex, View2D^ _view );
		void		SetGS( int _SlotIndex, View2D^ _view );
		void		SetPS( int _SlotIndex, View2D^ _view );
		void		SetCS( int _SlotIndex, View2D^ _view );
		void		RemoveFromLastAssignedSlots()	{ m_pTexture->RemoveFromLastAssignedSlots(); }

		// Upload the texture as a UAV for a compute shader
		void		SetCSUAV( int _SlotIndex )		{ m_pTexture->SetCSUAV( _SlotIndex ); }
		void		RemoveFromLastAssignedSlotUAV()	{ m_pTexture->RemoveFromLastAssignedSlotUAV(); }

	internal:

		Texture2D( const ::Texture2D& _ExistingTexture ) {
			m_pTexture = const_cast< ::Texture2D* >( &_ExistingTexture );
		}
	};
}
