/////////////////////////////////////////////////////////////////////////////////////////////////////
// Global Definitions
//
#ifndef __GLOBAL_INCLUDED
#define __GLOBAL_INCLUDED

#define PI		3.1415926535897932384626433832795
#define INVPI	0.31830988618379067153776752674503
#define SQRT2	1.4142135623730950488016887242097

#define TAN_HALF_FOV	0.6		// tan( vertical FOV / 2 ) with vertical FOV = 90°
#define Z_FAR			100.0	// 100m max encoded in the depth buffer

cbuffer CB_Main : register(b0) {
	float2		_resolution;	// viewport resolution (in pixels)
	float		_time;
	float		_deltaTime;

	float4		_debugValues;

	float4		_mouseUVs;		// XY=Current Mouse UV, ZW=Referenced Mouse UV (set when alt+clicking the screen)

	uint		_flags;
	uint		_framesCount;
	uint		_debugMipIndex;
	float		_environmentIntensity;

	float		_sunIntensity;
	float		_forcedAlbedo;
	float		_coneAngleBias;
	float		_exposure;
};

cbuffer CB_Camera : register(b1) {
	float4x4	_camera2World;
	float4x4	_world2Camera;
	float4x4	_proj2World;
	float4x4	_world2Proj;
	float4x4	_camera2Proj;
	float4x4	_proj2Camera;

	float4		_ZNearFar_Q_Z;			// XY=Near/Far Clip, Z=Q=Zf/(Zf-Zn), W=0
	float4		_cameraSubPixelOffset;	// XY=Un-jitter vector, ZW=sub-pixel jitter offset

	// Previous frame matrices
	float4x4	_previousWorld2Proj;
	float4x4	_previoucCamera2CurrentCamera;
	float4x4	_currentCamera2PrevioucCamera;
};

cbuffer	CBSH : register( b2 ) {
	float4	_SH[9];
}

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)

/////////////////////////////////////////////////////////////////////////////////////////////////////
// GEOMETRY

// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
void BuildOrthonormalBasis( float3 _normal, out float3 _tangent, out float3 _bitangent ) {
	float a = _normal.z > -0.9999999 ? 1.0 / (1.0 + _normal.z) : 0.0;
	float b = -_normal.x * _normal.y * a;

	_tangent = float3( 1.0 - _normal.x*_normal.x*a, b, -_normal.x );
	_bitangent = float3( b, 1.0 - _normal.y*_normal.y*a, -_normal.y );
}

// Builds an **unnormalized** camera ray from a screen UV
float3	BuildCameraRay( float2 _UV ) {
	_UV = 2.0 * _UV - 1.0;
	_UV.x *= TAN_HALF_FOV * _resolution.x / _resolution.y;	// Account for aspect ratio
	_UV.y *= -TAN_HALF_FOV;									// Positive Y as we go up the screen
	return float3( _UV, 1.0 );								// Not normalized!
}

