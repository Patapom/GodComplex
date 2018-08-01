#include "Global.hlsl"
#include "Scene.hlsl"
#include "LTC.hlsl"

//#define	WHITE_FURNACE_TEST	1

//#define	MS_ONLY	1	// Show only multiple scattering contribution

static const uint	SAMPLES_COUNT = 32;

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


/////////////////////////////////////////////////////////////////////////////////////////////
// Area Light Positionning

static const float3	AREA_LIGHT_POSITION = float3( -4, 2, 0 );
static const float3	AREA_LIGHT_RIGHT = float3( 0, 0, -1 );
static const float3	AREA_LIGHT_UP = float3( 0, 1, 0 );
static const float3	AREA_LIGHT_NORMAL = float3( 1, 0, 0 );
static const float2	AREA_LIGHT_HALF_SIZE = float2( 1, 2 );

static const float3	AREA_LIGHT_POWER = 20 * float3( 1, 1, 1 );	// Lumens


// Retrieves object information from its index
void	GetObjectInfo( uint _objectIndex, out float _roughnessSpecular, out float3 _objectF0, out float _roughnessDiffuse, out float3 _rho ) {
	_roughnessSpecular = max( 0.01, _objectIndex == 0 ? _roughnessSphereSpecular : _roughnessGround );
	_objectF0 = _objectIndex == 0 ? F0_TINT_SPHERE : F0_TINT_PLANE;

	_roughnessDiffuse = max( 0.01, _objectIndex == 0 ? _roughnessSphereDiffuse : _roughnessGround );
	_rho = _objectIndex == 0 ? ALBEDO_SPHERE : ALBEDO_PLANE;
}


