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
	z = ((z & M) << S3) ^ b;

	return z;
}

// Values of the z vector must vary per-thread and be randomly chosen and > 128
// Combined period is lcm(p1,p2,p3,p4)~ 2^121  
float	Random1( inout uint4 z )
{
	return 2.3283064365387e-10 * (					// Periods  
		TausStep( z.x, 13, 19, 12, 4294967294L ) ^	// p1=2^31-1  
		TausStep( z.y, 2, 25, 4, 4294967288L ) ^	// p2=2^30-1  
		TausStep( z.z, 3, 11, 17, 4294967280L ) ^	// p3=2^28-1  
		LCGStep( z.w, 1664525, 1013904223L ) );	// p4=2^32  
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
    float	Radius = sqrt( _UV.y );
    float	Phi = TWOPI * _UV.x;
 
	float2	SinCos;
	sincos( Phi, SinCos.x, SinCos.y );
 
    return float3( Radius * SinCos, sqrt( 1.0 - _UV.y ) );
}

// Same as above but stratified
float3	CosineSampleHemisphere( float2 _UV, const uint _RayIndex, const uint _RaysCount )
{
    float	Radius = sqrt( _UV.y );
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
	float3	Position;
	float3	Normal;
	float3	Tangent;
	float3	BiTangent;
	float2	UV;
	uint	MaterialID;
};

// Updates the intersection structure if the provided distance is closer than existing intersection
void	UpdateClosestIntersection( Ray _Ray, inout Intersection _Intersection, float _Distance, const float2 _InvHalfSize, const float3 _Center, const float3 _Normal, const float3 _Tangent, const float3 _BiTangent, const uint _MaterialID )
{
	float	Closer = saturate( 10000.0 * (_Intersection.Distance - _Distance) );	// 0 if superior or equal, 1 if inferior

	_Intersection.Distance = lerp( _Intersection.Distance, _Distance, Closer );
	_Intersection.Position = lerp( _Intersection.Position, _Ray.P + _Distance * _Ray.V, Closer );
	_Intersection.Normal = lerp( _Intersection.Normal, _Normal, Closer );
	_Intersection.Tangent = lerp( _Intersection.Tangent, _Tangent, Closer );
	_Intersection.BiTangent = lerp( _Intersection.BiTangent, _BiTangent, Closer );
	_Intersection.MaterialID = lerp( _Intersection.MaterialID, _MaterialID, Closer );

	float3	DPos = _Intersection.Position - _Center;
	float2	UV = 0.5 * (1.0 + float2( dot( DPos, _Tangent ), -dot( DPos, _BiTangent ) ) * _InvHalfSize);
	_Intersection.UV = lerp( _Intersection.UV, UV, Closer );
}

// Clamps to ]-infinity,-_Tolerance] and [+_Tolerance,+infinity[
float3	EnsureNotZeroFlubb( float3 _Value, float _Tolerance )
{
	return 2.0 * _Value - max( _Value, _Tolerance ) - min( _Value, -_Tolerance ) + _Tolerance * (sign( _Value ) + 1.0 - abs( sign( _Value ) ));
}

