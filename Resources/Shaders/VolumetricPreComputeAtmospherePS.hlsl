//////////////////////////////////////////////////////////////////////////
// This shader pre-computes the various tables required by the atmospheric rendering
// It's an implementation of http://www-ljk.imag.fr/Publications/Basilic/com.lmc.publi.PUBLI_Article@11e7cdda2f7_f64b69/article.pdf
//
#include "Inc/Global.hlsl"
#include "Inc/Atmosphere.hlsl"

#define	THREADS_COUNT_X	16	// Amount of thread per group
#define	THREADS_COUNT_Y	16
#define	THREADS_COUNT_Z	4	// 4 as the product of all thread counts cannot exceed 1024!

cbuffer	cbCompute	: register( b10 )
{
	uint3	_TargetSize;	// Final render target size (2D or 3D)
	uint3	_GroupsCount;	// Amount of render groups (2D or 3D) for a single pass
	uint3	_PassIndex;		// Index of the X,Y,Z pass (each pass computes THREAD_COUNT_X*THREAD_COUNT_Y*THREAD_COUNT_Z texels)

	bool	_bFirstPass;				// True if we're computing the first pass that reads single-scattering for Rayleigh & Mie from 2 separate tables
	float	_AverageGroundReflectance;
};

Texture2D	_TexIrradianceDelta : register(t10);			// deltaE
Texture3D	_TexScatteringDelta_Rayleigh : register(t11);	// deltaSR
Texture3D	_TexScatteringDelta_Mie : register(t12);		// deltaSM
Texture3D	_TexScatteringDelta : register(t13);			// deltaJ


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

// Computes the flattened texel informations
uint3	GetTexelInfos( PS_IN _In )
{
	uint2	PixelPosition = uint2( floor( _In.__Position.xy ) );

	uint	TexelX = (_GroupsCount.x * THREADS_COUNT_X) * _PassIndex.x + PixelPosition.x;
	uint	TexelY = (_GroupsCount.y * THREADS_COUNT_Y) * _PassIndex.y + PixelPosition.y;
	uint	TexelZ = (_GroupsCount.z * THREADS_COUNT_Z) * _PassIndex.z + _In.SliceIndex;
	return uint3( TexelX, TexelY, TexelZ );
}

