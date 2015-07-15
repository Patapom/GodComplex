static const float	WIDTH = 783;
static const float	HEIGHT = 573;
static const float	ASPECT_RATIO = HEIGHT / WIDTH;

struct VS_IN {
	float2	UV : TEXCOORD0;
	uint	InstanceID : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
//	float2	UV : TEXCOORD0;
	float3	Color : COLOR;
//	float	W : W;
};

StructuredBuffer<float4>	_BufferPoints : register(t0);

PS_IN	VS( VS_IN _In ) {

	float4	P = _BufferPoints[_In.InstanceID];

	float2	dPos = (2.0 / (1.0 + P.w)) * float2( 1.0 / WIDTH, 1.0 / HEIGHT );
//	float3	dPos = screenSize * float3( _In.UV.x - 1.0, 1.0 - 2.0 * _In.UV.y, 0.0 );

	PS_IN	Out;
	Out.__Position = P.w > 0.0 ? float4( P.xyz + float3( dPos * _In.UV, 0.0 ), 1 ) : float4( 2, 0, 0, 1 );
//	Out.UV = float2( 0.5 * (1.0+_In.UV.x), 0.5*(1.0-_In.UV.y) );
//	Out.Color = 1.0 * abs(P.w) * (P.w > 0.0 ? float3( 1, 0.5, 0.2 ) : float3( 0.2, 0.5, 1 ));
	Out.Color = 2.0 * P.w * float3( 1, 0.5, 0.2 );
//	Out.Color = lerp( float3( 0.2, 0.5, 1 ), float3( 1, 0.5, 0.2 ), P.w );
//	Out.W = P.w;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
//	clip( _In.W );
//	return float4( _In.UV, 0.5, 1 );
	return float4( _In.Color, 1 );
}