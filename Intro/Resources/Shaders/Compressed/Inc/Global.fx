////////////////////////////////////////////////////////////////////////////////////////
// Global stuff
////////////////////////////////////////////////////////////////////////////////////////
//
// WARNING! These must correspond to the resolution set in GodComplex.h !
static const float	RESX = 1280.0;
static const float	RESY = 720.0;
static const float	ASPECT_RATIO = RESX / RESY;
static const float	INV_ASPECT_RATIO = RESY / RESX;
static const float2	SCREEN_SIZE = float2( RESX, RESY );
static const float2	INV_SCREEN_SIZE = float2( 1.0/RESX, 1.0/RESY );

static const float	PI = 3.1415926535897932384626433832795;			// ...
static const float	TWOPI = 6.283185307179586476925286766559;		// 2PI
static const float	HALFPI = 1.5707963267948966192313216916398;		// PI/2
static const float	INVPI = 0.31830988618379067153776752674503;		// 1/PI
static const float	INVHALFPI = 0.63661977236758134307553505349006;	// 1/(PI/2)
static const float	INVTWOPI = 0.15915494309189533576888376337251;	// 1/2PI

static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2� observer (cf. http://wiki.patapom.com/index.php/Colorimetry)


#define TEX( Texture, Sampler, UV )					Texture.Sample( Sampler, UV )

// On old ATIs, the SampleLevel() function doesn't work so you should use the other implementation (although I'm pretty sure it will fuck everything up if you start sampling textures within conditional branches)
#define TEXLOD( Texture, Sampler, UV, MipLevel )	Texture.SampleLevel( Sampler, UV, MipLevel )
// #define TEXLOD( Texture, Sampler, UV, MipLevel )	Texture.Sample( Sampler, UV )


////////////////////////////////////////////////////////////////////////////////////////
// Samplers
SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );


////////////////////////////////////////////////////////////////////////////////////////
// Constants

//[ // Minifier doesn't'support cbuffers !
cbuffer	cbCamera	: register( b0 )
{
	float4		_CameraData;
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Camera2Proj;
	float4x4	_ProjCamera;
	float4x4	_World2Proj;
	float4x4	_Proj2World;
};

cbuffer	cbGlobal	: register( b1 )
{
	float4		_Time;		// X=time Y=DeltaTime Z=1/Time W=1/DeltaTime
};
//]


Texture3D	_TexNoise3D	: register(t0);


////////////////////////////////////////////////////////////////////////////////////////
// Distort position with noise
// float3	Distort( float3 _Position, float3 _Normal, float4 _NoiseOffset )
// {
// 	float	Noise = _NoiseOffset.w * (-1.0 + TEXLOD( _TexNoise3D, LinearWrap, 0.2 * (_Position + _NoiseOffset.xyz), 0.0 ).x);
// 	return	_Position + Noise * _Normal;
// }

float3	Distort( float3 _Position, float3 _Normal, float4 _NoiseOffset )
{
	return _Position + _NoiseOffset.w * TEXLOD( _TexNoise3D, LinearWrap, 0.2 * (_Position + _NoiseOffset.xyz), 0.0 ).xyz;
}


////////////////////////////////////////////////////////////////////////////////////////
// Standard bilinear interpolation on a quad
//
//	a ---- d --> U
//	|      |
//	|      |
//	|      |
//	b ---- c
//  :
//  v V
//
#define BILERP( a, b, c, d, uv )	lerp( lerp( a, d, uv.x ), lerp( b, c, uv.x ), uv.y )


////////////////////////////////////////////////////////////////////////////////////////
// Rotates a vector about an axis
// float3	RotateVector( float3 v, float3 _Axis, float _Angle )
// {
//     _Axis = normalize( _Axis );
//     float3	n = _Axis * dot( _Axis, v );
// 	float2	SC;
// 	sincos( _Angle, SC.x, SC.y );
//     return n + SC.y * (v - n) + SC.x * cross( _Axis, v );
// }

float3	RotateVector( float3 _Vector, float3 _Axis, float _Angle )
{
	float2	SinCos;
	sincos( _Angle, SinCos.x, SinCos.y );

	float3	Result = _Vector * SinCos.y;
	float	temp = dot( _Vector, _Axis );
			temp *= 1.0 - SinCos.y;

	Result += _Axis * temp;

	float3	Ortho = cross( _Axis, _Vector );

	Result += Ortho * SinCos.x;

	return Result;
}
