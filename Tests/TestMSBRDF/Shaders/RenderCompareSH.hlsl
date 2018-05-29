#include "Global.hlsl"
#include "Scene.hlsl"

//#define	WHITE_FURNACE_TEST	1

//#define	MS_ONLY	1	// Show only multiple scattering contribution

static const uint	SAMPLES_COUNT = 32;

// AMAZING! Including this BEFORE the definitions of SAMPLES_COUNT and the plane changes color!
#include "SphericalHarmonics.hlsl"

#if WHITE_FURNACE_TEST
	// All white!
	#define	ALBEDO_SPHERE	1
	#define	ALBEDO_PLANE	1
	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular)		// So we can see below the specular layer!
	#define	F0_TINT_PLANE	1
#else
// Only for blog post with Rho=100%
//#define	ALBEDO_SPHERE	(_reflectanceSphereSpecular * float3( 1, 1, 1 ))
//#define	ALBEDO_PLANE	(_reflectanceGround * float3( 1, 1, 1 ))

	#define	ALBEDO_SPHERE	(_reflectanceSphereDiffuse * float3( 0.1, 0.5, 0.9 ))	// Nicely saturated blue
//	#define	ALBEDO_SPHERE	(_reflectanceSphereDiffuse * float3( 0.9, 0.5, 0.1 ))	// Nicely saturated yellow
	#define	ALBEDO_PLANE	(_reflectanceGround * float3( 0.9, 0.5, 0.1 ))			// Nicely saturated yellow

	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * float3( 1, 0.765557, 0.336057 ))	// Gold (from https://seblagarde.wordpress.com/2011/08/17/feeding-a-physical-based-lighting-mode/)
//	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * float3( 0.336057, 0.765557, 1 ))
//	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * 1.0)
	#define	F0_TINT_PLANE	(_reflectanceGround * 1.0)
#endif

static const float3	AMBIENT = 0* 0.02 * float3( 0.5, 0.8, 0.9 );

cbuffer CB_Render : register(b2) {
	uint	_flags;
	uint	_groupsCount;
	uint	_groupIndex;
	float	_lightElevation;

	float	_roughnessSphereSpecular;
	float	_reflectanceSphereSpecular;
	float	_roughnessSphereDiffuse;
	float	_reflectanceSphereDiffuse;

	float	_roughnessGround;
	float	_reflectanceGround;
	float	_lightIntensity;
};

cbuffer CB_SH : register(b3) {
	float3	_SH[9];
};

TextureCube< float3 >	_tex_CubeMap : register( t0 );
Texture2D< float >		_tex_BlueNoise : register( t1 );

Texture2D< float >		_tex_GGX_Eo : register( t2 );
Texture2D< float >		_tex_GGX_Eavg : register( t3 );
Texture2D< float >		_tex_OrenNayar_Eo : register( t4 );
Texture2D< float >		_tex_OrenNayar_Eavg : register( t5 );


//Texture2D< float3 >		_tex_SH : register( t30 );


/////////////////////////////////////////////////////////////////////////////////////////////

// Converts a 2D UV into an upper hemisphere direction (tangent space) + solid angle
// The direction is weighted by the surface cosine lobe
float3	UV2Direction( float2 _UV, out float _solidAngleCosine ) {
	_UV.y += 1e-5;	// 0 is completely useless..

	// Uniform hemispherical solid angle
	_solidAngleCosine = 2*PI;

	float	cosTheta = saturate( _UV.y );
	float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );

	float	phi = _UV.x * TWOPI;
	float2	scPhi;
	sincos( phi, scPhi.x, scPhi.y );

	return float3( sinTheta * scPhi, cosTheta );
}

float3	SampleSky( float3 _wsDirection, float _mipLevel ) {

	float2	scRot;
	sincos( _lightElevation, scRot.x, scRot.y );
	_wsDirection.xy = float2( _wsDirection.x * scRot.y + _wsDirection.y * scRot.x,
							-_wsDirection.x * scRot.x + _wsDirection.y * scRot.y );

#if WHITE_FURNACE_TEST
return 1;	// White furnace
#endif

	return _lightIntensity * _tex_CubeMap.SampleLevel( LinearWrap, _wsDirection, _mipLevel );
}

// Retrieves object information from its index
void	GetObjectInfo( uint _objectIndex, out float _roughnessSpecular, out float3 _objectF0, out float _roughnessDiffuse, out float3 _rho ) {
	_roughnessSpecular = max( 0.01, _objectIndex == 0 ? _roughnessSphereSpecular : _roughnessGround );
	_objectF0 = _objectIndex == 0 ? F0_TINT_SPHERE : F0_TINT_PLANE;

	_roughnessDiffuse = max( 0.01, _objectIndex == 0 ? _roughnessSphereDiffuse : _roughnessGround );
	_rho = _objectIndex == 0 ? ALBEDO_SPHERE : ALBEDO_PLANE;
}


