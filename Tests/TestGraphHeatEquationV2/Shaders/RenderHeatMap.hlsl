#include "Global.hlsl"

Texture2D< float4 >	_texHeatMap : register(t0);
Texture2D< float4 >	_texObstacles : register(t1);
Texture2D< float3 >	_texFalseColors : register(t2);
Texture2D< float3 >	_texSearch : register(t3);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

static const float3	s_cellColors[8] = {
	float3( 1, 0, 0 ),
	float3( 1, 1, 0 ),
	float3( 0, 1, 0 ),
	float3( 0, 1, 1 ),
	float3( 0, 0, 1 ),
	float3( 1, 0, 1 ),
	float3( 1, 1, 1 ),
	float3( 1, 0.5, 0.25 ),
};


float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / 512.0;
//return float3( UV, 0 );
	float4	obstacles = _texObstacles[1 + GRAPH_SIZE * (_In.__Position.xy-0.5) / 512.0];
	if ( obstacles.x )
		return float3( 0.9, 0.8, 0 );

	float3	color = 0;

	float4	V00 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( -1, -1 ) );
	float4	V10 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2(  0, -1 ) );
	float4	V20 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( +1, -1 ) );
	float4	V01 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( -1,  0 ) );
	float4	V11 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2(  0,  0 ) );
	float4	V21 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( +1,  0 ) );
	float4	V02 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( -1, +1 ) );
	float4	V12 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2(  0, +1 ) );
	float4	V22 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( +1, +1 ) );

	uint	displayMode = (flags >> 1) & 0x3U;
	bool	showLog = flags & 0x8U;
	switch ( displayMode ) {
		case 0: {
			if ( showLog ) {
				// Render Log(Temp)
				float	logTemp = 0.43429448190325182765112891891661 * log( max( 1e-6, V11.x ) );	// Log10( temp )
				float	normalizedLogHeat = (6.0 + logTemp) / 6.0;
				color = _texFalseColors.SampleLevel( LinearClamp, float2( normalizedLogHeat, 0.5 ), 0.0 );
			} else {
				// Render plain temperature
				color = _texFalseColors.SampleLevel( LinearClamp, float2( V11.x, 0.5 ), 0.0 );
			}
			break;
		}

		case 1: {
			// Render laplacian
			float	laplacian = V00.x + V10.x + V20.x 
							  + V01.x         + V21.x 
							  + V02.x + V12.x + V22.x
							  - 8 * V11.x;
					laplacian /= 8.0;

			if ( showLog )
				color = laplacian < 0.0 ? float3( 0, 0, 1+log(-laplacian)/12 ) : float3( 1+log(laplacian)/12, 0, 0 );
			else
				color = 1000.0 * (laplacian < 0.0 ? float3( 0, 0, -laplacian ) : float3( laplacian, 0, 0 ));
			break;
		}

		case 2: {
			// Render Voronoi cell index
			uint	sourceBit = asuint( V11.y );
			uint	cellIndex = sourceBit != 0  ? 1+((sourceBit & 1) ? 0 : ((sourceBit & 2) ? 1 : ((sourceBit & 4) ? 2 : ((sourceBit & 8) ? 3 : ((sourceBit & 16) ? 4 : ((sourceBit & 32) ? 5 : 6))))))
												: 0;
			color = cellIndex == 0 ? 0.0 : s_cellColors[(cellIndex-1) & 0x7];
			break;
		}

		case 3: {
			// Render secondary Voronoi cell indices
			uint	sourceBit = asuint( V11.z );
			uint	cellIndex = sourceBit != 0  ? 1+((sourceBit & 1) ? 0 : ((sourceBit & 2) ? 1 : ((sourceBit & 4) ? 2 : ((sourceBit & 8) ? 3 : ((sourceBit & 16) ? 4 : ((sourceBit & 32) ? 5 : 6))))))
												: 0;
			color = cellIndex == 0 ? 0.0 : s_cellColors[(cellIndex-1) & 0x7];
			break;
		}
	}

	// Show search path as an overlay
	if ( flags & 1 ) {
		float	search = _texSearch.SampleLevel( PointClamp, UV, 0.0 ).x;
		if ( search > 0.0 )
			color = float3( 1, 0, 1 );
	}

	return color;
}
