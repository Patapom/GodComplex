
cbuffer CB_Camera : register( b0 ) {
	float4x4	_World2Proj;
};

cbuffer CB_Mesh : register( b1 ) {
	float		_SH[9];
	float		_ZH[3];
	float		_resultSH[9];
	uint		_flags;
};

struct VS_IN {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Normal : NORMAL;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	float4		wsPosition = float4( _In.Position, 1.0 );
	Out.__Position = mul( wsPosition, _World2Proj );
	Out.Normal = _In.Position;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	return abs( _In.Normal );
}