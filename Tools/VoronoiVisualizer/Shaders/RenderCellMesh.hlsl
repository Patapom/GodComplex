
cbuffer CB_Camera : register( b0 ) {
	float4x4	_World2Proj;
};

cbuffer CB_Mesh : register( b1 ) {
	float4		_Color;
};

struct VS_IN {
	float3	Position : POSITION;
	float3	Color : NORMAL;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Color : COLOR;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	float4		wsPosition = float4( _In.Position, 1.0 );
	Out.__Position = mul( wsPosition, _World2Proj );
	Out.Color = _In.Color;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
//	return _Color;
	return float4( _Color.xyz * _In.Color, 1 );
}