// AABox intersection, knowing that we are INSIDE the box
//	_Position, position of the center of the box
//	_InvHalfSize, 0.5/size of the box
//
float	IntersectAABoxIn( Ray _Ray, inout Intersection _Intersection, const float3 _Position, const float3 _InvHalfSize, const uint _BaseMaterialID )
{
	// Transform ray in "box space"
	float3	P = (_Ray.P - _Position) * _InvHalfSize;
	float3	V = _Ray.V * _InvHalfSize;

	// Now we look for an intersection with a box of size [-1,+1]
	float3	InvV = 1.0 / EnsureNotZeroFlubb( V, 1e-6 );
	float3	DeltaPos = (+1.0 - P) * InvV;	// Distances to positive +X +Y +Z faces
	float3	DeltaNeg = (-1.0 - P) * InvV;	// Distances to negative -X -Y -Z faces

	// Patch negative hits => Send back to infinity if hit is behind origin
	DeltaPos = lerp( DeltaPos, INFINITY, saturate( -10000.0 * DeltaPos ) );
	DeltaNeg = lerp( DeltaNeg, INFINITY, saturate( -10000.0 * DeltaNeg ) );

	// Isolate closest hit
	const float3	HalfSize = 1.0 / _InvHalfSize;
	UpdateClosestIntersection( _Ray, _Intersection, DeltaPos.y, float2( _InvHalfSize.x, _InvHalfSize.z ), _Position + float3( 0, +HalfSize.y, 0 ), float3( 0, -1, 0 ), float3( -1, 0, 0 ), float3( 0, 0, +1 ), _BaseMaterialID + 0 );	// Ceiling
	UpdateClosestIntersection( _Ray, _Intersection, DeltaNeg.y, float2( _InvHalfSize.x, _InvHalfSize.z ), _Position + float3( 0, -HalfSize.y, 0 ), float3( 0, +1, 0 ), float3( +1, 0, 0 ), float3( 0, 0, +1 ), _BaseMaterialID + 1 );	// Floor
	UpdateClosestIntersection( _Ray, _Intersection, DeltaPos.x, float2( _InvHalfSize.z, _InvHalfSize.y ), _Position + float3( -HalfSize.x, 0, 0 ), float3( +1, 0, 0 ), float3( 0, 0, +1 ), float3( 0, +1, 0 ), _BaseMaterialID + 2 );	// Left
 	UpdateClosestIntersection( _Ray, _Intersection, DeltaNeg.x, float2( _InvHalfSize.z, _InvHalfSize.y ), _Position + float3( +HalfSize.x, 0, 0 ), float3( -1, 0, 0 ), float3( 0, 0, -1 ), float3( 0, +1, 0 ), _BaseMaterialID + 3 );	// Right
 	UpdateClosestIntersection( _Ray, _Intersection, DeltaPos.z, float2( _InvHalfSize.x, _InvHalfSize.y ), _Position + float3( 0, 0, -HalfSize.z ), float3( 0, 0, +1 ), float3( -1, 0, 0 ), float3( 0, +1, 0 ), _BaseMaterialID + 4 );	// Back
 	UpdateClosestIntersection( _Ray, _Intersection, DeltaNeg.z, float2( _InvHalfSize.x, _InvHalfSize.y ), _Position + float3( 0, 0, +HalfSize.z ), float3( 0, 0, -1 ), float3( +1, 0, 0 ), float3( 0, +1, 0 ), _BaseMaterialID + 5 );	// Front

	return _Intersection.Distance;
}

// Intersection with a 2D rectangle
//	_Position, position of the center of the rectangle
//	_Normal, normal of the rectangle plane
//	_X, scaled X axis of the rectangle (should be the axis scaled by HalfWidth)
//	_Z, scaled Z axis of the rectangle (should be the axis scaled by HalfHeight)
// Returns the distance to the intersection or +INFINITY if no hit
//
float	IntersectRectangle( Ray _Ray, inout Intersection _Intersection, const float3 _Position, const float3 _Normal, const float3 _X, const float3 _Z, const uint _MaterialID )
{
	float3	ToCenter = _Position - _Ray.P;
	float	Distance2Center = dot( ToCenter, _Normal );	// Distance to reach the plane's center following the normal
	float	Velocity = dot( _Ray.V, _Normal );			// Velocity of the ray direction following the normal
	_Intersection.Distance = Distance2Center / Velocity;

	// Patch negative hits => Send back to infinity if hit is behind origin
	_Intersection.Distance = lerp( _Intersection.Distance, INFINITY, saturate( -10000.0 * _Intersection.Distance ) );

	// Compute hit position on the plane
	_Intersection.Position = _Ray.P + _Intersection.Distance * _Ray.V;

	// Compute "UV coordinates"
	float3	DPos = _Intersection.Position - _Position;
	_Intersection.UV = float2( dot( DPos, _X ), dot( DPos, _Z ) );

	// Patch hit distance if UV is outside [-1,+1]
	_Intersection.Distance = lerp( _Intersection.Distance, INFINITY, saturate( 10000.0 * (abs( _Intersection.UV.x ) - 1.0) ) );
	_Intersection.Distance = lerp( _Intersection.Distance, INFINITY, saturate( 10000.0 * (abs( _Intersection.UV.y ) - 1.0) ) );

	_Intersection.Normal = _Normal;
	_Intersection.Tangent = _X;
	_Intersection.BiTangent = _Z;
	_Intersection.MaterialID = _MaterialID;

	return _Intersection.Distance;
}

