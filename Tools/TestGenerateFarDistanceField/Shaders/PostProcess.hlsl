#include "Global.hlsl"

Texture2D< float4 >	_TexSource : register(t0);
Texture3D< float4 >	_TexDistance : register(t1);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float4	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	PixelPos = uint2(_In.__Position.xy);
	float4	Color = _TexSource[PixelPos];

	uint2	CellPosXY = PixelPos >> 3;	// Cells are 8x8 pixels
	uint	CellPosZ = uint( 64.0 * abs( 1.0 - 2.0 * frac( 0.1 * iGlobalTime ) ) );
	float4	DistanceField = _TexDistance[uint3( CellPosXY, CellPosZ )];

	Color = lerp( Color, float4( 1, 0, 0, 1 ), DistanceField.w );
//	Color += 0.1 * DistanceField;

	return Color;
}
