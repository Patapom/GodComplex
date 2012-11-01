////////////////////////////////////////////////////////////////////////////////////////
// RayTracing Helpers
// Many pieces of codes were stolen from:
//	http://madebyevan.com/webgl-path-tracing/webgl-path-tracing.js
//	http://http.developer.nvidia.com/GPUGems3/gpugems3_ch37.html
//
////////////////////////////////////////////////////////////////////////////////////////

static const float	INFINITY = 1e6;

////////////////////////////////////////////////////////////////////////////////////////
// Pseudo RNG

// From http://madebyevan.com/webgl-path-tracing/webgl-path-tracing.js
float	Random1( float3 _UVW, float3 _Scale, float _Seed )
{
	return frac( sin( dot( _UVW + _Seed, _Scale ) ) * 43758.5453 + _Seed );
}

float2	Random2( float3 _UVW, float _Seed )
{
	return float2( Random1( _UVW, float3( 12.9898, 78.233, 151.7182 ), _Seed ), Random1( _UVW, float3( 63.7264, 10.873, 623.6736 ), _Seed ) );
}

float3	Random3( float3 _UVW, float _Seed )
{
	return float3( Random2( _UVW, _Seed ), Random1( _UVW, float3( 51.123897, 76.16197810, 156.19895 ), _Seed ) );
}


// From http://gamedev.stackexchange.com/questions/32681/random-number-hlsl
float	rand_1_05( float2 uv )
{
    float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
    return abs(noise.x + noise.y) * 0.5;
}

float2	rand_2_10( float2 uv )
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}

float2	rand_2_0004( float2 uv )
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    return float2(noiseX, noiseY) * 0.004;
}


// Standard linear congruential generator from Knuth
uint	LCGStep( inout uint z, uint A, uint C )  
{  
  return z = (A*z+C);
}

// S1, S2, S3, and M are all constants, and z is part of the private per-thread generator state.  
uint	TausStep( inout uint z, int S1, int S2, int S3, uint M )
{  
	uint	b = ((z << S1) ^ z) >> S2;
	return z = ((z & M) << S3) ^ b;
}

// Values of the z vector must vary per-thread and be randomly chosen and > 128
// Combined period is lcm(p1,p2,p3,p4)~ 2^121  
float	Random1( inout uint4 z )
{  
	return 2.3283064365387e-10 * (					// Periods  
		TausStep( z.x, 13, 19, 12, 4294967294UL ) ^	// p1=2^31-1  
		TausStep( z.y, 2, 25, 4, 4294967288UL ) ^	// p2=2^30-1  
		TausStep( z.z, 3, 11, 17, 4294967280UL ) ^	// p3=2^28-1  
		LCGStep( z.w, 1664525, 1013904223UL ) );	// p4=2^32  
}

float2	Random2( inout uint4 _Seed )
{
	return float2( Random1( _Seed ), Random1( _Seed ) );
}

float3	Random3( inout uint4 _Seed )
{
	return float3( Random2( _Seed ), Random1( _Seed ) );
}


////////////////////////////////////////////////////////////////////////////////////////
// Position & Direction generators

// Generates a cosine-weighted sample on a hemisphere
// Stolen from http://www.rorydriscoll.com/2009/01/07/better-sampling/
//
float3	CosineSampleHemisphere( float2 _UV )
{
    float	Radius = sqrt(_UV.y);
    float	Phi = TWOPI * _UV.x;
 
	float2	SinCos;
	sincos( Phi, SinCos.x, SinCos.y );
 
    return float3( Radius * SinCos, sqrt( 1.0 - _UV.y ) );
}

