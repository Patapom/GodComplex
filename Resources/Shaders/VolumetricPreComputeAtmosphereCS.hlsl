//////////////////////////////////////////////////////////////////////////
// This shader pre-computes the various tables required by the atmospheric rendering
// It's an implementation of http://www-ljk.imag.fr/Publications/Basilic/com.lmc.publi.PUBLI_Article@11e7cdda2f7_f64b69/article.pdf
// Remasterized as a Compute Shader and different parameters for the 4D scattering table
//
#include "Inc/Global.hlsl"
#include "Inc/Atmosphere.hlsl"

#define	THREADS_COUNT_X	16	// Amount of thread per group
#define	THREADS_COUNT_Y	16
#define	THREADS_COUNT_Z	4	// 4 as the product of all thread counts cannot exceed 1024!

#define	INSCATTER_NON_LINEAR_VIEW			// Use non-linear mapping of cos(Theta_view)
//#define	INSCATTER_NON_LINEAR_VIEW_POM	// Use my "formula" instead of theirs
#define	INSCATTER_NON_LINEAR_SUN			// Use non-linear mapping of cos(Theta_sun)


cbuffer	cbCompute	: register( b10 )
{
	uint3	_TargetSize;	// Final render target size (2D or 3D)
	uint3	_GroupsCount;	// Amount of render groups (2D or 3D) for a single pass
	uint3	_PassIndex;		// Index of the X,Y,Z pass (each pass computes THREAD_COUNT_X*THREAD_COUNT_Y*THREAD_COUNT_Z texels)

	bool	_bFirstPass;				// True if we're computing the first pass that reads single-scattering for Rayleigh & Mie from 2 separate tables
	float	_AverageGroundReflectance;
};

struct	CS_IN
{
	uint3	GroupID			: SV_GroupID;			// Defines the group offset within a Dispatch call, per dimension of the dispatch call
	uint3	ThreadID		: SV_DispatchThreadID;	// Defines the global thread offset within the Dispatch call, per dimension of the group
	uint3	GroupThreadID	: SV_GroupThreadID;		// Defines the thread offset within the group, per dimension of the group
	uint	GroupIndex		: SV_GroupIndex;		// Provides a flattened index for a given thread within a given group
};

Texture2D	_TexIrradianceDelta : register(t10);			// deltaE
Texture3D	_TexScatteringDelta_Rayleigh : register(t11);	// deltaSR
Texture3D	_TexScatteringDelta_Mie : register(t12);		// deltaSM
Texture3D	_TexScatteringDelta : register(t13);			// deltaJ

StructuredBuffer<float4>	_Input0 : register(t14);		// Possible inputs from previous stage
StructuredBuffer<float4>	_Input1 : register(t15);

RWStructuredBuffer<float4>	_Target0 : register(u0);		// What we're computing
RWStructuredBuffer<float4>	_Target1 : register(u1);


//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////
// Helpers

// Computes the flattened texel informations
uint2	GetTexelIndex2D( CS_IN _In, out uint _TexelIndex )
{
	uint	TexelX = (_GroupsCount.x * THREADS_COUNT_X) * _PassIndex.x + _In.ThreadID.x;
	uint	TexelY = (_GroupsCount.y * THREADS_COUNT_Y) * _PassIndex.y + _In.ThreadID.y;

	_TexelIndex = _TargetSize.x * TexelY + TexelX;

	return uint2( TexelX, TexelY );
}

uint3	GetTexelIndex3D( CS_IN _In, out uint _TexelIndex )
{
	uint	TexelX = (_GroupsCount.x * THREADS_COUNT_X) * _PassIndex.x + _In.ThreadID.x;
	uint	TexelY = (_GroupsCount.y * THREADS_COUNT_Y) * _PassIndex.y + _In.ThreadID.y;
	uint	TexelZ = (_GroupsCount.z * THREADS_COUNT_Z) * _PassIndex.z + _In.ThreadID.z;

	_TexelIndex = _TargetSize.x * (_TargetSize.y * TexelZ + TexelY) + TexelX;

	return uint3( TexelX, TexelY, TexelZ );
}

