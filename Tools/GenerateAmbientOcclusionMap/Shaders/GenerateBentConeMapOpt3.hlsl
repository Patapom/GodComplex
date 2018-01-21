////////////////////////////////////////////////////////////////////////////////
// Bent Cone Map generator
// This compute shader will generate the bent normal/bent cone over a specific texel and store the result into a target UAV
//
// This is the "optimized" version of the other algorithm
// It's optimized in the sense it doesn't deal with the entire hemisphere of directions but rather a disk
//	subdivided into multiple slices and for each slice we compute the forward & backward horizon (like HBAO)
// Then we approximate the average "bent angle" which is not exactly the half angle between the 2 horizons
//	but a weighted average of the vectors instead.
//
// Our main problem comes from the fact that the vectors should be weighted by the solid angle expressed in the tangent space given by the normal
//	whereas we have no choice than to samples positions along a disc expressed in screen space (it's mandatory otherwise if we do our computations
//	in tangent space then we MUST ray-trace the height field from the tangent space's plane to find where we intersect the height field below,
//	and that would lose all interest as opposed to the brute force method that ray-casts lots of rays).
//
// 
//
//
//
//
//
//
//
// Technically, the brute force algorithm just does:
//		bentNormal = 1/N * Sum{i=1,N}[ (1-V(Wi)).(Wi.N).Wi ]
//
// Meaning we only average the unoccluded directions Wi, weighted by the cosine with the normal (so orthogonal directions have more weight than grazing ones)
// It works without bothering with the solid angle because all rays Wi have equal solid angle 2PI/N
//
// But we could simply find the maximum angle spanned to reach the 2 horizons (front and back)
//	and compute the actual integration of the directions between the 2 horizons, weighted by the cosine with the normal:
//
//		firstMomentBentNormal = Integral{theta0,theta1}[ sin(theta).cos(theta).sin(theta).dtheta ]
//
// Where:
//	• theta0 and theta1 are the front and back horizon angles in [-PI/2,+PI/2], always measured from the normal's direction
//	• The first sin(theta) represents the horizontal deviation of the Wi vector we're summing
//	• The first cos(theta) represents the "cosine weight" of the vector
//	• The second sin(theta) represents the solid angle of the spherical sector sin(theta).dphi reducing with elevation
//
//		firstMomentBentNormal = [1/3 sin^3(theta)]{theta0,theta1}
//
// The angle of the bent normal is then retrieved by computing the arcsine of that value.
//
//		                   ^ N                      
//  Negative Horizon \     |     . AverageN       
//		              \    |    .             __ Positive Horizon     
//		               \   |   .          __        
//		                \  |  .       __            
//		                 \ | .    __                  
//		                  \|. __                     
//		-------------------+----------------------
//
// Computing this for many slices of the hemisphere should yield the same (even better!) bent normal
//	as with the brute force method while requiring even less rays to shoot...
//
////////////////////////////////////////////////////////////////////////////////
//
static const uint	MAX_THREADS = 1024;
static const uint	ANGLE_BINS_COUNT = 16;

static const float	PI = 3.1415926535897932384626433832795;

static const uint	STEP_SIZE = 1;

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
RWTexture2D<float4>			_Target : register( u0 );

StructuredBuffer<float3>	_Rays : register( t1 );

Texture2D<float3>			_Tex_Normal : register( t2 );

