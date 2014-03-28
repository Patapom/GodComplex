//////////////////////////////////////////////////////////////////////////
// This shader displays the Global Illumination test room
// It's the second version of the GI test that uses direct probes instead of the complicated stuff I imagined earlier!
//
#include "Inc/Global.hlsl"
#include "Inc/SH.hlsl"
#include "Inc/ShadowMap.hlsl"
#include "Inc/GI.hlsl"

cbuffer	cbGeneral	: register( b8 )
{
	float3		_Ambient;		// Default ambient if no indirect is being used
	bool		_ShowIndirect;
	bool		_ShowOnlyIndirect;
	bool		_ShowWhiteDiffuse;
	bool		_ShowVertexProbeID;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float2	UV			: TEXCOORD0;

	float3	SH0			: SH0;
	float3	SH1			: SH1;
	float3	SH2			: SH2;
	float3	SH3			: SH3;
	float3	SH4			: SH4;
	float3	SH5			: SH5;
	float3	SH6			: SH6;
	float3	SH7			: SH7;
	float3	SH8			: SH8;
};

void	AccumulateProbeInfluence( ProbeStruct _Probe, float3 _WorldPosition, float3 _WorldNormal, inout float3 _SH[9], inout float _SumWeights )
{
	float3	ToProbe = _Probe.Position - _WorldPosition;
	float	Distance2Probe = length( ToProbe );
			ToProbe /= Distance2Probe;

	float	ProbeRadius = 2.0 * _Probe.Radius;
//	float	ProbeRadius = 1.0 * _Probe.Radius;

	// Weight by distance
// 	const float	MEAN_HARMONIC_DISTANCE = 4.0;
// 	const float	WEIGHT_AT_DISTANCE = 0.01;
// 	const float	EXP_FACTOR = log( WEIGHT_AT_DISTANCE ) / (MEAN_HARMONIC_DISTANCE * MEAN_HARMONIC_DISTANCE);
// 	float	ProbeWeight = exp( EXP_FACTOR * Distance2Probe * Distance2Probe );

//	float	ProbeWeight = pow( max( 0.01, Distance2Probe ), -3.0 );

	// Weight based on probe's max distance
	const float	WEIGHT_AT_DISTANCE = 0.05;
 	const float	EXP_FACTOR = log( WEIGHT_AT_DISTANCE ) / (ProbeRadius * ProbeRadius);
//###	float	ProbeWeight = 2.0 * exp( EXP_FACTOR * Distance2Probe * Distance2Probe );
	float	ProbeWeight = 10.0 * exp( EXP_FACTOR * Distance2Probe * Distance2Probe );

	// Also weight by orientation to avoid probes facing away from us
	ProbeWeight *= saturate( lerp( -0.1, 1.0, 0.5 * (1.0 + dot( _WorldNormal, ToProbe )) ) );


//ProbeWeight = 1;

// if ( ProbeIndex == 17 )
// {
// 	Out.SH0 = ProbeWeight;
// 	Out.SH1 = Probe.Radius;
// 	Out.SH2 = Probe.Position;
// 	return Out;
// }

	for ( int i=0; i < 9; i++ )
		_SH[i] += ProbeWeight * _Probe.SH[i];

	_SumWeights += ProbeWeight;
}

