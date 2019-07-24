#include "Global.hlsl"

Texture2D< float4 >	_texHeatMap : register(t0);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / 512.0;
//	return float3( UV, 0 );
	return _texHeatMap[UV];
}
