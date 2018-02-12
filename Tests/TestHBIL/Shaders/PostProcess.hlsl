#include "Global.hlsl"
#include "Scene/Scene.hlsl"

Texture2D<float4>		_tex_Albedo : register(t0);
Texture2D<float4>		_tex_Normal : register(t1);
Texture2D<float2>		_tex_MotionVectors : register(t2);
Texture2D<float>		_tex_Depth : register(t3);

Texture2DArray<float>	_tex_ShadowMap : register(t6);
Texture2DArray<float4>	_tex_Radiance : register(t8);
Texture2D<float4>		_tex_BentCone : register(t9);
Texture2D<float4>		_tex_FinalRender : register(t10);
Texture2D<float4>		_tex_SourceRadiance_PUSH : register(t11);
Texture2D<float4>		_tex_SourceRadiance_PULL : register(t12);

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

float	Depth2Weight( float _depth ) {
	return _depth < 1e-3 ? 0.0	// Keep uninitialized values as invalid
						 : smoothstep( 0.0, 1.0, _depth ) * smoothstep( 100.0, 40.0, _depth );	// Otherwise, we maximize the weights of samples whose depth is between 1 and 40 meters
}

float3	PS( float4 __Position : SV_POSITION ) : SV_TARGET0 {
	float2	UV = __Position.xy / _resolution;
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz + Z2Distance * Z_FAR * _tex_Depth[__Position.xy] * wsView;


//float2 distanceNearFar;
//float3	wsLightPosition = GetPointLightPosition( distanceNearFar );	// Returns world-space position for the point light and the (near,far) clip distances
//return 



return _exposure * _tex_FinalRender[__Position.xy].xyz;

#if 0	// DEBUG BENT CONE

	// Face-cam
	float3	wsRight = normalize( cross( wsView, _Camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	wsAt = -wsView;
//float3	N = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), -dot( wsNormal, wsView ) );	// Camera-space normal

	float4	csBentConeDev = _tex_BentCone[__Position.xy];
	float	cosConeAngle = length( csBentConeDev.xyz );
	float3	csBentCone = csBentConeDev.xyz / cosConeAngle;
	float	coneAngle = acos( cosConeAngle );
	float	stdDeviationAO = csBentConeDev.w;

	float3	wsBentCone = csBentCone.x * wsRight + csBentCone.y * wsUp + csBentCone.z * wsAt;

return 1-cosConeAngle;	// a.k.a. the ambient occlusion
//return coneAngle * 2.0 / PI;
return stdDeviationAO;
//return csBentCone;
return 0.5*(1.0+wsBentCone);
return wsBentCone;
return csBentConeDev.xyz;	// Show RAW value
#endif

#if 0	// DEBUG PUSH/PULL
	float4	V = (_flags & 0x100U) ? _tex_SourceRadiance_PULL.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex ) : _tex_SourceRadiance_PUSH.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex );
//	if ( _debugMipIndex == 0 )
//		V.w = Depth2Weight( V.w );
	return 0.1 * V.w;
//	return V.xyz / V.w;
	return V.xyz;
#endif

return 10.0 * _tex_ShadowMap.SampleLevel( LinearClamp, float3( __Position.xy / 512.0, _debugMipIndex ), 0.0 );	// Debug shadow map

//return _tex_Radiance[uint3(__Position.xy, _sourceRadianceIndex)].xyz;
//return _tex_Albedo[__Position.xy].xyz;
//return _tex_Normal[__Position.xy].xyz;
return _tex_Depth.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex );
return _tex_Depth.mips[_debugMipIndex][__Position.xy];
//return 0.5 * (1.0 + _tex_Normal[__Position.xy].xyz);
return float3( _tex_MotionVectors[__Position.xy], 0 );
}
