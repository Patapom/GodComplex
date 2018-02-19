////////////////////////////////////////////////////////////////////////////////
// Shaders to compute HBIL
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "HBIL.hlsl"

//#define MAX_ANGLES	16									// Amount of circle subdivisions per pixel
#define MAX_ANGLES	2									// Amount of circle subdivisions per pixel
#define MAX_SAMPLES	16									// Maximum amount of samples per circle subdivision

Texture2DArray< float >		_tex_splitDepth : register(t0);		// Depth or distance buffer (here we're given depth)
Texture2DArray< float3 >	_tex_splitNormal : register(t1);	// World-space normal vectors
Texture2DArray< float3 >	_tex_splitRadiance : register(t2);	// Last frame's reprojected radiance buffer
//Texture2D< float >	_tex_blueNoise : register(t3);

cbuffer CB_HBIL : register( b3 ) {
	uint2	_targetResolution;				// Small render-target resolution (1/4 the screen resolution)
	float2	_csDirection;					// Sampling direction in camera space

	uint3	_renderPassIndex;				// X=Index of the X pass in [0,3], Y=Index of the Y pass in [0,3], Z=Index of the render pass in [0,15]
	float	_gatherSphereMaxRadius_m;		// Radius of the sphere that will gather our irradiance samples (in meters)

	float4	_bilateralValues;

	float	_gatherSphereMaxRadius_p;		// Radius of the sphere that will gather our irradiance samples (in pixels)
	float	_temporalAttenuationFactor;		// Attenuation factor of radiance from previous frame
};

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

////////////////////////////////////////////////////////////////////////////////
// Implement the methods expected by the HBIL header
float	FetchDepth( float2 _pixelPosition, float _mipLevel ) {
	return Z_FAR * _tex_splitDepth.SampleLevel( LinearClamp, float3( _pixelPosition / _targetResolution, _renderPassIndex.z ), _mipLevel );
}

float3	FetchRadiance( float2 _pixelPosition, float _mipLevel ) {
	return _tex_splitRadiance.SampleLevel( LinearClamp, float3( _pixelPosition / _targetResolution, _renderPassIndex.z ), _mipLevel ).xyz;
}

// This clearly doesn't work: nasty silhouettes show up around objects
float	ComputeMipLevel_Depth( float _radius_pixels, float _stepSize_pixels ) {
return 0.0;
//return _debugMipIndex;
//	float	pixelArea = PI / (2.0 * MAX_ANGLES) * 2.0 * _radius_pixels * _stepSize_pixels;
//	return 0.5 * log2( pixelArea ) * _bilateralValues.y;
////	return 0.5 * log2( pixelArea ) * float2( 0, 2 );	// Unfortunately, sampling lower mips for depth gives very nasty halos! Maybe use max depth? Meh. Not conclusive either...
////	return 1.5 * 0.5 * log2( pixelArea );
////	return 0.5 * log2( pixelArea );
}

float	ComputeMipLevel_Radiance( float _centralZ, float _currentZ, float _radius_meters ) {
	float	deltaZ = _centralZ - _currentZ;
	float	distance = sqrt( _radius_meters*_radius_meters + deltaZ*deltaZ );	// Distance from origin
	float	sphereRadius_meters = (0.5 * PI / MAX_ANGLES) * distance;			// Radius of the sphere (in meters) that will serve as footprint for mip computation

	// Estimate how many pixels a disc
	float	screenSize_m = 2.0 * TAN_HALF_FOV * _currentZ;						// Size (in meters) covered by the entire screen at current Z
	float	meters2Pixels = _targetResolution.y / screenSize_m;
	float	sphereRadius_pixels = meters2Pixels * sphereRadius_meters;			// Radius of the sphere (in pixels)

//	return _bilateralValues.y * log2( sphereRadius_pixels );
	return 1.5 * log2( sphereRadius_pixels );	// Empirical

//	float	pixelArea = PI * sphereRadius_pixels * sphereRadius_pixels;			// Area of the disc covered by the sphere (in square pixels)
//	return _bilateralValues.y * 0.5 * log2( pixelArea );
}

