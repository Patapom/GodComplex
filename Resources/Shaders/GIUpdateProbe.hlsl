//////////////////////////////////////////////////////////////////////////
// This compute shader updates the dynamic probes
//
#include "Inc/Global.hlsl"
#include "Inc/GI.hlsl"
#include "Inc/SH.hlsl"
#include "Inc/ShadowMap.hlsl"

static const float	PCF_SHADOW_NORMAL_OFFSET = 0.1;	// Offset from surface to avoid shadow acnea
static const float	PCF_DISC_RADIUS_FACTOR = 1.0;	// Radius factor to go fetch PCF samples closer or further from sample's center


#define	THREADS_X	128						// The maximum amount of samples per probe (Must match the MAX_PROBE_SAMPLES define declared in ProbeEncoder.h!)
#define	THREADS_Y	1
#define	THREADS_Z	1

#define	MAX_PROBE_EMISSIVE_SURFACES	16		// Must match the MAX_PROBE_EMISSIVE_SURFACES define declared in the header file!
#define	MAX_PROBE_NEIGHBORS			4		// Must match the MAX_PROBE_NEIGHBORS define declared in the header file!


cbuffer	cbUpdateProbes : register(b10)
{
	float3	_AmbientSH[9];					// Ambient sky

	float	_SunBoost;
	float	_SkyBoost;
	float	_DynamicLightsBoost;
	float	_StaticLightingBoost;

	float	_EmissiveBoost;
	float	_NeighborProbesContributionBoost;
};

// Input probe structures
struct ProbeUpdateInfos
{
	uint		Index;						// The index of the probe we're updating
	uint		SamplesStart;				// Index of the first sample for the probe
	uint		SamplesCount;				// Amount of samples for the probe
	uint		EmissiveSurfacesStart;		// Index of the first emissive surface for the probe
	uint		EmissiveSurfacesCount;		// Amount of emissive surfaces for the probe
	float3		SHStatic[9];				// Precomputed static SH (static geometry + static lights)
	float		SHOcclusion[9];				// Directional ambient occlusion for the probe

	// Neighbor probes informations
//	uint4		NeighborProbeIDs;			// The IDs of the 4 most significant neighbor probes
	float4		SHConvolution[9];			// The SH coefficients to convolve the neighbor's SH with to obtain their contribution to this probe
};
StructuredBuffer<ProbeUpdateInfos>			_SBProbeUpdateInfos : register( t10 );

struct ProbeUpdateSampleInfo
{
	float3		Position;					// World position of the samples
	float3		Normal;						// World normal of the sample
	float		Radius;						// Radius of the sample's disc approximation
	float3		Albedo;						// Albedo of the sample's surface
	float		SH[9];						// SH contribution of the sample
};
StructuredBuffer<ProbeUpdateSampleInfo>		_SBProbeSamples : register( t11 );

struct	ProbeUpdateEmissiveSurfacesInfos
{
	float3		EmissiveColor;				// Color of the emissive material
	float		SH[9];						// SH for the set
};
StructuredBuffer<ProbeUpdateEmissiveSurfacesInfos>	_SBProbeUpdateEmissiveSurfacesInfos : register( t12 );


// Result structure (declared in Inc/GI.hlsl)
RWStructuredBuffer<ProbeStruct>	_Output : register( u0 );


groupshared float3	gs_SamplesSH[9*THREADS_X];
groupshared float3	gs_DynamicSH[9*MAX_PROBE_SETS];
groupshared float3	gs_EmissiveSurfaceSH[9*MAX_PROBE_EMISSIVE_SURFACES];
groupshared float3	gs_NeighborProbeSH[9*MAX_PROBE_NEIGHBORS];


