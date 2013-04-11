//////////////////////////////////////////////////////////////////////////
// This shader displays the actual volume
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"

static const float	STEPS_COUNT = 64.0;
static const float	INV_STEPS_COUNT = 1.0 / (1.0+STEPS_COUNT);

//[
cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
};
//]

Texture2D		_TexDepth			: register(t10);
Texture2DArray	_TexTransmittance	: register(t11);


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};


float	TempGetTransmittance( float3 _WorldPosition )
{
	float3	ShadowPosition = mul( float4( _WorldPosition, 1.0 ), _World2Shadow ).xyz;
	float2	UV = float2( 0.5 * (1.0 + ShadowPosition.x), 0.5 * (1.0 - ShadowPosition.y) );
	float	Z = ShadowPosition.z;

	float4	C0 = _TexTransmittanceMap.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
//return C0.x;
	float4	C1 = _TexTransmittanceMap.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );

	float2	ZMinMax = C1.zw;
	if ( Z < ZMinMax.x )
		return 1.0;	// We're not even in the shadow yet!

	float	x = saturate( (Z - ZMinMax.x) / (ZMinMax.y - ZMinMax.x) );

	const float4	CosTerm0 = PI * float4( 0, 1, 2, 3 );
	const float2	CosTerm1 = PI * float2( 4, 5 );

	float4	Cos0 = cos( CosTerm0 * x );
	float2	Cos1 = cos( CosTerm1 * x );

	Cos0.x = 0.5;	// Patch for inverse DCT

	return saturate( dot( Cos0, C0 ) + dot( Cos1, C1.xy ) );
}

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
float3	ComputeIsotropicScattering( float3 _Position, float _Density )
{
	const float	_Sigma_scattering_Isotropic = 0.01;

	float	y = saturate( 2.0 * (_Position.y / BOX_HEIGHT - 0.5) );
			y *= y;
			y = 1.0 - y;	// Max at center!
//			y *= y;

	float3	SkyRadiance = 0.05 * float3( 0.6, 0.71, 0.75 );
	float3	SunRadiance = 0.08 * float3( 1.0, 1.0, 1.0 );
	float3	GroundRadiance = 0.02 * float3( 1.0, 0.8, 0.2 );
	float3	IsotropicLightTop = y * (SkyRadiance + SunRadiance);
	float3	IsotropicLightBottom = y * (SkyRadiance + SunRadiance) + GroundRadiance;

	float	IsotropicSphereRadiusTopKm = BOX_HEIGHT - _Position.y;
	float	IsotropicSphereRadiusBottomKm = _Position.y;

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

//return float4( UV, 0, 1 );

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
ZMinMax.y = ZMinMax.x + min( 8.0 * BOX_HEIGHT, Depth );	// Don't trace more than 16 units in length (less precision, prefer line above)

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
	float	StepsCount = ceil( Depth * 4.0 );	// This introduces ugly artefacts
//	float	StepsCount = STEPS_COUNT;
			StepsCount = min( STEPS_COUNT, StepsCount );

	float4	Step = float4( WorldPosEnd - WorldPosStart, ZMinMax.y - ZMinMax.x ) / StepsCount;
	float4	Position = float4( WorldPosStart, 0.0 ) + 0.5 * Step;

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
	float	g = 0.25;
	float	CosTheta = dot( View, LightDirection );
	float	Phase = 1.0 / (4 * PI) * (1 - g*g) * pow( max(0.0, 1+g*g-g*CosTheta ), -1.5 );

	// Start integration
	float	Sigma_t = 0.0;
	float	Sigma_s = 0.0;
	float3	Light = GetTransmittance( WorldPosStart.xyz );		// Start with light at start position
	float3	Scattering = 0.0;
	float	Transmittance = 1.0;
	for ( float StepIndex=0.0; StepIndex < StepsCount; StepIndex++ )
	{

//MipBias = 0.01 * Position.w;

		float	Density = GetVolumeDensity( Position.xyz, MipBias );

//Density = 0.1;

		float	PreviousSigma_t = Sigma_t;
		float	PreviousSigma_s = Sigma_s;
		Sigma_t = EXTINCTION_COEFF * Density;
		Sigma_s = SCATTERING_COEFF * Density;

		float	Shadowing = GetTransmittance( Position.xyz );
//		float	Shadowing = GetTransmittance( (Position + 0.5 * Step).xyz );

const float	ShadowAttenuation = 0.95;
Shadowing = 1.0 - (ShadowAttenuation * (1.0 - Shadowing));

		float3	PreviousLight = Light;
		Light = Shadowing;

if ( false)//_VolumeParams.y > 0.5 )
{	// ======================== Old Strategy without sub-step integration ========================

		// Compute extinction
//		float	StepTransmittance = exp( -Sigma_t * Step.w );
		float	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );
// if ( _VolumeParams.y < 0.5 )
// 	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );

		Transmittance *= StepTransmittance;

		// Compute scattering
		float3	StepScattering = Sigma_s * Light * Step.w;	// Constant sigma
				StepScattering += ComputeIsotropicScattering( Position.xyz, Density );
		Scattering += Transmittance * StepScattering;
}
else
{	// ======================== New Strategy WITH sub-step integration ========================
	// More accurate but also a little more hungry, but always better than using more samples!
		float	StepTransmittance;
		float3	StepScattering = IntegrateScattering( PreviousLight, Light, PreviousSigma_s, Sigma_s, PreviousSigma_t, Sigma_t, Step.w, StepTransmittance );
				StepScattering += ComputeIsotropicScattering( Position.xyz, Density );
		Scattering += Transmittance * StepScattering;
		Transmittance *= StepTransmittance;
}

// Used to visualize transmittance function map
//Scattering += 0.025 * float3( 1, 0, 0 ) * Shadowing * Step.w;

		// Advance in world and phase
		Position += Step;
	}


	Scattering *= lerp( 15.0, 15.0, _VolumeParams.y ) * Phase;


// Transmittance = 0.0;
// Scattering = mul( float4( WorldPosEnd, 1.0 ), _World2Shadow ).xyz;
// Scattering.xy = float2( 0.5 * (1.0 + Scattering.x), 0.5 * (1.0 - Scattering.y) );
// Scattering.z *= 0.25;
// 
// Scattering = 0.9 * 0.25 * _TexTransmittanceMap.SampleLevel( LinearClamp, float3( Scattering.xy, 1 ), 0.0 ).w;

//Scattering = 0.03 * StepsCount;
//Scattering = Depth;

//return float4( Transmittance.xxx, 0 );
//return float4( 1.0 * MipBias.xxx, Transmittance );
	return float4( Scattering, Transmittance );
	return Transmittance;
}
