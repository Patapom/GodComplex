#include "Global.hlsl"

StructuredBuffer<SB_NodeSim>	_SB_Graph_In : register( t0 );
RWStructuredBuffer<SB_NodeSim>	_SB_Graph_Out : register( u0 );

RWStructuredBuffer<float2>		_SB_Graph_Forces : register( u1 );	// A giant NxN matrix of forces
																	// Row i contain the forces that a node i applies to each other column node j
StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t1 );
StructuredBuffer<uint>			_SB_Links : register( t2 );

cbuffer CB_Simulation : register(b1) {
	float	_deltaTime;
	float	_springConstant;
	float	_dampingConstant;
};


////////////////////////////////////////////////////////////////////////////
// First CS computes the giant matrix of forces that each node provides to each other
//
[numthreads( 256, 1, 1 )]
void	CS( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	nodeIndex = _dispatchThreadID.x;
	if ( nodeIndex >= _nodesCount )
		return;

	// Retrieve node info
	SB_NodeInfo	currentInfo = _SB_Nodes[nodeIndex];
	SB_NodeSim	current = _SB_Graph_In[nodeIndex];

	float2	position = current.m_position;

	////////////////////////////////////////////////////////////////////////////
	// Compute gravitational influence on neighbors
	//
	for ( uint neighborIndex=0; neighborIndex < _nodesCount; neighborIndex++ ) {
		if ( neighborIndex == nodeIndex )
			continue;	// Avoid influencing ourselves

		SB_NodeInfo	neighborInfo = _SB_Nodes[neighborIndex];
		SB_NodeSim	neighbor = _SB_Graph_In[neighborIndex];

		float2	delta = position - neighbor.m_position;
		float	distance = length( delta );
		float	recDistance = abs(distance) > 1e-6 ? 1.0 / distance : 0.0;
				delta *= recDistance;

		// Gravitational force
		float2	force = currentInfo.m_mass * neighborInfo.m_mass * pow2( recDistance ) * delta;

		_SB_Graph_Forces[_nodesCount * nodeIndex + neighborIndex] = force;
	}

	////////////////////////////////////////////////////////////////////////////
	// Compute spring force that we provide to neigbors
	//
	for ( uint linkIndex=0; linkIndex < currentInfo.m_linksCount; linkIndex++ ) {
		uint		neighborIndex = _SB_Links[currentInfo.m_linkOffset + linkIndex];
		SB_NodeSim	neighbor = _SB_Graph_In[neighborIndex];

		float2	delta = position - neighbor.m_position;
//		float	distance = length( delta );
//				delta *= abs(distance) > 1e-6 ? 1.0 / distance : 0.0;

		// Damped harmonic oscillator
		float2	force = _springConstant * delta
					  + _dampingConstant * neighbor.m_velocity;

		_SB_Graph_Forces[_nodesCount * nodeIndex + neighborIndex] += force;
	}
}

////////////////////////////////////////////////////////////////////////////
// Second CS performs the simulation
//
[numthreads( 256, 1, 1 )]
void	CS2( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	nodeIndex = _dispatchThreadID.x;
	if ( nodeIndex >= _nodesCount )
		return;

	// Retrieve node info
	SB_NodeInfo	currentInfo = _SB_Nodes[nodeIndex];
	SB_NodeSim	current = _SB_Graph_In[nodeIndex];

	// Retrieve forces
	float2	sumForces = 0.0;
	for ( uint neighborIndex=0; neighborIndex < _nodesCount; neighborIndex++ ) {
		if ( neighborIndex == nodeIndex )
			continue;	// Avoid influencing ourselves

		sumForces += _SB_Graph_Forces[_nodesCount * neighborIndex + nodeIndex];
	}

	// Apply simulation
	float2	acceleration = sumForces / currentInfo.m_mass;
	current.m_position += current.m_velocity * _deltaTime;
	current.m_velocity += acceleration * _deltaTime;

	_SB_Graph_Out[nodeIndex] = current;
}
