////////////////////////////////////////////////////////////////////////////////
// Horizon-Based Indirect Lighting
////////////////////////////////////////////////////////////////////////////////
// 
////////////////////////////////////////////////////////////////////////////////
//

// !!SLIGHTLY LESS ACCURATE!!
#define USE_FAST_ACOS 1			// Define this to use the "fast acos" function instead of true acos()
// !!SLIGHTLY LESS ACCURATE!!

// !!MORE ACCURATE BUT ALSO MORE EXPENSIVE!!
#define SAMPLE_NEIGHBOR_RADIANCE	1	// Define this to sample neighbor samples' radiance, otherwise only the central sample's radiance is used (much faster but also less accurate)
// !!MORE ACCURATE BUT ALSO MORE EXPENSIVE!!

// !!MORE EXPENSIVE AND LESS ACCURATE => DON'T USE EXCEPT FOR DEBUG!!
//#define USE_NUMERICAL_INTEGRATION 256	// Define this to compute bent normal numerically (value = integration steps count)
// !!MORE EXPENSIVE AND LESS ACCURATE => DON'T USE EXCEPT FOR DEBUG!!

// !!NOT AS PHYSICALLY CORRECT
//#define USE_NORMAL_INFLUENCE_FOR_BENT_NORMAL	1	// Define this to compute the bent normal by accounting for visibility + cos(angle with the normal), instead of visibility only for the regular case
// !!NOT AS PHYSICALLY CORRECT

// This is an interesting line as it will prevent radiance from being sampled again once too many invalid samples are encountered
//	(i.e. the bilateral filtering keeps on accumulating, making new samples less and less susceptible to be accepted once they started to get rejected)
//	but it also dims the lighting quite quickly so not sure if we need to use it or not... IMHO it should be enabled otherwise we get some visual artefacts...
#define	RUNNING_WEIGHT_DIM	1

// !!VERY NICE IDEA TO EXPLORE FURTHER
// This idea consists in changing the step size to start slow then go faster, instead of using a constant step size
// This is important for AO gathering where many details (i.e. fast horizon jumps) are often very close from the central position
// Unfortunately, enabling this seems to break the large-range indirect lighting gathering that suffers greatly.
// It seems to be a delicate balance between very nice AO or very nice IL. Ideally, obviously, we would like just "more steps", but it'd too expensive... :'(
#define	STEP_SIZE_FACTORS	1									// Constant steps give great IL but poorly detailed AO :/
//#define	STEP_SIZE_FACTORS	float4( 0.125, 0.25, 0.5, 1 );	// This gives greatly detailed AO but poor IL :/
// !!VERY NICE IDEA TO EXPLORE FURTHER


#define USE_NORMAL_INFLUENCE_FOR_AO	1	// Define this to compute AO as a ratio of visibility only + cos(angle with the normal), which is what we expect in our calculations since AO is supposed to be used for far-field computation and should be the complement of the near field lighting given by HBIL


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Pre-Declarations

// We assume there exist such methods declared somewhere where _pixelPosition is the position in screen space
float	FetchDepth( float2 _pixelPosition, float _mipLevel );		// The return value must be the depth (in meters)
float3	FetchNormal( float2 _pixelPosition, float _mipLevel );		// The return value must be the normal vector in local camera space (i.e. no negative Z component)
float3	FetchRadiance( float2 _pixelPosition, float _mipLevel );	// The return value must be the radiance from last frame (DIFFUSE ONLY!)

