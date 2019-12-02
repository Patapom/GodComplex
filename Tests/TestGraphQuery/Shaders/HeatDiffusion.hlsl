#include "Global.hlsl"

StructuredBuffer<float>			_SB_Graph_In : register( t0 );
RWStructuredBuffer<float>		_SB_Graph_Out : register( u0 );

StructuredBuffer<SB_NodeInfo>	_SB_Nodes : register( t1 );
StructuredBuffer<uint>			_SB_LinkTargets : register( t2 );

StructuredBuffer<uint>			_SB_SourceIndices : register( t3 );

float	ReadHeat( uint _nodeIndex, uint _sourceIndex ) {
	return _SB_Graph_In[Local2GlobalIndex( _nodeIndex, _sourceIndex )];
}

[numthreads( 64, 1, 1 )]
void	CS( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	nodeIndex = _dispatchThreadID.x;	// The node we're simulating
	if ( nodeIndex >= _nodesCount )
		return;

	uint	sourceIndex = _dispatchThreadID.y;	// The index of the source we're simulating

	// Retrieve node info
	SB_NodeInfo	currentInfo = _SB_Nodes[nodeIndex];

	///////////////////////////////////////////////////////////////////
	// Compute normalized laplacian
	float	currentTemp = ReadHeat( nodeIndex, sourceIndex );

#if 1
	// Use uniformly-split heat based on neighbors count
	float	sumNeighborTemps = 0.0;
	[loop]
	for ( uint i=0; i < currentInfo.m_linksCount; i++ ) {
		uint		neighborNodeIndex = _SB_LinkTargets[currentInfo.m_linkOffset + i];
		SB_NodeInfo	neighborInfo = _SB_Nodes[neighborNodeIndex];
		sumNeighborTemps += ReadHeat( neighborNodeIndex, sourceIndex ) / neighborInfo.m_linksCount;	// We only receive a small amount equally divided among neighbors
	}

	// Gain from neighbors, lose to neighbors
	currentTemp = _diffusionCoefficient * (sumNeighborTemps - currentTemp);

Rhaa! Ca conserve pas l'énergie là ! C'est quoi ce merdier ????

#elif 1
	// Use max heat * coeff
	float	maxNeighborTemp = 0.0;
	[loop]
	for ( uint i=0; i < currentInfo.m_linksCount; i++ ) {
		uint	neighborNodeIndex = _SB_LinkTargets[currentInfo.m_linkOffset + i];
		maxNeighborTemp = max( maxNeighborTemp, ReadHeat( neighborNodeIndex, sourceIndex ) );
	}

	currentTemp = max( currentTemp, _diffusionCoefficient * maxNeighborTemp );

#elif 1
	// Un-normalized version
	float		laplacian = 0.0;
	[loop]
	for ( uint i=0; i < currentInfo.m_linksCount; i++ ) {
		uint	neighborNodeIndex = _SB_LinkTargets[currentInfo.m_linkOffset + i];
		laplacian += ReadHeat( neighborNodeIndex, sourceIndex );
	}

	laplacian -= currentTemp;

	currentTemp += _diffusionCoefficient * laplacian;

#else
	// Normalized version
	float		laplacian = 0.0;
	[loop]
	for ( uint i=0; i < currentInfo.m_linksCount; i++ ) {
		uint	neighborNodeIndex = _SB_LinkTargets[currentInfo.m_linkOffset + i];
		laplacian += ReadHeat( neighborNodeIndex, sourceIndex );
	}

	laplacian -= currentInfo.m_linksCount * currentTemp;
	laplacian *= currentInfo.m_linksCount > 1 ? 1.0 / currentInfo.m_linksCount : 1.0;

	currentTemp += _diffusionCoefficient * laplacian;
#endif

	///////////////////////////////////////////////////////////////////
	// Apply diffusion
	_SB_Graph_Out[Local2GlobalIndex( nodeIndex, sourceIndex )] = currentTemp;
}

// Initializes the source nodes
[numthreads( 64, 1, 1 )]
void	CS2( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	sourceIndex = _dispatchThreadID.x;
	if ( sourceIndex >= _sourcesCount )
		return;

	_SB_Graph_Out[Local2GlobalIndex( _SB_SourceIndices[sourceIndex], sourceIndex )] = 1.0;
}