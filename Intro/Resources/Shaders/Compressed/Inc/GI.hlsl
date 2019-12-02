////////////////////////////////////////////////////////////////////////////////////////
// Global Illumination Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _GI_INC_
#define _GI_INC_

// City settings
// static const float	SHADOW_NORMAL_OFFSET = 0.1;	// City scene seems to require that amount of offseting to have "correct" shadows
// static const float	SHADOW_PCF_DISC_RADIUS = 0.02;

// Sponza
static const float	SHADOW_NORMAL_OFFSET = 0.05;
static const float	SHADOW_PCF_DISC_RADIUS = 0.01;

#if USE_SHADOW_MAP
#include "Inc/ShadowMap.hlsl"
#endif


////////////////////////////////////////////////////////////////////////////////////////
// Scene vertex format
struct	SCENE_VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float2	UV			: TEXCOORD0;

#ifdef PER_VERTEX_PROBE_ID
	uint	ProbeID		: INFO;
#endif
};


////////////////////////////////////////////////////////////////////////////////////////
// Constant Buffers

// Scene descriptor
cbuffer	cbScene	: register( b9 )
{
	uint		_StaticLightsCount;
	uint		_DynamicLightsCount;
	uint		_ProbesCount;
};

// Object descriptor
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};

// Material descriptor
cbuffer	cbMaterial	: register( b11 )
{
	uint		_MaterialID;
	float3		_DiffuseAlbedo;

	bool		_HasDiffuseTexture;
	float3		_SpecularAlbedo;

	bool		_HasSpecularTexture;
	float3		_EmissiveColor;

	float		_SpecularExponent;
	uint		_FaceOffset;	// The offset to apply to the object's face index
	bool		_HasNormalTexture;
};

////////////////////////////////////////////////////////////////////////////////////////
// Structured Buffers
struct	LightStruct {
	uint	Type;
	float3	Position;
	float3	Direction;
	float3	Color;
	float4	Parms;						// X=Falloff radius, Y=Cutoff radius, Z=Cos(Falloff angle), W=Cos(Cutoff angle)
};

struct SHCoeffs3 {
	float3	SH[9];
};
struct SHCoeffs1 {
	float	SH[9];
};

// Contains probe information
struct	ProbeStruct {
	float3		Position;
	float		Radius;
	uint		NeighborsOffset;
	uint		NeighborsCount;
};

struct NeighborsStruct {
	uint		ProbeID;
	float3		Position;
};

StructuredBuffer<LightStruct>		_SBLightsStatic : register( t5 );	// Structured Buffer with our static lights
StructuredBuffer<LightStruct>		_SBLightsDynamic : register( t6 );	// Structured Buffer with our dynamic lights
StructuredBuffer<ProbeStruct>		_SBProbes : register( t7 );			// Structured Buffer with our probes info (position + radius)
StructuredBuffer<SHCoeffs3>			_SBProbeSH : register( t8 );		// Structured Buffer with our probes SH coefficients
StructuredBuffer<NeighborsStruct>	_SBNeighborProbes : register( t9 );	// Structured Buffer with our neighbor probes info (ID + position)


// Optional textures associated to the material
Texture2D<float4>	_TexDiffuseAlbedo : register( t10 );
Texture2D<float4>	_TexNormal : register( t11 );
Texture2D<float4>	_TexSpecularAlbedo : register( t12 );


