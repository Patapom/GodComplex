#include "Global.hlsl"

cbuffer CB_Object : register(b2) {
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
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {
	
	PS_IN	Out;

	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Position = WorldPosition.xyz;
	Out.Normal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;
	Out.Tangent = mul( float4( _In.Tangent, 0.0 ), _Local2World ).xyz;
	Out.BiTangent = mul( float4( _In.BiTangent, 0.0 ), _Local2World ).xyz;
	Out.UV = _In.UV;
	
	return Out;
}

struct SurfaceContext {
	float3	wsPosition;
	float3	wsNormal;
	float3	wsTangent;
	float3	wsBiTangent;
	float3	wsView;
	float3	diffuseAlbedo;
	float	roughness;
	float3	IOR;
	float	fresnelStrength;
};

// From http://graphicrants.blogspot.fr/2013/08/specular-brdf-reference.html
float	Smith_GGX( float _dot, float _alpha2 ) {
	return 2.0 * _dot / (_dot + sqrt( _alpha2 + (1.0 - _alpha2) * _dot*_dot ));
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	float4	Debug = 0.0;

	float3	wsPosition = _In.Position;
	float3	wsNormal = normalize( _In.Normal );
	float3	wsTangent = normalize( _In.Tangent );
	float3	wsBiTangent = normalize( _In.BiTangent );
	float3	wsView = normalize( wsPosition - _Camera2World[3].xyz );
	
	float	Roughness = 1.0 - _Gloss;
			Roughness *= Roughness * Roughness;
			Roughness = max( 0.005, Roughness );

	const float3	RhoD = _DiffuseAlbedo;
	const float3	F0 = lerp( 0.04, _SpecularTint, _Metal );
	float3	IOR = Fresnel_IORFromF0( F0 );
	
	
// 	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 	// Add small normal perturbations
// 	wsNormal.x += 0.05 * sin( 1000.0 * wsPosition.x );
// 	wsNormal.z += 0.05 * sin( 1000.0 * wsPosition.z );
// 	wsNormal = normalize( wsNormal );
	
// 	float3	tsNormal = _TexNormal.Sample( LinearWrap, 10.0 * _In.UV );
// 	wsNormal = tsNormal.x * wsTangent + tsNormal.y * wsBiTangent + tsNormal.z * wsNormal;
// 	wsTangent = normalize( cross( wsNormal, wsBiTangent ) );
// 	wsBiTangent = cross( wsTangent, wsNormal );

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Prepare surface
	SurfaceContext	surf;
	surf.wsPosition = wsPosition;
	surf.wsNormal = wsNormal;
	surf.wsTangent = wsTangent;
	surf.wsBiTangent = wsBiTangent;
	surf.wsView = -wsView;	// In BSP, view points away from the surface 
	surf.diffuseAlbedo = RhoD / PI;
	surf.roughness = Roughness;
	surf.IOR = IOR;
	surf.fresnelStrength = 1.0;


	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Prepare a single light
	const float3	wsLightPos = float3( 0.0, 3.0, 0.0 );	// Assume point light
	const float3	LightIntensity = 20.0;

	float3	wsLight = wsLightPos - surf.wsPosition;
	float	Distance2Light = length( wsLight );
	wsLight *= Distance2Light > 1e-6 ? 1.0 / Distance2Light : 0.0;

	float	Shadow = 1.0;//ComputeShadow( wsPosition, wsNormal, Debug );
	float	RadiusFalloff = 16.0;
	float	RadiusCutoff = 20.0;
	float	Attenuation = Shadow * smoothstep( RadiusCutoff, RadiusFalloff, Distance2Light ) / (Distance2Light * Distance2Light);

	float3	Radiance_in = LightIntensity * Attenuation;

//return float4( Radiance_in, 1 );

	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// BRDF computation (GGX + Smith shadowing)
	float3	Half = normalize( wsLight + surf.wsView );
	float	LdotN = saturate( dot( wsLight, surf.wsNormal ) );
	float	VdotN = saturate( dot( surf.wsView, surf.wsNormal ) );
	float	HdotN = dot( Half, surf.wsNormal );

		// Specular
	float	alpha2 = surf.roughness * surf.roughness;
	float	den = (HdotN * HdotN * (alpha2 - 1.0) + 1.0);
	float	GGX = alpha2 / (PI * den * den);	// TODO: Find out what happens when varying the denominator's exponent (need to find proper normalization!)
	float	Smith = Smith_GGX( LdotN, alpha2 ) * Smith_GGX( VdotN, alpha2 );
	float3	Fresnel_specular = FresnelAccurate( surf.IOR, HdotN );
	float3	BRDF_specular = Fresnel_specular * Smith * GGX / (4.0 * LdotN * VdotN);

		// Diffuse
	float3	Fresnel_diffuse = 1.0 - Fresnel_specular;
	float3	BRDF_diffuse = Fresnel_diffuse * surf.diffuseAlbedo;

	float3	Radiance_out = Radiance_in * (BRDF_diffuse + BRDF_specular) * LdotN;

	float3	Result = 0.01 * float3( 1, 0.98, 0.8 ) + Radiance_out;
	return float4( Result, 1 );
}