// Gets the altitude and scales for a 3D target rendering depending on the target slice
void	GetSliceData( uint _SliceIndex, out float _AltitudeKm, out float4 _dhdH )
{
    float RadiusKm = _SliceIndex / (RESOLUTION_ALTITUDE - 1.0);

    RadiusKm = RadiusKm * RadiusKm;
    RadiusKm = sqrt( lerp( GROUND_RADIUS_KM * GROUND_RADIUS_KM, ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM, RadiusKm ) );	// Radius grows quadratically to have more precision near the ground
	if ( _SliceIndex == 0 )
		RadiusKm += 0.01;	// Never completely ground
	else if ( _SliceIndex == uint(RESOLUTION_ALTITUDE)-1 )
		RadiusKm -= 0.001;	// Never completely top of atmosphere

    float	dmin = ATMOSPHERE_RADIUS_KM - RadiusKm;
    float	dminp = RadiusKm - GROUND_RADIUS_KM;
    float	dh = sqrt( RadiusKm * RadiusKm - GROUND_RADIUS_KM * GROUND_RADIUS_KM );
    float	dH = dh + sqrt( ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - GROUND_RADIUS_KM * GROUND_RADIUS_KM );

	_dhdH = float4( dmin, dH, dminp, dh );
	_AltitudeKm = RadiusKm - GROUND_RADIUS_KM;
}


