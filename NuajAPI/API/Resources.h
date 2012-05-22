#pragma once
#include "Types.h"

//////////////////////////////////////////////////////////////////////////
// Shader Resources
//
// Low altitude cloud layer
static const NjResourceID	ResourceID_Shader_RenderDeepShadowMap				= 1;
static const NjResourceID	ResourceID_Shader_FilterDeepShadowMap				= 2;
static const NjResourceID	ResourceID_Shader_RenderSky							= 3;
static const NjResourceID	ResourceID_Shader_CombineSky						= 4;

// High altitude cloud layer
static const NjResourceID	ResourceID_Shader_RenderDeepShadowMapHemispherical  = 5;
static const NjResourceID	ResourceID_Shader_RenderCloudsHemispherical			= 6;
static const NjResourceID	ResourceID_Shader_RenderSkyDome						= 7;

// Ambient sky probes
static const NjResourceID	ResourceID_Shader_ProbeRenderDeepShadowMap			= 8;
static const NjResourceID	ResourceID_Shader_ProbeRender						= 9;
static const NjResourceID	ResourceID_Shader_ProbeConvolve						= 10;

// Environment map
static const NjResourceID	ResourceID_Shader_RenderEnvironmentMap				= 11;
