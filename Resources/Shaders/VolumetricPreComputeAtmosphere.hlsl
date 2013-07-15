//////////////////////////////////////////////////////////////////////////
// This shader pre-computes the various tables required by the atmospheric rendering
// It's an implementation of http://www-ljk.imag.fr/Publications/Basilic/com.lmc.publi.PUBLI_Article@11e7cdda2f7_f64b69/article.pdf
//
#include "Inc/Global.hlsl"
#include "Inc/Atmosphere.hlsl"

Texture2D		_TexDebug0	: register(t10);
Texture2D		_TexDebug1	: register(t11);
Texture2DArray	_TexDebug2	: register(t12);

Texture2D		_TexIrradianceDelta : register(t13);			// deltaE
Texture3D		_TexScatteringDelta_Rayleigh : register(t14);	// deltaSR
Texture3D		_TexScatteringDelta_Mie : register(t15);		// deltaSM
Texture3D		_TexScatteringDelta : register(t16);			// deltaJ

//[
cbuffer	cbObject	: register( b10 )
{
	float4		_dUVW;
	bool		_bFirstPass;
	float		_AverageGroundReflectance;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
	uint	InstanceID	: SV_INSTANCEID;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	uint	SliceIndex	: SV_RENDERTARGETARRAYINDEX;
};

