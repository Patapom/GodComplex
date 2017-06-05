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
	float3	wsTangent : TANGENT;

	float3	wsSphereCenter: SPHERE_CENTER;
	float	sphereRadius : RADIUS;

	float2	UV : TEXCOORD0;
};

float	OffsetPosition( inout float3 _wsPosition, inout float3 _wsNormal, float3 _wsView, float3 _wsSphereCenter, float _sphereRadius ) {
	float3	P = _wsPosition;
	float3	V = _wsView;
	float3	C = _wsSphereCenter;

	float3	D = P - C;
	float	b = dot( D, V );
	float	c = dot( D, D ) - _sphereRadius * _sphereRadius;
	float	delta = b*b - c;
	float	offset = -b + sqrt( delta );

	_wsPosition += offset * V;
	_wsNormal = normalize( _wsPosition - _wsSphereCenter );

	return offset;
}

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Out.__Position = mul( float4( _In.Position, 1.0 ), _World2Proj );
	Out.wsPosition = _In.Position;	// Assume already in world space
	Out.wsNormal = _In.Normal;
	Out.wsTangent = _In.Tangent;
	Out.UV = _In.UV;
	Out.sphereRadius = _In.BiTangent.x;	// Computed and stored as a new component of the mesh
	Out.wsSphereCenter = Out.wsPosition - Out.sphereRadius * Out.wsNormal;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {

	float3	wsPosition = _In.wsPosition;
	float3	wsNormal = normalize( _In.wsNormal );
	float3	wsView = normalize( _Camera2World[3].xyz - wsPosition );
//return float3( frac( _In.UV ), 0 );

	float	offset = 0.0;
	if ( _Flags & 2U ) {
		offset = OffsetPosition( wsPosition, wsNormal, wsView, _In.wsSphereCenter, _In.sphereRadius );
	}
//return 0.001 * _In.sphereRadius;
//return abs(offset);
//return offset;
//return 0.25 * _In.sphereRadius;
//return 0.25 * length( wsPosition - _In.wsSphereCenter );

	if ( _Flags & 1U )
//		return normalize( _In.wsTangent );
		return wsNormal;

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
