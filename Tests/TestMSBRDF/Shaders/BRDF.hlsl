/////////////////////////////////////////////////////////////////////////////////////////////////////
// BRDF Implementations
/////////////////////////////////////////////////////////////////////////////////////////////////////
//

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

	float   gamma = dot( normalize( v - n * VdotN ), normalize( l - n * LdotN ) );
//	float   gamma = dot( v - n * VdotN, l - n * LdotN )
//				  / (sqrt( saturate( 1.0 - VdotN*VdotN ) ) * sqrt( saturate( 1.0 - LdotN*LdotN ) ));	// This yields NaN when LdotN is exactly 1. Can be fixed using sqrt( saturate( 1.000001 - LdotN*LdotN ) ) instead, or a max...

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


/////////////////////////////////////////////////////////////////////////////////////////////////////
// ====== Multiple-scattering BRDF computed from energy compensation ======
// http://patapom.com/blog/BRDF/MSBRDF/
/////////////////////////////////////////////////////////////////////////////////////////////////////
//

// Estimates the view-dependent part of the MSBRDF
float	MSBRDF_View( float _mu_o, float _roughness, uint _BRDFIndex ) {
	float	E_o = 1.0 - SampleIrradiance( _mu_o, _roughness, _BRDFIndex );	// 1 - E_o
	float	E_avg = SampleAlbedo( _roughness, _BRDFIndex );					// E_avg
	return E_o / max( 0.001, PI - E_avg );
}

// Estimates the full MSBRDF (view- and light-dependent)
float	MSBRDF( float _roughness, float3 _tsNormal, float3 _tsView, float3 _tsLight, uint _BRDFIndex ) {

	float	mu_o = saturate( dot( _tsView, _tsNormal ) );
	float	mu_i = saturate( dot( _tsLight, _tsNormal ) );
	float	a = _roughness;

#if 1
	float	E_o = 1.0 - SampleIrradiance( mu_o, a, _BRDFIndex );	// 1 - E_o
	float	E_i = 1.0 - SampleIrradiance( mu_i, a, _BRDFIndex );	// 1 - E_i
	float	E_avg = SampleAlbedo( a, _BRDFIndex );					// E_avg

	return E_o * E_i / max( 0.001, PI - E_avg );
#else
	float	E_i = 1.0 - SampleIrradiance( mu_i, a, _BRDFIndex );	// 1 - E_i
	return E_i * MSBRDF_View( mu_o, a, _BRDFIndex );
#endif
}
