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
//	_Position, position of the ray in the texture (XY = pixel position offset by 0.5, Z = initial height in millimeters)
//	_Direction, direction of the ray
//
// Returns an occlusion value in [0,1] where 0 is completely occluded and 1 completely visible
//
float	ComputeDirectionalOcclusion( float2 _TextureDimensions, float3 _Position, float3 _Direction ) {

	#if 1
		// Scale the ray so we ensure to always walk at least a texel in the texture
		_Direction *= _Direction.z < 1.0 ? 1.0 / sqrt( 1.0 - _Direction.z * _Direction.z ) : 1.0;
	#endif

//_Direction.z *= 0.001;
//_Direction *= 2.0;

	float3	UVH = float3( _Position.xy / _TextureDimensions, _Position.z );
	float3	dUVH = float3( _Direction.xy / _TextureDimensions, _Direction.z * _TexelSize_mm );
	float	prevH_mm = UVH.z;

	float	Occlusion = 1.0;	// Start unoccluded
	for ( uint StepIndex=0; StepIndex < _MaxStepsCount; StepIndex++ ) {
		UVH += dUVH;	
		if ( UVH.z >= _Displacement_mm )
			break;		// Definitely escaped the surface!
		if ( UVH.z < 0.0 )
			return 0.0;	// Definitely occluded!

		if ( _Tile ) {
			UVH.xy = frac( UVH.xy );
		} else {
			if ( any( UVH.xy < 0 ) || any( UVH.xy >= 1.0 ) )
				break;	// Escaped the texture...
		}

//		float	H_mm = _Displacement_mm * _Source.Load( int3( UVH.xy, 0 ) );
		float	H_mm = _Displacement_mm * _Source.SampleLevel( LinearClamp, UVH.xy, 0 );

		// Simple test for a fully extruded height
		if ( UVH.z < H_mm ) {
			// Compute actual hit position
			float	t = (H_mm - UVH.z) / (2.0*UVH.z - dUVH.z + H_mm - prevH_mm);
			UVH -= t * dUVH;

			// Build local tangent space to orient rays
			float3	Normal = _SourceNormals.SampleLevel( LinearClamp, UVH.xy, 0 );
//			float3	Tangent, BiTangent;
//			BuildOrthonormalBasis( Normal, Tangent, BiTangent );

			// Reflect ray against plane
			float	d = dot( _Direction, Normal );
			_Direction -= 2.0 * d * Normal;

			_Position.xy = UVH.xy * _TextureDimensions;

			_Position += 1e-2 * Normal;	// Nudge a little to avoid acnea

			// Update UVs
			UVH.xy = _Position / _TextureDimensions;
			dUVH = float3( _Direction.xy / _TextureDimensions, _Direction.z * _TexelSize_mm );
		}

		prevH_mm = H_mm;
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
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID ) {
	uint2	PixelPosition = uint2( _GroupID.x, _Y0 + _GroupID.y );

	uint	RayIndex = _GroupThreadID.x;
	if ( RayIndex < _RaysCount ) {
		float2	fPixelPosition = 0.5 + PixelPosition;

		uint2	Dimensions;
		_Source.GetDimensions( Dimensions.x, Dimensions.y );

//		_Direction.z *= _TexelSize_mm / max( 1e-4, _Displacement_mm );	// Scale the vertical step so we're the proper size when comparing to normalized height map
//		float	MaxHeightPerTexel = _TexelSize_mm > 0.0 ? _Displacement_mm / _TexelSize_mm : 0.0;
		float	H0_mm = _Displacement_mm * _Source.Load( int3( PixelPosition, 0 ) );

		float3	RayPosition = float3( fPixelPosition, H0_mm );
		float3	RayDirection = _Rays[RayIndex];

		// Build local tangent space to orient rays
		float3	Normal = _SourceNormals.SampleLevel( LinearClamp, fPixelPosition / Dimensions, 0 );
		float3	Tangent, BiTangent;
		BuildOrthonormalBasis( Normal, Tangent, BiTangent );

		RayDirection = RayDirection.x * Tangent + RayDirection.y * BiTangent + RayDirection.z * Normal;

//RayPosition.z += 1e-2;	// Nudge a little to avoid acnea
RayPosition += 1e-2 * Normal;	// Nudge a little to avoid acnea

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
}