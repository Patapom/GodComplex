////////////////////////////////////////////////////////////////////////////////////////
// Volumetric Helpers
//
////////////////////////////////////////////////////////////////////////////////////////
#ifndef _VOLUMETRIC_INC_
#define _VOLUMETRIC_INC_

#define	USE_FAST_COS	// Use Taylor series instead of actual cosine

#define	ANIMATE
#define	BOX_BASE	4.0		// 8km (!!) (need to lower that but keep clouds' aspect)
#define	BOX_HEIGHT	2.0		// 4km thick
#define	PACK_R8				// Noise is packed in a R8 texture instead of R32F

static const float	EXTINCTION_COEFF = 8.0;
static const float	SCATTERING_COEFF = 8.0;

static const float	SUN_INTENSITY = 100.0;

static const float	WORLD2KM = 1.0;						// 1 World unit equals 0.5km


// static const float	FREQUENCY_MULTIPLIER_LOW = 0.25;	// Noise low frequency multiplier
// static const float	FREQUENCY_MULTIPLIER_HIGH = 1.5;	// Noise high frequency multiplier
static const float	FREQUENCY_MULTIPLIER_LOW = 3*0.25;	// Noise low frequency multiplier
static const float	FREQUENCY_MULTIPLIER_HIGH = 4*1.5;	// Noise high frequency multiplier


cbuffer	cbShadow	: register( b11 )
{
	float3		_LightDirection;
	float4x4	_World2Shadow;
	float4x4	_Shadow2World;
	float4x4	_World2TerrainShadow;
	float2		_ShadowZMinMax;
};


cbuffer	cbVolume	: register( b12 )
{
	float4		_VolumeParams;
}

Texture2DArray	_TexCloudTransmittance	: register(t5);
Texture2D		_TexTerrainShadow		: register(t6);
Texture3D		_TexFractal0			: register(t16);
Texture3D		_TexFractal1			: register(t17);


////////////////////////////////////////////////////////////////////////////////////////
// Fast analytical Perlin noise
float Hash( float n )
{
	return frac( sin(n) * 43758.5453 );
}

float FastNoise( float3 x )
{
	float3	p = floor(x);
	float3	f = frac(x);

	f = smoothstep( 0.0, 1.0, f );

	float	n = p.x + 57.0 * p.y + 113.0 * p.z;

	return lerp(	lerp(	lerp( Hash( n +   0.0 ), Hash( n +   1.0 ), f.x ),
							lerp( Hash( n +  57.0 ), Hash( n +  58.0 ), f.x ), f.y ),
					lerp(	lerp( Hash( n + 113.0 ), Hash( n + 114.0 ), f.x ),
							lerp( Hash( n + 170.0 ), Hash( n + 171.0 ), f.x ), f.y ), f.z );
}

// Fast analytical noise for screen-space perturbation
float	FastScreenNoise( float2 _XY )
{
	return Hash( 1.579849 * _XY.x - 2.60165409 * _XY.y )
		 * Hash( -1.3468489 * _XY.y + 2.31765 * _XY.x );
}

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

float	GetVolumeDensity( float3 _Position, float _MipBias )
{
//_Position.x += _Time.x;

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

	float	y = saturate( (_Position.y - BOX_BASE) / BOX_HEIGHT );	// That gives us a value in [0 (bottom), 1 (top)]

	float3	UVW0 = FREQUENCY_MULTIPLIER_LOW * float3( 0.01, 0.01, 0.01 ) * _Position;	// Very low frequency for the 32^3 noise
#ifdef	ANIMATE
	UVW0 += _Time.x * float3( 0.005, 0, -0.0125 );
#else
	UVW0 += float3( 0.005, 0, -0.13 );
#endif
//	float	Noise = _TexFractal0.SampleLevel( LinearWrap, 0.1 * UVW0, 4.0 ).x;
	float	Noise = _TexNoise3D.SampleLevel( LinearWrap, UVW0, 0*_MipBias ).x;	// Use small 32^3 noise (no need mip bias on that low freq noise anyway or we may lose the defining shape)

Noise *= sqrt(y);		// Goes to 0 at bottom
//Noise *= pow( y, 0.01 );	// Goes to 0 at bottom

//	float3	UVW1 = float3( 0.04, 0.04, 1.0 / BOX_HEIGHT ) * _Position.xzy;	// Low frequency for the high frequency noise
	float3	UVW1 = float3( FREQUENCY_MULTIPLIER_HIGH * 0.02, FREQUENCY_MULTIPLIER_HIGH * 0.02, 1.0 / BOX_HEIGHT ) * _Position.xzy;	// Low frequency for the high frequency noise
#ifdef	ANIMATE
	UVW1 += _Time.x * float3( 0.0, -0.01, 0.0 );	// Good
#endif

#ifdef	PACK_R8
	const float	Min = -0.15062222, Max = 0.16956991;
	float	Temp = _TexFractal1.SampleLevel( LinearWrap, UVW1, _MipBias ).x;	// Packed in [0,1]
			Temp = Min + (Max - Min) * Temp;	// Unpack in [Min,Max]
	float	Noise2 = Temp;
#else
	float	Noise2 = _TexFractal1.SampleLevel( LinearWrap, UVW1, _MipBias + _VolumeParams.x ).x;
#endif
	
//	Noise += 0.707 * (2.0 * (Noise2 - 0.02));	// Add detail
	Noise -= 0.707 * (1.0 * (Noise2 + 0.01));	// Add detail

//	Noise *= 4.0;

	y = 2.0 * y - 1.0;	// -1 at bottom, +1 at top
	float	TopY = 1-saturate(y);
			TopY *= TopY;
	float	BottomY = 1-saturate(-y);
			BottomY *= BottomY;

//	float3	HeightOffsets = float3( -0.005, 0.0, 0.1 );	// Bottom, Middle, Top offsets
//	float3	HeightOffsets = float3( 0.025, 0.05, 0.10 );	// Bottom, Middle, Top offsets
//	float3	HeightOffsets = float3( 0.0, +0.02, 0.1 );	// Bottom, Middle, Top offsets (Nice coverage!)
 	float3	HeightOffsets = -0.04 + float3( -0.01, +0.02, 0.1 );	// Bottom, Middle, Top offsets (Nice coverage!)
 	float	Offset = lerp( HeightOffsets.z, HeightOffsets.y, TopY ) + lerp( HeightOffsets.x, HeightOffsets.y, BottomY );

//float	Offset = -0.02;//###

	float	Contrast = 0.5;
	float	Gamma = 0.5;
	float	Density = pow( saturate( Contrast * (Noise + Offset) ), Gamma );

// 	Noise *= 2.0;
// 	float	Density = smoothstep( 0, 1, smoothstep( 0, 1, smoothstep( 0, 1, saturate( Noise + Offset ) ) ) );

	return Density;
//	return (1.0 - saturate(y)) * Density;	// Apply bevel
//	return sqrt(abs(y)) * Density;	// Apply bevel

#endif
}

