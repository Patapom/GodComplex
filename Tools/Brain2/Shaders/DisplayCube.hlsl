
#include "Global.hlsl"

struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
};

struct VS_OUT {
	float4	__Position : SV_POSITION;
	float3	wsPosition : POSITION;
	float3	wsNormal : NORMAL;
};

VS_OUT	VS( VS_IN _In ) {
	VS_OUT	Out;

	Out.wsPosition = _In.Position;
	Out.wsNormal = _In.Normal;
	Out.__Position = mul( float4( _In.Position, 1 ), _world2Proj );

	return Out;
}

float4	PS( VS_OUT _In ) : SV_TARGET0 {
	return float4( 0.5 * (_In.wsNormal + 1.0), 0.1 );
}
