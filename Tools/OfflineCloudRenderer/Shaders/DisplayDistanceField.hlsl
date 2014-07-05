////////////////////////////////////////////////////////////////////////////////
// Fullscreen shader that displays the distance field as a solid
////////////////////////////////////////////////////////////////////////////////
#include "Global.hlsl"
#include "DistanceField.hlsl"

cbuffer	cbRender : register(b8)
{
	float4		_Dimensions;		// XY=Dimensions of the render target, ZW=1/XY
}
 
float	Map( float3 _Position )
{
	return Distance2Ellipsoid( _Position, float3( 0.0, 0.0, 0.0 ), float3( 0.5, 1.0, 1.0 ) );
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
