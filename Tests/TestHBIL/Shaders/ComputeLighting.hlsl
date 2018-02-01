////////////////////////////////////////////////////////////////////////////////
// Computes final lighting
////////////////////////////////////////////////////////////////////////////////
// 
#include "Global.hlsl"
#include "SphericalHarmonics.hlsl"

Texture2D<float4>		_tex_Albedo : register(t0);
Texture2D<float4>		_tex_Normal : register(t1);
Texture2D<float2>		_tex_MotionVectors : register(t2);
Texture2D<float>		_tex_Depth : register(t3);

Texture2DArray<float4>	_tex_Radiance : register(t8);
Texture2D<float4>		_tex_BentCone : register(t9);

float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

struct PS_OUT {
	float4	radiance : SV_TARGET0;
	float4	finalColor : SV_TARGET1;
};

PS_OUT	PS( float4 __Position : SV_POSITION ) {
	uint2	pixelPosition = uint2( floor( __Position.xy ) );
	float2	UV = __Position.xy / _resolution;

	// Setup camera ray
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;

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

	// Read back albedo, depth/distance & rebuild world space position
	float3	albedo = _tex_Albedo[pixelPosition].xyz;
	float3	Z = Z_FAR * _tex_Depth[pixelPosition];
	float3	wsPos = _Camera2World[3].xyz + Z * Z2Distance * wsView;

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
	const float3	LIGHT_POS = float3( 0, 10, 0 );
	const float3	LIGHT_FLUX = 1000.0;

	float3	wsPosition2Light = LIGHT_POS - wsPos;
	float	sqDistance2Light = dot( wsPosition2Light, wsPosition2Light );
			wsPosition2Light /= sqrt( sqDistance2Light );
	float	NdotL = saturate( dot( wsPosition2Light, wsBentNormal ) );

	float3	lightIrradiance = LIGHT_FLUX / (4.0 * PI * sqDistance2Light);	// Assume point source

	float	coneVisibility = smoothstep( cosConeAnglesMinMax.y, cosConeAnglesMinMax.x, NdotL );		// Check if the light is standing inside the visibility cone of the surface
	if ( (_flags & 4) == 0 )
		coneVisibility = 1.0;
	float3	diffuse = coneVisibility * NdotL * lightIrradiance;

	float3	specular = 0.0;	// #TODO

	PS_OUT	Out;
	Out.radiance = float4( (albedo / PI) * (diffuse + indirectIrradiance), 0 );		// Transform irradiance into radiance + add direct contribution. This is ready for reprojection next frame...
//Out.radiance = 0;
	Out.finalColor = float4( (albedo / PI) * (diffuse + indirectIrradiance + specular), 0 );
//Out.finalColor = 0.1 * sqDistance2Light;
//Out.finalColor.xyz = 0.1 * wsPos;
//Out.finalColor.xyz = 0.1 * Z;
//Out.finalColor.xyz = wsView;
//Out.finalColor.xyz = diffuse;
//Out.finalColor.xyz = wsBentNormal;
//Out.finalColor.xyz = cosAverageConeAngle;
//Out.finalColor.xyz = cosConeAnglesMinMax.y;
//Out.finalColor.xyz = indirectIrradiance;
	return Out;
}
