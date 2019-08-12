#include "Global.hlsl"

StructuredBuffer<SB_NodeSim>	_SB_Graph_In : register( t0 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	position =  10.0 * (_In.__Position.xy / _resolution - 0.5);

	// Check EVERY node!! Yeah! This is madness!
	float	closestSqDistance = 1e38;

	[loop]
	for ( uint i=0; i < _nodesCount; i++ ) {
		SB_NodeSim	node = _SB_Graph_In[i];
		float2		delta = node.m_position - position;
		float		sqDistance = dot( delta, delta );
		closestSqDistance = min( closestSqDistance, sqDistance );
	}

	return 1 - 10.0 * sqrt( closestSqDistance );
}
