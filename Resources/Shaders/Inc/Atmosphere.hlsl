////////////////////////////////////////////////////////////////////////////////////////
// Atmosphere Helpers
// From http://www-ljk.imag.fr/Publications/Basilic/com.lmc.publi.PUBLI_Article@11e7cdda2f7_f64b69/article.pdf
// Much code stolen from Bruneton's sample
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _ATMOSPHERE_INC_
#define _ATMOSPHERE_INC_

static const float	SUN_INTENSITY = 100.0;

static const float	WORLD2KM = 1.0;							// 1 World unit equals 1.0km

#define	INSCATTER_NON_LINEAR_VIEW
#define	INSCATTER_NON_LINEAR_SUN

static const float	ATMOSPHERE_THICKNESS_KM = 60.0;
static const float	GROUND_RADIUS_KM = 6360.0;
static const float	ATMOSPHERE_RADIUS_KM = GROUND_RADIUS_KM + ATMOSPHERE_THICKNESS_KM;

static const float3	EARTH_CENTER_KM = float3( 0.0, -GROUND_RADIUS_KM, 0.0 );			// Far below us!

// Rayleigh Scattering
static const float3	SIGMA_SCATTERING_RAYLEIGH = float3( 0.0058, 0.0135, 0.0331 );		// For lambdas (680,550,440) nm


// 4D table resolution
static const float	RESOLUTION_COS_THETA_SUN = 32;										// Resolution for the Sun/Zenith angle
static const float	RESOLUTION_COS_GAMMA = 8;											// Resolution for the Sun/View angle)
static const float	RESOLUTION_U = RESOLUTION_COS_THETA_SUN * RESOLUTION_COS_GAMMA;		// U Size (Sun/Zenith + Sun/View packed along a single dimension)
static const float	RESOLUTION_COS_THETA = 128;											// V size (View/Zenith angle)
static const float	RESOLUTION_ALTITUDE = 32;											// W Size (Altitude)

static const float	MODULO_U = 1.0 / RESOLUTION_COS_GAMMA;								// Modulo to access each of the 8 slices of Sun/View angle

static const float	NORMALIZED_SIZE_U1 = 1.0 - 1.0 / RESOLUTION_COS_THETA_SUN;
static const float	NORMALIZED_SIZE_U2 = 1.0 - 1.0 / RESOLUTION_COS_GAMMA;
static const float	NORMALIZED_SIZE_V = 1.0 - 1.0 / RESOLUTION_COS_THETA;
static const float	NORMALIZED_SIZE_W = 1.0 - 1.0 / RESOLUTION_ALTITUDE;

cbuffer	cbAtmosphere	: register( b7 )
{
	float3		_LightDirection;
	float		_SunIntensity;

	float2		_AirParams;			// X=Scattering Factor, Y=Reference Altitude (km)
	float2		_GodraysStrength;	// X=Rayleigh, Y=Mie

	float4		_FogParams;			// X=Scattering Coeff, Y=Extinction Coeff, Z=Reference Altitude (km), W=Anisotropy
	float		_AltitudeOffsetKm;
}

Texture2D	_TexTransmittance : register(t7);
Texture3D	_TexScattering : register(t8);
Texture2D	_TexIrradiance : register(t9);


////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////
// Planetary Helpers
//
void	ComputeSphericalData( float3 _PositionKm, out float _AltitudeKm, out float3 _Normal )
{
	float3	Center2Position = _PositionKm - EARTH_CENTER_KM;
	float	Radius2PositionKm = length( Center2Position );
	_AltitudeKm = Radius2PositionKm - GROUND_RADIUS_KM;
	_Normal = Center2Position / Radius2PositionKm;
}

// ====== Intersections ======

// Computes the enter intersection of a ray and a sphere
float	SphereIntersectionEnter( float3 _PositionKm, float3 _View, float _SphereAltitudeKm )
{
	float	R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	float3	D = _PositionKm - EARTH_CENTER_KM;
	float	c = dot(D,D) - R*R;
	float	b = dot(D,_View);

	float	Delta = b*b - c;

	return Delta > 0.0 ? -b - sqrt(Delta) : INFINITY;
}

