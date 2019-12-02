//////////////////////////////////////////////////////////////////////////
// This shader computes the volume's transmittance at a very low resolution using only the low frequency noise
//	and stores min/max Z values at which we enter/exit the volume
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"

static const float	STEPS_COUNT = 64.0;		// Many steps

static const float	START_TRANSMITTANCE = 0.99;	// Start interval when transmittance goes below that level
static const float	END_TRANSMITTANCE = 0.01;	// End interval when transmittance goes below that level


cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
};

Texture2D		_TexVolumeDepth	: register(t10);
Texture2D		_TexDownsampledSceneDepth	: register(t12);


struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

float2	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = (_In.__Position.xy - 0.5) * _dUV.xy;

	// Sample min/max depths at position
	float2	ZMinMax = _TexVolumeDepth.SampleLevel( LinearClamp, UV, 0.0 ).xy;
	float	SceneZ = _TexDownsampledSceneDepth.mips[2][_In.__Position.xy].z;	// Use ZMax
	ZMinMax.y = min( ZMinMax.y, SceneZ );	// Limit trace to scene Z
	float	Depth = ZMinMax.y - ZMinMax.x;
	if ( Depth < 0.0 )
		return ZMinMax;

	// Retrieve start & end positions in world space
	float3	ViewCamera = float3( _CameraData.x * (2.0 * UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * UV.y), 1.0 );
	float4	View = float4( mul( float4( ViewCamera, 0.0 ), _Camera2World ).xyz, 1.0 );
	float4	WorldPosStart = float4( _Camera2World[3].xyz, 0.0 ) + ZMinMax.x * View;
	float4	WorldPosEnd = float4( _Camera2World[3].xyz, 0.0 ) + ZMinMax.y * View;

	// Compute initial position & step
	float	StepsCount = STEPS_COUNT;
	float4	Step = (WorldPosEnd - WorldPosStart) / StepsCount;
	float4	Position = WorldPosStart;

	// Start integration
	ZMinMax.x = 1.1*ZMinMax.y;	// Make the interval empty at first, start boundary should be written as soon as we enter the volume...
	float	Sigma_t = 0.0;
	float	Transmittance = 1.0;
	for ( float StepIndex=0.0; StepIndex < StepsCount; StepIndex++ )
	{
		// Advance in world
		float4	PreviousPosition = Position;
		Position += Step;

		// Sample density at position
		float	Density = GetVolumeDensity( Position.xyz, 0.0 );
		float	PreviousSigma_t = Sigma_t;
		Sigma_t = _CloudExtinctionScattering.x * Density;

		// Compute transmittance for that step
		float	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );

		// Accumulate transmittance
		float	PreviousTransmittance = Transmittance;
		Transmittance *= StepTransmittance;

		// Reduce Z min/max interval
		if ( PreviousTransmittance >= START_TRANSMITTANCE && Transmittance < START_TRANSMITTANCE )
		{	// Reduce min boundary
			ZMinMax.x = PreviousPosition.w;
		}
		if ( Transmittance < END_TRANSMITTANCE )
		{	// Reduce max boundary
			ZMinMax.y = Position.w;
			StepIndex = StepsCount;	// So we break right after...
		}
	}

// return 0.1 * (ZMinMax.y - ZMinMax.x);	// Store reduced min/max interval
// return 0.01 * ZMinMax.y;	// Store reduced min/max interval

	return ZMinMax;	// Store reduced min/max interval
}
