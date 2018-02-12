#include "Global.hlsl"

Texture2D<float3>		_tex_Emissive : register(t0);

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

float3	PS( float4 __Position : SV_POSITION ) : SV_TARGET0 {
//	float2	UV = __Position.xy / _resolution;
	return _tex_Emissive[__Position.xy];
}
