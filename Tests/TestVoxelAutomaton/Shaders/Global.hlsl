
#define PI			3.1415926535897932384626433832795
#define INVPI		0.31830988618379067153776752674503
#define SQRTPI		1.7724538509055160272981674833411
#define INFINITY	1e6

cbuffer CB_Main : register(b0) {
	uint2		_Resolution;
	float		_Time;
	float		_GlossRoom;
	float		_GlossSphere;
	float		_NoiseInfluence;
};

cbuffer CB_Camera : register(b1) {
	float4x4	_Camera2World;
	float4x4	_World2Camera;
	float4x4	_Proj2World;
	float4x4	_World2Proj;
	float4x4	_Camera2Proj;
	float4x4	_Proj2Camera;
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2� observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)


static const float	SPHERE_RADIUS = 0.2;
static const float3	SPHERE_CENTER = float3( 0.6, -0.8, 0.8 );

//static const float	GLOSS_ROOM = 0.6;
//static const float	GLOSS_SPHERE = 0.95;

struct VS_IN {
	float4	__Position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) { return _In; }


float	pow2( float x ) { return x * x; }
float	pow3( float x ) { return x * x * x; }

/////////////////////////////////////////////////////////////////////////////////////////////////////
// FRESNEL

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))�
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float	Fresnel_IORFromF0( float _F0 ) {
	float	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}
float3	Fresnel_IORFromF0( float3 _F0 ) {
	float3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.00001 - SqrtF0);
}

// Assuming n1=1 (air) we get:
//	IOR = (1 + sqrt(F0)) / (1 - sqrt(F0))
//	=> F0 = ((n2 - 1) / (n2 + 1))�
//
float	Fresnel_F0FromIOR( float _IOR ) {
	float	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}
float3	Fresnel_F0FromIOR( float3 _IOR ) {
	float3	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}

// Schlick's approximation to Fresnel reflection (http://en.wikipedia.org/wiki/Schlick's_approximation)
float	FresnelSchlick( float _F0, float _CosTheta, float _FresnelStrength=1.0 ) {
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

float3	FresnelSchlick( float3 _F0, float _CosTheta, float _FresnelStrength=1.0 ) {
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

// Full accurate Fresnel computation (from Walter's paper �5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float	FresnelAccurate( float _IOR, float _CosTheta, float _FresnelStrength=1.0 ) {
	float	c = lerp( 1.0, _CosTheta, _FresnelStrength );
	float	g_squared = max( 0.0, _IOR*_IOR - 1.0 + c*c );
// 	if ( g_squared < 0.0 )
// 		return 1.0;	// Total internal reflection

	float	g = sqrt( g_squared );

	float	a = (g - c) / (g + c);
			a *= a;
	float	b = (c * (g+c) - 1.0) / (c * (g-c) + 1.0);
			b = 1.0 + b*b;

	return 0.5 * a * b;
}

float3	FresnelAccurate( float3 _IOR, float _CosTheta, float _FresnelStrength=1.0 ) {
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


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Importance Sampling + RNG

// Code from http://forum.unity3d.com/threads/bitwise-operation-hammersley-point-sampling-is-there-an-alternate-method.200000/
float ReverseBits( uint bits ) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

float rand( float n ) { return frac(sin(n) * 43758.5453123); }

float rand( float2 _seed ) {
	return frac( sin( dot( _seed, float2( 12.9898, 78.233 ) ) ) * 43758.5453 );
//= frac( sin(_pixelPosition.x*i)*sin(1767.0654+_pixelPosition.y*i)*43758.5453123 );
}

// Build orthonormal basis from a 3D Unit Vector Without normalization [Frisvad2012])
void BuildOrthonormalBasis( float3 _normal, out float3 _tangent, out float3 _bitangent ) {
	float a = _normal.z > -0.9999999 ? 1.0 / (1.0 + _normal.z) : 0.0;
	float b = -_normal.x * _normal.y * a;

	_tangent = float3( 1.0 - _normal.x*_normal.x*a, b, -_normal.x );
	_bitangent = float3( b, 1.0 - _normal.y*_normal.y*a, -_normal.y );
}

// Error function implementation from formula 7.1.26 from "1964 Abramowitz, Stegun - Handbook of Mathematical Functions"
float erf( float x ) {
    // constants
    float	a1 =  0.254829592;
    float	a2 = -0.284496736;
    float	a3 =  1.421413741;
    float	a4 = -1.453152027;
    float	a5 =  1.061405429;
    float	p  =  0.3275911;
 
    // Save the sign of x
    float	sign = x < 0.0 ? -1 : 1;
    x = abs(x);
 
    // A&S formula 7.1.26
    float t = 1.0/(1.0 + p*x);
    float y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*exp(-x*x);
 
    return sign*y;
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Smith Masking term
float	GSmith( float3 _Wm, float3 _Wi, float _sqrAlpha ) {
	float	d = dot( _Wm, _Wi );
	return 2.0 * d / (d + sqrt( _sqrAlpha + (1.0 - _sqrAlpha) * d*d ));
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
float	IntersectBox( float3 _wsPos, float3 _wsView ) {
	float3	dir = _wsView < 0.0 ? -1.0 : 1.0;
	float3	wallDistance = dir - _wsPos;
	float3	t3 = wallDistance / _wsView;
	return min( min( t3.x, t3.y ), t3.z );
}

float	IntersectSphere( float3 _wsPos, float3 _wsView, float3 _wsCenter, float _radius ) {
	float3	D = _wsPos - _wsCenter;
	float	c = dot( D, D ) - _radius*_radius;
	float	b = dot( D, _wsView );
	float	delta = b*b - c;
	return delta >= 0.0 && b < 0.0 ? -b - sqrt( delta ) : INFINITY;
}

float3	ComputeSphereCenter() {
//	return SPHERE_CENTER;
	return float3( 0.6 * sin( 0.5 * _Time ), -0.8, 0.8 );
}

float2	Map( float3 _wsPos, float3 _wsView ) {
	float2	d = float2( IntersectBox( _wsPos, _wsView ), 0 );
	float2	ds = float2( IntersectSphere( _wsPos, _wsView, ComputeSphereCenter(), SPHERE_RADIUS ), 1 );
	return d.x < ds.x ? d : ds;
}
