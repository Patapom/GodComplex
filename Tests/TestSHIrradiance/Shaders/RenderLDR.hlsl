#include "Global.hlsl"

Texture2D<float4>	_TexHDRBackBuffer : register( t1 );

float3	PS( VS_IN _In ) : SV_TARGET0 {
	return _luminanceFactor * _TexHDRBackBuffer[_In.__Position.xy].xyz;
}