// Computes the shadow value for the given posiion, accounting for a radius around the position for soft shadowing
float	ComputeShadowCS( float3 _WorldPosition, float3 _WorldNormal, float _Radius ) {
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

	_Radius *= PCF_DISC_RADIUS_FACTOR;

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
		float3	SamplePosition = _WorldPosition + SampleOffset.x * X + SampleOffset.y * Y + PCF_SHADOW_NORMAL_OFFSET * _WorldNormal;
		float4	ShadowPosition = World2ShadowMapProj( SamplePosition );

		float2	UV = 0.5 * float2( 1.0 + ShadowPosition.x, 1.0 - ShadowPosition.y );
		float	Zproj = ShadowPosition.z / ShadowPosition.w;

		float	ShadowZproj = _ShadowMap.SampleLevel( LinearClamp, UV, 0.0 );

		Shadow += ShadowZproj - Zproj > 1e-3 ? 1.0 : 0.0;
//		Shadow += saturate( 10.0 * (ShadowZproj - Zproj) );
	}

	return Shadow * (1.0 / SHADOW_SAMPLES_COUNT);
}


// float	ComputeShadowPointCS( float3 _WorldPosition, float3 _WorldVertexNormal, float _NormalOffset=0.01 )
// {
// 	float3	LocalPosition = _WorldPosition + _NormalOffset * _WorldVertexNormal - _ShadowPointLightPosition;
// 	float3	Abs = abs( LocalPosition );
// 	float	Max = max( max( Abs.x, Abs.y ), Abs.z );
// 	float3	Proj = LocalPosition / Max;
// 
// 	float4	UV = 0.0;
// 	if ( abs( Max - Abs.x ) < 1e-5 )
// 	{
// 		UV = LocalPosition.x > 0.0 ? float4( LocalPosition.z, LocalPosition.y, 0, LocalPosition.x ) : float4( -LocalPosition.z, LocalPosition.y, 1, -LocalPosition.x );
// 	}
// 	else if ( abs( Max - Abs.y ) < 1e-5 )
// 	{
// 		UV = LocalPosition.y > 0.0 ? float4( -LocalPosition.x, -LocalPosition.z, 2, LocalPosition.y ) : float4( -LocalPosition.x, LocalPosition.z, 3, -LocalPosition.y );
// 	}
// 	else //if ( Abs == Abs.z )
// 	{
// 		UV = LocalPosition.z > 0.0 ? float4( -LocalPosition.x, LocalPosition.y, 4, LocalPosition.z ) : float4( LocalPosition.x, LocalPosition.y, 5, -LocalPosition.z );
// 	}
// 
// 	UV.xy /= UV.w;
// 	UV.xy = 0.5 * (1.0 + float2( UV.x, -UV.y ));
// 
// 	float	Z = UV.w;
// 
// 	const float	NearClip = 0.5;
// 	const float	FarClip = _ShadowPointFarClip;
// 	const float	Q = FarClip / (FarClip - NearClip);
// 
// 	float	Zproj = Q * (1.0 - NearClip / Z);
// 
// 	float	Zshadow = _ShadowMapPoint.SampleCmpLevelZero( LinearClamp, UV.xyz );
// 
// 	return step( Zproj, Zshadow );
// }


