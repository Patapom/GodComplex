#include "Global.hlsl"
#include "Scene/Scene.hlsl"

float4	VS( float4 __position : SV_POSITION ) : SV_POSITION { return __position; }

////////////////////////////////////////////////////////////////////////////////
// Renders the scene G-Buffer
////////////////////////////////////////////////////////////////////////////////
// 
struct PS_OUT {
	float4	albedo : SV_TARGET0;		// XYZ=albedo, W=F0
	float4	normal : SV_TARGET1;		// XYZ=Camera-Space Normal, W=Roughness
	float3	emissive : SV_TARGET2;
	float2	psVelocity : SV_TARGET3;	// Clip-space velocity
	#if USE_DEPTH_STENCIL
		float	depth : SV_DEPTH;		// When using a regular depth-stencil buffer
	#else
		float	depth : SV_TARGET4;		// When using a R32 target with mips
	#endif
};

PS_OUT	PS_RenderGBuffer( float4 __position : SV_POSITION ) {
	float2	UV = __position.xy / _resolution;

	// Setup camera ray
	float3	wsPos, csView, wsView;
	float	Z2Distance;
	BuildCameraRay( UV, wsPos, csView, wsView, Z2Distance );

	Intersection	result = TraceScene( wsPos, wsView );

	PS_OUT	Out;

//	Out.csVelocity = mul( float4( result.wsVelocity, 0.0 ), _World2Camera ).xyz;

	Out.psVelocity = 0.0;
	if ( result.shade > 0.5 ) {
		float3	wsPosition = result.wsHitPosition.xyz;
		float4	psPosition = mul( float4( wsPosition, 1.0 ), _world2Proj );
		float4	psPrevPosition = mul( float4( wsPosition - result.wsVelocity, 1.0 ), _previousWorld2Proj );
		Out.psVelocity = psPosition.xy / psPosition.w - psPrevPosition.xy / psPrevPosition.w;
	}

	Out.albedo = float4( result.albedo, dot( result.F0, LUMINANCE ) );
//	if ( _flags & 0x20 )
//		Out.albedo.xyz = dot( Out.albedo.xyz, LUMINANCE );			// Force monochrome
	if ( _flags & 0x40 ) {
		if ( _flags & 0x20 )
			Out.albedo.xyz = _forcedAlbedo * Out.albedo.xyz / dot( Out.albedo.xyz, LUMINANCE );			// Force albedo (default = 50%)
		else
			Out.albedo.xyz = _forcedAlbedo * float3( 1, 1, 1 );			// Force albedo (default = 50%)
	}

	// Convert world-space normal into local camera-space
	float3	wsNormal = normalize( result.wsNormal );
	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	csNormal = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), -dot( wsNormal, wsView ) );

	Out.normal = float4( csNormal, result.roughness );
	Out.emissive = result.emissive;
	Out.depth = result.wsHitPosition.w / (Z2Distance * Z_FAR);	// Store Z
//	Out.depth = result.wsHitPosition.w / Z_FAR;					// Store distance

	return Out;
}


////////////////////////////////////////////////////////////////////////////////
// Render shadow map
////////////////////////////////////////////////////////////////////////////////
//
float	PS_ShadowPoint( float4 __position : SV_POSITION ) : SV_DEPTH {

#if SCENE_TYPE == 2	// Only for Cornell
	float2	UV = __position.xy / SHADOW_MAP_SIZE_POINT;

	float3x3	shadowMap2World;
	GetShadowMapTransform( _faceIndex, shadowMap2World );

	// Setup camera ray
	float3	csView = float3( 2.0 * UV.x - 1.0, 1.0 - 2.0 * UV.y, 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;

	// Transform into shadow-map space
	float3	wsView = mul( csView, shadowMap2World );
	float2	distanceNearFar = 10.0;
	float3	wsLightPos = GetPointLightPosition( distanceNearFar );

	Intersection	result = TraceScene( wsLightPos, wsView );
	return result.shade > 0.5 ? result.wsHitPosition.w / (Z2Distance * distanceNearFar.y) : 1.0;	// Store Z
#else
	// FAILS COMPILING FOR LIBRARY SCENE FOR SOME REASON I CAN'T EXPLAIN (YET)
	return 1.0;
#endif
}

float	PS_ShadowDirectional( float4 __position : SV_POSITION ) : SV_DEPTH {

#if SCENE_TYPE == 1 || SCENE_TYPE == 3	// Only for infinite rooms
	float2	UV = 2.0 * __position.xy / SHADOW_MAP_SIZE_DIRECTIONAL - 1.0;
			UV.y = -UV.y;

	// Setup camera ray
	float3	wsPosition = _directionalShadowMap2World[3].xyz
					   + UV.x * 0.5 * _directionalShadowMap2World[0].w * _directionalShadowMap2World[0].xyz
					   + UV.y * 0.5 * _directionalShadowMap2World[1].w * _directionalShadowMap2World[1].xyz;
	float3	wsView = _directionalShadowMap2World[2].xyz;

	Intersection	result = TraceScene( wsPosition, wsView );
	return result.shade > 0.5 ? result.wsHitPosition.w / _directionalShadowMap2World[2].w : 1.0;	// Store Z
#else
	return 1.0;
#endif
}

