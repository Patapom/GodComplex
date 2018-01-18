////////////////////////////////////////////////////////////////////////////////
// Bent Cone Map generator
// This compute shader will generate the bent normal/bent cone over a specific texel and store the result into a target UAV
//
// This is the "optimized" version of the other algorithm
// It's optimized in the sense it doesn't deal with the entire hemisphere of directions but rather a disk
//	subdivided into multiple slices and for each slice we compute the forward & backward horizon (like HBAO)
// Then we approximate the average "bent angle" which is not exactly the half angle between the 2 horizons
//	but a cosine-weighted average of the vectors instead.
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
//groupshared float3			gs_lsRayDirection[MAX_THREADS];
//groupshared float3			gs_wsRayDirection[MAX_THREADS];
//groupshared float			gs_occlusion[MAX_THREADS];
//groupshared float3			gs_bentNormal;				// Computed direction for the bent normal
//groupshared uint			gs_maxCosAngle[ANGLE_BINS_COUNT];


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
		gs_ssCenterPosition.z = (_displacement_mm / _texelSize_mm) * _Tex_Height.SampleLevel( LinearClamp, gs_ssCenterPosition.xy / _textureDimensions, 0 );
	}

//	if ( rayIndex < ANGLE_BINS_COUNT ) {
//		gs_maxCosAngle[rayIndex] = 0;	// Initialize to 0 since we're looking for the maximum of the cos(angle) here
//	}

	GroupMemoryBarrierWithGroupSync();

	////////////////////////////////////////////////////////////////////////
	// Average unoccluded directions to obtain main "bent normal" direction
	if ( rayIndex < _raysCount ) {
		float2	tsDirection;
		sincos( PI * rayIndex / _raysCount, tsDirection.y, tsDirection.x );

		float2	ssDirection = normalize( tsDirection.x * gs_local2World[0].xy + tsDirection.y * gs_local2World[1].xy );

		float	heightFactor = _displacement_mm / _texelSize_mm;

		// March many pixels around central position in the slice's direction and find the horizons
		float3	ssPosition_Front = float3( pixelPosition + 0.5, 0.0 );
		float3	ssPosition_Back = float3( pixelPosition + 0.5, 0.0 );
		float	maxCos_Front = 0.0;
		float	maxCos_Back = 0.0;
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
			float3	tsPosition_Front = float3( dot( ssDeltaPosition_Front, gs_local2World[0] ), dot( ssDeltaPosition_Front, gs_local2World[1] ), dot( ssDeltaPosition_Front, gs_local2World[2] ) );

//tsPosition_Front.z *= 4.0;

			maxCos_Front = max( maxCos_Front, tsPosition_Front.z / length( tsPosition_Front ) );

			ssPosition_Back.z = _Tex_Height.SampleLevel( LinearClamp, ssPosition_Back.xy / _textureDimensions, 0 );
			ssPosition_Back.z *= heightFactor;	// Correct against aspect ratio
			float3	ssDeltaPosition_Back = ssPosition_Back - gs_ssCenterPosition;
			float3	tsPosition_Back = float3( dot( ssDeltaPosition_Back, gs_local2World[0] ), dot( ssDeltaPosition_Back, gs_local2World[1] ), dot( ssDeltaPosition_Back, gs_local2World[2] ) );

//tsPosition_Back.z *= 4.0;

			maxCos_Back = max( maxCos_Back, tsPosition_Back.z / length( tsPosition_Back ) );
		}

	#if 0
		// Perform integration of the cosine-weighted unoccluded directions between the 2 horizon angles
//		float	sinTheta_Front = sqrt( 1.0 - maxCos_Front*maxCos_Front );
//		float	sinTheta_Back = sqrt( 1.0 - maxCos_Back*maxCos_Back );
//		float	sinFirstMoment = (sinTheta_Front*sinTheta_Front*sinTheta_Front - sinTheta_Back*sinTheta_Back*sinTheta_Back) / 3.0;

		float	sinFirstMoment = 0.5 * (maxCos_Back*maxCos_Back - maxCos_Front*maxCos_Front);

