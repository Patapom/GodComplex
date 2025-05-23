#include "Global.hlsl"

cbuffer CB_Main : register(b0) {
	uint	_texturePOT;
	uint	_textureSize;
	uint	_textureMask;

	float	_kernelFactorSpatial;	// = 1/sigma_i�
	float	_kernelFactorValue;		// = 1/sigma_s�
};

cbuffer CB_Mips : register(b1) {
	uint	_textureMipSource;
	uint	_textureMipTarget;
};

Texture2D<float2>	_texVectorIn : register(t0);
RWTexture2D<float2>	_texVectorOut : register(u0);
Texture2D<float>	_texScoreIn : register(t0);
RWTexture2D<float>	_texScoreOut : register(u0);

StructuredBuffer<uint4>	_SBMutations : register(t1);

// Half-Size of the kernel used to sample surrounding a pixel
static const int	KERNEL_HALF_SIZE = 16;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Mutate the initial distribution into a new, slightly different one
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 16, 16, 1 )]
void	CS__Copy( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	_texVectorOut[_dispatchThreadID.xy] = _texVectorIn[_dispatchThreadID.xy];
}

[numthreads( 1, 1, 1 )]
void	CS__Mutate( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint4	pixelIndices = _SBMutations[_groupID.x];
	uint2	pixelIndexA = pixelIndices.xy;
	uint2	pixelIndexB = pixelIndices.zw;

	uint	swapFlags = pixelIndexB.y >> 30;
	pixelIndexB.y &= ~0xC0000000U;

	float2	pixelA = _texVectorIn[pixelIndexA];
	float2	pixelB = _texVectorIn[pixelIndexB];

	switch ( swapFlags ) {
		case 0x1U: {
				// Only swap Y
				float	temp = pixelA.x;
				pixelA.x = pixelB.x;
				pixelB.x = temp;
				break;
			}
		case 0x2U: {
				// Only swap X
				float	temp = pixelA.y;
				pixelA.y = pixelB.y;
				pixelB.y = temp;
				break;
			}
	}

	_texVectorOut[pixelIndexB] = pixelA;
	_texVectorOut[pixelIndexA] = pixelB;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes the energy score for a 2D texture of 1D vectors
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
[numthreads( 16, 16, 1 )]
void	CS__ComputeScore1D( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	int2	pixelIndex = _dispatchThreadID.xy;

	// Sample central value
	float	centralValue = _texVectorIn[pixelIndex].x;

	// Compute energy as indicated in the paper:
	//	E(M) = Sum[ Exp[ -|Pi - Qi|� / Sigma_i� -|Ps - Qs|^(d/2) / Sigma_s� ] ]
	//
	// For all pairs of pixels Pi != Qi (although we're only restraining to a 17x17 kernel surrounding our current pixel, assuming the score is too low to be significant otherwise)
	//
	float	score = 0.0;
	[loop]
	for ( int dY=-KERNEL_HALF_SIZE; dY <= KERNEL_HALF_SIZE; dY++ ) {
		float	sqDy = dY * dY;
		uint	finalY = uint( pixelIndex.y + dY ) & _textureMask;

		[loop]
		for ( int dX=-KERNEL_HALF_SIZE; dX <= KERNEL_HALF_SIZE; dX++ ) {
			if ( dY == 0 && dX == 0 )
				continue;	// Not interested in ourselves

			float	sqDx = dX * dX;
			uint	finalX = uint( pixelIndex.x + dX ) & _textureMask;

			// Compute -|Pi - Qi|� / Sigma_i�
			float	sqDistanceImage = sqDx + sqDy;
					sqDistanceImage *= _kernelFactorSpatial;

			// Compute -|Ps - Qs|^(d/2) / Sigma_s�
			float	value = _texVectorIn[uint2( finalX, finalY )].x;
			float	sqDistanceValue = sqrt( abs( value - centralValue ) );	// 1D value => d/2 = 0.5
					sqDistanceValue *= _kernelFactorValue;

			// Compute score as Exp[ -|Pi - Qi|� / Sigma_i� -|Ps - Qs|^(d/2) / Sigma_s� ]
			score += exp( sqDistanceImage + sqDistanceValue );
		}
	}

	_texScoreOut[pixelIndex] = score;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes the energy score for a 2D texture of 2D vectors
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
[numthreads( 16, 16, 1 )]
void	CS__ComputeScore2D( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	int2	pixelIndex = _dispatchThreadID.xy;

	// Sample central value
	float2	centralValue = _texVectorIn[pixelIndex];

	// Compute energy as indicated in the paper:
	//	E(M) = Sum[ Exp[ -|Pi - Qi|� / Sigma_i� -|Ps - Qs|^(d/2) / Sigma_s� ] ]
	//
	// For all pairs of pixels Pi != Qi (although we're only restraining to a 17x17 kernel surrounding our current pixel, assuming the score is too low to be significant otherwise)
	//
	float	score = 0.0;
	[loop]
	for ( int dY=-KERNEL_HALF_SIZE; dY <= KERNEL_HALF_SIZE; dY++ ) {
		float	sqDy = dY * dY;
		uint	finalY = uint( pixelIndex.y + dY ) & _textureMask;

		[loop]
		for ( int dX=-KERNEL_HALF_SIZE; dX <= KERNEL_HALF_SIZE; dX++ ) {
			if ( dY == 0 && dX == 0 )
				continue;	// Not interested in ourselves

			float	sqDx = dX * dX;
			uint	finalX = uint( pixelIndex.x + dX ) & _textureMask;

			// Compute -|Pi - Qi|� / Sigma_i�
			float	sqDistanceImage = sqDx + sqDy;
					sqDistanceImage *= _kernelFactorSpatial;

			// Compute -|Ps - Qs|^(d/2) / Sigma_s�
			float2	value = _texVectorIn[uint2( finalX, finalY )];
					value -= centralValue;
			float	sqDistanceValue = abs(value.x) + abs(value.y);		// 2D value => d/2 = 1
					sqDistanceValue *= _kernelFactorValue;

			// Compute score as Exp[ -|Pi - Qi|� / Sigma_i� -|Ps - Qs|^(d/2) / Sigma_s� ]
			score += exp( sqDistanceImage + sqDistanceValue );
		}
	}

	_texScoreOut[pixelIndex] = score;
}

// Factorized source texture loading as it seems to pose problems on my 2 different machines:
//	� On my GTX 680, the Load() instruction and [] operator are working normally
//	� On my GTX 980M, only the SampleLevel() is working otherwise only a single thread is returning something, the others are returning 0 from a Load() or []!
//
float	LoadSource( uint2 _position ) {
	return _texScoreIn.Load( int3( _position, _textureMipSource ) );
//	return _texScoreIn[_position];
//	return _texScoreIn.SampleLevel( PointClamp, float2( _position / _textureSize ), 0.0 );
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Accumulates the scores from a 16x16 to a 1x1 target
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
groupshared float	gs_scores[8*8];

[numthreads( 8, 8, 1 )]
void	CS__AccumulateScore16( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _dispatchThreadID.xy << 1U;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float4	rawScore;
	rawScore.x = LoadSource( position00 );
	rawScore.y = LoadSource( position01 );
	rawScore.z = LoadSource( position11 );
	rawScore.w = LoadSource( position10 );

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
	gs_scores[threadIndex] = rawScore.x + rawScore.y + rawScore.z + rawScore.w;

//uint3	Size;
//_texScoreIn.GetDimensions( 0, Size.x, Size.y, Size.z );
//gs_scores[threadIndex] = Size.z;
//gs_scores[threadIndex] = threadIndex;

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 4x4
	if ( all( threadPos < 4U ) ) {
		// Threads in range [{0,0},{4,4}[ will each span 4 samples in range [{0,0},{8,8}[
		float	score00 = gs_scores[threadIndex00];
		float	score01 = gs_scores[threadIndex01];
		float	score11 = gs_scores[threadIndex11];
		float	score10 = gs_scores[threadIndex10];
		gs_scores[threadIndex] = score00 + score01 + score10 + score11;
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 2x2
	if ( all( threadPos < 2U ) ) {
		// Threads in range [{0,0},{2,2}[ will each span 4 samples in range [{0,0},{4,4}[
		float	score00 = gs_scores[threadIndex00];
		float	score01 = gs_scores[threadIndex01];
		float	score11 = gs_scores[threadIndex11];
		float	score10 = gs_scores[threadIndex10];
		gs_scores[threadIndex] = score00 + score01 + score10 + score11;
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 1x1
	if ( threadIndex == 0 ) {
		// Threads in range [{0,0},{1,1}[ will each span 4 samples in range [{0,0},{2,2}[
		float	score00 = gs_scores[threadIndex00];
		float	score01 = gs_scores[threadIndex01];
		float	score11 = gs_scores[threadIndex11];
		float	score10 = gs_scores[threadIndex10];
		_texScoreOut[_groupID.xy] = score00 + score01 + score10 + score11;

//_texScoreOut[_groupID.xy] = gs_scores[8*7+7];
//_texScoreOut[_groupID.xy] = _texScoreIn[position00 + uint2( 7, 3 )];

	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Accumulates the scores from a 8x8 to a 1x1 target
//
[numthreads( 4, 4, 1 )]
void	CS__AccumulateScore8( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _dispatchThreadID.xy << 1U;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float4	rawScore;
	rawScore.x = LoadSource( position00 );
	rawScore.y = LoadSource( position01 );
	rawScore.z = LoadSource( position11 );
	rawScore.w = LoadSource( position10 );

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
	gs_scores[threadIndex] = rawScore.x + rawScore.y + rawScore.z + rawScore.w;

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 2x2
	if ( all( threadPos < 2U ) ) {
		// Threads in range [{0,0},{2,2}[ will each span 4 samples in range [{0,0},{4,4}[
		float	score00 = gs_scores[threadIndex00];
		float	score01 = gs_scores[threadIndex01];
		float	score11 = gs_scores[threadIndex11];
		float	score10 = gs_scores[threadIndex10];
		gs_scores[threadIndex] = score00 + score01 + score10 + score11;
	}

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 1x1
	if ( threadIndex == 0 ) {
		// Threads in range [{0,0},{1,1}[ will each span 4 samples in range [{0,0},{2,2}[
		float	score00 = gs_scores[threadIndex00];
		float	score01 = gs_scores[threadIndex01];
		float	score11 = gs_scores[threadIndex11];
		float	score10 = gs_scores[threadIndex10];
		_texScoreOut[_groupID.xy] = score00 + score01 + score10 + score11;
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Accumulates the scores from a 4x4 to a 1x1 target
//
[numthreads( 2, 2, 1 )]
void	CS__AccumulateScore4( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _dispatchThreadID.xy << 1U;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float4	rawScore;
	rawScore.x = LoadSource( position00 );
	rawScore.y = LoadSource( position01 );
	rawScore.z = LoadSource( position11 );
	rawScore.w = LoadSource( position10 );

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
	gs_scores[threadIndex] = rawScore.x + rawScore.y + rawScore.z + rawScore.w;

	GroupMemoryBarrierWithGroupSync();	// We wait until all samples from the group are available

	// ======================================================================
	// Downsample to 1x1
	if ( threadIndex == 0 ) {
		// Threads in range [{0,0},{1,1}[ will each span 4 samples in range [{0,0},{2,2}[
		float	score00 = gs_scores[threadIndex00];
		float	score01 = gs_scores[threadIndex01];
		float	score11 = gs_scores[threadIndex11];
		float	score10 = gs_scores[threadIndex10];
		_texScoreOut[_groupID.xy] = score00 + score01 + score10 + score11;
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Accumulates the scores from a 2x2 to a 1x1 target
//
[numthreads( 1, 1, 1 )]
void	CS__AccumulateScore2( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _groupID.xy << 1U;
	uint2	position11 = position00 + 1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float4	rawScore;
	rawScore.x = LoadSource( position00 );
	rawScore.y = LoadSource( position01 );
	rawScore.z = LoadSource( position11 );
	rawScore.w = LoadSource( position10 );

	_texScoreOut[_groupID.xy] = rawScore.x + rawScore.y + rawScore.z + rawScore.w;
}
