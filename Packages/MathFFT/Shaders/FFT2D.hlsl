////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 2D FFT
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#include "FFT.hlsl"

Texture2D<float2>	_texIn : register(t0);
RWTexture2D<float2>	_texOut : register(u0);

//#define	SYNC	GroupMemoryBarrier();	// We only need group barriers because groups don't interfere with each other
#define	SYNC	GroupMemoryBarrierWithGroupSync();

groupshared float2	gs_temp[16*16];

// Fetches and mixes inputs for a specific group shift
// We always have groups of size 1, 2, 4, 8, etc. that are classified into [Even] and (Odd) couples:
//
//	Stage 0 =>	[.](.) [.](.) [.](.) [.](.) [.](.) [.](.) [.](.) [.](.)		size 1
//	Stage 1 =>	 [..]   (..)   [..]   (..)   [..]   (..)   [..]   (..)		size 2
//	Stage 2 =>	    [....]       (....)         [....]        (....)		size 4
//	Stage 3 =>	        [........]   	             (........)				size 8
//
// Each thread fetches even and odd groups' values, mix them using the "twiddle factors" and stores the result:
//			
//				[E]		(O)
//				 |		 |
//				 |\     /|
//				 |  \ /  |
//				 |   X   |
//				 |  / \  |
//				 |/     \|
//				 |		 |
//			   (X_k)  (X_(k+N/2))
//
//	X_k = E_k + e^(-i * 2PI * k / N) * O_k
//	X_(k+N/2) = E_k - e^(-i * 2PI * k / N) * O_k
//
void	Twiddle( float2 _sinCos, inout float2 _V0, inout float2 _V1 ) {
	float2	E = _V0;
	float2	O = _V1;
	float	s = _sinCos.x;
	float	c = _sinCos.y;

	_V0 = float2(	E.x + c * O.x - s * O.y, 
					E.y + s * O.x + c * O.y );
	_V1 = float2(	E.x - c * O.x + s * O.y, 
					E.y - s * O.x - c * O.y );
}

float2	GetTemp( uint _X, uint _Y )					{ return gs_temp[(_Y << 4) + _X]; }
void	SetTemp( uint _X, uint _Y, float2 value )	{ gs_temp[(_Y << 4) + _X] = value; }

void	FetchAndMix( uint _groupShift, uint2 _groupThreadIndex, float _frequency ) {
	uint	groupSize = 1U << _groupShift;
	uint2	elementIndex = _groupThreadIndex & (groupSize-1U);
	uint2	groupOffset = ((_groupThreadIndex - elementIndex) << 1U)
						+ elementIndex;

	float2	scX, scY;
	sincos( elementIndex.x * _frequency, scX.x, scX.y );
	sincos( elementIndex.y * _frequency, scY.x, scY.y );

	float2	V00 = GetTemp( groupOffset.x, groupOffset.y );
	float2	V01 = GetTemp( groupOffset.x + groupSize, groupOffset.y );
	float2	V10 = GetTemp( groupOffset.x, groupOffset.y + groupSize );
	float2	V11 = GetTemp( groupOffset.x + groupSize, groupOffset.y + groupSize );

	Twiddle( scX, V00, V01 );
	Twiddle( scX, V10, V11 );
	Twiddle( scY, V00, V10 );
	Twiddle( scY, V01, V11 );

	SetTemp( groupOffset.x, groupOffset.y, V00 );
	SetTemp( groupOffset.x + groupSize, groupOffset.y, V01 );
	SetTemp( groupOffset.x, groupOffset.y + groupSize, V10 );
	SetTemp( groupOffset.x + groupSize, groupOffset.y + groupSize, V11 );
}

