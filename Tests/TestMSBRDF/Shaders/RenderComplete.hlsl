#include "Global.hlsl"
#include "Scene.hlsl"

#define	BRDF_SPECULAR	1

static const uint	SAMPLES_COUNT = 32;

static const float3	IBL_INTENSITY = 2.0;

static const float3	ALBEDO_SPHERE = float3( 0.9, 0.5, 0.1 );	// Nicely saturated yellow
static const float3	ALBEDO_PLANE = float3( 0.9, 0.5, 0.1 );		// Nicely saturated yellow
static const float3	F0 = 1.0;
static const float3	AMBIENT = 0* 0.02 * float3( 0.5, 0.8, 0.9 );

cbuffer CB_Render : register(b2) {
	float	_roughnessSpecular;
	float	_roughnessDiffuse;
	float	_albedo;
	float	_lightElevation;

	uint	_flags;
	uint	_groupsCount;
	uint	_groupIndex;
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

	// Solid angle
	_solidAngleCosine = PI;

	#if 0
		float	sqSinTheta = saturate( _UV.y );
		float	sinTheta = sqrt( sqSinTheta );
		float	cosTheta = sqrt( 1.0 - sqSinTheta );

//_solidAngleCosine *= sinTheta;
	#else
		float	cosTheta = saturate( sqrt( _UV.y ) );
		float	sinTheta = sqrt( 1.0 - cosTheta*cosTheta );
	#endif

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

//return 1;

	return IBL_INTENSITY * _tex_CubeMap.SampleLevel( LinearWrap, _wsDirection, _mipLevel );
}


#if BRDF_SPECULAR	// Specular

float	GGX_NDF2( float _NdotH, float _alpha2 ) {
	float	den = PI * pow2( pow2( _NdotH ) * (_alpha2 - 1) + 1 );
	return _alpha2 * rcp( den );
}

// Warning: 1 / (4 * NdotL * NdotV) is already accounted for!
float	GGX_Smith2( float _NdotL, float _NdotV, float _alpha2 ) {
	float	denL = _NdotL + sqrt( pow2( _NdotL ) * (1-_alpha2) + _alpha2 );
	float	denV = _NdotV + sqrt( pow2( _NdotV ) * (1-_alpha2) + _alpha2 );
	return rcp( denL * denV );
}

float3	BRDF_GGX2( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _alpha, float3 _IOR ) {
	float	NdotL = dot( _tsNormal, _tsLight );
	float	NdotV = dot( _tsNormal, _tsView );
	if ( NdotL < 0.0 || NdotV < 0.0 )
		return 0.0;

	float	a2 = pow2( _alpha );
	float3	H = normalize( _tsView + _tsLight );
	float	NdotH = dot( H, _tsNormal );
	float	HdotL = dot( H, _tsLight );

	float	D = GGX_NDF2( NdotH, a2 );
	float	G = GGX_Smith2( NdotL, NdotV, a2 );
	float3	F = FresnelDielectric( _IOR, HdotL );

//return 0.1 * NdotL;
//return 0.1 * NdotV;
//return 0.1 * HdotL;
//return 0.1 * NdotH;

//D = 1;
//F = 1;
//G = 0.1;

//return F;
//return 4.0 * NdotL * NdotV * G;

	return max( 0.0, F * G * D );
}

float3	ComputeBRDF( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughness, float3 _IOR ) {
	float3	BRDF = BRDF_GGX2( _tsNormal, _tsView, _tsLight, _roughness, _IOR );
	if ( _flags & 1 ) {
		// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-the-fresnel-reflectance-f_0f_0
		float3		F0 = Fresnel_F0FromIOR( _IOR );
		const float	tau = 0.45549984771950014;
		float3		reducedF0 = tau * F0;
		float3		MSFactor = reducedF0 * (0.04 + reducedF0 * (0.66 + reducedF0 * 0.3));

		BRDF += MSFactor * MSBRDF( _roughness, _tsNormal, _tsView, _tsLight, _tex_GGX_Eo, _tex_GGX_Eavg );
	}

	return BRDF;
}

#else	// Diffuse