// Gets the zenith/view angle (cos theta), zenith/Sun angle (cos theta Sun) and azimuth Sun angle (cos gamma) from a 2D parameter
//
//	_dhdH.x = ATMOSPHERE_RADIUS_KM - RadiusKm
//	_dhdH.y = sqrt( RadiusKm * RadiusKm - GROUND_RADIUS_KM * GROUND_RADIUS_KM ) + sqrt( ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - GROUND_RADIUS_KM * GROUND_RADIUS_KM ) = dh + H = dH
//	_dhdH.z = RadiusKm - GROUND_RADIUS_KM
//	_dhdH.w = sqrt( RadiusKm * RadiusKm - GROUND_RADIUS_KM * GROUND_RADIUS_KM ) = dh
//
void	GetAnglesFrom4D( float2 _UV, float4 _dhdH, float _AltitudeKm, out float _CosThetaView, out float _CosThetaSun, out float _CosGamma )
{
#ifdef INSCATTER_NON_LINEAR_VIEW

#ifdef INSCATTER_NON_LINEAR_VIEW_POM

	_CosThetaView = abs( 2.0 * _UV.y - 1.0 );
	_CosThetaView *= (_UV.y < 0.5 ? -1.0 : +1.0) * _CosThetaView;	// Squared progression for more precision near horizon

#else

	float	dmin = _dhdH.x;
	float	dminp = _dhdH.z;
	float	dH = _dhdH.y;
	float	dh = _dhdH.w;

	float r = GROUND_RADIUS_KM + _AltitudeKm;
	if ( _UV.y < 0.5 )
	{	// Viewing toward the sky
		float	d = 1.0 - 2.0 * _UV.y;
				d = clamp( d * dh, dminp, dh * 0.999 );

		_CosThetaView = (GROUND_RADIUS_KM * GROUND_RADIUS_KM - r * r - d * d) / (2.0 * r * d);
		_CosThetaView = min( _CosThetaView, -sqrt( 1.0 - (GROUND_RADIUS_KM / r) * (GROUND_RADIUS_KM / r) ) - 0.001 );
	}
	else
	{	// Viewing toward the ground
		float	d = 2.0 * _UV.y - 1.0;
				d = clamp( d * dH, dmin, dH * 0.999 );

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


//////////////////////////////////////////////////////////////////////////
// 0] Pre-Computes the transmittance table for all possible altitudes and zenith angles
//
float	ComputeOpticalDepth( float _AltitudeKm, float _CosTheta, const float _Href, const uint STEPS_COUNT=500 )
{
	// Compute distance to atmosphere or ground, whichever comes first
	float4	PositionKm = float4( 0.0, _AltitudeKm, 0.0, 0.0 );
	float3	View = float3( sqrt( 1.0 - _CosTheta*_CosTheta ), _CosTheta, 0.0 );
	bool	bGroundHit;
	float	TraceDistanceKm = ComputeNearestHit( PositionKm.xyz, View, ATMOSPHERE_THICKNESS_KM, bGroundHit );
	if ( bGroundHit )
		return 1e5;	// Completely opaque due to hit with ground: no light can come this way...
					// Be careful with large values in 16F!

	float	Result = 0.0;
	float4	StepKm = (TraceDistanceKm / STEPS_COUNT) * float4( View, 1.0 );

	float		PreviousAltitudeKm = _AltitudeKm;
	for ( uint i=0; i < STEPS_COUNT; i++ )
	{
		PositionKm += StepKm;
		_AltitudeKm = length( PositionKm.xyz - EARTH_CENTER_KM ) - GROUND_RADIUS_KM;
		Result += exp( (PreviousAltitudeKm + _AltitudeKm) * (-0.5 / _Href) );	// Gives the integral of a linear interpolation in altitude
		PreviousAltitudeKm = _AltitudeKm;
	}

	return Result * StepKm.w;
}

[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, 1 )]
void	PreComputeTransmittance( CS_IN _In )
{
	uint	TexelIndex;
	uint2	Texel = GetTexelIndex2D( _In, TexelIndex );
	float2	UV = float2( Texel ) / _TargetSize.xy;

	float	AltitudeKm = UV.y*UV.y * ATMOSPHERE_THICKNESS_KM;					// Grow quadratically to have more precision near the ground
	float	CosThetaView = -0.15 + tan( 1.5 * UV.x ) / tan(1.5) * (1.0 + 0.15);	// Grow tangentially to have more precision horizontally
//	float	CosThetaView = lerp( -0.15, 1.0, UV.x );							// Grow linearly

	float3	OpticalDepth = _AirParams.x * SIGMA_SCATTERING_RAYLEIGH * ComputeOpticalDepth( AltitudeKm, CosThetaView, _AirParams.y ) + _FogParams.y * ComputeOpticalDepth( AltitudeKm, CosThetaView, _FogParams.z );

//	_Target0[TexelIndex] = float4( exp( -OpticalDepth ), 0.0 );
	_Target0[TexelIndex] = float4( min( 1e5, OpticalDepth ), 0.0 );		// We directly store optical depth otherwise we lose too much precision using a division!
}


//////////////////////////////////////////////////////////////////////////
// 1] Pre-Computes the ground irradiance table accounting for single scattering only
//
[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, 1 )]
void	PreComputeIrradiance_Single( CS_IN _In )
{
	uint	TexelIndex;
	uint2	Texel = GetTexelIndex2D( _In, TexelIndex );
	float2	UV = float2( Texel ) / _TargetSize.xy;

	float	AltitudeKm = UV.y * ATMOSPHERE_THICKNESS_KM;
	float	CosThetaSun = lerp( -0.2, 1.0, UV.x );

	float	Reflectance = saturate( CosThetaSun );

	_Target0[TexelIndex] = float4( GetTransmittance( AltitudeKm, CosThetaSun ) * Reflectance, 0.0 );	// Return Sun reflectance attenuated by atmosphere as seen from given altitude
}


