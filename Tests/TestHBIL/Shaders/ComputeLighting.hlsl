////////////////////////////////////////////////////////////////////////////////
// Computes the final lighting using the scene's G-Buffer
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "Scene/Scene.hlsl"
#include "SphericalHarmonics.hlsl"
#include "HBIL/HBIL.hlsl"

#define USE_CONE_ANGLE 1	// Define this to use the cone aperture angle to get reduced estimate of the environment SH
							// If not defined, the full hemisphere is used and the result is later weighed by F0(AO) (Not as good a result IMHO)

//#define USE_STD_DEV 1		// Define this to use the standard deviation of the cone aperture angle to estimate a smoothstep interval for direct lighting and bias the cone a little

Texture2D<float4>		_tex_Albedo : register(t0);
Texture2D<float4>		_tex_Normal : register(t1);
Texture2D<float3>		_tex_Emissive : register(t2);
Texture2D<float>		_tex_Depth : register(t3);
Texture2D<float>		_tex_BlueNoise : register(t4);

#if USE_RECOMPOSED_BUFFER
	Texture2D<float4>		_tex_Radiance : register(t8);
	Texture2D<float4>		_tex_BentCone : register(t9);
#else
	Texture2DArray<float4>	_tex_Radiance : register(t8);
	Texture2DArray<float4>	_tex_BentCone : register(t9);
#endif

struct PS_OUT_FINAL {
	float4	radiance : SV_TARGET0;
	float4	finalColor : SV_TARGET1;
};

float4	VS( float4 __position : SV_POSITION ) : SV_POSITION { return __position; }

