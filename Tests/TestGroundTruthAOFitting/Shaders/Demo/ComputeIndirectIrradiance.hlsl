////////////////////////////////////////////////////////////////////////////////
// Compute indirect irradiance from last frame's irradiance + bent cone direction
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

static const uint	MAX_ANGLES = 16;			// Amount of angular subdivisions of the circle
static const uint	MAX_RADIUS = 16;			// Amount of radial subdivisions of the circle
static const float	RADIUS_STEP_SIZE = 2.0;	// Radial step size (in pixels)

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

// Samples the radiance reflected from a screen-space position located around 
//	_ssPosition, the screen-space position to sample
//	_csCenterPosition_mm, the camera-space center position
//	_radius_mm, the radius (in millimeters) from the center position
//	T, B, N, the local tangent space
//	_centerRho, the reflectance of the center pixel (fallback only necessary if no albedo map is available)
//	_maxCos2, the floating max cos²(theta) that indicates the perceived horzion
//
float3	SampleRadiance( float2 _ssPosition, float3 _csCenterPosition_mm, float _radius_mm, float3 _T, float3 _B, float3 _N, float3 _centerRho, inout float _maxCos2 ) {
	// Transform screen-space position into tangent-space position
	float3	csNeighborCameraPosition_mm = GetCameraSpacePosition_mm( _ssPosition );
	float3	lsNeighborPosition_mm = Project( csNeighborCameraPosition_mm, _csCenterPosition_mm, _T, _B, _N );

//return float3( csNeighborCameraPosition_mm.xy / 1000.0, 0 );
//return float3( lsNeighborPosition_mm.xy / 10.0, 0 );
//return (csNeighborCameraPosition_mm - _csCenterPosition_mm) / 20.0;
//return float3( (csNeighborCameraPosition_mm - _csCenterPosition_mm).xy / 20.0, 0 );

	float	H2 = lsNeighborPosition_mm.z*lsNeighborPosition_mm.z;
	float	hyp2 = _radius_mm*_radius_mm + H2;	// Square hypotenuse
	float	cos2 = H2 / hyp2;					// Square cosine
//return 0.01 * hyp2;
//return cos2;
	if ( cos2 <= _maxCos2 )
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

	_maxCos2 = cos2;		// Register a new positive horizon

	return incomingRadiance;
}