//////////////////////////////////////////////////////////////////////////
// 2] Pre-Computes the single scattering table
// Store separately Rayleigh and Mie contributions, WITHOUT the phase function factor (cf "Angular precision")
//
void	Integrand_Single( float _RadiusKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, float _DistanceKm, out float3 _Rayleigh, out float3 _Mie )
{
	// P0 = [0, _RadiusKm]
	// V  = [SinTheta, CosTheta]
	// L  = [SinThetaSun, CosThetaSun]
	// CosGamma = V.L
	//
	float	CurrentRadiusKm = sqrt( _RadiusKm * _RadiusKm + _DistanceKm * _DistanceKm + 2.0 * _RadiusKm * _CosThetaView * _DistanceKm );	// sqrt[ (P0 + d.V)² ]
//	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _CosGamma * _DistanceKm) / CurrentRadiusKm;	//### How do they get that???
	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _DistanceKm) / CurrentRadiusKm;				//### I can't find any gamma in there! And that seems logical because only Cos(Theta_sun) is important to guide altitude!

	CurrentRadiusKm = max( GROUND_RADIUS_KM, CurrentRadiusKm );
	if ( CurrentCosThetaSun < -sqrt( 1.0 - GROUND_RADIUS_KM * GROUND_RADIUS_KM / (CurrentRadiusKm * CurrentRadiusKm) ) )
	{	// We're hitting the ground in that direction, ignore contribution...
		_Rayleigh = 0.0;
		_Mie = 0.0;
		return;
	}

	float	StartAltitudeKm = _RadiusKm - GROUND_RADIUS_KM;
	float	CurrentAltitudeKm = CurrentRadiusKm - GROUND_RADIUS_KM;

	float3	ViewTransmittance_Source2Point = GetTransmittance( StartAltitudeKm, _CosThetaView, _DistanceKm );	// Transmittance from view point to integration point at distance
	float3	SunTransmittance_Atmosphere2Point = GetTransmittance( CurrentAltitudeKm, CurrentCosThetaSun );		// Transmittance from top of atmosphere to integration point at distance (Sun light attenuation)
	float3	Transmittance = SunTransmittance_Atmosphere2Point * ViewTransmittance_Source2Point;
	_Rayleigh = exp( -CurrentAltitudeKm / _AirParams.y ) * Transmittance;
	_Mie = exp( -CurrentAltitudeKm / _FogParams.z ) * Transmittance;
}

[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, THREADS_COUNT_Z )]
void	PreComputeInScattering_Single( CS_IN _In )
{
	const uint STEPS_COUNT = 50;

	uint	TexelIndex;
	uint3	Texel = GetTexelIndex3D( _In, TexelIndex );
	float2	UV = float2( Texel.xy ) / _TargetSize.xy;

	float	AltitudeKm;
	float4	dhdH;
	GetSliceData( Texel.z, AltitudeKm, dhdH );

	// Retrieve the 3 cosines for the current slice
	float	CosThetaView, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UV, dhdH, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Compute distance to atmosphere or ground, whichever comes first
	float4	PositionKm = float4( 0.0, AltitudeKm, 0.0, 0.0 );
	float3	View = float3( sqrt( 1.0 - CosThetaView*CosThetaView ), CosThetaView, 0.0 );
	bool	bGroundHit;
	float	TraceDistanceKm = ComputeNearestHit( PositionKm.xyz, View, ATMOSPHERE_THICKNESS_KM, bGroundHit );
	float	StepSizeKm = TraceDistanceKm / STEPS_COUNT;

	float3	PreviousRayleigh;
	float3	PreviousMie;
	Integrand_Single( GROUND_RADIUS_KM + AltitudeKm, CosThetaView, CosThetaSun, CosGamma, 0.0, PreviousRayleigh, PreviousMie );

	// Begin accumulation
	float3	Rayleigh = 0.0;
	float3	Mie = 0.0;

	float	DistanceKm = StepSizeKm;
	for ( uint i=0; i < STEPS_COUNT; i++ )
	{
		float3	CurrentRayleigh, CurrentMie;
		Integrand_Single( GROUND_RADIUS_KM + AltitudeKm, CosThetaView, CosThetaSun, CosGamma, DistanceKm, CurrentRayleigh, CurrentMie );

		Rayleigh += 0.5 * (PreviousRayleigh + CurrentRayleigh);
		Mie += 0.5 * (PreviousMie + CurrentMie);

		PreviousRayleigh = CurrentRayleigh;
		PreviousMie = CurrentMie;
		DistanceKm += StepSizeKm;
	}

	Rayleigh *= _AirParams.x * SIGMA_SCATTERING_RAYLEIGH * StepSizeKm;
	Mie *= _FogParams.x * StepSizeKm;

	_Target0[TexelIndex] = float4( Rayleigh, 0.0 );
	_Target1[TexelIndex] = float4( Mie, 0.0 );
}


