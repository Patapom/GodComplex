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

Texture2D<float>			_source : register( t0 );
RWTexture2D<float>			_target : register( u0 );

StructuredBuffer<float3>	_rays : register( t1 );

Texture2D<float3>			_sourceNormals : register( t2 );

RWStructuredBuffer<uint>	_indirectPixelsStackSize : register( u1 );
RWStructuredBuffer<uint2>	_indirectPixelsStack : register( u2 );
//RWTexture2D<uint>			_indirectPixelsHistogram : register( u3 );


groupshared float			gs_occlusion[MAX_THREADS];


uint	PackPixelPosition( float2 _position ) {
	uint2	intPosition = uint2( floor( _position ) );
	return ((intPosition.y & 0xFFFFU) << 16) | (intPosition.x & 0xFFFFU);
}

// Computes the occlusion of the pixel in the specified direction
//	_textureDimensions, size of the texture in pixels
//	_position, position of the ray in the texture (XY = pixel position offset by 0.5, Z = initial height in millimeters)
//	_direction, direction of the ray
//
// Returns an occlusion value in [0,1] where 0 is completely occluded and 1 completely visible
//
float	ComputeDirectionalOcclusion( uint2 _pixelPosition, float2 _textureDimensions, float3 _position, float3 _direction ) {

	// Scale the ray so we ensure to always walk at least a texel in the texture
	_direction *= _direction.z < 1.0 ? 1.0 / sqrt( 1.0 - _direction.z * _direction.z ) : 1.0;

	float3	UVH = float3( _position.xy / _textureDimensions, _position.z );
	float3	dUVH = float3( _direction.xy / _textureDimensions, _direction.z * _TexelSize_mm );
	float	prevH_mm = UVH.z;

	uint	sourcePackedPixelPosition = PackPixelPosition( _pixelPosition );

	for ( uint stepIndex=0; stepIndex < _MaxStepsCount; stepIndex++ ) {
		UVH += dUVH;	
		if ( UVH.z >= _Displacement_mm )
			break;		// Definitely escaped the surface!

		if ( _Tile ) {
			UVH.xy = frac( UVH.xy );
		} else {
			if ( any( UVH.xy < 0 ) || any( UVH.xy >= 1.0 ) )
				break;	// Escaped the texture...
		}

		float	H_mm = _Displacement_mm * _source.SampleLevel( LinearClamp, UVH.xy, 0 );

		// Simple test for a fully extruded height
		if ( UVH.z < H_mm ) {
			// Compute actual hit position
			float	t = (H_mm - UVH.z) / (2.0*UVH.z - dUVH.z + H_mm - prevH_mm);
			UVH -= t * dUVH;

			// Register a new hit
			uint	stackIndex;
			InterlockedAdd( _indirectPixelsStackSize[0], 1, stackIndex );	// Allocate a new slot where to store our indirect hit

			uint	targetPackedPixelPosition = PackPixelPosition( frac( UVH.xy ) * _textureDimensions );

//sourcePackedPixelPosition = 0x12341234;
//targetPackedPixelPosition = 0x56785678;

			_indirectPixelsStack[stackIndex] = uint2( sourcePackedPixelPosition, targetPackedPixelPosition );	// Register indirect hit

//			uint	onSenFout;
//			InterlockedAdd( _indirectPixelsHistogram[sourcePixelPosition], 1, onSenFout );		// Increase histogram counter for our source pixel

			return 0.0;
		}

		prevH_mm = H_mm;
	}

	return 1.0;
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
	uint2	pixelPosition = uint2( _GroupID.x, _Y0 + _GroupID.y );

	uint	rayIndex = _GroupThreadID.x;
	if ( rayIndex < _RaysCount ) {
		float2	fPixelPosition = 0.5 + pixelPosition;

		uint2	dimensions;
		_source.GetDimensions( dimensions.x, dimensions.y );

//		_direction.z *= _TexelSize_mm / max( 1e-4, _Displacement_mm );	// Scale the vertical step so we're the proper size when comparing to normalized height map
//		float	MaxHeightPerTexel = _TexelSize_mm > 0.0 ? _Displacement_mm / _TexelSize_mm : 0.0;
		float	H0_mm = _Displacement_mm * _source.Load( int3( pixelPosition, 0 ) );

		float3	rayPosition = float3( fPixelPosition, H0_mm );
		float3	rayDirection = _rays[rayIndex];

		// Build local tangent space to orient rays
		float3	Normal = _sourceNormals.SampleLevel( LinearClamp, fPixelPosition / dimensions, 0 );
		float3	Tangent, BiTangent;
		BuildOrthonormalBasis( Normal, Tangent, BiTangent );

		rayDirection = rayDirection.x * Tangent + rayDirection.y * BiTangent + rayDirection.z * Normal;

//rayPosition.z += 1e-2 * _Displacement_mm;			// Nudge a little to avoid acnea
rayPosition += 1e-2 * (_Displacement_mm / _TexelSize_mm) * Normal;	// Nudge a little to avoid acnea

		gs_occlusion[rayIndex] = ComputeDirectionalOcclusion( pixelPosition, dimensions, rayPosition, rayDirection );

//gs_occlusion[rayIndex] = Normal.y;
	} else {
		// Clear remaining rays so they don't interfere with the accumulation
		gs_occlusion[rayIndex] = 0.0;
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex == 0 ) {
		float	result = 0.0;
		for ( uint i=0; i < _RaysCount; i++ )
			result += gs_occlusion[i];
		result /= _RaysCount;

// Shows bilateral filtered source
//result = _source.Load( int3( pixelPosition, 0 ) ).x;

		_target[pixelPosition] = result;
	}
}