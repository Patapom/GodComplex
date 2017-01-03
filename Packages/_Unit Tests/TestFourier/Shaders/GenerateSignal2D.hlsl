#include "Global.hlsl"

float2	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = float2(_In.__Position.xy) / _signalSize;
	return float2( GenerateSignal2D( UV, _time, _signalFlags & 7, (_signalFlags >> 4) & 7, _signalFlags & 0x800U ), 0.0 );
}