#include "Global.hlsl"

StructuredBuffer<SB_NodeSim>	_SB_Graph_In : register( t0 );

StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t1 );
StructuredBuffer<uint>			_SB_Links : register( t2 );
StructuredBuffer<uint>			_SB_LinkSources : register( t3 );

Texture2D<float3>	_tex_FalseColors : register( t4 );

struct VS_IN {
	float4	__Position : SV_POSITION;
	uint	instanceID : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	UV : TEXCOORDS0;
};

float4	TransformPosition( float2 _position ) {
	float4	result;
	result.xy = 2.0 * (_position - _cameraCenter) / _cameraSize;
	result.y = -result.y;
	result.z = 0;
	result.w = 1;
	return result;
}

///////////////////////////////////////////////////////////////////////////////////////
// NODE DRAWING
//
PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	uint		nodeIndex = _In.instanceID;
	SB_NodeSim	node = _SB_Graph_In[nodeIndex];
	SB_NodeInfo	info = _SB_Nodes[nodeIndex];

	const float	SIZE = 0.1;

	float2	position = node.m_position + SIZE * _In.__Position.xy;

	Out.__Position = TransformPosition( position );

	Out.UV.xy = _In.__Position.xy;
	Out.UV.z = info.m_mass / _maxMass;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	float	sqDistance = 1.0 - dot( _In.UV.xy, _In.UV.xy );
	clip( sqDistance );

	return sqrt( sqDistance ) * _tex_FalseColors.SampleLevel( LinearClamp, float2( lerp( 0.15, 1.0, _In.UV.z ), 0.5 ), 0.0 );
}


///////////////////////////////////////////////////////////////////////////////////////
// LINK DRAWING
//
PS_IN	VS2( VS_IN _In ) {
	PS_IN	Out;

	uint		linkIndex = _In.instanceID;
	uint		souceIndex = _SB_LinkSources[linkIndex];
	uint		targetIndex = _SB_Links[linkIndex];
	SB_NodeSim	sourceNode = _SB_Graph_In[souceIndex];
	SB_NodeInfo	sourceInfo = _SB_Nodes[souceIndex];
	SB_NodeSim	targetNode = _SB_Graph_In[targetIndex];
	SB_NodeInfo	targetInfo = _SB_Nodes[targetIndex];

	const float	SIZE = 0.02;

	float2	P0 = sourceNode.m_position;
	float2	P1 = targetNode.m_position;

	float2	dir = P1 - P0;
	float2	ortho = SIZE * normalize( float2( -dir.y, dir.x ) );

	float	U = 0.5 * (1.0 + _In.__Position.x);
	float2	finalPos = P0 + U * dir +  _In.__Position.y * ortho;

	Out.__Position = TransformPosition( finalPos );

	Out.UV.xy = _In.__Position.xy;
	Out.UV.z = lerp( sourceInfo.m_mass, targetInfo.m_mass, U ) / _maxMass;

	return Out;
}

float3	PS2( PS_IN _In ) : SV_TARGET0 {
	return 0.5 * sqrt( 1.0 - pow2( _In.UV.y ) ) * _tex_FalseColors.SampleLevel( LinearClamp, float2( lerp( 0.15, 1.0, _In.UV.z ), 0.5 ), 0.0 );
}