//		float	sinFirstMoment = maxCos_Back - maxCos_Front;

		float3	tsBentNormal = float3( sinFirstMoment * tsDirection, sqrt( 1.0 - sinFirstMoment*sinFirstMoment ) );

		float	sumWeights = 0;

	#elif 0
		// EQUI-ANGULAR DISTRIBUTION IS WRONG => Doesn't account for solid angle at the top of the hemi-circle that is reduced
		// Half brute force where we perform the integration numerically as a sum...
		float	thetaFront = 0.5*PI - acos( saturate( maxCos_Front ) );
		float	thetaBack = 0.5*PI + acos( saturate( maxCos_Back ) );

		float3	tsBentNormal = 0.0;
		for ( uint i=0; i < 256; i++ ) {
			float	theta = lerp( thetaFront, thetaBack, (i+0.5) / 256.0 );
			float2	scTheta;
			sincos( theta, scTheta.x, scTheta.y );
			float3	tsUnOccludedDirection = float3( scTheta.y * tsDirection, scTheta.x );
			float	weight = tsUnOccludedDirection.z;
			tsBentNormal += weight * tsUnOccludedDirection;
		}
		tsBentNormal /= 128.0;

//ssBentNormal = float3( ssDirection, 0 );
//ssBentNormal = gs_local2World[2];
//ssBentNormal = float3( 1.0 * maxCos_Front, 0, 0 );

		float	sumWeights = 0;

	#elif 1
		// Half brute force where we perform the integration numerically as a sum...
		float3	tsBentNormal = 0.0;
		float	sumWeights = 0.0;
		for ( uint i=0; i < 256; i++ ) {
			float	cosTheta = (i+0.5) / 256.0;
			float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );

			// Accumulate front
			float3	tsUnOccludedDirection = float3( sinTheta * tsDirection, cosTheta );
			float	weight = (cosTheta > maxCos_Front ? 1.0 : 0.0) * cosTheta;
			tsBentNormal += weight * tsUnOccludedDirection;
			sumWeights += weight;

			// Accumulate back
			tsUnOccludedDirection = float3( -sinTheta * tsDirection, cosTheta );
			weight = (cosTheta > maxCos_Back ? 1.0 : 0.0) * cosTheta;
			tsBentNormal += weight * tsUnOccludedDirection;
			sumWeights += weight;
		}
		tsBentNormal /= 256.0;
		sumWeights /= 256.0;
//		tsBentNormal /= 32.0;

//tsBentNormal = float3( 1, 0, 0 );

	#else
		// Half brute force where we perform the integration numerically as a sum...
		float3	wsBentNormal = 0.0;
		float	sumWeights = 0.0;
		for ( uint i=0; i < 256; i++ ) {
			float	cosTheta = (i+0.5) / 256.0;
			float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );

			// Accumulate front
			float3	tsUnOccludedDirection = float3( sinTheta * tsDirection, cosTheta );
			float	weight = (cosTheta > maxCos_Front ? 1.0 : 0.0) * cosTheta;
			float3	wsDirection = mul( tsUnOccludedDirection, gs_local2World );
			wsBentNormal += weight * wsDirection;
			sumWeights += weight;
//sumWeights += 1;

			// Accumulate back
			tsUnOccludedDirection = float3( -sinTheta * tsDirection, cosTheta );
//			tsUnOccludedDirection.xy *= -1.0;
			weight = (cosTheta > maxCos_Back ? 1.0 : 0.0) * cosTheta;
			wsDirection = mul( tsUnOccludedDirection, gs_local2World );
			wsBentNormal += weight * wsDirection;
			sumWeights += weight;
