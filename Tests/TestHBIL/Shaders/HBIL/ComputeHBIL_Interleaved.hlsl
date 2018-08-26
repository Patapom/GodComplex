////////////////////////////////////////////////////////////////////////////////
// Shaders to compute HBIL
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "HBIL/HBIL.hlsl"

#define MAX_SAMPLES	8	// Maximum amount of samples per circle subdivision

Texture2DArray< float >		_tex_splitDepth : register(t0);		// Depth or distance buffer (here we're given depth)
Texture2DArray< float2 >	_tex_splitNormal : register(t1);	// Camera-space normal vectors
Texture2DArray< float3 >	_tex_splitRadiance : register(t2);	// Last frame's reprojected radiance buffer
Texture2D< float >			_tex_blueNoise : register(t3);

cbuffer CB_HBIL : register( b3 ) {
	uint2	_targetResolution;				// Small render-target resolution (1/4 the screen resolution)
	float2	_csDirection;					// Sampling direction in camera space

	uint3	_renderPassIndex;				// X=Index of the X pass in [0,3], Y=Index of the Y pass in [0,3], Z=Index of the render pass in [0,15]
	float	_gatherSphereMaxRadius_m;		// Radius of the sphere that will gather our irradiance samples (in meters)

	float4	_bilateralValues;

	float	_gatherSphereMaxRadius_p;		// Radius of the sphere that will gather our irradiance samples (in pixels)
	float	_temporalAttenuationFactor;		// Attenuation factor of radiance from previous frame
	uint	_jitterOffset;					// A jitter value in [0,67] that changes per frame
};

float4	VS( float4 __position : SV_POSITION ) : SV_POSITION { return __position; }

////////////////////////////////////////////////////////////////////////////////
// Implement the methods expected by the HBIL header
float	FetchDepth( float2 _pixelPosition, float _mipLevel ) {

//_pixelPosition = clamp( _pixelPosition, 0.5, _resolution.xy - 0.5 );

	return Z_FAR * _tex_splitDepth[uint3( floor( _pixelPosition ), _renderPassIndex.z )];
//	return Z_FAR * _tex_splitDepth.SampleLevel( LinearClamp, float3( _pixelPosition / _targetResolution, _renderPassIndex.z ), _mipLevel );
}

float3	FetchNormal( float2 _pixelPosition, float _mipLevel ) {
	float3	N;
//	N.xy = _tex_splitNormal[uint3( floor( _pixelPosition ), _renderPassIndex.z )];
	N.xy = _tex_splitNormal.SampleLevel( LinearClamp, float3( _pixelPosition / _targetResolution, _renderPassIndex.z ), 0.0 );
	N.z = sqrt( saturate( 1.0 - dot( N.xy, N.xy ) ) );
	return N;
}

float3	FetchRadiance( float2 _pixelPosition, float _mipLevel ) {
//	return _tex_splitRadiance.SampleLevel( LinearClamp, float3( _pixelPosition / _targetResolution + 4*_cameraSubPixelOffset.zw, _renderPassIndex.z ), _mipLevel ).xyz;

//_pixelPosition = clamp( _pixelPosition, 0.5, _resolution.xy - 0.5 );

	return _tex_splitRadiance[uint3( floor( _pixelPosition ), _renderPassIndex.z )];
}

