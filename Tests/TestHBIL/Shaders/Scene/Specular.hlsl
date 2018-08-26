////////////////////////////////////////////////////////////////////////////////
// Specular Lighting Computation
////////////////////////////////////////////////////////////////////////////////
//
// Contains useful expressions to compute the various terms of the micro-facets specular BRDF model:
//
//		f( Wo, Wi, a ) = [ F( Wi, h ) * G1( Wi, Wo, h, a ) * D( h, a ) ] / [ 4 * (Wo.n) * (Wi.n) ]
//
// Where:
//	Wo is the output direction
//	Wi is the input direction
//	h is the normalized half vector h = normalize( Wi + Wo ).
//		It can be freely interchanged with m, the micro-facet normal, since we always assume there is a micro-facet aligned with our perfect mirror normal aligned with h.
//	n is the surface's macroscopic normal
//	a is the surface's roughness
//
////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"


/////////////////////////////////////////////////////////////////////////////////////////////////////
// FRESNEL TERM

// Assuming n1=1 (air) we get:
//	F0 = ((n2 - n1) / (n2 + n1))²
//	=> n2 = (1 + sqrt(F0)) / (1 - sqrt(F0))
//
float	Fresnel_IORFromF0( float _F0 ) {
	float	sqrtF0 = sqrt( _F0 );
	return (1.0 + sqrtF0) / (1.00001 - sqrtF0);
}
float3	Fresnel_IORFromF0( float3 _F0 ) {
	float3	sqrtF0 = sqrt( _F0 );
	return (1.0 + sqrtF0) / (1.00001 - sqrtF0);
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
float3	FresnelSchlick( float3 _F0, float _cosTheta, float _fresnelStrength=1.0 ) {
	float	t = 1.0 - saturate( _cosTheta );
	float	t2 = t * t;
	float	t4 = t2 * t2;
	return lerp( _F0, 1.0, _fresnelStrength * t4 * t );
}

// Full accurate Fresnel computation (from Walter's paper §5.1 => http://www.cs.cornell.edu/~srm/publications/EGSR07-btdf.pdf)
// For dielectrics only but who cares!?
float3	FresnelAccurate( float3 _IOR, float _cosTheta, float _fresnelStrength=1.0 ) {
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


/////////////////////////////////////////////////////////////////////////////////////////////////////
// NORMAL DISTRIBUTION + SHADOWING/MASKING
//

// ========== Beckmann ==========

// From Walter 2007
// D(m) = exp( -tan( theta_m )² / a² ) / (PI * a² * cos(theta_m)^4)
float	BeckmannNDF( float _cosTheta_M, float _roughness ) {
	float	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
	float	a2 = _roughness*_roughness;
	return exp( -(1.0 - sqCosTheta_M) / (sqCosTheta_M * a2) ) / (PI * a2 * sqCosTheta_M*sqCosTheta_M);
}

// Masking G1(v,m) = 2 / (1 + erf( 1/(a * tan(theta_v)) ) + exp(-a²) / (a*sqrt(PI)))
float	BeckmannG1( float _cosTheta, float _roughness ) {
	float	sqCosTheta_V = _cosTheta * _cosTheta;
	float	tanThetaV = sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
	float	a = 1.0 / (_roughness * tanThetaV);

	#if 1	// Simplified numeric version
		return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
	#else	// Analytical
		return 2.0 / (1.0 + erf( a ) + exp( -a*a ) / (a * SQRTPI));
	#endif
}

// ========== GGX ==========

// D(m) = a² / (PI * cos(theta_m)^4 * (a² + tan(theta_m)²)²)
// Simplified into  D(m) = a² / (PI * (cos(theta_m)²*(a²-1) + 1)²)
float	GGXNDF( float _cosTheta_M, float _roughness ) {
	float	sqCosTheta_M = _cosTheta_M * _cosTheta_M;
	float	a2 = _roughness*_roughness;
//	return a2 / (PI * sqCosTheta_M*sqCosTheta_M * pow2( a2 + (1.0-sqCosTheta_M)/sqCosTheta_M ));
	return a2 / (PI * pow2( sqCosTheta_M * (a2-1.0) + 1.0 ));
}

// Masking G1(v,m) = 2 / (1 + sqrt( 1 + a² * tan(theta_v)² ))
// Simplified into G1(v,m) = 2*cos(theta_v) / (1 + sqrt( cos(theta_v)² * (1-a²) + a² )) (certainly from http://graphicrants.blogspot.fr/2013/08/specular-brdf-reference.html although I think I rewrote it myself)
float	GGXG1( float _cosTheta, float _roughness ) {
	float	sqCosTheta_V = _cosTheta * _cosTheta;
	float	a2 = _roughness*_roughness;
	return 2.0 * _cosTheta / (1.0 + sqrt( sqCosTheta_V * (1.0 - a2) + a2 ));
}


// ========== Phong ==========
float	Roughness2PhongExponent( float _roughness ) {
//	return exp2( 10.0 * (1.0 - _roughness) + 1.0 );	// From https://seblagarde.wordpress.com/2011/08/17/hello-world/
	return exp2( 10.0 * (1.0 - _roughness) + 0.0 ) - 1.0;	// Actually, we'd like some fatter rough lobes
}

// D(m) = (n+2)/(2*PI) * cos(theta_m)^n
float	PhongNDF( float _cosTheta_M, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	return (n+2)*pow( _cosTheta_M, n ) / (2.0*PI);
}

// Same as Beckmann but modified a bit
float	PhongG1( float _cosTheta, float _roughness ) {
	float	n = Roughness2PhongExponent( _roughness );
	float	sqCosTheta_V = _cosTheta * _cosTheta;
	float	tanThetaV = sqrt( (1.0 - sqCosTheta_V) / sqCosTheta_V );
	float	a = sqrt( 1.0 + 0.5 * n ) / tanThetaV;
	return a < 1.6 ? (3.535 * a + 2.181 * a*a) / (1.0 + 2.276 * a + 2.577 * a*a) : 1.0;
}


/////////////////////////////////////////////////////////////////////////////////////////////////////
// Prepared specular models

float3	GGX( float3 _Wi, float3 _Wo, float3 _N, float _roughness, float3 _IOR, out float3 _F ) {
	float3	H = normalize( _Wi + _Wo );
	float	NdotL = saturate( dot( _Wi, _N ) );
	float	NdotV = saturate( dot( _Wo, _N ) );
	float	NdotH = dot( _N, H );
	float	LdotH = dot( _Wi, H );

	_F = FresnelAccurate( _IOR, LdotH );
	float	G = GGXG1( NdotL, _roughness ) * GGXG1( NdotV, _roughness );
	float	D = GGXNDF( NdotH, _roughness );
//return _F * G * D;
	return _F * (G * D / max( 1e-3, 4.0 * NdotL * NdotV ));
//	return isfinite( _F * (G * D / ( 4.0 * NdotL * NdotV )) );
//	return isfinite( _F * (G * D / max( 1e-3, 4.0 * NdotL * NdotV )) );
}