/////////////////////////////////////////////////////////////////////////////////////////////
// SH Environment Approximation for the MSBRDF
float3	EstimateMSIrradiance_SH_GGX( float _roughness, float3 _wsNormal, float _mu_o, float3 _environmentSH[9], Texture2D<float> _Eo, Texture2D<float> _Eavg ) {

	// Estimate the MSBRDF response in the normal direction
	const float3x3	fit = float3x3( -0.01792303243636725, 1.0561278339405598, -0.4865495717038784,
									-0.06127443169094851, 1.3380225947779523, -0.6195823982255909,
									-0.10732852337149004, 0.8686198207608287, -0.3980009298364805 );
	float3	roughness = sqrt( _roughness ) * float3( 1, _roughness, _roughness*_roughness );
	float3	ZH = saturate( mul( fit, roughness ) );
			ZH *= ZH_FACTORS;

	float	SH[9];
	RotateZH( ZH, _wsNormal, SH );

	// Estimate MSBRDF irradiance
	float3	result = 0.0;
	[unroll]
	for ( uint i=0; i < 9; i++ )
		result += SH[i] * _environmentSH[i];

	result = max( 0.0, result );

	// Finish by estimating the view-dependent part
	float	E_o = _Eo.SampleLevel( LinearClamp, float2( _mu_o, _roughness ), 0.0 );
	float	E_avg = _Eavg.SampleLevel( LinearClamp, float2( _roughness, 0.5 ), 0.0 );	// E_avg

	result *= (1.0 - E_o) / max( 0.001, PI - E_avg );

	return result;
}

float3	EstimateMSIrradiance_SH_OrenNayar( float _roughness, float3 _wsNormal, float _mu_o, float3 _environmentSH[9], Texture2D<float> _Eo, Texture2D<float> _Eavg ) {

	// Estimate the MSBRDF response in the normal direction
	const float3x4	fit = float3x4( -0.0919559140506979, 1.467037714315657, -1.673544888379740, 0.607800523815945,
									-0.1136684128860008, 1.901273744271233, -2.322322430339633, 0.909815621695672,
									-0.0412482175221291, 1.093354950053632, -1.417191923789875, 0.581084435989362 );
	float4	roughness = sqrt( _roughness ) * float4( 1, _roughness, _roughness*_roughness, _roughness*_roughness*_roughness );
	float3	ZH = saturate( mul( fit, roughness ) );
			ZH *= ZH_FACTORS;

	float	SH[9];
	RotateZH( ZH, _wsNormal, SH );

	// Estimate MSBRDF irradiance
	float3	result = 0.0;
	[unroll]
	for ( uint i=0; i < 9; i++ )
		result += SH[i] * _environmentSH[i];

	result = max( 0.0, result );

	// Finish by estimating the view-dependent part
	float	E_o = _Eo.SampleLevel( LinearClamp, float2( _mu_o, _roughness ), 0.0 );
	float	E_avg = _Eavg.SampleLevel( LinearClamp, float2( _roughness, 0.5 ), 0.0 );	// E_avg

	result *= (1.0 - E_o) / max( 0.001, PI - E_avg );

	return result;
}

float3	EstimateMSIrradiance_SH( float3 _wsNormal, float3 _wsReflected, float _mu_o, float _roughnessSpecular, float3 _IOR, float _roughnessDiffuse, float3 _albedo, float3 _environmentSH[9] ) {

	// Estimate specular irradiance
	float3	F0 = Fresnel_F0FromIOR( _IOR );
	float3	MSFactor_spec = (_flags & 2) ? F0 * (0.04 + F0 * (0.66 + F0 * 0.3)) : F0;	// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-the-fresnel-reflectance-f_0f_0
	float3	Favg = FresnelAverage( _IOR );

//MSFactor_spec = 1.0;

	float3	E_spec = MSFactor_spec * EstimateMSIrradiance_SH_GGX( _roughnessSpecular, _wsReflected, _mu_o, _environmentSH, _tex_GGX_Eo, _tex_GGX_Eavg );

//return E_spec;

	// Estimate diffuse irradiance
	const float	tau = 0.28430405702379613;
	const float	A1 = (1.0 - tau) / pow2( tau );
	float3		rho = tau * _albedo;
	float3		MSFactor_diff = (_flags & 2) ? A1 * pow2( rho ) / (1.0 - rho) : rho;	// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-diffuse-reflectance-rhorho

//MSFactor_diff = 1;

	float3	E_diff = MSFactor_diff * EstimateMSIrradiance_SH_OrenNayar( _roughnessDiffuse, _wsNormal, _mu_o, _environmentSH, _tex_OrenNayar_Eo, _tex_OrenNayar_Eavg );

//return E_diff;

	// Attenuate diffuse contribution
	float	a = _roughnessSpecular;
	float	E_o = _tex_GGX_Eo.SampleLevel( LinearClamp, float2( _mu_o, a ), 0.0 );	// Already sampled by MSBRDF earlier, optimize!

	float3	kappa = 1 - (Favg * E_o + MSFactor_spec * (1.0 - E_o));

	return E_spec + kappa * E_diff;
}

