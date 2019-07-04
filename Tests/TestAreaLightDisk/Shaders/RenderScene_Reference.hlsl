#include "Global.hlsl"

struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float3	color : SV_TARGET0;
	float	depth : SV_DEPTH;
};

VS_IN	VS( VS_IN _in ) { return _in; }


// Computes the area of an elliptical disk sector comprised between angles [Phi0,Phi1] and the normalized radii [t0,t1]
// The radius of the elliptical disk of radii (Rx, Ry) depends on the angle phi based on the relationship:
//	r(phi)² = Rx² cos(phi)² + Ry² sin(phi)²
// The area of a surface element for the ellipse is dA = Rx Ry r dr dPhi where r is the normalized radius in [0,1]
//
// The double integral to compute the area is thus:
//	A = Integral[Phi0, Phi1]{ Integral[r0, r1]{ dA } } = Rx Ry (Phi1 - Phi0) * [r1² - r0²] / 2
//
float	EllipticalDiskArea( float _Rx, float _Ry, float _phi0, float _phi1, float _r0, float _r1 ) {
	return _Rx * _Ry * (_phi1 - _phi0) * (_r1*_r1 - _r0*_r0) / 2.0;
}

float3	ComputeRadiance_DiffuseLambert( float3 _wsPosition, float3 _wsNormal, uint2 _seed ) {

	const uint	SAMPLES_PHI_COUNT = 64;
	const uint	SAMPLES_RHO_COUNT = 32;

	const float3	wsAxisX = _wsLight2World[0].xyz;
	const float3	wsAxisY = _wsLight2World[1].xyz;
	const float3	wsAxisZ = _wsLight2World[2].xyz;
	const float3	wsPosition = _wsLight2World[3].xyz;
	const float		Rx = _wsLight2World[0].w;
	const float		Ry = _wsLight2World[1].w;

	const float	diskArea = Rx * Ry * (2.0 * PI / SAMPLES_PHI_COUNT) * (0.5 / SAMPLES_RHO_COUNT);	// Assuming we choose rho(t) = sqrt( t ), all samples will have the same area

	float	sumLuminance = 0.0;
//float	sumLuminance = 1e38;
	for ( uint phiIndex=0; phiIndex < SAMPLES_PHI_COUNT; phiIndex++, _seed.y++ ) {
		float	phi = 2*PI * phiIndex / SAMPLES_PHI_COUNT;
		float2	scPhi;
		sincos( phi, scPhi.x, scPhi.y );

		for ( uint rhoIndex=0; rhoIndex < SAMPLES_RHO_COUNT; rhoIndex++, _seed.x++ ) {
			float	rho = sqrt( (rhoIndex + 0.5) / SAMPLES_RHO_COUNT );

			float3	wsSamplePosition = wsPosition + rho * (Rx * wsAxisX * scPhi.y + Ry * wsAxisY * scPhi.x);
			float3	wsLight = wsSamplePosition - _wsPosition;
			float	distance = length( wsLight );
			if ( distance < 1e-4 )
				continue;

			wsLight /= distance;

			float	LdotN = dot( wsLight, _wsNormal );
			if ( LdotN <= 0.0 )
				continue;	// The sample is BELOW the surface

			float	projectedDiskArea = -dot( wsLight, wsAxisZ ) * diskArea;		// The sample's perceived area, depending on the angle of the light vector with the disk
			if ( projectedDiskArea <= 0.0 )
				continue;	// The sample is BEHIND the disk

			float	sampleSolidAngle = projectedDiskArea / (distance * distance);	// Actual solid angle = Area / r²

			sumLuminance += _diskLuminance * (1.0 / PI) * LdotN * sampleSolidAngle;	// The illuminance (lm/m² or lux) given by the perceived elliptical disk sample

//sumLuminance = min( sumLuminance, distance );

		}
	}

	return sumLuminance;
}

//float3	ComputeRadiance_SpecularGGX( float3 _wsPosition, float3 _wsNormal, float _sqRoughness, uint2 _seed ) {
//}

PS_OUT	PS( VS_IN _In ) {
	float3	csView = GenerateCameraRay( _In.__Position.xy );
	float3	wsView = mul( float4( csView, 0 ), _camera2World ).xyz;
	float3	wsCamPos = _camera2World[3].xyz;

	float	t = -wsCamPos.y / wsView.y;
	clip( t );

	float3	wsPos = wsCamPos + t * wsView;

	float3	wsNormal = float3( 0, 1, 0 );	// Simple plane
	float3	surfaceAlbedo = 0.5;			// Assume a regular 50% diffuse reflectance

	// Compute reference diffuse lighting
	float3	diffuse = ComputeRadiance_DiffuseLambert( wsPos, wsNormal, uint2( _In.__Position.xy ) );
			diffuse *= surfaceAlbedo;

	float4	ndcPos = mul( float4( wsPos, 1 ), _world2Proj );

	PS_OUT	result;
	result.color = diffuse;
	result.depth = ndcPos.z / ndcPos.w;

	return result;
}
