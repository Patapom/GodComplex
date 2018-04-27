#include "Global.hlsl"
#include "Scene.hlsl"

static const float3	LIGHT_COLOR = 1.0;
static const float3	ALBEDO_SPHERE = float3( 0.9, 0.5, 0.1 );	// Nicely saturated yellow
static const float3	ALBEDO_PLANE = float3( 0.9, 0.5, 0.1 );		// Nicely saturated yellow
static const float3	F0 = 1.0;
static const float3	AMBIENT = 0* 0.02 * float3( 0.5, 0.8, 0.9 );

cbuffer CB_Render : register(b2) {
	float	_roughnessSpecular;
	float	_roughnessDiffuse;
	float	_albedo;
	float	_lightElevation;
};

TextureCube< float3 >	_tex_CubeMap : register( t0 );
Texture2D< float >		_tex_BlueNoise : register( t1 );

Texture2D< float >		_tex_IrradianceComplement : register( t2 );
Texture2D< float >		_tex_IrradianceAverage : register( t3 );

//////////////////////////////////////////////////////////////////////////////
// New multiple-scattering term computed from energy compensation

float3	ComputeLightingMS( float3 _albedo, float _roughness, float3 _F0, float3 _wsNormal, float3 _wsView, float3 _wsLight, float3 _lightColor ) {

	float3	rho = _albedo / PI;

	float	a = _roughness;

	float	mu_o = saturate( dot( _wsView, _wsNormal ) );
	float	mu_i = saturate( dot( _wsLight, _wsNormal ) );

	float	E_o = 1.0 - _tex_IrradianceComplement.SampleLevel( LinearClamp, float2( mu_o, a ), 0.0 );	// 1 - E_o
	float	E_i = 1.0 - _tex_IrradianceComplement.SampleLevel( LinearClamp, float2( mu_i, a ), 0.0 );	// 1 - E_i
	float	E_avg = _tex_IrradianceAverage.SampleLevel( LinearClamp, float2( a, 0.5 ), 0.0 );			// E_avg

	float3	BRDF_GGX_ms = E_o * E_i / (PI - E_avg);

//return 0.5 * E_i;
//return 0.5 * (PI - E_avg) / PI;
return BRDF_GGX_ms;

	float3	specular = BRDF_GGX_ms;
return 0*_lightColor * specular;

//	float	LdotN = saturate( dot( _wsLight, _wsNormal ) );
//	return (_albedo / PI) * saturate( LdotN ) * _lightColor;
	return 0.0;
}


// Converts a 2D UV into an upper hemisphere direction (tangent space) + solid angle
// The direction is weighted by the surface cosine lobe
float3	UV2Direction( float2 _UV, out float _solidAngleCosine ) {
	_UV.y += 1e-5;	// 0 is completely useless..

	// Solid angle
	_solidAngleCosine = PI;

	float	sqSinTheta = _UV.y;
	float	sinTheta = sqrt( sqSinTheta );
	float	cosTheta = sqrt( 1.0 - sqSinTheta );
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

	return 2.0 * _tex_CubeMap.SampleLevel( LinearWrap, _wsDirection, _mipLevel );
}

