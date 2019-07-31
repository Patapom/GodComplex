#include "Global.hlsl"

Texture2D< float4 >	_texHeatMap : register(t0);
Texture2D< float4 >	_texObstacles : register(t1);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

void	Compare( float4 _value, uint _centralBit, inout float4 _largest ) {
	uint	neighborBit = asuint(_value.y);
//	if ( _value.x > _largest.x && neighborBit != _centralBit )
	if ( neighborBit != _centralBit )
		_largest.xy = _value.xy;		// Only check for different neighbor is enough
	else if ( _value.x > _largest.z )
		_largest.zw = _value.xy;		// If neighbors have the same source, keep the largest
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	P = _In.__Position.xy - 0.5;
	uint2	Po = P + 1;	// Obstacles have a border of 1 pixel

	float4	obstacles = _texObstacles[Po];
	if ( obstacles.x )
		return 0.0;	// Don't compute anything for obstacles

//	if ( obstacles.y || obstacles.z ) {
//		return float4( 1.0, obstacles.w * 255, 0, 0 );	// We're a source
//	}

	///////////////////////////////////////////////////////////////////
	// Compute heat laplacian
	uint3	O0 = uint3( !_texObstacles[uint2( Po.x-1, Po.y-1 )].x, !_texObstacles[uint2( Po.x+0, Po.y-1 )].x, !_texObstacles[uint2( Po.x+1, Po.y-1 )].x );
	uint3	O1 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+0 )].x, !obstacles.x,							  !_texObstacles[uint2( Po.x+1, Po.y+0 )].x );
	uint3	O2 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+1 )].x, !_texObstacles[uint2( Po.x+0, Po.y+1 )].x, !_texObstacles[uint2( Po.x+1, Po.y+1 )].x );
	uint	neighborsCount = dot( O0, 1 ) + dot( O1.xz, 1 ) + dot( O2, 1 );

	float4	sourceHeat = _texHeatMap[P];

	float4	V[3*3];
	V[3*0+0] = _texHeatMap[uint2( P.x-1, P.y-1 )];
	V[3*0+1] = _texHeatMap[uint2( P.x+0, P.y-1 )];
	V[3*0+2] = _texHeatMap[uint2( P.x+1, P.y-1 )];

	V[3*1+0] = _texHeatMap[uint2( P.x-1, P.y+0 )];
//	V[3*1+1] = sourceHeat
	V[3*1+2] = _texHeatMap[uint2( P.x+1, P.y+0 )];

	V[3*2+0] = _texHeatMap[uint2( P.x-1, P.y+1 )];
	V[3*2+1] = _texHeatMap[uint2( P.x+0, P.y+1 )];
	V[3*2+2] = _texHeatMap[uint2( P.x+1, P.y+1 )];

	V[3*0+0].x *= O0.x;
	V[3*0+1].x *= O0.y;
	V[3*0+2].x *= O0.z;

	V[3*1+0].x *= O1.x;
//	V[3*1+1].x *= O1.y;
	V[3*1+2].x *= O1.z;

	V[3*2+0].x *= O2.x;
	V[3*2+1].x *= O2.y;
	V[3*2+2].x *= O2.z;

	float	laplacian = V[3*0+0].x + V[3*0+1].x + V[3*0+2].x
					  + V[3*1+0].x			    + V[3*1+2].x
					  + V[3*2+0].x + V[3*2+1].x + V[3*2+2].x
					  - neighborsCount * sourceHeat.x;

	// Normalize
	laplacian *= neighborsCount > 0 ? 1.0 / neighborsCount : 0;


	///////////////////////////////////////////////////////////////////
	// Find largest neighbors for both fields
	float4	largestNeighborSource = float4( sourceHeat.xy, 0, 0 );
	uint	centralBit = asuint(sourceHeat.y);
	Compare( V[3*0+0], centralBit, largestNeighborSource );
	Compare( V[3*0+1], centralBit, largestNeighborSource );
	Compare( V[3*0+2], centralBit, largestNeighborSource );

	Compare( V[3*1+0], centralBit, largestNeighborSource );
//	Compare( V[3*1+1], centralBit, largestNeighborSource );
	Compare( V[3*1+2], centralBit, largestNeighborSource );

	Compare( V[3*2+0], centralBit, largestNeighborSource );
	Compare( V[3*2+1], centralBit, largestNeighborSource );
	Compare( V[3*2+2], centralBit, largestNeighborSource );


	///////////////////////////////////////////////////////////////////
	// Assign bitfields at Voronoi cell boundary (null laplacian) and at saddle points (center of cell boundary edge)
	float4	newHeat = sourceHeat;
//	if ( laplacian.x < 1e-6 ) {
//	if ( largestNeighborSource.y != sourceHeat.y && largestNeighborSource.y != 0 ) {
	if ( asuint(largestNeighborSource.y) != asuint(sourceHeat.y) ) {

		// Check if none of our neighbors is higher than us, this means we're at the inflection point of the heat front
		if ( largestNeighborSource.z < sourceHeat.x )
			newHeat.z = largestNeighborSource.y;
	}

	return newHeat;
}
