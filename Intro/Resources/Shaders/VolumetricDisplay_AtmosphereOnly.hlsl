//////////////////////////////////////////////////////////////////////////
// This shader renders the atmosphere without clouds, need to debug these tables!!!
//
#include "Inc/Global.hlsl"
#include "Inc/Atmosphere.hlsl"

cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
	bool		_bSampleTerrainShadow;
};

Texture2D		_TexSceneDepth				: register(t11);
Texture2D		_TexDownsampledSceneDepth	: register(t12);

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

void	ComputeSkyColor( float3 _PositionWorldKm, float3 _View, float _DistanceKm, float3 _Sun, float4 _CloudScatteringExtinction, float _GroundBlocking, float _StepOffset, out float3 _Scattering, out float3 _Extinction )
{
//_PositionWorldKm.y -= _AltitudeOffsetKm;

//_DistanceKm = 1000.0;

	////////////////////////////////////////////////////////////
	// Compute sky radiance arriving at camera, not accounting for clouds
	float3	StartPositionKm = _PositionWorldKm - EARTH_CENTER_KM;	// Start position from origin (i.e. center of the Earth)
	float	StartRadiusKm = length( StartPositionKm );
	float	StartAltitudeKm = StartRadiusKm - GROUND_RADIUS_KM;
	float3	StartNormal = StartPositionKm / StartRadiusKm;
	float	CosThetaView = dot( StartNormal, _View );
	float	CosThetaSun = dot( StartNormal, _Sun );
	float	CosGamma = dot( _View, _Sun );

	float4	Lin_camera2atmosphere = Sample4DScatteringTable( _TexScattering, StartAltitudeKm, CosThetaView, CosThetaSun, CosGamma );
	float3	Lin_camera2atmosphere_Rayleigh = Lin_camera2atmosphere.xyz;
	float3	Lin_camera2atmosphere_Mie = GetMieFromRayleighAndMieRed( Lin_camera2atmosphere );

	////////////////////////////////////////////////////////////
	// Account for obstacle occlusion
	float3	HitPositionKm = _PositionWorldKm + _DistanceKm * _View - EARTH_CENTER_KM;
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

	float3	Lin_camera2hit_Rayleigh = max( 0.0, Lin_camera2atmosphere_Rayleigh - Transmittance * Lin_hit2atmosphere_Rayleigh );
	float3	Lin_camera2hit_Mie = max( 0.0, Lin_camera2atmosphere_Mie - Transmittance * Lin_hit2atmosphere_Mie );

	////////////////////////////////////////////////////////////
	// Rebuild final scattering, accounting for extinction
	float	BlockerExtinction = _GroundBlocking * _CloudScatteringExtinction.w;	// Partialy mask remaining segment

	float3	Lin_Rayleigh = Lin_camera2hit_Rayleigh + BlockerExtinction * Transmittance * Lin_hit2atmosphere_Rayleigh;
	float3	Lin_Mie = Lin_camera2hit_Mie + BlockerExtinction * Transmittance * Lin_hit2atmosphere_Mie;
//	float3	Lin_Rayleigh = Lin_camera2atmosphere_Rayleigh;
//	float3	Lin_Mie = Lin_camera2atmosphere_Mie;
//	float3	Lin_Rayleigh = Lin_camera2hit_Rayleigh;
//	float3	Lin_Mie = Lin_camera2hit_Mie;
//	float3	Lin_Rayleigh = Transmittance * Lin_hit2atmosphere_Rayleigh;
//	float3	Lin_Mie = Transmittance * Lin_hit2atmosphere_Mie;

	// Finalize extinction & scattering
	_Extinction = Transmittance * _CloudScatteringExtinction.w;	// Combine with cloud
	_Scattering = SUN_INTENSITY * (PhaseFunctionRayleigh( CosGamma ) * Lin_Rayleigh + PhaseFunctionMie( CosGamma ) * Lin_Mie) + _CloudScatteringExtinction.xyz;

// _Extinction = 0;
// _Scattering = 4.0 * Transmittance;

}


// // Rayleigh
// const float HR = 8.0;
// const vec3 betaR = vec3(5.8e-3, 1.35e-2, 3.31e-2);
// 
// // Mie
// // DEFAULT
// const float HM = 1.2;
// const vec3 betaMSca = vec3(4e-3);
// const vec3 betaMEx = betaMSca / 0.9;
// const float mieG = 0.8;


