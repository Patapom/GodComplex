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
	return Z_FAR * _tex_splitDepth[uint3( floor( _pixelPosition ), _renderPassIndex.z )];
}

float3	FetchNormal( float2 _pixelPosition, float _mipLevel ) {
	float3	N;
//	N.xy = _tex_splitNormal[uint3( floor( _pixelPosition ), _renderPassIndex.z )];
	N.xy = _tex_splitNormal.SampleLevel( LinearClamp, float3( _pixelPosition / _targetResolution, _renderPassIndex.z ), 0.0 );
	N.z = sqrt( saturate( 1.0 - dot( N.xy, N.xy ) ) );
	return N;
}

float3	FetchRadiance( float2 _pixelPosition, float _mipLevel ) {
	return _tex_splitRadiance[uint3( floor( _pixelPosition ), _renderPassIndex.z )];
}

float2	BilateralFilter( float2 _ssCentralPosition, float _centralZ, float3 _lcsCentralNormal, float2 _ssCurrentPosition, float _currentZ, float3 _lcsCurrentNormal, float _radius_meters, float _horizonCosTheta, float _newCosTheta ) {

	// Rebuild camera-space positions & normals
	// This has room for *A LOT* of optimization!!!
	//
		// For central point
	float2	UV0 = _ssCentralPosition / _targetResolution;
	float3	csView0 = BuildCameraRay( UV0 );
	float3	csPos0 = _centralZ * csView0;
			csView0 = normalize( csView0 );
	float3	csAt0 = -csView0;
	float3	csRight0 = normalize( cross( csAt0, float3( 0, 1, 0 ) ) );
	float3	csUp0 = cross( csRight0, csAt0 );
	float3	csNormal0 = _lcsCentralNormal.x * csRight0 + _lcsCentralNormal.y * csUp0 + _lcsCentralNormal.z * csAt0;

		// For current point
	float2	UV1 = _ssCurrentPosition / _targetResolution;
	float3	csView1 = BuildCameraRay( UV1 );
	float3	csPos1 = _currentZ * csView1;
			csView1 = normalize( csView1 );
	float3	csAt1 = -csView1;
	float3	csRight1 = normalize( cross( csAt1, float3( 0, 1, 0 ) ) );
	float3	csUp1 = cross( csRight1, csAt1 );
	float3	csNormal1 = _lcsCurrentNormal.x * csRight1 + _lcsCurrentNormal.y * csUp1 + _lcsCurrentNormal.z * csAt1;

	// ====== Compute depth filter value ======
	// Our criterion is that current position and normal must see our central position to contribute...
	float3	csToCentralPosition = csPos0 - csPos1;
	float	distance2CentralPosition = length( csToCentralPosition );
			csToCentralPosition /= distance2CentralPosition;

	const float2	toleranceMin = float2( -0.02, -0.04 );
	const float2	toleranceMax = float2( -0.4, -0.8 );
	float	verticality = smoothstep( 0.5, 0.9, saturate( -dot( csToCentralPosition, csNormal0 ) ) );
	float2	tolerance = lerp( toleranceMin, toleranceMax, verticality );	// We grow more tolerant for very vertical pixels
	float	depthFilter = smoothstep( tolerance.y, tolerance.x, dot( csToCentralPosition, csNormal1 ) );

	// ====== Compute radiance filter value ======
	// To avoid sudden radiance jumps we use a dot product
	#if 0
		float	radianceFilter = saturate( dot( csToCentralPosition, csNormal1 ) );
//				radianceFilter = pow( radianceFilter, 0.5 );
				radianceFilter = fastSqrtNR0( radianceFilter );				// Not full dot product, sqrt( dot( ) ) actually! (smoother result)
				radianceFilter = 1.0
							   - saturate( 0.5 * distance2CentralPosition )	// Dot product fade out is fully effective after 2 meters
							   * (1.0 - radianceFilter);
	#else
		float	radianceFilter = 1.0
							   - saturate( 0.5 * distance2CentralPosition )	// Dot product fade out is fully effective after 2 meters
							   * pow2( 1.0 - saturate( dot( csToCentralPosition, csNormal1 ) ) );
	#endif

	return float2( depthFilter, radianceFilter );
}


////////////////////////////////////////////////////////////////////////////////
// Computes bent cone & irradiance gathering from surrounding pixels
struct PS_OUT {
	float4	irradiance : SV_TARGET0;
	float4	bentCone : SV_TARGET1;
};