struct	PS_OUT
{
	float3	Color0		: SV_TARGET0;
	float3	Color1		: SV_TARGET1;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

[maxvertexcount(3)]
void	GS( triangle VS_IN _In[3], inout TriangleStream<PS_IN> _Stream )
{
	PS_IN	Out;
	Out.SliceIndex = _In[0].InstanceID;
	Out.__Position = _In[0].__Position;
	_Stream.Append( Out );
	Out.__Position = _In[1].__Position;
	_Stream.Append( Out );
	Out.__Position = _In[2].__Position;
	_Stream.Append( Out );
}


//////////////////////////////////////////////////////////////////////////
// Helpers

// Gets the altitude and scales for a 3D target rendering depending on the target slice
void	GetLayerData( uint _SliceIndex, out float _AltitudeKm, out float4 _dhdH )
{
    float RadiusKm = _SliceIndex / (RESOLUTION_ALTITUDE - 1.0);

    RadiusKm = RadiusKm * RadiusKm;
    RadiusKm = sqrt( lerp( GROUND_RADIUS_KM * GROUND_RADIUS_KM, ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM, RadiusKm ) );	// Radius grows quadratically to have more precision near the ground
	if ( _SliceIndex == 0 )
		RadiusKm += 0.01;	// Never completely ground
	else if ( _SliceIndex == uint(RESOLUTION_ALTITUDE)-1 )
		RadiusKm -= 0.001;	// Never completely top of atmosphere

    float	dmin = ATMOSPHERE_RADIUS_KM - RadiusKm;
    float	dmax = sqrt( RadiusKm * RadiusKm - GROUND_RADIUS_KM * GROUND_RADIUS_KM ) + sqrt( ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - GROUND_RADIUS_KM * GROUND_RADIUS_KM );
    float	dminp = RadiusKm - GROUND_RADIUS_KM;
    float	dmaxp = sqrt( RadiusKm * RadiusKm - GROUND_RADIUS_KM * GROUND_RADIUS_KM );

	_dhdH = float4( dmin, dmax, dminp, dmaxp );
	_AltitudeKm = RadiusKm - GROUND_RADIUS_KM;
}

void GetAnglesFrom4D( float2 _UV, float3 _dUV, float _AltitudeKm, float4 dhdH, out float _CosThetaView, out float _CosThetaSun, out float _CosGamma )
{
	_UV -= 0.5 * _dUV.xy;	// Remove the half pixel offset

#ifdef INSCATTER_NON_LINEAR_VIEW

#ifdef INSCATTER_NON_LINEAR_VIEW_POM

	_CosThetaView = abs( 2.0 * _UV.y - 1.0 );
	_CosThetaView *= (_UV.y < 0.5 ? -1.0 : +1.0) * _CosThetaView;	// Squared progression for more precision near horizon

#else	// !POM?

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


//////////////////////////////////////////////////////////////////////////
// Pre-Computes the transmittance table for all possible altitudes and zenith angles
float	ComputeOpticalDepth( float _AltitudeKm, float _CosTheta, const float _Href, uniform uint _StepsCount=500 )
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
	float4	StepKm = (TraceDistanceKm / _StepsCount) * float4( View, 1.0 );

	float		PreviousAltitudeKm = _AltitudeKm;
	for ( uint i=0; i < _StepsCount; i++ )
	{
		PositionKm += StepKm;
		_AltitudeKm = length( PositionKm.xyz - EARTH_CENTER_KM ) - GROUND_RADIUS_KM;
		Result += exp( (PreviousAltitudeKm + _AltitudeKm) * (-0.5 / _Href) );	// Gives the integral of a linear interpolation in altitude
		PreviousAltitudeKm = _AltitudeKm;
	}

	return Result * StepKm.w;
}

float4	PreComputeTransmittance( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUVW.xy;

	float	AltitudeKm = UV.y*UV.y * ATMOSPHERE_THICKNESS_KM;				// Grow quadratically to have more precision near the ground
	float	CosTheta = -0.15 + tan( 1.5 * UV.x ) / tan(1.5) * (1.0 + 0.15);	// Grow tangentially to have more precision horizontally
//	float	CosTheta = lerp( -0.15, 1.0, UV.x );							// Grow linearly

	float3	OpticalDepth = _AirParams.x * SIGMA_SCATTERING_RAYLEIGH * ComputeOpticalDepth( AltitudeKm, CosTheta, _AirParams.y ) + _FogParams.y * ComputeOpticalDepth( AltitudeKm, CosTheta, _FogParams.z );

//	return float4( exp( -OpticalDepth ), 0.0 );
	return float4( min( 1e5, OpticalDepth ), 0.0 );		// We directly store optical depth otherwise we lose too much precision using a division!
}

//////////////////////////////////////////////////////////////////////////
// Pre-Computes the ground irradiance table accounting for single scattering only
float4	PreComputeIrradiance_Single( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUVW.xy;

	float	AltitudeKm = UV.y * ATMOSPHERE_THICKNESS_KM;
	float	CosThetaSun = lerp( -0.2, 1.0, UV.x - 0.5 * _dUVW.x );

	float	Reflectance = saturate( CosThetaSun );

    return float4( GetTransmittance( AltitudeKm, CosThetaSun ) * Reflectance, 0.0 );	// Return Sun reflectance attenuated by atmosphere as seen from given altitude
}


//////////////////////////////////////////////////////////////////////////
// Pre-Computes the single scattering table
void	Integrand_Single( float _RadiusKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, float _DistanceKm, out float3 _Rayleigh, out float3 _Mie )
{
	// P0 = [0, _RadiusKm]
	// V  = [SinTheta, CosTheta]
	// L  = [SinThetaSun, CosThetaSun]
	// CosGamma = V.L
	//
	float	CurrentRadiusKm = sqrt( _RadiusKm * _RadiusKm + _DistanceKm * _DistanceKm + 2.0 * _RadiusKm * _CosThetaView * _DistanceKm );	// sqrt[ (P0 + d.V)² ]
    float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _CosGamma * _DistanceKm) / CurrentRadiusKm;	//### How do they get that???

    CurrentRadiusKm = max( GROUND_RADIUS_KM, CurrentRadiusKm );
    if ( CurrentCosThetaSun < -sqrt( 1.0 - GROUND_RADIUS_KM * GROUND_RADIUS_KM / (CurrentRadiusKm * CurrentRadiusKm) ) )
	{	// We're hitting the ground in that direction, ignore contribution...
		_Rayleigh = 0.0;
		_Mie = 0.0;
		return;
	}

