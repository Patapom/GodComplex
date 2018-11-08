#include "Global.hlsl"
#include "FGD.hlsl"
#include "BRDF.hlsl"

//#define FULL_SCENE		1	// Define this to render the full scene (diffuse plane + specular sphere)
#include "Scene.hlsl"


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

TextureCube< float3 >	_tex_CubeMap : register( t0 );
Texture2D< float >		_tex_BlueNoise : register( t1 );

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

float3	SampleSkyRadiance( float3 _wsDirection, float _mipLevel ) {

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
// Full scene estimate
//
float3	SampleSecondaryLight( float3 _wsPosition, float3 _wsNormal, float3 _wsView, uint _objectIndex, uint2 _seeds ) {

	// Prepare surface characteristics
	float3	rho, F0;
	float	alphaS, alphaD;
	GetObjectInfo( _objectIndex, alphaS, F0, alphaD, rho );

	#if FORCE_BRDF != 1
		if ( _objectIndex == 1 ) {
			// Plane was hit, sample diffuse sky

			// Sample incoming projected irradiance
			float3	Ei = SampleSkyRadiance( _wsNormal, 8 );						// Incoming "diffuse" lighting
//					Ei *= ComputeShadow( _wsPosition, wsLight );		// * plane shadowing <== CAN'T! No clear direction! Should be coming from indirect samples instead of 1 unique diffuse sample. Or use AO but we don't have it...
					Ei *= 1-ComputeSphereAO( _wsPosition, _wsNormal );	// * Sphere AO

			const float dw = PI;
			float	LdotN = 1;//saturate( -dot( _wsNormal, _wsView ) );		// cos( theta )
			Ei *= dw;
			Ei *= LdotN;

			// Compute diffusely reflected radiance
			float3	Lr = Ei * (rho / PI);

			return Lr;
		}
	#endif

	////////////////////////////////////////////////////////////
	// Sphere was hit, use many samples but don't cast rays anymore (we approximate scene intersection depending on ray's upward direction)

	// Build tangent space
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( _wsNormal, wsTangent, wsBiTangent );

	float3	tsView = -float3( dot( _wsView, wsTangent ), dot( _wsView, wsBiTangent ), dot( _wsView, _wsNormal ) );

	float	u = frac( _time + _seeds.x * 2.3283064365386963e-10 );

	bool	enableMS = _flags & 0x1U;
	bool	enableMSSaturation = _flags & 0x2U;

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
			BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, F0, enableMS, enableMSSaturation );
		#elif FORCE_BRDF == 2
			BRDF = ComputeBRDF_OrenNayar( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho, enableMS, enableMSSaturation );
		#else
			if ( _objectIndex == 0 ) {
				#if DIELECTRIC_SPHERE
					BRDF = ComputeBRDF_Full( float3( 0, 0, 1 ), tsView, tsLight, alphaS, F0, alphaD, rho, enableMS, enableMSSaturation );
				#else
					BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, F0, enableMS, enableMSSaturation );
				#endif
			} else {
				BRDF = ComputeBRDF_OrenNayar( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho, enableMS, enableMSSaturation );
			}
		#endif

		// Sample incoming projected irradiance
		#if FORCE_BRDF == 1
			float3	Ei = wsLight.y > 0.0 ? SampleSkyRadiance( wsLight, 3.0 )								// Assume sky hit when sampling upward
										 : 0.0;														// Assume black otherwise
		#else
			float3	Ei = wsLight.y > 0.0 ? SampleSkyRadiance( wsLight, 3.0 )								// Assume sky hit when sampling upward
										 : (ALBEDO_PLANE / PI) * SampleSkyRadiance( float3( 0, 1, 0 ), 8 );	// Assume ground hit when sampling downward (apply plane reflectance * diffuse IBL intensity)
		#endif
		Ei *= dw;
		Ei *= LdotN;

		// Compute reflected radiance
		float3	Lr = Ei * BRDF;

		// Accumulate
		Lo += Lr;
	}

	Lo /= SAMPLES_COUNT;

	return Lo;
}

