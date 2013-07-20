//////////////////////////////////////////////////////////////////////////
// This shader displays the actual volume
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"
#include "Inc/Atmosphere.hlsl"

static const float	MIN_STEPS_COUNT = 8.0;
static const float	MAX_STEPS_COUNT = 64.0;

static const float	GODRAYS_STEPS_COUNT = 32.0;

cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
	bool		_bSampleTerrainShadow;
};

Texture2D		_TexVolumeDepth				: register(t10);
Texture2D		_TexSceneDepth				: register(t11);
Texture2D		_TexDownsampledSceneDepth	: register(t12);
Texture2D		_TexVolumeDepthPass			: register(t13);

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_OUT
{
	float3	Scattering	: SV_TARGET0;
	float3	Extinction	: SV_TARGET1;
};



// Read Z from the ZBuffer
float	ReadDepth( float2 _UV )
{
	float	Zproj = _TexSceneDepth.SampleLevel( LinearClamp, _UV, 0.0 ).x;

	float	Q = _CameraData.w / (_CameraData.w - _CameraData.z);	// Zf / (Zf-Zn)
	return (Q * _CameraData.z) / (Q - Zproj);
}

#define COMPLEX_SHADOWING
#ifdef COMPLEX_SHADOWING
// Complex version returning single scattered light
void	ComputeCloudShadowing( float3 _PositionWorld, float3 _View, float3 _Sun, float _Distance, out float3 _ShadowingRayleigh, out float3 _ShadowingMie, float _StepOffset=0.5, uniform uint _StepsCount=GODRAYS_STEPS_COUNT )
{
//	uint	StepsCount = ceil( lerp( 16.0, float(_StepsCount), saturate( _Distance / 150.0 ) ) );
//	uint	StepsCount = ceil( 2.0 * _StepOffset + lerp( 16.0, float(_StepsCount), saturate( _Distance / 150.0 ) ) );	// Fantastic noise hides banding!
//	uint	StepsCount = ceil( 2.0 * _StepOffset + lerp( 16.0, float(_StepsCount), saturate( _Distance / 50.0 ) ) );	// Fantastic noise hides banding!
	uint	StepsCount = ceil( lerp( 16.0, float(_StepsCount), saturate( _Distance / 50.0 ) ) );
//	uint	StepsCount = _StepsCount;

	float4	Position = float4( _PositionWorld, 0.0 );
	float4	Step = (_Distance / StepsCount) * float4( _View, 1.0 );
	Position += _StepOffset * Step;

	float3	Rayleigh = 0.0;
	float3	Mie = 0.0;

	_ShadowingRayleigh = 0.0;
	_ShadowingMie = 0.0;
	for ( uint StepIndex=0; StepIndex < StepsCount; StepIndex++ )
	{
		// Compute angles at current position
		float3	EarthPositionKm = WORLD2KM * Position.xyz - EARTH_CENTER_KM;
		float	RadiusKm = length( EarthPositionKm );
		float	AltitudeKm = RadiusKm - GROUND_RADIUS_KM;
		float3	Normal = EarthPositionKm / RadiusKm;
		float	CosThetaView = dot( Normal, _View );
		float	CosThetaSun = dot( Normal, _Sun );

		// Retrieve single-scattered sunlight reaching the camera
		float	StartAltitudeKm = WORLD2KM * _PositionWorld.y;
		float3	ViewTransmittance_Source2Point = GetTransmittance( StartAltitudeKm, CosThetaView, Position.w );	// Transmittance from view point to integration point at distance
		float3	SunTransmittance_Atmosphere2Point = GetTransmittance( AltitudeKm, CosThetaSun );					// Transmittance from top of atmosphere to integration point at distance (Sun light attenuation)
		float3	Transmittance_Sun2View = SunTransmittance_Atmosphere2Point * ViewTransmittance_Source2Point;

		float3	PreviousRayleigh = Rayleigh;
		float3	PreviousMie = Mie;

		Rayleigh = exp( -AltitudeKm / _AirParams.y ) * Transmittance_Sun2View;
		Mie = exp( -AltitudeKm / _FogParams.z ) * Transmittance_Sun2View;

		// Compute cloud transmittance
		float	CloudTransmittance = GetFastCloudTransmittance( Position.xyz );
		if ( _bSampleTerrainShadow )
			CloudTransmittance *= GetTerrainShadow( Position.xyz );	// Use cloud transmittance + terrain shadow

		CloudTransmittance -= 1.0;	// A transmittance of 1 (transparent cloud) gives 0 removed light, a transmittance of 1 gives a negative scattered light

		Rayleigh *= CloudTransmittance;
		Mie *= CloudTransmittance;

 		// Compute removed single-scattered Sun light due to shadowing
		_ShadowingRayleigh += 0.5 * (PreviousRayleigh + Rayleigh);
		_ShadowingMie += 0.5 * (PreviousMie + Mie);

		Position += Step;
	}

	float	StepSizeKm = WORLD2KM * Step.w;
	_ShadowingRayleigh *= _AirParams.x * SIGMA_SCATTERING_RAYLEIGH * StepSizeKm;
	_ShadowingMie *= _FogParams.x * StepSizeKm;
}

