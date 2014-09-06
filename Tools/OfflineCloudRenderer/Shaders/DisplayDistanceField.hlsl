////////////////////////////////////////////////////////////////////////////////
// Fullscreen shader that displays the distance field as a solid
////////////////////////////////////////////////////////////////////////////////
#include "Global.hlsl"
#include "DistanceField.hlsl"
#include "Noise.hlsl"

cbuffer	cbRender : register(b8)
{
	float4		_Dimensions;		// XY=Dimensions of the render target, ZW=1/XY
	float4		_DEBUG;
}

////////////////////////////////////////////////////////////////////////////////
// 3D Tests

float	Distance0( float3 _Position )
{
	float	d0 = Distance2Ellipsoid( _Position, float3( 0.0, 0.0, 0.0 ), float3( 0.5, 1.0, 1.0 ) );
	float	d1 = Distance2Ellipsoid( _Position, float3( -1.5, 1.4, 0.0 ), float3( 0.7, 1.0, 1.0 ) );
	float	d2 = Distance2Ellipsoid( _Position, float3( -1.2, 0.2, 0.2 ), float3( 0.4, 1.2, 0.6 ) );

//return d0 + d1 - d0 * d1;
	return SmoothMin( d0, SmoothMin( d1, d2, 0.2 ), 0.2 );
	return min( d0, d1 );
}

float3	Normal0( float3 p, out float _GradientLength, float eps=0.0001 )
{
	const float2 e = float2( eps, 0.0 );
	float3	n = float3(
		Distance0( p + e.xyy ) - Distance0( p - e.xyy ),
		Distance0( p + e.yxy ) - Distance0( p - e.yxy ),
		Distance0( p + e.yyx ) - Distance0( p - e.yyx )
		);
	_GradientLength = length( n );
	return n / _GradientLength;
}

float	Distance1( float3 _Position )
{
	float	d = Distance0( _Position );
	float	l;
	float3	n = Normal0( _Position, l );
// 	float	k = Cellular( _Position, 10.0 );
//	float	k = 0.5 * FastNoise( 4.0 * _Position );
//	float	k = 0.5 * FBM( 2.0 * _Position, 4, 2.0f * BuildRotationMatrix( 0.03f, float3( 0.95641, 0.10619, -0.10251981 ) ) );


//	float	k = 0.5 *  Turbulence( 0.5 * _Position, 4, 8.0f * BuildRotationMatrix( 0.03f, float3( 0.95641, 0.10619, -0.10251981 ) ) );	// Poilu!

	float	k = -0.5 * Turbulence( 0.5 * _Position, 8, 4.0f * BuildRotationMatrix( 0.3f, float3( 0.95641, 0.10619, -0.10251981 ) ) );

	return Distance0( _Position + k * n );
}

float3	Normal1( float3 p, out float _GradientLength, float eps=0.0001 )
{
	const float2 e = float2( eps, 0.0 );
	float3	n = float3(
		Distance1( p + e.xyy ) - Distance1( p - e.xyy ),
		Distance1( p + e.yxy ) - Distance1( p - e.yxy ),
		Distance1( p + e.yyx ) - Distance1( p - e.yyx )
		);
	_GradientLength = length( n );
	return n / _GradientLength;
}

float	Map( float3 _Position )
{
	float	d = Distance1( _Position );
	float	l;
	float3	n = Normal0( _Position, l );
// 	float	k = Cellular( _Position, 10.0 );
//	float	k = 0.25 * FastNoise( 8.0 * _Position );
	float	k = -0.25 * FBM( 1.0 * _Position, 8, 2.0f * BuildRotationMatrix( 0.03f, float3( 0.81441, -0.5613, 0.5619 ) ) );
	return Distance1( _Position + k * n );
}



////////////////////////////////////////////////////////////////////////////////
// 2D Tests

#if 0
float	EstimateDistance( float2 _Position )
{
	float2	Center = float2( 0.0, 0.0 );
	float2	Size = float2( 0.4, 0.2 );

	float	d = length( (_Position - Center) / Size ) - 1.0;

// 	// Pyroclastic puff, as described by Tessendorf in http://people.clemson.edu/~jtessen/papers_files/ProdVolRender.pdf
// 	const float2	SphereCenter = float2( 0, 0 );
// 	const float		SphereRadius = 0.2;
// 
// 	float2	Center2Pos = _Position - SphereCenter;
// 	float	R2 = dot( Center2Pos, Center2Pos );
// 	float2	N = Center2Pos / sqrt( R2 );
// 
// 	float	d = abs( _DEBUG.w * FBM( float3( _DEBUG.y * N, 0 ), _DEBUG.z ) ) + _DEBUG.x - R2 / (SphereRadius*SphereRadius);
// 			d = -d;
 
	return d;
}

