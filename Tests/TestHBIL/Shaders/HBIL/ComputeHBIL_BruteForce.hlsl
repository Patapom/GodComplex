////////////////////////////////////////////////////////////////////////////////
// Shaders to compute HBIL
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "HBIL/HBIL.hlsl"

#define MAX_ANGLES	16									// Amount of circle subdivisions per pixel
#define MAX_SAMPLES	16									// Maximum amount of samples per circle subdivision

Texture2D< float >	_tex_depth : register(t0);			// Depth or distance buffer (here we're given depth)
Texture2D< float3 >	_tex_normal : register(t1);			// Camera-space normal vectors
Texture2D< float3 >	_tex_sourceRadiance : register(t2);	// Last frame's reprojected radiance buffer
Texture2D< float >	_tex_blueNoise : register(t3);

cbuffer CB_HBIL : register( b3 ) {
	float4	_bilateralValues;
	float	_gatherSphereMaxRadius_m;		// Radius of the sphere that will gather our irradiance samples (in meters)
	float	_gatherSphereMaxRadius_p;		// Radius of the sphere that will gather our irradiance samples (in pixels)
	float	_temporalAttenuationFactor;		// Attenuation factor of radiance from previous frame
};

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

////////////////////////////////////////////////////////////////////////////////
// Implement the methods expected by the HBIL header
float	FetchDepth( float2 _pixelPosition, float _mipLevel ) {
//	return _tex_sourceRadiance.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel ).w;
	return Z_FAR * _tex_depth.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel );
//	return Z_FAR * _tex_depth[_pixelPosition];
}

float3	FetchNormal( float2 _pixelPosition, float _mipLevel ) {
//	return _tex_normal[round( _pixelPosition )];
	return _tex_normal.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel );
}

float3	FetchRadiance( float2 _pixelPosition, float _mipLevel ) {
	return _tex_sourceRadiance.SampleLevel( LinearClamp, _pixelPosition / _resolution, _mipLevel ).xyz;
}

// This clearly doesn't work: nasty silhouettes show up around objects
float	ComputeMipLevel_Depth( float _radius_pixels, float _stepSize_pixels ) {
//return _debugMipIndex;
	float	pixelArea = PI / (2.0 * MAX_ANGLES) * 2.0 * _radius_pixels * _stepSize_pixels;
	return 0.5 * log2( pixelArea ) * _bilateralValues.y;
//	return 0.5 * log2( pixelArea ) * float2( 0, 2 );	// Unfortunately, sampling lower mips for depth gives very nasty halos! Maybe use max depth? Meh. Not conclusive either...
//	return 1.5 * 0.5 * log2( pixelArea );
//	return 0.5 * log2( pixelArea );
}

float	ComputeMipLevel_Radiance( float2 _ssPosition, float _centralZ, float _currentZ, float _radius_meters ) {
	float	deltaZ = _centralZ - _currentZ;
	float	distance = sqrt( _radius_meters*_radius_meters + deltaZ*deltaZ );	// Distance from origin
	float	sphereRadius_meters = (0.5 * PI / MAX_ANGLES) * distance;			// Radius of the sphere (in meters) that will serve as footprint for mip computation

	// Estimate how many pixels a disc
	float	screenSize_m = 2.0 * TAN_HALF_FOV * _currentZ;						// Size (in meters) covered by the entire screen at current Z
	float	meters2Pixels = _resolution.y / screenSize_m;
	float	sphereRadius_pixels = meters2Pixels * sphereRadius_meters;			// Radius of the sphere (in pixels)

//	return _bilateralValues.y * log2( sphereRadius_pixels );
	return 1.5 * log2( sphereRadius_pixels );	// Empirical

//	float	pixelArea = PI * sphereRadius_pixels * sphereRadius_pixels;			// Area of the disc covered by the sphere (in square pixels)
//	return _bilateralValues.y * 0.5 * log2( pixelArea );
}

