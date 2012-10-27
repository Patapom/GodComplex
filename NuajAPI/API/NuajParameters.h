#pragma once

#include "Types.h"
#include "IEngine.h"

//////////////////////////////////////////////////////////////////////////
// Configuration parameters (change once per level/engine instance)
struct  NuajConfigurationCloud
{
// Temporarily moved to realtime parameters
//	float		FarClipKm;
//	float		ShadowMaxTraceDistanceKm;
// 	float		NoiseMipBias;
// 	float		NoiseMipBiasOctave;

	NuajConfigurationCloud()
//		: FarClipKm					( 10.0f )
//		, ShadowMaxTraceDistanceKm	( 20.0f )
// 		, NoiseMipBias				( 2.38f )
// 		, NoiseMipBiasOctave		( 0.5f )
	{}
};

struct  NuajConfigurationCloudLow : public NuajConfigurationCloud
{
// 	int			MinStepsCount;
// 	int			MaxStepsCount;
//	float		MaxStepsForThicknessKm;
	int			ShadowMinStepsCount;
	int			ShadowMaxStepsCount;
	float		ShadowMaxStepsForThicknessKm;
// 	float		ShadowMapFilterSize;
// 	float		ShadowFilterMinWeight;
//	NjFloat2	ShadowMapInitialAttenuation;  // We attenuate the initial shadowing by the high-altitude clouds depending on the dot(View,Light) using 2 thresholds given by this variable

	NuajConfigurationCloudLow() : NuajConfigurationCloud()
// 		, MinStepsCount				( 64 )
// 		, MaxStepsCount				( 96 )
//		, MaxStepsForThicknessKm	( 7.5f )
		, ShadowMinStepsCount		( 16 )
		, ShadowMaxStepsCount		( 16 )
		, ShadowMaxStepsForThicknessKm( 50.0f )
// 		, ShadowMapFilterSize		( 3.0f )
// 		, ShadowFilterMinWeight		( 0.05f )
//		, ShadowMapInitialAttenuation( 0.8f, 1.0f )   // We only use shadow map initial values if dot(View,Light) > within this range of values
	{}
};

struct  NuajConfigurationCloudHigh : public NuajConfigurationCloud
{
	int			FullRefreshFramesCount;					// The amount of frames after which the entire cloud texture should be refreshed
														// This is an important parameter as it will dictate the amount of time to allocate
														//  to the task of computing the high-altitude cloud map.
														// . A low value will accelerate the refresh but also take a longer time per frame for
														//  the computation, with a loss of framerate obviously.
														// . A high value will make the cloud refresh slowly but the framerate will be spared.
	float		MaxCameraAltitudeKm;					// The maximum altitude we allow the camera to go
	float		HemisphereMaxRadiusFactor;				// The factor to apply to the computed radius of the hemispherical map

	// Ray-marching details
	int			CloudStepsCount;						// Amount of steps to use for ray-marching

	// Shadow map
	int			ShadowStepsCount;						// Amount of steps to use for shadow ray-marching

	NuajConfigurationCloudHigh() : NuajConfigurationCloud()
		, FullRefreshFramesCount	( 32 )
		, MaxCameraAltitudeKm		( 2.0f )
		, HemisphereMaxRadiusFactor	( 0.5f )
		, CloudStepsCount			( 32 )
		, ShadowStepsCount			( 8 )
	{}
};

struct  NuajConfiguration
{
	// Global
	bool		Enabled;
	float		WorldUnit2Kilometer;
	float		GroundAltitude;

	NjFloat3	MyXInYourWorld;						// How do you write my unit X vector in your world space ?
	NjFloat3	MyYInYourWorld;						// How do you write my unit Y vector in your world space ?
	NjFloat3	MyZInYourWorld;						// How do you write my unit Z vector in your world space ?
//	float		RefinementZThreshold;

	float		SunIntensity;
	NjFloat3	RayleighWavelengths;
	float		MoonAlbedo;

	// Cloud layers
	NuajConfigurationCloudLow   CloudLayerLow;
	NuajConfigurationCloudHigh  CloudLayerHigh;

	NuajConfiguration()
		: Enabled					( true )
		, WorldUnit2Kilometer		( 0.001f )					// Typical: 1 unit = 1 meter
		, GroundAltitude			( 0.0f )
		, MyXInYourWorld			( 1, 0, 0 )
		, MyYInYourWorld			( 0, 1, 0 )
		, MyZInYourWorld			( 0, 0, 1 )
//		, RefinementZThreshold		( 100.0f )

