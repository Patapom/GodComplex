/////////////////////////////////////////////////////////////////////////////////////////////
// Renders a MSBRDF with a simple directional light
// An attempt to show that the effect is negligible and should only be computed for large area lights
/////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "FGD.hlsl"
#include "BRDF.hlsl"

#define FULL_SCENE		1	// Define this to render the full scene (diffuse plane + specular sphere)
#include "Scene.hlsl"

//#define	WHITE_FURNACE_TEST	1


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

	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * float3( 1, 0.765557, 0.336057 ))	// Gold (from https://seblagarde.wordpress.com/2011/08/17/feeding-a-physical-based-lighting-mode/)
//	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * float3( 0.336057, 0.765557, 1 ))
//	#define	F0_TINT_SPHERE	(_reflectanceSphereSpecular * 1.0)
	#define	F0_TINT_PLANE	(_reflectanceGround * 1.0)
#endif


cbuffer CB_Render : register(b2) {
	uint		_flags;		// 0x1 = Enable MSBRDF
							// 0x2 = Enable MS Factor
							// 0x100 = Use realtime approximation
							// 0x200 = Use LTC

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

static const float3	LIGHT_IRRADIANCE = 5.0f * float3( 1, 1, 1 );	// In lm/m² (lux)

TextureCube< float3 >	_tex_CubeMap : register( t0 );
Texture2D< float >		_tex_BlueNoise : register( t1 );

/////////////////////////////////////////////////////////////////////////////////////////////
// Retrieves object information from its index
void	GetObjectInfo( uint _objectIndex, out float _roughnessSpecular, out float3 _objectF0, out float _roughnessDiffuse, out float3 _rho ) {
	_roughnessSpecular = max( 0.01, _objectIndex == 0 ? _roughnessSphereSpecular : _roughnessGround );
	_objectF0 = _objectIndex == 0 ? F0_TINT_SPHERE : F0_TINT_PLANE;

	_roughnessDiffuse = max( 0.01, _objectIndex == 0 ? _roughnessSphereDiffuse : _roughnessGround );
	_rho = _objectIndex == 0 ? ALBEDO_SPHERE : ALBEDO_PLANE;
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


float4	PS( VS_IN _In ) : SV_TARGET0 {

	float	noise = 0*_tex_BlueNoise[uint2(_In.__position.xy + uint2( _groupIndex, 0 )) & 0x3F];

	float2	UV = float2( _screenSize.x / _screenSize.y * (2.0 * (_In.__position.x+noise) / _screenSize.x - 1.0), 1.0 - 2.0 * _In.__position.y / _screenSize.y );

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

	// Build light direction
	float3	wsLight = float3( -sin( _lightElevation ), cos( _lightElevation ), 0 );

	// Build tangent space
	float3	wsTangent, wsBiTangent;
	BuildOrthonormalBasis( wsNormal, wsTangent, wsBiTangent );

	float3	tsView = -float3( dot( wsView, wsTangent ), dot( wsView, wsBiTangent ), dot( wsView, wsNormal ) );
	float3	tsLight = float3( dot( wsLight, wsTangent ), dot( wsLight, wsBiTangent ), dot( wsLight, wsNormal ) );

	// Prepare surface characteristics
	float3	rho, F0;
	float	alphaS, alphaD;
	GetObjectInfo( hit.y, alphaS, F0, alphaD, rho );

	bool	enableMS = _flags & 0x1U;
	bool	enableMSSaturation = _flags & 0x2U;

	// Compute BRDF
	float3	BRDF = 0.0;
	if ( hit.y == 0 ) {
		BRDF = ComputeBRDF_Full( float3( 0, 0, 1 ), tsView, tsLight, alphaS, F0, alphaD, rho, enableMS, enableMSSaturation );
	} else {
		BRDF = ComputeBRDF_OrenNayar( float3( 0, 0, 1 ), tsView, tsLight, alphaD, rho, enableMS, enableMSSaturation );
	}

	// Sample incoming projected irradiance
	float	LdotN = saturate( tsLight.z );
	float3	Ei = _lightIntensity * LIGHT_IRRADIANCE;	// Illuminance in lm/m² (lux)
			Ei *= LdotN;								// projected illuminance

	// Compute reflected radiance
	float3	Lo = Ei * BRDF;


#if WHITE_FURNACE_TEST
Lo *= 0.9;	// Attenuate a bit to see in front of white sky...
#endif

	// Accumulate
	return float4( Lo, 1 );
}
