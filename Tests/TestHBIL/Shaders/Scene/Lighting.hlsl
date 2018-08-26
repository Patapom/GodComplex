////////////////////////////////////////////////////////////////////////////////
// Lighting Management
////////////////////////////////////////////////////////////////////////////////
//
#include "Specular.hlsl"

static const uint	SHADOW_MAP_SIZE_DIRECTIONAL = 1024;	// Caution! Must match value declared in form!
static const uint	SHADOW_MAP_SIZE_POINT = 512;		// Caution! Must match value declared in form!

cbuffer CB_Shadow : register(b4) {
	// Point light
	float3		_wsPointLightPosition;		// Point light world-space position
	float		_pointLightZFar;			// Far clip distance for point light

	// Directional light
	float4x4	_directionalShadowMap2World;	// Row 0 => XYZ=Right, W=Width
												// Row 1 => XYZ=Up, W=Height
												// Row 2 => XYZ=At, W=Z far
												// Row 3 => XYZ=Position, W=1

	uint		_faceIndex;					// Point face index or cascade slice index (render-time only)
};

Texture2DArray<float>	_tex_ShadowMap : register( t6 );
Texture2DArray<float>	_tex_ShadowMapDirectional : register(t7);

// Generated from a blue-noise texture
static const uint		SHADOW_SAMPLES_COUNT = 64;
static const float2		SHADOW_SAMPLES[64] = {
float2( -0.75, 0.6875 ), float2( 0.1875, -0.6875 ), float2( -0.125, 0.5625 ), float2( -0.8125, -0.78125 ), float2( -0.21875, -0.21875 ), float2( 0.375, 0.1875 ), float2( 0.71875, -0.375 ), float2( 0.40625, 0.71875 ), float2( -0.3125, -0.9375 ), float2( 0.84375, 0.375 ), float2( 0.6875, -0.875 ), float2( 0.28125, -0.25 ), float2( -0.65625, -0.375 ), float2( -0.34375, 0.1875 ), float2( 0.0625, 0.9375 ), float2( 0.875, 0.78125 ), float2( 0.875, -0.03125 ), float2( 0.03125, 0.0625 ), float2( -0.1875, -0.59375 ), float2( -0.46875, 0.5 ), float2( 0.1875, 0.46875 ), float2( -0.96875, -0.46875 ), float2( -0.5, -0.65625 ), float2( -0.8125, 0.375 ), float2( 0.5625, -0.09375 ), float2( 0.375, -0.96875 ), float2( 0.5, -0.625 ), float2( -0.625, 0.96875 ), float2( 0.53125, 0.4375 ), float2( -0.5, -0.09375 ), float2( -0.375, 0.78125 ), float2( 0.03125, -0.40625 ), float2( -0.875, -0.1875 ), float2( 0.96875, -0.9375 ), float2( 0.65625, 0.15625 ), float2( 0.8125, -0.625 ), float2( -0.0625, 0.3125 ), float2( 0.625, 0.875 ), float2( -0.0625, -0.8125 ), float2( -0.40625, -0.40625 ), float2( -1, 0.5625 ), float2( -1, 0.1875 ), float2( -0.59375, 0.25 ), float2( 0.6875, 0.625 ), float2( 0.09375, 0.6875 ), float2( -0.15625, 0.84375 ), float2( 0.46875, -0.375 ), float2( -0.875, 0.875 ), float2( 0.25, 0 ), float2( 0.0625, -0.15625 ), float2( -0.1875, 0 ), float2( 0.90625, -0.25 ), float2( 0.25, -0.46875 ), float2( 0.15625, 0.25 ), float2( -0.75, -0.5625 ), float2( -0.28125, 0.40625 ), float2( 0.25, 0.84375 ), float2( -1, -0.71875 ), float2( -0.5, -0.875 ), float2( 0.1875, -0.90625 ), float2( -0.5625, 0.71875 ), float2( -0.6875, -0.15625 ), float2( -0.65625, 0.5 ), float2( 0.375, -0.78125 ), };