#else
// Simple version retunring a shadowing factor in [0,1]
float	ComputeCloudShadowing( float3 _PositionWorld, float3 _View, float _Distance, float _StepOffset=0.5, uniform uint _StepsCount=GODRAYS_STEPS_COUNT )
{
//	uint	StepsCount = ceil( lerp( 16.0, float(_StepsCount), saturate( _Distance / 150.0 ) ) );
//	uint	StepsCount = ceil( 2.0 * _StepOffset + lerp( 16.0, float(_StepsCount), saturate( _Distance / 150.0 ) ) );	// Fantastic noise hides banding!
//	uint	StepsCount = ceil( 2.0 * _StepOffset + lerp( 16.0, float(_StepsCount), saturate( _Distance / 50.0 ) ) );	// Fantastic noise hides banding!
	uint	StepsCount = ceil( lerp( 16.0, float(_StepsCount), saturate( _Distance / 50.0 ) ) );	// Fantastic noise hides banding!
//	uint	StepsCount = _StepsCount;

#if 1	// Linear steps
	float3	Step = (_Distance / StepsCount) * _View;
	_PositionWorld += _StepOffset * Step;

	float	SumIncomingLight = 0.0;
	for ( uint StepIndex=0; StepIndex < StepsCount; StepIndex++ )
	{
		if ( _bSampleTerrainShadow )
			SumIncomingLight += GetFastCloudTransmittance( _PositionWorld ) * GetTerrainShadow( _PositionWorld );	// Use cloud transmittance + terrain shadow
		else
			SumIncomingLight += GetFastCloudTransmittance( _PositionWorld );	// Use only cloud transmittance

		_PositionWorld += Step;
	}
#endif

	return saturate( SumIncomingLight / _StepsCount );
}
#endif

#if 0
void	ComputeFinalColor( float3 _PositionWorld, float3 _View, float _DistanceKm, float3 _Sun, float4 _CloudScatteringExtinction, float _GroundBlocking, float _StepOffset, out float3 _Scattering, out float3 _Extinction )
{
	float3	PositionKm = WORLD2KM * _PositionWorld;

PositionKm.y -= _AltitudeOffsetKm;

	////////////////////////////////////////////////////////////
	// Compute sky radiance arriving at camera, not accounting for clouds
	float3	StartPositionKm = PositionKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
	float	StartRadiusKm = length( StartPositionKm );
	float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;
	float3	StartNormal = StartPositionKm / StartRadiusKm;
	float	CosThetaView = dot( StartNormal, _View );
	float	CosThetaSun = dot( StartNormal, _Sun );

	float	CosGamma = dot( _View, _Sun );

	float4	Lin = Sample4DScatteringTable( _TexScattering, StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );
	float3	Lin_Rayleigh = Lin.xyz;
	float3	Lin_Mie = GetMieFromRayleighAndMieRed( Lin );

	////////////////////////////////////////////////////////////
	// Account for cloud shadowing
	float3	HitPositionKm = PositionKm + _DistanceKm * _View - EARTH_CENTER_KM;
	float	HitRadiusKm = length( HitPositionKm );
	float	HitAltitudeKm = HitRadiusKm - GROUND_RADIUS_KM;
	float3	HitNormal = HitPositionKm / HitRadiusKm;
	float	HitCosThetaView = dot( HitNormal, _View );
	float	HitCosThetaSun = dot( HitNormal, _Sun );

	// Compute sky radiance arriving at cloud/ground (i.e. above and inside cloud)
	float4	Lin_hit2atmosphere = Sample4DScatteringTable( _TexScattering, HitAltitudeKm, HitCosThetaView, HitCosThetaSun, CosGamma );
	float3	Lin_hit2atmosphere_Rayleigh = Lin_hit2atmosphere.xyz;
	float3	Lin_hit2atmosphere_Mie = GetMieFromRayleighAndMieRed( Lin_hit2atmosphere );

	// Compute sky radiance between camera and hit
	float3	Transmittance = GetTransmittance( StartAltitudeKm, CosThetaView, _DistanceKm );
	float3	Lin_camera2hit_Rayleigh = max( 0.0, Lin_Rayleigh - Transmittance * Lin_hit2atmosphere_Rayleigh );
	float3	Lin_camera2hit_Mie = max( 0.0, Lin_Mie - Transmittance * Lin_hit2atmosphere_Mie );

	// Attenuate in-scattered light between camera and hit due to shadowing by the cloud
#ifdef COMPLEX_SHADOWING
	float3	ShadowingRayleigh;
	float3	ShadowingMie;
	ComputeCloudShadowing( _PositionWorld, _View, _Sun, _DistanceKm / WORLD2KM, ShadowingRayleigh, ShadowingMie, _StepOffset );

// 	float	GodraysStrength = saturate( lerp( 1.0 - _GodraysStrength, 1.0, Shadowing ) );
	Lin_camera2hit_Rayleigh += ShadowingRayleigh;
	Lin_camera2hit_Mie += ShadowingMie;
#else
	float	Shadowing = ComputeCloudShadowing( _PositionWorld, _View, _DistanceKm / WORLD2KM, _StepOffset );

 	float	GodraysStrength = saturate( lerp( 1.0 - _GodraysStrength.x, 1.0, Shadowing ) );
	Lin_camera2hit_Rayleigh *= GodraysStrength;
	Lin_camera2hit_Mie *= GodraysStrength;
#endif

	////////////////////////////////////////////////////////////
	// Rebuild final camera2atmosphere scattering, accounting for cloud extinction
	float	CloudExtinction = _GroundBlocking * _CloudScatteringExtinction.w;	// Completely mask remaining segment if we hit the ground

	Lin_Rayleigh = Lin_camera2hit_Rayleigh + CloudExtinction * Transmittance * Lin_hit2atmosphere_Rayleigh;
	Lin_Mie = Lin_camera2hit_Mie + CloudExtinction * Transmittance * Lin_hit2atmosphere_Mie;

	// Finalize extinction & scattering
	_Extinction = Transmittance * _CloudScatteringExtinction.w;	// Combine with cloud
	_Scattering = SUN_INTENSITY * (PhaseFunctionRayleigh( CosGamma ) * Lin_Rayleigh + PhaseFunctionMie( CosGamma ) * Lin_Mie) + _CloudScatteringExtinction.xyz;
}