//////////////////////////////////////////////////////////////////////////
// 3] Pre-Computes the delta scattering table
[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, THREADS_COUNT_Z )]
void	PreComputeInScattering_Delta( CS_IN _In )
{
	const uint	STEPS_COUNT = 16;

	const float	dPhi = PI / STEPS_COUNT;
	const float	dTheta = PI / STEPS_COUNT;

	uint	TexelIndex;
	uint3	Texel = GetTexelIndex3D( _In, TexelIndex );
	float2	UV = float2( Texel.xy ) / _TargetSize.xy;

	float	AltitudeKm;
	float4	dhdH;
	GetSliceData( Texel.z, AltitudeKm, dhdH );

	// Retrieve the 3 cosines for the current slice
	float	CosThetaView, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UV, dhdH, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Clamp values
	float	r = GROUND_RADIUS_KM + clamp( AltitudeKm, 0.0, ATMOSPHERE_THICKNESS_KM );
	CosThetaView = clamp( CosThetaView, -1.0, 1.0 );
	CosThetaSun = clamp( CosThetaSun, -1.0, 1.0 );
	float	var = sqrt( 1.0 - CosThetaView*CosThetaView ) * sqrt( 1.0 - CosThetaSun*CosThetaSun );
	CosGamma = clamp( CosGamma, CosThetaSun * CosThetaView - var, CosThetaSun * CosThetaView + var );	//### WTF?? Clarify!!!

	float	cthetaground = -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (r*r) );	// Minimum cos(theta) before we hit the ground

	float3	View = float3( sqrt( 1.0 - CosThetaView * CosThetaView ), CosThetaView, 0.0 );

	// We simply deduce Phi, the azimuth between Sun & View from the SSS formula from http://en.wikipedia.org/wiki/Solution_of_triangles#Three_sides_given
	// Phi = acos( (cos(gamma) - cos(ThetaV)*cos(ThetaS)) / (sin(ThetaV)*sin(ThetaS))
	// Next, we need the X coordinate of the Sun vector which is simply:
	// sx = cos(Phi)*sin(ThetaS) = (cos(gamma) - cos(ThetaV)*cos(ThetaS)) / sin(ThetaV)
	//
	float	sx = View.x == 0.0 ? 0.0 : (CosGamma - CosThetaSun * CosThetaView) / View.x;
	float3	Sun = float3( sx, CosThetaSun, sqrt( max( 0.0, 1.0 - sx * sx - CosThetaSun * CosThetaSun ) ) );	// Z is deduced from other coordinates

	float3	Scattering = 0.0;

	// Integral over 4.PI around x with two nested loops over w directions (theta,phi) -- Eq (7)
	for ( uint ThetaIndex=0; ThetaIndex < STEPS_COUNT; ThetaIndex++ )
	{
		float	Theta = (ThetaIndex + 0.5) * dTheta;
		float	stheta, ctheta;
		sincos( Theta, stheta, ctheta );

		float3	GroundReflectance = 0.0;
		float	Distance2Ground = -1.0;		// -1 = A hint that ground is not visible in that direction
		if ( ctheta < cthetaground )
		{	// Ground is visible in sampling direction w: compute transmittance between x and ground
			Distance2Ground = -r * ctheta - sqrt( r * r * (ctheta * ctheta - 1.0 ) + GROUND_RADIUS_KM * GROUND_RADIUS_KM);
			GroundReflectance = (_AverageGroundReflectance / PI) * GetTransmittance( 0.0, -(r * ctheta + Distance2Ground) / GROUND_RADIUS_KM, Distance2Ground );
		}

		for ( uint PhiIndex=0; PhiIndex < 2 * STEPS_COUNT; PhiIndex++ )
		{
			float	Phi = PhiIndex * dPhi;
			float	sphi, cphi;
			sincos( Phi, sphi, cphi );

			// Rebuild sampling direction & solid angle
			float3	w = float3( cphi * stheta, ctheta, sphi * stheta );
			float	dw = stheta * dTheta * dPhi;

			float3	dScattering = 0.0;

			// First term = light reflected from the ground and attenuated before reaching x
			if ( Distance2Ground > 0.0 )
			{	// Compute irradiance received at ground in direction w (if ground visible) =deltaE
				float3	GroundNormal = (float3( 0.0, r, 0.0 ) + Distance2Ground * w) / GROUND_RADIUS_KM;
				float3	GroundIrradiance = GetIrradiance( _TexIrradianceDelta, 0.0, dot( GroundNormal, Sun ) );

				dScattering += GroundReflectance * GroundIrradiance;	// (rho/PI) * single-scattered Lsun
			}

			// Second term = inscattered light, = deltaS
			float	CosPhaseAngleSun = dot( Sun, w );
			if ( _bFirstPass )
			{	// First iteration is special because Rayleigh and Mie were stored separately, without the phase functions factors; they must be reintroduced here
				float3	InScatteredRayleigh = Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;
				float3	InScatteredMie = Sample4DScatteringTable( _TexScatteringDelta_Mie, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;
				dScattering += InScatteredRayleigh * PhaseFunctionRayleigh( CosPhaseAngleSun ) + InScatteredMie * PhaseFunctionMie( CosPhaseAngleSun );
			}
			else
			{	// Next pass only uses the Rayleigh table containing both Rayleigh & Mie
				dScattering += Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;
			}

			float	CosPhaseAngleView = dot( View, w );
			float	PhaseRayleigh = PhaseFunctionRayleigh( CosPhaseAngleView );
			float	PhaseMie = PhaseFunctionMie( CosPhaseAngleView );

			// Light coming from direction w and scattered in view direction
			// = light arriving at x from direction w (dScattering) * SUM( scattering coefficient * phaseFunction )
			// see Eq (7)
			Scattering += dScattering * (_AirParams.x * SIGMA_SCATTERING_RAYLEIGH * exp( -AltitudeKm / _AirParams.y ) * PhaseRayleigh + _FogParams.x * exp( -AltitudeKm / _FogParams.z ) * PhaseMie) * dw;
		}
	}

	_Target0[TexelIndex] = float4( Scattering, 0.0 );	// output In-Scattering = J[T.alpha/PI.deltaE + deltaS] (line 7 in algorithm 4.1)
}