// Expensive bilateral filtering
//	_ssCentralPosition, the screen-space position of the central point we're computing the lighting for
//	_centralZ, the depth (from FetchDepth()) of the central point we're computing the lighting for
//	_lcsCentralNormal, the normal (from FetchNormal()) of the central point we're computing the lighting for
//	_ssCurrentPosition, the screen-space position of the point we're computing the filtering values for
//	_currentZ, the depth (from FetchDepth()) of the point we're computing the filtering values for
//	_lcsCurrentNormal, the normal (from FetchNormal()) of the point we're computing the filtering values for
//	_radius_meters, distance (in meters) between current and central positions
//	_horizonCosTheta, cosine of the current angle to the horizon
//	_newCosTheta, cosine of the angle to the new candidate horizon
//
// The X return value must be a [0,1] weight telling whether or not we accept the DEPTH sample
// The Y return value must be a [0,1] weight telling whether or not we accept the IRRADIANCE sample
float2	BilateralFilter( float2 _ssCentralPosition, float _centralZ, float3 _lcsCentralNormal, float2 _ssCurrentPosition, float _currentZ, float3 _lcsCurrentNormal, float _radius_meters, float _horizonCosTheta, float _newCosTheta );


// Integrates the dot product of the normal with a vector interpolated between slice horizon angle theta0 and theta1 (equation 18 from the paper)
// We're looking to obtain the irradiance reflected from a tiny slice of terrain:
//	E = Integral[theta0, theta1]{ L(Xi) (Wi.N) sin(theta) dTheta }
// Where
//	Xi, the location of the neighbor surface reflecting radiance
//	L(Xi), the surface's radiance at that location
//	Wi, the direction of the vector within the slice that we're integrating Wi = { cos(phi).sin(theta), sin(phi).sin(theta), cos(theta) } expressed in screen space
//	phi, the azimutal angle of the screen-space slice
//	N, the surface's normal at current location (not neighbor location!)
//
// The integral transforms into an irradiance-agnostic version:
//	E = L(Xi) * Integral[theta0, theta1]{ (Wi.N) sin(theta) dTheta } = L(Xi) * I
//
// With:
//	I = Integral[theta0, theta1]{ cos(phi).sin(theta).Nx.sin(theta) dTheta }	<= I0
//	  + Integral[theta0, theta1]{ sin(phi).sin(theta).Ny.sin(theta) dTheta }	<= I1
//	  + Integral[theta0, theta1]{          cos(theta).Nz.sin(theta) dTheta }	<= I2
//
//	I0 = [cos(phi).Nx] * Integral[theta0, theta1]{ sin�(theta) dTheta } = [cos(phi).Nx] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I1 = [sin(phi).Ny] * Integral[theta0, theta1]{ sin�(theta) dTheta } = [sin(phi).Ny] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I2 = Nz * Integral[theta0, theta1]{ cos(theta).sin(theta) dTheta } = Nz * { 1/2 * cos�(theta)) }[theta1,theta0]	<== Watch out for theta reverse here!
//
float	IntegrateSolidAngle( float2 _integralFactors, float _cosTheta0, float _cosTheta1 ) {
	float	sinTheta0 = sqrt( 1.0 - _cosTheta0*_cosTheta0 );
	float	sinTheta1 = sqrt( 1.0 - _cosTheta1*_cosTheta1 );

	// SUPER EXPENSIVE PART!
	#if USE_FAST_ACOS
		float	theta0 = FastPosAcos( _cosTheta0 );
		float	theta1 = FastPosAcos( _cosTheta1 );
	#else
		float	theta0 = acos( _cosTheta0 );
		float	theta1 = acos( _cosTheta1 );
	#endif

	float	I0 = (theta1 - sinTheta1*_cosTheta1)
			   - (theta0 - sinTheta0*_cosTheta0);
	float	I1 = _cosTheta0*_cosTheta0 - _cosTheta1*_cosTheta1;
	return _integralFactors.x * I0 + _integralFactors.y * I1;
}
float2	ComputeIntegralFactors( float2 _csDirection, float3 _N ) {
	return 0.5 * float2( dot( _N.xy, _csDirection ), _N.z );	// = Nx/2, Ny/2
}

