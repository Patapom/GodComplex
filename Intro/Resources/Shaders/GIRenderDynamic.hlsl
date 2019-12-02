//////////////////////////////////////////////////////////////////////////
// This shader renders dynamic objects as balls with normals
//
#define USE_SHADOW_MAP	1

#include "Inc/Global.hlsl"
#include "Inc/ShadowMap.hlsl"
#include "Inc/GI.hlsl"
#include "Inc/SH.hlsl"

// Copied from GIRenderScene2.hlsl
cbuffer	cbGeneral	: register( b8 )
{
	float3		_Ambient;		// Default ambient if no indirect is being used
	bool		_ShowIndirect;
	bool		_ShowOnlyIndirect;
	bool		_ShowWhiteDiffuse;
	bool		_ShowVertexProbeID;
};

cbuffer	cbDynamic : register( b10 )
{
	float3	_Position;
	uint	_ProbeID;	// ID of the nearest probe
};

struct	VS_IN
{
	float3	Position	: POSITION;
 	float3	Normal		: NORMAL;
 	float3	Tangent		: TANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
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

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = float4( _Position + 0.5 * _In.Position, 1.0 );

	// Gather SH
	float3	SH[9];
	[unroll]
	for ( uint i=0; i < 9; i++ )
		SH[i] = 0.0;

	if ( _ProbeID != 0xFFFFFFFF )
		GatherProbeSH( WorldPosition.xyz, _In.Position, _ProbeID, SH );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = _In.Position;
	Out.Tangent = _In.Tangent;
	Out.UV = _In.UV;
	Out.SH0 = SH[0];
	Out.SH1 = SH[1];
	Out.SH2 = SH[2];
	Out.SH3 = SH[3];
	Out.SH4 = SH[4];
	Out.SH5 = SH[5];
	Out.SH6 = SH[6];
	Out.SH7 = SH[7];
	Out.SH8 = SH[8];

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	float3	DiffuseAlbedo = 0.5 * INVPI;	// White spheres everywhere
	float3	tsNormal = 2.0 * _TexNormal.Sample( LinearWrap, _In.UV ).xyz - 1.0;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	float3	View = normalize( _In.Position - _Camera2World[3].xyz );

	float3	VertexNormal = normalize( _In.Normal );
	float3	VertexTangent = normalize( _In.Tangent );
	float3	VertexBiTangent = cross( VertexNormal, VertexTangent );

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

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting
	float3	SHIndirect[9] = { _In.SH0, _In.SH1, _In.SH2, _In.SH3, _In.SH4, _In.SH5, _In.SH6, _In.SH7, _In.SH8 };
	float3	Indirect = DiffuseAlbedo * EvaluateSHIrradiance( Normal, SHIndirect );

	AccumDiffuse *= _ShowOnlyIndirect ? 1.0 : 0.0;

	if ( !_ShowIndirect )
	{	// Dummy dull uniform ambient sky
		Indirect = _Ambient * DiffuseAlbedo * lerp( 0.5, 1.0, 0.5 * (1.0 + Normal.y) );
	}

	return float4( Indirect + AccumDiffuse, 1 );
}
