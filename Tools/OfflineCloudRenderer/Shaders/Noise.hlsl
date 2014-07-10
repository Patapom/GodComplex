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
	return 	Hash(  13.5798490 * _XY.x - 23.60165409 * _XY.y )
		  * Hash( -17.3468489 * _XY.y + 27.31765563 * _XY.x );
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

// Generate a location within a grid cell given the integer cell index
float3	GenerateRandomLocation( float3 _GridCellIndex )
{
	return _GridCellIndex
		 + float3(	Hash( 0.894205 + 17.219 * _GridCellIndex.x - 19.1965 * _GridCellIndex.y + 7.0689 * _GridCellIndex.z ),
					Hash( 0.136515 - 23.198 * _GridCellIndex.x + 7.95145 * _GridCellIndex.y + 12.123 * _GridCellIndex.z ),
					Hash( 0.654318 + 19.161 * _GridCellIndex.y - 15.6317 * _GridCellIndex.y - 51.561 * _GridCellIndex.z )
				 );
}

float	Cellular( float3 _Position, float3 _InvGridCellSize )
{
	_Position = _InvGridCellSize * (_Position + 137.56);

	float3	nPosition = floor( _Position );

	float	ClosestDistance = INFINITY;
	for ( int z=-1; z <= 1; z++ )
		for ( int y=-1; y <= 1; y++ )
			for ( int x=-1; x <= 1; x++ )
			{
				float3	nCellIndex = nPosition + float3( x, y, z );
				float3	Location = GenerateRandomLocation( nCellIndex );	// Interest point within cell
				float3	Delta = _Position - Location;
				float	SqDistance = dot( Delta, Delta );
				ClosestDistance = lerp( ClosestDistance, SqDistance, step( SqDistance, ClosestDistance ) );
			}

	return sqrt( ClosestDistance );
}

#endif