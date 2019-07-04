
#define PI		3.1415926535897932384626433832795
#define INVPI	0.31830988618379067153776752674503

cbuffer CB_Main : register(b0) {
	float2	iResolution;	// viewport resolution (in pixels)
	float	tanHalfFOV;		// tan( Vertical FOV / 2 )
	float	iGlobalTime;	// shader playback time (in seconds)
};

cbuffer CB_Camera : register(b1) {
	float4x4	_camera2World;
	float4x4	_world2Camera;
	float4x4	_proj2World;
	float4x4	_world2Proj;
	float4x4	_camera2Proj;
	float4x4	_proj2Camera;
};

cbuffer CB_AreaLight : register(b2) {
	float4x4	_wsLight2World;			// Row 0 [XYZ] = Axis X	[W] = Radius X
										// Row 1 [XYZ] = Axis Y	[W] = Radius Y
										// Row 2 [XYZ] = At		[W] = Disk area = PI * radius X * radius Y
										// Row 3 [XYZ] = wsPos	[W] = 1
	float		_diskLuminance;			// In lm/cd/m² (cd/m²)
};

SamplerState LinearClamp	: register( s0 );
SamplerState PointClamp		: register( s1 );
SamplerState LinearWrap		: register( s2 );
SamplerState PointWrap		: register( s3 );
SamplerState LinearMirror	: register( s4 );
SamplerState PointMirror	: register( s5 );
SamplerState LinearBorder	: register( s6 );	// Black border


static const float3	LUMINANCE = float3( 0.2126, 0.7152, 0.0722 );	// D65 Illuminant and 2° observer (cf. http://wiki.nuaj.net/index.php?title=Colorimetry)


float	pow2( float a ) { return a*a; }
float2	pow2( float2 a ) { return a*a; }
float3	pow2( float3 a ) { return a*a; }
float4	pow2( float4 a ) { return a*a; }
float	pow3( float a ) { return a*a*a; }
float2	pow3( float2 a ) { return a*a*a; }
float3	pow3( float3 a ) { return a*a*a; }
float4	pow3( float4 a ) { return a*a*a; }

// Generates a normalized ray in camera space given a screen pixel position
float3	GenerateCameraRay( float2 _pixelPosition ) {
	float3	csView = float3( tanHalfFOV * (2.0 * _pixelPosition / iResolution - 1.0), 1.0 );
			csView.x *= iResolution.x / iResolution.y;
			csView.y = -csView.y;
			
	float	Z2Length = length( csView );
			csView /= Z2Length;

	return csView;
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
float3	FresnelDielectricSchlick( float3 _F0, float _CosTheta, float _FresnelStrength=1.0 )
{
	float	t = 1.0 - saturate( _CosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _FresnelStrength * t4 * t );
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float3	FresnelDielectric( float3 _IOR, float _CosTheta, float _FresnelStrength=1.0 )
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

// Smooth minimum by iQ
float SmoothMin( float a, float b, float k ) {
    float res = exp( -k*a ) + exp( -k*b );
    return -log( res ) / k;
}

// Code from http://forum.unity3d.com/threads/bitwise-operation-hammersley-point-sampling-is-there-an-alternate-method.200000/
float ReverseBits( uint bits ) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}