////////////////////////////////////////////////////////////////////////////////////////////////
//
float3	ComputeBRDF_Oren( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughness, float3 _albedo ) {
	float3	BRDF = _albedo * BRDF_OrenNayar( _tsNormal, _tsView, _tsLight, _roughness );
	if ( (_flags & 1) && !(_flags & 0x100) ) {
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


////////////////////////////////////////////////////////////////////////////////////////////////

#if 0


//-----------------------------------------------------------------------------
// EvaluateBSDF_Area - Reference
//-----------------------------------------------------------------------------

void SampleRectangle(real2   u,
                     real4x4 localToWorld,
                     real    width,
                     real    height,
                 out real    lightPdf,
                 out real3   P,
                 out real3   Ns)
{
    // Random point at rectangle surface
    P = real3((u.x - 0.5) * width, (u.y - 0.5) * height, 0);
    Ns = real3(0, 0, -1); // Light down (-Z)

    // Transform to world space
    P = mul(real4(P, 1.0), localToWorld).xyz;
    Ns = mul(Ns, (real3x3)(localToWorld));

    // pdf is inverse of area
    lightPdf = 1.0 / (width * height);
}

void IntegrateBSDF_AreaRef(float3 V, float3 positionWS,
                           PreLightData preLightData, LightData lightData, BSDFData bsdfData,
                           out float3 diffuseLighting, out float3 specularLighting,
                           uint sampleCount = 512)
{
    // Add some jittering on Hammersley2d
    float2 randNum = InitRandom(V.xy * 0.5 + 0.5);

    diffuseLighting = float3(0.0, 0.0, 0.0);
    specularLighting = float3(0.0, 0.0, 0.0);

    for (uint i = 0; i < sampleCount; ++i)
    {
        float3 P = float3(0.0, 0.0, 0.0);   // Sample light point. Random point on the light shape in local space.
        float3 Ns = float3(0.0, 0.0, 0.0);  // Unit surface normal at P
        float lightPdf = 0.0;               // Pdf of the light sample

        float2 u = Hammersley2d(i, sampleCount);
        u = frac(u + randNum);

        // Lights in Unity point backward.
        float4x4 localToWorld = float4x4(float4(lightData.right, 0.0), float4(lightData.up, 0.0), float4(-lightData.forward, 0.0), float4(lightData.positionRWS, 1.0));

        switch (lightData.lightType)
        {
            //case GPULIGHTTYPE_SPHERE:
            //    SampleSphere(u, localToWorld, lightData.size.x, lightPdf, P, Ns);
            //    break;
            //case GPULIGHTTYPE_HEMISPHERE:
            //    SampleHemisphere(u, localToWorld, lightData.size.x, lightPdf, P, Ns);
            //    break;
            //case GPULIGHTTYPE_CYLINDER:
            //    SampleCylinder(u, localToWorld, lightData.size.x, lightData.size.y, lightPdf, P, Ns);
            //    break;
            case GPULIGHTTYPE_RECTANGLE:
                SampleRectangle(u, localToWorld, lightData.size.x, lightData.size.y, lightPdf, P, Ns);
                break;
            //case GPULIGHTTYPE_DISK:
            //    SampleDisk(u, localToWorld, lightData.size.x, lightPdf, P, Ns);
            //   break;
            // case GPULIGHTTYPE_LINE: handled by a separate function.
        }

        // Get distance
        float3 unL = P - positionWS;
        float sqrDist = dot(unL, unL);
        float3 L = normalize(unL);

        // Cosine of the angle between the light direction and the normal of the light's surface.
        float cosLNs = saturate(dot(-L, Ns));

        // We calculate area reference light with the area integral rather than the solid angle one.
        float LdotN = saturate(dot(bsdfData.normalWS, L));
        float illuminance = cosLNs * LdotN / (sqrDist * lightPdf);

        float3 localDiffuseLighting = float3(0.0, 0.0, 0.0);
        float3 localSpecularLighting = float3(0.0, 0.0, 0.0);

        if (illuminance > 0.0)
        {
            BSDF(V, L, LdotN, positionWS, preLightData, bsdfData, localDiffuseLighting, localSpecularLighting);
            localDiffuseLighting *= lightData.color * illuminance * lightData.diffuseScale;
            localSpecularLighting *= lightData.color * illuminance * lightData.specularScale;
        }

        diffuseLighting += localDiffuseLighting;
        specularLighting += localSpecularLighting;
    }

    diffuseLighting /= float(sampleCount);
    specularLighting /= float(sampleCount);
}

#endif

float2	RayTraceScene2( float3 _wsPos, float3 _wsDir, out float3 _wsNormal, out float3 _wsClosestPosition ) {
	float2	hit = RayTraceScene( _wsPos, _wsDir, _wsNormal, _wsClosestPosition );

	// Check hit against area rectangle
	_wsPos -= AREA_LIGHT_POSITION;
	float	t = -dot( _wsPos, AREA_LIGHT_NORMAL ) / dot( _wsDir, AREA_LIGHT_NORMAL );
	if ( t > 0.0 && t < hit.x ) {
		// Investigate hit
		_wsPos += t * _wsDir;
		float2	lsHitDistance = float2( dot( _wsPos, AREA_LIGHT_RIGHT ), dot( _wsPos, AREA_LIGHT_UP ) );
		if ( all( abs( lsHitDistance ) < AREA_LIGHT_HALF_SIZE ) ) {
			hit = float2( t, 10 );	// New hit: the area light rectangle!
		}
	}

	return hit;	// No hit...
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
	float2	hit = RayTraceScene2( wsPosition, wsView, wsNormal, wsClosestPosition );
	if ( hit.x > 1e4 )
		return float4( 0, 0, 0, 1 );
//		return float4( SampleSky( wsView, 0.0 ), 1 );	// No hit or plane hit (we only render the sphere here)
	if ( hit.y == 10 )
		return float4( AREA_LIGHT_POWER / (4.0 * AREA_LIGHT_HALF_SIZE.x * AREA_LIGHT_HALF_SIZE.y), 1 );

	wsPosition += hit.x * wsView;
	wsPosition += 1e-3 * wsNormal;	// Offset from surface

	// Build tangent space
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

	float3	tsView = -float3( dot( wsView, wsTangent ), dot( wsView, wsBiTangent ), dot( wsView, wsNormal ) );

	// Prepare surface characteristics
	float3	rho, F0;
	float	alphaS, alphaD;
	GetObjectInfo( hit.y, alphaS, F0, alphaD, rho );

	float3	IOR = Fresnel_IORFromF0( F0 );

	float	u = seeds.x * 2.3283064365386963e-10;

	float3	Lo = 0.0;
	uint	validSamplesCount = 0;

//	if ( (_flags & 1) && !(_flags & 0x100) ) {
	if ( 1 ) {
		uint	groupIndex = _groupIndex;

		float	lightPDF = 1.0 / (4.0 * AREA_LIGHT_HALF_SIZE.x * AREA_LIGHT_HALF_SIZE.y);

//return float4( wsAreaLightNormal, 1 );

		[loop]
		for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
			float	X0 = frac( u + float(groupIndex) / totalGroupsCount );
			float	X1 = (ReverseBits( groupIndex ) ^ seeds.y) * 2.3283064365386963e-10; // / 0x100000000
			groupIndex += _groupsCount;	// Giant leaps give us large changes

			// Retrieve light direction + solid angle
			float3	wsAreaLightPosition = AREA_LIGHT_POSITION
										+ (2.0 * X0 - 1.0) * AREA_LIGHT_HALF_SIZE.x * AREA_LIGHT_RIGHT
										+ (2.0 * X1 - 1.0) * AREA_LIGHT_HALF_SIZE.y * AREA_LIGHT_UP;

			float3	wsLight = wsAreaLightPosition - wsPosition;
			float	r2 = dot( wsLight, wsLight );
					wsLight /= sqrt(r2);

			float3	tsLight = float3( dot( wsLight, wsTangent ), dot( wsLight, wsBiTangent ), dot( wsLight, wsNormal ) );
			float	LdotN = tsLight.z;
			if ( LdotN <= 0.0 )
				continue;

			float	cosLNs = saturate( -dot( wsLight, AREA_LIGHT_NORMAL ) );	// Cosine of the angle between the light direction and the normal of the light's surface.

			// We calculate area reference light with the area integral rather than the solid angle one.
			float3	Li = AREA_LIGHT_POWER * cosLNs / (r2 * lightPDF);

//Li = AREA_LIGHT_POWER * saturate( -dot( wsLight, wsAreaLightNormal ) ) / (r2 * lightPDF);

			// Compute BRDF
			float3	BRDF = 0.0;
			if ( hit.y == 0 ) {
				BRDF = ComputeBRDF_Full( float3( 0, 0, 1 ), tsView, tsLight, alphaS, IOR, alphaD, rho );
			} else {
				BRDF = ComputeBRDF_Oren( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho );
			}

			// Compute reflected radiance
			float3	Lr = Li * BRDF;


#if WHITE_FURNACE_TEST
Lr *= 0.9;	// Attenuate a bit to see in front of white sky...
#endif

			// Accumulate
			Lo += Lr * LdotN;
			validSamplesCount++;
		}
	}

//	///////////////////////////////////////////////////////////////////////////////////////
//	// Real-time Approximation
//	if ( (_flags & 1) && (_flags & 0x100) ) {
//		float3	wsReflectedView = reflect( wsView, wsNormal );
//		float3	wsReflected = normalize( lerp( wsReflectedView, wsNormal, alphaS ) );	// Go more toward prefectly reflected direction when roughness drops to 0
//
//wsReflected = wsNormal;
//
//		Lo += validSamplesCount * EstimateMSIrradiance_SH( wsNormal, wsReflected, saturate( tsView.z ), alphaS, IOR, alphaD, rho, envSH );
//	}

//	return float4( Lo, SAMPLES_COUNT);
	return float4( Lo, validSamplesCount );
}
