////////////////////////////////////////////////////////////////////////////////
// Bilateral Filtering
// This compute shader applies bilateral filtering to the input height map to smooth out 
//	noise while conserving main features.
// This is essential to obtain smooth directional occlusion results.
//
////////////////////////////////////////////////////////////////////////////////
//
cbuffer	CBInput : register( b0 )
{
	float	_Radius;			// Bilateral filtering radius
	float	_Tolerance;			// Bilateral filtering range tolerance
	bool	_Tile;				// Tiling flag
}

Texture2D<float>			_Source : register( t0 );
RWTexture2D<float>			_Target : register( u0 );

groupshared float2			gs_Samples[64*64];

float2	GaussianSample( uint2 _Dimensions, int2 _PixelPosition, int2 _PixelOffset, float _H0 )
{
	_PixelPosition += _PixelOffset;
	if ( _Tile )
		_PixelPosition = (_PixelPosition + _Dimensions) % _Dimensions;

	int2	PixelPosition = int2( _PixelPosition );
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

[numthreads( 32, 32, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID )
{
	int2	PixelPosition = _GroupID.xy;

	uint2	Dimensions;
	_Source.GetDimensions( Dimensions.x, Dimensions.y );

	float	H0 =  _Source.Load( int3( PixelPosition, 0 ) ).x;

	uint	SampleOffset = 4*(32*_GroupThreadID.y+_GroupThreadID.x);
	int2	PixelOffset = 2*_GroupThreadID.xy - 32;
	gs_Samples[SampleOffset+0] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );	PixelOffset.x++;
	gs_Samples[SampleOffset+1] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );	PixelOffset.y++;
	gs_Samples[SampleOffset+2] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );	PixelOffset.x--;
	gs_Samples[SampleOffset+3] = GaussianSample( Dimensions, PixelPosition, PixelOffset, H0 );

	GroupMemoryBarrierWithGroupSync();

	if ( all( _GroupThreadID == 0 ) )
	{
		float2	Result = 0.0;
		for ( int i=0; i < 64*64; i++ )
			Result += gs_Samples[i];
		Result.x /= Result.y;

		_Target[PixelPosition] = Result.x;
	}
}
