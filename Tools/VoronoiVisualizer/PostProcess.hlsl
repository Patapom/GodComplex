
cbuffer CB_Camera : register( b0 ) {
	float4x4	_World2Proj;
};

cbuffer CB_Mesh : register( b1 ) {
	float4		_Color;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
};

PS_IN	VS( PS_IN _In ) { return _In; }


Texture2D< float4 >	_TexWorldPosition : register(t0);

float4	PS( PS_IN _In ) : SV_TARGET0 {
	float4	wsPosition = _TexWorldPosition[_In.__Position.xy];
	clip( wsPosition.w - 1e-3 );

	return float4( wsPosition.xyz + float3( 0.0, 0.1, 0.0 ), wsPosition.w * 0.98 );
}


float4	PS2( PS_IN _In ) : SV_TARGET0 {
	float4	wsPosition = _TexWorldPosition[_In.__Position.xy];
	clip( wsPosition.w - 1e-3 );


}
