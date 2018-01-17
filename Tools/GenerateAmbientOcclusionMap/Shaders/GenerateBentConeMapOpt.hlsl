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
//
// But we could simply find the maximum angle spanned to reach the 2 horizons (front and back)
//	and compute the actual integration of the weight of the directions between the 2 horizons,
//	weighted by the cosine with the normal:
//
//		sineBentNormal = Integral{theta0,theta1}[ sin(theta).cos(theta).dtheta ]
//		sineBentNormal = [-1/2 cos²(theta)]{theta0,theta1}
//
// Where theta0 and theta1 are the front and back horizon angles in [-PI/2,+PI/2], always measured from the normal's direction
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
// Computing this for many slices of the hemisphere should yield the same (even better!) bent normal as with the brute force method
//	while requiring even less rays to shoot...
//
////////////////////////////////////////////////////////////////////////////////
//
//#define SCREEN_SPACE	1

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

groupshared float3			gs_position;				// Initial position, common to all rays
groupshared float3x3		gs_local2World;
groupshared float3			gs_lsRayDirection[MAX_THREADS];
groupshared float3			gs_wsRayDirection[MAX_THREADS];
groupshared float			gs_occlusion[MAX_THREADS];
groupshared uint3			gs_occlusionDirectionAccumulator = 0;
groupshared float3			gs_bentNormal;				// Computed direction for the bent normal
groupshared uint			gs_maxCosAngle[ANGLE_BINS_COUNT];


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

// Don't use this because nudging in normal's direction can offset the start position INSIDE the height map. Use vertical nudge instead.
//		float3	RayPosition_mm = rayPosition * float3( _texelSize_mm.xx, _displacement_mm );
//		RayPosition_mm += 0.01 * _displacement_mm * normal;	// Nudge a little to avoid acnea
//		gs_position = RayPosition_mm / float3( _texelSize_mm.xx, _displacement_mm );

//		gs_position.z += 1e-2;	// Nudge a little to avoid acnea
	}

