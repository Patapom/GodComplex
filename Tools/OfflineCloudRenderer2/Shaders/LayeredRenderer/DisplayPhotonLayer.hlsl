////////////////////////////////////////////////////////////////////////////////
// Displays the photons on a horizontal layer
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"

cbuffer	cbRender : register(b8)
{
	float3		_CloudScapeSize;
	uint		_LayersCount;
}

Texture2DArray<float4>	_TexPhotons : register(t0);

struct VS_IN
{
	float4	__Position : SV_POSITION;
	uint	LayerIndex : SV_INSTANCEID;
};

struct PS_IN
{
	float4	__Position	: SV_POSITION;
	float2	UV			: TEXCOORDS0;
	uint	LayerIndex	: LAYER_INDEX;
};

PS_IN	VS( VS_IN _In )
{
	float3	WorldPosition = float3( _In.__Position_In.x, (float) _In.LayerIndex / _LayersCount, -_In.__Position_In.y );

	PS_IN	Out;
	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );
	Out.UV = 0.5 * (1.0 + float2( _In.__Position.x, -_In.__Position.y ));

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return _TexPhotons.Sample( LinearClamp, float3( _In.UV, _In.LayerIndex ) );
}
