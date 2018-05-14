
#define PI			3.1415926535897932384626433832795
#define INVPI		0.31830988618379067153776752674503
#define SQRTPI		1.7724538509055160272981674833411
#define INFINITY	1e6

static const uint	MAX_SCATTERING_ORDER = 6;

// Dimensions of the height field (must match C# declaration)
static const uint	HEIGHTFIELD_SIZE = 512;
static const float	INV_HEIGHTFIELD_SIZE = 1.0 / HEIGHTFIELD_SIZE;

// Dimensions of the hemispherical lobe (must match C# declaration)
static const uint	LOBES_COUNT_THETA = 128;
static const uint	LOBES_COUNT_PHI = 2*LOBES_COUNT_THETA;

cbuffer CB_Main : register(b0) {
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


static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)


float	pow2( float x ) { return x * x; }
float	pow3( float x ) { return x * x * x; }
float2	pow2( float2 x ) { return x * x; }
float2	pow3( float2 x ) { return x * x * x; }
float3	pow2( float3 x ) { return x * x; }
float3	pow3( float3 x ) { return x * x * x; }
float4	pow2( float4 x ) { return x * x; }
float4	pow3( float4 x ) { return x * x * x; }

/////////////////////////////////////////////////////////////////////////////////////////////////////
// FRESNEL
// NOTE:	• When F0 < 0.2, you should ALWAYS use the dielectric Fresnel (metallic Fresnel strongly diverges)
//			• When F0 > 0.2
//				• If F90 = 1, you can continue to use the dielectric Fresnel
//				• If F90 != 1, you should use the metallic Fresnel
//
// Overall, if F90 is not important, you can use the dielectric Fresnel for any F0 (i.e. for dielectrics or metals alike)

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
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
//	=> F0 = ((n2 - 1) / (n2 + 1))²
//
float	Fresnel_F0FromIOR( float _IOR ) {
	float	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}
float3	Fresnel_F0FromIOR( float3 _IOR ) {
	float3	ratio = (_IOR - 1.0) / (_IOR + 1.0);
	return ratio * ratio;
}

// Schlick's approximation to dielectric Fresnel reflection (http://en.wikipedia.org/wiki/Schlick's_approximation)
float	FresnelDielectricSchlick( float _F0, float _cosTheta, float _FresnelStrength=1.0 ) {
	float	t = 1.0 - saturate( _cosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

float3	FresnelDielectricSchlick( float3 _F0, float _cosTheta, float _FresnelStrength=1.0 ) {
	float	t = 1.0 - saturate( _cosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

// Full accurate dielectric Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// NOTE: When F0 > 0.2, the dielectic and metallic fresnel start diverging
float	FresnelDielectric( float _IOR, float _cosTheta, float _FresnelStrength=1.0 ) {
	float	c = lerp( 1.0, _cosTheta, _FresnelStrength );
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

float3	FresnelDielectric( float3 _IOR, float _cosTheta, float _FresnelStrength=1.0 ) {
	float	c = lerp( 1.0, _cosTheta, _FresnelStrength );
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

// From Ole Gulbrandsen "Artist Friendly Metallic Fresnel" (http://jcgt.org/published/0003/04/03/paper.pdf)
// NOTE: When F0 > 0.2, the dielectic and metallic fresnel start diverging
//	_F0, metal color at normal incidence (i.e. theta=0°)
//	_F90, metal color at grazing incidence (i.e. theta=90°)
//	_cosTheta, cos(theta) of the light/view angle (should be the angle between the view/light vector and half vector in a standard micro-facet model)
//
float3	FresnelMetal( float3 _F0, float3 _F90, float _cosTheta ) {

	float3	r = clamp( _F0, 0, 0.99 );
	float3	g = _F90;
	float	c = _cosTheta;
	float	c2 = pow2(c);

	// Compute n and k
	float3	sqrtR = sqrt( r );
	float3	n_min = (1-r) / (1+r);
	float3	n_max = (1+sqrtR) / (1-sqrtR);
	float3	n = lerp( n_min, n_max, g );
	float3	n2 = pow2(n);

	float3	nr = pow2(n+1) * r - pow2(n-1);
	float3	k2 = nr / (1 - r);

	// Compute perpendicular polarized Fresnel
	float3	numPe = n2 + k2 - 2*n*c + c2;
	float3	denPe = n2 + k2 + 2*n*c + c2;
	float3	Pe = numPe / denPe;

	// Compute parallel polarized Fresnel
	float3	numPa = (n2 + k2)*c2 - 2*n*c + 1;
	float3	denPa = (n2 + k2)*c2 + 2*n*c + 1;
	float3	Pa = numPa / denPa;

	return 0.5 * (Pe + Pa);
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
uint ReverseBitsInt( uint bits ) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return bits;
}

float rand( float n ) { return frac(sin(n) * 43758.5453123); }

float rand( float2 _seed ) {
	return frac( sin( dot( _seed, float2( 12.9898, 78.233 ) ) ) * 43758.5453 );
//= frac( sin(_pixelPosition.x*i)*sin(1767.0654+_pixelPosition.y*i)*43758.5453123 );
}

// From Nathan Reed (http://reedbeta.com/blog/quick-and-easy-gpu-random-numbers-in-d3d11/)
uint wang_hash(uint seed) {
    seed = (seed ^ 61) ^ (seed >> 16);
    seed *= 9;
    seed = seed ^ (seed >> 4);
    seed *= 0x27d4eb2d;
    seed = seed ^ (seed >> 15);
    return seed;
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
