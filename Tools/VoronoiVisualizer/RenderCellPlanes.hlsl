
cbuffer CB_Camera {
	float4x4	_World2Proj;
};

struct Neighbor {
	float3	Position;
	float3	Color;
};
StructuredBuffer<Neighbor>	_BufferNeighbors : register(t0);

struct VS_IN {
	float2	UV : TEXCOORD0;
	uint	InstanceIndex : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Color : COLOR;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Neighbor	N = _BufferNeighbors[_In.InstanceIndex];

// N.Position = float3( 0, 0, 1 );
// N.Color = float3( 1, 0, 0 );

	float3		P = N.Position;
	float3		Normal = normalize( -N.Position );
	float3		Tangent = normalize( cross( float3( 0, 1, 0 ), Normal ) );
	float3		BiTangent = cross( Normal, Tangent );

	float4		wsPosition = float4( P + 10.0 * ((_In.UV.x - 0.5) * Tangent + (0.5 - _In.UV.y) * BiTangent), 1.0 );
	Out.__Position = mul( wsPosition, _World2Proj );
	Out.Color = N.Color;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	return float4( _In.Color, 1.0 );
}