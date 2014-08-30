////////////////////////////////////////////////////////////////////////////////
// Displays the photons on a horizontal layer
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"

cbuffer	cbRender : register(b8)
{
	float3		_CloudScapeSize;
	uint		_LayersCount;
	float		_IntensityFactor;		// Multiplier for display intensity
	uint		_DisplayType;			// 0=Flux, 1=Directions, bit #2 for normalization by weight
}

Texture3D<float4>	_TexPhotons_Flux : register(t0);
Texture3D<float4>	_TexPhotons_Direction : register(t1);

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
	float3	WorldPosition = float3( _In.__Position.x, 1.0 - (float) _In.LayerIndex / _LayersCount, _In.__Position.y );

	PS_IN	Out;
	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );
	Out.UV = 0.5 * (1.0 + float2( _In.__Position.x, -_In.__Position.y ));
	Out.LayerIndex = _In.LayerIndex;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	float3	UVW = float3( _In.UV, (0.5+_In.LayerIndex) / (_LayersCount+1) );
//	float3	UVW = float3( _In.UV, 0.25 );

//	float4	FluxWeight = _TexPhotons_Flux.Sample( PointClamp, UVW );
//	float3	Direction = _TexPhotons_Direction.Sample( PointClamp, UVW ).xyz;
	float4	FluxWeight = _TexPhotons_Flux.Sample( LinearWrap, UVW );
	float3	Direction = _TexPhotons_Direction.Sample( LinearWrap, UVW ).xyz;

	if ( _DisplayType & 2 )
	{
		float	Normalizer = FluxWeight.w > 1.0 ? 1.0 / FluxWeight.w : 1.0;
		FluxWeight.xyz *= Normalizer;
		Direction *= Normalizer;
	}

	return float4( _IntensityFactor * ((_DisplayType & 1) == 0 ? FluxWeight.xyz : abs(Direction)), 1.0 );
}