//////////////////////////////////////////////////////////////////////////
// 4] Pre-Computes the irradiance table accounting for multiple scattering
[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, 1 )]
void	PreComputeIrradiance_Delta( CS_IN _In )
{
	const uint	STEPS_COUNT = 32;

	const float	dPhi = PI / STEPS_COUNT;
	const float	dTheta = PI / STEPS_COUNT;

	uint	TexelIndex;
	uint2	Texel = GetTexelIndex2D( _In, TexelIndex );
	float2	UV = float2( Texel ) / _TargetSize.xy;

	float	AltitudeKm = UV.y * ATMOSPHERE_THICKNESS_KM;
	float	CosThetaSun = lerp( -0.2, 1.0, UV.x );

	float3	Sun = float3( sqrt( 1.0 - saturate( CosThetaSun * CosThetaSun ) ), CosThetaSun, 0.0 );

	// Integral over 2.PI around x with two nested loops over w directions (theta,phi) -- Eq (15)
	float3	Result = 0.0;
	for ( uint PhiIndex=0; PhiIndex < 2 * STEPS_COUNT; PhiIndex++ )
	{
		float	Phi = PhiIndex * dPhi;
		float	sphi, cphi;
		sincos( Phi, sphi, cphi );

		for ( uint ThetaIndex=0; ThetaIndex < STEPS_COUNT / 2; ThetaIndex++ )
		{
			float	Theta = (ThetaIndex + 0.5) * dTheta;
			float	stheta, ctheta;
			sincos( Theta, stheta, ctheta );

			// Rebuild sampling direction & solid angle
			float3	w = float3( cphi * stheta, ctheta, sphi * stheta );
			float	dw = stheta * dTheta * dPhi;

			float	CosPhaseAngleSun = dot( Sun, w );
			float3	InScattering = 0.0;
			if ( _bFirstPass )
			{	// First iteration is special because Rayleigh and Mie were stored separately, without the phase functions factors; they must be reintroduced here
				float3	InScatteredRayleigh = Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;
				float3	InScatteredMie = Sample4DScatteringTable( _TexScatteringDelta_Mie, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;
				InScattering = PhaseFunctionRayleigh( CosPhaseAngleSun ) * InScatteredRayleigh + PhaseFunctionMie( CosPhaseAngleSun ) * InScatteredMie;
			}
			else
				InScattering = Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;

			Result += InScattering * w.y * dw;	// InScattering * (w.n) * dw
		}
	}

	_Target0[TexelIndex] = float4( Result, 0.0 );
}

//////////////////////////////////////////////////////////////////////////
// Pre-Computes the multiple scattering table
float3	Integrand_Multiple( float _RadiusKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, float _DistanceKm )
{
	// P0 = [0, _RadiusKm]
	// V  = [SinTheta, CosTheta]
	// L  = [SinThetaSun, CosThetaSun]
	// CosGamma = V.L
	//
	float	CurrentRadiusKm = sqrt( _RadiusKm * _RadiusKm + _DistanceKm * _DistanceKm + 2.0 * _RadiusKm * _CosThetaView * _DistanceKm );	// sqrt[ (P0 + d.V)² ]
//	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _CosGamma * _DistanceKm) / CurrentRadiusKm;	//### How do they get that???
	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _DistanceKm) / CurrentRadiusKm;				//### I can't find any gamma in there! And that seems logical because only Cos(Theta_sun) is important to guide altitude!
	float	CurrentCosThetaView = (_RadiusKm * _CosThetaView + _DistanceKm) / CurrentRadiusKm;

	float	CurrentAltitudeKm = CurrentRadiusKm - GROUND_RADIUS_KM;

	return  GetTransmittance( CurrentAltitudeKm, _CosThetaView, _DistanceKm ) * Sample4DScatteringTable( _TexScatteringDelta, CurrentAltitudeKm, CurrentCosThetaView, CurrentCosThetaSun, _CosGamma ).xyz;
}

