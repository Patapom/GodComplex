////////////////////////////////////////////////////////////////////////////////
// Lighting Management
////////////////////////////////////////////////////////////////////////////////
//
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

void	ComputeGenericData( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax, float3 _wsLightPosition, float2 _distanceAttenuation, out float _NdotL, out float _attenuation ) {
	float3	wsPosition2Light = _wsLightPosition - _wsPosition;
	float	sqDistance2Light = dot( wsPosition2Light, wsPosition2Light );
	float	distance2Light = sqrt( sqDistance2Light );
			wsPosition2Light /= distance2Light;

	_NdotL = saturate( dot( wsPosition2Light, _wsNormal ) );

	_attenuation = 1.0 / sqDistance2Light;	// Physical 1/r²
	_attenuation *= smoothstep( _distanceAttenuation.y, _distanceAttenuation.x, distance2Light );	// Now with forced smooth cutoff (so we keep the physically correct 1/r² but can nonetheless artificially attenuate early)
	_attenuation *= smoothstep( _cosConeAnglesMinMax.y, _cosConeAnglesMinMax.x, _NdotL );			// Check if the light is standing inside the visibility cone of the surface
}

// This must be called by the scene for each of its point lights
void	ComputeLightPoint( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax, LightInfoPoint _light, inout LightingResult _result ) {
	float	NdotL, attenuation;
	ComputeGenericData( _wsPosition, _wsNormal, _cosConeAnglesMinMax, _light.wsPosition, _light.distanceAttenuation, NdotL, attenuation );

	float3	lightIrradiance = (_light.flux / (4.0 * PI)) * attenuation;	// Assume point source
	_result.diffuse += lightIrradiance * NdotL;
	_result.specular += 0.0;	// #TODO
}

// This must be called by the scene for each of its spot lights
void	ComputeLightSpot( float3 _wsPosition, float3 _wsNormal, float2 _cosConeAnglesMinMax, LightInfoSpot _light, inout LightingResult _result ) {
	float	NdotL, attenuation;
	ComputeGenericData( _wsPosition, _wsNormal, _cosConeAnglesMinMax, _light.wsPosition, _light.distanceAttenuation, NdotL, attenuation );

	attenuation *= smoothstep( _light.angularAttenuation.y, _light.angularAttenuation.x, NdotL );

	float3	lightIrradiance = (_light.flux / (4.0 * PI)) * attenuation;	// Assume point source
	_result.diffuse += lightIrradiance * NdotL;
	_result.specular += 0.0;	// #TODO
}