PS_OUT	PS( VS_IN _In ) {
//	float2	UV = _In.__Position.xy / _resolution;
//	float2	AO_E0 = _texAO.Sample( SAMPLER, UV );
//	return AO_E0.x / (2.0*PI);

	// Retrieve central height and TBN
//	float3	csCenterPosition_mm = float3( _texelSize_mm * _In.__Position.xy, 0.0 );
//			csCenterPosition_mm.z = _displacement_mm * _texHeight.SampleLevel( SAMPLER, CameraSpacePosition_mm2UV( csCenterPosition_mm ), 0.0 );
	float3	csCenterPosition_mm = GetCameraSpacePosition_mm( _In.__Position.xy );
	float3	N = 2.0 * _texNormal.SampleLevel( SAMPLER, CameraSpacePosition_mm2UV( csCenterPosition_mm ), 0.0 ).xyz - 1.0;
			N.y *= -1.0;
	float3	T, B;
	BuildOrthonormalBasis( N, T, B );

	float3	centerRho = _rho;	// Uniform reflectance

	// Samples circular surroundings in screen space
	float3	tsAverageBentNormal = 0.0;
	float	averageConeAngle = 0.0;
	float	varianceConeAngle = 0.0;
	float3	sumIrradiance = 0.0;

#if 1
	#if MAX_ANGLES > 1
		for ( uint angleIndex=0; angleIndex < MAX_ANGLES; angleIndex++ ) {
	#else
	{
		uint	angleIndex = 0;
	#endif

		// Compute normalized screen-space and tangent-space direction
		float	phi = PI * angleIndex / MAX_ANGLES;
		float2	ssDirection;
		sincos( phi, ssDirection.y, ssDirection.x );

		float3	tsDirection = float3( dot( ssDirection, T.xy ), dot( ssDirection, B.xy ), dot( ssDirection, N.xy ) );
		float	L = length( tsDirection );
				tsDirection *= L > 0.0 ? 1.0 / L : 0.0;

// #TODO: Project first, convert in mm, use this to advance in SS in //, reproject?

		// Accumulate perceived irradiance in front & back + update floating horizon (in the form of cos²(horizon angle))
		float	maxCos2_p = 1e-3;	// Initialize positive horizon to flat
		float	maxCos2_n = 1e-3;	// Initialize negative horizon to flat
		for ( uint radiusIndex=1; radiusIndex <= MAX_RADIUS; radiusIndex++ ) {
			float	radiusPixel = RADIUS_STEP_SIZE * radiusIndex;
			float	radius_mm = _texelSize_mm * radiusPixel;
			sumIrradiance += SampleRadiance( _In.__Position.xy + radiusPixel * ssDirection, csCenterPosition_mm, radius_mm, T, B, N, centerRho, maxCos2_p );	// Sample forward
			sumIrradiance += SampleRadiance( _In.__Position.xy - radiusPixel * ssDirection, csCenterPosition_mm, radius_mm, T, B, N, centerRho, maxCos2_n );	// Sample backward
		}


//maxCos2_p = 0;
//maxCos2_n = 0;

		// Accumulate bent normal direction by rebuilding and averaging the front & back horizon vectors
		float3	tsHorizon_p = (1.0 - maxCos2_p) * tsDirection + float3( 0, 0, maxCos2_p );	// UN-NORMALIZED!!
#if 0
// Verbose
		float3	tsHorizon_n = (maxCos2_n - 1.0) * tsDirection + float3( 0, 0, maxCos2_n );	// UN-NORMALIZED!!
//		float3	tsBentNormal = normalize( tsHorizon_p + tsHorizon_n );
		float3	tsBentNormal = tsHorizon_p + tsHorizon_n;
		tsAverageBentNormal += tsBentNormal;
#else
// Optimized
		float3	tsBentNormal = (maxCos2_n - maxCos2_p) * tsDirection;
				tsBentNormal.z += maxCos2_n + maxCos2_p;
		tsAverageBentNormal += tsBentNormal;
#endif

		// Update average aperture angle and variance
		float	coneAngle = acos( saturate( dot( tsBentNormal, normalize( tsHorizon_p ) ) ) );	// #TODO: Optimize! Can't exploit cos² and sin² for a fast dot?

		// We're using running variance computation from https://www.johndcook.com/blog/standard_deviation/
		//	Avg(N) = Avg(N-1) + [V(N) - Avg(N-1)] / N
		//	S(N) = S(N-1) + [V(N) - Avg(N-1)] * [V(N) - Avg(N)]
		// And variance = S(finalN) / (finalN-1)
		//
		float	previousAverageConeAngle = averageConeAngle;
		averageConeAngle += (coneAngle + averageConeAngle) * (angleIndex+1);
		varianceConeAngle += (coneAngle - previousAverageConeAngle) * (coneAngle - averageConeAngle); 
	}

	// Finalize bent cone
	#if MAX_ANGLES > 1
		varianceConeAngle /= MAX_ANGLES - 1;
	#endif
//	tsAverageBentNormal /= MAX_ANGLES;
	float3	csBentNormal = tsAverageBentNormal.x * T + tsAverageBentNormal.y * B + tsAverageBentNormal.z * N;	// Back in camera space!


//csBentNormal = normalize(csBentNormal);


	// Finalize indirect irradiance
	const float	dPhi = PI / MAX_ANGLES;	// Hemisphere is sliced into 2*MAX_ANGLES parts
	sumIrradiance *= dPhi;

	// Compute this frame's irradiance
	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
	sumIrradiance += EvaluateSHIrradiance( N, 0.0, SH );

//sumIrradiance =0;

#else
	// DEBUG
	float2	scPhi = float2( 0, 1 );
	float	radiusPixel = 4.0;
	float	radius_mm = _texelSize_mm * radiusPixel;
	float	maxCos2_p = 1e-3;
	float3	csBentNormal = SampleRadiance( _In.__Position.xy + radiusPixel * scPhi.yx, csCenterPosition_mm, radius_mm, T, B, N, centerRho, maxCos2_p );

//csBentNormal = maxCos2_p * 0.5;
#endif

	PS_OUT	Out;
	Out.irradiance = float4( sumIrradiance, 0.0 );
	Out.bentCone = float4( csBentNormal, varianceConeAngle );
//	Out.bentCone = float4( N, varianceConeAngle );

//Out.bentCone.xyz = csCenterPosition_mm / 1024.0;
//Out.bentCone.xyz = N;
//Out.bentCone = _texelSize_mm / (1000.0 / 512) / 2;

	return Out;
}
