//////////////////////////////////////////////////////////////////////////
// This shader displays the Global Illumination test room
// It's the second version of the GI test that uses direct probes instead of the complicated stuff I imagined earlier!
//
#include "Inc/Global.hlsl"
#include "Inc/SH.hlsl"
#include "Inc/ShadowMap.hlsl"

//[
cbuffer	cbGeneral	: register( b9 )
{
	bool		_ShowIndirect;
};
//]

//[
cbuffer	cbScene	: register( b10 )
{
	uint		_LightsCount;
	uint		_ProbesCount;
};
//]

//[
cbuffer	cbObject	: register( b11 )
{
	float4x4	_Local2World;
};
//]

//[
cbuffer	cbMaterial	: register( b12 )
{
	float3		_DiffuseAlbedo;
	bool		_HasDiffuseTexture;
	float3		_SpecularAlbedo;
	bool		_HasSpecularTexture;
	float		_SpecularExponent;
};
//]


Texture2D<float4>	_TexDiffuseAlbedo : register( t10 );
Texture2D<float4>	_TexSpecularAlbedo : register( t11 );

// DEBUG!
TextureCube<float4>	_TexCubemapProbe0 : register( t64 );
TextureCube<float4>	_TexCubemapProbe1 : register( t65 );


// Structured Buffers with our lights & probes
struct	LightStruct
{
	float3		Position;
	float3		Color;
	float		Radius;	// Light radius to compute the solid angle for the probe injection
};
StructuredBuffer<LightStruct>	_SBLights : register( t8 );

// This tiny probe struct is only 120 bytes long!! \o/ ^^
struct	ProbeStruct
{
	float3		Position;
	float		Radius;
	float3		SHBounce[9];
};
StructuredBuffer<ProbeStruct>	_SBProbes : register( t9 );



struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float3	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	BiTangent	: BITANGENT;
	float3	UV			: TEXCOORD0;

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

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	Out.Tangent = mul( float4( _In.Tangent, 0.0 ), _Local2World ).xyz;
	Out.BiTangent = mul( float4( _In.BiTangent, 0.0 ), _Local2World ).xyz;
	Out.UV = _In.UV;


	// Iterate over all the probes and do a weighted sum based on their distance to the vertex's position
	float	SumWeights = 0.0;
	float3	SH[9];
	for ( int i=0; i < 9; i++ )
		SH[i] = 0.0;
	for ( uint ProbeIndex=0; ProbeIndex < _ProbesCount; ProbeIndex++ )
//for ( uint ProbeIndex=0; ProbeIndex < 1; ProbeIndex++ )
	{
		ProbeStruct	Probe = _SBProbes[ProbeIndex];

		float3	ToProbe = Probe.Position - Out.Position;
		float	Distance2Probe = length( ToProbe );
				ToProbe /= Distance2Probe;

		// Weight by distance
// 		const float	MEAN_HARMONIC_DISTANCE = 4.0;
// 		const float	WEIGHT_AT_DISTANCE = 0.01;
// 		const float	EXP_FACTOR = log( WEIGHT_AT_DISTANCE ) / (MEAN_HARMONIC_DISTANCE * MEAN_HARMONIC_DISTANCE);
// 		float	ProbeWeight = exp( EXP_FACTOR * Distance2Probe * Distance2Probe );

		float	ProbeWeight = pow( max( 0.01, Distance2Probe ), -3.0 );

		// Also weight by orientation to avoid probes facing away from us
		ProbeWeight *= saturate( lerp( -0.98, 1.0, 0.5 * (1.0 + dot( Out.Normal, ToProbe )) ) );


//ProbeWeight = 1;

// if ( ProbeIndex == 1 )
// {
// 	Out.SH0 = ProbeWeight;
// //	Out.SH0 = Probe.Radius;
// 	return Out;
// }

		for ( int i=0; i < 9; i++ )
			SH[i] += ProbeWeight * Probe.SHBounce[i];

		SumWeights += ProbeWeight;
	}

	// Normalize & store
	float	Norm = 1.0 / SumWeights;

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
//return float4( _In.SH0, 0 );


//	clip( 0.5 - _HasDiffuseTexture );
	float3	DiffuseAlbedo = _DiffuseAlbedo;
	if ( _HasDiffuseTexture )
		DiffuseAlbedo = _TexDiffuseAlbedo.Sample( LinearWrap, _In.UV.xy ).xyz;

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


	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	float3	View = normalize( _In.Position - _Camera2World[3].xyz );
	float3	Normal = _In.Normal;

	float3	AccumDiffuse = 0.0;
	float3	AccumSpecular = 0.0;
	for ( int LightIndex=0; LightIndex < _LightsCount; LightIndex++ )
	{
		LightStruct	LightSource = _SBLights[LightIndex];

		float3	Irradiance;
		float3	Light;
		if ( LightSource.Radius >= 0.0 )
		{	// Compute a standard point light
			Light = LightSource.Position - _In.Position;
			float	Distance2Light = length( Light );
			float	InvDistance2Light = 1.0 / Distance2Light;
			Light *= InvDistance2Light;

			Irradiance = LightSource.Color * InvDistance2Light * InvDistance2Light;
		}
		else
		{	// Compute a sneaky directional with shadow map
			Light = LightSource.Position;	// We're directly given the direction here
			Irradiance = LightSource.Color;	// Simple!

			Irradiance *= ComputeShadow( _In.Position, 0.0 );
		}

		float	NdotL = saturate( dot( Normal, Light ) );
		AccumDiffuse += Irradiance * NdotL;
	}
	AccumDiffuse *= DiffuseAlbedo;

//return float4( _SBLights[0].Position, 0 );

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting
	float3	SHIndirect[9] = { _In.SH0, _In.SH1, _In.SH2, _In.SH3, _In.SH4, _In.SH5, _In.SH6, _In.SH7, _In.SH8 };
	float3	Indirect = DiffuseAlbedo * EvaluateSHIrradiance( Normal, SHIndirect );
//	float3	Indirect = DiffuseAlbedo * EvaluateSH( Normal, SHIndirect );

//return float4( _In.SH0, 0 );

AccumDiffuse *= 1.0;
Indirect *= _ShowIndirect ? 1.0 : 0.0;

//Indirect *= _In.__Position.x < 1280.0/2.0 ? 1.0 : 0.0;

	return float4( Indirect + AccumDiffuse, 0 );
}
