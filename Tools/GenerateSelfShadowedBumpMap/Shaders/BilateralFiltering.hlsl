////////////////////////////////////////////////////////////////////////////////
// Bilateral Filtering
// This compute shader applies bilateral filtering to the input height map to smooth out 
//	noise while conserving main features.
// This is essential to obtain smooth directional occlusion results.
//
////////////////////////////////////////////////////////////////////////////////
//
static const uint	THREADS_COUNT = 32;
static const uint	KERNEL_AREA = (2*THREADS_COUNT)*(2*THREADS_COUNT);

cbuffer	CBInput : register( b0 )
{
	uint	_Y0;				// Start scanline for this group
	float	_Radius;			// Bilateral filtering radius
	float	_Tolerance;			// Bilateral filtering range tolerance
	bool	_Tile;				// Tiling flag
}

Texture2D<float>			_Source : register( t0 );
RWTexture2D<float>			_Target : register( u0 );

groupshared float2			gs_Samples[KERNEL_AREA];

float2	GaussianSample( int2 _Dimensions, int2 _PixelPosition, int2 _PixelOffset, float _H0 )
{
	_PixelPosition += _PixelOffset;
	if ( _Tile )
		_PixelPosition = uint2(_PixelPosition + _Dimensions) % uint2(_Dimensions);
	else if ( any( _PixelPosition.xy < 0 ) || any( _PixelPosition.xy >= _Dimensions ) )
		return 0.0;

	float	H = _Source.Load( int3( _PixelPosition, 0 ) ).x;

	// Domain filter
	const float	SIGMA_DOMAIN = -0.5 * pow( _Radius / 3.0f, -2.0 );
	float	DomainGauss = exp( dot( _PixelOffset, _PixelOffset ) * SIGMA_DOMAIN );

//DomainGauss = 1.0;
//return DomainGauss * float2( H, 1.0 );

	// Range filter
	const float	SIGMA_RANGE = -0.5 * pow( _Tolerance, -2.0 );

	float	Diff = abs( H - _H0 );
	float	RangeGauss = exp( Diff*Diff * SIGMA_RANGE );

	return DomainGauss * RangeGauss * float2( H, 1.0 );
}

uint	GetSampleIndex( uint2 _Pos ) {
	return 2*THREADS_COUNT*_Pos.y+_Pos.x;
}

[numthreads( THREADS_COUNT, THREADS_COUNT, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID )
{
	uint2	PixelPosition = uint2( _GroupID.x, _Y0 + _GroupID.y );
	uint2	ThreadIndex = _GroupThreadID.xy;

	int2	Dimensions;
	_Source.GetDimensions( Dimensions.x, Dimensions.y );

	float	H0 =  _Source.Load( uint3( PixelPosition, 0 ) ).x;

	// Each thred processes 4 samples
	uint	SampleOffset = GetSampleIndex( 2*ThreadIndex );
	int2	PixelOffset = 2*int2(ThreadIndex) - int(THREADS_COUNT);
	gs_Samples[SampleOffset+0] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );					PixelOffset.x++;
	gs_Samples[SampleOffset+1] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );					PixelOffset.y++;
	gs_Samples[SampleOffset+2*THREADS_COUNT+1] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );	PixelOffset.x--;
	gs_Samples[SampleOffset+2*THREADS_COUNT+0] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );

	GroupMemoryBarrierWithGroupSync();

#if 0	// Sum using a single thread
 	if ( all( _GroupThreadID == 0 ) )
 	{
 		float2	Result = 0.0;
 		for ( uint i=0; i < KERNEL_AREA; i++ )
 			Result += gs_Samples[i];
 		Result.x /= Result.y;
 
 		_Target[PixelPosition] = Result.x;
 	}
#else	// Perform parallel reduction

	uint	SampleIndex = GetSampleIndex( ThreadIndex );

	if ( all( ThreadIndex < 32U ) && THREADS_COUNT >= 32 ) {
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(32, 0) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2( 0,32) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(32,32) )];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( all( ThreadIndex < 16 ) && THREADS_COUNT >= 16 ) {
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(16, 0) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2( 0,16) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(16,16) )];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( all( ThreadIndex < 8 ) && THREADS_COUNT >= 8 ) {
		uint	SampleIndex = GetSampleIndex( ThreadIndex );
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(8,0) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(0,8) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(8,8) )];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( all( ThreadIndex < 4 ) && THREADS_COUNT >= 4 ) {
		uint	SampleIndex = GetSampleIndex( ThreadIndex );
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(4,0) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(0,4) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(4,4) )];
	}

	GroupMemoryBarrierWithGroupSync();

	if ( all( ThreadIndex < 2 ) && THREADS_COUNT >= 2 ) {
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(2,0) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(0,2) )];
		gs_Samples[SampleIndex] += gs_Samples[GetSampleIndex( ThreadIndex+uint2(2,2) )];
	}

//	GroupMemoryBarrierWithGroupSync();	// Wavefront is large enough so we don't need to sync

	// Final sum
	if ( all( ThreadIndex == 0 ) ) {
		float2	Result  = gs_Samples[0] + gs_Samples[1] + gs_Samples[2] + gs_Samples[3];

		_Target[PixelPosition] = Result.x / Result.y;
	}
#endif
}
