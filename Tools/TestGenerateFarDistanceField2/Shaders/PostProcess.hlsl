#include "Global.hlsl"

Texture2D< float3 >	_TexSource : register(t0);
Texture3D< float4 >	_TexDistance : register(t1);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float4	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	PixelPos = uint2(_In.__Position.xy);
	float3	Color = _TexSource[PixelPos];

	uint2	CellPosXY = PixelPos >> 3;	// Cells are 8x8 pixels
	uint	CellPosZ = uint( 64.0 * abs( 1.0 - 2.0 * frac( 1.0 * iGlobalTime ) ) );
	float3	DistanceField = _TexDistance[uint3( CellPosXY, CellPosZ )].xyz;

//	Color = lerp( float3( 1, 0, 0 ), Color, saturate( DistanceField.w ) );
//	Color += 0.1 * DistanceField;

	float2	UV = _In.__Position.xy / iResolution.xy;
	if ( all( UV < 0.4 ) ) {
		UV /= 0.4;
		float	time = 0.25 * iGlobalTime;
//		float	time = 4.0 * iGlobalTime;
//		float3	UVW = float3( UV, abs( 2.0 * frac( time ) - 1.0 ) );
		float3	UVW = float3( UV, (0.5 + floor( 64.0 * abs( 2.0 * frac( time ) - 1.0 ) )) / 64.0 );
		Color = _TexDistance.SampleLevel( LinearClamp, UVW, 0.0 ).xyz;
//		Color = Color.z >= 1.0 ? float3( 0, 0, 0 ) : Color;
//return float4( Color, 1 );

//		float	Z = 1.0;
//		[unroll]
//		for ( uint i=0; i < 4; i++ ) Z = min( Z, _TexDistance.SampleLevel( PointClamp, float3( UV, (i+0.5) / 4.0 ), 0.0 ).z );
//		for ( uint i=0; i < 16; i++ ) Z = min( Z, _TexDistance.SampleLevel( PointClamp, float3( UV, (i+0.5) / 16.0 ), 0.0 ).z );

		float	Z = 0.0;
		uint3	iUVW = uint3( 64.0 * UV.x, 64.0 * (1.0 - UV.y), 0 );
		for ( ; iUVW.z < 64; iUVW.z++ ) {
			float4	distance = _TexDistance[iUVW];
			if ( distance.w < 1.0 ) {
				Z = iUVW.z / 64.0;
				break;
			}
		}

//		float	Z = 0.0;
//		float	SumWeights = 0.0;
//		[unroll]
//		for ( uint i=0; i < 64; i++ ) {
//			float biZ = _TexDistance.SampleLevel( PointClamp, float3( UV, (i+0.5) / 64.0 ), 0.0 ).z;
//			if ( biZ < 1.0 ) {
//				Z += biZ;
//				SumWeights += 1.0;
//			}
//		}
//		Z *= SumWeights > 0.0 ? 1.0 / SumWeights : 0.0;

		Color = Z;
	}

	return float4( Color, 1.0 );
}