float2	EstimateNormal( float2 _Position )
{
//return normalize( float2( -_Position.y, _Position.x ) );

	const float2 e = float2( 0.0001, 0.0 );
	float2	N = float2(
		EstimateDistance( _Position + e.xy ) - EstimateDistance( _Position - e.xy ),
		EstimateDistance( _Position + e.yx ) - EstimateDistance( _Position - e.yx )
		);
	return normalize(N);
}

float	CellularFBM( float2 _Position, float _Amplitude, float _GridSize )
{
	for ( int i=0; i < 4; i++ )
	{
		float2	N = EstimateNormal( _Position );
		_Position += _Amplitude * Cellular( float3( _Position, 3.6516 * i ), 1.0 / _GridSize ) * N;	// Move in the direction of the normal
		_Amplitude *= 0.5;
		_GridSize *= 0.5;
	}

	return EstimateDistance( _Position );
}

float4	TestDistanceFieldDisplacement2D( float2 _Position )
{
	float4	SkyColor = float4( 135, 206, 235, 255 ) / 255.0;
//return 1.0 * Cellular( float3( _Position, 0 ), 20.0 );

// 	float	d = EstimateDistance( _Position );
// 	float2	N = EstimateNormal( _Position );
// //return float4( N, 0, 1 );
// 
// // 	_Position += 0.1 * Cellular( float3( _Position, 0.0 ), 20.0 ) * N;
// // 	d = EstimateDistance( _Position );
// 
// //	d = CellularFBM( _Position, 0.05, 0.1 );


	// Pyroclastic puff, as described by Tessendorf in http://people.clemson.edu/~jtessen/papers_files/ProdVolRender.pdf
	const float2	SphereCenter = float2( 0, 0 );
	const float		SphereRadius = 0.2;

	float2	Center2Pos = _Position - SphereCenter;
	float	R2 = dot( Center2Pos, Center2Pos );
	float2	N = Center2Pos / sqrt( R2 );

	float	d = abs( _DEBUG.w * FBM( float3( _DEBUG.y * N, 0 ), _DEBUG.z ) ) + _DEBUG.x - R2 / (SphereRadius*SphereRadius);
			d = -d;

	return d < 0.0 ? float4( 1, 1, 1, 1 ) : SkyColor;
}
#else

////////////////////////////////////////////////////////////////////////////////
// Pyroclastic noise test
static const float2 eps = float2( 0.0001, 0.0 );

static const float	A0 = (1+_DEBUG.z) * 0.1, f0 = (1+_DEBUG.w) * 1.0;
static const float	A1 = 0.5 * A0, f1 = 2.0 * f0;
static const float	A2 = 0.5 * A1, f2 = 2.0 * f1;
static const float	A3 = 0.5 * A2, f3 = 2.0 * f2;

// Main, underlying distance field
float	D0( float2 _Position )
{
	float2	Center = float2( 0.0, 0.0 );
	float2	Size = float2( 0.4, 0.2 );

	float	d = length( (_Position - Center) / Size ) - 1.0;
	return d;
}

float2	N0( float2 _Position )
{
	return normalize( float2(	D0( _Position + eps.xy ) - D0( _Position - eps.xy ),
								D0( _Position + eps.yx ) - D0( _Position - eps.yx ) ) );
}

// Level 1 displacement
float2	M1( float2 _Position )
{
	float2	N = N0( _Position );
	return _Position - A0 * _DEBUG.x * abs(FBM( float3( f0 * N, 0.0 ), 1+_DEBUG.y )) * N;
}

float	D1( float2 _Position )
{
	return D0( M1( _Position ) );
}

float2	N1( float2 _Position )
{
	return normalize( float2(	D1( _Position + eps.xy ) - D1( _Position - eps.xy ),
								D1( _Position + eps.yx ) - D1( _Position - eps.yx ) ) );
}

// Level 2 displacement
float2	M2( float2 _Position )
{
	_Position = M1( _Position );
	float2	N = N1( _Position );
	return _Position;// - A1 * _DEBUG.x * abs(FBM( float3( f1 * N, 0.0 ), 1+_DEBUG.y )) * N;
}

float	D2( float2 _Position )
{
	return D1( M2( _Position ) );
}

float2	N2( float2 _Position )
{
	return normalize( float2(	D2( _Position + eps.xy ) - D2( _Position - eps.xy ),
								D2( _Position + eps.yx ) - D2( _Position - eps.yx ) ) );
}

