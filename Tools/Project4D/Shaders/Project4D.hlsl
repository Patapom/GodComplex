// Projects a bunch of 4D points into 2D
//
#include "Global.hlsl"

static const uint	TOTAL_POINTS_COUNT = 200 * 256;

cbuffer CB_Camera4D : register(b2) {
	float4	_Camera4DPosition;
	float4	_Camera4DX;				// Camera vectors are already factored by 1/Far clip
	float4	_Camera4DY;
	float4	_Camera4DZ;
	float4	_Camera4DW;
	float	_Camera4DTanHalfFOV;	// tan(FOV/2)
};

//struct SB_Point {
//	float4	__Position;	// Projected position (Z=Z coordinate in 3D camera space, W=W coordinate in 4D camera space)
//	float	_Size;		// Point screen size
//};

StructuredBuffer<float4>	_BufferPointsIn : register(t0);
RWStructuredBuffer<float4>	_BufferPointsOut : register(u0);

[numthreads( 256, 1, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint	pointIndex = _DispatchThreadID.x;

	// Transform into 4D camera space
#if 1
	float4	wsPosition4 = _BufferPointsIn[pointIndex];
#else
	float	phi = 2.0 * PI * pointIndex / TOTAL_POINTS_COUNT;
	float	theta = 2.0 * acos( sqrt( sin( 43789.34 * pointIndex / TOTAL_POINTS_COUNT ) ) );
	float	alpha = sin( 13787.16 * pointIndex / TOTAL_POINTS_COUNT );
	float4	wsPosition4 = float4( sin(theta)*cos(phi), cos(theta), sin(theta)*sin(phi), alpha );
#endif
	
	wsPosition4 -= _Camera4DPosition;

	float4	csPosition4 = float4(
				dot( wsPosition4, _Camera4DX ),
				dot( wsPosition4, _Camera4DY ),
				dot( wsPosition4, _Camera4DZ ),
				dot( wsPosition4, _Camera4DW )
			);

	// Project 4D->3D
	float	proj = 1.0 / (_Camera4DTanHalfFOV * csPosition4.w);
	csPosition4.xyz *= proj;

	// Project into 2D space
#if 1
	float3	wsPosition3 = csPosition4.xyz;
#else
	float	phi2 = 2.0 * PI * pointIndex / TOTAL_POINTS_COUNT;
	float	theta2 = 2.0 * acos( sqrt( sin( 43789.34 * pointIndex / TOTAL_POINTS_COUNT ) ) );
	float3	wsPosition3 = float3( sin(theta2)*cos(phi2), cos(theta2), sin(theta2)*sin(phi2) );
#endif

	float4	projPosition3 = mul( float4( wsPosition3, 1.0 ), _World2Proj );
			projPosition3 /= projPosition3.w;

//projPosition3.x = -1.0 + 2.0 * pointIndex / 100000;
//projPosition3.y = sin( 2.0 * PI * pointIndex / 100000 );

	_BufferPointsOut[pointIndex] = float4( projPosition3.xyz, csPosition4.w );
}
