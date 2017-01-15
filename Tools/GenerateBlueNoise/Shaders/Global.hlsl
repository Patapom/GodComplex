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

cbuffer CB_Mips : register(b1) {
	uint	_textureMipSource;
	uint	_textureMipTarget;
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border