// Optical depth for ray (r,_CosThetaView) of length d, using analytic formula, intersections with ground ignored
// H=height scale of exponential density function
float	OpticalDepth( float H, float r, float _CosThetaView, float d )
{
	float	a = sqrt( (0.5/H) * r );
	float2	a01 = a * float2( _CosThetaView, _CosThetaView + d / r );
	float2	a01s = sign(a01);
	float2	a01sq = a01*a01;
	float	x = a01s.y > a01s.x ? exp( a01sq.x ) : 0.0;
	float2	y = a01s / (2.3193*abs(a01) + sqrt( 1.52*a01sq + 4.0 )) * float2( 1.0, exp( min( 0.0, -d/H * (d/(2.0*r)+_CosThetaView) ) )  );
	return sqrt( max( 0.1, TWOPI*H*r ) ) * exp( (GROUND_RADIUS_KM-r) / H ) * (x + dot( y, float2( 1.0, -1.0 ) ));
}

// transmittance(=transparency) of atmosphere for ray (r,_CosThetaView) of length d, intersections with ground ignored
// uses analytic formula instead of transmittance texture
float3	AnalyticTransmittance( float r, float _CosThetaView, float _Distance )
{
	const float3	betaR = _AirParams.x * SIGMA_SCATTERING_RAYLEIGH;
//	const float3	betaR = float3( 0.0058, 0.0135, 0.0331 );
	float	HrefR = _AirParams.y;
//	float	HrefR = 8.0;
	const float	betaM = _FogParams.y;
//	const float	betaM = 0.004 / 0.9;
	float	HrefM = _FogParams.z;
//	float	HrefM = 1.2;

	return exp( -betaR * OpticalDepth( HrefR, r, _CosThetaView, _Distance ) - betaM * OpticalDepth( HrefM, r, _CosThetaView, _Distance ) );
}

// Rayleigh phase function
float phaseFunctionR( float mu )
{
    return (3.0 / (16.0 * PI)) * (1.0 + mu * mu);
}

// Mie phase function
float phaseFunctionM( float mu )
{
	float g = _FogParams.w;
	return 1.5 * INVFOURPI * (1.0 - g*g) * pow( 1.0 + (g*g) - 2.0*g*mu, -1.5 ) * (1.0 + mu * mu) / (2.0 + g*g);
}

// approximated single Mie scattering (cf. approximate Cm in paragraph "Angular precision")
float3	getMie( float4 rayMie )
{
	// rayMie.rgb=C*, rayMie.w=Cm,r
	const float3	betaR = float3( 0.0058, 0.0135, 0.0331 );

	return rayMie.xyz * rayMie.w / max( 1e-4, rayMie.x ) * (betaR.r / betaR);
}

void	ComputeSkyColor2( float3 _PositionWorldKm, float3 _View, float _DistanceKm, float3 _Sun, float4 _CloudScatteringExtinction, float _GroundBlocking, float _StepOffset, out float3 _Scattering, out float3 _Extinction )
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
	float4	InScattering = max( 0.0, Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma ) );

