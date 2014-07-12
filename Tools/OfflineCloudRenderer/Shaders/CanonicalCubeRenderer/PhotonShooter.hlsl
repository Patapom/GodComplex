////////////////////////////////////////////////////////////////////////////////
// Photon Shooter
////////////////////////////////////////////////////////////////////////////////
//
// The goal of this compute shader is to shoot an important amount of photons from the top side of a canonical cube
// The various parameters are:
//	_ Initial photon beam position on the surface (x,y)
//	_ Initial photon beam incidence on the surface (Phi, Theta)
//	_ Size of the cube side in meters
//	_ Phase function, a 1D texture depending solely on the scattering angle (we are provided with a cumulative distribution function
//						so drawing a uniform random number in [0,1] and accessing the table will return the scattering angle)
//	_ Medium density, the 
//
// We assume no absorption, only scattering since we're dealing with water droplets that reflect/refract 99.9% of the light
//
// We will store as a result:
//	_ The side of the cube the photon exited at
//	_ The position on that side where it exited
//	_ The angle at which it exited
//
#include "Global.hlsl"
#include "Noise.hlsl"

static const uint	MAX_THREADS = 1024;

static const float	PI = 3.1415926535897932384626433832795;

cbuffer	CBInput : register( b0 )
{
	float2	_InitialPosition;	// Initial beam position in [-1,+1] on the top side of the cube (Y=+1)
	float2	_InitialIncidence;	// Initial beam angular incidence (Phi,Theta)
	float	_CubeSize;			// Size of the canonical cube in meters
	uint	_MaxScattering;		// Maximum scattering events before exiting the cube (default is 30)
	float	_
}

Texture2D<float>			_PhaseCDF : register( t0 );

struct PhotonOut
{
	float3	ExitPosition;			// Exit position in [-1,+1]
	float3	ExitDirection;			// Exit direction
	float	MarchedLength;			// Length the photon had to march before exiting (in canonical [-1,+1] units, multiply by 0.5*CubeSize to get length in meters)
	uint	ScatteringEventsCount;	// Amount of scattering events before exiting
};

RWStructuredBuffer<PhotonOut>	_Photons : register( t1 );


// Performs photon scattering
// Changes the photon direction and returns the length of the path before next scattering
float3	Scatter( uint _PhotonIndex, uint _ScatteringEventIndex, float3 _OriginalDirection, out float _Length )
{
	float	Random = Hash( 0.371819 * (_MaxScattering * _PhotonIndex + _ScatteringEventIndex) );
}

float	ComputeCubeExitDistance( float3 _Position, float3 _Direction )
{
	float3	ExitDistancesNeg = (-1.0 - R.ExitPosition) / R.ExitDirection;
			ExitDistancesNeg = lerp( ExitDistancesNeg, INFINITY, step( ExitDistancesNeg, 0.0 ) );	// Brings all backward intersections to infinity
	float3	ExitDistancesPos = (1.0 - R.ExitPosition) / R.ExitDirection;
			ExitDistancesPos = lerp( ExitDistancesPos, INFINITY, step( ExitDistancesPos, 0.0 ) );	// Brings all backward intersections to infinity

	float3	MinDistances = min( ExitDistancesNeg, ExitDistancesPos );
	return min( min( MinDistances.x, MinDistances.y ), MinDistances.z );
}

PhotonOut	ShootPhoton( uint _PhotonIndex )
{
	// Build initial photon position and direction
	float2	SinCosPhi, SinCosTheta;
	sincos( _InitialIncidence.x, SinCosPhi.x, SinCosPhi.y );
	sincos( _InitialIncidence.y, SinCosTheta.x, SinCosTheta.y );

	PhotonOut	R;
	R.ExitPosition = float3( _InitialPosition.x, 1.0, _InitialPosition.y );
	R.ExitDirection = -float3( SinCosPhi.y * SinCosTheta.x, SinCosTheta.y, SinCosPhi.x * SinCosTheta.x );
	R.MarchedLength = 0.0;

	// Don't scatter initially, but draw a random march length
	float	MarchLength;
	Scatter( _PhotonIndex, -1, R.ExitDirection, MarchLength );

	for ( R.ScatteringEventsCount=0; R.ScatteringEventsCount < _MaxScattering; R.ScatteringEventsCount++ )
	{
		// Compute the intersection with the cube's sides and return if any
		float	ExitLength = ComputeCubeExitDistance( R.ExitPosition, R.ExitDirection );
		if ( ExitLength < MarchLength )
		{	// Photon is exiting!
			R.ExitPosition += ExitLength * R.ExitDirection;
			R.MarchedLength += ExitLength;
			return R;
		}

		// No intersection, proceed with marching and scattering
		R.MarchedLength += MarchLength;
		Scatter( _PhotonIndex, R.ScatteringEventsCount, R.ExitDirection, MarchLength );
	}

	// We've been scattered more than allowed!
	// Simply continue in the same direction until the photon exits a side of the cube
	MarchLength = ComputeCubeExitDistance( R.ExitPosition, R.ExitDirection );
	R.ExitPosition += MarchLength * R.ExitDirection;
	R.MarchedLength += MarchLength;

	return R;
}

[numthreads( MAX_THREADS, 1, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID )
{
	uint	PhotonIndex = MAX_THREADS * _GroupID.x + _GroupThreadID.x;
	_Photons[PhotonIndex] = ShootPhoton( PhotonIndex );
}