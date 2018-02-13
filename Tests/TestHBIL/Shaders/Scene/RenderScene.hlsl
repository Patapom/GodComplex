#include "Global.hlsl"
#include "Scene/Scene.hlsl"
#include "SphericalHarmonics.hlsl"
#include "HBIL.hlsl"

#define USE_CONE_ANGLE 1	// Define this to use the cone aperture angle to get reduced estimate of the environment SH
							// If not defined, the full hemisphere is used and the result is later weighed by F0(AO) (Not as good a result IMHO)

#define USE_STD_DEV 1		// Define this to use the standard deviation of the cone aperture angle to estimate a smoothstep interval for direct lighting and bias the cone a little



float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

void	BuildCameraRay( float2 _UV, out float3 _wsPos, out float3 _csView, out float3 _wsView, out float _Z2Distance ) {
	_csView = BuildCameraRay( _UV );
	_Z2Distance = length( _csView );
	_csView /= _Z2Distance;
	_wsView = mul( float4( _csView, 0.0 ), _Camera2World ).xyz;
	_wsPos = _Camera2World[3].xyz;
}

////////////////////////////////////////////////////////////////////////////////
// Renders the scene G-Buffer
////////////////////////////////////////////////////////////////////////////////
// 
struct PS_OUT {
	float3	albedo : SV_TARGET0;
	float3	normal : SV_TARGET1;
	float3	emissive : SV_TARGET2;
	float3	csVelocity : SV_TARGET3;	// Camera-space velocity
	#if USE_DEPTH_STENCIL
		float	depth : SV_DEPTH;		// When using a regular depth-stencil buffer
	#else
		float	depth : SV_TARGET4;		// When using a R32 target with mips
	#endif
};

PS_OUT	PS_RenderGBuffer( float4 __Position : SV_POSITION ) {
	float2	UV = __Position.xy / _resolution;

	// Setup camera ray
	float3	wsPos, csView, wsView;
	float	Z2Distance;
	BuildCameraRay( UV, wsPos, csView, wsView, Z2Distance );

	Intersection	result = TraceScene( wsPos, wsView );

	PS_OUT	Out;
	Out.csVelocity = mul( float4( result.wsVelocity, 0.0 ), _World2Camera ).xyz;
	Out.albedo = result.albedo;
	if ( _flags & 0x20 )
		Out.albedo = dot( Out.albedo, LUMINANCE );				// Force monochrome
	if ( _flags & 0x40 )
		Out.albedo = _forcedAlbedo * float3( 1, 1, 1 );			// Force albedo (default = 50%)

	Out.emissive = result.emissive;
	Out.normal = result.wsNormal;
	Out.depth = result.wsHitPosition.w / (Z2Distance * Z_FAR);	// Store Z
//	Out.depth = result.wsHitPosition.w / Z_FAR;					// Store distance

	return Out;
}


////////////////////////////////////////////////////////////////////////////////
// Render shadow map
////////////////////////////////////////////////////////////////////////////////
//
cbuffer CB_Shadow : register(b3) {
	uint	_faceIndex;
};

float	PS_RenderShadow( float4 __Position : SV_POSITION ) : SV_DEPTH {

#if SCENE_TYPE == 1
	// FAILS COMPILING FOR LIBRARY SCENE FOR SOME REASON I CAN'T EXPLAIN (YET)

	float2	UV = __Position.xy / SHADOW_MAP_SIZE;

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
	return 1.0;
#endif
}


////////////////////////////////////////////////////////////////////////////////
// Computes final lighting
////////////////////////////////////////////////////////////////////////////////
// 
Texture2D<float4>		_tex_Albedo : register(t0);
Texture2D<float4>		_tex_Normal : register(t1);
Texture2D<float3>		_tex_Emissive : register(t2);
Texture2D<float>		_tex_Depth : register(t3);

Texture2DArray<float4>	_tex_Radiance : register(t8);
Texture2D<float4>		_tex_BentCone : register(t9);

struct PS_OUT_FINAL {
	float4	radiance : SV_TARGET0;
	float4	finalColor : SV_TARGET1;
};

