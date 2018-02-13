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


// We assume there exist such methods declared somewhere where _pixelPosition is the position in screen space
float	FetchDepth( float2 _pixelPosition, float _mipLevel );							// The return value must be the depth (in meters)
float3	FetchRadiance( float2 _pixelPosition, float _mipLevel );						// The return value must be the radiance from last frame (DIFFUSE ONLY!)
float	BilateralFilterDepth( float _centralZ, float _neighborZ, float _radius_m );		// The return value must be a [0,1] weight telling whether or not we accept the sample at the specified radius and depth (in meter)
float	BilateralFilterRadiance( float _centralZ, float _neighborZ, float _radius_m );	// The return value must be a [0,1] weight telling whether or not we accept the sample at the specified radius and depth (in meter)
float2	ComputeMipLevel( float2 _radius, float2 _radialStepSizes );

// Integrates the dot product of the normal with a vector interpolated between slice horizon angle theta0 and theta1
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
//	I0 = [cos(phi).Nx] * Integral[theta0, theta1]{ sin²(theta) dTheta } = [cos(phi).Nx] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I1 = [sin(phi).Ny] * Integral[theta0, theta1]{ sin²(theta) dTheta } = [sin(phi).Ny] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I2 = Nz * Integral[theta0, theta1]{ cos(theta).sin(theta) dTheta } = Nz * { 1/2 * cos²(theta)) }[theta1,theta0]	<== Watch out for theta reverse here!
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
	return 0.5 * (_integralFactors.x * I0 + _integralFactors.y * I1);
}
float2	ComputeIntegralFactors( float2 _csDirection, float3 _N ) {
	return float2( dot( _N.xy, _csDirection ), _N.z );
}

// Samples the irradiance reflected from a screen-space position located around the center pixel position
//	_ssPosition, the screen-space position to sample
//	_H0, the center height
//	_radius_m, the radius (in meters) from the center position
//	_integralFactors, some pre-computed factors to feed the integral
//	_maxCos, the floating maximum cos(theta) that indicates the angle of the perceived horizon
//	_optionnal_centerRho, the reflectance of the center pixel (fallback only necessary if no albedo map is available and if it's only irradiance that is stored in the source irradiance map instead of radiance, in which case albedo is already pre-mutliplied)
//
float3	SampleIrradiance( float2 _ssPosition, float _H0, float _radius, float2 _mipLevel, float2 _integralFactors, inout float3 _previousRadiance, inout float _maxCos ) {

	// Sample new height and update horizon angle
	float	neighborH = FetchDepth( _ssPosition, _mipLevel.x );
	float	deltaH = _H0 - neighborH;
			deltaH *= BilateralFilterDepth(  _H0, neighborH, _radius );
	float	H2 = deltaH * deltaH;
	float	hyp2 = _radius * _radius + H2;		// Square hypotenuse
	float	cosHorizon = deltaH / sqrt( hyp2 );	// Cosine to horizon angle
	if ( cosHorizon <= _maxCos )
		return 0.0;	// Below the horizon... No visible contribution.

	#if SAMPLE_NEIGHBOR_RADIANCE
// NOW USELESS I THINK...
//		// Sample neighbor's incoming radiance value, only if difference in depth is not too large
//		float	bilateralWeight = BilateralFilterRadiance( _H0, neighborH, _radius );
//		if ( bilateralWeight > 0.0 )
//			_previousRadiance = lerp( _previousRadiance, FetchRadiance( _ssPosition ), bilateralWeight );	// Accept new height and its radiance value

		// Sample always (actually, it's okay now we accepted the height through the first bilateral filter earlier)
		_previousRadiance = FetchRadiance( _ssPosition, _mipLevel.y );
	#endif

	// Integrate over horizon difference (always from smallest to largest angle otherwise we get negative results!)
	float3	incomingRadiance = _previousRadiance * IntegrateSolidAngle( _integralFactors, cosHorizon, _maxCos );

// #TODO: Integrate with linear interpolation of irradiance as well??
// #TODO: Integrate with Fresnel F0!

	_maxCos = cosHorizon;		// Register a new positive horizon

	return incomingRadiance;
}

