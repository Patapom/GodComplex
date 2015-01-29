// This is the main DLL file.

#include "stdafx.h"

#include "DirectXTexManaged.h"

#include "../../RendererD3D11/Components/Texture2D.h"
#include "../../RendererD3D11/Components/Texture3D.h"

namespace DirectXTexManaged {

	void TextureCreator::CreateDDS( String^ _FileName, RendererManaged::Texture2D^ _Texture ) {

		::Texture2D&	Texture = *((::Texture2D*) _Texture->GetWrappedtexture());

		int	W =  Texture.GetWidth();
		int	H = Texture.GetHeight();
		int	A = Texture.GetArraySize();
		int	MipsCount = Texture.GetMipLevelsCount();

		const ::IPixelFormatDescriptor&	Descriptor = static_cast< const ::IPixelFormatDescriptor& >( Texture.GetFormatDescriptor() );

		// Copy to staging image
		::Texture2D*	TextureStaging = new ::Texture2D( Texture.GetDevice(), W, H, A, Descriptor, MipsCount, nullptr, true, false );
		TextureStaging->CopyFrom( Texture );

		// Build DTex scratch image
		DirectX::ScratchImage*	DXT = new DirectX::ScratchImage();

		DXGI_FORMAT	DXFormat = Descriptor.DirectXFormat();
		HRESULT	hr = DXT->Initialize2D( DXFormat, W, H, A, MipsCount );

		// Copy staging to scratch
		for ( int MipLevel=0; MipLevel < MipsCount; MipLevel++ ) {
			for ( int ArrayIndex=0; ArrayIndex < A; ArrayIndex++ ) {
				D3D11_MAPPED_SUBRESOURCE	SourceData = TextureStaging->Map( MipLevel, ArrayIndex );
				const uint8_t*				pSourceBuffer = (uint8_t*) SourceData.pData;
				const DirectX::Image*		pTarget = DXT->GetImage( MipLevel, ArrayIndex, 0 );
				ASSERT( pTarget->rowPitch == SourceData.RowPitch, "Row pitches mismatch!" );

				for ( int Y=0; Y < H; Y++ ) {
					const void*	pSourceScanline = pSourceBuffer + Y * SourceData.RowPitch;
					void*		pTargetScanline = pTarget->pixels + Y * pTarget->rowPitch;
					memcpy_s( pTargetScanline, pTarget->rowPitch, pSourceScanline, SourceData.RowPitch );
				}
			}
		} 

		delete TextureStaging;


		// Get array of images
		size_t						ImagesCount = DXT->GetImageCount();
		const DirectX::Image*		pImages = DXT->GetImages();
		const DirectX::TexMetadata&	Meta = DXT->GetMetadata();

		// Save the result
		System::IntPtr	pFileName = System::Runtime::InteropServices::Marshal::StringToHGlobalUni( _FileName );
		LPCWSTR			wpFileName = LPCWSTR( pFileName.ToPointer() );

		DWORD	flags = DirectX::DDS_FLAGS_FORCE_DX10_EXT;
		hr = DirectX::SaveToDDSFile( pImages, ImagesCount, Meta, flags, wpFileName );

		delete DXT;
	} 

}