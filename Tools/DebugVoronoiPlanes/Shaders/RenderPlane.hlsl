#include "Includes/global.hlsl"

cbuffer CB_Plane : register(b2) {
	float3	_wsPlanePosition;
	float	_PlaneSize;
	float3	_wsPlaneNormal;
	float3	_wsPlaneTangent;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	float3	wsPlaneBiTangent = cross( _wsPlaneNormal, _wsPlaneTangent );
	float3	wsPosition = _wsPlanePosition + _PlaneSize * (_In.Position.x * _wsPlaneTangent + _In.Position.y * wsPlaneBiTangent);

	Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );
	Out.wsPosition = _In.Position;
	Out.wsNormal = _In.Normal;
	Out.UV = 0.0;

	return Out;
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	return float4( 1, 1, 1, 0.2 );
}
