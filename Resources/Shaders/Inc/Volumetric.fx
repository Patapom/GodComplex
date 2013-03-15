////////////////////////////////////////////////////////////////////////////////////////
// Volumetric Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _VOLUMETRIC_INC_
#define _VOLUMETRIC_INC_

static const float	SCATTERING_COEFF = 10.0;


cbuffer	cbShadow	: register( b11 )
{
	float4x4	_World2Shadow;
	float4x4	_Shadow2World;
	float2		_ShadowZMax;
};

Texture2DArray	_TexTransmittanceMap	: register(t11);


////////////////////////////////////////////////////////////////////////////////////////
// Volume density
float4	fbm( float3 _UVW, float _AmplitudeFactor, float _FrequencyFactor )
{
	float	Amplitude = _AmplitudeFactor;

	float4	V  =			 _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;

	return V;
}

float	GetVolumeDensity( float3 _Position )
{
	float3	UVW = 0.1 * _Position;
	float	Noise = fbm( UVW, 0.5, 2.0 ).x;
			Noise *= Noise;

	return saturate( Noise + 0.02 );
}

////////////////////////////////////////////////////////////////////////////////////////
// Shadow Mapping
float4	FastCos( float4 _Angle )
{
	float4	x2 = _Angle * _Angle;
// 	float4	x4 = x2 * x2;
// 	float4	x6 = x4 * x2;
//	return 1.0 - 0.5 * x2 + 0.04166666666666666666666666666667 * x4 - 0.00833333333333333333333333333333 * x6;
	return 1.0 + x2 * (- 0.5 + x2 * (0.04166666666666666666666666666667 - x2 * 0.00833333333333333333333333333333));
}

float	GetTransmittance( float3 _WorldPosition )
{
//	float3	ShadowPosition = mul( float4( _WorldPosition, 1.0 ), _World2Shadow ).xyz;
float3	ShadowPosition = _WorldPosition;
	float2	UV = float2( 0.5 * (1.0 + ShadowPosition.x), 0.5 * (1.0 - ShadowPosition.y) );
	float	Z = ShadowPosition.z;

	float4	C0 = _TexTransmittanceMap.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	C1 = _TexTransmittanceMap.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );

	float	x = Z * _ShadowZMax.y;
			x = 1.0 + 2.0 * x;

	const float4	CosTerm0 = HALFPI / 8.0 * float4( 0, 1, 2, 3 );	// /8 is there because we use 8 coefficients
	const float4	CosTerm1 = HALFPI / 8.0 * float4( 4, 5, 6, 7 );

	float4	Cos0 = cos( CosTerm0 * x );
	float4	Cos1 = cos( CosTerm1 * x );

	return dot( Cos0, 1.0 ) + dot( Cos1, 1.0 );
}

#endif	// _VOLUMETRIC_INC_
