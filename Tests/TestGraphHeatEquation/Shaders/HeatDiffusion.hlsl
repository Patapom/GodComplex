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
	if ( obstacles.y )
		return 10.0;	// We're a source

	///////////////////////////////////////////////////////////////////
	// Compute heat laplacian
	float4	laplacian = 0.0;
	uint	neighborsCount = 0;

	if ( _texObstacles[uint2( Po.x-1, Po.y-1 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x-1, P.y-1 )];
		neighborsCount++;
	}
	if ( _texObstacles[uint2( Po.x+0, Po.y-1 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x+0, P.y-1 )];
		neighborsCount++;
	}
	if ( _texObstacles[uint2( Po.x+1, Po.y-1 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x+1, P.y-1 )];
		neighborsCount++;
	}
	if ( _texObstacles[uint2( Po.x+1, Po.y+0 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x+1, P.y+0 )];
		neighborsCount++;
	}
	if ( _texObstacles[uint2( Po.x+1, Po.y+1 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x+1, P.y+1 )];
		neighborsCount++;
	}
	if ( _texObstacles[uint2( Po.x+0, Po.y+1 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x+0, P.y+1 )];
		neighborsCount++;
	}
	if ( _texObstacles[uint2( Po.x-1, Po.y+1 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x-1, P.y+1 )];
		neighborsCount++;
	}
	if ( _texObstacles[uint2( Po.x-1, Po.y+0 )].x == 0 ) {
		laplacian += _texHeatMap[uint2( P.x-1, P.y+0 )];
		neighborsCount++;
	}

	float4	sourceHeat = _texHeatMap[uint2( P.x, P.y )];
	laplacian -= neighborsCount * sourceHeat;

	///////////////////////////////////////////////////////////////////
	// Apply diffusion
	return sourceHeat + deltaTime * diffusionCoefficient * laplacian;
}
