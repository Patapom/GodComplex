//
//
#include "Global.hlsl"

cbuffer CB_PostProcess : register(b10) {
	float	_lightSize;
};

Texture2DArray<float4>	_Tex_GBuffer : register(t0);
Texture2D<float4>		_Tex_Scene : register(t1);

float4	PS( VS_IN _In ) : SV_TARGET0 {
	return _Tex_Scene[_In.__Position.xy];
}