#else

// New version with horizon line interpolation
void	ComputeFinalColor( float3 _PositionWorldKm, float3 _View, float _DistanceKm, float3 _Sun, float4 _CloudScatteringExtinction, float _GroundBlocking, float _StepOffset, out float3 _Scattering, out float3 _Extinction )
{
	_Scattering = 0.0;
	_Extinction = 1.0;

	float3	x = _PositionWorldKm - EARTH_CENTER_KM;
	float	r = length(x);
	float	CosThetaView = dot( x, _View ) / r;
	float	d = -r * CosThetaView - sqrt( r * r * (CosThetaView * CosThetaView - 1.0) + ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM );
	if ( d > 0.0 )
	{	// If x in space and ray intersects atmosphere, move x to nearest intersection of ray with top atmosphere boundary
		x += d * _View;
		_DistanceKm -= d;
		CosThetaView = (r * CosThetaView + d) / ATMOSPHERE_RADIUS_KM;
		r = ATMOSPHERE_RADIUS_KM;
	}

	if ( r > ATMOSPHERE_RADIUS_KM )
		return;	// Lost in space...

	// If ray intersects atmosphere
	float	CosGamma = dot( _View, _Sun );
	float	CosThetaSun = dot( x, _Sun ) / r;
	float4	Scattering_cam2atm = Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma );

// _Scattering = CosGamma;
// _Extinction = 0;
// return;

	if ( _DistanceKm > 0.0 )
	{	// Looking at the ground
// 		if ( _GroundBlocking < 0.5 )
// 			CosThetaView = min( -0.01, CosThetaView );	// Force view downward to tap into the "ground part" of the 4D table
		CosThetaView = min( 2.0*_GroundBlocking-0.01, CosThetaView );	// Force view downward to tap into the "ground part" of the 4D table (same as lines abobe without the condition)

		// Compute angles at hit distance
		float3	x0 = x + _DistanceKm * _View;
		float	r0 = length(x0);
		float	CosThetaView0 = dot( x0, _View ) / r0;
		float	CosThetaSun0 = dot( x0, _Sun ) / r0;

		// Avoids imprecision problems in transmittance computations based on textures
//		_Extinction = AnalyticTransmittance( r, CosThetaView, _DistanceKm );
		_Extinction = GetTransmittance( r-GROUND_RADIUS_KM, CosThetaView, _DistanceKm );

		float4	Scattering_hit2atm = 0.0;
		if ( r0 > GROUND_RADIUS_KM + 0.01 )
		{	// Computes S[L]-T(x,x0)S[L] at x0

//#define	EPS	0.004
#define	EPS	0.04
#ifdef EPS
			// Avoids imprecision problems near horizon by interpolating between two points above and below horizon
			float	CosThetaViewHorizon = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (r*r) );
			if ( abs( CosThetaView - CosThetaViewHorizon ) < EPS )
			{
				float	a = ((CosThetaView - CosThetaViewHorizon) + EPS) / (2.0 * EPS);	// 0 for below the horizon, 1 for above

				// Sample below
				CosThetaView = CosThetaViewHorizon - EPS;
				r0 = sqrt( r * r + _DistanceKm * _DistanceKm + 2.0 * r * _DistanceKm * CosThetaView );
				CosThetaView0 = (r * CosThetaView + _DistanceKm) / r0;
// 				float4	InScattering0 = Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma );
// 				float4	InScattering1 = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );
// 				float4	InScatteringBelow = max( 0.0, InScattering0 - _Extinction.xyzx * InScattering1 );
// 
// 				CosThetaView = CosThetaViewHorizon + EPS;
// 				r0 = sqrt( r * r + _DistanceKm * _DistanceKm + 2.0 * r * _DistanceKm * CosThetaView );
// 				CosThetaView0 = (r * CosThetaView + _DistanceKm) / r0;
// 				InScattering0 = Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma );
// 				InScattering1 = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );
// 				float4	InScatteringAbove = max( 0.0, InScattering0 - _Extinction.xyzx * InScattering1 );
// 
// 				InScattering = lerp( InScatteringBelow, InScatteringAbove, a );

				float4	Scattering_cam2atm_below = Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma );
				float4	Scattering_hit2atm_below = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );

				// Sample above
				CosThetaView = CosThetaViewHorizon + EPS;
				r0 = sqrt( r * r + _DistanceKm * _DistanceKm + 2.0 * r * _DistanceKm * CosThetaView );
				CosThetaView0 = (r * CosThetaView + _DistanceKm) / r0;
				float4	Scattering_cam2atm_above = Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma );
				float4	Scattering_hit2atm_above = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );

				// Interpolate
				Scattering_cam2atm = lerp( Scattering_cam2atm_below, Scattering_cam2atm_above, a );
				Scattering_hit2atm = lerp( Scattering_hit2atm_below, Scattering_hit2atm_above, a );
			}
			else