float2	BilateralFilterDepth( float2 _ssCentralPosition, float _centralZ, float3 _lcsCentralNormal, float2 _ssCurrentPosition, float _currentZ, float _radius_meters, float _horizonCosTheta, float _newCosTheta ) {

//if ( _bilateralValues.x > 0.5 )
//	return 1.0;

//le critère est bien, mais il manque sans doute un blend avec le delta Z qui devient vraiment trop grand!

	// Rebuild camera-space positions & normals
	float2	UV0 = _ssCentralPosition / _targetResolution;
	float3	csView0 = BuildCameraRay( UV0 );
	float3	csPos0 = _centralZ * csView0;
			csView0 = normalize( csView0 );
	float3	csAt0 = -csView0;
	float3	csRight0 = normalize( cross( csAt0, float3( 0, 1, 0 ) ) );
	float3	csUp0 = cross( csRight0, csAt0 );
	float3	csNormal0 = _lcsCentralNormal.x * csRight0 + _lcsCentralNormal.y * csUp0 + _lcsCentralNormal.z * csAt0;

	float2	UV1 = _ssCurrentPosition / _targetResolution;
	float3	csView1 = BuildCameraRay( UV1 );
	float3	csPos1 = _currentZ * csView1;
			csView1 = normalize( csView1 );
	float3	csAt1 = -csView1;
	float3	csRight1 = normalize( cross( csAt1, float3( 0, 1, 0 ) ) );
	float3	csUp1 = cross( csRight1, csAt1 );
	float3	lcsCurrentNormal = FetchNormal( _ssCurrentPosition, 0.0 );
	float3	csNormal1 = lcsCurrentNormal.x * csRight1 + lcsCurrentNormal.y * csUp1 + lcsCurrentNormal.z * csAt1;

	// Our criterion is that current position and normal must see our central position to contribute...
	float3	csToCentralPosition = csPos0 - csPos1;
	float	distance2CentralPosition = length( csToCentralPosition );
			csToCentralPosition /= distance2CentralPosition;

	const float2	toleranceMin = float2( -0.02, -0.04 );
	const float2	toleranceMax = float2( -0.4, -0.8 );
	float	verticality = smoothstep( 0.5, 0.9, saturate( -dot( csToCentralPosition, csNormal0 ) ) );
	float2	tolerance = lerp( toleranceMin, toleranceMax, verticality );	// We grow more tolerant for very vertical pixels

//tolerance = toleranceMax;

//float	fade = dot( csToCentralPosition, csNormal1 );
//	float	fade = lerp( _bilateralValues.x, 1.0, smoothstep( -0.01, -0.0, dot( csToCentralPosition, csNormal1 ) ) );
	float	fade = smoothstep( tolerance.y, tolerance.x, dot( csToCentralPosition, csNormal1 ) );

	// To avoid sudden radiance jumps we use a dot product
	#if 0
		float	radianceFade = saturate( dot( csToCentralPosition, csNormal1 ) );
//				radianceFade = pow( radianceFade, 0.5 );
				radianceFade = fastSqrtNR0( radianceFade );					// Not full dot product, sqrt( dot( ) ) actually! (smoother result)
				radianceFade = 1.0
							 - saturate( 0.5 * distance2CentralPosition )	// Dot product fade out is fully effective after 2 meters
							 * (1.0 - radianceFade);
	#else
		float	radianceFade = 1.0
							 - saturate( 0.5 * distance2CentralPosition )	// Dot product fade out is fully effective after 2 meters
							 * pow2( 1.0 - saturate( dot( csToCentralPosition, csNormal1 ) ) );
//radianceFade = 1-saturate( 0.5 * distance2CentralPosition );
//radianceFade = pow( saturate( dot( csToCentralPosition, csNormal1 ) ), 0.5 );
	#endif

//radianceFade = 1;

	return float2( fade, radianceFade );
}

