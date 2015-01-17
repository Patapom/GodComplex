#include "Global.hlsl"
#include "AreaLight.hlsl"
#include "ParaboloidShadowMap.hlsl"

cbuffer CB_Object : register(b3) {
	float4x4	_Local2World;
	float4x4	_World2Local;
};

struct VS_IN {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
};

PS_IN	VS( VS_IN _In ) {

	PS_IN	Out;

	float3	wsPosition = mul( float4( _In.Position, 1.0 ), _Local2World ).xyz;

	// Transform into area light space
	float3	lsDeltaPos = wsPosition - _AreaLightT;
	float3	lsPosition = float3(	dot( lsDeltaPos, _AreaLightX ),
									dot( lsDeltaPos, _AreaLightY ),
									dot( lsDeltaPos, _AreaLightZ ) );

	// Apply paraboloid projection
	float	Distance = length( lsPosition );
	float3	lsDirection = lsPosition / Distance;

	float2	projPosition = lsDirection.xy / (1.0 + lsDirection.z);

	float	Z = saturate( Distance / SHADOW_ZFAR );


// Exponential Z
Z = exp( -EXP_CONSTANT * Z );
//Z = exp( EXP_CONSTANT * (Z-1.0) );


	Out.__Position = float4( projPosition, Z, 1.0 );

	return Out;
}

// float4	PS( PS_IN _In ) : SV_TARGET0 {
// }
void	PS( PS_IN _In ) {
}