////////////////////////////////////////////////////////////////////////////////////////
// Computes light's irradiance
float3	AccumulateLight( float3 _wsPosition, float3 _wsNormal, float3 _WorldVertexNormal, float3 _WorldVertexTangent, LightStruct _LightSource )
{
	float3	Irradiance = 0.0;
	float3	Light = 0.0;

	if ( _LightSource.Type == 0 || _LightSource.Type == 2 )
	{	// Compute a standard point light
		Light = _LightSource.Position - _wsPosition;
		float	Distance2Light = length( Light );
		float	InvDistance2Light = 1.0 / Distance2Light;
		Light *= InvDistance2Light;

		Irradiance = _LightSource.Color * InvDistance2Light * InvDistance2Light;

		if ( _LightSource.Type == 2 )
		{	// Account for spots' angular falloff
			float	LdotD = -dot( Light, _LightSource.Direction );
			Irradiance *= smoothstep( _LightSource.Parms.w, _LightSource.Parms.z, LdotD );
		}

#if USE_SHADOW_MAP
		if ( _LightSource.Type == 0 && dot( _wsNormal, Light ) > 0.0 )
			Irradiance *= ComputeShadowPoint( _wsPosition, _WorldVertexNormal, SHADOW_NORMAL_OFFSET );
// if ( _LightSource.Type == 0 )
// return float3( ComputeShadowPoint( _wsPosition, _WorldVertexNormal, SHADOW_NORMAL_OFFSET ), 0 );
#endif
	}
	else if ( _LightSource.Type == 1 )
	{	// Compute a sneaky directional with shadow map
		Light = _LightSource.Direction;
		Irradiance = _LightSource.Color;	// Simple!

#if USE_SHADOW_MAP
		Irradiance *= ComputeShadowPCF( _wsPosition, _WorldVertexNormal, _WorldVertexTangent, SHADOW_PCF_DISC_RADIUS, SHADOW_NORMAL_OFFSET );
#endif
	}

	float	NdotL = saturate( dot( _wsNormal, Light ) );

	return Irradiance * NdotL;
}


////////////////////////////////////////////////////////////////////////////////////////
// Accumulates probe SH based on weight computed from position & normal
void	AccumulateProbeInfluence( ProbeStruct _Probe, uint _ProbeID, float3 _wsPosition, float3 _wsNormal, inout float3 _SH[9], inout float _SumWeights ) {
	float3	ToProbe = _Probe.Position - _wsPosition;
	float	Distance2Probe = length( ToProbe );
			ToProbe /= Distance2Probe;

	float	ProbeRadius = 1.0 * _Probe.Radius;

	// Weight by distance
// 	const float	MEAN_HARMONIC_DISTANCE = 4.0;
// 	const float	WEIGHT_AT_DISTANCE = 0.01;
// 	const float	EXP_FACTOR = log( WEIGHT_AT_DISTANCE ) / (MEAN_HARMONIC_DISTANCE * MEAN_HARMONIC_DISTANCE);
// 	float	ProbeWeight = exp( EXP_FACTOR * Distance2Probe * Distance2Probe );

//	float	ProbeWeight = pow( max( 0.01, Distance2Probe ), -3.0 );

	// Weight based on probe's max distance
	const float	WEIGHT_AT_DISTANCE = 0.1;
 	const float	EXP_FACTOR = log( WEIGHT_AT_DISTANCE ) / (ProbeRadius * ProbeRadius);
//###	float	ProbeWeight = 2.0 * exp( EXP_FACTOR * Distance2Probe * Distance2Probe );
	float	ProbeWeight = 10.0 * exp( EXP_FACTOR * Distance2Probe * Distance2Probe );

	// Also weight by orientation to avoid probes facing away from us
	ProbeWeight *= saturate( lerp( -0.1, 1.0, 0.5 * (1.0 + dot( _wsNormal, ToProbe )) ) );

	// Accumulate SH
	SHCoeffs3	ProbeSH = _SBProbeSH[_ProbeID];
	for ( int i=0; i < 9; i++ )
		_SH[i] += ProbeWeight * ProbeSH.SH[i];

	_SumWeights += ProbeWeight;
}