// Gets the altitude, zenith/view angle (cos theta), zenith/Sun angle (cos theta Sun) and azimuth Sun angle (cos gamma) for a 3D target rendering depending on the target slice
//
void	GetSliceData( uint3 _Texel, out float _AltitudeKm, out float _CosThetaView, out float _CosThetaSun, out float _CosGamma )
{
    float RadiusKm = _Texel.z / (RESOLUTION_ALTITUDE - 1.0);

    RadiusKm = RadiusKm * RadiusKm;
    RadiusKm = sqrt( lerp( GROUND_RADIUS_KM * GROUND_RADIUS_KM, ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM, RadiusKm ) );	// Radius grows quadratically to have more precision near the ground
	if ( _Texel.z == 0 )
		RadiusKm += 0.01;	// Never completely ground
	else if ( _Texel.z == uint(RESOLUTION_ALTITUDE)-1 )
		RadiusKm -= 0.001;	// Never completely top of atmosphere

	_AltitudeKm = RadiusKm - GROUND_RADIUS_KM;

	// Compute the view angles
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
	//	reversed in a manner that transition from atmosphere to ground makes V -> 0
	//	and transition from ground to atmosphere makes V -> 1, the CLAMP address mode prevents
	//	the unwanted bilinear interpolation that would occur otherwise...
	//
	// And this explains all the mysterious bullshit terms and optimizations the authors wrote
	//	and that was so difficult to grasp looking solely at the code...
	//
	// =====================================================
	// Here, we want to retrieve cos(theta) from the distance ratio so we need to invert the computation
	// When we're hitting the ground, we can write ||P||² = Rg² at distance d
	// First we retrieve d from V by writing d = V * d_h = V * sqrt( r² - Rg² )
	// Second, ||P||² = d²*sin²(theta) + (r + d*cos(theta))²
	//	= d²(1-cos²(theta)) + r² + 2*r*d*cos(theta) + d²*cos²(theta)
	//	= d² + r² + 2*r*d*cos(theta) = Rg²			(3)
	//
	// So we finally get:
	//	cos(theta) = (Rg² - r² - d²) / (2*r*d)
	//
	//
	// Identically, when we're hitting the atmosphere we can write ||P||² = Rt² at distance d
	// We retrieve d from V by writing d = V * d_H = V * [sqrt( r² - Rg² ) + sqrt( Rt² - Rg² )]
	// Re-using (3) we get:
	//	d² + r² + 2*r*d*cos(theta) = Rt²
	//
	// And finally we get:
	//	cos(Theta) = (Rt² - r² - d²) / (2*r*d)
	//
	float r = RadiusKm;

	if ( _Texel.y < uint( RESOLUTION_COS_THETA) / 2 )
	{	// Viewing toward the ground
		float	d_ground = r - GROUND_RADIUS_KM;										// Distance to the ground (the minimum distance we can see viewing straight down)
		float	d_horizon = 0.999 * sqrt( r*r - GROUND_RADIUS_KM*GROUND_RADIUS_KM );	// Distance to the horizon (the maximum distance we can see viewing down)
		float	d = 1.0 - 2.0 * _Texel.y / RESOLUTION_COS_THETA;						// 1 at texel 0, ~0 at RESOLUTION/2-1
				d = lerp( d_ground, d_horizon, d );										// d_horizon at texel 0, ~d_ground at RESOLUTION/2-1

		_CosThetaView = (GROUND_RADIUS_KM * GROUND_RADIUS_KM - r * r - d * d) / (2.0 * r * d);
		_CosThetaView = min( _CosThetaView, -sqrt( 1.0 - (GROUND_RADIUS_KM*GROUND_RADIUS_KM) / (r*r) ) - 0.001 );	// Make sure we're always slightly below the horizon angle Theta_H
	}
	else
	{	// Viewing toward the sky
		float	d_atmosphere = ATMOSPHERE_RADIUS_KM - r;								// Distance to the atmosphere (the minimum distance we can see viewing straight up)
		float	d_horizon = 0.999 * (sqrt( r*r - GROUND_RADIUS_KM*GROUND_RADIUS_KM ) + sqrt( ATMOSPHERE_RADIUS_KM*ATMOSPHERE_RADIUS_KM - GROUND_RADIUS_KM*GROUND_RADIUS_KM ));	// Distance to the horizon (the maximum distance we can see viewing up)
		float	d = 2.0 * _Texel.y / (RESOLUTION_COS_THETA-1) - 1.0;					// 1 at texel RESOLUTION-1, ~0 at RESOLUTION/2
				d = lerp( d_atmosphere, d_horizon, d );									// d_horizon at texel RESOLUTION-1, ~d_atmosphere at RESOLUTION/2

		_CosThetaView = (ATMOSPHERE_RADIUS_KM * ATMOSPHERE_RADIUS_KM - r * r - d * d) / (2.0 * r * d);
	}

#else
	_CosThetaView = lerp( -1.0, 1.0, _UV.y );
#endif

	_CosThetaSun = frac( _Texel.x / RESOLUTION_COS_THETA_SUN );

#ifdef INSCATTER_NON_LINEAR_SUN
	// paper formula
	//_CosThetaSun = -(0.6 + log(1.0 - _CosThetaSun * (1.0 -  exp(-3.6)))) / 3.0;

	// better formula
	_CosThetaSun = tan( (2.0 * _CosThetaSun - 1.0 + 0.26) * 1.1 ) * 0.18692904279186995490534690217449;	// / tan( 1.26 * 1.1 );
#else
	_CosThetaSun = lerp( -0.2, 1.0, _CosThetaSun );
#endif

	_CosGamma = lerp( -1.0, 1.0, floor( _Texel.x / RESOLUTION_COS_THETA_SUN ) / (RESOLUTION_COS_GAMMA-1) );
}


