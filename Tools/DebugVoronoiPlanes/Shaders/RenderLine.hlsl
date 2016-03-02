#include "Includes/global.hlsl"

cbuffer CB_Line : register(b2) {
	float3	_wsPosition0;
	float	_LineThickness;
	float3	_wsPosition1;
	float3	_wsOrtho;
	float4	_LineColor;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	float3	wsPosition  = lerp( _wsPosition0, _wsPosition1, _In.UV.x )
						+ (2.0 * _In.UV.y - 1.0) * _LineThickness * _wsOrtho;

	Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );
	Out.wsPosition = _In.Position;
	Out.wsNormal = _In.Normal;
	Out.UV = _In.UV;

	return Out;
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	return _LineColor;
}
