////////////////////////////////////////////////////////////////////////////////
// AOMap generator
// This compute shader will generate the ambient occlusion over a specific texel and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////
//
static const uint	MAX_THREADS = 1024;

static const float	PI = 3.1415926535897932384626433832795;
static const float	INFINITY = 1e6;

SamplerState LinearClamp	: register( s0 );

cbuffer	CBInput : register( b0 ) {
	uint2	_textureDimensions;	// Texture dimensions
	uint	_Y0;				// Start scanline for this group
	uint	_raysCount;			// Amount of rays to cast
	uint	_maxStepsCount;		// Maximum amount of steps to take before stopping
	bool	_tile;				// Tiling flag
	float	_texelSize_mm;		// Texel size in millimeters
	float	_displacement_mm;	// Height map max encoded displacement in millimeters
}

Texture2D<float>			_sourceHeights : register( t0 );
Texture2D<float3>			_sourceNormals : register( t1 );
StructuredBuffer<float3>	_rays : register( t2 );

RWTexture2D<float2>			_targetAO : register( u0 );
RWStructuredBuffer<uint>	_indirectPixels : register( u1 );

groupshared uint			gs_accumulatorAO = 0;	// AO accumulator where integral of the visibility term is done (without the cosine-weighting!)
groupshared uint			gs_accumulatorE0 = 0;	// Direct illuminance accumulator where integral of the visibility term times the cosine term is done

uint	PackPixelPosition( float2 _position ) {
	uint2	intPosition = uint2( floor( _position ) );
	return ((intPosition.y & 0xFFFFU) << 16) | (intPosition.x & 0xFFFFU);
}

// Computes the occlusion of the pixel in the specified direction
//	_position_mm, position of the ray in the "world" (i.e. in actual millimeters)
//	_direction, direction of the ray
//
// Returns an occlusion value in [0,1] where 0 is completely occluded and 1 completely visible
//
float	ComputeSurfaceHit( float3 _position_mm, float3 _direction ) {

	float3	startPosition_mm = _position_mm;

	// Scale the ray so we ensure to always walk at least a texel in the texture
	_direction *= _direction.z < 1.0 ? 1.0 / sqrt( 1.0 - _direction.z * _direction.z ) : 1.0;	// Ensure we always walk a single millimeter
	_direction *= _texelSize_mm;																// Scale by the amount of millimeters to walk a whole pixel

	float	prevH_mm = _position_mm.z;
	for ( uint stepIndex=0; stepIndex < _maxStepsCount; stepIndex++ ) {
		_position_mm += _direction;	
		if ( _position_mm.z >= _displacement_mm )
			break;		// Definitely escaped the surface!

		float2	UV = _position_mm.xy / (_textureDimensions * _texelSize_mm);
		if ( _tile ) {
			UV = frac( UV );
		} else {
			if ( any( UV < 0.0 ) || any( UV >= 1.0 ) )
				break;	// Escaped the texture...
		}

		float	H_mm = _displacement_mm * _sourceHeights.SampleLevel( LinearClamp, UV, 0 );

		// Simple test for a fully extruded height
		if ( _position_mm.z < H_mm ) {
			// Compute actual hit position
			float	t = (H_mm - _position_mm.z) / (2.0*_position_mm.z - _direction.z + H_mm - prevH_mm);
			_position_mm -= t * _direction;

			return length( _position_mm - startPosition_mm );
		}

		prevH_mm = H_mm;
	}

	return INFINITY;
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
	if ( rayIndex < _raysCount ) {

		// Build local tangent space to re-orient ray
		float3	normal = _sourceNormals[pixelPosition];
		float3	tangent, biTangent;
		BuildOrthonormalBasis( normal, tangent, biTangent );

		// Build ray position & direction
		float	H0_mm = _displacement_mm * _sourceHeights[pixelPosition];

		float3	lsRayDirection = _rays[rayIndex];
		float3	rayDirection = lsRayDirection.x * tangent + lsRayDirection.y * biTangent + lsRayDirection.z * normal;

		float3	rayPosition_mm  = float3( _texelSize_mm * (0.5 + pixelPosition), H0_mm );

		// Nudge a little to avoid acnea
//		rayPosition_mm   += 1e-2 * _displacement_mm * normal;	// This may push inside the surface in deep cavities
		rayPosition_mm.z += 1e-2 * _displacement_mm;			// Use vertical nudge instead!

		// Compute hit position
		float	hitDistance_mm = ComputeSurfaceHit( rayPosition_mm, rayDirection );

		uint	packedPixelPosition = ~0U;
		if ( hitDistance_mm < 1e4 ) {
			// Store link to indirect bounce pixel
			float3	hitPosition_mm = rayPosition_mm + hitDistance_mm * rayDirection;
			float2	hitPosition_pixels = hitPosition_mm.xy / _texelSize_mm;
			packedPixelPosition = PackPixelPosition( _textureDimensions + hitPosition_pixels );	// Offset with texture dimension so pixel index is always positive
		} else {
			// Accumulate unit luminance with cos(theta)
			uint	dontCare;
			InterlockedAdd( gs_accumulatorAO, uint( 65536.0 * 1.0 ), dontCare );				// Without cosine-weighting ==> AO output
			InterlockedAdd( gs_accumulatorE0, uint( 65536.0 * lsRayDirection.z ), dontCare );	// Cosine-weighting ==> Unit luminance
		}


// Verify summing cos(theta) yields 0.5 as expected
//uint	dontCare;
//InterlockedAdd( gs_accumulator, uint( 1024.0 * lsRayDirection.z ), dontCare );


		_indirectPixels[_raysCount * (_textureDimensions.x * _GroupID.y + _GroupID.x) + rayIndex] = packedPixelPosition;
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex == 0 )
		_targetAO[pixelPosition] = 2.0 * PI * float2( gs_accumulatorAO, gs_accumulatorE0 ) / (65536.0 * _raysCount);	// Store AO and direct illuminance terms
}