[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, THREADS_COUNT_Z )]
void	PreComputeInScattering_Multiple( CS_IN _In )
{
	const uint STEPS_COUNT = 50;

	uint	TexelIndex;
	uint3	Texel = GetTexelIndex3D( _In, TexelIndex );
	float2	UV = float2( Texel.xy ) / _TargetSize.xy;

	float	AltitudeKm;
	float4	dhdH;
	GetSliceData( Texel.z, AltitudeKm, dhdH );

	// Retrieve the 3 cosines for the current slice
	float	CosThetaView, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UV, dhdH, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Compute distance to atmosphere or ground, whichever comes first
	float4	PositionKm = float4( 0.0, AltitudeKm, 0.0, 0.0 );
	float3	View = float3( sqrt( 1.0 - CosThetaView*CosThetaView ), CosThetaView, 0.0 );
	bool	bGroundHit;
	float	TraceDistanceKm = ComputeNearestHit( PositionKm.xyz, View, ATMOSPHERE_THICKNESS_KM, bGroundHit );
	float	StepSizeKm = TraceDistanceKm / STEPS_COUNT;

	float3	Result = 0.0;
	float3	PreviousScatteringRayleighMie = Integrand_Multiple( GROUND_RADIUS_KM + AltitudeKm, CosThetaView, CosThetaSun, CosGamma, 0.0 );

	float	DistanceKm = StepSizeKm;
	for ( uint i=0; i < STEPS_COUNT; i++ )
	{
		float3	ScatteringRayleighMie = Integrand_Multiple( GROUND_RADIUS_KM + AltitudeKm, CosThetaView, CosThetaSun, CosGamma, DistanceKm );
		Result += 0.5 * (PreviousScatteringRayleighMie + ScatteringRayleighMie);

		PreviousScatteringRayleighMie = ScatteringRayleighMie;
		DistanceKm += StepSizeKm;
	}

	_Target0[TexelIndex] = float4( Result * StepSizeKm, 0.0 );
}

