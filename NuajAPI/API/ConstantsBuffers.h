#pragma once

#include "Types.h"

//////////////////////////////////////////////////////////////////////////
// Constant Buffers used throughout Nuaj'
// All fields are padded to the nearest float4


// This will provide the general atmosphere parameters
// Buffer Name: "NuajCB_GeneralParams"
// Slot:		0
struct	NjConstantBuffer_General
{
	float		WorldUnit2Kilometer;
	NjFloat3	SunDirection;

	float		SunIntensity;
	float		Kilometer2WorldUnit;
	float		GroundAltitudeKm;
	float		__PAD0;

	NjFloat4	LightningPositionIntensity0;
	NjFloat4	LightningPositionIntensity1;

	NjFloat4x4	Camera2World;
	NjFloat4x4	Camera2Proj;
	NjFloat4	CameraData;

	NjFloat3	AmbientNightSky;
	float		__PAD1;
	NjFloat3	SkyBackgroundColor;
	float		__PAD2;
	NjFloat3	TerrainAlbedo;
	float		__PAD3;
	NjFloat3	SunColorFromGround;
	float		__PAD4;

//	float		pseudoRandomDistanceFactor;	// UNUSED AT THE TIME
};


// This will provide the general atmosphere parameters
// Buffer Name: "NuajCB_SkyParams"
// Slot:		1
struct	NJConstantBuffer_SkyScattering
{
	NjFloat3	Sigma_Rayleigh;
	float		Sigma_Mie;

	NjFloat3	DensitySeaLevel_Rayleigh_InvLambda4;
	float		DensitySeaLevel_Mie;

	float		MiePhaseAnisotropy;
	float		FarClipAirKm;
};


// This provides the parameters to render a specific cloud layer
// Buffer Name: "NuajCB_CloudParams"
// Slot:		2
struct	NjConstantBuffer_CloudLayer 
{
	NjFloat4	CloudAltitudeKm;

	float		ShadowOpacity;
	float		ShadowMaxTraceDistanceKm;
	float		FarClipCloudKm;
	float		InvVoxelSizeKm;

	// Cloud physical parameters
	float		CloudSigma_t;
	float		CloudSigma_s;
	float		CloudSigma_s_Isotropy;
	float		DirectionalFactor;

	NjFloat3	IsotropicScatteringFactors;
	float		IsotropicFactor;

	// Noise data
	NjFloat3	NoiseSize;
	float		__PAD1;
	NjFloat4	NoiseAmplitudeFrequencyFactors;
	NjFloat4	CloudPosition;
	NjFloat3	NoiseCoverage;
	float		__PAD2;

	// Local coverage data
	NjFloat4	CloudLocalCoveragePosition;

	float		CloudLocalCoverageEnabled;
	float		ScatteringAnisotropyStrongForward;
	float		PhaseWeightStrongForward;
	float		ScatteringAnisotropyForward;

	float		PhaseWeightForward;
	float		ScatteringAnisotropyBackward;
	float		PhaseWeightBackward;
	float		ScatteringAnisotropySide;

	float		PhaseWeightSide;
	float		PhaseWeightIsotropic;
	NjFloat2	__PAD0;

	NjFloat4	AmbientSkyColorBlend;
};


// This provides parameters to correctly setup the planar deep shadow map used by the low-altitude clouds
// Buffer Name: "NuajCB_ShadowMapPlanar"
// Slot:		3
struct NjConstantBuffer_DeepShadowMapPlanar 
{
	NjFloat3	ShadowPlaneNormal;
	float		__PAD0;
	NjFloat3	ShadowPlaneCenterKm;
	float		__PAD1;
	NjFloat3	ShadowPlaneX;
	float		__PAD2;
	NjFloat3	ShadowPlaneY;
	float		__PAD3;
	NjFloat2	ShadowPlaneOffsetKm;
	NjFloat2	__PAD4;

	NjFloat4	ShadowQuadVertices;
	NjFloat4	ShadowNormalsU;
	NjFloat4	ShadowNormalsV;

	NjFloat3	ShadowABC;
	float		__PAD5;
	NjFloat3	ShadowDEF;
	float		__PAD6;
	NjFloat3	ShadowGHI;
	float		__PAD7;
	NjFloat3	ShadowJKL;
	float		__PAD8;
};


// This provides parameters to correctly setup the hemispherical deep shadow map used by the high-altitude clouds
// Buffer Name: "NuajCB_ShadowMapHemispherical"
// Slot:		4
struct NjConstantBuffer_DeepShadowMapHemispherical
{
	NjFloat2	CloudRadiusMax;
};


// Buffer Name: "NuajCB_AmbientSkyProbe"
// Slot:		5
struct NjConstantBuffer_ProbeUse
{
	NjFloat3	SkyProbePositionKm;	// Position of the sky probe in kilometers
};

// Buffer Name: "NuajCB_RenderParams"
// Slot:		10
struct NjConstantBuffer_RenderParams
{
	NjFloat4	QuadParams;
	NjFloat3	dUV;
	float		__PAD0;
};
