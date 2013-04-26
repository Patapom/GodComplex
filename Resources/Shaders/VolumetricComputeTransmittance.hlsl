//////////////////////////////////////////////////////////////////////////
// This shader computes the transmittance function map
//
#include "Inc/Global.hlsl"
#include "Inc/Volumetric.hlsl"

//#define USE_OBJECT_ZBUFFER	// Define this to read ZMin/Max from the object's ZMin/Max buffer
//#define USE_FRUSTUM_SPLAT		// Define this to sample the camera frustum splat texture that indicates us whether a shadow pixel is relevant for computation or not

static const float	STEPS_COUNT = 32.0;
static const float	INV_STEPS_COUNT = 1.0 / (1.0+STEPS_COUNT);

//[
cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
};
//]

Texture2D		_TexDepth			: register(t10);
Texture2D		_TexSplatFrustum	: register(t11);

struct	VS_IN_FRUSTUM
{
	float3	Position	: POSITION;
};

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_OUT
{
	float4	C0 : SV_TARGET0;
	float4	C1 : SV_TARGET1;
};


//////////////////////////////////////////////////////////////////////////
// Simple splatting of the camera frustum
VS_IN	VS_SplatFrustum( VS_IN_FRUSTUM _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1 ), _Camera2World );	// Transform into world space
	float4	ShadowPosition = mul( WorldPosition, _World2Shadow );				// Then into shadow map space

	VS_IN	Out;
	Out.__Position = float4( 2.0 * ShadowPosition.x - 1.0, 1.0 - 2.0 * ShadowPosition.y, 0, 1 );
	return Out;
}

float	PS_SplatFrustum( VS_IN _In ) : SV_TARGET0
{
	return 1;
}


//////////////////////////////////////////////////////////////////////////
// Actual Transmittance Function Map computation
VS_IN	VS( VS_IN _In )	{ return _In; }

PS_OUT	PS( VS_IN _In )
{
	PS_OUT	Out;
	Out.C0 = float4( 2, 0, 0, 0 );	// These are the default DCT coefficients to obtain a transmittance of 1
	Out.C1 = float4( 0, 0, 0, 1 );

//	float2	UV = _In.__Position.xy * _dUV.xy;
	float2	UV = (_In.__Position.xy - 0.5) * _dUV.xy;

#ifdef USE_FRUSTUM_SPLAT
	// Read back splat frustum
	clip( _TexSplatFrustum.SampleLevel( LinearClamp, UV, 0.0 ).x - 0.5 );
#endif

	// Sample min/max depths at position
#ifdef USE_OBJECT_ZBUFFER
	float2	ZMinMax = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).xy;
#else
	float2	ZMinMax = float2( 0.0, _ShadowZMax.x );
#endif

	float	Depth = ZMinMax.y - ZMinMax.x;
	if ( Depth <= 1e-3 )
		return Out;	// Empty interval, no trace needed...

	// Ensure we trace a minimum distance
	const float	MinDepth = 0.1;
	if ( Depth < MinDepth )
	{
		Depth = MinDepth;
		float	CenterZ = 0.5 * (ZMinMax.x + ZMinMax.y);
		ZMinMax = CenterZ + float2( -0.5, +0.5 ) * Depth;
	}

	// Ensure we trace a maximum distance
	const float	MaxDepth = 32.0 * BOX_HEIGHT;
	if ( Depth > MaxDepth )
	{
		Depth = MaxDepth;
		ZMinMax.y = ZMinMax.x + Depth;
	}

	float	InvDepth = 1.0 / Depth;

	// Retrieve start & end positions in world space
// 	float2	ShadowPos = float2( 2.0 * UV.x - 1.0, 1.0 - 2.0 * UV.y );
// 	float3	WorldPosStart = mul( float4( ShadowPos, ZMinMax.x, 1.0 ), _Shadow2World ).xyz;
// 	float3	WorldPosEnd = mul( float4( ShadowPos, ZMinMax.y, 1.0 ), _Shadow2World ).xyz;

		// Now using UVs instead of projected coordinates
	float3	WorldPosStart = mul( float4( UV, ZMinMax.x, 1.0 ), _Shadow2World ).xyz;
	float3	WorldPosEnd = mul( float4( UV, ZMinMax.y, 1.0 ), _Shadow2World ).xyz;

// Out.C0 = float4( WorldPosStart, 0 );
// Out.C1 = float4( WorldPosEnd, 0 );
// return Out;

	float4	Step = float4( WorldPosEnd - WorldPosStart, Depth ) * INV_STEPS_COUNT;
	float4	Position = float4( WorldPosStart, 0.0 ) + 0.5 * Step;

	// Prepare the angles & angle steps for the DCT coefficients
	const float		dx = INV_STEPS_COUNT;					// Normalized Z step size
	const float4	CosTerm0 = PI * float4( 0, 1, 2, 3 );
	const float2	CosTerm1 = PI * float2( 4, 5 );
	float	StartX = 0.5 * dx;								// This is the normalized Z we're starting from...
	float4	Angle0 = CosTerm0 * StartX;
	float2	Angle1 = CosTerm1 * StartX;
	float4	dAngle0 = CosTerm0 * dx;						// This is the increment in phase
	float2	dAngle1 = CosTerm1 * dx;

	Out.C0 = Out.C1 = 0.0;

	float	Sigma_t = 0.0;
	float	Transmittance = 1.0;
	for ( float StepIndex=0; StepIndex < STEPS_COUNT; StepIndex++ )
	{
		float	Density = GetVolumeDensity( Position.xyz, 0.0 );


// Hardcode empty density outside the box
// if ( abs(Position.x) > 1.0 || abs(Position.z) > 1.0 || abs(Position.y-2.0) > 2.0 )
// 	Density = 0.0;

//Density = 0.0;
//Density *= 4.0;

		float	PreviousSigma_t = Sigma_t;
		Sigma_t = EXTINCTION_COEFF * Density;

//Sigma_t *= 10.0;//###

//		float	StepTransmittance = exp( -Sigma_t * Step.w );
		float	StepTransmittance = IntegrateExtinction( PreviousSigma_t, Sigma_t, Step.w );
		Transmittance *= StepTransmittance;

		// Accumulate cosine weights for the DCT
#ifndef USE_FAST_COS
		Out.C0 += Transmittance * cos( Angle0 );
		Out.C1.xy += Transmittance * cos( Angle1 );
#else
		float4	Temp0 = FastCos( float4( Angle0.yzw, Angle1.x ) );
		float	Temp1 = FastCos( Angle1.y );
		Out.C0 += Transmittance * float4( 1, Temp0.xyz );
		Out.C1.xy += Transmittance * float2( Temp0.w, Temp1 );
#endif

		// Advance in world and phase
		Position += Step;
		Angle0 += dAngle0;
		Angle1 += dAngle1;
	}

	Out.C0 *= 2.0 * dx;
	Out.C1.xy *= 2.0 * dx;

	Out.C1.zw = ZMinMax;	// We keep in/out distances for decoding...

//Out.C0 = Out.C1 = Transmittance;

	return Out;
}
