#pragma once

#include "Types.h"
#include "IEngine.h"

//////////////////////////////////////////////////////////////////////////
// Configuration parameters (change once per level/engine instance)
struct  NuajConfigurationCloud
{
	NuajConfigurationCloud()
		: m_FarClipCloudsKm			( 100.0f )
		, m_ShadowMaxTraceDistanceKm( 200.0f )
		, m_VoxelMipFactor			( 0.003f )
		, m_VoxelMipFactorShadow	( 0.002f )
	{}

	float		m_FarClipCloudsKm;
	float		m_ShadowMaxTraceDistanceKm;
	float		m_VoxelMipFactor;
	float		m_VoxelMipFactorShadow;
};

struct  NuajConfigurationCloudLow : public NuajConfigurationCloud
{
	NuajConfigurationCloudLow() : NuajConfigurationCloud()
		, m_MinStepsCount			( 64 )
		, m_MaxStepsCount			( 96 )
		, m_MaxStepsForThicknessKm	( 30.0f )
		, m_ShadowMinStepsCount		( 16 )
		, m_ShadowMaxStepsCount		( 16 )
		, m_ShadowMaxStepsForThicknessKm( 50.0f )
		, m_ShadowMapFilterSize		( 3.0f )
		, m_ShadowFilterMinWeight	( 0.05f )
		, m_ShadowMapInitialAttenuation( 0.8f, 1.0f )   // We only use shadow map initial values if dot(View,Light) > within this range of values
	{}

	int			m_MinStepsCount;
	int			m_MaxStepsCount;
	float		m_MaxStepsForThicknessKm;
	int			m_ShadowMinStepsCount;
	int			m_ShadowMaxStepsCount;
	float		m_ShadowMaxStepsForThicknessKm;
	float		m_ShadowMapFilterSize;
	float		m_ShadowFilterMinWeight;
	NjFloat2	m_ShadowMapInitialAttenuation;  // We attenuate the initial shadowing by the high-altitude clouds depending on the dot(View,Light) using 2 thresholds given by this variable
};

struct  NuajConfigurationCloudHigh : public NuajConfigurationCloud
{
	NuajConfigurationCloudHigh() : NuajConfigurationCloud()
		, m_FullRefreshFramesCount	( 32 )
		, m_MaxCameraAltitudeKm		( 8.0f )
		, m_HemisphereMaxRadiusFactor( 0.4f )
		, m_CloudStepsCount			( 32 )
		, m_ShadowStepsCount		( 8 )
	{}

	int			m_FullRefreshFramesCount;
	float		m_MaxCameraAltitudeKm;
	float		m_HemisphereMaxRadiusFactor;
	int			m_CloudStepsCount;
	int			m_ShadowStepsCount;
};

struct  NuajConfiguration
{
	NuajConfiguration()
		: m_Enabled					( false )
		, m_WorldUnit2Kilometer		( 0.01f )
		, m_GroundAltitude			( 0.0f )
		, m_MyXInYourWorld			( 1, 0, 0 )
		, m_MyYInYourWorld			( 0, 1, 0 )
		, m_MyZInYourWorld			( 0, 0, 1 )
		, m_RefinementZThreshold	( 100.f )

		, m_SunIntensity			( 100.f)
		, m_RayleighWavelengths		( 0.650f, 0.570f, 0.475f )  // Standard RGB wavelengths in µm
		, m_MoonAlbedo				( 0.12f )
	{}

	// Global
	bool		m_Enabled;
	float		m_WorldUnit2Kilometer;
	float		m_GroundAltitude;

	NjFloat3	m_MyXInYourWorld;		   // How do you write my unit X vector in your world space ?
	NjFloat3	m_MyYInYourWorld;		   // How do you write my unit Y vector in your world space ?
	NjFloat3	m_MyZInYourWorld;		   // How do you write my unit Z vector in your world space ?
	float		m_RefinementZThreshold;

	float		m_SunIntensity;
	NjFloat3	m_RayleighWavelengths;
	float		m_MoonAlbedo;

	// Cloud layers
	NuajConfigurationCloudLow   m_CloudLayerLow;
	NuajConfigurationCloudHigh  m_CloudLayerHigh;
};

//////////////////////////////////////////////////////////////////////////
// Quality settings
struct	NuajQualityCloudLow
{
	NuajQualityCloudLow()
		: m_DeepShadowMapSize			( 512 )
		, m_FilteredDeepShadowMapSize	( 256 )
	{}

