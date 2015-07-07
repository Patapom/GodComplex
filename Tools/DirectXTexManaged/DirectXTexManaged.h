// DirectXTexManaged.h

#pragma once

#pragma unmanaged
#include "DirectXTex.h"
#pragma managed

#include "../../RendererD3D11/Device.h"

using namespace System;
using namespace WMath;

namespace DirectXTexManaged {

	public ref class TextureCreator
	{
	public:

		// Creates a DDS file from a texture
		static void CreateDDS( String^ _FileName, RendererManaged::Texture2D^ _Texture );

// 		static void	CreateNormalMapBC5File( String^ _FileName, cli::array< cli::array<WMath::Vector4D^,2>^>^ _Mips )
// 		{
// 			int	MipsCount = _Mips->Length;
// 
// 			int	Width = _Mips[0]->GetLength( 0 );
// 			int	Height = _Mips[0]->GetLength( 1 );
// 
// 			// Create the image and fill it with our data
// 			DirectX::ScratchImage*	DXT = new DirectX::ScratchImage();
// 
// 			HRESULT	hr = DXT->Initialize2D( DXGI_FORMAT_R32G32B32A32_FLOAT, Width, Height, 1, MipsCount );
// 
// 			for ( int MipIndex=0; MipIndex < MipsCount; MipIndex++ )
// 			{
// 				int	W = Math::Max( 1, Width >> MipIndex );
// 				int	H = Math::Max( 1, Height >> MipIndex );
// 
// 				cli::array<WMath::Vector4D^,2>^	Content = _Mips[MipIndex];
// 
// 				const DirectX::Image*	pImage = DXT->GetImage( MipIndex, 0, 0 );
// 
// 				for ( int Y=0; Y < H; Y++ )
// 				{
// 					float*	pScanline = (float*) (pImage->pixels + Y * pImage->rowPitch);
// 					for ( int X=0; X < W; X++ )
// 					{
// 						float	R = Content[X,Y]->x;
// 						float	G = Content[X,Y]->y;
// 						float	B = Content[X,Y]->z;
// 						float	A = Content[X,Y]->w;
// 
// 						*pScanline++ = R;
// 						*pScanline++ = G;
// 						*pScanline++ = B;
// 						*pScanline++ = A;
// 					}
// 				}
// 			}
// 
// 			// Get array of images
// 			size_t						ImagesCount = DXT->GetImageCount();
// 			const DirectX::Image*		pImages = DXT->GetImages();
// 			const DirectX::TexMetadata&	Meta = DXT->GetMetadata();
// 
// 			HRESULT	hr = DXT->Initialize2D( DXGI_FORMAT_BC5_SNORM, Width, Height, 1, MipsCount );
// 
// 			// Save the result
// 			System::IntPtr	pFileName = System::Runtime::InteropServices::Marshal::StringToHGlobalUni( _FileName );
// 			LPCWSTR			wpFileName = LPCWSTR( pFileName.ToPointer() );
// 
// //			DWORD	flags = DirectX::DDS_FLAGS_NONE;
// 			DWORD	flags = DirectX::DDS_FLAGS_FORCE_DX10_EXT;
// 			hr = DirectX::SaveToDDSFile( pImages, ImagesCount, Meta, flags, wpFileName );
// 
// 			delete DXT;
// 		}

		static void	CreateCubeMapFile( String^ _FileName, int _CubeSize, cli::array< cli::array< cli::array<WMath::Vector4D^,2>^>^ >^ _CubeFaces )
		{
			int	MipsCount = _CubeFaces->Length;

			// Create the image and fill it with our data
			DirectX::ScratchImage*	DXT = new DirectX::ScratchImage();

			HRESULT	hr = DXT->InitializeCube( DXGI_FORMAT_R32G32B32A32_FLOAT, _CubeSize, _CubeSize, 1, MipsCount );

			for ( int MipIndex=0; MipIndex < _CubeFaces->Length; MipIndex++ )
			{
				int	CubeSize = _CubeSize >> MipIndex;
				cli::array< cli::array<WMath::Vector4D^,2>^>^	CubeFaces = _CubeFaces[MipIndex];
				for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ )
				{
					cli::array<WMath::Vector4D^,2>^	CubeFace = CubeFaces[FaceIndex];
					const DirectX::Image*	pImage = DXT->GetImage( MipIndex, FaceIndex, 0 );

// 					float	R, G, B;
// 					switch ( FaceIndex )
// 					{
// 					case 0: R = 1; G = 0; B = 0; break;
// 					case 1: R = 1; G = 1; B = 0; break;
// 					case 2: R = 0; G = 1; B = 0; break;
// 					case 3: R = 0; G = 1; B = 1; break;
// 					case 4: R = 0; G = 0; B = 1; break;
// 					case 5: R = 1; G = 0; B = 1; break;
// 					}

					for ( int Y=0; Y < CubeSize; Y++ )
					{
						float*	pScanline = (float*) (pImage->pixels + Y * pImage->rowPitch);
						for ( int X=0; X < CubeSize; X++ )
						{
							float	R = CubeFace[X,Y]->x;
							float	G = CubeFace[X,Y]->y;
							float	B = CubeFace[X,Y]->z;
							float	A = CubeFace[X,Y]->w;

							*pScanline++ = R;
							*pScanline++ = G;
							*pScanline++ = B;
							*pScanline++ = A;
						}
					}
				}
			}

			// Get array of images
			size_t						ImagesCount = DXT->GetImageCount();
			const DirectX::Image*		pImages = DXT->GetImages();
			const DirectX::TexMetadata&	Meta = DXT->GetMetadata();

			// Save the result
			System::IntPtr	pFileName = System::Runtime::InteropServices::Marshal::StringToHGlobalUni( _FileName );
			LPCWSTR			wpFileName = LPCWSTR( pFileName.ToPointer() );

//			DWORD	flags = DirectX::DDS_FLAGS_NONE;
			DWORD	flags = DirectX::DDS_FLAGS_FORCE_DX10_EXT;
			hr = DirectX::SaveToDDSFile( pImages, ImagesCount, Meta, flags, wpFileName );

			delete DXT;
		}