//				InScattering = max( 0.0, InScattering - _Extinction.xyzx * Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma ) );
				Scattering_hit2atm = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );
#else
//			InScattering = max( 0.0, InScattering - _Extinction.xyzx * Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma ) );
			Scattering_hit2atm = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );
#endif
		}

		// Compute scattering happening between camera and hit
		float4	Scattering_cam2hit = max( 0.01, Scattering_cam2atm - _Extinction.xyzx * Scattering_hit2atm );

		// Attenuate in-scattered light between camera and hit due to shadowing by the cloud
#ifdef COMPLEX_SHADOWING
		float3	ShadowingRayleigh;
		float3	ShadowingMie;
		ComputeCloudShadowing( _PositionWorldKm / WORLD2KM, _View, _Sun, _DistanceKm / WORLD2KM, ShadowingRayleigh, ShadowingMie, _StepOffset );

		float	SunsetAttenuation = 1;//### lerp( 1.0, 0.5, smoothstep( 0.1, 0.0, abs(_Sun.y) ) );

		Scattering_cam2hit.xyz += SunsetAttenuation * _GodraysStrength.x * ShadowingRayleigh;
		Scattering_cam2hit.w += _GodraysStrength.y * dot( ShadowingMie, LUMINANCE );
		Scattering_cam2hit = max( 0.001, Scattering_cam2hit );
#else
		float	Shadowing = ComputeCloudShadowing( _PositionWorldKm / WORLD2KM, _View, _DistanceKm / WORLD2KM, _StepOffset );
 		float	GodraysStrength = saturate( lerp( 1.0 - _GodraysStrength.x, 1.0, Shadowing ) );
		Scattering_cam2hit *= GodraysStrength;
#endif

		// Recompose scattering happening between camera and atmosphere by adding the part beyond hit but blocked by the obstacle's extinction
		Scattering_cam2atm = Scattering_cam2hit + (_CloudScatteringExtinction.w * _GroundBlocking) * _Extinction.xyzx * Scattering_hit2atm;
//		Scattering_cam2atm = Scattering_cam2hit + _Extinction.xyzx * Scattering_hit2atm;
	}
	else
		_Extinction = GetTransmittance( r-GROUND_RADIUS_KM, CosThetaView );

	// Combine clouds with atmosphere extinction
	_CloudScatteringExtinction.xyz *= _Extinction;

	// Combine extinction with cloud's
	_Extinction *= _CloudScatteringExtinction.w;

	// Avoids imprecision problems in Mie scattering when sun is below horizon
	Scattering_cam2atm.w *= smoothstep( 0.00, 0.02, CosThetaSun );

	float	PhaseR = PhaseFunctionRayleigh( CosGamma );
	float	PhaseM = PhaseFunctionMie( CosGamma );

	_Scattering = _SunIntensity * max( 0.0, Scattering_cam2atm.xyz * PhaseR + GetMieFromRayleighAndMieRed( Scattering_cam2atm ) * PhaseM ) + _CloudScatteringExtinction.xyz;
}
#endif

float	Eval( float a, float b, float c, float d, float x )
{
	return 1.0 / (4.0 * pow( d, 1.5 )) * exp( -x * (c + d * x) ) *
		( -1.7724538509055160272981674833411 * exp( pow( c + 2.0 * d * x, 2.0 ) / (4.0 * d) ) * (b * c - 2 * a * d) * Erf( (c + 2 * d * x) / (2 * sqrt(d)) ) - 2 * b * sqrt( d ) );
}

// Doesn't work when current density is lower than previous one: b and b become negative and the sqrts get fucked up!
// I guess this should use erfi() and complex numbers but that's a bit too harsh for me!
float	__IntegrateScattering( float _PrevSigmaS, float _SigmaS, float _PrevSigmaT, float _SigmaT, float _Step )
{
	float	InvStep = 1.0 / _Step;
	float	a = _PrevSigmaS;
	float	b = (_SigmaS - _PrevSigmaS) * InvStep;
	float	c = _PrevSigmaT;
	float	d = (_SigmaT - _PrevSigmaT) * InvStep;

	return Eval( a, b, c, d, _Step ) - Eval( a, b, c, d, 0.0 );
}

