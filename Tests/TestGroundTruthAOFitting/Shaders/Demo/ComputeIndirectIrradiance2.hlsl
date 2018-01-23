////////////////////////////////////////////////////////////////////////////////
// Compute indirect irradiance from last frame's irradiance + bent cone direction
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

#define	MAX_ANGLES	16			// Amount of angular subdivisions of the circle
#define	MAX_RADIUS	16			// Amount of radial subdivisions of the circle
#define	RADIUS_STEP_SIZE 2.0	// Radial step size (in pixels)

#define	SAMPLER	LinearWrap

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

Texture2D<float4>		_texSourceIrradiance : register( t5 );		// Irradiance from last frame

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
//	E = Integral[theta0, theta1]{ (rho(Xi)/PI * E(Xi)) (Wi.N) sin(theta) dTheta }
// Where
//	Xi, the location of the neighbor surface reflecting radiance
//	rho(Xi), the surface's albedo at that location
//	E(Xi), the surface's irradiance at that location
//	Wi, the direction of the vector within the slice that we're integrating Wi = { cos(phi).sin(theta), sin(phi).sin(theta), cos(theta) } expressed in screen space
//	phi, the azimutal angle of the screen-space slice
//
// The integral transforms into an irradiance-agnostic version:
//	E = (rho(Xi)/PI * E(Xi)) * Integral[theta0, theta1]{ (Wi.N) sin(theta) dTheta }
//
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
float3	SampleRadiance( float2 _ssPosition, float _H0, float _radius, float2 _sinCosPhi, float3 _T, float3 _B, float3 _N, float3 _centerRho, inout float _maxCos ) {

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
		float3	incomingRadiance = _texSourceIrradiance.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;	// Directly Li-1 (Ei is already pre-multiplied by albedo/rho from last frame so don't bother!)
	#else
		float3	neighborIrradiance = _texSourceIrradiance.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;	// Only Ei is available, Li need to be computed by fetching albedo
//		float3	neighborRho = _texSourceAlbedo.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;				// #TODO: Sample albedo G-buffer entry for better reflectance
		float3	neighborRho = _centerRho;																// Uniform reflectance in neighborhood
		float3	incomingRadiance = (neighborRho / PI) * neighborIrradiance;								// Li-1
	#endif

	// Integrate over horizon difference
	incomingRadiance *= IntegrateSolidAngle( _sinCosPhi, _maxCos, cosHorizon, _N );

	// #TODO: Integrate with linear interpolation of irradiance as well??

	_maxCos = cosHorizon;		// Register a new positive horizon

	return incomingRadiance;
}

PS_OUT	PS( VS_IN _In ) {
	// Retrieve central height and TBN
	float	H0 = (_displacement_mm / _texelSize_mm) * _texHeight.SampleLevel( SAMPLER, _In.__Position.xy / _textureDimensions, 0.0 );
	float3	N = 2.0 * _texNormal[_In.__Position.xy].xyz - 1.0;
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
		float	phi = PI * angleIndex / MAX_ANGLES;
		float2	ssDirection;
		sincos( phi, ssDirection.y, ssDirection.x );

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
			sumIrradiance += SampleRadiance( ssPosition_Front, H0, radius, ssDirection.yx, T, B, N, centerRho, maxCos_Front );	// Sample forward
			sumIrradiance += SampleRadiance( ssPosition_Back, H0, radius, ssDirection.yx, T, B, N, centerRho, maxCos_Back );	// Sample backward
		}
//*/
		// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
		// Half brute force where we perform the integration numerically as a sum...
		// This solution is prefered to the analytical integral that shows some precision artefacts unfortunately...
		//
		float	thetaFront = acos( maxCos_Front );
		float	thetaBack = -acos( maxCos_Back );

		float3	ssBentNormal = 0.001 * N;
		const uint	STEPS_COUNT = 256;
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
		float3	ssHorizon = float3( sqrt( 1.0 - maxCos_Front*maxCos_Front ) * ssDirection, maxCos_Front );
		float	coneAngle = acos( dot( ssBentNormal, ssHorizon ) );

		// We're using running variance computation from https://www.johndcook.com/blog/standard_deviation/
		//	Avg(N) = Avg(N-1) + [V(N) - Avg(N-1)] / N
		//	S(N) = S(N-1) + [V(N) - Avg(N-1)] * [V(N) - Avg(N)]
		// And variance = S(finalN) / (finalN-1)
		//
		float	previousAverageConeAngle = averageConeAngle;
		averageConeAngle += (coneAngle + averageConeAngle) / (angleIndex+1);
		varianceConeAngle += (coneAngle - previousAverageConeAngle) * (coneAngle - averageConeAngle); 
	}

	// Finalize bent cone
	#if MAX_ANGLES > 2
		varianceConeAngle /= MAX_ANGLES - 1;
	#endif
	ssAverageBentNormal = normalize( ssAverageBentNormal );

	float	stdDeviation = sqrt( varianceConeAngle );

	// Finalize indirect irradiance
	const float	dPhi = PI / MAX_ANGLES;	// Hemisphere is sliced into 2*MAX_ANGLES parts
	sumIrradiance *= dPhi;

	// Compute this frame's irradiance
	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
//	sumIrradiance += EvaluateSHIrradiance( N, 0.0, SH );
	sumIrradiance += EvaluateSHIrradiance( ssAverageBentNormal, cos( averageConeAngle ), SH );

//sumIrradiance =0;


	#if PREMULTIPLY_ALBEDO
	// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	// This MUST be done either now, or later when the irradiance is used!
	// My advice is to use it right now before writing to the texture so next frame, (rho/PI) * Irradiance is available straight from the sampler
	//	and this avoids to sample the neighbor albedo map
		sumIrradiance *= (centerRho / PI);
	// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	#endif


	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0.0 );
//	Out.bentCone = float4( max( 0.01, cos( averageConeAngle ) ) * ssAverageBentNormal, 1.0 - stdDeviation );
ssAverageBentNormal.y *= -1.0;
Out.bentCone = float4( ssAverageBentNormal, 1.0 );

//ssAverageBentNormal.y *= -1.0;
//Out.bentCone = float4( 0.5 * (1.0+ssAverageBentNormal), 1.0 );

	return Out;
}
