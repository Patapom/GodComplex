// This is the main DLL file.

#include "stdafx.h"

#include "Texture2D.h"

namespace RendererManaged {

	Texture2D::Texture2D( Device^ _Device, int _Width, int _Height, int _ArraySize, int _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _MipLevelsContent )
	{
 		IPixelFormatDescriptor*	pDescriptor = GetDescriptor( _PixelFormat );

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

	void	Texture2D::Set( int _SlotIndex, View2D^ _view )		{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetVS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetHS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetDS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetGS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetPS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetCS( int _SlotIndex, View2D^ _view )	{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }


	::ID3D11ShaderResourceView*		View2D::SRV::get() { return m_Owner->m_pTexture->GetSRV( m_MipLevelStart, m_MipLevelsCount, m_ArrayStart, m_ArraySize ); }
	::ID3D11RenderTargetView*		View2D::RTV::get() { return m_Owner->m_pTexture->GetRTV( m_MipLevelStart, m_ArrayStart, m_ArraySize ); }
	::ID3D11UnorderedAccessView*	View2D::UAV::get() { return m_Owner->m_pTexture->GetUAV( m_MipLevelStart, m_ArrayStart, m_ArraySize ); }
	::ID3D11DepthStencilView*		View2D::DSV::get() { return m_Owner->m_pTexture->GetDSV( m_ArrayStart, m_ArraySize ); }
}
