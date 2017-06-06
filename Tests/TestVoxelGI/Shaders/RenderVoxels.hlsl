/////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Renders many instanced voxels
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "Voxel.hlsl"

cbuffer CB_PostProcess : register(b10) {
	float	_lightSize;
};

Texture3D< float4 >	_Tex_VoxelScene_Albedo : register(t0);
Texture3D< float4 >	_Tex_VoxelScene_Normal : register(t1);

struct VS_IN_P3 {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float4	Color : COLOR;
};

PS_IN	VS( VS_IN_P3 _In, uint _instanceIndex : SV_INSTANCEID ) {
	PS_IN	Out;

	uint3	voxelIndex;
	voxelIndex.x = _instanceIndex & VOXEL_MASKS.x;	_instanceIndex >>= VOXEL_POTS.x;
	voxelIndex.y = _instanceIndex & VOXEL_MASKS.y;	_instanceIndex >>= VOXEL_POTS.y;
	voxelIndex.z = _instanceIndex & VOXEL_MASKS.z;//	_instanceIndex >>= VOXEL_POTS.z;

	float4	albedo = _Tex_VoxelScene_Albedo[voxelIndex];
			albedo.xyz *= albedo.xyz;	// To simulate sRGB target

	Out.__Position = float4( 0, 0, 0, -1 );
	if ( albedo.w > 0.0 ) {
		float3	wsVoxelCenter = VOXEL_MIN + (voxelIndex + 0.5) * VOXEL_SIZE;
		float3	wsPosition = wsVoxelCenter + 0.5 * VOXEL_SIZE * _In.Position;
		Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );

albedo = _Tex_VoxelScene_Normal[voxelIndex];

//albedo = 0.001 * _instanceIndex;
	}
	Out.Color = albedo;
	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	return _In.Color;
}