	float	StartAltitudeKm = _RadiusKm - GROUND_RADIUS_KM;
	float	CurrentAltitudeKm = CurrentRadiusKm - GROUND_RADIUS_KM;

	float3	ViewTransmittanceSource2Point = GetTransmittance( StartAltitudeKm, _CosThetaView, _DistanceKm );	// Transmittance from view point to integration point at distance
	float3	SunTransmittanceAtmosphere2Point = GetTransmittance( CurrentAltitudeKm, CurrentCosThetaSun );		// Transmittance from top of atmosphere to integration point at distance (Sun light attenuation)
    float3	Transmittance = SunTransmittanceAtmosphere2Point * ViewTransmittanceSource2Point;
    _Rayleigh = exp( -CurrentAltitudeKm / _AirParams.y ) * Transmittance;
    _Mie = exp( -CurrentAltitudeKm / _FogParams.z ) * Transmittance;
}

void	InScatter_Single( float _AltitudeKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, out float3 _Rayleigh, out float3 _Mie, uniform uint _StepsCount=50 )
{
	// Compute distance to atmosphere or ground, whichever comes first
	float4	PositionKm = float4( 0.0, _AltitudeKm, 0.0, 0.0 );
	float3	View = float3( sqrt( 1.0 - _CosThetaView*_CosThetaView ), _CosThetaView, 0.0 );
	bool	bGroundHit;
	float	TraceDistanceKm = ComputeNearestHit( PositionKm.xyz, View, ATMOSPHERE_THICKNESS_KM, bGroundHit );
	float	StepSizeKm = TraceDistanceKm / _StepsCount;

	float3	PreviousRayleigh;
	float3	PreviousMie;
	Integrand_Single( GROUND_RADIUS_KM + _AltitudeKm, _CosThetaView, _CosThetaSun, _CosGamma, 0.0, PreviousRayleigh, PreviousMie );

	_Rayleigh = 0.0;
	_Mie = 0.0;

	float	DistanceKm = StepSizeKm;
	for ( uint i=0; i < _StepsCount; i++ )
	{
		float3	CurrentRayleigh, CurrentMie;
		Integrand_Single( GROUND_RADIUS_KM + _AltitudeKm, _CosThetaView, _CosThetaSun, _CosGamma, DistanceKm, CurrentRayleigh, CurrentMie );

		_Rayleigh += 0.5 * (PreviousRayleigh + CurrentRayleigh);
		_Mie += 0.5 * (PreviousMie + CurrentMie);

		PreviousRayleigh = CurrentRayleigh;
		PreviousMie = CurrentMie;
		DistanceKm += StepSizeKm;
	}

	_Rayleigh *= _AirParams.x * SIGMA_SCATTERING_RAYLEIGH * StepSizeKm;
	_Mie *= _FogParams.x * StepSizeKm;
}

PS_OUT	PreComputeInScattering_Single( PS_IN _In )
{
	float2	UV = _In.__Position.xy * _dUVW.xy;

	float	AltitudeKm;
	float4	dhdH;
	GetLayerData( _In.SliceIndex, AltitudeKm, dhdH );

	// Retrieve the 3 angle cosines for the current slice
	float	CosTheta, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UV, _dUVW.xyw, AltitudeKm, dhdH, CosTheta, CosThetaSun, CosGamma );

	// Compute scattering
    // Store separately Rayleigh and Mie contributions, WITHOUT the phase function factor (cf "Angular precision")
	PS_OUT	Out;
	InScatter_Single( AltitudeKm, CosTheta, CosThetaSun, CosGamma, Out.Color0, Out.Color1 );

	return Out;
}

