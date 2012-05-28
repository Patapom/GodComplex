//////////////////////////////////////////////////////////////////////////
// Blur methods
//
#pragma once

class	Blur
{
protected:	// CONSTANTS

protected:	// FIELDS

public:		// METHODS

	// _MinWeight is the value the gaussian weight will take farthest from the kernel center
	static void	BlurGaussian( TextureBuilder& _Builder, float _SizeX, float _SizeY, bool _bWrap=true, float _MinWeight=0.05f );
};
