#include "Global.hlsl"

struct VS_IN {
	float3	P : POSITION;
	float3	N : NORMAL;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	color : COLOR;
};

PS_IN	VS( VS_IN _In ) {
	float3	wsPosition  = _wsLight2World[3].xyz
						+ _In.N.y * (_In.P.x * _wsLight2World[0].w) * _wsLight2World[0].xyz
						+ _In.N.y * (_In.P.y * _wsLight2World[1].w) * _wsLight2World[1].xyz;

	PS_IN	Out;
	Out.__Position = mul( float4( wsPosition, 1 ), _world2Proj );
	Out.color = 1;	// The illusion of light

//Out.color = _In.N;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	return _In.color;
}