float3	ComputeIncomingRadiance( float3 _wsPosition, float3 _wsView, uint2 _seeds ) {
	#ifdef FULL_SCENE
		// Compute secondary hit with scene
		float3	wsClosestPosition = 0;
		float3	wsNormal = float3( 0, 1, 0 );
		float2	hit = RayTraceScene( _wsPosition, _wsView, wsNormal, wsClosestPosition );
		if ( hit.x > 1e4 )
			return SampleSkyRadiance( _wsView, 0.0 );	// Sample sky

		// Sample reflection from secondary hit
		_wsPosition += hit.x * _wsView;	// Go to hit
		_wsPosition += 1e-3 * wsNormal;	// Offset from surface

		return SampleSecondaryLight( _wsPosition, wsNormal, _wsView, hit.y, _seeds );

	#else
		// Sample sky incoming radiance (ignore interreflections with scene)
		return SampleSkyRadiance( _wsView, 0.0 );
	#endif
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
	if ( hit.x > 1e4 )
		return float4( SampleSkyRadiance( wsView, 0.0 ), 1 );	// No hit

	wsPosition += hit.x * wsView;
	wsPosition += 1e-3 * wsNormal;	// Offset from surface

	// Build tangent space
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

	float3	tsView = -float3( dot( wsView, wsTangent ), dot( wsView, wsBiTangent ), dot( wsView, wsNormal ) );

//return float4( SampleSkyRadiance( wsNormal, 0.0 ), 1 );
//return wsNormal;
//return float4( tsView, 1 );

	// Prepare surface characteristics
	float3	rho, F0;
	float	alphaS, alphaD;
	GetObjectInfo( hit.y, alphaS, F0, alphaD, rho );

	float	u = seeds.x * 2.3283064365386963e-10;

	bool	enableMS = _flags & 0x1U;
	bool	enableMSSaturation = _flags & 0x2U;

	float3	Lo = 0.0;
	uint	groupIndex = _groupIndex;
	uint	validSamplesCount = 0;

	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
        float	X0 = frac( u + float(groupIndex) / totalGroupsCount );
        float	X1 = (ReverseBits( groupIndex ) ^ seeds.y) * 2.3283064365386963e-10; // / 0x100000000
		groupIndex += _groupsCount;	// Giant leaps give us large changes

		// Retrieve light direction + solid angle
		#if 1
			// Generate a ray in tangent space
			float	dw;
			float3	tsLight = UV2Direction( float2( X0, X1 ), dw );
			float3	wsLight = tsLight.x * wsTangent + tsLight.y * wsBiTangent + tsLight.z * wsNormal;

			float	LdotN = tsLight.z;

		#elif 1
			// Generate a ray in world space + discard if not in correct hemisphere (what a waste!=

			// Uniform sphere sampling from SH gritty details
			float	theta = 2.0 * acos( sqrt( X1 ) );
			float	phi = 2.0 * PI * X0;
			float2	scTheta, scPhi;
			sincos( theta, scTheta.x, scTheta.y );
			sincos( phi, scPhi.x, scPhi.y );

			float3	wsLight = float3( scPhi.yx * scTheta.x, scTheta.y );
			float3	tsLight = float3( dot( wsLight, wsTangent ), dot( wsLight, wsBiTangent ), dot( wsLight, wsNormal ) );
			if ( tsLight.z <= 0.0 )
				continue;	// Below the surface

			float	dw = 4.0 * PI;	// Uniform sampling

			float	LdotN = tsLight.z;

		#else
			// Disney people generate wsLight first then transform into tangent space
			X0 *= 6.0;
			uint	faceIndex = floor( X0 );
			X0 -= faceIndex;
			float2	faceUV = 2.0 * float2( X0, X1 ) - 1.0;
			float3	wsLight;
			if ( faceIndex < 2 ) {
				float	s = faceIndex == 0 ? -1 : +1;
				wsLight = float3( s, faceUV.y, -s*faceUV.x );
			} else if ( faceIndex < 4 ) {
				float	s = faceIndex == 2 ? -1 : +1;
				wsLight = float3( faceUV.x, s, -s*faceUV.y );
			} else {
				float	s = faceIndex == 4 ? -1 : +1;
				wsLight = float3( s*faceUV.x, faceUV.y, s );
			}

			float3	tsLight = float3( dot( wsLight, wsTangent ), dot( wsLight, wsBiTangent ), dot( wsLight, wsNormal ) );
			if ( tsLight.z <= 0.0 )
				continue;	// Below the surface

			// Normalize
			float	tsLightLength = length( tsLight );
			tsLight /= tsLightLength;

			// dA (area of cube) = (6*2*2)/N  (Note: divide by N happens later)
			// dw = dA / r^3 = 24 * pow(x*x + y*y + z*z, -1.5) (see pbrt v2 p 947).
			float dw = 24 / (tsLightLength * tsLightLength * tsLightLength);

//			wsLight = normalize( wsLight );

			float	LdotN = tsLight.z;
		#endif

		// Compute BRDF
		float3	BRDF = 0.0;
		#if FORCE_BRDF == 1
			BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, F0, enableMS, enableMSSaturation );
		#elif FORCE_BRDF == 2
			BRDF = ComputeBRDF_OrenNayar( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho, enableMS, enableMSSaturation );
		#else
			if ( hit.y == 0 ) {
				#if DIELECTRIC_SPHERE
					BRDF = ComputeBRDF_Full( float3( 0, 0, 1 ), tsView, tsLight, alphaS, F0, alphaD, rho, enableMS, enableMSSaturation );
				#else
					BRDF = ComputeBRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, alphaS, F0, enableMS, enableMSSaturation );
				#endif
			} else {
				BRDF = ComputeBRDF_OrenNayar( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho, enableMS, enableMSSaturation );
			}
		#endif

		// Sample incoming projected irradiance
		float3	Ei = ComputeIncomingRadiance( wsPosition, wsLight, seeds );
				Ei *= dw;		// Irradiance
				Ei *= LdotN;	// Projected irradiance

		// Compute reflected radiance
		float3	Lr = Ei * BRDF;


#if WHITE_FURNACE_TEST
Lr *= 0.9;	// Attenuate a bit to see in front of white sky...
#endif

		// Accumulate
		Lo += Lr;
		validSamplesCount++;
    }

//	Lo += AMBIENT;

//	return float4( Lo, SAMPLES_COUNT);
	return float4( Lo, validSamplesCount );
}
