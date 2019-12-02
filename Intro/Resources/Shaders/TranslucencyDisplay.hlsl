//////////////////////////////////////////////////////////////////////////
// This shader finally displays the object with its diffused irradiance
//
#include "Inc/Global.hlsl"

Texture2D	_TexIrradiance	: register(t10);

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
	float4		_EmissiveColor;
	float4		_NoiseOffset;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float2	UV			: TEXCOORD0;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Normal		: NORMAL;
	float2	UV			: TEXCOORD0;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( Distort( _In.Position, _In.Normal, _NoiseOffset ), 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Normal = _In.Normal;

	Out.UV = 0.5 * (1.0 + _In.Position.xy);	// Planar mapping
	Out.UV.y = 1.0 - Out.UV.y;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//	return float4( _In.Normal, 1.0 );
//	return float4( _In.UV, 0, 1.0 );
//	return float4( 0.5 * abs(TEX( _TexNoise3D, LinearWrap, 0.04 * _In.Normal ).xyz), 1.0 );
	return float4( lerp( TEX( _TexIrradiance, LinearClamp, _In.UV ).xyz, _EmissiveColor.xyz, _EmissiveColor.w ), 1.0 );
}
