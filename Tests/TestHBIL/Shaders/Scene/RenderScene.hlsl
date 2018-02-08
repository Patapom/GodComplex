#include "Global.hlsl"
#include "Scene/Scene.hlsl"
#include "SphericalHarmonics.hlsl"
#include "HBIL.hlsl"

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
	float3	csVelocity : SV_TARGET2;	// Camera-space velocity
	#if USE_DEPTH_STENCIL
		float	depth : SV_DEPTH;		// When using a regular depth-stencil buffer
	#else
		float	depth : SV_TARGET3;		// When using a R32 target with mips
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

	Out.normal = result.wsNormal;
	Out.depth = result.wsHitPosition.w / (Z2Distance * Z_FAR);	// Store Z
//	Out.depth = result.wsHitPosition.w / Z_FAR;					// Store distance

	return Out;
}


////////////////////////////////////////////////////////////////////////////////
// Computes final lighting
////////////////////////////////////////////////////////////////////////////////
// 
Texture2D<float4>		_tex_Albedo : register(t0);
Texture2D<float4>		_tex_Normal : register(t1);
Texture2D<float2>		_tex_MotionVectors : register(t2);
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
	float	cosAverageConeAngle, stdDeviationConeAngle;
	ReconstructBentCone( wsView, _Camera2World[1].xyz, csBentCone, csBentNormal, wsBentNormal, cosAverageConeAngle, stdDeviationConeAngle );

	// Check if we should use the bent normal (for direct & indirect lighting)
	float3	wsNormalDirect = _tex_Normal[pixelPosition].xyz;
	float3	wsNormalIndirect = wsNormalDirect;
	if ( _flags & 0x2 )
		wsNormalIndirect = wsBentNormal;
	if ( _flags & 0x8 ) {
		wsNormalDirect = wsBentNormal;
//wsNormalDirect = lerp( wsBentNormal, wsNormalDirect, 1-cosAverageConeAngle );	// Experiment blending normals depending on AO => a fully open cone gives the bent normal
	}

	// Check if we should use the cone angle (for direct & indirect lighting)
#define USE_STD_DEV 1
	float	cosSamplingConeAngle = 0.0;
	if ( _flags & 0x4 ) {
		#if USE_STD_DEV
			float	averageConeAngle = FastPosAcos( cosAverageConeAngle );
			float	samplingConeAngle = clamp( averageConeAngle + _coneAngleBias * stdDeviationConeAngle, 0.0, 0.5 * PI );	// -0.2 seems to be a good empirical value
			cosSamplingConeAngle = cos( samplingConeAngle );
		#else
			cosSamplingConeAngle = cosAverageConeAngle;
		#endif
	}
	float2	cosConeAnglesMinMax = float2( 0, -1 );	// Make sure we're always inside cone so visibility is always 1
	if ( _flags & 0x10 ) {
		#if USE_STD_DEV
			float	averageConeAngle = FastPosAcos( cosAverageConeAngle );
			cosConeAnglesMinMax = float2( cos( max( 0.0, averageConeAngle - stdDeviationConeAngle ) ), cos( min( 0.5 * PI, averageConeAngle + stdDeviationConeAngle ) ) );	// If deviation is available
		#else
//			cosConeAnglesMinMax = float2( cosAverageConeAngle, _coneAngleBias );
//			cosConeAnglesMinMax = float2( cosAverageConeAngle, 0.0 );
			cosConeAnglesMinMax = float2( cosAverageConeAngle, 0.95 * cosAverageConeAngle );
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
	float3	distantEnvironmentIrradiance = EvaluateSHIrradiance( wsNormalIndirect, cosSamplingConeAngle, SH );	// Use bent-normal direction + cone angle
//float3	distantEnvironmentIrradiance = EvaluateSHIrradiance( wsNormalIndirect, SH );	// Use bent-normal direction, full aperture
			distantEnvironmentIrradiance *= _environmentIntensity;

	apply F0!!

	float3	indirectIrradiance = HBILIrradiance + distantEnvironmentIrradiance;


	////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	LightingResult	lighting = LightScene( wsPos, wsNormalDirect, cosConeAnglesMinMax );

	PS_OUT_FINAL	Out;
	Out.radiance = float4( (albedo / PI) * (lighting.diffuse + indirectIrradiance), 0 );		// Transform irradiance into radiance + add direct contribution. This is ready for reprojection next frame...
//Out.radiance = 0;
	Out.finalColor = float4( (albedo / PI) * (lighting.diffuse + indirectIrradiance + lighting.specular), 0 );
//Out.finalColor = 0.1 * sqDistance2Light;
//Out.finalColor.xyz = 0.1 * wsPos;
//Out.finalColor.xyz = 0.1 * Z;
//Out.finalColor.xyz = wsView;
//Out.finalColor.xyz = lighting.diffuse;
//Out.finalColor.xyz = wsBentNormal;
//Out.finalColor.xyz = cosAverageConeAngle;
//Out.finalColor.xyz = cosConeAnglesMinMax.y;
//Out.finalColor.xyz = indirectIrradiance;
	return Out;
}
