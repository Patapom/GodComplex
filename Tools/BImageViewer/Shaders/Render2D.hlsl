
cbuffer CB_Camera : register( b0 ) {
	float4x4	_World2Proj;
};

Texture2DArray<float4>		_Tex2D : register(t0);
TextureCubeArray<float4>	_TexCube : register(t1);


struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};


PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Out.__Position = _In.__Position;
	Out.UV = float2( 0.5 * (1.0 + _In.__Position.x), 0.5 * (1.0 - _In.__Position.y ) );

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
	return float4( _In.UV, 0, 1 );
}