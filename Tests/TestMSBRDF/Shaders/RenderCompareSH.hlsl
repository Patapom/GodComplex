#include "Global.hlsl"
#include "Scene.hlsl"
#include "SphericalHarmonics.hlsl"

#define FULL_SCENE		1	// Define this to render the full scene (diffuse plane + specular sphere)
//#define FORCE_BRDF		2	// Define this to force all surfaces as specular (1), diffuse (2)

#define	DIELECTRIC_SPHERE	1	// Define this to use the full dielectric sphere model

//#define	WHITE_FURNACE_TEST	1


static const uint	SAMPLES_COUNT = 32;

#if WHITE_FURNACE_TEST
	// All white!
	#define	ALBEDO_SPHERE	1
	#define	ALBEDO_PLANE	1
	#define	F0_TINT_SPHERE	1
	#define	F0_TINT_PLANE	1
#else
// Only for blog post with Rho=100%
//#define	ALBEDO_SPHERE	(_reflectanceSphereSpecular * float3( 1, 1, 1 ))
//#define	ALBEDO_PLANE	(_reflectanceGround * float3( 1, 1, 1 ))

	#define	ALBEDO_SPHERE	(_reflectanceSphereDiffuse * float3( 0.1, 0.5, 0.9 ))	// Nicely saturated blue
//	#define	ALBEDO_SPHERE	(_reflectanceSphereDiffuse * float3( 0.9, 0.5, 0.1 ))	// Nicely saturated yellow
	#define	ALBEDO_PLANE	(_reflectanceGround * float3( 0.9, 0.5, 0.1 ))			// Nicely saturated yellow

//	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * float3( 1, 0.765557, 0.336057 ))	// Gold (from https://seblagarde.wordpress.com/2011/08/17/feeding-a-physical-based-lighting-mode/)
//	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * float3( 0.336057, 0.765557, 1 ))
	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * 1.0)
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

////////////////////////////////////////////////////////////////////////////////////////////////
//
float3	ComputeBRDF_GGX( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughness, float3 _IOR ) {
	float3	BRDF = BRDF_GGX( _tsNormal, _tsView, _tsLight, _roughness, _IOR );
	if ( _flags & 1 ) {
		// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-the-fresnel-reflectance-f_0f_0
		float3		F0 = Fresnel_F0FromIOR( _IOR );
		float3		MSFactor = (_flags & 2) ? F0 * (0.04 + F0 * (0.66 + F0 * 0.3)) : F0;

		BRDF += MSFactor * MSBRDF( _roughness, _tsNormal, _tsView, _tsLight, _tex_GGX_Eo, _tex_GGX_Eavg );
	}

	return BRDF;
}

float3	ComputeBRDF_Oren( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughness, float3 _albedo ) {
	float3	BRDF = _albedo * BRDF_OrenNayar( _tsNormal, _tsView, _tsLight, _roughness );
	if ( _flags & 1 ) {
		// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-diffuse-reflectance-rhorho
		const float	tau = 0.28430405702379613;
		const float	A1 = (1.0 - tau) / pow2( tau );
		float3		rho = tau * _albedo;
		float3		MSFactor = (_flags & 2) ? A1 * pow2( rho ) / (1.0 - rho) : rho;

		BRDF += MSFactor * MSBRDF( _roughness, _tsNormal, _tsView, _tsLight, _tex_OrenNayar_Eo, _tex_OrenNayar_Eavg );
	}

	return BRDF;
}