// Exponential Integral
// (http://en.wikipedia.org/wiki/Exponential_integral)
float	Ei( float z )
{
 	return 0.5772156649015328606065 + log( 1e-4 + abs(z) ) + z * (1.0 + z * (0.25 + z * ( (1.0/18.0) + z * ( (1.0/96.0) + z * (1.0/600.0) ) ) ) );		// For x!=0
}

// =============================================
// Compute an approximate isotropic diffusion through infinite slabs
float3	ComputeIsotropicScattering( float3 _Position, float _Density, float3 _SunLight, float3 _SkyLightTop, float3 _SkyLightBottom )
{
	float	y_bottom = (_Position.y - _CloudAltitudeThickness.x) / _CloudAltitudeThickness.y;
	float	y_top = 1.0 - y_bottom;

	float3	SkyRadianceTop = _CloudIsotropicFactors.x * _SkyLightTop;
	float3	SkyRadianceBottom = _CloudIsotropicFactors.x * _SkyLightBottom;

	float3	SunRadiance = lerp( y_top, y_bottom, smoothstep( -0.05, 0.0, _LightDirection.y ) ) * _CloudIsotropicFactors.y * _SunLight;

	float3	GroundReflectance = _CloudIsotropicFactors.z * INVPI * float3( 1.0, 0.8, 0.2 );
	float3	GroundRadiance = y_top * GroundReflectance * _SunLight;

	float3	IsotropicLightTop = SkyRadianceTop + SunRadiance;
	float3	IsotropicLightBottom = SkyRadianceBottom + SunRadiance + GroundRadiance;

	float	BoxTop = _CloudAltitudeThickness.x + _CloudAltitudeThickness.y;
	float	BoxBottom = _CloudAltitudeThickness.x;
	float	IsotropicSphereRadiusTopKm = BoxTop - _Position.y;
	float	IsotropicSphereRadiusBottomKm = _Position.y - BoxBottom;

	float	a = -_CloudIsotropicScattering * IsotropicSphereRadiusTopKm;
	float3  IsotropicScatteringTop = IsotropicLightTop * max( 0.0, exp( a ) - a * Ei( a ));
			a = -_CloudIsotropicScattering * IsotropicSphereRadiusBottomKm;
	float3  IsotropicScatteringBottom = IsotropicLightBottom * max( 0.0, exp( a ) - a * Ei( a ));

	return  _Density * INVFOURPI * (IsotropicScatteringTop + IsotropicScatteringBottom);
}


#if 1
// ============= FORWARD TRACE ============= 
float3	IntegrateScattering( float3 _Light0, float3 _Light1, float _Sigma_s0, float _Sigma_s1, float _Sigma_t0, float _Sigma_t1, float _Step, out float _Extinction )
{

// This is completely wrong obviously, I just wanted to test if sampling a "pre-integration texture" would be faster than
//	performing the integration with ALU instructions and... it is!
// Except that unfortunately we have 3 varying parameters here: Start & End Densities as well as step distance
// The only way to make it work as a 2D texture would be to fix the step distance...
//
// _Extinction = 0;
// return _TexVolumeDepth.SampleLevel( LinearClamp, float2( _Sigma_s0, _Sigma_s1 ), 0.0 ).x;


	const float	SUB_STEPS_COUNT = 4.0;
	const float	INV_SUB_STEPS_COUNT = 1.0 / SUB_STEPS_COUNT;

	float	Sigma_s = _Sigma_s0;
	float	DSigma_s = (_Sigma_s1 - _Sigma_s0) * INV_SUB_STEPS_COUNT;
	float	Sigma_t = _Sigma_t0;
	float	DSigma_t = (_Sigma_t1 - _Sigma_t0) * INV_SUB_STEPS_COUNT;
	float3	L = _Light0;
	float3	dL = (_Light1 - _Light0) * INV_SUB_STEPS_COUNT;
	float	d = _Step * INV_SUB_STEPS_COUNT;

	_Extinction = 1.0;
	float3	I = 0.0;
	for ( float SubStep=0; SubStep < SUB_STEPS_COUNT; SubStep++ )
	{
		_Extinction = IntegrateExtinction( _Sigma_t0, Sigma_t, SubStep * d );
//		_Extinction *= exp( -Sigma_t * d );
		I += L * Sigma_s * d * _Extinction;

		Sigma_s += DSigma_s;
		Sigma_t += DSigma_t;
		L += dL;
	}

	_Extinction = IntegrateExtinction( _Sigma_t0, _Sigma_t1, _Step );

	return I;
}
#else
// ============= BACKWARD TRACE ============= 
float3	IntegrateScattering( float3 _Light0, float3 _Light1, float _Sigma_s0, float _Sigma_s1, float _Sigma_t0, float _Sigma_t1, float _Step, out float _Extinction )
{
	const float	SUB_STEPS_COUNT = 8.0;
	const float	INV_SUB_STEPS_COUNT = 1.0 / SUB_STEPS_COUNT;

	float	Sigma_s = _Sigma_s1;
	float	DSigma_s = (_Sigma_s0 - _Sigma_s1) * INV_SUB_STEPS_COUNT;
	float	Sigma_t = _Sigma_t1;
	float	DSigma_t = (_Sigma_t0 - _Sigma_t1) * INV_SUB_STEPS_COUNT;
	float3	L = _Light1;
	float3	dL = (_Light0 - _Light1) * INV_SUB_STEPS_COUNT;
	float	d = _Step * INV_SUB_STEPS_COUNT;


//return _Light1 * _Sigma_s1 * _Step;

	_Extinction = 1.0;
	float3	I = 0.0;
	for ( float SubStep=0; SubStep < SUB_STEPS_COUNT; SubStep++ )
	{
//_Extinction = IntegrateExtinction( _Sigma_t0, _Sigma_t0 + (_Sigma_t1 - _Sigma_t0) * SubStep * INV_SUB_STEPS_COUNT, SubStep * d );
		I *= exp( -Sigma_t * d );
		I += L * Sigma_s * d;

		Sigma_s += DSigma_s;
		Sigma_t += DSigma_t;
		L += dL;
	}

_Extinction = IntegrateExtinction( _Sigma_t0, _Sigma_t1, _Step );

	return I;
}
#endif