		static void	CreateRGBA16FFile( String^ _FileName, cli::array<WMath::Vector4D^,2>^ _Image )
		{
			int	Width = _Image->GetLength( 0 );
			int	Height = _Image->GetLength( 1 );

			// Create the image and fill it with our data
			DirectX::ScratchImage*	TempDXT = new DirectX::ScratchImage();
			HRESULT					hr = TempDXT->Initialize2D( DXGI_FORMAT_R32G32B32A32_FLOAT, Width, Height, 1, 1 );
			const DirectX::Image*	pTempImage = TempDXT->GetImage( 0, 0, 0 );

			for ( int Y=0; Y < Height; Y++ )
			{
				float*	pScanline = (float*) (pTempImage->pixels + Y * pTempImage->rowPitch);
				for ( int X=0; X < Width; X++ )
				{
					float	R = _Image[X,Y]->x;
					float	G = _Image[X,Y]->y;
					float	B = _Image[X,Y]->z;
					float	A = _Image[X,Y]->w;

					*pScanline++ = R;
					*pScanline++ = G;
					*pScanline++ = B;
					*pScanline++ = A;
				}
			}

			// Convert to our format
			DirectX::ScratchImage*	DXT = new DirectX::ScratchImage();
			DirectX::Convert( *pTempImage, DXGI_FORMAT_R16G16B16A16_FLOAT, DirectX::TEX_FILTER_DEFAULT, 0.0f, *DXT );
			delete TempDXT;

			// Get array of images
			size_t						ImagesCount = DXT->GetImageCount();
			const DirectX::Image*		pImages = DXT->GetImages();
			const DirectX::TexMetadata&	Meta = DXT->GetMetadata();

			// Save the result
			System::IntPtr	pFileName = System::Runtime::InteropServices::Marshal::StringToHGlobalUni( _FileName );
			LPCWSTR			wpFileName = LPCWSTR( pFileName.ToPointer() );

//			DWORD	flags = DirectX::DDS_FLAGS_NONE;
			DWORD	flags = DirectX::DDS_FLAGS_FORCE_DX10_EXT;
			hr = DirectX::SaveToDDSFile( pImages, ImagesCount, Meta, flags, wpFileName );

			delete DXT;
		}

