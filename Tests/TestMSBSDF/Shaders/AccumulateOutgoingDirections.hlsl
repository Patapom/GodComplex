//////////////////////////////////////////////////////////////////////////
// This shader will accumulate outgoing directions resulting from the ray-tracing pass into the global histogram
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Finalize : register(b10) {
	uint	_iterationsCount;
}

Texture2DArray< float4 >	_Tex_OutgoingDirections : register( t0 );
RWTexture2DArray< uint >	_Tex_DirectionsHistogram_Decimal : register( u0 );
RWTexture2DArray< uint >	_Tex_DirectionsHistogram_Integer : register( u1 );
RWTexture2DArray< float >	_Tex_DirectionsHistogram_Final : register( u2 );

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;
	uint	scatteringOrder = _DispatchThreadID.z;

	float4	directionWeight = _Tex_OutgoingDirections[uint3( pixelPosition, scatteringOrder )];

	directionWeight.xyz = normalize( directionWeight.xyz );

//	float	phi = fmod( 2.0 * PI + atan2( directionWeight.y, directionWeight.x ), 2.0 * PI );
//	uint	iPhi = uint( floor( LOBES_COUNT_PHI * phi / (2.0 * PI) ) );
	uint	iPhi = uint( floor( fmod( LOBES_COUNT_PHI * (1.0 + atan2( directionWeight.y, directionWeight.x ) / (2.0 * PI)), LOBES_COUNT_PHI ) ) );

// Formerly, I used the wrong discretization for histogram bins based on cosine-lobe weighted theta = 2*asin( sqrt( i / (2*N) ))
//	float	theta = acos( clamp( directionWeight.z, -1.0, 1.0 ) );
//	uint	iTheta = uint( floor( 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) ) ) );	// Inverse of 2*asin( sqrt( i / (2 * N) ) )

	// Now we're using simply theta = acos( 1 - i / N )
	// And so, to retrieve the bin index from theta we have i = N * (1 - cos( theta ))
	uint	iTheta = uint( floor( LOBES_COUNT_THETA * (1.0 - directionWeight.z) ) );


iTheta = min( LOBES_COUNT_THETA-1, iTheta );


//iPhi = pixelPosition.x * LOBES_COUNT_PHI / HEIGHTFIELD_SIZE;
//iTheta = pixelPosition.y * LOBES_COUNT_THETA / HEIGHTFIELD_SIZE;
//directionWeight.w = abs( sin( 2.0 * PI * pixelPosition.x / HEIGHTFIELD_SIZE ) );
//directionWeight.w *= abs( sin( 2.0 * PI * pixelPosition.y / HEIGHTFIELD_SIZE ) );

	// Here, the factor applied to the weight is chosen such as factor * HEIGHTFIELD_SIZE * HEIGHTFIELD_SIZE = 2^32
	// It is chosen so even if all rays contributed exactly to the same histogram slot, a single iteration of HEIGHTFIELD_SIZE*HEIGHTFIELD_SIZE rays
	//	would add up to 1*(2^32) + 0 in our 2 32-bits counters and so we could still use 2^32 total iterations to exhaust the MSW...
	//
	uint	value = uint( floor( 16384.0 * directionWeight.w ) );

	uint	oldValue;
	if ( iTheta < LOBES_COUNT_THETA ) {
		uint3	UVW = uint3( iPhi, iTheta, scatteringOrder );
		InterlockedAdd( _Tex_DirectionsHistogram_Decimal[UVW], value, oldValue );						// Decimal point addition
		value += oldValue;																				// Perform local addition to see if we need to add carry to integers accumulator
		InterlockedAdd( _Tex_DirectionsHistogram_Integer[UVW], value < oldValue ? 1 : 0, oldValue );	// Integer addition with carry
	}
}

// Finalize the histogram into a nice float texture
// This code is a bit tricky as accumulation was performed into 2 UINT32 UAVs just because we needed to account for values larger than 2^32 (that's because we encode the weight on 8 bits)
[numthreads( 16, 16, 1 )]
void	CS_Finalize( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint2	pixelPosition = _DispatchThreadID.xy;

	uint2	dimensions;
	uint	onsenfout;
	_Tex_DirectionsHistogram_Final.GetDimensions( dimensions.x, dimensions.y, onsenfout );
	if ( any( pixelPosition > dimensions ) )
		return;

	uint	scatteringOrder = _DispatchThreadID.z;
	uint3	UVW = uint3( pixelPosition, scatteringOrder );
	uint	counter_decimal = _Tex_DirectionsHistogram_Decimal[UVW];	// LSW part of the counter
	uint	counter_integer = _Tex_DirectionsHistogram_Integer[UVW];	// MSW part of the counter

//counter_decimal = 0xFFFFFFFFU;
//counter_integer = 0;

	float	finalCounter = (counter_integer + (float(counter_decimal) / 4294967296.0)) / _iterationsCount;

	_Tex_DirectionsHistogram_Final[UVW] = finalCounter;
}
