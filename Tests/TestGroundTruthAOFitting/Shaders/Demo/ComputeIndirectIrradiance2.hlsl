////////////////////////////////////////////////////////////////////////////////
// Compute indirect irradiance from last frame's irradiance + bent cone direction
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

#define	MAX_ANGLES	32			// Amount of angular subdivisions of the circle
#define	MAX_RADIUS	32			// Amount of radial subdivisions of the circle
#define	RADIUS_STEP_SIZE 1.0	// Radial step size (in pixels)

#define	SAMPLER	LinearWrap

#define SAMPLE_BENT_CONE_MAP 1	// Define this to use precomputed bent-cone map instead of our own runtime computation (more precise to start with) (DEBUG ONLY!)

#define PREMULTIPLY_ALBEDO 1	// Define this to store (albedo/PI)*Irradiance instead of simply Irradiance
								// The main interest is to be able to use the value straight from the sampler
								//	next frame and avoid to sample the albedo buffer to redo the computation manually...

cbuffer	CBCompute : register( b2 ) {
	uint2	_textureDimensions;	// Height map resolution
	float	_texelSize_mm;		// Texel size in millimeters
	float	_displacement_mm;	// Height map max encoded displacement in millimeters

	float3	_rho;
}

Texture2D<float>		_texHeight : register( t0 );
Texture2D<float3>		_texNormal : register( t1 );
Texture2D<float2>		_texAO : register( t2 );

#if SAMPLE_BENT_CONE_MAP
Texture2D<float4>		_texBentCone : register( t4 );				// Optional high-resolution bent cone map to avoid computing it ourselves in low-res (DEBUG PURPOSE ONLY!)
#endif

Texture2D<float4>		_texSourceIrradiance : register( t5 );		// Irradiance from last frame

Texture2D<float>		_texBlueNoise : register( t10 );


struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float4	irradiance	: SV_TARGET0;
	float4	bentCone	: SV_TARGET1;
};

VS_IN	VS( VS_IN _In ) { return _In; }

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
//	E = L(Xi) * I
//
// With:
//	I = Integral[theta0, theta1]{ (Wi.N) sin(theta) dTheta }
//	  = Integral[theta0, theta1]{ cos(phi).sin(theta).Nx.sin(theta) dTheta }	<= I0
//	  + Integral[theta0, theta1]{ sin(phi).sin(theta).Ny.sin(theta) dTheta }	<= I1
//	  + Integral[theta0, theta1]{          cos(theta).Nz.sin(theta) dTheta }	<= I2
//
//	I0 = [cos(phi).Nx] * Integral[theta0, theta1]{ sin²(theta) dTheta } = [cos(phi).Nx] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I1 = [sin(phi).Ny] * Integral[theta0, theta1]{ sin²(theta) dTheta } = [sin(phi).Ny] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I2 = Nz * Integral[theta0, theta1]{ cos(theta).sin(theta) dTheta } = Nz * { 1/2 * cos²(theta)) }[theta1,theta0]	<== Watch out for theta reverse here!
//
float	IntegrateSolidAngle( float2 _sinCosPhi, float _cosTheta0, float _cosTheta1, float3 _N ) {
	float	theta0 = acos( _cosTheta0 );
	float	theta1 = acos( _cosTheta1 );
	float	I0 = (theta1 - sqrt( 1.0 - _cosTheta1*_cosTheta1 )*_cosTheta1)
			   - (theta0 - sqrt( 1.0 - _cosTheta0*_cosTheta0 )*_cosTheta0);
	float	I1 = _cosTheta1*_cosTheta1 - _cosTheta0*_cosTheta0;
	return 0.5 * ((_sinCosPhi.y * _N.x + _sinCosPhi.x * _N.y) * I0 + _N.z * I1);
}

// Samples the radiance reflected from a screen-space position located around the center pixel position
//	_ssPosition, the screen-space position to sample
//	_radius, the radius from the center position
//	T, B, N, the local tangent space
//	_centerRho, the reflectance of the center pixel (fallback only necessary if no albedo map is available)
//	_maxCos, the floating max cos(theta) that indicates the perceived horzion
//
float3	SampleIrradiance( float2 _ssPosition, float _H0, float _radius, float2 _sinCosPhi, float3 _T, float3 _B, float3 _N, float3 _centerRho, inout float _maxCos ) {

	// Sample new height and update horizon angle
	float	deltaH = (_displacement_mm / _texelSize_mm) * _texHeight.SampleLevel( SAMPLER, _ssPosition / _textureDimensions, 0.0 ) - _H0;
	float	H2 = deltaH * deltaH;
	float	hyp2 = _radius*_radius + H2;		// Square hypotenuse
	float	cosHorizon = deltaH / sqrt( hyp2 );	// Cosine to horizon angle
	if ( cosHorizon <= _maxCos )
		return 0.0;	// Below the horizon... No visible contribution.

	// Compute bounced incoming radiance
	float2	neighborUV = _ssPosition / _textureDimensions;

	#if PREMULTIPLY_ALBEDO
		// Source texture directly contains Li-1
		// (Ei-1 is already pre-multiplied by albedo/rho from last frame so don't bother!)
		float3	incomingRadiance = _texSourceIrradiance.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;
	#else
		// Only Ei-1 is available, Li-1 need to be computed by fetching albedo and Li-1 = Ei-1 * (rho/PI)
		// This is more costly...
		float3	neighborIrradiance = _texSourceIrradiance.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;
//		float3	neighborRho = _texSourceAlbedo.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;				// #TODO: Sample albedo G-buffer entry for better reflectance
		float3	neighborRho = _centerRho;																// Uniform reflectance in neighborhood
		float3	incomingRadiance = (neighborRho / PI) * neighborIrradiance;
	#endif

	// Integrate over horizon difference
	incomingRadiance *= IntegrateSolidAngle( _sinCosPhi, _maxCos, cosHorizon, _N );

	// #TODO: Integrate with linear interpolation of irradiance as well??

	_maxCos = cosHorizon;		// Register a new positive horizon

	return incomingRadiance;
}

