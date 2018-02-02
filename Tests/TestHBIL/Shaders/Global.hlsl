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
	float2	_resolution;	// viewport resolution (in pixels)
	float	_time;
	float	_deltaTime;
	uint	_flags;
	uint	_sourceRadianceIndex;
	uint	_debugMipIndex;
	float	_environmentIntensity;
	float	_forcedAlbedo;
	float	_coneAngleBias;
	float	_exposure;
};

cbuffer CB_Camera : register(b1) {
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Proj2World;
	float4x4	_World2Proj;
	float4x4	_Camera2Proj;
	float4x4	_Proj2Camera;

	float4x4	_PrevioucCamera2CurrentCamera;
	float4x4	_CurrentCamera2PrevioucCamera;
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

/////////////////////////////////////////////////////////////////////////////////////////////////////
// MATH

// Handbook of Mathematical Functions
// M. Abramowitz and I.A. Stegun, Ed.
// Absolute error <= 6.7e-5
// Source: https://web.archive.org/web/20161223122122/http://http.developer.nvidia.com:80/Cg/acos.html
float fastAcos( float x ) {
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
// FRESNEL

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float	Fresnel_IORFromF0( float _F0 )
{
	float	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}
float3	Fresnel_IORFromF0( float3 _F0 )
{
	float3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}

// Assuming n1=1 (air) we get:
//	IOR = (1 + sqrt(F0)) / (1 - sqrt(F0))
//	=> F0 = ((n2 - 1) / (n2 + 1))²
//
float	Fresnel_F0FromIOR( float _IOR )
{
	float	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}
float3	Fresnel_F0FromIOR( float3 _IOR )
{
	float3	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}

// Schlick's approximation to Fresnel reflection (http://en.wikipedia.org/wiki/Schlick's_approximation)
float3	FresnelSchlick( float3 _F0, float _CosTheta, float _FresnelStrength=1.0 )
{
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float3	FresnelAccurate( float3 _IOR, float _CosTheta, float _FresnelStrength=1.0 )
{
	float	c = lerp( 1.0, _CosTheta, _FresnelStrength );
	float3	g_squared = max( 0.0, _IOR*_IOR - 1.0 + c*c );
// 	if ( g_squared < 0.0 )
// 		return 1.0;	// Total internal reflection

	float3	g = sqrt( g_squared );

	float3	a = (g - c) / (g + c);
			a *= a;
	float3	b = (c * (g+c) - 1.0) / (c * (g-c) + 1.0);
			b = 1.0 + b*b;

	return 0.5 * a * b;
}

#endif	// #ifndef __GLOBAL_INCLUDED
