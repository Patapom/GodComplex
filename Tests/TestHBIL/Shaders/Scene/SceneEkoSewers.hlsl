///////////////////////////////////////////////////////////////////////////////////
// EKO Sewers Scene
///////////////////////////////////////////////////////////////////////////////////
//
static const float	SHADOW_BIAS = 1e-3;

LightingResult	LightScene( float3 _wsPosition, float3 _wsNormal, float3 _wsView, float _roughness, float3 _IOR, float _pixelSize_m, float3 _wsBentNormal, float2 _cosConeAnglesMinMax, float _noise ) {
	LightingResult	result = (LightingResult) 0;

	// Compute directional light
	LightInfoDirectional	lightInfo;
							lightInfo.illuminance = _sunIntensity * GetShadowDirectional( _wsPosition, _wsNormal, _pixelSize_m, _tex_ShadowMapDirectional, _noise, SHADOW_BIAS );
							lightInfo.wsDirection = -_directionalShadowMap2World[2].xyz;
	ComputeLightDirectional( _wsPosition, _wsNormal, _wsView, _roughness, _IOR, _wsBentNormal, _cosConeAnglesMinMax, lightInfo, result );

	return result;
}