float3	PS( VS_IN _In ) : SV_TARGET0 {

	float2	UV = float2( _ScreenSize.x / _ScreenSize.y * (2.0 * _In.__Position.x / _ScreenSize.x - 1.0), 1.0 - 2.0 * _In.__Position.y / _ScreenSize.y );
	uint	seed1 = wang_hash( _ScreenSize.x * _In.__Position.y + _In.__Position.x );
    uint	seed2 = hash( seed1, 1000u );
//	float	noise = seed1 * 2.3283064365386963e-10;
	float	noise = _tex_BlueNoise[uint2(_In.__Position.xy) & 0x3F];

	float3	csView = normalize( float3( UV, 1 ) );
	float3	wsRight = _Camera2World[0].xyz;
	float3	wsUp = _Camera2World[1].xyz;
	float3	wsAt = _Camera2World[2].xyz;
	float3	wsView = csView.x * wsRight + csView.y * wsUp + csView.z * wsAt;
	float3	wsPosition = _Camera2World[3].xyz;

//return  0.9 * ReverseBits( _In.__Position.x, seed2 );


//return 1-_tex_IrradianceComplement.SampleLevel( LinearClamp, _In.__Position.xy / _ScreenSize.xy, 0.0 );
//return _tex_IrradianceAverage.SampleLevel( LinearClamp, _In.__Position.xy / _ScreenSize.xy, 0.0 ) / PI;

	float3	wsClosestPosition;
	float3	wsNormal;
	float2	hit = RayTraceScene( wsPosition, wsView, wsNormal, wsClosestPosition );
	if ( hit.x > 1e4 )
		return SampleSky( wsView, 0.0 );	// No hit

	wsPosition += hit.x * wsView;
	wsPosition += 1e-3 * wsNormal;	// Offset from surface

hit.y = 0;
//return wsView;

	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

//return SampleSky( wsNormal, 0.0 );

	float3	tsView = -float3( dot( wsView, wsTangent ), dot( wsView, wsBiTangent ), dot( wsView, wsNormal ) );
	if ( tsView.z <= 0.0 )
		return 0;

//return wsBiTangent;
//return tsView;

	const float	roughness = hit.y == 0  ? max( 0.01, _roughnessSpecular )
										: _roughnessDiffuse;

	const uint	SAMPLES_COUNT = 128;

	float3	Lo = 0.0;
	[loop]
	for ( uint i=0; i < SAMPLES_COUNT; i++ ) {
		float	X0 = float( noise + i ) / SAMPLES_COUNT;
		float	X1 = noise;//frac( ReverseBits( i, seed2 ) );
//		float	X1 = ReverseBits( 1+i, seed2 );

		#if 1
			// Importance sample half vector direction
			float2	scPhi;
			sincos( TWOPI * X0, scPhi.x, scPhi.y );
			float	thetaH = atan( roughness * sqrt( X1 ) / sqrt( 1.00001 - X1 ) );	// GGX importance sampling
			float2	scThetaH;
			sincos( thetaH, scThetaH.x, scThetaH.y );
			float3	tsHalf = float3( scThetaH.x * scPhi, scThetaH.y );

			// Generate light vector by mirroring view against half vector
			float	VdotH = dot( tsView, tsHalf );
			float3	tsLight = 2.0 * VdotH * tsHalf - tsView;

//			float	dw = 0.25 * PI * sinThetaH * VdotH;
			float	LdotN = tsLight.z;
			float	LdotH = VdotH;//dot( tsLight, tsHalf );
			float	HdotN = tsHalf.z;
			float	BRDF = LdotH * GGX_Smith( LdotH, VdotH, pow2( roughness ) )
						 / ( LdotN * HdotN );
//			float	BRDF = LdotH * GGX_Smith( LdotH, VdotH, pow2( roughness ) );
		#else
			// Retrieve light direction + solid angle
			float	dw;
			float3	tsLight = UV2Direction( float2( X0, X1 ), dw );

			// Get reflectance in that direction
//			float3	BRDF = BRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, roughness, F0 );
			float3	BRDF = BRDF_GGX( float3( 0, 0, 1 ), tsView, tsLight, roughness, F0 );

			BRDF *= tsLight.z * dw;

		#endif

		float3	wsLight = tsLight.x * wsTangent + tsLight.y * wsBiTangent + tsLight.z * wsNormal;

		// Sample incoming radiance
		float3	Li = SampleSky( wsLight, 0.0 );

		// Lo = Li * BRDF * (L.N) * dw
		Lo += Li * BRDF;

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

	Lo /= SAMPLES_COUNT;

	float3	color = Lo;

//color = 10;

	return AMBIENT + color;
}
