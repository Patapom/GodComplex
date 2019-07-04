#include "Global.hlsl"

struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float3	color : SV_TARGET0;
	float	depth : SV_DEPTH;
};

VS_IN	VS( VS_IN _in ) { return _in; }

PS_OUT	PS( VS_IN _In ) {
	float3	csView = GenerateCameraRay( _In.__Position.xy );
	float3	wsView = mul( float4( csView, 0 ), _camera2World ).xyz;
	float3	wsCamPos = _camera2World[3].xyz;

	float	t = -wsCamPos.y / wsView.y;
	clip( t );
//	if ( t < 0.0 ) {
//		discard;
//	}

	float3	wsPos = wsCamPos + t * wsView;
	float4	ndcPos = mul( float4( wsPos, 1 ), _world2Proj );

	PS_OUT	result;
	result.color = 0.1 * wsPos;
	result.depth = ndcPos.z / ndcPos.w;

	return result;
}