// Computes the exit intersection of a ray and a sphere
// (No check for validity!)
float	SphereIntersectionExit( float3 _PositionKm, float3 _View, float _SphereAltitudeKm )
{
	float	R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	float3	D = _PositionKm - EARTH_CENTER_KM;
	float	c = dot(D,D) - R*R;
	float	b = dot(D,_View);

	float	Delta = b*b - c;

	return Delta > 0.0 ? -b + sqrt(Delta) : INFINITY;
}

// Computes both intersections of a ray and a sphere
// Returns INFINITY if no hit is found
float2	SphereIntersections( float3 _PositionKm, float3 _View, float _SphereAltitudeKm )
{
	float	R = _SphereAltitudeKm + GROUND_RADIUS_KM;
	float3	D = _PositionKm - EARTH_CENTER_KM;
	float	c = dot(D,D) - R*R;
	float	b = dot(D,_View);

	float	Delta = b*b - c;
	if ( Delta < 0.0 )
		return INFINITY;

	Delta = sqrt(Delta);

	return float2( -b - Delta, -b + Delta );
}

// Computes the nearest hit between provided sphere and ground sphere
float	ComputeNearestHit( float3 _PositionKm, float3 _View, float _SphereAltitudeKm, out bool _IsGround )
{
	float2	GroundHit = SphereIntersections( _PositionKm, _View, 0.0 );
	float	SphereHit = SphereIntersectionExit( _PositionKm, _View, _SphereAltitudeKm );

	_IsGround = false;
	if ( GroundHit.x < 0.0 || SphereHit < GroundHit.x )
		return SphereHit;	// We hit the top of the atmosphere...
	
	// We hit the ground first
	_IsGround = true;
	return GroundHit.x;
}

////////////////////////////////////////////////////////////////////////////////////////
// Phase functions
float	PhaseFunctionRayleigh( float _CosPhaseAngle )
{
    return (3.0 / (16.0 * PI)) * (1.0 + _CosPhaseAngle * _CosPhaseAngle);
}

float	PhaseFunctionMie( float _CosPhaseAngle )
{
	float	g = _FogParams.w;
	return 1.5 * 1.0 / (4.0 * PI) * (1.0 - g*g) * pow( max( 0.0, 1.0 + (g*g) - 2.0*g*_CosPhaseAngle ), -1.5 ) * (1.0 + _CosPhaseAngle * _CosPhaseAngle) / (2.0 + g*g);
}

// Gets the full Mie RGB components from full Rayleigh RGB and only Mie Red
// This is possible because both values are proportionally related (cf. Bruneton paper, chapter 4 on Angular Precision)
// _RayleighMieRed : XYZ = C*, W=Cmie.red
float3	GetMieFromRayleighAndMieRed( float4 _RayleighMieRed )
{
	return _RayleighMieRed.xyz * (_RayleighMieRed.w * (SIGMA_SCATTERING_RAYLEIGH.x / SIGMA_SCATTERING_RAYLEIGH) / max( _RayleighMieRed.x, 1e-4 ));
}

////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////
// Tables access
float3	GetTransmittance( float _AltitudeKm, float _CosTheta )
{
	float	NormalizedAltitude = sqrt( saturate( _AltitudeKm * (1.0 / ATMOSPHERE_THICKNESS_KM) ) );

const float	TAN_MAX = 1.5;

#if 0
	// Table was packed using the minimum possible angle at this altitude
	float	RadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
	float	CosThetaMin = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (RadiusKm*RadiusKm) );
#else
	// Table uses a fixed minimum angle
	float	CosThetaMin = -0.15;
#endif
 	float	NormalizedCosTheta = atan( (_CosTheta - CosThetaMin) / (1.0 - CosThetaMin) * tan(TAN_MAX) ) / TAN_MAX;

	float2	UV = float2( NormalizedCosTheta, NormalizedAltitude );	// For CosTheta=0.01  => U=0.73294567479959475196454899060789
																	// For CosTheta=0.001 => U=0.7170674487513882415177428025293

	return _TexTransmittance.SampleLevel( LinearClamp, UV, 0.0 ).xyz;
}

