#include "Global.hlsl"

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _in ) { return _in; }

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float3	csView = float3( 2.0 * _In.__Position.xy / iResolution.xy - 1.0, 1.0 );
			csView.x *= iResolution.x / iResolution.y;
			csView.y = -csView.y;
			
	float	Z2Length = length( csView );
			csView /= Z2Length;

	float3	wsView = mul( float4( csView, 0 ), _camera2World ).xyz;
	float3	wsCamPos = _camera2World[3].xyz;


	float	t = -wsCamPos.y / wsView.y;
	if ( t < 0.0 )
		return 0;

	float3	wsPos = wsCamPos + t * wsView;

	return 0.1 * wsPos;
}
