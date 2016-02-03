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

	float	theta = acos( clamp( directionWeight.z, -1.0, 1.0 ) );
	float	phi = fmod( 2.0 * PI + atan2( directionWeight.y, directionWeight.x ), 2.0 * PI );

	uint	iTheta = uint( floor( 2.0 * LOBES_COUNT_THETA * pow2( sin( 0.5 * theta ) ) ) );	// Inverse of 2*asin( sqrt( i / (2 * N) ) )
	uint	iPhi = uint( floor( LOBES_COUNT_PHI * phi / (2.0 * PI) ) );


//iPhi = pixelPosition.x * LOBES_COUNT_PHI / HEIGHTFIELD_SIZE;
//iTheta = pixelPosition.y * LOBES_COUNT_THETA / HEIGHTFIELD_SIZE;
//directionWeight.w = abs( sin( 2.0 * PI * pixelPosition.x / HEIGHTFIELD_SIZE ) );
//directionWeight.w *= abs( sin( 2.0 * PI * pixelPosition.y / HEIGHTFIELD_SIZE ) );


	uint	value = uint( floor( 256.0 * directionWeight.w ) );
	uint	oldValue;
	if ( iTheta < LOBES_COUNT_THETA ) {
		uint3	UVW = uint3( iPhi, iTheta, scatteringOrder );
		InterlockedAdd( _Tex_DirectionsHistogram_Decimal[UVW], value, oldValue );						// Decimal point addition
		value += oldValue;																				// Perform local addition to see if we need to add carry to integers accumulator
		InterlockedAdd( _Tex_DirectionsHistogram_Integer[UVW], value < oldValue ? 1 : 0, oldValue );	// Integer addition with carry
	}
}

// Finalize the histogram into a nice float texture
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

	float	integerFactor = _iterationsCount > 256 ? (256.0 * HEIGHTFIELD_SIZE * HEIGHTFIELD_SIZE) / ((_iterationsCount & 0xFF00U) * HEIGHTFIELD_SIZE * HEIGHTFIELD_SIZE) : 1.0;
	float	decimalFactor = 1.0 / (256.0 * min( 256.0, _iterationsCount ) * HEIGHTFIELD_SIZE * HEIGHTFIELD_SIZE);	// At most 2^-32 when iterations count exceed 256
	float	finalCounter = integerFactor * (counter_integer + decimalFactor * counter_decimal);

	_Tex_DirectionsHistogram_Final[UVW] = finalCounter;
}