		static RendererManaged::Texture2D^	CreateTexture2DFromDDSFile( RendererManaged::Device^ _Device, String^ _FileName ) {

			// Create the image and fill it with our data
			DirectX::ScratchImage*	DXT = new DirectX::ScratchImage();

			DirectX::TexMetadata	meta;

			// Load into memory
			System::IntPtr	pFileName = System::Runtime::InteropServices::Marshal::StringToHGlobalUni( _FileName );
			LPCWSTR			wpFileName = LPCWSTR( pFileName.ToPointer() );

			DWORD	flags = DirectX::DDS_FLAGS_NONE;
//			DWORD	flags = DirectX::DDS_FLAGS_FORCE_DX10_EXT;
			DirectX::LoadFromDDSFile( wpFileName, flags, &meta, *DXT );

			//////////////////////////////////////////////////////////////////////////
			// Convert into texture

			// Retrieve supported format
			RendererManaged::PIXEL_FORMAT	format = RendererManaged::PIXEL_FORMAT::UNKNOWN;
			int	pixelSize = 0;
			switch ( meta.format ) {
				case DXGI_FORMAT_R8_UNORM: format = RendererManaged::PIXEL_FORMAT::R8_UNORM; pixelSize = 1; break;
				case DXGI_FORMAT_R8G8B8A8_UNORM: format = RendererManaged::PIXEL_FORMAT::RGBA8_UNORM; pixelSize = 4; break;
				case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB: format = RendererManaged::PIXEL_FORMAT::RGBA8_UNORM_sRGB; pixelSize = 4; break;
				case DXGI_FORMAT_R16_FLOAT: format = RendererManaged::PIXEL_FORMAT::R16_FLOAT; pixelSize = 2; break;
				case DXGI_FORMAT_R16_UNORM: format = RendererManaged::PIXEL_FORMAT::R16_UNORM; pixelSize = 2; break;
				case DXGI_FORMAT_R16G16_FLOAT: format = RendererManaged::PIXEL_FORMAT::RG16_FLOAT; pixelSize = 4; break;
				case DXGI_FORMAT_R16G16_UNORM: format = RendererManaged::PIXEL_FORMAT::RG16_UNORM; pixelSize = 4; break;
				case DXGI_FORMAT_R16G16B16A16_FLOAT: format = RendererManaged::PIXEL_FORMAT::RGBA16_FLOAT; pixelSize = 8; break;
				case DXGI_FORMAT_R32_FLOAT: format = RendererManaged::PIXEL_FORMAT::R32_FLOAT; pixelSize = 4; break;
				case DXGI_FORMAT_R32G32_FLOAT: format = RendererManaged::PIXEL_FORMAT::RG32_FLOAT; pixelSize = 8; break;
				case DXGI_FORMAT_R32G32B32A32_FLOAT: format = RendererManaged::PIXEL_FORMAT::RGBA32_FLOAT; pixelSize = 16; break;
				case DXGI_FORMAT_BC3_UNORM: format = RendererManaged::PIXEL_FORMAT::BC3_UNORM; break;
				case DXGI_FORMAT_BC3_UNORM_SRGB: format = RendererManaged::PIXEL_FORMAT::BC3_UNORM_sRGB; break;
			}

			// Build content slices
			if ( DXT->GetImageCount() != meta.arraySize * meta.mipLevels )
				throw gcnew Exception( "Unexpected amount of images!" );

			cli::array< RendererManaged::PixelsBuffer^ >^	content = gcnew cli::array< RendererManaged::PixelsBuffer^ >( meta.arraySize * meta.mipLevels );
			for ( int arrayIndex=0; arrayIndex < int(meta.arraySize); arrayIndex++ ) {
				int	W = meta.width;
				int	H = meta.height;
				for ( int mipIndex=0; mipIndex < int(meta.mipLevels); mipIndex++ ) {
					const DirectX::Image*	sourceImage = DXT->GetImage( mipIndex, arrayIndex, 0U );

					RendererManaged::PixelsBuffer^	buffer = gcnew RendererManaged::PixelsBuffer( sourceImage->slicePitch );
					content[arrayIndex*meta.mipLevels+mipIndex] = buffer;

					cli::array< Byte >^	byteArray = gcnew cli::array< Byte >( sourceImage->slicePitch );
					System::Runtime::InteropServices::Marshal::Copy( (IntPtr) sourceImage->pixels, byteArray, 0, sourceImage->slicePitch );

					System::IO::BinaryWriter^	writer = buffer->OpenStreamWrite();
					writer->Write( byteArray );
					buffer->CloseStream();
				}
			}

			// Build texture
			RendererManaged::Texture2D^	Result = gcnew RendererManaged::Texture2D( _Device, meta.width, meta.height, meta.IsCubemap() ? -int(meta.arraySize) : int(meta.arraySize), meta.mipLevels, format, false, false, content );

			delete DXT;

			return Result;
		}
	};
}
