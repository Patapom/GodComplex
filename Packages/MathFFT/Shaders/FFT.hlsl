////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Common FFT code
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//

cbuffer : register(b0) {
	float	_sign;
//	uint	_groupSize;
};


uint	wang_hash(uint seed) {
    seed = (seed ^ 61) ^ (seed >> 16);
    seed *= 9;
    seed = seed ^ (seed >> 4);
    seed *= 0x27d4eb2d;
    seed = seed ^ (seed >> 15);
    return seed;
}

groupshared float2	gs_temp[128];

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
uint2	ComputeIndices( uint _groupShift, uint _dispatchThreadIndex ) {
	uint	groupSize = 1 << _groupShift;
	uint	groupIndex = _dispatchThreadIndex >> _groupShift;
	uint	elementIndex = _dispatchThreadIndex & (groupSize-1);	// in [0,groupSize[

	uint	k_even = groupIndex * (2 * groupSize) + elementIndex;
	uint	k_odd = k_even + groupSize;
	return uint2( k_even, k_odd );
}

void	FetchAndMix( uint _groupShift, uint _dispatchThreadIndex, float _frequency ) {
	_dispatchThreadIndex &= 0x7FU;	// Just because we're fetching/writing from a local memory of size 128
	uint2	k = ComputeIndices( _groupShift, _dispatchThreadIndex );

	float2	E = gs_temp[k.x];
	float2	O = gs_temp[k.y];

	float	s, c;
	sincos( elementIndex * _frequency, s, c );

	gs_temp[k.x] = float2(	E.x + c * O.x - s * O.y, 
							E.y + s * O.x + c * O.y );
	gs_temp[k.y] = float2(	E.x - c * O.x + s * O.y, 
							E.y - s * O.x - c * O.y );
}

// Special case for stage 0 and group size 1, reading from source buffer
void	FetchAndMix_First( uint _dispatchThreadIndex, Texture2D<float2> _texSource ) {
	uint	elementIndex = _dispatchThreadIndex << 1;

	float2	E = _texSource[uint2(elementIndex,0)];
	float2	O = _texSource[uint2(elementIndex+1,0)];

	// Sine = 0, Cosine = 1
	uint	targetIndex = elementIndex & 0x7EU;	// Just because we're fetching/writing from a local memory of size 128
	gs_temp[targetIndex] = float2(		E.x + O.x, 
										E.y + O.y );
	gs_temp[targetIndex+1] = float2(	E.x - O.x, 
										E.y - O.y );
}

// Special case for last stage, writing to target buffer
void	FetchAndMix_Last( uint _groupShift, uint _dispatchThreadIndex, RWTexture2D<float2> _texTarget ) {
	_dispatchThreadIndex &= 0x7FU;	// Just because we're fetching/writing from a local memory of size 128
	uint2	k = ComputeIndices( _groupShift, _dispatchThreadIndex );

	float2	E = gs_temp[k.x];
	float2	O = gs_temp[k.y];

	float	s, c;
	sincos( elementIndex * _frequency, s, c );

	_texTarget[k.x] = float2(	E.x + c * O.x - s * O.y, 
								E.y + s * O.x + c * O.y );
	_texTarget[k.y] = float2(	E.x - c * O.x + s * O.y, 
								E.y - s * O.x - c * O.y );
}

// General case for large groups that don't fit into local storage
// Each thread must read from and write to the target texture...
void	FetchAndMix_Large( uint _groupShift, uint _dispatchThreadIndex, float _frequency, RWTexture2D<float2> _texTarget ) {
	uint2	k = ComputeIndices( _groupShift, _dispatchThreadIndex );

	float2	E = _texTarget[k.x];
	float2	O = _texTarget[k.y];

	float	s, c;
	sincos( elementIndex * _frequency, s, c );

	_texTarget[k.x] = float2(	E.x + c * O.x - s * O.y, 
								E.y + s * O.x + c * O.y );
	_texTarget[k.y] = float2(	E.x - c * O.x + s * O.y, 
								E.y - s * O.x - c * O.y );
}
