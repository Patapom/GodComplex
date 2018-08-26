#include "Global.hlsl"
#include "Scene/Lighting.hlsl"

cbuffer CB_Object : register(b3) {
	float4x4	_local2World;
	float4x4	_previousLocal2World;
	float		_F0;
};

Texture2D<float4>	_tex_MeshAlbedo : register(t32);
Texture2D<float4>	_tex_MeshNormal : register(t33);
Texture2D<float3>	_tex_MeshSpecular : register(t34);
Texture2D<float3>	_tex_MeshEmissive : register(t35);

struct VS_IN {
	float3	lsPosition : POSITION;
	float3	lsNormal : NORMAL;
	float3	lsTangent : TANGENT;
	float3	lsBiTangent : BITANGENT;
	float3	UVW : TEXCOORD0;
};

struct PS_IN {
	float4	__position : SV_POSITION;
	float4	__projPosition : PROJPOSITION;
	float4	__prevProjPosition : PREV_PROJPOSITION;
	float3	wsPosition : POSITION;
	float3	wsNormal : NORMAL;
	float3	wsTangent : TANGENT;
	float3	wsBiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_OUT {
	float4	albedo : SV_TARGET0;		// XYZ=albedo, W=F0
	float4	normal : SV_TARGET1;		// XYZ=Camera-Space Normal, W=Roughness
	float3	emissive : SV_TARGET2;
	float2	psVelocity : SV_TARGET3;	// Clip-space velocity
};

PS_IN	VS( VS_IN _in ) {

	PS_IN		Out;
	Out.wsPosition = mul( float4( _in.lsPosition, 1.0 ), _local2World ).xyz;
	Out.wsNormal = normalize( mul( float4( _in.lsNormal, 0.0 ), _local2World ).xyz );
	Out.wsTangent = normalize( mul( float4( _in.lsTangent, 0.0 ), _local2World ).xyz );
	Out.wsBiTangent = normalize( mul( float4( _in.lsBiTangent, 0.0 ), _local2World ).xyz );
//	Out.wsBiTangent = normalize( mul( float4( cross( _in.lsNormal, _in.lsTangent ), 0.0 ), _local2World ).xyz );
	Out.UV = _in.UVW.xy;

//	float2	projPosition = Out.csPosition.xy / max( 1e-3, TAN_HALF_FOV * Out.csPosition.z );
//	float2	projPosition = Out.csPosition.xy / abs( TAN_HALF_FOV * Out.csPosition.z );
//	Out.__position = float4( projPosition.x * _resolution.y / _resolution.x, projPosition.y, Out.csPosition.z / Z_FAR, sign(Out.csPosition.z) );

//	float2	projPosition = Out.csPosition.xy / abs( TAN_HALF_FOV * Out.csPosition.z );
//	Out.__position = float4(	Out.csPosition.x * _resolution.y / (_resolution.x * TAN_HALF_FOV),
//								Out.csPosition.y / TAN_HALF_FOV,
//								Out.csPosition.z * Out.csPosition.z / Z_FAR,
//								Out.csPosition.z );

#if 1
	Out.__position = mul( float4( Out.wsPosition, 1.0 ), _world2Proj );

	Out.__projPosition = Out.__position;
	float4	wsPrevPosition = mul( float4( _in.lsPosition, 1.0 ), _previousLocal2World );
	Out.__prevProjPosition = mul( wsPrevPosition, _previousWorld2Proj );

#elif 1
	float	Z_NEAR = 0.01;
	float	Q = Z_FAR / (Z_FAR - Z_NEAR);

	Out.__position = float4(	Out.csPosition.x * _resolution.y / (_resolution.x * TAN_HALF_FOV),
								Out.csPosition.y / TAN_HALF_FOV,
								Q * (Out.csPosition.z - Z_NEAR),
								Out.csPosition.z
							);
#else
	Out.__position = float4(	Out.csPosition.x * _resolution.y / (_resolution.x * TAN_HALF_FOV),
								Out.csPosition.y / TAN_HALF_FOV,
								max( 1e-6, Out.csPosition.z * Out.csPosition.z ) / Z_FAR,
								Out.csPosition.z
							);
#endif

	return Out;
}


////////////////////////////////////////////////////////////////////////////////
// Renders the scene G-Buffer
////////////////////////////////////////////////////////////////////////////////
// 
PS_OUT	PS_RenderGBuffer( PS_IN _in ) {

	float4	albedoAlpha = _tex_MeshAlbedo.Sample( LinearWrap, _in.UV );
	clip( albedoAlpha.w - 0.5 );

//	float	F0 = 0.04;
	float	F0 = _F0;

#if SCENE_TYPE == 4
	albedoAlpha.xyz = pow( saturate( albedoAlpha.xyz ), 0.5 );	// Their textures are too ugly otherwise! :'(
#endif

	PS_OUT	Out;
	Out.psVelocity = _in.__projPosition.xy / _in.__projPosition.w - _in.__prevProjPosition.xy / _in.__prevProjPosition.w;
	Out.albedo = float4( albedoAlpha.xyz, F0 );
//	if ( _flags & 0x20 )
//		Out.albedo.xyz = dot( Out.albedo.xyz, LUMINANCE );			// Force monochrome
	if ( _flags & 0x40 ) {
		if ( _flags & 0x20 )
			Out.albedo.xyz = _forcedAlbedo * Out.albedo.xyz / dot( Out.albedo.xyz, LUMINANCE );			// Force albedo (default = 50%)
		else
			Out.albedo.xyz = _forcedAlbedo * float3( 1, 1, 1 );			// Force albedo (default = 50%)
	}
	Out.emissive = _tex_MeshEmissive.Sample( LinearWrap, _in.UV );

	float3	tsNormal = _tex_MeshNormal.Sample( LinearWrap, _in.UV ).xyz;
	float3	wsNormal = normalize( tsNormal.x * _in.wsTangent + tsNormal.y * _in.wsBiTangent + tsNormal.z * _in.wsNormal );
//wsNormal = _in.wsNormal;
//wsNormal = _in.wsTangent;
//wsNormal = _in.wsBiTangent;
//wsNormal = tsNormal;

	// Convert world-space normal into local camera-space
	float3	wsView = normalize( _in.wsPosition - _camera2World[3].xyz );
	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	csNormal = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), -dot( wsNormal, wsView ) );
	Out.normal.xyz = csNormal;

	// Only an assumption, Sponza doesn't use a PBR model so meh...
	float	roughness = 1.0 - dot( LUMINANCE, _tex_MeshSpecular.Sample( LinearWrap, _in.UV ).xyz );
			roughness = pow2( roughness );
	Out.normal.w = roughness;

	return Out;
}


