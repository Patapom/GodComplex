////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Common FFT code
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;

cbuffer CB_Main : register(b0) {
	float	_sign;
	uint	_bitReversalShift;			// = 32 - log2( signal size )
	float	_normalizationFirstPass;
	float	_normalizationFinal;
};

uint	ReverseBits( uint x ) {
	x = (((x & 0xaaaaaaaaU) >> 1) | ((x & 0x55555555U) << 1));
	x = (((x & 0xccccccccU) >> 2) | ((x & 0x33333333U) << 2));
	x = (((x & 0xf0f0f0f0U) >> 4) | ((x & 0x0f0f0f0fU) << 4));
	x = (((x & 0xff00ff00U) >> 8) | ((x & 0x00ff00ffU) << 8));
	x = ((x >> 16) | (x << 16));
	x >>= _bitReversalShift;	// Last shift to ensure indices are in [0,2^POT[
	return x;
}


/*uint3	ComputeIndices( uint _groupShift, uint _dispatchThreadIndex ) {
	uint	groupSize = 1 << _groupShift;
	uint	groupIndex = _dispatchThreadIndex >> _groupShift;
	uint	elementIndex = _dispatchThreadIndex & (groupSize-1U);	// in [0,groupSize[

	uint	k_even = groupIndex * (2 * groupSize) + elementIndex;
	uint	k_odd = k_even + groupSize;
	return uint3( k_even, k_odd, elementIndex );
}

void	FetchAndMix( uint _groupShift, uint _dispatchThreadIndex, float _frequency ) {
	_dispatchThreadIndex &= 0x7FU;	// Just because we're fetching/writing from a local memory of size 128
	uint3	k = ComputeIndices( _groupShift, _dispatchThreadIndex );

	float2	E = gs_temp[k.x];
	float2	O = gs_temp[k.y];

	float	s, c;
	sincos( k.z * _frequency, s, c );

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
	uint	targetIndex = elementIndex & 0x7EU;	// Just because we're writing to a local memory of size 128
	gs_temp[targetIndex] = float2(		E.x + O.x, 
										E.y + O.y );
	gs_temp[targetIndex+1] = float2(	E.x - O.x, 
										E.y - O.y );
}

// Special case for last stage, writing to target buffer
void	FetchAndMix_Last( uint _groupShift, uint _dispatchThreadIndex, float _frequency, RWTexture2D<float2> _texTarget ) {
//	_dispatchThreadIndex &= 0x7FU;	// Just because we're fetching/writing from a local memory of size 128
//	uint3	k = ComputeIndices( _groupShift, _dispatchThreadIndex );
	uint3	k = ComputeIndices( _groupShift, _dispatchThreadIndex & 0x3FU );

	float2	E = gs_temp[k.x];
	float2	O = gs_temp[k.y];

	float	s, c;
	sincos( k.z * _frequency, s, c );

	uint	index = _dispatchThreadIndex.x << 1;
	_texTarget[uint2(index,0)] = float2(	E.x + c * O.x - s * O.y, 
											E.y + s * O.x + c * O.y );
	_texTarget[uint2(index+1,0)] = float2(	E.x - c * O.x + s * O.y, 
											E.y - s * O.x - c * O.y );
}

// General case for large groups that don't fit into local storage
// Each thread must read from and write to the target texture...
void	FetchAndMix_Large( uint _groupShift, uint _dispatchThreadIndex, float _frequency, Texture2D<float2> _texSource, RWTexture2D<float2> _texTarget ) {
	uint3	k = ComputeIndices( _groupShift, _dispatchThreadIndex );

	uint2	k_even = uint2( k.x, 0 );
	uint2	k_odd = uint2( k.y, 0 );
	float2	E = _texSource[k_even];
	float2	O = _texSource[k_odd];

	float	s, c;
	sincos( k.z * _frequency, s, c );

	_texTarget[k_even] = float2(	E.x + c * O.x - s * O.y, 
									E.y + s * O.x + c * O.y );
	_texTarget[k_odd] = float2(		E.x - c * O.x + s * O.y, 
									E.y - s * O.x - c * O.y );
}
*/