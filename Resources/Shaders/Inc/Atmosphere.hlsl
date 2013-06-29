////////////////////////////////////////////////////////////////////////////////////////
// Atmosphere Helpers
// From http://www-ljk.imag.fr/Publications/Basilic/com.lmc.publi.PUBLI_Article@11e7cdda2f7_f64b69/article.pdf
// Much code stolen from Bruneton's sample
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _ATMOSPHERE_INC_
#define _ATMOSPHERE_INC_

static const float	ATMOSPHERE_THICKNESS_KM = 60.0;
static const float	GROUND_RADIUS_KM = 6360.0;
static const float	ATMOSPHERE_RADIUS_KM = GROUND_RADIUS_KM + ATMOSPHERE_THICKNESS_KM;

static const float3	EARTH_CENTER_KM = float3( 0.0, -GROUND_RADIUS_KM, 0.0 );			// Far below us!

static const float	TRANSMITTANCE_OPTICAL_DEPTH_FACTOR = 10.0;							// Optical depths are stored divided by this factor...

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
	float		_GodraysStrength;
	float		_AltitudeOffsetKm;

	float4		_FogParams;			// X=Scattering Coeff, Y=Extinction Coeff, Z=Reference Altitude (km), W=Anisotropy
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
float3	GetOpticalDepth( float _AltitudeKm, float _CosTheta )
{
	float	NormalizedAltitude = sqrt( saturate( _AltitudeKm * (1.0 / ATMOSPHERE_THICKNESS_KM) ) );

const float	TAN_MAX = 1.5;

#if 0
	float	RadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
	float	CosThetaMin = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (RadiusKm*RadiusKm) );
#else
	float	CosThetaMin = -0.15;
#endif
 	float	NormalizedCosTheta = atan( (_CosTheta - CosThetaMin) / (1.0 - CosThetaMin) * tan(TAN_MAX) ) / TAN_MAX;

	float2	UV = float2( NormalizedCosTheta, NormalizedAltitude );	// For CosTheta=0.01  => U=0.73294567479959475196454899060789
																	// For CosTheta=0.001 => U=0.7170674487513882415177428025293

	return TRANSMITTANCE_OPTICAL_DEPTH_FACTOR * _TexTransmittance.SampleLevel( LinearClamp, UV, 0.0 ).xyz;
}