//////////////////////////////////////////////////////////////////////////
// Pre-Computes the delta scattering table
float3	InScatter_Delta( float _AltitudeKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, const int _StepsCount=16 )
{
	const float	dPhi = PI / _StepsCount;
	const float	dTheta = PI / _StepsCount;

	float	r = GROUND_RADIUS_KM + clamp( _AltitudeKm, 0.0, ATMOSPHERE_THICKNESS_KM );
	_CosThetaView = clamp( _CosThetaView, -1.0, 1.0 );
	_CosThetaSun = clamp( _CosThetaSun, -1.0, 1.0 );

	float	var = sqrt( 1.0 - _CosThetaView*_CosThetaView ) * sqrt( 1.0 - _CosThetaSun*_CosThetaSun );
	_CosGamma = clamp( _CosGamma, _CosThetaSun * _CosThetaView - var, _CosThetaSun * _CosThetaView + var );	//### WTF?? Clarify!!!

	float	cthetaground = -sqrt( 1.0 - (GROUND_RADIUS_KM / r) * (GROUND_RADIUS_KM / r) );	// Minimum cos(theta) before we hit the ground
 
	float3	View = float3( sqrt( 1.0 - _CosThetaView * _CosThetaView ), _CosThetaView, 0.0 );
	float	sx = View.x == 0.0 ? 0.0 : (_CosGamma - _CosThetaSun * _CosThetaView) / View.x;
	float3	Sun = float3( sx, _CosThetaSun, sqrt( max( 0.0, 1.0 - sx * sx - _CosThetaSun * _CosThetaSun ) ) );	// Sun from View + Gamma?

	float3	Scattering = 0.0;

	// Integral over 4.PI around x with two nested loops over w directions (theta,phi) -- Eq (7)
	for ( int ThetaIndex=0; ThetaIndex < _StepsCount; ThetaIndex++ )
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

		for ( int PhiIndex=0; PhiIndex < 2 * _StepsCount; PhiIndex++ )
		{
			float	Phi = (PhiIndex + 0.5) * dPhi;
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

				dScattering += GroundReflectance * GroundIrradiance;	// = T.alpha/PI.deltaE
			}

			// Second term = inscattered light, = deltaS
			float	CosPhaseAngleSun = dot( Sun, w );
			if ( _bFirstPass )
			{	// First iteration is special because Rayleigh and Mie were stored separately, without the phase functions factors; they must be reintroduced here
				float3	InScatteredRayleigh = Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, _AltitudeKm, w.y, _CosThetaSun, CosPhaseAngleSun ).xyz;
				float3	InScatteredMie = Sample4DScatteringTable( _TexScatteringDelta_Mie, _AltitudeKm, w.y, _CosThetaSun, CosPhaseAngleSun ).xyz;
				float	PhaseRayleigh = PhaseFunctionRayleigh( CosPhaseAngleSun );
				float	PhaseMie = PhaseFunctionMie( CosPhaseAngleSun );
				dScattering += PhaseRayleigh * InScatteredRayleigh + PhaseMie * InScatteredMie;
			}
			else	// Next pass only use the Rayleigh table
				dScattering += Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, _AltitudeKm, w.y, _CosThetaSun, CosPhaseAngleSun ).xyz;

			float	CosPhaseAngleView = dot( View, w );
			float	PhaseRayleigh = PhaseFunctionRayleigh( CosPhaseAngleView );
			float	PhaseMie = PhaseFunctionMie( CosPhaseAngleView );

			// Light coming from direction w and scattered in view direction
			// = light arriving at x from direction w (dScattering) * SUM( scattering coefficient * phaseFunction )
			// see Eq (7)
			Scattering += dScattering * (_AirParams.x * SIGMA_SCATTERING_RAYLEIGH * exp( -_AltitudeKm / _AirParams.y ) * PhaseRayleigh + _FogParams.x * exp( -_AltitudeKm / _FogParams.z ) * PhaseMie) * dw;
		}
	}

	return Scattering;	// output In-Scattering = J[T.alpha/PI.deltaE + deltaS] (line 7 in algorithm 4.1)
}