		, SunIntensity				( 100.0f)
		, RayleighWavelengths		( 0.650f, 0.570f, 0.475f )  // Standard RGB wavelengths in µm
		, MoonAlbedo				( 0.012f )
	{}
};

//////////////////////////////////////////////////////////////////////////
// Quality settings
struct	NuajQualityCloudLow
{
	int			DeepShadowMapSize;
//	int			FilteredDeepShadowMapSize;

	NuajQualityCloudLow()
		: DeepShadowMapSize			( 512 )
//		, FilteredDeepShadowMapSize	( 256 )
	{}
};

struct	NuajQualityCloudHigh
{
	int			CloudMapSize;
	int			DeepShadowMapSize;

	NuajQualityCloudHigh()
		: CloudMapSize				( 2048 )
		, DeepShadowMapSize			( 64 )
	{}
};

struct	NuajQuality
{
	// Skydome geometry Resolution
	int			SkyDomeSubdivisionsTheta;
	int			SkyDomeSubdivisionsPhi;

	// Probes & Environment Map Resolution
	int			SkyEnvironmentMapWidth;
	int			SkyEnvironmentMapHeight;
	int			ProbeRenderWidth;
	int			ProbeRenderHeight;

	// Cloud layers
	NuajQualityCloudLow		CloudLayerLow;
	NuajQualityCloudHigh	CloudLayerHigh;

	NuajQuality()
		: SkyDomeSubdivisionsTheta	( 40 )
		, SkyDomeSubdivisionsPhi	( 80 )

		, SkyEnvironmentMapWidth	( 256 )
		, SkyEnvironmentMapHeight	( 128 )
		, ProbeRenderWidth			( 32 )
		, ProbeRenderHeight			( 16 )
	{}
};

//////////////////////////////////////////////////////////////////////////
// Realtime Sky & Clouds parameters
struct  NuajParametersCloud
{
	bool		Enabled;

	// Geometry
	float		AltitudeKm;
	float		ThicknessKm;
	float		FarClipKm;

	// Density and coverage
	float		Density;
	float		CoverageOffset;
	float		CoverageContrast;
	float		CoverageGamma;
	float		CoverageBias;
	float		CoverageBiasReferenceDistanceKm;
	float		CoverageBiasStrength;
	float		BevelSoftness;

	// Local coverage
	NjFloat2	LocalCoverageCenter;
	NjFloat2	LocalCoverageSize;
	bool		LocalCoverageEnabled;
	bool		LocalCoverageScrollWithWind;

	// Noise & octaves 
	float		NoiseSize;
	float		NoiseSizeVerticalFactor;
	float		NoiseAmplitudeFactor;
	float		NoiseFrequencyFactor;

	NjFloat3	NoiseDeltaRotationXYZ;

	// Lighting params
	float		DirectionalFactor;
	float		AlbedoDirectional;
	float		IsotropicFactor;
	float		AlbedoIsotropic;
	float		IsotropicDirectLightFactor;
	float		IsotropicAmbientLightFactor;
	float		IsotropicTerrainReflectedLightFactor;
	NjFloat3	IsotropicAmbientColor;
	float		IsotropicBlendWithSkyColor;
	float		SunLightingFromBelow;					// Factor of Sun light at sunrise/sunset for dramatic effects

	// Shadowing
	float		ShadowOpacity;

	// Animation
	float		WindForce;
	float		WindAngle;
	float		EvolutionSpeed;

// To be moved back into Configuration once it's okay
float		NoiseMipBias;
float		NoiseMipBiasOctave;
float		ShadowMaxTraceDistanceKm;