// Computes the full dielectric BRDF model as described in http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#complete-approximate-model
//
float3	ComputeBRDF_Full(  float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughnessSpecular, float3 _IOR, float _roughnessDiffuse, float3 _albedo ) {
	// Compute specular BRDF
	float3	F0 = Fresnel_F0FromIOR( _IOR );
	float3	MSFactor_spec = (_flags & 2) ? F0 * (0.04 + F0 * (0.66 + F0 * 0.3)) : F0;	// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-the-fresnel-reflectance-f_0f_0
	float3	Favg = FresnelAverage( _IOR );

	float3	BRDF_spec = BRDF_GGX( _tsNormal, _tsView, _tsLight, _roughnessSpecular, _IOR );
	if ( _flags & 1 ) {
		BRDF_spec += MSFactor_spec * MSBRDF( _roughnessSpecular, _tsNormal, _tsView, _tsLight, _tex_GGX_Eo, _tex_GGX_Eavg );
	}

	// Compute diffuse contribution
	float3	BRDF_diff = _albedo * BRDF_OrenNayar( _tsNormal, _tsView, _tsLight, _roughnessDiffuse );
	if ( _flags & 1 ) {
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


////////////////////////////////////////////////////////////////////////////////////////////////
// Full scene estimate
//
float3	SampleSecondaryLight( float3 _wsPosition, float3 _wsNormal, float3 _wsView, uint _objectIndex, uint2 _seeds ) {

	// Prepare surface characteristics
	float3	rho, F0;
	float	alphaS, alphaD;
	GetObjectInfo( _objectIndex, alphaS, F0, alphaD, rho );
	float3	IOR = Fresnel_IORFromF0( F0 );

	#if FORCE_BRDF != 1
		if ( _objectIndex == 1 ) {
			// Plane was hit, sample diffuse sky

			// Sample incoming radiance
			float3	Li = SampleSky( _wsNormal, 8 );						// Incoming "diffuse" lighting
//					Li *= ComputeShadow( _wsPosition, wsLight );		// * plane shadowing <== CAN'T! No clear direction! Should be coming from indirect samples instead of 1 unique diffuse sample. Or use AO but we don't have it...
					Li *= 1-ComputeSphereAO( _wsPosition, _wsNormal );	// * Sphere AO

			// Compute reflected radiance
			float3	Lr = Li * (rho / PI);								// Diffuse reflectance

			float	LdotN = 1;//saturate( -dot( _wsNormal, _wsView ) );		// cos( theta )

			const float dw = PI;
			return Lr * LdotN * dw;
		}
	#endif

	////////////////////////////////////////////////////////////
	// Sphere was hit, use many samples but don't cast rays anymore (we approximate scene intersection depending on ray's upward direction)

	// Build tangent space
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( _wsNormal, wsTangent, wsBiTangent );

	float3	tsView = -float3( dot( _wsView, wsTangent ), dot( _wsView, wsBiTangent ), dot( _wsView, _wsNormal ) );

	float	u = frac( _time + _seeds.x * 2.3283064365386963e-10 );

//	uint	totalGroupsCount = _groupsCount * SAMPLES_COUNT;
	uint	totalGroupsCount = SAMPLES_COUNT;

	float3	Lo = 0.0;
	uint	groupIndex = _groupIndex;
//	uint	validSamplesCount = 0;

	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		float	X0 = frac( u + float(groupIndex) / totalGroupsCount );
		float	X1 = (ReverseBits( groupIndex ) ^ _seeds.y) * 2.3283064365386963e-10; // / 0x100000000
		groupIndex += _groupsCount;	// Giant leaps give us large changes

		// Retrieve light direction + solid angle
		float	dw;
		float3	tsLight = UV2Direction( float2( X0, X1 ), dw );
		float3	wsLight = tsLight.x * wsTangent + tsLight.y * wsBiTangent + tsLight.z * _wsNormal;

		float	LdotN = tsLight.z;

		// Compute BRDF
		float3	BRDF = 0.0;
		#if FORCE_BRDF == 1
			BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR );
		#elif FORCE_BRDF == 2
			BRDF = ComputeBRDF_Oren( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho );
		#else
			if ( _objectIndex == 0 ) {
				#if DIELECTRIC_SPHERE
					BRDF = ComputeBRDF_Full( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR, alphaD, rho );
				#else
					BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR );
				#endif
			} else {
				BRDF = ComputeBRDF_Oren( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho );
			}
		#endif

		// Sample incoming radiance
		#if FORCE_BRDF == 1
			float3	Li = wsLight.y > 0.0 ? SampleSky( wsLight, 3.0 )								// Assume sky hit when sampling upward
										 : 0.0;														// Assume black otherwise
		#else
			float3	Li = wsLight.y > 0.0 ? SampleSky( wsLight, 3.0 )								// Assume sky hit when sampling upward
										 : (ALBEDO_PLANE / PI) * SampleSky( float3( 0, 1, 0 ), 8 );	// Assume ground hit when sampling downward (apply plane reflectance * diffuse IBL intensity)
		#endif

		// Compute reflected radiance
		float3	Lr = Li * BRDF;

		// Accumulate
		Lo += Lr * LdotN * dw;
	}

	Lo /= SAMPLES_COUNT;

	return Lo;
}

