// This is the main DLL file.

#include "stdafx.h"

#include "Texture2D.h"

using namespace ImageUtility;

namespace Renderer {

	Texture2D::Texture2D( Device^ _device, UInt32 _width, UInt32 _height, int _arraySize, UInt32 _mipLevelsCount, PIXEL_FORMAT _pixelFormat, bool _staging, bool _UAV, array<PixelsBuffer^>^ _content ) {
 		IPixelFormatDescriptor*	descriptor = GetDescriptor( _pixelFormat );

		void**	ppContent = NULL;
		if ( _content != nullptr ) {
			pin_ptr< PixelsBuffer^ >	ptr = &_content[0];

			UInt32	arraySize = abs(_arraySize);
			ppContent = new void*[_mipLevelsCount*arraySize];
			for ( UInt32 arrayIndex=0; arrayIndex < arraySize; arrayIndex++ ) {
				for ( UInt32 mipLevelIndex=0; mipLevelIndex < _mipLevelsCount; mipLevelIndex++ ) {
					pin_ptr< Byte >	ptrContent = &_content[arrayIndex*_mipLevelsCount+mipLevelIndex]->m_Buffer[0];
					ppContent[arrayIndex*_mipLevelsCount+mipLevelIndex] = ptrContent;
				}
			}
		}

		m_pTexture = new ::Texture2D( *_device->m_pDevice, _width, _height, _arraySize, _mipLevelsCount, *descriptor, ppContent, _staging, _UAV );

		delete[] ppContent;
	}

	Texture2D::Texture2D( Device^ _device, UInt32 _width, UInt32 _height, UInt32 _arraySize, DEPTH_STENCIL_FORMAT _depthStencilFormat ) {
 		IDepthStencilFormatDescriptor*	pDescriptor = GetDescriptor( _depthStencilFormat );
		m_pTexture = new ::Texture2D( *_device->m_pDevice, _width, _height, _arraySize, *pDescriptor );
	}

