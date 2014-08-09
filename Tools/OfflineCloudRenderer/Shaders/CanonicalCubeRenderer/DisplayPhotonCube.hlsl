////////////////////////////////////////////////////////////////////////////////
// Displays the photons on a cube
////////////////////////////////////////////////////////////////////////////////
#include "../Global.hlsl"

cbuffer	cbRender : register(b8)
{
	float4		_Dimensions;		// XY=Dimensions of the render target, ZW=1/XY
	float4		_DEBUG;
	float		_FluxMultiplier;
	uint		_SplatType;
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
	float3	Data0 = _TexPhotons.Sample( LinearClamp, _In.UVW + float3( 0, 0, 6*0 ) ).xyz;
	float3	Data1 = _TexPhotons.Sample( LinearClamp, _In.UVW + float3( 0, 0, 6*1 ) ).xyz;
	float3	Data2 = _TexPhotons.Sample( LinearClamp, _In.UVW + float3( 0, 0, 6*2 ) ).xyz;
	float4	Data3 = _TexPhotons.Sample( LinearClamp, _In.UVW + float3( 0, 0, 6*3 ) );

	float3	ExitPosition = Data0;
	float3	ExitDirection = Data1;
	float	MarchedLength = Data2.x;
	float	ScatteringEventsCount = Data2.y;
	float	Intensity = Data3.x;

	float4	Color = 0.0;
	switch ( _SplatType )
	{
	// Positive
	case 0: Color = float4( ExitPosition, 0 ); break;
	case 1: Color = float4( ExitDirection, 0 ); break;

	// Negative
	case 16+0: Color = float4( -ExitPosition, 0 ); break;
	case 16+1: Color = float4( -ExitDirection, 0 ); break;

	// Absolute
	case 32+0: Color = float4( abs(ExitPosition), 0 ); break;
	case 32+1: Color = float4( abs(ExitDirection), 0 ); break;

	case 2: Color = 0.01 * ScatteringEventsCount; break;

	case 3: Color = 1e-3 * _FluxMultiplier * MarchedLength; break;

	case 4: Color = _FluxMultiplier * Intensity; break;
	}

	return Color;
}
