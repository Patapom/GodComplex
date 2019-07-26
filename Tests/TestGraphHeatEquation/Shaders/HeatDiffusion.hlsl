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
	if ( obstacles.y || obstacles.z )
		return 10.0;	// We're a source

	///////////////////////////////////////////////////////////////////
	// Compute heat laplacian
	float4	laplacian = 0.0;
	uint	neighborsCount = 0;

	uint3	O0 = uint3( !_texObstacles[uint2( Po.x-1, Po.y-1 )].x, !_texObstacles[uint2( Po.x+0, Po.y-1 )].x, !_texObstacles[uint2( Po.x+1, Po.y-1 )].x );
	uint3	O1 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+0 )].x, !obstacles.x,							  !_texObstacles[uint2( Po.x+1, Po.y+0 )].x );
	uint3	O2 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+1 )].x, !_texObstacles[uint2( Po.x+0, Po.y+1 )].x, !_texObstacles[uint2( Po.x+1, Po.y+1 )].x );

	// Check easy cardinal directions
	if ( O1.x ) {
		laplacian += _texHeatMap[uint2( P.x-1, P.y+0 )];
		neighborsCount++;
	}
	if ( O1.z ) {
		laplacian += _texHeatMap[uint2( P.x+1, P.y+0 )];
		neighborsCount++;
	}
	if ( O0.y ) {
		laplacian += _texHeatMap[uint2( P.x+0, P.y-1 )];
		neighborsCount++;
	}
	if ( O2.y) {
		laplacian += _texHeatMap[uint2( P.x+0, P.y+1 )];
		neighborsCount++;
	}

	// Check diagonal directions with extra care that we don't cross a boundary
	if ( O0.x && (O0.y || O1.x) ) {
		laplacian += _texHeatMap[uint2( P.x-1, P.y-1 )];
		neighborsCount++;
	}
	if ( O0.z && (O0.y || O1.z) ) {
		laplacian += _texHeatMap[uint2( P.x+1, P.y-1 )];
		neighborsCount++;
	}
	if ( O2.z && (O1.z || O2.y) ) {
		laplacian += _texHeatMap[uint2( P.x+1, P.y+1 )];
		neighborsCount++;
	}
	if ( O2.x && (O1.x || O2.y) ) {
		laplacian += _texHeatMap[uint2( P.x-1, P.y+1 )];
		neighborsCount++;
	}

	float4	sourceHeat = _texHeatMap[uint2( P.x, P.y )];
	laplacian -= neighborsCount * sourceHeat;

	// Normalize?
	laplacian *= neighborsCount > 0 ? 1.0 / neighborsCount : 0;

	///////////////////////////////////////////////////////////////////
	// Apply diffusion
	return sourceHeat + deltaTime * diffusionCoefficient * laplacian;
}