// Builds the entire reference frame for a **normalized** world-space and screen-space camera ray
void	BuildCameraRay( float2 _UV, out float3 _wsPos, out float3 _csView, out float3 _wsView, out float _Z2Distance ) {
	_csView = BuildCameraRay( _UV );
	_Z2Distance = length( _csView );
	_csView /= _Z2Distance;
	_wsView = mul( float4( _csView, 0.0 ), _camera2World ).xyz;
	_wsPos = _camera2World[3].xyz;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// MATH

#include "FastMath.hlsl"	// Fast Math library by M. Drobot

float	pow2( float  a ) { return a*a; }
float2	pow2( float2 a ) { return a*a; }
float3	pow2( float3 a ) { return a*a; }
float4	pow2( float4 a ) { return a*a; }

float	pow3( float  a ) { return a*a*a; }
float2	pow3( float2 a ) { return a*a*a; }
float3	pow3( float3 a ) { return a*a*a; }
float4	pow3( float4 a ) { return a*a*a; }

// Handbook of Mathematical Functions
// M. Abramowitz and I.A. Stegun, Ed.
// Absolute error <= 6.7e-5
// Source: https://web.archive.org/web/20161223122122/http://http.developer.nvidia.com:80/Cg/acos.html
float FastAcos( float x ) {
	float negate = float(x < 0);
	x = abs(x);
	float ret = -0.0187293;
	ret = ret * x;
	ret = ret + 0.0742610;
	ret = ret * x;
	ret = ret - 0.2121144;
	ret = ret * x;
	ret = ret + 1.5707288;
	ret = ret * sqrt(1.0-x);
	ret = ret - 2 * negate * ret;
	return negate * PI + ret;
}
float FastPosAcos( float x ) {	// If you're sure x>0 then use this version
	float ret = -0.0187293;
	ret = ret * x;
	ret = ret + 0.0742610;
	ret = ret * x;
	ret = ret - 0.2121144;
	ret = ret * x;
	ret = ret + 1.5707288;
	ret = ret * sqrt(1.0-x);
	return ret;
}

// Smooth minimum by iQ
float SmoothMin( float a, float b, float k ) {
    float res = exp( -k*a ) + exp( -k*b );
    return -log( res ) / k;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Noise & Hash

// Simple recursive Bayer matrix generation
// We start from the root permutation matrix:
//	I2 = |1 2|
//       |3 0|
//
// Next, we can recursively obtain successive matrices by applying:
//  I2n = | 4*In+1 4*In+2 |
//        | 4*In+3 4*In+0 |
//
// Generates the basic 2x2 Bayer permutation matrix:
//  [1 2]
//  [3 0]
// Expects _P in [0,1]
uint B2( uint _X, uint _Y ) {
	return ((_Y << 1) + _X + 1) & 3;
}

// Generates the 4x4 matrix
// Expects _P any pixel coordinate
uint B4( uint _X, uint _Y ) {
	return (B2( _X & 1, _Y & 1 ) << 2)
		  + B2( (_X >> 1) & 1, (_Y >> 1) & 1);
}

uint	Bayer1D_1( uint _time ) {	// [0,2[
	return 1 - (_time & 1);
}
uint	Bayer1D_4( uint _time ) {	// [0,4[
	return Bayer1D_1( _time >> 1 ) + (Bayer1D_1( _time ) << 1);
}
uint	Bayer1D_16( uint _time ) {	// [0,16[
	return Bayer1D_4( _time >> 2 ) + (Bayer1D_4( _time & 3 ) << 2);
}
uint	Bayer1D_64( uint _time ) {	// [0,63[
	return Bayer1D_16( _time >> 3 ) + (Bayer1D_16( _time & 7 ) << 3);
}

// Very good for "wide initialization" of ther "deep RNG" like iQ's hash or xor-shift
// http://www.reedbeta.com/blog/quick-and-easy-gpu-random-numbers-in-d3d11/
uint wang_hash( uint _seed ) {
    _seed = (_seed ^ 61) ^ (_seed >> 16);
    _seed *= 9;
    _seed = _seed ^ (_seed >> 4);
    _seed *= 0x27d4eb2d;
    _seed = _seed ^ (_seed >> 15);
    return _seed;
}

// Xorshift algorithm from George Marsaglia's paper
// Very good for "deep RNG", use wang_hash above for initialization
uint rand_xorshift( uint _state ) {
    _state ^= (_state << 13);
    _state ^= (_state >> 17);
    _state ^= (_state << 5);
    return _state;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Tone Mapping
float3	ToneMap( float3 x )			{ return x; }
float3	InverseToneMap( float3 x )	{ return x; }

/////////////////////////////////////////////////////////////////////////////////////////////////////
//
float3	SampleSRGB( Texture2D<float4> _texture, float2 _UV, float _diffuseGammaCorrection=2.2 ) {
	float3	sRGB = _texture.Sample( LinearWrap, _UV ).xyz;
return sRGB;	// We already have sRGB textures!
	return pow( saturate( sRGB ), _diffuseGammaCorrection );
}

#endif	// #ifndef __GLOBAL_INCLUDED
