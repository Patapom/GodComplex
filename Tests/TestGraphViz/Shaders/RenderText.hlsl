#include "Global.hlsl"

Texture2D< float >	_tex_FontAtlas : register( t0 );
Texture2D< float4 >	_tex_FontRectangles : register( t1 );

struct SB_Letter {
	uint	m_letterIndex;	// Letter index in the _tex_FontRectangles
	float	m_offset;		// Normalized offset within the display rectangle
	float	m_ratio;		// Normalized ratio within the display rectangle
};
StructuredBuffer<SB_Letter>	_SB_Text : register( t2 );

cbuffer CB_Text : register(b2) {
	float2	_position;	// Top-Left position in NDC
	float2	_right;		// Right vector in NDC
	float2	_up;		// Up vector in NDC
};

struct VS_IN {
	float4	__Position : SV_POSITION;
	uint	instanceID : SV_INSTANCEID;
};

struct PS_IN {
	float4	__Position : SV_POSITION;
	float2	UV : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In ) {
	PS_IN	Out;

	Out.UV = 0.5 * (_In.__Position.xy + 1.0);

	SB_Letter	letter = _SB_Text[_In.instanceID];

	// Compute proper target position
	float	X = letter.m_offset + Out.UV.x * letter.m_ratio;
	float	Y = Out.UV.y;
	Out.__Position.xy = _position + X * _right - Y * _up;
//Out.__Position.xy = _position + Out.UV.x * _right + Out.UV.y * _up;
	Out.__Position.z = 0;
	Out.__Position.w = 1;

	// Target proper letter UVs within the atlas
	float4	letterRectangle = _tex_FontRectangles[uint2(letter.m_letterIndex,0)];
	Out.UV = letterRectangle.xy + letterRectangle.zw * Out.UV;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0 {
//return float3( _In.UV, 0 );
	float	color = _tex_FontAtlas.SampleLevel( LinearClamp, _In.UV, 0.0 );
	clip( color - 0.01 );
	return color;
}
