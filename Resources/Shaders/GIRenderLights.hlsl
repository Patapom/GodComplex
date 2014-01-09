//////////////////////////////////////////////////////////////////////////
// This shader renders lights as luminous balls
//
#include "Inc/Global.hlsl"
#include "Inc/GI.hlsl"

struct	VS_IN
{
	float3	Position	: POSITION;
 	float3	Normal		: NORMAL;

	uint	InstanceID	: SV_INSTANCEID;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float3	Color		: COLOR;
};

PS_IN	VS( VS_IN _In )
{
	LightStruct	Light = _SBLightsDynamic[_In.InstanceID];

	float4	WorldPosition = float4( Light.Position + Light.Parms.y * _In.Position, 1.0 );
	if ( Light.Parms.y < 0.0 )
//		WorldPosition.xyz = 1.5 * saturate( dot( _In.Position, Light.Position ) ) * Light.Position;
		WorldPosition.xyz = _In.Position * lerp( 0.001, 0.1, saturate( dot( _In.Position, Light.Position ) ) );

	PS_IN	Out;
	Out.__Position = mul( WorldPosition, _World2Proj );
//	Out.Color = Light.Color * pow( lerp( 0.1, 1.0, dot( _In.Normal, normalize( _Camera2World[3].xyz - WorldPosition.xyz ) ) ), 8.0 );
//	Out.Color = Light.Color * lerp( 0.005, 1.0, pow( dot( _In.Normal, _Camera2World[2].xyz ), 8.0 ) );
	Out.Color = Light.Color / max( 1.0, max( max( Light.Color.x, Light.Color.y ), Light.Color.z ) ) * lerp( 0.5, 1.2, pow( saturate( dot( _In.Normal, normalize( _Camera2World[3].xyz - WorldPosition.xyz ) ) ), 0.8 ) );

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return float4( _In.Color, 0 );
}
