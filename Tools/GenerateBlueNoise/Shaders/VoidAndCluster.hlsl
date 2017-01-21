#include "Global.hlsl"

cbuffer CB_Main : register(b0) {
	uint	_texturePOT;
	uint	_textureSize;
	uint	_textureMask;
	float	_kernelFactor;		// = -1/(2*sigma²)

	uint2	_randomOffset;
	uint	_iterationIndex;
};

cbuffer CB_Mips : register(b1) {
	uint	_textureMipSource;
	uint	_textureMipTarget;
};

RWTexture2D<uint>	_texBinaryPatternOut : register(u0);
RWTexture2D<float>	_texDitheringArrayOut : register(u1);

Texture2D<float2>	_texFilterIn : register(t0);
RWTexture2D<float2>	_texFilterOut : register(u2);



float	WritePos( uint2 _position ) {
	return asfloat( (_position.y << 16) | _position.x );
}
uint2	ReadPos( float _position ) {
	uint	posXY = asuint( _position );
	return uint2( posXY & 0xFFFF, (posXY >> 16) & 0xFFFF );
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Filters the binary pattern texture to compute a "clustering score"
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
static const int	KERNEL_HALF_SIZE = 16;

[numthreads( 16, 16, 1 )]
void	CS__Filter( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	int2	pixelIndex = _dispatchThreadID.xy;
	float2	score = float2( 1e6, WritePos( pixelIndex ) );	// Default invalid score

	// Sample central value
	uint	centralValue = _texBinaryPatternOut[pixelIndex];
	if ( centralValue != 0 ) {
		// Already filled!
		_texFilterOut[pixelIndex] = score;
		return;
	}

	// Filter neighbors
	score.x = 0.0;
	[loop]
	for ( int dY=-KERNEL_HALF_SIZE; dY <= KERNEL_HALF_SIZE; dY++ ) {
		float	sqDy = dY * dY;
		uint	finalY = uint( _textureSize + pixelIndex.y + dY ) & _textureMask;

		[loop]
		for ( int dX=-KERNEL_HALF_SIZE; dX <= KERNEL_HALF_SIZE; dX++ ) {
			float	sqDx = dX * dX;
			uint	finalX = uint( _textureSize + pixelIndex.x + dX ) & _textureMask;

			score.x += _texBinaryPatternOut[uint2( finalX, finalY )] * exp( _kernelFactor * (sqDx + sqDy) );
		}
	}

//score.x *= 1.0 / (2.0 * 3.14159265358979 * 2.1*2.1);
//score.x *= 4.0;
//score.x = _kernelFactor;

	_texFilterOut[pixelIndex] = score;
}

// Factorized source texture loading as it seems to pose problems on my 2 different machines:
//	• On my GTX 680, the Load() instruction and [] operator are working normally
//	• On my GTX 980M, only the SampleLevel() is working otherwise only a single thread is returning something, the others are returning 0 from a Load() or []!
//
float2	LoadSource( uint2 _position ) {
	return _texFilterIn.Load( int3( _position, _textureMipSource ) );
//	return _texIn[_position];
//	return _texIn.SampleLevel( PointClamp, float2( _position / _textureSize ), 0.0 );
}

float2	KeepBestScore( float2 _score00, float2 _score01, float2 _score10, float2 _score11 ) {
	float2	bestScore = _score00;
	if ( _score01.x < bestScore.x )
		bestScore = _score01;
	if ( _score10.x < bestScore.x )
		bestScore = _score10;
	if ( _score11.x < bestScore.x )
		bestScore = _score11;
	return bestScore;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Finds and isolates the minimum score
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
groupshared float2	gs_scores[8*8];

// Selects the scores from a 16x16 to a 1x1 target
[numthreads( 8, 8, 1 )]
void	CS__DownsampleScore16( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
//	uint2	position00 = _dispatchThreadID.xy << 1U;
	uint2	position00 = ((_dispatchThreadID.xy << 1U) + (_randomOffset & ~1)) & _textureMask;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float2	score00 = LoadSource( position00 );
	float2	score01 = LoadSource( position01 );
	float2	score10 = LoadSource( position10 );
	float2	score11 = LoadSource( position11 );

	// ======================================================================
	// Build thread indices so each thread addresses groups of 2x2 pixels in shared memory
	uint2	threadPos = _groupThreadID.xy;		// Thread position within group
	uint	threadIndex = 8 * threadPos.y + threadPos.x;

	uint	threadIndex00 = threadIndex << 1;
	uint	threadIndex01 = threadIndex00 + 1;	// Right pixel
	uint	threadIndex11 = threadIndex01 + 8;	// Bottom-Right pixel
	uint	threadIndex10 = threadIndex11 - 1;	// Bottom pixel

	// ======================================================================
	// Store initial values
	// Threads in range [{0,0},{8,8}[ will each span 4 samples in range [{0,0},{16,16}[
	gs_scores[threadIndex] = KeepBestScore( score00, score01, score10, score11 );

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 4x4
	if ( all( threadPos < 4U ) ) {
		// Threads in range [{0,0},{4,4}[ will each span 4 samples in range [{0,0},{8,8}[
		score00 = gs_scores[threadIndex00];
		score01 = gs_scores[threadIndex01];
		score11 = gs_scores[threadIndex11];
		score10 = gs_scores[threadIndex10];
		gs_scores[threadIndex] = KeepBestScore( score00, score01, score10, score11 );
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 2x2
	if ( all( threadPos < 2U ) ) {
		// Threads in range [{0,0},{2,2}[ will each span 4 samples in range [{0,0},{4,4}[
		score00 = gs_scores[threadIndex00];
		score01 = gs_scores[threadIndex01];
		score11 = gs_scores[threadIndex11];
		score10 = gs_scores[threadIndex10];
		gs_scores[threadIndex] = KeepBestScore( score00, score01, score10, score11 );
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 1x1
	if ( threadIndex == 0 ) {
		// Threads in range [{0,0},{1,1}[ will each span 4 samples in range [{0,0},{2,2}[
		score00 = gs_scores[threadIndex00];
		score01 = gs_scores[threadIndex01];
		score11 = gs_scores[threadIndex11];
		score10 = gs_scores[threadIndex10];
		_texFilterOut[_groupID.xy] = KeepBestScore( score00, score01, score10, score11 );
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Selects the scores from a 8x8 to a 1x1 target
//
[numthreads( 4, 4, 1 )]
void	CS__DownsampleScore8( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _dispatchThreadID.xy << 1U;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float2	score00 = LoadSource( position00 );
	float2	score01 = LoadSource( position01 );
	float2	score10 = LoadSource( position10 );
	float2	score11 = LoadSource( position11 );

	// ======================================================================
	// Build thread indices so each thread addresses groups of 2x2 pixels in shared memory
	uint2	threadPos = _groupThreadID.xy;		// Thread position within group
	uint	threadIndex = 4 * threadPos.y + threadPos.x;

	uint	threadIndex00 = threadIndex << 1;
	uint	threadIndex01 = threadIndex00 + 1;	// Right pixel
	uint	threadIndex11 = threadIndex01 + 4;	// Bottom-Right pixel
	uint	threadIndex10 = threadIndex11 - 1;	// Bottom pixel

	// ======================================================================
	// Store initial values
	// Threads in range [{0,0},{4,4}[ will each span 4 samples in range [{0,0},{8,8}[
	gs_scores[threadIndex] = KeepBestScore( score00, score01, score10, score11 );

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 2x2
	if ( all( threadPos < 2U ) ) {
		// Threads in range [{0,0},{2,2}[ will each span 4 samples in range [{0,0},{4,4}[
		score00 = gs_scores[threadIndex00];
		score01 = gs_scores[threadIndex01];
		score11 = gs_scores[threadIndex11];
		score10 = gs_scores[threadIndex10];
		gs_scores[threadIndex] = KeepBestScore( score00, score01, score10, score11 );
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 1x1
	if ( threadIndex == 0 ) {
		// Threads in range [{0,0},{1,1}[ will each span 4 samples in range [{0,0},{2,2}[
		score00 = gs_scores[threadIndex00];
		score01 = gs_scores[threadIndex01];
		score11 = gs_scores[threadIndex11];
		score10 = gs_scores[threadIndex10];
		_texFilterOut[_groupID.xy] = KeepBestScore( score00, score01, score10, score11 );
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Selects the scores from a 4x4 to a 1x1 target
//
[numthreads( 2, 2, 1 )]
void	CS__DownsampleScore4( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _dispatchThreadID.xy << 1U;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float2	score00 = LoadSource( position00 );
	float2	score01 = LoadSource( position01 );
	float2	score10 = LoadSource( position10 );
	float2	score11 = LoadSource( position11 );

	// ======================================================================
	// Build thread indices so each thread addresses groups of 2x2 pixels in shared memory
	uint2	threadPos = _groupThreadID.xy;		// Thread position within group
	uint	threadIndex = 2 * threadPos.y + threadPos.x;

	uint	threadIndex00 = threadIndex << 1;
	uint	threadIndex01 = threadIndex00 + 1;	// Right pixel
	uint	threadIndex11 = threadIndex01 + 2;	// Bottom-Right pixel
	uint	threadIndex10 = threadIndex11 - 1;	// Bottom pixel

	// ======================================================================
	// Store initial values
	// Threads in range [{0,0},{2,2}[ will each span 4 samples in range [{0,0},{4,4}[
	gs_scores[threadIndex] = KeepBestScore( score00, score01, score10, score11 );

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 1x1
	if ( threadIndex == 0 ) {
		// Threads in range [{0,0},{1,1}[ will each span 4 samples in range [{0,0},{2,2}[
		score00 = gs_scores[threadIndex00];
		score01 = gs_scores[threadIndex01];
		score11 = gs_scores[threadIndex11];
		score10 = gs_scores[threadIndex10];
		_texFilterOut[_groupID.xy] = KeepBestScore( score00, score01, score10, score11 );


//_texFilterOut[_groupID.xy] = float2( 0.0, WritePos( uint2( _iterationIndex & _textureMask, _iterationIndex >> _texturePOT ) ) );
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Selects the scores from a 2x2 to a 1x1 target
//
[numthreads( 1, 1, 1 )]
void	CS__DownsampleScore2( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _groupID.xy << 1U;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float2	score00 = LoadSource( position00 );
	float2	score01 = LoadSource( position01 );
	float2	score10 = LoadSource( position10 );
	float2	score11 = LoadSource( position11 );

	_texFilterOut[_groupID.xy] = KeepBestScore( score00, score01, score10, score11 );
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Splat a value where we located the maximum
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 1, 1, 1 )]
void	CS__Splat( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	float2	bestScore = LoadSource( 0 );
	uint2	splatPos = ReadPos( bestScore.y );

//splatPos.x = _iterationIndex & _textureMask;
//splatPos.y = _iterationIndex >> _texturePOT;

	if ( any(splatPos >= _textureSize) ) {
//		_texDitheringArrayOut[splatPos & _textureMask] = 10.0;
		return;	// OUCH!!
	}

	_texBinaryPatternOut[splatPos] = 1.0;
	_texDitheringArrayOut[splatPos] = float( _iterationIndex ) / (_textureSize*_textureSize);
//	_texDitheringArrayOut[splatPos] = bestScore.x;
}
