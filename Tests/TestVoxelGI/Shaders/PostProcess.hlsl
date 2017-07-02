//
//
#include "Global.hlsl"
#include "Voxel.hlsl"

cbuffer CB_PostProcess : register(b10) {
	uint	_flags;
	uint	_filterLevel;
};

Texture2DArray<float4>	_Tex_GBuffer : register(t0);
Texture2D<float4>		_Tex_Scene : register(t1);

Texture3D< float4 >		_Tex_VoxelScene_Lighting : register(t2);
Texture3D< float4 >		_Tex_VoxelScene_IndirectLighting0 : register(t3);
Texture3D< float4 >		_Tex_VoxelScene_IndirectLighting1 : register(t4);
Texture3D< float4 >		_Tex_VoxelScene_IndirectLighting2 : register(t5);

float3	QueryIndirectLighting( float3 _wsPosition ) {
	float3	positionUVW = (_wsPosition - VOXEL_MIN) * INV_VOXEL_VOLUME_SIZE;
//return positionUVW;
//	return _Tex_VoxelScene_Lighting.SampleLevel( LinearClamp, positionUVW, 0 ).xyz;
	return _Tex_VoxelScene_IndirectLighting2.SampleLevel( LinearClamp, positionUVW, _filterLevel ).xyz;
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	// Render as voxels?
	if ( _flags & 1U )
		return _Tex_Scene[_In.__Position.xy];

	float3	lighting_Direct = _Tex_GBuffer[uint3(_In.__Position.xy, 3)].xyz;

	float3	wsPosition = _Tex_GBuffer[uint3(_In.__Position.xy, 2)].xyz;		// From screen space G-Buffer, normally computed with depth buffer only
	float3	lighting_Indirect = QueryIndirectLighting( wsPosition );
//return float4( lighting_Indirect, 0 );
//return float4( lighting_Direct, 0 );
return float4( lighting_Direct + (_flags & 2U ? lighting_Indirect : 0), 0 );

//uint2	pos = 2.0f * _In.__Position.xy;
//return _Tex_Voxels.mips[_filterLevel][uint3( pos & 0x7F, (pos.x / 128) + 10 * (pos.y / 128) )];
//return _Tex_Voxels.mips[_filterLevel][uint3( _In.__Position.xy, 128 * abs(sin(_time)) )];
}