PS_OUT	PS( float4 __position : SV_POSITION ) {
	float2	fullScreenPixelPosition = 4.0 * (__position.xy - 0.5) + _renderPassIndex.xy + 0.5;	// Account for sub-pixel accuracy
	float2	UV = fullScreenPixelPosition / _resolution;
	uint2	pixelPosition = uint2( floor( __position.xy ) );

	#if 0
		// Funky blue noise
		float	noise = frac( _tex_blueNoise[uint2(fullScreenPixelPosition) & 0x3F] + SQRT2 * _framesCount );	// ACTUAL GOOD VALUE!
	#else
		// Martin's noise
		float	noise = (wang_hash(fullScreenPixelPosition.y * _resolution.x + fullScreenPixelPosition.x)
						^ wang_hash(uint(_jitterOffset)))
						* 2.3283064365386963e-10;
	#endif

	// Setup camera ray
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;

	/////////////////////////////////////////////////////////////////////
	// Read back depth, normal & central radiance value from last frame
	float	Z = FetchDepth( __position.xy, 0.0 );

			Z -= 1e-2;	// !IMPORTANT! Prevent acnea by offseting the central depth a tiny bit closer

	float3	centralRadiance = _tex_splitRadiance[uint3( pixelPosition, _renderPassIndex.z )].xyz;	// Read back last frame's radiance value that we can use as a fallback for neighbor areas

	// Compute local camera-space normal
	float3	csNormal;
			csNormal.xy = _tex_splitNormal[uint3( pixelPosition, _renderPassIndex.z )];
			csNormal.z = sqrt( saturate( 1.0 - dot( csNormal.xy, csNormal.xy ) ) );
			csNormal.z = max( 1e-3, csNormal.z );	// Make sure it's never 0!


	/////////////////////////////////////////////////////////////////////
	// Build camera-space and screen-space walk directions

	// Compute local camera-space
	float3	wsPos = _camera2World[3].xyz + Z * Z2Distance * wsView;
	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	wsAt = -wsView;

	// Express local camera-space vectors in global camera-space
	float3	gcsRight = float3( dot( wsRight, _camera2World[0].xyz ), dot( wsRight, _camera2World[1].xyz ), dot( wsRight, _camera2World[2].xyz ) );
	float3	gcsUp = float3( dot( wsUp, _camera2World[0].xyz ), dot( wsUp, _camera2World[1].xyz ), dot( wsUp, _camera2World[2].xyz ) );

	float2	csDirection = _csDirection;

	// Here, you can see the same code as in the C# driver file
	#if 0
		float	phi = (B4( _renderPassIndex.x, _renderPassIndex.y ) / 16.0 + 2.6457513110645905905016157536393 * _framesCount) * 0.5 * PI;	// ACTUAL GOOD VALUE!
		//float	phi = (B4( _renderPassIndex.x, _renderPassIndex.y ) + 2.6457513110645905905016157536393 * _mouseUVs.x) / 16.0 * 0.5 * PI;	// MANUALLY CONTROLED VALUE
		csDirection = float2( cos( phi ), sin( phi ) );
	#endif


	/////////////////////////////////////////////////////////////////////
	// Compute screen radius of gather sphere
	float	screenSize_m = 2.0 * Z * TAN_HALF_FOV;																	// Vertical size of the screen in meters when extended to distance Z
	float	meter2Pixel = _targetResolution.y / screenSize_m;														// Gives us the conversion factor to go from meters to pixels
	float	sphereRadius_pixels = meter2Pixel * _gatherSphereMaxRadius_m;
			sphereRadius_pixels = min( _gatherSphereMaxRadius_p, sphereRadius_pixels );								// Prevent it to grow larger than our fixed limit
	float	radiusStepSize_pixels = max( 1.0, sphereRadius_pixels / MAX_SAMPLES );									// This gives us our radial step size in pixels
	uint	samplesCount = clamp( uint( ceil( sphereRadius_pixels / radiusStepSize_pixels ) ), 1, MAX_SAMPLES );	// Reduce samples count if possible

//	float	radiusStepSize_meters = sphereRadius_pixels / (samplesCount * meter2Pixel);								// This gives us our radial step size in meters
	float	radiusStepSize_meters = radiusStepSize_pixels / (csView.z * meter2Pixel);								// This gives us our radial step size in meters


	/////////////////////////////////////////////////////////////////////
	// Sample along first direction
	/////////////////////////////////////////////////////////////////////
	//
	float3	sumIrradiance = 0.0;
	float	sumAO = 0.0;
	float3	csAverageBentNormal = 0.0;
	float	averageAO = 0.0;
//	float	varianceAO = 0.0;	// We don't accumulate variance anymore because we're only using 2 directions so we only retrieve 4 horizon values... Not enough for a proper measure of variance. It made sense with a lot of directions but not anymore...

	float3	gcsDirection = csDirection.x * gcsRight + csDirection.y * gcsUp;	// Since csDirection gives us the components along local camera-space's right and up vectors, and we know their expression in global camera-space, it's easy find the equivalent global camera-space direction...
	float2	ssDirection = normalize( gcsDirection.xy );							// We normalize since we want to take integer pixel steps
			ssDirection *= radiusStepSize_pixels;								// Scale by our step size
			ssDirection.y = -ssDirection.y;										// Going upward in camera space means going downward in screen space...

	// Build angular correction based on deviation from central camera axis
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
	// We easily find that cos(b) = R / sqrt( z� + R� ) and sin(b) = z / sqrt( z� + R� )
	// So finaly:
	//	cos(theta) = [sin(a)*R + cos(a)*z] / sqrt( z� + R� )
	//
	// We notice that it behaves a bit like a rotation and is finally an angular interpolation between sin(b) and cos(b) that depends on the camera axis deviation...
	//
	float2	sinCosGamma;
	sinCosGamma.x = gcsDirection.z;
	sinCosGamma.y = sqrt( 1.0 - sinCosGamma.x*sinCosGamma.x );
	sinCosGamma *= 0.99;	// Needed otherwise sometimes the cos(theta) is outside the [-1,1] range and acos gives a NaN!

	// Gather irradiance and average cone direction for that slice
	float3	csBentNormal;
	float	AO;
	sumIrradiance += GatherIrradiance( __position.xy, ssDirection, _targetResolution, csDirection, csNormal, sinCosGamma, radiusStepSize_meters, samplesCount, Z, centralRadiance, noise, csBentNormal, AO );
	csAverageBentNormal += csBentNormal;
	sumAO += AO;

	/////////////////////////////////////////////////////////////////////
	// Sample along 2nd direction (orthogonal to the first one)
	/////////////////////////////////////////////////////////////////////
	//
	#if !USE_SINGLE_DIRECTION
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
		sumIrradiance += GatherIrradiance( __position.xy, ssDirection, _targetResolution, csDirection, csNormal, sinCosGamma, radiusStepSize_meters, samplesCount, Z, centralRadiance, noise, csBentNormal, AO );
		csAverageBentNormal += csBentNormal;
		sumAO += AO;
	#endif


	/////////////////////////////////////////////////////////////////////
	// Accumulate temporal radiance (optional)
	// Here we attempt to re-inject last frame's radiance into the mix as well
	/////////////////////////////////////////////////////////////////////
	//
	#if 1
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
	/////////////////////////////////////////////////////////////////////
	//
	#if USE_SINGLE_DIRECTION
		#if !USE_NORMAL_INFLUENCE_FOR_AO
			sumAO *= 0.5;														// / 2 (slice interval in [0,2]) / 1 (direction)
		#endif
		float3	radiance = max( 0.0, PI * sumIrradiance / (1+temporalWeight) );	// * PI / 1 (direction)
	#else
		#if USE_NORMAL_INFLUENCE_FOR_AO
			sumAO *= 0.5;														// / 2 (directions). Each AO interval is [0,1] here
		#else
			sumAO *= 0.25;														// / 2 (slice interval in [0,2]) / 2 (directions)
		#endif

		float3	radiance = max( 0.0, PI * sumIrradiance / (2+temporalWeight) );	// * PI / 2 (directions)
	#endif


	#if USE_RECOMPOSED_BUFFER
		float4	csBentCone = float4( csAverageBentNormal, sqrt(sumAO) );	// Store as is, will be dealt with by RecomposeBuffers shader
	#else

		float	cosAverageConeAngle = 1.0 - sumAO;		// Use AO as cone angle
		float	stdDeviation = 0.0;						// Not available if no reconstruction is done... :'(
														// Plus, we're only using 2 sampling directions. Not enough precision to get a nice variance estimate... :/

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