////////////////////////////////////////////////////////////////////////////////
// Lighting Helpers
////////////////////////////////////////////////////////////////////////////////
// 
struct LightingResult {
	float3	diffuse;
	float3	specular;
};
struct LightInfoPoint {
	float3	flux;					// In lumens
	float3	wsPosition;
	float2	distanceAttenuation;	// X=Fall off distance, Y=Cut off distance
};
struct LightInfoSpot {
	float3	flux;					// In lumens
	float3	wsPosition;
	float3	wsDirection;
	float2	distanceAttenuation;	// X=Fall off distance, Y=Cut off distance
	float2	angularAttenuation;		// X=Fall off cos(alpha), Y=Cut off cos(alpha)
};
struct LightInfoDirectional {
	float3	illuminance;			// In lumens/m²
	float3	wsDirection;			// Toward the light
};

// Computes generic angular and distance attenuation values
//	_cosConeAnglesHotSpotFallOff, X=cos(HotSpot Angle) Y=cos(FallOff Angle).
//	_distanceAttenuation, X=HotSpot distance, Y=FallOff distance
void	ComputeGenericData( float3 _wsPosition, float3 _wsNormal, float3 _wsBentNormal, float2 _cosConeAnglesHotSpotFallOff, float3 _wsLightPosition, float2 _distanceAttenuation, out float3 _wsLight, out float _NdotL, out float _attenuation ) {
	_wsLight = _wsLightPosition - _wsPosition;
	float	sqDistance2Light = dot( _wsLight, _wsLight );
	float	distance2Light = sqrt( sqDistance2Light );
			_wsLight /= distance2Light;

	_NdotL = saturate( dot( _wsLight, _wsNormal ) );

	_attenuation = 1.0 / sqDistance2Light;	// Physical 1/r²
	_attenuation *= smoothstep( _distanceAttenuation.y, _distanceAttenuation.x, distance2Light );			// Add forced smooth cutoff (so we keep the physically correct 1/r² but can nonetheless artificially attenuate early)

	// Check if the light is standing inside the visibility cone of the surface
	float	BNdotL = dot( _wsLight, _wsBentNormal );
	_attenuation *= smoothstep( _cosConeAnglesHotSpotFallOff.y, _cosConeAnglesHotSpotFallOff.x, BNdotL );
}

// This must be called by the scene for each of its point lights
void	ComputeLightPoint( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _roughness, float3 _IOR, float3 _wsBentNormal, float2 _cosConeAnglesHotSpotFallOff, LightInfoPoint _light, inout LightingResult _result ) {
	float	NdotL, attenuation;
	float3	wsLight;
	ComputeGenericData( _wsPosition, _wsNormal, _wsBentNormal, _cosConeAnglesHotSpotFallOff, _light.wsPosition, _light.distanceAttenuation, wsLight, NdotL, attenuation );

	float3	lightIrradiance = (_light.flux / (4.0 * PI)) * attenuation;	// Assume point source
	float3	F;
	_result.specular += lightIrradiance * GGX( wsLight, _wsView, _wsNormal, _roughness, _IOR, F );
	_result.diffuse += lightIrradiance * (1.0-F) * NdotL;
}

// This must be called by the scene for each of its spot lights
void	ComputeLightSpot( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _roughness, float3 _IOR, float3 _wsBentNormal, float2 _cosConeAnglesHotSpotFallOff, LightInfoSpot _light, inout LightingResult _result ) {
	float	NdotL, attenuation;
	float3	wsLight;
	ComputeGenericData( _wsPosition, _wsNormal, _wsBentNormal, _cosConeAnglesHotSpotFallOff, _light.wsPosition, _light.distanceAttenuation, wsLight, NdotL, attenuation );

	attenuation *= smoothstep( _light.angularAttenuation.y, _light.angularAttenuation.x, NdotL );

	float3	lightIrradiance = (_light.flux / (4.0 * PI)) * attenuation;	// Assume point source
	float3	F;
	_result.specular += lightIrradiance * GGX( wsLight, _wsView, _wsNormal, _roughness, _IOR, F );
	_result.diffuse += lightIrradiance * (1.0-F) * NdotL;
}

