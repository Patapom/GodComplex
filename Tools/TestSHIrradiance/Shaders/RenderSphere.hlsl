#include "Global.hlsl"

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / _Size;

//	if ( (_Flags & 1) == 0 )
//		return float3( sin( _Time ) * UV, 0 );
//
//	// Build the camera ray
//	float3	At = normalize( _CameraTarget - _CameraPos );
//	float3	Right = normalize( cross( At, _CameraUp ) );
//	float3	Up = cross( Right, At );
//
//	float	TanHalfFOV = tan( 0.5 * CAMERA_FOV );
//	float3	csView = normalize( float3( _Size.x / _Size.y * TanHalfFOV * (2.0 * UV.x - 1.0), TanHalfFOV * (1.0 - 2.0 * UV.y ), 1.0 ) );
//	float3	wsView = csView.x * Right + csView.y * Up + csView.z * At;
//
//	// Intersect with ground plane
//	float3	Color = 0.0;
//	float	t = -_CameraPos.y / wsView.y;
//	if ( t > 0.0 )
//		Color = ComputeSSBump( _CameraPos + t * wsView );
//// 	else
//// 		t = 1000.0;
//
//	// Add light scattering
//	Color += LIGHT_INTENSITY * Airlight( _CameraPos, wsView, _Light, _CameraPos + t * wsView, AIRLIGHT_BETA );

	return Color;
}