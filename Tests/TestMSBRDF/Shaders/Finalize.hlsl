/////////////////////////////////////////////////////////////////////////////////////////////
// Finalize rendering
/////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

Texture2D< float4 >		_tex_Accumulator : register( t6 );

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float4	V = _tex_Accumulator[_In.__position.xy];

//return 0.5*V.w / 271.0;
//return 0.001 * V.w;
//return 0.5 * V.xyz;

			V *= V.w > 0.0 ? 1.0 / V.w : 1.0;

//V *= 0.5;

	return V.xyz;
}
