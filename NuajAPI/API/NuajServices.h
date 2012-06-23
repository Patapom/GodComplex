#pragma once

#include "Types.h"
#include "IEngine.h"

//////////////////////////////////////////////////////////////////////////
// Ambient Sky Probe data to feed your shader if you decide to include the  "AtmosphereAmbientProbeSupport.inc.fx" file
struct  NuajAmbientSkyProbeSupport
{
	NjFloat3		_ProbePositionKm;
	NjITextureView*	_TexAmbientSkySH0;
	NjITextureView* _TexAmbientSkySH1;
	NjITextureView* _TexAmbientSkySH2;
};

//////////////////////////////////////////////////////////////////////////
// Atmosphere data to feed your shader if you decide to include the "AtmosphereSupport.inc.fx" file
struct  NuajAtmosphereSupport 
{
	float		_WorldUnit2Kilometer;					// 1 World Unit equals XXX kilometers

	float		_SunIntensity;							// Intensity of the Sun in outer space
	NjFloat3	_SunDirection;							// Direction towards the Sun

	// Lightning
	NjFloat4	_LightningPositionIntensity0;			// XYZ=Position W=Intensity
	NjFloat4	_LightningPositionIntensity1;			// XYZ=Position W=Intensity


	// ====== SKY VARIABLES ======
	NjFloat3	_Sigma_Rayleigh;						// 4.0 * PI * _DensitySeaLevel_Rayleigh / WAVELENGTHS_POW4
	float		_Sigma_Mie;								// 4.0 * PI * _DensitySeaLevel_Mie;
	NjITextureView* _TexSkyDensity;

	// ====== CLOUD VARIABLES ======
	NjFloat4	_CloudAltitudeKm;						// X=Bottom Altitude Y=Top Altitude Z=Thickness W=1/Thickness (all in kilometers)
	float		_ShadowOpacity;
	float		_ShadowMaxTraceDistanceKm;				// The maximum distance we can trace within the cloud before extrapolating optical depth


	// ====== SHADOW VARIABLES ======
	NjFloat3	_ShadowPlaneNormal;						// The shadow map's plane normal (usually equal to the direction of the Sun)
	NjFloat3	_ShadowPlaneCenterKm;					// The center of the shadow plane where UVs are (0,0)
	NjFloat3	_ShadowPlaneX;							// First shadow plane tangent
	NjFloat3	_ShadowPlaneY;							// Second shadow plane tangent
	NjFloat2	_ShadowPlaneOffsetKm;					// An offset to bring back UVs to a reasonable range

		// World => UV
	NjFloat4	_ShadowQuadVertices;					// The 2 opposite vertices at uv=(0,0) and uv=(1,1)
	NjFloat4	_ShadowNormalsU;						// The 2 edge normals used for U computation
	NjFloat4	_ShadowNormalsV;						// The 2 edge normals used for V computation

		// UV => World
	NjFloat3	_ShadowABC;
	NjFloat3	_ShadowDEF;
	NjFloat3	_ShadowGHI;
	NjFloat3	_ShadowJKL;

	NjITextureView* _TexDeepShadowMapLayer00;				// 2 layers of deep shadow map (up to 8 opacity values)
	NjITextureView* _TexDeepShadowMapLayer01;
};