PS_OUT_FINAL	PS( float4 __position : SV_POSITION ) {
	uint2	pixelPosition = uint2( floor( __position.xy ) );
	float2	UV = __position.xy / _resolution;
	float	noise = _tex_BlueNoise[pixelPosition&0x3F];

	// Setup camera space
	float3	wsPos, csView, wsView;
	float	Z2Distance;
	BuildCameraRay( UV, wsPos, csView, wsView, Z2Distance );

	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );

	// Read back depth and rebuild world space position
	float	Z = Z_FAR * _tex_Depth[pixelPosition];
	wsPos += Z * Z2Distance * wsView;
	float	pixelSize_m = Z * TAN_HALF_FOV / _resolution.y;

	// Read back bent cone and radiance from HBIL buffers
	float3	HBILIrradiance;
	float4	csBentCone;
	SampleHBILData( pixelPosition, _tex_Radiance, _tex_BentCone, HBILIrradiance, csBentCone );

	if ( _flags & 0x200 ) {
		// Debug AO
		PS_OUT_FINAL	Out;
		Out.radiance = 0.0;

		float	cosConeAngle = length( csBentCone.xyz );
		csBentCone.xyz / cosConeAngle;
		cosConeAngle *= cosConeAngle;	// Now stored as sqrt!

		Out.finalColor = 1.0 - cosConeAngle;

// Show bent normal
//csBentCone.xyz = _tex_Normal[pixelPosition].xyz;
//		float3	wsBentNormal = csBentCone.x * wsRight + csBentCone.y * wsUp - csBentCone.z * wsView;
//		Out.finalColor = float4( wsBentNormal, 0 );
//		Out.finalColor = float4( 0.5 * (1.0 + wsBentNormal), 0 );
//		Out.finalColor = length(csBentCone.xyz);
//		Out.finalColor = normalize( csBentCone.xyz );
		return Out;
	}

	// Read back bent normal
	float3	csBentNormal, wsBentNormal;
	float	cosConeAngle, stdDeviationAO;
	ReconstructBentCone( wsView, _camera2World[1].xyz, csBentCone, csBentNormal, wsBentNormal, cosConeAngle, stdDeviationAO );

	// Check if we should use the bent normal (for direct & indirect lighting)
	float4	csNormal_Roughness = _tex_Normal[pixelPosition];
	float3	wsNormalDirect = csNormal_Roughness.x * wsRight + csNormal_Roughness.y * wsUp - csNormal_Roughness.z * wsView;
	float3	wsBentNormalDirect = wsNormalDirect;
	float3	wsNormalIndirect = wsNormalDirect;
	if ( _flags & 0x2 )
		wsNormalIndirect = wsBentNormal;
	if ( _flags & 0x8 ) {
		wsBentNormalDirect = wsBentNormal;
//wsBentNormalDirect = lerp( wsBentNormal, wsNormalDirect, 1-cosConeAngle );	// Experiment blending normals depending on AO => a fully open cone gives the bent normal
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
//			float	coneAngle = FastPosAcos( cosConeAngle );
//			cosSamplingConeAngle = cos( clamp( coneAngle * (1.0+_coneAngleBias), 0.0, 0.5 * PI ) );	// -0.2 seems to be a good empirical value;
//			cosSamplingConeAngle = pow( saturate(cosConeAngle), 1.0 + _coneAngleBias );	// -0.2 seems to be a good empirical value;
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
//			cosConeAnglesMinMax = float2( cosConeAngle, 0.0 );
//			cosConeAnglesMinMax = float2( cosConeAngle, _coneAngleBias );
//			cosConeAnglesMinMax = float2( cosConeAngle, _coneAngleBias * cosConeAngle );
			cosConeAnglesMinMax = float2( cosConeAngle, 0.5 * cosConeAngle );
		#endif
	}

	// Read back albedo, roughness, F0
	float4	albedo_F0 = _tex_Albedo[pixelPosition];
	float	metal = saturate( albedo_F0.w - 0.04 ) / (1.0 - 0.04);
	float3	albedo = (1.0-metal) * albedo_F0.xyz;					// The more F0->1, the more the diffuse albedo tends to 0
	float3	F0 = albedo_F0.w * lerp( 1.0, albedo_F0.xyz, metal );	// The more F0->1, the more the albedo is used as a specular color
	float3	IOR = Fresnel_IORFromF0( F0 );
	float	roughness = csNormal_Roughness.w;


	////////////////////////////////////////////////////////////////////////////////
	// Compute indirect lighting

	// Retrieve IL gathered from the screen by HBIL
	if ( (_flags & 1) == 0 )
		HBILIrradiance = 0.0;

	// Compute this frame's distant environment coming from some SH probe
	float3	SH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };
	#if USE_CONE_ANGLE
		// Use bent-normal direction + cone angle for reduced SH estimate (I like it better)
		float3	distantEnvironmentIrradiance = _environmentIntensity * EvaluateSHIrradiance( wsNormalIndirect, cosSamplingConeAngle, SH );
	#else
		// Use bent-normal direction at full aperture and attenuate by AO
		float3	distantEnvironmentIrradiance = _environmentIntensity * EvaluateSHIrradiance( wsNormalIndirect, SH );
		float	a = saturate( 1.0 - cosConeAngle );	// a.k.a. AO
		float	f0 = a * (1.0 + 0.5 * pow( 1.0 - a, 0.75 ));
		distantEnvironmentIrradiance *= f0;
	#endif

	float3	indirectIrradiance = HBILIrradiance + distantEnvironmentIrradiance;
			indirectIrradiance *= 1.0 - F0;	// As per eq. 19


	////////////////////////////////////////////////////////////////////////////////
	// Compute direct lighting
	LightingResult	lighting = LightScene( wsPos, wsNormalDirect, -wsView, roughness, IOR, pixelSize_m, wsBentNormalDirect, cosConeAnglesMinMax, noise );

	float3	emissive = _tex_Emissive[pixelPosition];

	PS_OUT_FINAL	Out;
	Out.radiance = float4( emissive + (albedo / PI) * (lighting.diffuse + indirectIrradiance), 0 );		// Transform irradiance into radiance + add direct contribution. This is ready for reprojection next frame...
	Out.finalColor = _exposure * float4( emissive + (albedo / PI) * (lighting.diffuse + indirectIrradiance) + lighting.specular, 0 );

	if ( _flags & 0x400 ) {
//		Out.finalColor = float4( _exposure * indirectIrradiance * (1.0 / PI), 0 );
		Out.finalColor = float4( _exposure * HBILIrradiance * (1.0 / PI), 0 );
	}

// Various debug values to experiment with
//Out.finalColor = 0.1 * sqDistance2Light;
//Out.finalColor.xyz = 0.1 * wsPos;
//Out.finalColor.xyz = 0.1 * Z;
//Out.finalColor.xyz = wsView;
//Out.finalColor.xyz = lighting.diffuse;
//Out.finalColor.xyz = lighting.specular;
//Out.finalColor.xyz = wsBentNormal;
//Out.finalColor.xyz = cosConeAngle;
//Out.finalColor.xyz = cosConeAnglesMinMax.y;
//Out.finalColor.xyz = indirectIrradiance;
//Out.finalColor.xyz = albedo;

	return Out;
}
