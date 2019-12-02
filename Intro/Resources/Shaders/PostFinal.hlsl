#include "Inc/Global.hlsl"

Texture2D	_TexNoise	: register(t10);
Texture2D	_TexHDR		: register(t11);

//[
cbuffer	cbTextureLOD	: register( b10 )
{
	float	_LOD;
	float	_BackLight;
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
	float3	PlanePosition = float3( 0.0, -1.0f, 0.0 );
	float3	PlaneNormal = float3( 0, 1, 0 );

	float3	LightPosition = float3( 0.0, 0.0, 0.0 );
	float3	LightPower = 10.0 * 1.0.xxx;

	float3	Ambient = 0.5 * 1.0.xxx;

	// Compute view
	float3	CamPos = _Camera2World[3].xyz;
	float3	CamView = mul( float4( _CameraData.x * (2.0 * _UV.x - 1.0), _CameraData.y * (1.0 - 2.0 * _UV.y), 1.0, 0.0 ), _Camera2World ).xyz;

	// Compute plane intersection distance
	float	PlaneDistance = dot( PlanePosition - CamPos, PlaneNormal ) / dot( CamView, PlaneNormal );
			PlaneDistance = PlaneDistance < 0.0 ? 1000.0 : PlaneDistance;
	float3	PlaneHit = CamPos + PlaneDistance * CamView;
//return float4( 0.2 * PlaneHit, 1 );

	PlaneDistance = 0.5 * pow( abs(PlaneDistance), 0.25 );
//return PlaneDistance;

	// Compute distance to the sphere
	float3	SphereCenter = float3( 0.0, 1.0, 0.0 );
	float	SphereDistance = length( SphereCenter - PlaneHit );

	SphereDistance = pow( saturate( 0.42 * SphereDistance ), 3.0 );
//return SphereDistance;

	// Combine
	return PlaneDistance * SphereDistance;
}

float4	PS( VS_IN _In ) : SV_TARGET0
{
	float4	SourceHDR = _TexHDR.Sample( LinearWrap, _In.Position.xy * INV_SCREEN_SIZE );

	float2	UV = 2.0 * float2( ASPECT_RATIO * _In.Position.x, _In.Position.y ) * INV_SCREEN_SIZE;
//	float4	Background = TEXLOD( _TexNoise, LinearWrap, UV, _LOD );
	float4	Background = lerp( 0.1, 1.0, _BackLight ) * ComputeBackground( _In.Position.xy * INV_SCREEN_SIZE );


//return Background;
//return 0.5 * ((Background.y - Background.x) - (Background.w - Background.z));
//return 0.5 * Background.y;
//return 1.0 * (Background.y - Background.x);
//return 1.0 * (Background.w - Background.z);
//return float4( UV, 0, 0 );
//return TEXLOD( _TexNoise3D, LinearWrap, float3( UV, frac( 0.125 * _Time.x ) ), 0.0 );

	return lerp( Background, SourceHDR, SourceHDR.w );	// Alpha blend...
}
