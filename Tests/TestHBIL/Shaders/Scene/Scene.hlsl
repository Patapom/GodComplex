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
	float3	emissive;		// Emissive radiance value for the material
	float	roughness;		// Surface roughness
	float3	F0;				// Surface's specular F0
	float3	wsVelocity;		// World-space velocity vector
};

// These methods must be provided by the scene
Intersection	TraceScene( float3 _wsPos, float3 _wsDir );
LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _roughness, float3 _IOR, float _pixelSize_m, float3 _wsBentNormal, float2 _cosConeAnglesMinMax, float _noise );
float3			GetPointLightPosition( out float2 _distanceNearFar );	// Returns world-space position for the point light and the (near,far) clip distances

////////////////////////////////////////////////////////////////////////////////
// Scene inclusion
////////////////////////////////////////////////////////////////////////////////
//

	// Ray-marched Scenes
#if SCENE_TYPE == 0
	#include "Scene/SceneRayMarchingLibrary.hlsl"
#elif SCENE_TYPE == 1
	#include "Scene/SceneRayMarchingInfiniteRooms.hlsl"
#elif SCENE_TYPE == 2
	#include "Scene/SceneRayTraceCornell.hlsl"
#elif SCENE_TYPE == 3
	#include "Scene/SceneRayTraceHeightfield.hlsl"

	// Mesh Scenes
#elif SCENE_TYPE == 4
	#include "Scene/SceneSponza.hlsl"
#elif SCENE_TYPE == 5
	#include "Scene/SceneSibenik.hlsl"
#elif SCENE_TYPE == 6
	#include "Scene/SceneEkoSewers.hlsl"
#elif SCENE_TYPE == 7
	#include "Scene/SceneSchaeffer.hlsl"
#endif