////////////////////////////////////////////////////////////////////////////////
// Render shadow map
////////////////////////////////////////////////////////////////////////////////
//
float4	VS_ShadowPoint( VS_IN _in ) : SV_POSITION {

	float3		wsPosition = mul( float4( _in.lsPosition, 1.0 ), _local2World ).xyz;

	// Transform into point-light space
	float3x3	shadowMap2World;
	GetShadowMapTransform( _faceIndex, shadowMap2World );

	float3		lsPosition = mul( shadowMap2World, wsPosition - _wsPointLightPosition );
	return float4( lsPosition.x, lsPosition.y, lsPosition.z * lsPosition.z / _pointLightZFar, lsPosition.z );
}

float4	VS_ShadowDirectional( VS_IN _in ) : SV_POSITION {

	float3		wsPosition = mul( float4( _in.lsPosition, 1.0 ), _local2World ).xyz;

	// Transform into directional-light space
	float3		wsDelta = wsPosition - _directionalShadowMap2World[3].xyz;
	float3		lsDelta = float3( dot( wsDelta, _directionalShadowMap2World[0].xyz ), dot( wsDelta, _directionalShadowMap2World[1].xyz ), dot( wsDelta, _directionalShadowMap2World[2].xyz ) );
				lsDelta.x /= 0.5 * _directionalShadowMap2World[0].w;
				lsDelta.y /= 0.5 * _directionalShadowMap2World[1].w;
				lsDelta.z /= _directionalShadowMap2World[2].w;

	return float4( lsDelta, 1.0 );
}