////////////////////////////////////////////////////////////////////////////////////////
// Pre-integration
float	IntegrateExtinction( float _Sigma_t0, float _Sigma_t1, float _StepSize )
{
	return exp( -0.5 * (_Sigma_t0 + _Sigma_t1) * _StepSize );
}

// From http://mathworld.wolfram.com/Erf.html
float	Erf( float z )
{
	float	z2 = z*z;
	return 1.1283791670955125738961589031215 * z * (1.0 + z2 * ((-1.0/3.0) + z2 * ((1.0/10.0) + z2 * ((-1.0/42.0) + z2 * (1.0/216.0)))));
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

float	GetCloudTransmittance( float3 _WorldPosition )
{
	float3	ShadowPosition = mul( float4( _WorldPosition, 1.0 ), _World2Shadow ).xyz;
	float2	UV = ShadowPosition.xy;
	float	Z = ShadowPosition.z;

	float4	C0 = _TexCloudTransmittance.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
	float4	C1 = _TexCloudTransmittance.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );

	float2	ZMinMax = C1.zw;
	if ( Z < ZMinMax.x )
		return 1.0;	// We're not even in the shadow yet!

	float	x = saturate( (Z - ZMinMax.x) / (ZMinMax.y - ZMinMax.x) );

	const float4	CosTerm0 = PI * float4( 0, 1, 2, 3 );
	const float2	CosTerm1 = PI * float2( 4, 5 );

#ifndef USE_FAST_COS
	float4	Temp0 = cos( float4( CosTerm0.yzw, CosTerm1.x) * x );
	float	Temp1 = cos( CosTerm1.y * x );
// 	float4	Cos0 = float4( 0.5, Temp0.xyz );
// 	float2	Cos1 = float2( Temp0.w, Temp1 );
	float4	Cos0 = float4( 1.0, Temp0.xyz );
	float2	Cos1 = float2( Temp0.w, Temp1 );
#else
	float4	Temp0 = FastCos( float4( CosTerm0.yzw, CosTerm1.x) * x );
	float	Temp1 = FastCos( CosTerm1.y * x );
	float4	Cos0 = float4( 0.5, Temp0.xyz );
	float2	Cos1 = float2( Temp0.w, Temp1 );
#endif

	return saturate( dot( Cos0, C0 ) + dot( Cos1, C1.xy ) );
}

// This function assumes we're standing below the cloud and thus get the full extinction
float	GetFastCloudTransmittance( float3 _WorldPosition )
{
	float3	ShadowPosition = mul( float4( _WorldPosition, 1.0 ), _World2Shadow ).xyz;
//	float2	UV = float2( 0.5 * (1.0 + ShadowPosition.x), 0.5 * (1.0 - ShadowPosition.y) );
	float2	UV = ShadowPosition.xy;

	float4	C0 = _TexCloudTransmittance.SampleLevel( LinearClamp, float3( UV, 0 ), 0.0 );
return C0.x - C0.y + C0.z - C0.w;	// Skip smaller coefficients... No need to tap further.
	float4	C1 = _TexCloudTransmittance.SampleLevel( LinearClamp, float3( UV, 1 ), 0.0 );
	return C0.x - C0.y + C0.z - C0.w + C1.x - C1.y;
}

float	GetTerrainShadow( float3 _Position )
{
	float4	PositionProj = mul( float4( _Position, 1.0 ), _World2TerrainShadow );
//			PositionProj /= PositionProj.w;
	float2	UV = float2( 0.5 * (1.0 + PositionProj.x), 0.5 * (1.0 - PositionProj.y) );

	float	Zproj = _TexTerrainShadow.SampleLevel( LinearClamp, UV, 0.0 ).x;

return 0.001+Zproj > PositionProj.z ? 1.0 : 0.0;

	return saturate( -100.0 * (PositionProj.z - Zproj) );
}

#endif	// _VOLUMETRIC_INC_
