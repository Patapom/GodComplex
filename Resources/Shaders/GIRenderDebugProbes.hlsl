//////////////////////////////////////////////////////////////////////////
// This shader renders probes as balls with color informations pulled from SH coefficients
//
#include "Inc/Global.hlsl"
#include "Inc/GI.hlsl"
#include "Inc/SH.hlsl"

struct	VS_IN
{
	float3	Position	: POSITION;
 	float3	Normal		: NORMAL;

	uint	InstanceID	: SV_INSTANCEID;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Normal		: NORMAL;
	uint	ProbeID		: PROBEID;
};

PS_IN	VS( VS_IN _In )
{
	ProbeStruct	Probe = _SBProbes[_In.InstanceID];

	float4	WorldPosition = float4( Probe.Position + (0.5+0*Probe.Radius) * _In.Position, 1.0 );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Normal = _In.Position;
	Out.ProbeID = _In.InstanceID;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	ProbeStruct	Probe = _SBProbes[_In.ProbeID];

//return _In.ProbeID == 16;

	float3	Color = 1.0 * EvaluateSH( _In.Normal, Probe.SH );
	return float4( Color, 0 );
}
