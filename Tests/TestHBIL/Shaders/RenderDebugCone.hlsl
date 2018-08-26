////////////////////////////////////////////////////////////////////////////////
// Renders the debug cone
////////////////////////////////////////////////////////////////////////////////
// 
#include "Global.hlsl"

static const float	VECTOR_RADIUS = 0.01;
static const float	VECTOR_SIZE = 0.5;

cbuffer CB_DebugCone : register(b3) {
	float3	_wsConePosition;
	float	_coneAngle;
	float3	_wsConeDirection;
	float	_coneStdDeviation;
	uint	_coneFlags;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Color : COLOR;
};

PS_IN	VS( float3 _Position : POSITION ) {
	PS_IN	Out;

	float3	N = _wsConeDirection;
	float3	T, B;
	BuildOrthonormalBasis( N, T, B );

	float3	wsPosition = _wsConePosition;
	float3	lsNormal;
	if ( _coneFlags & 1 ) {
		// Render vector
		wsPosition = _wsConePosition + VECTOR_RADIUS * (_Position.x * T + _Position.y * B) + VECTOR_SIZE * _Position.z * N;
		lsNormal = float3( _Position.xy, 0 );
	} else {
		// Render cone
		if ( 1 && _Position.z > 0.5 ) {
//			float	a = 0.5 * (1+sin(_time)) * _coneAngle;
			float	a = _coneAngle;
			wsPosition = _wsConePosition + 2*VECTOR_SIZE * (sin( a ) * (_Position.x * T + _Position.y * B) + cos( a ) * N);
		}
		lsNormal = float3( sin( _coneAngle ) * _Position.xy, - cos( _coneAngle ) );
	}

	Out.Color = lerp( 0.5, 1.0, lsNormal.x ) * float3( 1, 0.9, 0.2 );
	Out.__Position = mul( float4( wsPosition, 1.0 ), _world2Proj );

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	return _In.Color; 
}