float	BilateralFilterDepth( float _centralZ, float _previousDeltaZ, float _newDeltaZ, float _horizonCosTheta, float _newCosTheta, float _radius_meters ) {
//Il fout grave la merde!

//return 1.0 - pow( saturate( _bilateralValues.x * _radius_m / _gatherSphereMaxRadius_m ), 2.0 * _bilateralValues.y );	// As per http://developer.download.nvidia.com/presentations/2008/SIGGRAPH/HBAO_SIG08b.pdf pp. 23
return 1;
/*
	// Compute an horizon penalty when the horizon rises too quickly
	float	deltaTheta = saturate( (acos(_horizonCosTheta) - acos(_newCosTheta)) / PI );
//	float	penaltyCos = saturate( (deltaTheta - _bilateralValues.x) / _bilateralValues.y );
float	penaltyCos = saturate( (deltaTheta - 0.0) / 1.0 );

//return 1.0 - penaltyCos;

	// Compute a delta Z penalty when the depth rises too quickly
	float	relativeZ0 = max( 0.0, _previousDeltaZ ) / _centralZ;
	float	relativeZ1 = max( 0.0, _newDeltaZ ) / _centralZ;
//	float	penaltyZ = 1.0 - saturate( (relativeZ0 - relativeZ1 - _bilateralValues.z) / _bilateralValues.w );
float	penaltyZ = 1.0 - saturate( (relativeZ0 - relativeZ1 - 0.5) / 1.0 );

//return 1.0 - penaltyZ;

	// If the penalty flag is raised, we accept rising the horizon only if the difference in relative depth is not too high
//	return 1.0 - penaltyCos * penaltyZ / (40.0*_radius_m);
	return 1.0 - penaltyCos * penaltyZ / (100.0*_bilateralValues.x*_radius_m);
*/
}


float3	SampleIrradiance_TEMP( float2 _ssPosition, float _radius_meters, float2 _sinCosGamma, float _centralZ, float _mipLevelDepth, float2 _integralFactors, inout float3 _previousRadiance, inout float _maxCosTheta ) {

	// Read new Z and compute new horizon angle candidate
	float	Z = _centralZ - FetchDepth( _ssPosition, _mipLevelDepth );						// Z difference, in meters
	float	recHypo = rsqrt( _radius_meters*_radius_meters + Z*Z );							// 1 / sqrt( z² + r² )
	float	cosTheta = (_sinCosGamma.x * _radius_meters + _sinCosGamma.y * Z) * recHypo;	// cos(theta) = [sin(gamma)*r + cos(gamma)*z] / sqrt( z² + r² )

	// Filter outlier horizon values
//float	previousZ = 0.0;	// NEEDED?
//	float	bilateralWeight = BilateralFilterDepth( _centralZ, previousZ, Z, _maxCosTheta, cosTheta, _radius_meters );
//	cosTheta = lerp( -1.0, cosTheta, bilateralWeight );	// Flatten if rejected

	// Update any rising horizon
	if ( cosTheta <= _maxCosTheta )
		return 0.0;	// Below the horizon... No visible contribution.

	#if SAMPLE_NEIGHBOR_RADIANCE
		float	mipLevel_Radiance = ComputeMipLevel_Radiance( _centralZ, _centralZ - Z, _radius_meters );
		_previousRadiance = FetchRadiance( _ssPosition, mipLevel_Radiance );
	#endif

	// Integrate over horizon difference (always from smallest to largest angle otherwise we get negative results!)
	float3	incomingRadiance = _previousRadiance * IntegrateSolidAngle( _integralFactors, cosTheta, _maxCosTheta );

// #TODO: Integrate with linear interpolation of irradiance as well??
// #TODO: Integrate with Fresnel F0!

	_maxCosTheta = cosTheta;	// Register a new positive horizon
//	_previousDeltaZ = Z;		// Accept new depth difference

	return incomingRadiance;
}