float3	GetTransmittanceWithShadow( float _AltitudeKm, float _CosTheta )
{
	float	RadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
	float	CosThetaGround = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM / (RadiusKm*RadiusKm)) );
	return _CosTheta < CosThetaGround ? 0.0 : GetTransmittance( _AltitudeKm, _CosTheta );
}

// Transmittance(=transparency) of atmosphere up to a given distance
// We assume the segment is not intersecting ground
float3	GetTransmittance( float _AltitudeKm, float _CosTheta, float _DistanceKm )
{
	// P0 = [0, _RadiusKm]
	// V  = [SinTheta, CosTheta]
	//
	float	RadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
	float	RadiusKm2 = sqrt( RadiusKm*RadiusKm + _DistanceKm*_DistanceKm + 2.0 * RadiusKm * _CosTheta * _DistanceKm );	// sqrt[ (P0 + d.V)² ]
	float	CosTheta2 = (RadiusKm * _CosTheta + _DistanceKm) / RadiusKm2;												// dot( P0 + d.V, V ) / RadiusKm2
	float	AltitudeKm2 = RadiusKm2 - GROUND_RADIUS_KM;

	return _CosTheta > 0.0	? GetTransmittance( _AltitudeKm, _CosTheta ) / GetTransmittance( AltitudeKm2, CosTheta2 )
							: GetTransmittance( AltitudeKm2, -CosTheta2 ) / GetTransmittance( _AltitudeKm, -_CosTheta );

// 	return _CosTheta > 0.0	? exp( -max( 0.0, GetOpticalDepth( _AltitudeKm, _CosTheta ) - GetOpticalDepth( AltitudeKm2, CosTheta2 ) ) )
// 							: exp( -max( 0.0, GetOpticalDepth( AltitudeKm2, -CosTheta2 ) - GetOpticalDepth( _AltitudeKm, -_CosTheta ) ) );
}

float3	GetIrradiance( Texture2D _TexIrradiance, float _AltitudeKm, float _CosThetaSun )
{
	float	NormalizedAltitude = _AltitudeKm / ATMOSPHERE_THICKNESS_KM;
	float	NormalizedCosThetaSun = (_CosThetaSun + 0.2) / (1.0 + 0.2);
	float2	UV = float2( NormalizedCosThetaSun, NormalizedAltitude );

	return _TexIrradiance.SampleLevel( LinearClamp, UV, 0.0 ).xyz;
}

float3	GetIrradiance( float _AltitudeKm, float _CosThetaSun )
{
	return GetIrradiance( _TexIrradiance, _AltitudeKm, _CosThetaSun );
}

