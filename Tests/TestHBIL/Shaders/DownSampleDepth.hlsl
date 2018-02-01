////////////////////////////////////////////////////////////////////////////////
// Simple depth downsampling to mips
////////////////////////////////////////////////////////////////////////////////
//
#define THREADS_X	16
#define THREADS_Y	16

Texture2D< float >		_tex_sourceDepth : register(t0);
RWTexture2D< float >	_tex_targetDepth : register(u0);

cbuffer CB_DownSample : register( b3 ) {
	uint2	_targetSize;
};

[numthreads( THREADS_X, THREADS_Y, 1 )]
void	CS( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID, uint3 _dispatchThreadID : SV_dispatchThreadID ) {
	uint2	targetPixelIndex = _dispatchThreadID.xy;
	if ( any( targetPixelIndex >= _targetSize ) )
		return;

	uint2	sourcePixelIndex = targetPixelIndex << 1;
	float	V00 = _tex_sourceDepth[sourcePixelIndex];	sourcePixelIndex.x++;
	float	V10 = _tex_sourceDepth[sourcePixelIndex];	sourcePixelIndex.y++;
	float	V11 = _tex_sourceDepth[sourcePixelIndex];	sourcePixelIndex.x--;
	float	V01 = _tex_sourceDepth[sourcePixelIndex];	sourcePixelIndex.y--;

	_tex_targetDepth[targetPixelIndex] = 0.25 * (V00 + V10 + V01 + V11);
}
