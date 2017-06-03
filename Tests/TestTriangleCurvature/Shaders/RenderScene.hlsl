//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};

static const float3	LIGHT_POSITION = float3( 0, 4, 2 );
static const float3	LIGHT_COLOR = 50.0;

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	wsPosition : POSITION;
	float3	wsNormal : NORMAL;

	float	sphereRadius : RADIUS;
	float3	wsFaceNormal : FACE_NORMAL;
	float3	wsFaceCenter: FACE_CENTER;

	float2	UV : TEXCOORD0;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Out.__Position = mul( float4( _In.Position, 1.0 ), _World2Proj );
	Out.wsPosition = _In.Position;	// Assume already in world space
	Out.wsNormal = _In.Normal;
	Out.wsFaceNormal = _In.Tangent;
	Out.wsFaceCenter = _In.BiTangent;
	Out.UV = _In.UV;

	// Compute sphere radius
	Out.sphereRadius = 1.0;

	return Out;
}

float	NDF_GGX( float _cosTheta, float _alpha ) {
	float	a2 = _alpha * _alpha;
	float	c2 = _cosTheta * _cosTheta;
	float	k  = 1 + c2 * (a2 - 1.0);
	return a2 / (PI * k * k);
}
float	SmithG( float _cosTheta, float _alpha ) {
	float	a2 = _alpha * _alpha;
	float	c = saturate( _cosTheta );
	float	c2 = c * c;
	return 2.0 * c / (c + sqrt( c2 + a2 * (1 - c2) ));
}

float3	PS( PS_IN _In ) : SV_TARGET0 {

	float3	wsPosition = _In.wsPosition;
	float3	wsNormal = normalize( _In.wsNormal );
	float3	wsView = normalize( _Camera2World[3].xyz - wsPosition );
	float3	wsLight = LIGHT_POSITION - wsPosition;
	float	distance2Light = length( wsLight );
			wsLight /= distance2Light;

	float3	H = normalize( wsLight + wsView );
	float	NdotH = saturate( dot( wsNormal, H ) );
	float	NdotL = dot( wsNormal, wsLight );
	float	NdotV = dot( wsNormal, wsView );

	const float3	F0 = 0.04;
	const float3	RhoD = float3( 0.05, 0.2, 0.6 );
	const float		roughness = 0.15;

	float3	F = FresnelAccurate( Fresnel_IORFromF0( F0 ), NdotH );
	float	G = SmithG( NdotL, roughness ) * SmithG( NdotV, roughness );
	float	D = NDF_GGX( NdotH, roughness );

	float3	Lin = LIGHT_COLOR / (distance2Light * distance2Light);
	float3	specularBRDF = F * G * D / (4.0 * NdotL * NdotV);
	float3	diffuseBRDF = (1.0 - F) * (INVPI * RhoD);
	float3	indirectDiffuse = 0.3 * (INVPI * RhoD);	// Should be coming from a pre-filtered cube map or something
	float3	indirectSpecular = 0.0;					// Should be coming from a pre-filtered cube map or something
	float3	Lout = (diffuseBRDF + specularBRDF) * saturate( NdotL ) * Lin
				 + indirectDiffuse
				 + indirectSpecular;

	return Lout;
}
