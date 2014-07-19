////////////////////////////////////////////////////////////////////////////////
// Displays the photons on a cube
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"

cbuffer	cbRender : register(b8)
{
	float4		_Dimensions;		// XY=Dimensions of the render target, ZW=1/XY
	float4		_DEBUG;
	float		_FluxMultiplier;
}

Texture2DArray<float4>	_TexPhotons : register(t0);

struct VS_IN
{
	float3	Position : POSITION;
	float3	UVW : NORMAL;
};

struct PS_IN
{
	float4	__Position : SV_POSITION;
	float3	UVW : TEXCOORDS0;
};

PS_IN	VS( VS_IN _In )
{
	PS_IN	Out;
	Out.__Position = mul( float4( _In.Position, 1.0 ), _World2Proj );
	Out.UVW = _In.UVW;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
	return _FluxMultiplier * _TexPhotons.Sample( LinearClamp, _In.UVW );
	return float4( _In.UVW.xy, 0, 0 );
}
