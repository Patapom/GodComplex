//////////////////////////////////////////////////////////////////////////
// This shader displays the Global Illumination test room
//
#include "Inc/Global.hlsl"
#include "Inc/SH.hlsl"

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

struct	ProbeStruct
{
	float3		Position;
	float		InfluenceDistance;
	float3		SHBounce[9];
	float3		SHLight[9];
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

#if 0	// BOUNCED LIGHT

	// Iterate over all the probes and do a weighted sum based on their distance to the vertex's position
	float	SumWeights = 0.0;
	float3	SH[9];
	for ( int i=0; i < 9; i++ )
		SH[i] = 0.0;
	for ( int ProbeIndex=0; ProbeIndex < _ProbesCount; ProbeIndex++ )
//	for ( int ProbeIndex=5; ProbeIndex < 6; ProbeIndex++ )
	{
		ProbeStruct	Probe = _SBProbes[ProbeIndex];

		float	Distance2Probe = length( Probe.Position - Out.Position );
//		float	ProbeWeight = 1.0 / (1.0 + 1.0 * Distance2Probe );

		const float	WEIGHT_AT_DISTANCE = 0.1;
		const float	EXP_FACTOR = log( WEIGHT_AT_DISTANCE ) / (Probe.InfluenceDistance * Probe.InfluenceDistance);
		float	ProbeWeight = exp( EXP_FACTOR * Distance2Probe * Distance2Probe );

		for ( int i=0; i < 9; i++ )
			SH[i] += ProbeWeight * Probe.SHBounce[i];
//SH[0] = ProbeWeight;

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

//Out.SH0 = 1.0 / (1.0 + 100.0 * length( _SBProbes[0].Position - Out.Position ));
//Out.SH0 = 1.0 / (1.0 + 100.0 * length( _SBProbes[0].Position - Out.Position ));

#else	// COMPUTE THE BOUNCE OURSELVES

	// Iterate over all the probes and do a weighted sum based on their distance to the vertex's position
	float	SumWeights = 0.0;
	float4	SHBounce[9];
	float4	SHLight[9];
	for ( int i=0; i < 9; i++ )
	{
		SHBounce[i] = 0.0;
		SHLight[i] = 0.0;
	}
	for ( int ProbeIndex=0; ProbeIndex < _ProbesCount; ProbeIndex++ )
//	for ( int ProbeIndex=0; ProbeIndex < 1; ProbeIndex++ )
	{
		ProbeStruct	Probe = _SBProbes[ProbeIndex];

		float	Distance2Probe = length( Probe.Position - Out.Position );
		float	InfluenceDistance = 2.0 * Probe.InfluenceDistance;

		const float	WEIGHT_AT_DISTANCE = 0.1;
		const float	EXP_FACTOR = log( WEIGHT_AT_DISTANCE ) / (InfluenceDistance * InfluenceDistance);
		float	ProbeWeight = exp( EXP_FACTOR * Distance2Probe * Distance2Probe );

		for ( int i=0; i < 9; i++ )
		{
			SHBounce[i] += ProbeWeight * float4( Probe.SHBounce[i], 0 );
			SHLight[i] += ProbeWeight * float4( Probe.SHLight[i], 0 );
		}

		SumWeights += ProbeWeight;
	}

	// Product
	float4	SH[9];
	SHProduct( SHBounce, SHLight, SH );

	// Normalize & store
	float	Norm = 1.0 / SumWeights;
	Out.SH0 = Norm * SH[0].xyz;
	Out.SH1 = Norm * SH[1].xyz;
	Out.SH2 = Norm * SH[2].xyz;
	Out.SH3 = Norm * SH[3].xyz;
	Out.SH4 = Norm * SH[4].xyz;
	Out.SH5 = Norm * SH[5].xyz;
	Out.SH6 = Norm * SH[6].xyz;
	Out.SH7 = Norm * SH[7].xyz;
	Out.SH8 = Norm * SH[8].xyz;

#endif

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

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	float3	View = normalize( _In.Position - _Camera2World[3].xyz );
	float3	Normal = _In.Normal;

	float3	AccumDiffuse = 0.0;
	float3	AccumSpecular = 0.0;
	for ( int LightIndex=0; LightIndex < _LightsCount; LightIndex++ )
	{
		LightStruct	LightSource = _SBLights[LightIndex];

		float3	Light = LightSource.Position - _In.Position;
		float	Distance2Light = length( Light );
		float	InvDistance2Light = 1.0 / Distance2Light;
		Light *= InvDistance2Light;

		float3	Irradiance = LightSource.Color * InvDistance2Light * InvDistance2Light;

		float	NdotL = saturate( dot( Normal, Light ) );
		AccumDiffuse += Irradiance * NdotL;
	}
	AccumDiffuse *= DiffuseAlbedo;

//return float4( _SBLights[0].Position, 0 );

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting
	float3	SHIndirect[9] = { _In.SH0, _In.SH1, _In.SH2, _In.SH3, _In.SH4, _In.SH5, _In.SH6, _In.SH7, _In.SH8 };
	float3	Indirect = EvaluateSHIrradiance( Normal, SHIndirect );

AccumDiffuse *= 1.0;
Indirect *= 1.0;

//Indirect *= _In.__Position.x < 1280.0/2.0 ? 1.0 : 0.0;

	return float4( Indirect + AccumDiffuse, 0 );
}
