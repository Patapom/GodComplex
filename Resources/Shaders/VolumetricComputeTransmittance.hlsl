//////////////////////////////////////////////////////////////////////////
// This shader computes the transmittance function map
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"

static const float	STEPS_COUNT = 128.0;
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

struct	PS_OUT
{
	float4	C0 : SV_TARGET0;
	float4	C1 : SV_TARGET1;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

PS_OUT	PS( VS_IN _In )
{
	PS_OUT	Out;
	Out.C0 = float4( 2, 0, 0, 0 );	// These are the default DCT coefficients to obtain a transmittance of 1
	Out.C1 = float4( 0, 0, 0, 0 );

	float2	UV = _In.__Position.xy * _dUV.xy;

	// Sample min/max depths at position
	float2	ZMinMax = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).xy;
	float	Depth = ZMinMax.y - ZMinMax.x;
	if ( Depth <= 0.0 )
		return Out;	// Empty interval, no trace needed...

	// Retrieve start & end positions in world space
	float2	ShadowPos = float2( 2.0 * UV.x - 1.0, 1.0 - 2.0 * UV.y );
	float3	WorldPosStart = mul( float4( ShadowPos, ZMinMax.x, 1.0 ), _Shadow2World ).xyz;
	float3	WorldPosEnd = mul( float4( ShadowPos, ZMinMax.y, 1.0 ), _Shadow2World ).xyz;

// Out.C0 = float4( WorldPosStart, 0 );
// Out.C1 = float4( WorldPosEnd, 0 );
// return Out;

	float4	Step = float4( WorldPosEnd - WorldPosStart, ZMinMax.y - ZMinMax.x ) * INV_STEPS_COUNT;
	float4	Position = float4( WorldPosStart, 0.0 ) + 0.5 * Step;

	float	dx = Step.w * _ShadowZMax.y;					// Normalized Z step size

	// Prepare the angles & angle steps for the DCT coefficients
	const float4	CosTerm0 = PI * float4( 0, 1, 2, 3 );
	const float4	CosTerm1 = PI * float4( 4, 5, 6, 7 );
	float	StartX = ZMinMax.x * _ShadowZMax.y + 0.5 * dx;	// This is the normalized Z we're starting from...
	float4	Angle0 = CosTerm0 * StartX;
	float4	Angle1 = CosTerm1 * StartX;
	float4	dAngle0 = CosTerm0 * dx;						// This is the increment in phase
	float4	dAngle1 = CosTerm1 * dx;

	float	Transmittance = 1.0;
	for ( float StepIndex=0; StepIndex < STEPS_COUNT; StepIndex++ )
	{
		float	Density = GetVolumeDensity( Position.xyz );

//Density = 0.0;
Density *= 2.0;

		float	Sigma_t = SCATTERING_COEFF * Density;

		float	StepTransmittance = exp( -Sigma_t * Step.w );
		Transmittance *= StepTransmittance;

		// Accumulate cosine weights for the DCT
		Out.C0 += Transmittance * cos( Angle0 );
		Out.C1 += Transmittance * cos( Angle1 );

		// Advance in world and phase
		Position += Step;
		Angle0 += dAngle0;
		Angle1 += dAngle1;
	}

	Out.C0 *= 2.0 * dx;
	Out.C1 *= 2.0 * dx;

//Out.C0 = Out.C1 = Transmittance;

	return Out;
}
