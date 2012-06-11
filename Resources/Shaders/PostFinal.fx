#include "Inc/Global.fx"

Texture2D	_TexNoise	: register(t0);
Texture2D	_TexHDR		: register(t1);

//[
cbuffer	cbTextureLOD	: register( b1 )
{
	float	_LOD;
};
//]

struct	VS_IN
{
	float4	Position	: SV_POSITION;
};

VS_IN	VS( VS_IN _In )
{
	return _In;
}

float4	ComputeBackground( float2 _UV )
{
	float3	PlanePosition = float3( 0.0, -4.0f, 0.0 );
	float3	PlaneNormal = float3( 0, 1, 0 );

	float3	LightPosition = float3( 0.0, 0.0, 0.0 );
	float3	LightPower = 10.0 * 1.0.xxx;

	float3	Ambient = 0.5 * 1.0.xxx;

	// Compute view
	float3	CamPos = _Camera2World[3].xyz;
	float3	CamView = mul( float4( _UV.xy * _CameraData.xy, 1.0, 0.0 ), _Camera2World ).xyz;

	float	HitDistance = dot( CamPos - PlanePosition, PlaneNormal ) / dot( CamView, PlaneNormal );
	return 0.5 * pow( abs(HitDistance), 0.25 );
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float4	SourceHDR = TEX2D( _TexHDR, LinearWrap, _In.Position.xy * INV_SCREEN_SIZE );

	float2	UV = 2.0 * float2( ASPECT_RATIO * _In.Position.x, _In.Position.y ) * INV_SCREEN_SIZE;
//	float4	Background = TEX2DLOD( _TexNoise, LinearWrap, UV, _LOD );
	float4	Background = ComputeBackground( UV );
//return Background;
//return 0.5 * ((Background.y - Background.x) - (Background.w - Background.z));
//return 0.5 * Background.y;
//return 1.0 * (Background.y - Background.x);
//return 1.0 * (Background.w - Background.z);
//return float4( UV, 0, 0 );

	return lerp( Background, SourceHDR, SourceHDR.w );	// Alpha blend...
}
