//////////////////////////////////////////////////////////////////////////
// This shader renders probes as balls with color informations pulled from SH coefficients
//
#include "Inc/Global.hlsl"
#include "Inc/GI.hlsl"
#include "Inc/SH.hlsl"

//#define	DEBUG_SAMPLES 1	// Only works if 1 probe in the scene!

#if DEBUG_SAMPLES
struct ProbeUpdateSampleInfo
{
	float3		Position;					// World position of the samples
	float3		Normal;						// World normal of the sample
	float		Radius;						// Radius of the sample's disc approximation
	float3		Albedo;						// Albedo of the sample's surface
	float		SH[9];						// SH contribution of the sample
};
StructuredBuffer<ProbeUpdateSampleInfo>		_SBProbeSamples : register( t11 );
#endif

struct	NetworkProbeStruct
{
	uint2		ProbeIDs;
	float2		SolidAngles;
};
StructuredBuffer<NetworkProbeStruct>	_SBProbesNetwork : register( t16 );


struct	VS_IN
{
	float3	Position	: POSITION;
 	float3	Normal		: NORMAL;

	uint	InstanceID	: SV_INSTANCEID;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	uint	ProbeID		: PROBEID;
};

PS_IN	VS( VS_IN _In )
{
	ProbeStruct	Probe = _SBProbes[_In.InstanceID];

	float4	WorldPosition = float4( Probe.Position + (0.5+0*Probe.Radius) * _In.Position, 1.0 );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = _In.Position;
	Out.ProbeID = _In.InstanceID;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
#if DEBUG_SAMPLES
	for ( uint SampleIndex=0; SampleIndex < 128; SampleIndex++ ) {
		ProbeUpdateSampleInfo	Sample = _SBProbeSamples[SampleIndex];
		float3	ToSample = normalize( Sample.Position - _In.Position );
		if ( dot( ToSample, _In.Normal ) > 0.9 ) {
			float3	Color = (0.5 + (SampleIndex & 7)) / 8;
			return float4( Color, 1 );
		}
	}
#endif


//return _In.ProbeID == 16;

//	ProbeStruct	Probe = _SBProbes[_In.ProbeID];
	SHCoeffs3	Probe = _SBProbeSH[_In.ProbeID];

	float3	Color = 1.0 * EvaluateSH( _In.Normal, Probe.SH );
	return float4( Color, 0 );
}


//////////////////////////////////////////////////////////////////////////
// This second shader draws the links between probes

struct	VS_IN2
{
	float3	Position	: POSITION;	// I don't give a sh.. about position here but since we can't create empty vertex layouts...
	uint	InstanceID	: SV_INSTANCEID;
};

struct	GS_IN
{
	float3	ProbePosition0		: PROBE_POSITION0;
	float	ProbeSolidAngle0	: PROBE_SOLIDANGLE0;
	float3	ProbePosition1		: PROBE_POSITION1;
	float	ProbeSolidAngle1	: PROBE_SOLIDANGLE1;
};

struct	PS_IN2
{
	float4	__Position	: SV_POSITION;
	float	SolidAngle	: SOLID_ANGLE;
};

GS_IN	VS_Network( VS_IN2 _In )
{
	NetworkProbeStruct	Connection = _SBProbesNetwork[_In.InstanceID];

	ProbeStruct	Probe0 = _SBProbes[Connection.ProbeIDs.x];
	ProbeStruct	Probe1 = _SBProbes[Connection.ProbeIDs.y];

	GS_IN	Out;
	Out.ProbePosition0 = Probe0.Position;
	Out.ProbeSolidAngle0 = Connection.SolidAngles.x;
	Out.ProbePosition1 = Probe1.Position;
	Out.ProbeSolidAngle1 = Connection.SolidAngles.y;

	return Out;
}

[maxvertexcount( 4 )]
void	GS_Network( point GS_IN _In[1], inout TriangleStream<PS_IN2> _OutStream )
{
	float3	Axis = normalize( _In[0].ProbePosition1 - _In[0].ProbePosition0 );
	float3	CameraAt = _Camera2World[2].xyz;
	float3	Up = cross( Axis, CameraAt );
	float	ConnectionWorldSize = 0.1;

	PS_IN2	Out;
	float4	WorldPosition = float4( _In[0].ProbePosition0 + ConnectionWorldSize * Up, 1.0 );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.SolidAngle = _In[0].ProbeSolidAngle0;
	_OutStream.Append( Out );

	WorldPosition = float4( _In[0].ProbePosition0 - ConnectionWorldSize * Up, 1.0 );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.SolidAngle = _In[0].ProbeSolidAngle0;
	_OutStream.Append( Out );

	WorldPosition = float4( _In[0].ProbePosition1 + ConnectionWorldSize * Up, 1.0 );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.SolidAngle = _In[0].ProbeSolidAngle1;
	_OutStream.Append( Out );

	WorldPosition = float4( _In[0].ProbePosition1 - ConnectionWorldSize * Up, 1.0 );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.SolidAngle = _In[0].ProbeSolidAngle1;
	_OutStream.Append( Out );
}

float4	PS_Network( PS_IN2 _In ) : SV_TARGET0
{
	return _In.SolidAngle;
}