////////////////////////////////////////////////////////////////////////////////////////
// Gather SH from the specified probe ID and its direct neighbors
void	GatherProbeSH_OLD( float3 _Position, float3 _Normal, uint _ProbeID, inout SHCoeffs3 _SH ) {
	[unroll]
	for ( uint i=0; i < 9; i++ )
		_SH.SH[i] = 0.0;

	float	SumWeights = 0.0;

	// Accumulate the main probe
	ProbeStruct	OriginProbe = _SBProbes[_ProbeID];
	AccumulateProbeInfluence( OriginProbe, _ProbeID, _Position, _Normal, _SH, SumWeights );


#if 0
	// Then accumulate valid neighbors
	uint	NeighborProbeID = OriginProbe.NeighborIDs.x & 0xFFFF;
	if ( NeighborProbeID != 0xFFFF )
		AccumulateProbeInfluence( _SBProbes[NeighborProbeID], NeighborProbeID, _Position, _Normal, _SH, SumWeights );

	NeighborProbeID = OriginProbe.NeighborIDs.x >> 16;
	if ( NeighborProbeID != 0xFFFF )
		AccumulateProbeInfluence( _SBProbes[NeighborProbeID], NeighborProbeID, _Position, _Normal, _SH, SumWeights );

	NeighborProbeID = OriginProbe.NeighborIDs.y & 0xFFFF;
	if ( NeighborProbeID != 0xFFFF )
		AccumulateProbeInfluence( _SBProbes[NeighborProbeID], NeighborProbeID, _Position, _Normal, _SH, SumWeights );

	NeighborProbeID = OriginProbe.NeighborIDs.y >> 16;
	if ( NeighborProbeID != 0xFFFF )
		AccumulateProbeInfluence( _SBProbes[NeighborProbeID], NeighborProbeID, _Position, _Normal, _SH, SumWeights );
#endif


	// Normalize
	float	Norm = 1.0 / (1e-5 + SumWeights);
//	float	Norm = 1.0 / max( 1.0, SumWeights );	// This max allows single, low influence probes to decrease with distance anyway
													// But it correctly averages influences when many probes have strong weight
	[unroll]
	for ( uint i=0; i < 9; i++ )
		_SH.SH[i] *= Norm;
}


// Stolen from http://www.iquilezles.org/www/articles/smin/smin.htm
float	SmoothMin( float a, float b ) {
#if 0	// Exponential version
	const float	k = -16.0;
	float	res = exp( k*a ) + exp( k*b );
	return log( res ) / k;
#elif 0	// Power version
	const float k = 16.0;
	a = pow( saturate(a), k );
	b = pow( saturate(b), k );
	return pow( (a*b) / (a+b), 1.0/k );
#else
	return min( a, b );
#endif
}

void	GatherProbeSH( float3 _Position, float3 _Normal, uint _ProbeID, inout SHCoeffs3 _SH ) {
	[unroll]
	for ( uint i=0; i < 9; i++ )
		_SH.SH[i] = 0.0;

	uint	ProbeIDs[16];
	float	Weights[16] = { 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6 };

	ProbeStruct	OriginProbe = _SBProbes[_ProbeID];
	ProbeIDs[0] = _ProbeID;

	[loop]
	for ( uint n=0; n < OriginProbe.NeighborsCount; n++ ) {
		NeighborsStruct	NeighborInfo = _SBNeighborProbes[OriginProbe.NeighborsOffset+n];
		ProbeIDs[1+n] = NeighborInfo.ProbeID;

		// Compute bidirectional weight
		float3	Normal = NeighborInfo.Position - OriginProbe.Position;
		float	Distance = length( Normal );
				Normal /= Distance;

 		float	Weight0 = dot( NeighborInfo.Position - _Position, Normal ) / Distance;
 		float	Weight1 = dot( _Position - OriginProbe.Position, Normal ) / Distance;

		Weights[0] = SmoothMin( Weights[0], Weight0 );
		Weights[1+n] = SmoothMin( Weights[1+n], Weight1 );

//Weights[1+n] = 0.0;
	}

	// Accumulate SH
	float	SumWeights = 0.0;

	[loop]
	for ( uint n=0; n <= OriginProbe.NeighborsCount; n++ ) {
		SHCoeffs3	ProbeSH = _SBProbeSH[ProbeIDs[n]];
		float		Weight = saturate( Weights[n] );
		SumWeights += Weight;

		[unroll]
		for ( uint i=0; i < 9; i++ )
			_SH.SH[i] += Weight * ProbeSH.SH[i];
	}


	// Normalize
	float	Norm = 1.0 / (1e-5 + SumWeights);
//	float	Norm = 1.0 / max( 1.0, SumWeights );	// This max allows single, low influence probes to decrease with distance anyway
													// But it correctly averages influences when many probes have strong weight
	[unroll]
	for ( uint i=0; i < 9; i++ )
		_SH.SH[i] *= Norm;
}

#endif	// _GI_INC_
