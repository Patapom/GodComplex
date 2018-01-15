////////////////////////////////////////////////////////////////////////////////
// Compute indirect irradiance from last frame's irradiance + bent cone direction
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"


const uint	MAX_ANGLES = 8;			// Amount of angular subdivisions of the circle
const uint	MAX_RADIUS = 8;			// Amount of radial subdivisions of the circle
const float	RADIUS_STEP_SIZE = 1.0;	// Radial step size (in pixels)

cbuffer	CBMain : register( b0 ) {
	uint2	_textureDimensions;	// Height map resolution
	float	_texelSize_mm;		// Texel size in millimeters
	float	_displacement_mm;	// Height map max encoded displacement in millimeters

	float3	_rho;
}

Texture2D<float>		_texHeight : register( t0 );
Texture2D<float3>		_texNormal : register( t1 );
Texture2D<float2>		_texAO : register( t2 );
//Texture2DArray<float4>	_texGroundTruthIrradiance : register( t3 );
//Texture2D<float4>		_texBentCone : register( t4 );

Texture2D<float4>		_texSourceIrradiance : register();		// Irradiance from last frame

struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float3	irradiance	: SV_TARGET0;
	float4	bentCone	: SV_TARGET1;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float2	CameraSpacePosition_mm2UV( float3 _position_mm ) {
	return _position_mm.xy / (_texelSize_mm * _textureDimensions);
}
float3	GetCameraSpacePosition_mm( float2 _pixelPosition ) {
	float3	position_mm = float3( _texelSize_mm * _pixelPosition, 0.0 );
			position_mm.z = _displacement_mm * _texHeight.SampleLevel( LinearClamp, CameraSpacePosition_mm2UV( position_mm ), 0.0 );
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
//	_maxCos2, the floating max cos²(theta) that indicates the perceived horzion
//
float3	SampleRadiance( float2 _ssPosition, float3 _csCenterPosition_mm, float _radius_mm, float3 _T, float3 _B, float3 _N, inout float _maxCos2 ) {
//	float3	neighborCameraPosition_mm = float3( _csCenterPosition_mm.xy + _radius_mm * scPhi, 0.0 );
//			neighborCameraPosition_mmm.z = _displacement_mm * _texHeight.SampleLevel( LinearClamp, CameraSpacePosition_mm2UV( neighborPosition_mm ), 0.0 );
	float3	csNeighborCameraPosition_mm = GetCameraSpacePosition_mm( _csCenterPosition_mm );
	float3	neighborPosition_mm = Project( csNeighborCameraPosition_mm, _csCenterPosition_mm, _T, _B, _N );

//	float	tangent = lsNeighborPosition_mm.z / length( lsNeighborPosition_mm.xy );
//	float	cos2 = dot(lsNeighborPosition_mm.xy,lsNeighborPosition_mm.xy) / (dot(lsNeighborPosition_mm.xy,lsNeighborPosition_mm.xy) + lsNeighborPosition_mm.z*lsNeighborPosition_mm.z);
	float	hyp2 = _radius_mm*_radius_mm + lsNeighborPosition_mm.z*lsNeighborPosition_mm.z;	// Square hypotenuse
	float	cos2 = _radius_mm*_radius_mm / hyp2;												// Square cosine
	if ( cos2 <= _maxCos2 )
		return 0.0;	// Below the horizon... No visible contribution.

	// Compute bounced incoming radiance
	float2	neighborUV = CameraSpacePosition_mm2UV( csNeighborCameraPosition_mm );
	float3	neighborIrradiance = _texSourceIrradiance.SampleLevel( LinearClamp, neighborUV, 0.0 ).xyz;
//	float3	neighborRho = _texSourceAlbedo.SampleLevel( LinearClamp, neighborUV, 0.0 ).xyz;	// #TODO: Sample albedo G-buffer entry for better reflectance
	float3	neighborRho = centerRho;														// Uniform reflectance in neighborhood

	float3	incomingRadiance = (neighborRho / PI) * neighborIrradiance;						// Li-1
//			incomingRadiance *= IntegrateSolidAngle( _maxCos2, cos2 );						// * Integral{theta0, theta1}( cos(theta) sin(theta) dtheta )
			incomingRadiance *= 0.5 * (cos2 - _maxCos2);									// * Integral{theta0, theta1}( cos(theta) sin(theta) dtheta ) = [cos²(theta)/2]{theta0, theta1}

	// #TODO: Integrate with linear interpolation of irradiance as well??

	_maxCos2 = cos2;		// Register a new positive horizon

	return incomingRadiance;
}

PS_OUT	PS( VS_IN _In ) {
//	float2	UV = _In.__Position.xy / _resolution;
//	float2	AO_E0 = _texAO.Sample( LinearClamp, UV );
//	return AO_E0.x / (2.0*PI);

	// Retrieve central height and TBN
//	float3	csCenterPosition_mm = float3( _texelSize_mm * _In.__Position.xy, 0.0 );
//			csCenterPosition_mm.z = _displacement_mm * _texHeight.SampleLevel( LinearClamp, CameraSpacePosition_mm2UV( csCenterPosition_mm ), 0.0 );
	float3	csCenterPosition_mm = GetCameraSpacePosition_mm( _In.__Position.xy );
	float3	N = 2.0 * _texNormal.SampleLevel( LinearClamp, CameraSpacePosition_mm2UV( csCenterPosition_mm ), 0.0 ).xyz - 1.0;
			N.y *= -1.0;
	float3	T, B;
	BuildOrthonormalBasis( N, T, B );

	float3	centerRho = _rho;	// Uniform reflectance

	// Samples circular surroundings in screen space
	float3	tsAverageBentNormal = 0.0;
	float	averageConeAngle = 0.0;
	float	varianceConeAngle = 0.0;
	float3	sumIrradiance = 0.0;
	for ( uint angleIndex=0; angleIndex < MAX_ANGLES; angleIndex++ ) {
		float	phi = PI * angleIndex / MAX_ANGLES;
		float2	scPhi;
		sincos( phi, scPhi.x, scPhi.y );

// #TODO: Project first, convert in mm, use this to advance in SS in //, reproject?

		// Accumulate perceived irradiance in front/back
		float	maxCos2_p = 0.0;	// Initialize positive horizon to flat
		float	maxCos2_n = 0.0;	// Initialize negative horizon to flat
		for ( uint radiusIndex=1; radiusIndex <= MAX_RADIUS; radiusIndex++ ) {
			float	radiusPixel = RADIUS_STEP_SIZE * radiusIndex;
			float	radius_mm = _texelSize_mm * radiusPixel;
			sumIrradiance += SampleRadiance( _In.__Position.xy + radiusPixel * scPhi, csCenterPosition_mm, radius_mm, T, B, N, maxCos2_p );	// Sample forward
			sumIrradiance += SampleRadiance( _In.__Position.xy - radiusPixel * scPhi, csCenterPosition_mm, radius_mm, T, B, N, maxCos2_n );	// Sample backward
		}

		// Compute normalized tangent space direction
		float3	tsDirection = Project( float3( scPhi, 0.0 ), csCenterPosition_mm, T, B, N );
		float	L = length( tsDirection );
				tsDirection *= L > 0.0 ? 1.0 / L : 0.0;

		// Compute min/max horizon vectors & bent normal
		float3	tsHorizon_p = (1.0 - maxCos2_p) * tsDirection + maxCos2_p ) * float3( 0, 0, 1 );	// UN-NORMALIZED!!
		float3	tsHorizon_n = (1.0 - maxCos2_n) * tsDirection + maxCos2_n ) * float3( 0, 0, 1 );	// UN-NORMALIZED!!
		float3	tsBentNormal = normalize( tsHorizon_p + tsHorizon_n );
		tsAverageBentNormal += tsBentNormal;		// Accumulate bent normal direction by rebuilding and averaging the horizon vectors

		// Compute aperture angle
		float	coneAngle = acos( saturate( dot( tsBentNormal, normalize( tsHorizon_p ) ) ) );

		// We're using running variance computation from https://www.johndcook.com/blog/standard_deviation/
		//	Avg(N) = Avg(N-1) + [V(N) - Avg(N-1)] / N
		//	S(N) = S(N-1) + [V(N) - Avg(N-1)] * [V(N) - Avg(N)]
		// And variance = S(finalN) / (finalN-1)
		//
		float	previousAverageConeAngle = averageConeAngle;
		averageConeAngle += (coneAngle + averageConeAngle) * (angleIndex+1);
		varianceConeAngle += (coneAngle - previousAverageConeAngle) * (coneAngle - averageConeAngle); 
	}
	varianceConeAngle /= MAX_ANGLES - 1;
	tsAverageBentNormal /= MAX_ANGLES;

	const float	dPhi = PI / MAX_ANGLES;	// Hemisphere is sliced into 2*MAX_ANGLES parts

	PS_OUT	Out;
	Out.irradiance = sumIrradiance * dPhi;
	Out.bentCone = float4( tsAverageBentNormal, varianceConeAngle );

	return Out;
}
