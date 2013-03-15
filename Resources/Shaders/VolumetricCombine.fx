//////////////////////////////////////////////////////////////////////////
// This shader finally combines the volumetric rendering with the actual screen
//
#include "Inc/Global.fx"
#include "Inc/Volumetric.fx"

Texture2D		_TexDebug0	: register(t10);
Texture2DArray	_TexDebug1	: register(t11);

//[
cbuffer	cbObject	: register( b10 )
{
	float3		_dUV;
};
//]

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )	{ return _In; }

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;
//return float4( UV, 0, 1 );

// 	float2	Depth = _TexDebug0.SampleLevel( LinearClamp, UV, 0.0 ).xy;
// return 0.3 * (Depth.y - Depth.x);

	float4	C0 = _TexDebug1.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	C1 = _TexDebug1.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
//return abs( C0.x );
return 1.0 * abs(C0);

//return fmod( 0.5 * _Time.x, 1.0 );

	float3	ShadowPos = float3( 2.0 * fmod( 0.5 * _Time.x, 1.0 ) - 1.0, 1.0 - 2.0 * UV.y, _ShadowZMax.x * UV.x );
	return 0.2 * GetTransmittance( ShadowPos );
}