float3	GatherIrradiance_TEMP( float2 _ssPosition, float2 _csDirection, float2 _ssStep, float3 _csNormal, float2 _sinCosGamma, float _stepSize_meters, uint _stepsCount, float _centralZ, float3 _centralRadiance, out float3 _csBentNormal, out float _AO ) {

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
	float2	ssPosition_Front = _ssPosition;
	float2	ssPosition_Back = _ssPosition;
	float	maxCosTheta_Front = planeCosTheta_Front;
	float	maxCosTheta_Back = planeCosTheta_Back;
	float2	backSinCosGamma = float2( -_sinCosGamma.x, _sinCosGamma.y );
	[loop]
	for ( uint stepIndex=0; stepIndex < _stepsCount; stepIndex++ ) {
		radius_meters += _stepSize_meters;
		ssPosition_Front += _ssStep;
		ssPosition_Back -= _ssStep;

//radius_pixels += stepSize_pixels;
//float	mipLevel = ComputeMipLevel_Depth( radius_pixels, stepSize_pixels );
float	mipLevel = 0.0;

		sumRadiance += SampleIrradiance_TEMP( ssPosition_Front, radius_meters, _sinCosGamma, _centralZ, mipLevel, integralFactors_Front, previousRadiance_Front, maxCosTheta_Front );
		sumRadiance += SampleIrradiance_TEMP( ssPosition_Back, radius_meters, backSinCosGamma, _centralZ, mipLevel, integralFactors_Back, previousRadianceBack, maxCosTheta_Back );
	}
//*/

	// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
	_csBentNormal = IntegrateNormal( _csDirection, _csNormal, maxCosTheta_Back, maxCosTheta_Front );

	// DON'T NORMALIZE THE RESULT NOW OR WE GET BIAS!
//	_csBentNormal = normalize( _csBentNormal );

	// Compute AO for this slice (in [0,2]!!)
	_AO = 2.0 - maxCosTheta_Back - maxCosTheta_Front;

	return sumRadiance;
}


////////////////////////////////////////////////////////////////////////////////
// Computes bent cone & irradiance gathering from pixel's surroundings
struct PS_OUT {
	float4	irradiance : SV_TARGET0;
	float4	bentCone : SV_TARGET1;
};

