struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Color : COLOR;
};

cbuffer CBCamera : register(b0) {
	float4x4	_World2Proj;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;
	Out.__Position = mul( float4( _In.Position, 1.0 ), _World2Proj );
	Out.Color = float3( _In.UV, 0 );
	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	return float4( _In.Color, 1.0 );
}