//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};

Texture2DArray< float4 >	_Tex_GBuffer : register(t0);

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / 64.0;

uint	count = uint( _In.__Position.y + 10.0 * _Time );
uint	mask = 1 << (uint( _In.__Position.x ) & 15);
return count & mask;
//	return float3( abs( sin( 4.0 * _Time ) ) * UV, 0 );
}
