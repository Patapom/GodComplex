// This is the main DLL file.

#include "stdafx.h"

#include "Texture2D.h"

namespace RendererManaged {

	Texture2D::Texture2D( Device^ _Device, int _Width, int _Height, int _ArraySize, int _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _Content ) {
 		IPixelFormatDescriptor*	pDescriptor = GetDescriptor( _PixelFormat );

		void**	ppContent = NULL;
		if ( _Content != nullptr ) {

			cli::pin_ptr< PixelsBuffer^ >	PinThis = &_Content[0];

			int		ArraySize = abs(_ArraySize);
			ppContent = new void*[_MipLevelsCount*ArraySize];
			for ( int ArrayIndex=0; ArrayIndex < ArraySize; ArrayIndex++ ) {
				for ( int MipLevelIndex=0; MipLevelIndex < _MipLevelsCount; MipLevelIndex++ ) {
					cli::pin_ptr< Byte >	PinThat = &_Content[ArrayIndex*_MipLevelsCount+MipLevelIndex]->m_Buffer[0];
					ppContent[ArrayIndex*_MipLevelsCount+MipLevelIndex] = PinThat;
				}
			}
		}

		m_pTexture = new ::Texture2D( *_Device->m_pDevice, _Width, _Height, _ArraySize, *pDescriptor, _MipLevelsCount, ppContent, _Staging, _UAV );

		delete[] ppContent;
	}

	Texture2D::Texture2D( Device^ _Device, int _Width, int _Height, int _ArraySize, DEPTH_STENCIL_FORMAT _DepthStencilFormat ) {
 		IDepthStencilFormatDescriptor*	pDescriptor = GetDescriptor( _DepthStencilFormat );
		m_pTexture = new ::Texture2D( *_Device->m_pDevice, _Width, _Height, *pDescriptor, _ArraySize );
	}

	void	Texture2D::Set( int _SlotIndex, View2D^ _view )		{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetVS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetHS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetDS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetGS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetPS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetCS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }


	int								View2D::Width::get() { return m_Owner->Width; }
	int								View2D::Height::get() { return m_Owner->Height; }
	int								View2D::ArraySizeOrDepth::get() { return m_Owner->ArraySize; }
	::ID3D11ShaderResourceView*		View2D::SRV::get() { return m_Owner->m_pTexture->GetSRV( m_MipLevelStart, m_MipLevelsCount, m_ArrayStart, m_ArraySize, m_AsArray ); }
	::ID3D11RenderTargetView*		View2D::RTV::get() { return m_Owner->m_pTexture->GetRTV( m_MipLevelStart, m_ArrayStart, m_ArraySize ); }
	::ID3D11UnorderedAccessView*	View2D::UAV::get() { return m_Owner->m_pTexture->GetUAV( m_MipLevelStart, m_ArrayStart, m_ArraySize ); }
	::ID3D11DepthStencilView*		View2D::DSV::get() { return m_Owner->m_pTexture->GetDSV( m_ArrayStart, m_ArraySize ); }
}
