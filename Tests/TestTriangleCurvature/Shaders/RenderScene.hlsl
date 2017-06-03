//////////////////////////////////////////////////////////////////////////
// 
//////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"

cbuffer CB_Render : register(b10) {
};


struct VS_IN {
	float3	Position : POSITION;
	float3	Normal : NORMAL;
	float3	Tangent : TANGENT;
	float3	BiTangent : BITANGENT;
	float2	UV : TEXCOORD0;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	wsPosition : POSITION;
	float3	wsNormal : NORMAL;

	float	sphereRadius : RADIUS;
	float3	wsFaceNormal : FACE_NORMAL;
	float3	wsFaceCenter: FACE_CENTER;

	float2	UV : TEXCOORD0;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Out.__Position = mul( float4( _In.Position, 1.0 ), _World2Proj );
	Out.wsPosition = _In.Position;	// Assume already in world space
	Out.wsNormal = _In.Normal;
	Out.wsFaceNormal = _In.Tangent;
	Out.wsFaceCenter = _In.BiTangent;
	Out.UV = _In.UV;

	// Compute sphere radius
	Out.sphereRadius = 1.0;

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	return float3( _In.UV, 0 );
}
