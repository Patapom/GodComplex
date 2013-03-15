////////////////////////////////////////////////////////////////////////////////////////
// Volumetric Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _VOLUMETRIC_INC_
#define _VOLUMETRIC_INC_

static const float	INFINITY = 1e6;

////////////////////////////////////////////////////////////////////////////////////////
// 
float4	fbm( float3 _Position, float _AmplitudeFactor, float _FrequencyFactor )
{
	float3	UVW = _Position;
	float	Amplitude = _AmplitudeFactor;

	float4	V  =			 _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );	UVW *= _FrequencyFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );	UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );	UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );	UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );	UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, UVW, 0.0 );	UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;

	return V;
}


#endif	// _VOLUMETRIC_INC_