//////////////////////////////////////////////////////////////////////////
// 0] Pre-Computes the transmittance table for all possible altitudes and zenith angles
//
float	ComputeOpticalDepth( float _AltitudeKm, float _CosTheta, const float _Href, uniform uint STEPS_COUNT=500 )
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

float4	PreComputeTransmittance( VS_IN _In ) : SV_TARGET0
{
	uint3	Texel = GetTexelInfos( _In );
	float2	UV = float2( Texel.xy ) / _TargetSize.xy;

	float	AltitudeKm = UV.y*UV.y * ATMOSPHERE_THICKNESS_KM;				// Grow quadratically to have more precision near the ground
	float	CosTheta = -0.15 + tan( 1.5 * UV.x ) / tan(1.5) * (1.0 + 0.15);	// Grow tangentially to have more precision horizontally
//	float	CosTheta = lerp( -0.15, 1.0, UV.x );							// Grow linearly

	float3	OpticalDepth = _AirParams.x * SIGMA_SCATTERING_RAYLEIGH * ComputeOpticalDepth( AltitudeKm, CosTheta, _AirParams.y ) + _FogParams.y * ComputeOpticalDepth( AltitudeKm, CosTheta, _FogParams.z );

	return float4( exp( -OpticalDepth ), 0.0 );
}

//////////////////////////////////////////////////////////////////////////
// 1] Pre-Computes the ground irradiance table accounting for direct lighting only
//
float4	PreComputeIrradiance_Single( VS_IN _In ) : SV_TARGET0
{
	uint2	Texel = GetTexelInfos( _In ).xy;
	float2	UV = float2( Texel ) / _TargetSize.xy;

	float	AltitudeKm = UV.y * ATMOSPHERE_THICKNESS_KM;
	float	CosThetaSun = lerp( -0.2, 1.0, UV.x );

	float	Reflectance = saturate( CosThetaSun );

    return float4( GetTransmittance( AltitudeKm, CosThetaSun ) * Reflectance, 0.0 );	// Return Sun reflectance attenuated by atmosphere as seen from given altitude
}


//////////////////////////////////////////////////////////////////////////
// 2] Pre-Computes the single scattering table
// Store Rayleigh and Mie contributions separately, WITHOUT the phase function factor (cf "Angular precision" chapter)
//
void	Integrand_Single( float _RadiusKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, float _DistanceKm, out float3 _Rayleigh, out float3 _Mie )
{
	// P0 = [0, _RadiusKm]
	// V  = [SinTheta, CosTheta]
	// L  = [SinThetaSun, CosThetaSun]
	// CosGamma = V.L
	//
	float	CurrentRadiusKm = sqrt( _RadiusKm * _RadiusKm + _DistanceKm * _DistanceKm + 2.0 * _RadiusKm * _CosThetaView * _DistanceKm );	// sqrt[ (P0 + d.V)² ]
//	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _CosGamma * _DistanceKm) / CurrentRadiusKm;	//### How do they get that??? I can't find any gamma in there! And that seems logical because only Cos(Theta_sun) is driving the altitude!
	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _DistanceKm) / CurrentRadiusKm;				// From the 3 sides of the triangle (_RadiusKm, CurrentRadiusKm and _DistanceKm) we can retrieve cos(Theta_Sun') = (CurrentRadiusKm² + _RadiusKm² - _DistanceKm²)/(2*_DistanceKm*CurrentRadiusKm)

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
    float3	Transmittance = SunTransmittance_Atmosphere2Point * ViewTransmittance_Source2Point;					// Total transmittance is the product of the View => Hit & Hit => Atmosphere transmittances
	_Rayleigh = exp( -CurrentAltitudeKm / _AirParams.y ) * Transmittance;										// Air density * Transmittance
	_Mie = exp( -CurrentAltitudeKm / _FogParams.z ) * Transmittance;											// Fog density * Transmittance
}

