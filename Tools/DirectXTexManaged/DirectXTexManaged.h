// DirectXTexManaged.h

#pragma once

#pragma unmanaged
#include "DirectXTex.h"
#pragma managed

using namespace System;
using namespace WMath;

namespace DirectXTexManaged {

	public ref class TextureCreator
	{
	public:

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
	};
}
