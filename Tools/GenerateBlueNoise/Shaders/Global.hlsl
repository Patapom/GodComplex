////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Common code
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
cbuffer CB_Main : register(b0) {
	uint	_texturePOT;
	uint	_textureSize;
	uint	_textureMask;

	float	_kernelFactorSpatial;	// = 1/sigma_i²
	float	_kernelFactorValue;		// = 1/sigma_s²
};

cbuffer CB_Mutation : register(b1) {
	uint4	_pixelSourceX;
	uint4	_pixelSourceY;
	uint4	_pixelTargetX;
	uint4	_pixelTargetY;
};

// Half-Size of the kernel used to sample surrounding a pixel
static const int	KERNEL_HALF_SIZE = 8;