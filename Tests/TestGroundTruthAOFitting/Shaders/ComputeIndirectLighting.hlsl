////////////////////////////////////////////////////////////////////////////////
// AOMap generator
// This compute shader will generate the ambient occlusion over a specific texel and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////
//
static const uint	MAX_THREADS = 1024;

static const float	PI = 3.1415926535897932384626433832795;

SamplerState LinearClamp	: register( s0 );

cbuffer	CBInput : register( b0 ) {
	uint2	_textureDimensions;	// Texture dimensions
	uint	_raysCount;			// Amount of rays to cast
	float	_texelSize_mm;		// Texel size in millimeters
	float	_displacement_mm;	// Height map max encoded displacement in millimeters
}

Texture2D<float>			_sourceIlluminance : register( t0 );
RWTexture2D<float>			_targetIlluminance : register( u0 );

Texture2D<float>			_sourceAO : register( t1 );
Texture2D<float>			_sourceHeight : register( t2 );
Texture2D<float3>			_sourceNormals : register( t3 );
StructuredBuffer<uint>		_indirectPixelsStack : register( t4 );


//groupshared uint2			gs_offsetCount;
//groupshared float			gs_sumCosTheta;
groupshared float3			gs_position_mm;
groupshared float3			gs_normal;
groupshared uint			gs_accumulator = 0;


uint	PackPixelPosition( float2 _position ) {
	uint2	intPosition = uint2( floor( _position ) );
	return ((intPosition.y & 0xFFFFU) << 16) | (intPosition.x & 0xFFFFU);
}

// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
void BuildOrthonormalBasis( float3 _normal, out float3 _tangent, out float3 _bitangent ) {
	float	a = _normal.z > -0.9999999 ? 1.0 / (1.0 + _normal.z) : 0.0;
	float	b = -_normal.x * _normal.y * a;

	_tangent = float3( 1.0 - _normal.x*_normal.x*a, b, -_normal.x );
	_bitangent = float3( b, 1.0 - _normal.y*_normal.y*a, -_normal.y );
}

[numthreads( MAX_THREADS, 1, 1 )]
void	CS( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID ) {
	uint2	pixelPosition = _groupID.xy;

	// Only thread 0 will read the texture information
	uint	rayIndex = _groupThreadID.x;
	if ( rayIndex == 0 ) {
		gs_position_mm = float3( _texelSize_mm * pixelPosition, _displacement_mm * _sourceHeight[pixelPosition] );
		gs_normal = _sourceNormals.SampleLevel( LinearClamp, float2( pixelPosition ) / _textureDimensions, 0 );
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < _raysCount ) {
		// Accumulate indirect luminance
		uint	packedNeighborPixelPosition = _indirectPixelsStack[_raysCount * (_textureDimensions.x * _groupID.y + _groupID.x) + rayIndex];
		if ( packedNeighborPixelPosition != ~0U ) {
			// Retrieve indirect sample's position and normal
			uint2	neighborPixelPosition = uint2( packedNeighborPixelPosition & 0xFFFFU, packedNeighborPixelPosition >> 16 );
			uint2	tiledNeighborPixelPosition = neighborPixelPosition % _textureDimensions;

//			float3	neighborNormal = _sourceNormals.SampleLevel( LinearClamp, neighborUV, 0 );
			float3	neighborPosition_mm = float3( _texelSize_mm * neighborPixelPosition, _displacement_mm * _sourceHeight[tiledNeighborPixelPosition] );

			// Compute perceived luminance
			float	neighborAO = _sourceAO[tiledNeighborPixelPosition];
			float	neighborIlluminance = _sourceIlluminance[tiledNeighborPixelPosition];

			#if 0
			// We're approximating the direct lighting perceived by the neighbor position by computing the
			//	unit luminance L0 directly perceived by the neighbor position and weighted by the cos(theta)
			//	through a cone whose aperture angle is given by AO:
			//
			//	E(x) = 2PI * Integral[0,alpha]( L0 cos(theta) sin(theta) dtheta )
			//	alpha = PI/2 * AO the aperture angle of the cone
			//
			// This integral reduces into:
			//	E(x) = L0 * 2PI * (-1/2) * [cos²(alpha) - cos²(0)] = L0 * PI * [1 - cos²(alpha)] = L0 * PI * sin²(alpha)
			//
			// We obtain reflected luminance Li(x,w) as:
			//	Li(x,w) = E(x) * rho/PI
			//	rho = neighbor surface's albedo that we assume to be 1 everywhere and that will be properly weighted on CPU side
			//
			float	sinAlpha = sin( neighborAO * PI / 2.0 );
			float	directNeighborIlluminance = PI * sinAlpha * sinAlpha * neighborIlluminance;	// Assuming a unit luminance in all directions from normal cone
			#else
				// The AO texture directly gives us the cosine-weighted direct illuminance perceived by the neighbor position
				float	directNeighborIlluminance = neighborAO * neighborIlluminance;
			#endif
			float	albedo = 1.0;	// Assuming a unit albedo (will be scaled on CPU by actual albedo)
			float	directNeighborLuminance = (albedo / PI) * directNeighborIlluminance;

			// We then accumulate the neighbor's luminance weighted by the cosine of the angle formed by the incoming ray and normal
			float3	incomingDirection = neighborPosition_mm - gs_position_mm;
			float	distance2Neighbor = length( incomingDirection );
					incomingDirection *= distance2Neighbor > 0.0 ? 1.0 / distance2Neighbor : 0.0;

			float	cosTheta = saturate( dot( incomingDirection, gs_normal ) );
//			float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
//			float	solidAngle = sinTheta * dTheta;	// C'est quoi dTHeta???
			float	solidAngle = 1.0;				// Assume equal solid angle distribution of samples

			float	dontCare;
			InterlockedAdd( gs_accumulator, uint( 1024.0 * directNeighborLuminance * cosTheta * solidAngle ), dontCare );
		}
	}

	GroupMemoryBarrierWithGroupSync();

	// Use thread 0 to accumulate final result
	if ( rayIndex == 0 ) {
//		float	result = 0.0;
//		for ( uint i=0; i < _raysCount.y; i++ )
//			result += gs_accumulator[i];
//		_targetIlluminance[pixelPosition] = result;
		_targetIlluminance[pixelPosition] = gs_accumulator / 1024.0;
	}
}