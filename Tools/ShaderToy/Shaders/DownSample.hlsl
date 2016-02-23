Texture2DArray<float4>	_TexSource : register(t0);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float4	Color : SV_TARGET0;
	float4	Depth : SV_TARGET1;
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


VS_IN	VS( VS_IN _In ) {
	return _In;
}

PS_OUT	PS( VS_IN _In ) {
	PS_OUT	Out;

	uint3	pos = uint3( 2 * _In.__Position.xy, 0 );
	float4	V00 = _TexSource[pos + uint3( 0, 0, 0 )];
	float4	D00 = _TexSource[pos + uint3( 0, 0, 1 )];
	float4	V01 = _TexSource[pos + uint3( 1, 0, 0 )];
	float4	D01 = _TexSource[pos + uint3( 1, 0, 1 )];
	float4	V11 = _TexSource[pos + uint3( 1, 1, 0 )];
	float4	D11 = _TexSource[pos + uint3( 1, 1, 1 )];
	float4	V10 = _TexSource[pos + uint3( 0, 1, 0 )];
	float4	D10 = _TexSource[pos + uint3( 0, 1, 1 )];

	Out.Color = 0.25 * (V00 + V01 + V10 + V11);
	Out.Depth = float4( 0.25 * (D00.x + D01.x + D10.x + D11.x), min( min( min( D00.y, D01.y ), D10.y ), D11.y ), max( max( max( D00.z, D01.z ), D10.z ), D11.z ), 0 );

	return Out;
}
