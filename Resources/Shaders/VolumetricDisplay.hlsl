//////////////////////////////////////////////////////////////////////////
// This shader displays the actual volume
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"
#include "Inc/Atmosphere.hlsl"

static const float	STEPS_COUNT = 32.0;
static const float	INV_STEPS_COUNT = 1.0 / (1.0+STEPS_COUNT);

//[
cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
};
//]

Texture2D		_TexDepth	: register(t10);


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};


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
	const float	_Sigma_scattering_Isotropic = 0.1;

#if 0	// Ponder contributions with "insideness" of layer
	float	y = saturate( 2.0 * ((_Position.y - BOX_BASE) / BOX_HEIGHT - 0.5) );
			y *= y;
			y = 1.0 - y;	// Max at center!
#elif 0	// Ponder contribution by distance to top
	float	y = saturate( (_Position.y - BOX_BASE) / BOX_HEIGHT );
			y *= y;
#else
	float	y = 1.0;	// Constant throughout the layer
#endif

//y = 1.0;

	float3	SkyRadianceTop = 0.1 * _SkyLightTop;
	float3	SkyRadianceBottom = 0.1 * _SkyLightBottom;

	float3	SunRadiance = 0.02 * _SunLight;

	float3	GroundReflectance = 0.005 * float3( 1.0, 0.8, 0.2 );
	float3	GroundRadiance = GroundReflectance * _SunLight;

	float3	IsotropicLightTop = y * (SkyRadianceTop + SunRadiance);
	float3	IsotropicLightBottom = 1 * (SkyRadianceBottom + SunRadiance) + GroundRadiance;

	float	BoxTop = BOX_BASE + BOX_HEIGHT;
	float	BoxBottom = BOX_BASE;
	float	IsotropicSphereRadiusTopKm = BoxTop - _Position.y;
	float	IsotropicSphereRadiusBottomKm = _Position.y - BoxBottom;

	float	a = -_Sigma_scattering_Isotropic * IsotropicSphereRadiusTopKm;
	float3  IsotropicScatteringTop = IsotropicLightTop * max( 0.0, exp( a ) - a * Ei( a ));
			a = -_Sigma_scattering_Isotropic * IsotropicSphereRadiusBottomKm;
	float3  IsotropicScatteringBottom = IsotropicLightBottom * max( 0.0, exp( a ) - a * Ei( a ));
	return  _Density * (IsotropicScatteringTop + IsotropicScatteringBottom);
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
// return _TexDepth.SampleLevel( LinearClamp, float2( _Sigma_s0, _Sigma_s1 ), 0.0 ).x;


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

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;

	// Sample min/max depths at position
	float2	ZMinMax = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).xy;
	float	Depth = ZMinMax.y - ZMinMax.x;
//	Depth = max( 1e-3, Depth );
	if ( Depth <= 1e-3 )
		return float4( 0, 0, 0, 1 );	// Empty interval, no trace needed...
//return Depth;

#if 1

//### Super important line to get a nice precision everywhere: we lack details at a distance (i.e. we don't trace the clouds fully) but who cares since we keep a nice precision?
//ZMinMax.y = ZMinMax.x + min( 8.0, Depth );	// Don't trace more than 8 units in length
ZMinMax.y = ZMinMax.x + min( 8.0 * BOX_HEIGHT, Depth );	// Don't trace more than N times the box height's in length (less precision, prefer line above)

	float	MipBias = 0.0;

#elif 0
	// Instead of clamping max depth, I'm instead add mip bias based on the max depth we should have clamped and actual depth
	float	MaxDepth = 8.0;	// We should have clamped at 4 units max
	float	DepthClampingRatio = max( 1.0, Depth / MaxDepth );

	float	MipBias = 0.0 + 0.5 * log2( DepthClampingRatio );

	// For example, if we need to trace twice the max depth, then the mip bias will be 1

#else
	float	MipBias = 0.01 * ZMinMax.x;
#endif

	// Retrieve start & end positions in world space
	float3	View = float3( _CameraData.x * (2.0 * UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * UV.y), 1.0 );
	float3	WorldPosStart = mul( float4( ZMinMax.x * View, 1.0 ), _Camera2World ).xyz;
	float3	WorldPosEnd = mul( float4( ZMinMax.y * View, 1.0 ), _Camera2World ).xyz;
// return float4( 1.0 * WorldPosStart, 0 );
//return float4( 1.0 * WorldPosEnd, 0 );

//#define FIX_STEP_SIZE	0.01
#ifndef FIX_STEP_SIZE
//	float	StepsCount = ceil( Depth * 2.0 * BOX_HEIGHT );
	float	StepsCount = ceil( 2.0 * FastScreenNoise( _In.__Position.xy ) + Depth * 2.0 * BOX_HEIGHT );	// Add noise to hide banding
			StepsCount = min( STEPS_COUNT, StepsCount );
//	float	StepsCount = STEPS_COUNT;

	float4	Step = float4( WorldPosEnd - WorldPosStart, ZMinMax.y - ZMinMax.x ) / StepsCount;

	float	PosOffset = 0.5;	// Fixed offset
