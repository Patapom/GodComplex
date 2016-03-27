#include "Includes/global.hlsl"


cbuffer CB_PostProcess : register(b2) {
};


struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}

Texture2DArray<float4>	_TexScattering : register(t0);

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy * _ScreenSize.zw;

float3	BackgroundColor = float3( UV, 0 );
float3	Scattering = _TexScattering.Sample( LinearWrap, float3( UV, 0.0 ) ).xyz;
float3	Extinction = _TexScattering.Sample( LinearWrap, float3( UV, 1.0 ) ).xyz;

	return BackgroundColor * Extinction + Scattering;
}
