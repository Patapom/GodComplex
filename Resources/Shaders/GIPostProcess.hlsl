//////////////////////////////////////////////////////////////////////////
// This shader post-processes the Global Illumination test room
//
#include "Inc/Global.hlsl"

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


Texture2D<float4>	_TexSourceImage : register( t10 );

// DEBUG!
TextureCube<float4>	_TexCubemapProbe0 : register( t64 );
TextureCube<float4>	_TexCubemapProbe1 : register( t65 );


float4	PS( VS_IN _In ) : SV_TARGET0
{
	float2	UV = _In.__Position.xy * _dUV.xy;

float	Angle = PI;//_Time.x;
float	AspectRatio = 1280.0 / 720.0;
float3	View = normalize( float3( AspectRatio * (2.0 * UV.x - 1.0), 1.0 - 2.0 * UV.y, 1.0 ) );
// float3	Up = float3( 0, 1, 0 );
// float3	Right = -float3( cos( Angle ), 0, sin( Angle ) );
// float3	At = float3( -sin( Angle ), 0, cos( Angle ) );
// 		View = View.x * Right + View.y * Up + View.z * At;

View = mul( float4( View, 0.0 ), _Camera2World ).xyz;


return _TexCubemapProbe0.Sample( LinearClamp, View );
//return _TexCubemapProbe1.Sample( LinearClamp, View );
return 0.1 * _TexCubemapProbe1.Sample( LinearClamp, View ).w;

	return _TexSourceImage.SampleLevel( LinearClamp, UV, 0.0 );
	return float4( _In.__Position.xy * _dUV.xy, 0, 0 );
}