//	if ( rayIndex < ANGLE_BINS_COUNT ) {
//		gs_maxCosAngle[rayIndex] = 0;	// Initialize to 0 since we're looking for the maximum of the cos(angle) here
//	}

	GroupMemoryBarrierWithGroupSync();

	////////////////////////////////////////////////////////////////////////
	// Average unoccluded directions to obtain main "bent normal" direction
	if ( rayIndex < _raysCount ) {
		float2	ssDirection;
		sincos( PI * rayIndex / _raysCount, ssDirection.y, ssDirection.x );

		float	heightFactor = _displacement_mm / _texelSize_mm;

		// Project normal into ssDirection's plane and compute front & back horizon cos(angles)
//		float3	ssOrthoDirection = float3( -ssDirection.y, ssDirection.x, 0.0 );
//		float3	ssReprojectedNormal = normalize( gs_local2World[2] - dot( gs_local2World[2], ssOrthoDirection ) * ssOrthoDirection );
//		float2	lsReprojectedNormal = float2( dot( ssReprojectedNormal.xy, ssDirection ), ssReprojectedNormal.z );
//		float	ssCosThetaMin_Front = -lsReprojectedNormal.x;
//		float	ssCosThetaMin_Back = lsReprojectedNormal.x;
		float	ssCosThetaMin_Front = -1.0;
		float	ssCosThetaMin_Back = -1.0;

		// March many pixels around central position in the slice's direction and find the horizons
		float4	ssPosition_Front = float4( pixelPosition + 0.5, ssCosThetaMin_Front, 1.0 );
		float4	ssPosition_Back = float4( pixelPosition + 0.5, ssCosThetaMin_Back, 1.0 );

		for ( uint radius=1; radius <= _maxStepsCount; radius+=STEP_SIZE ) {
			ssPosition_Front.xy += ssDirection;
			ssPosition_Back.xy -= ssDirection;
			if ( _tile ) {
				ssPosition_Front.xy = fmod( ssPosition_Front.xy + _textureDimensions, _textureDimensions );
				ssPosition_Back.xy = fmod( ssPosition_Back.xy + _textureDimensions, _textureDimensions );
			} else {
				if (	ssPosition_Front.x < 0 || ssPosition_Front.x >= float(_textureDimensions.x)
					||	ssPosition_Front.y < 0 || ssPosition_Front.y >= float(_textureDimensions.y) ) {
					ssPosition_Front.w = 0;	// Cancel any further influence on horizon
				}
				if (	ssPosition_Back.x < 0 || ssPosition_Back.x >= float(_textureDimensions.x)
					||	ssPosition_Back.y < 0 || ssPosition_Back.y >= float(_textureDimensions.y) ) {
					ssPosition_Back.w = 0;	// Cancel any further influence on horizon
				}
			}

			// Read height difference and update horizon angles
			float	deltaH_Front = _Tex_Height.SampleLevel( LinearClamp, ssPosition_Front.xy / _textureDimensions, 0 ) - gs_position.z;
					deltaH_Front *= heightFactor;	// Correct against aspect ratio
			float	cos_Front = deltaH_Front / sqrt(deltaH_Front*deltaH_Front + radius*radius);
			ssPosition_Front.z = max( ssPosition_Front.z, ssPosition_Front.w * cos_Front );

			float	deltaH_Back = _Tex_Height.SampleLevel( LinearClamp, ssPosition_Back.xy / _textureDimensions, 0 ) - gs_position.z;
					deltaH_Back *= heightFactor;	// Correct against aspect ratio
			float	cos_Back = deltaH_Back / sqrt(deltaH_Back*deltaH_Back + radius*radius);
			ssPosition_Back.z = max( ssPosition_Back.z, ssPosition_Back.w * cos_Back );
		}

#if SCREEN_SPACE
		// ===============================================================================
		// SCREEN-SPACE
		// Results are very smooth and don't use normal map, this gives bad results when used at runtime
		// PROBLEM: theta is aligned on screen-space normal (0,0,1) instead of actual normal!
		//
		float	cosTheta0 = ssPosition_Front.z;
		float	cosTheta1 = ssPosition_Back.z;
		#if 1
			// Regular integration
			float	averageCosTheta = 0.5 * (cosTheta1*cosTheta1 - cosTheta0*cosTheta0);
			float3	ssBentNormal = float3( averageCosTheta * ssDirection, sqrt( 1.0 - averageCosTheta*averageCosTheta ) );
		#else
			// Vectors recomposition
			float2	horizonFront = float2( sqrt(1.0-cosTheta0*cosTheta0), cosTheta0 );
			float2	horizonBack = float2( -sqrt(1.0-cosTheta1*cosTheta1), cosTheta1 );
			float2	averageHorizon = horizonFront + horizonBack;
			float	L = length( averageHorizon );
					averageHorizon = L > 0.0 && averageHorizon.y > 0.0 ? averageHorizon / L : float2( 0, 1 ); 
			float3	ssBentNormal = float3( averageHorizon.x * ssDirection, averageHorizon.y );
		#endif
#else

		#if 0
			// Still not the proper result, essentially because I forgot to include the solid angle
			// Problem is: the solid angle of the tangent-space hemisphere is hell of a shithole
			float	ssCosThetaFront = ssPosition_Front.z;
			float	ssCosThetaBack = ssPosition_Back.z;

			float3	ssHorizonFront = float3( sqrt(1.0-ssCosThetaFront*ssCosThetaFront) * ssDirection, ssCosThetaFront );
			float3	ssHorizonBack = float3( -sqrt(1.0-ssCosThetaBack*ssCosThetaBack) * ssDirection, ssCosThetaBack );

			float3	tsHorizonFront = float3( dot( ssHorizonFront, gs_local2World[0] ), dot( ssHorizonFront, gs_local2World[1] ), dot( ssHorizonFront, gs_local2World[2] ) );
			float3	tsHorizonBack = float3( dot( ssHorizonBack, gs_local2World[0] ), dot( ssHorizonBack, gs_local2World[1] ), dot( ssHorizonBack, gs_local2World[2] ) );

			float	tsCosTheta0 = max( 0.0, tsHorizonFront.z );
			float	tsCosTheta1 = max( 0.0, tsHorizonBack.z );
			float	tsAverageSinTheta = tsCosTheta1 - tsCosTheta0;

			float3	tsDirection = float3( dot( ssDirection, gs_local2World[0].xy ), dot( ssDirection, gs_local2World[1].xy ), dot( ssDirection, gs_local2World[2].xy ) );
			float3	tsBentNormal = tsAverageSinTheta * tsDirection + sqrt( 1.0 - tsAverageSinTheta*tsAverageSinTheta ) * float3( 0, 0, 1 );
			float3	ssBentNormal = tsBentNormal.x * gs_local2World[0] + tsBentNormal.y * gs_local2World[1] + tsBentNormal.z * gs_local2World[2];

		#else
			// Half brute force where we perform the integration numerically as a sum...
			float	thetaBack = 0.5*PI + acos( ssPosition_Back.z );
			float	thetaFront = 0.5*PI - acos( ssPosition_Front.z );
			float3	ssBentNormal = 0.0;
			for ( uint i=0; i < 256; i++ ) {
				float	theta = lerp( thetaFront, thetaBack, (i+0.5) / 256.0 );
				float2	scTheta;
				sincos( theta, scTheta.x, scTheta.y );
				float3	ssUnOccludedDirection = float3( scTheta.y * ssDirection, scTheta.x );
				float	weight = saturate( dot( ssUnOccludedDirection, gs_local2World[2] ) );
				ssBentNormal += weight * ssUnOccludedDirection;
			}
			ssBentNormal /= 128.0;
		#endif

/*		// ===============================================================================
		// TANGENT-SPACE
		// Results are sharper and more accurate for real time usage
		//
		#if 1
			float	ssCosThetaFront = ssPosition_Front.z;
			float	ssCosThetaBack = ssPosition_Back.z;

			float3	ssHorizonFront = float3( sqrt(1.0-ssCosThetaFront*ssCosThetaFront) * ssDirection, ssCosThetaFront );
			float3	ssHorizonBack = float3( -sqrt(1.0-ssCosThetaBack*ssCosThetaBack) * ssDirection, ssCosThetaBack );

			float3	tsHorizonFront = float3( dot( ssHorizonFront, gs_local2World[0] ), dot( ssHorizonFront, gs_local2World[1] ), dot( ssHorizonFront, gs_local2World[2] ) );
			float3	tsHorizonBack = float3( dot( ssHorizonBack, gs_local2World[0] ), dot( ssHorizonBack, gs_local2World[1] ), dot( ssHorizonBack, gs_local2World[2] ) );

			#if 1
				// Regular integration
				float3	tsDirection = float3( dot( ssDirection, gs_local2World[0].xy ), dot( ssDirection, gs_local2World[1].xy ), dot( ssDirection, gs_local2World[2].xy ) );
//				float3	tsDirection = normalize( tsHorizonFront );
//				float	tsCosTheta0 = max( 0.0, tsHorizonFront.z );
//				float	tsCosTheta1 = max( 0.0, tsHorizonBack.z );
				float	tsCosTheta0 = tsHorizonFront.z;
				float	tsCosTheta1 = tsHorizonBack.z;
				float	tsAverageSinTheta = 0.5 * (tsCosTheta1*tsCosTheta1 - tsCosTheta0*tsCosTheta0);
				float3	tsBentNormal = tsAverageSinTheta * tsDirection + sqrt( 1.0 - tsAverageSinTheta*tsAverageSinTheta ) * float3( 0, 0, 1 );
			#else
				// Vectors recomposition
				tsHorizonFront.z = max( 0.0, tsHorizonFront.z );	// This is useless since we properly initialized min horizon angles before entering the loop but better be safe than sorry! :D
				tsHorizonBack.z = max( 0.0, tsHorizonBack.z );

//if ( tsHorizonFront.z < -1e-3 || tsHorizonBack.z < -1e-3 ) {
//	// ERROR!
//	tsHorizonFront = float3( 10, 0, 0 );
//	tsHorizonBack = 0.0;
//}

				float3	tsBentNormal = tsHorizonFront + tsHorizonBack + float3( 0, 0, 1e-3 );
				float	L = length( tsBentNormal );
						tsBentNormal = L > 0.0 ? tsBentNormal / L : float3( 0, 0, 1 ); 
			#endif

			float3	ssBentNormal = tsBentNormal.x * gs_local2World[0] + tsBentNormal.y * gs_local2World[1] + tsBentNormal.z * gs_local2World[2];

		#else
			// Reproject normal into the slice's plane & built tangent direction
			float3	ssOrthoDirection = float3( -ssDirection.y, ssDirection.x, 0.0 );
			float3	slicePlaneN = normalize( gs_local2World[2] - dot( gs_local2World[2], ssOrthoDirection ) * ssOrthoDirection );
			float2	ssNormal = float2( dot( slicePlaneN.xy, ssDirection ), slicePlaneN.z );
			float2	ssTangent = float2( ssNormal.y, -ssNormal.x );

			// Transform horizons back into tangent space
			float2	ssHorizon_Front = float2( ssPosition_Front.z, 1.0 - ssPosition_Front.z );
			float2	tsHorizon_Front = float2( dot( ssHorizon_Front, ssTangent ), dot( ssHorizon_Front, ssNormal ) );
			float2	ssHorizon_Back = float2( ssPosition_Back.z, 1.0 - ssPosition_Back.z );
			float2	tsHorizon_Back = float2( dot( ssHorizon_Back, ssTangent ), dot( ssHorizon_Back, ssNormal ) );

			// Unfortunately here, we're already dealing with cos²(theta) but not from the same axes so we need
			//	to normalize the vectors and retrieve the new theta's in tangent space...
			tsHorizon_Front = normalize( tsHorizon_Front );
			tsHorizon_Back = normalize( tsHorizon_Back );

			// Perform integration of bent normal
// #TODO: Optimize later!
			float	theta0 = atan2( tsHorizon_Front.y, tsHorizon_Front.x );
			float	theta1 = atan2( tsHorizon_Back.y, tsHorizon_Back.x );
			float	averageCosTheta = 0.5 * (cos(theta1)*cos(theta1) - cos(theta0)*cos(theta0));
			float	finalTheta = acos( saturate( averageCosTheta ) );

			float2	ssBentNormal2D = cos(finalTheta) * ssTangent + sin(finalTheta) * ssNormal;
			float3	ssBentNormal = float3( ssBentNormal2D.x * ssDirection, ssBentNormal2D.y );
		#endif
*/
#endif

//ssBentNormal = float3( -1, -1, -1 );

		uint	dontCare;
		InterlockedAdd( gs_occlusionDirectionAccumulator.x, uint(65536.0 * (1.0 + ssBentNormal.x)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.y, uint(65536.0 * (1.0 + ssBentNormal.y)), dontCare );
		InterlockedAdd( gs_occlusionDirectionAccumulator.z, uint(65536.0 * (1.0 + ssBentNormal.z)), dontCare );
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

		float3	ssBentNormal = gs_occlusionDirectionAccumulator / 65536.0 / _raysCount - 1.0;
		ssBentNormal.y = -ssBentNormal.y;	// Normal textures are stored with inverted Y

		_Target[pixelPosition] = float4( 0.5 * (1.0+ssBentNormal), 1.0 );	// Ready for texture
//_Target[pixelPosition] = float4( 0.5 * (1.0+gs_local2World[2]), 1 );	// Ready for texture
//_Target[pixelPosition] = float4( dot( normalize(bentNormal), gs_local2World[2] ).xxx, 1 );
//_Target[pixelPosition] = float4( _raysCount.xxx / 2048.0, 1 );
//_Target[pixelPosition] = float4( float2( pixelPosition ) / _textureDimensions, 0, 1 );
	}
}