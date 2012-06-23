#pragma once

#include "Types.h"
#include "IEngine.h"

//////////////////////////////////////////////////////////////////////////
// Configuration parameters (change once per level/engine instance)
struct  NuajConfigurationCloud
{
	NuajConfigurationCloud()
		: FarClipCloudsKm			( 100.0f )
		, ShadowMaxTraceDistanceKm	( 200.0f )
		, VoxelMipFactor			( 0.003f )
		, VoxelMipFactorShadow		( 0.002f )
	{}

	float		FarClipCloudsKm;
	float		ShadowMaxTraceDistanceKm;
	float		VoxelMipFactor;
	float		VoxelMipFactorShadow;
};

struct  NuajConfigurationCloudLow : public NuajConfigurationCloud
{
	NuajConfigurationCloudLow() : NuajConfigurationCloud()
		, MinStepsCount				( 64 )
		, MaxStepsCount				( 96 )
		, MaxStepsForThicknessKm	( 30.0f )
		, ShadowMinStepsCount		( 16 )
		, ShadowMaxStepsCount		( 16 )
		, ShadowMaxStepsForThicknessKm( 50.0f )
		, ShadowMapFilterSize		( 3.0f )
		, ShadowFilterMinWeight		( 0.05f )
		, ShadowMapInitialAttenuation( 0.8f, 1.0f )   // We only use shadow map initial values if dot(View,Light) > within this range of values
	{}

	int			MinStepsCount;
	int			MaxStepsCount;
	float		MaxStepsForThicknessKm;
	int			ShadowMinStepsCount;
	int			ShadowMaxStepsCount;
	float		ShadowMaxStepsForThicknessKm;
	float		ShadowMapFilterSize;
	float		ShadowFilterMinWeight;
	NjFloat2	ShadowMapInitialAttenuation;  // We attenuate the initial shadowing by the high-altitude clouds depending on the dot(View,Light) using 2 thresholds given by this variable
};

struct  NuajConfigurationCloudHigh : public NuajConfigurationCloud
{
	NuajConfigurationCloudHigh() : NuajConfigurationCloud()
		, FullRefreshFramesCount	( 32 )
		, MaxCameraAltitudeKm		( 8.0f )
		, HemisphereMaxRadiusFactor	( 0.4f )
		, CloudStepsCount			( 32 )
		, ShadowStepsCount			( 8 )
	{}

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
};

struct  NuajConfiguration
{
	NuajConfiguration()
		: Enabled					( true )
		, WorldUnit2Kilometer		( 0.01f )
		, GroundAltitude			( 0.0f )
		, MyXInYourWorld			( 1, 0, 0 )
		, MyYInYourWorld			( 0, 1, 0 )
		, MyZInYourWorld			( 0, 0, 1 )
		, RefinementZThreshold		( 100.f )

		, SunIntensity				( 100.f)
		, RayleighWavelengths		( 0.650f, 0.570f, 0.475f )  // Standard RGB wavelengths in µm
		, MoonAlbedo				( 0.12f )
	{}

	// Global
	bool		Enabled;
	float		WorldUnit2Kilometer;
	float		GroundAltitude;

	NjFloat3	MyXInYourWorld;						// How do you write my unit X vector in your world space ?
	NjFloat3	MyYInYourWorld;						// How do you write my unit Y vector in your world space ?
	NjFloat3	MyZInYourWorld;						// How do you write my unit Z vector in your world space ?
	float		RefinementZThreshold;

	float		SunIntensity;
	NjFloat3	RayleighWavelengths;
	float		MoonAlbedo;

	// Cloud layers
	NuajConfigurationCloudLow   CloudLayerLow;
	NuajConfigurationCloudHigh  CloudLayerHigh;
};

//////////////////////////////////////////////////////////////////////////
// Quality settings
struct	NuajQualityCloudLow
{
	NuajQualityCloudLow()
		: DeepShadowMapSize			( 512 )
		, FilteredDeepShadowMapSize	( 256 )
	{}

	int			DeepShadowMapSize;
	int			FilteredDeepShadowMapSize;
};

struct	NuajQualityCloudHigh
{
	NuajQualityCloudHigh()
		: CloudMapSize				( 2048 )
		, DeepShadowMapSize			( 512 )
	{}

	int			CloudMapSize;
	int			DeepShadowMapSize;
};

struct	NuajQuality
{
	NuajQuality()
		: SkyDomeSubdivisionsTheta	( 40 )
		, SkyDomeSubdivisionsPhi	( 80 )

		, SkyEnvironmentMapWidth	( 256 )
		, SkyEnvironmentMapHeight	( 128 )
		, ProbeRenderWidth			( 32 )
		, ProbeRenderHeight			( 16 )
	{}

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
};

