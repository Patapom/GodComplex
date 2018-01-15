////////////////////////////////////////////////////////////////////////////////
// Computes the Ground Truth image
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

static const uint	MAX_THREADS = 1024;

cbuffer	CBInput : register( b0 ) {
	uint2	_textureDimensions;	// Texture dimensions
	uint	_Y0;				// Start scanline for this group
	uint	_raysCount;			// Amount of rays to cast

	float	_texelSize_mm;		// Texel size in millimeters
	float	_displacement_mm;	// Height map max encoded displacement in millimeters

	float3	_rho;				// Reflectance
}

Texture2D<float4>			_sourceIlluminance : register( t0 );
RWTexture2D<float4>			_targetIlluminance : register( u0 );

//Texture2D<float>			_sourceHeight : register( t1 );
Texture2D<float3>			_sourceNormals : register( t2 );
StructuredBuffer<uint>		_indirectPixelsStack : register( t3 );
StructuredBuffer<float3>	_rays : register( t4 );

groupshared float3x3		gs_Local2World;
groupshared uint3			gs_accumulator = 0;


[numthreads( MAX_THREADS, 1, 1 )]
void	CS_Direct( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID ) {
	uint2	pixelPosition = uint2( _groupID.x, _Y0 + _groupID.y );

	// Only thread 0 will read the texture information
	uint	rayIndex = _groupThreadID.x;
	if ( rayIndex == 0 ) {
		gs_Local2World[2] = _sourceNormals[pixelPosition];
		BuildOrthonormalBasis( gs_Local2World[2], gs_Local2World[0], gs_Local2World[1] );
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < _raysCount ) {
		uint	packedNeighborPixelPosition = _indirectPixelsStack[_raysCount * (_textureDimensions.x * _groupID.y + _groupID.x) + rayIndex];
		if ( packedNeighborPixelPosition == ~0U ) {
			// Direct visibility
			float3	lsRayDirection = _rays[rayIndex];
			float3	wsRayDirection = mul( lsRayDirection, gs_Local2World );
			float3	incomingRadiance = EvaluateSHRadiance( wsRayDirection, _SH ).xyz;
					incomingRadiance *= lsRayDirection.z;	// L(x,Wi) * (N.Wi)

			InterlockedAdd( gs_accumulator.x, uint( 65536.0 * incomingRadiance.x ) );
			InterlockedAdd( gs_accumulator.y, uint( 65536.0 * incomingRadiance.y ) );
			InterlockedAdd( gs_accumulator.z, uint( 65536.0 * incomingRadiance.z ) );
		}
	}

	GroupMemoryBarrierWithGroupSync();

	// Only thread 0 writes the result
	if ( rayIndex == 0 ) {
		float	normalizer = 2.0 * PI		// This factor is here because are actually integrating over the entire hemisphere of directions
											//	and we only accounted for cosine-weighted distribution along theta, we need to account for phi as well!
							/ _raysCount;

		_targetIlluminance[pixelPosition] = float4( normalizer * gs_accumulator / 65536.0, 0.0 );	// We're storing direct IRRADIANCE E0
	}
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Compute reflecting radiance
//
[numthreads( MAX_THREADS, 1, 1 )]
void	CS_Indirect( uint3 _groupID : SV_groupID, uint3 _groupThreadID : SV_groupThreadID ) {
	uint2	pixelPosition = uint2( _groupID.x, _Y0 + _groupID.y );

	// Only thread 0 will read the texture information
	uint	rayIndex = _groupThreadID.x;
	if ( rayIndex == 0 ) {
		gs_Local2World[2] = _sourceNormals[pixelPosition];
		BuildOrthonormalBasis( gs_Local2World[2], gs_Local2World[0], gs_Local2World[1] );
	}

	GroupMemoryBarrierWithGroupSync();

	if ( rayIndex < _raysCount ) {
		uint	packedNeighborPixelPosition = _indirectPixelsStack[_raysCount * (_textureDimensions.x * _groupID.y + _groupID.x) + rayIndex];
		if ( packedNeighborPixelPosition != ~0U ) {
			// Retrieve indirect sample's position
			uint2	neighborPixelPosition = uint2( packedNeighborPixelPosition & 0xFFFFU, packedNeighborPixelPosition >> 16 );
			uint2	tiledNeighborPixelPosition = neighborPixelPosition % _textureDimensions;

			// Sample neighbor's illuminance and compute incoming bounced luminance
			float3	neighborIrradiance = _sourceIlluminance[tiledNeighborPixelPosition].xyz;	// Ei-1 from previous bounce
			float3	incomingRadiance = (_rho / PI) * neighborIrradiance;						// Li-1

			float3	lsRayDirection = _rays[rayIndex];
			incomingRadiance *= lsRayDirection.z;	// L(x,Wi) * (N.Wi)

			InterlockedAdd( gs_accumulator.x, uint( 65536.0 * incomingRadiance.x ) );
			InterlockedAdd( gs_accumulator.y, uint( 65536.0 * incomingRadiance.y ) );
			InterlockedAdd( gs_accumulator.z, uint( 65536.0 * incomingRadiance.z ) );
		}
	}

	GroupMemoryBarrierWithGroupSync();

	// Only thread 0 writes the result
	if ( rayIndex == 0 ) {
		float	normalizer = 2.0 * PI		// This factor is here because are actually integrating over the entire hemisphere of directions
											//	and we only accounted for cosine-weighted distribution along theta, we need to account for phi as well!
							/ _raysCount;

		_targetIlluminance[pixelPosition] = float4( normalizer * gs_accumulator / 65536.0, 0.0 );	// We're storing indirect IRRADIANCE Ei
	}
}