float3	ComputeBRDF( float3 _tsNormal, float3 _tsView, float3 _tsLight, float _roughness, float3 _albedo ) {
	float3	BRDF = _albedo * BRDF_OrenNayar( _tsNormal, _tsView, _tsLight, _roughness );
	if ( _flags & 1 ) {
		// From http://patapom.com/blog/BRDF/MSBRDFEnergyCompensation/#varying-diffuse-reflectance-rhorho
		const float	tau = 0.28430405702379613;
		const float	A1 = (1.0 - tau) / pow2( tau );
		float3		rho = tau * _albedo;
		float3		MSFactor = A1 * pow2( rho ) / (1.0 - rho);

		BRDF += MSFactor * MSBRDF( _roughness, _tsNormal, _tsView, _tsLight, _tex_OrenNayar_Eo, _tex_OrenNayar_Eavg );
	}

	return BRDF;
}
#endif


float4	PS( VS_IN _In ) : SV_TARGET0 {

//return float4( (1 - _tex_GGX_Eo.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 )).xxx, 1 );
//return float4( (_tex_GGX_Eavg.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 ) / PI).xxx, 1 );
//return float4( (1 - _tex_OrenNayar_Eo.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 )).xxx, 1 );
//return float4( (_tex_OrenNayar_Eavg.SampleLevel( LinearClamp, _In.__position.xy / _screenSize.xy, 0.0 ) / PI).xxx, 1 );

	float	noise = 0*_tex_BlueNoise[uint2(_In.__position.xy + uint2( _groupIndex, 0 )) & 0x3F];

	float2	UV = float2( _screenSize.x / _screenSize.y * (2.0 * (_In.__position.x+noise) / _screenSize.x - 1.0), 1.0 - 2.0 * _In.__position.y / _screenSize.y );

//	uint	seed1 = wang_hash( _screenSize.x * _In.__position.y + _In.__position.x );
	uint	seed1 = wang_hash( asuint(_In.__position.x + _time) + _groupIndex ) ^ wang_hash( asuint(_In.__position.y - _time*_groupIndex) );
    uint	seed2 = hash( seed1, 1000u );
//	float	noise = seed1 * 2.3283064365386963e-10;

	uint	totalGroupsCount = _groupsCount * SAMPLES_COUNT;

//return float4( seed1.xxx * 2.3283064365386963e-10, 1 );

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
//	if ( tsView.z <= 0.0 )
//		return float4( 0, 0, 0, 0 );

//return float4( SampleSky( wsNormal, 0.0 ), 1 );
//return wsNormal;
//return float4( tsView, 1 );

//	float	roughness = hit.y == 0  ? _roughnessSpecular
//									: _roughnessDiffuse;

	float	roughness = _roughnessSpecular;

	roughness = max( 0.01, roughness );

//roughness = pow2( roughness );

	float	u = seed1 * 2.3283064365386963e-10;

	#if BRDF_SPECULAR
		float3	IOR = Fresnel_IORFromF0( F0 );
	#else
//		float3	albedo = hit.y == 0 ? ALBEDO_SPHERE
//									: ALBEDO_PLANE;
		float3	albedo = _albedo;
	#endif

	float3	Lo = 0.0;
	uint	groupIndex = _groupIndex;
	uint	validSamplesCount = 0;

//return float4( 1.0*fmod( (ReverseBits( groupIndex ) ^ seed2) * 2.3283064365386963e-10, 1.0 ).xxx, 1 );
//return float4( 1.0*frac((ReverseBits( groupIndex ) ^ seed2) * 2.3283064365386963e-10).xxx, 1 );
//return float4( 1.0*((ReverseBits( groupIndex ) ^ seed2) * 2.3283064365386963e-10).xxx, 1 );
//return float4( 10*frac( u + float(groupIndex) / totalGroupsCount ).xxx, 1 );


	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
        float	X0 = frac( u + float(groupIndex) / totalGroupsCount );
        float	X1 = (ReverseBits( groupIndex ) ^ seed2) * 2.3283064365386963e-10; // / 0x100000000
		groupIndex += _groupsCount;	// Giant leaps give us large changes

		// Retrieve light direction + solid angle
		#if 0
//			float	dw;
//			float3	tsLight = UV2Direction( float2( X0, X1 ), dw );
//			float3	wsLight = tsLight.x * wsTangent + tsLight.y * wsBiTangent + tsLight.z * wsNormal;

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

LdotN = 1.0;// * sqrt( 1.0 - tsLight.z*tsLight.z );

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

//LdotN = 0.25 * sqrt( 1.0 - tsLight.z*tsLight.z );
//LdotN *= 0.25 * sqrt( 1.0 - tsLight.z*tsLight.z );

		#endif

		// Get reflectance in that direction
		#if BRDF_SPECULAR
			float3	BRDF = ComputeBRDF( float3( 0, 0, 1 ), tsView, tsLight, roughness, IOR );
		#else
			float3	BRDF = ComputeBRDF( float3( 0, 0, 1 ), tsView, tsLight, roughness, albedo );
		#endif

		// Sample incoming radiance
		float3	Li = SampleSky( wsLight, 0.0 );

//Li = 2;
//LdotN = 1;
//dw = 1;
//BRDF = 1;


//float	a2 = pow2( roughness );
//float	NdotH = normalize( tsView + tsLight ).z;
//BRDF = a2 / (PI * pow2(NdotH*NdotH*(a2-1) + 1));

//IOR = Fresnel_IORFromF0( _albedo );
//BRDF = BRDF_GGX2( float3( 0, 0, 1 ), tsView, tsLight, roughness, IOR );

		Lo += Li * BRDF * LdotN * dw;
//Lo += BRDF * dw / (4 * PI);

		validSamplesCount++;

//Lo += LdotN;
//Lo += X0 / SAMPLES_COUNT;

/*		float	phiH = TWOPI * (noise+X0);
		float	sinPhiH, cosPhiH;
		sincos( phiH, sinPhiH, cosPhiH );

//		float	thetaH = atan( -roughness * roughness * log( 1.0 - X1 ) );		// Ward importance sampling
//		float	thetaH = hit.y == 0 ? atan( -roughness * sqrt( X1 ) / sqrt( 1.0 - X1 ) )	// GGX importance sampling
//									: acos( X1 );											// Lambert diffuse

		float	thetaH = acos( X1 );

		float	sinThetaH, cosThetaH;
		sincos( thetaH, sinThetaH, cosThetaH );

		float3	lsHalf = float3(	sinThetaH * cosPhiH,
									sinThetaH * sinPhiH,
									cosThetaH
								);

//lsHalf = float3( 0, 0, 1 );

		float3	wsHalf = lsHalf.x * wsTangent + lsHalf.y * wsBiTangent + lsHalf.z * wsNormal;

 		float3	wsLight = wsView - 2.0 * dot( wsView, wsHalf ) * wsHalf;	// Light is on the other size of the half vector...
		float	LdotN = dot( wsLight, wsNormal );
		if ( LdotN <= 0.0 )
			continue;	// Goes below the surface...

		// Sample lighting
		float3	wsClosestPositionLight;
		float3	wsNormalLight;
		float2	hit2 = RayTraceScene( wsPosition, wsLight, wsNormalLight, wsClosestPositionLight );
		float3	Li = 0.0;
		if ( hit2.x > 1e4 ) {
			// Sample sky
			Li = SampleSky( wsLight, 0.0 );
		} else {
			// Sample scene
			if ( hit2.y == 0 ) {
				// Sphere was hit, assume 0
				Li = 0.0;
			} else {
				// Plane was hit, sample diffuse 
				Li = SampleSky( wsNormalLight, 8 );			// Incoming "diffuse" lighting
//				Li *= ComputeShadow( wsPosition + hit2.x * wsLight, wsLight );			// * plane shadowing <== CAN'T! No clear direction! Should be coming from indirect samples instead of 1 unique diffuse sample. Or use AO but we don't have it...
				Li *= ComputeSphereAO( wsPosition + hit2.x * wsLight, wsNormalLight );	// * Sphere AO
				Li *= saturate( -dot( wsNormalLight, wsLight ) );						// * cos( theta )
				Li *= ALBEDO_PLANE / PI;												// * diffuse reflectance
			}
		}

		float	solidAngle = sqrt( 1.0 - LdotN*LdotN );

		// Apply BRDF
		float3	BRDF = hit.y == 0 ? BRDF_GGX( wsNormal, -wsView, wsLight, roughness, F0 )
								  : BRDF_OrenNayar( wsNormal, -wsView, wsLight, roughness );
//		float3	BRDF = BRDF_GGX( wsNormal, -wsView, wsLight, roughness, F0 );
//		float	BRDF = BRDF_OrenNayar( wsNormal, -wsView, wsLight, roughness );
		Lo += Li * BRDF * LdotN * solidAngle;
*/
    }

//	Lo += AMBIENT;

	return float4( Lo, SAMPLES_COUNT);
//	return float4( Lo, validSamplesCount );
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