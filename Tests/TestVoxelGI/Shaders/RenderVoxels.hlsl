/////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Renders many instanced voxels
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "Voxel.hlsl"

cbuffer CB_RenderVoxels : register(b10) {
	float4	_voxelSize;
	uint3	_voxelPOTs;
	uint	_mipLevel;
	uint4	_voxelMasks;
};

Texture3D< float4 >	_Tex_VoxelScene_Albedo : register(t0);
Texture3D< float4 >	_Tex_VoxelScene_Normal : register(t1);
Texture3D< float4 >	_Tex_VoxelScene_Lighting : register(t2);

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
	voxelIndex.x = _instanceIndex & _voxelMasks.x;	_instanceIndex >>= _voxelPOTs.x;
	voxelIndex.y = _instanceIndex & _voxelMasks.y;	_instanceIndex >>= _voxelPOTs.y;
	voxelIndex.z = _instanceIndex & _voxelMasks.z;//	_instanceIndex >>= _voxelPOTs.z;

	float4	albedo = _Tex_VoxelScene_Albedo.mips[_mipLevel][voxelIndex];
			albedo.xyz *= albedo.xyz;	// To simulate sRGB target

	Out.__Position = float4( 0, 0, 0, -1 );
	if ( albedo.w > 0.0 ) {
		float3	wsVoxelCenter = VOXEL_MIN + (voxelIndex + 0.5) * _voxelSize.xyz;
		float3	wsPosition = wsVoxelCenter + 0.5 * _voxelSize.xyz * _In.Position;
		Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );

//albedo = 2.0 * _Tex_VoxelScene_Normal.mips[_mipLevel][voxelIndex] - 1.0;
albedo = _Tex_VoxelScene_Lighting.mips[_mipLevel][voxelIndex];

//albedo = 0.001 * _instanceIndex;
	}
	Out.Color = albedo;
	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	return _In.Color;
}