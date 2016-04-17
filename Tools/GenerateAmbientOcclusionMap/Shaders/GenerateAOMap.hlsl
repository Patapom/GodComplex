////////////////////////////////////////////////////////////////////////////////
// AOMap generator
// This compute shader will generate the ambient occlusion over a specific texel and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////
//
static const uint	MAX_THREADS = 1024;

static const float	PI = 3.1415926535897932384626433832795;

SamplerState LinearClamp	: register( s0 );

cbuffer	CBInput : register( b0 )
{
	uint	_Y0;				// Start scanline for this group
	uint	_RaysCount;			// Amount of rays to cast
	uint	_MaxStepsCount;		// Maximum amount of steps to take before stopping
	bool	_Tile;				// Tiling flag
	float	_TexelSize_mm;		// Texel size in millimeters
	float	_Displacement_mm;	// Height map max encoded displacement in millimeters
}

Texture2D<float>			_Source : register( t0 );
RWTexture2D<float>			_Target : register( u0 );

StructuredBuffer<float3>	_Rays : register( t1 );

Texture2D<float3>			_SourceNormals : register( t2 );

groupshared float			gs_Occlusion[MAX_THREADS];


// Computes the occlusion of the pixel in the specified direction
//	_TextureDimensions, size of the texture in pixels
//	_Position, position of the ray in the texture (XY = pixel position offset by 0.5, Z = initial height)
//	_Direction, direction of the ray
//
// Returns an occlusion value in [0,1] where 0 is completely occluded and 1 completely visible
//
float	ComputeDirectionalOcclusion( float2 _TextureDimensions, float3 _Position, float3 _Direction ) {

	#if 1
		// Scale the ray so we ensure to always walk at least a texel in the texture
		_Direction *= 1.0 / sqrt( 1.0 - _Direction.z * _Direction.z );
	#endif

	// Scale the vertical step so we're the proper size
	_Direction.z *= _TexelSize_mm / max( 1e-4, _Displacement_mm );

//_Direction.z *= 0.001;
//_Direction *= 2.0;

	float	Occlusion = 1.0;	// Start unoccluded
	for ( uint StepIndex = 0; StepIndex < _MaxStepsCount; StepIndex++ ) {
		_Position += _Direction;	
		if ( _Position.z >= 1.0 )
			break;		// Definitely escaped the surface!
		if ( _Position.z < 0.0 )
			return 0.0;	// Definitely occluded!

		if ( _Tile ) {
			_Position.xy = fmod( _Position.xy + _TextureDimensions, _TextureDimensions );
		} else {
			if (	_Position.x < 0 || _Position.x >= _TextureDimensions.x
				||	_Position.y < 0 || _Position.y >= _TextureDimensions.y )
				break;
		}

//		float	H = _Source.Load( int3( _Position.xy, 0 ) );
		float	H = _Source.SampleLevel( LinearClamp, _Position.xy / _TextureDimensions, 0 );

		#if 1
			// Simple test for a fully extruded height
			if ( _Position.z < H )
				return 0.0;
		#else
			// Assume a height interval
			float	Hmax = H;
			float	Hmin = H - 0.01;
			if ( _Position.z > Hmin && _Position.z < Hmax )
				return 0.0;
		#endif
	}

	return Occlusion;
}

// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
void BuildOrthonormalBasis( float3 _normal, out float3 _tangent, out float3 _bitangent ) {
	float a = _normal.z > -0.9999999 ? 1.0 / (1.0 + _normal.z) : 0.0;
	float b = -_normal.x * _normal.y * a;

	_tangent = float3( 1.0 - _normal.x*_normal.x*a, b, -_normal.x );
	_bitangent = float3( b, 1.0 - _normal.y*_normal.y*a, -_normal.y );
}

[numthreads( MAX_THREADS, 1, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID )
{
	uint2	PixelPosition = uint2( _GroupID.x, _Y0 + _GroupID.y );

	uint	RayIndex = _GroupThreadID.x;

	if ( RayIndex < _RaysCount ) {
		float2	fPixelPosition = 0.5 + PixelPosition;

		uint2	Dimensions;
		_Source.GetDimensions( Dimensions.x, Dimensions.y );

		float	H0 = _Source.Load( int3( PixelPosition, 0 ) );

		float3	RayPosition = float3( fPixelPosition, H0 );
		float3	RayDirection = _Rays[RayIndex];

		#if 1
			// Build local tangent space to orient rays
			float3	Normal = _SourceNormals.SampleLevel( LinearClamp, fPixelPosition / Dimensions, 0 );
//					Normal.y = -Normal.y;
			float3	Tangent, BiTangent;
			BuildOrthonormalBasis( Normal, Tangent, BiTangent );

			RayDirection = RayDirection.x * Tangent + RayDirection.y * BiTangent + RayDirection.z * Normal;

//			float3	RayPosition_mm = RayPosition * float3( _TexelSize_mm.xx, _Displacement_mm );
//			RayPosition_mm += 0.01 * _Displacement_mm * Normal;	// Nudge a little to avoid acnea
//			RayPosition = RayPosition_mm / float3( _TexelSize_mm.xx, _Displacement_mm );

RayPosition.z += 1e-2;	// Nudge a little to avoid acnea

		#else
			RayPosition.z += 1e-2;	// Nudge a little to avoid acnea
		#endif

		gs_Occlusion[RayIndex] = ComputeDirectionalOcclusion( Dimensions, RayPosition, RayDirection );

//gs_Occlusion[RayIndex] = Normal.y;
	} else {
		// Clear remaining rays so they don't interfere with the accumulation
		gs_Occlusion[RayIndex] = 0.0;
	}

	GroupMemoryBarrierWithGroupSync();

	if ( RayIndex == 0 ) {
		float	Result = 0.0;
		for ( uint i=0; i < _RaysCount; i++ )
			Result += gs_Occlusion[i];
		Result /= _RaysCount;

// Shows bilateral filtered source
//Result = _Source.Load( int3( PixelPosition, 0 ) ).x;

		_Target[PixelPosition] = Result;
	}

// This code should be optimum but it fails and introduces noise for some reason I can't explain...
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