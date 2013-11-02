// DirectXTexManaged.h

#pragma once

#pragma unmanaged
#include "DirectXTex.h"
#pragma managed

using namespace System;
using namespace WMath;

namespace DirectXTexManaged {

	public ref class CubeMapCreator
	{
	public:

		static void	CreateCubeMapFile( String^ _FileName, int _CubeSize, cli::array< cli::array< cli::array<WMath::Vector4D^,2>^>^ >^ _CubeFaces )
		{
			int	MipsCount = _CubeFaces->Length;



// 			{
// 				DirectX::ScratchImage	Scratch;
// 				DirectX::TexMetadata	Meta;
// 				DirectX::LoadFromDDSFile( L"Test.dds", 0, &Meta, Scratch );
// 			}






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

							*pScanline++ = R;
							*pScanline++ = G;
							*pScanline++ = B;
							*pScanline++ = 1;
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
	};
}
