#include "Global.hlsl"

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _Size;
	float	aspectRatio = float(_Size.x) / _Size.y;
	float3	csView = float3( aspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;
	// Make view Z-up
	wsView = float3( wsView.x, -wsView.z, wsView.y );

	float3	wsPos = float3( _camera2World[3].x, -_camera2World[3].z, _camera2World[3].y );

	float3	color = 0.0;
	float	dist = IntersectSphere( wsPos, wsView, 0.0, 1.0 );
	if ( dist > 0.0 && dist < 1000.0 ) {
		float3	wsHitPos = wsPos + dist * wsView;
		float3	wsNormal = wsHitPos;
//		color = wsNormal;
//		color = 0.01 * dist * _cosAO;

		float3	irradianceOFF = 2.0 * INVPI * acos( _cosAO ) * EvaluateSHIrradiance( wsNormal, EnvironmentSH );
//		float3	irradianceOFF = EvaluateSH( wsNormal, EnvironmentSH );
		float3	irradianceON = EvaluateSHIrradiance( wsNormal, _cosAO, EnvironmentSH );
		switch ( _Flags & 3 ) {
			case 0:	color = irradianceOFF; break;
			case 1:	color = irradianceON; break;
			case 2:
			case 3:
				const float	dU = 3.0 / _Size.x;
				float	left = smoothstep( 0.5, 0.5-dU, UV.x );
				float	right = smoothstep( 0.5, 0.5+dU, UV.x );
				color = left * irradianceOFF + right * irradianceON;
				break;
		}
	} else {
		color = SampleHDREnvironment( wsView );
	}

	return color;
	return wsView;
	return float3( UV, 0 );
}