// _Scattering = CosGamma;
// _Extinction = 0;
// return;

	if ( _DistanceKm > 0.0 )
	{	// Looking at the ground
CosThetaView = min( -0.01, CosThetaView );	// Force view downward to tap into the "ground part" of the 4D table

		float3	x0 = x + _DistanceKm * _View;
		float	r0 = length(x0);
		float	CosThetaView0 = dot( x0, _View ) / r0;
		float	CosThetaSun0 = dot( x0, _Sun ) / r0;

		// Avoids imprecision problems in transmittance computations based on textures
//		_Extinction = AnalyticTransmittance( r, CosThetaView, _DistanceKm );
		_Extinction = GetTransmittance( r-GROUND_RADIUS_KM, CosThetaView, _DistanceKm );
		if ( r0 > GROUND_RADIUS_KM + 0.01 )
		{
			// Computes S[L]-T(x,x0)S[L] at x0
			InScattering = max( 0.0, InScattering - _Extinction.xyzx * Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma ) );

//#define	EPS	0.004
#define	EPS	0.04
#ifdef EPS
			// Avoids imprecision problems near horizon by interpolating between two points above and below horizon
			float	CosThetaViewHorizon = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (r*r) );
			if ( abs( CosThetaView - CosThetaViewHorizon ) < EPS )
			{
				float	a = ((CosThetaView - CosThetaViewHorizon) + EPS) / (2.0 * EPS);	// 0 for below the horizon, 1 for above

				CosThetaView = CosThetaViewHorizon - EPS;
				r0 = sqrt( r * r + _DistanceKm * _DistanceKm + 2.0 * r * _DistanceKm * CosThetaView );
				CosThetaView0 = (r * CosThetaView + _DistanceKm) / r0;
				float4	InScattering0 = Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma );
				float4	InScattering1 = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );
				float4	InScatteringBelow = max( 0.0, InScattering0 - _Extinction.xyzx * InScattering1 );

				CosThetaView = CosThetaViewHorizon + EPS;
				r0 = sqrt( r * r + _DistanceKm * _DistanceKm + 2.0 * r * _DistanceKm * CosThetaView );
				CosThetaView0 = (r * CosThetaView + _DistanceKm) / r0;
				InScattering0 = Sample4DScatteringTable( _TexScattering, r-GROUND_RADIUS_KM, CosThetaView, CosThetaSun, CosGamma );
				InScattering1 = Sample4DScatteringTable( _TexScattering, r0-GROUND_RADIUS_KM, CosThetaView0, CosThetaSun0, CosGamma );
				float4	InScatteringAbove = max( 0.0, InScattering0 - _Extinction.xyzx * InScattering1 );

				InScattering = lerp( InScatteringBelow, InScatteringAbove, a );
			}
#endif
		}
	}

	// Avoids imprecision problems in Mie scattering when sun is below horizon
	InScattering.w *= smoothstep( 0.00, 0.02, CosThetaSun );

	float	PhaseR = phaseFunctionR( CosGamma );
	float	PhaseM = phaseFunctionM( CosGamma );

	_Scattering = _SunIntensity * max( 0.0, InScattering.xyz * PhaseR + getMie(InScattering) * PhaseM );

// _Scattering = 10.0 * PhaseM;
// _Extinction = 0.0;
// return;
}



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

	// Sample ZBuffer
//	float	SceneZ = ReadDepth( UV );
	float	SceneZ = _TexDownsampledSceneDepth.mips[1][_In.__Position.xy].z;	// Use max Z
	float	GroundBlocking = step( 0.9*_CameraData.w, SceneZ );					// 0 if we hit anything

	float3	ViewCamera = float3( _CameraData.x * (2.0 * UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * UV.y), 1.0 );
	float3	View = mul( float4( ViewCamera, 0.0 ), _Camera2World ).xyz;
	float	ViewLength = length( View );
			View /= ViewLength;

	float3	PositionWorld = _Camera2World[3].xyz;
	float3	PositionWorldKm = WORLD2KM * PositionWorld;
	float	GroundHitDistanceKm = WORLD2KM * ViewLength * SceneZ;

//	float	HitDistanceKm = min( GroundHitDistanceKm, SphereIntersectionExit( PositionWorldKm, View, ATMOSPHERE_THICKNESS_KM ) );
//	float	HitDistanceKm = GroundHitDistanceKm;
// 	float	HitDistanceKm = lerp( GroundHitDistanceKm, SphereIntersectionExit( PositionWorldKm, View, ATMOSPHERE_THICKNESS_KM ), GroundBlocking );

	float	HitDistanceKm = lerp( GroundHitDistanceKm, -1.0, GroundBlocking );	// Negative distance means no funky computation...


	// Compute sky color & compose with cloud
	PS_OUT	Out;
//	ComputeSkyColor( PositionWorldKm, View, HitDistanceKm, _LightDirection, float4( 0, 0, 0, 1 ), GroundBlocking, 0, Out.Scattering, Out.Extinction );
	ComputeSkyColor2( PositionWorldKm, View, HitDistanceKm, _LightDirection, float4( 0, 0, 0, 1 ), GroundBlocking, 0, Out.Scattering, Out.Extinction );

//Out.Scattering = 0;

	return Out;
}
