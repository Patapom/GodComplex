////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 1D FFT
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#include "FFT.hlsl"

Texture2D<float2>	_texIn : register(t0);
RWTexture2D<float2>	_texOut : register(u0);

//#define	SYNC	GroupMemoryBarrier();	// We only need group barriers because groups don't interfere with each other
#define	SYNC	GroupMemoryBarrierWithGroupSync();

groupshared float2	gs_temp[256];

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

void	FetchAndMix( uint _groupShift, uint _groupThreadIndex, float _frequency ) {
	uint	groupSize = 1U << _groupShift;
	uint	elementIndex = _groupThreadIndex & (groupSize-1U);
	uint	groupOffset = ((_groupThreadIndex - elementIndex) << 1U)
						+ elementIndex;

	float2	sc;
	sincos( elementIndex * _frequency, sc.x, sc.y );
	Twiddle( sc, gs_temp[groupOffset], gs_temp[groupOffset + groupSize] );
}

// Applies FFT from stage 0 (size 1) to stage 8 (size 256)
// NOTE: Each thread reads and writes 2 values so each thread at stage 0 reads 2 size-1 groups and writes an entire size-2 group by itself
//			while at stage 7, each thread group will read 2*128 values and write them as a single final size-256 group.
[numthreads( 128, 1, 1 )]
void	CS__1to256( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _groupThreadID.x;

	// Fetch level 0 - Group size = 2*1->1*2 - Frequency = 2PI/2
	gs_temp[2*index+0] = _texIn[uint2( ReverseBits( 2*_dispatchThreadID.x+0 ), 0)];
	gs_temp[2*index+1] = _texIn[uint2( ReverseBits( 2*_dispatchThreadID.x+1 ), 0)];
	Twiddle( float2( 0, 1 ), gs_temp[2*index+0], gs_temp[2*index+1] );
	SYNC

	// Fetch level 1 - Group size = 2*2->1*4 - Frequency = 2PI/4
	float	frequency = _sign * (0.5 * PI);
	FetchAndMix( 1, index, frequency );
	SYNC

	// Fetch level 2 - Group size = 2*4->1*8 - Frequency = 2PI/8
	frequency *= 0.5;
	FetchAndMix( 2, index, frequency );
	SYNC

	// Fetch level 3 - Group size = 2*8->1*16 - Frequency = 2PI/16
	frequency *= 0.5;
	FetchAndMix( 3, index, frequency );
	SYNC

	// Fetch level 4 - Group size = 2*16->1*32 - Frequency = 2PI/32
	frequency *= 0.5;
	FetchAndMix( 4, index, frequency );
	SYNC

	// Fetch level 5 - Group size = 2*32->1*64 - Frequency = 2PI/64
	frequency *= 0.5;
	FetchAndMix( 5, index, frequency );
	SYNC

	// Fetch level 6 - Group size = 2*64->1*128 - Frequency = 2PI/128
	frequency *= 0.5;
	FetchAndMix( 6, index, frequency );
	SYNC

	// Fetch level 7 - Group size = 2*128->1*256 - Frequency = 2PI/256
	frequency *= 0.5;
	FetchAndMix( 7, index, frequency );
	SYNC

	_texOut[uint2(2*_dispatchThreadID.x+0, 0)] = _normalizationFirstPass * gs_temp[2*index+0];
	_texOut[uint2(2*_dispatchThreadID.x+1, 0)] = _normalizationFirstPass * gs_temp[2*index+1];
}

// Applies FFT from stage 8 (size 256) to stage 9 (size 512)
[numthreads( 64, 1, 1 )]
void	CS__256to512( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;	// in [0,256[ (4 groups of 64 threads)
	uint2	pos = uint2( index, 0 );

	// Read 2 source values with a stride of 256
	float2	V0 = _texIn[pos];	pos.x += 256U;
	float2	V1 = _texIn[pos];

	// Apply twiddling - Group size = 2*256->1*512 - Frequency = 2PI/512
	float	frequency = index * _sign * (PI / 256.0);
	float2	sinCos;
	sincos( frequency, sinCos.x, sinCos.y );

	Twiddle( sinCos, V0, V1 );

	// Store
	_texOut[pos] = _normalizationFinal * V1;	pos.x -= 256U;
	_texOut[pos] = _normalizationFinal * V0;
}

// Applies FFT from stage 8 (size 256) to stage 10 (size 1024)
[numthreads( 64, 1, 1 )]
void	CS__256to1024( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;	// in [0,256[ (4 groups of 64 threads)
	uint2	pos = uint2( index, 0 );

	// Read 4 source values with a stride of 256
	float2	V0 = _texIn[pos];	pos.x += 256U;
	float2	V1 = _texIn[pos];	pos.x += 256U;
	float2	V2 = _texIn[pos];	pos.x += 256U;
	float2	V3 = _texIn[pos];

	// Apply twiddling - Group size = 2*256->1*512 - Frequency = 2PI/512
	float	frequency = index * _sign * (PI / 256.0);
	float2	sinCos;
	sincos( frequency, sinCos.x, sinCos.y );

	Twiddle( sinCos, V0, V1 );
	Twiddle( sinCos, V2, V3 );

	// Apply twiddling - Group size = 2*512->1*1024 - Frequency = 2PI/1024
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V0, V2 );
	sinCos = float2( _sign * sinCos.y, -_sign * sinCos.x );			// frequency + PI/2
	Twiddle( sinCos, V1, V3 );

	// Store
	_texOut[pos] = _normalizationFinal * V3;	pos.x -= 256U;
	_texOut[pos] = _normalizationFinal * V2;	pos.x -= 256U;
	_texOut[pos] = _normalizationFinal * V1;	pos.x -= 256U;
	_texOut[pos] = _normalizationFinal * V0;
}