PS_OUT_FINAL	PS_Light( float4 __Position : SV_POSITION ) {
	uint2	pixelPosition = uint2( floor( __Position.xy ) );
	float2	UV = __Position.xy / _resolution;

	// Setup camera ray
	float3	wsPos, csView, wsView;
	float	Z2Distance;
	BuildCameraRay( UV, wsPos, csView, wsView, Z2Distance );

	// Read back bent normal
	float4	csBentCone = _tex_BentCone[pixelPosition];
	float3	csBentNormal, wsBentNormal;
	float	cosConeAngle, stdDeviationAO;
	ReconstructBentCone( wsView, _Camera2World[1].xyz, csBentCone, csBentNormal, wsBentNormal, cosConeAngle, stdDeviationAO );

	// Check if we should use the bent normal (for direct & indirect lighting)
	float3	wsNormalDirect = _tex_Normal[pixelPosition].xyz;
	float3	wsNormalIndirect = wsNormalDirect;
	if ( _flags & 0x2 )
		wsNormalIndirect = wsBentNormal;
	if ( _flags & 0x8 ) {
		wsNormalDirect = wsBentNormal;
//wsNormalDirect = lerp( wsBentNormal, wsNormalDirect, 1-cosConeAngle );	// Experiment blending normals depending on AO => a fully open cone gives the bent normal
	}

	// Check if we should use the cone angle for indirect lighting
	float	cosSamplingConeAngle = 0.0;
	if ( _flags & 0x4 ) {
		#if USE_STD_DEV
			float	coneAngle = FastPosAcos( cosConeAngle );
			float	stdDeviationConeAngle = FastPosAcos( 1.0 - stdDeviationAO );
			float	samplingConeAngle = clamp( coneAngle + _coneAngleBias * stdDeviationConeAngle, 0.0, 0.5 * PI );	// -0.2 seems to be a good empirical value
			cosSamplingConeAngle = cos( samplingConeAngle );
		#else
			cosSamplingConeAngle = cosConeAngle;
		#endif
	}

	// Check if we should use the cone angle for direct lighting
	float2	cosConeAnglesMinMax = float2( 0, -1 );	// Make sure we're always inside cone so visibility is always 1
	if ( _flags & 0x10 ) {
		#if USE_STD_DEV
			float	coneAngle = FastPosAcos( cosConeAngle );
			float	stdDeviationConeAngle = FastPosAcos( 1.0 - stdDeviationAO );
//			cosConeAnglesMinMax = float2( cos( max( 0.0, coneAngle - abs(_coneAngleBias) * stdDeviationConeAngle ) ), cos( min( 0.5 * PI, coneAngle + abs(_coneAngleBias) * stdDeviationConeAngle ) ) );	// If deviation is available
			cosConeAnglesMinMax = float2( cos( max( 0.0, coneAngle - stdDeviationConeAngle ) ), cos( min( 0.5 * PI, coneAngle + stdDeviationConeAngle ) ) );	// If deviation is available
//cosConeAnglesMinMax = float2( cos( max( 0.0, coneAngle + stdDeviationConeAngle ) ), -1 );
		#else
//			cosConeAnglesMinMax = float2( cosConeAngle, _coneAngleBias );
//			cosConeAnglesMinMax = float2( cosConeAngle, 0.0 );
			cosConeAnglesMinMax = float2( cosConeAngle, 0.95 * cosConeAngle );
		#endif
	}

	// Read back albedo, depth/distance & rebuild world space position
	float3	albedo = _tex_Albedo[pixelPosition].xyz;
	float3	Z = Z_FAR * _tex_Depth[pixelPosition];
	wsPos += Z * Z2Distance * wsView;


	////////////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting

	// Retrieve IL gathered from the screen by HBIL
	float3	HBILIrradiance = _tex_Radiance[uint3( pixelPosition, 0 )].xyz;
	if ( (_flags & 1) == 0 )
		HBILIrradiance *= 0.0;

	// Compute this frame's distant environment coming from some SH probe
	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
	#if USE_CONE_ANGLE
		// Use bent-normal direction + cone angle for reduced SH estimate (I think it's better)
		float3	distantEnvironmentIrradiance = _environmentIntensity * EvaluateSHIrradiance( wsNormalIndirect, cosSamplingConeAngle, SH );
	#else
		// Use bent-normal direction at full aperture and attenuate by AO
		float3	distantEnvironmentIrradiance = _environmentIntensity * EvaluateSHIrradiance( wsNormalIndirect, SH );
		float	a = saturate( 1.0 - cosConeAngle );	// a.k.a. AO
		float	F0 = a * (1.0 + 0.5 * pow( 1.0 - a, 0.75 ));
		distantEnvironmentIrradiance *= F0;
	#endif

	float3	indirectIrradiance = HBILIrradiance + distantEnvironmentIrradiance;


	////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	LightingResult	lighting = LightScene( wsPos, wsNormalDirect, cosConeAnglesMinMax );

	float3	emissive = _tex_Emissive[pixelPosition];

	PS_OUT_FINAL	Out;
	Out.radiance = float4( emissive + (albedo / PI) * (lighting.diffuse + indirectIrradiance), 0 );		// Transform irradiance into radiance + add direct contribution. This is ready for reprojection next frame...
//Out.radiance = 0;
	Out.finalColor = float4( emissive + (albedo / PI) * (lighting.diffuse + indirectIrradiance + lighting.specular), 0 );
//Out.finalColor = 0.1 * sqDistance2Light;
//Out.finalColor.xyz = 0.1 * wsPos;
//Out.finalColor.xyz = 0.1 * Z;
//Out.finalColor.xyz = wsView;
//Out.finalColor.xyz = lighting.diffuse;
//Out.finalColor.xyz = wsBentNormal;
//Out.finalColor.xyz = cosConeAngle;
//Out.finalColor.xyz = cosConeAnglesMinMax.y;
//Out.finalColor.xyz = indirectIrradiance;
	return Out;
}