//////////////////////////////////////////////////////////////////////////
// Merges single-scattering tables for Rayleigh & Mie into the single initial scattering table
//	_Input0 = _TexScatteringDelta_Rayleigh
//	_Input1 = _TexScatteringDelta_Mie
//
[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, THREADS_COUNT_Z )]
void	MergeInitialScattering( CS_IN _In )
{
// 	float3	UVW = float3( _In.__Position.xy * _dUVW.xy, (_In.SliceIndex + 0.5) / RESOLUTION_ALTITUDE );
// 	float3	Rayleigh = _TexScatteringDelta_Rayleigh.SampleLevel( PointClamp, UVW, 0.0 ).xyz;
// 	float	Mie = _TexScatteringDelta_Mie.SampleLevel( PointClamp, UVW, 0.0 ).x;
// 
// 	return float4( Rayleigh, Mie ); // Store only red component of single Mie scattering (cf. "Angular precision")

	uint	TexelIndex;
	uint3	Texel = GetTexelIndex3D( _In, TexelIndex );

	_Target0[TexelIndex] = float4( _Input0[TexelIndex].xyz, _Input1[TexelIndex].x );
}

//////////////////////////////////////////////////////////////////////////
// Accumulates delta in-scattering into the final scattering table
//	_Input0 = _TexScatteringDelta_Rayleigh
//
[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, THREADS_COUNT_Z )]
void	AccumulateInScattering( CS_IN _In )
{
// 	float3	UVW = float3( _In.__Position.xy * _dUVW.xy, (_In.SliceIndex + 0.5) / RESOLUTION_ALTITUDE );
// 
// 	// We need to divide in-scattering by the Rayleigh phase function so we need CosGamma
// 	float	AltitudeKm;
// 	float4	dhdH;
// 	GetSliceData( _In.SliceIndex, AltitudeKm, dhdH );
// 
// 	float	CosThetaView, CosThetaSun, CosGamma;
// 	GetAnglesFrom4D( UVW.xy, dhdH, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );
// 
// 	// Get rayleigh scattering
// 	float3	Rayleigh = _TexScatteringDelta_Rayleigh.SampleLevel( PointClamp, UVW, 0.0 ).xyz;
// 			Rayleigh /= PhaseFunctionRayleigh( CosGamma );
// 
// 	return float4( Rayleigh, 0.0 );

	uint	TexelIndex;
	uint3	Texel = GetTexelIndex3D( _In, TexelIndex );
	float2	UV = float2( Texel.xy ) / _TargetSize.xy;

 	// We need to divide in-scattering by the Rayleigh phase function so we need CosGamma
	float	AltitudeKm;
	float4	dhdH;
	GetSliceData( Texel.z, AltitudeKm, dhdH );

	// Retrieve the 3 cosines for the current slice
	float	CosThetaView, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UV, dhdH, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Get Rayleigh scattering
	float3	Rayleigh = _Input0[TexelIndex].xyz;
			Rayleigh /= PhaseFunctionRayleigh( CosGamma );

	_Target0[TexelIndex] = float4( Rayleigh, 0.0 );
}

//////////////////////////////////////////////////////////////////////////
// Accumulates irradiance into the final irradiance table
//	_Input0 = _TexIrradianceDelta
//
[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, 1 )]
void	AccumulateIrradiance( CS_IN _In )
{
// 	float2	UV = _In.__Position.xy * _dUVW.xy;
// 	return _TexIrradianceDelta.SampleLevel( PointClamp, UV, 0.0 ).xyz;

	uint	TexelIndex;
	uint2	Texel = GetTexelIndex2D( _In, TexelIndex );

	_Target0[TexelIndex] += _Input0[TexelIndex];
}