	int			m_DeepShadowMapSize;
	int			m_FilteredDeepShadowMapSize;
};

struct	NuajQualityCloudHigh
{
	NuajQualityCloudHigh()
		: m_CloudMapSize			( 2048 )
		, m_DeepShadowMapSize		( 512 )
	{}

	int			m_CloudMapSize;
	int			m_DeepShadowMapSize;
};

struct	NuajQuality
{
	NuajQuality()
		: m_SkyDomeSubdivisionsTheta( 40 )
		, m_SkyDomeSubdivisionsPhi	( 80 )

		, m_SkyEnvironmentMapWidth	( 256 )
		, m_SkyEnvironmentMapHeight	( 128 )
		, m_ProbeRenderWidth		( 32 )
		, m_ProbeRenderHeight		( 16 )
	{}

	// Skydome geometry Resolution
	int			m_SkyDomeSubdivisionsTheta;
	int			m_SkyDomeSubdivisionsPhi;

	// Probes & Environment Map Resolution
	int			m_SkyEnvironmentMapWidth;
	int			m_SkyEnvironmentMapHeight;
	int			m_ProbeRenderWidth;
	int			m_ProbeRenderHeight;

	// Cloud layers
	NuajQualityCloudLow		m_CloudLayerLow;
	NuajQualityCloudHigh	m_CloudLayerHigh;
};

//////////////////////////////////////////////////////////////////////////
// Realtime Sky & Clouds parameters
struct  NuajParametersCloud
{
	NuajParametersCloud()
		: m_Enabled					( true )
		, m_AltitudeKm				( 2.0f )
		, m_ThicknessKm				( 5.0f )
		, m_Density					( 1.f)
		, m_CoverageOffset			( -0.05f)
		, m_CoverageContrast		( 0.2f)
		, m_CoverageGamma			( 1.0f)
		, m_FarClipKm				( 100.f )
		, m_DirectionalFactor		( 0.22f )
		, m_AlbedoDirectional		( 0.95f )
		, m_IsotropicFactor			( 1.0f )
		, m_AlbedoIsotropic			( 0.22f )
		, m_IsotropicAmbientColor	( 0.7045f, 0.7399f, 0.7773f )
		, m_IsotropicBlendWithSkyColor( 0.0f )  // Use full ambient color, no sky color (to be increased toward 1 at sunset to get the nice redish sky colors)
		, m_IsotropicDirectLightFactor( 50.0f )
		, m_IsotropicAmbientLightFactor( 20.0f )
		, m_IsotropicTerrainReflectedLightFactor( 0.05f )
		, m_NoiseSize				( 0.005f )
		, m_NoiseSizeVerticalFactor	( 1.0f )
		, m_NoiseFrequencyFactor	( 3.0f )
		, m_NoiseAmplitudeFactor	( 0.4f )
		, m_ShadowOpacity			( 1.0f )
		, m_ShadowFarClipKm			( 200.f )
		, m_WindForce				( 0.2f )
		, m_WindAngle				( 0.0f )
		, m_EvolutionSpeed			( 8.0f )
		, m_LocalCoverageCenter		( 0, 0 )
		, m_LocalCoverageSize		( 1, 1 )
		, m_LocalCoverageEnabled	( false )
		, m_LocalCoverageScrollWithWind ( true )
	{
	}

	bool		m_Enabled;

	// Geometry
	float		m_AltitudeKm;
	float		m_ThicknessKm;
	float		m_FarClipKm;

	// Density and coverage
	float		m_Density;
	float		m_CoverageOffset;
	float		m_CoverageContrast;
	float		m_CoverageGamma;

	// Local coverage
	NjFloat2	m_LocalCoverageCenter;
	NjFloat2	m_LocalCoverageSize;
	bool		m_LocalCoverageEnabled;
	bool		m_LocalCoverageScrollWithWind;

	// Noise & octaves 
	float		m_NoiseSize;
	float		m_NoiseSizeVerticalFactor;
	float		m_NoiseAmplitudeFactor;
	float		m_NoiseFrequencyFactor;

	// Lighting params
	float		m_DirectionalFactor;
	float		m_AlbedoDirectional;
	float		m_IsotropicFactor;
	float		m_AlbedoIsotropic;
	float		m_IsotropicDirectLightFactor;
	float		m_IsotropicAmbientLightFactor;
	float		m_IsotropicTerrainReflectedLightFactor;
	NjFloat3	m_IsotropicAmbientColor;
	float		m_IsotropicBlendWithSkyColor;

