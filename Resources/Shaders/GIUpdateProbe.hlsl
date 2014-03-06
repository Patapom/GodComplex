//////////////////////////////////////////////////////////////////////////
// This compute shader updates the dynamic probes
//
#include "Inc/Global.hlsl"
#include "Inc/GI.hlsl"
#include "Inc/SH.hlsl"
#include "Inc/ShadowMap.hlsl"

#define	THREADS_X	64						// The maximum amount of sampling points per probe (Must match the MAX_SET_SAMPLES define declared in the header file!)
#define	THREADS_Y	1
#define	THREADS_Z	1

#define	MAX_PROBE_SETS			16			// Must match the MAX_PROBE_SETS define declared in the header file!
#define	MAX_PROBE_EMISSIVE_SETS	16			// Must match the MAX_PROBE_EMISSIVE_SETS define declared in the header file!

#define IRRADIANCE_BOOST_POINT_SPOT		100.0		// Artificial boost of irradiance (and consequently, bounce light)
#define IRRADIANCE_BOOST_DIRECTIONAL	10.0		// Artificial boost of irradiance (and consequently, bounce light)


cbuffer	cbUpdateProbes : register(b10)
{
	float3	_AmbientSH[9];					// Ambient sky
};

// Input probe structures
struct ProbeUpdateInfos
{
	uint		Index;						// The index of the probe we're updating
	uint		SetsStart;					// Index of the first set for the probe
	uint		SetsCount;					// Amount of sets for the probe
	uint		EmissiveSetsStart;			// Index of the first emissive set for the probe
	uint		EmissiveSetsCount;			// Amount of emissive sets for the probe
	uint		SamplingPointsStart;		// Index of the first sampling point for the probe
	uint		SamplingPointsCount;		// Amount of sampling points for the probe
	float3		SHStatic[9];				// Precomputed static SH (static geometry + static lights)
	float		SHOcclusion[9];				// Directional ambient occlusion for the probe
};
StructuredBuffer<ProbeUpdateInfos>	_SBProbeUpdateInfos : register( t10 );

struct	ProbeUpdateSetInfos
{
	uint		SamplingPointsStart;		// Index of the first sampling point
	uint		SamplingPointsCount;		// Amount of sampling points
	float3		SH[9];						// SH for the set
};
StructuredBuffer<ProbeUpdateSetInfos>	_SBProbeUpdateSetInfos : register( t11 );

struct	ProbeUpdateEmissiveSetInfos
{
	float3		EmissiveColor;				// Color of the emissive material
	float		SH[9];						// SH for the set
};
StructuredBuffer<ProbeUpdateEmissiveSetInfos>	_SBProbeUpdateEmissiveSetInfos : register( t12 );

struct ProbeSamplingPoint
{
	float3	Position;						// World position of the sampling point
	float3	Normal;							// World normal of the sampling point
	float	Radius;							// Radius of the sampling point's disc approximation
};
StructuredBuffer<ProbeSamplingPoint>	_SBProbeSamplingPoints : register( t13 );


// Result structure (declared in Inc/GI.hlsl)
RWStructuredBuffer<ProbeStruct>	_Output : register( u0 );


groupshared float3	gsSamplingPointIrradiance[THREADS_X];
groupshared float3	gsSetSH[9*MAX_PROBE_SETS];
groupshared float3	gsEmissiveSetSH[9*MAX_PROBE_EMISSIVE_SETS];


