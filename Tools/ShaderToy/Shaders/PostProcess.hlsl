#include "Includes/global.hlsl"

Texture2DArray<float4>	_TexSource : register(t0);
Texture2D<float>		_TexLinearDepth : register(t1);
Texture2D<float4>		_TexDownsampledDepth : register(t2);

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;

	float3	Color = _TexSource[uint3( _In.__Position.xy, 0)].xyz;
	float4	Depth = _TexSource[uint3( _In.__Position.xy, 1)];

Depth = _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 1.0 );

//uint	mipLevel = 4;
//Color = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 0)].xyz;
//Depth = _TexSource.mips[mipLevel][uint3( uint2( floor( _In.__Position.xy ) ) >> mipLevel, 1)];

//return 0.1 * Depth.x;

	Color = pow( max( 0.0, Color ), 1.0 / 2.2 );	// Gamma-correct



//return 0.1 * _TexSource.SampleLevel( LinearClamp, float3( UV, 0.0 ), 0.0 ).w;	// Show distance
//return 0.1 * _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).w;	// Show projZ

//float	projZ = _TexSource.SampleLevel( LinearClamp, float3( UV, 1.0 ), 0.0 ).w;
//float	temp = _Proj2Camera[2].w * projZ + _Proj2Camera[3].w;
//return 0.1 * projZ / temp;	// Show linear Z

//return 0.1 * _TexLinearDepth.SampleLevel( LinearClamp, UV, 0.0 );
//return 0.1 * _TexDownsampledDepth.SampleLevel( LinearClamp, UV, 1.0 ).xyz;
//return 0.1 * _TexDownsampledDepth.SampleLevel( LinearClamp, UV, 1.0 ).xyz;

uint	mipLevel = 2;
uint2	pixelPos = _In.__Position.xy;
		pixelPos >>= mipLevel;
return 0.1 * (mipLevel > 0 ? _TexDownsampledDepth.mips[mipLevel-1][pixelPos].xyz : _TexLinearDepth.SampleLevel( LinearClamp, UV, 0.0 ));

	return Color;
}