float2	BilateralFilterDepth( float2 _ssCentralPosition, float _centralZ, float3 _lcsCentralNormal, float2 _ssCurrentPosition, float _currentZ, float _radius_meters, float _horizonCosTheta, float _newCosTheta ) {

	// Rebuild camera-space positions & normals
	float2	UV0 = _ssCentralPosition / _resolution;
	float3	csView0 = BuildCameraRay( UV0 );
	float3	csPos0 = _centralZ * csView0;
			csView0 = normalize( csView0 );
	float3	csAt0 = -csView0;
	float3	csRight0 = normalize( cross( csAt0, float3( 0, 1, 0 ) ) );
	float3	csUp0 = cross( csRight0, csAt0 );
	float3	csNormal0 = _lcsCentralNormal.x * csRight0 + _lcsCentralNormal.y * csUp0 + _lcsCentralNormal.z * csAt0;

	float2	UV1 = _ssCurrentPosition / _resolution;
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

float3	SampleIrradiance_TEMP( float2 _ssCentralPosition, float2 _ssPosition, float2 _ssStep, float _radius_meters, float2 _sinCosGamma, float _centralZ, float3 _csCentralNormal, float _mipLevelDepth, float2 _integralFactors, inout float _radianceSamplingWeight, inout float3 _previousRadiance, inout float _maxCosTheta ) {

	// Read new Z and compute new horizon angle candidate
	float	Z = FetchDepth( _ssPosition, _mipLevelDepth );
	float	deltaZ = _centralZ - Z;																// Z difference, in meters
	float	recHypo = rsqrt( _radius_meters*_radius_meters + deltaZ*deltaZ );					// 1 / sqrt( z� + r� )
	float	cosTheta = (_sinCosGamma.x * _radius_meters + _sinCosGamma.y * deltaZ) * recHypo;	// cos(theta) = [sin(gamma)*r + cos(gamma)*z] / sqrt( z� + r� )

	// Filter outlier horizon values
//float	previousZ = 0.0;	// NEEDED?
	float2	bilateralWeights = BilateralFilterDepth( _ssCentralPosition, _centralZ, _csCentralNormal, _ssPosition, Z, _radius_meters, _maxCosTheta, cosTheta );
	_radianceSamplingWeight *= bilateralWeights.x;
//	cosTheta = lerp( -1.0, cosTheta, bilateralWeight );	// Flatten if rejected

	// Update any rising horizon
	if ( cosTheta <= _maxCosTheta )
		return 0.0;	// Below the horizon... No visible contribution.

	#if SAMPLE_NEIGHBOR_RADIANCE
		float	mipLevel_Radiance = ComputeMipLevel_Radiance( _ssPosition, _centralZ, Z, _radius_meters );
//		_previousRadiance = FetchRadiance( _ssPosition, mipLevel_Radiance );
//		_previousRadiance = FetchRadiance( _ssPosition - _bilateralValues.w * _ssStep, 0.0 );
		if ( _radianceSamplingWeight > 0.0 ) {
//			_previousRadiance = lerp( _previousRadiance, FetchRadiance( _ssPosition, 0.0 ), _radianceSamplingWeight );	// Accept new radiance depending on accumulated sampling weight...
			float3	newRadiance = bilateralWeights.y * FetchRadiance( _ssPosition, 0.0 );
			_previousRadiance = lerp( _previousRadiance, newRadiance, _radianceSamplingWeight );	// Accept new radiance depending on accumulated sampling weight...
		}
	#endif

	// Integrate over horizon difference (always from smallest to largest angle otherwise we get negative results!)
	float3	incomingRadiance = _previousRadiance * IntegrateSolidAngle( _integralFactors, cosTheta, _maxCosTheta );

// #TODO: Integrate with linear interpolation of irradiance as well??
// #TODO: Integrate with Fresnel F0!

	_maxCosTheta = cosTheta;	// Register a new positive horizon
//	_previousDeltaZ = deltaZ;		// Accept new depth difference

	return incomingRadiance;
}

float3	GatherIrradiance_TEMP( float2 _ssPosition, float2 _ssStep, float2 _ssMaxPosition, float2 _csDirection, float3 _csNormal, float _noise, float2 _sinCosGamma, float _stepSize_meters, uint _stepsCount, float _centralZ, float3 _centralRadiance, out float3 _csBentNormal, out float _AO, inout float4 _DEBUG ) {

	// Pre-compute factors for the integrals
	float2	integralFactors_Front = ComputeIntegralFactors( _csDirection, _csNormal );
//	float2	integralFactors_Back = ComputeIntegralFactors( -_csDirection, _csNormal );
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
	float3	previousRadiance_Front = _centralRadiance;
	float3	previousRadianceBack = _centralRadiance;
//*
	float	radius_meters = 0.0;
//float	radius_pixels = 0.0;
//float	stepSize_pixels = length(_ssStep);
	float2	ssPosition_Front = _ssPosition - _noise * _ssStep;
	float2	ssPosition_Back = _ssPosition + _noise * _ssStep;
	float	maxCosTheta_Front = planeCosTheta_Front;
	float	maxCosTheta_Back = planeCosTheta_Back;
	float	radianceSamplingWeight_Front = 1.0;
	float	radianceSamplingWeight_Back = 1.0;
	[loop]
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		radius_meters += _stepSize_meters;
		ssPosition_Front += _ssStep;
		ssPosition_Back -= _ssStep;

//radius_pixels += stepSize_pixels;
//float	mipLevel = ComputeMipLevel_Depth( radius_pixels, stepSize_pixels );
float	mipLevel = 0.0;

		sumRadiance += SampleIrradiance_TEMP( _ssPosition, ssPosition_Front, _ssStep, radius_meters, _sinCosGamma, _centralZ, _csNormal, mipLevel, integralFactors_Front, radianceSamplingWeight_Front, previousRadiance_Front, maxCosTheta_Front );
		sumRadiance += SampleIrradiance_TEMP( _ssPosition, ssPosition_Back, -_ssStep, radius_meters, float2( -_sinCosGamma.x, _sinCosGamma.y ), _centralZ, _csNormal, mipLevel, integralFactors_Back, radianceSamplingWeight_Back, previousRadianceBack, maxCosTheta_Back );
	}
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


_DEBUG = float4( _csBentNormal, 0 );


	return sumRadiance;
}

