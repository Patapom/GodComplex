#include "Global.hlsl"

Texture2D< float >	_Tex_HeightField : register( t0 );

struct VS_IN {
	float3	Position : POSITION;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float3	Normal : NORMAL;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {

	float3	lsPosition = _In.Position;

	PS_IN	Out;
	Out.UV = float2( 0.5 * (1.0 + lsPosition.x), 0.5 * (1.0 - lsPosition.y) );

	float	H0 = 0.0 * _Tex_HeightField.SampleLevel( LinearClamp, Out.UV, 0.0 );

	Out.__Position = mul( float4( lsPosition.x, H0, -lsPosition.y, 1.0 ), _World2Proj );
	Out.Normal = float3( 0.0, H0, 0.0 );

	return Out;
}

float3	PS( PS_IN _In ) : SV_TARGET0 {
	return (2.0+_Tex_HeightField.SampleLevel( LinearClamp, _In.UV, 0.0 )) / 4.0;
	return float3( _In.UV, 0 );
	return 0.5 * (1.0 + _In.Normal.y );
	return float3( 1, 0, 0 );
}
