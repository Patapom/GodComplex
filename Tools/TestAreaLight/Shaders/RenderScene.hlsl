

// Check cette ligne dans ward.mrpr, changer ce facteur hardcodé pour voir si ça limite pas un peu nos pics de spec
// anisotropicRoughness = max( 0.01, anisotropicRoughness );	// Make sure we don't go below 0.01 otherwise specularity is unnatural for our poor lights (only IBL with many samples would solve that!)


#include "Global.hlsl"
#include "AreaLight2.hlsl"
#include "ParaboloidShadowMap.hlsl"

cbuffer CB_Object : register(b4) {
	float4x4	_Local2World;
	float4x4	_World2Local;
	float3		_DiffuseAlbedo;
	float		_Gloss;
	float3		_SpecularTint;
	float		_Metal;
	uint		_UseTexture;
	uint		_FalseColors;
	float		_FalseColorsMaxRange;
};

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {
	
	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	Out.UV = _In.UV;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	float4	Debug = 0.0;
	
	float3	wsPosition = _In.Position;
	float3	wsNormal = normalize( _In.Normal );
	float3	wsView = normalize( wsPosition - _Camera2World[3].xyz );

	float	Roughness = 1.0 - _Gloss;
//	float	Roughness = abs( fmod( 0.5*iGlobalTime, 2.0 ) - 1.0 );
	
	const float3	RhoD = _DiffuseAlbedo;
	const float3	F0 = lerp( 0.04, _SpecularTint, _Metal );
	float3	IOR = Fresnel_IORFromF0( F0 );
	
	float	Shadow = ComputeShadow( wsPosition, wsNormal, Debug );

	float	RadiusFalloff = 8.0;
	float	RadiusCutoff = 10.0;

#if 0
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// VERSION 1
	//
 	float2	UV0, UV1;
 	float	SolidAngle;
 	float4	Debug;

	float3	wsReflectedView = reflect( wsView, wsNormal );

	// Compute diffuse lighting
	float3	Ld = 0.0;
	if ( ComputeSolidAngleDiffuse( wsPosition, wsNormal, UV0, UV1, SolidAngle, Debug ) ) {
		float3	Irradiance = SampleAreaLight( UV0, UV1 ).xyz;
//		float3	Irradiance = SampleAreaLight( _TexAreaLightSATFade, UV0, UV1 ).xyz;	// FADE?
		
		Ld = RhoD / PI * Irradiance * SolidAngle;
		
//return Debug;
	}
	
	// Compute specular lighting
	float3	Ls = 0.0;
  	if ( ComputeSolidAngleSpecular( wsPosition, wsNormal, wsReflectedView, _Gloss, UV0, UV1, SolidAngle, Debug ) ) {
		float3	Irradiance = SampleAreaLight( UV0, UV1 ).xyz;
		
//		Ls = ComputeWard( -wsView, wsNormal, wsReflectedView, Roughness ) * Irradiance * SolidAngle;
//		Ls = RhoD / PI * Irradiance * SolidAngle;
		Ls = 1.0 / PI * Irradiance * SolidAngle;
		
// float3	Pipo = normalize( lerp( wsReflectedView, wsNormal, Roughness ) );
// float	k = lerp( 0.4, 0.001, _Gloss );
// 		Ls = k * ComputeWard( -wsView, wsNormal, Pipo, Roughness ) * Irradiance * SolidAngle;
		
//Ls = ComputeWard( -wsView, wsNormal, wsReflectedView, Roughness ) * SolidAngle;
//Ls = SolidAngle;
//return 1 * float4( Irradiance, 0 );
//Ls = Debug;
	}
	
	// Compute Fresnel
	float	VdotN = saturate( -dot( wsView, wsNormal ) );
	float3	FresnelSpecular = FresnelAccurate( IOR, VdotN );
	
//FresnelSpecular = _Metal;
	
	float3	FresnelDiffuse = 1.0 - FresnelSpecular;
	
//Shadow = 1;
 //return Debug;

	float3	Result = 0.05 + Shadow * _AreaLightIntensity * (FresnelDiffuse * Ld + FresnelSpecular * Ls);
//	float3	Result = 0.05 + Ld + Ls;

	if ( _FalseColors )
		Result = float3( 1, 0, 0 );
//		Result = _TexFalseColors.SampleLevel( LinearClamp, float2( 0.01 * dot( LUMINANCE, Result ), 0.5 ), 0.0 ).xyz;

	return float4( Result, 1 );
	
#else

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// VERSION 2
	SurfaceContext	surf;
	surf.wsPosition = wsPosition;
	surf.wsNormal = wsNormal;
	surf.wsView = -wsView;	// In BSP, view points away from the surface 
	surf.diffuseAlbedo = RhoD / PI;
	surf.roughness = Roughness;
	surf.IOR = IOR;
	surf.fresnelStrength = 1.0;

	uint	AreaLightSliceIndex = _UseTexture ? 0 : ~0U;

	float3	RadianceDiffuse, RadianceSpecular;
	ComputeAreaLightLighting( surf, AreaLightSliceIndex, Shadow, float2( RadiusFalloff, RadiusCutoff ), RadianceDiffuse, RadianceSpecular );
	
//return float4( RadianceDiffuse, 0 );
//return float4( RadianceSpecular, 0 );

//return Shadow;

	float3	Result = 0.01 * float3( 1, 0.98, 0.8 ) + RadianceDiffuse + RadianceSpecular;
	if ( _FalseColors )
		Result = _TexFalseColors.SampleLevel( LinearClamp, float2( dot( LUMINANCE, Result ) / _FalseColorsMaxRange, 0.5 ), 0.0 ).xyz;
		

//Result = normalize( -_ProjectionDirectionDiff );


	return float4( Result, 1 );

#endif
}
