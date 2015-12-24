#include "Global.hlsl"
#include "CommonDistanceField.hlsl"

Texture3D< uint >		_TexAccumulator0 : register(t0);
Texture3D< uint >		_TexAccumulator1 : register(t1);
RWTexture3D< float4 >	_TexTarget : register(u0);

//////////////////////////////////////////////////////////////////////////////////////////
// Normalizes the accumulated data and the previous frame's reprojected data
//
[numthreads( 4, 4, 4 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	// Read packed values
	uint	value0 = _TexAccumulator0[_DispatchThreadID];
	uint	value1 = _TexAccumulator1[_DispatchThreadID];

	float3	voxelInnerCoordinate = float3( (value0 >> 16) & 0xFFFFU, value0 & 0xFFFFU, (value1 >> 16) & 0xFFFFU );
	value1 &= 0xFFFFU;

	// Read existing reprojection
//	float4	previousFrameResult = _TexSource[_DispatchThreadID];
//	voxelInnerCoordinate.xyz += previousFrameResult.xyz;
//	value1 += previousFrameResult.w;

	_TexTarget[_DispatchThreadID] = value1 > 10 ? float4( voxelInnerCoordinate / value1, 1.0 ) : 0.0;
}
