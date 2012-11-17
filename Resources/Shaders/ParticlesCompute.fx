//////////////////////////////////////////////////////////////////////////
// 
//
#include "Inc/Global.fx"

Texture2D	_TexParticles0	: register(t10);
Texture2D	_TexParticles1	: register(t11);

//[
cbuffer	cbRender	: register( b10 )
{
	float3	_dUV;		// XY=1/BufferSize Z=0
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

float3	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV;

	float3	Pt_2 = _TexParticles0.SampleLevel( PointClamp, UV, 0.0 ).xyz;
	float3	Pt_1 = _TexParticles1.SampleLevel( PointClamp, UV, 0.0 ).xyz;

//	float3	Acceleration = 0.0;

	float3	UVW = 0.1 * Pt_1;
	float3	Acceleration = 0.002 * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 ).xyz;

	float3	NewPosition = 2.0 * Pt_1 - Pt_2 + Acceleration * _Time.y * _Time.y;

	return NewPosition;
}
