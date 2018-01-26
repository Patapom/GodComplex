#include "Global.hlsl"
#include "SceneRayMarchingLibrary.hlsl"

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

struct PS_OUT {
	float3	albedo : SV_TARGET0;
	float3	normal : SV_TARGET1;
	float2	motionVectors : SV_TARGET2;
	float	depth : SV_DEPTH;
};

// Builds an **unnormalized** camera ray from a screen UV
float3	BuildCameraRay2( float2 _UV ) {
	_UV = 2.0 * _UV - 1.0;
	_UV.x *= TAN_HALF_FOV * _resolution.x / _resolution.y;	// Account for aspect ratio
	_UV.y *= -TAN_HALF_FOV;									// Positive Y as we go up the screen
	return float3( _UV, 1.0 );								// Not normalized!
}

PS_OUT	PS_Depth( float4 __Position : SV_POSITION ) {
	float2	UV = __Position.xy / _resolution;

	float3	csView = BuildCameraRay2( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;

	Intersection	result = RayMarchScene( wsPos, wsView, UV );

	PS_OUT	Out;
	Out.motionVectors = float2( sin( 10.0 * UV.x + _time ), sin( PI * UV.y * UV.x - 0.9 * _time ) );
	Out.albedo = result.albedo;
	Out.normal = result.wsNormal;
	Out.depth = 0.01 * result.hitPosition.w;// / Z2Distance;
//Out.normal = wsView;

	return Out;
}
