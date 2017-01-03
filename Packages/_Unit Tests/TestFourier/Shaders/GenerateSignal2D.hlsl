#include "Global.hlsl"

float2	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = float2(_In.__Position.xy) / _signalSize;
	float2	time = _time * float2( 1.0, 0.97856756 );

//	time *= _signalScaleUV;
	UV *= _signalScaleUV;

	return float2( GenerateSignal2D( UV, time, _signalFlags & 7, (_signalFlags >> 4) & 7 ), 0.0 );
}