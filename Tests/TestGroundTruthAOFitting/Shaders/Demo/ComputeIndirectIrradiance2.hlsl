////////////////////////////////////////////////////////////////////////////////
// Compute indirect irradiance from last frame's irradiance + bent cone direction
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

#if 1
	// Quality values
	#define	SUBDIV_COUNT_ANGULAR	32		// Amount of angular subdivisions of the circle
	#define	SUBDIV_COUNT_RADIAL		32		// Amount of radial subdivisions of the circle
#else
	// Fast values
	#define	SUBDIV_COUNT_ANGULAR	2		// Amount of angular subdivisions of the circle
	#define	SUBDIV_COUNT_RADIAL		8		// Amount of radial subdivisions of the circle
#endif

#define	MAX_SIZE_RADIUS			128.0		// Maximum covered radius (in pixels)

#define	SAMPLER	LinearWrap

//#define SAMPLE_BENT_CONE_MAP 1	// Define this to use precomputed bent-cone map instead of our own runtime computation (more precise to start with) (DEBUG ONLY!)

// !!SLIGHTLY LESS ACCURATE!!
#define USE_FAST_ACOS 1			// Define this to use the "fast acos" function instead of true acos()
// !!SLIGHTLY LESS ACCURATE!!

// !!MORE EXPENSIVE AND LESS ACCURATE => DON'T USE EXCEPT FOR DEBUG!!
//#define USE_NUMERICAL_INTEGRATION 256	// Define this to compute bent normal numerically (value = integration steps count)
// !!MORE EXPENSIVE AND LESS ACCURATE => DON'T USE EXCEPT FOR DEBUG!!

// !!WRONG IF YOU INTEND TO FOLLOW THE TRUE DEFINITION OF THE BENT NORMAL THAT DOESN'T INVOLVE THE DOT PRODUCT IN THE WEIGHT!!
// Personnaly, I think we're closer to the ground truth than using the plain bent normal definition...
// We'll see...
#define	ACCOUNT_FOR_DOT_PRODUCT_WITH_NORMAL	1	// Define this to account for dot product with normal when computing the bent-normal
// !!WRONG IF YOU INTEND TO FOLLOW THE TRUE DEFINITION OF THE BENT NORMAL THAT DOESN'T INVOLVE THE DOT PRODUCT IN THE WEIGHT!!

// !!MORE EXPENSIVE FOR NO REAL IMPROVEMENT IN QUALITY
//#define USE_QUADRATIC_PROGRESSION	1
// !!MORE EXPENSIVE FOR NO REAL IMPROVEMENT IN QUALITY


#define PREMULTIPLY_ALBEDO 1	// Define this to store (albedo/PI)*Irradiance instead of simply Irradiance
								// The main interest is to be able to use the value straight from the sampler
								//	next frame and avoid to sample the albedo buffer to redo the computation manually...


