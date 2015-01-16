

// Check cette ligne dans ward.mrpr, changer ce facteur hardcodé pour voir si ça limite pas un peu nos pics de spec
// anisotropicRoughness = max( 0.01, anisotropicRoughness );	// Make sure we don't go below 0.01 otherwise specularity is unnatural for our poor lights (only IBL with many samples would solve that!)


#include "Global.hlsl"
#include "AreaLight.hlsl"
#include "ParaboloidShadowMap.hlsl"

#define USE_SAT	1	// If not defined, will use mip mapping instead

cbuffer CB_Object : register(b3) {
	float4x4	_Local2World;
	float4x4	_World2Local;
	float3		_DiffuseAlbedo;
	float		_Gloss;
	float3		_SpecularTint;
	float		_Metal;
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

// More a Beckmann really, don't have time
float	ComputeWard( float3 _wsView, float3 _wsNormal, float3 _wsLight, float _Roughness ) {
	float3	H = normalize( _wsView + _wsLight );
	float	NdotH = dot( _wsNormal, H );
	return exp( -(1.0 - NdotH*NdotH) / (_Roughness*_Roughness*NdotH*NdotH) ) / (PI * _Roughness*_Roughness * NdotH*NdotH*NdotH*NdotH);
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	float3	wsPosition = _In.Position;
	float3	wsNormal = normalize( _In.Normal );
	float3	wsView = normalize( wsPosition - _Camera2World[3].xyz );
	
	const float3	RhoD = _DiffuseAlbedo;
	const float3	F0 = lerp( 0.04, _SpecularTint, _Metal );

 	float2	UV0, UV1;
 	float	SolidAngle;
 	float4	Debug;
	
 	// Compute diffuse lighting
 	float3	Ld = 0.0;
	if ( ComputeSolidAngleDiffuse( wsPosition, wsNormal, UV0, UV1, SolidAngle, Debug ) ) {
#if USE_SAT
//		float3	Irradiance = SampleSAT( _TexAreaLightSAT, UV0, UV1 ).xyz;
		float3	Irradiance = SampleSAT( _TexAreaLightSATFade, UV0, UV1 ).xyz;
#else
		float3	Irradiance = SampleMip( _TexAreaLight, UV0, UV1 ).xyz;
#endif

//return Debug;

		Ld = RhoD / PI * Irradiance * SolidAngle;
	}
	
	// Compute specular lighting
	float3	Ls = 0.0;
 	float3	wsReflectedView = reflect( wsView, wsNormal );
  	if ( ComputeSolidAngleSpecular( wsPosition, wsNormal, wsReflectedView, _Gloss, UV0, UV1, SolidAngle, Debug ) ) {
#if USE_SAT
		float3	Irradiance = SampleSAT( _TexAreaLightSAT, UV0, UV1 ).xyz;
#else
		float3	Irradiance = SampleMip( _TexAreaLight, UV0, UV1 ).xyz;
#endif
		
//		float	Roughness = max( 0.005, 1.0 * (1.0 - _Gloss) );
		float	Roughness = max( 0.01, 1.0 * (1.0 - _Gloss) );
		
//		Ls = ComputeWard( -wsView, wsNormal, wsReflectedView, Roughness ) * Irradiance * SolidAngle;
		Ls = RhoD / PI * Irradiance * SolidAngle;
		
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
	float3	IOR = Fresnel_IORFromF0( F0 );
	float3	FresnelSpecular = FresnelAccurate( IOR, VdotN );
	
//FresnelSpecular = _Metal;
	
	float3	FresnelDiffuse = 1.0 - FresnelSpecular;
	
	float	Shadow = 1;// ComputeShadow( wsPosition, wsNormal, Debug );
	
 //return Debug;

	return float4( 0.05 + Shadow * _AreaLightIntensity * (FresnelDiffuse * Ld + FresnelSpecular * Ls), 1 );
	return float4( 0.05 + Ld + Ls, 1 );
}