// Integrates the bent normal between the 2 horizon angles (equations 5 and 6 from the paper)
//	_csDirection, the slice direction (in camera-space)
//	_csNormal, the normal direction (in camera-space)
//	_cosThetaBack, the cosine of the backward horizon angle
//	_cosThetaFront, the cosine of the forward horizon angle
// Returns the average bent normal for the slice
//
float3	IntegrateNormal( float2 _csDirection, float3 _csNormal, float _cosThetaBack, float _cosThetaFront ) {
	float2	ssNormal = float2( dot( _csNormal.xy, _csDirection ), _csNormal.z );	// Project normal onto the slice plane

	#if USE_NUMERICAL_INTEGRATION
		// Half brute force where we perform the integration numerically as a sum...
		//
		float	thetaFront = acos( _cosThetaFront );
		float	thetaBack = -acos( _cosThetaBack );

		float2	ssBentNormal = 0.0;
		for ( uint i=0; i < USE_NUMERICAL_INTEGRATION; i++ ) {
			float	theta = lerp( thetaBack, thetaFront, (i+0.5) / USE_NUMERICAL_INTEGRATION );
			float	sinTheta, cosTheta;
			sincos( theta, sinTheta, cosTheta );
			float2	ssOmega = float2( sinTheta, cosTheta );

			float	cosAlpha = saturate( dot( ssOmega, ssNormal ) );

cosAlpha = 1.0;	// No influence after all!!

			float	weight = cosAlpha * abs(sinTheta);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)

			ssBentNormal += weight * ssOmega;
		}

		float	dTheta = (thetaFront - thetaBack) / USE_NUMERICAL_INTEGRATION;
		ssBentNormal *= dTheta;

		float3	csBentNormal = float3( ssBentNormal.x * _csDirection, ssBentNormal.y );
	#elif USE_NORMAL_INFLUENCE_FOR_BENT_NORMAL
		// Analytical solution
		// ==== WITH NORMAL INFLUENCE ==== 
		// These integrals are more complicated and we used to account for the dot product with the normal but that's not the way to compute the bent normal after all!!
		float	cosTheta0 = _cosThetaFront;
		float	cosTheta1 = _cosThetaBack;	// This should be in [-PI,0] but instead I take the absolute value so [0,PI] instead
		float	sinTheta0 = sqrt( 1.0 - cosTheta0*cosTheta0 );
		float	sinTheta1 = sqrt( 1.0 - cosTheta1*cosTheta1 );
		float	cosTheta0_3 = cosTheta0*cosTheta0*cosTheta0;
		float	cosTheta1_3 = cosTheta1*cosTheta1*cosTheta1;
		float	sinTheta0_3 = sinTheta0*sinTheta0*sinTheta0;
		float	sinTheta1_3 = sinTheta1*sinTheta1*sinTheta1;

		float	averageX = ssNormal.x * (cosTheta0_3 + cosTheta1_3 - 3.0 * (cosTheta0 + cosTheta1) + 4.0)
						 + ssNormal.y * (sinTheta0_3 - sinTheta1_3);

		float	averageY = ssNormal.x * (sinTheta0_3 - sinTheta1_3)
						 + ssNormal.y * (2.0 - cosTheta0_3 - cosTheta1_3);

		float3	csBentNormal = float3( averageX * _csDirection, averageY );	// Rebuild normal in camera space
	#else
		// Analytical solution
		// ==== WITHOUT NORMAL INFLUENCE ==== 
		// 
		#if USE_FAST_ACOS
			float	theta0 = -FastAcos( _cosThetaBack );
			float	theta1 = FastAcos( _cosThetaFront );
		#else
			float	theta0 = -acos( _cosThetaBack );
			float	theta1 = acos( _cosThetaFront );
		#endif
		float	cosTheta0 = _cosThetaBack;
		float	cosTheta1 = _cosThetaFront;
		float	sinTheta0 = -sqrt( 1.0 - cosTheta0*cosTheta0 );
		float	sinTheta1 = sqrt( 1.0 - cosTheta1*cosTheta1 );

		float	averageX = theta1 + theta0 - sinTheta0*cosTheta0 - sinTheta1*cosTheta1;
		float	averageY = 2.0 - cosTheta0*cosTheta0 - cosTheta1*cosTheta1;

		float3	csBentNormal = float3( averageX * _csDirection, averageY );	// Rebuild normal in camera space
	#endif

	return csBentNormal;
}

