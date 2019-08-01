#include "Global.hlsl"

Texture2DArray< float4 >	_texHeatMap : register(t0);
Texture2D< float4 >			_texObstacles : register(t1);
Texture2D< float3 >			_texFalseColors : register(t2);
Texture2D< float3 >			_texSearch : register(t3);

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

//	float4	V00 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( -1, -1 ) );
//	float4	V10 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2(  0, -1 ) );
//	float4	V20 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( +1, -1 ) );
//	float4	V01 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( -1,  0 ) );
//	float4	V11 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2(  0,  0 ) );
//	float4	V21 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( +1,  0 ) );
//	float4	V02 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( -1, +1 ) );
//	float4	V12 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2(  0, +1 ) );
//	float4	V22 = _texHeatMap.SampleLevel( PointClamp, UV, 0.0, int2( +1, +1 ) );

	float4	V11 = _texHeatMap.SampleLevel( PointClamp, float3( UV, sourceIndex ), 0.0, int2(  0,  0 ) );

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
			// Render normalized heat map
			color = _texFalseColors.SampleLevel( LinearClamp, float2( V11.y, 0.5 ), 0.0 );
			break;
		}

		case 2: {
			// Render results
			float	sqDistance = 0.0;
			float	barycentricCenter = 1.0 / sourcesCount;	// The ideal center is a vector with all components equal to this value
			bool	isSource = false;
			for ( uint i=0; i < sourcesCount; i++ ) {
				float4	heat = _texHeatMap.SampleLevel( PointClamp, float3( UV, i ), 0.0 );
				if ( abs( heat.x - 1.0 ) < 1e-3 )
					isSource = true;
				float	barycentric = heat.y;
				float	delta = barycentric - barycentricCenter;
				sqDistance += delta * delta;
			}

			float	distance = sqrt( sqDistance ) - resultsConfinementDistance;
//					distance /= resultsConfinementDistance;

			color = distance < 0.0 ? float3( -distance / resultsConfinementDistance, 0, 0 ) : float3( 0, 0, distance );

			if ( isSource )
				color = 1.0;	// Mark sources

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