float3	PreComputeInScattering_Delta( PS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUVW.xy;

	float	AltitudeKm;
	float4	dhdH;
	GetLayerData( _In.SliceIndex, AltitudeKm, dhdH );

	// Retrieve the 3 angle cosines for the current slice
	float	CosTheta, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UV, _dUVW.xyw, AltitudeKm, dhdH, CosTheta, CosThetaSun, CosGamma );

	return InScatter_Delta( AltitudeKm, CosTheta, CosThetaSun, CosGamma );
}

//////////////////////////////////////////////////////////////////////////
// Pre-Computes the irradiance table accounting for multiple scattering
//float3	PreComputeIrradiance_Delta( VS_IN _In, uniform uint _StepsCount=32 ) : SV_TARGET0	// warning on usage about a missing CB at slot 2... ?
float3	PreComputeIrradiance_Delta( VS_IN _In ) : SV_TARGET0
{
const uint _StepsCount=32;

	const float	dPhi = PI / _StepsCount;
	const float	dTheta = PI / _StepsCount;

	float2	UV = _In.__Position.xy * _dUVW.xy;

	float	AltitudeKm = (UV.y - 0.5 * _dUVW.y) * ATMOSPHERE_THICKNESS_KM;
	float	CosThetaSun = lerp( -0.2, 1.0, UV.x - 0.5 * _dUVW.x );

	float3	Sun = float3( sqrt( 1.0 - saturate( CosThetaSun * CosThetaSun ) ), CosThetaSun, 0.0 );

	// Integral over 2.PI around x with two nested loops over w directions (theta,phi) -- Eq (15)
	float3	Result = 0.0;
	for ( uint PhiIndex=0; PhiIndex < 2 * _StepsCount; PhiIndex++ )
	{
		float	Phi = (PhiIndex + 0.5) * dPhi;
		float	sphi, cphi;
		sincos( Phi, sphi, cphi );

		for ( uint ThetaIndex=0; ThetaIndex < _StepsCount / 2; ThetaIndex++ )
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
				float	PhaseRayleigh = PhaseFunctionRayleigh( CosPhaseAngleSun );
				float	PhaseMie = PhaseFunctionMie( CosPhaseAngleSun );
				InScattering = InScatteredRayleigh * PhaseRayleigh + PhaseMie * InScatteredMie;
			}
			else
				InScattering = Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;

			Result += InScattering * w.y * dw;	// InScattering * (w.n) * dw
		}
	}

	return Result;
}

//////////////////////////////////////////////////////////////////////////
// Pre-Computes the multiple scattering table
float3	Integrand_Multiple( float _AltitudeKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, float _DistanceKm )
{
	float	StartRadiusKm = GROUND_RADIUS_KM + _AltitudeKm;
	float	CurrentRadiusKm = sqrt( StartRadiusKm * StartRadiusKm + _DistanceKm * _DistanceKm + 2.0 * StartRadiusKm * _CosThetaView * _DistanceKm );
	float	CurrentCosThetaView = (StartRadiusKm * _CosThetaView + _DistanceKm) / CurrentRadiusKm;
	float	CurrentCosThetaSun = (_CosGamma * _DistanceKm + _CosThetaSun * StartRadiusKm) / CurrentRadiusKm;	//### How do they get that???

	float	CurrentAltitudeKm = CurrentRadiusKm - GROUND_RADIUS_KM;

	return  GetTransmittance( _AltitudeKm, _CosThetaView, _DistanceKm ) * Sample4DScatteringTable( _TexScatteringDelta, CurrentAltitudeKm, CurrentCosThetaView, CurrentCosThetaSun, _CosGamma ).xyz;
}

