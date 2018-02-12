////////////////////////////////////////////////////////////////////////////////
// Scene Management
////////////////////////////////////////////////////////////////////////////////
//
#include "Scene/Lighting.hlsl"

struct Intersection {
	float4	wsHitPosition;	// World-space hit position (XYZ) and hit distance (W)
	float	shade;			// 1 if valid (hit), 0 for invalid (miss)
	float3	wsNormal;		// World-space normal
	float	materialID;		// Material ID (integer part) + salt (decimal part)
	float3	albedo;			// Surface albedo
	float	roughness;		// Surface roughness
	float3	F0;				// Surface's specular F0
	float3	wsVelocity;		// World-space velocity vector
};

// These methods must be provided by the scene
Intersection	TraceScene( float3 _wsPos, float3 _wsDir );
LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax );
float3			GetPointLightPosition( out float2 _distanceNearFar );	// Returns world-space position for the point light and the (near,far) clip distances

////////////////////////////////////////////////////////////////////////////////
// Scene inclusion
////////////////////////////////////////////////////////////////////////////////
//
#if SCENE_TYPE == 0
	#include "Scene/SceneRayMarchingLibrary.hlsl"
#elif SCENE_TYPE == 1
	#include "Scene/SceneRayTraceCornell.hlsl"
#elif SCENE_TYPE == 2
	#include "Scene/SceneRayTraceHeightfield.hlsl"
#endif
