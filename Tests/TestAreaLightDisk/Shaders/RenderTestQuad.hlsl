#include "Global.hlsl"


cbuffer CB_TestQuad : register(b3) {
	float4x4	_wsQuad2World;
	float3x3	_invM_transposed;
};


struct PS_IN {
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( float4 _position : SV_POSITION ) {
	float3	wsPosition  = _wsQuad2World[3].xyz
						+ _position.x * _wsQuad2World[0].xyz
						+ _position.y * _wsQuad2World[1].xyz;

	// Convert to tangent space position, assuming the point we're lighting is the origin (0,0,0)
	float3	tsPosition = float3( wsPosition.x, -wsPosition.z, wsPosition.y );

	// Convert to LTC canonical space
	float3	ltcPosition = mul( tsPosition, _invM_transposed );
//	float3	ltcPosition = mul( _invM_transposed, tsPosition );
//float3	ltcPosition = tsPosition;

	// Assuming LTC canonical space is our new world space, build a new world position
	wsPosition = float3( ltcPosition.x, ltcPosition.z, -ltcPosition.y );

	// Project and finalize
	PS_IN	Out;
	Out.__Position = mul( float4( wsPosition, 1 ), _world2Proj );
	Out.UV = float2( 0.5*(1+_position.x), 0.5*(1-_position.y) );

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	return any( 1.0 - abs( 2.0 * _In.UV - 1.0 ) < 0.05 ) ? 1 : 0.05 * float3( 1, 0.3, 0.2 );
//	return float3( _In.UV, 0 );
//	return float3( abs( 2 * _In.UV - 1 ), 0.01 );
}
