//////////////////////////////////////////////////////////////////////////
// This shader renders lights as luminous balls
//
#include "Inc/Global.hlsl"

//[
cbuffer	cbScene	: register( b10 )
{
	uint		_LightsCount;
	uint		_ProbesCount;
};
//]

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

	uint	InstanceID	: SV_INSTANCEID;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Color		: COLOR;
};

PS_IN	VS( VS_IN _In )
{
	LightStruct	Light = _SBLights[_In.InstanceID];

	float4	WorldPosition = float4( Light.Position + Light.Radius * _In.Position, 1.0 );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Color = Light.Color;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return float4( _In.Color, 0 );
}
