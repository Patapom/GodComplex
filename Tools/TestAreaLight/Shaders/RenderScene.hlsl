

// Check cette ligne dans ward.mrpr, changer ce facteur hardcodé pour voir si ça limite pas un peu nos pics de spec
// anisotropicRoughness = max( 0.01, anisotropicRoughness );	// Make sure we don't go below 0.01 otherwise specularity is unnatural for our poor lights (only IBL with many samples would solve that!)


#include "Global.hlsl"
#include "AreaLight.hlsl"

cbuffer CB_Object : register(b2) {
	float4x4	_Local2World;
	float4x4	_World2Local;
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

float4	SampleSATSinglePixel( float2 _UV ) {

	float2	PixelIndex = _UV * TEX_SIZE;
	float2	NextPixelIndex = PixelIndex + 1;
	float2	UV2 = NextPixelIndex / TEX_SIZE;

	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xz );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.zy );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV + dUV.xy );

	return C11 - C10 - C01 + C00;
}

float4	SampleSAT( float2 _UV0, float2 _UV1 ) {
	// Sample SAT
	float4	C00 = _TexAreaLightSAT.Sample( LinearClamp, _UV0 );
	float4	C01 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV1.x, _UV0.y ) );
	float4	C10 = _TexAreaLightSAT.Sample( LinearClamp, float2( _UV0.x, _UV1.y ) );
	float4	C11 = _TexAreaLightSAT.Sample( LinearClamp, _UV1 );
	float4	C = C11 - C10 - C01 + C00;

	// Compute normalization factor
	float2	DeltaUV = _UV1 - _UV0;
	float	PixelsCount = (DeltaUV.x * TEX_SIZE.x) * (DeltaUV.y * TEX_SIZE.y);

// PixelsCount = 1.0;

	return C / (1e-6 + PixelsCount);
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
// 	float4	StainedGlass = _TexAreaLight.Sample( LinearClamp, _In.UV );
// 	float4	StainedGlass = 0.0001 * _TexAreaLightSAT.Sample( LinearClamp, _In.UV );
//	float4	StainedGlass = SampleSATSinglePixel( _In.UV );

	float3	wsPosition = _In.Position;
	float3	wsNormal = normalize( _In.Normal );
	float3	wsView = normalize( wsPosition - _Camera2World[3].xyz );

	const float3	RhoD = 0.5;	// 50% diffuse albedo
	const float3	F0 = 0.04;							// DIELECTRIC
//	const float3	F0 = float3( 0.95, 0.94, 0.93 );	// METAL

 	float2	UV0, UV1;
 	float	SolidAngle;
 	float4	Debug;

 	// Compute diffuse lighting
 	float3	Ld = 0.0;
	if ( ComputeSolidAngleDiffuse( wsPosition, wsNormal, UV0, UV1, SolidAngle, Debug ) ) {

//return Debug;
//return float4( 100.0 * (UV1 - UV0), 0, 1 );
// float4	Test = float4( UV0, UV1 );
// return Test;

//SolidAngle = 1;
//return _LightIntensity * SolidAngle;

		float3	Irradiance = _LightIntensity * SampleSAT( UV0, UV1 ).xyz;
		Ld = RhoD / PI * Irradiance * SolidAngle;
		return float4( Ld, 1 );
	}

Ld = float3( 1, 1, 0 );

	// Compute specular lighting

//Calculer l'intersection avec le frustum et le portal, puis utiliser le facteur de diffusion pour grossir les UVs!! On va pas s'faire chier hein!


	float3	Ls = 0.0;
// 	float3	wsReflectedView = reflect( wsView, wsNormal );
// 	float	TanHalfAngle = tan( (1.0 - _Gloss) * 0.5 * PI );
//  	if ( ComputeSolidAngleSpecular( wsPosition, wsNormal, wsReflectedView, TanHalfAngle, UV0, UV1, SolidAngle, Debug ) ) {
// 
// // return Debug;
// 
// 		float3	Irradiance = _LightIntensity * SampleSAT( UV0, UV1 ).xyz;
// 		Ls = RhoS * Irradiance * SolidAngle;
// 	}

	// Compute Fresnel
	float	VdotN = saturate( dot( wsView, wsNormal ) );
	float3	IOR = Fresnel_IORFromF0( F0 );
	float3	FresnelSpecular = FresnelAccurate( IOR, VdotN );
	float3	FresnelDiffuse = 1.0 - FresnelSpecular;

//	return float4( FresnelDiffuse * Ld + FresnelSpecular * Ls, 1 );
	return float4( Ld + Ls, 1 );
}
