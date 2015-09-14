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
	uint	CellPosZ = uint( 64.0 * abs( 1.0 - 2.0 * frac( 1.0 * iGlobalTime ) ) );
	float4	DistanceField = _TexDistance[uint3( CellPosXY, CellPosZ )];

//	Color = lerp( float4( 1, 0, 0, 1 ), Color, saturate( DistanceField.w ) );
//	Color += 0.1 * DistanceField;

	float2	UV = _In.__Position.xy / iResolution.xy;
	if ( all( UV < 0.2 ) ) {
		UV /= 0.2;
		float3	UVW = float3( UV, 0.5 * (1.0 + sin( 4.0 * iGlobalTime ) ) );
		Color.xyz = 1.0 * _TexDistance.SampleLevel( LinearClamp, UVW, 0.0 ).z;
		Color = Color.z >= 1.0 ? float4( 1, 0, 0, 0 ) : Color;
	}

	return Color;
}