groupshared float3			gs_ssCenterPosition;		// Initial position, common to all rays
groupshared float3x3		gs_local2World;
groupshared uint4			gs_occlusionDirectionAccumulator = 0;


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

		gs_ssCenterPosition = float3( pixelPosition + 0.5, (_displacement_mm / _texelSize_mm) * _Tex_Height[pixelPosition] );
	}

	GroupMemoryBarrierWithGroupSync();

	////////////////////////////////////////////////////////////////////////
	// Average unoccluded directions to obtain main "bent normal" direction
	if ( rayIndex < _raysCount ) {
		float2	ssDirection;
		sincos( PI * rayIndex / _raysCount, ssDirection.y, ssDirection.x );

		float	heightFactor = _displacement_mm / _texelSize_mm;

		// March many pixels around central position in the slice's direction and find the horizons
		float3	ssPosition_Front = float3( pixelPosition + 0.5, 0.0 );
		float3	ssPosition_Back = float3( pixelPosition + 0.5, 0.0 );
		float	maxCos_Front = -1.0;
		float	maxCos_Back = -1.0;
		for ( uint radius=1; radius <= _maxStepsCount; radius++ ) {
			ssPosition_Front.xy += ssDirection;
			ssPosition_Back.xy -= ssDirection;
			if ( _tile ) {
				ssPosition_Front.xy = fmod( ssPosition_Front.xy + _textureDimensions, _textureDimensions );
				ssPosition_Back.xy = fmod( ssPosition_Back.xy + _textureDimensions, _textureDimensions );
			}

			// Project height difference in tangent space and update horizon angles
			ssPosition_Front.z = _Tex_Height.SampleLevel( LinearClamp, ssPosition_Front.xy / _textureDimensions, 0 );
			ssPosition_Front.z *= heightFactor;	// Correct against aspect ratio
			float3	ssDeltaPosition_Front = ssPosition_Front - gs_ssCenterPosition;
			float	cos_Front = ssDeltaPosition_Front.z / sqrt( radius*radius + ssDeltaPosition_Front.z * ssDeltaPosition_Front.z );
			maxCos_Front = max( maxCos_Front, cos_Front );

			ssPosition_Back.z = _Tex_Height.SampleLevel( LinearClamp, ssPosition_Back.xy / _textureDimensions, 0 );
			ssPosition_Back.z *= heightFactor;	// Correct against aspect ratio
			float3	ssDeltaPosition_Back = ssPosition_Back - gs_ssCenterPosition;
			float	cos_Back = ssDeltaPosition_Back.z / sqrt( radius*radius + ssDeltaPosition_Back.z * ssDeltaPosition_Back.z );
			maxCos_Back = max( maxCos_Back, cos_Back );
		}

		// Half brute force where we perform the integration numerically as a sum...
		// EQUI-ANGULAR DISTRIBUTION IS WRONG => Doesn't account for solid angle at the top of the hemi-circle that is reduced
		float	thetaFront = acos( maxCos_Front );
		float	thetaBack = -acos( maxCos_Back );

//thetaFront = 0.0;
//thetaBack = PI;

		float3	tsBentNormal = 0.01 * float3( 0, 0, 1 );
		float	sumWeights = 0.01;
//*
		for ( uint i=0; i < 256; i++ ) {
			float	theta = lerp( thetaFront, thetaBack, (i+0.5) / 256.0 );
			float2	scTheta;
			sincos( theta, scTheta.x, scTheta.y );
			float3	ssUnOccludedDirection = float3( scTheta.x * ssDirection, scTheta.y );

			float3	tsUnOccludedDirection = float3( dot( ssUnOccludedDirection, gs_local2World[0] ), dot( ssUnOccludedDirection, gs_local2World[1] ), dot( ssUnOccludedDirection, gs_local2World[2] ) );

			float	cosTheta = max( 0.0, tsUnOccludedDirection.z );
			float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
			float	weight = cosTheta * sinTheta;	// #TODO: Compute solid angle in tangent space
			tsBentNormal += weight * tsUnOccludedDirection;
			sumWeights += weight;
		}
//*/
//		tsBentNormal /= sumWeights;
//		sumWeights = 1.0;
		tsBentNormal /= 256.0;
		sumWeights /= 256.0;
//		tsBentNormal += float3( 0, 0, 1e-3 );

		uint	dontCare;
		InterlockedAdd( gs_occlusionDirectionAccumulator.x, uint(65536.0 * (1.0 + tsBentNormal.x)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.y, uint(65536.0 * (1.0 + tsBentNormal.y)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.z, uint(65536.0 * (1.0 + tsBentNormal.z)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.w, uint(65536.0 * sumWeights), dontCare );
	}

	GroupMemoryBarrierWithGroupSync();

	////////////////////////////////////////////////////////////////////////
	// Finalize average bent normal direction (unnormalized)
	if ( rayIndex == 0 ) {
		float3	tsBentNormal = (float3( gs_occlusionDirectionAccumulator.xyz ) - _raysCount * 65536.0) / gs_occlusionDirectionAccumulator.w;
//		float	L = length( tsBentNormal );
//		tsBentNormal = L > 1e-3 ? tsBentNormal / L : float3( 0, 0, 1 );
tsBentNormal  = normalize( tsBentNormal );

		float3	ssBentNormal = tsBentNormal.x * gs_local2World[0] + tsBentNormal.y * gs_local2World[1] + tsBentNormal.z * gs_local2World[2];

				ssBentNormal.y = -ssBentNormal.y;	// Normal textures are stored with inverted Y

		_Target[pixelPosition] = float4( 0.5 * (1.0+ssBentNormal), 1.0 );	// Ready for texture
	}
}