// This must be called by the scene for each of its direction lights
void	ComputeLightDirectional( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _roughness, float3 _IOR, float3 _wsBentNormal, float2 _cosConeAnglesHotSpotFallOff, LightInfoDirectional _light, inout LightingResult _result ) {
	float	NdotL = saturate( dot( _wsNormal, _light.wsDirection ) );
	float3	F;
	_result.specular += _light.illuminance * GGX( _light.wsDirection, _wsView, _wsNormal, _roughness, _IOR, F );
	_result.diffuse += _light.illuminance * (1.0-F) * NdotL;
}


////////////////////////////////////////////////////////////////////////////////
// Point Shadow map helpers
////////////////////////////////////////////////////////////////////////////////
//
void	GetShadowMapTransform( uint _faceIndex, out float3x3 _shadowMap2World ) {
	float3	axisX = 0, axisZ = 0;
	switch ( _faceIndex ) {
		case 0:	axisZ = float3( -1, 0, 0 ); axisX = float3( 0, 0, -1 ); break;	// -X
		case 1:	axisZ = float3( +1, 0, 0 ); axisX = float3( 0, 0, +1 ); break;	// +X
		case 2:	axisZ = float3( 0, -1, 0 ); axisX = float3( +1, 0, 0 ); break;	// -Y
		case 3:	axisZ = float3( 0, +1, 0 ); axisX = float3( +1, 0, 0 ); break;	// +Y
		case 4:	axisZ = float3( 0, 0, -1 ); axisX = float3( -1, 0, 0 ); break;	// -Z
		case 5:	axisZ = float3( 0, 0, +1 ); axisX = float3( +1, 0, 0 ); break;	// +Z
	}
	_shadowMap2World[0] = axisX;
	_shadowMap2World[1] = cross( axisX, axisZ );
	_shadowMap2World[2] = axisZ;
}
float	SampleShadowMapPoint( float3 _UV, Texture2DArray<float> _texShadow, float _shadowBias ) {
	float	shadowZ = _texShadow.SampleLevel( LinearClamp, _UV, 0.0 );
//			shadowZ += 1e-3;	// Bias
			shadowZ += _shadowBias;
	return shadowZ;
}
float	GetShadowPoint( float3 _wsPosition, float3 _wsNormal, float _pixelSize_m, float3 _wsLightPosition, float _lightFar, Texture2DArray<float> _texShadow, float _noise, float _shadowBias ) {
	float3	wsDelta = _wsPosition - _wsLightPosition;
	float3	dir = wsDelta;
	float	L = length( dir );
			dir /= L;

	// Find principal axis
	float3	absDir = abs(dir);
	float2	axisIndex = absDir.x > absDir.y ? float2( 0, dir.x ) : float2( 2, dir.y );
	axisIndex = absDir.z > abs(axisIndex.y) ? float2( 4, dir.z ) : axisIndex;
	axisIndex += axisIndex.y >= 0.0 ? 1 : 0;

	// Retrieve transform
	float3x3	shadowMap2World;
	GetShadowMapTransform( uint(axisIndex.x), shadowMap2World );

	// Transform into shadow-map space
	float3	ssPosition = mul( shadowMap2World, wsDelta );
			ssPosition.xy /= ssPosition.z;

	float	shadowZ = ssPosition.z / _lightFar;

	// Sample shadow map
	float2	UV = float2( 0.5 * (1.0 + ssPosition.x), 0.5 * (1.0 - ssPosition.y) );
#if 1
	// Compute shadow mip size
	const float	TAN_HALF_FOV_SHADOW = sqrt(2.0)/2;	// tan(45°)
	float	shadowTexelSize_m = ssPosition.z * TAN_HALF_FOV_SHADOW / SHADOW_MAP_SIZE_POINT;
	float	coveredTexels = shadowTexelSize_m / _pixelSize_m;
	float2	dUV = 1.0 * coveredTexels / SHADOW_MAP_SIZE_POINT;
	float2x2	rot;
	sincos( 0.5 * PI * _noise, rot[0][0], rot[1][0] );
	rot[0][1] = -rot[1][0];
	rot[1][1] =  rot[0][0];

	float	shadow = 0.0;
	for ( uint sampleIndex=0; sampleIndex < SHADOW_SAMPLES_COUNT; sampleIndex++ )
		shadow += step( shadowZ, SampleShadowMapPoint( float3( UV + dUV * SHADOW_SAMPLES[sampleIndex], axisIndex.x ), _texShadow, _shadowBias ) );

	return shadow / SHADOW_SAMPLES_COUNT;
#else
//	float2	dUV = 1.0 * Z / (SHADOW_MAP_SIZE_POINT * _lightFar);
	float2	dUV = 0.5 / SHADOW_MAP_SIZE_POINT;

	float	shadow = 0.0;
	for ( int Y=-2; Y <= 2; Y++ ) {
		for ( int X=-2; X <= 2; X++ ) {
			shadow += step( shadowZ, SampleShadowMapPoint( float3( UV + dUV * float2( _noise + X, Y ), axisIndex.x ), _texShadow ) );
		}
	}

	return shadow / 25.0;
#endif
}