// Computes the shadow value for the given posiion, accounting for a radius around the position for soft shadowing
float	ComputeShadowCS( float3 _WorldPosition, float3 _WorldNormal, float _Radius )
{
	// Recompute tangent space
	float3	X = cross( _WorldNormal, float3( 0, 1, 0 ) );
	float3	Y;
	if ( dot(X,X) > 1e-6 )
	{
		Y = cross( _WorldNormal, X );
	}
	else
	{
		X = float3( 1, 0, 0 );
		Y = float3( 0, 0, 1 );
	}

_Radius *= 1.0;

	X *= _Radius;
	Y *= _Radius;

	// Fetch multiple samples
#if 0
	const uint		SHADOW_SAMPLES_COUNT = 16;
	const float2	SamplesOffset[SHADOW_SAMPLES_COUNT] = {	// Samples generated using my little Hammersley sequence generator available in the Tools/TestFilmCurve project
		float2( 0.65328145, 0.270598054 ),
		float2( 0.353553385, 0.353553385 ),
		float2( 0.331413567, 0.8001031 ),
		float2( -0.00000001545431, 0.353553385 ),
		float2( -0.3025379, 0.7303909 ),
		float2( -0.433012724, 0.433012724 ),
		float2( -0.8642103, 0.357967436 ),
		float2( -0.25, -0.0000000218556941 ),
		float2( -0.6929096, -0.287012577 ),
		float2( -0.395284653, -0.395284772 ),
		float2( -0.3449459, -0.832773864 ),
		float2( 0.00000000516362464, -0.4330127 ),
		float2( 0.3173045, -0.7660404 ),
		float2( 0.4677073, -0.467707 ),
		float2( 0.8945426, -0.370531648 ),
		float2( 0.176776692, 0.0 ),
	};
#else
	const uint		SHADOW_SAMPLES_COUNT = 32;
	const float2	SamplesOffset[SHADOW_SAMPLES_COUNT] = {
		float2( 0.6935199, 0.1379497 ),
		float2( 0.4619398, 0.1913417 ),
		float2( 0.7200738, 0.4811379 ),
		float2( 0.25, 0.25 ),
		float2( 0.4392168, 0.6573345 ),
		float2( 0.2343448, 0.5657583 ),
		float2( 0.1824902, 0.9174407 ),
		float2( -1.092785E-08, 0.25 ),
		float2( -0.1463178, 0.7355889 ),
		float2( -0.2139266, 0.5164644 ),
		float2( -0.5007843, 0.7494765 ),
		float2( -0.3061862, 0.3061862 ),
		float2( -0.6894183, 0.4606545 ),
		float2( -0.6110889, 0.2531212 ),
		float2( -0.9496413, 0.1888954 ),
		float2( -0.1767767, -1.545431E-08 ),
		float2( -0.714864, -0.1421954 ),
		float2( -0.4899611, -0.2029485 ),
		float2( -0.7349222, -0.4910594 ),
		float2( -0.2795084, -0.2795085 ),
		float2( -0.4500631, -0.6735675 ),
		float2( -0.2439136, -0.5888601 ),
		float2( -0.1857205, -0.9336798 ),
		float2( 3.651234E-09, -0.3061862 ),
		float2( 0.1503273, -0.7557458 ),
		float2( 0.2243682, -0.5416723 ),
		float2( 0.510324, -0.7637535 ),
		float2( 0.330719, -0.3307188 ),
		float2( 0.7049127, -0.4710076 ),
		float2( 0.6325371, -0.2620054 ),
		float2( 0.9653389, -0.1920177 ),
		float2( 0.125, 0 ),
	};
#endif

	float	Shadow = 0.0;
	for ( uint SampleIndex=0; SampleIndex < SHADOW_SAMPLES_COUNT; SampleIndex++ )
	{
		float2	SampleOffset = SamplesOffset[SampleIndex];
		float3	SamplePosition = _WorldPosition + SampleOffset.x * X + SampleOffset.y * Y + 0.01 * _WorldNormal;
		float4	ShadowPosition = World2ShadowMapProj( SamplePosition );

		float2	UV = 0.5 * float2( 1.0 + ShadowPosition.x, 1.0 - ShadowPosition.y );
		float	Zproj = ShadowPosition.z / ShadowPosition.w;

		float	ShadowZproj = _ShadowMap.SampleLevel( LinearClamp, UV, 0.0 );

		Shadow += ShadowZproj - Zproj > 1e-3 ? 1.0 : 0.0;
//		Shadow += saturate( 10.0 * (ShadowZproj - Zproj) );
	}

	return Shadow * (1.0 / SHADOW_SAMPLES_COUNT);
}