cbuffer	CBCompute : register( b2 ) {
	uint2	_textureDimensions;	// Height map resolution
	float	_texelSize_mm;		// Texel size in millimeters
	float	_displacement_mm;	// Height map max encoded displacement in millimeters

	float3	_rho;

	float4	_debugValues;
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
#if 0
// With:
//	I = Integral[theta0, theta1]{ cos(psi).sin(theta) dtheta }
// Here psi is the angle between Wi and N that can be found by the spherical law of cosines (https://en.wikipedia.org/wiki/Spherical_law_of_cosines):
//	cos(psi) = cos(theta).cos(alpha) + sin(theta).sin(alpha).cos(beta)
// Where:
//	alpha, the angle between the normal and the vertical Z axis (0,0,1)
//	beta, the azimutal angle between the slice plane and the normal
//
// Thus:
//	I = Integral[theta0, theta1]{ cos(alpha).cos(theta).sin(theta) dtheta }
//	  + Integral[theta0, theta1]{ sin(alpha).cos(beta).cos(theta).sin(theta) dtheta }
//
// And finally:
//	I = cos(alpha)           * {-1/2 cos�(theta)}[theta0, theta1]
//	  + sin(alpha).cos(beta) * {theta/2 - sin(theta).cos(theta)/2}[theta0, theta1]
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

	return 0.5 * (
			  _integralFactors.x * (_cosTheta0*_cosTheta0 - _cosTheta1*_cosTheta1)
			+ _integralFactors.y * (theta1 - theta0 - _cosTheta1*sinTheta1 + _cosTheta0*sinTheta0)
		);
}
float2	ComputeIntegralFactors( float2 _ssDirection, float3 _N ) {
	float	cosAlpha = _N.z;
// Actual computation
//	float	sinAlpha = sqrt( 1.0 - cosAlpha*cosAlpha );
//	float	L = length( _N.xy );
//	float	cosBeta = dot( _N.xy, _ssDirection );
//			cosBeta *= L > 0.0 ? 1.0 / L : 0.0;
//	return float2( cosAlpha, sinAlpha * cosBeta );
	// But we end up doing sin(alpha) * dot( _N.xy, _ssDirection ) / sin(alpha) so what's the point? :D
	return float2( cosAlpha, dot( _N.xy, _ssDirection ) );
}
#else
// With:
//	I = Integral[theta0, theta1]{ (Wi.N) sin(theta) dTheta }
//	  = Integral[theta0, theta1]{ cos(phi).sin(theta).Nx.sin(theta) dTheta }	<= I0
//	  + Integral[theta0, theta1]{ sin(phi).sin(theta).Ny.sin(theta) dTheta }	<= I1
//	  + Integral[theta0, theta1]{          cos(theta).Nz.sin(theta) dTheta }	<= I2
//
//	I0 = [cos(phi).Nx] * Integral[theta0, theta1]{ sin�(theta) dTheta } = [cos(phi).Nx] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I1 = [sin(phi).Ny] * Integral[theta0, theta1]{ sin�(theta) dTheta } = [sin(phi).Ny] * { 1/2 * (theta - sin(theta)*cos(theta)) }[theta0,theta1]
//	I2 = Nz * Integral[theta0, theta1]{ cos(theta).sin(theta) dTheta } = Nz * { 1/2 * cos�(theta)) }[theta1,theta0]	<== Watch out for theta reverse here!
//
float2	ComputeIntegralFactors( float2 _ssDirection, float3 _N ) {
	return float2( dot( _N.xy, _ssDirection ), _N.z );
}
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
#endif

