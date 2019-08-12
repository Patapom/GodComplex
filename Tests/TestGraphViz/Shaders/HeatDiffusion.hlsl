#include "Global.hlsl"

Texture2D< float4 >	_texHeatMap : register(t0);
Texture2D< float4 >	_texObstacles : register(t1);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

float4	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	P = _In.__Position.xy - 0.5;
	uint2	Po = P + 1;	// Obstacles have a border of 1 pixel

	float4	obstacles = _texObstacles[Po];
	if ( obstacles.x )
		return 0.0;	// Don't compute anything for obstacles

	uint	obstacleSourceIndex = uint(obstacles.z * 255) - 1;
	if ( obstacles.y || obstacleSourceIndex == sourceIndex ) {
		return float4( 1, 0, 0, 0 );	// We're a source, set temperature
	}

	///////////////////////////////////////////////////////////////////
	// Compute heat laplacian
	uint3	O0 = uint3( !_texObstacles[uint2( Po.x-1, Po.y-1 )].x, !_texObstacles[uint2( Po.x+0, Po.y-1 )].x, !_texObstacles[uint2( Po.x+1, Po.y-1 )].x );
	uint3	O1 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+0 )].x, !obstacles.x,							  !_texObstacles[uint2( Po.x+1, Po.y+0 )].x );
	uint3	O2 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+1 )].x, !_texObstacles[uint2( Po.x+0, Po.y+1 )].x, !_texObstacles[uint2( Po.x+1, Po.y+1 )].x );
	uint	neighborsCount = dot( O0, 1 ) + dot( O1.xz, 1 ) + dot( O2, 1 );

	float4	sourceHeat = _texHeatMap[P];

	float4	V[3*3];
	V[3*0+0] = O0.x * _texHeatMap[uint2( P.x-1, P.y-1 )];
	V[3*0+1] = O0.y * _texHeatMap[uint2( P.x+0, P.y-1 )];
	V[3*0+2] = O0.z * _texHeatMap[uint2( P.x+1, P.y-1 )];

	V[3*1+0] = O1.x * _texHeatMap[uint2( P.x-1, P.y+0 )];
//	V[3*1+1] = O1.y * sourceHeat
	V[3*1+2] = O1.z * _texHeatMap[uint2( P.x+1, P.y+0 )];

	V[3*2+0] = O2.x * _texHeatMap[uint2( P.x-1, P.y+1 )];
	V[3*2+1] = O2.y * _texHeatMap[uint2( P.x+0, P.y+1 )];
	V[3*2+2] = O2.z * _texHeatMap[uint2( P.x+1, P.y+1 )];

	float	laplacian = V[3*0+0].x + V[3*0+1].x + V[3*0+2].x
					  + V[3*1+0].x			    + V[3*1+2].x
					  + V[3*2+0].x + V[3*2+1].x + V[3*2+2].x
					  - neighborsCount * sourceHeat.x;

	// Normalize
	laplacian *= neighborsCount > 0 ? 1.0 / neighborsCount : 0;

//	sourceHeat.y = max( sourceHeat.y, V[3*0+0].y );
//	sourceHeat.y = max( sourceHeat.y, V[3*0+1].y );
//	sourceHeat.y = max( sourceHeat.y, V[3*0+2].y );
//	sourceHeat.y = max( sourceHeat.y, V[3*1+0].y );
////	sourceHeat.y = max( sourceHeat.y, V[3*1+1].y );
//	sourceHeat.y = max( sourceHeat.y, V[3*1+2].y );
//	sourceHeat.y = max( sourceHeat.y, V[3*2+0].y );
//	sourceHeat.y = max( sourceHeat.y, V[3*2+1].y );
//	sourceHeat.y = max( sourceHeat.y, V[3*2+2].y );

	///////////////////////////////////////////////////////////////////
	// Apply diffusion
	sourceHeat.x += diffusionCoefficient * laplacian;

	return sourceHeat;
}