////////////////////////////////////////////////////////////////////////////////////////////////
//
//float3	ComputeBRDF_GGX( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughness, float3 _IOR ) {
//	float3	BRDF = BRDF_GGX( _tsNormal, _tsView, _tsLight, _roughness, _IOR );
//	if ( (_flags & 1) && !(_flags & 0x100) ) {
//		// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-the-fresnel-reflectance-f_0f_0
//		float3		F0 = Fresnel_F0FromIOR( _IOR );
//		float3		MSFactor = (_flags & 2) ? F0 * (0.04 + F0 * (0.66 + F0 * 0.3)) : F0;
//
//		BRDF += MSFactor * MSBRDF( _roughness, _tsNormal, _tsView, _tsLight, _tex_GGX_Eo, _tex_GGX_Eavg );
//	}
//
//	return BRDF;
//}
//
//float3	ComputeBRDF_Oren( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughness, float3 _albedo ) {
//	float3	BRDF = _albedo * BRDF_OrenNayar( _tsNormal, _tsView, _tsLight, _roughness );
//	if ( (_flags & 1) && !(_flags & 0x100) ) {
//		// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-diffuse-reflectance-rhorho
//		const float	tau = 0.28430405702379613;
//		const float	A1 = (1.0 - tau) / pow2( tau );
//		float3		rho = tau * _albedo;
//		float3		MSFactor = (_flags & 2) ? A1 * pow2( rho ) / (1.0 - rho) : rho;
//
//		BRDF += MSFactor * MSBRDF( _roughness, _tsNormal, _tsView, _tsLight, _tex_OrenNayar_Eo, _tex_OrenNayar_Eavg );
//	}
//
//	return BRDF;
//}

// Computes the full dielectric BRDF model as described in http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#complete-approximate-model
//
float3	ComputeBRDF_Full(  float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughnessSpecular, float3 _IOR, float _roughnessDiffuse, float3 _albedo ) {
	// Compute specular BRDF
	float3	F0 = Fresnel_F0FromIOR( _IOR );
	float3	MSFactor_spec = (_flags & 2) ? F0 * (0.04 + F0 * (0.66 + F0 * 0.3)) : F0;	// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-the-fresnel-reflectance-f_0f_0
	float3	Favg = FresnelAverage( _IOR );

	#if MS_ONLY
		float3	BRDF_spec = 0.0;
	#else
		float3	BRDF_spec = BRDF_GGX( _tsNormal, _tsView, _tsLight, _roughnessSpecular, _IOR );
	#endif
	if ( (_flags & 1) && !(_flags & 0x100) ) {
		BRDF_spec += MSFactor_spec * MSBRDF( _roughnessSpecular, _tsNormal, _tsView, _tsLight, _tex_GGX_Eo, _tex_GGX_Eavg );
	}

	// Compute diffuse contribution
	#if MS_ONLY
		float3	BRDF_diff = 0.0;
	#else
		float3	BRDF_diff = _albedo * BRDF_OrenNayar( _tsNormal, _tsView, _tsLight, _roughnessDiffuse );
	#endif
	if ( (_flags & 1) && !(_flags & 0x100) ) {
		const float	tau = 0.28430405702379613;
		const float	A1 = (1.0 - tau) / pow2( tau );
		float3		rho = tau * _albedo;
		float3		MSFactor_diff = (_flags & 2) ? A1 * pow2( rho ) / (1.0 - rho) : rho;	// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-diffuse-reflectance-rhorho

		BRDF_diff += MSFactor_diff * MSBRDF( _roughnessDiffuse, _tsNormal, _tsView, _tsLight, _tex_OrenNayar_Eo, _tex_OrenNayar_Eavg );
	}

	// Attenuate diffuse contribution
	float	mu_o = saturate( dot( _tsView, _tsNormal ) );
	float	a = _roughnessSpecular;
	float	E_o = _tex_GGX_Eo.SampleLevel( LinearClamp, float2( mu_o, a ), 0.0 );	// Already sampled by MSBRDF earlier, optimize!

	float3	kappa = 1 - (Favg * E_o + MSFactor_spec * (1.0 - E_o));

	return BRDF_spec + kappa * BRDF_diff;
}