VS_IN	VS( VS_IN _In )	{ return _In; }

PS_OUT	ReturnTestValue( float3 _Value )
{
	PS_OUT	Out;
	Out.Scattering = Out.Extinction = _Value;
	return Out;
}

PS_OUT	PS( VS_IN _In )
{
	float2	UV = _In.__Position.xy * _dUV.xy;

#if 1
	// Sample min/max depths at position
	float2	ZMinMax = _TexVolumeDepth.SampleLevel( LinearClamp, UV, 0.0 ).xy;
#elif 0
	// New strategy: sample the interval length at the four corners of the larger texel from the low resolution depth pass
	//	and keep the min/max of the 4 values to ensure we don't miss any feature...
	float3	dUV_Low = 2.0 * _dUV;

	float2	Pixel_Low = floor( 0.5 * _In.__Position.xy );
	float2	UV2 = (Pixel_Low + 0.5) * dUV_Low.xy;

	float2	ZMinMax00 = _TexVolumeDepthPass.SampleLevel( PointClamp, UV2, 0.0 ).xy;	UV2 += dUV_Low.xz;
	float2	ZMinMax01 = _TexVolumeDepthPass.SampleLevel( PointClamp, UV2, 0.0 ).xy;	UV2 += dUV_Low.zy;
	float2	ZMinMax11 = _TexVolumeDepthPass.SampleLevel( PointClamp, UV2, 0.0 ).xy;	UV2 -= dUV_Low.xz;
	float2	ZMinMax10 = _TexVolumeDepthPass.SampleLevel( PointClamp, UV2, 0.0 ).xy;	UV2 -= dUV_Low.zy;
	float2	ZMinMax = float2(
		min( min( min( ZMinMax00.x, ZMinMax01.x ), ZMinMax10.x ), ZMinMax11.x ),
		max( max( max( ZMinMax00.y, ZMinMax01.y ), ZMinMax10.y ), ZMinMax11.y )
		);
#else
	// Even newer strategy using the downsampled depth buffer: sample the interval length at the four corners of the larger texel from the low resolution depth pass
	//	and keep the min/max of the 4 values to ensure we don't miss any feature...
	uint2	LowPixelIndex = uint2( _In.__Position.xy ) / 2;
	float2	ZMinMax00 = _TexVolumeDepthPass[LowPixelIndex].xy;	LowPixelIndex.x++;
	float2	ZMinMax01 = _TexVolumeDepthPass[LowPixelIndex].xy;	LowPixelIndex.y++;
	float2	ZMinMax11 = _TexVolumeDepthPass[LowPixelIndex].xy;	LowPixelIndex.x--;
	float2	ZMinMax10 = _TexVolumeDepthPass[LowPixelIndex].xy;	LowPixelIndex.y--;
	float2	ZMinMax = float2(
		min( min( min( ZMinMax00.x, ZMinMax01.x ), ZMinMax10.x ), ZMinMax11.x ) - 0.0,
		max( max( max( ZMinMax00.y, ZMinMax01.y ), ZMinMax10.y ), ZMinMax11.y ) + 0.0
		);
#endif


	// Sample ZBuffer
//	float	Z = ReadDepth( UV );
	float	Z = _TexDownsampledSceneDepth.mips[1][_In.__Position.xy].z;	// Use ZMax
	ZMinMax.y = min( ZMinMax.y, Z );

	float	Depth = ZMinMax.y - ZMinMax.x;
//	Depth = max( 1e-3, Depth );

//return ReturnTestValue( 0.04 * Z );
//return ReturnTestValue( 0.1 * ZMinMax.y );

//return ReturnTestValue( 0.1 * _TexVolumeDepthPass.SampleLevel( LinearClamp, UV, 0.0 ).x );
// return ReturnTestValue( 0.01 * _TexVolumeDepthPass.SampleLevel( LinearClamp, UV, 0.0 ).y );

#if 1

//### Super important line to get a nice precision everywhere: we lack details at a distance (i.e. we don't trace the clouds fully) but who cares since we keep a nice precision?
//ZMinMax.y = ZMinMax.x + min( 8.0, Depth );	// Don't trace more than 8 units in length
ZMinMax.y = ZMinMax.x + min( 8.0 * _CloudAltitudeThickness.y, Depth );	// Don't trace more than N times the box height's in length (less precision, prefer line above)

	float	MipBias = 0.0;

#elif 0
	// Instead of clamping max depth, I'm instead adding mip bias based on the max depth we should have clamped and actual depth
	float	MaxDepth = 8.0;	// We should have clamped at 4 units max
	float	DepthClampingRatio = max( 1.0, Depth / MaxDepth );

	float	MipBias = 0.0 + 0.5 * log2( DepthClampingRatio );

	// For example, if we need to trace twice the max depth, then the mip bias will be 1

#else
	float	MipBias = 0.01 * ZMinMax.x;
#endif

	// Retrieve start & end positions in world space
	float3	ViewCamera = float3( _CameraData.x * (2.0 * UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * UV.y), 1.0 );
	float4	View = float4( mul( float4( ViewCamera, 0.0 ), _Camera2World ).xyz, 1.0 );
	float4	WorldPosStart = float4( _Camera2World[3].xyz, 0.0 ) + ZMinMax.x * View;
	float4	WorldPosEnd = float4( _Camera2World[3].xyz, 0.0 ) + ZMinMax.y * View;


//#define FIXED_STEP_SIZE	0.01
#ifndef FIXED_STEP_SIZE
	float	StepsCount = ceil( Depth * MAX_STEPS_COUNT * _CloudAltitudeThickness.y/8.0 );	// Good for 64 steps
//	float	StepsCount = ceil( Depth * MAX_STEPS_COUNT * _CloudAltitudeThickness.y/4.0 );	// Good for 32 steps
//	float	StepsCount = ceil( 2.0 * FastScreenNoise( _In.__Position.xy ) + Depth * 32.0 * _CloudAltitudeThickness.y/8.0 );	// Add noise to hide banding
//	float	StepsCount = MAX_STEPS_COUNT;
 			StepsCount = min( MAX_STEPS_COUNT, StepsCount );

	if ( Depth > 0.0 )
		StepsCount = max( MIN_STEPS_COUNT, StepsCount );

	float4	Step = (WorldPosEnd - WorldPosStart) / StepsCount;

//	float	PosOffset = 0.5;	// Fixed offset
	float	PosOffset = 0.25 * FastNoise( float3( 2.0*_In.__Position.xy, 0.0 ) );	// Random offset
	float4	Position = WorldPosStart + PosOffset * Step;

#else
	// Here, the steps have a fixed size and we simply determine their amount
	// This strategy misses a lot of details and requires lots of steps!
	// It was necessary to use a 2D pre-integration table with constant steps but unfortunately the
	//	time we would have gained sampling a 2D texture instead of performing integration with ALU
	//	is lost due to the fact we're using too many steps!
	float	StepsCount = Depth / FIXED_STEP_SIZE;
			StepsCount = min( MAX_STEPS_COUNT, StepsCount );

	float4	Step = FIXED_STEP_SIZE * float4( View, 1.0 );
	float4	Position = float4( WorldPosStart, 0.0 ) + 0.5 * Step;

#endif

	// Compute phase
	float	ViewLength = length( View.xyz );
	View.xyz /= ViewLength;

	const float	g_iso = _CloudPhases.x;
	const float	g_forward = _CloudPhases.y;
	float	CosTheta = dot( View.xyz, _LightDirection );
	float	Phase_iso = 1.0 / (4 * PI) * (1 - g_iso*g_iso) * pow( max(0.0, 1+g_iso*g_iso-g_iso*CosTheta ), -1.5 );
	float	Phase_forward = 1.0 / (4 * PI) * (1 - g_forward*g_forward) * pow( max(0.0, 1+g_forward*g_forward-g_forward*CosTheta ), -1.5 );
	float	Phase = lerp( Phase_iso, Phase_forward, 0.2 );

	// Get Sun light & Sky lights
	float3	SunLight = SUN_INTENSITY * GetTransmittanceWithShadow( _CloudAltitudeThickness.x + _CloudAltitudeThickness.y, _LightDirection.y );
	float3	SkyLightTop = SUN_INTENSITY * GetIrradiance( _TexIrradiance, (_CloudAltitudeThickness.x + 1.0 * _CloudAltitudeThickness.y), _LightDirection.y );
	float3	SkyLightBottom = SUN_INTENSITY * GetIrradiance( _TexIrradiance, (_CloudAltitudeThickness.x + 0.0 * _CloudAltitudeThickness.y), _LightDirection.y );

	// Start integration
	float	Sigma_t = 0.0;
	float	Sigma_s = 0.0;
	float3	Light = 0;//SunLight * GetCloudTransmittance( WorldPosStart.xyz );		// Start with light at start position
	float3	Scattering = 0.0;
	float	Transmittance = 1.0;

	for ( float StepIndex=0.0; StepIndex < StepsCount; StepIndex++ )
	{
		float	Density = GetVolumeDensity( Position.xyz, MipBias );

		float	PreviousSigma_t = Sigma_t;
		float	PreviousSigma_s = Sigma_s;
		Sigma_t = _CloudExtinctionScattering.x * Density;
		Sigma_s = _CloudExtinctionScattering.y * Density;

		float	Shadowing = GetCloudTransmittance( Position.xyz );
//		float	Shadowing = GetCloudTransmittance( (Position + 0.5 * Step).xyz );

		Shadowing = saturate( lerp( 1.0 - _CloudShadowStrength, 1.0, Shadowing ) );
		Shadowing *= smoothstep( 0.0, 1.0, smoothstep( 0.0, 0.03, abs(_LightDirection.y) ) );	// Full shadowing when the light is horizontal

//		Light = SunLight * Shadowing;

#if 1
		// ======================== Old Strategy without sub-step integration ========================
		float	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );	// Compute extinction
		Transmittance *= StepTransmittance;

		float3	PreviousLight = Light;
		Light = Sigma_s * Phase * SunLight * Shadowing;
		Light += ComputeIsotropicScattering( Position.xyz, Sigma_s, SunLight, SkyLightTop, SkyLightBottom );

		// Compute scattering
		Scattering += Transmittance * 0.5 * (PreviousLight + Light) * Step.w;
//###		Scattering += Transmittance * 0.5 * (PreviousLight + Light) * (1.0 - StepTransmittance) / max( 1e-2, 0.5 * (PreviousSigma_t + Sigma_t));	// From Tessendorf. No noticeable chanes except a darkening of some regions


		// Compute scattering
// 		float3	StepScattering  = Sigma_s * Phase * Light * Step.w;
// 				StepScattering += ComputeIsotropicScattering( Position.xyz, Density, SunLight, SkyLightTop, SkyLightBottom ) * Step.w;
// 		Scattering += Transmittance * StepScattering;

#else
		// ======================== New Strategy WITH sub-step integration ========================
		// More accurate but also a little more hungry, but always better than using more samples!
		float	StepTransmittance;
		float3	StepScattering  = Phase * IntegrateScattering( PreviousLight, Light, PreviousSigma_s, Sigma_s, PreviousSigma_t, Sigma_t, Step.w, StepTransmittance );
				StepScattering += ComputeIsotropicScattering( Position.xyz, Density, SunLight, SkyLightTop, SkyLightBottom ) * Step.w;
		Scattering += Transmittance * StepScattering;
		Transmittance *= StepTransmittance;
#endif

		// Advance in world and phase
		Position += Step;
	}

	// Compute intersection with the top cloud plane or the ground