	// Shadowing
	float		m_ShadowOpacity;
	float		m_ShadowFarClipKm;

	// Animation
	float		m_WindForce;
	float		m_WindAngle;
	float		m_EvolutionSpeed;
};

struct  NuajParametersCloudLow : NuajParametersCloud
{
	NuajParametersCloudLow() : NuajParametersCloud()
		, m_AutoComputeShadowOpacity( true )
		, m_ShadowFarDistanceKm		( 200.0f )
	{
		//////////////////////////////////////////////////////////////////////////
		// Override the parameters for low altitude clouds
		m_AltitudeKm = 4.0f;
		m_ThicknessKm = 8.0f;

		m_Density = 2.0f;
		m_DirectionalFactor = 0.22f;
		m_AlbedoDirectional = 0.95f;
		m_IsotropicFactor = 1.0f;
		m_AlbedoIsotropic = 0.022f;

		m_CoverageOffset = -0.147f;
		m_CoverageContrast = 0.588f;
		m_NoiseSize = 0.0015f;
		m_NoiseAmplitudeFactor = 0.4f;
		m_NoiseFrequencyFactor = 4.0f;
	}

	bool		m_AutoComputeShadowOpacity;
	float		m_ShadowFarDistanceKm;
};

struct  NuajSkyParameters
{
	NuajSkyParameters()
		: m_Enabled				( true )
		, m_SunDirection		( 0, 1, 1 )
		, m_LightSourceBlend	( 0 )				// Full sun !
		, m_RayleighDensity		( 1e-5f * 8.0f )	// Clear sky
		, m_MieDensity			( 1e-4f * 1.0f )	// Almost no fog
		, m_ScatteringAnisotropy( 0.75f )
		, m_FarClipKm			( 200.f )
		, m_NightSkyAmbientColor( 0.01f, 0.01f, 0.01f )
		, m_SkyBackgroundColor	( 0.0f, 0.0f, 0.0f )								// Clear the sky with a black background so we have the exact sky color
		, m_TerrainAlbedo		( 37.0f / 255.0f, 24.0f / 255.0f, 16.0f / 255.0f )  // Some red-ish tint for terrain reflection. Could be localized by a texture...
		, m_pLocalCoverageTexture( 0 )
	{}

	bool		m_Enabled;
	NjFloat3	m_SunDirection;
	NjFloat3	m_MoonDirection;
	float		m_LightSourceBlend;
	float		m_RayleighDensity;
	float		m_MieDensity;
	float		m_ScatteringAnisotropy;
	float		m_FarClipKm;
	NjFloat3	m_NightSkyAmbientColor;
	NjFloat3	m_SkyBackgroundColor;
	NjFloat3	m_TerrainAlbedo;
	NjITexture* m_pLocalCoverageTexture;

	NjFloat3	m_LightningPosition0;
	NjFloat3	m_LightningColor0;
	NjFloat3	m_LightningPosition1;
	NjFloat3	m_LightningColor1;

	NuajParametersCloudLow	m_CloudLayerLow;
	NuajParametersCloud		m_CloudLayerHigh;
};

//////////////////////////////////////////////////////////////////////////
// Rendering parameters, fed each frame to the renderer
struct  NuajRenderParameters
{
	NuajRenderParameters( NjITextureView& _ZBuffer, NjITextureView& _DownscaledZBuffer, NjITexture& _HDRTarget )
		: m_ZBuffer( _ZBuffer )
		, m_DownscaledZBuffer( _DownscaledZBuffer )
		, m_HDRTarget( _HDRTarget )
	{}

	float		m_DeltaTime;

	bool		m_IsPerspectiveProjection;
	float		m_CameraFOV;			// VERTICAL FOV !
	float		m_CameraHeight;			// Vertical height if ortho
	float		m_CameraAspectRatio;	// Width / Height
	float		m_CameraNearClip;
	float		m_CameraFarClip;
	NjFloat4x4  m_Camera2World;			// View matrix

	NjITextureView&	m_ZBuffer;			// The ZBuffer (full resolution)
	NjITextureView& m_DownscaledZBuffer;// The ZBuffer, downscaled by 1/4
	NjITexture& m_HDRTarget;			// The HDR buffer we will render the sky into
};
