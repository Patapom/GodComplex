#include "Global.hlsl"

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _size;
	float	aspectRatio = float(_size.x) / _size.y;
	float3	csView = float3( aspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0.0 ), _camera2World ).xyz;
	// Make view Z-up
	wsView = float3( wsView.x, -wsView.z, wsView.y );

	float3	wsPos = float3( _camera2World[3].x, -_camera2World[3].z, _camera2World[3].y );

	float3	filteredEnvironmentSH[9];
	FilterHanning( EnvironmentSH, filteredEnvironmentSH, _filterWindowSize );

	float3	color = 0.0;
	float	dist = IntersectSphere( wsPos, wsView, 0.0, 1.0 );
	if ( dist < 0.0 || dist > NO_HIT ) {
//		return (_flags & 0x100U) ? EvaluateSHIrradiance( wsView, filteredEnvironmentSH )
		return (_flags & 0x100U) ? _luminanceFactor * EvaluateSH( wsView, filteredEnvironmentSH )
								 : _luminanceFactor * SampleHDREnvironment( wsView );
	}

	// Regular rendering
	float3	wsHitPos = wsPos + dist * wsView;
	float3	wsNormal = wsHitPos;
//	color = wsNormal;
//	color = 0.01 * dist * _cosAO;

	float3	irradianceOFF = _luminanceFactor * 2.0 * INVPI * acos( _cosAO ) * EvaluateSHIrradiance( wsNormal, filteredEnvironmentSH );
//	float3	irradianceOFF = _luminanceFactor * EvaluateSH( wsNormal, filteredEnvironmentSH );
	float3	irradianceON = _luminanceFactor * EvaluateSHIrradiance( wsNormal, _cosAO, filteredEnvironmentSH );

	switch ( _flags & 1 ) {
	case 0:	color = (_flags & 0x8U) ? irradianceON : irradianceOFF; break;
	case 1:
		const float	dU = 3.0 / _size.x;
		float	left = smoothstep( 0.5, 0.5-dU, UV.x );
		float	right = smoothstep( 0.5, 0.5+dU, UV.x );
		color = left * irradianceOFF + right * irradianceON;
		break;
	}

	return color;
	return wsView;
	return float3( UV, 0 );
}