#include "Global.hlsl"

Texture2D< float4 >	_texHeatMap : register(t0);
Texture2D< float4 >	_texObstacles : register(t1);

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }

void	UpdateGradient( float2 _centralValue, float4 _neighborValue, inout float2 _largestGradient ) {
	uint	centralBit = asuint(_centralValue.y);
	uint	neighborBit = asuint(_neighborValue.y);
	if ( neighborBit == centralBit ) {
		// Same cell, gradient computation is possible
		float	gradient = _centralValue.x - _neighborValue.x;
		if ( gradient > _largestGradient.x ) {
			_largestGradient.x = gradient;
			_largestGradient.y = _neighborValue.z;	// Use neighbor's bitfield
		}
	}
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
	// Read neighbors
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

	///////////////////////////////////////////////////////////////////
	// Propagate neighbor bitfields by following the largest gradient
	float2	largestGradient = 0;
	UpdateGradient( sourceHeat.xy, V[3*0+0], largestGradient );
	UpdateGradient( sourceHeat.xy, V[3*0+1], largestGradient );
	UpdateGradient( sourceHeat.xy, V[3*0+2], largestGradient );

	UpdateGradient( sourceHeat.xy, V[3*1+0], largestGradient );
//	UpdateGradient( sourceHeat.xy, V[3*1+1], largestGradient );
	UpdateGradient( sourceHeat.xy, V[3*1+2], largestGradient );

	UpdateGradient( sourceHeat.xy, V[3*2+0], largestGradient );
	UpdateGradient( sourceHeat.xy, V[3*2+1], largestGradient );
	UpdateGradient( sourceHeat.xy, V[3*2+2], largestGradient );

	uint	centralBitField = asuint( sourceHeat.z );
	uint	neighborBitField = asuint( largestGradient.y );
			centralBitField |= neighborBitField;

	sourceHeat.z = asfloat( centralBitField );

	return sourceHeat;
}
