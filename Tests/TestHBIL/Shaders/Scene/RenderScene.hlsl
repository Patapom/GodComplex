#include "Global.hlsl"
#include "Scene/Scene.hlsl"
#include "SphericalHarmonics.hlsl"

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
	if ( _flags & 0x8 )
		Out.albedo = dot( Out.albedo, LUMINANCE );
	if ( _flags & 0x10 )
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
	float	cosAverageConeAngle = length( csBentCone.xyz );
	float	averageConeAngle = FastPosAcos( cosAverageConeAngle );
	float	stdDeviationConeAngle = 0.5 * PI * (1.0 - csBentCone.w);
	float2	cosConeAnglesMinMax = float2( cos( max( 0.0, averageConeAngle - stdDeviationConeAngle ) ), cos( min( 0.5 * PI, averageConeAngle + stdDeviationConeAngle ) ) );

	float3	csBentNormal = csBentCone.xyz / cosAverageConeAngle;
//	float3	csBentNormal = normalize( csBentCone.xyz );

	float3	wsRight = normalize( cross( wsView, _World2Camera[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	wsBentNormal = csBentNormal.x * wsRight + csBentNormal.y * wsUp - csBentNormal.z * wsView;

//wsBentNormal = mul( float4( csBentNormal, 0 ), _Camera2World ).xyz;

	if ( (_flags & 2) == 0 ) {
		wsBentNormal = _tex_Normal[pixelPosition].xyz;
		cosConeAnglesMinMax = 0;
	}
	if ( (_flags & 4) == 0 )
		cosConeAnglesMinMax = float2( 0, -1 );	// Make sure we're always inside cone so visibility is always 1

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
	float	samplingConeAngle = clamp( averageConeAngle + _coneAngleBias * stdDeviationConeAngle, 0.0, 0.5 * PI );	// -0.2 is a good empirical value
	if ( (_flags & 4) == 0 )
		samplingConeAngle = 0.5 * PI;

	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
	float3	directEnvironmentIrradiance = EvaluateSHIrradiance( wsBentNormal, cos( samplingConeAngle ), SH );	// Use bent-normal direction + cone angle
//float3	directEnvironmentIrradiance = EvaluateSHIrradiance( wsBentNormal, SH );	// Use bent-normal direction + cone angle
			directEnvironmentIrradiance *= _environmentIntensity;

	float3	indirectIrradiance = HBILIrradiance + directEnvironmentIrradiance;


	////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	LightingResult	lighting = LightScene( wsPos, wsBentNormal, cosConeAnglesMinMax );

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