// Applies FFT from stage 0 (group size 1x1) to stage 4 (group size 16x16)
// NOTE: Each thread reads and writes 4 values so each thread at stage 0 reads 4 size-1 groups and writes an entire size-2 group by itself
//			while at stage 3, each thread group will read 4*8 values and write them as a single final size-16 group.
[numthreads( 8, 8, 1 )]
void	CS__1to64( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint2	index = _groupThreadID.xy;

	// Fetch level 0 - Group size = 1x1->2x2 - Frequency = 2PI/2
	uint4	dispatchThreadIndexShifted = (_dispatchThreadID.xyxy << 1) + uint4( 0, 0, 1, 1 );
	uint4	reversedBits = uint4( ReverseBits( dispatchThreadIndexShifted.x ), ReverseBits( dispatchThreadIndexShifted.y ), ReverseBits( dispatchThreadIndexShifted.z ), ReverseBits( dispatchThreadIndexShifted.w ) );

	uint2	groupThreadIndexShifted = index << 1;
	float2	V00 = _texIn[reversedBits.xy];	// 2X+0, 2Y+0
	float2	V01 = _texIn[reversedBits.zy];	// 2X+1, 2Y+0
	float2	V10 = _texIn[reversedBits.xw];	// 2X+0, 2Y+1
	float2	V11 = _texIn[reversedBits.zw];	// 2X+1, 2Y+1

	Twiddle( float2( 0, 1 ), V00, V01 );
	Twiddle( float2( 0, 1 ), V10, V11 );
	Twiddle( float2( 0, 1 ), V00, V10 );
	Twiddle( float2( 0, 1 ), V01, V11 );

	SetTemp( groupThreadIndexShifted.x + 0, groupThreadIndexShifted.y + 0, V00 );
	SetTemp( groupThreadIndexShifted.x + 1, groupThreadIndexShifted.y + 0, V01 );
	SetTemp( groupThreadIndexShifted.x + 0, groupThreadIndexShifted.y + 1, V10 );
	SetTemp( groupThreadIndexShifted.x + 1, groupThreadIndexShifted.y + 1, V11 );
	SYNC

	// Fetch level 1 - Group size = 2x2->4x4 - Frequency = 2PI/4
	float	frequency = _sign * (0.5 * PI);
	FetchAndMix( 1, index, frequency );
	SYNC

	// Fetch level 2 - Group size = 4x4->8x8 - Frequency = 2PI/8
	frequency *= 0.5;
	FetchAndMix( 2, index, frequency );
	SYNC

	// Fetch level 3 - Group size = 8x8->16x16 - Frequency = 2PI/16
	frequency *= 0.5;
	FetchAndMix( 3, index, frequency );
	SYNC

//	// Fetch level 4 - Group size = 2*16->1*32 - Frequency = 2PI/32
//	frequency *= 0.5;
//	FetchAndMix( 4, index, frequency );
//	SYNC
//	
//	// Fetch level 5 - Group size = 2*32->1*64 - Frequency = 2PI/64
//	frequency *= 0.5;
//	FetchAndMix( 5, index, frequency );
//	SYNC
//	
//	// Fetch level 6 - Group size = 2*64->1*128 - Frequency = 2PI/128
//	frequency *= 0.5;
//	FetchAndMix( 6, index, frequency );
//	SYNC
//	
//	// Fetch level 7 - Group size = 2*128->1*256 - Frequency = 2PI/256
//	frequency *= 0.5;
//	FetchAndMix( 7, index, frequency );
//	SYNC
	
	V00 = GetTemp( groupThreadIndexShifted.x + 0, groupThreadIndexShifted.y + 0 );
	V01 = GetTemp( groupThreadIndexShifted.x + 1, groupThreadIndexShifted.y + 0 );
	V10 = GetTemp( groupThreadIndexShifted.x + 0, groupThreadIndexShifted.y + 1 );
	V11 = GetTemp( groupThreadIndexShifted.x + 1, groupThreadIndexShifted.y + 1 );

	float	factor = _normalizationFirstPass;

	_texOut[dispatchThreadIndexShifted.xy] = factor * V00;	// 2X+0, 2Y+0
	_texOut[dispatchThreadIndexShifted.zy] = factor * V01;	// 2X+1, 2Y+0
	_texOut[dispatchThreadIndexShifted.xw] = factor * V10;	// 2X+0, 2Y+1
	_texOut[dispatchThreadIndexShifted.zw] = factor * V11;	// 2X+1, 2Y+1
}
