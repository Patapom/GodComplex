////////////////////////////////////////////////////////////////////////////////
// Fullscreen shader that displays the distance field as a solid
////////////////////////////////////////////////////////////////////////////////
#include "Global.hlsl"
#include "DistanceField.hlsl"
#include "Noise.hlsl"

cbuffer	cbRender : register(b8)
{
	float4		_Dimensions;		// XY=Dimensions of the render target, ZW=1/XY
}

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
//	float	k = 0.5 * FBM( 2.0 * _Position, 2.0f * BuildRotationMatrix( 0.03f, float3( 0.95641, 0.10619, -0.10251981 ) ), 4 );


//	float	k = 0.5 * Turbulence( 0.5 * _Position, 8.0f * BuildRotationMatrix( 0.03f, float3( 0.95641, 0.10619, -0.10251981 ) ), 4 );	// Poilu!

	float	k = -0.5 * Turbulence( 0.5 * _Position, 4.0f * BuildRotationMatrix( 0.3f, float3( 0.95641, 0.10619, -0.10251981 ) ), 8 );

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
	float	k = -0.25 * FBM( 1.0 * _Position, 2.0f * BuildRotationMatrix( 0.03f, float3( 0.81441, -0.5613, 0.5619 ) ), 8 );
	return Distance1( _Position + k * n );
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

	// Compute view direction in world space
	float3	View = normalize( float3( _CameraData.xy * float2( 2.0 * UV.x - 1.0, 1.0 - 2.0 * UV.y ), 1.0 ) );
			View = mul( float4( View, 0.0 ), _Camera2World ).xyz;

	float4	Hit = ComputeIntersectionEnter( _Camera2World[3].xyz, View );

	return lerp( 0.1 * Hit.w, 0.5 * float4( 135, 206, 235, 255 ) / 255.0, IsInfinity( Hit.w ) );
}