////////////////////////////////////////////////////////////////////////////////
// Gathers the irradiance around the central position
//	_ssPosition, the central screen-space position (in pixel) where to gather from
//	_ssDirection, the screen-space direction of the disc slice we're sampling
//	_csDirection, the camear-space direction of the disc slice we're sampling
//	_Z0, the central depth value (WARNING: Always offset it toward the camera by a little epsilon to avoid horizon acnea)
//	_csNormal, camera-space normal
//	_radialStepSizes, size of a radial step to jump from one sample to another (X=step size in pixels, Y=step size in meters)
//	_stepsCount, amount of steps to take (front & back so actual samples count will always be twice that value!)
//	_centralRadiance, last frame's radiance at central position that we can always use as a safe backup for irradiance integration (in case bilateral filter rejects neighbor height as too different)
//
// Returns:
//	[OUT] _csBentNormal, the average bent normal for the slice (Warning: NOT NORMALIZED, and must be accumulated unnormalized otherwise result will get biased!)
//	[OUT] _coneAngles, the front & back cone angles from the direction of the bent normal to the front & back horizons
//	The irradiance gathered along the sampling
//
float3	GatherIrradiance( float2 _ssPosition, float2 _ssDirection, float2 _csDirection, float _Z0, float3 _csNormal, float2 _radialStepSizes, uint _stepsCount, float3 _centralRadiance, out float3 _csBentNormal, out float2 _coneAngles, inout float4 _DEBUG ) {

	// Pre-compute factors for the integrals
	float2	integralFactors_Front = ComputeIntegralFactors( _csDirection, _csNormal );
	float2	integralFactors_Back = ComputeIntegralFactors( -_csDirection, _csNormal );

	// Compute initial cos(angle) for front & back horizons
	// We do that by projecting the screen-space direction ssDirection onto the tangent plane given by the normal
	//	then the cosine of the angle from the Z axis is simply given by the Pythagorean theorem:
	//                             P
	//			   N\  |Z		  -*-
	//				 \ |	  ---  ^
	//				  \|  ---      |
	//             --- *..........>+ ssDirection
	//        --- 
	//
	float	hitDistance_Front = -dot( _csDirection, _csNormal.xy ) * (abs(_csNormal.z) > 1e-6 ? 1.0 / _csNormal.z : 0.0);
	float	maxCosTheta_Front = hitDistance_Front / sqrt( hitDistance_Front*hitDistance_Front + dot(_csDirection,_csDirection) );
	float	maxCosTheta_Back = -maxCosTheta_Front;	// Back cosine is simply the mirror value

	// Gather irradiance from front & back directions while updating the horizon angles at the same time
	float3	sumRadiance = 0.0;
	float2	radius = 0.0;
	float2	ssStep = _radialStepSizes.x * _ssDirection;
	float2	ssPosition_Front = _ssPosition;
	float2	ssPosition_Back = _ssPosition;
	float3	previousRadiance_Front = _centralRadiance;
	float3	previousRadianceBack = _centralRadiance;
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		radius += _radialStepSizes;
		ssPosition_Front += ssStep;
		ssPosition_Back -= ssStep;

		float2	mipLevel = ComputeMipLevel( radius, _radialStepSizes );

		sumRadiance += SampleIrradiance( ssPosition_Front, _Z0, radius.y, mipLevel, integralFactors_Front, previousRadiance_Front, maxCosTheta_Front );
		sumRadiance += SampleIrradiance( ssPosition_Back, _Z0, radius.y, mipLevel, integralFactors_Back, previousRadianceBack, maxCosTheta_Back );
	}

	// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
	#if USE_NUMERICAL_INTEGRATION
		// Half brute force where we perform the integration numerically as a sum...
		//
		float	thetaFront = acos( maxCosTheta_Front );
		float	thetaBack = -acos( maxCosTheta_Back );

		_csBentNormal = 0.001 * _csNormal;
		for ( uint i=0; i < USE_NUMERICAL_INTEGRATION; i++ ) {
			float	theta = lerp( thetaBack, thetaFront, (i+0.5) / USE_NUMERICAL_INTEGRATION );
			float	sinTheta, cosTheta;
			sincos( theta, sinTheta, cosTheta );
			float3	ssUnOccludedDirection = float3( sinTheta * _csDirection, cosTheta );

			float	cosAlpha = saturate( dot( ssUnOccludedDirection, _csNormal ) );

			float	weight = cosAlpha * abs(sinTheta);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
			_csBentNormal += weight * ssUnOccludedDirection;
		}

		float	dTheta = (thetaFront - thetaBack) / USE_NUMERICAL_INTEGRATION;
		_csBentNormal *= dTheta;
	#else
		// Analytical solution
		float	cosTheta0 = maxCosTheta_Front;
		float	cosTheta1 = maxCosTheta_Back;
		float	sinTheta0 = sqrt( 1.0 - cosTheta0*cosTheta0 );
		float	sinTheta1 = sqrt( 1.0 - cosTheta1*cosTheta1 );
		float	cosTheta0_3 = cosTheta0*cosTheta0*cosTheta0;
		float	cosTheta1_3 = cosTheta1*cosTheta1*cosTheta1;
		float	sinTheta0_3 = sinTheta0*sinTheta0*sinTheta0;
		float	sinTheta1_3 = sinTheta1*sinTheta1*sinTheta1;

		float2	sliceSpaceNormal = float2( dot( _csNormal.xy, _csDirection ), _csNormal.z );

		float	averageX = sliceSpaceNormal.x * (cosTheta0_3 + cosTheta1_3 - 3.0 * (cosTheta0 + cosTheta1) + 4.0)
						 + sliceSpaceNormal.y * (sinTheta0_3 - sinTheta1_3);

		float	averageY = sliceSpaceNormal.x * (sinTheta0_3 - sinTheta1_3)
						 + sliceSpaceNormal.y * (2.0 - cosTheta0_3 - cosTheta1_3);

		_csBentNormal = float3( averageX * _csDirection, averageY );
	#endif

	// DON4T
	_csBentNormal = normalize( _csBentNormal );

	// Compute cone angles
	float3	csHorizon_Front = float3( sqrt( 1.0 - maxCosTheta_Front*maxCosTheta_Front ) * _csDirection, maxCosTheta_Front );
	float3	csHorizon_Back = float3( -sqrt( 1.0 - maxCosTheta_Back*maxCosTheta_Back ) * _csDirection, maxCosTheta_Back );
	#if USE_FAST_ACOS
		_coneAngles.x = FastPosAcos( saturate( dot( _csBentNormal, csHorizon_Front ) ) );
		_coneAngles.y = FastPosAcos( saturate( dot( _csBentNormal, csHorizon_Back ) ) ) ;
	#else
		_coneAngles.x = acos( saturate( dot( _csBentNormal, csHorizon_Front ) ) );
		_coneAngles.y = acos( saturate( dot( _csBentNormal, csHorizon_Back ) ) );
	#endif


#if AVERAGE_COSINES
_coneAngles = float2( saturate( dot( _csBentNormal, csHorizon_Front ) ), saturate( dot( _csBentNormal, csHorizon_Back ) ) );
#endif


	return sumRadiance;
}


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