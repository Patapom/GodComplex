// Simulates a constant velocity applied to a bunch of 4 points
//
#include "Global.hlsl"

cbuffer CB_Simulator : register(b3) {
	uint	_Flags;
	float	_TimeStep;
};

StructuredBuffer<float4>	_BufferPointsIn : register(t0);
StructuredBuffer<float4>	_BufferVelocities : register(t1);
RWStructuredBuffer<float4>	_BufferPointsOut : register(u0);

[numthreads( 256, 1, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint	pointIndex = _DispatchThreadID.x;

	float4	wsPosition4 = _BufferPointsIn[pointIndex];
	float4	wsVelocity = _BufferVelocities[pointIndex];

	wsPosition4 += _TimeStep * wsVelocity;
//	wsPosition4 += _TimeStep * float4( 1, 0, 0, 0 );

	wsPosition4 = normalize( wsPosition4 );	// This constrains the position on the unit sphere-4

	_BufferPointsOut[pointIndex] = wsPosition4;
}