float4	PS( VS_IN _In ) : SV_TARGET0 {

	float	noise = 0*_tex_BlueNoise[uint2(_In.__position.xy + uint2( _groupIndex, 0 )) & 0x3F];

	float2	UV = float2( _screenSize.x / _screenSize.y * (2.0 * (_In.__position.x+noise) / _screenSize.x - 1.0), 1.0 - 2.0 * _In.__position.y / _screenSize.y );

	uint2	seeds;
	seeds.x = wang_hash( asuint(_In.__position.x + _time) + _groupIndex ) ^ wang_hash( asuint(_In.__position.y - _time*_groupIndex) );
	seeds.y = hash( seeds.x, 1000u );

	uint	totalGroupsCount = _groupsCount * SAMPLES_COUNT;

//return float4( seeds.xxx * 2.3283064365386963e-10, 1 );

	// Build camera ray
	float3	csView = normalize( float3( UV, 1 ) );
	float3	wsRight = _Camera2World[0].xyz;
	float3	wsUp = _Camera2World[1].xyz;
	float3	wsAt = _Camera2World[2].xyz;
	float3	wsView = csView.x * wsRight + csView.y * wsUp + csView.z * wsAt;
	float3	wsPosition = _Camera2World[3].xyz;

	float3	wsClosestPosition;
	float3	wsNormal;
	float2	hit = RayTraceScene( wsPosition, wsView, wsNormal, wsClosestPosition );
	if ( hit.x > 1e4 || hit.y != 0 )
		return float4( SampleSky( wsView, 0.0 ), 1 );	// No hit or plane hit (we only render the sphere here)

	wsPosition += hit.x * wsView;
	wsPosition += 1e-3 * wsNormal;	// Offset from surface

	// Build tangent space
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

	float3	tsView = -float3( dot( wsView, wsTangent ), dot( wsView, wsBiTangent ), dot( wsView, wsNormal ) );

	// Build environment SH as an array
	float3	envSH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };

// Check SH coefficients
//if ( hit.y != 0 )
//	return float4( SampleSky( wsView, 0.0 ), 1 );	// No hit
//return float4( EvaluateSHRadiance( wsNormal, envSH ), 1 );				// Order 2 (9 coefficients)
////return float4( EvaluateSHRadiance( wsNormal, 9, _tex_SH, 8.7 ), 1 );	// Order 9 (100 coefficients)

	// Prepare surface characteristics
	float3	rho, F0;
	float	alphaS, alphaD;
	GetObjectInfo( hit.y, alphaS, F0, alphaD, rho );

	float3	IOR = Fresnel_IORFromF0( F0 );

	float	u = seeds.x * 2.3283064365386963e-10;

	float3	Lo = 0.0;
	uint	groupIndex = _groupIndex;
	uint	validSamplesCount = 0;

	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		float	X0 = frac( u + float(groupIndex) / totalGroupsCount );
		float	X1 = (ReverseBits( groupIndex ) ^ seeds.y) * 2.3283064365386963e-10; // / 0x100000000
		groupIndex += _groupsCount;	// Giant leaps give us large changes

		// Retrieve light direction + solid angle
		float	dw;
		float3	tsLight = UV2Direction( float2( X0, X1 ), dw );	// Generate ray in tangent space
		float3	wsLight = tsLight.x * wsTangent + tsLight.y * wsBiTangent + tsLight.z * wsNormal;

		float	LdotN = tsLight.z;

		// Compute BRDF
		float3	BRDF = ComputeBRDF_Full( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR, alphaD, rho );

		// Sample incoming radiance
		float3	Li = SampleSky( wsLight, 0.0 );
//		float3	Li = EvaluateSHRadiance( wsLight, envSH );		// Order 2 (9 coefficients)
//		float3	Li = EvaluateSHRadiance( wsLight, 9, _tex_SH );	// Order 9 (100 coefficients)

		// Compute reflected radiance
		float3	Lr = Li * BRDF;


#if WHITE_FURNACE_TEST
Lr *= 0.9;	// Attenuate a bit to see in front of white sky...
#endif

		// Accumulate
		Lo += Lr * LdotN * dw;
		validSamplesCount++;
    }

	///////////////////////////////////////////////////////////////////////////////////////
	// Real-time Approximation
	if ( (_flags & 1) && (_flags & 0x100) ) {
		float3	wsReflectedView = reflect( wsView, wsNormal );
		float3	wsReflected = normalize( lerp( wsReflectedView, wsNormal, alphaS ) );	// Go more toward prefectly reflected direction when roughness drops to 0

wsReflected = wsNormal;

		Lo += validSamplesCount * EstimateMSIrradiance_SH( wsNormal, wsReflected, saturate( tsView.z ), alphaS, IOR, alphaD, rho, envSH );
	}

//	return float4( Lo, SAMPLES_COUNT);
	return float4( Lo, validSamplesCount );
}