// Applies FFT from stage 8 (size 256) to stage 11 (size 2048)
[numthreads( 64, 1, 1 )]
void	CS__256to2048( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;	// in [0,256[ (4 groups of 64 threads)
	uint2	pos = uint2( index, 0 );

	// Read 8 source values with a stride of 256
	float2	V[8];
	[unroll]
	for ( uint i=0; i < 8; i++ ) {
		V[i] = _texIn[pos];
		pos.x += 256U;
	}

	// Apply twiddling - Group size = 2*256->1*512 - Frequency = 2PI/512
	float	frequency = index * _sign * (PI / 256.0);
	float2	sinCos;
	sincos( frequency, sinCos.x, sinCos.y );

	Twiddle( sinCos, V[0], V[1] );
	Twiddle( sinCos, V[2], V[3] );
	Twiddle( sinCos, V[4], V[5] );
	Twiddle( sinCos, V[6], V[7] );

	// Apply twiddling - Group size = 2*512->1*1024 - Frequency = 2PI/1024
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V[0], V[2] );
	Twiddle( sinCos, V[4], V[6] );

	sinCos = float2( _sign * sinCos.y, -_sign * sinCos.x );			// frequency + PI/2
	Twiddle( sinCos, V[1], V[3] );
	Twiddle( sinCos, V[5], V[7] );

	// Apply twiddling - Group size = 2*1024->1*2048 - Frequency = 2PI/2048
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V[0], V[4] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + PI/2
	Twiddle( sinCos, V[2], V[6] );

	sincos( frequency + _sign * (0.25 * PI), sinCos.x, sinCos.y );	// frequency + PI/4
	Twiddle( sinCos, V[1], V[5] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + 3PI/4
	Twiddle( sinCos, V[3], V[7] );

	// Store
	pos.x = _dispatchThreadID.x;
	[unroll]
	for ( uint j=0; j < 8; j++ ) {
		_texOut[pos] = _normalizationFinal * V[j];
		pos.x += 256U;
	}
}

// Applies FFT from stage 8 (size 256) to stage 11 (size 2048)
[numthreads( 64, 1, 1 )]
void	CS__256to4096( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;	// in [0,256[ (4 groups of 64 threads)
	uint2	pos = uint2( index, 0 );

	// Read 16 source values with a stride of 256
	float2	V[16];
	[unroll]
	for ( uint i=0; i < 16; i++ ) {
		V[i] = _texIn[pos];
		pos.x += 256U;
	}

	// Apply twiddling - Group size = 2*256->1*512 - Frequency = 2PI/512
	float	frequency = index * _sign * (PI / 256.0);
	float2	sinCos;
	sincos( frequency, sinCos.x, sinCos.y );

	Twiddle( sinCos, V[0], V[1] );
	Twiddle( sinCos, V[2], V[3] );
	Twiddle( sinCos, V[4], V[5] );
	Twiddle( sinCos, V[6], V[7] );
	Twiddle( sinCos, V[8], V[9] );
	Twiddle( sinCos, V[10], V[11] );
	Twiddle( sinCos, V[12], V[13] );
	Twiddle( sinCos, V[14], V[15] );

	// Apply twiddling - Group size = 2*512->1*1024 - Frequency = 2PI/1024
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V[0], V[2] );
	Twiddle( sinCos, V[4], V[6] );
	Twiddle( sinCos, V[8], V[10] );
	Twiddle( sinCos, V[12], V[14] );

	sinCos = float2( _sign * sinCos.y, -_sign * sinCos.x );			// frequency + PI/2
	Twiddle( sinCos, V[1], V[3] );
	Twiddle( sinCos, V[5], V[7] );
	Twiddle( sinCos, V[9], V[11] );
	Twiddle( sinCos, V[13], V[15] );

	// Apply twiddling - Group size = 2*1024->1*2048 - Frequency = 2PI/2048
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V[0], V[4] );
	Twiddle( sinCos, V[8], V[12] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + PI/2
	Twiddle( sinCos, V[2], V[6] );
	Twiddle( sinCos, V[10], V[14] );

	sincos( frequency + _sign * (0.25 * PI), sinCos.x, sinCos.y );	// frequency + PI/4
	Twiddle( sinCos, V[1], V[5] );
	Twiddle( sinCos, V[9], V[13] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + PI/4 + PI/2
	Twiddle( sinCos, V[3], V[7] );
	Twiddle( sinCos, V[11], V[15] );

	// Apply twiddling - Group size = 2*2048->1*4096 - Frequency = 2PI/4096
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V[0], V[8] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + PI/2
	Twiddle( sinCos, V[4], V[12] );

	sincos( frequency + _sign * (0.125 * PI), sinCos.x, sinCos.y );	// frequency + PI/8
	Twiddle( sinCos, V[1], V[9] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + PI/8 + PI/2
	Twiddle( sinCos, V[5], V[13] );

	sincos( frequency + _sign * (0.25 * PI), sinCos.x, sinCos.y );	// frequency + PI/4
	Twiddle( sinCos, V[2], V[10] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + PI/4 + PI/2
	Twiddle( sinCos, V[6], V[14] );

	sincos( frequency + _sign * (0.375 * PI), sinCos.x, sinCos.y );	// frequency + 3PI/4
	Twiddle( sinCos, V[3], V[11] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + 3PI/4 + PI/2
	Twiddle( sinCos, V[7], V[15] );

	// Store
	pos.x = _dispatchThreadID.x;
	[unroll]
	for ( uint j=0; j < 16; j++ ) {
		_texOut[pos] = _normalizationFinal * V[j];
		pos.x += 256U;
	}
}