/* Recursive functions are not allowed!
float2	Move( float2 _Position, uint _IterationsCount )
{
	float	A = 1.0;
	float	f = 1.0;
	for ( uint i=0; i < _IterationsCount; i++ )
	{
		float2	Normal = EstimateNormal( _Position, i );
		_Position += (1.0+_DEBUG.x) * A * FBM( float3( f * _Position, 0.0 ), 1+_DEBUG.y ) * Normal;
		A *= 0.5;
		f *= 2.0;
	}

	return _Position;
}

float2	EstimateNormal( float2 _Position, uint _Level )
{
	float2	N = float2(
		EstimateDistance( Move( _Position + e.xy, _Level ) ) - EstimateDistance( Move( _Position - e.xy, _Level ) ),
		EstimateDistance( Move( _Position + e.yx, _Level ) ) - EstimateDistance( Move( _Position - e.yx, _Level ) )
		);
	return normalize(N);
}
*/

// // This needs to be a recursive function...
// float	EstimateDistance( float2 _Position, float _DisplacementAmplitude, float _DisplacementFrequency, uint _IterationsCount )
// {
// 	if ( _IterationsCount == 0 )
// 	{	// Level 0 is main distance field estimate
// 		return EstimateDistance( _Position );
// 	}
// 
// 	// Compute displacement amplitude and frequency for lower levels
// 	_DisplacementAmplitude *= 2.0;
// 	_DisplacementFrequency *= 0.5;
// 
// 	// Compute normal at position from lower level distance field
// 	const float2 e = float2( 0.0001, 0.0 );
// 	float2	N = float2(
// 		EstimateDistance( _Position + e.xy, _DisplacementAmplitude, _DisplacementFrequency, _IterationsCount-1 ) - EstimateDistance( _Position - e.xy, _DisplacementAmplitude, _DisplacementFrequency, _IterationsCount-1 ),
// 		EstimateDistance( _Position + e.yx, _DisplacementAmplitude, _DisplacementFrequency, _IterationsCount-1 ) - EstimateDistance( _Position - e.yx, _DisplacementAmplitude, _DisplacementFrequency, _IterationsCount-1 )
// 		);
// 	N = normalize( N );
// 
// 	// Displace position along normal
// 	float	Displacement = _DisplacementAmplitude * FBM( float3( (1+_DEBUG.y) * _DisplacementFrequency * _Position, 0 ), 1+_DEBUG.w );
// 	_Position += Displacement * N;
// 
// 	// Recurse
// 	return EstimateDistance( _Position, _DisplacementAmplitude, _DisplacementFrequency, _IterationsCount-1 );
// }
// 
// float2	EstimateNormal( float2 _Position, float _DisplacementAmplitude, float _DisplacementFrequency, uint _IterationsCount )
// {
// //return normalize( float2( -_Position.y, _Position.x ) );
// 
// 	const float2 e = float2( 0.0001, 0.0 );
// 	float2	N = float2(
// 		EstimateDistance( _Position + e.xy, _IterationsCount ) - EstimateDistance( _Position - e.xy, _IterationsCount ),
// 		EstimateDistance( _Position + e.yx, _IterationsCount ) - EstimateDistance( _Position - e.yx, _IterationsCount )
// 		);
// 	return normalize(N);
// }

#endif


float2	EstimateVector( float2 _Position )
{
	return N2( _Position );
}

struct VS_IN
{
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _Dimensions.zw;

	float2	P = float2( _Dimensions.x * _Dimensions.w * (UV.x - 0.5), 0.5 - UV.y );

//return float4( EstimateNormal( P ), 0, 0 );

return D2( P ) < 0.0 ? 1.0 : float4( 135, 206, 235, 255 ) / 255.0;
//return D2( P ) > 0.0 ? Show2DVectorField( P, _Dimensions.w ) : 1.0;
//return FastScreenNoise( P );
//return TestDistanceFieldDisplacement2D( P );

	// Compute view direction in world space
	float3	View = normalize( float3( _CameraData.xy * float2( 2.0 * UV.x - 1.0, 1.0 - 2.0 * UV.y ), 1.0 ) );
			View = mul( float4( View, 0.0 ), _Camera2World ).xyz;

	float4	Hit = ComputeIntersectionEnter( _Camera2World[3].xyz, View );

	return lerp( 0.1 * Hit.w, 0.5 * float4( 135, 206, 235, 255 ) / 255.0, IsInfinity( Hit.w ) );
}
