////////////////////////////////////////////////////////////////////////////////
// AOMap generator
// This compute shader will generate the ambient occlusion over a specific texel and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////
//
static const uint	MAX_THREADS = 1024;

static const float	PI = 3.1415926535897932384626433832795;

SamplerState LinearClamp	: register( s0 );

cbuffer	CBInput : register( b0 ) {
	uint2	_textureDimensions;
	uint	_Y0;				// Start scanline for this group
	uint	_raysCount;			// Amount of rays to cast
	uint	_maxStepsCount;		// Maximum amount of steps to take before stopping
	bool	_tile;				// Tiling flag
	float	_texelSize_mm;		// Texel size in millimeters
	float	_displacement_mm;	// Height map max encoded displacement in millimeters
}

Texture2D<float>			_Tex_Height : register( t0 );
RWTexture2D<float>			_Target : register( u0 );

StructuredBuffer<float3>	_Rays : register( t1 );

Texture2D<float3>			_Tex_Normal : register( t2 );

groupshared float3			gs_position;				// Initial position, common to all rays
groupshared float3x3		gs_local2World;
groupshared uint			gs_occlusionAccumulator = 0;


// Computes the occlusion of the pixel in the specified direction
//	_position, position of the ray in the texture (XY = pixel position offset by 0.5, Z = initial height)
//	_direction, direction of the ray
//
// Returns an occlusion value in [0,1] where 0 is completely occluded and 1 completely visible
//
float	ComputeDirectionalOcclusion( float3 _position, float3 _direction ) {

	#if 1
		// Scale the ray so we ensure to always walk at least a texel in the texture
		_direction *= 1.0 / sqrt( 1.0 - _direction.z * _direction.z );
	#endif

	// Scale the vertical step so we're the proper size
	_direction.z *= _texelSize_mm / max( 1e-4, _displacement_mm );

	float	occlusion = 1.0;	// Start unoccluded
	for ( uint stepIndex=0; stepIndex < _maxStepsCount; stepIndex++ ) {
		_position += _direction;	
		if ( _position.z >= 1.0 )
			break;		// Definitely escaped the surface!
		if ( _position.z < 0.0 )
			return 0.0;	// Definitely occluded!

		if ( _tile ) {
			_position.xy = fmod( _position.xy + _textureDimensions, _textureDimensions );
		} else {
			if (	_position.x < 0 || _position.x >= float(_textureDimensions.x)
				||	_position.y < 0 || _position.y >= float(_textureDimensions.y) )
				break;
		}

//		float	H = _Tex_Height.Load( int3( _position.xy, 0 ) );
		float	H = _Tex_Height.SampleLevel( LinearClamp, _position.xy / _textureDimensions, 0 );

		#if 1
			// Simple test for a fully extruded height
			if ( _position.z < H )
				return 0.0;
		#else
			// Assume a height interval
			float	Hmax = H;
			float	Hmin = H - 0.01;
			if ( _position.z > Hmin && _position.z < Hmax )
				return 0.0;
		#endif
	}

	return occlusion;
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
	if ( rayIndex == 0 ) {
		// Build local tangent space to orient rays
		float3	normal = _Tex_Normal[pixelPosition];
		float3	tangent, biTangent;
		BuildOrthonormalBasis( normal, tangent, biTangent );

		gs_local2World[0] = tangent;
		gs_local2World[1] = biTangent;
		gs_local2World[2] = normal;

		float	H0 = _Tex_Height[pixelPosition];
		gs_position = float3( pixelPosition + 0.5, H0 );

//		float3	RayPosition_mm = rayPosition * float3( _texelSize_mm.xx, _displacement_mm );
//		RayPosition_mm += 0.01 * _displacement_mm * normal;	// Nudge a little to avoid acnea
//		gs_position = RayPosition_mm / float3( _texelSize_mm.xx, _displacement_mm );

		gs_position.z += 1e-2;	// Nudge a little to avoid acnea
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < _raysCount ) {
		float3	lsRayDirection = _Rays[rayIndex];
		float3	wsRayDirection = mul( lsRayDirection, gs_local2World );
		float	occlusion = ComputeDirectionalOcclusion( gs_position, wsRayDirection );

		uint	dontCare;
		InterlockedAdd( gs_occlusionAccumulator, uint(65536.0 * occlusion), dontCare );
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex == 0 ) {
		_Target[pixelPosition] = gs_occlusionAccumulator / _raysCount / 65536.0;
	}
}
