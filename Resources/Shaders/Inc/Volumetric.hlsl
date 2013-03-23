////////////////////////////////////////////////////////////////////////////////////////
// Volumetric Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _VOLUMETRIC_INC_
#define _VOLUMETRIC_INC_

//#define	USE_FAST_COS	// Use Taylor series instead of actual cosine
							// After testing, it takes more time to user Taylor series!! ^^

static const float	EXTINCTION_COEFF = 20.0;
static const float	SCATTERING_COEFF = 20.0;


cbuffer	cbShadow	: register( b11 )
{
	float3		_LightDirection;
	float4x4	_World2Shadow;
	float4x4	_Shadow2World;
	float2		_ShadowZMax;
};

Texture2DArray	_TexTransmittanceMap	: register(t11);
Texture3D		_TexFractal0	: register(t16);
Texture3D		_TexFractal1	: register(t17);


////////////////////////////////////////////////////////////////////////////////////////
// Volume density
float4	fbm( float3 _UVW, float _AmplitudeFactor, float _FrequencyFactor )
{
	float	Amplitude = _AmplitudeFactor;

#if 0
	float4	V  =			 _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
#elif 1
	float4	V  = abs( _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 ) );	_UVW *= _FrequencyFactor;
			V += abs( Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 ) );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += abs( Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 ) );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += abs( Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 ) );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += abs( Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 ) );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += abs( Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 ) );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
#else
	float4	V  = 1.0 / 			   _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;
			V += 1.0 / Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += 1.0 / Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += 1.0 / Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += 1.0 / Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
			V += 1.0 / Amplitude * _TexNoise3D.SampleLevel( LinearWrap, _UVW, 0.0 );	_UVW *= _FrequencyFactor;	Amplitude *= _AmplitudeFactor;
#endif

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
	//====================================================
	// Use 6 octaves of noise
	float3	UVW = 0.00125 * _Position;

//	float	Noise = fbm( UVW, 0.5, 2.0 ).x;
	float	Noise = fbm( UVW, 0.707, 3.0 ).x;
			Noise *= Noise;
			Noise *= 4.0;

	return saturate( Noise - 0.27 );

#elif 0
	//====================================================
	// Use a low frequency texture perturbed by a scrolling high frequency texture

//	float3	UVW = 0.025 * _Position;
//	float3	UVW = 0.0125 * _Position;	// Good
	float3	UVW = 0.0 + 0.0125 * _Position;	// Good

//UVW.y -= 0.1 * _Time.x;
//UVW.y -= 0.1 * _Time.x + sin( 0.1 * (UVW.x + _Time.x) );
	float	Noise = _TexFractal0.SampleLevel( LinearWrap, UVW + 0*float3( 0, 0, 0.043 * _Time.x ), 0.0 ).x;
//	float	Noise = sin( UVW.x ) * cos( UVW.z );
//	float	Noise = _TexNoise3D.SampleLevel( LinearWrap, 0.5 * UVW, 0.0 ).x;

//	UVW *= 2.957;	UVW.x += 0.5 * _Time.x;
	UVW *= 3.71321;		UVW.z -= 0.1 * _Time.x;	// Good
//	UVW *= 6.71321;		UVW.z -= 0.05 * _Time.x;	// Good
			Noise += 0.707 * _TexFractal1.SampleLevel( LinearWrap, UVW, 0.0 ).x;
//			Noise *= 16.0 * _TexFractal1.SampleLevel( LinearWrap, UVW, 0.0 ).x;
			Noise *= 4.0;
			Noise *= Noise;

//float	y = 0.5 * (_Position.y+1.0);
//float	y = _Position.y+0.5;
float	y = 0.5 * _Position.y;
//float	Offset = lerp( -0.02, 0.0, y );
float	Offset = lerp( -0.25, -0.025, y );	// FBM
//float	Offset = lerp( -5.0, -4.5, y );	// Cellular

	return saturate( Noise + Offset );

#else
	//====================================================
	// Use a low frequency texture perturbed by a scrolling high frequency texture
	// This time, the high frequency texture is much larger in XZ then in Y (thin slab) so tiling is not visible

	float3	UVW0 = 0.25 * (float3( 0.01, 0.05, 0.01 ) * _Position + float3( 0, 0, -0.05 * _Time.x ));	// Very low frequency for the 32^3 noise
//	float	Noise = _TexFractal0.SampleLevel( LinearWrap, 0.1 * UVW0, 4.0 ).x;
	float	Noise = _TexNoise3D.SampleLevel( LinearWrap, UVW0, 0.0 ).x;

	float3	UVW1 = 0.0 + float3( 0.02, 0.02, 0.5 ) * _Position.xzy;	// Low frequency for the high frequency noise
			UVW1.z -= 0.05 * _Time.x;	// Good
	Noise += 0.707 * _TexFractal1.SampleLevel( LinearWrap, UVW1, 0.0 ).x;
//	Noise *= 4.0;
//	Noise *= Noise;

	float	y = _Position.y - 1.0;	// Slab is in [0,2] so that gives us a value in [-1 (bottom), 1 (top)]
	float	TopY = 1-saturate(y);
			TopY *= TopY;
	float	BottomY = 1-saturate(-y);
			BottomY *= BottomY;

	float3	HeightOffsets = float3( -0.005, 0.0, 0.1 );	// Bottom, Middle, Top offsets
//	float3	HeightOffsets = float3( 0.105, 0.05, 0.05 );	// Bottom, Middle, Top offsets
	float	Offset = lerp( HeightOffsets.z, HeightOffsets.y, TopY ) + lerp( HeightOffsets.x, HeightOffsets.y, BottomY );

	float	Density = saturate( Noise + Offset );
	return (1.0 - saturate(y)) * Density;	// Apply bevel

#endif
}

////////////////////////////////////////////////////////////////////////////////////////
// Pre-integration
float	IntegrateExtinction( float _Sigma_t0, float _Sigma_t1, float _StepSize )
{
	return exp( -0.5 * (_Sigma_t0 + _Sigma_t1) * _StepSize );
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