////////////////////////////////////////////////////////////////////////////////
// Computes bent cone & irradiance gathering from pixel's surroundings
struct PS_OUT {
	float4	irradiance : SV_TARGET0;
	float4	bentCone : SV_TARGET1;
};

PS_OUT	PS( float4 __Position : SV_POSITION ) {
	float2	UV = __Position.xy / _resolution;
	uint2	pixelPosition = uint2( floor( __Position.xy ) );
//	float	noise = frac( _time + _tex_blueNoise[pixelPosition & 0x3F] );
//	float	noise = frac( _time + _tex_blueNoise[pixelPosition & 0x3F] );
//	float	noise = _tex_blueNoise[uint2(pixelPosition + float2( _time, 0 )) & 0x3F];
	float	noise = frac( _tex_blueNoise[pixelPosition & 0x3F] + SQRT2 * _framesCount );	// ACTUAL GOOD VALUE!
	float	noise2 = frac( _time + _tex_blueNoise[uint2( float2( 32659.167 * UV.x, 173227.3 * UV.y ) ) & 0x3F] );
//	float	noise = 0.0;
//	float	noise2 = frac( sin( 14357.91 * noise ) );

//noise2 = lerp( 0.1, 1.0, noise );

	// Setup camera ray
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;

	// Read back depth, normal & central radiance value from last frame
//	float	Z = FetchDepth( pixelPosition, 0.0 );
	float	Z = Z_FAR * _tex_depth[pixelPosition];

			Z -= 1e-2;	// !IMPORTANT! Prevent acnea by offseting the central depth a tiny bit closer

	float3	centralRadiance = _tex_sourceRadiance[pixelPosition].xyz;	// Read back last frame's radiance value that we can use as a fallback for neighbor areas

	// Compute local camera-space
	float3	wsPos = _camera2World[3].xyz + Z * Z2Distance * wsView;
	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	wsAt = -wsView;


	// Express local camera-space vectors in global camera-space
	float3	gcsRight = float3( dot( wsRight, _camera2World[0].xyz ), dot( wsRight, _camera2World[1].xyz ), dot( wsRight, _camera2World[2].xyz ) );
	float3	gcsUp = float3( dot( wsUp, _camera2World[0].xyz ), dot( wsUp, _camera2World[1].xyz ), dot( wsUp, _camera2World[2].xyz ) );



// Simulate perfect alignment
//gcsRight = float3( 1, 0, 0 );
//gcsUp = float3( 0, 1, 0 );



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
	//	|  \   theta  -* Z1
	//	|              |
	//	|   \          |
	//	|              |
	//	|    \         |
	//	|              |
	//	|  a  \        |
	//
	// We walk from x to x' along the global camera space's X axis for a distance R and we sample a new depth Z1 (along the camera's Z axis)
	// The local camera space's view axis makes an angle "a" with the global camera space's Z axis (the vertical axis).
	// Its tangent plane (represented at the top, starting slant from x) also makes an angle alpha with the screen plane represented by X
	// We are looking for cos(theta) and we simply note that theta + a + b = PI/2
	// Thus, cos(theta) = cos( PI/2 - a - b ) = sin( a + b ) = sin(a)*cos(b) + cos(a)*sin(b)
	// We easily find that cos(b) = R / sqrt( z� + R� ) and sin(b) = z / sqrt( z� + R� )
	// So finaly:
	//	cos(theta) = [sin(a)*R + cos(a)*z] / sqrt( z� + R� )
	//
	// We notice that it behaves a bit like a rotation and is finally an angular interpolation between sin(b) and cos(b) that depends on the camera axis deviation...
	//
	float2	sinCosGamma;
//	sinCosGamma.y = csView.z;// dot( wsView, _camera2World[2].xyz );
//	sinCosGamma.x = sqrt( 1.0 - sinCosGamma.y*sinCosGamma.y );
//	sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!


// Simulate perfect alignment
//sinCosGamma = float2( 0, 1 );


	// Compute local camera-space normal
	float3	csNormal = _tex_normal[pixelPosition];
			csNormal.z = max( 1e-3, csNormal.z );	// Make sure it's never 0!

	// Compute screen radius of gather sphere
	float	screenSize_m = 2.0 * Z * TAN_HALF_FOV;																	// Vertical size of the screen in meters when extended to distance Z
	float	meter2Pixel = _resolution.y / screenSize_m;																// Gives us the conversion factor to go from meters to pixels
	float	sphereRadius_pixels = meter2Pixel * _gatherSphereMaxRadius_m;
			sphereRadius_pixels = min( _gatherSphereMaxRadius_p, sphereRadius_pixels );								// Prevent it to grow larger than our fixed limit
	float	radiusStepSize_pixels = max( 1.0, sphereRadius_pixels / MAX_SAMPLES );									// This gives us our radial step size in pixels
	uint	samplesCount = clamp( uint( ceil( sphereRadius_pixels / radiusStepSize_pixels ) ), 1, MAX_SAMPLES );	// Reduce samples count if possible
//	float	radiusStepSize_meters = sphereRadius_pixels / (samplesCount * meter2Pixel);								// This gives us our radial step size in meters
	float	radiusStepSize_meters = radiusStepSize_pixels / (csView.z * meter2Pixel);								// This gives us our radial step size in meters

	// Start gathering radiance and bent normal by subdividing the screen-space disk around our pixel into N slices
	float4	GATHER_DEBUG = 0.0;
	float3	sumIrradiance = 0.0;
	float	sumAO = 0.0;
	float3	csAverageBentNormal = 0.0;
	float	averageAO = 0.0;
	float	varianceAO = 0.0;
//	float	phiNoise = Bayer1D_16( _framesCount ) / 16.0f;
	float	phiNoise = 2.6457513110645905905016157536393 * _framesCount;
//phiNoise = 0.0;
//phiNoise = noise;
//noise = 0.0;

#if MAX_ANGLES > 1
	[loop]
	for ( uint angleIndex=0; angleIndex < MAX_ANGLES; angleIndex++ )
#else
	uint	angleIndex = 0;
#endif
	{
		float	phi = (angleIndex + phiNoise) * PI / MAX_ANGLES;

		// Build camera-space and screen-space walk directions
		float2	csDirection;
		sincos( phi, csDirection.y, csDirection.x );

		float3	gcsDirection = csDirection.x * gcsRight + csDirection.y * gcsUp;	// Since csDirection gives us the components along local camera-space's right and up vectors, and we know their expression in global camera-space, it's easy find the equivalent global camera-space direction...

		float2	ssDirection = normalize( gcsDirection.xy );							// We normalize since we want to take integer pixel steps
				ssDirection.y = -ssDirection.y;										// Going upward in camera space means going downward in screen space...
				ssDirection *= radiusStepSize_pixels;								// Scale by our step size

		// Build angular correction based on deviation from central camera axis
		sinCosGamma.x = gcsDirection.z;
		sinCosGamma.y = sqrt( 1.0 - sinCosGamma.x*sinCosGamma.x );
		sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

		// Gather irradiance and average cone direction for that slice
		float3	csBentNormal;
		float	AO;
		sumIrradiance += GatherIrradiance_TEMP( __Position.xy, ssDirection, _resolution, csDirection, csNormal, noise, sinCosGamma, radiusStepSize_meters, samplesCount, Z, centralRadiance, csBentNormal, AO, GATHER_DEBUG );
		csAverageBentNormal += csBentNormal;
		sumAO += AO;

		// We're using running variance computation from https://www.johndcook.com/blog/standard_deviation/
		//	Avg(N) = Avg(N-1) + [V(N) - Avg(N-1)] / N
		//	S(N) = S(N-1) + [V(N) - Avg(N-1)] * [V(N) - Avg(N)]
		// And variance = S(finalN) / (finalN-1)
		//
		float	previousAverageAO = averageAO;
		averageAO += (AO - averageAO) / (1+angleIndex);
		varianceAO += (AO - previousAverageAO) * (AO - averageAO);
	}

	// Finalize bent cone
	csAverageBentNormal = normalize( csAverageBentNormal );

	// Use AO to compute cone angle
	#if USE_NORMAL_INFLUENCE_FOR_AO
		sumAO /= MAX_ANGLES;		// Normalize
	#else
		sumAO /= 2.0 * MAX_ANGLES;	// Normalize (remember here that the returned AO is in [0,2] range for each slice)
	#endif

	float	cosAverageConeAngle = 1.0 - sumAO;

	#if MAX_ANGLES > 1
		varianceAO /= MAX_ANGLES;
	#endif
//	float	stdDeviation = PI * sqrt( varianceAO );		// Technically in [0,2PI]
	float	stdDeviation = 0.5 * sqrt( varianceAO );	// Now AO standard deviation in [0,1]

//stdDeviation = 0.0;

	// Finalize irradiance
	sumIrradiance *= PI / MAX_ANGLES;
	sumIrradiance = max( 0.0, sumIrradiance );

	// Write result
	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0 );


