#include "Global.hlsl"

Texture2D< float4 >	_texHeatMap : register(t0);
Texture2D< float4 >	_texObstacles : register(t1);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

void	Compare( float4 _value, float2 _centralID, inout float4 _largest ) {
	if ( _value.x > _largest.x && _value.y != _centralID.x )
		_largest.xy = _value.xy;
	if ( _value.z > _largest.z && _value.w != _centralID.y )
		_largest.zw = _value.zw;
}

float4	PS( VS_IN _In ) : SV_TARGET0 {
	uint2	P = _In.__Position.xy - 0.5;
	uint2	Po = P + 1;	// Obstacles have a border of 1 pixel

	float4	obstacles = _texObstacles[Po];
	if ( obstacles.x )
		return 0.0;	// Don't compute anything for obstacles
	if ( obstacles.y || obstacles.z ) {
		return float4( 1.0, obstacles.w * 255, 0, 0 );	// We're a source
	}

	float4	sourceHeat = _texHeatMap[P];

	///////////////////////////////////////////////////////////////////
	// Fetch neighbor values
	uint3	O0 = uint3( !_texObstacles[uint2( Po.x-1, Po.y-1 )].x, !_texObstacles[uint2( Po.x+0, Po.y-1 )].x, !_texObstacles[uint2( Po.x+1, Po.y-1 )].x );
	uint3	O1 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+0 )].x, !obstacles.x,							  !_texObstacles[uint2( Po.x+1, Po.y+0 )].x );
	uint3	O2 = uint3( !_texObstacles[uint2( Po.x-1, Po.y+1 )].x, !_texObstacles[uint2( Po.x+0, Po.y+1 )].x, !_texObstacles[uint2( Po.x+1, Po.y+1 )].x );

	float4	V[3*3];
	V[3*0+0] = O0.x * _texHeatMap[uint2( P.x-1, P.y-1 )];
	V[3*0+1] = O0.y * _texHeatMap[uint2( P.x+0, P.y-1 )];
	V[3*0+2] = O0.z * _texHeatMap[uint2( P.x+1, P.y-1 )];

	V[3*1+0] = O1.x * _texHeatMap[uint2( P.x-1, P.y+0 )];
//	V[3*1+1] = sourceHeat
	V[3*1+2] = O1.z * _texHeatMap[uint2( P.x+1, P.y+0 )];

	V[3*2+0] = O2.x * _texHeatMap[uint2( P.x-1, P.y+1 )];
	V[3*2+1] = O2.y * _texHeatMap[uint2( P.x+0, P.y+1 )];
	V[3*2+2] = O2.z * _texHeatMap[uint2( P.x+1, P.y+1 )];

	uint	neighborsCount = dot( O0, 1 ) + dot( O1.xz, 1 ) + dot( O2, 1 );


	///////////////////////////////////////////////////////////////////
	// Find largest neighbors for both fields
	float4	largestNeighborSource = 0;	// We must start with empty ID values
	float2	centralID = sourceHeat.yy;
	Compare( V[3*0+0], centralID, largestNeighborSource );
	Compare( V[3*0+1], centralID, largestNeighborSource );
	Compare( V[3*0+2], centralID, largestNeighborSource );

	Compare( V[3*1+0], centralID, largestNeighborSource );
//	Compare( V[3*1+1], centralID, largestNeighborSource );
	Compare( V[3*1+2], centralID, largestNeighborSource );

	Compare( V[3*2+0], centralID, largestNeighborSource );
	Compare( V[3*2+1], centralID, largestNeighborSource );
	Compare( V[3*2+2], centralID, largestNeighborSource );


	///////////////////////////////////////////////////////////////////
	// Compute heat laplacians for both fields
	float2	laplacian = V[3*0+0].xz + V[3*0+1].xz + V[3*0+2].xz
					  + V[3*1+0].xz				  + V[3*1+2].xz
					  + V[3*2+0].xz + V[3*2+1].xz + V[3*2+2].xz
					  - neighborsCount * sourceHeat.xz;

	// Normalize
	laplacian *= neighborsCount > 0 ? 1.0 / neighborsCount : 0;


	///////////////////////////////////////////////////////////////////
	// Apply diffusion
