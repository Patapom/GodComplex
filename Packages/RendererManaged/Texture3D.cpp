// This is the main DLL file.

#include "stdafx.h"

#include "Texture3D.h"


namespace RendererManaged {

	Texture3D::Texture3D( Device^ _Device, int _Width, int _Height, int _Depth, int _MipLevelsCount, PIXEL_FORMAT _PixelFormat, bool _Staging, bool _UAV, cli::array<PixelsBuffer^>^ _MipLevelsContent )
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

		m_pTexture = new ::Texture3D( *_Device->m_pDevice, _Width, _Height, _Depth, *pDescriptor, _MipLevelsCount, ppContent, _Staging, _UAV );

		delete[] ppContent;
	}

	void	Texture3D::Set( int _SlotIndex, View3D^ _view )		{ m_pTexture->Set( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture3D::SetVS( int _SlotIndex, View3D^ _view )	{ m_pTexture->SetVS( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture3D::SetHS( int _SlotIndex, View3D^ _view )	{ m_pTexture->SetHS( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture3D::SetDS( int _SlotIndex, View3D^ _view )	{ m_pTexture->SetDS( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture3D::SetGS( int _SlotIndex, View3D^ _view )	{ m_pTexture->SetGS( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture3D::SetPS( int _SlotIndex, View3D^ _view )	{ m_pTexture->SetPS( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture3D::SetCS( int _SlotIndex, View3D^ _view )	{ m_pTexture->SetCS( _SlotIndex, true, _view != nullptr ? _view->SRV : NULL ); }


	int								View3D::Width::get() { return m_Owner->Width; }
	int								View3D::Height::get() { return m_Owner->Height; }
	int								View3D::ArraySizeOrDepth::get() { return m_Owner->Depth; }
	::ID3D11ShaderResourceView*		View3D::SRV::get() { return m_AsArray ? m_Owner->m_pTexture->GetSRV( m_MipLevelStart, m_MipLevelsCount, m_SliceStart, m_SlicesCount, m_AsArray ) : m_Owner->m_pTexture->GetSRV( m_MipLevelStart, m_MipLevelsCount ); }
	::ID3D11RenderTargetView*		View3D::RTV::get() { return m_Owner->m_pTexture->GetRTV( m_MipLevelStart, m_SliceStart, m_SlicesCount ); }
	::ID3D11UnorderedAccessView*	View3D::UAV::get() { return m_Owner->m_pTexture->GetUAV( m_MipLevelStart, m_SliceStart, m_SlicesCount ); }
}