PS_OUT	PreComputeInScattering_Single( PS_IN _In )
{
	const uint STEPS_COUNT = 50;

	uint3	Texel = GetTexelInfos( _In );

	// Retrieve the 3 cosines for the current slice
	float	AltitudeKm, CosThetaView, CosThetaSun, CosGamma;
	GetSliceData( Texel, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Compute distance to atmosphere or ground, whichever comes first
	float4	PositionKm = float4( 0.0, AltitudeKm, 0.0, 0.0 );
	float3	View = float3( sqrt( 1.0 - CosThetaView*CosThetaView ), CosThetaView, 0.0 );
	bool	bGroundHit;
	float	TraceDistanceKm = ComputeNearestHit( PositionKm.xyz, View, ATMOSPHERE_THICKNESS_KM, bGroundHit );
	float	StepSizeKm = TraceDistanceKm / STEPS_COUNT;

	float3	PreviousRayleigh;
	float3	PreviousMie;
	Integrand_Single( GROUND_RADIUS_KM + AltitudeKm, CosThetaView, CosThetaSun, CosGamma, 0.0, PreviousRayleigh, PreviousMie );

	float3	_Rayleigh = 0.0;
	float3	_Mie = 0.0;

	// Begin accumulation
	float	DistanceKm = StepSizeKm;
	for ( uint i=0; i < STEPS_COUNT; i++ )
	{
		float3	CurrentRayleigh, CurrentMie;
		Integrand_Single( GROUND_RADIUS_KM + AltitudeKm, CosThetaView, CosThetaSun, CosGamma, DistanceKm, CurrentRayleigh, CurrentMie );

		_Rayleigh += 0.5 * (PreviousRayleigh + CurrentRayleigh);
		_Mie += 0.5 * (PreviousMie + CurrentMie);

		PreviousRayleigh = CurrentRayleigh;
		PreviousMie = CurrentMie;
		DistanceKm += StepSizeKm;
	}

	_Rayleigh *= _AirParams.x * SIGMA_SCATTERING_RAYLEIGH * StepSizeKm;
	_Mie *= _FogParams.x * StepSizeKm;

    // Store separately Rayleigh and Mie contributions, WITHOUT the phase function factor (cf "Angular precision")
	PS_OUT	Out;
	Out.Color0 = _Rayleigh;
	Out.Color1 = _Mie;

	return Out;
}

//////////////////////////////////////////////////////////////////////////
// Pre-Computes the delta scattering table
float3	PreComputeInScattering_Delta( PS_IN _In ) : SV_TARGET0
{
	const uint	STEPS_COUNT = 16;

	const float	dPhi = PI / STEPS_COUNT;
	const float	dTheta = PI / STEPS_COUNT;

	uint3	Texel = GetTexelInfos( _In );

	// Retrieve the 3 cosines for the current slice
	float	AltitudeKm, CosThetaView, CosThetaSun, CosGamma;
	GetSliceData( Texel, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Clamp values
	float	r = GROUND_RADIUS_KM + clamp( AltitudeKm, 0.0, ATMOSPHERE_THICKNESS_KM );
	CosThetaView = clamp( CosThetaView, -1.0, 1.0 );
	CosThetaSun = clamp( CosThetaSun, -1.0, 1.0 );

	float	var = sqrt( 1.0 - CosThetaView*CosThetaView ) * sqrt( 1.0 - CosThetaSun*CosThetaSun );
	CosGamma = clamp( CosGamma, CosThetaSun * CosThetaView - var, CosThetaSun * CosThetaView + var );	//### WTF?? Clarify!!!

	float	cthetaground = -sqrt( 1.0 - (GROUND_RADIUS_KM / r) * (GROUND_RADIUS_KM / r) );	// Minimum cos(theta) before we hit the ground

	float3	View = float3( sqrt( 1.0 - CosThetaView * CosThetaView ), CosThetaView, 0.0 );

	// We simply deduce Phi, the azimuth between Sun & View from the SSS formula from http://en.wikipedia.org/wiki/Solution_of_triangles#Three_sides_given
	// Phi = acos( (cos(gamma) - cos(ThetaV)*cos(ThetaS)) / (sin(ThetaV)*sin(ThetaS) )
	// Next, we need the X coordinate of the Sun vector which is simply:
	// sx = cos(Phi)*sin(ThetaS) = (cos(gamma) - cos(ThetaV)*cos(ThetaS)) / sin(ThetaV)
	//
	float3	Sun;
	Sun.x = View.x == 0.0 ? 0.0 : (CosGamma - CosThetaSun * CosThetaView) / View.x;
	Sun.y = CosThetaSun;
	Sun.z = sqrt( max( 0.0, 1.0 - Sun.x * Sun.x - Sun.y * Sun.y ) );	// Z is deduced from other coordinates

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
				float3	InScatteredRayleigh = Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;
				float3	InScatteredMie = Sample4DScatteringTable( _TexScatteringDelta_Mie, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;
				float	PhaseRayleigh = PhaseFunctionRayleigh( CosPhaseAngleSun );
				float	PhaseMie = PhaseFunctionMie( CosPhaseAngleSun );
				dScattering += PhaseRayleigh * InScatteredRayleigh + PhaseMie * InScatteredMie;
			}
			else	// Next pass only use the Rayleigh table
				dScattering += Sample4DScatteringTable( _TexScatteringDelta_Rayleigh, AltitudeKm, w.y, CosThetaSun, CosPhaseAngleSun ).xyz;

			float	CosPhaseAngleView = dot( View, w );
			float	PhaseRayleigh = PhaseFunctionRayleigh( CosPhaseAngleView );
			float	PhaseMie = PhaseFunctionMie( CosPhaseAngleView );

			// Light coming from direction w and scattered in view direction
			// = light arriving at x from direction w (dScattering) * SUM( scattering coefficient * phaseFunction )
			// see Eq (7)
			Scattering += dScattering * (_AirParams.x * SIGMA_SCATTERING_RAYLEIGH * exp( -AltitudeKm / _AirParams.y ) * PhaseRayleigh + _FogParams.x * exp( -AltitudeKm / _FogParams.z ) * PhaseMie) * dw;
		}
	}

	return Scattering;	// output In-Scattering = J[T.alpha/PI.deltaE + deltaS] (line 7 in algorithm 4.1)
}

