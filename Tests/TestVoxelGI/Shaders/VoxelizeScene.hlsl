//////////////////////////////////////////////////////////////////////////////////////////////
// Voxelizes a distance field scene
//////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "DistanceField.hlsl"
#include "Voxel.hlsl"

RWTexture3D< float4 >	_Tex_VoxelScene_Albedo : register(u0);
RWTexture3D< float4 >	_Tex_VoxelScene_Normal : register(u1);

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint3	voxelIndex = _DispatchThreadID;
	float3	wsVoxelCenter = VOXEL_MIN + (voxelIndex + 0.5) * VOXEL_SIZE;

	// Estimate distance field at center position
	float2	distance = Map( wsVoxelCenter );
	if ( distance.x > 0.5 * VOXEL_DIAG_SIZE ) {
		// Empty
		_Tex_VoxelScene_Albedo[voxelIndex] = 0.0;
		_Tex_VoxelScene_Normal[voxelIndex] = float4( 0.5.xxx, 0 );
		return;
	}

	// Estimate albedo and normal
	float3	albedo = Albedo( wsVoxelCenter, distance.y );
	float3	wsNormal = Normal( wsVoxelCenter );
	_Tex_VoxelScene_Albedo[voxelIndex] = float4( sqrt( albedo ), 1.0 );		// Store as sqrt() to simulate sRGB (sRGB is forbidden when buffer is set as UAV)
	_Tex_VoxelScene_Normal[voxelIndex] = float4( 0.5 * (1.0 + wsNormal ), 0 );
}