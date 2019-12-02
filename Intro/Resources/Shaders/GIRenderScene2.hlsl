//////////////////////////////////////////////////////////////////////////
// This shader displays the Global Illumination test room
// It's the second version of the GI test that uses direct probes instead of the complicated stuff I imagined earlier!
//
#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"
#include "Inc/GI.hlsl"
#include "Inc/SH.hlsl"


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



// Stolen from http://www.iquilezles.org/www/articles/smin/smin.htm
float	TEMPSmoothMin( float a, float b ) {
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


void	TEMPGatherProbeSH( float3 _Position, float3 _Normal, uint _ProbeID, inout SHCoeffs3 _SH ) {
	[unroll]
	for ( uint i=0; i < 9; i++ )
		_SH.SH[i] = 0.0;

	uint	ProbeIDs[16];
	float3	ProbePositions[16];
	float	Weights[16] = { 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6, 1e6 };

	ProbeStruct	OriginProbe = _SBProbes[_ProbeID];
	ProbeIDs[0] = _ProbeID;
	ProbePositions[0] = OriginProbe.Position;

	[loop]
	for ( uint n=0; n < OriginProbe.NeighborsCount; n++ ) {
		NeighborsStruct	NeighborInfo = _SBNeighborProbes[OriginProbe.NeighborsOffset+n];
		ProbeIDs[1+n] = NeighborInfo.ProbeID;
		ProbePositions[1+n] = NeighborInfo.Position;
	}

	// Compute bidirectional weights
	[loop]
	for ( uint i0=0; i0 < OriginProbe.NeighborsCount; i0++ ) {
		float3	P0 = ProbePositions[i0];
		[loop]
		for ( uint i1=i0+1; i1 <= OriginProbe.NeighborsCount; i1++ ) {
			float3	P1 = ProbePositions[i1];

			float3	Normal = P1 - P0;
			float	Distance = length( Normal );
					Normal /= Distance;

 			float	Weight0 = dot( P1 - _Position, Normal ) / Distance;
 			float	Weight1 = dot( _Position - P0, Normal ) / Distance;

			Weights[i0] = TEMPSmoothMin( Weights[i0], Weight0 );
			Weights[i1] = TEMPSmoothMin( Weights[i1], Weight1 );

//Weights[1+n] = 0.0;
		}
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

	SHCoeffs3	Coeffs;

#ifdef PER_VERTEX_PROBE_ID	// Only use the entry point probe and its direct neighbors
	if ( _In.ProbeID != 0xFFFFFFFF ) {
		// We have an entry point into the probes network!
//		SH[0] = _In.ProbeID;
		TEMPGatherProbeSH( Out.Position, Normal, _In.ProbeID, Coeffs );
		if ( _ShowVertexProbeID )
			Coeffs.SH[0] = _In.ProbeID;
	}
#else	// SUM ALL THE SCENE'S PROBES!
	// Iterate over all the probes and do a weighted sum based on their distance to the vertex's position
	[unroll]
	for ( uint i=0; i < 9; i++ )
		Coeffs.SH[i] = 0.0;

	float	SumWeights = 0.0;
	for ( uint ProbeIndex=0; ProbeIndex < _ProbesCount; ProbeIndex++ ) {
		ProbeStruct	Probe = _SBProbes[ProbeIndex];
		AccumulateProbeInfluence( Probe, ProbeIndex, Out.Position, Normal, Coeffs.SH, SumWeights );
	}

	// Normalize
//	float	Norm = 1.0 / SumWeights;
	float	Norm = 1.0 / max( 1.0, SumWeights );	// This max allows single, low influence probes to decrease with distance anyway
													// But it correctly averages influences when many probes have strong weight
	[unroll]
	for ( uint i=0; i < 9; i++ )
		Coeffs.SH[i] *= Norm;

#endif

	// Store for pixel shader
	Out.SH0 = Coeffs.SH[0];
	Out.SH1 = Coeffs.SH[1];
	Out.SH2 = Coeffs.SH[2];
	Out.SH3 = Coeffs.SH[3];
	Out.SH4 = Coeffs.SH[4];
	Out.SH5 = Coeffs.SH[5];
	Out.SH6 = Coeffs.SH[6];
	Out.SH7 = Coeffs.SH[7];
	Out.SH8 = Coeffs.SH[8];

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//return float4( 1, 0, 0, 1 );


//return float4( 0.01 * _In.SH0, 0 );
// return float4( 0.01 * _In.SH1, 0 );

	if ( _ShowVertexProbeID ) {
//		return float4( (1+(uint3( 0.5 + _In.SH0.xxx ) & 0x7)) / 8.0, 1 );

		static float3	PipoColors[8] = {
			float3( 1, 0, 0 ),
			float3( 1, 1, 0 ),
			float3( 0, 1, 0 ),
			float3( 0, 1, 1 ),
			float3( 0, 0, 1 ),
			float3( 1, 0, 1 ),
			float3( 1, 0.5, 0.5 ),
			float3( 0.5, 0.5, 1 ),
		};

		uint	ProbeID = uint( 0.5 + _In.SH0.x );
		return float4( PipoColors[ProbeID&7], 1 );
	}

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

	float3	tsNormal = _HasNormalTexture ? 2.0 * _TexNormal.Sample( LinearWrap, _In.UV.xy ).xyz - 1.0 : float3( 0, 0, 1 );

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
	{
		for ( uint LightIndex=0; LightIndex < _StaticLightsCount; LightIndex++ ) {
			LightStruct	LightSource = _SBLightsStatic[LightIndex];
			AccumDiffuse += AccumulateLight( _In.Position, Normal, VertexNormal, VertexTangent, LightSource );
		}
	}

	// Process dynamic lights
	{
		for ( uint LightIndex=0; LightIndex < _DynamicLightsCount; LightIndex++ ) {
			LightStruct	LightSource = _SBLightsDynamic[LightIndex];
			AccumDiffuse += AccumulateLight( _In.Position, Normal, VertexNormal, VertexTangent, LightSource );
//return AccumulateLight( _In.Position, Normal, VertexNormal, VertexTangent, LightSource ).x;
		}
	}

	AccumDiffuse *= DiffuseAlbedo;

//return float4( _SBLights[0].Position, 0 );

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting
	float3	SHIndirect[9] = { _In.SH0, _In.SH1, _In.SH2, _In.SH3, _In.SH4, _In.SH5, _In.SH6, _In.SH7, _In.SH8 };
	float3	Indirect = DiffuseAlbedo * EvaluateSHIrradiance( Normal, SHIndirect );
//	float3	Indirect = DiffuseAlbedo * EvaluateSH( Normal, SHIndirect );

	AccumDiffuse *= _ShowOnlyIndirect ? 1.0 : 0.0;
//	Indirect *= _ShowIndirect ? 1.0 : 0.0;

	if ( !_ShowIndirect )
		// Dummy dull uniform ambient sky
		Indirect = _Ambient * DiffuseAlbedo * lerp( 0.5, 1.0, 0.5 * (1.0 + Normal.y) );

	return float4( Indirect + AccumDiffuse, 1 );

#endif
}