// Each group processes a single probe
// Each thread processes a single sample and will later process sets and even later collapse SH into a single vector
[numthreads( THREADS_X, THREADS_Y, THREADS_Z )]
void	CS( uint3 _GroupID			: SV_GroupID,			// Defines the group offset within a Dispatch call, per dimension of the dispatch call
			uint3 _ThreadID			: SV_DispatchThreadID,	// Defines the global thread offset within the Dispatch call, per dimension of the group
			uint3 _GroupThreadID	: SV_GroupThreadID,		// Defines the thread offset within the group, per dimension of the group
			uint  _GroupIndex		: SV_GroupIndex )		// Provides a flattened index for a given thread within a given group
{
	int		ProbeIndex = _GroupID.x;

	ProbeUpdateInfos	Probe = _SBProbeUpdateInfos[ProbeIndex];


	//////////////////////////////////////////////////////////////////////////
	// 1] Process the samples

	// Determine which sample this thread should process
	uint	SampleIndex = _GroupThreadID.x;
	if ( SampleIndex < Probe.SamplesCount ) {
		ProbeUpdateSampleInfo	Sample = _SBProbeSamples[Probe.SamplesStart + SampleIndex];

		// Iterate on lights
// TODO: Light list per probe!! Don't process all lights every time!
		float3	SumIrradiance = 0.0;
		for ( uint LightIndex=0; LightIndex < _DynamicLightsCount; LightIndex++ ) {
			LightStruct	LightSource = _SBLightsDynamic[LightIndex];

			float3	Irradiance = 0.0;
			float3	Light = 0.0;
			if ( LightSource.Type == 0 || LightSource.Type == 2 ) {
				// Compute a standard point light
				Light = LightSource.Position - Sample.Position;
				float	Distance2Light = length( Light );
// 				float	InvDistance2Light = 1.0 / Distance2Light;
// 				Light *= InvDistance2Light;
//
//// Try and avoid highlights when lights get too close to the sample
//InvDistance2Light = min( 1.0, InvDistance2Light );

				Light /= Distance2Light;
				float	InvDistance2Light = 1.0 / max( 0.5, Distance2Light );	// Try and avoid highlights when lights get too close to the sample


				Irradiance = LightSource.Color * InvDistance2Light * InvDistance2Light;

				if ( LightSource.Type == 2 ) {
					// Account for spots' angular falloff
					float	LdotD = -dot( Light, LightSource.Direction );
					Irradiance *= smoothstep( LightSource.Parms.w, LightSource.Parms.z, LdotD );
				} else if ( LightSource.Type == 0 ) {
					// Account for point light shadowing
					Irradiance *= ComputeShadowPoint( Sample.Position, Sample.Normal, PCF_SHADOW_NORMAL_OFFSET );
				}

				Irradiance *= _DynamicLightsBoost;

			} else if ( LightSource.Type == 1 ) {
				// Compute a sneaky directional with shadow map
				Light = LightSource.Direction;
				Irradiance = LightSource.Color;	// Simple!
				Irradiance *= ComputeShadowCS( Sample.Position, Sample.Normal, Sample.Radius );

				Irradiance *= _SunBoost;
			}

			float	NdotL = saturate( dot( Sample.Normal, Light ) );
			SumIrradiance += Irradiance * NdotL;
		}

		// Encode into SH
		float3	Radiance = SumIrradiance * Sample.Albedo;	// Radiance = Sum( Rho/PI L0.(N.L) )
		for ( int i=0; i < 9; i++ ) {
			gs_SamplesIrradiance[9*SampleIndex+i] = Radiance * Sample.SH[i];
	}

	// Ensure all threads have finished computing their sample irradiance
	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////
	// 2] Process the emissive surfaces
	uint	EmissiveSetIndex = _GroupThreadID.x;
	if ( EmissiveSetIndex < Probe.EmissiveSurfacesCount ) {
		// Retrieve emissive set infos for that thread
		ProbeUpdateEmissiveSurfacesInfos	EmissiveSet = _SBProbeUpdateEmissiveSurfacesInfos[Probe.EmissiveSurfacesStart + EmissiveSetIndex];

		// Compute SH irradiance
		for ( int i=0; i < 9; i++ )
			gs_EmissiveSurfaceSH[9*EmissiveSetIndex+i] = EmissiveSet.EmissiveColor * EmissiveSet.SH[i];
	}

	// Ensure all threads have finished computing their set SH
	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////
	// 3] Use 4 threads to add contribution from neighbor probes
	uint	NeighborIndex = _GroupThreadID.x;
	if ( NeighborIndex < 4 ) {
		uint2	NeighborIDs = _Output[Probe.Index].NeighborIDs;	// Packed as X=11|00, Y=33|22
		uint	NeighborProbeID = 0xFFFF;
		float4	Swizzle;
		switch ( NeighborIndex )
		{
		case 0:	NeighborProbeID = NeighborIDs.x & 0xFFFF;			Swizzle = float4( 1, 0, 0, 0 ); break;
		case 1:	NeighborProbeID = (NeighborIDs.x >> 16) & 0xFFFF;	Swizzle = float4( 0, 1, 0, 0 ); break;
		case 2:	NeighborProbeID = NeighborIDs.y & 0xFFFF;			Swizzle = float4( 0, 0, 1, 0 ); break;
		case 3:	NeighborProbeID = (NeighborIDs.y >> 16) & 0xFFFF;	Swizzle = float4( 0, 0, 0, 1 ); break;
		}

		if ( NeighborProbeID < _ProbesCount )
		{
//			float3	NeighborProbeSH[9] = _SBProbes[NeighborProbeID].SH;	// Unless we use a double-buffer of probes?
			float3	NeighborProbeSH[9] = _Output[NeighborProbeID].SH;

			float	NeighborConvolution[9];
			for ( int i=0; i < 9; i++ )
				NeighborConvolution[i] = dot( Probe.SHConvolution[i], Swizzle );

			float3	PerceivedNeighborSH[9];
			SHProduct( NeighborProbeSH, NeighborConvolution, PerceivedNeighborSH );

			for ( int i=0; i < 9; i++ )
				gs_NeighborProbeSH[9*NeighborIndex+i] = PerceivedNeighborSH[i];
//				gs_NeighborProbeSH[9*NeighborIndex+i] = NeighborConvolution[i];	// Use this to show the exhanges
//				gs_NeighborProbeSH[9*NeighborIndex+i] = NeighborIndex == 0 && i == 0 ? NeighborProbeID : 0.0;	// Use this to show the neighbor's ID
//				gs_NeighborProbeSH[9*NeighborIndex+i] = NeighborIndex == 0 ? NeighborProbeSH[i] : 0.0;			// Use this to show the neighbor's SH
		}
		else
		{
			float3	Zero = NeighborProbeID == 0xFFFF ? 0.0 : float3( 1, 0, 1 );
			for ( int i=0; i < 9; i++ )
				gs_NeighborProbeSH[9*NeighborIndex+i] = Zero;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// 4] Accumulate the SH from all samples using a single thread
	if ( _GroupThreadID.x == 4 ) {
		for ( int i=0; i < 9; i++ )
			gs_DynamicSH[i] = 0.0;

		for ( SampleIndex=0; SampleIndex < Probe.SamplesCount; SampleIndex++ ) {
			for ( int i=0; i < 9; i++ )
				gs_DynamicSH[i] += gs_SamplesIrradiance[9*SampleIndex+i];
		}
	}

	// Ensure all threads have finished computing their set SH
	GroupMemoryBarrierWithGroupSync();


	//////////////////////////////////////////////////////////////////////////
	// 5] Use a single thread to finalize SH
	if ( _GroupThreadID.x == 0 ) {
		// 3.1] First, the ambient term needs to be computed by a product of the probe's occlusion SH and the ambient SH so it gets the occluded sky SH
		float3	OccludedAmbientSH[9];
		SHProduct( _AmbientSH, Probe.SHOcclusion, OccludedAmbientSH );

		// 3.2] Then accumulate static + ambient + neighbor + dynamic + emissive SH
		for ( int i=0; i < 9; i++ ) {
#if 1
			// Static + occluded sky
			float3	SHCoeff = abs(_SkyBoost) * OccludedAmbientSH[i] + _StaticLightingBoost * Probe.SHStatic[i];

			// + Neighbors
			SHCoeff += _NeighborProbesContributionBoost * (gs_NeighborProbeSH[9*0+i] + gs_NeighborProbeSH[9*1+i] + gs_NeighborProbeSH[9*2+i] + gs_NeighborProbeSH[9*3+i]);

			// + Dynamic lights
			SHCoeff += gs_DynamicSH[i];

			// + Emissive materials
			for ( uint j=0; j < Probe.EmissiveSurfacesCount; j++ )
				SHCoeff += _EmissiveBoost * gs_EmissiveSurfaceSH[9*j+i];
#else
			// Debug SH
			float3	SHCoeff = 0.0;
			for ( uint j=0; j < Probe.SamplesCount; j++ ) {
				ProbeUpdateSetInfos	Set = _SBProbeUpdateSetInfos[Probe.SamplesStart + j];
				SHCoeff += 100.0 * Set.SH[i];
			}
#endif

			// Store result and we're done!
			_Output[Probe.Index].SH[i] = SHCoeff;

//_Output[Probe.Index].SH[i] = 0.5 * (gs_SamplesIrradiance[63]);
//_Output[Probe.Index].SH[i] = 0.5 * (gs_DynamicSH[9*0+i]);
//_Output[Probe.Index].SH[i] = 0.5 * (Probe.Sets[3].SamplesCount / 64.0);
		}
	}
}
