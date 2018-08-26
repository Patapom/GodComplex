///////////////////////////////////////////////////////////////////////////////////
// Sponza Atrium Scene
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

//	float3	wsLightDirection = -_directionalShadowMap2World[2].xyz;
//	float3	irradianceDirect = _sunIntensity * saturate( dot( wsNormalDirect, wsLightDirection ) );
//			irradianceDirect *= GetShadowDirectional( _wsPosition, _wsNormal, pixelSize_m, _tex_ShadowMapDirectional, noise, SHADOW_BIAS );
////	if ( _flags & 0x8 ) {
////		float	bentConeAttenuation = smoothstep( cosConeAnglesMinMax.y, cosConeAnglesMinMax.x, dot( wsBentNormal, wsLightDirection ) );
////		irradianceDirect *= bentConeAttenuation;
//////wsNormalDirect = lerp( wsBentNormal, wsNormalDirect, 1-cosConeAngle );	// Experiment blending normals depending on AO => a fully open cone gives the bent normal
////	}
//	result.diffuse += irradianceDirect;


	#if SPONZA_POINT_LIGHT
		// Compute point light
		LightInfoPoint	lightInfo2;
						lightInfo2.wsPosition = _wsPointLightPosition;
						lightInfo2.distanceAttenuation = float2( 10, _pointLightZFar );
						lightInfo2.flux = 250.0 * float3( 1, 0.95, 0.8 ) * GetShadowPoint( _wsPosition, _wsNormal, _pixelSize_m, lightInfo2.wsPosition, lightInfo2.distanceAttenuation.y, _tex_ShadowMap, _noise, SHADOW_BIAS );
//						lightInfo2.flux = 5.0 * float3( 1, 0.95, 0.8 ) * GetShadowPoint( _wsPosition, _wsNormal, _pixelSize_m, lightInfo2.wsPosition, lightInfo2.distanceAttenuation.y, _tex_ShadowMap, _noise, SHADOW_BIAS );
		ComputeLightPoint( _wsPosition, _wsNormal, _wsView, _roughness, _IOR, _wsBentNormal, _cosConeAnglesMinMax, lightInfo2, result );
	#endif

	return result;
}
