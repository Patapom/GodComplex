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
/*
float2	CameraSpacePosition_mm2UV( float3 _position_mm ) {
	return _position_mm.xy / (_texelSize_mm * _textureDimensions);
}
float3	GetCameraSpacePosition_mm( float2 _pixelPosition ) {
	float3	position_mm = float3( _texelSize_mm * _pixelPosition, 0.0 );
//			position_mm.z = _displacement_mm * _texHeight.SampleLevel( SAMPLER, CameraSpacePosition_mm2UV( position_mm ), 0.0 );
			position_mm.z = _displacement_mm * _texHeight.SampleLevel( SAMPLER, _pixelPosition / _textureDimensions, 0.0 );
	return position_mm;
}
// Projects a camera space position into surface's local tangent space
float3	Project( float3 _csPosition_mm, float3 _csCenterPosition_mm, float3 _T, float3 _B, float3 _N ) {
	float3	csDeltaPosition_mm = _csPosition_mm - _csCenterPosition_mm;

	float	verticalRatio = _texelSize_mm / _displacement_mm;
	_T.z *= verticalRatio;
	_B.z *= verticalRatio;
	_N.z *= verticalRatio;

	return float3( dot( csDeltaPosition_mm, _T ), dot( csDeltaPosition_mm, _B ), dot( csDeltaPosition_mm, _N ) );
}

// Computes the integration of the spherical solid angle between two elevations given by their tangents
// We are given the integral:
//		Lo(x,wo) = Integral{0,PI/2}( Li(x,wi) (wi.n) dwi )
// If we assume Li(x,wi) is constant then we are left with the simple integral:
//		Lo(x,wo) = Li(x,wi) Integral{0,PI/2}( (wi.n) dwi )
// dwi is the PHI part of the integrand that depends on theta: sin(theta) dtheta and finally
//		Lo(x,wo) = Li(x,wi) Integral{0,PI/2}( cos(theta) sin(theta) dtheta ) = Li(x,wi) * [-1/2 cos²(theta)]{0,PI/2} = Li(x,wi) * 0.5 * [cos²(theta0) - cos²(theta1)]
// We let theta0 and theta1 be free here since we don't necessarily need to integrate over the entire hemisphere
//
float	IntegrateSolidAngle( float _cos2Theta0, float _cos2Theta1 ) {
	return 0.5 * (_cos2Theta0 - _cos2Theta1);
}
*/
// Samples the radiance reflected from a screen-space position located around the center pixel position
//	_ssPosition, the screen-space position to sample
//	_radius, the radius from the center position
//	T, B, N, the local tangent space
//	_centerRho, the reflectance of the center pixel (fallback only necessary if no albedo map is available)
//	_maxCos, the floating max cos(theta) that indicates the perceived horzion
//
float3	SampleRadiance( float2 _ssPosition, float _radius, float3 _T, float3 _B, float3 _N, float3 _centerRho, inout float _maxCos ) {

	// Sample new height and update horizon angle
	float	H = (_displacement_mm / _texelSize_mm) * _texHeight.SampleLevel( SAMPLER, _ssPosition / _textureDimensions, 0.0 );
	float	H2 * H * H;
	float	hyp2 = _radius*_radius + H2;	// Square hypotenuse
	float	cosHorizon = H / sqrt( hyp2 );	// Cosine to horizon angle
	if ( cosHorizon <= _maxCos )
		return 0.0;	// Below the horizon... No visible contribution.

	// Compute bounced incoming radiance
	float2	neighborUV = CameraSpacePosition_mm2UV( csNeighborCameraPosition_mm );
	float3	neighborIrradiance = _texSourceIrradiance.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;
//	float3	neighborRho = _texSourceAlbedo.SampleLevel( SAMPLER, neighborUV, 0.0 ).xyz;		// #TODO: Sample albedo G-buffer entry for better reflectance
	float3	neighborRho = _centerRho;														// Uniform reflectance in neighborhood

	float3	incomingRadiance = (neighborRho / PI) * neighborIrradiance;						// Li-1
//			incomingRadiance *= IntegrateSolidAngle( _maxCos2, cos2 );						// * Integral{theta0, theta1}( cos(theta) sin(theta) dtheta )
			incomingRadiance *= 0.5 * (cos2 - _maxCos2);									// * Integral{theta0, theta1}( cos(theta) sin(theta) dtheta ) = [cos²(theta)/2]{theta0, theta1}

	// #TODO: Integrate with linear interpolation of irradiance as well??

	_maxCos = cosHorizon;		// Register a new positive horizon

	return incomingRadiance;
}

PS_OUT	PS( VS_IN _In ) {
	// Retrieve central height and TBN
	float3	N = 2.0 * _texNormal[_In.__Position.xy].xyz - 1.0;
//			N.y *= -1.0;
	float3	T, B;
	BuildOrthonormalBasis( N, T, B );

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
		float	recZdotN = abs(N.z) > 1e-6 ? 1.0 / N.z : 1e6 * sign(N.z);
		float	hitDistance_Front = -dot( ssDirection, N.xy ) * recZdotN;
		float3	tsDirection_Front = normalize( float3( ssDirection, hitDistance_Front ) );
		float	maxCos_Front = tsDirection_Front.z;
		float	maxCos_Back = -tsDirection_Front.z;

		// Accumulate perceived irradiance in front and back & update floating horizon (in the form of cos(horizon angle))
		float2	ssPosition_Front = _In.__Position.xy;
		float2	ssPosition_Back = _In.__Position.xy;
		float2	ssStep = ssDirection * RADIUS_STEP_SIZE;

		for ( uint radiusIndex=1; radiusIndex <= MAX_RADIUS; radiusIndex++ ) {
			ssPosition_Front += ssStep;
			ssPosition_Back -= ssStep;
			sumIrradiance += SampleRadiance( ssPosition_Front, radius, T, B, N, centerRho, maxCos_Front );	// Sample forward
			sumIrradiance += SampleRadiance( ssPosition_Back, radius, T, B, N, centerRho, maxCos_Back );	// Sample backward
		}

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
			float2	scTheta;
			sincos( theta, scTheta.x, scTheta.y );
			float3	ssUnOccludedDirection = float3( scTheta.x * ssDirection, scTheta.y );

			float	cosAlpha = saturate( dot( ssUnOccludedDirection, N ) );

			float	weight = cosAlpha * abs(scTheta.x);		// cos(alpha) * sin(theta).dTheta  (be very careful to take abs(sin(theta)) because our theta crosses the pole and becomes negative here!)
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

	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0.0 );
	Out.bentCone = float4( max( 0.01, cos( averageConeAngle ) ) * ssAverageBentNormal, 1.0 - stdDeviation );

//Out.bentCone.xyz = csCenterPosition_mm / 1024.0;
//Out.bentCone.xyz = N;
//Out.bentCone.xyz = 0.5 * (1.0+N);
//Out.bentCone.xyz = 0.5 * (1.0 + csBentNormal);
//Out.bentCone = _texelSize_mm / (1000.0 / 512) / 2;

	return Out;
}
