//////////////////////////////////////////////////////////////////////////
// This shader applies indirect lighting and finalizes rendering
//
#include "Inc/Global.hlsl"
#include "Inc/LayeredMaterials.hlsl"

cbuffer	cbRender	: register( b10 )
{
	float3		_dUV;
	float3		_MainLightDirection;	// Main light direction
};

//////////////////////////////////////////////////////////////////////////
//
struct	VS_IN
{
	float3	Position	: POSITION;
	uint	VertexID	: SV_INSTANCEID;
};

struct	GS_IN
{
	float2	UV				: TEXCOORD0;
	float4	Color			: COLOR;
	float	Intensity		: INTENSITY;
	float	CoC				: RADIUS;
};

struct	PS_IN
{
	float4	__Position		: SV_POSITION;
	float2	UV				: TEXCOORD0;
	float4	Color			: COLOR;
};

Texture2D			_TexInput		: register(t16);	// Original image
Texture2D			_TexBokeh		: register(t11);	// Our nice bokeh
//Texture2D			_TexHiPixels	: register(t11);	// The downsampled map containing high-intensity pixels
Texture2D			_TexDepth		: register(t12);	// Depth

GS_IN	VS( VS_IN _In )
{
	float	DOWNSAMPLED_RESX = 0.25 * RESX;
	float	PixelX = fmod( _In.VertexID, DOWNSAMPLED_RESX );
	float	PixelY = floor( _In.VertexID / DOWNSAMPLED_RESX );
	float2	SourceUV = float2( PixelX, PixelY ) * _dUV.xy;

	float3	PixelInfos = _TexInput.SampleLevel( PointClamp, SourceUV, 2.0 ).xyz;
	float	Intensity = PixelInfos.x > 0.15 ? PixelInfos.x : -1.0;

	float2	UV = 0.0;
	float3	PixelColor = 0.0;
	float	CoC = 0.0;
	if ( Intensity > 0.0 )
	{	// Is the pixel interesting?
		UV = PixelInfos.yz;

		PixelColor = _TexInput.SampleLevel( PointClamp, UV, 0.0 ).xyz;

		float	Z = _TexDepth.SampleLevel( LinearClamp, UV, 0.0 ).x;

		CoC = 0.02;//0.00001 * Z;
	}

	// Assign
	GS_IN	Out;
	Out.UV = UV;
	Out.Color = float4( 0.05 * PixelColor, 0.0 );
	Out.Intensity = Intensity;
	Out.CoC = CoC;

	return Out;
}

[maxvertexcount( 4 )]
void	GS( point GS_IN _In[1], inout TriangleStream<PS_IN> _OutStream )
{
	if ( _In[0].Intensity < 0.0 )
		return;

	float	CoC = _In[0].CoC;

	float4	PositionProj = float4( 2.0 * _In[0].UV.x - 1.0, 1.0 - 2.0 * _In[0].UV.y, 0.0, 1.0 );
	float3	DeltaProj = float3( 2.0 * CoC, -2.0 * CoC, 0.0 );
	float4	Ratio = float4( 1.0, _CameraData.x / _CameraData.y, 0, 0 );

	PS_IN	Out;
	Out.__Position = PositionProj + Ratio * DeltaProj.yyzz;
	Out.UV = float2( 0, 0 );
	Out.Color = _In[0].Color;
	_OutStream.Append( Out );

	Out.__Position = PositionProj + Ratio * DeltaProj.yxzz;
	Out.UV = float2( 0, 1 );
	_OutStream.Append( Out );

	Out.__Position = PositionProj + Ratio * DeltaProj.xyzz;
	Out.UV = float2( 1, 0 );
	_OutStream.Append( Out );

	Out.__Position = PositionProj + Ratio * DeltaProj.xxzz;
	Out.UV = float2( 1, 1 );
	_OutStream.Append( Out );

//	_OutStream.RestartStrip();
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
// 	return 1.0;
// 	return float4( _In.Color, 1.0 );
	return _In.Color * _TexBokeh.SampleLevel( LinearClamp, _In.UV, 0.0 );
}