////////////////////////////////////////////////////////////////////////////////
// Directional Shadow map helpers
////////////////////////////////////////////////////////////////////////////////
//
float	SampleShadowMapDirectional( float3 _UV, Texture2DArray<float> _texShadow, float _shadowBias ) {
	float	shadowZ = _texShadow.SampleLevel( LinearClamp, _UV, 0.0 );
//			shadowZ += 1e-3;	// Bias
			shadowZ += _shadowBias;
	return shadowZ;
}
float	GetShadowDirectional( float3 _wsPosition, float3 _wsNormal, float _pixelSize_m, Texture2DArray<float> _texShadow, float _noise, float _shadowBias ) {

//float	NdotL = max( 1e-3, dot( _wsNormal, _directionalShadowMap2World[2].xyz ) );
//_wsPosition += _shadowBias * _wsNormal / NdotL;
//_shadowBias = 0.0;

	float3	wsDelta = _wsPosition - _directionalShadowMap2World[3].xyz;
	float3	lsDelta = float3( dot( wsDelta, _directionalShadowMap2World[0].xyz ), dot( wsDelta, _directionalShadowMap2World[1].xyz ), dot( wsDelta, _directionalShadowMap2World[2].xyz ) );
			lsDelta.x /= _directionalShadowMap2World[0].w;
			lsDelta.y /= -_directionalShadowMap2World[1].w;
			lsDelta.z /= _directionalShadowMap2World[2].w;

	float	shadowZ = lsDelta.z;

	// Sample shadow map
	float2	UV = 0.5 + lsDelta.xy;
#if 1
	// Compute shadow mip size
	float2	shadowTexelSize_m = float2( _directionalShadowMap2World[0].w, _directionalShadowMap2World[1].w ) / SHADOW_MAP_SIZE_DIRECTIONAL;
	float2	coveredTexels = _pixelSize_m / shadowTexelSize_m;
	float2	dUV = 32.0 * coveredTexels / SHADOW_MAP_SIZE_DIRECTIONAL;
	float2x2	rot;
	sincos( 0.5 * PI * _noise, rot[0][0], rot[1][0] );
	rot[0][1] = -rot[1][0];
	rot[1][1] =  rot[0][0];

	float	shadow = 0.0;
	for ( uint sampleIndex=0; sampleIndex < SHADOW_SAMPLES_COUNT; sampleIndex++ ) {
		shadow += step( shadowZ, SampleShadowMapDirectional( float3( UV + dUV * mul( rot, SHADOW_SAMPLES[sampleIndex] ), 0 ), _texShadow, _shadowBias ) );
	}

	return shadow / SHADOW_SAMPLES_COUNT;
#else
//	float2	dUV = 1.0 * Z / (SHADOW_MAP_SIZE_DIRECTIONAL * _lightFar);
	#if SCENE_TYPE == 1
		float2	dUV = 0.5 / SHADOW_MAP_SIZE_DIRECTIONAL;
	#else
		float2	dUV = 0.75 / SHADOW_MAP_SIZE_DIRECTIONAL;
	#endif

	float	shadow = 0.0;
	for ( int Y=-4; Y <= 4; Y++ ) {
		for ( int X=-4; X <= 4; X++ ) {
			shadow += step( shadowZ, SampleShadowMapDirectional( float3( UV + dUV * float2( _noise+X, Y ), 0 ), _texShadow, _shadowBias ) );
		}
	}

	return shadow / 81.0;
#endif
}