float3 InScatter_Multiple( float _AltitudeKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, uniform uint _StepsCount=50 )
{
	// Compute distance to atmosphere or ground, whichever comes first
	float4	PositionKm = float4( 0.0, _AltitudeKm, 0.0, 0.0 );
	float3	View = float3( sqrt( 1.0 - _CosThetaView*_CosThetaView ), _CosThetaView, 0.0 );
	bool	bGroundHit;
	float	TraceDistanceKm = ComputeNearestHit( PositionKm.xyz, View, ATMOSPHERE_THICKNESS_KM, bGroundHit );
	float	StepSizeKm = TraceDistanceKm / _StepsCount;

	float3	Result = 0.0;
	float3	PreviousScatteringRayleighMie = Integrand_Multiple( _AltitudeKm, _CosThetaView, _CosThetaSun, _CosGamma, 0.0 );

	float	DistanceKm = StepSizeKm;
	for ( uint i=0; i < _StepsCount; i++ )
	{
		float3 ScatteringRayleighMie = Integrand_Multiple( _AltitudeKm, _CosThetaView, _CosThetaSun, _CosGamma, DistanceKm );
		Result += 0.5 * (PreviousScatteringRayleighMie + ScatteringRayleighMie);

		PreviousScatteringRayleighMie = ScatteringRayleighMie;
		DistanceKm += StepSizeKm;
	}

	return Result * StepSizeKm;
}

float3	PreComputeInScattering_Multiple( PS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUVW.xy;

	float	AltitudeKm;
	float4	dhdH;
	GetLayerData( _In.SliceIndex, AltitudeKm, dhdH );

	// Retrieve the 3 angle cosines for the current slice
	float	CosTheta, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UV, _dUVW.xyw, AltitudeKm, dhdH, CosTheta, CosThetaSun, CosGamma );

	return InScatter_Multiple( AltitudeKm, CosTheta, CosThetaSun, CosGamma );
}

//////////////////////////////////////////////////////////////////////////
// Merges single-scattering tables for Rayleigh & Mie into the single initial scattering table
float4	MergeInitialScattering( PS_IN _In ) : SV_TARGET0
{
	float3	UVW = float3( _In.__Position.xy * _dUVW.xy, (_In.SliceIndex + 0.5) / RESOLUTION_ALTITUDE );
	float3	Rayleigh = _TexScatteringDelta_Rayleigh.SampleLevel( PointClamp, UVW, 0.0 ).xyz;
	float	Mie = _TexScatteringDelta_Mie.SampleLevel( PointClamp, UVW, 0.0 ).x;

	return float4( Rayleigh, Mie ); // Store only red component of single Mie scattering (cf. "Angular precision")
}

//////////////////////////////////////////////////////////////////////////
// Accumulates delta in-scattering into the final scattering table
float4	AccumulateInScattering( PS_IN _In ) : SV_TARGET0
{
	float3	UVW = float3( _In.__Position.xy * _dUVW.xy, (_In.SliceIndex + 0.5) / RESOLUTION_ALTITUDE );

	// We need to divide in-scattering by the Rayleigh phase function so we need CosGamma
	float	AltitudeKm;
	float4	dhdH;
	GetLayerData( _In.SliceIndex, AltitudeKm, dhdH );

	float	CosTheta, CosThetaSun, CosGamma;
	GetAnglesFrom4D( UVW.xy, _dUVW.xyw, AltitudeKm, dhdH, CosTheta, CosThetaSun, CosGamma );

	// Get rayleigh scattering
	float3	Rayleigh = _TexScatteringDelta_Rayleigh.SampleLevel( PointClamp, UVW, 0.0 ).xyz;
			Rayleigh /= PhaseFunctionRayleigh( CosGamma );

	return float4( Rayleigh, 0.0 );
}

//////////////////////////////////////////////////////////////////////////
// Accumulates irradiance into the final irradiance table
float3	AccumulateIrradiance( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUVW.xy;
	return _TexIrradianceDelta.SampleLevel( PointClamp, UV, 0.0 ).xyz;
}
