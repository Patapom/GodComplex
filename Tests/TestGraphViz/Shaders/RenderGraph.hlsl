#include "Global.hlsl"

StructuredBuffer<SB_NodeSim>	_SB_Graph_In : register( t0 );
StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t1 );

Texture2D<float3>	_tex_FalseColors : register( t4 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	position =  _cameraSize * (_In.__Position.xy / _resolution - 0.5) + _cameraCenter;

	// Check EVERY node!! Yeah! This is madness!
	float2	closestSqDistance = float2( 1e38, 0 );

	[loop]
	for ( uint i=0; i < _nodesCount; i++ ) {
		SB_NodeSim	node = _SB_Graph_In[i];
		SB_NodeInfo	info = _SB_Nodes[i];
		float2		delta = node.m_position - position;
		float		sqDistance = dot( delta, delta );
//		closestSqDistance = min( closestSqDistance, sqDistance );
		if ( sqDistance < closestSqDistance.x )
			closestSqDistance = float2( sqDistance, info.m_mass );
	}

	return saturate( 1 - 10.0 * sqrt( closestSqDistance.x ) ) * _tex_FalseColors.SampleLevel( LinearClamp, float2( lerp( 0.15, 1.0, closestSqDistance.y / _maxMass ), 0.5 ), 0.0 );
}
