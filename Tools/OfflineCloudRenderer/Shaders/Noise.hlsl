////////////////////////////////////////////////////////////////////////////////
// Noise Computation routines
////////////////////////////////////////////////////////////////////////////////
#ifndef __NOISE
#define __NOISE

////////////////////////////////////////////////////////////////////////////////////////
// Fast analytical Perlin noise
float	Hash( float n )
{
	return frac( sin(n) * 43758.5453 );
}

float	FastNoise( float3 x )
{
	float3	p = floor(x);
	float3	f = frac(x);

	f = smoothstep( 0.0, 1.0, f );

	float	n = p.x + 57.0 * p.y + 113.0 * p.z;

	return lerp(	lerp(	lerp( Hash( n +   0.0 ), Hash( n +   1.0 ), f.x ),
							lerp( Hash( n +  57.0 ), Hash( n +  58.0 ), f.x ), f.y ),
					lerp(	lerp( Hash( n + 113.0 ), Hash( n + 114.0 ), f.x ),
							lerp( Hash( n + 170.0 ), Hash( n + 171.0 ), f.x ), f.y ), f.z );
}

// Fast analytical noise for screen-space perturbation
float	FastScreenNoise( float2 _XY )
{
	return Hash(  1.5798490 * _XY.x - 2.60165409 * _XY.y )
		 * Hash( -1.3468489 * _XY.y + 2.31765563 * _XY.x );
}

float	FBM( float3 _Position, float3x3 _Rotation, uint _OctavesCount )
{
	float2	N = 0.0;
	float	A = 1.0;
	for ( uint OctaveIndex=0; OctaveIndex < _OctavesCount; OctaveIndex++ )
	{
		N += A * float2( 2.0 * FastNoise( _Position ) - 1.0, 1.0 );
		A *= 0.5;
		_Position = mul( _Position, _Rotation );
	}

	return N.x / N.y;
}

float	Turbulence( float3 _Position, float3x3 _Rotation, uint _OctavesCount )
{
	float2	N = 0.0;
	float	A = 1.0;
	for ( uint OctaveIndex=0; OctaveIndex < _OctavesCount; OctaveIndex++ )
	{
		N += A * float2( abs( 2.0 * FastNoise( _Position ) - 1.0 ), 1.0 );
		A *= 0.5;
		_Position = mul( _Position, _Rotation );
	}

	return N.x / N.y;
}


////////////////////////////////////////////////////////////////////////////////////////
// Simple cellular noise with a single point per grid cell

// Generate a [0,1[ location within a grid cell given the integer cell index
float3	GenerateRandomLocation( float3 _GridCellIndex )
{
	return float3(
		Hash( 0.894205 + _GridCellIndex.x ),
		Hash( 0.136515 + _GridCellIndex.y ),
		Hash( 0.654318 + _GridCellIndex.z )
		);
}

float4	Cellular( float3 _Position, float3 _InvGridCellSize )
{
	_Position *= _InvGridCellSize;

	float3	nPosition = _Position - frac( _Position );

// 	float3	Positions[3*3*3] = {
// 		GenerateRandomLocation( nPosition + float3( -1, -1, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0, -1, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1, -1, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3( -1,  0, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0,  0, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1,  0, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3( -1,  1, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0,  1, -1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1,  1, -1 ) ),
// 
// 		GenerateRandomLocation( nPosition + float3( -1, -1,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0, -1,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1, -1,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3( -1,  0,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0,  0,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1,  0,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3( -1,  1,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0,  1,  0 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1,  1,  0 ) ),
// 
// 		GenerateRandomLocation( nPosition + float3( -1, -1,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0, -1,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1, -1,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3( -1,  0,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0,  0,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1,  0,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3( -1,  1,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  0,  1,  1 ) ),
// 		GenerateRandomLocation( nPosition + float3(  1,  1,  1 ) ),
// 	};

	float4	ClosestPosition = float4( 0, 0, 0, INFINITY );
	for ( int z=-1; z <= 1; z++ )
		for ( int y=-1; y <= 1; y++ )
			for ( int x=-1; x <= 1; x++ )
			{
				float3	Location = GenerateRandomLocation( nPosition + float3( x, y, z ) );
				float3	Delta = _Position - Location;
				float	SqDistance = dot( Delta, Delta );
				ClosestPosition = lerp( ClosestPosition, float4( Location, SqDistance ), step( SqDistance, ClosestPosition.w ) );
			}

	ClosestPosition.w = sqrt( ClosestPosition.w );
	return ClosestPosition;
}

#endif