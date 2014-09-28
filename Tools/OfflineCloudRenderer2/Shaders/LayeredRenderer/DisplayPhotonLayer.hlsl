////////////////////////////////////////////////////////////////////////////////
// Displays the photons on a horizontal layer
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"

cbuffer	cbRender : register(b8)
{
	float3		_CloudScapeSize;
	uint		_LayersCount;
	uint		_StartLayerIndex;
	float		_IntensityFactor;		// Multiplier for display intensity
	uint		_DisplayType;			// 0=Flux, 1=Directions, 2=Density, bit #3 for normalization by weight
}

Texture3D<float4>	_TexPhotons_Flux : register(t0);
Texture3D<float4>	_TexPhotons_Direction : register(t1);
Texture3D<float>	_TexDensityField : register(t2);

struct VS_IN
{
	float4	__Position : SV_POSITION;
	uint	LayerIndex : SV_INSTANCEID;
};

struct PS_IN
{
	float4	__Position	: SV_POSITION;
	uint	LayerIndex	: LAYER_INDEX;
	float2	UV			: TEXCOORDS0;
};

PS_IN	VS( VS_IN _In )
{
	uint	LayerIndex = _StartLayerIndex + _In.LayerIndex;

	float3	WorldPosition = float3( _In.__Position.x, 1.0 - (float) LayerIndex / _LayersCount, _In.__Position.y );

	WorldPosition *= 2.0;

	PS_IN	Out;
	Out.__Position = mul( float4( WorldPosition, 1.0 ), _World2Proj );
	Out.UV = 0.5 * (1.0 + float2( _In.__Position.x, -_In.__Position.y ));
	Out.LayerIndex = LayerIndex;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	float3	UVW = float3( _In.UV, (0.5+_In.LayerIndex) / (_LayersCount+1) );

//	float4	FluxWeight = _TexPhotons_Flux.Sample( PointClamp, UVW );
//	float3	Direction = _TexPhotons_Direction.Sample( PointClamp, UVW ).xyz;
	float4	FluxWeight = _TexPhotons_Flux.Sample( LinearWrap, UVW );
	float3	Direction = _TexPhotons_Direction.Sample( LinearWrap, UVW ).xyz;
	float	Density = _TexDensityField.Sample( LinearWrap, UVW );

	if ( _DisplayType & 4 )
	{
		float	Normalizer = FluxWeight.w > 1.0 ? 1.0 / FluxWeight.w : 1.0;
		FluxWeight.xyz *= Normalizer;
		Direction *= Normalizer;
	}

	uint	DisplayType = _DisplayType & 3;
	if ( DisplayType == 0 )
		return float4( _IntensityFactor * FluxWeight.xyz, 1.0 );
	else if ( DisplayType == 1 )
		return float4( _IntensityFactor * abs(Direction), 1.0 );
	else
		return Density;
}
