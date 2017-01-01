#include "Global.hlsl"

float2	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = float(_In.__Position.xy) / _signalSize;
	return float2( sin( 2.0 * PI * UV.x + _time ), 0.0 );
//	return GenerateSignal( U, _signalFlags & 7 );
}