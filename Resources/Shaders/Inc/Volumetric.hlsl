////////////////////////////////////////////////////////////////////////////////////////
// Volumetric Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _VOLUMETRIC_INC_
#define _VOLUMETRIC_INC_

//#define	USE_FAST_COS	// Use Taylor series instead of actual cosine
							// After testing, it takes more time to user Taylor series!! ^^

static const float	SCATTERING_COEFF = 50.0;


cbuffer	cbShadow	: register( b11 )
{
	float3		_LightDirection;
	float4x4	_World2Shadow;
	float4x4	_Shadow2World;
	float2		_ShadowZMax;
};

Texture2DArray	_TexTransmittanceMap	: register(t11);
Texture3D		_TexFractal	: register(t16);


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
//_Position.x += _Time.x;

// Hardcoded sphere
// float3	Center = float3( 0.0, 2.5, 0.0 );
// float	Radius = 0.4;
// float	Distance = length( _Position - Center );
// return Distance < Radius ? 1 : 0;


#if 0
	float3	UVW = 0.05 * _Position;

	float	Noise = fbm( UVW, 0.5, 2.0 ).x;
			Noise *= Noise;
			Noise *= 4.0;

	return saturate( Noise - 0.07 );
#else

//	float3	UVW = 0.025 * _Position;
	float3	UVW = 0.125 * _Position;

//UVW.y -= 0.1 * _Time.x;
//UVW.y -= 0.1 * _Time.x + sin( 0.1 * (UVW.x + _Time.x) );

	float	Noise = _TexFractal.SampleLevel( LinearWrap, UVW, 0.0 ).x;
			Noise *= 2.0;
			Noise *= Noise;

float	y = 0.5 * (_Position.y+1.0);
float	Offset = lerp( -0.02, 0.0, y );

	return saturate( Noise + Offset );
#endif
}

////////////////////////////////////////////////////////////////////////////////////////
// Shadow Mapping
float	FastCos( float _Angle )
{
	_Angle = fmod( _Angle + PI, TWOPI ) - PI;
	float	x2 = _Angle * _Angle;
	return 1.0 + x2 * (-0.5 + x2 * ((1.0/24.0) + x2 * (-(1.0/720.0) + x2 * (1.0/40320.0))));
}
float2	FastCos( float2 _Angle )
{
	_Angle = fmod( _Angle + PI, TWOPI ) - PI;
	float2	x2 = _Angle * _Angle;
	return 1.0 + x2 * (-0.5 + x2 * ((1.0/24.0) + x2 * (-(1.0/720.0) + x2 * (1.0/40320.0))));
}
float4	FastCos( float4 _Angle )
{
	_Angle = fmod( _Angle + PI, TWOPI ) - PI;
	float4	x2 = _Angle * _Angle;
	return 1.0 + x2 * (-0.5 + x2 * ((1.0/24.0) + x2 * (-(1.0/720.0) + x2 * (1.0/40320.0))));
}

float	GetTransmittance( float3 _WorldPosition )
{
	float3	ShadowPosition = mul( float4( _WorldPosition, 1.0 ), _World2Shadow ).xyz;
	float2	UV = float2( 0.5 * (1.0 + ShadowPosition.x), 0.5 * (1.0 - ShadowPosition.y) );
	float	Z = ShadowPosition.z;

	float4	C0 = _TexTransmittanceMap.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	C1 = _TexTransmittanceMap.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );

	float2	ZMinMax = C1.zw;
	if ( Z < ZMinMax.x )
		return 1.0;	// We're not even in the shadow yet!

	float	x = saturate( (Z - ZMinMax.x) / (ZMinMax.y - ZMinMax.x) );

	const float4	CosTerm0 = PI * float4( 0, 1, 2, 3 );
	const float2	CosTerm1 = PI * float2( 4, 5 );

#ifndef USE_FAST_COS
	float4	Temp0 = cos( float4( CosTerm0.yzw, CosTerm1.x) * x );
	float	Temp1 = cos( CosTerm1.y * x );
	float4	Cos0 = float4( 0.5, Temp0.xyz );
	float2	Cos1 = float2( Temp0.w, Temp1 );
#else
	float4	Temp0 = FastCos( float4( CosTerm0.yzw, CosTerm1.x) * x );
	float	Temp1 = FastCos( CosTerm1.y * x );
	float4	Cos0 = float4( 0.5, Temp0.xyz );
	float2	Cos1 = float2( Temp0.w, Temp1 );
#endif

	return saturate( dot( Cos0, C0 ) + dot( Cos1, C1.xy ) );
}

#endif	// _VOLUMETRIC_INC_