float3	ComputeIncomingRadiance( float3 _wsPosition, float3 _wsView, uint2 _seeds ) {
	#if FULL_SCENE
		// Compute secondary hit with scene
		float3	wsClosestPosition = 0;
		float3	wsNormal = float3( 0, 1, 0 );
		float2	hit = RayTraceScene( _wsPosition, _wsView, wsNormal, wsClosestPosition );
		if ( hit.x > 1e4 )
			return SampleSky( _wsView, 0.0 );	// Sample sky

		// Sample reflection from secondary hit
		_wsPosition += hit.x * _wsView;	// Go to hit
		_wsPosition += 1e-3 * wsNormal;	// Offset from surface

		return SampleSecondaryLight( _wsPosition, wsNormal, _wsView, hit.y, _seeds );

	#else
		// Sample sky incoming radiance (ignore interreflections with scene)
		return SampleSky( _wsView, 0.0 );
	#endif
}


float4	PS( VS_IN _In ) : SV_TARGET0 {

//return float4( (1 - _tex_GGX_Eo.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 )).xxx, 1 );
//return float4( (_tex_GGX_Eavg.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 ) / PI).xxx, 1 );
//return float4( (1 - _tex_OrenNayar_Eo.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 )).xxx, 1 );
//float	Eavg = _tex_OrenNayar_Eavg.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 );
//return float4( Eavg <= PI ? (Eavg / PI).xxx : float3( 1, 0, 0 ), 1 );

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
	if ( hit.x > 1e4 )
		return float4( SampleSky( wsView, 0.0 ), 1 );	// No hit

	wsPosition += hit.x * wsView;
	wsPosition += 1e-3 * wsNormal;	// Offset from surface

	// Build tangent space
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

	float3	tsView = -float3( dot( wsView, wsTangent ), dot( wsView, wsBiTangent ), dot( wsView, wsNormal ) );

//return float4( SampleSky( wsNormal, 0.0 ), 1 );
//return wsNormal;
//return float4( tsView, 1 );

	// Build environment SH as an array
	float3	envSH[9] = { _SH[0].xyz, _SH[1].xyz, _SH[2].xyz, _SH[3].xyz, _SH[4].xyz, _SH[5].xyz, _SH[6].xyz, _SH[7].xyz, _SH[8].xyz };

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
		float3	BRDF = 0.0;
		#if FORCE_BRDF == 1
			BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR );
		#elif FORCE_BRDF == 2
			BRDF = ComputeBRDF_Oren( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho );
		#else
			if ( hit.y == 0 ) {
				#if DIELECTRIC_SPHERE
					BRDF = ComputeBRDF_Full( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR, alphaD, rho );
				#else
					BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR );
				#endif
			} else {
				BRDF = ComputeBRDF_Oren( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho );
			}
		#endif

		// Sample incoming radiance
//		float3	Li = ComputeIncomingRadiance( wsPosition, wsLight, seeds );
		float3	Li = EvaluateSHRadiance( wsLight, envSH );

		// Compute reflected radiance
		float3	Lr = Li * BRDF;


#if WHITE_FURNACE_TEST
Lr *= 0.9;	// Attenuate a bit to see in front of white sky...
#endif

		// Accumulate
		Lo += Lr * LdotN * dw;
		validSamplesCount++;
    }

//	Lo += AMBIENT;

//	return float4( Lo, SAMPLES_COUNT);
	return float4( Lo, validSamplesCount );
}

/////////////////////////////////////////////////////////////////////////////////////////////
// Finalize rendering
//
Texture2D< float4 >		_tex_Accumulator : register( t6 );

float3	PS_Finalize( VS_IN _In ) : SV_TARGET0 {
	float4	V = _tex_Accumulator[_In.__position.xy];

//return 0.5*V.w / 271.0;
//return 0.001 * V.w;
//return 0.5 * V.xyz;

			V *= V.w > 0.0 ? 1.0 / V.w : 1.0;

//V *= 0.5;

	return V.xyz;
}
