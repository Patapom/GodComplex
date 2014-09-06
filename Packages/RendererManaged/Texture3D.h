// RendererManaged.h

#pragma once
#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace RendererManaged {

	ref class Texture3D;

	public ref class	View3D : public IView
	{
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

	public ref class Texture3D
	{
	internal:

		::Texture3D*	m_pTexture;

	public:

		property int	Width			{ int get() { return m_pTexture->GetWidth(); } }
		property int	Height			{ int get() { return m_pTexture->GetHeight(); } }
		property int	Depth			{ int get() { return m_pTexture->GetDepth(); } }
		property int	MipLevelsCount	{ int get() { return m_pTexture->GetMipLevelsCount(); } }

	public:

		Texture3D( Device^ _Device, int _Width, int _Height, int _Depth, int _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _MipLevelsContent );
		~Texture3D()
		{
 			delete m_pTexture;
		}

		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture3D^ _Source )
		{
			m_pTexture->CopyFrom( *_Source->m_pTexture );
		}

		PixelsBuffer^	Map( int _MipLevelIndex )
		{
			D3D11_MAPPED_SUBRESOURCE&	MappedResource = m_pTexture->Map( _MipLevelIndex );
			return gcnew PixelsBuffer( MappedResource );
		}

		void			UnMap( int _MipLevelIndex )
		{
			m_pTexture->UnMap( _MipLevelIndex );
		}

		// Views
		View3D^		GetView()				{ return GetView( 0, 0, 0, 0 ); }
		View3D^		GetView( int _MipLevelStart, int _MipLevelsCount, int _SliceStart, int _SlicesCount ) { return gcnew View3D( this, _MipLevelStart, _MipLevelsCount, _SliceStart, _SlicesCount ); }
		View3D^		GetView( int _MipLevelStart, int _MipLevelsCount, int _SliceStart, int _SlicesCount, bool _AsArray ) { return gcnew View3D( this, _MipLevelStart, _MipLevelsCount, _SliceStart, _SlicesCount, _AsArray ); }

		// Uploads the texture to the shader
		void		Set( int _SlotIndex )	{ Set( _SlotIndex, nullptr ); }
		void		SetVS( int _SlotIndex )	{ SetVS( _SlotIndex, nullptr ); }
		void		SetHS( int _SlotIndex )	{ SetHS( _SlotIndex, nullptr ); }
		void		SetDS( int _SlotIndex )	{ SetDS( _SlotIndex, nullptr ); }
		void		SetGS( int _SlotIndex )	{ SetGS( _SlotIndex, nullptr ); }
		void		SetPS( int _SlotIndex )	{ SetPS( _SlotIndex, nullptr ); }
		void		SetCS( int _SlotIndex )	{ SetCS( _SlotIndex, nullptr ); }

		void		Set( int _SlotIndex, View3D^ _view );
		void		SetVS( int _SlotIndex, View3D^ _view );
		void		SetHS( int _SlotIndex, View3D^ _view );
		void		SetDS( int _SlotIndex, View3D^ _view );
		void		SetGS( int _SlotIndex, View3D^ _view );
		void		SetPS( int _SlotIndex, View3D^ _view );
		void		SetCS( int _SlotIndex, View3D^ _view );
		void		RemoveFromLastAssignedSlots()	{ m_pTexture->RemoveFromLastAssignedSlots(); }

		// Upload the texture as a UAV for a compute shader
		void		SetCSUAV( int _SlotIndex )		{ m_pTexture->SetCSUAV( _SlotIndex ); }
		void		RemoveFromLastAssignedSlotUAV()	{ m_pTexture->RemoveFromLastAssignedSlotUAV(); }

	internal:

		Texture3D( const ::Texture3D& _ExistingTexture )
		{
			m_pTexture = const_cast< ::Texture3D* >( &_ExistingTexture );
		}
	};
}
