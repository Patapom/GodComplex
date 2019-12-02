//////////////////////////////////////////////////////////////////////////
// This shader displays the room
//
#include "Inc/Global.hlsl"

Texture2DArray	_TexLightMaps	: register(t10);
Texture2DArray	_TexWalls		: register(t11);

Texture2D	_TexVoronoi	: register(t12);

//[
cbuffer	cbObject	: register( b10 )
{
	float4x4	_Local2World;

	float3		_LightColor0;
	float3		_LightColor1;
	float3		_LightColor2;
	float3		_LightColor3;
};
//]

struct	VS_IN
{
	float3	Position	: POSITION;
	float3	Normal		: NORMAL;
	float3	Tangent		: TANGENT;
	float3	UV			: TEXCOORD0;
	float3	UV2			: TEXCOORD1;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Normal		: NORMAL;
	float3	UV			: TEXCOORD0;
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
	float4	LightInfluences = _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );	// Radiance weight from each light source
	float3	Irradiance  = LightInfluences.x  * _LightColor0
						+ LightInfluences.y  * _LightColor1
						+ LightInfluences.z  * _LightColor2
						+ LightInfluences.w  * _LightColor3;

	float3	Radiance = Irradiance * RECITWOPI;

//return 10.0 * _TexWalls.Sample( LinearWrap, float3( _In.UV, 1.0 ) ).z;	// Show height
	float4	TexColor = 1.0;
	if ( _In.UV.z > 0.0 )
		TexColor = _TexWalls.Sample( LinearWrap, float3( _In.UV.xy, 0.0 ) );
//	return TexColor;
	Radiance *= clamp( TexColor.xyz, 0.1, 1.0 );


	// Check voronoï pattern
	int	ParticleIndex = int( _TexVoronoi.SampleLevel( PointWrap, _In.UV.xy, 0.0 ).x );
	float3	Colors[] = {
		float3( 1, 0, 0 ),
		float3( 1, 1, 0 ),
		float3( 1, 0, 1 ),
		float3( 0, 1, 0 ),
		float3( 1, 1, 0 ),
		float3( 0, 1, 1 ),
		float3( 0, 0, 1 ),
		float3( 1, 0, 1 ),
		float3( 0, 1, 1 ),
		float3( 1, 1, 1 ),
	};
//Radiance = _TexVoronoi.Sample( PointWrap, _In.UV.xy ).x / 256.0;
Radiance = Colors[ParticleIndex % 10];
Radiance = _TexVoronoi.SampleLevel( PointWrap, _In.UV.xy, 0.0 ).y;


	return float4( Radiance, 1 );
}

float4	PS_Emissive( PS_IN _In ) : SV_TARGET0
{
	// Isolate light index
	float	LightIndex = _In.UV2.z;
	float3	LightColor = _LightColor0;
			LightColor = lerp( LightColor, _LightColor1, saturate( 10000.0 * (LightIndex - 0.5) ) );
			LightColor = lerp( LightColor, _LightColor2, saturate( 10000.0 * (LightIndex - 1.5) ) );
			LightColor = lerp( LightColor, _LightColor3, saturate( 10000.0 * (LightIndex - 2.5) ) );

	float3	Radiance = LightColor * RECITWOPI;	// Light color is light's irradiance in W/m² but we need to get radiance
			Radiance = max( Radiance, 0.2 );

	return float4( Radiance, 1 );
}

// float4	PS( PS_IN _In ) : SV_TARGET0
// {
// //	return 1;
// //	return float4( _In.Normal, 1.0 );
// //	return float4( _In.UV, 0, 1.0 );
// //	return float4( _In.UV2, 1.0 );
// //	return float4( _In.UV2.xy, 0, 1.0 );
// 
// // 	return 0.25 * (1+_In.UV2.z);
// // 	return 10.0 * _TexLightMaps.SampleLevel( LinearClamp, float3( _In.UV, 1 ), 0.0 );
// 	return 5.0 * _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );	// Radiance
// 	return _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 ) / 6.0;	// MaterialID
// //	return _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );			// UVs
// //	return 0.2 * _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );	// Position
// //	return _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 );
// 
// 	float3	Normal = _TexLightMaps.SampleLevel( LinearClamp, _In.UV2, 0.0 ).xyz;
// 	return float4( 100.0 * abs( Normal - _In.Normal ), 0 );
// 
// // 	float4	Color = _TexLightMap.SampleLevel( LinearClamp, _In.UV, 0.0 );
// // 	return float4( Color.xyz, 1.0 );
// // 	return float4( lerp( float3( _In.UV, 0 ), Color, Color.x + Color.y ), 1.0 );
// }
