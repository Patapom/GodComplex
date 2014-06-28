////////////////////////////////////////////////////////////////////////////////
// SSBumpMap generator
// This compute shader will generate the directional and ambient occlusion over a specific texel
//	and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////
//
static const uint	MAX_THREADS = 1024;

static const float	PI = 3.1415926535897932384626433832795;

cbuffer	CBInput : register( b0 )
{
	uint	_Y0;				// Start scanline for this group
	uint	_RaysCount;			// Amount of rays to cast
	uint	_MaxStepsCount;		// Maximum amount of steps to take before stopping
	float	_TexelSize_mm;		// Texel size in millimeters
	float	_Displacement_mm;	// Height map max encoded displacement in millimeters
	float	_AOFactor;			// Darkening factor for AO when ray goes below height map
}

Texture2D<float>			_Source : register( t0 );
RWTexture2D<float4>			_Target : register( u0 );

StructuredBuffer<float3>	_Rays : register( t1 );

groupshared float4			gs_Occlusion[MAX_THREADS];


// float Hash( float n )
// {
// 	return frac( sin(n) * 43758.5453 );
// }
// 

// Computes the occlusion of the pixel in the specified direction
// Returns an occlusion value in [0,1] where 0 is completely occluded and 1 completely visible
float	ComputeDirectionalOcclusion( float2 _TextureDimensions, float2 _PixelPosition, float _H0, float3 _Dir )
{
// float	SinTheta = sqrt( 1.0 - _Dir.z*_Dir.z );
// float	dTheta = 0.5 * PI;
// float	dPhi = 2.0 * PI;
// return 1.0;//0.5 * (SinTheta * dPhi * dTheta) / (2.0 * PI);

	_Dir.z *= _TexelSize_mm / max( 1e-4, _Displacement_mm );
//	_Dir.z = max( 1.0 / MAX_STEPS_COUNT, _Dir.z );	// Can't take more than maximum steps to exit the surface

	float	Occlusion = 1.0;	// Start unoccluded
	float3	Position = float3( _PixelPosition, _H0 );
	for ( uint StepIndex = 0; StepIndex < _MaxStepsCount; StepIndex++ )
	{
		Position += _Dir;
		if (	Position.x < 0 || Position.x >= _TextureDimensions.x
			||	Position.y < 0 || Position.y >= _TextureDimensions.y
			||	Position.z >= 1.0 )
			break;

		float	H = _Source.Load( int3( Position.xy, 0 ) ).x;
		Occlusion *= 1.0 - saturate( _AOFactor * (H - Position.z) );	// Will get darker as soon as height map goes above ray position
// 		if ( Occlusion < 1e-3 )
// 			return 0.0;

// 		if ( H > Position.z )
// // // if ( abs( H - Position.z ) > 1e-3 )
//   			return 0.0;
	}

//Occlusion = Hash( dot( _Dir, float3( 0.320159, -0.87651, 0.16512603 ) ) );

//	return 1.0;
	return Occlusion;
}

[numthreads( MAX_THREADS, 1, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID )
{
	uint2	PixelPosition = uint2( _GroupID.x, _Y0 + _GroupID.y );

	uint	RayIndex = _GroupThreadID.x;

	if ( RayIndex < _RaysCount )
	{
		float2	fPixelPosition = PixelPosition;

		float3	Dimensions;
		_Source.GetDimensions( Dimensions.x, Dimensions.y );

		float	H0 = _Source.Load( int3( PixelPosition, 0 ) ).x;

		float4	Occlusion;
		Occlusion.x = ComputeDirectionalOcclusion( Dimensions.xy, fPixelPosition, H0, _Rays[0*MAX_THREADS+RayIndex] );
		Occlusion.y = ComputeDirectionalOcclusion( Dimensions.xy, fPixelPosition, H0, _Rays[1*MAX_THREADS+RayIndex] );
		Occlusion.z = ComputeDirectionalOcclusion( Dimensions.xy, fPixelPosition, H0, _Rays[2*MAX_THREADS+RayIndex] );
		Occlusion.w = dot( Occlusion.xyz, 1.0 / 3.0 );

		gs_Occlusion[RayIndex] = Occlusion;
	}
	else
	{	// Clear remaining rays so they don't interfere with the accumulation
		gs_Occlusion[RayIndex] = 0.0;
	}

	GroupMemoryBarrierWithGroupSync();

	if ( RayIndex == 0 )
	{
		float4	Result = 0.0;
		for ( int i=0; i < MAX_THREADS; i++ )
			Result += gs_Occlusion[i];
		Result /= _RaysCount;

		_Target[PixelPosition] = Result;
	}


// // 	// Perform accumulation
// // 	if ( RayIndex < 512 )
// // 	{
// // 		for ( int i=0; i < 512; i++ )
// // 			gs_Occlusion[i] += gs_Occlusion[512+i];
// // 	}
// // 
// // 	GroupMemoryBarrierWithGroupSync();
// // 
// // 	if ( RayIndex < 256 )
// // 	{
// // 		for ( int i=0; i < 256; i++ )
// // 			gs_Occlusion[i] += gs_Occlusion[256+i];
// // 	}
// // 
// // 	GroupMemoryBarrierWithGroupSync();
// 
// 	if ( RayIndex < 128 )
// 	{
// 		for ( int i=0; i < 128; i++ )
// 			gs_Occlusion[i] += gs_Occlusion[128+i];
// 	}
// 
// 	GroupMemoryBarrierWithGroupSync();
// 
// 	if ( RayIndex < 64 )
// 	{
// 		for ( int i=0; i < 64; i++ )
// 			gs_Occlusion[i] += gs_Occlusion[64+i];
// 	}
// 
// 	GroupMemoryBarrierWithGroupSync();
// 
// 	if ( RayIndex < 32 )
// 	{
// 		for ( int i=0; i < 32; i++ )
// 			gs_Occlusion[i] += gs_Occlusion[32+i];
// 	}
// 
// 	GroupMemoryBarrierWithGroupSync();
// 
// 	if ( RayIndex < 16 )
// 	{
// 		for ( int i=0; i < 16; i++ )
// 			gs_Occlusion[i] += gs_Occlusion[16+i];
// 	}
// 
// 	GroupMemoryBarrierWithGroupSync();
// 
// 	if ( RayIndex < 8 )
// 	{
// 		for ( int i=0; i < 8; i++ )
// 			gs_Occlusion[i] += gs_Occlusion[8+i];
// 	}
// 
// 	GroupMemoryBarrierWithGroupSync();
// 
// 	if ( RayIndex < 4 )
// 	{
// 		for ( int i=0; i < 4; i++ )
// 			gs_Occlusion[i] += gs_Occlusion[4+i];
// 	}
// 
// 	GroupMemoryBarrierWithGroupSync();
// 
// 	if ( RayIndex == 0 )
// 	{
// 		float4	Result = (gs_Occlusion[0] + gs_Occlusion[1] + gs_Occlusion[2] + gs_Occlusion[3]) / RaysCount;
// Result *= 0.5;
// 		_Target[PixelPosition] = Result;
// 
// // _Target[PixelPosition] = H0 * float4( 1, 0.5, 0.25, 1 );
// // _Target[PixelPosition] = float4( abs(_Rays[min( 3*MAX_THREADS, PixelPosition.x+512*PixelPosition.y )]), 1 );
// 
// 	}
}