// Samples the irradiance reflected from a screen-space position located around the center pixel position
//	_ssPosition, the screen-space position to sample
//	_H0, the center height
//	_radius, the radius from the center position
//	_integralFactors, some pre-computed factors to feed the integral
//	_maxCos, the floating maximum cos(theta) that indicates the angle of the perceived horizon
//	_optionnal_centerRho, the reflectance of the center pixel (fallback only necessary if no albedo map is available and if it's only irradiance that is stored in the source irradiance map instead of radiance, in which case albedo is already pre-mutliplied)
//
float3	SampleIrradiance( float2 _ssPosition, float _H0, float _radius, float2 _integralFactors, inout float _maxCos, float3 _optionnal_centerRho ) {

	// Sample new height and update horizon angle
	float	deltaH = (_displacement_mm / _texelSize_mm) * _texHeight.SampleLevel( SAMPLER, _ssPosition / _textureDimensions, 0.0 ) - _H0;
	float	H2 = deltaH * deltaH;
	float	hyp2 = _radius * _radius + H2;		// Square hypotenuse
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
		float3	neighborRho = _optionnal_centerRho;														// Uniform reflectance in neighborhood
		float3	incomingRadiance = (neighborRho / PI) * neighborIrradiance;
	#endif

	// Integrate over horizon difference (always from smallest to largest angle otherwise we get negative results!)
	incomingRadiance *= IntegrateSolidAngle( _integralFactors, cosHorizon, _maxCos );

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

	float3	centerRho = _rho;	// Uniform reflectance all around our central pixel

	// Samples circular surroundings in screen space
	float3	ssAverageBentNormal = 0.0;
	float	averageConeAngle = 0.0;
	float	varianceConeAngle = 0.0;
	float3	sumIrradiance = 0.0;

	#if SUBDIV_COUNT_ANGULAR > 1
		for ( uint angleIndex=0; angleIndex < SUBDIV_COUNT_ANGULAR; angleIndex++ ) {
	#else
	{
		uint	angleIndex = 0;
	#endif

		// Compute direction of the screen-space slice
		float	phi = PI * (angleIndex + noise) / SUBDIV_COUNT_ANGULAR;
		float2	sinCosPhi;
		sincos( phi, sinCosPhi.x, sinCosPhi.y );
		float2	ssDirection = float2( sinCosPhi.y, sinCosPhi.x );

		// Pre-compute factors for the integrals
		float2	integralFactors_Front = ComputeIntegralFactors( ssDirection, N );
		float2	integralFactors_Back = ComputeIntegralFactors( -ssDirection, N );

		// Project screen-space direction onto tangent plane to determine max possible horizon angles
		float	hitDistance_Front = -dot( ssDirection, N.xy ) * recZdotN;
		float3	tsDirection_Front = normalize( float3( ssDirection, hitDistance_Front ) );
		float	maxCosTheta_Front = tsDirection_Front.z;
		float	maxCosTheta_Back = -tsDirection_Front.z;

//*		// Accumulate perceived irradiance in front and back & update floating horizon (in the form of cos(horizon angle))
		#if !USE_QUADRATIC_PROGRESSION
			float	stepSize = MAX_SIZE_RADIUS / SUBDIV_COUNT_RADIAL;
//			float	stepSize = 16 * _debugValues.w;	// Good value => _debugValues.w = 0.13
			float2	ssStep = ssDirection * stepSize;
		#endif
		float	radius = 0.0;

		float2	ssPosition_Front = _In.__Position.xy;
		float2	ssPosition_Back = _In.__Position.xy;
		for ( uint radiusIndex=0; radiusIndex < SUBDIV_COUNT_RADIAL; radiusIndex++ ) {
			#if USE_QUADRATIC_PROGRESSION
				radius = 1.0 + (MAX_SIZE_RADIUS-1.0) * (1.0+radiusIndex) / SUBDIV_COUNT_RADIAL;
				ssPosition_Front = _In.__Position.xy + radius * ssDirection;
				ssPosition_Back = _In.__Position.xy - radius * ssDirection;
			#else
				ssPosition_Front += ssStep;
				ssPosition_Back -= ssStep;
				radius += stepSize;
			#endif

			sumIrradiance += SampleIrradiance( ssPosition_Front, H0, radius, integralFactors_Front, maxCosTheta_Front, centerRho );	// Sample forward
			sumIrradiance += SampleIrradiance( ssPosition_Back, H0, radius, integralFactors_Back, maxCosTheta_Back, centerRho );		// Sample backward
		}
//*/
		// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
		#if ACCOUNT_FOR_DOT_PRODUCT_WITH_NORMAL
			#if USE_NUMERICAL_INTEGRATION
				// Half brute force where we perform the integration numerically as a sum...
				// This solution is prefered to the analytical integral that shows some precision artefacts unfortunately...
				//
				float	thetaFront = acos( maxCosTheta_Front );
				float	thetaBack = -acos( maxCosTheta_Back );

				float3	ssBentNormal = 0.001 * N;
				for ( uint i=0; i < USE_NUMERICAL_INTEGRATION; i++ ) {
					float	theta = lerp( thetaBack, thetaFront, (i+0.5) / USE_NUMERICAL_INTEGRATION );
					float	sinTheta, cosTheta;
					sincos( theta, sinTheta, cosTheta );
					float3	ssUnOccludedDirection = float3( sinTheta * ssDirection, cosTheta );

					float	cosAlpha = saturate( dot( ssUnOccludedDirection, N ) );

					float	weight = cosAlpha * abs(sinTheta);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
					ssBentNormal += weight * ssUnOccludedDirection;
				}

				float	dTheta = (thetaFront - thetaBack) / USE_NUMERICAL_INTEGRATION;
				ssBentNormal *= dTheta;
			#else
				// Integral computation
				float	cosTheta0 = maxCosTheta_Front;
				float	cosTheta1 = maxCosTheta_Back;
				float	sinTheta0 = sqrt( 1.0 - cosTheta0*cosTheta0 );
				float	sinTheta1 = sqrt( 1.0 - cosTheta1*cosTheta1 );
				float	cosTheta0_3 = cosTheta0*cosTheta0*cosTheta0;
				float	cosTheta1_3 = cosTheta1*cosTheta1*cosTheta1;
				float	sinTheta0_3 = sinTheta0*sinTheta0*sinTheta0;
				float	sinTheta1_3 = sinTheta1*sinTheta1*sinTheta1;

				float2	sliceSpaceNormal = float2( dot( N.xy, ssDirection ), N.z );

				float	averageX = sliceSpaceNormal.x * (cosTheta0_3 + cosTheta1_3 - 3.0 * (cosTheta0 + cosTheta1) + 4.0)
								 + sliceSpaceNormal.y * (sinTheta0_3 - sinTheta1_3);

				float	averageY = sliceSpaceNormal.x * (sinTheta0_3 - sinTheta1_3)
								 + sliceSpaceNormal.y * (2.0 - cosTheta0_3 - cosTheta1_3);

				float3	ssBentNormal = float3( averageX * ssDirection, averageY );
			#endif
		#else // if !ACCOUNT_FOR_DOT_PRODUCT_WITH_NORMAL
			#if USE_NUMERICAL_INTEGRATION
				// Half brute force where we perform the integration numerically as a sum...
				// This solution is prefered to the analytical integral that shows some precision artefacts unfortunately...
				//
				float	thetaFront = acos( maxCosTheta_Front );
				float	thetaBack = -acos( maxCosTheta_Back );

				float3	ssBentNormal = 0.001 * N;
				for ( uint i=0; i < USE_NUMERICAL_INTEGRATION; i++ ) {
					float	theta = lerp( thetaBack, thetaFront, (i+0.5) / USE_NUMERICAL_INTEGRATION );
					float	sinTheta, cosTheta;
					sincos( theta, sinTheta, cosTheta );
					float3	ssUnOccludedDirection = float3( sinTheta * ssDirection, cosTheta );

					float	weight = abs(sinTheta);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
					ssBentNormal += weight * ssUnOccludedDirection;
				}

				float	dTheta = (thetaFront - thetaBack) / USE_NUMERICAL_INTEGRATION;
				ssBentNormal *= dTheta;
			#else
				// Integral computation
				float	theta0 = -acos( maxCosTheta_Back );
				float	theta1 = acos( maxCosTheta_Front );
				float	cosTheta0 = maxCosTheta_Back;
				float	cosTheta1 = maxCosTheta_Front;
				float	sinTheta0 = -sqrt( 1.0 - cosTheta0*cosTheta0 );
				float	sinTheta1 = sqrt( 1.0 - cosTheta1*cosTheta1 );

				float	averageX = theta1 + theta0 - sinTheta0*cosTheta0 - sinTheta1*cosTheta1;
				float	averageY = 2.0 - cosTheta0*cosTheta0 - cosTheta1*cosTheta1;

				float3	ssBentNormal = float3( averageX * ssDirection, averageY );
			#endif
		#endif

		ssAverageBentNormal += ssBentNormal;	// Accumulate unnormalized!

		// Compute cone angles
		ssBentNormal = normalize( ssBentNormal );

		float3	ssHorizon_Front = float3( sqrt( 1.0 - maxCosTheta_Front*maxCosTheta_Front ) * ssDirection, maxCosTheta_Front );
		float3	ssHorizon_Back = float3( -sqrt( 1.0 - maxCosTheta_Back*maxCosTheta_Back ) * ssDirection, maxCosTheta_Back );
		#if USE_FAST_ACOS
			float	coneAngle_Front = FastPosAcos( saturate( dot( ssBentNormal, ssHorizon_Front ) ) );
			float	coneAngle_Back = FastPosAcos( saturate( dot( ssBentNormal, ssHorizon_Back ) ) ) ;
		#else
			float	coneAngle_Front = acos( saturate( dot( ssBentNormal, ssHorizon_Front ) ) );
			float	coneAngle_Back = acos( saturate( dot( ssBentNormal, ssHorizon_Back ) ) );
		#endif

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
	#if SUBDIV_COUNT_ANGULAR > 2
		varianceConeAngle /= 2.0*SUBDIV_COUNT_ANGULAR - 1.0;
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
		stdDeviation = 0.5 * PI * (1.0 - bentCone.w);
		averageConeAngle = acos( cosAlpha );
	#endif

	// Finalize indirect irradiance
	const float	dPhi = PI / SUBDIV_COUNT_ANGULAR;	// Hemisphere is sliced into 2*SUBDIV_COUNT_ANGULAR parts
	sumIrradiance *= dPhi;

	// Compute this frame's irradiance
	// Increase cone aperture a little (based on manual fitting of irradiance compared to ground truth)
//	float	samplingConeAngle = averageConeAngle + stdDeviation * lerp( -1.0, 1.0, _debugValues.z );
	float	samplingConeAngle = averageConeAngle + -0.2 * stdDeviation;	// Good value

	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
//	float3	directIrradiance = EvaluateSHIrradiance( N, 0.0, SH );											// Use normal direction
	float3	directIrradiance = EvaluateSHIrradiance( ssAverageBentNormal, cos( samplingConeAngle ), SH );	// Use bent-normal direction + cone angle

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
//Out.bentCone = float4( 0.5 * (1.0+ssAverageBentNormal), 1.0 );

	return Out;
}
