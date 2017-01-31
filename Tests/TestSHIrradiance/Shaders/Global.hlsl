////////////////////////////////////////////////////////////////////////////////
// Global Defines
////////////////////////////////////////////////////////////////////////////////
//
static const float	PI = 3.1415926535897932384626433832795;
static const float	INVPI = 0.31830988618379067153776752674503;
static const float	SQRT2 = 1.4142135623730950488016887242097;

static const float	CAMERA_FOV = 90.0 * PI / 180.0;
static const float	TAN_HALF_FOV = tan( 0.5 * CAMERA_FOV );
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)
static const float	INFINITY = 1e12;
static const float	NO_HIT = 1e6;

cbuffer	CBDisplay : register( b0 ) {
	uint2		_size;
	float		_time;
	uint		_flags;
	float4		_mouse;
	float4x4	_world2Proj;
//	float4x4	_proj2World;
	float4x4	_camera2World;
	float		_cosAO;
	float		_luminanceFactor;
	float		_filterWindowSize;
	float		_influenceAO;
	float		_influenceBentNormal;
	uint		_SHOrdersCount;
	int			_customM;
}

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border

Texture2D<float4>	_TexHDR : register( t0 );

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }


#include "SphericalHarmonics.hlsl"

// Samples the panormaic environment HDR map
float3	SampleHDREnvironment( float3 _wsDirection ) {
	float	phi = atan2( _wsDirection.y, _wsDirection.x );
	float	theta = acos( _wsDirection.z );
	float2	UV = float2( 0.5 + 0.5 * phi * INVPI, theta * INVPI );
	return _TexHDR.SampleLevel( LinearWrap, UV, 0.0 ).xyz;
}

float	IntersectSphere( float3 _pos, float3 _dir, float3 _center, float _radius ) {
	float3	D = _pos - _center;
	float	b = dot( D, _dir );
	float	c = dot( D, D ) - _radius*_radius;
	float	delta = b*b - c;
	return delta > 0.0 ? -b - sqrt( delta ) : INFINITY;
}

float	IntersectPlane( float3 _pos, float3 _dir, float3 _planePosition, float3 _normal ) {
	float3	D = _pos - _planePosition;
	return -dot( D, _normal ) / dot( _dir, _normal );
}

// bmayaux (2016-01-04) Builds the remaining 2 orthogonal vectors from a given vector (very fast! no normalization or square root involved!)
// Original code from http://orbit.dtu.dk/files/57573287/onb_frisvad_jgt2012.pdf
//
void	BuildOrthogonalVectors( float3 n, out float3 b1, out float3 b2 ) {
	const float	a = n.z > -0.9999999 ? 1.0 / (1.0 + n.z) : 0.0;	// Instead of the condition, I used this ternary op but beware that b1=(1,0,0) and b2=(0,1,0) in the case n.z=-1 instead of the expected (0,-1,0), (-1,0,0)
	const float	b = -n.x*n.y*a;
	b1 = float3( 1.0 - n.x*n.x*a, b, -n.x );
	b2 = float3( b, 1.0 - n.y*n.y*a, -n.y );
}
