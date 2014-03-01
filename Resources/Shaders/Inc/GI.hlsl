////////////////////////////////////////////////////////////////////////////////////////
// Global Illumination Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _GI_INC_
#define _GI_INC_

#if USE_SHADOW_MAP
#include "Inc/ShadowMap.hlsl"
#endif

// Structured Buffer with our lights
struct	LightStruct
{
	uint	Type;
	float3	Position;
	float3	Direction;
	float3	Color;
	float4	Parms;						// X=Falloff radius, Y=Cutoff radius, Z=Cos(Falloff angle), W=Cos(Cutoff angle)
};
StructuredBuffer<LightStruct>	_SBLightsStatic : register( t7 );
StructuredBuffer<LightStruct>	_SBLightsDynamic : register( t8 );

// Structured Buffer with our probes
// This tiny probe struct is only 120 bytes long!! \o/ ^^
struct	ProbeStruct
{
	float3		Position;
	float		Radius;
	float3		SH[9];
};
StructuredBuffer<ProbeStruct>	_SBProbes : register( t9 );

// Scene descriptor
cbuffer	cbScene	: register( b10 )
{
	uint		_StaticLightsCount;
	uint		_DynamicLightsCount;
	uint		_ProbesCount;
};

// Object descriptor
cbuffer	cbObject	: register( b11 )
{
	float4x4	_Local2World;
};

// Material descriptor
cbuffer	cbMaterial	: register( b12 )
{
	uint		_MaterialID;
	float3		_DiffuseAlbedo;

	bool		_HasDiffuseTexture;
	float3		_SpecularAlbedo;

	bool		_HasSpecularTexture;
	float3		_EmissiveColor;

	float		_SpecularExponent;
	uint		_FaceOffset;	// The offset to apply to the object's face index
	bool		_HasNormalTexture;
};

// Optional textures associated to the material
Texture2D<float4>	_TexDiffuseAlbedo : register( t10 );
Texture2D<float4>	_TexNormal : register( t11 );
Texture2D<float4>	_TexSpecularAlbedo : register( t12 );


// Computes light's irradiance
float3	AccumulateLight( float3 _WorldPosition, float3 _WorldNormal, float3 _WorldVertexNormal, float3 _WorldVertexTangent, LightStruct _LightSource )
{
	float3	Irradiance;
	float3	Light;
	if ( _LightSource.Type == 0 || _LightSource.Type == 2 )
	{	// Compute a standard point light
		Light = _LightSource.Position - _WorldPosition;
		float	Distance2Light = length( Light );
		float	InvDistance2Light = 1.0 / Distance2Light;
		Light *= InvDistance2Light;

		Irradiance = _LightSource.Color * InvDistance2Light * InvDistance2Light;

		if ( _LightSource.Type == 2 )
		{	// Account for spots' angular falloff
			float	LdotD = -dot( Light, _LightSource.Direction );
			Irradiance *= smoothstep( _LightSource.Parms.w, _LightSource.Parms.z, LdotD );
		}
	}
	else if ( _LightSource.Type == 1 )
	{	// Compute a sneaky directional with shadow map
		Light = _LightSource.Direction;
		Irradiance = _LightSource.Color;	// Simple!

#if USE_SHADOW_MAP
		Irradiance *= ComputeShadowPCF( _WorldPosition, _WorldVertexNormal, _WorldVertexTangent, 0.1 );
#endif
	}

	float	NdotL = saturate( dot( _WorldNormal, Light ) );

	return Irradiance * NdotL;
}

#endif	// _GI_INC_
