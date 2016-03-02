#include "Includes/global.hlsl"

cbuffer CB_Plane : register(b2) {
	float3	_wsPlanePosition;
	float	_PlaneSizeX0;
	float3	_wsPlaneNormal;
	float	_PlaneSizeX1;
	float3	_wsPlaneTangent;
	float	_PlaneSizeY;
	float4	_PlaneColor;
	uint	_Flags;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	float3	wsPlaneBiTangent = cross( _wsPlaneNormal, _wsPlaneTangent );
	float2	PlaneSize = float2( lerp( _PlaneSizeX0, _PlaneSizeX1, _In.UV.y ), _PlaneSizeY );
	float3	wsPosition = _wsPlanePosition + _In.Position.x * PlaneSize.x * _wsPlaneTangent + _In.Position.y * PlaneSize.y * wsPlaneBiTangent;

	Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );
	Out.wsPosition = _In.Position;
	Out.wsNormal = _In.Normal;
	Out.UV = _In.UV;

	return Out;
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	float	value = 1.0;
	if ( _Flags & 1 ) {
		float2	UV = 2.0 * _In.UV - 1.0;
		value = 1.0 - dot( UV, UV );
	}
	clip( value );

	return _PlaneColor;
}