// #ifdef CAMERA_ABOVE_CLOUDS
// 	float	HitDistance = ZMinMax.y < 0.0 ? Z : ZMinMax.y;	// Top cloud distance is ZMin
// #else
// 	float	HitDistance = ZMinMax.y < 0.0 ? Z : ZMinMax.x;	// Top cloud distance is ZMax
// #endif
	float	HitDistance = ViewLength * (ZMinMax.y < 0.0 ? Z : min( Z, 0.5 * (ZMinMax.x + ZMinMax.y)));
	float	HitDistanceKm = WORLD2KM * HitDistance;

//	HitDistanceKm = min( 30.0, HitDistanceKm );			// Beyond that, we're outside the clouds...

	float	StepOffset = 0;//0.5 * FastScreenNoise( _In.__Position.xy );
	float3	PositionWorld = _Camera2World[3].xyz;
	float3	PositionWorldKm = WORLD2KM * PositionWorld;

	float	GroundBlocking = Z < 0.99*_CameraData.w ? 0 : 1;
	if ( GroundBlocking > 0.5 )
	{	// Not blocked by terrain, check for other blockers
		float	GroundHitDistanceKm = SphereIntersectionEnter( PositionWorldKm, View.xyz, -0.5 );
		if ( GroundHitDistanceKm > 0.0 && GroundHitDistanceKm < HitDistanceKm )
			HitDistanceKm = GroundHitDistanceKm;
		else
			HitDistanceKm = min( 100.0, HitDistanceKm );	// Don't trace further than 100km no matter what...
	}

	HitDistanceKm = min( HitDistanceKm, SphereIntersectionExit( PositionWorldKm, View.xyz, ATMOSPHERE_THICKNESS_KM ) );	// Don't trace further than atmosphere (shouldn't be necessary since sampling any table at this distance should return the correct value)


	// Compute sky color & compose with cloud
	PS_OUT	Out;
	ComputeFinalColor( PositionWorldKm, View.xyz, HitDistanceKm, _LightDirection, float4( Scattering, Transmittance ), GroundBlocking, StepOffset, Out.Scattering, Out.Extinction );

// Out.Scattering = 0.1 * HitDistance;
// Out.Extinction = 0.0;

	return Out;
}
