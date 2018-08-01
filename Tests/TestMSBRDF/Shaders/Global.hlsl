/////////////////////////////////////////////////////////////////////////////////////////////////////
//
//
static const float	ASPECT_RATIO = 16.0 / 9.0;

static const float	PI = 3.1415926535897932384626433832795;
static const float	TWOPI = 6.283185307179586476925286766559;
static const float	FOURPI = 12.566370614359172953850573533118;
static const float	INVPI = 0.31830988618379067153776752674503;
static const float	SQRTPI = 1.7724538509055160272981674833411;
static const float	INFINITY = 1e6;
static const float	SQRT2 = 1.4142135623730950488016887242097;
static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)

cbuffer CB_Global : register(b0) {
	float4		_screenSize;	// viewport resolution (in pixels)
	float		_time;
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

struct VS_IN {
	float4	__position : SV_POSITION;
};

VS_IN	VS( VS_IN _In ) {
	return _In;
}

float	pow2( float x ) { return x * x; }
float2	pow2( float2 x ) { return x * x; }
float3	pow2( float3 x ) { return x * x; }
float4	pow2( float4 x ) { return x * x; }
float	pow3( float x ) { return x * x * x; }
float2	pow3( float2 x ) { return x * x * x; }
float3	pow3( float3 x ) { return x * x * x; }
float4	pow3( float4 x ) { return x * x * x; }

/////////////////////////////////////////////////////////////////////////////////////////////////////
// FRESNEL

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float	Fresnel_IORFromF0( float _F0 ) {
	float	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.0001 - SqrtF0);
}
float3	Fresnel_IORFromF0( float3 _F0 ) {
	float3	SqrtF0 = sqrt( _F0 );
	return (1.0 + SqrtF0) / (1.0001 - SqrtF0);
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

// Schlick's approximation to Fresnel reflection (http://en.wikipedia.org/wiki/Schlick's_approximation)
float	FresnelDielectricSchlick( float _F0, float _cosTheta, float _fresnelStrength=1.0 ) {
	float	t = 1.0 - saturate( _cosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _fresnelStrength * t4 * t );
}

float3	FresnelDielectricSchlick( float3 _F0, float _cosTheta, float _fresnelStrength=1.0 ) {
	float	t = 1.0 - saturate( _cosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _fresnelStrength * t4 * t );
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float	FresnelDielectric( float _IOR, float _cosTheta, float _fresnelStrength=1.0 ) {
	float	c = lerp( 1.0, _cosTheta, _fresnelStrength );
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

float3	FresnelDielectric( float3 _IOR, float _cosTheta, float _fresnelStrength=1.0 ) {
	float	c = lerp( 1.0, _cosTheta, _fresnelStrength );
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

// Returns the "average Fresnel" term for dielectrics, as given by Kulla & Conty, slide 18 (http://blog.selfshadow.com/publications/s2017-shading-course/imageworks/s2017_pbs_imageworks_slides.pdf)
//	Favg = Integral[Omega+]{ F(µi) µi dwi }
//
// _IOR, from 1 to 400
float3	FresnelAverage( float3 _IOR ) {
	return (_IOR - 1.0) / (4.08567 + 1.00071 * _IOR);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Importance Sampling + RNG + Geometry

// Code from http://forum.unity3d.com/threads/bitwise-operation-hammersley-point-sampling-is-there-an-alternate-method.200000/
uint ReverseBits( uint bits ) {
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

uint hash(uint x, uint y) {
    const uint M = 1664525u, C = 1013904223u;
    uint seed = (x * M + y + C) * M;
    // tempering (from Matsumoto)
    seed ^= (seed >> 11u);
    seed ^= (seed << 7u) & 0x9d2c5680u;
    seed ^= (seed << 15u) & 0xefc60000u;
    seed ^= (seed >> 18u);
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

// iQ's analytical sphere AO (http://iquilezles.org/www/articles/sphereao/sphereao.htm)
// Code from https://www.shadertoy.com/view/4djSDy)
//
float	ComputeSphereAO( float3 _position, float3 _normal, float3 _center, float _radius ) {
	float3	di = _center - _position;
	float	l  = length( di );
			di /= l;
	float	nl = dot( _normal, di );
	float	h  = l / _radius;
	float	h2 = h*h;
	float	k2 = 1.0 - h2*nl*nl;

	// above/below horizon: Quilez - 
	float	res = max( 0.0, nl ) / h2;
//	if ( k2 > 0.0 )  {
//		// Intersecting horizon: Lagarde/de Rousiers - http://www.frostbite.com/wp-content/uploads/2014/11/course_notes_moving_frostbite_to_pbr.pdf
//		#if 1
//			res = nl*acos(-nl*sqrt( (h2-1.0)/(1.0-nl*nl) )) - sqrt(k2*(h2-1.0));
//			res = res/h2 + atan( sqrt(k2/(h2-1.0)));
//			res /= PI;
//		#else
//			// cheap approximation: Quilez
//			res = pow( clamp(0.5*(nl*h+1.0)/h2,0.0,1.0), 1.5 );
//		#endif
//	}

	return res;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////
// Phase functions
float	PhaseFunctionRayleigh( float _CosPhaseAngle ) {
    return (3.0 / (16.0 * PI)) * (1.0 + _CosPhaseAngle * _CosPhaseAngle);
}

float	PhaseFunctionMie( float _CosPhaseAngle, float g ) {
	return 1.5 * 1.0 / (4.0 * PI) * (1.0 - g*g) * pow( max( 0.0, 1.0 + (g*g) - 2.0*g*_CosPhaseAngle ), -1.5 ) * (1.0 + _CosPhaseAngle * _CosPhaseAngle) / (2.0 + g*g);
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// BRDFs

// ====== GGX ======
float	GGX_NDF( float _NdotH, float _alpha2 ) {
	float	den = PI * pow2( pow2( _NdotH ) * (_alpha2 - 1) + 1 );
	return _alpha2 * rcp( den );
}

#if 1
	// Height-correlated shadowing/masking: G2 = 1 / (1 + Lambda(light) + Lambda(view))
	// Warning: 1 / (4 * NdotL * NdotV) is already accounted for!
	float	GGX_Smith( float _NdotL, float _NdotV, float _alpha2 ) {
		float	denL = _NdotV * sqrt( pow2( _NdotL ) * (1-_alpha2) + _alpha2 );
		float	denV = _NdotL * sqrt( pow2( _NdotV ) * (1-_alpha2) + _alpha2 );
		return rcp( 2.0 * (denL + denV) );
	}
#else
	// Uncorrelated shadowing/masking: G2 = G1(light) * G1(view)
	// Warning: 1 / (4 * NdotL * NdotV) is already accounted for!
	float	GGX_Smith( float _NdotL, float _NdotV, float _alpha2 ) {
		float	denL = _NdotL + sqrt( pow2( _NdotL ) * (1-_alpha2) + _alpha2 );
		float	denV = _NdotV + sqrt( pow2( _NdotV ) * (1-_alpha2) + _alpha2 );
		return rcp( denL * denV );
	}
#endif

float3	BRDF_GGX( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _alpha, float3 _IOR ) {
	float	NdotL = dot( _tsNormal, _tsLight );
	float	NdotV = dot( _tsNormal, _tsView );
	if ( NdotL < 0.0 || NdotV < 0.0 )
		return 0.0;

	float	a2 = pow2( _alpha );
	float3	H = normalize( _tsView + _tsLight );
	float	NdotH = saturate( dot( H, _tsNormal ) );
	float	HdotL = saturate( dot( H, _tsLight ) );

	float	D = GGX_NDF( NdotH, a2 );
	float	G = GGX_Smith( NdotL, NdotV, a2 );
	float3	F = FresnelDielectric( _IOR, HdotL );

	return max( 0.0, F * G * D );
}

// ====== Simple Oren-Nayar implementation ======
//  _normal, unit surface normal
//  _light, unit vector pointing toward the light
//  _view, unit vector pointing toward the view
//  _roughness, Oren-Nayar roughness parameter in [0,PI/2]
//
float   BRDF_OrenNayar( in float3 _normal, in float3 _view, in float3 _light, in float _roughness ) {
	float3  n = _normal;
	float3  l = _light;
	float3  v = _view;

	float   LdotN = dot( l, n );
	float   VdotN = dot( v, n );

	float   gamma = dot( v - n * VdotN, l - n * LdotN )
				  / (sqrt( saturate( 1.0 - VdotN*VdotN ) ) * sqrt( saturate( 1.0 - LdotN*LdotN ) ));

	float rough_sq = _roughness * _roughness;
	float A = 1.0 - 0.5 * (rough_sq / (rough_sq + 0.33));   // You can replace 0.33 by 0.57 to simulate the missing inter-reflection term, as specified in footnote of page 22 of the 1992 paper
	float B = 0.45 * (rough_sq / (rough_sq + 0.09));

	// Original formulation
	//  float angle_vn = acos( VdotN );
	//  float angle_ln = acos( LdotN );
	//  float alpha = max( angle_vn, angle_ln );
	//  float beta  = min( angle_vn, angle_ln );
	//  float C = sin(alpha) * tan(beta);

	// Optimized formulation (without tangents, arccos or sines)
	float2  cos_alpha_beta = VdotN < LdotN ? float2( VdotN, LdotN ) : float2( LdotN, VdotN );   // Here we reverse the min/max since cos() is a monotonically decreasing function
	float2  sin_alpha_beta = sqrt( saturate( 1.0 - cos_alpha_beta*cos_alpha_beta ) );           // Saturate to avoid NaN if ever cos_alpha > 1 (it happens with floating-point precision)
	float   C = sin_alpha_beta.x * sin_alpha_beta.y / (1e-6 + cos_alpha_beta.y);

	return INVPI * (A + B * max( 0.0, gamma ) * C);
}

// ====== Multiple-scattering BRDF computed from energy compensation ======
//
float	MSBRDF( float _roughness, float3 _tsNormal, float3 _tsView, float3 _tsLight, Texture2D<float> _Eo, Texture2D<float> _Eavg ) {

	float	mu_o = saturate( dot( _tsView, _tsNormal ) );
	float	mu_i = saturate( dot( _tsLight, _tsNormal ) );
	float	a = _roughness;

	float	E_o = 1.0 - _Eo.SampleLevel( LinearClamp, float2( mu_o, a ), 0.0 );	// 1 - E_o
	float	E_i = 1.0 - _Eo.SampleLevel( LinearClamp, float2( mu_i, a ), 0.0 );	// 1 - E_i
	float	E_avg = _Eavg.SampleLevel( LinearClamp, float2( a, 0.5 ), 0.0 );	// E_avg

	return E_o * E_i / max( 0.001, PI - E_avg );
}
