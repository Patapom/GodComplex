//////////////////////////////////////////////////////////////////////////
// This shader displays the actual volume
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"

static const float	STEPS_COUNT = 32.0;
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

	return saturate( dot( Cos0, C0 ) + dot( Cos1, C1 ) );
}



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

//### Super important line to get a nice precision everywhere: we lack details at a distance (i.e. we don't trace the clouds fully) but who cares since we keep a nice precision?
ZMinMax.y = ZMinMax.x + min( 8.0, Depth );	// Don't trace more than 16 units in length
//ZMinMax.y = ZMinMax.x + min( 16.0, Depth );	// Don't trace more than 16 units in length

	// Retrieve start & end positions in world space
	float3	View = float3( _CameraData.x * (2.0 * UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * UV.y), 1.0 );
	float3	WorldPosStart = mul( float4( ZMinMax.x * View, 1.0 ), _Camera2World ).xyz;
	float3	WorldPosEnd = mul( float4( ZMinMax.y * View, 1.0 ), _Camera2World ).xyz;
// return float4( 1.0 * WorldPosStart, 0 );
//return float4( 1.0 * WorldPosEnd, 0 );

	float	StepsCount = ceil( Depth * 8.0 );
//	float	StepsCount = Depth * 4.0;
//	float	StepsCount = STEPS_COUNT;
			StepsCount = min( STEPS_COUNT, StepsCount );
	float	InvStepsCount = 1.0 / StepsCount;

	float4	Step = float4( WorldPosEnd - WorldPosStart, ZMinMax.y - ZMinMax.x ) * InvStepsCount;
	float4	Position = float4( WorldPosStart, 0.0 ) + 0.5 * Step;

	// Compute phase
	float3	LightDirection = mul( float4( _LightDirection, 0.0 ), _World2Camera ).xyz;	// Light in camera space
			View = normalize( View );
	float	g = 0.25;
	float	CosTheta = dot( View, LightDirection );
	float	Phase = 1.0 / (4 * PI) * (1 - g*g) * pow( 1+g*g-g*CosTheta, -1.5 );

	// Start integration
	float	Sigma_t = 0.0;
	float3	Scattering = 0.0;
	float	Transmittance = 1.0;
	for ( float StepIndex=0.0; StepIndex < StepsCount; StepIndex++ )
	{
		float	Density = GetVolumeDensity( Position.xyz );

//Density = 0.1;

		float	PreviousSigma_t = Sigma_t;
		Sigma_t = EXTINCTION_COEFF * Density;
		float	Sigma_s = SCATTERING_COEFF * Density;

		// Compute extinction
//		float	StepTransmittance = exp( -Sigma_t * Step.w );
		float	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );
		Transmittance *= StepTransmittance;

		// Compute scattering
		float	Shadowing = GetTransmittance( Position.xyz );

const float	ShadowAttenuation = 0.9;
Shadowing = 1.0 - (ShadowAttenuation * (1.0 - Shadowing));

		float3	Light = 15.0 * Shadowing;
		float3	StepScattering = Sigma_s * Phase * Light * Step.w;
		Scattering += Transmittance * StepScattering;

//Scattering += 0.25 * float3( 1, 0, 0 ) * Shadowing * Step.w;

		// Advance in world and phase
		Position += Step;
	}

// Transmittance = 0.0;
// Scattering = mul( float4( WorldPosEnd, 1.0 ), _World2Shadow ).xyz;
// Scattering.xy = float2( 0.5 * (1.0 + Scattering.x), 0.5 * (1.0 - Scattering.y) );
// Scattering.z *= 0.25;
// 
// Scattering = 0.9 * 0.25 * _TexTransmittanceMap.SampleLevel( LinearClamp, float3( Scattering.xy, 1 ), 0.0 ).w;

//Scattering = 0.03 * StepsCount;
//Scattering = Depth;

//return float4( Transmittance.xxx, 0 );
	return float4( Scattering, Transmittance );
	return Transmittance;
}
