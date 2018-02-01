#include "Global.hlsl"
#include "SceneRayMarchingLibrary.hlsl"

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

struct PS_OUT {
	float3	albedo : SV_TARGET0;
	float3	normal : SV_TARGET1;
	float3	csVelocity : SV_TARGET2;	// Camera-space velocity
	float	depth : SV_DEPTH;
};

PS_OUT	PS_Depth( float4 __Position : SV_POSITION ) {
	float2	UV = __Position.xy / _resolution;

	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;

	Intersection	result = RayMarchScene( wsPos, wsView, UV, 100, 0.8 );

	PS_OUT	Out;
	Out.csVelocity = mul( float4( result.wsVelocity, 0.0 ), _World2Camera ).xyz;
	Out.albedo = result.albedo;
	if ( _flags & 0x8 )
		Out.albedo = dot( Out.albedo, LUMINANCE );
	if ( _flags & 0x10 )
		Out.albedo = _forcedAlbedo * float3( 1, 1, 1 );			// Force albedo (default = 50%)

	Out.normal = result.wsNormal;
	Out.depth = result.hitPosition.w / (Z2Distance * Z_FAR);	// Store Z
//	Out.depth = result.hitPosition.w / Z_FAR;					// Store distance

	return Out;
}