PS_OUT	PS( VS_IN _In ) {
	uint2	pixelPosition = uint2(floor(_In.__Position.xy));
	float	noise = _texBlueNoise[pixelPosition & 0x3F];

	// Retrieve central height and TBN
	float	heightEpsilon = 1e-2;	// Add a tiny epsilon to avoid horizon acnea
	float	H0 = (_displacement_mm / _texelSize_mm) * (heightEpsilon + _texHeight[pixelPosition]);
	float3	N = 2.0 * _texNormal[pixelPosition].xyz - 1.0;
			N.y *= -1.0;
	float3	T, B;
	BuildOrthonormalBasis( N, T, B );

	float	recZdotN = abs(N.z) > 1e-6 ? 1.0 / N.z : 1e6 * sign(N.z);

	float3	centerRho = _rho;	// Uniform reflectance

	// Samples circular surroundings in screen space
	float3	ssAverageBentNormal = 0.0;
	float	averageConeAngle = 0.0;
	float	varianceConeAngle = 0.0;
	float3	sumIrradiance = 0.0;

	#if MAX_ANGLES > 1
		for ( uint angleIndex=0; angleIndex < MAX_ANGLES; angleIndex++ ) {
	#else
	{
		uint	angleIndex = 0;
	#endif

		// Compute direction of the screen-space slice
		float	phi = PI * (angleIndex + noise) / MAX_ANGLES;
		float2	sinCosPhi;
		sincos( phi, sinCosPhi.x, sinCosPhi.y );
		float2	ssDirection = float2( sinCosPhi.y, sinCosPhi.x );

sinCosPhi *= -1.0;

		// Project screen-space direction onto tangent plane to determine max possible horizon angles
		float	hitDistance_Front = -dot( ssDirection, N.xy ) * recZdotN;
		float3	tsDirection_Front = normalize( float3( ssDirection, hitDistance_Front ) );
		float	maxCos_Front = tsDirection_Front.z;
		float	maxCos_Back = -tsDirection_Front.z;

//*		// Accumulate perceived irradiance in front and back & update floating horizon (in the form of cos(horizon angle))
		float2	ssPosition_Front = _In.__Position.xy;
		float2	ssPosition_Back = _In.__Position.xy;
		float2	ssStep = ssDirection * RADIUS_STEP_SIZE;
		float	radius = 0.0;
		for ( uint radiusIndex=1; radiusIndex <= MAX_RADIUS; radiusIndex++ ) {
			ssPosition_Front += ssStep;
			ssPosition_Back -= ssStep;
			radius += RADIUS_STEP_SIZE;
			sumIrradiance += SampleIrradiance( ssPosition_Front, H0, radius, sinCosPhi, T, B, N, centerRho, maxCos_Front );	// Sample forward
			sumIrradiance += SampleIrradiance( ssPosition_Back, H0, radius, float2( -sinCosPhi.x, sinCosPhi.y ), T, B, N, centerRho, maxCos_Back );	// Sample backward
		}
//*/
		// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
		// Half brute force where we perform the integration numerically as a sum...
		// This solution is prefered to the analytical integral that shows some precision artefacts unfortunately...
		//
		float	thetaFront = acos( maxCos_Front );
		float	thetaBack = -acos( maxCos_Back );

		const uint	STEPS_COUNT = 256;

		float3	ssBentNormal = 0.001 * N;
		for ( uint i=0; i < STEPS_COUNT; i++ ) {
			float	theta = lerp( thetaBack, thetaFront, (i+0.5) / STEPS_COUNT );
			float	sinTheta, cosTheta;
			sincos( theta, sinTheta, cosTheta );
			float3	ssUnOccludedDirection = float3( sinTheta * ssDirection, cosTheta );

			float	cosAlpha = saturate( dot( ssUnOccludedDirection, N ) );

			float	weight = cosAlpha * abs(sinTheta);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
			ssBentNormal += weight * ssUnOccludedDirection;
		}

		float	dTheta = (thetaFront - thetaBack) / STEPS_COUNT;
		ssBentNormal *= dTheta;
		ssBentNormal = normalize( ssBentNormal );

		ssAverageBentNormal += ssBentNormal;

		// Compute cone angle
		float3	ssHorizon_Front = float3( sqrt( 1.0 - maxCos_Front*maxCos_Front ) * ssDirection, maxCos_Front );
		float3	ssHorizon_Back = float3( -sqrt( 1.0 - maxCos_Back*maxCos_Back ) * ssDirection, maxCos_Back );
		float	coneAngle_Front = acos( dot( ssBentNormal, ssHorizon_Front ) );
		float	coneAngle_Back = acos( dot( ssBentNormal, ssHorizon_Back ) );

		// We're using running variance computation from https://www.johndcook.com/blog/standard_deviation/
		//	Avg(N) = Avg(N-1) + [V(N) - Avg(N-1)] / N
		//	S(N) = S(N-1) + [V(N) - Avg(N-1)] * [V(N) - Avg(N)]
		// And variance = S(finalN) / (finalN-1)
		//
		float	previousAverageConeAngle = averageConeAngle;
		averageConeAngle += (coneAngle_Front - averageConeAngle) / (2*angleIndex+1);
		varianceConeAngle += (coneAngle_Front - previousAverageConeAngle) * (coneAngle_Front - averageConeAngle);

		previousAverageConeAngle = averageConeAngle;
		averageConeAngle += (coneAngle_Back - averageConeAngle) / (2*angleIndex+2);
		varianceConeAngle += (coneAngle_Back - previousAverageConeAngle) * (coneAngle_Back - averageConeAngle);
	}

	// Finalize bent cone
	#if MAX_ANGLES > 2
		varianceConeAngle /= 2*MAX_ANGLES - 1;
	#endif
	ssAverageBentNormal = normalize( ssAverageBentNormal );

	float	stdDeviation = sqrt( varianceConeAngle );

#if SAMPLE_BENT_CONE_MAP
	// Replace runtime computation by precise bent cone map sampling
	float4	bentCone = _texBentCone[pixelPosition];
			bentCone.xyz = 2.0 * bentCone.xyz - 1.0;
			bentCone.y *= -1.0;
	float	cosAlpha = length( bentCone.xyz );
	ssAverageBentNormal = bentCone.xyz * (cosAlpha > 0.0 ? 1.0 / cosAlpha : 0.0);
	stdDeviation = 0.5 * PI * bentCone.w;
	averageConeAngle = acos( cosAlpha );
#endif

	// Increase cone aperture a little (based on manual fitting of irradiance compared to ground truth with 0 bounce)
//	averageConeAngle += 0.18 * stdDeviation;
//	averageConeAngle = min( 0.5 * PI, averageConeAngle + 0.09 * stdDeviation );

	// Finalize indirect irradiance
	const float	dPhi = PI / MAX_ANGLES;	// Hemisphere is sliced into 2*MAX_ANGLES parts
	sumIrradiance *= dPhi;

	// Compute this frame's irradiance
	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
//	float3	directIrradiance = EvaluateSHIrradiance( N, 0.0, SH );											// Use normal direction
	float3	directIrradiance = EvaluateSHIrradiance( ssAverageBentNormal, cos( averageConeAngle ), SH );	// Use bent-normal direction + cone angle

	sumIrradiance += directIrradiance;


	#if PREMULTIPLY_ALBEDO
		// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		// This MUST be done either now (in which case we're storing RADIANCE and not irradiance), or later when the irradiance is used!
		// My advice is to use it right now before writing to the irradiance render target so next frame,
		//	(rho/PI) * Irradiance is available straight from the sampler and this avoids to uselessly
		//	sampling the neighbor albedo map again, which is very costly in terms of bandwidth...
		sumIrradiance *= _rho / PI;
//		sumIrradiance *= 0.5 * float3( 1.0, 0.9, 0.8 ) / PI;
		//
		// And sumIrradiance has now become radiance!
		//
		// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	#endif

ssAverageBentNormal.y *= -1.0;

	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0.0 );
	Out.bentCone = float4( max( 0.01, cos( averageConeAngle ) ) * ssAverageBentNormal, 1.0 - stdDeviation / (0.5*PI) );
//Out.bentCone = float4( ssAverageBentNormal, 1.0 );
//Out.bentCone = float4( 1.0 * averageConeAngle.xxx * 2.0 / PI, 1.0 );
//Out.bentCone = float4( 1.0 * varianceConeAngle.xxx * 2.0 / PI, 1.0 );

//ssAverageBentNormal.y *= -1.0;
//Out.bentCone = float4( 0.5 * (1.0+ssAverageBentNormal), 1.0 );

	return Out;
}