//////////////////////////////////////////////////////////////////////////
// Pre-Computes the irradiance table accounting for multiple scattering
float3	PreComputeIrradiance_Delta( VS_IN _In ) : SV_TARGET0
{
const uint STEPS_COUNT=32;

	const float	dPhi = PI / STEPS_COUNT;
	const float	dTheta = PI / STEPS_COUNT;

	float2	UV = (_In.__Position.xy - 0.5) / _TargetSize.xy;

	float	AltitudeKm = UV.y * ATMOSPHERE_THICKNESS_KM;
	float	CosThetaSun = lerp( -0.2, 1.0, UV.x );

	float3	Sun = float3( sqrt( 1.0 - saturate( CosThetaSun * CosThetaSun ) ), CosThetaSun, 0.0 );

	// Integral over 2.PI around x with two nested loops over w directions (theta,phi) -- Eq (15)
	float3	Result = 0.0;
	for ( uint PhiIndex=0; PhiIndex < 2 * STEPS_COUNT; PhiIndex++ )
	{
		float	Phi = (PhiIndex + 0.5) * dPhi;
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
float3	Integrand_Multiple( float _RadiusKm, float _CosThetaView, float _CosThetaSun, float _CosGamma, float _DistanceKm )
{
	// P0 = [0, _RadiusKm]
	// V  = [SinTheta, CosTheta]
	// L  = [SinThetaSun, CosThetaSun]
	// CosGamma = V.L
	//
	float	CurrentRadiusKm = sqrt( _RadiusKm * _RadiusKm + _DistanceKm * _DistanceKm + 2.0 * _RadiusKm * _CosThetaView * _DistanceKm );	// sqrt[ (P0 + d.V)² ]
//	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _CosGamma * _DistanceKm) / CurrentRadiusKm;	//### How do they get that??? I can't find any gamma in there! And that seems logical because only Cos(Theta_sun) is important to guide altitude!
	float	CurrentCosThetaSun = (_RadiusKm * _CosThetaSun + _DistanceKm) / CurrentRadiusKm;				// From the 3 sides of the triangle (_RadiusKm, CurrentRadiusKm and _DistanceKm) we can retrieve cos(Theta_Sun') = (CurrentRadiusKm² + _RadiusKm² - _DistanceKm²)/(2*_DistanceKm*CurrentRadiusKm)
	float	CurrentCosThetaView = (_RadiusKm * _CosThetaView + _DistanceKm) / CurrentRadiusKm;

	float	StartAltitudeKm = _RadiusKm - GROUND_RADIUS_KM;
	float	CurrentAltitudeKm = CurrentRadiusKm - GROUND_RADIUS_KM;

	return  GetTransmittance( StartAltitudeKm, _CosThetaView, _DistanceKm ) * Sample4DScatteringTable( _TexScatteringDelta, CurrentAltitudeKm, CurrentCosThetaView, CurrentCosThetaSun, _CosGamma ).xyz;
}

float3	PreComputeInScattering_Multiple( PS_IN _In ) : SV_TARGET0
{
	const uint STEPS_COUNT = 50;

	uint3	Texel = GetTexelInfos( _In );

	// Retrieve the 3 cosines for the current slice
	float	AltitudeKm, CosThetaView, CosThetaSun, CosGamma;
	GetSliceData( Texel, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

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
		float3 ScatteringRayleighMie = Integrand_Multiple( GROUND_RADIUS_KM + AltitudeKm, CosThetaView, CosThetaSun, CosGamma, DistanceKm );
		Result += 0.5 * (PreviousScatteringRayleighMie + ScatteringRayleighMie);

		PreviousScatteringRayleighMie = ScatteringRayleighMie;
		DistanceKm += StepSizeKm;
	}

	return Result * StepSizeKm;
}

//////////////////////////////////////////////////////////////////////////
// Merges single-scattering tables for Rayleigh & Mie into the single initial scattering table
float4	MergeInitialScattering( PS_IN _In ) : SV_TARGET0
{
	uint3	Texel = GetTexelInfos( _In );

	float3	Rayleigh = _TexScatteringDelta_Rayleigh[Texel].xyz;
	float	Mie = _TexScatteringDelta_Mie[Texel].x;

	return float4( Rayleigh, Mie ); // Store only red component of single Mie scattering (cf. "Angular precision")
}

//////////////////////////////////////////////////////////////////////////
// Accumulates delta in-scattering into the final scattering table
float4	AccumulateInScattering( PS_IN _In ) : SV_TARGET0
{
	uint3	Texel = GetTexelInfos( _In );

	// We need to divide in-scattering by the Rayleigh phase function so we need CosGamma
	float	AltitudeKm, CosThetaView, CosThetaSun, CosGamma;
	GetSliceData( Texel, AltitudeKm, CosThetaView, CosThetaSun, CosGamma );

	// Get rayleigh scattering
	float3	Rayleigh = _TexScatteringDelta_Rayleigh[Texel].xyz;
			Rayleigh /= PhaseFunctionRayleigh( CosGamma );

	return float4( Rayleigh, 0.0 );
}

//////////////////////////////////////////////////////////////////////////
// Accumulates irradiance into the final irradiance table
float3	AccumulateIrradiance( VS_IN _In ) : SV_TARGET0
{
	uint3	Texel = GetTexelInfos( _In );
	return _TexIrradianceDelta[Texel.xy].xyz;
}