// Generates a position on a rectangular patch using stratified sampling
// Returns a position in [-0.5*_Size,+0.5*_Size]
float2	GenerateRectanglePosition( uint _RayIndex, uint _RaysCount, inout uint4 _Seed, float2 _Size )
{
	float	AspectRatio = _Size.x / _Size.y;
	uint2	RaysCount;
			RaysCount.y = max( 1, uint( floor( AspectRatio * _RaysCount ) ) );
			RaysCount.x = _RaysCount / RaysCount.y;

	float2	StratifiedSize = _Size / RaysCount;	// Size of stratified patch

	// Stratified position
	uint2	iPos;
			iPos.y = _RayIndex / RaysCount.x;
			iPos.x = _RayIndex - RaysCount.x * iPos.y;

	float2	uv = Random2( _Seed );

	return (iPos + uv) * StratifiedSize - 0.5 * _Size;
}


////////////////////////////////////////////////////////////////////////////////////////
// Intersections
struct	Ray
{
	float3	P;
	float3	V;
};

struct	Intersection
{
	float	Distance;
//	float3	Position;
	float3	Normal;
	float3	Tangent;
	uint	MaterialID;
};

// Initializes the intersection structure
void	InitializeIntersection( inout Intersection _Intersection, float _Distance, float3 _Normal, float3 _Tangent, uint _MaterialID )
{
	_Intersection.Distance = _Distance;
	_Intersection.Normal = _Normal;
	_Intersection.Tangent = _Tangent;
	_Intersection.MaterialID = _MaterialID;
}

// Updates the intersection structure if the provided distance is closer than existing intersection
void	UpdateClosestIntersection( inout Intersection _Intersection, float _Distance, float3 _Normal, float3 _Tangent, uint _MaterialID )
{
	float	Closer = saturate( 10000.0 * (_Intersection.Distance - _Distance) );	// 0 if superior or equal, 1 if inferior
	_Intersection.Distance = lerp( _Intersection.Distance, _Distance, Closer );
	_Intersection.Normal = lerp( _Intersection.Normal, _Normal, Closer );
	_Intersection.Tangent = lerp( _Intersection.Tangent, _Tangent, Closer );
	_Intersection.MaterialID = lerp( _Intersection.MaterialID, _MaterialID, Closer );
}

// AABox intersection, knowing that we are INSIDE the box
//	_Position, position of the center of the box
//	_InvHalfSize, 0.5/size of the box
float	CalcAABoxIn( Ray _Ray, Intersection _Intersection, float3 _Position, float3 _InvHalfSize )
{
	// Transform ray in "box space"
	float3	P = (_Ray.P - _Position) * _InvHalfSize;
	float3	V = _Ray.V * _InvHalfSize;

	// Now we look for an intersection with a box of size [-1,+1]
	float3	InvV = 1.0 / V;
	float3	DeltaPos = (+1.0 - P) * InvV;	// Distances to positive +X +Y +Z faces
	float3	DeltaNeg = (-1.0 - P) * InvV;	// Distances to negative -X -Y -Z faces

	// Patch negative hits => Send back to infinity if hit is behind origin
	DeltaPos = lerp( DeltaPos, INFINITY, saturate( -10000.0 * DeltaPos ) );
	DeltaNeg = lerp( DeltaNeg, INFINITY, saturate( -10000.0 * DeltaNeg ) );

	// Isolate closest hit
	InitializeIntersection   ( _Intersection, DeltaPos.x, float3( -1, 0, 0 ), float3( 0, 0, -1 ), 0 );
	UpdateClosestIntersection( _Intersection, DeltaNeg.x, float3( +1, 0, 0 ), float3( 0, 0, +1 ), 1 );
	UpdateClosestIntersection( _Intersection, DeltaPos.y, float3( 0, -1, 0 ), float3( 0, 0, +1 ), 2 );
	UpdateClosestIntersection( _Intersection, DeltaNeg.y, float3( 0, +1, 0 ), float3( 0, 0, +1 ), 3 );
	UpdateClosestIntersection( _Intersection, DeltaPos.z, float3( 0, 0, -1 ), float3( +1, 0, 0 ), 4 );
	UpdateClosestIntersection( _Intersection, DeltaNeg.z, float3( 0, 0, +1 ), float3( -1, 0, 0 ), 5 );

	return _Intersection.Distance;
}