PS_IN	VS( SCENE_VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	Out.Tangent = mul( float4( _In.Tangent, 0.0 ), _Local2World ).xyz;
	Out.BiTangent = mul( float4( -_In.BiTangent, 0.0 ), _Local2World ).xyz;
	Out.UV = _In.UV;

	float3	Normal = normalize( Out.Normal );

	// Iterate over all the probes and do a weighted sum based on their distance to the vertex's position
	float	SumWeights = 0.0;
	float3	SH[9];
	for ( int i=0; i < 9; i++ )
		SH[i] = 0.0;

#ifdef PER_VERTEX_PROBE_ID	// Only use the entry point probe and neighbors

	if ( _In.ProbeID != 0xFFFFFFFF )
	{	// We have an entry point into the probes network!
//		SH[0] = _In.ProbeID;

		// Accumulate this probe
		ProbeStruct	OriginProbe = _SBProbes[_In.ProbeID];
		AccumulateProbeInfluence( OriginProbe, Out.Position, Normal, SH, SumWeights );

		// Then accumulate valid neighbors
		uint	NeighborProbeID = OriginProbe.NeighborIDs.x & 0xFFFF;
		if ( NeighborProbeID != 0xFFFF )
			AccumulateProbeInfluence( _SBProbes[NeighborProbeID], Out.Position, Normal, SH, SumWeights );

		NeighborProbeID = OriginProbe.NeighborIDs.x >> 16;
		if ( NeighborProbeID != 0xFFFF )
			AccumulateProbeInfluence( _SBProbes[NeighborProbeID], Out.Position, Normal, SH, SumWeights );

		NeighborProbeID = OriginProbe.NeighborIDs.y & 0xFFFF;
		if ( NeighborProbeID != 0xFFFF )
			AccumulateProbeInfluence( _SBProbes[NeighborProbeID], Out.Position, Normal, SH, SumWeights );

		NeighborProbeID = OriginProbe.NeighborIDs.y >> 16;
		if ( NeighborProbeID != 0xFFFF )
			AccumulateProbeInfluence( _SBProbes[NeighborProbeID], Out.Position, Normal, SH, SumWeights );

		if ( _ShowVertexProbeID )
			SH[0] = _In.ProbeID;
	}

#else	// SUM ALL THE SCENE'S PROBES!

	for ( uint ProbeIndex=0; ProbeIndex < _ProbesCount; ProbeIndex++ )
//for ( uint ProbeIndex=0; ProbeIndex < 1; ProbeIndex++ )
	{
		ProbeStruct	Probe = _SBProbes[ProbeIndex];
		AccumulateProbeInfluence( Probe, Out.Position, Normal, SH, SumWeights );
	}

#endif

	// Normalize & store
//	float	Norm = 1.0 / SumWeights;
	float	Norm = 1.0 / max( 1.0, SumWeights );	// This max allows single, low influence probes to decrease with distance anyway
													// But it correctly averages influences when many probes have strong weight

	Out.SH0 = Norm * SH[0];
	Out.SH1 = Norm * SH[1];
	Out.SH2 = Norm * SH[2];
	Out.SH3 = Norm * SH[3];
	Out.SH4 = Norm * SH[4];
	Out.SH5 = Norm * SH[5];
	Out.SH6 = Norm * SH[6];
	Out.SH7 = Norm * SH[7];
	Out.SH8 = Norm * SH[8];

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//return float4( 0.01 * _In.SH0, 0 );
// return float4( 0.01 * _In.SH1, 0 );

	if ( _ShowVertexProbeID )
		return float4( (1+(int3( _In.SH0.xxx ) & 0x7)) / 8.0, 1 );

#if EMISSIVE

//return float4( 1, 0, 0, 1 );
	return float4( 0.1 * _EmissiveColor, 1 );

#else

//	clip( 0.5 - _HasDiffuseTexture );
	float3	DiffuseAlbedo = _DiffuseAlbedo;
	if ( _HasDiffuseTexture )
		DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV.xy ).xyz;
	if ( _ShowWhiteDiffuse )
		DiffuseAlbedo = 0.25;

	DiffuseAlbedo *= INVPI;

//	return float4( DiffuseAlbedo, 1 );
//	return float4( normalize( _In.Normal ), 1 );


// Debug shadow map
//float4	ShadowMapPos = World2ShadowMapProj( _In.Position );
//return (ShadowMapPos.w - _ShadowBoundMin.z) / (_ShadowBoundMax.z - _ShadowBoundMin.z);
//return ShadowMapPos.z / _ShadowBoundMax.z;
//return ShadowMapPos.z / ShadowMapPos.w;
//return 1.0 * _ShadowMap.SampleLevel( LinearClamp, 0.5 * (1.0 + ShadowMapPos.xy), 0.0 ).x;
//return float4( ShadowMapPos.xy, 0, 0 );

	float3	tsNormal = _HasNormalTexture ? 2.0 * (_TexNormal.Sample( LinearWrap, _In.UV.xy ).xyz - 0.5) : float3( 0, 0, 1 );

//tsNormal = float3( 0, 0, 1 );


	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	float3	View = normalize( _In.Position - _Camera2World[3].xyz );

	float3	VertexNormal = normalize( _In.Normal );
	float3	VertexTangent = normalize( _In.Tangent );
	float3	VertexBiTangent = normalize( _In.BiTangent );

	float3	Normal = normalize( tsNormal.x * VertexTangent + tsNormal.y * VertexBiTangent + tsNormal.z * VertexNormal );

//return float4( Normal, 1 );

	float3	AccumDiffuse = 0.0;
	float3	AccumSpecular = 0.0;

	// Process static lights
	for ( uint LightIndex=0; LightIndex < _StaticLightsCount; LightIndex++ )
	{
		LightStruct	LightSource = _SBLightsStatic[LightIndex];
		AccumDiffuse += AccumulateLight( _In.Position, Normal, VertexNormal, VertexTangent, LightSource );
	}

	// Process dynamic lights
	for ( uint LightIndex=0; LightIndex < _DynamicLightsCount; LightIndex++ )
	{
		LightStruct	LightSource = _SBLightsDynamic[LightIndex];
		AccumDiffuse += AccumulateLight( _In.Position, Normal, VertexNormal, VertexTangent, LightSource );
	}

	AccumDiffuse *= DiffuseAlbedo;

//return float4( _SBLights[0].Position, 0 );

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting
	float3	SHIndirect[9] = { _In.SH0, _In.SH1, _In.SH2, _In.SH3, _In.SH4, _In.SH5, _In.SH6, _In.SH7, _In.SH8 };
	float3	Indirect = DiffuseAlbedo * EvaluateSHIrradiance( _In.Normal, SHIndirect );
//	float3	Indirect = DiffuseAlbedo * EvaluateSH( _In.Normal, SHIndirect );


	AccumDiffuse *= _ShowOnlyIndirect ? 1.0 : 0.0;
//	Indirect *= _ShowIndirect ? 1.0 : 0.0;

	if ( !_ShowIndirect )
	{	// Dummy dull uniform ambient sky
		Indirect = _Ambient * DiffuseAlbedo * lerp( 0.5, 1.0, 0.5 * (1.0 + Normal.y) );
	}

	return float4( Indirect + AccumDiffuse, 1 );

#endif
}
