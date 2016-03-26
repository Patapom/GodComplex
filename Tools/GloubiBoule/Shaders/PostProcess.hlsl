#include "Includes/global.hlsl"

Texture2DArray<float4>	_TexSource : register(t0);

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;

	float3	Color = _TexSource[uint3( _In.__Position.xy, 0)].xyz;
//	float4	Depth = _TexSource[uint3( _In.__Position.xy, 1)];

//Depth = _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 1.0 );

//uint	mipLevel = 4;
//Color = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 0)].xyz;
//Depth = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 1)];

//return 0.1 * Depth.x;

	return Color;
}