//////////////////////////////////////////////////////////////////////////
// Realtime Sky & Clouds parameters
struct  NuajParametersCloud
{
	NuajParametersCloud()
		: Enabled					( true )
		, AltitudeKm				( 2.0f )
		, ThicknessKm				( 5.0f )
		, Density					( 1.f)
		, CoverageOffset			( -0.05f)
		, CoverageContrast			( 0.2f)
		, CoverageGamma				( 1.0f)
		, FarClipKm					( 100.f )
		, DirectionalFactor			( 0.22f )
		, AlbedoDirectional			( 0.95f )
		, IsotropicFactor			( 1.0f )
		, AlbedoIsotropic			( 0.22f )
		, IsotropicAmbientColor		( 0.7045f, 0.7399f, 0.7773f )
		, IsotropicBlendWithSkyColor( 0.0f )  // Use full ambient color, no sky color (to be increased toward 1 at sunset to get the nice redish sky colors)
		, IsotropicDirectLightFactor( 50.0f )
		, IsotropicAmbientLightFactor( 20.0f )
		, IsotropicTerrainReflectedLightFactor( 0.05f )
		, NoiseSize					( 0.005f )
		, NoiseSizeVerticalFactor	( 1.0f )
		, NoiseFrequencyFactor		( 3.0f )
		, NoiseAmplitudeFactor		( 0.4f )
		, ShadowOpacity				( 1.0f )
		, ShadowFarClipKm			( 200.f )
		, WindForce					( 0.2f )
		, WindAngle					( 0.0f )
		, EvolutionSpeed			( 8.0f )
		, LocalCoverageCenter		( 0, 0 )
		, LocalCoverageSize			( 1, 1 )
		, LocalCoverageEnabled		( false )
		, LocalCoverageScrollWithWind( true )
	{
	}

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

	// Shadowing
	float		ShadowOpacity;
	float		ShadowFarClipKm;

	// Animation
	float		WindForce;
	float		WindAngle;
	float		EvolutionSpeed;
};

struct  NuajParametersCloudLow : NuajParametersCloud
{
	NuajParametersCloudLow() : NuajParametersCloud()
		, AutoComputeShadowOpacity	( true )
		, ShadowFarDistanceKm		( 200.0f )
	{
		//////////////////////////////////////////////////////////////////////////
		// Override the parameters for low altitude clouds
		AltitudeKm = 4.0f;
		ThicknessKm = 8.0f;

		Density = 2.0f;
		DirectionalFactor = 0.22f;
		AlbedoDirectional = 0.95f;
		IsotropicFactor = 1.0f;
		AlbedoIsotropic = 0.022f;

		CoverageOffset = -0.147f;
		CoverageContrast = 0.588f;
		NoiseSize = 0.0015f;
		NoiseAmplitudeFactor = 0.4f;
		NoiseFrequencyFactor = 4.0f;
	}

	bool		AutoComputeShadowOpacity;
	float		ShadowFarDistanceKm;
};

struct  NuajSkyParameters
{
	NuajSkyParameters()
		: Enabled				( true )
		, SunDirection			( 0, 1, 1 )
		, LightSourceBlend		( 0 )				// Full sun !
		, RayleighDensity		( 1e-5f * 8.0f )	// Clear sky
		, MieDensity			( 1e-4f * 1.0f )	// Almost no fog
		, ScatteringAnisotropy( 0.75f )
		, FarClipKm				( 200.f )
		, NightSkyAmbientColor	( 0.01f, 0.01f, 0.01f )
		, SkyBackgroundColor	( 0.0f, 0.0f, 0.0f )								// Clear the sky with a black background so we have the exact sky color
		, TerrainAlbedo			( 37.0f / 255.0f, 24.0f / 255.0f, 16.0f / 255.0f )  // Some red-ish tint for terrain reflection. Could be localized by a texture...
		, pTexLocalCoverage		( 0 )
	{}

	bool					Enabled;
	NjFloat3				SunDirection;
	NjFloat3				MoonDirection;
	float					LightSourceBlend;
	float					RayleighDensity;
	float					MieDensity;
	float					ScatteringAnisotropy;
	float					FarClipKm;
	NjFloat3				NightSkyAmbientColor;
	NjFloat3				SkyBackgroundColor;
	NjFloat3				TerrainAlbedo;
	NjITexture*				pTexLocalCoverage;

	NjFloat3				LightningPosition0;
	NjFloat3				LightningColor0;
	NjFloat3				LightningPosition1;
	NjFloat3				LightningColor1;

	NuajParametersCloudLow	CloudLayerLow;
	NuajParametersCloud		CloudLayerHigh;
};

//////////////////////////////////////////////////////////////////////////
// Rendering parameters, fed each frame to the renderer
struct  NuajRenderParameters
{
	NuajRenderParameters( NjITextureView& _TexZBuffer, NjITextureView& _TexDownsampledZBuffer, NjITexture& _TexHDRTarget )
		: TexZBuffer( _TexZBuffer )
		, TexDownsampledZBuffer( _TexDownsampledZBuffer )
		, TexHDRTarget( _TexHDRTarget )
	{}

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

	NjITextureView&		TexZBuffer;						// The ZBuffer (full resolution)
	NjITextureView&		TexDownsampledZBuffer;			// The ZBuffer, downsampled by 1/4
	NjITexture&			TexHDRTarget;					// The HDR buffer we will render the sky into
};