float3	GetTransmittance( float _AltitudeKm, float _CosTheta )
{
	return exp( -GetOpticalDepth( _AltitudeKm, _CosTheta ) );	// We now store the optical depth instead of directly the transmittance
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

	return _CosTheta > 0.0	? exp( -max( 0.0, GetOpticalDepth( _AltitudeKm, _CosTheta ) - GetOpticalDepth( AltitudeKm2, CosTheta2 ) ) )
							: exp( -max( 0.0, GetOpticalDepth( AltitudeKm2, -CosTheta2 ) - GetOpticalDepth( _AltitudeKm, -_CosTheta ) ) );
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

// Gets the zenith/view angle (cos theta), zenith/Sun angle (cos theta Sun) and view/Sun angle (cos gamma) from a 2D parameter
#define	INSCATTER_NON_LINEAR_VIEW
#define	INSCATTER_NON_LINEAR_VIEW_POM	// Use my "formula" instead of theirs
#define	INSCATTER_NON_LINEAR_SUN

// Samples the scattering table from 4 parameters
float4	Sample4DScatteringTable( Texture3D _TexScattering, float _AltitudeKm, float _CosThetaView, float _CosThetaSun, float _CosGamma )
{
	float	r = GROUND_RADIUS_KM + _AltitudeKm;
	float	H = sqrt( ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - GROUND_RADIUS_KM * GROUND_RADIUS_KM );
	float	rho = sqrt( r * r - GROUND_RADIUS_KM * GROUND_RADIUS_KM );

	float	uAltitude = 0.5 / RESOLUTION_ALTITUDE + (rho / H) * NORMALIZED_SIZE_W;

#ifdef INSCATTER_NON_LINEAR_VIEW

#ifdef INSCATTER_NON_LINEAR_VIEW_POM

//  	float	uCosThetaView = 0.5 * (_CosThetaView < 0.0 ? 1.0 - sqrt( abs(_CosThetaView) ) : 1.0 + sqrt( saturate(_CosThetaView) ));
// 			uCosThetaView = 0.5 / RESOLUTION_COS_THETA + uCosThetaView * NORMALIZED_SIZE_V;

//###@@@
float	uCosThetaView = _CosThetaView < 0.0 ? 1.0 + 0.5 * _CosThetaView : 0.5 * _CosThetaView;

#else	// !POM?
// Note that this code produces a warning about floating point precision because of the sqrt( H*H + delta )...
	float	rmu = r * _CosThetaView;
	float	delta = rmu * rmu - r * r + GROUND_RADIUS_KM * GROUND_RADIUS_KM;

// This code is "optimized" below
// 	float	uCosThetaView = 0.0;
// 	if ( rmu < 0.0 && delta > 0.0 )
// 		uCosThetaView = (0.5 * NORMALIZED_SIZE_V) + (rmu + sqrt( max( 0.0, delta ) )) / (rho) * (0.5 - 1.0 / RESOLUTION_COS_THETA);
// 	else
//		uCosThetaView = (1.0 - 0.5 * NORMALIZED_SIZE_V) + (-rmu + sqrt( max( 0.0, H*H + delta ) )) / (H + rho) * (0.5 - 1.0 / RESOLUTION_COS_THETA);
//
	float4	cst = (rmu < 0.0 && delta > 0.0) ? float4( 1.0, 0.0, 0.0, 0.5 * NORMALIZED_SIZE_V ) : float4( -1.0, H * H, H, 1.0 - 0.5 * NORMALIZED_SIZE_V );
	float	uCosThetaView = cst.w + (rmu * cst.x + sqrt( delta + cst.y )) / (rho + cst.z) * (0.5 - 1.0 / RESOLUTION_COS_THETA);

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

	float4	V0 = _TexScattering.SampleLevel( LinearClamp, float3( (uGamma + uCosThetaSun) / RESOLUTION_COS_GAMMA, uCosThetaView, uAltitude ), 0.0 );

return V0;//@@@###

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

/* ARKANE ATMOSPHERE FOR COMPARISON

		static const float	ATMOSPHERE_THICKNESS_KM = 60.0;
		static const float	GROUND_RADIUS_KM = 6360.0;
		static const float	ATMOSPHERE_RADIUS_KM = GROUND_RADIUS_KM + ATMOSPHERE_THICKNESS_KM;

		static const float3	EARTH_CENTER_KM = float3( 0.0, 0.0, -GROUND_RADIUS_KM );			// Far below us!

		// Rayleigh Scattering
		static const float	HREF_RAYLEIGH = 8.0;
		static const float3	SIGMA_SCATTERING_RAYLEIGH = float3( 0.0058, 0.0135, 0.0331 );		// For lambdas (680,550,440) nm

		// Mie Scattering + Extinction
		static const float	HREF_MIE = 1.2;
		static const float	SIGMA_SCATTERING_MIE = 0.004;
		static const float	SIGMA_EXTINCTION_MIE = SIGMA_SCATTERING_MIE / 0.9;
		static const float	MIE_ANISOTROPY = 0.76;

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



		////////////////////////////////////////////////////////////////////////////////////////
		// Phase functions
		float	PhaseFunctionRayleigh( float _CosPhaseAngle )
		{
			return (3.0 / (16.0 * PI)) * (1.0 + _CosPhaseAngle * _CosPhaseAngle);
		}

		float	PhaseFunctionMie( float _CosPhaseAngle, float g )
		{
			return 1.5 * INVFOURPI * (1.0 - g*g) * pow( max( 0.0, 1.0 + (g*g) - 2.0*g*_CosPhaseAngle ), -1.5 ) * (1.0 + _CosPhaseAngle * _CosPhaseAngle) / (2.0 + g*g);
		}

		// An interesting variation and more general phase function taking 2 parameters (from: http://arxiv.org/pdf/astro-ph/0304060.pdf)
		//	g is the same as usual, i.e. Henyey-Greenstein scattering anisotropy
		//	a is new and interestingly gives:
		//		a=0, g!=0 reverts back to Henyey-Greenstein
		//		a=0, g=0 reverts back to Rayleigh scattering
		//		a=1, g!=0 reverts back to Cornette-Shanks
		//
		float	PhaseFunctionDraine( float _CosPhaseAngle, float g, float a )
		{
			return (1.0 - g*g) * (1.0 + a * _CosPhaseAngle*_CosPhaseAngle) / (
					FOURPI * (1.0 + a * (1.0 + 2.0*g*g)/3.0) * pow( max( 0.0, 1.0 + (g*g) - 2.0*g*_CosPhaseAngle ), 1.5 )
				);
		}


`ifdef ATMOSPHERE_ENABLED

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

		// Computes shadowing by the Earth
		// Return 1 if Sun is visible from specified altitude, 0 otherwise
		float	ComputeEarthShadowing( float _AltitudeKm, float _CosSun )
		{
			float	RadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
			float	MinSin = GROUND_RADIUS_KM / RadiusKm;
			float	MinCos = sqrt( 1.0 - MinSin*MinSin );
			return smoothstep( MinCos-2e-2, MinCos+2e-2, _CosSun );
		}

		////////////////////////////////////////////////////////////////////////////////////////
		// Phase functions

		// Gets the full Mie RGB components from full Rayleigh RGB and only Mie Red
		// This is possible because both values are proportionally related (cf. Bruneton paper, chapter 4 on Angular Precision)
		// _RayleighMieRed : XYZ = Crayleigh, W=Cmie.red
		float3	GetMieFromRayleighAndMieRed( float4 _RayleighMieRed )
		{
			_RayleighMieRed.w = max( 1e-3, _RayleighMieRed.w );
			return _RayleighMieRed.xyz * (_RayleighMieRed.w * (SIGMA_SCATTERING_RAYLEIGH.x / SIGMA_SCATTERING_RAYLEIGH) / max( _RayleighMieRed.x, 1e-3 ));
		}

		////////////////////////////////////////////////////////////////////////////////////////
		////////////////////////////////////////////////////////////////////////////////////////
		// Tables access
		float3	GetOpticalDepth( float _AltitudeKm, float _CosTheta )
		{
			float	NormalizedAltitude = sqrt( _AltitudeKm * (1.0 / ATMOSPHERE_THICKNESS_KM) );
		//	float	NormalizedCosTheta = (_CosTheta + 0.15) / (1.0 + 0.15);				// Linear
		//	float	NormalizedCosTheta = sqrt( (_CosTheta + 0.15) / (1.0 + 0.15) );		// Quadratic
			float	NormalizedCosTheta = atan( (_CosTheta + 0.15) / (1.0 + 0.15) * tan(1.5) ) / 1.5;
			float2	UV = float2( NormalizedCosTheta, NormalizedAltitude );

			return $(atm/sky/TexTransmittance).SampleLevel( $linearClamp, UV, 0.0 ).xyz;
		}

		float3	GetTransmittance( float _AltitudeKm, float _CosTheta )
		{
			return exp( -GetOpticalDepth( _AltitudeKm, _CosTheta ) );	// We now store the optical depth instead of directly the transmittance
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

			float3	OD0 = GetOpticalDepth( _AltitudeKm, abs(_CosTheta) );
			float3	OD1 = GetOpticalDepth( _AltitudeKm, abs(CosTheta2) );

			return _CosTheta > 0.0	? exp( min( -0.0001, OD1 - OD0 ) ) : exp( min( -0.0001, OD0 - OD1 ) );

// 			return _CosTheta > 0.0	? exp( min( -0.0001, GetOpticalDepth( AltitudeKm2, CosTheta2 ) - GetOpticalDepth( _AltitudeKm, _CosTheta ) ) )
// 									: exp( min( -0.0001, GetOpticalDepth( _AltitudeKm, -_CosTheta ) - GetOpticalDepth( AltitudeKm2, -CosTheta2 ) ) );
		}
		
		float3	GetIrradiance( Texture2D _TexIrradiance, float _AltitudeKm, float _CosThetaSun )
		{
			float	NormalizedAltitude = _AltitudeKm / ATMOSPHERE_THICKNESS_KM;
			float	NormalizedCosThetaSun = (_CosThetaSun + 0.2) / (1.0 + 0.2);
			float2	UV = float2( NormalizedCosThetaSun, NormalizedAltitude );

			return _TexIrradiance.SampleLevel( $linearClamp, UV, 0.0 ).xyz;
		}

		float3	GetIrradiance( float _AltitudeKm, float _CosThetaSun )
		{
			return GetIrradiance( $(atm/sky/TexIrradiance), _AltitudeKm, _CosThetaSun );
		}

		// Gets the zenith/view angle (cos theta), zenith/Sun angle (cos theta Sun) and view/Sun angle (cos gamma) from a 2D parameter
		#define	INSCATTER_NON_LINEAR_VIEW
		#define	INSCATTER_NON_LINEAR_VIEW_POM	// Use my "formula" instead of theirs
		#define	INSCATTER_NON_LINEAR_SUN
		void GetAnglesFrom4D( float2 _UV, float3 _dUV, float _AltitudeKm, float4 dhdH, out float _CosThetaView, out float _CosThetaSun, out float _CosGamma )
		{
			_UV -= 0.5 * _dUV.xy;	// Remove the half pixel offset

#ifdef INSCATTER_NON_LINEAR_VIEW

#ifdef INSCATTER_NON_LINEAR_VIEW_POM

			_CosThetaView = abs( 2.0 * _UV.y - 1.0 );
			_CosThetaView *= (_UV.y < 0.5 ? -1.0 : +1.0) * _CosThetaView;	// Squared progression for more precision near horizon

#else

			float r = GROUND_RADIUS_KM + _AltitudeKm;
			if ( _UV.y < 0.5 )
			{	// Viewing toward the sky
				float	d = 1.0 - 2.0 * _UV.y;
						d = min( max( dhdH.z, d * dhdH.w ), dhdH.w * 0.999 );

				_CosThetaView = (GROUND_RADIUS_KM * GROUND_RADIUS_KM - r * r - d * d) / (2.0 * r * d);
				_CosThetaView = min( _CosThetaView, -sqrt( 1.0 - (GROUND_RADIUS_KM / r) * (GROUND_RADIUS_KM / r) ) - 0.001 );
			}
			else
			{	// Viewing toward the ground
				float	d = 2.0 * (_UV.y - 0.5);
						d = min( max( dhdH.x, d * dhdH.y ), dhdH.y * 0.999 );

				_CosThetaView = (ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - r * r - d * d) / (2.0 * r * d);
			}
#endif	// POM?

#else
			_CosThetaView = lerp( -1.0, 1.0, _UV.y );
#endif

#ifdef INSCATTER_NON_LINEAR_SUN
			_CosThetaSun = fmod( _UV.x, MODULO_U ) / MODULO_U;

			// paper formula
			//_CosThetaSun = -(0.6 + log(1.0 - _CosThetaSun * (1.0 -  exp(-3.6)))) / 3.0;

			// better formula
			_CosThetaSun = tan( (2.0 * _CosThetaSun - 1.0 + 0.26) * 1.1 ) * 0.18692904279186995490534690217449;	// / tan( 1.26 * 1.1 );
#else
			_CosThetaSun = lerp( -0.2, 1.0, fmod( _UV.x, MODULO_U ) / MODULO_U );
#endif

			_CosGamma = lerp( -1.0, 1.0, floor( _UV.x / MODULO_U ) / (RESOLUTION_COS_THETA_SUN-1) );
		}

		// Samples the scattering table from 4 parameters
		float4	Sample4DScatteringTable( Texture3D _TexScattering, float _AltitudeKm, float _CosThetaView, float _CosThetaSun, float _CosGamma )
		{
			float	r = GROUND_RADIUS_KM + _AltitudeKm;
			float	H = sqrt( ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - GROUND_RADIUS_KM * GROUND_RADIUS_KM );
			float	rho = sqrt( r * r - GROUND_RADIUS_KM * GROUND_RADIUS_KM );

			float	uAltitude = 0.5 / RESOLUTION_ALTITUDE + (rho / H) * NORMALIZED_SIZE_W;

#ifdef INSCATTER_NON_LINEAR_VIEW

#ifdef INSCATTER_NON_LINEAR_VIEW_POM

 			float	uCosThetaView = 0.5 * (_CosThetaView < 0.0 ? 1.0 - sqrt( abs(_CosThetaView) ) : 1.0 + sqrt( saturate(_CosThetaView) ));
					uCosThetaView = 0.5 / RESOLUTION_COS_THETA + uCosThetaView * NORMALIZED_SIZE_V;

//			float	uCosThetaView = 0.5 / RESOLUTION_COS_THETA + 0.5 * (_CosThetaView + 1.0) * NORMALIZED_SIZE_V;

#else		// POM?
			// Note that this code produces a warning about floating point precision because of the sqrt( H*H + delta )...
			float	rmu = r * _CosThetaView;
			float	delta = rmu * rmu - r * r + GROUND_RADIUS_KM * GROUND_RADIUS_KM;

			// This code is "optimized" below
			// 	float	uCosThetaView = 0.0;
			// 	if ( rmu < 0.0 && delta > 0.0 )
			// 		uCosThetaView = (0.5 * NORMALIZED_SIZE_V) + (rmu + sqrt( max( 0.0, delta ) )) / (rho) * (0.5 - 1.0 / RESOLUTION_COS_THETA);
			// 	else
			//		uCosThetaView = (1.0 - 0.5 * NORMALIZED_SIZE_V) + (-rmu + sqrt( max( 0.0, H*H + delta ) )) / (H + rho) * (0.5 - 1.0 / RESOLUTION_COS_THETA);
			//
			float4	cst = (rmu < 0.0 && delta > 0.0) ? float4( 1.0, 0.0, 0.0, 0.5 * NORMALIZED_SIZE_V ) : float4( -1.0, H * H, H, 1.0 - 0.5 * NORMALIZED_SIZE_V );
			float	uCosThetaView = cst.w + (rmu * cst.x + sqrt( delta + cst.y )) / (rho + cst.z) * (0.5 - 1.0 / RESOLUTION_COS_THETA);

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

			float4	V0 = _TexScattering.SampleLevel( $linearClamp, float3( (uGamma + uCosThetaSun) / RESOLUTION_COS_GAMMA, uCosThetaView, uAltitude ), 0.0 );
			float4	V1 = _TexScattering.SampleLevel( $linearClamp, float3( (uGamma + uCosThetaSun + 1.0) / RESOLUTION_COS_GAMMA, uCosThetaView, uAltitude ), 0.0 );
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
		float3	ComputeSkyColor( float3 _PositionKm, float3 _View, float3 _Sun, float _SunIntensity, float _DistanceKm=-1, float3 _GroundReflectance=0.0 )
		{
			float3	StartPositionKm = _PositionKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
			float	StartRadiusKm = length( StartPositionKm );
			float3	StartNormal = StartPositionKm / StartRadiusKm;
			float	CosThetaView = dot( StartNormal, _View );

// 			// Check if we're outside the atmosphere
//			float	d = -StartRadiusKm * CosThetaView - sqrt( StartRadiusKm * StartRadiusKm * (CosThetaView * CosThetaView - 1.0) + ATMOSPHERE_RADIUS_KM*ATMOSPHERE_RADIUS_KM );
// 			if ( d > 0.0 )
// 			{	// if we're in space and ray intersects atmosphere, move to nearest intersection of ray with top atmosphere boundary
//				StartPositionKm += d * _View;
//				StartRadiusKm = ATMOSPHERE_RADIUS_KM-0.01;
//				StartNormal = StartPositionKm / StartRadiusKm;
//				_DistanceKm -= d;
//				CosThetaView = (StartRadiusKm * CosThetaView + d) / ATMOSPHERE_RADIUS_KM;
//			}
//			if ( StartRadiusKm > ATMOSPHERE_RADIUS_KM )
//				return 0.0;	// Lost in space...

			float	CosThetaSun = dot( StartNormal, _Sun );
			float	CosGamma = dot( _View, _Sun );
			float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;

			// Compute sky radiance
			float4	Lin = _SunIntensity * Sample4DScatteringTable( $(atm/sky/TexScattering), StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );

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

				float3	GroundIrradiance = _SunIntensity * GetIrradiance( $(atm/sky/TexIrradiance), EndAltitudeKm, EndCosThetaSun );	// Lighting by multiple-scattered light

				L0 = (_GroundReflectance * INVPI) * (DirectSunLight + GroundIrradiance);

				// Subtract end in-scattering if blocked by an obstacle other than ground (since ground has been accounted for in the pre-computation)
		 		if ( EndAltitudeKm > 0.01 )
		 		{
		 			float3	ViewTransmittance = GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );						
		 			float4	EndLin = _SunIntensity * Sample4DScatteringTable( $(atm/sky/TexScattering), EndAltitudeKm, EndCosThetaView, EndCosThetaSun, CosGamma );
		 			Lin -= ViewTransmittance.xyzx * EndLin;
		 		}

				L0 *= GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );	// Attenuated through the atmosphere until end point
			}
			else
			{	// We're looking up. Check if we can see the Sun...
				L0 = smoothstep( 0.999, 0.9995, CosGamma );					// 1 if we're looking directly at the Sun (warning: bad for the eyes!)
				L0 *= GetTransmittance( StartAltitudeKm, CosThetaView );	// Attenuated through the atmosphere
			}

			// Compute final radiance
			Lin = max( 0.0, Lin );

			return PhaseFunctionRayleigh( CosGamma ) * Lin.xyz + PhaseFunctionMie( CosGamma, MIE_ANISOTROPY ) * GetMieFromRayleighAndMieRed( Lin ) + L0;
		}

		// Computes the final screen color with sky
		//  _PositionWorldKm, position in kilometers
		//  _View, view in world space
		//  _DistanceKm, distance to the obstacle (i.e. ZBuffer or cloud) in kilometers
		//  _Sun, direction of the Sun
		//  _CloudScatteringExtinction, scattering & extinction provided by the clouds
		//  _GroundBlocking, =0 if hitting opaque ZBuffer. 1 otherwise.
		//  _StepOffset, offset in [0,1] to add noise to godrays computation
		//
		void	ComputeSkyExtinctionScattering( float3 _PositionWorldKm, float3 _View, float _DistanceKm, float3 _Sun, float4 _CloudScatteringExtinction, float _GroundBlocking, float _StepOffset, out float3 _Scattering, out float3 _Extinction )
		{
`define UNPACK_EARLY

			////////////////////////////////////////////////////////////
			// Compute sky radiance arriving at camera, not accounting for clouds
			float3	StartPositionKm = _PositionWorldKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
			float	StartRadiusKm = length( StartPositionKm );
			float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;
			float3	StartNormal = StartPositionKm / StartRadiusKm;
			float	CosThetaView = dot( StartNormal, _View );
			float	CosThetaSun = dot( StartNormal, _Sun );

			float	CosGamma = dot( _View, _Sun );

			float4	Lin = Sample4DScatteringTable( $(atm/sky/TexScattering), StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );
`ifdef UNPACK_EARLY
			float3	Lin_Rayleigh = Lin.xyz;
			float3	Lin_Mie = GetMieFromRayleighAndMieRed( Lin );
`endif

			////////////////////////////////////////////////////////////
			// Account for obstacle hit & cloud shadowing
			float3	HitPositionKm = _PositionWorldKm + _DistanceKm * _View - EARTH_CENTER_KM;
			float	HitRadiusKm = length( HitPositionKm );
			float	HitAltitudeKm = HitRadiusKm - GROUND_RADIUS_KM;
			float3	HitNormal = HitPositionKm / HitRadiusKm;
			float	HitCosThetaView = dot( HitNormal, _View );
			float	HitCosThetaSun = dot( HitNormal, _Sun );

			// Compute sky radiance arriving at cloud/ground (i.e. above and inside cloud)
			float4	Lin_hit2atmosphere = Sample4DScatteringTable( $(atm/sky/TexScattering), HitAltitudeKm, HitCosThetaView, HitCosThetaSun, CosGamma );
`ifdef UNPACK_EARLY
			float3	Lin_hit2atmosphere_Rayleigh = Lin_hit2atmosphere.xyz;
			float3	Lin_hit2atmosphere_Mie = GetMieFromRayleighAndMieRed( Lin_hit2atmosphere );
`endif

			// Compute sky radiance between camera and hit
			float3	Transmittance = GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );

`ifdef UNPACK_EARLY
			float3	Lin_camera2hit_Rayleigh = max( 0.0, Lin_Rayleigh - Transmittance * Lin_hit2atmosphere_Rayleigh );
			float3	Lin_camera2hit_Mie = max( 0.0, Lin_Mie - Transmittance * Lin_hit2atmosphere_Mie );
`else
			float4	Lin_camera2hit = max( 0.0, Lin - Transmittance.xyzx * Lin_hit2atmosphere );
`endif

// 			// Attenuate in-scattered light between camera and hit due to shadowing by the cloud
// 			float	Shadowing = ComputeCloudShadowing( _PositionWorld, _View, _DistanceKm / WORLD2KM, _StepOffset );
// 
//			float	GodraysStrength = saturate( lerp( 1.0 - $(atm/sky/GodraysStrength).x, 1.0, Shadowing ) );
//			Lin_camera2hit *= GodraysStrength;
//// 			Lin_camera2hit_Rayleigh *= GodraysStrength;
//// 			Lin_camera2hit_Mie *= GodraysStrength;

			////////////////////////////////////////////////////////////
			// Rebuild final camera2atmosphere scattering, accounting for cloud extinction
			float	CloudExtinction = _GroundBlocking * _CloudScatteringExtinction.w;	// Completely mask remaining segment if we hit the ground

`ifdef UNPACK_EARLY
			Lin_Rayleigh = Lin_camera2hit_Rayleigh + CloudExtinction * Transmittance * Lin_hit2atmosphere_Rayleigh;
			Lin_Mie = Lin_camera2hit_Mie + CloudExtinction * Transmittance * Lin_hit2atmosphere_Mie;
`else
			Lin = Lin_camera2hit + CloudExtinction * Transmittance.xyzx * Lin_hit2atmosphere;
`endif

			// Finalize extinction & scattering
`ifndef UNPACK_EARLY
			float3	Lin_Rayleigh = Lin.xyz;
			float3	Lin_Mie = GetMieFromRayleighAndMieRed( Lin );
`endif

			_Extinction = Transmittance * _CloudScatteringExtinction.w;	// Combine with cloud
			_Scattering = $(atm/sky/SunIntensity).x * (PhaseFunctionRayleigh( CosGamma ) * Lin_Rayleigh + PhaseFunctionMie( CosGamma, MIE_ANISOTROPY ) * Lin_Mie) + _CloudScatteringExtinction.xyz;
		}

		////////////////////////////////////////////////////////////
		// Sky reflection
		// The sky env map is based on the paraboloïd env map algorithm that maps a paraboloïdal sheet:
		//	P(x,y) = ( x, y, f(x, y) )  for x² + y² <= 1 (i.e. inside the unit circle)
		//
		// with:
		//	f(x,y) = 0.5 * (1 - x² - y²)
		//
		// Each (x,y) on the paraboloïd will map to a specific normal N:
		//	N(x,y) = (x,y,1)
		//
		// The interesting fact with a paraboloïd is that rays from the entire hemisphere all get reflected in a beam of parallel rays.
		// So, when computing the paraboloïdal reflection map, we simply choose a unit vector V = (0,0,1)
		//	pointing toward the paraboloïd, we compute the normal at position (x,y) of interest then
		//	we find the reflected direction in which to sample the environment:
		//		R = -V + 2.dot( V, N' ).N'   (where N'=N/||N|| is the normalized normal)
		//		R = (0,0,-1) + 2.N'z.N'
		//
		// Similarly, when we need to sample the paraboloïd envmap in a given direction R, we can write:
		//		Nsum = R + V
		// Since N=(x,y,1) and Nsum = (Rx,Ry,Rz+1), to obtain N from Nsum we simply divide Nsum by its Z component:
		//		N=Nsum/Nsum.z = (Rx/(1+Rz), Ry/(1+Rz), 1 )
		//
		// This implies:
		//		x = Rx/(1+Rz)
		//		y = Ry/(1+Rz)
		//

		// Samples the sky env map in the specified direction (world space)
		float3	GetSkyReflection( float3 _Direction )
		{
			float2	UV = _Direction.xy / (1.0 + saturate(_Direction.z));
					UV = 0.5 * (1.0 + UV);
					UV *= 0.995;	// Scale a bit so we never quite get to the horizon...
			return $(atm/sky/TexEnvMap).SampleLevel( $linearClamp, UV, 0.0 ).xyz;
		}

		float	FresnelSchlick( float _R0, float _CosTheta )
		{
			float	t = 1.0 - saturate( _CosTheta );
			float	t2 = t * t;
			float	t4 = t2 * t2;
			return lerp( _R0, 1.0, t4 * t );
		}

*/

