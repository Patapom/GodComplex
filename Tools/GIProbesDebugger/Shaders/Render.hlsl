#include "Global.hlsl"

cbuffer CB_Main : register(b0) {
	float4x4	_Local2World;
	float4		_TargetSize;	// XY=Size, ZW=1/XY
	uint		_Type;			// Visualization type
};

TextureCubeArray<float4>	_TexCube : register(t0);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}

float4	PS( VS_IN _In ) : SV_TARGET0 {

	float2	UV = _In.__Position.xy * _TargetSize.zw;
	float2	TanFOV = float2( _TargetSize.x * _TargetSize.w, 1.0 ) * tan( 0.5 * 80.0 * PI / 180.0 );
	float3	csView = float3( TanFOV.x * (2.0 * UV.x - 1.0), TanFOV.y * (1.0 - 2.0 * UV.y), 1.0 );
	float3	wsView = normalize( mul( float4( csView, 1.0 ), _Local2World ).xyz );
//return float4( wsView, 1 );

	float4	Value = _TexCube.SampleLevel( LinearWrap, float4( wsView, _Type ), 0.0 );
	return float4( Value.xyz, 1 );
}
