
struct VS_IN {
	float2	UV : TEXCOORD0;
	uint	InstanceID : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
//	float2	UV : TEXCOORD0;
	float3	Color : COLOR;
};

StructuredBuffer<float4>	_BufferPoints : register(t0);

PS_IN	VS( VS_IN _In ) {

	float4	P = _BufferPoints[_In.InstanceID];

	float	screenSize = 0.005 / P.z;
//	float3	dPos = screenSize * float3( _In.UV.x - 1.0, 1.0 - 2.0 * _In.UV.y, 0.0 );

	PS_IN	Out;
	Out.__Position = float4( P.xyz + float3( screenSize * _In.UV, 0.0 ), 1 );
//	Out.UV = float2( 0.5 * (1.0+_In.UV.x), 0.5*(1.0-_In.UV.y) );
	Out.Color = 2.0 * abs(P.w) * (P.w > 0.0 ? float3( 1, 0.5, 0.2 ) : float3( 0.2, 0.5, 1 ));

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
//	return float4( _In.UV, 0.5, 1 );
	return float4( _In.Color, 1 );
}