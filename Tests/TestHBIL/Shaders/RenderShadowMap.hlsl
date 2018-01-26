#include "Global.hlsl"

cbuffer CB_Object : register(b4) {
	float4x4	_Local2World;
	float4x4	_World2Local;
};

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
};

PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float3	wsPosition = mul( float4( _In.Position, 1.0 ), _Local2World ).xyz;
	float3	wsNormal = mul( float4( _In.Normal, 0.0 ), _Local2World ).xyz;

//wsPosition -= 0.1 * wsNormal;

	// Apply paraboloid projection
	float	Distance;
	float3	projPosition = World2Paraboloid( wsPosition, Distance );

	const float	BIAS = 0.0;

	float	Z = (Distance + BIAS) * _ShadowZFar.y;	// Distance / Far => in [0,1]

	// Exponential Z
	Z = exp( -_ShadowHardeningFactor.y * Z );

	Out.__Position = float4( projPosition.xy, 1.0 - Z, sign( projPosition.z ) );	// Store the complement of the 

	return Out;
}

// float4	PS( PS_IN _In ) : SV_TARGET0 {
// }
void	PS( PS_IN _In ) {
}