	NuajParametersCloud()
		: Enabled					( true )
		, AltitudeKm				( 2.0f )
		, ThicknessKm				( 5.0f )
		, Density					( 5.0f )
		, CoverageOffset			( -0.05f )
		, CoverageContrast			( 0.2f )
		, CoverageGamma				( 1.0f )
		, CoverageBias				( 0.0f )
		, CoverageBiasReferenceDistanceKm( 15.0f )
		, CoverageBiasStrength		( 0.1f )
		, BevelSoftness				( 1.0f )
		, FarClipKm					( 20.0f )
		, DirectionalFactor			( 0.22f )
		, AlbedoDirectional			( 0.95f )
		, IsotropicFactor			( 0.6f )
		, AlbedoIsotropic			( 0.5f )//( 0.0075f )
		, IsotropicAmbientColor		( 0.5724f, 0.7068f, 0.9102f )
		, IsotropicBlendWithSkyColor( 0.0f )  // Use full ambient color, no sky color (to be increased toward 1 at sunset to get the nice redish sky colors)
		, IsotropicDirectLightFactor( 50.0f )
		, IsotropicAmbientLightFactor( 100.0f )
		, IsotropicTerrainReflectedLightFactor( 1.0f )
		, SunLightingFromBelow		( 20.0f )
		, NoiseSize					( 0.01f )
		, NoiseSizeVerticalFactor	( 1.0f )
		, NoiseFrequencyFactor		( 5.0f )
		, NoiseAmplitudeFactor		( 0.4f )
		, NoiseDeltaRotationXYZ		( 0, 0, 0 )
		, ShadowOpacity				( 0.3f )
		, WindForce					( 0.2f )
		, WindAngle					( 0.0f )
		, EvolutionSpeed			( 8.0f )
		, LocalCoverageCenter		( 0, 0 )
		, LocalCoverageSize			( 2000, 2000 )
		, LocalCoverageEnabled		( false )
		, LocalCoverageScrollWithWind( true )

, NoiseMipBias				( 2.38f )
, NoiseMipBiasOctave		( 0.5f )
, ShadowMaxTraceDistanceKm	( 20.0f )

	{
	}
};

struct  NuajParametersFog : NuajParametersCloud
{
	NuajParametersFog() : NuajParametersCloud()
	{
		//////////////////////////////////////////////////////////////////////////
		// Override the parameters for low altitude clouds
		AltitudeKm = -0.5f;
		ThicknessKm = 2.0f;
		FarClipKm = 100.0f;

		IsotropicAmbientLightFactor = 20.0f;
		IsotropicDirectLightFactor = 10.0f;
		IsotropicTerrainReflectedLightFactor = 1.0f;

		CoverageOffset = 0.93f;
		CoverageContrast = 1.0f;
		NoiseSize = 0.007f;

		WindForce = 0.05f;
	}
};

struct  NuajParametersCloudLow : NuajParametersCloud
{
	bool		AutoComputeShadowOpacity;
	float		ShadowFarClipKm;

// To be moved into Configuration once it's okay
int			MinStepsCount;
int			MaxStepsCount;
float		MaxStepsForThicknessKm;
// float		ShadowMapFilterSize;
// float		ShadowFilterMinWeight;

	NuajParametersCloudLow() : NuajParametersCloud()
		, AutoComputeShadowOpacity	( true )
		, ShadowFarClipKm			( 40.0f )
, MinStepsCount				( 64 )
, MaxStepsCount				( 96 )
, MaxStepsForThicknessKm		( 7.5f )
// , ShadowMapFilterSize		( 3.0f )
// , ShadowFilterMinWeight		( 0.05f )
	{
		//////////////////////////////////////////////////////////////////////////
		// Override the parameters for low altitude clouds
		AltitudeKm = 3.0f;
		ThicknessKm = 5.0f;

		Density = 2.6f;
		DirectionalFactor = 0.22f;
		AlbedoDirectional = 0.95f;
		IsotropicFactor = 1.0f;
		AlbedoIsotropic = 0.5f;//0.022f;

		CoverageOffset = -0.147f;
		CoverageContrast = 0.588f;
		NoiseSize = 0.0015f;
		NoiseAmplitudeFactor = 0.3f;
		NoiseFrequencyFactor = 5.0f;
	}
};

struct  NuajParametersCloudHigh : NuajParametersCloud
{
	NuajParametersCloudHigh() : NuajParametersCloud()
	{
		//////////////////////////////////////////////////////////////////////////
		// Override the parameters for high altitude clouds
		AltitudeKm = 10.0f;
		ThicknessKm = 0.5f;

		CoverageOffset = -0.147f;
		CoverageContrast = 0.588f;
		NoiseSize = 0.0015f;
		NoiseAmplitudeFactor = 0.4f;
		NoiseFrequencyFactor = 4.0f;

		CoverageBiasReferenceDistanceKm = 80.0f;
		CoverageBiasStrength = 0.015f;

		LocalCoverageSize = NjFloat2( 20000, 20000 );	// Large structures...

		WindForce = 0.001f;	// Veeeeery slow !
	}
};

