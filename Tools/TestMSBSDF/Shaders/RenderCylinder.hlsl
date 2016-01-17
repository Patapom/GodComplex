#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
	float3	_Direction;
	float	_Length;
	float	_Radius;
}

struct VS_IN {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Color : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	float3	lsPosition = _In.Position;	// The cylinder is composed of 2 unit radius circles at Y=0 and Y=1
			lsPosition.xz *= _Radius;
			lsPosition.y *= _Length;	// Now properly scaled

	float3	wsDirection = -float3( _Direction.x, _Direction.z, -_Direction.y );	// Actual ray incoming direction in Y-up, reversed as we want the one pointing out of the surface
	float3	tangent, biTangent;
	BuildOrthonormalBasis( wsDirection, tangent, biTangent );

	float3	wsPosition = lsPosition.x * tangent + lsPosition.y * wsDirection + lsPosition.z * biTangent;

	PS_IN	Out;
	Out.__Position = mul( float4( wsPosition, 1.0 ), _World2Proj );
	Out.Color = float3( 1, 0, 0 );

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	return _In.Color;
}
