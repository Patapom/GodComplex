#include "Global.hlsl"

float2	PS( VS_IN _In ) : SV_TARGET0 {
	float	U = float(_In.__Position.x) / _signalSize;
//	return sin( 2.0 * PI * U + _time );
	return GenerateSignal( U, _signalFlags & 7 );
}