	void	Texture2D::Set( UInt32 _slotIndex, View2D^ _view )			{ m_pTexture->Set( _slotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetVS( UInt32 _slotIndex, View2D^ _view )		{ m_pTexture->SetVS( _slotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetHS( UInt32 _slotIndex, View2D^ _view )		{ m_pTexture->SetHS( _slotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetDS( UInt32 _slotIndex, View2D^ _view )		{ m_pTexture->SetDS( _slotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetGS( UInt32 _slotIndex, View2D^ _view )		{ m_pTexture->SetGS( _slotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetPS( UInt32 _slotIndex, View2D^ _view )		{ m_pTexture->SetPS( _slotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetCS( UInt32 _slotIndex, View2D^ _view )		{ m_pTexture->SetCS( _slotIndex, true, _view != nullptr ? _view->SRV : NULL ); }
	void	Texture2D::SetCSUAV( UInt32 _slotIndex, View2D^ _view )		{ m_pTexture->SetCSUAV( _slotIndex, _view != nullptr ? _view->UAV : NULL ); }


	//////////////////////////////////////////////////////////////////////////
	// View
	UInt32							View2D::Width::get() { return m_owner->Width; }
	UInt32							View2D::Height::get() { return m_owner->Height; }
	UInt32							View2D::ArraySizeOrDepth::get() { return m_owner->ArraySize; }
	::ID3D11ShaderResourceView*		View2D::SRV::get() { return m_owner->m_pTexture->GetSRV( m_mipLevelStart, m_mipLevelsCount, m_arrayStart, m_arraySize, m_asArray ); }
	::ID3D11RenderTargetView*		View2D::RTV::get() { return m_owner->m_pTexture->GetRTV( m_mipLevelStart, m_arrayStart, m_arraySize ); }
	::ID3D11UnorderedAccessView*	View2D::UAV::get() { return m_owner->m_pTexture->GetUAV( m_mipLevelStart, m_arrayStart, m_arraySize ); }
	::ID3D11DepthStencilView*		View2D::DSV::get() { return m_owner->m_pTexture->GetDSV( m_arrayStart, m_arraySize ); }


	//////////////////////////////////////////////////////////////////////////
	// Image bridge
	Texture2D^	Texture2D::CreateTexture2D( Device^ _device, ImageFile^ _image, UInt32 _mipLevelsCount, COMPONENT_FORMAT _componentFormat ) {
		if ( _image == nullptr ) throw gcnew Exception( "Invalid image!" );
		return CreateTexture2DArray( _device, gcnew array< ImageFile^ >( 1 ) { _image }, _mipLevelsCount, _componentFormat );
	}
	Texture2D^	Texture2D::CreateTexture2DArray( Device^ _device, array< ImageFile^ >^ _images, UInt32 _mipLevelsCount, COMPONENT_FORMAT _componentFormat ) {
		if ( _images == nullptr ) throw gcnew Exception( "Invalid array of images!" );
		if ( (_images->Length % 6) != 0 ) throw gcnew Exception( "The length of the provided array of images is not a multiple of 6!" );
		return CreateTexture2DArray_internal( _device, _images, _mipLevelsCount, _componentFormat, false );
	}
	Texture2D^	Texture2D::CreateCubeMapsArray( Device^ _device, array< ImageFile^ >^ _images, UInt32 _mipLevelsCount, COMPONENT_FORMAT _componentFormat ) {
		if ( _images == nullptr ) throw gcnew Exception( "Invalid array of images!" );
		return CreateTexture2DArray_internal( _device, _images, _mipLevelsCount, _componentFormat, true );
	}

	Texture2D^	Texture2D::CreateTexture2DArray_internal( Device^ _device, array< ImageFile^ >^ _images, UInt32 _mipLevelsCount, COMPONENT_FORMAT _componentFormat, bool _isCubeMap ) {
		if ( _images == nullptr ) throw gcnew Exception( "Invalid array of images!" );
		if ( _images->Length == 0 ) throw gcnew Exception( "The length of the provided array of images is 0!" );

		UInt32	arraySize = _images->Length;

		// Ensure all textures have the same size and pixel format
		UInt32					W = _images[0]->Width;
		UInt32					H = _images[0]->Height;
		ImageFile::PIXEL_FORMAT	sourceFormat = _images[0]->PixelFormat;
		ColorProfile^			profile = _images[0]->ColorProfile;
		for ( UInt32 i=1; i < arraySize; i++ ) {
			ImageFile^	image = _images[i];
			if ( image->Width != W ) throw gcnew Exception( "Not all images in the provided array have the same width!" );
			if ( image->Height != H ) throw gcnew Exception( "Not all images in the provided array have the same height!" );
			if ( image->PixelFormat != sourceFormat ) throw gcnew Exception( "Not all images in the provided array have the same pixel format!" );
		}

		// Convert the pixel format into a DXGI format
		bool			sRGB = profile->GammaCurve == ColorProfile::GAMMA_CURVE::sRGB;
		bool			requiresChannelExtension = false;
		PIXEL_FORMAT	targetFormat = ImagePixelFormat2TextureFormat( sourceFormat, _componentFormat, sRGB, requiresChannelExtension );
		if ( targetFormat == PIXEL_FORMAT::UNKNOWN )
			throw gcnew Exception( "Source image format " + sourceFormat.ToString() + " cannot be converted to a valid texture format!" );

		IPixelFormatDescriptor*	descriptor = GetDescriptor( targetFormat );
		UInt32	pixelSize = descriptor->Size();

		// Build the texture's mips
		array<PixelsBuffer^>^	content = gcnew array<PixelsBuffer^>( arraySize * _mipLevelsCount );
		int	i = 0;
		for ( UInt32 arrayIndex=0; arrayIndex < arraySize; arrayIndex++ ) {
			int	currentWidth = W;
			int	currentHeight = H;
			for ( UInt32 mipLevelIndex=0; mipLevelIndex < _mipLevelsCount; mipLevelIndex++ ) {
				PixelsBuffer^	pixels = gcnew PixelsBuffer( currentWidth * currentHeight * pixelSize );

			}
		}

		Texture2D^	result = gcnew Texture2D( _device, W, H, _isCubeMap ? -_images->Length : _images->Length, _mipLevelsCount, targetFormat, false, false, content );

		return result;
	}

	PIXEL_FORMAT	Texture2D::ImagePixelFormat2TextureFormat( ImageUtility::ImageFile::PIXEL_FORMAT _format, COMPONENT_FORMAT _componentFormat, bool _sRGB, bool% _requiresChannelExtension ) {
		_requiresChannelExtension = false;
		if ( _componentFormat == COMPONENT_FORMAT::AUTO )
			_componentFormat = COMPONENT_FORMAT::UNORM;		// Default for integers is "UNORM"

		switch ( _format ) {
		// ---------------------------------------------------------------
		// 8-bits
		case ImageFile::PIXEL_FORMAT::R8:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::UNORM:	return PIXEL_FORMAT::R8_UNORM;
			}
			break;
		case ImageFile::PIXEL_FORMAT::RG8:
			_requiresChannelExtension = true;
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::UNORM:	return PIXEL_FORMAT::RGBA8_UNORM;
			}
			break;
		case ImageFile::PIXEL_FORMAT::RGB8:
			_requiresChannelExtension = true;
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::UNORM:	return PIXEL_FORMAT::RGBA8_UNORM;
			}
			break;
		case ImageFile::PIXEL_FORMAT::RGBA8:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::UNORM:	 return _sRGB ? PIXEL_FORMAT::RGBA8_UNORM_sRGB : PIXEL_FORMAT::RGBA8_UNORM;
			}
			break;

		// ---------------------------------------------------------------
		// 16-bits
		case ImageFile::PIXEL_FORMAT::R16:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::UNORM:	return PIXEL_FORMAT::R16_UNORM;
			}
			break;
// Unsupported
// 		case ImageFile::PIXEL_FORMAT::RG16:
// 			_requiresChannelExtension = true;
// 			switch ( _componentFormat ) {
// 			case COMPONENT_FORMAT::UNORM:	return PIXEL_FORMAT::RG16_UNORM;
// 			}
// 			break;
		case ImageFile::PIXEL_FORMAT::RGB16:
			_requiresChannelExtension = true;
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::UNORM:	return PIXEL_FORMAT::RGBA16_UNORM;
			case COMPONENT_FORMAT::UINT:	return PIXEL_FORMAT::RGBA16_UINT;
			}
			break;
		case ImageFile::PIXEL_FORMAT::RGBA16:
			switch ( _componentFormat ) {
			case COMPONENT_FORMAT::UNORM:	return PIXEL_FORMAT::RGBA16_UNORM;
			case COMPONENT_FORMAT::UINT:	return PIXEL_FORMAT::RGBA16_UINT;
			}
			break;

		// ---------------------------------------------------------------
		// 16-bits half-precision floating points
		case ImageFile::PIXEL_FORMAT::R16F:		return PIXEL_FORMAT::R16_FLOAT;
 		case ImageFile::PIXEL_FORMAT::RG16F:	return PIXEL_FORMAT::RG16_FLOAT;
		case ImageFile::PIXEL_FORMAT::RGB16F:
			_requiresChannelExtension = true;
			return PIXEL_FORMAT::RGBA16_FLOAT;
		case ImageFile::PIXEL_FORMAT::RGBA16F:	return PIXEL_FORMAT::RGBA16_FLOAT;

		// ---------------------------------------------------------------
		// 32-bits
		case ImageFile::PIXEL_FORMAT::R32F:		return PIXEL_FORMAT::R32_FLOAT;
// Unsupported
// 		case ImageFile::PIXEL_FORMAT::RG32F:	return PIXEL_FORMAT::RG32_FLOAT;
		case ImageFile::PIXEL_FORMAT::RGB32F:
			_requiresChannelExtension = true;
			return PIXEL_FORMAT::RGBA32_FLOAT;
		case ImageFile::PIXEL_FORMAT::RGBA32F:	return PIXEL_FORMAT::RGBA32_FLOAT;
		}

		return PIXEL_FORMAT::UNKNOWN;
	}
}
