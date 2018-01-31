#include "Global.hlsl"

Texture2D<float4>		_tex_Albedo : register(t0);
Texture2D<float4>		_tex_Normal : register(t1);
Texture2D<float2>		_tex_MotionVectors : register(t2);
Texture2D<float>		_tex_Depth : register(t3);

Texture2DArray<float4>	_tex_Radiance : register(t8);
Texture2D<float4>		_tex_BentCone : register(t9);
Texture2D<float4>		_tex_FinalRender : register(t10);
Texture2D<float4>		_tex_SourceRadiance_PUSH : register(t11);
Texture2D<float4>		_tex_SourceRadiance_PULL : register(t12);

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

float3	PS( float4 __Position : SV_POSITION ) : SV_TARGET0 {
	float2	UV = __Position.xy / _resolution;
//	return float3( UV, 0 );
//return _exposure * _tex_FinalRender[__Position.xy].xyz;
//return length(_tex_BentCone[__Position.xy].xyz);
//return _tex_BentCone[__Position.xy].w;
//return _tex_BentCone[__Position.xy].xyz;
//return _tex_SourceRadiance[__Position.xy].w / Z_FAR;
//return _tex_SourceRadiance.mips[_debugMipIndex][__Position.xy].xyz;
//return saturate( _tex_SourceRadiance.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex ).w );
if ( _flags & 0100U )
	return _tex_SourceRadiance_PULL.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex ).xyz;
else
	return _tex_SourceRadiance_PUSH.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex ).xyz;

return _tex_Radiance[uint3(__Position.xy, _sourceRadianceIndex)].xyz;
return _tex_Normal[__Position.xy].xyz;
//return 0.5 * (1.0 + _tex_Normal[__Position.xy].xyz);
return _tex_Depth[__Position.xy];
return float3( _tex_MotionVectors[__Position.xy], 0 );
}
