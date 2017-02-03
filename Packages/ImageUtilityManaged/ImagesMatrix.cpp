// This is the main DLL file.

#include "stdafx.h"

#include "ImagesMatrix.h"

namespace ImageUtility {

	//////////////////////////////////////////////////////////////////////////
	// Mips Class
// 	void	ImagesMatrix::Mips::BuildMips( ImageUtility::ImageFile^ _mip0 ) {
// 		m_images[0] = _mip0;
// 		if ( m_images->Length == 1 )
// 			return;	// Nothing to build...
// 
// 		UInt32					mipLevelsCount = m_images->Length;
// 		UInt32					W = _mip0->Width;
// 		UInt32					H = _mip0->Height;
// 		ImageFile::PIXEL_FORMAT	format = _mip0->PixelFormat;
// 		ImageFile^				currentMip = _mip0;
// 		ColorProfile^			profile = _mip0->ColorProfile;
// 
// 		UInt32	W2 = (W+1) & ~1U;	// Ensure an even number of pixels
// 		array< float4 >^	scanline0 = gcnew array<float4>( W2 );
// 		array< float4 >^	scanline1 = gcnew array<float4>( W2 );
// 		array< float4 >^	scanlineMip = gcnew array<float4>( W2 );
// 		float4	V00, V01, V10, V11, V;
// 
// 		for ( UInt32 mipLevelIndex=1; mipLevelIndex < mipLevelsCount; mipLevelIndex++ ) {
// 			UInt32	prevW = W;
// 			UInt32	prevH = H;
// 			NextMipSize( W, H );
// 
// 			ImageFile^	prevMip = currentMip;
// 			currentMip = gcnew ImageFile( W, H, format, profile );
// 
// 			for ( UInt32 Y=0; Y < H; Y++ ) {
// 				prevMip->ReadScanline( 2*Y+0, scanline0 );
// 				prevMip->ReadScanline( MIN( prevH-1, 2*Y+1 ), scanline1 );	// Duplicate end scanline if odd height
// 				if ( prevW & 1 ) {
// 					// Duplicate end pixels if odd width
// 					scanline0[prevW] = scanline0[prevW-1];
// 					scanline1[prevW] = scanline1[prevW-1];
// 				}
// 
// 				UInt32	prevX = 0;
// 				for ( UInt32 X=0; X < W; X++ ) {
// 					// Read 4 pixel values in linear space
// 					profile->GammaRGB2LinearRGB( scanline0[prevX], V00 );
// 					profile->GammaRGB2LinearRGB( scanline1[prevX], V10 );
// 					prevX++;
// 					profile->GammaRGB2LinearRGB( scanline0[prevX], V01 );
// 					profile->GammaRGB2LinearRGB( scanline1[prevX], V11 );
// 
// 					// Average and convert back to gamma space
// 					V = 0.25f * (V00 + V01 + V10 + V11);
// 					profile->LinearRGB2GammaRGB( scanlineMip[X], V );
// 				}
// 
// 				currentMip->WriteScanline( Y, scanlineMip );
// 			}
// 		}
// 
// 		delete scanlineMip;
// 		delete scanline1;
// 		delete scanline0;
// 	}
}