PS_OUT	PS( float4 __Position : SV_POSITION ) {
//	float2	UV = __Position.xy / _targetResolution;
	float2	UV = 4.0 * (__Position.xy + _renderPassIndex.xy) / _resolution;	// Account for sub-pixel accuracy
	uint2	pixelPosition = uint2( floor( __Position.xy ) );
//	float	noise = frac( _time + _tex_blueNoise[pixelPosition & 0x3F] );

	// Setup camera ray
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

	// Read back depth, normal & central radiance value from last frame
	float	Z = FetchDepth( pixelPosition, 0.0 );

			Z -= 1e-2;	// !IMPORTANT! Prevent acnea by offseting the central depth a tiny bit closer

	float3	wsNormal = normalize( _tex_splitNormal[uint3( pixelPosition, _renderPassIndex.z )] );

	float3	centralRadiance = _tex_splitRadiance[uint3( pixelPosition, _renderPassIndex.z )].xyz;	// Read back last frame's radiance value that we can use as a fallback for neighbor areas

	// Compute local camera-space
	float3	wsPos = _Camera2World[3].xyz + Z * Z2Distance * wsView;
	float3	wsRight = normalize( cross( wsView, _Camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	wsAt = -wsView;

	// Express local camera-space vectors in global camera-space
	float3	gcsRight = float3( dot( wsRight, _Camera2World[0].xyz ), dot( wsRight, _Camera2World[1].xyz ), dot( wsRight, _Camera2World[2].xyz ) );
	float3	gcsUp = float3( dot( wsUp, _Camera2World[0].xyz ), dot( wsUp, _Camera2World[1].xyz ), dot( wsUp, _Camera2World[2].xyz ) );

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
//	sinCosGamma.y = csView.z;// dot( wsView, _Camera2World[2].xyz );
//	sinCosGamma.x = sqrt( 1.0 - sinCosGamma.y*sinCosGamma.y );
//	sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

	// Compute local camera-space normal
	float3	N = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), dot( wsNormal, wsAt ) );
			N.z = max( 1e-3, N.z );	// Make sure it's never 0!

	// Compute screen radius of gather sphere
	float	screenSize_m = 2.0 * Z * TAN_HALF_FOV;																	// Vertical size of the screen in meters when extended to distance Z
	float	meter2Pixel = _targetResolution.y / screenSize_m;														// Gives us the conversion factor to go from meters to pixels
	float	sphereRadius_pixels = meter2Pixel * _gatherSphereMaxRadius_m;
			sphereRadius_pixels = min( _gatherSphereMaxRadius_p, sphereRadius_pixels );								// Prevent it to grow larger than our fixed limit
	float	radiusStepSize_pixels = max( 1.0, sphereRadius_pixels / MAX_SAMPLES );									// This gives us our radial step size in pixels
	uint	samplesCount = clamp( uint( ceil( sphereRadius_pixels / radiusStepSize_pixels ) ), 1, MAX_SAMPLES );	// Reduce samples count if possible
//	float	radiusStepSize_meters = sphereRadius_pixels / (samplesCount * meter2Pixel);								// This gives us our radial step size in meters
	float	radiusStepSize_meters = radiusStepSize_pixels / (csView.z * meter2Pixel);								// This gives us our radial step size in meters

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
	float3	gcsDirection = csDirection.x * gcsRight + csDirection.y * gcsUp;	// Since csDirection gives us the components along local camera-space's right and up vectors, and we know their expression in global camera-space, it's easy find the equivalent global camera-space direction...
	float2	ssDirection = normalize( gcsDirection.xy );							// We normalize since we want to take integer pixel steps
			ssDirection *= radiusStepSize_pixels;								// Scale by our step size
			ssDirection.y = -ssDirection.y;										// Going upward in camera space means going downward in screen space...

	// Build angular correction based on deviation from central camera axis
	sinCosGamma.x = gcsDirection.z;
	sinCosGamma.y = sqrt( 1.0 - sinCosGamma.x*sinCosGamma.x );
	sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

	// Gather irradiance and average cone direction for that slice
	sumIrradiance += GatherIrradiance_TEMP( __Position.xy, csDirection, ssDirection, N, sinCosGamma, radiusStepSize_meters, samplesCount, Z, centralRadiance, csBentNormal, AO );
	csAverageBentNormal += csBentNormal;
	sumAO += AO;

	/////////////////////////////////////////////////////////////////////
	// Build orthogonal camera-space and screen-space walk directions
	csDirection = float2( -_csDirection.y, _csDirection.x );
	gcsDirection = csDirection.x * gcsRight + csDirection.y * gcsUp;
	ssDirection = normalize( gcsDirection.xy );
	ssDirection *= radiusStepSize_pixels;
	ssDirection.y = -ssDirection.y;

	// Build angular correction based on deviation from central camera axis
	sinCosGamma.x = gcsDirection.z;
	sinCosGamma.y = sqrt( 1.0 - sinCosGamma.x*sinCosGamma.x );
	sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

	// Gather irradiance and average cone direction for that slice
	sumIrradiance += GatherIrradiance_TEMP( __Position.xy, csDirection, ssDirection, N, sinCosGamma, radiusStepSize_meters, samplesCount, Z, centralRadiance, csBentNormal, AO );
	csAverageBentNormal += csBentNormal;
	sumAO += AO;

	/////////////////////////////////////////////////////////////////////
	// Accumulate temporal radiance
//	float	temporalExtinction = _bilateralValues.x;	// How much of last frame's value remains after 1 second?
//	float	temporalMaxWeight =  _bilateralValues.y;	// Importance of temporal weight over spatial weight
	float	temporalExtinction = 1.0;					// How much of last frame's value remains after 1 second? (empirical: 5%)
	float	temporalMaxWeight = 0.5;					// Importance of temporal weight over spatial weight (empirical: 2)

	float	temporalWeight = temporalMaxWeight * exp( log( max( 1e-4, temporalExtinction ) ) * _deltaTime );	// How much of last frame's value remains for this frame?
	sumIrradiance += temporalWeight * centralRadiance;


	/////////////////////////////////////////////////////////////////////
	// Write result
	sumAO *= 0.25;												// / 2 (slice interval in [0,2]) / 2 (directions)

//	float3	radiance = max( 0.0, 0.5 * PI * sumIrradiance );	// * PI / 2 (directions)
	float3	radiance = max( 0.0, PI * sumIrradiance / (2+temporalWeight) );	// * PI / 2 (directions)


	PS_OUT	Out;
	Out.irradiance = float4( radiance, 0 );
	Out.bentCone = float4( csAverageBentNormal, sumAO );

	return Out;
}
