//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};

struct PS_OUT {
	float4	RT0 : SV_TARGET0;
	float4	RT1 : SV_TARGET1;
};

float	IntersectBox2( float3 _wsPos, float3 _wsView, out float3 _wsNormal ) {
	float3	dir = _wsView < 0.0 ? -1.0 : 1.0;
	float3	wallDistance = dir - _wsPos;
	float3	t3 = wallDistance / _wsView;
	float	t = t3.x;
	_wsNormal = float3( -dir.x, 0, 0 );
	if ( t3.y < t ) {
		t = t3.y;
		_wsNormal = float3( 0, -dir.y, 0 );
	}
	if ( t3.z < t ) {
		t = t3.z;
		_wsNormal = float3( 0, 0, -dir.z );
	}
	return t;
}

float	IntersectSphere2( float3 _wsPos, float3 _wsView, float3 _wsCenter, float _radius, out float3 _wsNormal ) {
	float3	D = _wsPos - _wsCenter;
	float	c = dot( D, D ) - _radius*_radius;
	float	b = dot( D, _wsView );
	float	delta = b*b - c;
	float	t = -b - sqrt( delta );
	_wsNormal = normalize( _wsPos + t * _wsView - _wsCenter );
	return delta >= 0.0 && b < 0.0 ? t : INFINITY;
}

float2	Map2( float3 _wsPos, float3 _wsView, out float3 _wsNormal ) {
	float2	d = float2( IntersectBox2( _wsPos, _wsView, _wsNormal ), 0 );

	float3	wsNormal2;
	float2	ds = float2( IntersectSphere2( _wsPos, _wsView, ComputeSphereCenter(), SPHERE_RADIUS, wsNormal2 ), 1 );
	if ( ds.x < d.x ) {
		_wsNormal = wsNormal2;
	}
	return d.x < ds.x ? d : ds;
}


PS_OUT	PS( VS_IN _In ) {
	float2	UV = _In.__Position.xy / _Resolution;
	float3	csView = float3( float(_Resolution.x) / _Resolution.y * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 );
//return float3( csView.xy, 0 );
	float	viewLength = length( csView );
			csView /= viewLength;

	float3	wsView = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	float3	wsPos = _Camera2World[3].xyz;

	float3	wsNormal;
	float2	bisou = Map2( wsPos, wsView, wsNormal );

	PS_OUT	Out;
			Out.RT0 = float4( wsNormal, bisou.x );
			Out.RT1 = float4( bisou.y, 1.0 - lerp( _GlossRoom, _GlossSphere, bisou.y ), 0, 0 );

	return Out;
}
