//////////////////////////////////////////////////////////////////////////
// This shader downsamples the color buffer
//
#include "Inc/Global.hlsl"


cbuffer	cbSplat	: register( b10 )
{
	float3		_dUV;
};

Texture2D<float4>	_TexSourceImage : register( t10 );

struct	VS_IN
{
	float4	__Position	: SV_POSITION;
};

struct	PS_IN
{
	float4	__Position	: SV_POSITION;
	float4	UV0			: UV0;
	float4	UV1			: UV1;
};


PS_IN	VS( VS_IN _In )
{
	PS_IN	Out;
	Out.__Position = _In.__Position;

	float2	UV = 0.5 * float2( 1.0 + _In.__Position.x, 1.0 - _In.__Position.y );
	Out.UV0.xy = UV + float2(-1.0f, -1.0f) * _dUV.xy;
	Out.UV0.zw = UV + float2( 1.0f, -1.0f) * _dUV.xy;
	Out.UV1.xy = UV + float2(-1.0f,  1.0f) * _dUV.xy;
	Out.UV1.zw = UV + float2( 1.0f,  1.0f) * _dUV.xy;

	return Out;
}

float4	PS( PS_IN _In ) : SV_TARGET0
{
#if TYPE == 0 //MIN_G16R16
	float4	s01, s02;
	s01.xy = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.xy , 0.0).rg;
	s01.zw = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.zw , 0.0).rg;
	s02.xy = _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.xy, 0.0).rg;
	s02.zw = _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.zw, 0.0).rg;
	float4	s0102 = min(s01,s02);
	float2	smin = min(s0102.xy,s0102.zw);
	return smin.xyxy;

#elif TYPE == 1	//MAX_ARGB
	float4	s01 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.xy , 0.0);
	float4	s02 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.zw , 0.0);
	float4	s03 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.xy, 0.0);
	float4	s04 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.zw, 0.0);
	float4	s0102 = max(s01,s02);
	float4	s0304 = max(s03,s04);
	float4	smax = max(s0102,s0304);
	sample = smax;

#elif TYPE == 2 //MIN_ARGB
	float4	s01 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.xy , 0.0);
	float4	s02 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.zw , 0.0);
	float4	s03 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.xy, 0.0);
	float4	s04 = _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.zw, 0.0);
	float4	s0102 = min(s01,s02);
	float4	s0304 = min(s03,s04);
	return min(s0102,s0304);

#elif TYPE == 3	//AVG_ARGB
	float4	sample  = _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.xy, 0.0);
			sample += _TexSourceImage.SampleLevel( LinearClamp, _In.UV0.zw , 0.0);
			sample += _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.xy, 0.0);
			sample += _TexSourceImage.SampleLevel( LinearClamp, _In.UV1.zw, 0.0);
	return 0.25 * sample;

#endif
}
