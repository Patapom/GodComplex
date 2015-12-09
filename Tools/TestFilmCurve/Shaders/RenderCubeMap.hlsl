#include "Global.hlsl"

struct VS_IN {
	float4	__Position : SV_POSITION;
};

TextureCube<float4>	_texCubeMap : register(t0);

VS_IN	VS( VS_IN _In ) { return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0 {

	float2	UV = _In.__Position.xy / _Resolution.xy;

	float4	wsView4 = mul( float4( 2.0 * UV.x - 1.0, 1.0 - 2.0 * UV.y, 0, 1 ), _Proj2World );
	float3	wsView = normalize( wsView4.xyz / wsView4.w - _Camera2World[3].xyz );

	wsView = wsView.zxy;	// idTech Z-up orientation

	return _texCubeMap.Sample( LinearWrap, wsView ).xyz;
}