#if 0
	float4	newHeat = sourceHeat;
	newHeat.xz = sourceHeat.xz + diffusionCoefficient * laplacian.xy;	// Regular radiation into field #0 & field #1
	newHeat.yw = largestNeighborSource.yw;

	if ( largestNeighborSource.x > sourceHeat.x && largestNeighborSource.y != sourceHeat.y && largestNeighborSource.y * sourceHeat.y > 0.0 ) {
//	if ( laplacian.x < 1e-3 && largestNeighborSource.y != sourceHeat.y && largestNeighborSource.y * sourceHeat.y > 0.0 ) {
		// Heat from neighbors is becoming preponderant and neighbors don't have the same source ID
		// This means we're at a Voronoi cell boundary (i.e. on the medial axis)
		// We must start to radiate into field #1 since we can't go any further into field 0
		// (i.e. the medial axis becomes a heat source for field #1)
		//
		newHeat.z = sourceHeat.x;				// Formerly field 0 is now radiating into field 1
		newHeat.w = largestNeighborSource.y;	// Source 0 is now becoming a lesser heat source in field 1
	}
#elif 1
	float4	newHeat = sourceHeat;
//	if ( largestNeighborSource.x > sourceHeat.x && largestNeighborSource.y != sourceHeat.y && largestNeighborSource.y * sourceHeat.y > 0.0 ) {
	if ( laplacian.x < 1e-3 && largestNeighborSource.y != sourceHeat.y && largestNeighborSource.y * sourceHeat.y > 0.0 ) {
		// Heat from neighbors is becoming preponderant and neighbors don't have the same source ID
		// This means we're at a Voronoi cell boundary (i.e. on the medial axis)
		// We must start to radiate into field #1 since we can't go any further into field 0
		// (i.e. the medial axis becomes a heat source for field #1)
		//
//		newHeat.x = sourceHeat.x + diffusionCoefficient * laplacian.x;	// Regular radiation into field #0
//		newHeat.y = largestNeighborSource.y;										// Except now source #1 is preponderant

		// Formerly field 0 is now radiating into field 1
		newHeat.z = sourceHeat.x;	// Keep strength
//		newHeat.z = 1.0;			// Renew strength
//		newHeat.z = 1e12 / sourceHeat.x;		// Stronger if farther
		newHeat.w = largestNeighborSource.y;													// Source 0 is now becoming a lesser heat source in field 1

	} else {
		// Heat from field 0 is still preponderant, just propagate both fields
//		newHeat.x = sourceHeat.x + diffusionCoefficient * laplacian.x;
//		newHeat.y = largestNeighborSource.y;
		newHeat.z = sourceHeat.z + diffusionCoefficient * laplacian.y;

		#if 1
			// This keeps the original IDs
			newHeat.w = sourceHeat.w == 0 ? largestNeighborSource.w : sourceHeat.w;
		#else
			// This propagates the strongest IDs but it can lead to difficult moving fronts and hard to control "run length" for the algorithm
			newHeat.w = largestNeighborSource.w;
		#endif
	}
#else
	float4	newHeat = 0.0;
	if ( largestNeighborSource.x > sourceHeat.x && largestNeighborSource.y != sourceHeat.y && largestNeighborSource.y * sourceHeat.y > 0.0 ) {
//	if ( laplacian.x > 0.0 && largestNeighborSource.y != sourceHeat.y && largestNeighborSource.y * sourceHeat.y > 0.0 ) {
		// Heat from neighbors is becoming preponderant and neighbors don't have the same source ID
		// This means we're at a Voronoi cell boundary (i.e. on the medial axis)
		// We must start to radiate into field #1 since we can't go any further into field 0
		// (i.e. the medial axis becomes a heat source for field #1)
		//
		newHeat.x = sourceHeat.x + diffusionCoefficient * laplacian.x;	// Regular radiation into field #0
		newHeat.y = largestNeighborSource.y;										// Except now source #1 is preponderant
		newHeat.z = sourceHeat.x;													// Formerly field 0 is now radiating into field 1
		newHeat.w = sourceHeat.y;													// Source 0 is now becoming a lesser heat source in field 1
	} else {
		// Heat from field 0 is still preponderant, just propagate both fields
		newHeat.x = sourceHeat.x + diffusionCoefficient * laplacian.x;
		newHeat.y = largestNeighborSource.y;
		newHeat.z = sourceHeat.z + diffusionCoefficient * laplacian.y;
		newHeat.w = sourceHeat.w;
	}
#endif

	return newHeat;
}
