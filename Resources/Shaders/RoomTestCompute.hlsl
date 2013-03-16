//////////////////////////////////////////////////////////////////////////
// This shader is my first test of compute shaders! WOOOOOHOOOO!
//
#include "Inc/Global.hlsl"

static const int	THREADS_COUNT_X = 16;
static const int	THREADS_COUNT_Y = 16;
static const int	THREADS_COUNT_PER_GROUP = THREADS_COUNT_X * THREADS_COUNT_X;
static const int	GROUPS_COUNT_X = 2;
static const int	GROUPS_COUNT_Y = 2;

struct	Data
{
	uint	Constant;
	float3	Color;
};


//[
cbuffer	cbRender	: register( b10 )
{
	float3	_dUV;
	float2	_TesselationFactors;
};
//]

//[
//StructuredBuffer<Data>		_Input : register( t0 );
RWStructuredBuffer<Data>	_Output : register( u0 );
//]


// For more infos about the thread groups and threads: http://msdn.microsoft.com/en-us/library/windows/desktop/ff476405(v=vs.85).aspx
//[
[numthreads( THREADS_COUNT_X, THREADS_COUNT_Y, 1 )]
//]
void	CS(	uint3 _GroupID			: SV_GroupID,			// Defines the group offset within a Dispatch call, per dimension of the dispatch call
			uint3 _ThreadID			: SV_DispatchThreadID,	// Defines the global thread offset within the Dispatch call, per dimension of the group
			uint3 _GroupThreadID	: SV_GroupThreadID,		// Defines the thread offset within the group, per dimension of the group
			uint  _GroupIndex		: SV_GroupIndex )		// Provides a flattened index for a given thread within a given group
{
	uint	FlattenedIndex = THREADS_COUNT_PER_GROUP * (GROUPS_COUNT_X * _GroupID.y + _GroupID.x) + _GroupIndex;

//	_Output[FlattenedIndex].Constant = _GroupIndex;			// Will vary between [0,255] => Flatenned number of threads in a group
	_Output[FlattenedIndex].Constant = FlattenedIndex;		// Will vary between [0,1023] => Total number of threads
// 	_Output[FlattenedIndex].Color.x = _GroupID.x;			// Will vary between [0,1] because we dispatched the computation on 2x2 thread groups
// 	_Output[FlattenedIndex].Color.y = _GroupID.y;
// 	_Output[FlattenedIndex].Color.x = _GroupThreadID.x;		// Will vary between [0,15] because we dispatched the computation using 16x16 threads per group
// 	_Output[FlattenedIndex].Color.y = _GroupThreadID.y;
	_Output[FlattenedIndex].Color.x = _ThreadID.x;			// Will vary between [0,31] since it's the thread ID offset by the group ID, so we can have up to Groups*Threads = 2x16 threads in X and Y
	_Output[FlattenedIndex].Color.y = _ThreadID.y;
	_Output[FlattenedIndex].Color.z = 0.0;
}


//////////////////////////////////////////////////////////////////////////
// Bitonic sort, as in the DX sample
// groupshared uint	_SharedData[GROUP_THREADS_COUNT];
// 
// [numthreads( GROUP_THREADS_COUNT, 1, 1 )]
// void	CS(	uint3 _GroupID			: SV_GroupID, 
// 			uint3 _ThreadID			: SV_DispatchThreadID, 
// 			uint3 _GroupThreadID	: SV_GroupThreadID, 
// 			uint  _GroupIndex		: SV_GroupIndex )
// {
// 	// Load shared data
// 	_SharedData[_GroupIndex] = Data[_ThreadID.x];
// 	GroupMemoryBarrierWithGroupSync();
//     
// 	// Sort the shared data
// 	for ( uint j=g_iLevel >> 1; j > 0; j >>= 1 )
// 	{
// 		uint	Result = ((_SharedData[_GroupIndex & ~j] <= _SharedData[_GroupIndex | j]) == (bool)(g_iLevelMask & _ThreadID.x))? _SharedData[_GroupIndex ^ j] : _SharedData[_GroupIndex];
// 		GroupMemoryBarrierWithGroupSync();
// 
// 		_SharedData[_GroupIndex] = Result;
// 		GroupMemoryBarrierWithGroupSync();
// 	}
//     
// 	// Store shared data
// 	Data[_ThreadID.x] = _SharedData[_GroupIndex];
// }