struct  NuajSkyParameters
{
	bool					Enabled;
	NjFloat3				SunDirection;
	NjFloat3				MoonDirection;
	float					LightSourceBlend;
	float					RayleighDensity;
	float					MieDensity;
	float					ScatteringAnisotropy;
	float					ScatteringBoost;
	float					ExtinctionBoost;
	float					EnvironmentMapFakeCloudDeckAltitudeKm;
	NjFloat3				NightSkyAmbientColor;
	NjFloat3				SkyBackgroundColor;
	NjFloat3				TerrainAlbedo;
	NjITexture*				pTexLocalCoverage;
	NjITexture*				pTexHighAltitudeClouds;

	NjFloat3				LightningPosition0;
	NjFloat3				LightningColor0;
	NjFloat3				LightningPosition1;
	NjFloat3				LightningColor1;

	NuajParametersFog		FogLayer;
	NuajParametersCloudLow	CloudLayerLow;
	NuajParametersCloudHigh	CloudLayerHigh;

// To be moved back into Configuration once it's okay
float		RefinementZThreshold;

	NuajSkyParameters()
		: Enabled				( true )
		, SunDirection			( 0, 1, 1 )
		, LightSourceBlend		( 0 )				// Full sun !
		, RayleighDensity		( 1e-5f * 8.0f )	// Clear sky
		, MieDensity			( 1e-4f * 1.0f )	// Almost no fog
		, ScatteringAnisotropy	( 0.6f )
		, ScatteringBoost		( 5.0f )
		, ExtinctionBoost		( 2.0f )
		, EnvironmentMapFakeCloudDeckAltitudeKm( 2.0f )
		, NightSkyAmbientColor	( 0.01f, 0.01f, 0.01f )
		, SkyBackgroundColor	( 0.0f, 0.0f, 0.0f )								// Clear the sky with a black background so we have the exact sky color
		, TerrainAlbedo			( 37.0f / 255.0f, 24.0f / 255.0f, 16.0f / 255.0f )  // Some red-ish tint for terrain reflection. Could be localized by a texture...
		, pTexLocalCoverage		( 0 )
		, LightningPosition0	( 0.0f, -6400.0f, 0.0 )
		, LightningColor0		( 0.0f, 0.0f, 0.0 )
		, LightningPosition1	( 0.0f, -6400.0f, 0.0 )
		, LightningColor1		( 0.0f, 0.0f, 0.0 )

, RefinementZThreshold		( 100.0f )
	{}
};

//////////////////////////////////////////////////////////////////////////
// Rendering parameters, fed each frame to the renderer
struct  NuajRenderParameters
{
	float				DeltaTime;

	bool				IsPerspectiveProjection;
	float				CameraFOV;						// VERTICAL FOV !
	float				CameraHeight;					// Vertical height if ortho
	float				CameraAspectRatio;				// Width / Height
	float				CameraNearClip;
	float				CameraFarClip;
	bool				bCameraZPointingTowardTarget;	// True if your view matrix's Z (i.e. AT vector) is pointing toward the target (i.e. viewing the scene). If false, your Z is considered to be -AT and will be reversed by Nuaj'
	NjFloat4x4			Camera2World;					// View matrix
	NjFloat4x4			Camera2Proj;					// Projection matrix
	int					ShowDebugInfos;					// Show some debug infos (0 is disabled)

	NjFloat4			DebugParams0;					// Some float parameters for debugging
	NjFloat4			DebugParams1;					// Some float parameters for debugging
	NjFloat4			DebugParams2;					// Some float parameters for debugging
	NjFloat4			DebugParams3;					// Some float parameters for debugging

	NjITextureView&		TexZBuffer;						// The ZBuffer (full resolution)
	NjITextureView&		TexDownsampledZBuffer;			// The ZBuffer, downsampled by 1/4
	NjITexture&			TexHDRTarget;					// The HDR buffer we will render the sky into

	NuajRenderParameters( NjITextureView& _TexZBuffer, NjITextureView& _TexDownsampledZBuffer, NjITexture& _TexHDRTarget )
		: TexZBuffer( _TexZBuffer )
		, TexDownsampledZBuffer( _TexDownsampledZBuffer )
		, TexHDRTarget( _TexHDRTarget )
	{}
};