// Samples the scattering table from 4 parameters
float4	Sample4DScatteringTable( Texture3D _TexScattering, float _AltitudeKm, float _CosThetaView, float _CosThetaSun, float _CosGamma )
{
	const float	H = sqrt( ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - GROUND_RADIUS_KM * GROUND_RADIUS_KM );

	float	r = GROUND_RADIUS_KM + _AltitudeKm;
	float	h = sqrt( r * r - GROUND_RADIUS_KM * GROUND_RADIUS_KM );

	float	uAltitude = 0.5 / RESOLUTION_ALTITUDE + (h / H) * NORMALIZED_SIZE_W;

#ifdef INSCATTER_NON_LINEAR_VIEW

	// The idea here is no more to encode cos(Theta_view) for the V coordinate but rather
	//	a value that behaves more nicely.
	//
	// Bruneton et al. chose to encode a ratio of distances instead (cf. fig 3 of http://www-ljk.imag.fr/Publications/Basilic/com.lmc.publi.PUBLI_Article@11e7cdda2f7_f64b69/article.pdf).
	//
	// When the ray is not intersecting the ground, they chose to encode the ratio:
	//	r = distance to atmosphere following view / distance to atmosphere following the horizon vector
	// (the horizon vector is the vector starting from the viewpoint and tangent to the ground)
	//
	// When the ray is intersecting the ground, they chose to encode the ratio:
	//	r = distance to ground following view / distance to the ground following the horizon vector
	//
	// First, we need to know if and where the view hits the ground.
	// We pose:
	//	P0 = (0, r)
	//	V = (sin(theta), cos(theta))
	//	P = P0 + d.V
	//
	// We solve: P² = Rg²
	//
	//	P0.P0 + 2*P0.V*d + V².d² = Rg²				(1)
	//	[r²-Rg²] + 2*[r*cos(theta)]*d + d² = 0
	//	Delta = r²*cos²(theta) - [r²-Rg²]
	//
	// We hit the ground if we look down (i.e. cos(theta) < 0) and if delta > 0
	//
	// ==== HITTING THE GROUND ====
	// We want to encode distance to ground / distance to horizon
	//
	// We hit the ground at distance d = -r*cos(theta) - sqrt(Delta)
	// The distance to the horizon is simply d_h = sqrt( r² - Rg² )
	//
	// So our V coordinate is V = d / d_h = (-r*cos(theta) - sqrt(r²*cos²(theta) - [r²-Rg²])) / sqrt( r² - Rg² )
	//
	//
	// ==== HITTING THE ATMOSPHERE ====
	// We want to encode distance to atmosphere / distance to atmosphere following horizon vector
	//
	// We rewrite equation (1) as before but with Rt instead of Rg
	//	P0.P0 + 2*P0.V*d + V².d² = Rt²				(2)
	//	[r²-Rt²] + 2*[r*cos(theta)]*d + d² = 0
	//	Delta = r²*cos²(theta) - [r²-Rt²]
	//
	// We hit the atmosphere at distance d = -r*cos(theta) + sqrt(Delta)
	//
	// Next, the distance to the atmosphere d_H following the horizon vector is the largest distance to the atmosphere we can hit
	//	from the current altitude and is obtained by replacing theta in equation (2) with theta_H, the angle at which
	//	we're tangent to the horizon from the given altitude.
	//
	// First we find that cos( theta_H ) = -sqrt( 1 - Rg²/r² )
	// Replacing in (2) gives:
	//	[r²-Rt²] + 2*[r*cos(theta_H)]*d_H + d_H² = 0
	//	Delta_H = r²*cos²(theta_H) - [r²-Rt²]
	//	d_H = -r*cos(theta_H) + sqrt( r²*cos²(theta_H) - [r²-Rt²] )
	//	d_H = r*sqrt( 1 - Rg²/r² ) + sqrt( r²*(1 - Rg²/r²) - [r²-Rt²] )
	//	d_H = sqrt( r² - Rg² ) + sqrt( Rt² - Rg² )
	//
	// And finally, our V coordinate is V = d / d_H = (-r*cos(theta) + sqrt( r²*cos²(theta) - [r²-Rt²] )) / (sqrt( r² - Rg² ) + sqrt( Rt² - Rg² ))
	//
	// As a final step, to account for the discontinuity when we hit the ground, the V's are
	//	reversed in a manner that transition from atmosphere to ground makes V -> 1
	//	and transition from ground to atmosphere makes V -> 0, the CLAMP address mode prevents
	//	any unwanted bilinear interpolation that would occur otherwise...
	//
	// And this explains all the mysterious bullshit terms and optimizations the authors wrote
	//	and that was so difficult to grasp looking solely at the code...
	//

	// Note that this code produces a warning about floating point precision because of the sqrt( H*H + delta )...
	float	r_cosTheta = r * _CosThetaView;
	float	Delta = r_cosTheta * r_cosTheta + GROUND_RADIUS_KM * GROUND_RADIUS_KM - r * r;

#if 1
	// This code is "optimized" below
	float	uCosThetaView = 0.0;
	if ( _CosThetaView < 0.0 && Delta > 0.0 )	// Hitting the ground
	{
		float	GroundHitDistanceKm = -r_cosTheta - sqrt( max( 0.0, Delta ) );
		float	HorizonHitDistanceKm = h;
		uCosThetaView = GroundHitDistanceKm / HorizonHitDistanceKm;												// That's our V coordinate. It equals 1 when we're about to stop hitting the ground (horizon hit) and Delta is becoming negative
		uCosThetaView = (0.5 * NORMALIZED_SIZE_V) - uCosThetaView * (0.5 - 1.0 / RESOLUTION_COS_THETA);			// This results in mapping to 0.5-€ when viewing straight down, and to 0 when reaching the horizon
	}
	else										// Hitting the atmosphere
	{
		Delta = r_cosTheta * r_cosTheta + ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - r * r;
		float	AtmosphereHitDistanceKm = -r_cosTheta + sqrt( max( 0.0, Delta ) );
		float	HorizonHitDistanceKm = h + H;
		uCosThetaView = AtmosphereHitDistanceKm / HorizonHitDistanceKm;											// That's our V coordinate. It equals 1 when we're about to start hitting the ground (horizon hit) and Delta is becoming positive
		uCosThetaView = (1.0 - 0.5 * NORMALIZED_SIZE_V) + uCosThetaView * (0.5 - 1.0 / RESOLUTION_COS_THETA);	// This results in mapping to 0.5+€ when viewing straight up, and to 1 when reaching the horizon
	}
#else
// 	//TODO: REWRITE!
// 	float4	cst = (rmu < 0.0 && delta > 0.0) ? float4( 1.0, 0.0, 0.0, 0.5 * NORMALIZED_SIZE_V ) : float4( -1.0, H * H, H, 1.0 - 0.5 * NORMALIZED_SIZE_V );
// 	float	uCosThetaView = cst.w + (rmu * cst.x + sqrt( delta + cst.y )) / (rho + cst.z) * (0.5 - 1.0 / RESOLUTION_COS_THETA);
#endif

#else
	float	uCosThetaView = 0.5 / RESOLUTION_COS_THETA + 0.5 * (_CosThetaView + 1.0) * NORMALIZED_SIZE_V;
#endif

#ifdef INSCATTER_NON_LINEAR_SUN
	// paper formula
	//float	uCosThetaSun = 0.5 / RESOLUTION_COS_THETA_SUN + max((1.0 - exp(-3.0 * _CosThetaSun - 0.6)) / (1.0 - exp(-3.6)), 0.0) * NORMALIZED_SIZE_U1;

	// better formula
	float	uCosThetaSun = 0.5 / RESOLUTION_COS_THETA_SUN + (atan( max( _CosThetaSun, -0.1975 ) * tan( 1.26 * 1.1 ) ) / 1.1 + (1.0 - 0.26)) * 0.5 * NORMALIZED_SIZE_U1;
#else
	float	uCosThetaSun = 0.5 / RESOLUTION_COS_THETA_SUN + max( 0.2 + _CosThetaSun, 0.0 ) / 1.2 * NORMALIZED_SIZE_U1;
#endif

	float	t = 0.5 * (_CosGamma + 1.0) * (RESOLUTION_COS_GAMMA - 1.0);
	float	uGamma = floor( t );
	t = t - uGamma;

//@@@###
//return _TexScattering.SampleLevel( LinearClamp, float3( uCosThetaSun / RESOLUTION_COS_GAMMA, uCosThetaView, uAltitude ), 0.0 );
//return _TexScattering.SampleLevel( LinearClamp, float3( 0.0, uCosThetaView, uAltitude ), 0.0 );


	float4	V0 = _TexScattering.SampleLevel( LinearClamp, float3( (uGamma + uCosThetaSun) / RESOLUTION_COS_GAMMA, uCosThetaView, uAltitude ), 0.0 );
	float4	V1 = _TexScattering.SampleLevel( LinearClamp, float3( (uGamma + uCosThetaSun + 1.0) / RESOLUTION_COS_GAMMA, uCosThetaView, uAltitude ), 0.0 );
	return lerp( V0, V1, t );
}

