////////////////////////////////////////////////////////////////////////////////
// Lighting Management
////////////////////////////////////////////////////////////////////////////////
//
static const uint	SHADOW_MAP_SIZE = 512;	// Caution! Must match value declared in form!

struct LightingResult {
	float3	diffuse;
	float3	specular;
};
struct LightInfoPoint {
	float3	flux;
	float3	wsPosition;
	float2	distanceAttenuation;	// X=Fall off distance, Y=Cut off distance
};
struct LightInfoSpot {
	float3	flux;
	float3	wsPosition;
	float3	wsDirection;
	float2	distanceAttenuation;	// X=Fall off distance, Y=Cut off distance
	float2	angularAttenuation;		// X=Fall off cos(alpha), Y=Cut off cos(alpha)
};

// Computes generic angular and distance attenuation values
//	_cosConeAnglesHotSpotFallOff, X=cos(HotSpot Angle) Y=cos(FallOff Angle).
//	_distanceAttenuation, X=HotSpot distance, Y=FallOff distance
void	ComputeGenericData( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesHotSpotFallOff, float3 _wsLightPosition, float2 _distanceAttenuation, out float _NdotL, out float _attenuation ) {
	float3	wsPosition2Light = _wsLightPosition - _wsPosition;
	float	sqDistance2Light = dot( wsPosition2Light, wsPosition2Light );
	float	distance2Light = sqrt( sqDistance2Light );
			wsPosition2Light /= distance2Light;

	_NdotL = saturate( dot( wsPosition2Light, _wsNormal ) );

	_attenuation = 1.0 / sqDistance2Light;	// Physical 1/r²
	_attenuation *= smoothstep( _distanceAttenuation.y, _distanceAttenuation.x, distance2Light );			// Add forced smooth cutoff (so we keep the physically correct 1/r² but can nonetheless artificially attenuate early)
	_attenuation *= smoothstep( _cosConeAnglesHotSpotFallOff.y, _cosConeAnglesHotSpotFallOff.x, _NdotL );	// Check if the light is standing inside the visibility cone of the surface
}

// This must be called by the scene for each of its point lights
void	ComputeLightPoint( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesHotSpotFallOff, LightInfoPoint _light, inout LightingResult _result ) {
	float	NdotL, attenuation;
	ComputeGenericData( _wsPosition, _wsNormal, _cosConeAnglesHotSpotFallOff, _light.wsPosition, _light.distanceAttenuation, NdotL, attenuation );

	float3	lightIrradiance = (_light.flux / (4.0 * PI)) * attenuation;	// Assume point source
	_result.diffuse += lightIrradiance * NdotL;
	_result.specular += 0.0;	// #TODO
}

// This must be called by the scene for each of its spot lights
void	ComputeLightSpot( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesHotSpotFallOff, LightInfoSpot _light, inout LightingResult _result ) {
	float	NdotL, attenuation;
	ComputeGenericData( _wsPosition, _wsNormal, _cosConeAnglesHotSpotFallOff, _light.wsPosition, _light.distanceAttenuation, NdotL, attenuation );

	attenuation *= smoothstep( _light.angularAttenuation.y, _light.angularAttenuation.x, NdotL );

	float3	lightIrradiance = (_light.flux / (4.0 * PI)) * attenuation;	// Assume point source
	_result.diffuse += lightIrradiance * NdotL;
	_result.specular += 0.0;	// #TODO
}


////////////////////////////////////////////////////////////////////////////////
// Shadow map helpers
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
float	SampleShadowMap( float3 _UV, Texture2DArray<float> _texShadow ) {
	float	shadowZ = _texShadow.SampleLevel( LinearClamp, _UV, 0.0 );
			shadowZ += 1e-3;	// Bias
	return shadowZ;
}
float	GetShadow( float3 _wsPosition, float3 _wsLightPosition, float _lightFar, Texture2DArray<float> _texShadow ) {
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
	float	Z = ssPosition.z;
			ssPosition.xy /= Z;

	Z /= _lightFar;

	// Sample shadow map
	float2	UV = float2( 0.5 * (1.0 + ssPosition.x), 0.5 * (1.0 - ssPosition.y) );
//	float2	dUV = 1.0 * Z / (SHADOW_MAP_SIZE * _lightFar);
	float2	dUV = 0.5 / SHADOW_MAP_SIZE;

	float	shadow = 0.0;
	for ( int Y=-2; Y <= 2; Y++ ) {
		for ( int X=-2; X <= 2; X++ ) {
			shadow += step( Z, SampleShadowMap( float3( UV + dUV * float2( X, Y ), axisIndex.x ), _texShadow ) );
		}
	}

	return shadow / 25.0;
}