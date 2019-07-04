#include "Global.hlsl"
#include "FGD.hlsl"
#include "LTC.hlsl"
#include "BRDF.hlsl"

struct VS_IN {
	float4	__Position : SV_POSITION;
};

struct PS_OUT {
	float3	color : SV_TARGET0;
	float	depth : SV_DEPTH;
};

VS_IN	VS( VS_IN _in ) { return _in; }

// Computes the diffuse and specular luminance of the area light that is reflected from the surface
void	ComputeLTCAreaLightLuminance( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _alphaD, float _alphaS, out float3 _diffuse, out float3 _specular ) {
	// Build tangent space
	// Construct a right-handed view-dependent orthogonal basis around the normal
	float3	wsTangent = normalize( _wsView - _wsNormal * dot( _wsView, _wsNormal ) );
	float3	wsBiTangent = cross( _wsNormal, wsTangent );

	float3x3	world2TangentSpace = transpose( float3x3( wsTangent, wsBiTangent, _wsNormal ) );
//	float3		tsView = mul( _wsView, world2TangentSpace );

	float		VdotN = saturate( dot( _wsView, _wsNormal ) );

	float		perceptualAlphaD = sqrt( _alphaD );
	float		perceptualAlphaS = sqrt( _alphaS );

	float3x3	LTC_diffuse = LTCSampleMatrix( VdotN, perceptualAlphaD, LTC_BRDF_INDEX_OREN_NAYAR );
	float3x3	LTC_specular = LTCSampleMatrix( VdotN, perceptualAlphaS, LTC_BRDF_INDEX_GGX );
	float		magnitude_diffuse = SampleIrradiance( VdotN, _alphaD, FGD_BRDF_INDEX_OREN_NAYAR );
	float3		magnitude_specular = SampleIrradiance( VdotN, _alphaS, FGD_BRDF_INDEX_GGX );

	// Build rectangular area light corners in local space
	float3		lsAreaLightPosition = _wsLight2World[3].xyz - _wsPosition;
	float4x3    lsLightCorners;
				lsLightCorners[0] = lsAreaLightPosition + _wsLight2World[0].w * _wsLight2World[0].xyz + _wsLight2World[1].w * _wsLight2World[1].xyz;
				lsLightCorners[1] = lsAreaLightPosition + _wsLight2World[0].w * _wsLight2World[0].xyz - _wsLight2World[1].w * _wsLight2World[1].xyz;
				lsLightCorners[2] = lsAreaLightPosition - _wsLight2World[0].w * _wsLight2World[0].xyz - _wsLight2World[1].w * _wsLight2World[1].xyz;
				lsLightCorners[3] = lsAreaLightPosition - _wsLight2World[0].w * _wsLight2World[0].xyz + _wsLight2World[1].w * _wsLight2World[1].xyz;

	float4x3    tsLightCorners = mul( lsLightCorners, world2TangentSpace );		// Transform them into tangent-space

	float3		Li = _diskLuminance;

	_diffuse = Li * magnitude_diffuse * PolygonIrradiance( mul( tsLightCorners, LTC_diffuse ) );	// Diffuse LTC is already multiplied by 1/PI
	_specular = Li * magnitude_specular * PolygonIrradiance( mul( tsLightCorners, LTC_specular ) );
}

PS_OUT	PS( VS_IN _In ) {
	float3	csView = GenerateCameraRay( _In.__Position.xy );
	float3	wsView = mul( float4( csView, 0 ), _camera2World ).xyz;
	float3	wsCamPos = _camera2World[3].xyz;

	float	t = -wsCamPos.y / wsView.y;
	clip( t );

	float3	wsPos = wsCamPos + t * wsView;

	float3	wsNormal = float3( 0, 1, 0 );	// Simple plane
	float3	diffuseAlbedo = 0.5;			// Assume a regular 50% diffuse reflectance
	float3	specularF0 = 0.04;				// 1.5 IOR
	float	roughnessDiffuse = 0.0;			// Lambert
	float	roughnessSpecular = 0.2;

	// Compute reference diffuse lighting
	float3	diffuse, specular;
	ComputeLTCAreaLightLuminance( wsPos, wsNormal, -wsView, roughnessDiffuse, roughnessSpecular, diffuse, specular );
	diffuse *= diffuseAlbedo;

	float4	ndcPos = mul( float4( wsPos, 1 ), _world2Proj );

	PS_OUT	result;
	result.color = diffuse;
	result.depth = ndcPos.z / ndcPos.w;

	return result;
}