float4	SampleIrradiance_TEMP( float2 _ssCentralPosition, float2 _ssPosition, float2 _ssStep, float _radius_meters, float2 _sinCosGamma, float _centralZ, float3 _csCentralNormal, float _mipLevelDepth, float2 _integralFactors, inout float _radianceSamplingWeight, inout float3 _previousRadiance, inout float _maxCosTheta ) {

	// Read new Z and compute new horizon angle candidate
	float	Z = FetchDepth( _ssPosition, _mipLevelDepth );
	float	deltaZ = _centralZ - Z;																// Z difference, in meters
	float	recHypo = rsqrt( _radius_meters*_radius_meters + deltaZ*deltaZ );					// 1 / sqrt( z² + r² )
	float	cosTheta = (_sinCosGamma.x * _radius_meters + _sinCosGamma.y * deltaZ) * recHypo;	// cos(theta) = [sin(gamma)*r + cos(gamma)*z] / sqrt( z² + r² )


// Move bilateral filtering back there if we ever need to filter cosTheta again!
//	cosTheta = lerp( -1.0, cosTheta, bilateralWeights.x );	// Flatten if rejected


	// Update any rising horizon
	if ( cosTheta <= _maxCosTheta )
		return 0.0;	// Below the horizon... No visible contribution.

	// Filter outlier horizon values
	float2	bilateralWeights = BilateralFilterDepth( _ssCentralPosition, _centralZ, _csCentralNormal, _ssPosition, Z, _radius_meters, _maxCosTheta, cosTheta );


// This is an interesting line as it will prevent radiance from being sampled again once too many invalid samples are encountered
//	but it also dims the lighting quite quickly so not sure if we need to use it or not... IMHO it should be disabled...
#define	RUNNING_WEIGHT_DIM	1
#if RUNNING_WEIGHT_DIM
//if ( _bilateralValues.x < 0.5 )
	_radianceSamplingWeight *= bilateralWeights.x;	// Cumulate fades so we can never sample radiance again after too many invalid samples are encountered
#endif


// Check rejection AFTER test!
//if ( BilateralFilterDepth( _ssCentralPosition, _centralZ, _csCentralNormal, _ssPosition, Z, _radius_meters, _maxCosTheta, cosTheta ) < 0.5 )
//	return 0.0;
////////////////////


	#if SAMPLE_NEIGHBOR_RADIANCE
//_previousRadiance = FetchRadiance( _ssPosition - _bilateralValues.w * _ssStep, 0.0 );
//_previousRadiance = FetchRadiance( _ssPosition - bilateralWeights.x * _ssStep, 0.0 );
//_previousRadiance = FetchRadiance( _ssPosition - (1.0-bilateralWeights.x) * _ssStep, 0.0 );	// WORKING NICELY!!!
#if RUNNING_WEIGHT_DIM
if ( _radianceSamplingWeight > 0.0 ) {
#else
if ( bilateralWeights.y ) {
#endif
	float3	newRadiance = bilateralWeights.y * FetchRadiance( _ssPosition, 0.0 );
	_previousRadiance = lerp( _previousRadiance, newRadiance, _radianceSamplingWeight );	// Accept new radiance depending on accumulated sampling weight...
}

	#endif

	// Integrate over horizon difference (always from smallest to largest angle otherwise we get negative results!)
	float	radianceIntegral = IntegrateSolidAngle( _integralFactors, cosTheta, _maxCosTheta );
	float4	incomingRadiance = float4( radianceIntegral * _previousRadiance, radianceIntegral );

// #TODO: Integrate with linear interpolation of irradiance as well??
// #TODO: Integrate with Fresnel F0!

	_maxCosTheta = cosTheta;	// Register a new positive horizon
//	_previousDeltaZ = Z;		// Accept new depth difference

	return incomingRadiance;
}

float3	GatherIrradiance_TEMP( float2 _ssPosition, float2 _ssStep, float2 _ssMaxPosition, float2 _csDirection, float3 _csNormal, float2 _sinCosGamma, float _stepSize_meters, uint _maxStepsCount, float _centralZ, float3 _centralRadiance, float _noise, out float3 _csBentNormal, out float _AO ) {

	// Pre-compute factors for the integrals
	float2	integralFactors_Front = ComputeIntegralFactors( _csDirection, _csNormal );
	float2	integralFactors_Back = float2( -integralFactors_Front.x, integralFactors_Front.y );

	// Compute initial cos(angle) for front & back horizons
	// We do that by projecting the camera-space direction csDirection onto the tangent plane given by the normal
	//	then the cosine of the angle from the Z axis is simply given by the Pythagorean theorem:
	//                             P
	//			   N\  |Z		  -*-
	//				 \ |	  ---  ^
	//				  \|  ---      |
	//             --- *..........>+ csDirection
	//        --- 
	//
	float	hitDistance_Front = -dot( _csDirection, _csNormal.xy ) / _csNormal.z;
	float	planeCosTheta_Front = hitDistance_Front / sqrt( hitDistance_Front*hitDistance_Front + 1.0 );	// Assuming length(_csDirection) == 1
	float	planeCosTheta_Back = -planeCosTheta_Front;	// Back cosine is simply the mirror value

// Show horizon effect ON/OFF
//planeCosTheta_Front = -1.0;
//planeCosTheta_Back = -1.0;

	// Gather irradiance from front & back directions while updating the horizon angles at the same time
	float3	sumRadiance = 0.0;

//*
	#if 1
		// [18-02-27] Separated front & back steps
		// Less expensive for long paths, could easily span the entire screen without any penalty!
		float	irradianceLuminanceThreshold = 4.0 * dot( _centralRadiance, LUMINANCE );	// Value above which we consider a high radiance luminance
		float4	sumRadiance_low = 0.0;
		float4	sumRadiance_high = 0.0;

		// Compute border intersection
		float4	deltaBorderXY = float4( _ssMaxPosition.x - _ssPosition.x, _ssMaxPosition.y - _ssPosition.y, _ssPosition.x, _ssPosition.y );					// XY=distance to border for positive step, ZW=distance to border for negative step
		float4	deltaBorder = float4( _ssStep.x > 0.0 ? deltaBorderXY.xz : deltaBorderXY.zx, _ssStep.y > 0.0 ? deltaBorderXY.yw : deltaBorderXY.wy ).xzyw;	// XY = distance to border for front march, ZW = distance to border for back march
		float4	distance2Border = deltaBorder / max( 1e-3, abs( _ssStep ).xyxy );
		uint	stepsCount_Front = min( _maxStepsCount, uint( floor( min( distance2Border.x, distance2Border.y ) ) ) );
		uint	stepsCount_Back = min( _maxStepsCount, uint( floor( min( distance2Border.z, distance2Border.w ) ) ) );

		float	mipLevel = 0.0;

//_noise = _bilateralValues.y;

//_noise *= 0.0;

		// March forward
		const float4	STEP_SIZE_FACTORS = 1;
//		const float4	STEP_SIZE_FACTORS = float4( 0.25, 0.5, 1, 1 );
//		const float4	STEP_SIZE_FACTORS = float4( 0.125, 0.25, 0.5, 1 );

		float4	stepSizeFactor = STEP_SIZE_FACTORS;

		float	radius_meters_front = 0.0;
		float2	ssPosition_Front = _ssPosition - _noise * stepSizeFactor.x * _ssStep;
		float	maxCosTheta_Front = planeCosTheta_Front;
		float3	previousRadiance_Front = _centralRadiance;
		float	radianceSamplingWeight_Front = 1.0;

		[loop]
		for ( uint stepIndex=0; stepIndex < stepsCount_Front; stepIndex++ ) {
			radius_meters_front += stepSizeFactor.x * _stepSize_meters;
			ssPosition_Front += stepSizeFactor.x * _ssStep;
			float4	irradiance = SampleIrradiance_TEMP( _ssPosition, ssPosition_Front, stepSizeFactor.x * _ssStep, radius_meters_front, _sinCosGamma, _centralZ, _csNormal, mipLevel, integralFactors_Front, radianceSamplingWeight_Front, previousRadiance_Front, maxCosTheta_Front );
//			if ( dot( irradiance.xyz, LUMINANCE ) < irradianceLuminanceThreshold )
//				sumRadiance_low += irradiance;
//			else
//				sumRadiance_high += irradiance;
			sumRadiance += irradiance.xyz;
			stepSizeFactor.xyz = stepSizeFactor.yzw;	// Shift step sizes
		}

		// March backward
				stepSizeFactor = STEP_SIZE_FACTORS;

		float	radius_meters_back = 0.0;
		float2	ssPosition_Back = _ssPosition + _noise * stepSizeFactor.x * _ssStep;
		float	maxCosTheta_Back = planeCosTheta_Back;
		float2	sinCosGamma_Back = float2( -_sinCosGamma.x, _sinCosGamma.y );
		float3	previousRadianceBack = _centralRadiance;
		float	radianceSamplingWeight_Back = 1.0;

		[loop]
		for ( uint stepIndex2=0; stepIndex2 < stepsCount_Back; stepIndex2++ ) {
			radius_meters_back += stepSizeFactor.x * _stepSize_meters;
			ssPosition_Back -= stepSizeFactor.x * _ssStep;
			float4	irradiance = SampleIrradiance_TEMP( _ssPosition, ssPosition_Back, -stepSizeFactor.x * _ssStep, radius_meters_back, sinCosGamma_Back, _centralZ, _csNormal, mipLevel, integralFactors_Back, radianceSamplingWeight_Back, previousRadianceBack, maxCosTheta_Back );
//			if ( dot( irradiance.xyz, LUMINANCE ) < irradianceLuminanceThreshold )
//				sumRadiance_low += irradiance;
//			else
//				sumRadiance_high += irradiance;
			sumRadiance += irradiance.xyz;
			stepSizeFactor.xyz = stepSizeFactor.yzw;	// Shift step sizes
		}

		#if 0
			// Check if high luminance irradiance is not a single sparkle (must account more than 10% total weight to be deemed useable and not a firefly!)
			if ( sumRadiance_high.w > 0.1 * (sumRadiance_low.w + sumRadiance_high.w) ) {
//			if ( sumRadiance_high.w > _bilateralValues.y * (sumRadiance_low.w + sumRadiance_high.w) ) {
//			if ( sumRadiance_high.w > _bilateralValues.y * sumRadiance_low.w ) {
				sumRadiance = sumRadiance_low.xyz + sumRadiance_high.xyz;
			} else {
				sumRadiance = sumRadiance_low.xyz * (1.0 + sumRadiance_high.w / (sumRadiance_low.w + sumRadiance_high.w));	// Transfer back high radiance weight onto regular radiance
			}
		#endif

	#else
		// All in one loop + conditions
		// Less expensive for short paths apparently
		float	radius_meters = 0.0;
		float2	ssPosition_Front = _ssPosition + _noise * _ssStep;
		float2	ssPosition_Back = _ssPosition - _noise * _ssStep;
		float	maxCosTheta_Front = planeCosTheta_Front;
		float	maxCosTheta_Back = planeCosTheta_Back;
		float2	sinCosGamma_Back = float2( -_sinCosGamma.x, _sinCosGamma.y );
		float3	previousRadiance_Front = _centralRadiance;
		float3	previousRadianceBack = _centralRadiance;
		float	radianceSamplingWeight_Front = 1.0;
		float	radianceSamplingWeight_Back = 1.0;

		[loop]
		for ( uint stepIndex=0; stepIndex < _maxStepsCount; stepIndex++ ) {
			radius_meters += _stepSize_meters;
			ssPosition_Front += _ssStep;
			ssPosition_Back -= _ssStep;

			float	mipLevel = 0.0;
			if ( all( ssPosition_Front > 0.0 && ssPosition_Front < _ssMaxPosition ) )
				sumRadiance += SampleIrradiance_TEMP( _ssPosition, ssPosition_Front, _ssStep, radius_meters, _sinCosGamma, _centralZ, _csNormal, mipLevel, integralFactors_Front, radianceSamplingWeight_Front, previousRadiance_Front, maxCosTheta_Front ).xyz;
			if ( all( ssPosition_Back > 0.0 && ssPosition_Back < _ssMaxPosition ) )
				sumRadiance += SampleIrradiance_TEMP( _ssPosition, ssPosition_Back, -_ssStep, radius_meters, sinCosGamma_Back, _centralZ, _csNormal, mipLevel, integralFactors_Back, radianceSamplingWeight_Back, previousRadianceBack, maxCosTheta_Back ).xyz;
		}

	#endif
//*/

	// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
	_csBentNormal = IntegrateNormal( _csDirection, _csNormal, maxCosTheta_Back, maxCosTheta_Front );

	// DON'T NORMALIZE THE RESULT NOW OR WE GET BIAS!
//	_csBentNormal = normalize( _csBentNormal );

	#if USE_NORMAL_INFLUENCE_FOR_AO
		// Compute AO for this slice. Interval is regular [0,1]
//		_AO = IntegrateSolidAngle( integralFactors_Front, 1.0, maxCosTheta_Front )
//			+ IntegrateSolidAngle( integralFactors_Back, 1.0, maxCosTheta_Back );

		// For our special case, theta0=0
		_AO = integralFactors_Front.x * (FastPosAcos( maxCosTheta_Front ) - maxCosTheta_Front * sqrt( 1.0 - maxCosTheta_Front*maxCosTheta_Front )) + integralFactors_Front.y * (1.0 - maxCosTheta_Front*maxCosTheta_Front)
			+ integralFactors_Back.x * (FastPosAcos( maxCosTheta_Back ) - maxCosTheta_Back * sqrt( 1.0 - maxCosTheta_Back*maxCosTheta_Back )) + integralFactors_Back.y * (1.0 - maxCosTheta_Back*maxCosTheta_Back);
	#else
		// Compute AO for this slice (in [0,2]!!)
		_AO = 2.0 - maxCosTheta_Back - maxCosTheta_Front;
	#endif

	return sumRadiance;
}


////////////////////////////////////////////////////////////////////////////////
// Computes bent cone & irradiance gathering from pixel's surroundings
struct PS_OUT {
	float4	irradiance : SV_TARGET0;
	float4	bentCone : SV_TARGET1;
};

PS_OUT	PS( float4 __position : SV_POSITION ) {
	float2	subPixelOffset = _resolution * _cameraSubPixelOffset.zw;
	float2	fullScreenPixelPosition = 4.0 * (__position.xy - 0.5) + _renderPassIndex.xy + 0.5;	// Account for sub-pixel accuracy
//	float2	fullScreenPixelPosition = 4.0 * (__position.xy - 0.5) + _renderPassIndex.xy + subPixelOffset;	// Account for sub-pixel accuracy
	float2	UV = fullScreenPixelPosition / _resolution;
	uint2	pixelPosition = uint2( floor( __position.xy ) );
//	float	noise = frac( _tex_blueNoise.SampleLevel( LinearWrap, (fullScreenPixelPosition + subPixelOffset) / 64.0f, 0.0 ) + SQRT2 * _framesCount );	// DEFEATS TAA'S PURPOSE I SUPPOSE...

#if 0
	float	noise = frac( _tex_blueNoise[uint2(fullScreenPixelPosition) & 0x3F] + SQRT2 * _framesCount );	// ACTUAL GOOD VALUE!
#else	// Martin's noise
    float	noise = (wang_hash(fullScreenPixelPosition.y * _resolution.x + fullScreenPixelPosition.x)
					^ wang_hash(uint(_jitterOffset)))
					* 2.3283064365386963e-10;
#endif

	// Setup camera ray
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;

	// Read back depth, normal & central radiance value from last frame
//	float	Z = FetchDepth( __position.xy, 0.0 );
//	float	Z = Z_FAR * _tex_splitDepth.SampleLevel( LinearClamp, float3( float2( pixelPosition + 0.5 ) / _targetResolution, _renderPassIndex.z ), 0.0 );
	float	Z = Z_FAR * _tex_splitDepth[uint3( pixelPosition, _renderPassIndex.z )];

			Z -= 1e-2;	// !IMPORTANT! Prevent acnea by offseting the central depth a tiny bit closer

	float3	centralRadiance = _tex_splitRadiance[uint3( pixelPosition, _renderPassIndex.z )].xyz;	// Read back last frame's radiance value that we can use as a fallback for neighbor areas

	// Compute local camera-space
	float3	wsPos = _camera2World[3].xyz + Z * Z2Distance * wsView;
	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	wsAt = -wsView;

	// Express local camera-space vectors in global camera-space
	float3	gcsRight = float3( dot( wsRight, _camera2World[0].xyz ), dot( wsRight, _camera2World[1].xyz ), dot( wsRight, _camera2World[2].xyz ) );
	float3	gcsUp = float3( dot( wsUp, _camera2World[0].xyz ), dot( wsUp, _camera2World[1].xyz ), dot( wsUp, _camera2World[2].xyz ) );

	// Compute the correction factors for our horizon angles
	// The idea is that we need to walk in screen space but still want to express our angles in local camera space
	// The configuration of the problem for this situation is something like this:
	//	
	//	             ---
	//	        ---
	//	x  ---  a      x'
	//	*------R-------* Z0 ---> X
	//	|\--    b      |
	//	|    --        |
	//	| \     --     z
	//	|          --  |
	//	|  \  theta   -* Z1
	//	|              |
	//	|   \          |
	//	|              |
	//	|    \         |
	//	|              |
	//	|  a  \        |
	//
	// We walk from x to x' along the global camera space's X axis for a distance R and we sample a new depth Z1 (along the camera's Z axis shown as the vertical | characters)
	// The local camera space's view axis makes an angle "a" with the global camera space's Z axis.
	// Its tangent plane (represented at the top, starting slant from x) also makes an angle alpha with the screen plane represented by X
	// We are looking for cos(theta) and we simply note that theta + a + b = PI/2
	// Thus, cos(theta) = cos( PI/2 - a - b ) = sin( a + b ) = sin(a)*cos(b) + cos(a)*sin(b)
	// We easily find that cos(b) = R / sqrt( z² + R² ) and sin(b) = z / sqrt( z² + R² )
	// So finaly:
	//	cos(theta) = [sin(a)*R + cos(a)*z] / sqrt( z² + R² )
	//
	// We notice that it behaves a bit like a rotation and is finally an angular interpolation between sin(b) and cos(b) that depends on the camera axis deviation...
	//
	float2	sinCosGamma;
//	sinCosGamma.y = csView.z;// dot( wsView, _camera2World[2].xyz );
//	sinCosGamma.x = sqrt( 1.0 - sinCosGamma.y*sinCosGamma.y );
//	sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

	// Compute local camera-space normal
	float3	csNormal;
			csNormal.xy = _tex_splitNormal[uint3( pixelPosition, _renderPassIndex.z )];
			csNormal.z = sqrt( saturate( 1.0 - dot( csNormal.xy, csNormal.xy ) ) );
			csNormal.z = max( 1e-3, csNormal.z );	// Make sure it's never 0!

	// Compute screen radius of gather sphere
	float	screenSize_m = 2.0 * Z * TAN_HALF_FOV;																	// Vertical size of the screen in meters when extended to distance Z
	float	meter2Pixel = _targetResolution.y / screenSize_m;														// Gives us the conversion factor to go from meters to pixels
	float	sphereRadius_pixels = meter2Pixel * _gatherSphereMaxRadius_m;
			sphereRadius_pixels = min( _gatherSphereMaxRadius_p, sphereRadius_pixels );								// Prevent it to grow larger than our fixed limit
	float	radiusStepSize_pixels = max( 1.0, sphereRadius_pixels / MAX_SAMPLES );									// This gives us our radial step size in pixels
	uint	samplesCount = clamp( uint( ceil( sphereRadius_pixels / radiusStepSize_pixels ) ), 1, MAX_SAMPLES );	// Reduce samples count if possible
//	float	radiusStepSize_meters = sphereRadius_pixels / (samplesCount * meter2Pixel);								// This gives us our radial step size in meters
	float	radiusStepSize_meters = radiusStepSize_pixels / (csView.z * meter2Pixel);								// This gives us our radial step size in meters


//samplesCount = MAX_SAMPLES;


	// Start gathering radiance and bent normal by subdividing the screen-space disk around our pixel into N slices
	float3	sumIrradiance = 0.0;
	float	sumAO = 0.0;
	float3	csAverageBentNormal = 0.0;
	float	averageAO = 0.0;
//	float	varianceAO = 0.0;
	float3	csBentNormal;
	float	AO;

	/////////////////////////////////////////////////////////////////////
	// Build camera-space and screen-space walk directions
	float2	csDirection = _csDirection;
float	phi = (B4( _renderPassIndex.x, _renderPassIndex.y ) / 16.0 + 2.6457513110645905905016157536393 * _framesCount) * 0.5 * PI;	// ACTUAL GOOD VALUE!
//float	phi = (B4( _renderPassIndex.x, _renderPassIndex.y ) + 2.6457513110645905905016157536393 * _mouseUVs.x) / 16.0 * 0.5 * PI;	// MANUALLY CONTROLED VALUE

// Dumb tests
//phi = _renderPassIndex.z / 16.0 * 0.5 * PI;
//phi = (B4( _renderPassIndex.x, _renderPassIndex.y ) + _bilateralValues.x * _framesCount) / 16.0 * 0.5 * PI;
//phi = _bilateralValues.x * 0.5 * PI;
//noise = 0;
//phi = (B4( _renderPassIndex.x, _renderPassIndex.y ) / 16.0) * 0.5 * PI;
//phi = _renderPassIndex.z / 16.0 * 0.5 * PI;
//phi = _time + Bayer1D_16( _framesCount) / 16.0 * 0.5 * PI;
//phi = _time * 0.5 * PI;

//csDirection = float2( cos( phi ), sin( phi ) );

	float3	gcsDirection = csDirection.x * gcsRight + csDirection.y * gcsUp;	// Since csDirection gives us the components along local camera-space's right and up vectors, and we know their expression in global camera-space, it's easy find the equivalent global camera-space direction...
	float2	ssDirection = normalize( gcsDirection.xy );							// We normalize since we want to take integer pixel steps
			ssDirection *= radiusStepSize_pixels;								// Scale by our step size
			ssDirection.y = -ssDirection.y;										// Going upward in camera space means going downward in screen space...

	// Build angular correction based on deviation from central camera axis
	sinCosGamma.x = gcsDirection.z;
	sinCosGamma.y = sqrt( 1.0 - sinCosGamma.x*sinCosGamma.x );
	sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

	// Gather irradiance and average cone direction for that slice
	sumIrradiance += GatherIrradiance_TEMP( __position.xy, ssDirection, _targetResolution, csDirection, csNormal, sinCosGamma, radiusStepSize_meters, samplesCount, Z, centralRadiance, noise, csBentNormal, AO );
	csAverageBentNormal += csBentNormal;
	sumAO += AO;

	#if !USE_SINGLE_DIRECTION
		/////////////////////////////////////////////////////////////////////
		// Build orthogonal camera-space and screen-space walk directions
		csDirection = float2( -csDirection.y, csDirection.x );
		gcsDirection = csDirection.x * gcsRight + csDirection.y * gcsUp;
		ssDirection = normalize( gcsDirection.xy );
		ssDirection *= radiusStepSize_pixels;
		ssDirection.y = -ssDirection.y;

		// Build angular correction based on deviation from central camera axis
		sinCosGamma.x = gcsDirection.z;
		sinCosGamma.y = sqrt( 1.0 - sinCosGamma.x*sinCosGamma.x );
		sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

		// Gather irradiance and average cone direction for that slice
		sumIrradiance += GatherIrradiance_TEMP( __position.xy, ssDirection, _targetResolution, csDirection, csNormal, sinCosGamma, radiusStepSize_meters, samplesCount, Z, centralRadiance, noise, csBentNormal, AO );
		csAverageBentNormal += csBentNormal;
		sumAO += AO;
	#endif

	#if 1
		/////////////////////////////////////////////////////////////////////
		// Accumulate temporal radiance
		// Here we attempt to re-inject last frame radiance as well
		//
//		float	temporalExtinction = _bilateralValues.x;	// How much of last frame's value remains after 1 second?
//		float	temporalMaxWeight =  _bilateralValues.y;	// Importance of temporal weight over spatial weight
		float	temporalExtinction = 0.25;					// How much of last frame's value remains after 1 frame? (empirical: 25%)
		float	temporalMaxWeight = 1.0;					// Importance of temporal weight over spatial weight (empirical: 1)

//		const float	referenceFrameDeltaTime = 0.016;	// I wish! 
//		float	deltaFrames = clamp( 1.0, 4.0, ceil( _deltaTime / referenceFrameDeltaTime ) );	// Fluctuates too much!
		float	deltaFrames = 1.0;
		float	temporalWeight = temporalMaxWeight * exp( log( max( 1e-4, temporalExtinction ) ) * deltaFrames );	// How much of last frame's value remains for this frame?

		sumIrradiance += temporalWeight * centralRadiance;
	#else
		float	temporalWeight = 0.0;
	#endif

	/////////////////////////////////////////////////////////////////////
	// Finalize results
	#if USE_SINGLE_DIRECTION
		#if !USE_NORMAL_INFLUENCE_FOR_AO
			sumAO *= 0.5;															// / 2 (slice interval in [0,2]) / 1 (direction)
		#endif
		float3	radiance = max( 0.0, PI * sumIrradiance / (1+temporalWeight) );	// * PI / 1 (direction)
	#else
		#if USE_NORMAL_INFLUENCE_FOR_AO
			sumAO *= 0.5;															// / 2 (directions). Each AO interval is [0,1] here
		#else
			sumAO *= 0.25;															// / 2 (slice interval in [0,2]) / 2 (directions)
		#endif

//		float3	radiance = max( 0.0, 0.5 * PI * sumIrradiance );				// * PI / 2 (directions)
		float3	radiance = max( 0.0, PI * sumIrradiance / (2+temporalWeight) );	// * PI / 2 (directions)
	#endif


	#if USE_RECOMPOSED_BUFFER
		float4	csBentCone = float4( csAverageBentNormal, sqrt(sumAO) );	// Store as is, will be dealt with by RecomposeBuffers shader
	#else

// Poor attempt at regaining a nicer look, as with many samples...
//sumAO = pow( saturate( sumAO ), _bilateralValues.x );

		float	cosAverageConeAngle = 1.0 - sumAO;		// Use AO as cone angle
		float	stdDeviation = 0.0;						// Not available if no reconstruction is done... :'(

//csAverageBentNormal = csNormal;

		// Scale normalized bent normal by the cosine of the cone angle
//		const float	MIN_ENCODABLE_VALUE = 1.0 / 128.0;	// If about to store to a RGBA8_SNORM target then use this value
		const float	MIN_ENCODABLE_VALUE = 0.001;		// If using directly then just use this value
		csAverageBentNormal *= sqrt( max( MIN_ENCODABLE_VALUE, cosAverageConeAngle ) ) / length( csAverageBentNormal );
		float4	csBentCone = float4( csAverageBentNormal, stdDeviation );
	#endif

	/////////////////////////////////////////////////////////////////////
	// Write
	PS_OUT	Out;
	Out.irradiance = float4( radiance, 0 );
	Out.bentCone = csBentCone;

	return Out;
}

