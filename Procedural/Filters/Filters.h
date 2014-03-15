//////////////////////////////////////////////////////////////////////////
// Filters methods
//
#pragma once

class	Filters
{
public:		// METHODS

	// _MinWeight is the value the gaussian weight will take farthest away from the kernel center
	static void	BlurGaussian( TextureBuilder& _Builder, float _SizeX, float _SizeY, bool _bWrap=true, float _MinWeight=0.05f );

	static void	UnsharpMask( TextureBuilder& _Builder, float _Size );

	static void	BrightnessContrastGamma( TextureBuilder& _Builder, float _Brightness=0.0f, float _Contrast=0.0f, float _Gamma=1.0f );

	static void	Emboss( TextureBuilder& _Builder, const float2& _Direction, float _Amplitude=1.0f );

	static void	Erode( TextureBuilder& _Builder, int _KernelSize=4 );

	static void	Dilate( TextureBuilder& _Builder, int _KernelSize=4 );
};
