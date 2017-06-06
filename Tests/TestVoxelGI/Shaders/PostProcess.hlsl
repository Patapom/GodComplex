//
//
#include "Global.hlsl"

cbuffer CB_PostProcess : register(b10) {
	uint	_filterLevel;
};

Texture2DArray<float4>	_Tex_GBuffer : register(t0);
Texture2D<float4>		_Tex_Scene : register(t1);
Texture3D<float4>		_Tex_Voxels: register(t2);

float4	PS( VS_IN _In ) : SV_TARGET0 {
//	return _Tex_GBuffer[uint3(_In.__Position.xy, 2)];
//return _Tex_Voxels.mips[_filterLevel][uint3( _In.__Position.xy, 128 * abs(sin(_time)) )];
// uint2	pos = 2.0f * _In.__Position.xy;
// return _Tex_Voxels.mips[_filterLevel][uint3( pos & 0x7F, (pos.x / 128) + 10 * (pos.y / 128) )];
	return _Tex_Scene[_In.__Position.xy];
}