// Each group processes a single probe
// Each thread processes a single sampling point and will later process sets and even later collapse SH into a single vector
[numthreads( THREADS_X, THREADS_Y, THREADS_Z )]
void	CS( uint3 _GroupID			: SV_GroupID,			// Defines the group offset within a Dispatch call, per dimension of the dispatch call
			uint3 _ThreadID			: SV_DispatchThreadID,	// Defines the global thread offset within the Dispatch call, per dimension of the group
			uint3 _GroupThreadID	: SV_GroupThreadID,		// Defines the thread offset within the group, per dimension of the group
			uint  _GroupIndex		: SV_GroupIndex )		// Provides a flattened index for a given thread within a given group
{
	int	ProbeIndex = _GroupID.x;

	ProbeUpdateInfos	Probe = _SBProbeUpdateInfos[ProbeIndex];

	//////////////////////////////////////////////////////////////////////////
	// 1] Process the sampling point

	// Determine which sampling point this thread should process
	uint	SamplingPointIndex = _GroupThreadID.x;
	if ( SamplingPointIndex < Probe.SamplingPointsCount )
	{
		ProbeSamplingPoint	SamplingPoint = _SBProbeSamplingPoints[Probe.SamplingPointsStart + SamplingPointIndex];

		// Iterate on lights
// TODO: Light list per probe!! Don't process all lights every time!
		float3	SumIrradiance = 0.0;
		for ( uint LightIndex=0; LightIndex < _DynamicLightsCount; LightIndex++ )
		{
			LightStruct	LightSource = _SBLightsDynamic[LightIndex];

			float3	Irradiance;
			float3	Light;
			if ( LightSource.Type == 0 || LightSource.Type == 2 )
			{	// Compute a standard point light
				Light = LightSource.Position - SamplingPoint.Position;
				float	Distance2Light = length( Light );
// 				float	InvDistance2Light = 1.0 / Distance2Light;
// 				Light *= InvDistance2Light;
//
//// Try and avoid highlights when lights get too close to the sampling point
//InvDistance2Light = min( 1.0, InvDistance2Light );

				Light /= Distance2Light;
				float	InvDistance2Light = 1.0 / max( 0.5, Distance2Light );	// Try and avoid highlights when lights get too close to the sampling point


				Irradiance = LightSource.Color * InvDistance2Light * InvDistance2Light;

				if ( LightSource.Type == 2 )
				{	// Account for spots' angular falloff
					float	LdotD = -dot( Light, LightSource.Direction );
					Irradiance *= smoothstep( LightSource.Parms.w, LightSource.Parms.z, LdotD );
				}


				Irradiance *= IRRADIANCE_BOOST_POINT_SPOT;
			}
			else if ( LightSource.Type == 1 )
			{	// Compute a sneaky directional with shadow map
				Light = LightSource.Direction;
				Irradiance = LightSource.Color;	// Simple!
				Irradiance *= ComputeShadowCS( SamplingPoint.Position, SamplingPoint.Normal, SamplingPoint.Radius );

				Irradiance *= IRRADIANCE_BOOST_DIRECTIONAL;
			}

			float	NdotL = saturate( dot( SamplingPoint.Normal, Light ) );
			SumIrradiance += Irradiance * NdotL;
		}
		gsSamplingPointIrradiance[SamplingPointIndex] = SumIrradiance;
	}

	// Ensure all threads have finished computing their sampling point irradiance
	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////
	// 2] Process the sets points
	uint	SetIndex = _GroupThreadID.x;
	if ( SetIndex < Probe.SetsCount )
	{
		// Retrieve set infos for that thread
		ProbeUpdateSetInfos	Set = _SBProbeUpdateSetInfos[Probe.SetsStart + SetIndex];

		// Accumulate irradiance from all sampling points
		uint	StartIndex = Set.SamplingPointsStart;
		uint	EndIndex = StartIndex + Set.SamplingPointsCount;
		float3	SetIrradiance = 0.0;
		for ( SamplingPointIndex=StartIndex; SamplingPointIndex < EndIndex; SamplingPointIndex++ )
			SetIrradiance += gsSamplingPointIrradiance[SamplingPointIndex];
		SetIrradiance /= Set.SamplingPointsCount;

		// Compute SH irradiance
		for ( int i=0; i < 9; i++ )
			gsSetSH[9*SetIndex+i] = SetIrradiance * Set.SH[i];
	}

	// Ensure all threads have finished computing their set SH
	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////
	// 3] Process the emissive sets points
	uint	EmissiveSetIndex = _GroupThreadID.x;
	if ( EmissiveSetIndex < Probe.EmissiveSetsCount )
	{
		// Retrieve emissive set infos for that thread
		ProbeUpdateEmissiveSetInfos	EmissiveSet = _SBProbeUpdateEmissiveSetInfos[Probe.EmissiveSetsStart + EmissiveSetIndex];

		// Compute SH irradiance
		for ( int i=0; i < 9; i++ )
			gsEmissiveSetSH[9*EmissiveSetIndex+i] = EmissiveSet.EmissiveColor * EmissiveSet.SH[i];
	}

	// Ensure all threads have finished computing their set SH
	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////
	// 4] Use a single thread to finalize SH
	if ( _GroupThreadID.x == 0 )
	{
		// 3.1] First, the ambient term needs to be computed by a product of the probe's occlusion SH and the ambient SH to get the occluded sky SH
		float3	OccludedAmbientSH[9];
		SHProduct( _AmbientSH, Probe.SHOcclusion, OccludedAmbientSH );

		// 3.2] Then accumulate static + ambient + dynamic + emissive SH
		for ( int i=0; i < 9; i++ )
		{
#if 1
			float3	SHCoeff = OccludedAmbientSH[i] + Probe.SHStatic[i];
			for ( uint j=0; j < Probe.SetsCount; j++ )
				SHCoeff += gsSetSH[9*j+i];
			for ( uint j=0; j < Probe.EmissiveSetsCount; j++ )
				SHCoeff += gsEmissiveSetSH[9*j+i];
#else
			// Debug sets' SH
			float3	SHCoeff = 0.0;
			for ( uint j=0; j < Probe.SetsCount; j++ )
			{
				ProbeUpdateSetInfos	Set = _SBProbeUpdateSetInfos[Probe.SetsStart + j];
				SHCoeff += 100.0 * Set.SH[i];
			}
#endif

			// Store result and we're done!
			_Output[Probe.Index].SH[i] = SHCoeff;

//_Output[Probe.Index].SH[i] = 0.5 * (gsSamplingPointIrradiance[63]);
//_Output[Probe.Index].SH[i] = 0.5 * (gsSetSH[9*0+i]);
//_Output[Probe.Index].SH[i] = 0.5 * (Probe.Sets[3].SamplingPointsCount / 64.0);
		}
	}
}