//sumWeights += 1;
		}
		wsBentNormal /= 256.0;
		sumWeights /= 256.0;

	#endif

		uint	dontCare;
		InterlockedAdd( gs_occlusionDirectionAccumulator.x, uint(65536.0 * (1.0 + tsBentNormal.x)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.y, uint(65536.0 * (1.0 + tsBentNormal.y)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.z, uint(65536.0 * (1.0 + tsBentNormal.z)), dontCare );
//		InterlockedAdd( gs_occlusionDirectionAccumulator.x, uint(65536.0 * (1.0 + wsBentNormal.x)), dontCare );
//		InterlockedAdd( gs_occlusionDirectionAccumulator.y, uint(65536.0 * (1.0 + wsBentNormal.y)), dontCare );
//		InterlockedAdd( gs_occlusionDirectionAccumulator.z, uint(65536.0 * (1.0 + wsBentNormal.z)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.w, uint(65536.0 * sumWeights), dontCare );
	}

	GroupMemoryBarrierWithGroupSync();

/*	////////////////////////////////////////////////////////////////////////
	// Finalize average bent normal direction (unnormalized)
	if ( rayIndex == 0 ) {
		gs_bentNormal = float3( gs_occlusionDirectionAccumulator.xyz ) / gs_occlusionDirectionAccumulator.w - 1.0;
		gs_occlusionDirectionAccumulator = 0;

		// Rebuild tangent space around the new bent normal
		gs_local2World[2] = gs_bentNormal;
		BuildOrthonormalBasis( gs_bentNormal, gs_local2World[0], gs_local2World[1] );
	}

	GroupMemoryBarrierWithGroupSync();

	////////////////////////////////////////////////////////////////////////
	// Compute average cone aperture angle
	if ( rayIndex < _raysCount ) {
		float3	wsRayDirection = gs_wsRayDirection[rayIndex];
		float	occlusion = gs_occlusion[rayIndex];


		#if 1
			float3	lsRayDirection = float3(	dot( wsRayDirection, gs_local2World[0] ),
												dot( wsRayDirection, gs_local2World[1] ),
												dot( wsRayDirection, gs_local2World[2] )
											);

			uint	angleBinIndex = uint( ANGLE_BINS_COUNT * (1.0 + atan2( lsRayDirection.y, lsRayDirection.x ) / PI) ) & (ANGLE_BINS_COUNT-1);

			// Keep maximum cos( angle ) for each bin
			uint	dontCare;
			InterlockedMax( gs_maxCosAngle[angleBinIndex], uint( 65536.0 * lerp( lsRayDirection.z, 0.0, occlusion ) ), dontCare );
		#else
			float3	lsRayDirection = gs_lsRayDirection[rayIndex];

//			float	weight = occlusion * lsRayDirection.z;
			float	weight = occlusion;

			uint	dontCare;
			InterlockedAdd( gs_occlusionDirectionAccumulator.x, uint(65536.0 * weight * dot( wsRayDirection, gs_bentNormal )), dontCare );
			InterlockedAdd( gs_occlusionDirectionAccumulator.w, uint(65536.0 * weight), dontCare );
		#endif
	}

	GroupMemoryBarrierWithGroupSync();
	*/

	////////////////////////////////////////////////////////////////////////
	// Finalize average bent normal direction (unnormalized)
	if ( rayIndex == 0 ) {
//		float	averageCos = float(gs_occlusionDirectionAccumulator.x) / gs_occlusionDirectionAccumulator.w;

//		float	averageAngle = 0.0;
//		for ( uint angleBinIndex=0; angleBinIndex < ANGLE_BINS_COUNT; angleBinIndex++ )
//			averageAngle += acos( saturate( gs_maxCosAngle[angleBinIndex] / 65536.0 ) );
//		averageAngle /= ANGLE_BINS_COUNT;
//		float	averageCos = cos( averageAngle );
//
//averageCos = 2.0 * averageAngle / PI;	// Store as angle, makes more sense visually and brings more precision

		float3	tsBentNormal = (gs_occlusionDirectionAccumulator.xyz - 65536.0 * _raysCount) / 65536.0;
//		float3	tsBentNormal = (float3( gs_occlusionDirectionAccumulator.xyz ) - _raysCount * 65536.0) / gs_occlusionDirectionAccumulator.w;

tsBentNormal = normalize( tsBentNormal );

		float3	ssBentNormal = tsBentNormal.x * gs_local2World[0] + tsBentNormal.y * gs_local2World[1] + tsBentNormal.z * gs_local2World[2];
//float3	ssBentNormal = tsBentNormal;

				ssBentNormal.y = -ssBentNormal.y;	// Normal textures are stored with inverted Y

//ssBentNormal = normalize( ssBentNormal );

		_Target[pixelPosition] = float4( 0.5 * (1.0+ssBentNormal), 1.0 );	// Ready for texture
//_Target[pixelPosition] = float4( 0.5 * (1.0+gs_local2World[2]), 1 );	// Ready for texture
//_Target[pixelPosition] = float4( dot( normalize(bentNormal), gs_local2World[2] ).xxx, 1 );
//_Target[pixelPosition] = float4( _raysCount.xxx / 2048.0, 1 );
//_Target[pixelPosition] = float4( float2( pixelPosition ) / _textureDimensions, 0, 1 );
	}
}