// Debugging the RGB10A2 and R11G11B10_FLOAT formats
//Out.irradiance = float4( 0, 0, 0, 0.75 / 4.0 );
//Out.irradiance = float4( 0, 0, 0, 1.999 / 3.0 );
//Out.irradiance = float4( 1, 0, 0, 1 );



	// [18/02/13] RGBA8_SNORM requires some encoding
//	Out.bentCone = float4( max( 0.01, cosAverageConeAngle ) * csAverageBentNormal, stdDeviation );
	Out.bentCone = float4( max( 0.01, sqrt( cosAverageConeAngle ) ) * csAverageBentNormal, stdDeviation );

//const float	MIN_ENCODABLE_VALUE = 1.0 / 128.0;
//csAverageBentNormal = sqrt( max( MIN_ENCODABLE_VALUE, cosAverageConeAngle ) ) * csAverageBentNormal;
//csAverageBentNormal = csNormal;
//Out.bentCone = float4( csAverageBentNormal, stdDeviation );


//////////////////////////////////////////////
// WRITE DEBUG VALUE INTO BENT CONE BUFFER
float3	DEBUG_VALUE = float3( 1,0,1 );
DEBUG_VALUE = csNormal;
DEBUG_VALUE = csAverageBentNormal;
DEBUG_VALUE = csAverageBentNormal.x * wsRight + csAverageBentNormal.y * wsUp + csAverageBentNormal.z * wsAt;	// World-space normal
//DEBUG_VALUE = cosAverageConeAngle;
//DEBUG_VALUE = dot( ssAverageBentNormal, N );
//DEBUG_VALUE = 0.01 * Z;
//DEBUG_VALUE = sphereRadius_pixels / _gatherSphereMaxRadius_p;
//DEBUG_VALUE = 0.1 * (radiusStepSize_pixels-1);
//DEBUG_VALUE = 0.5 * float(samplesCount) / MAX_SAMPLES;
//DEBUG_VALUE = varianceConeAngle;
//DEBUG_VALUE = stdDeviation;
//DEBUG_VALUE = float3( GATHER_DEBUG.xy, 0 );
//DEBUG_VALUE = float3( GATHER_DEBUG.zw, 0 );
//DEBUG_VALUE = 0.4 * localCamera2World[3];
DEBUG_VALUE = GATHER_DEBUG.xyz;
//Out.bentCone = float4( DEBUG_VALUE, 1 );
//
//////////////////////////////////////////////

	return Out;
}
