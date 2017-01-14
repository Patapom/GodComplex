#include "Global.hlsl"

Texture2D<float>	_texIn : register(t0);
RWTexture2D<float>	_texOut : register(u0);


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Mutate the initial distribution into a new, slightly different one
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
[numthreads( 16, 16, 1 )]
void	CS__Copy( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	_texOut[_dispatchThreadID.xy] = _texIn[_dispatchThreadID.xy];
}

[numthreads( 1, 1, 1 )]
void	CS__Mutate( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	pixelIndexA = uint2( _pixelSourceX[_groupID.x], _pixelSourceY[_groupID.x] );
	uint2	pixelIndexB = uint2( _pixelTargetX[_groupID.x], _pixelTargetY[_groupID.x] );

	float	pixelA = _texIn[pixelIndexA];
	float	pixelB = _texIn[pixelIndexB];
	_texOut[pixelIndexB] = pixelA;
	_texOut[pixelIndexA] = pixelB;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Computes the energy score for a 2D texture of 1D vectors
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
[numthreads( 16, 16, 1 )]
void	CS__ComputeScore1D( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	int2	pixelIndex = _dispatchThreadID.xy;

	// Sample central value
	float	centralValue = _texIn[pixelIndex];

	// Compute energy as indicated in the paper:
	//	E(M) = Sum[ Exp[ -|Pi - Qi|² / Sigma_i² -|Ps - Qs|^(d/2) / Sigma_s² ] ]
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

			// Compute -|Pi - Qi|² / Sigma_i²
			float	sqDistanceImage = sqDx + sqDy;
					sqDistanceImage *= _kernelFactorSpatial;

			// Compute -|Ps - Qs|^(d/2) / Sigma_s²
			float	value = _texIn[uint2( finalX, finalY )];
//			float	sqDistanceValue = value - centralValue;					// 2D value => d/2 = 1
			float	sqDistanceValue = sqrt( abs( value - centralValue ) );	// 1D value => d/2 = 0.5
					sqDistanceValue *= _kernelFactorValue;

			// Compute score as Exp[ -|Pi - Qi|² / Sigma_i² -|Ps - Qs|^(d/2) / Sigma_s² ]
			score += exp( sqDistanceImage + sqDistanceValue );
		}
	}

score = 1.0;//pixelIndex.y / 256.0;
	_texOut[pixelIndex] = score;
}

//		uint3	UVW = uint3( iPhi, iTheta, scatteringOrder );
//		InterlockedAdd( _Tex_DirectionsHistogram_Decimal[UVW], value, oldValue );						// Decimal point addition
//		value += oldValue;																				// Perform local addition to see if we need to add carry to integers accumulator
//		InterlockedAdd( _Tex_DirectionsHistogram_Integer[UVW], value < oldValue ? 1 : 0, oldValue );	// Integer addition with carry


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Accumulates the scores from a 16x16 to a 1x1 target
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
#define	NUMTHREADX	8
#define	NUMTHREADY	8

groupshared float	gs_scores[NUMTHREADX*NUMTHREADY];

[numthreads( NUMTHREADX, NUMTHREADY, 1 )]
void	CS__AccumulateScore16( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	position00 = _dispatchThreadID.xy << 1U;
	uint2	position11 = position00+1U;
	uint2	position01 = uint2( position11.x, position00.y );
	uint2	position10 = uint2( position00.x, position11.y );

	float4	rawScore;
	rawScore.x = _texIn.Load( int3( position00, 0 ) );
	rawScore.y = _texIn.Load( int3( position01, 0 ) );
	rawScore.z = _texIn.Load( int3( position11, 0 ) );
	rawScore.w = _texIn.Load( int3( position10, 0 ) );

	float	sum = rawScore.x + rawScore.y + rawScore.z + rawScore.w;

	// ======================================================================
	// Build thread indices so each thread addresses groups of 2x2 pixels in shared memory
	uint2	threadPos = _groupThreadID.xy;				// Thread position within group
	uint	threadIndex = NUMTHREADY * threadPos.y + threadPos.x;

	uint	threadIndex00 = threadIndex << 1;
	uint	threadIndex01 = threadIndex00 + 1;			// Right pixel
	uint	threadIndex11 = threadIndex01 + NUMTHREADX;	// Bottom-Right pixel
	uint	threadIndex10 = threadIndex11 - 1;			// Bottom pixel

	// ======================================================================
	// Store initial values
	{
		// Threads in range [{0,0},{7,7}[ will each span 4 samples in range [{0,0},{16,16}[
		uint2	targetPos = 8U * _groupID.xy + threadPos;
		gs_scores[threadIndex] = sum;
	}

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
		_texOut[_groupID.xy] = score00 + score01 + score10 + score11;

_texOut[_groupID.xy] = 3456.0;
//_texOut[_groupID.xy] = 1234.0;
_texOut[_groupID.xy] = _groupID.y;
	}
}
