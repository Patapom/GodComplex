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
		UInt32		m_mipLevelStart;
		UInt32		m_mipLevelsCount;
		UInt32		m_arrayStart;
		UInt32		m_arraySize;
		bool		m_asArray;

		View2D( Texture2D^ _Owner, UInt32 _MipLevelStart, UInt32 _MipLevelsCount, UInt32 _ArrayStart, UInt32 _ArraySize ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _MipLevelsCount ), m_arrayStart( _ArrayStart ), m_arraySize( _ArraySize ), m_asArray( false ) {}
		View2D( Texture2D^ _Owner, UInt32 _MipLevelStart, UInt32 _MipLevelsCount, UInt32 _ArrayStart, UInt32 _ArraySize, bool _AsArray ) : m_owner( _Owner ), m_mipLevelStart( _MipLevelStart ), m_mipLevelsCount( _MipLevelsCount ), m_arrayStart( _ArrayStart ), m_arraySize( _ArraySize ), m_asArray( _AsArray ) {}

	public:

		virtual property UInt32	Width				{ UInt32 get(); }
		virtual property UInt32	Height				{ UInt32 get(); }
		virtual property UInt32	ArraySizeOrDepth	{ UInt32 get(); }

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

		property UInt32	Width			{ UInt32 get() { return m_pTexture->GetWidth(); } }
		property UInt32	Height			{ UInt32 get() { return m_pTexture->GetHeight(); } }
		property UInt32	ArraySize		{ UInt32 get() { return m_pTexture->GetArraySize(); } }
		property UInt32	MipLevelsCount	{ UInt32 get() { return m_pTexture->GetMipLevelsCount(); } }

		void*	GetWrappedtexture()	{ return m_pTexture; }

	public:

		// _Content must be of size _ArraySize * _MipLevelsCount and must contain all consecutive mips for each slice (e.g. 3 mips and array size 2 : [ Mip0_slice0, Mip1_slice0, Mip2_slice0, Mip0_slice1, Mip1_slice1, Mip2_slice1])
		Texture2D( Device^ _device, UInt32 _Width, UInt32 _Height, int _ArraySize, UInt32 _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _Content );
		Texture2D( Device^ _device, UInt32 _Width, UInt32 _Height, UInt32 _ArraySize, DEPTH_STENCIL_FORMAT _DepthStencilFormat );
		~Texture2D() {
 			delete m_pTexture;
		}


		// Generally used to copy a GPU texture to a CPU staging resource or vice-versa
		void	CopyFrom( Texture2D^ _source ) {
			m_pTexture->CopyFrom( *_source->m_pTexture );
		}

		PixelsBuffer^	Map( UInt32 _mipLevelIndex, UInt32 _arrayIndex ) {
			D3D11_MAPPED_SUBRESOURCE&	MappedResource = m_pTexture->Map( _mipLevelIndex, _arrayIndex );
			return gcnew PixelsBuffer( MappedResource );
		}

		void			UnMap( UInt32 _mipLevelIndex, UInt32 _arrayIndex ) {
			m_pTexture->UnMap( _mipLevelIndex, _arrayIndex );
		}

		// Views
		View2D^		GetView()				{ return GetView( 0, 0, 0, 0 ); }
		View2D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _arrayStart, UInt32 _arraySize )					{ return gcnew View2D( this, _mipLevelStart, _mipLevelsCount, _arrayStart, _arraySize ); }
		View2D^		GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _arrayStart, UInt32 _arraySize, bool _asArray )	{ return gcnew View2D( this, _mipLevelStart, _mipLevelsCount, _arrayStart, _arraySize, _asArray ); }

		// Uploads the texture to the shader
		void		Set( UInt32 _slotIndex )	{ Set( _slotIndex, nullptr ); }
		void		SetVS( UInt32 _slotIndex )	{ SetVS( _slotIndex, nullptr ); }
		void		SetHS( UInt32 _slotIndex )	{ SetHS( _slotIndex, nullptr ); }
		void		SetDS( UInt32 _slotIndex )	{ SetDS( _slotIndex, nullptr ); }
		void		SetGS( UInt32 _slotIndex )	{ SetGS( _slotIndex, nullptr ); }
		void		SetPS( UInt32 _slotIndex )	{ SetPS( _slotIndex, nullptr ); }
		void		SetCS( UInt32 _slotIndex )	{ SetCS( _slotIndex, nullptr ); }

		void		Set( UInt32 _slotIndex, View2D^ _view );
		void		SetVS( UInt32 _slotIndex, View2D^ _view );
		void		SetHS( UInt32 _slotIndex, View2D^ _view );
		void		SetDS( UInt32 _slotIndex, View2D^ _view );
		void		SetGS( UInt32 _slotIndex, View2D^ _view );
		void		SetPS( UInt32 _slotIndex, View2D^ _view );
		void		SetCS( UInt32 _slotIndex, View2D^ _view );
		void		RemoveFromLastAssignedSlots()	{ m_pTexture->RemoveFromLastAssignedSlots(); }

		// Upload the texture as a UAV for a compute shader
		void		SetCSUAV( UInt32 _slotIndex )					{ m_pTexture->SetCSUAV( _slotIndex ); }
		void		SetCSUAV( UInt32 _slotIndex, View2D^ _view  );
		void		RemoveFromLastAssignedSlotUAV()	{ m_pTexture->RemoveFromLastAssignedSlotUAV(); }

	internal:

		Texture2D( const ::Texture2D& _existingTexture ) {
			m_pTexture = const_cast< ::Texture2D* >( &_existingTexture );
		}
	};
}