////////////////////////////////////////////////////////////////////////////////////////
// Actual Sky Rendering
//	_PositionKm, the view position (in kilometers)
//	_View, the normalized view direction
//	_Sun, the normalized direction pointing toward the Sun
//	_DistanceKm, an (optional) distance to the ground (in kilometers). If not provided then the ray is assumed to look at the sky
//	_GroundReflectance, an (optional) reflectance (in [0,1]) for the ground at the end of the ray
// returns the color of the sky for a unit Sun intensity.
// You need to multiply this by your Sun's intensity to get the actual value.
//
float3	ComputeSkyColor( float3 _PositionKm, float3 _View, float3 _Sun, float _DistanceKm=-1, float3 _GroundReflectance=0.0 )
{
	float3	StartPositionKm = _PositionKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
	float	StartRadiusKm = length( StartPositionKm );
	float3	StartNormal = StartPositionKm / StartRadiusKm;
	float	CosThetaView = dot( StartNormal, _View );

// 	// Check if we're outside the atmosphere
//	float	d = -StartRadiusKm * CosThetaView - sqrt( StartRadiusKm * StartRadiusKm * (CosThetaView * CosThetaView - 1.0) + ATMOSPHERE_RADIUS_KM*ATMOSPHERE_RADIUS_KM );
// 	if ( d > 0.0 )
// 	{	// if we're in space and ray intersects atmosphere, move to nearest intersection of ray with top atmosphere boundary
//		StartPositionKm += d * _View;
//		StartRadiusKm = ATMOSPHERE_RADIUS_KM-0.01;
//		StartNormal = StartPositionKm / StartRadiusKm;
//		_DistanceKm -= d;
//		CosThetaView = (StartRadiusKm * CosThetaView + d) / ATMOSPHERE_RADIUS_KM;
//	}
//	if ( StartRadiusKm > ATMOSPHERE_RADIUS_KM )
//		return 0.0;	// Lost in space...

	float	CosThetaSun = dot( StartNormal, _Sun );
	float	CosGamma = dot( _View, _Sun );
	float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;

	// Compute sky radiance
	float4	Lin = Sample4DScatteringTable( _TexScattering, StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Compute end point's radiance
	float3	L0 = 0.0;
	if ( _DistanceKm > 0.0 )
	{	// We're looking at the ground. Compute perceived reflected radiance...
		float3	EndPositionKm = _PositionKm + _DistanceKm * _View - EARTH_CENTER_KM;	// Ground position from origin (i.e. center of the Earth)
		float	EndRadiusKm = length( EndPositionKm );
		float	EndAltitudeKm = EndRadiusKm - GROUND_RADIUS_KM;
		float3	GroundNormal = EndPositionKm / EndRadiusKm;
		float	EndCosThetaView = dot( GroundNormal, _View );
		float	EndCosThetaSun = dot( GroundNormal, _Sun );

		float	CosThetaGround = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM / (EndRadiusKm*EndRadiusKm)) );
		float3	SunTransmittance = EndCosThetaSun > CosThetaGround ? GetTransmittance( EndAltitudeKm, EndCosThetaSun ) : 0.0;	// Here, we account for shadowing by the planet
		float3	DirectSunLight = saturate( EndCosThetaSun ) * SunTransmittance;													// Lighting by direct Sun light

		float3	GroundIrradiance = GetIrradiance( _TexIrradiance, EndAltitudeKm, EndCosThetaSun );								// Lighting by multiple-scattered light

		L0 = (_GroundReflectance * INVPI) * (DirectSunLight + GroundIrradiance);

		// Subtract end in-scattering if blocked by an obstacle other than ground (since ground has been accounted for in the pre-computation)
		if ( EndAltitudeKm > 0.01 )
		{
			float3	ViewTransmittance = GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );						
			float4	EndLin = Sample4DScatteringTable( _TexScattering, EndAltitudeKm, EndCosThetaView, EndCosThetaSun, CosGamma );
			Lin -= ViewTransmittance.xyzx * EndLin;
		}

		L0 *= GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );	// Attenuated through the atmosphere until end point
	}
	else
	{	// We're looking up. Check if we can see the Sun...
		L0 = smoothstep( 0.9997, 0.9999, CosGamma );					// 1 if we're looking directly at the Sun (warning: bad for the eyes!)
		L0 *= GetTransmittance( StartAltitudeKm, CosThetaView );	// Attenuated through the atmosphere
	}

	// Compute final radiance
	Lin = max( 0.0, Lin );

	return PhaseFunctionRayleigh( CosGamma ) * Lin.xyz + PhaseFunctionMie( CosGamma ) * GetMieFromRayleighAndMieRed( Lin ) + L0;
}

#endif	// _ATMOSPHERE_INC_
