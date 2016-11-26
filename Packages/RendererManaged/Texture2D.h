// RendererManaged.h

#pragma once

#include "Device.h"

#include "PixelFormats.h"
#include "PixelsBuffer.h"

using namespace System;

namespace Renderer {

	ref class Texture2D;

	// Wraps a 2D texture view (SRV, RTV, DSV or UAV)
	public ref class	View2D : public IView {
	internal:
		Texture2D^	m_owner;
		int			m_mipLevelStart;
		int			m_mipLevelsCount;
		int			m_arrayStart;
		int			m_arraySize;
		bool		m_asArray;

		View2D( Texture2D^ _Owner, int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _MipLevelsCount ), m_arrayStart( _ArrayStart ), m_arraySize( _ArraySize ), m_asArray( false ) {}
		View2D( Texture2D^ _Owner, int _MipLevelStart, int _MipLevelsCount, int _ArrayStart, int _ArraySize, bool _AsArray ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _MipLevelsCount ), m_arrayStart( _ArrayStart ), m_arraySize( _ArraySize ), m_asArray( _AsArray ) {}

	public:

		virtual property int	Width				{ int get(); }
		virtual property int	Height				{ int get(); }
		virtual property int	ArraySizeOrDepth	{ int get(); }

		virtual property ::ID3D11ShaderResourceView*	SRV { ::ID3D11ShaderResourceView*	get(); }
		virtual property ::ID3D11RenderTargetView*		RTV { ::ID3D11RenderTargetView*		get(); }
		virtual property ::ID3D11UnorderedAccessView*	UAV { ::ID3D11UnorderedAccessView*	get(); }
		virtual property ::ID3D11DepthStencilView*		DSV { ::ID3D11DepthStencilView*		get(); }
	};

	// Wraps a 2D texture (2D, 2DArray, CubeMap, CubeMapArray, RenderTarget, DepthStencilBuffer)
	public ref class Texture2D {
	internal:

		::Texture2D*	m_pTexture;

	public:

		property int	Width			{ int get() { return m_pTexture->GetWidth(); } }
		property int	Height			{ int get() { return m_pTexture->GetHeight(); } }
		property int	ArraySize		{ int get() { return m_pTexture->GetArraySize(); } }
		property int	MipLevelsCount	{ int get() { return m_pTexture->GetMipLevelsCount(); } }

		void*	GetWrappedtexture()	{ return m_pTexture; }

	public:

		// _Content must be of size _ArraySize * _MipLevelsCount and must contain all consecutive mips for each slice (e.g. 3 mips and array size 2 : [ Mip0_slice0, Mip1_slice0, Mip2_slice0, Mip0_slice1, Mip1_slice1, Mip2_slice1])
		Texture2D( Device^ _device, int _Width, int _Height, int _ArraySize, int _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _Content );
		Texture2D( Device^ _device, int _Width, int _Height, int _ArraySize, DEPTH_STENCIL_FORMAT _DepthStencilFormat );
		~Texture2D() {
 			delete m_pTexture;
		}


		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture2D^ _source ) {
			m_pTexture->CopyFrom( *_source->m_pTexture );
		}

		PixelsBuffer^	Map( int _mipLevelIndex, int _arrayIndex ) {
			D3D11_MAPPED_SUBRESOURCE&	MappedResource = m_pTexture->Map( _mipLevelIndex, _arrayIndex );
			return gcnew PixelsBuffer( MappedResource );
		}

		void			UnMap( int _mipLevelIndex, int _arrayIndex ) {
			m_pTexture->UnMap( _mipLevelIndex, _arrayIndex );
		}

		// Views
		View2D^		GetView()				{ return GetView( 0, 0, 0, 0 ); }
		View2D^		GetView( int _mipLevelStart, int _mipLevelsCount, int _arrayStart, int _arraySize ) { return gcnew View2D( this, _mipLevelStart, _mipLevelsCount, _arrayStart, _arraySize ); }
		View2D^		GetView( int _mipLevelStart, int _mipLevelsCount, int _arrayStart, int _arraySize, bool _asArray ) { return gcnew View2D( this, _mipLevelStart, _mipLevelsCount, _arrayStart, _arraySize, _asArray ); }

		// Uploads the texture to the shader
		void		Set( int _slotIndex )	{ Set( _slotIndex, nullptr ); }
		void		SetVS( int _slotIndex )	{ SetVS( _slotIndex, nullptr ); }
		void		SetHS( int _slotIndex )	{ SetHS( _slotIndex, nullptr ); }
		void		SetDS( int _slotIndex )	{ SetDS( _slotIndex, nullptr ); }
		void		SetGS( int _slotIndex )	{ SetGS( _slotIndex, nullptr ); }
		void		SetPS( int _slotIndex )	{ SetPS( _slotIndex, nullptr ); }
		void		SetCS( int _slotIndex )	{ SetCS( _slotIndex, nullptr ); }

		void		Set( int _slotIndex, View2D^ _view );
		void		SetVS( int _slotIndex, View2D^ _view );
		void		SetHS( int _slotIndex, View2D^ _view );
		void		SetDS( int _slotIndex, View2D^ _view );
		void		SetGS( int _slotIndex, View2D^ _view );
		void		SetPS( int _slotIndex, View2D^ _view );
		void		SetCS( int _slotIndex, View2D^ _view );
		void		RemoveFromLastAssignedSlots()	{ m_pTexture->RemoveFromLastAssignedSlots(); }

		// Upload the texture as a UAV for a compute shader
		void		SetCSUAV( int _slotIndex )					{ m_pTexture->SetCSUAV( _slotIndex ); }
		void		SetCSUAV( int _slotIndex, View2D^ _view  );
		void		RemoveFromLastAssignedSlotUAV()	{ m_pTexture->RemoveFromLastAssignedSlotUAV(); }

	internal:

		Texture2D( const ::Texture2D& _existingTexture ) {
			m_pTexture = const_cast< ::Texture2D* >( &_existingTexture );
		}
	};
}
