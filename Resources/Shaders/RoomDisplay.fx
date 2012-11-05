//////////////////////////////////////////////////////////////////////////
// This shader displays the room
//
#include "Inc/Global.fx"

Texture2DArray	_TexLightMaps	: register(t10);

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float2	UV			: TEXCOORD0;
	float3	UV2			: TEXCOORD1;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Normal		: NORMAL;
	float2	UV			: TEXCOORD0;
	float3	UV2			: TEXCOORD1;
};

PS_IN	VS( VS_IN _In )
{
	float4	WorldPosition = mul( float4( _In.Position, 1.0 ), _Local2World );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
	Out.Normal = _In.Normal;
	Out.UV = _In.UV;
	Out.UV2 = _In.UV2;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
//	return 1;
//	return float4( _In.Normal, 1.0 );
//	return float4( _In.UV, 0, 1.0 );
//	return float4( _In.UV2, 1.0 );
//	return float4( _In.UV2.xy, 0, 1.0 );

// 	return 0.25 * (1+_In.UV2.z);
// 	return 10.0 * _TexLightMaps.SampleLevel( LinearClamp, float3( _In.UV, 1 ), 0.0 );
	return 5.0 * _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );
//	return _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 ) / 20.0;
//	return 0.2 * _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );
//	return _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );

	float3	Normal = _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 ).xyz;
	return float4( 100.0 * abs( Normal - _In.Normal ), 0 );

// 	float4	Color = _TexLightMap.SampleLevel( LinearClamp, _In.UV, 0.0 );
// 	return float4( Color.xyz, 1.0 );
// 	return float4( lerp( float3( _In.UV, 0 ), Color, Color.x + Color.y ), 1.0 );
}