// Samples the irradiance reflected from a screen-space position located around the center pixel position
//	_ssPosition, the screen-space position to sample
//	_ssStep, the screen-space marching step (potentially used to back track radiance sampling to avoid reading "edge colors" when the horizon is rising)
//	_radius_meters, the radius (in meters) from the center position
//	_sinCosGamma, the sin/cos of the camera deviation angle used for local camera-space reprojection of the horizon
//	_centralZ, the depth value of the central pixel
//	_integralFactors, some pre-computed factors to feed the radiance integral
//	_maxCosTheta, the floating maximum cos(theta) that indicates the angle of the perceived horizon
//
float4	SampleIrradiance( float2 _ssCentralPosition, float2 _ssPosition, float2 _ssStep, float _radius_meters, float2 _sinCosGamma, float _centralZ, float3 _csCentralNormal, float _mipLevelDepth, float2 _integralFactors, inout float _radianceSamplingWeight, inout float3 _previousRadiance, inout float _maxCosTheta ) {

	// Read new Z and compute new horizon angle candidate
	float	Z = FetchDepth( _ssPosition, _mipLevelDepth );
	float	deltaZ = _centralZ - Z;																// Z difference, in meters
	float	recHypo = rsqrt( _radius_meters*_radius_meters + deltaZ*deltaZ );					// 1 / sqrt( z� + r� )
	float	cosTheta = (_sinCosGamma.x * _radius_meters + _sinCosGamma.y * deltaZ) * recHypo;	// cos(theta) = [sin(gamma)*r + cos(gamma)*z] / sqrt( z� + r� )

	float3	lcsCurrentNormal = FetchNormal( _ssPosition, 0.0 );

// Move bilateral filtering back there if we ever need to filter cosTheta again!
//	float2	bilateralWeights = BilateralFilter( _ssCentralPosition, _centralZ, _csCentralNormal, _ssPosition, Z, lcsCurrentNormal, _radius_meters, _maxCosTheta, cosTheta );
//	cosTheta = lerp( -1.0, cosTheta, bilateralWeights.x );	// Flatten if rejected


	// Update any rising horizon
	if ( cosTheta <= _maxCosTheta )
		return 0.0;	// Below the horizon... No visible contribution.

	// Filter outlier horizon values
	float2	bilateralWeights = BilateralFilter( _ssCentralPosition, _centralZ, _csCentralNormal, _ssPosition, Z, lcsCurrentNormal, _radius_meters, _maxCosTheta, cosTheta );

	#if RUNNING_WEIGHT_DIM
		_radianceSamplingWeight *= bilateralWeights.x;	// Cumulate fades so we can never sample radiance again after too many invalid samples are encountered
	#endif

	#if SAMPLE_NEIGHBOR_RADIANCE

		#if RUNNING_WEIGHT_DIM
			if ( _radianceSamplingWeight > 0.0 ) {
				float3	newRadiance = bilateralWeights.y * FetchRadiance( _ssPosition, 0.0 );
				_previousRadiance = lerp( _previousRadiance, newRadiance, _radianceSamplingWeight );	// Accept new radiance depending on accumulated sampling weight...
			}
		#else
			if ( bilateralWeights.y ) {
				float3	newRadiance = bilateralWeights.y * FetchRadiance( _ssPosition, 0.0 );
				_previousRadiance = lerp( _previousRadiance, newRadiance, _radianceSamplingWeight );	// Accept new radiance depending on accumulated sampling weight...
			}
		#endif

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

////////////////////////////////////////////////////////////////////////////////
// Gathers the irradiance around the central position
//	_ssPosition, the central screen-space position (in pixel) where to gather from
//	_ssStep, the screen-space direction of the disc slice we're sampling
//	_ssMaxPosition, the screen-space position of the bottom right corner (for clipping)
//	_csDirection, the camera-space direction of the disc slice we're sampling
//	_csNormal, camera-space normal
//	_sinCosGamma, the sin/cos of the camera deviation angle used for local camera-space reprojection of the horizon
//	_stepSize_meters, size (in meters) of a radial step to jump from one sample to another
//	_maxStepsCount, amount of steps to take (front & back so actual samples count will always be twice that value!)
//	_centralZ, the central depth value (WARNING: Always offset it toward the camera by a little epsilon before entering this function, to avoid horizon acnea)
//	_centralRadiance, last frame's radiance at central position that we can always use as a safe backup for irradiance integration (in case bilateral filter rejects neighbor height as too different)
//	_noise, noise value in [0,1]
//
//	[OUT] _csBentNormal, the average camera-space bent normal for the slice (Warning: NOT NORMALIZED, and must be accumulated unnormalized otherwise result will get biased! Only normalize at the end of the loop.)
//	[OUT] _AO, the collected ambient occlusion for the slice (in [0,2] since we collected for the front and back direction!)
//	Returns the irradiance gathered along the sampling
//
float3	GatherIrradiance( float2 _ssPosition, float2 _ssStep, float2 _ssMaxPosition, float2 _csDirection, float3 _csNormal, float2 _sinCosGamma, float _stepSize_meters, uint _maxStepsCount, float _centralZ, float3 _centralRadiance, float _noise,
						  out float3 _csBentNormal, out float _AO ) {

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

// DEBUG: Show the effect of horizon initialization ON/OFF by uncommenting those lines
//planeCosTheta_Front = -1.0;
//planeCosTheta_Back = -1.0;

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	// Gather irradiance from front & back directions while updating the horizon angles at the same time
	float3	sumRadiance = 0.0;

	#if 1
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// [18-02-27] Separated front & back steps
		// Less expensive for long paths, could easily span the entire screen without any penalty!
		// We can clip each front/back path independently if we reach the screen borders.
		//

		// Compute border intersection
		float4	deltaBorderXY = float4( _ssMaxPosition.x - _ssPosition.x, _ssMaxPosition.y - _ssPosition.y, _ssPosition.x, _ssPosition.y );					// XY = distance to border for positive step, ZW = distance to border for negative step
		float4	deltaBorder = float4( _ssStep.x > 0.0 ? deltaBorderXY.xz : deltaBorderXY.zx, _ssStep.y > 0.0 ? deltaBorderXY.yw : deltaBorderXY.wy ).xzyw;	// XY = distance to border for front march, ZW = distance to border for back march
		float4	distance2Border = deltaBorder / max( 1e-3, abs( _ssStep ).xyxy );
		uint	stepsCount_Front = min( _maxStepsCount, uint( floor( min( distance2Border.x, distance2Border.y ) ) ) );
		uint	stepsCount_Back = min( _maxStepsCount, uint( floor( min( distance2Border.z, distance2Border.w ) ) ) );

		float	mipLevel = 0.0;

		// March forward
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
			float4	irradiance = SampleIrradiance( _ssPosition, ssPosition_Front, stepSizeFactor.x * _ssStep, radius_meters_front, _sinCosGamma, _centralZ, _csNormal, mipLevel, integralFactors_Front, radianceSamplingWeight_Front, previousRadiance_Front, maxCosTheta_Front );
			sumRadiance += irradiance.xyz;
			stepSizeFactor.xyz = stepSizeFactor.yzw;	// Shift step sizes
		}

		// March backward
		stepSizeFactor = STEP_SIZE_FACTORS;	// Reset size factors

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
			float4	irradiance = SampleIrradiance( _ssPosition, ssPosition_Back, -stepSizeFactor.x * _ssStep, radius_meters_back, sinCosGamma_Back, _centralZ, _csNormal, mipLevel, integralFactors_Back, radianceSamplingWeight_Back, previousRadianceBack, maxCosTheta_Back );
			sumRadiance += irradiance.xyz;
			stepSizeFactor.xyz = stepSizeFactor.yzw;	// Shift step sizes
		}

	#else
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// All in one loop + conditions
		// Less expensive for short paths apparently
		// But we can't early exit any path and we have to keep a condition inside the loop...
		//
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
				sumRadiance += SampleIrradiance( _ssPosition, ssPosition_Front, _ssStep, radius_meters, _sinCosGamma, _centralZ, _csNormal, mipLevel, integralFactors_Front, radianceSamplingWeight_Front, previousRadiance_Front, maxCosTheta_Front ).xyz;
			if ( all( ssPosition_Back > 0.0 && ssPosition_Back < _ssMaxPosition ) )
				sumRadiance += SampleIrradiance( _ssPosition, ssPosition_Back, -_ssStep, radius_meters, sinCosGamma_Back, _centralZ, _csNormal, mipLevel, integralFactors_Back, radianceSamplingWeight_Back, previousRadianceBack, maxCosTheta_Back ).xyz;
		}

	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////////////
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
// Helper to sample the HBIL data either from a recomposed buffer or directly from split buffers
////////////////////////////////////////////////////////////////////////////////
//
#if USE_RECOMPOSED_BUFFER
	// When buffers have been previously recomposed, only a single fetch at fullscreen resolution is enough
	void	SampleHBILData( uint2 _pixelPosition, Texture2D<float4> _tex_RecomposedRadiance, Texture2D<float4> _tex_RecomposedBentCone, out float3 _radiance, out float4 _csBentCone ) {
		_radiance = _tex_RecomposedRadiance[_pixelPosition].xyz;
		_csBentCone = _tex_RecomposedBentCone[_pixelPosition];
	}