//	float	PosOffset = FastNoise( float3( 2.0*_In.__Position.xy, 0.0 ) );	// Random offset
	float4	Position = float4( WorldPosStart, 0.0 ) + PosOffset * Step;

#else	// Here, the steps have a fixed size and we simply determine their amount
		// This strategy misses a lot of details and requires lots of steps!
		// It was necessary to use a 2D pre-integration table with constant steps but unfortunately the
		//	time we would have gained sampling a 2D texture instead of performing integration with ALU
		//	is lost due to the fact we're using too many steps!
	float	StepsCount = Depth / FIX_STEP_SIZE;
			StepsCount = min( STEPS_COUNT, StepsCount );

	float4	Step = FIX_STEP_SIZE * float4( View, 1.0 );
	float4	Position = float4( WorldPosStart, 0.0 ) + 0.5 * Step;

#endif

	// Compute phase
	float3	LightDirection = mul( float4( _LightDirection, 0.0 ), _World2Camera ).xyz;	// Light in camera space
			View = normalize( View );

	const float	g_iso = 0.1;
	const float	g_forward = 0.85;
	float	CosTheta = dot( View, LightDirection );
	float	Phase_iso = 1.0 / (4 * PI) * (1 - g_iso*g_iso) * pow( max(0.0, 1+g_iso*g_iso-g_iso*CosTheta ), -1.5 );
	float	Phase_forward = 1.0 / (4 * PI) * (1 - g_forward*g_forward) * pow( max(0.0, 1+g_forward*g_forward-g_forward*CosTheta ), -1.5 );
	float	Phase = lerp( Phase_iso, Phase_forward, 0.2 );

	// Get Sun light & Sky lights
	float3	SunLight = SUN_INTENSITY * GetTransmittanceWithShadow( BOX_BASE + BOX_HEIGHT, _LightDirection.y );
	float3	SkyLightTop = SUN_INTENSITY * GetIrradiance( _TexIrradiance, (BOX_BASE + 1.0 * BOX_HEIGHT), _LightDirection.y );
	float3	SkyLightBottom = SUN_INTENSITY * GetIrradiance( _TexIrradiance, 0*(BOX_BASE + 0.0 * BOX_HEIGHT), _LightDirection.y );

	// Start integration
	float	Sigma_t = 0.0;
	float	Sigma_s = 0.0;
	float3	Light = SunLight * GetCloudTransmittance( WorldPosStart.xyz );		// Start with light at start position
	float3	Scattering = 0.0;
	float	Transmittance = 1.0;
	for ( float StepIndex=0.0; StepIndex < StepsCount; StepIndex++ )
	{
		float	Density = GetVolumeDensity( Position.xyz, MipBias );

		float	PreviousSigma_t = Sigma_t;
		float	PreviousSigma_s = Sigma_s;
		Sigma_t = EXTINCTION_COEFF * Density;
		Sigma_s = SCATTERING_COEFF * Density;

//		float	Shadowing = GetCloudTransmittance( Position.xyz );
		float	Shadowing = GetCloudTransmittance( (Position + 0.5 * Step).xyz );

		const float	ShadowStrength = 0.85;	// Shadow %
		Shadowing = 1.0 - (ShadowStrength * (1.0 - Shadowing));
		Shadowing *= smoothstep( 0.0, 1.0, smoothstep( 0.0, 0.03, abs(_LightDirection.y) ) );	// Full shadowing when the light is horizontal

		float3	PreviousLight = Light;
		Light = SunLight * Shadowing;

if ( true )//_VolumeParams.y > 0.5 )
{	// ======================== Old Strategy without sub-step integration ========================

		// Compute extinction
//		float	StepTransmittance = exp( -Sigma_t * Step.w );
		float	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );
// if ( _VolumeParams.y < 0.5 )
// 	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );

		Transmittance *= StepTransmittance;

		// Compute scattering
		float3	StepScattering  = Sigma_s * Phase * Light * Step.w;
//				StepScattering += ComputeIsotropicScattering( Position.xyz, Density, SunLight, SkyLightTop, SkyLightBottom ) * Step.w;
		Scattering += Transmittance * StepScattering;
}
else
{	// ======================== New Strategy WITH sub-step integration ========================
	// More accurate but also a little more hungry, but always better than using more samples!
		float	StepTransmittance;
		float3	StepScattering  = Phase * IntegrateScattering( PreviousLight, Light, PreviousSigma_s, Sigma_s, PreviousSigma_t, Sigma_t, Step.w, StepTransmittance );
				StepScattering += ComputeIsotropicScattering( Position.xyz, Density, SunLight, SkyLightTop, SkyLightBottom ) * Step.w;
		Scattering += Transmittance * StepScattering;
		Transmittance *= StepTransmittance;
}

// Used to visualize transmittance function map
//Scattering += 0.025 * float3( 1, 0, 0 ) * Shadowing * Step.w;

		// Advance in world and phase
		Position += Step;
	}

	return float4( Scattering, Transmittance );
}
