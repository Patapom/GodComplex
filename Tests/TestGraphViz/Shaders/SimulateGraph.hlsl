#include "Global.hlsl"

struct SB_NodeSim {
	float		m_mass;			// Node mass
	float2		m_position;
	float2		m_velocity;
};
StructuredBuffer<SB_NodeSim>	_SB_Graph_In : register( t0 );
RWStructuredBuffer<SB_NodeSim>	_SB_Graph_Out : register( u0 );
RWStructuredBuffer<float3>		_SB_Graph_Forces : register( u1 );


struct SB_NodeInfo {
	uint		m_linkOffset;	// Start link index in the links array
	uint		m_linksCount;	// Amount of links in the array
};
StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t1 );
StructuredBuffer<uint>			_SB_Links : register( t2 );


void	CS( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	nodeIndex = _dispatchThreadID.x;

	// Retrieve node info
	SB_NodeInfo	info = _SB_Nodes[nodeIndex];
	SB_NodeSim	current = _SB_Graph_In[nodeIndex];

	float3	position = sourceSim.m_position;
	float3	force = 0.0;

	////////////////////////////////////////////////////////////////////////////
	// Compute spring force
	float	k = SPRING_CONSTANT;
	for ( uint i=0; i < info.m_linksCount; i++ ) {
		uint		neighborIndex = _SB_Links[info.m_linkOffset + i];
		SB_NodeSim	neigbor = _SB_Graph_In[neighborIndex];

		float3	delta = neighbor.m_position - position;
		float	distance = length( delta );
				delta *= abs(distance) > 1e-6 ? 1.0 / distance : 0.0;

		force += delta * k * neighbor.m_mass;
		_SB_Graph_Forces[neighborIndex] += -delta * k * current.m_mass;
	}

	////////////////////////////////////////////////////////////////////////////


	obstacle.y = 0.0;	// Always clear sources

	uint2	pos = uint2(mousePosition)+1;

//	if ( all(pos == P) ) {
	if ( abs(pos.x-P.x) < 2 && abs(pos.y-P.y) < 2 ) {
		// Use right mouse button to set or clear obstacles
		if ( mouseButtons & 4 )
			obstacle.x = 1;	// Right = set
		if ( (mouseButtons & 12) == 12 )
			obstacle.x = 0;	// Right + Shift = clear
	}

	if ( all(pos == P) ) {
		// Use middle mouse button to set or clear permanent sources
		if ( mouseButtons & 2 ) {
			obstacle.z = (1+sourceIndex) / 255.0;
		} else if ( (mouseButtons & 10) == 10 ) {
			obstacle.z = 0;
		}

		// Use left mouse button to set source
		if ( mouseButtons & 1 )
			obstacle.y = 1.0;
	}

	return obstacle;
}
