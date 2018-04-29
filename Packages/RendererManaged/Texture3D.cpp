// This is the main DLL file.

#include "stdafx.h"

#include "Texture3D.h"

namespace Renderer {

	Texture3D::Texture3D( Device^ _device, UInt32 _width, UInt32 _height, UInt32 _depth, UInt32 _mipLevelsCount, ImageUtility::PIXEL_FORMAT _pixelFormat, ImageUtility::COMPONENT_FORMAT _componentFormat, bool _staging, bool _UAV, cli::array<PixelsBuffer^>^ _mipLevelsContent )
		: m_pixelFormat( _pixelFormat )
		, m_componentFormat( _componentFormat )
	{
 		BaseLib::PIXEL_FORMAT		pixelFormat = BaseLib::PIXEL_FORMAT( _pixelFormat );
		BaseLib::COMPONENT_FORMAT	componentFormat = BaseLib::COMPONENT_FORMAT( _componentFormat );

		void**	ppContent = NULL;
		if ( _mipLevelsContent != nullptr ) {
			ppContent = new void*[_mipLevelsCount];
			pin_ptr<Byte>	ptr;
			for ( UInt32 MipLevelIndex=0; MipLevelIndex < _mipLevelsCount; MipLevelIndex++ ) {
				ptr = &_mipLevelsContent[MipLevelIndex]->m_Buffer[0];
				ppContent[MipLevelIndex] = ptr;
			}
		}

		m_texture = new ::Texture3D( *_device->m_pDevice, _width, _height, _depth, _mipLevelsCount, pixelFormat, componentFormat, ppContent, _staging, _UAV );

		delete[] ppContent;
	}
	Texture3D::Texture3D( Device^ _device, ImageUtility::ImagesMatrix^ _images, ImageUtility::COMPONENT_FORMAT _componentFormat )
		: m_pixelFormat( _images->Format)
		, m_componentFormat( _componentFormat )
	{
		
		ImageUtilityLib::ImagesMatrix*	nativeObject = reinterpret_cast< ImageUtilityLib::ImagesMatrix* >( _images->NativeObject.ToPointer() );
		m_texture = new ::Texture3D( *_device->m_pDevice, *nativeObject, BaseLib::COMPONENT_FORMAT( _componentFormat ) );
	}

	void	Texture3D::Set( UInt32 _slotIndex )		{ m_texture->Set( _slotIndex, true, NULL ); }
	void	Texture3D::SetVS( UInt32 _slotIndex )	{ m_texture->SetVS( _slotIndex, true, NULL ); }
	void	Texture3D::SetHS( UInt32 _slotIndex )	{ m_texture->SetHS( _slotIndex, true, NULL ); }
	void	Texture3D::SetDS( UInt32 _slotIndex )	{ m_texture->SetDS( _slotIndex, true, NULL ); }
	void	Texture3D::SetGS( UInt32 _slotIndex )	{ m_texture->SetGS( _slotIndex, true, NULL ); }
	void	Texture3D::SetPS( UInt32 _slotIndex )	{ m_texture->SetPS( _slotIndex, true, NULL ); }
	void	Texture3D::SetCS( UInt32 _slotIndex )	{ m_texture->SetCS( _slotIndex, true, NULL ); }
	void	Texture3D::SetCSUAV( int _slotIndex )	{ m_texture->SetCSUAV( _slotIndex, NULL ); }

	View3D^		Texture3D::GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _sliceStart, UInt32 _slicesCount ) { return gcnew View3D( this, _mipLevelStart, _mipLevelsCount, _sliceStart, _slicesCount ); }
	View3D^		Texture3D::GetView( UInt32 _mipLevelStart, UInt32 _mipLevelsCount, UInt32 _sliceStart, UInt32 _slicesCount, bool _asArray ) { return gcnew View3D( this, _mipLevelStart, _mipLevelsCount, _sliceStart, _slicesCount, _asArray ); }

	//////////////////////////////////////////////////////////////////////////
	// View
	UInt32							View3D::Width::get()			{ return m_owner->WidthAtMip[m_mipLevelStart]; }
	UInt32							View3D::Height::get()			{ return m_owner->HeightAtMip[m_mipLevelStart]; }
	UInt32							View3D::ArraySizeOrDepth::get()	{ return m_owner->DepthAtMip[m_mipLevelStart]; }
	::ID3D11ShaderResourceView*		View3D::SRV::get() { return m_asArray ? m_owner->m_texture->GetSRV( m_mipLevelStart, m_mipLevelsCount, m_sliceStart, m_slicesCount, m_asArray ) : m_owner->m_texture->GetSRV( m_mipLevelStart, m_mipLevelsCount ); }
	::ID3D11RenderTargetView*		View3D::RTV::get() { return m_owner->m_texture->GetRTV( m_mipLevelStart, m_sliceStart, m_slicesCount ); }
	::ID3D11UnorderedAccessView*	View3D::UAV::get() { return m_owner->m_texture->GetUAV( m_mipLevelStart, m_sliceStart, m_slicesCount ); }

	void	View3D::Set( UInt32 _slotIndex )		{ m_owner->m_texture->Set(		_slotIndex, true, SRV ); }
	void	View3D::SetVS( UInt32 _slotIndex )		{ m_owner->m_texture->SetVS(	_slotIndex, true, SRV ); }
	void	View3D::SetHS( UInt32 _slotIndex )		{ m_owner->m_texture->SetHS(	_slotIndex, true, SRV ); }
	void	View3D::SetDS( UInt32 _slotIndex )		{ m_owner->m_texture->SetDS(	_slotIndex, true, SRV ); }
	void	View3D::SetGS( UInt32 _slotIndex )		{ m_owner->m_texture->SetGS(	_slotIndex, true, SRV ); }
	void	View3D::SetPS( UInt32 _slotIndex )		{ m_owner->m_texture->SetPS(	_slotIndex, true, SRV ); }
	void	View3D::SetCS( UInt32 _slotIndex )		{ m_owner->m_texture->SetCS(	_slotIndex, true, SRV ); }
	void	View3D::SetCSUAV( UInt32 _slotIndex )	{ m_owner->m_texture->SetCSUAV(	_slotIndex,		  UAV ); }
}
