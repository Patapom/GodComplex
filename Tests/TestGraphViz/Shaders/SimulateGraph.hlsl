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
	float	_restDistance;
	float4	_K;
};

//#define DEBUG_MASS 10.0


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

#ifdef DEBUG_MASS
currentInfo.m_mass = DEBUG_MASS;
#endif

	////////////////////////////////////////////////////////////////////////////
	// Compute gravitational influence on neighbors
	//
	for ( uint neighborIndex=0; neighborIndex < _nodesCount; neighborIndex++ ) {
		SB_NodeInfo	neighborInfo = _SB_Nodes[neighborIndex];
		SB_NodeSim	neighbor = _SB_Graph_In[neighborIndex];

		float2	delta = current.m_position - neighbor.m_position;
		float	distance = length( delta );
		float	recDistance = abs(distance) > 1e-6 ? 1.0 / distance : 0.0;
				delta *= recDistance;

//		float2	force = currentInfo.m_mass * neighborInfo.m_mass * pow2( recDistance ) * delta;
//float2	force =  currentInfo.m_mass * neighborInfo.m_mass * (log( 1.0 + 0.1 * distance ) - log( 1.5 + 0.01 * distance )) * delta;
float2	force =  currentInfo.m_mass * neighborInfo.m_mass * (log( _K.x + _K.y * distance ) - log( _K.z + _K.w * distance )) * delta;

//force = 0;

		_SB_Graph_Forces[_nodesCount * nodeIndex + neighborIndex] = force;
	}

	////////////////////////////////////////////////////////////////////////////
	// Compute spring force that we provide to neigbors
	//
#if 1
	for ( uint linkIndex=0; linkIndex < currentInfo.m_linksCount; linkIndex++ ) {
		uint		neighborIndex = _SB_Links[currentInfo.m_linkOffset + linkIndex];
		SB_NodeSim	neighbor = _SB_Graph_In[neighborIndex];

		float2	delta = current.m_position - neighbor.m_position;
		float	distance = length( delta );
//				delta *= abs(distance) > 1e-6 ? 1.0 / distance : 0.0;


// @TODO: Use a spring rest length based on ???

//		float	springConstant = _springConstant * log( distance - 1.0 );
//		float	springConstant = _springConstant * (distance > 1.0 ? 1.0 : 1.0 + log( 1.0 * distance ));
		float	springConstant = _springConstant * (distance - _restDistance);


		// Damped harmonic oscillator
		float2	force = springConstant * delta
					  + _dampingConstant * neighbor.m_velocity;

		_SB_Graph_Forces[_nodesCount * nodeIndex + neighborIndex] += force;
	}
#endif

	// Clear self-influence
	_SB_Graph_Forces[_nodesCount * nodeIndex + nodeIndex] = 0.0;
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

#ifdef DEBUG_MASS
currentInfo.m_mass = DEBUG_MASS;
#endif

	// Retrieve forces
	float2	sumForces = 0.0;
	for ( uint neighborIndex=0; neighborIndex < _nodesCount; neighborIndex++ ) {
		sumForces += _SB_Graph_Forces[_nodesCount * neighborIndex + nodeIndex];
	}
//	sumForces -= _SB_Graph_Forces[_nodesCount * nodeIndex + nodeIndex];

	// Apply simulation
	float2	acceleration = sumForces / currentInfo.m_mass;
	current.m_velocity += acceleration * _deltaTime;
	current.m_position += current.m_velocity * _deltaTime;

	_SB_Graph_Out[nodeIndex] = current;
}