#else
	// If buffers are split, then we need to identify which buffer to fetch from given our fullscreen resolution
	void	SampleHBILData( uint2 _pixelPosition, Texture2DArray<float4> _tex_SplitRadiance, Texture2DArray<float4> _tex_SplitBentCone, out float3 _radiance, out float4 _csBentCone ) {
		uint2	subPixelIndex = _pixelPosition & 3;
		uint	sliceIndex = (subPixelIndex.y << 2) + subPixelIndex.x;
		uint3	samplingPosition = uint3( _pixelPosition >> 2, sliceIndex );

		_radiance = _tex_SplitRadiance[samplingPosition].xyz;
		_csBentCone = _tex_SplitBentCone[samplingPosition];
	}
#endif


////////////////////////////////////////////////////////////////////////////////
// Helper to reconstruct world-space bent cone
////////////////////////////////////////////////////////////////////////////////
//
//	_wsView, the world-space view direction of the camera (pointing toward the scene)
//	_wsCameraUp, the world-space "Up" direction of the camera (pointing up)
//	_lcsBentCone, the local camera-space packed bent-cone data sampled from the bent-cone buffer
// Returns:
//	_wsBentNormal, the world-space normalized bent-normal direction
//	_cosConeAngle, the cosine of the cone half aperture angle. NOTE: AO = 1-_cosConeAngle
//	_stdDeviationAO, the standard deviation of the AO value, that can be thought of as the deviation in angle since stdDevAngle = acos( 1 - _stdDeviationAO )
//						(beware that the average AO + or - this standard deviation could exceed the [0,1] interval so DO clamp!)
//
void	ReconstructBentCone( float3 _wsView, float3 _wsCameraUp, float4 _lcsBentCone, out float3 _csBentNormal, out float3 _wsBentNormal, out float _cosConeAngle, out float _stdDeviationAO ) {
	// Extract information from the packed bent cone data
	_cosConeAngle = length( _lcsBentCone.xyz );	// Technically never 0
	_csBentNormal = _lcsBentCone.xyz / _cosConeAngle;
	_stdDeviationAO = _lcsBentCone.w;
	_cosConeAngle *= _cosConeAngle;	// Cosine was stored sqrt

	// Rebuild local camera space
	float3	wsRight = normalize( cross( _wsView, _wsCameraUp ) );
	float3	wsUp = cross( wsRight, _wsView );

	// Transform local camera-space bent cone back into world space
	_wsBentNormal = _csBentNormal.x * wsRight + _csBentNormal.y * wsUp - _csBentNormal.z * _wsView;
}
