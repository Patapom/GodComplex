////////////////////////////////////////////////////////////////////////////////
// Distance Field Computation routines
////////////////////////////////////////////////////////////////////////////////
#ifndef __DISTANCE_FIELD
#define __DISTANCE_FIELD

// Pre-Declaration of the distance field evaluator that MUST be declared by your program to be able to use most of the routines written in this file
float	Map( float3 _Position );

////////////////////////////////////////////////////////////////////////////////
// Distance functions

// Computes the distance to an ellipsoid
//	_Position, the position where to evaluate the distance
//	_Center, the ellipsoid's center
//	_InvRadius, 1 over the ellipsoid's radius
//
float	Distance2Ellipsoid( float3 _Position, float3 _Center, float3 _InvRadius )
{
	return length( (_Position - _Center) * _InvRadius ) - 1.0;
}


////////////////////////////////////////////////////////////////////////////////
// Composition functions (from http://iquilezles.org/www/articles/smin/smin.htm)

// Polynomial smooth min (k = 0.1);
float	SmoothMin( float a, float b, float k )
{
    float h = clamp( 0.5+0.5*(b-a)/k, 0.0, 1.0 );
    return lerp( b, a, h ) - k*h*(1.0-h);
}
// Exponential smooth min (k = 32);
float	SmoothMin2( float a, float b, float k )
{
    float res = exp( -k*a ) + exp( -k*b );
    return -log( res ) / k;
}

// Power smooth min (k = 8);
float	SmoothMin3( float a, float b, float k )
{
    a = pow( a, k ); b = pow( b, k );
    return pow( (a*b)/(a+b), 1.0/k );
}


////////////////////////////////////////////////////////////////////////////////
// Algorithms

// Computes the normal by evaluating the distance field's gradient
float3	Normal( float3 p, out float _GradientLength, float eps=0.0001 )
{
	const float2 e = float2( eps, 0.0 );
//	float c = Map( p );
	float3	n = float3(
		Map( p + e.xyy ) - Map( p - e.xyy ),
		Map( p + e.yxy ) - Map( p - e.yxy ),
		Map( p + e.yyx ) - Map( p - e.yyx )
		);
	_GradientLength = length( n );
	return n / _GradientLength;
}

// Computes the intersection entering the isosurface represented by d=0 (thus assuming the view point is outside the isosurface)
//	_StartPosition, the start position of the ray
//	_View, the view direction of the ray
// Returns a float4 where XYZ=Hit position and W=Hit Distance (W=+oo if no hit was found)
//
float4	ComputeIntersectionEnter( float3 _StartPosition, float3 _View, const float _DistanceThreshold=0.005, const uint _STEPS_COUNT=256 )
{
	float4	Position = float4( _StartPosition, 0.0 );
	float4	View = float4( _View, 1.0 );

	for ( uint StepIndex=0; StepIndex < _STEPS_COUNT; StepIndex++ )
	{
		float	D = Map( Position.xyz );
		if ( D < _DistanceThreshold )
			return Position;	// Hit!
		Position += D * View;
	}
	return INFINITY;
}

// Computes the intersection exiting the isosurface represented by d=0 (thus assuming the view point is inside the isosurface)
//	_StartPosition, the start position of the ray
//	_View, the view direction of the ray
// Returns a float4 where XYZ=Hit position and W=Hit Distance (W=+oo if no hit was found)
//
float4	ComputeIntersectionExit( float3 _StartPosition, float3 _View, const float _DistanceThreshold=-0.005, const uint _STEPS_COUNT=256 )
{
	float4	Position = float4( _StartPosition, 0.0 );
	float4	View = float4( _View, 1.0 );

	for ( uint StepIndex=0; StepIndex < _STEPS_COUNT; StepIndex++ )
	{
		float	D = Map( Position.xyz );
		if ( D > _DistanceThreshold )
			return Position;	// Hit!
		Position += D * View;
	}
	return INFINITY;
}


////////////////////////////////////////////////////////////////////////////////
// Advanced displacement

#endif